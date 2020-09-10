using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using autoPublish.Core.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace autoPublish.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSerilog((context, config)=> 
            {
                config.Enrich.FromLogContext()
                  .MinimumLevel.Debug()
                  .MinimumLevel.Override("System", LogEventLevel.Information)
                  .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Debug).WriteTo.Console().WriteTo.Async(
                    a => a.File("logs/Debug-.txt", rollingInterval: RollingInterval.Day)
                ))
                .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Information).WriteTo.Console().WriteTo.Async(
                    a => a.File($"logs/Information-.txt", rollingInterval: RollingInterval.Day)
                ))
                .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Warning).WriteTo.Console().WriteTo.Async(
                    a => a.File("logs/Warning-.txt", rollingInterval: RollingInterval.Day)
                ))
                .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Error).WriteTo.Console().WriteTo.Async(
                    a => a.File("logs/Error-.txt", rollingInterval: RollingInterval.Day)
                ))
                .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Fatal).WriteTo.Console().WriteTo.Async(
                    a => a.File("logs/Fatal-.txt", rollingInterval: RollingInterval.Day)
                ));
            }).ConfigureWebHostDefaults(config =>
            {
                config.UseStartup<Startup>();
            });
    }
}
