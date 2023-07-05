using PuppeteerSharp;

namespace self_bot.modules.humblebundle
{
    public class HumbleBundleScraper
    {
        public static async Task ScrapeHB(string bundle_type)
        {
            string HBurl = "https://www.humblebundle.com/"+bundle_type;

            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });

            var page = await browser.NewPageAsync();
            await page.GoToAsync(HBurl);

            //List of all filter buttons starting with highest tier of rewards
            var filters = await page.QuerySelectorAllAsync("a.js-tier-filter:nth-child(n)");
            //Reverse the list order so we begin with the lowest tier of rewards
            filters.Reverse();


            foreach (var filter in filters)
            {
                await filter.ClickAsync();
                await page.WaitForNavigationAsync();
                var Title = await page.QuerySelectorAsync(".tier-header");
            }
        }
    }
}