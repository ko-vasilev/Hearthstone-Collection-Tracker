using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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

        public static double GetAvgHue(this Bitmap bmp, double saturationThreshold = 0.05)
        {
            var totalHue = 0.0f;
            var validPixels = 0;
            for (var i = 0; i < bmp.Width; i++)
            {
                for (var j = 0; j < bmp.Height; j++)
                {
                    var pixel = bmp.GetPixel(i, j);

                    //ignore sparkle
                    if (pixel.GetSaturation() > saturationThreshold)
                    {
                        totalHue += pixel.GetHue();
                        validPixels++;
                    }
                }
            }

            return totalHue / validPixels;
        }

        public static double GetAvgBrightness(this Bitmap bmp, double saturationThreshold = 0.05)
        {
            var totalBrightness = 0.0f;
            var validPixels = 0;
            for (var i = 0; i < bmp.Width; i++)
            {
                for (var j = 0; j < bmp.Height; j++)
                {
                    var pixel = bmp.GetPixel(i, j);

                    //ignore sparkle
                    if (pixel.GetSaturation() > saturationThreshold)
                    {
                        totalBrightness += pixel.GetBrightness();
                        validPixels++;
                    }
                }
            }

            return totalBrightness / validPixels;
        }
    }
}
