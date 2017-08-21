using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AiApiSdk.Logging
{
    public class LoggingHandler : DelegatingHandler
    {
        public LoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine("Request:");
            System.Diagnostics.Debug.WriteLine(request.ToString());
            if (request.Content != null)
            {
                System.Diagnostics.Debug.WriteLine(await request.Content.ReadAsStringAsync());
            }
            System.Diagnostics.Debug.WriteLine("");

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            System.Diagnostics.Debug.WriteLine("Response:");
            System.Diagnostics.Debug.WriteLine(response.ToString());
            if (response.Content != null)
            {
                System.Diagnostics.Debug.WriteLine(await response.Content.ReadAsStringAsync());
            }
            System.Diagnostics.Debug.WriteLine("");

            return response;
        }
    }
}