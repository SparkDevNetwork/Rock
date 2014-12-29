// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Linq;
using Microsoft.Web.XmlTransform;
using NuGet;
using Rock.Model;

namespace Rock.Services.NuGet
{
    /// <summary>
    /// This class inherits from NuGet.ProjectManager and handles installing new Rock packages (updates)
    /// in very specific way -- such as pre processing web.config XDT transforms.
    /// </summary>
    public class RockProjectManager : ProjectManager
    {
        /// <summary>
        /// The special transform file prefix that Rock recognizes.
        /// </summary>
        public static readonly string TRANSFORM_FILE_PREFIX = ".rock.xdt";

        /// <summary>
        /// The standard constructor.  We'll just call the base constructor.
        /// </summary>
        /// <param name="sourceRepository"></param>
        /// <param name="pathResolver"></param>
        /// <param name="project"></param>
        /// <param name="localRepository"></param>
        public RockProjectManager( IPackageRepository sourceRepository, IPackagePathResolver pathResolver, IProjectSystem project, IPackageRepository localRepository )
            : base( sourceRepository, pathResolver, project, localRepository )
        {
        }

        /// <summary>
        /// This method will be called first when a package is installed.  We're using
        /// this method to perform some pre processing of XDT files in the 
        /// </summary>
        /// <param name="package"></param>
        protected override void ExtractPackageFilesToProject( IPackage package )
        {
            List<IPackageFile> contentFiles = package.GetContentFiles().ToList();
            Dictionary<string, string> transformedFiles = new Dictionary<string, string>();
            string packageRestorePath = Path.Combine( Project.Root, "App_Data", "PackageRestore" );

            // go through each *.rock.xdt file and apply the transform first,
            foreach ( var xdtFile in contentFiles.Where( f => f.Path.EndsWith( TRANSFORM_FILE_PREFIX, StringComparison.OrdinalIgnoreCase ) ) )
            {
                using ( Stream stream = xdtFile.GetStream() )
                {
                    var fileName = xdtFile.EffectivePath;

                    // write the transform file out to the PackageRestore/xdt folder
                    var transformFilefullPath = Path.Combine( packageRestorePath, "xdt", fileName );
                    Directory.CreateDirectory( Path.GetDirectoryName( transformFilefullPath ) );
                    using ( var fileStream = File.Create( transformFilefullPath ) )
                    {
                        stream.CopyTo( fileStream );
                    }

                    var sourceFile = fileName.Remove( fileName.Length - TRANSFORM_FILE_PREFIX.Length );
                    var sourceFileFullPath = Path.Combine( Project.Root, sourceFile );
                    var tempPathOfTransformedFile = Path.Combine( packageRestorePath, "xdt", sourceFile );

                    // now transform the Rock file using the xdt file, but write it to the PackageRestore\xdt folder and we'll
                    // move it into place after the update is finished.
                    // If the transform fails, then we have to quit and inform the user.
                    if ( !ProcessXmlDocumentTransformation( transformFilefullPath, sourceFileFullPath, tempPathOfTransformedFile ) )
                    {
                        throw new System.Xml.XmlException( sourceFile );
                    }
                    transformedFiles.Add( tempPathOfTransformedFile, sourceFile );
                }
            }

            // now let the package installation proceed as normal
            base.ExtractPackageFilesToProject( package );

            // lastly, move the transformed xml files into place
            MoveTransformedFiles( transformedFiles );

            try
            {
                Directory.Delete( packageRestorePath, recursive: true );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
                ExceptionLogService.LogException( new Exception( string.Format( "Unable to delete package restore folder ({0}) after a successful update.", packageRestorePath ) ), System.Web.HttpContext.Current );
            }
        }

        /// <summary>
        /// Move the files in the given Dictionary from their full path (key) to 
        /// the new name relative to the project root.
        /// </summary>
        /// <param name="transformedFiles"></param>
        private void MoveTransformedFiles( Dictionary<string, string> transformedFiles )
        {
            string destFileName;
            foreach ( var item in transformedFiles )
            {
                destFileName = Path.Combine( Project.Root, item.Value );

                try
                {
                    if ( File.Exists( destFileName ) )
                    {
                        File.Delete( destFileName );
                    }
                    File.Move( item.Key, destFileName );
                }
                catch( Exception )
                {
                    throw new System.IO.IOException( destFileName );
                }
            }
        }

        /// <summary>
        /// Perform an XDT transform using the transformFile against the sourceFile and save the results to the destinationFile.
        /// </summary>
        /// <param name="transformFile">an XDT file</param>
        /// <param name="sourceFile">an XML source file suitable for Xml Document Transforming</param>
        /// <param name="destinationFile">a full path to the destination file where you want to save the transformed file.</param>
        /// <returns></returns>
        private bool ProcessXmlDocumentTransformation( string transformFile, string sourceFile, string destinationFile )
        {
            bool isSuccess = true;

            if ( !File.Exists( sourceFile ) )
            {
                ExceptionLogService.LogException( new FileNotFoundException( string.Format( "Source transform file ({0}) does not exist.", sourceFile ) ), System.Web.HttpContext.Current );
                return false;
            }

            // This really shouldn't happen since it was theoretically‎ just added before
            // we were called.
            if ( !File.Exists( transformFile ) )
            {
                ExceptionLogService.LogException( new FileNotFoundException( string.Format( "Transform file ({0}) does not exist.", transformFile ) ), System.Web.HttpContext.Current );
                return false;
            }

            try
            {
                using ( XmlTransformableDocument document = new XmlTransformableDocument() )
                {
                    document.PreserveWhitespace = true;
                    document.Load( sourceFile );

                    using ( XmlTransformation transform = new XmlTransformation( transformFile ) )
                    {
                        isSuccess = transform.Apply( document );
                        if ( isSuccess )
                        {
                            document.Save( destinationFile );
                            File.Delete( transformFile );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                isSuccess = false;
                ExceptionLogService.LogException( new FileNotFoundException( string.Format( "Error while transforming {0} : {1}.", sourceFile, ex.Message ) ), System.Web.HttpContext.Current );
            }

            return isSuccess;
        }
    }
}
