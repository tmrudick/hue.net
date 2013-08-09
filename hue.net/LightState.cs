using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hue
{
    public class LightState
    {
        public bool On { get; set; }
        public int Hue { get; set; }

        [JsonProperty("sat")]
        public int Saturation { get; set; }

        [JsonProperty("bri")]
        public int Brightness { get; set; }
    }
}
