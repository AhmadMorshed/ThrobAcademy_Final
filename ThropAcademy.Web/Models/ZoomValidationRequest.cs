namespace ThropAcademy.Web.Models
{
    public class ZoomValidationRequest
    {
        // هذه الحقول هي التي يتوقع Zoom أن تُعيدها في الرد
        [System.Text.Json.Serialization.JsonPropertyName("plainToken")]
        public string PlainToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("encryptedToken")]
        public string EncryptedToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("event")]
        public string Event { get; set; }
    }
}
