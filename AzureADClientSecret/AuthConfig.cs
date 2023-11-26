using System;
using System.IO;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

namespace CSharpWpfShazam.AzureADClientSecret
{
    public class AuthConfig
    {
        public string Instance { get; set; } = "https://login.microsoftonline.com/{0}";
        public string TenantId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string Authority => string.Format(CultureInfo.InvariantCulture, Instance, TenantId);
        public string ClientSecret { get; set; } = string.Empty;
        public string WebApiEndpoint { get; set; } = string.Empty;
        public string ResourceId { get; set; } = string.Empty;

        public static async Task<AzureADInfo> GetAzureADInfoAsync()
        {
            var azureADInfo = new AzureADInfo();
            AuthConfig? config = ReadFromJsonFile(@"AzureADClientSecret\appsettings.json");
            if (config != null)
            {
                IConfidentialClientApplication app =
                        ConfidentialClientApplicationBuilder.Create(config.ClientId)
                            .WithClientSecret(config.ClientSecret)
                            .WithAuthority(new Uri(config.Authority))
                            .Build();
                string[] resourceIds = new string[] { config.ResourceId };

                // This Azure AD call uses both client info (app) and web api info (resourceIds)
                AuthenticationResult authResult = await app.AcquireTokenForClient(resourceIds).ExecuteAsync();
                azureADInfo.WebApiEndpoint = config.WebApiEndpoint;
                azureADInfo.AccessToken = authResult?.AccessToken ?? string.Empty;
            }
            return azureADInfo;
        }

        // 2023-11-25: created a user at Azure, but this method is not used in this app, just as an FYI.
        //              See MsalClientException in AzureService.cs.
        public static async Task<AzureADInfo> GetAzureADInfoUserNamePasswordAsync()
        {
            var azureADInfo = new AzureADInfo();
            AuthConfig? config = ReadFromJsonFile(@"AzureADClientSecret\appsettings.json");
            if (config != null)
            {
                IPublicClientApplication app = PublicClientApplicationBuilder
                                                    .Create(config.ClientId)
                                                    .WithAuthority(AzureCloudInstance.AzurePublic, config.TenantId)
                                                    .Build();
                string username = "shazamappuser";
                string password = "<password>";
                AuthenticationResult authResult = await app.AcquireTokenByUsernamePassword(
                                                                new string[] { "User.Read" }, username, password)
                                                            .ExecuteAsync();
                azureADInfo.WebApiEndpoint = config.WebApiEndpoint;                
                azureADInfo.AccessToken = authResult?.AccessToken ?? string.Empty;
            }
            return azureADInfo;
        }

        private static AuthConfig? ReadFromJsonFile(string path)
        {
            IConfiguration Configuration;

            var builder = new ConfigurationBuilder()
                                  .SetBasePath(Directory.GetCurrentDirectory())
                                  .AddJsonFile(path);

            Configuration = builder.Build();

            return Configuration.Get<AuthConfig>();
        }
    }
}
