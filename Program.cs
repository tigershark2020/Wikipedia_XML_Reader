using System.Linq;
using System.Xml.Linq;

namespace Wikipedia_XML_Reader
{
    class Program
    {

        public class WikipediaItem
        {
            public string articleTitle { get; set; }
            public string articleDescription { get; set; }
        }
        public class StockData
        {
            public string Symbol { get; set; }
            public string URL { get; set; }
        }
        static System.Collections.Generic.IEnumerable<XElement> StreamRootChildDoc(string uri)
        {
            using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(uri))
            {
                reader.MoveToContent();

                // Parse the file and return each of the nodes.
                while (!reader.EOF)
                {
                    if (reader.NodeType == System.Xml.XmlNodeType.Element && reader.Name == "page")
                    {
                        XElement el = XElement.ReadFrom(reader) as XElement;
                        if (el != null)
                            yield return el;
                    }
                    else
                    {
                        reader.Read();
                    }
                }
            }
        }

        public static string Clean_URL(string url)
        {
            url = url.Replace("url|", "");
            if(url.Contains("|"))
            {
                url = url.Substring(0, url.IndexOf("|"));
            }

            if (url.Contains("https://") || url.Contains("http://"))
            {
                url = url.Replace("}", "").Replace("{", "").Trim();
            }
            else
            {
                url = "https://" + url.Replace("}", "").Replace("{", "").Trim();
            }
            if(url.Contains(" "))
            {
                url = url.Substring(0, url.IndexOf(" "));
            }
            if (url.Contains("&"))
            {
                url = url.Substring(0, url.IndexOf("&"));
            }

            url = url.Replace("[", "");
            if (url.EndsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }    

            return url;
        }

        public static void Process_Articles(string xmlPath)
        {
            XNamespace xmlns = "http://www.mediawiki.org/xml/export-0.10/";

            System.Collections.Generic.List<StockData> stockDataList = new System.Collections.Generic.List<StockData>();

            System.Collections.Generic.IEnumerable<XElement> pageData =
                from el in StreamRootChildDoc(xmlPath)
                where true
                select el;

            int counter = 0;
            foreach (XElement elem in pageData)
            {
                StockData stockDetails = new StockData();

                string contents = elem.ToString();

                string title = elem.Element(xmlns + "title").Value;
                string[] contentsData = contents.Split("\n");

                bool infoboxCompany = false;

                foreach (string data in contentsData)
                {
                    if(data.Contains("Infobox company"))
                    {
                        infoboxCompany = true;

                    }

                    if (infoboxCompany.Equals(true))
                    {
                        string infoboxCompanyDataString = data.Replace(" ", "").Replace("| symbol", "|symbol");

                        if (infoboxCompanyDataString.StartsWith("|symbol="))
                        {
                            string[] symbolData = data.Split("=");
                            stockDetails.Symbol = symbolData[1].Trim();
                            System.Console.WriteLine(stockDetails.Symbol);
                        }
                        else
                        {
                            if (infoboxCompanyDataString.Contains("|symbol="))
                            {
                                string[] infoboxCompanyDataArray = infoboxCompanyDataString.Split("|");
                                foreach (string infoboxDataString in infoboxCompanyDataArray)
                                {
                                    if (infoboxDataString.Contains("symbol") && infoboxDataString.Contains("="))
                                    {
                                        string[] symbolData = infoboxDataString.Split("=");
                                        stockDetails.Symbol = symbolData[1].Trim();
                                        System.Console.WriteLine(stockDetails.Symbol);
                                    }
                                }
                            }
                        }

                        string dataWebsite = data.Replace(" ", "");
                        dataWebsite = dataWebsite.Replace("| website", "|website");

                        if (dataWebsite.Length >= 9)
                        {
                            dataWebsite = dataWebsite.Substring(0, 9);
                            if (dataWebsite.Equals("|website="))
                            {
                                string[] websiteData = data.Split("=");
                                if (websiteData.Length > 0)
                                {
                                    int urlPosition = websiteData[1].ToLower().IndexOf("url|");
                                    if (urlPosition != -1)
                                    {
                                        stockDetails.URL = websiteData[1].Substring(urlPosition + 4, websiteData[1].Length - urlPosition - 4).Trim();
                                        stockDetails.URL = Clean_URL(stockDetails.URL);

                                        if (stockDetails.Symbol != null && stockDetails.URL != null)
                                        {
                                            infoboxCompany = false;
                                        }
                                    }
                                    else
                                    {
                                        string url = websiteData[1].Trim();
                                        stockDetails.URL = url;
                                        stockDetails.URL = Clean_URL(stockDetails.URL);

                                        if (stockDetails.URL != null)
                                        {
                                            if (stockDetails.Symbol != null && stockDetails.URL.Length > 0)
                                            {
                                                infoboxCompany = false;
                                            }
                                        }
                                    }
                                }
                            }

                        }

                        string dataHomePage = data.Replace(" ", ""); ;
                        dataHomePage = dataHomePage.Replace("| homepage", "");
                        if (dataHomePage.Length >= 10)
                        {
                            dataHomePage = dataHomePage.Substring(0, 10);
                            if(dataHomePage.Equals("|homepage="))
                            {
                                string[] homepageData = data.Split("=");
                                if (homepageData.Length > 0)
                                {
                                    int urlPosition = homepageData[1].ToLower().IndexOf("url|");
                                    if (urlPosition != -1)
                                    {
                                        stockDetails.URL = homepageData[1].Substring(urlPosition + 4, homepageData[1].Length - urlPosition - 4).Trim();
                                        stockDetails.URL = Clean_URL(stockDetails.URL);

                                        if (stockDetails.Symbol != null && stockDetails.URL != null)
                                        {
                                            infoboxCompany = false;
                                        }
                                    }
                                    else
                                    {
                                        string url = homepageData[1].Trim();
                                        stockDetails.URL = url;
                                        stockDetails.URL = Clean_URL(stockDetails.URL);

                                        if (stockDetails.URL != null)
                                        {
                                            if (stockDetails.Symbol != null && stockDetails.URL.Length > 0)
                                            {
                                                infoboxCompany = false;
                                            }
                                        }
                                    }
                                }
                            }
                        
                        }
                    }
                }

                if(stockDetails.Symbol != null && stockDetails.URL != null)
                {
                    if(stockDetails.URL.Contains(".") && stockDetails.Symbol != "" && !stockDetails.Symbol.Contains("symbol") && !stockDetails.Symbol.Contains("|") && !stockDetails.Symbol.Contains(":"))
                    {
                        counter++;
                        string dataString = stockDetails.Symbol + "\t" + stockDetails.URL + "\r\n";
                        System.Console.Write(counter + "\t" + dataString);
                        System.IO.File.AppendAllText(@"C:\Users\tigershark2020\Documents\Datasets\Websites\websites_all_stocks_2022.csv", dataString);
                    }
                    else
                    {
                        string dataString = stockDetails.Symbol + "\t" + stockDetails.URL + "\r\n";
                        System.IO.File.WriteAllText(@"C:\Logs\Stocks\" + stockDetails.Symbol + ".xml", contents);
                        System.IO.File.AppendAllText(@"C:\Users\tigershark2020\Documents\Datasets\Websites\websites_all_stocks_errors_2022.csv", dataString);
                    }

                }
            }
        }

        static void Main(string[] args)
        {

            string xmlPath = @"G:\Data\Wikipedia\enwiki-20220401\enwiki-20220401-pages-articles-multistream.xml\enwiki-20220401-pages-articles-multistream.xml";
            Process_Articles(xmlPath);

        }
    }
}
