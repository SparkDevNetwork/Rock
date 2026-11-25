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
using System.Diagnostics;
using System.IO;
using System.Xml;

using Microsoft.Data.SqlClient;

namespace Rock.BaselineTool;

/// <summary>
/// What the tool reads from B's RockWeb folder: its Rock version, its
/// connection string and the settings <c>verify</c> checks.
/// </summary>
internal sealed class BaselineSite
{
    /// <summary>
    /// The RockWeb folder.
    /// </summary>
    public string RockWebPath { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaselineSite"/> class.
    /// </summary>
    /// <param name="rockWebPath">The RockWeb folder of B's site.</param>
    public BaselineSite( string rockWebPath )
    {
        if ( !Directory.Exists( rockWebPath ) )
        {
            throw new ArgumentException( $"The RockWeb folder '{rockWebPath}' does not exist." );
        }

        RockWebPath = rockWebPath;
    }

    /// <summary>
    /// The Rock version of the site, read from <c>Bin\Rock.Version.dll</c>
    /// the way Rock reads its own version.
    /// </summary>
    /// <returns>The file version.</returns>
    public Version GetRockVersion()
    {
        var path = Path.Combine( RockWebPath, "Bin", "Rock.Version.dll" );

        if ( !File.Exists( path ) )
        {
            throw new InvalidOperationException( $"'{path}' was not found. Build B's site first." );
        }

        return Version.Parse( FileVersionInfo.GetVersionInfo( path ).FileVersion );
    }

    /// <summary>
    /// Reads the <c>RockContext</c> connection string from <c>web.ConnectionStrings.config</c>.
    /// </summary>
    /// <returns>The connection string.</returns>
    public string GetConnectionString()
    {
        var path = Path.Combine( RockWebPath, "web.ConnectionStrings.config" );

        if ( !File.Exists( path ) )
        {
            throw new InvalidOperationException( $"'{path}' was not found." );
        }

        var document = new XmlDocument();
        document.Load( path );

        var value = document.DocumentElement?.SelectSingleNode( "add[@name = \"RockContext\"]" )?.Attributes?["connectionString"]?.Value
            ?? throw new InvalidOperationException( $"The RockContext connection string was not found in '{path}'." );

        return new SqlConnectionStringBuilder( value.Trim() ).ConnectionString;
    }

    /// <summary>
    /// Reads an <c>appSettings</c> value from <c>web.config</c>, or <c>null</c> when it is not set.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <returns>The value.</returns>
    public string GetAppSetting( string key )
    {
        var document = new XmlDocument();
        document.Load( Path.Combine( RockWebPath, "web.config" ) );

        return document.SelectSingleNode( $"/configuration/appSettings/add[@key = \"{key}\"]" )?.Attributes?["value"]?.Value;
    }

    /// <summary>
    /// Turns a database argument into a connection string. A value
    /// containing <c>=</c> is already a connection string; any other value
    /// is a database name on the site's server; <c>null</c> is the site's database.
    /// </summary>
    /// <param name="database">The database argument, or <c>null</c>.</param>
    /// <param name="site">The site, or <c>null</c> when only a connection string was given.</param>
    /// <returns>The connection string.</returns>
    public static string ResolveDatabase( string database, BaselineSite site )
    {
        if ( database != null && database.Contains( "=" ) )
        {
            return database;
        }

        if ( site == null )
        {
            throw new ArgumentException( "--rockweb is needed to resolve the database." );
        }

        var connectionString = site.GetConnectionString();

        return database == null
            ? connectionString
            : new SqlConnectionStringBuilder( connectionString ) { InitialCatalog = database }.ConnectionString;
    }
}
