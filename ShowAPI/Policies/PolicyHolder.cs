using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Polly;

namespace ShowAPI.Policies
{
    public static class PolicyHolder
    {
        private static HttpStatusCode[] TransientStatusCodes => new[]
        {
            HttpStatusCode.TooManyRequests,
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.GatewayTimeout
        };
        
        public static IAsyncPolicy<HttpResponseMessage> GetDefaultPolicy(int retryCount = 5, int timeoutSeconds = 180)
        {
            var retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => TransientStatusCodes.Contains(r.StatusCode))
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(retryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(timeoutSeconds));
            return timeoutPolicy.WrapAsync(retryPolicy);
        }
    }
}