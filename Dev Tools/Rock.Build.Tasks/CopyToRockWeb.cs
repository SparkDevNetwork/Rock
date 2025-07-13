using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Rock.Build.Tasks
{
    /// <summary>
    /// Copies the specified assembly to the RockWeb bin folder.
    /// </summary>
    public class CopyToRockWeb : Task
    {
        /// <summary>
        /// The source directory that the files are in.
        /// </summary>
        [Required]
        public string Source { get; set; }

        /// <summary>
        /// The files to copy from the <see cref="Source"/> directory.
        /// </summary>
        [Required]
        public ITaskItem[] Files { get; set; }

        /// <summary>
        /// The destination directory to copy the files into.
        /// </summary>
        [Required]
        public string Destination { get; set; }

        /// <summary>
        /// Files to exclude from the copy, relative to the destination.
        /// </summary>
        public string Exclude { get; set; }

        private List<string> _excluded = new List<string>();

        /// <inheritdoc/>
        public override bool Execute()
        {
            if ( string.IsNullOrWhiteSpace( Source ) )
            {
                Log.LogError( "The Source property must be set." );
                return false;
            }

            if ( string.IsNullOrWhiteSpace( Destination ) )
            {
                Log.LogError( "The Destination property must be set." );
                return false;
            }

            if ( !Directory.Exists( Destination ) )
            {
                Log.LogError( "The Destination directory must exist." );
                return false;
            }

            _excluded = Exclude?.Split( new[] { ';' }, StringSplitOptions.RemoveEmptyEntries ).ToList() ?? new List<string>();

            var sourceDirectory = Source;
            var files = Files.Select( f => f.ItemSpec ).ToList();

            Log.LogMessage( MessageImportance.High, "Copying files to RockWeb." );

            foreach ( var file in files )
            {
                if ( !File.Exists( file ) )
                {
                    Log.LogError( $"The file {file} does not exist." );
                    return false;
                }

                CopyFile( file );
            }

            return true;
        }

        /// <summary>
        /// Copy the file to the destination.
        /// </summary>
        /// <param name="sourceFile">The file to be copied.</param>
        private void CopyFile( string sourceFile )
        {
            var sourcePath = Path.GetFullPath( Source );
            var relativeSource = sourceFile.Replace( sourcePath, string.Empty );
            var destFile = Path.Combine( Path.GetFullPath( Destination ), relativeSource );

            if ( _excluded.Any( e => relativeSource.Equals( e, StringComparison.OrdinalIgnoreCase ) ) )
            {
                return;
            }

            if ( IsMatchingSizeAndTimeStamp( sourceFile, destFile ) )
            {
                return;
            }

            var relativeDestination = destFile;

#if NET6_0_OR_GREATER
            relativeDestination = Path.GetRelativePath( Directory.GetCurrentDirectory(), destFile );
#endif

            Log.LogMessage( MessageImportance.High, $"  {relativeSource} => {relativeDestination}" );

            Directory.CreateDirectory( Path.GetDirectoryName( destFile ) );
            File.Copy( sourceFile, destFile, true );
        }

        /// <summary>
        /// Method compares two files and returns true if their size and timestamp are identical.
        /// </summary>
        /// <param name="sourceFile">The source file</param>
        /// <param name="destinationFile">The destination file</param>
        private static bool IsMatchingSizeAndTimeStamp( string sourceFile, string destinationFile )
        {
            var sf = new FileInfo( sourceFile );
            var df = new FileInfo( destinationFile );

            // If the destination doesn't exist, then it is not a matching file.
            if ( !df.Exists )
            {
                return false;
            }

            if ( sf.LastWriteTimeUtc != df.LastWriteTimeUtc )
            {
                return false;
            }

            if ( sf.Length != df.Length )
            {
                return false;
            }

            return true;
        }
    }
}
