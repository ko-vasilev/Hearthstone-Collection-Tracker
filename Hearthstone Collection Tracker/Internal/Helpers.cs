
using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;

namespace Hearthstone_Collection_Tracker.Internal
{
    public static class Helpers
    {
        public static int Clamp(this int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static async Task<Version> GetLatestVersion()
        {
            const string latestReleaseRequestUrl =
                @"https://api.github.com/repos/ko-vasilev/Hearthstone-Collection-Tracker/releases/latest";

            try
            {
                string versionStr;
                using (var wc = new WebClient())
                {
                    wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                    versionStr = await wc.DownloadStringTaskAsync(latestReleaseRequestUrl);
                }
                var versionObj = JsonConvert.DeserializeObject<GithubRelease>(versionStr);
                return versionObj == null ? null : new Version(versionObj.TagName);
            }
            catch (Exception)
            {
            }
            return null;
        }

        public static string GetValidFileName(this string fileName)
        {
            StringBuilder initialString = new StringBuilder(fileName);
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                initialString = initialString.Replace(c, '_');
            }
            return initialString.ToString();
        }

        private class GithubRelease
        {
            [JsonProperty("tag_name")]
            public string TagName { get; set; }
        }
    }
}
