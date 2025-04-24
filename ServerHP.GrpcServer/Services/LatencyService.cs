using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Latency;

namespace ServerHP.GrpcServer.Services;

public class LatencyServiceImpl : LatencyService.LatencyServiceBase
{
    public override Task<MessageResponse> SendMessage(MessageInfo request, ServerCallContext context)
    {
        var response = new MessageResponse
        {
            Id = request.Id,
            SentContent = request.SentContent,
            SentAt = request.SentAt,
            ReceivedAt = Timestamp.FromDateTime(DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc))
        };

        return Task.FromResult(response);
    }
    
    public override async Task StreamLatency(
        IAsyncStreamReader<MessageInfo> requestStream,
        IServerStreamWriter<MessageResponse> responseStream,
        ServerCallContext context)
    {
        await foreach (var request in requestStream.ReadAllAsync())
        {
            var response = new MessageResponse
            {
                Id = request.Id,
                SentContent = request.SentContent,
                SentAt = request.SentAt,
                ReceivedAt = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            await responseStream.WriteAsync(response);
        }
    }
}