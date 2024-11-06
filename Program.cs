using System.Diagnostics;

var builder = WebApplication.CreateBuilder();
const int HTTP_PORT = 8088;
const int HTTPS_PORT = 8443;
builder.WebHost.UseKestrel(options =>
{

    options.ListenLocalhost(HTTP_PORT);
    options.ListenLocalhost(HTTPS_PORT, listenOptions =>
    {
        listenOptions.UseHttps();
        if (Debugger.IsAttached)
        {
            listenOptions.UseConnectionLogging();
        }
    });
});


builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Information);


builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

var app = builder.Build();

app.UseServiceModel(serviceBuilder =>
{

    var wsHttpBindingWithCredential = new WSHttpBinding(SecurityMode.TransportWithMessageCredential);
    wsHttpBindingWithCredential.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
    serviceBuilder.AddService<Service>(serviceOptions =>
    {
        serviceOptions.BaseAddresses.Add(new Uri($"http://localhost:{HTTP_PORT}/EchoService"));
        serviceOptions.BaseAddresses.Add(new Uri($"https://localhost:{HTTPS_PORT}/EchoService"));
    })
    .AddServiceEndpoint<Service, IService>(new WSHttpBinding(SecurityMode.None), "/wsHttp")
    .AddServiceEndpoint<Service, IService>(new WSHttpBinding(SecurityMode.Transport), "/wsHttps")
    .AddServiceEndpoint<Service, IService>(wsHttpBindingWithCredential, "/wsHttpUserPassword", ep => ep.Name = "AuthenticatedEP");
    serviceBuilder.ConfigureServiceHostBase<Service>(CustomUsernamePasswordValidator.AddToHost);

    var serviceMetadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
    serviceMetadataBehavior.HttpsGetEnabled = serviceMetadataBehavior.HttpGetEnabled = true;
});

app.Run();
