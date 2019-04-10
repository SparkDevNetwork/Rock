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
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

using NuGet;

namespace Rock.Services.NuGet
{
    /// <summary>
    /// 
    /// </summary>
    public class WebProjectSystem : PhysicalFileSystem, IProjectSystem, IFileSystem
    {
        /// <summary>
        /// 
        /// </summary>
        private bool _isBindingRedirectSupported = false;

        /// <summary>
        /// Gets a value indicating whether this instance is binding redirect supported.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is binding redirect supported; otherwise, <c>false</c>.
        /// </value>
        public bool IsBindingRedirectSupported { get { return _isBindingRedirectSupported; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebProjectSystem" /> class.
        /// </summary>
        /// <param name="siteRoot">The site root.</param>
        public WebProjectSystem( string siteRoot ) : base( siteRoot ) {    }

        /// <summary>
        /// Adds the framework reference.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void AddFrameworkReference( string name )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds the reference.
        /// </summary>
        /// <param name="referencePath">The reference path.</param>
        /// <param name="stream">The stream.</param>
        public void AddReference( string referencePath, Stream stream )
        {
            // NOTE ********************************************************************
            // There is a bug with current version of NuGet.Core resulting in the stream 
            // parameter being null when this method is called.  Because of this The 
            // ProjectManager_PackageReferenceAdded event hander has been added to the 
            // WeProjectManager.cs class to handle assembly references
            // *************************************************************************
            //string fileName = Path.GetFileName( referencePath );
            //string fullPath = this.GetFullPath( GetReferencePath( fileName ) );
            //this.AddFile( fullPath, stream );
        }

        /// <summary>
        /// Gets the reference path.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        protected virtual string GetReferencePath( string name )
        {
            return Path.Combine( "bin", name );
        }

        /// <summary>
        /// Determines whether [is supported file] [the specified path].
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        ///   <c>true</c> if [is supported file] [the specified path]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsSupportedFile( string path )
        {
            return ( !path.StartsWith( "tools", StringComparison.OrdinalIgnoreCase ) && !Path.GetFileName( path ).Equals( "app.config", StringComparison.OrdinalIgnoreCase ) );
        }

        /// <summary>
        /// Gets the name of the project.
        /// </summary>
        /// <value>
        /// The name of the project.
        /// </value>
        public string ProjectName
        {
            get { return Root; }
        }

        /// <summary>
        /// References the exists.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public bool ReferenceExists( string name )
        {
            string referencePath = GetReferencePath( name );
            return FileExists( referencePath );
        }

        /// <summary>
        /// Removes the reference.
        /// </summary>
        /// <param name="name">The name.</param>
        public void RemoveReference( string name )
        {
            DeleteFile( GetReferencePath( name ) );
            if ( !this.GetFiles( "bin", false ).Any<string>() )
            {
                DeleteDirectory( "bin" );
            }
        }

        /// <summary>
        /// Resolves the path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public string ResolvePath( string path )
        {
            return path;
        }

        /// <summary>
        /// Gets the target framework.
        /// </summary>
        /// <value>
        /// The target framework.
        /// </value>
        public FrameworkName TargetFramework
        {
            get { return VersionUtility.DefaultTargetFramework; }
        }

        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public dynamic GetPropertyValue( string propertyName )
        {
            if ( ( propertyName != null ) && propertyName.Equals( "RootNamespace", StringComparison.OrdinalIgnoreCase ) )
            {
                return string.Empty;
            }
            return null;
        }

        /// <summary>
        /// Workaround to deal with the "because it is being used by 
        /// another process" dll problem.
        /// This method will always add the file to the filesystem.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="stream"></param>
        public override void AddFile( string path, Stream stream )
        {
            string fileToDelete = string.Empty;
            int fileCount = 0;

            // If dll file not in the bin folder...
            if ( path.EndsWith( ".dll" ) && !path.Contains( @"\bin\" ) )
            {
                string physicalFile = System.Web.HttpContext.Current.Server.MapPath( Path.Combine( "~", path ) );
                if ( File.Exists( physicalFile ) )
                {
                    // generate a unique *.#.rdelete filename
                    do
                    {
                        fileCount++;
                    }
                    while ( File.Exists( string.Format( "{0}.{1}.rdelete", physicalFile, fileCount ) ) );

                    fileToDelete = string.Format( "{0}.{1}.rdelete", physicalFile, fileCount );
                    File.Move( physicalFile, fileToDelete );
                }
            }

            if ( ! path.EndsWith( RockProjectManager.TRANSFORM_FILE_PREFIX ) )
            {
                base.AddFile( path, stream );
            }

            if ( fileToDelete != string.Empty )
            {
                try
                {
                    File.Delete( fileToDelete );
                }
                catch
                { // delete at a later date
                };
            }

            if ( path.Equals( Path.Combine( "App_Data", "deletefile.lst" ) ) )
            {
                ProcessFilesToDelete( path );
            }
        }

        /// <summary>
        /// Workaround until we get to NuGet 2.7 - Always treat the file as though it does not exist so that it will be replaced.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public override bool FileExists( string path )
        {
            return false;
        }

        /// <summary>
        /// Deletes each file listed in the App_Data/deletefile.lst and then deletes that file.
        /// </summary>
        private void ProcessFilesToDelete( string deleteListFile )
        {
            // Turn relative path to virtual path
            deleteListFile = System.Web.HttpContext.Current.Server.MapPath( Path.Combine( "~", deleteListFile ) );

            using ( StreamReader file = new StreamReader( deleteListFile ) )
            {
                string filenameLine;
                while ( ( filenameLine = file.ReadLine() ) != null )
                {
                    if ( !string.IsNullOrWhiteSpace( filenameLine ) )
                    {
                        if ( filenameLine.StartsWith( @"RockWeb\" ) )
                        {
                            filenameLine = filenameLine.Substring( 8 );
                        }

                        string physicalFile = System.Web.HttpContext.Current.Server.MapPath( Path.Combine( "~", filenameLine ) );
                        
                        if ( File.Exists( physicalFile ) )
                        {
                            // guard against things like file is temporarily locked, wait then try delete, etc.
                            try
                            {
                                File.Delete( physicalFile );
                            }
                            catch
                            {
                                // if the delete failed, we'll try moving the file to a *.rdelete file for
                                // removal later.

                                // generate a unique *.#.rdelete filename
                                int fileCount = 0;
                                do
                                {
                                    fileCount++;
                                }
                                while ( File.Exists( string.Format( "{0}.{1}.rdelete", physicalFile, fileCount ) ) );

                                string fileToDelete = string.Format( "{0}.{1}.rdelete", physicalFile, fileCount );
                                File.Move( physicalFile, fileToDelete );
                            }
                        }
                    }
                }
                file.Close();
            }
            File.Delete( deleteListFile );
        }

        /// <summary>
        /// Adds the import.
        /// </summary>
        /// <param name="targetFullPath">The target full path.</param>
        /// <param name="location">The location.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void AddImport( string targetFullPath, ProjectImportLocation location )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Files the exists in project.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool FileExistsInProject( string path )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the import.
        /// </summary>
        /// <param name="targetFullPath">The target full path.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void RemoveImport( string targetFullPath )
        {
            throw new NotImplementedException();
        }
    }
}
