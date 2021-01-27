using System.Text.Json.Serialization;

namespace SharPiDelivery
{
    /// <summary>Represents a Pi digit response.</summary>
    public class PiDigitResponse
    {
        /// <summary>Gets or sets the starting index of the retrieved digits.</summary>
        public int StartingIndex { get; set; }

        /// <summary>Gets or sets the retrieved digits.</summary>
        [JsonPropertyName("content")]
        public string Digits { get; set; }

        /// <summary>The length of the retrieved digits.</summary>
        public int Length => Digits.Length;
    }
}
