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

using Rock.Web.Cache;

namespace Rock.Lava
{
    /// <summary>
    /// A file system used by the Lava Engine in a website environment to locate and load templates referenced by an include tag.
    /// </summary>
    public class WebsiteLavaFileSystem : ILavaFileSystem
    {
        /// <summary>
        /// Gets or sets the root directory of the file system.
        /// </summary>
        public string Root { get; set; }

        /// <summary>
        /// Check if the specified file exists in the file system.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool FileExists( string filePath )
        {
            filePath = GetMatchingFileFromPath( filePath );

            return !string.IsNullOrEmpty( filePath );
        }

        /// <summary>
        /// Load the contents of a template file.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="templatePath"></param>
        /// <returns></returns>
        /// <exception cref="LavaException">LavaFileSystem Template Not Found</exception>
        public string ReadTemplateFile( ILavaRenderContext context, string templatePath )
        {
            // Try to find exact file specified
            var resolvedPath = ResolveTemplatePath( templatePath );

            var file = new FileInfo( resolvedPath );
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

            throw new LavaException( $"LavaFileSystem Template Not Found. The file \"{templatePath}\" does not exist." );
        }

        private string GetMatchingFileFromPath( string templateFilePath )
        {
            var resolvedPath = ResolveTemplatePath( templateFilePath );

            // Try to find exact file specified
            var file = new FileInfo( resolvedPath );

            if ( file.Exists )
            {
                return file.FullName;
            }

            // If requested template file does not include an extension
            if ( string.IsNullOrWhiteSpace( file.Extension ) )
            {
                // Try to find file with .lava extension
                string filePath = file.FullName + ".lava";
                if ( File.Exists( filePath ) )
                {
                    return filePath;
                }

                // Try to find file with .liquid extension
                filePath = file.FullName + ".liquid";
                if ( File.Exists( filePath ) )
                {
                    return filePath;
                }

                // If file still not found, try prefixing filename with an underscore
                if ( !file.Name.StartsWith( "_" ) )
                {
                    filePath = Path.Combine( file.DirectoryName, string.Format( "_{0}.lava", file.Name ) );
                    if ( File.Exists( filePath ) )
                    {
                        return filePath;
                    }
                    filePath = Path.Combine( file.DirectoryName, string.Format( "_{0}.liquid", file.Name ) );
                    if ( File.Exists( filePath ) )
                    {
                        return filePath;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Resolves the absolute path for the input template.
        /// </summary>
        /// <param name="templatePath">The template path.</param>
        /// <returns></returns>
        private string ResolveTemplatePath( string templatePath )
        {
            if ( templatePath == null )
            {
                throw new Exception( string.Format( "LavaFileSystem Illegal Template Name. (\"{0}\") ", templatePath ) );
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