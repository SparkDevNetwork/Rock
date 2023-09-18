using System;
using System.IO;

using Rock.CodeGeneration.XmlDoc;

namespace Rock.CodeGeneration.Utility
{
    /// <summary>
    /// Helpful utilities for the code generator.
    /// </summary>
    public static class SupportTools
    {
        #region Methods

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

        /// <summary>
        /// Gets the XML document reader that will provide access to XML
        /// documentation for the various code generation pages.
        /// </summary>
        /// <returns>An instance of <see cref="XmlDocReader"/> that is ready to read.</returns>
        public static XmlDocReader GetXmlDocReader()
        {
            var reader = new XmlDocReader();

            // Read the XML documentation for the Rock assembly.
            var assemblyFileName = new FileInfo( new Uri( typeof( Data.IEntity ).Assembly.CodeBase ).LocalPath ).FullName;

            try
            {
                reader.ReadCommentsFrom( assemblyFileName.Substring( 0, assemblyFileName.Length - 4 ) + ".xml" );
            }
            catch
            {
                /* Intentionally left blank. */
            }

            // Read the XML documentation for the Rock.ViewModels assembly.
            assemblyFileName = new FileInfo( new Uri( typeof( Rock.ViewModels.Utility.EntityBagBase ).Assembly.CodeBase ).LocalPath ).FullName;

            try
            {
                reader.ReadCommentsFrom( assemblyFileName.Substring( 0, assemblyFileName.Length - 4 ) + ".xml" );
            }
            catch
            {
                /* Intentionally left blank. */
            }

            // Read the XML documentation for the Rock.Enums assembly.
            assemblyFileName = new FileInfo( new Uri( typeof( Enums.Reporting.FieldFilterSourceType ).Assembly.CodeBase ).LocalPath ).FullName;

            try
            {
                reader.ReadCommentsFrom( assemblyFileName.Substring( 0, assemblyFileName.Length - 4 ) + ".xml" );
            }
            catch
            {
                /* Intentionally left blank. */
            }

            return reader;
        }

        /// <summary>
        /// Gets the file system folder name (and C# namespace name) for the
        /// specified rock domain. If the string is not all caps then it will
        /// be returned as is. If it is all caps then special rules will be
        /// applied to format it correctly.
        /// </summary>
        /// <param name="domain">The Rock domain, such as CRM or Group.</param>
        /// <returns>The name in a format that can be used for file system folders and C# namespaces.</returns>
        public static string GetDomainFolderName( string domain )
        {
            // If the domain isn't all caps, then its already in the correct format.
            if ( domain != domain.ToUpper() )
            {
                return domain;
            }

            // 2 letter acronyms (such as UI) are kept as-is.
            if ( domain.Length == 2 )
            {
                return domain;
            }

            return domain.ToUpper()[0] + domain.Substring( 1 ).ToLower();
        }

        #endregion
    }
}
