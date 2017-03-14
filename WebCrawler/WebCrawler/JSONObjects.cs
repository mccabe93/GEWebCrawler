using System;
using System.Collections.Generic;

namespace GECrawler
{
    /// <summary>
    /// The root of all the pages in the given "internet."
    /// </summary>
    public class Pages
    {
        public List<Page> pages { get; set; }
    }
    
    /// <summary>
    /// A parseable page in the internet: contains an address and possibly some links.
    /// </summary>
    public class Page
    {
        public string address { get; set; }
        public List<String> links { get; set; }
    }
}
