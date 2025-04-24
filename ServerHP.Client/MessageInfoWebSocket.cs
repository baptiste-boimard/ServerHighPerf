namespace ServerHP.Client;

public class MessageInfoWebSocket
{
    public Guid Id { get; set; }
    public string? SentContent { get; set; }
    public string? ReceivedContent { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public TimeSpan? Latency => ReceivedAt.HasValue && SentAt.HasValue
        ? ReceivedAt.Value - SentAt.Value
        : null;
}