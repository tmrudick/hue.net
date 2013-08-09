using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Hue
{
    public class Bridge
    {
        private const string DISCOVERY_URL = "http://www.meethue.com/api/nupnp";
        private string ipAddress = null;
        private string applicationName = null;
        internal HttpClient client = new HttpClient();

        public event EventHandler ButtonRequired;

        public Bridge(string applicationName, string ipAddress)
        {
            this.ipAddress = ipAddress;
            this.applicationName = applicationName;
        }

        public Bridge(string applicationName) : this(applicationName, null) 
        {
        }

        public Bridge()
        {
        }

        public async Task<string> Discover()
        {
            HttpResponseMessage response = await this.client.GetAsync(DISCOVERY_URL);
            string body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                JArray json = JArray.Parse(body);

                if (json.Count == 0)
                {
                    throw new Exception("Cannot find bridge");
                }
                else
                {
                    var obj = (JObject)json[0];
                    this.ipAddress = (string)obj["internalipaddress"];

                    return this.ipAddress;
                }
            }
            else
            {
                throw new Exception(body);
            }
        }

        public async Task<string> Register()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Light>> GetLights()
        {
            string result = await client.GetStringAsync(string.Format("http://{0}/api/{1}/lights", this.ipAddress, this.applicationName));
            JObject json = JsonConvert.DeserializeObject<JObject>(result);

            List<Light> lights = new List<Light>();

            foreach (JProperty property in json.Properties())
            {
                var light = await GetLight(property.Name);
                lights.Add(light);
            }

            return lights;
        }

        public async Task<Light> GetLight(string id)
        {
            string lightStr = await client.GetStringAsync(string.Format("http://{0}/api/{1}/lights/{2}", this.ipAddress, this.applicationName, id));

            return JsonConvert.DeserializeObject<Light>(lightStr);
        }

        public async Task<Light> SetLights(IEnumerable<Light> lights)
        {
            throw new NotImplementedException();
        }

        public async Task<Light> SetLight(IEnumerable<Light> light)
        {
            throw new NotImplementedException();
        }
    }
}
