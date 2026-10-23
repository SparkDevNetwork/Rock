using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RockWebCore
{
    public class Program
    {
        public static void Main( string[] args )
        {
            CreateHostBuilder( args ).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder( string[] args ) =>
            Host.CreateDefaultBuilder( args )
                .ConfigureAppConfiguration( ( context, config ) =>
                {
                    // Machine specific settings that are not committed, such
                    // as connection strings. Insert after the other appsettings
                    // files so that environment variables and command line
                    // arguments still take priority.
                    var lastJsonIndex = config.Sources.ToList().FindLastIndex( s => s is JsonConfigurationSource );

                    config.Sources.Insert( lastJsonIndex + 1, new JsonConfigurationSource
                    {
                        Path = "appsettings.Local.json",
                        Optional = true,
                        ReloadOnChange = true
                    } );
                } )
                .ConfigureWebHostDefaults( webBuilder =>
                 {
                     // The web files (themes, scripts, styles) still live in
                     // the RockWeb folder, so serve that as the web root.
                     // The path is relative to the content root.
                     webBuilder.UseWebRoot( "../RockWeb" );
                     webBuilder.UseStartup<Startup>();
                 } );
    }
}
