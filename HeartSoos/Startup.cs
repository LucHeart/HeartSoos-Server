using System.Net;
using System.Net.Sockets;
using LucHeart.HeartSoos.Config;
using LucHeart.LoggingHelper;
using Newtonsoft.Json;

namespace LucHeart.HeartSoos;

public class Startup
{
    public Startup(IConfiguration configuration) => Configuration = configuration;

    public static IConfiguration Configuration { get; set; }


    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        });
        services.AddCors();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        ApplicationLogging.LoggerFactory = loggerFactory;
        var logger = ApplicationLogging.CreateLogger<Startup>();
        app.UseLoggingHelperWithRequestLogging();
        if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

        app.UseWebSockets();
        app.UseRouting();
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

        var ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList
            .First(x => x.AddressFamily == AddressFamily.InterNetwork);
        logger.LogInformation("Local IP: {Ip}", ip);
        logger.LogInformation("Default Websocket URL: {Url}", $"ws://{ip}:666/ws/default");
        
        foreach (var pushoverConfig in HeartSoosConfig.Config.Paths)
        foreach (var pushover in pushoverConfig.OscConfig)
            HeartRateManager.AddVrChatOSC(pushoverConfig.Name, pushover.ParameterPath);
    }
}