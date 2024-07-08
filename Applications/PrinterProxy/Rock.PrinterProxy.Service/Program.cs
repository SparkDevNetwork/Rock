// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.Extensions.Options;

namespace Rock.PrinterProxy.Service;

public class Program
{
    public static int Main( string[] args )
    {
        var builder = Host.CreateApplicationBuilder( args );

        builder.Services.AddWindowsService( options =>
        {
            options.ServiceName = "Rock Printer Proxy Service";
        } );

        if ( OperatingSystem.IsWindows() )
        {
            LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>( builder.Services );
        }

        builder.Services.Configure<PrinterProxyOptions>( builder.Configuration );
        builder.Services.AddHostedService<ProxyWorker>();

        var host = builder.Build();

        var proxyOptions = host.Services.GetRequiredService<IOptions<PrinterProxyOptions>>();

        if ( !WindowsServiceHelpers.IsWindowsService() )
        {
            // Make sure we have the required configuration.
            if ( string.IsNullOrWhiteSpace( proxyOptions.Value.Url ) )
            {
                Console.Error.WriteLine( "Invalid configuration: Host must be specified." );
                Console.WriteLine();

                ShowHelp();

                return 1;
            }

            if ( string.IsNullOrWhiteSpace( proxyOptions.Value.Id ) )
            {
                Console.Error.WriteLine( "Invalid configuration: Id must be specified." );
                Console.WriteLine();
                ShowHelp();

                return 1;
            }
        }

        host.Run();

        return 0;
    }

    /// <summary>
    /// Displays the help text when executed from the command line.
    /// </summary>
    private static void ShowHelp()
    {
        var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

        Console.WriteLine( $"Usage: {processName} [OPTION]..." );
        Console.WriteLine( "Hosts the printer proxy for a Rock server instance." );
        Console.WriteLine();
        Console.WriteLine( "  --url                  The URL of the Rock server" );
        Console.WriteLine( "  --id                   The device IdKey or Guid of the proxy to connect as" );
        Console.WriteLine( "  --priority             The numeric priority of this device" );
        Console.WriteLine( "                         (optional)" );
        Console.WriteLine( "  --name                 The friendly name of this proxy" );
        Console.WriteLine( "                         (optional)" );
        Console.WriteLine();
        Console.WriteLine( "Examples:" );
        Console.WriteLine( $"  {processName} --url https://rock.rocksolidchurchdemo.com --id da0BJR0Bpz" );
    }
}
