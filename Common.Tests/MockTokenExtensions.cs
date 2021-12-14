using System.Collections.Generic;
using System.Net.Http;

namespace VH.MiniService.Common.Tests
{
    public static class MockTokenExtensions
    {
        /// <summary>
        /// Mock access token for test HttpClient
        /// </summary>
        public static void SetAccessToken(this HttpClient client, IDictionary<string, object> claims)
        {
            // TODO: Use authorization token from Task 54118: Research and add Authorization to Microservice template (service-template-c-sharp)
            foreach (var (type, value) in claims)
            {
                client.DefaultRequestHeaders.Add(type, value.ToString());
            }
        }
    }
}
