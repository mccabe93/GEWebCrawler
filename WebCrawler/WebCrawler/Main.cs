/// <summary>
/// tile:   GE Web Crawler 
/// author: Mike McCabe
/// 
/// Traversing an 'internet' of json objects. The crawler parses the pages that exist,
/// skipping over ones already parsed, and stating failure for those that were discovered
/// but not parsed.
/// 
/// </summary>
namespace GECrawler
{
    class ProgramMain
    {
        /// <summary>
        /// A single argument may be provided.
        /// runtests or no arguments will run the crawler on the given test cases.
        /// Any other single argument will be processed as a filename, except if that filename is runtests.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Crawler crawler = new Crawler();
            // Begin crawling on the input file
            if (args.Length == 0 || args[1].ToLower().Equals("runtests"))
            {
                System.Console.WriteLine("BEGIN Internet1 TEST");
                crawler.StartCrawling("Internet1.json");
                crawler.PrintCrawlReport();
                System.Console.WriteLine("END Internet1 TEST\n\n");
                System.Console.WriteLine("BEGIN Internet2 TEST");
                crawler = new Crawler();
                crawler.StartCrawling("Internet2.json");
                crawler.PrintCrawlReport();
                System.Console.WriteLine("END Internet2 TEST");
            }
            else if(args.Length == 1)
            {
                crawler.StartCrawling(args[1]);
                crawler.PrintCrawlReport();
            }
            else
            {
                System.Console.WriteLine("Invalid arguments provided.");
            }
            System.Console.Write("Press any key to exit . . .");
            System.Console.ReadLine();
        }
    }
}
