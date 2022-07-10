using LucHeart.HeartSoos;
using Serilog;

HostBuilder builder = new();
builder.UseContentRoot(Directory.GetCurrentDirectory())
    .ConfigureHostConfiguration(config =>
    {
        config.AddEnvironmentVariables(prefix: "DOTNET_");
        if (args is { Length: > 0 }) config.AddCommandLine(args);
    })
    .ConfigureAppConfiguration((_, config) =>
    {
        config.AddEnvironmentVariables();
        if (args is { Length: > 0 }) config.AddCommandLine(args);
    })
    .UseDefaultServiceProvider((context, options) =>
    {
        var isDevelopment = context.HostingEnvironment.IsDevelopment();
        options.ValidateScopes = isDevelopment;
        options.ValidateOnBuild = isDevelopment;
    })
    .UseSerilog((_, config) =>
    {
        config.WriteTo
            .Console(
                outputTemplate:
                "[{Timestamp:HH:mm:ss.fff}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .Enrich.FromLogContext()
            .Enrich.WithCorrelationId()
            .MinimumLevel.Information();
    })
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseKestrel();
        webBuilder.ConfigureKestrel(serverOptions =>
        {
            serverOptions.ListenAnyIP(666);
            serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMilliseconds(3000);
        });

        webBuilder.UseStartup<Startup>();
    });

await builder.Build().RunAsync();