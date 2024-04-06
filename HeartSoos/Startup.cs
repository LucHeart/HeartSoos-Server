using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using LucHeart.HeartSoos.Config;

namespace LucHeart.HeartSoos;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddControllers().AddJsonOptions(x =>
        {
            x.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        
        services.AddCors();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        ApplicationLogging.LoggerFactory = loggerFactory;
        var logger = ApplicationLogging.CreateLogger<Startup>();
        
        if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

        var webSocketOptions = new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromMinutes(2)
        };

        app.UseWebSockets(webSocketOptions);
        app.UseRouting();
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

        var ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList
            .First(x => x.AddressFamily == AddressFamily.InterNetwork);
        logger.LogInformation("Local IP: {Ip}", ip);
        logger.LogInformation("Default Websocket URL (IP): {Url}", $"ws://{ip}:666/ws/default");
        logger.LogInformation("Default Websocket URL (DN): {Url}", $"ws://{Dns.GetHostName()}:666/ws/default");
        
        foreach (var pushoverConfig in HeartSoosConfig.Config.Paths)
        foreach (var pushover in pushoverConfig.OscConfig)
            HeartRateManager.AddVrChatOSC(pushoverConfig.Name, pushover.ParameterPath);
    }
}