using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace VH.MiniService.Common.Tests
{
    public static class TestExtensions
    {
        public static async Task<TResponse?> Get<TResponse>(this HttpClient client, string requestUri, HttpStatusCode? expectedCode = null)
        {
            using var response = await client.GetAsync(requestUri);
            return await response.VerifyAndRead<TResponse>(expectedCode);
        }

        public static Task Put(this HttpClient client, string requestUri, object requestBody, HttpStatusCode? expectedCode = null)
            => client.Put<object>(requestUri, requestBody, expectedCode);

        public static async Task<TResponse?> Put<TResponse>(this HttpClient client, string requestUri, object requestBody, HttpStatusCode? expectedCode = null)
        {
            using var response = await client.PutAsJsonAsync(requestUri, requestBody);
            return await response.VerifyAndRead<TResponse>(expectedCode);
        }

        public static Task Post(this HttpClient client, string requestUri, object requestBody, HttpStatusCode? expectedCode = null)
            => client.Post<object>(requestUri, requestBody, expectedCode);

        public static async Task<TResponse?> Post<TResponse>(this HttpClient client, string requestUri, object requestBody, HttpStatusCode? expectedCode = null)
        {
            using var response = await client.PostAsJsonAsync(requestUri, requestBody);
            return await response.VerifyAndRead<TResponse>(expectedCode);
        }

        public static Task Patch(this HttpClient client, string requestUri, object requestBody, HttpStatusCode? expectedCode = null)
            => client.Patch<object>(requestUri, requestBody, expectedCode);

        public static async Task<TResponse?> Patch<TResponse>(this HttpClient client, string requestUri, object requestBody, HttpStatusCode? expectedCode = null)
        {
            using var response = await client.PatchJsonAsync(requestUri, requestBody);
            return await response.VerifyAndRead<TResponse>(expectedCode);
        }

        public static async Task Delete(this HttpClient client, string requestUri, HttpStatusCode? expectedCode = null)
        {
            using var response = await client.DeleteAsync(requestUri);
            await response.IsSucceed();
        }

        private static async Task<TResponse?> VerifyAndRead<TResponse>(this HttpResponseMessage response, HttpStatusCode? expectedCode = null)
        {
            // output error if not expected code
            if (expectedCode != null && expectedCode != response.StatusCode)
                response.StatusCode.Should().Be(expectedCode, await response.Content.ReadAsStringAsync());

            // output error if not success
            if (expectedCode == null)
                await response.IsSucceed();

            // get expected model
            return response.StatusCode != HttpStatusCode.NoContent
                ? await response.Content.ReadFromJsonAsync<TResponse>()
                : default;
        }

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

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        // ReSharper disable UnusedMember.Local
        // ReSharper disable InconsistentNaming
        // ReSharper disable ClassNeverInstantiated.Local
        private class ErrorModel
        {
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
