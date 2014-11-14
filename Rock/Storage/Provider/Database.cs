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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Web;
using Rock.Model;

namespace Rock.Storage.Provider
{
    /// <summary>
    /// Storage provider for saving binary files to Rock database
    /// </summary>
    [Description( "Database-driven file storage" )]
    [Export( typeof( ProviderComponent ) )]
    [ExportMetadata( "ComponentName", "Database" )]
    public class Database : ProviderComponent
    {
        /// <summary>
        /// Removes the file from the external storage medium associated with the provider.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="context">The context.</param>
        public override void RemoveFile( BinaryFile file, HttpContext context )
        {
            // Database storage just stores everything in the BinaryFile table, so there is no external file data to delete
        }

        /// <summary>
        /// Saves the file to the external storage medium associated with the provider.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="context">The context.</param>
        public override void SaveFile( BinaryFile file, HttpContext context )
        {
            // Database storage just stores everything in the BinaryFile table, so there is no external file data to save, but we do need to set the Url
            file.Url = GenerateUrl( file );
        }

        /// <summary>
        /// Gets the file bytes from the external storage medium associated with the provider.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override byte[] GetFileContent( BinaryFile file, HttpContext context )
        {
            var stream = this.GetFileContentStream( file, context );
            var result = new byte[stream.Length];
            stream.Seek( 0, SeekOrigin.Begin );
            stream.Read( result, 0, result.Length );
            return result;
        }

        /// <summary>
        /// Gets the file bytes in chunks from the external storage medium associated with the provider.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override Stream GetFileContentStream( BinaryFile file, HttpContext context )
        {
            if ( file.Data != null )
            {
                return file.Data.ContentStream;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Generate a URL for the file based on the rules of the StorageProvider
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public override string GenerateUrl( BinaryFile file )
        {
            if ( file.MimeType.StartsWith( "image/", StringComparison.OrdinalIgnoreCase ) )
            {
                return string.Format( "~/GetImage.ashx?guid={1}", file.Guid );
            }
            else
            {
                return string.Format( "~/GetFile.ashx?guid={1}", file.Guid );
            }
        }
    }
}
