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
using System.Collections.Generic;

namespace Rock.BaselineTool;

/// <summary>
/// Checks the database and site before B's site is started: the database
/// is empty, the server is at least the minimum SQL Server version B
/// supports, and recurring jobs will not run. A newer server is fine:
/// the export targets the minimum version, and the baseline test proves
/// the scripts on every supported version.
/// </summary>
internal static class VerifyCommand
{
    /// <summary>
    /// Runs the checks and prints each problem.
    /// </summary>
    /// <param name="options">The parsed options.</param>
    /// <returns>0 when every check passes, otherwise 1.</returns>
    public static int Run( BaselineOptions options )
    {
        var site = new BaselineSite( options.RockWebPath );
        var problems = new List<string>();

        Console.WriteLine( $"Site Rock version: {site.GetRockVersion()}" );

        if ( !string.Equals( site.GetAppSetting( "RunJobsInIISContext" ), "false", StringComparison.OrdinalIgnoreCase ) )
        {
            problems.Add( "RunJobsInIISContext must be False in web.config so recurring jobs do not run during startup." );
        }

        using var database = new BaselineDatabase( BaselineSite.ResolveDatabase( options.Database, site ) );
        var tableCount = ( int ) database.Scalar( "SELECT COUNT(*) FROM [sys].[tables] WHERE [is_ms_shipped] = 0;" );
        var majorVersion = Convert.ToInt32( database.Scalar( "SELECT CONVERT( INT, SERVERPROPERTY( 'ProductMajorVersion' ) );" ) );

        Console.WriteLine( $"Database: {database.Connection.Database} on {database.Connection.DataSource}, SQL Server major version {majorVersion}" );

        if ( tableCount > 0 )
        {
            problems.Add( $"The database must be empty, but it has {tableCount} tables." );
        }

        if ( majorVersion < options.SqlMajorVersion )
        {
            problems.Add( $"The server must be SQL Server major version {options.SqlMajorVersion} (the minimum B supports) or newer, but it is {majorVersion}." );
        }

        foreach ( var problem in problems )
        {
            Console.WriteLine( $"FAIL  {problem}" );
        }

        Console.WriteLine( problems.Count == 0 ? "verify: PASSED." : $"verify: FAILED. {problems.Count} problems." );

        return problems.Count == 0 ? 0 : 1;
    }
}
