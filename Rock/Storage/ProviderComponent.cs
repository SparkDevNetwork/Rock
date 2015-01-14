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
using Rock.Web.Cache;

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
            var stream = GetFileContentStream( file, context );
            var result = new byte[stream.Length];
            stream.Seek( 0, SeekOrigin.Begin );
            stream.Read( result, 0, result.Length );
            return result;
        }

        /// <summary>
        /// Gets the file content
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public abstract Stream GetFileContentStream( BinaryFile file, HttpContext context );

        /// <summary>
        /// Generate a URL for the file based on the rules of the StorageProvider
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public virtual string GenerateUrl( BinaryFile file )
        {
            string url = string.Empty;

            if ( file != null )
            {
                string handler = file.MimeType.StartsWith( "image/", StringComparison.OrdinalIgnoreCase ) ? "GetImage.ashx" : "GetFile.ashx";
                url = System.Web.VirtualPathUtility.ToAbsolute( "~/" + handler);
                url += "?guid=" + file.Guid.ToString();

                Uri uri = null;
                try
                {
                    if ( HttpContext.Current != null && HttpContext.Current.Request != null )
                    {
                        uri = new Uri( HttpContext.Current.Request.Url.ToString() );
                    }
                }
                catch { }

                if ( uri == null )
                {
                    uri = new Uri( GlobalAttributesCache.Read().GetValue( "PublicApplicationRoot" ) );
                }

                if ( uri != null )
                {
                    return uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + url;
                }
            }

            return url;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderComponent"/> class.
        /// </summary>
        public ProviderComponent()
        {
            this.LoadAttributes();
        }
    }
}
