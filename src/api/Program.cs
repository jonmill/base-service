using api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel((context, serverOptions) =>
{
    serverOptions.Listen(System.Net.IPAddress.Loopback, 7224, opts =>
    {
        opts.UseHttps();
    });
});

builder.Services.AddGrpc(opts =>
{
    opts.EnableDetailedErrors = true;
});

var app = builder.Build();

app.UseRouting();
app.UseGrpcWeb(new GrpcWebOptions() { DefaultEnabled = true });
app.UseEndpoints(eps =>
{
    eps.MapGrpcService<GreeterService>().EnableGrpcWeb();
});

app.Run();
