using Microsoft.AspNetCore.Server.Kestrel.Core;
using ServerHP.GrpcServer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5024, o => o.Protocols = HttpProtocols.Http2);
});

var app = builder.Build();

app.MapGrpcService<LatencyServiceImpl>(); 

app.MapGet("/", () => "gRPC server ready!");
app.Run();