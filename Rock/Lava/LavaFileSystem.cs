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
using System.Web;

using DotLiquid;
using DotLiquid.Exceptions;
using DotLiquid.FileSystems;
using Rock.Web.Cache;

namespace Rock.Lava
{
    /// <summary>
    /// Lava's <seealso cref="IFileSystem"/>. This is used when Lava templates retrieve other Lava templates when using the include tag.
    /// </summary>
    public class LavaFileSystem : IFileSystem
    {
        /// <summary>
        /// Gets or sets the root.
        /// </summary>
        /// <value>
        /// The root.
        /// </value>
        public string Root { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaFileSystem"/> class.
        /// </summary>
        public LavaFileSystem() { }

        /// <summary>
        /// Called by Liquid to retrieve a template file
        /// </summary>
        /// <param name="context"></param>
        /// <param name="templateName"></param>
        /// <returns></returns>
        /// <exception cref="FileSystemException">LavaFileSystem Template Not Found</exception>
        public string ReadTemplateFile( Context context, string templateName )
        {
            string templatePath = ( string ) context[templateName];

            // Try to find exact file specified
            var file = new FileInfo( FullPath( templatePath ) );
            if ( file.Exists )
            {
                return File.ReadAllText( file.FullName );
            }

            // If requested template file does not include an extension
            if ( string.IsNullOrWhiteSpace( file.Extension ) )
            {
                // Try to find file with .lava extension
                string filePath = file.FullName + ".lava";
                if ( File.Exists( filePath ) )
                {
                    return File.ReadAllText( filePath );
                }

                // Try to find file with .liquid extension
                filePath = file.FullName + ".liquid";
                if ( File.Exists( filePath ) )
                {
                    return File.ReadAllText( filePath );
                }

                // If file still not found, try prefixing filename with an underscore
                if ( !file.Name.StartsWith( "_" ) )
                {
                    filePath = Path.Combine( file.DirectoryName, string.Format( "_{0}.lava", file.Name ) );
                    if ( File.Exists( filePath ) )
                    {
                        return File.ReadAllText( filePath );
                    }
                    filePath = Path.Combine( file.DirectoryName, string.Format( "_{0}.liquid", file.Name ) );
                    if ( File.Exists( filePath ) )
                    {
                        return File.ReadAllText( filePath );
                    }
                }
            }

            throw new FileSystemException( "LavaFileSystem Template Not Found", templatePath );
        }

        /// <summary>
        /// Fulls the path.
        /// </summary>
        /// <param name="templatePath">The template path.</param>
        /// <returns></returns>
        /// <exception cref="FileSystemException">LavaFileSystem Illegal Template Name</exception>
        public string FullPath( string templatePath )
        {
            if ( templatePath == null )
            {
                throw new FileSystemException( "LavaFileSystem Illegal Template Name", templatePath );
            }

            /*
	            07/07/2020 - MSB 

                We need to be careful here because if this is being run from a job or workflow the HttpContext.Current will
                most likely be null. We shouldn't be relying on HttpContext.Current for information here unless we know it will not be null.
	
	            Reason: Update Persisted Datasets Job with Lava includes.
            */

            if ( HttpContext.Current != null )
            {
                if ( templatePath.StartsWith( "~~" ) &&
                    HttpContext.Current.Items != null &&
                    HttpContext.Current.Items.Contains( "Rock:PageId" ) )
                {
                    var rockPage = PageCache.Get( HttpContext.Current.Items["Rock:PageId"].ToString().AsInteger() );
                    if ( rockPage != null &&
                        rockPage.Layout != null &&
                        rockPage.Layout.Site != null )
                    {
                        templatePath = "~/Themes/" + rockPage.Layout.Site.Theme + ( templatePath.Length > 2 ? templatePath.Substring( 2 ) : string.Empty );
                    }
                }

                return HttpContext.Current.Server.MapPath( templatePath );
            }

            return Path.Combine( AppDomain.CurrentDomain.BaseDirectory, templatePath.Replace( "~~", "Themes/Rock" ).Replace( "~/", "" ) );
        }
    }
}