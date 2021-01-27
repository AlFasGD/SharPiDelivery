using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharPiDelivery
{
    /// <summary>Encapsulates retrieval of Pi digits.</summary>
    public sealed class PiDelivery
    {
        private const string baseAPIAddress = "https://api.pi.delivery/v1/";

        /// <summary>The max number of digits that can be retrieved with each request.</summary>
        public const int DigitLimit = 1000;

        private HttpClient client;

        public PiDelivery()
        {
            client = new HttpClient
            {
                BaseAddress = new(baseAPIAddress),
            };
        }

        /// <summary>Gets the requested number of digits starting from the specified starting index.</summary>
        /// <param name="startingIndex">The index of the first digit to start from. That index is equivalent to the respective digit at magnitude 10 ^ (-index). For example, from index 0 the resulting content is "31415...", whereas from index 1 it is "1415...".</param>
        /// <param name="digits">The number of digits to request from the server. Multiple requests will be sent in the case the digits exceeds the limit, which is equal to <seealso cref="DigitLimit"/>.</param>
        /// <returns>A collection of the digit sequences that were asynchronously retrieved, in order.</returns>
        public async IAsyncEnumerable<string> GetDigits(long startingIndex, int digits)
        {
            var options = new PiDeliveryRequestOptions(startingIndex, digits);

            for (int offset = 0; offset < digits; offset += DigitLimit)
            {
                int remaining = Math.Min(digits - offset, DigitLimit);
                yield return await GetLimitedDigits(startingIndex + offset, remaining, options);
            }
        }

        private async Task<string> GetLimitedDigits(long startingIndex, int digits, PiDeliveryRequestOptions options)
        {
            if (startingIndex < 0)
                throw new ArgumentException("The starting index may not be a negative number.");

            if (digits < 0)
                throw new ArgumentException("The number of digits may not be a negative number.");

            if (digits > DigitLimit)
                throw new ArgumentException($"The number of digits may not be greater than {DigitLimit}.");

            options.StartingIndex = startingIndex;
            options.Digits = digits;

            var streamTask = (await GetEndpointResponse("pi", options.ToURLOptionDictionary())).Content.ReadAsStringAsync();
            try
            {
                return JsonSerializer.Deserialize<PiDigitResponse>(await streamTask).Digits;
            }
            catch (Exception e)
            {
                throw new Exception("The requested statring index was beyond the valid range.");
            }
        }
        private async Task<HttpResponseMessage> GetEndpointResponse(string endpoint, Dictionary<string, string> urlOptions)
        {
            var urlString = new StringBuilder($"{baseAPIAddress}{endpoint}?");

            foreach (var option in urlOptions)
                urlString.Append(option.Key).Append('=').Append(option.Value).Append('&');
            
            urlString.Remove(urlString.Length - 1, 1);

            var requestMessage = new HttpRequestMessage()
            {
                RequestUri = new(urlString.ToString()),
            };
            return await client.SendAsync(requestMessage);
        }

        private sealed class PiDeliveryRequestOptions
        {
            private const string startingIndexHeaderName = "start";
            private const string digitsHeaderName = "numberOfDigits";

            [JsonPropertyName(startingIndexHeaderName)]
            public long StartingIndex { get; set; }
            [JsonPropertyName(digitsHeaderName)]
            public int Digits { get; set; }

            public PiDeliveryRequestOptions()
                : this(0, 100) { }
            public PiDeliveryRequestOptions(long startingIndex, int digits)
            {
                StartingIndex = startingIndex;
                Digits = digits;
            }

            public Dictionary<string, string> ToURLOptionDictionary()
            {
                return new()
                {
                    [startingIndexHeaderName] = StartingIndex.ToString(),
                    [digitsHeaderName] = Digits.ToString(),
                };
            }
        }
    }
}
