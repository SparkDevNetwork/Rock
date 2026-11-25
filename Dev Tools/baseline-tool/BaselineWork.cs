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
using System.IO;

using Newtonsoft.Json;

namespace Rock.BaselineTool;

/// <summary>
/// The state <c>cleanup</c> records for <c>package</c>, kept as JSON in
/// the work folder because cleanup leaves no trace in the database.
/// </summary>
internal sealed class BaselineWork
{
    /// <summary>
    /// The file name in the work folder.
    /// </summary>
    private const string FileName = "baseline-work.json";

    /// <summary>
    /// B's Rock version, such as <c>19.7.0.0</c>.
    /// </summary>
    public string RockVersion { get; set; }

    /// <summary>
    /// The ID of the last migration the baseline replaces: the highest
    /// ID in <c>__MigrationHistory</c> or <c>[RockMigration]</c>, truncated to 14 digits.
    /// </summary>
    public long IncludesThroughMigrationId { get; set; }

    /// <summary>
    /// The last identity value of every table that has issued one, keyed
    /// by <c>[schema].[table]</c>. <c>package</c> reseeds each table to it.
    /// </summary>
    public Dictionary<string, long> IdentityValues { get; set; } = new Dictionary<string, long>();

    /// <summary>
    /// When cleanup recorded this state.
    /// </summary>
    public DateTime CleanedDateTime { get; set; }

    /// <summary>
    /// Writes the state to the work folder.
    /// </summary>
    /// <param name="workPath">The work folder.</param>
    /// <returns>The file written.</returns>
    public string Save( string workPath )
    {
        Directory.CreateDirectory( workPath );

        var path = Path.Combine( workPath, FileName );
        File.WriteAllText( path, JsonConvert.SerializeObject( this, Formatting.Indented ) );

        return path;
    }

    /// <summary>
    /// Reads the state from the work folder.
    /// </summary>
    /// <param name="workPath">The work folder.</param>
    /// <returns>The state.</returns>
    public static BaselineWork Load( string workPath )
    {
        var path = Path.Combine( workPath, FileName );

        if ( !File.Exists( path ) )
        {
            throw new InvalidOperationException( $"'{path}' was not found. Run cleanup first." );
        }

        return JsonConvert.DeserializeObject<BaselineWork>( File.ReadAllText( path ) );
    }
}
