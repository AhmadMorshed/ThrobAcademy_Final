using System.Text.Json.Serialization;

namespace Throb.Service.Models
{
    public class ParticipantDetails
    {
        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }

        [JsonPropertyName("user_email")]
        public string? UserEmail { get; set; }

        [JsonPropertyName("user_name")]
        public string? UserName { get; set; }
    }

    public class WebhookPayloadObject
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; } // معرّف الاجتماع (Meeting ID)

        [JsonPropertyName("uuid")]
        public string? Uuid { get; set; }

        [JsonPropertyName("participant")]
        public ParticipantDetails? Participant { get; set; }
    }

    public class ZoomWebhookPayload
    {
        [JsonPropertyName("event")]
        public string? Event { get; set; } // مثال: meeting.participant_joined

        [JsonPropertyName("payload")]
        public WebhookPayloadObject? Payload { get; set; }
    }
}