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

using System;
using System.IO;
#if WEBFORMS
using System.Web.Hosting;
#endif

using Rock.Configuration;
using Rock.Model;

namespace Rock.Update.Helpers
{
    /// <summary>
    /// Helper methods for accessing the file system.
    /// </summary>
    public static class FileManagementHelper
    {
#if WEBFORMS
        /*
            10/26/2026 - CLAUDE

            ROOT_PATH is a public field computed when the type loads, using the
            WebForms HostingEnvironment. It is replaced by the RootPath property,
            which is computed on first use through RockApp so it works on .NET
            Core. Changing the field itself into a property would break plugins
            compiled against it, so it is kept on WebForms and marked obsolete
            to give plugins time to move to RootPath.

            Reason: Replace HostingEnvironment.MapPath without breaking plugins.
        */

        /// <summary>
        /// The root physical file system path of the web application.
        /// </summary>
        [Obsolete( "Use RootPath instead." )]
        [RockObsolete( "21.0" )]
        public static readonly string ROOT_PATH = HostingEnvironment.MapPath( "~/" ) ?? AppDomain.CurrentDomain.BaseDirectory;
#endif

        /// <summary>
        /// The lazily computed root physical file system path of the web application.
        /// </summary>
        private static readonly Lazy<string> _rootPath = new Lazy<string>( () => RockApp.Current.MapPath( "~/" ) );

        /// <summary>
        /// Gets the root physical file system path of the web application.
        /// </summary>
        public static string RootPath => _rootPath.Value;

        private static readonly string DELETE_FILE_EXTENSION = "rdelete";

        /// <summary>
        /// Tries the delete.
        /// </summary>
        /// <remarks>
        /// This method will always log any exception that occurs even if the exception isn't thrown.
        /// </remarks>
        /// <param name="filePath">The file path.</param>
        /// <param name="shouldBubbleException">if set to <c>true</c> [should bubble exception].</param>
        public static void TryDelete( string filePath, bool shouldBubbleException )
        {
            TryDelete( filePath, ( ex ) => ExceptionLogService.LogException( ex ), shouldBubbleException );
        }

        /// <summary>
        /// Tries the delete.
        /// </summary>
        /// <remarks>
        /// Will not log the exception by default.
        /// </remarks>
        /// <param name="filePath">The file path.</param>
        /// <param name="catchMethod">The catch method.</param>
        /// <param name="shouldBubbleException">if set to <c>true</c> [should bubble exception].</param>
        public static void TryDelete( string filePath, Action<Exception> catchMethod, bool shouldBubbleException )
        {
            try
            {
                File.Delete( filePath );
            }
            catch ( Exception ex )
            {
                catchMethod( ex );
                if ( shouldBubbleException )
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Deletes the or rename.
        /// </summary>
        /// <param name="filepath">The file path.</param>
        public static void DeleteOrRename( string filepath )
        {
            if ( File.Exists( filepath ) )
            {
                TryDelete( filepath, ( ex ) => RenameFile( filepath ), false );
            }
        }

        /// <summary>
        /// Renames the file.
        /// </summary>
        /// <param name="physicalFile">The physical file.</param>
        public static void RenameFile( string physicalFile )
        {
            if ( File.Exists( physicalFile ) )
            {
                File.Move( physicalFile, GetRenameFileName( physicalFile ) );
            }
        }

        /// <summary>
        /// Renames the active file.
        /// </summary>
        /// <param name="filepathToRename">The file path to rename.</param>
        public static void RenameActiveFile( string filepathToRename )
        {
            bool dllFileNotInBin = filepathToRename.EndsWith( ".dll" ) && !filepathToRename.Contains( @"\bin\" );
            bool roslynAssembly = ( filepathToRename.EndsWith( ".dll" ) || filepathToRename.EndsWith( ".exe" ) ) && filepathToRename.Contains( @"\roslyn\" );

            // If this a roslyn assembly or a dll file from the Content files, rename it so that we don't have problems with it being locks
            if ( roslynAssembly || dllFileNotInBin )
            {
                string physicalFile;
                if ( roslynAssembly )
                {
                    physicalFile = filepathToRename;
                }
                else
                {
                    physicalFile = Path.Combine( RootPath, filepathToRename );
                }

                RenameFile( physicalFile );
            }
        }

        /// <summary>
        /// Cleans up deleted files.
        /// </summary>
        public static void CleanUpDeletedFiles()
        {
            var filesToDelete = Directory.GetFiles( RootPath, $"*.{DELETE_FILE_EXTENSION}", SearchOption.AllDirectories );
            foreach ( var file in filesToDelete )
            {
                FileManagementHelper.TryDelete( file, false );
            }
        }

        /// <summary>
        /// Gets the name of the rename file.
        /// </summary>
        /// <param name="physicalFile">The physical file.</param>
        /// <returns></returns>
        private static string GetRenameFileName( string physicalFile )
        {
            var fileCount = 1;
            var fileToDelete = $"{physicalFile}.{fileCount}.{DELETE_FILE_EXTENSION}";

            // generate a unique *.#.rdelete filename
            while ( File.Exists( fileToDelete ) )
            {
                fileCount++;
                fileToDelete = $"{physicalFile}.{fileCount}.{DELETE_FILE_EXTENSION}";
            }

            return fileToDelete;
        }
    }
}
