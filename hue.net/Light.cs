using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hue
{
    public class Light
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public LightState State { get; set; }
    }
}
