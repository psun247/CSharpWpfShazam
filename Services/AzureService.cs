using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClientServerShared;
using CSharpWpfShazam.AzureADClientSecret;
using CSharpWpfShazam.Models;

namespace CSharpWpfShazam.Services
{
    public class AzureService
    {
        private WebApiClient? _webApiClientNoAuth;
        private WebApiClient? _webApiClientAuth;

        private AzureService()
        {
            // Failed to initialize _webApiClient           
        }

        private AzureService(AzureADInfo azureADInfo)
        {
            // Make https://localhost:7025/SongRepoNoAuth
            _webApiClientNoAuth = new WebApiClient(azureADInfo.RestApiEndpoint + "NoAuth");
            _webApiClientAuth = new WebApiClient(azureADInfo.RestApiEndpoint, azureADInfo.AccessToken);            
        }

        public static async Task<AzureService> CreateAsync()
        {
            try
            {
                AzureADInfo azureADInfo = await AuthConfig.GetAzureADInfoAsync();
                return new AzureService(azureADInfo);
            }
            catch (Exception ex)
            {
                string todoLogThisMessage = ex.Message;
            }
            return new AzureService();
        }
        
        public string? RestApiUrlNoAuth => _webApiClientNoAuth?.AzureServiceWebApiEndpoint;
        public string? RestApiUrlAuth => _webApiClientAuth?.AzureServiceWebApiEndpoint;

        public async Task<List<SongInfo>> GetAllSongInfoListAsync(bool useAuth)
        {
            WebApiClient webApiClient = GetWebApiClient(useAuth)!;
            GetAllSongInfoListResponse? response = await webApiClient.GetAllSongInfoListAsync(new GetAllSongInfoListRequest());

            return response?.SongInfoDtoList.Select(x => new SongInfo
            {
                Artist = x.Artist,
                Description = x.Description,
                CoverUrl = x.CoverUrl,
                Lyrics = x.Lyrics,
                SongUrl = x.SongUrl
            }).ToList() ?? new List<SongInfo>();
        }

        public async Task<string> AddSongInfoAsync(SongInfo songInfo, bool useAuth)
        {
            WebApiClient webApiClient = GetWebApiClient(useAuth)!;
            AddSongInfoResponse? response =
                await webApiClient.AddSongInfoAsync(new AddSongInfoRequest
                {
                    SongInfoDTO = new SongInfoDTO
                    {
                        Artist = songInfo.Artist,
                        Description = songInfo.Description,
                        CoverUrl = songInfo.CoverUrl,
                        Lyrics = songInfo.Lyrics,
                        SongUrl = songInfo.SongUrl
                    }
                });

            return response?.Error ?? "Error: didn't get a response from REST API";
        }

        public async Task<string> DeleteSongInfoAsync(string songUrl, bool useAuth)
        {
            WebApiClient webApiClient = GetWebApiClient(useAuth)!;
            DeleteSongInfoResponse? response =
                await webApiClient.DeleteSongInfoAsync(new DeleteSongInfoRequest { SongUrl = songUrl });

            return response?.Error ?? "Error: didn't get a response from REST API";
        }

        private WebApiClient? GetWebApiClient(bool useAuth)
        {
            WebApiClient? webApiClient = useAuth ? _webApiClientAuth : _webApiClientNoAuth;
            if (webApiClient == null)
            {
                throw new Exception("Azure web client is not properly set up");
            }

            return webApiClient;
        }
    }
}
