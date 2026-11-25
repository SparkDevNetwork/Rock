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
using System;

namespace Rock.BaselineTool;

/// <summary>
/// Console entry point for building the SQL migrations baseline from a
/// database that Rock version B built: verify, monitor, cleanup, export, package.
/// </summary>
internal class Program
{
    /// <summary>
    /// The console entry point.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>0 on success, 1 when a check stopped the command, 2 when the tool could not run.</returns>
    public static int Main( string[] args )
    {
        BaselineOptions options;

        try
        {
            options = BaselineOptions.Parse( args );
        }
        catch ( ArgumentException ex )
        {
            Console.Error.WriteLine( ex.Message );
            Console.Error.WriteLine();
            Console.Error.WriteLine( BaselineOptions.Usage );
            return 2;
        }

        try
        {
            switch ( options.Command )
            {
                case "verify":
                    return VerifyCommand.Run( options );

                case "monitor":
                    return MonitorCommand.Run( options );

                case "cleanup":
                    return CleanupCommand.Run( options );

                case "export":
                    return ExportCommand.Run( options );

                default:
                    return PackageCommand.Run( options );
            }
        }
        catch ( Exception ex )
        {
            Console.Error.WriteLine( $"{options.Command} failed: {ex}" );
            return 2;
        }
    }
}
