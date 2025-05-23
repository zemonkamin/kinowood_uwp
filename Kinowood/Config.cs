using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinowood
{
    public static class Config
    {
        public const string ApiBaseUrl = "https://kinowood.ru/";
        public const string ApiEndpoint = "api/index.php";
        
        public static string GetSearchUrl(string query)
        {
            var url = $"{ApiBaseUrl}{ApiEndpoint}?q={Uri.EscapeDataString(query)}";
            System.Diagnostics.Debug.WriteLine($"Generated URL: {url}");
            return url;
        }
    }
}
