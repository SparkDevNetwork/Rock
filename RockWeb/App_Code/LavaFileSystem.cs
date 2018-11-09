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
using System.Text.RegularExpressions;
using System.Web;

using DotLiquid;
using DotLiquid.Exceptions;
using DotLiquid.FileSystems;

using Rock;
using Rock.Web.Cache;

namespace RockWeb
{

    /// <summary>
    /// 
    /// </summary>
    public class LavaFileSystem : IFileSystem
    {
        public string Root { get; set; }

        public LavaFileSystem() {}

        public string ReadTemplateFile( Context context, string templateName )
        {
            string templatePath = (string)context[templateName];

            // Try to find exact file specified
            var file = new FileInfo( FullPath( templatePath ));
            if ( file.Exists)
            {
                return File.ReadAllText(file.FullName);
            }

            // If requested template file does not include an extension
            if ( string.IsNullOrWhiteSpace( file.Extension ) )
            {
                // Try to find file with .lava extension
                string filePath = file.FullName + ".lava";
                if ( File.Exists( filePath) )
                {
                    return File.ReadAllText(filePath);
                }

                // Try to find file with .liquid extension
                filePath = file.FullName + ".liquid";
                if ( File.Exists( filePath) )
                {
                    return File.ReadAllText(filePath);
                }

                // If file still not found, try prefixing filename with an underscore
                if ( !file.Name.StartsWith("_") )
                {
                    filePath = Path.Combine( file.DirectoryName, string.Format("_{0}.lava", file.Name));
                    if ( File.Exists( filePath) )
                    {
                        return File.ReadAllText(filePath);
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

        public string FullPath( string templatePath )
        {
            if ( templatePath != null && HttpContext.Current != null )
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
            }

            if ( templatePath == null )
            {
                throw new FileSystemException( "LavaFileSystem Illegal Template Name", templatePath );
            }

            return HttpContext.Current.Server.MapPath( templatePath );
        }

    }
}