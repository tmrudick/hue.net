using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hue;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Moq;
using System.Net.Http;
using System.Threading;
using System.Net;

namespace Hue.Tests
{
    [TestClass]
    public class TestBridge
    {

        [TestMethod]
        public async Task nupnpDiscovery()
        {
            // Arrange mock handler requests/responses
            MockHttpHandler handler = new MockHttpHandler();
            handler.Responds("[{\"internalipaddress\":\"10.10.10.1\"}]")
                .Expects(new HttpRequestMessage
                    {
                        RequestUri = new Uri("http://www.meethue.com/api/nupnp"),
                        Method = HttpMethod.Get
                    });

            Bridge bridge = new Bridge();
            bridge.setHttpClient(handler.Client);
            
            // Try to discover the bridge and get the IP
            string ip = await bridge.Discover();

            // Assert that the IP we get is what we expect
            Assert.AreEqual("10.10.10.1", ip);

            // Verify that all of our expected requests executed
            handler.Verify();
        }

        [TestMethod]
        public async Task nupnpDiscoveryFails()
        {
            // Arrange mock handler requests/responses
            MockHttpHandler handler = new MockHttpHandler();
            handler.Responds("[]")
                .Expects(new HttpRequestMessage
                {
                    RequestUri = new Uri("http://www.meethue.com/api/nupnp"),
                    Method = HttpMethod.Get
                });

            Bridge bridge = new Bridge();
            bridge.setHttpClient(handler.Client);

            try
            {
                await bridge.Discover();
            }
            catch (Exception e)
            {
                Assert.AreEqual("Cannot find bridge", e.Message);
            }

            handler.Verify();
        }

        [TestMethod]
        public async Task GetLightsReturnsLights()
        {
            HttpResponseMessage msg = new HttpResponseMessage();

            Bridge bridge = new Bridge("tmrudick5991", "192.168.1.3");

            IEnumerable<Light> lights = await bridge.GetLights();           
        }
    }
}
