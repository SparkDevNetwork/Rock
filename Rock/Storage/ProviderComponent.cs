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

using Rock;
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
        /// Saves the binary file contents to the external storage medium associated with the provider.
        /// </summary>
        /// <param name="file">The file.</param>
        public abstract void SaveContent( BinaryFile file );

        /// <summary>
        /// Deletes the content from the external storage medium associated with the provider.
        /// </summary>
        /// <param name="file">The file.</param>
        public abstract void DeleteContent( BinaryFile file );

        /// <summary>
        /// Gets the contents from the external storage medium associated with the provider
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public abstract Stream GetContentStream( BinaryFile file );

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public virtual string GetPath( BinaryFile file )
        {
            string url = string.Empty;

            if ( file != null )
            {
                url = "~/GetFile.ashx";
                if ( file.MimeType != null && file.MimeType.StartsWith( "image/", StringComparison.OrdinalIgnoreCase ) )
                {
                    url = "~/GetImage.ashx";
                }

                url += "?guid=" + file.Guid.ToString();
            }

            return url;
        }

        /// <summary>
        /// Generate a URL for the file based on the rules of the StorageProvider
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public virtual string GetUrl( BinaryFile file )
        {
            if ( !string.IsNullOrWhiteSpace( file.Path ) )
            {
                string url = file.Path.StartsWith( "~" ) ? System.Web.VirtualPathUtility.ToAbsolute( file.Path ) : file.Path;
                if ( url.StartsWith( "http", StringComparison.OrdinalIgnoreCase ) )
                {
                    return url;
                }

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
            return string.Empty; ;
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
