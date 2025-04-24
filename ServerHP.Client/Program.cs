using Latency;
using ServerHP.Client.Components;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5193); // écoute sur le port 5000
});

builder.Services.AddGrpcClient<LatencyService.LatencyServiceClient>(o =>
{
    o.Address = new Uri("http://grpc-server:5024");
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.UseAntiforgery();     // <-- ajouter CE middleware ici

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();