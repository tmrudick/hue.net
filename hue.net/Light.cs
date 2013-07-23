using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hue
{
    public class Light
    {
        public string Name { get; protected set; }
        public LightState State { get; protected set; }
    }
}
