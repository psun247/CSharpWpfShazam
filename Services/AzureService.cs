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
            // Make https://localhost:7025/songreponoauth
            _webApiClientNoAuth = new WebApiClient(azureADInfo.WebApiEndpoint + "noauth");
            _webApiClientAuth = new WebApiClient(azureADInfo.WebApiEndpoint, azureADInfo.AccessToken);
        }

        public static async Task<AzureService> CreateAsync()
        {
            try
            {
                AzureADInfo azureADInfo = await AuthConfig.GetAzureADInfoAsync();
#if DEBUG               
                // Overwrite WebApiEndpoint in appsettings.json in Debug build
                //azureADInfo.WebApiEndpoint = "https://localhost:7025/songrepo";
                //Debug.WriteLine($"****Overwrite WebApiEndpoint in Debug build: {azureADInfo.WebApiEndpoint}");
#endif
                return new AzureService(azureADInfo);
            }
            catch (Microsoft.Identity.Client.MsalClientException)
            {
                // AuthConfig.GetAzureADInfoUserNamePasswordAsync() call would cause this error:
                // { "Unsupported User Type 'Unknown'. Please see https://aka.ms/msal-net-up. "}                
                // https://learn.microsoft.com/en-us/entra/msal/dotnet/acquiring-tokens/desktop-mobile/username-password-authentication
                // Guess: Only available for work and school accounts and not personal Microsoft accounts.
                // So GetAzureADInfoUserNamePasswordAsync() should work in theory, but not in this app
            }
            catch (Exception ex)
            {
                string todoLogThisMessage = ex.Message;
            }
            return new AzureService();
        }

        public string? WebApiUrlNoAuth => _webApiClientNoAuth?.AzureServiceWebApiEndpoint;
        public string? WebApiUrlAuth => _webApiClientAuth?.AzureServiceWebApiEndpoint;

        public async Task UseNewAccessTokenAsync()
        {
            AzureADInfo azureADInfo = await AuthConfig.GetAzureADInfoAsync();
            _webApiClientAuth?.ReplaceAccessToken(azureADInfo.AccessToken);
        }

        public async Task<List<SongInfo>> GetAllSongInfoListAsync(bool viaAuth)
        {
            WebApiClient webApiClient = GetWebApiClient(viaAuth)!;
            GetAllSongInfoListResponse? response = await webApiClient.GetAllSongInfoListAsync(new GetAllSongInfoListRequest());

            return response?.SongInfoDtoList.Select(x => new SongInfo
            {
                Artist = x.Artist,
                Description = x.Description,
                CoverUrl = x.CoverUrl,
                Lyrics = x.Lyrics,
                SongUrl = x.SongUrl,                                
            }).ToList() ?? new List<SongInfo>();
        }

        public async Task<string> AddSongInfoAsync(SongInfo songInfo, bool viaAuth)
        {
            WebApiClient webApiClient = GetWebApiClient(viaAuth)!;
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

            return response?.Error ?? "Error: didn't get a response from Web API";
        }

        public async Task<string> DeleteSongInfoAsync(string songUrl, bool viaAuth)
        {
            WebApiClient webApiClient = GetWebApiClient(viaAuth)!;
            DeleteSongInfoResponse? response =
                await webApiClient.DeleteSongInfoAsync(new DeleteSongInfoRequest { SongUrl = songUrl });

            return response?.Error ?? "Error: didn't get a response from Web API";
        }

        private WebApiClient? GetWebApiClient(bool viaAuth)
        {
            WebApiClient? webApiClient = viaAuth ? _webApiClientAuth : _webApiClientNoAuth;
            if (webApiClient == null)
            {
                throw new Exception("Azure web client is not properly set up");
            }

            return webApiClient;
        }
    }
}
