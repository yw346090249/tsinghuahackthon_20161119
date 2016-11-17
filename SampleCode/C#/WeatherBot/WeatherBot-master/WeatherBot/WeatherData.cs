using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeatherBot
{
    public class WeatherData
    {
        public string weathermain { get; set; }
        public string weatherdescription { get; set; }
        public string temp { get; set; }
        public string humidity { get; set; }
        public string pressure { get; set; }
        public string temp_min { get; set; }
        public string temp_max { get; set; }
        public string windspeed { get; set; }
        public string winddeg { get; set; }
        public string city { get; set;}
    }
}
