using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hue.Tests
{
    public class MockHttpHandler : DelegatingHandler
    {
        private List<HttpResponseMessage> responses = new List<HttpResponseMessage>();
        private List<HttpRequestMessage> requests = new List<HttpRequestMessage>();
      
        public HttpClient Client
        {
            get
            {
                return new HttpClient(this);
            }
        }

        public MockHttpHandler Expects(HttpRequestMessage msg)
        {
            this.requests.Add(msg);

            return this;
        }

        public MockHttpHandler Expects(string url, HttpMethod method, string content)
        {
            this.requests.Add(new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = method,
                Content = new StringContent(content)
            });

            return this;
        }

        public MockHttpHandler Expects(List<HttpRequestMessage> msgs)
        {
            this.requests.AddRange(msgs);

            return this;
        }

        public MockHttpHandler Responds(HttpResponseMessage msg)
        {
            this.responses.Add(msg);

            return this;
        }

        public MockHttpHandler Responds(List<HttpResponseMessage> msgs)
        {
            this.responses.AddRange(msgs);

            return this;
        }

        public MockHttpHandler Responds(string body)
        {
            this.responses.Add(new HttpResponseMessage() { Content = new StringContent(body) });

            return this;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            HttpRequestMessage expectedRequest = null;

            if (this.responses.Count > 0)
            {
                response = this.responses.First();
                this.responses.RemoveAt(0);
            }
            else
            {
                throw new Exception("No more expected responses?");
            }

            if (this.requests.Count > 0)
            {
                expectedRequest = this.requests.First();
                this.requests.RemoveAt(0);
            }

            if (expectedRequest != null)
            {
                Assert.AreEqual(expectedRequest.Method, request.Method);
                Assert.AreEqual(expectedRequest.RequestUri, request.RequestUri);

                if (expectedRequest.Content != null)
                {
                    string expectedContent = await expectedRequest.Content.ReadAsStringAsync();
                    string actualContent = await request.Content.ReadAsStringAsync();
                    Assert.AreEqual(expectedContent, actualContent);
                }
            }

            return response;
        }

        public void Verify()
        {
            Assert.AreEqual(0, this.requests.Count);
            Assert.AreEqual(0, this.responses.Count);
        }
    }
}
