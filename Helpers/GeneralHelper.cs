using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CSharpWpfShazam.Helpers
{
    public static class GeneralHelper
    {         
        public static async Task ExecuteOpenUrlCommandAsync(string url, bool lanuchMSEdge = false)
        {
            if (!string.IsNullOrEmpty((string)url))
            {                               
                await Task.Run(() =>
                {
                    if (lanuchMSEdge)
                    {
                        url = $"microsoft-edge:{url}";
                    }

                    try
                    {
                        // https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/                        
                        // Suppressing the second command prompt,
                        // and escaping the “&” with “^&” so the shell does not treat them as command separators.                       
                        url = url.Replace("&", "^&");
                        Process.Start(new ProcessStartInfo("cmd", $"/c start {url}")
                        {
                            UseShellExecute = false, // Already false in .NET 6
                            CreateNoWindow = true
                        });
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message;
                    }
                });
            }
        }
    }
}
