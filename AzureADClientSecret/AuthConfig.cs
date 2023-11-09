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
        public string Authority => string.Format(CultureInfo.InvariantCulture,   Instance, TenantId);               
        public string ClientSecret { get; set; } = string.Empty;
        public string RestApiEndpoint { get; set; } = string.Empty;        
        public string ResourceId { get; set; } = string.Empty;
        
        public static async Task<AzureADInfo> GetAzureADInfoAsync()
        {            
            AzureADInfo azureADInfo = new ();
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
                azureADInfo.RestApiEndpoint = config.RestApiEndpoint;
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
