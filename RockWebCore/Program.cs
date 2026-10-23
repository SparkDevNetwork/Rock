using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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
