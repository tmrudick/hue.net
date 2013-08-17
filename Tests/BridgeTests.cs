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

        [TestMethod]
        public async Task GetMultipleLights()
        {
            MockHttpHandler handler = new MockHttpHandler();
            handler.Expects(new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://10.10.10.1/api/testapp/lights")
                }).Responds("{\"1\":{\"name\": \"Hue Lamp 1\"}, \"2\":{\"name\": \"Hue Lamp 2\"}")
                .Expects(new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://10.10.10.1/api/testapp/lights/1")
                }).Responds("{\"state\": {\"on\":false,\"bri\":235,\"hue\":47212,\"sat\":195,\"xy\":[0.7640,0.9634],\"ct\":464,\"alert\":\"none\",\"effect\":\"none\",\"colormode\":\"ct\",\"reachable\":true}, \"type\": \"Extended color light\", \"name\": \"Hue Lamp 1\", \"modelid\": \"LCT001\", \"swversion\": \"66009663\", \"pointsymbol\": { \"1\":\"none\", \"2\":\"none\", \"3\":\"none\", \"4\":\"none\", \"5\":\"none\", \"6\":\"none\", \"7\":\"none\", \"8\":\"none\" }}")
                .Expects(new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://10.10.10.1/api/testapp/lights/2")
                }).Responds("{\"state\": {\"on\":true,\"bri\":10,\"hue\":256,\"sat\":100,\"xy\":[0.7640,0.9634],\"ct\":464,\"alert\":\"none\",\"effect\":\"none\",\"colormode\":\"ct\",\"reachable\":true}, \"type\": \"Extended color light\", \"name\": \"Hue Lamp 2\", \"modelid\": \"LCT001\", \"swversion\": \"66009663\", \"pointsymbol\": { \"1\":\"none\", \"2\":\"none\", \"3\":\"none\", \"4\":\"none\", \"5\":\"none\", \"6\":\"none\", \"7\":\"none\", \"8\":\"none\" }}");

            Bridge bridge = new Bridge("testapp", "10.10.10.1");
            bridge.client = handler.Client;

            List<Light> lights = (await bridge.GetLights()).ToList();

            Assert.AreEqual(2, lights.Count());

            Light light = lights[0];
            Assert.AreEqual("1", light.Id);
            Assert.AreEqual("Hue Lamp 1", light.Name);
            Assert.IsFalse(light.State.On);
            Assert.AreEqual(235, light.State.Brightness);
            Assert.AreEqual(47212, light.State.Hue);
            Assert.AreEqual(195, light.State.Saturation);

            light = lights[1];
            Assert.AreEqual("2", light.Id);
            Assert.AreEqual("Hue Lamp 2", light.Name);
            Assert.IsTrue(light.State.On);
            Assert.AreEqual(10, light.State.Brightness);
            Assert.AreEqual(256, light.State.Hue);
            Assert.AreEqual(100, light.State.Saturation);

            handler.Verify();
        }

        [TestMethod]
        public async Task GetSingleLight()
        {
            MockHttpHandler handler = new MockHttpHandler();
            handler.Expects(new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("http://10.10.10.1/api/testapp/lights/1")
                }).Responds("{\"state\": {\"on\":false,\"bri\":235,\"hue\":47212,\"sat\":195,\"xy\":[0.7640,0.9634],\"ct\":464,\"alert\":\"none\",\"effect\":\"none\",\"colormode\":\"ct\",\"reachable\":true}, \"type\": \"Extended color light\", \"name\": \"Hue Lamp 1\", \"modelid\": \"LCT001\", \"swversion\": \"66009663\", \"pointsymbol\": { \"1\":\"none\", \"2\":\"none\", \"3\":\"none\", \"4\":\"none\", \"5\":\"none\", \"6\":\"none\", \"7\":\"none\", \"8\":\"none\" }}");

            Bridge bridge = new Bridge("testapp", "10.10.10.1");
            bridge.client = handler.Client;

            Light light = await bridge.GetLight("1");
            Assert.AreEqual("1", light.Id);
            Assert.AreEqual("Hue Lamp 1", light.Name);
            Assert.IsFalse(light.State.On);
            Assert.AreEqual(235, light.State.Brightness);
            Assert.AreEqual(47212, light.State.Hue);
            Assert.AreEqual(195, light.State.Saturation);

            handler.Verify();
        }

        [TestMethod]
        public async Task SetSingleLight()
        {
            Light light = new Light() {
                Id = "1",
                Name = "Light 1",
                State = new LightState() {
                    On = true,
                    Brightness = 75
                }
            };

            MockHttpHandler handler = new MockHttpHandler();
            handler.Expects(new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                Content = new StringContent("{\"on\":true,\"hue\":0,\"sat\":0,\"bri\":75}"),
                RequestUri = new Uri("http://10.10.10.1/api/testapp/lights/1/state")
            }).Responds("{\"success\": {}}");

            Bridge bridge = new Bridge("testapp", "10.10.10.1");
            bridge.client = handler.Client;

            await bridge.SetLight(light);

            handler.Verify();
        }

        [TestMethod]
        public async Task SetMultipleLights()
        {
            List<Light> lights = new List<Light>();
            lights.Add(new Light()
            {
                Id = "1",
                State = new LightState()
                {
                    On = true
                }
            });
            lights.Add(new Light()
            {
                Id = "2",
                State = new LightState()
                {
                    On = false
                }
            });

            MockHttpHandler handler = new MockHttpHandler();
            handler.Expects("http://10.10.10.1/api/testapp/lights/1/state", HttpMethod.Put, "{\"on\":true,\"hue\":0,\"sat\":0,\"bri\":0}")
                .Responds("{\"success\":{}}")
                .Expects("http://10.10.10.1/api/testapp/lights/2/state", HttpMethod.Put, "{\"on\":false,\"hue\":0,\"sat\":0,\"bri\":0}")
                .Responds("{\"success\":{}}");

            Bridge bridge = new Bridge("testapp", "10.10.10.1");
            bridge.client = handler.Client;

            await bridge.SetLights(lights);

            handler.Verify();
        }
    }
}
