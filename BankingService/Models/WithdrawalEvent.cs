using System;
using System.Text.Json.Serialization;

namespace BankingService.Models
{
    public record WithdrawalEvent
    {
        [JsonPropertyName("eventId")]
        public Guid EventId { get; init; } = Guid.NewGuid();

        [JsonPropertyName("eventType")]
        public string EventType { get; init; } = "WITHDRAWAL";

        [JsonPropertyName("schemaVersion")]
        public string SchemaVersion { get; init; } = "1.0";

        [JsonPropertyName("accountId")]
        public long AccountId { get; init; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; init; }

        [JsonPropertyName("status")]
        public string Status { get; init; }

        [JsonPropertyName("occurredAt")]
        public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    }
}