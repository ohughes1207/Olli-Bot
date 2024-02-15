using PuppeteerSharp;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;

namespace self_bot.modules.humblebundle
{
    public class Bundle
    {
        public string Name { get; set;}
        public TimeSpan timeRemaining { get; set;}
        public string tier1_price { get; set;}
        public List<string> tier1_items { get; set;}
        public string tier2_price { get; set;}
        public List<string> tier2_items { get; set;}
        public string tier3_price { get; set;}
        public List<string> tier3_items { get; set;}
    }
    public class HumbleBundleScraper
    {
        public static async Task ScrapeHB()//string bundle_type)
        {
            Console.WriteLine("HB scraper being executed");
            //string HBurl = "https://www.humblebundle.com/games"+bundle_type;

            string HBurl = "https://www.humblebundle.com/games/controllerd-chaos";
            //string HBurl = "https://www.humblebundle.com/software/complete-python-mega-bundle-software?hmb_source=&hmb_medium=product_tile&hmb_campaign=mosaic_section_1_layout_index_1_layout_type_threes_tile_index_1_c_completepythonmegabundle_softwarebundle";
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                DefaultViewport = new ViewPortOptions
                {
                    Width = 1920,
                    Height = 1080
                }
            });

            var page = await browser.NewPageAsync();
            await page.GoToAsync(HBurl);


            var bundle = new Bundle();
            //bundle.timeRemaining = await page.QuerySelectorAsync(".tier-header").EvaluateFunctionAsync<string>("e => e.innerText");


            //List of all filter buttons starting with lowest tier of rewards
            var filters = (await page.QuerySelectorAllAsync("a.js-tier-filter:nth-child(n)")).Reverse().ToList();
            Console.WriteLine(filters.Count);

            for (int i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];

                //string initalHeader = await page.QuerySelectorAsync(".tier-header").EvaluateFunctionAsync<string>("e => e.innerText");
                //Console.WriteLine("FILTER FOUND");
                //await page.EvaluateFunctionAsync("element => element.scrollIntoView()", filter);
                await filter.ClickAsync();
                //await page.Wait();
                Console.WriteLine("filter clicked");

                
                //Jank method but works when making sure each tier is getting the right items, will come back to this in future
                await Task.Delay(300);

                string headerElement = await page.QuerySelectorAsync(".tier-header").EvaluateFunctionAsync<string>("e => e.innerText");
                Console.WriteLine(headerElement);
                var item_elem_list = await page.QuerySelectorAllAsync("div.tier-item-view > a:nth-child(1) > span");

                List<string> itemListStrings = new List<string>();
                foreach (var item in item_elem_list)
                {
                    string itemText = await item.EvaluateFunctionAsync<string>("e => e.innerText");
                    Console.WriteLine(itemText);
                    itemListStrings.Add(itemText);
                }
                Console.WriteLine(itemListStrings.Count);

                switch (i)
                {
                    case 0:
                        bundle.tier1_items = itemListStrings;
                        break;
                    case 1:
                        List<string> tier2_unique_items = itemListStrings.Except(bundle.tier1_items).ToList();
                        Console.WriteLine(tier2_unique_items.Count);
                        bundle.tier2_items = tier2_unique_items;
                        break;

                    case 2:
                        List<string> tier3_unique_items = itemListStrings.Except(bundle.tier2_items).ToList();
                        bundle.tier3_items = tier3_unique_items;
                        break;
                }
            }
        }
    }
}