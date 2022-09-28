using System.IO;

namespace BlockGenerator.Utility
{
    /// <summary>
    /// Helpful utilities for the code generator.
    /// </summary>
    public static class SupportTools
    {
        /// <summary>
        /// Determines whether the source files are newer than the binary file for
        /// a given C# project directory.
        /// </summary>
        /// <param name="relativeBinaryPath">The relative path to the binary file.</param>
        /// <param name="relativeSourcePath">The relative path to the source files.</param>
        /// <returns><c>true</c> if any source file is newer than the binary file; otherwise, <c>false</c>.</returns>
        public static bool IsSourceNewer( string relativeBinaryPath, string relativeSourcePath )
        {
            var binaryDateTime = new FileInfo( Path.Combine( GetSolutionPath(), relativeBinaryPath ) ).LastWriteTime;
            var files = Directory.GetFiles( Path.Combine( GetSolutionPath(), relativeSourcePath ), "*.cs", SearchOption.AllDirectories );

            foreach ( var file in files )
            {
                if ( new FileInfo( file ).LastWriteTime > binaryDateTime )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the absolute path to the directory containing the Rock.sln file.
        /// </summary>
        /// <returns>A string with the solution path or <c>null</c> if it could not be determined.</returns>
        public static string GetSolutionPath()
        {
            var directoryInfo = new DirectoryInfo( Directory.GetCurrentDirectory() );

            while ( directoryInfo != null )
            {
                if ( File.Exists( Path.Combine( directoryInfo.FullName, "Rock.sln" ) ) )
                {
                    return directoryInfo.FullName;
                }

                directoryInfo = directoryInfo.Parent;
            }

            return null;
        }
    }
}
