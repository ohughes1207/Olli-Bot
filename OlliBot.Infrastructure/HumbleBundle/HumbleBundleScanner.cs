using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using OlliBot.Application.HumbleBundle;
using OlliBot.Application.HumbleBundle.Models;
using OlliBot.Domain.Enums;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace OlliBot.Infrastructure.HumbleBundle;
public class HumbleBundleScanner(ILogger<IHumbleBundleScanner> logger, IConfiguration config) : IHumbleBundleScanner
{
    public async Task<IReadOnlyCollection<ScannedHumbleBundle>> ScanAsync(HumbleBundleType bundleType, CancellationToken ct)
    {
        using IPlaywright pw = await Playwright.CreateAsync();
        await using IBrowser browser = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions()
        {
            Headless = true
        });
        await using IBrowserContext context = await browser.NewContextAsync();
        IPage page = await context.NewPageAsync();

        string type = bundleType switch
        {
            HumbleBundleType.Games => "games",
            HumbleBundleType.Books => "books",
            HumbleBundleType.Software => "software",
            _ => "games"
        };

        await page.GotoAsync("https://www.humblebundle.com/" + type);

        ILocator bundles = page.Locator(".full-tile-view.one-third.bundle");
        int bundleCount = await bundles.CountAsync();
        logger.LogInformation("Found {BundleCount} bundles of type {BundleType}", bundleCount, bundleType);

        var tasks = new List<Task>();
        int MAX_CONCURRENCY = config.GetValue<int>("HumbleBundleScannerMaxConcurrency", 4);
        var semaphore = new SemaphoreSlim(MAX_CONCURRENCY);

        var humbleBundles = new ConcurrentBag<ScannedHumbleBundle>();

        for (int i = 0; i < bundleCount; i++)
        {
            int index = i;

            tasks.Add(ProcessBundle(bundleType, context, bundles, semaphore, index, humbleBundles));
        }

        await Task.WhenAll(tasks);
        await context.CloseAsync();

