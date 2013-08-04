using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

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
            bridge.client = handler.Client;
            
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
            bridge.client = handler.Client;

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
        public async Task GetLightsReturnsLightsWithoutLights()
        {
            MockHttpHandler handler = new MockHttpHandler();
            handler.Responds("{}")
                .Expects(new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://10.10.10.1/api/testapp/lights")
                });

            Bridge bridge = new Bridge("testapp", "10.10.10.1");
            bridge.client = handler.Client;

            IEnumerable<Light> lights = await bridge.GetLights();

            Assert.AreEqual(0, lights.Count());

            handler.Verify();
        }
    }
}
