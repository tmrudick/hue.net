﻿using Newtonsoft.Json;
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
        private HttpClient client = new HttpClient();

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

        public async void Discover()
        {
            HttpResponseMessage response = await this.client.GetAsync(DISCOVERY_URL);
            string body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // HACK: This sucks
                body = body.Substring(1, body.Length - 2);
                dynamic json = JObject.Parse(body);
                this.ipAddress = json.internalipaddress;
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
            return JsonConvert.DeserializeObject<IEnumerable<Light>>(result);
        }

        public async Task<Light> GetLight(string id)
        {
            throw new NotImplementedException();
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
