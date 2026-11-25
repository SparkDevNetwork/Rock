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
using System.IO.Compression;
using System.Linq;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Rock.Build.Tasks;

/// <summary>
/// Compresses each file into the output directory as <c>{file name}.gz</c>.
/// The output is flat, so two inputs with the same file name are an error.
/// </summary>
public class GzipFiles : Task
{
    /// <summary>
    /// The files to compress.
    /// </summary>
    [Required]
    public ITaskItem[] Files { get; set; }

    /// <summary>
    /// The directory the compressed files are written to.
    /// </summary>
    [Required]
    public string OutputDirectory { get; set; }

    /// <summary>
    /// The compressed files that were written.
    /// </summary>
    [Output]
    public ITaskItem[] CompressedFiles { get; set; }

    /// <summary>
    /// Compresses the files.
    /// </summary>
    /// <returns><c>true</c> when every file was compressed.</returns>
    public override bool Execute()
    {
        var duplicate = Files.GroupBy( f => Path.GetFileName( f.ItemSpec ), StringComparer.OrdinalIgnoreCase ).FirstOrDefault( g => g.Count() > 1 );

        if ( duplicate != null )
        {
            Log.LogError( $"More than one file is named '{duplicate.Key}': {string.Join( ", ", duplicate.Select( f => f.ItemSpec ) )}. Compressed files are written to one folder, so file names must be unique." );
            return false;
        }

        Directory.CreateDirectory( OutputDirectory );

        var compressedFiles = new List<ITaskItem>();

        foreach ( var file in Files )
        {
            var destination = Path.Combine( OutputDirectory, Path.GetFileName( file.ItemSpec ) + ".gz" );

            using var input = File.OpenRead( file.ItemSpec );
            using var output = File.Create( destination );
            using var gzip = new GZipStream( output, CompressionLevel.Optimal );
            input.CopyTo( gzip );

            Log.LogMessage( MessageImportance.Low, $"Compressed {file.ItemSpec} to {destination}." );
            compressedFiles.Add( new TaskItem( destination ) );
        }

        CompressedFiles = compressedFiles.ToArray();

        return true;
    }
}
