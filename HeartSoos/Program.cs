using Serilog;

namespace LucHeart.HeartSoos;

public static class Program
{
    public static async Task Main(string[] args) => await Host.CreateDefaultBuilder(args)
        .UseSerilog((context, _, config) => config.ReadFrom.Configuration(context.Configuration))
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseKestrel();
            webBuilder.ConfigureKestrel(serverOptions =>
            {
                serverOptions.ListenAnyIP(666);
                serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMilliseconds(3000);
            });

            webBuilder.UseStartup<Startup>();
        }).Build().RunAsync();
}