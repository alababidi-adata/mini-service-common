using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace Common.Tests
{
    public static class TestExtensions
    {
        public static async Task<HttpResponseMessage> PatchJsonAsync<T>(this HttpClient client, string requestUri, T model)
        {
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.Default, "application/json");
            return await client.PatchAsync(requestUri, content);
        }

        public static async Task<T?> TryReadFromJsonAsync<T>(this HttpContent content)
        {
            try
            {
                return await content.ReadFromJsonAsync<T>();
            }
            catch
            {
                return default;
            }
        }

        public static async Task IsSucceed(this HttpResponseMessage apiResponse)
        {
            if (apiResponse.IsSuccessStatusCode) return;

            var newLine = Environment.NewLine;

            string? errorInfo = null;
            var errorModel = await apiResponse.Content.TryReadFromJsonAsync<ErrorModel>();
            if (errorModel != null)
            {
                var mainError = $".{newLine}{errorModel.message}:{newLine}";
                var nestedErrors = !errorModel.reasons.Any() ? "" : $" - {string.Join(newLine + " - ", errorModel.reasons.Select(_ => _.message))}{newLine}";
                errorInfo = mainError + nestedErrors;
            }

            var reason = $"{errorInfo}.{newLine} Status code: {apiResponse.StatusCode} ({(int)apiResponse.StatusCode}){newLine}";

            apiResponse.IsSuccessStatusCode.Should().BeTrue(reason);
        }

        /*
             {
                "localizedMessage": "Invalid request.",
                "internalMessage": null,
                "reasons": [
                            {
                                "reasons": [],
                                "message": "'Id' must be equal to '0'.",
                                "metadata": {}
                            }
                ],
                "message": "Invalid request.",
                "metadata": {}
            }
        */

        private class ErrorModel
        {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            public string localizedMessage { get; set; }
            public object internalMessage { get; set; }
            public Descriptor descriptor { get; set; }
            public Reason[] reasons { get; set; }
            public string message { get; set; }
            public Dictionary<string, object> metadata { get; set; }
        }

        private class Descriptor
        {
            public string code { get; set; }
            public string errorDescription { get; set; }
            public int httpCode { get; set; }
            public int grpcCode { get; set; }
        }

        private class Reason
        {
            public object[] reasons { get; set; }
            public string message { get; set; }
            public Dictionary<string, object> metadata { get; set; }
        }
    }
}
