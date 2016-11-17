using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft;
using Newtonsoft.Json;

namespace WeatherBot
{
    public class LuisService
    {
        public static async Task<LuisData> ParseUserInput(string input)
        {
            string queryString = String.Empty;
            queryString = "https://api.projectoxford.ai/luis/v1/application?id=78abfe80-d8b9-4319-b6ec-185fca9a6e49&subscription-key=746f4447ce6e4e98b597eaace9fb599c&q=" + Uri.EscapeDataString(input);
            LuisData lData;

            using (var client = new HttpClient())
            {
                HttpResponseMessage msg = await client.GetAsync(queryString);

                if(msg.IsSuccessStatusCode)
                {
                    var jsonResponse = await msg.Content.ReadAsStringAsync();

                    lData = JsonConvert.DeserializeObject<LuisData>(jsonResponse);
                    return lData;
                }
                else
                    return null;
            }
        } 
    }    
}