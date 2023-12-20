using System;
using System.Text;
using Autofac.Extensions.DependencyInjection;
using Bedrock.Framework;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using Skateboard3Server.Host.Blaze;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Skateboard3Server.Host;

public class Program
{
    public static void Main(string[] args)
    {
        //Setup NLog
        LogManager.Setup().LoadConfigurationFromAppSettings();

        try
        {
            CreateHostBuilder(args).Build().Run();
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            LogManager.Shutdown();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Trace);
            })
            .UseNLog()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        //gosredirector (Blaze) [TCP]
                        serverOptions.ListenAnyIP(42100,
                            options =>
                            {
                                options.UseConnectionLogging(loggingFormatter: HexLoggingFormatter)
                                    .UseConnectionHandler<BlazeConnectionHandler>();
                            });
                        //eadpgs-blapp001 (Blaze) [TCP]
                        //This normally is ssl, but the gosredirector response tells the game server is not ssl
                        serverOptions.ListenAnyIP(10744,
                            options =>
                            {
                                options.UseConnectionLogging(loggingFormatter: HexLoggingFormatter)
                                    .UseConnectionHandler<BlazeConnectionHandler>();
                            });
                        //downloads.skate.online (HTTP) [TCP]
                        //Set in appsettings.json
                        //serverOptions.ListenAnyIP(80);

                        //TODO: figure out if we need these
                        //gostelemetry.lobby [TCP]?
                        //serverOptions.ListenAnyIP(9946,
                        //    options =>
                        //    {
                        //        options.UseConnectionLogging(loggingFormatter: HexLoggingFormatter)
                        //            .UseConnectionHandler<DummyConnectionHandler>();
                        //    });
                        //tick server [TCP]?
                        //serverOptions.ListenAnyIP(8999,
                        //    options =>
                        //    {
                        //        options.UseConnectionLogging(loggingFormatter: HexLoggingFormatter)
                        //            .UseConnectionHandler<DummyConnectionHandler>();
                        //    });
                    })
                    .UseStartup<Startup>();
            });
    }


    private static void HexLoggingFormatter(ILogger logger, string method, ReadOnlySpan<byte> buffer)
    {
        if (!logger.IsEnabled(LogLevel.Trace))
            return;

        var builder = new StringBuilder($"{method}[{buffer.Length}] ");

        // Write the hex
        foreach (var b in buffer)
        {
            builder.Append(b.ToString("X2"));
            builder.Append(" ");
        }

        logger.LogTrace(builder.ToString());
    }
}

public static class ConnectionBuilderExtensions
{
    public static TBuilder UseBlazeServerSsl<TBuilder>(this TBuilder builder, BlazeTlsOptions options)
        where TBuilder : IConnectionBuilder
    {
        builder.Use(next =>
        {
            var middleware = new BlazeSslServerMiddleware(next, options);
            return middleware.OnConnectionAsync;
        });
        return builder;
    }
}