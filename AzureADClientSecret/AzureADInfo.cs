namespace CSharpWpfShazam.AzureADClientSecret
{
    public class AzureADInfo
    {
        // https://localhost:7025/songrepo in AzureADClientSecret\appsettings.json (copied to runtime folder)
        public string WebApiEndpoint { get; set; } = string.Empty;
        // Bearer token
        public string AccessToken { get; set; } = string.Empty;
    }
}
