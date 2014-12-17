﻿// <copyright>
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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Rock.Data;
using Rock.Model;

namespace Rock.Storage.Provider
{
    /// <summary>
    /// Storage provider for saving binary files to file system
    /// </summary>
    [Description( "File System-driven file storage" )]
    [Export( typeof( ProviderComponent ) )]
    [ExportMetadata( "ComponentName", "FileSystem" )]
    public class FileSystem : ProviderComponent
    {
        /// <summary>
        /// Saves the file to the external storage medium associated with the provider.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="System.ArgumentException">File Data must not be null.</exception>
        public override void SaveFile( BinaryFile file, HttpContext context )
        {
            var physicalPath = GetPhysicalPath( file, context );
            var directoryName = Path.GetDirectoryName( physicalPath );

            if ( !Directory.Exists( directoryName ) )
            {
                Directory.CreateDirectory( directoryName );
            }

            if ( file.Data != null && file.Data.ContentStream != null )
            {
                
                FileStream sourceFileStream = file.Data.ContentStream as FileStream;
                bool sameFile = ( sourceFileStream != null ) && sourceFileStream.Name == physicalPath;

                if ( !sameFile )
                {
                    using ( FileStream writeStream = File.OpenWrite( physicalPath ) )
                    {
                        file.Data.ContentStream.CopyTo( writeStream );
                    }


                    file.Data.ContentStream.Dispose();
                    file.Data.ContentStream = null;
                }
            }

            // Set Data to null after successful OS save so the the binary data is not 
            // written into the database.
            file.Data = null;
        }

        /// <summary>
        /// Removes the file from the external storage medium associated with the provider.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="context">The context.</param>
        public override void RemoveFile( BinaryFile file, HttpContext context )
        {
            var physicalPath = GetPhysicalPath( file, context );

            if ( File.Exists( physicalPath ) )
            {
                File.Delete( physicalPath );
            }
        }

        /// <summary>
        /// Gets the file bytes in chunks from the external storage medium associated with the provider.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override Stream GetFileContentStream( BinaryFile file, HttpContext context )
        {
            var physicalPath = GetPhysicalPath( file, context );

            if ( File.Exists( physicalPath ) )
            {
                var fileStream = File.OpenRead( physicalPath );

                return fileStream;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the root path.
        /// </summary>
        /// <param name="binaryFileTypeId">The binary file type identifier.</param>
        /// <returns></returns>
        /// <value>
        /// The root path.
        /// </value>
        private string GetRootPath( int binaryFileTypeId )
        {
            BinaryFileType binaryFileType = new BinaryFileTypeService( new RockContext() ).Get( binaryFileTypeId );
            binaryFileType.LoadAttributes();
            string rootPath = binaryFileType.GetAttributeValue( "RootPath" );
            if (string.IsNullOrEmpty(rootPath))
            {
                rootPath = "~/App_Data/Files";
            }
            return rootPath;
        }

        /// <summary>
        /// Gets the physical path.
        /// </summary>
        /// <param name="binaryFile">The binary file.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private string GetPhysicalPath( BinaryFile binaryFile, HttpContext context )
        {
            if ( string.IsNullOrWhiteSpace( binaryFile.FileName ) )
            {
                return null;
            }

            var urlBuilder = new StringBuilder();

            string rootPath = GetRootPath( binaryFile.BinaryFileTypeId ?? 0 );

            urlBuilder.Append( rootPath.EnsureTrailingForwardslash() );

            // if the file doesn't have a folderPath, prefix the FileName on disk with the Guid so that we can have multiple files with the same name (for example, setup.exe and setup.exe)
            if ( Path.GetDirectoryName( binaryFile.FileName ) == string.Empty )
            {
                urlBuilder.Append( binaryFile.Guid + "_" );
            }

            string urlFileName = binaryFile.FileName.Replace( "\\", "/" ).TrimStart( new char[] { '/' } );

            urlBuilder.Append( urlFileName );
            
            var path = urlBuilder.ToString();
            if ( Regex.Match(path, @"^[A-Z,a-z]:\\").Success  || path.StartsWith( "\\\\" ) )
            {
                return path;
            }

            if ( path.StartsWith( "http:" ) || path.StartsWith( "https:" ) || path.StartsWith( "/" ) || path.StartsWith( "~" ) )
            {
                return context.Server.MapPath( path );
            }

            return Path.Combine( AppDomain.CurrentDomain.BaseDirectory, path );
        }
    }
}
