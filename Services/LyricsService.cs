using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
using CSharpWpfShazam.Helpers;
using CSharpWpfShazam.Models;

namespace CSharpWpfShazam.Services
{
    public class LyricsService
    {
        private bool _lyricsApiKeyAvailable;
        private HttpClient _httpClient = new() { BaseAddress = new Uri("https://api.genius.com"), Timeout = TimeSpan.FromSeconds(6) };  // 3 would be too short

        public LyricsService(string lyricsApiKey)
        {
            if (lyricsApiKey.IsNotBlank())
            {
                _lyricsApiKeyAvailable = true;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", lyricsApiKey);
            }
        }

        public async Task<string> GetLyricsAsync(string song, string artist)
        {
            if (!_lyricsApiKeyAvailable)
            {
                return "Lyrics service is disabled";
            }

            // query: "No Promises+Shayne Ward" (replacing + with space seems fine too)
            string query = $"{song}+{artist}";
            LyricsSearchResponse? searchResponse = await Search(query);
            string lyricsUrl = searchResponse?.Response?.Hits?.Count > 0 ?
                                searchResponse.Response.Hits[0].Result?.Url ?? string.Empty : string.Empty;
            if (lyricsUrl.IsBlank())
            {
                return string.Empty;
            }

            HttpResponseMessage responseMessage = await _httpClient.GetAsync(lyricsUrl);
            string lyricsHtml = await responseMessage.Content.ReadAsStringAsync();
            return ParseLyricsFromHtml(lyricsHtml);
        }

        // query: "No Promises+Shayne Ward" (replacing + with space seems fine too)
        private async Task<LyricsSearchResponse?> Search(string query)
        {
            HttpResponseMessage responseMessage = await _httpClient.GetAsync("/search?q=/" + query);
            string jsonResponse = await responseMessage.Content.ReadAsStringAsync();
            return JsonHelper.DeserializeToClass<LyricsSearchResponse?>(jsonResponse);
        }

        private string ParseLyricsFromHtml(string lyricsHtml)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(lyricsHtml);            

            var clickables = htmlDocument.DocumentNode.SelectNodes("//a[contains(@class, 'ReferentFragmentVariantdesktop')]");
            if (clickables != null)
                foreach (HtmlNode node in clickables)
                    node.ParentNode.ReplaceChild(htmlDocument.CreateTextNode(node.ChildNodes[0].InnerHtml), node);
            var spans = htmlDocument.DocumentNode.SelectNodes("//span");
            if (spans != null)
                foreach (HtmlNode node in spans)
                    node.Remove();

            var nodes = htmlDocument.DocumentNode.SelectNodes("//div[@data-lyrics-container]");

            // Try to remove what I found, addition to the original:
            // <div data-exclude-from-selection="true" class="InreadContainer__Container-sc-19040w5-0 cujBpY
            //          PrimisPlayer__InreadContainer-sc-1tvdtf7-0 juOVWZ">
            //      <div class="PrimisPlayer__Container-sc-1tvdtf7-1 csMTdh"></div>
            // </div>
            foreach (HtmlNode node in nodes)
            {
                // Remove all <a href=....></a>
                // https://stackoverflow.com/questions/25688847/html-agility-pack-get-all-anchors-href-attributes-on-page
                var subNodes = node.SelectNodes("//a[@href]");
                if (subNodes != null)
                    foreach (HtmlNode subNode in subNodes)
                        subNode.Remove();

                subNodes = node.SelectNodes("//div[@data-exclude-from-selection]");
                if (subNodes != null)
                    foreach (HtmlNode subNode in subNodes)
                        subNode.Remove();
            }

            string? lyrics = WebUtility.HtmlDecode(string.Join(string.Empty,
                                            nodes.Select(node => node.InnerHtml)).Replace("<br>", "\n"));
            if (lyrics == null)
            {
                return string.Empty;
            }

            return lyrics.Replace("<i>", string.Empty).Replace("</i>", string.Empty).
                        Replace("<b>", string.Empty).Replace("</b>", string.Empty).Trim('\n');
        }
    }
}