        RemoveDuplicateItems(humbleBundles);
        return humbleBundles;
    }

    private void RemoveDuplicateItems(ConcurrentBag<ScannedHumbleBundle> humbleBundles)
    {
        foreach (ScannedHumbleBundle bundle in humbleBundles)
        {
            var seenItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (ScannedHumbleBundleTier? tier in bundle.BundleTiers.OrderBy(t => t.Tier))
            {
                tier.HumbleBundleItems = tier.HumbleBundleItems
                    .GroupBy(item => item.ItemName)
                    .Select(g => g.First())
                    .Where(item => seenItems.Add(item.ItemName))
                    .ToList();
            }
        }
    }

    private async Task ProcessBundle(HumbleBundleType bundleType, IBrowserContext context, ILocator bundles, SemaphoreSlim semaphore, int index, ConcurrentBag<ScannedHumbleBundle> humbleBundles)
    {
        await semaphore.WaitAsync();

        ILocator bundle = bundles.Nth(index);
        string bundleName = await bundle.Locator(".name").InnerHTMLAsync();
        logger.LogInformation("Processing bundle {BundleName}", bundleName);

        var humbleBundle = new ScannedHumbleBundle
        {
            Name = bundleName,
            BundleType = bundleType,
        };

        string href = await bundle.GetAttributeAsync("href") ?? throw new Exception();
        try
        {
            await ParseBundle(context, href, humbleBundle);
            humbleBundles.Add(humbleBundle);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing bundle {BundleName} at index {Index}", bundleName, index);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task<ScannedHumbleBundle> ParseBundle(IBrowserContext context, string href, ScannedHumbleBundle bundle)
    {
        string bundleUrl = "https://www.humblebundle.com" + href;
        IPage page = await context.NewPageAsync();

        await page.GotoAsync(bundleUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });

        var uri = new Uri(page.Url);
        bundle.Url = uri.GetLeftPart(UriPartial.Path);

        await page.Locator("p.marketing-blurb").WaitForAsync(new LocatorWaitForOptions
        {
            Timeout = 10000
        });

        //await RemoveCookiesBannerAsync(page);
        await AcceptCookiesAsync(page);

        bundle.Description = await page.Locator("p.marketing-blurb").EvaluateAsync<string>(@"
element => {
    const clone = element.cloneNode(true);
    clone.querySelector('span')?.remove();
    return clone.innerText.trim();
}");


        ILocator noteLocator = page.Locator("p.marketing-blurb span").First;

        bundle.Note = await noteLocator.CountAsync() > 0
            ? await noteLocator.InnerTextAsync()
            : string.Empty;

        string? imageUrl = await page.Locator("img.bundle-logo").First.GetAttributeAsync("src");
        bundle.ImageUrl = imageUrl ?? string.Empty;

        int.TryParse(await page.Locator(".js-hours").First.InnerTextAsync(), out int hours);

        ILocator daysLocator = page.Locator(".js-days");

        int days = 0;

        if (await daysLocator.CountAsync() > 0)
        {
            int.TryParse(await daysLocator.First.InnerTextAsync(), out days);
        }

        ILocator minutesLocator = page.Locator(".js-minutes").First;
        int minutes = 0;
        if (await minutesLocator.CountAsync() > 0)
        {
            int.TryParse(await minutesLocator.InnerTextAsync(), out minutes);
        }

        DateTime expiryDate = DateTime.UtcNow.Add(new TimeSpan(days, hours, minutes, 0));

        bundle.ExpiryDate = expiryDate;

        ILocator tierFilters = page.Locator(".js-tier-filter.chip");
        int tierCount = Math.Max(1, await tierFilters.CountAsync());

        for (int tier = 0; tier < tierCount; tier++)
        {
            var bundleTier = new ScannedHumbleBundleTier
            {
                Tier = tierCount - tier
            };
            ILocator tierFilter = tierFilters.Nth(tier);
            if (await tierFilter.IsVisibleAsync())
                await tierFilter.ClickAsync();

            ILocator bundleItems = page.Locator(".tier-item-view");

            if (tier != 0)
            {
                await page.WaitForFunctionAsync("() => !document.querySelector('.tier-item-view.flipping')");
            }

            string priceHeader = await page.Locator(".tier-header.js-tier-header").First.InnerTextAsync();

            bundleTier.Price = ExtractPrice(priceHeader);

            int itemCount = await bundleItems.CountAsync();

            for (int item = 0; item < itemCount; item++)
            {
                string title = await bundleItems.Nth(item).Locator(".item-title").InnerTextAsync();

                ILocator extraInfoLocator = bundleItems
                    .Nth(item)
                    .Locator(".extra-info.fine-print");

                string extraInfo = await extraInfoLocator.CountAsync() > 0
                    ? await extraInfoLocator.InnerTextAsync()
                    : string.Empty;

                var bundleItem = new ScannedHumbleBundleItem
                {
                    ItemName = title,
                    //ExtraInfo = await extraInfo.InnerTextAsync(),
                    ExtraInfo = extraInfo
                };

                bundleTier.HumbleBundleItems.Add(bundleItem);
            }
            bundle.BundleTiers.Add(bundleTier);
        }
        await page.CloseAsync();
        return bundle;
    }

    private static decimal ExtractPrice(string priceHeader)
    {
        Match match = Regex.Match(priceHeader, @"£(\d+(?:\.\d{1,2})?)");

        return match.Success &&
               decimal.TryParse(match.Groups[1].Value, out decimal price)
            ? price
            : 0m;
    }

    private static async Task RemoveCookiesBannerAsync(IPage page)
    {
        await page.EvaluateAsync("""
() => {
    document.getElementById('onetrust-consent-sdk')?.remove();
}
""");
    }

    private static async Task AcceptCookiesAsync(IPage page)
    {
        ILocator acceptButton =
            page.Locator("#onetrust-accept-btn-handler");

        try
        {
            await acceptButton.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 5000
            });

            await acceptButton.ClickAsync();
        }
        catch (TimeoutException)
        {
            // OneTrust didn't appear.
        }
    }

    public async Task<ScannedHumbleBundle> GetBundleDetails(Domain.Entities.HumbleBundle bundle, CancellationToken ct)
    {
        using IPlaywright pw = await Playwright.CreateAsync();
        await using IBrowser browser = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions()
        {
            Headless = true
        });
        await using IBrowserContext context = await browser.NewContextAsync();

        var bundleScan = new ScannedHumbleBundle
        {
            Name = bundle.Name,
            BundleType = bundle.BundleType,
        };

        var s = new Uri(bundle.Url);

        ScannedHumbleBundle humbleBundle = await ParseBundle(context, s.LocalPath, bundleScan);

        return humbleBundle;
    }
}
