using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Collections;
using System.Collections.Concurrent;

namespace GECrawler
{
    // simple enum for tracking the state of a given page
    public enum PageState {
        ERROR,
        SUCCESS,
        SKIPPED
    }

    public class Crawler
    {
        // a queue would be optimal for a non-parallel solution. 
        private Dictionary<String, Page> discoveredPages = new Dictionary<String, Page>();

        // We use a hashmap/dictionary to minimize the overhead of the frequent searches. A queue would be optimal for a non-parallel solution.
        // Dicionaries also have the benefit of uniqueness, so adding the same page multiple times is disallowed, and thus each page has one state.
        private ConcurrentDictionary<String, PageState> pagesInfo = new ConcurrentDictionary<String, PageState>();

        // These lists may be filled if the pages are necessary elsewhere in the software
        private List<String> successPages = new List<String>();
        private List<String> skippedPages = new List<String>();
        private List<String> failurePages = new List<String>();

        // used for limiting parallel search depth to prevent race conditions
        List<Task> threads = new List<Task>();
        
        /// <summary>
        /// Initalizes the crawling through the input file, checking for possible nullness and so on.
        /// </summary>
        /// <param name="inputFile">json file to parse and crawl through</param>
        public void StartCrawling(String inputFile)
        {
            Pages pages = EnumeratePagesFromFile(inputFile);
            // We could check if it's a .json object, but as long as it contains parseable json we'll give it a shot.
            Page firstPage = GetFirstPage(pages);
            if (firstPage != null)
                Crawl(firstPage);
            else
                System.Console.WriteLine("Crawl aborted due to nonexistent first page.");
            Task.WaitAll(threads.ToArray());
        }
        /// <summary>
        /// Returns the first Page object in the parsed Pages. An entrypoint for our crawler.
        /// </summary>
        /// <param name="pages">The Pages object from our enumerated input file</param>
        /// <returns>The first page in the Internet if possible, null otherwise.</returns>
        public Page GetFirstPage(Pages pages)
        {
            if (pages != null && pages.pages.Count > 0)
                return pages.pages[0];
            return null;
        }

        /// <summary>
        /// Retrieves a Pages object from the json input file.
        /// </summary>
        /// <param name="jsonFileObject"></param>
        /// <returns></returns>
        public Pages EnumeratePagesFromFile(String jsonFileObject)
        {
            string jsonString = "";
            try
            {
                jsonString = File.ReadAllText(jsonFileObject);
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Error reading the given file: " + jsonFileObject);
                return null;
            }
            JavaScriptSerializer deserializer = new JavaScriptSerializer();
            Pages jsonPages = deserializer.Deserialize<Pages>(jsonString);
            foreach (Page p in jsonPages.pages)
            {
                discoveredPages.Add(p.address, p);
            }
            return jsonPages;
        }

        /// <summary>
        /// Recursively and in parallel crawls through the given internet of pages. Thanks to ConcurrentDictionary this is easy,
        /// but usually a breadth-first search is very difficult to parallelize.
        /// </summary>
        /// <param name="page"></param>
        private void Crawl(Page page)
        {
            // if we haven't parsed this page, continue on to parse it. otherwise skip it.
            bool newPage = pagesInfo.TryAdd(page.address, PageState.SUCCESS);
            if (!newPage)
                return;
            foreach (String link in page.links)
            {
                if (discoveredPages.ContainsKey(link))
                {
                    if (pagesInfo.ContainsKey(link))
                    {
                        pagesInfo[link] = PageState.SKIPPED;
                    }
                    else
                        threads.Add(Task.Factory.StartNew(() => Crawl(discoveredPages[link])));
                }
                else
                    pagesInfo.TryAdd(link, PageState.ERROR);
            }
            //System.Console.WriteLine("Crawled page " + page.address);
        }

        private void FillLists()
        {
            foreach(var page in pagesInfo)
            {
                string address = page.Key;
                PageState state = page.Value;
                if (state >= PageState.SUCCESS)
                {
                    successPages.Add(address);
                    if (state == PageState.SKIPPED)
                    {
                        skippedPages.Add(address);
                    }
                }
                else 
                {
                    failurePages.Add(address);
                }
            }
        }
        
        /// <summary>
        /// Generates the output string as defined in the instructions.
        /// This could be expanded to check if the lists have been filled, and therefore save time creating the report.
        /// </summary>
        /// <returns>The formatted output string.</returns>
        private String CreateCrawlReport()
        {
            string success = "";
            string skipped = "";
            string failure = "";
            int numPrinted = 0;
            foreach (var val in pagesInfo)
            {
                // enum 1 = success, 2 = skipped. so
                if (val.Value >= PageState.SUCCESS)
                {
                    if (!success.Equals(""))
                        success += ", ";
                    success += val.Key;
                    if (val.Value == PageState.SKIPPED)
                    {
                        if (!skipped.Equals(""))
                            skipped += ", ";
                        skipped += val.Key;
                    }
                }
                else // if(val.Value == PageState.ERROR) // if statement for clarity
                {
                    if (!failure.Equals(""))
                        failure += ", ";
                    failure += val.Key;
                }
                numPrinted++;
            }
            return "Success: [" + success + "]\nSkipped: [" + skipped + "]\nFailure: [" + failure + "]";
        }

        /// <summary>
        /// Prints out the crawl report.
        /// </summary>
        public void PrintCrawlReport()
        {
            System.Console.WriteLine(CreateCrawlReport());
        }

        // getters for the page lists

        public List<String> GetSuccessPages()
        {
            return successPages;
        }

        public List<String> GetFailurePages()
        {
            return failurePages;
        }

        public List<String> GetSkippedPages()
        {
            return skippedPages;
        }
    }
}
