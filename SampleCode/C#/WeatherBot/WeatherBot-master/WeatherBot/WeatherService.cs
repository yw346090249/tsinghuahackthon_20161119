using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace WeatherBot
{
    public class WeatherService
    {
        public static async Task<WeatherData> GetWeatherfromService(string city)
        {
            string url = "http://api.openweathermap.org/data/2.5/weather?q=" + city + "&appid=cb881b3004e248e78c4a7585bb577eb9";
            string response;
            string tmp;
            WeatherData wd = new WeatherData();

            if (string.IsNullOrWhiteSpace(city))
            {
                return null;
            }

            using (WebClient client = new WebClient())
            {
                 response = await client.DownloadStringTaskAsync(url);
            }

            JsonReader reader = new JsonTextReader(new StringReader(response));

            JObject rss = JObject.Parse(response);
        

            tmp = (string)rss["main"]["temp"];

            double ctmp = Convert.ToDouble(tmp)- 273.15;
            wd.temp = String.Format("{0:f1}",ctmp);
            wd.weathermain = (string)rss["weather"][0]["main"];
            wd.weatherdescription= (string)rss["weather"][0]["description"];
            wd.city = (string)rss["name"];
            return wd;
             
        }
    }
}