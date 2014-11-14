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
using System.IO;

using System.Web;
using Rock.Extension;
using Rock.Model;

namespace Rock.Storage
{
    /// <summary>
    /// Base class for BinaryFile storage components
    /// </summary>
    public abstract class ProviderComponent : Component
    {
        /// <summary>
        /// Saves the file to the external storage medium associated with the provider.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="context">The context.</param>
        public abstract void SaveFile( BinaryFile file, HttpContext context );

        /// <summary>
        /// Removes the file from the external storage medium associated with the provider.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="context">The context.</param>
        public abstract void RemoveFile( BinaryFile file, HttpContext context );

        /// <summary>
        /// Gets the file bytes from the external storage medium associated with the provider.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        [Obsolete( "This will be removed post McKinley. Use GetFileContentStream() instead." )]
        public virtual byte[] GetFileContent( BinaryFile file, HttpContext context )
        {
            return null;
        }

        /// <summary>
        /// Gets the file content
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public virtual Stream GetFileContentStream( BinaryFile file, HttpContext context )
        {
            // should be overridden, but just in case...
            return new MemoryStream( GetFileContent( file, context ) );
        }

        /// <summary>
        /// Generate a URL for the file based on the rules of the StorageProvider
        /// NOTE: This is the storage URL for use by the provider, not the URL that is served to a Rock client
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public abstract string GenerateUrl( BinaryFile file );

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderComponent"/> class.
        /// </summary>
        public ProviderComponent()
        {
            this.LoadAttributes();
        }
    }
}
