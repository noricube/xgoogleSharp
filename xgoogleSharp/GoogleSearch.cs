using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using System.Web;

namespace xgoogleSharp
{
    public class GoogleSearch
    {
        protected const string SEARCH_URL_0 = "https://www.google.com/search?hl=en&q={0}&btnG=Google+Search";
        protected const string NEXT_PAGE_0 = "https://www.google.com/search?hl=en&q={0}&start={1}";
        protected const string SEARCH_URL_1 = "https://www.google.com/search?hl=en&q={0}&num={1}&btnG=Google+Search";
        protected const string NEXT_PAGE_1 = "https://www.google.com/search?hl=en&q={0}&num={1}&start={2}";

        public int Page { get; protected set; }
        public bool HasNext { get; protected set; }
        protected int RequestPerPage;
        protected string Query;
        
        private Browser Browser;


        public GoogleSearch(string query, int requestPerPage = 10)
        {
            Query = query;
            Page = 1;
            RequestPerPage = requestPerPage;
            Browser = new Browser();
        }

        public async Task<List<GoogleSearchResult>> FetchResults()
        {
            var results = new List<GoogleSearchResult>();

            string fetchUrl;
            if ( RequestPerPage == 10)
            {
                if (Page == 1)
                {
                    fetchUrl = String.Format(SEARCH_URL_0, Query);
                }
                else
                {
                    fetchUrl = String.Format(NEXT_PAGE_0, Query, (Page-1) * RequestPerPage);
                }
            }
            else
            {
                if (Page == 1)
                {
                    fetchUrl = String.Format(SEARCH_URL_1, Query, RequestPerPage);
                }
                else
                {
                    fetchUrl = String.Format(NEXT_PAGE_1, Query, RequestPerPage, (Page - 1) * RequestPerPage);
                }
            }

            //fetchUrl = "http://chp.ly.lv/google.html";

            //Console.WriteLine(fetchUrl);
            await Task.Delay(1000);
                
            string content = await Browser.FetchPage(fetchUrl);

            var html = new HtmlDocument();
            html.LoadHtml(content);

            var document = html.DocumentNode;

            // 모든 글 선택
            foreach (var node in document.QuerySelectorAll("li.g"))
            {
                var titleNode = node.QuerySelector("h3.r a");
                var descNode = node.QuerySelector("span.st");

                if (descNode == null )
                {
                    continue;
                }
                //Console.WriteLine(node.InnerText);
                results.Add(new GoogleSearchResult()
                {
                    Title = HttpUtility.HtmlDecode(titleNode.InnerText),
                    Description = HttpUtility.HtmlDecode(descNode.InnerText),
                    Url = titleNode.Attributes["href"].Value
                });
            }

            // 다음글 있는지 확인
            HasNext = false;
            foreach (var node in document.QuerySelectorAll("span"))
            {
                if (node.InnerText.Equals("Next"))
                {
                    HasNext = true;
                    Page++;
                }
            }

            return results;
        }
    }
}