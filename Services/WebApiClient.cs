using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ClientServerShared;

namespace CSharpWpfShazam.Services
{
    public class WebApiClient
    {
        private HttpClient _httpClient;

        // WebApiClient class supports *one* endpoint (e.g. auth or no-auth)
        public WebApiClient(string azureServiceWebApiEndpoint, string? accessToken = null)
        {
            _httpClient = new HttpClient();
            AzureServiceWebApiEndpoint = azureServiceWebApiEndpoint;

            var defaultRequestHeaders = _httpClient.DefaultRequestHeaders;
            if (defaultRequestHeaders.Accept == null ||
               !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new
                            MediaTypeWithQualityHeaderValue("application/json"));
            }

            if (!string.IsNullOrEmpty(accessToken))
            {
                defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
            }
        }

        public string AzureServiceWebApiEndpoint { get; private set; }

        public async Task<GetAllSongInfoListResponse?> GetAllSongInfoListAsync(GetAllSongInfoListRequest request)
        {
            return await CallWebAPIAsync<GetAllSongInfoListRequest, GetAllSongInfoListResponse>(request, "GetAllSongInfoList");
        }

        public async Task<AddSongInfoResponse?> AddSongInfoAsync(AddSongInfoRequest request)
        {
            return await CallWebAPIAsync<AddSongInfoRequest, AddSongInfoResponse>(request, "AddSongInfo");
        }
        
        public async Task<DeleteSongInfoResponse?> DeleteSongInfoAsync(DeleteSongInfoRequest request)
        {
            // Note: as of 2023-11-08, DeleteSongInfo with HttpDelete on the server side
            //          doesn't work in Azure (working on my machine!).
            //          So use PostAsJsonAsync instead of DeleteAsync.
            return await CallWebAPIAsync<DeleteSongInfoRequest, DeleteSongInfoResponse>(request, "DeleteSongInfo");
        }

        private async Task<RSP?> CallWebAPIAsync<REQ, RSP>(REQ request, string methodName)
        {
            string url = $"{AzureServiceWebApiEndpoint}/{methodName}";

            HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync(url, request);
            responseMessage.EnsureSuccessStatusCode();
            RSP? response = await responseMessage.Content.ReadFromJsonAsync<RSP>();
            return response;
        }
    }
}
