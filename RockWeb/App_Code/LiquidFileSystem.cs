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
    public class LiquidFileSystem : IFileSystem
    {
        public string Root { get; set; }

        public LiquidFileSystem() {}

        public string ReadTemplateFile( Context context, string templateName )
        {
            string templatePath = (string)context[templateName];
            string fullPath = FullPath( templatePath );

            if ( !File.Exists( fullPath ) )
            {
                // Check to see if file name does not have extension and doesn't start with underscore (old format)
                var fileInfo = new FileInfo(fullPath);
                if ( !fileInfo.Name.StartsWith("_") && string.IsNullOrWhiteSpace(fileInfo.Extension))
                {
                    fullPath = Path.Combine( fileInfo.DirectoryName, string.Format("_{0}.liquid", fileInfo.Name));
                }
                if ( !File.Exists( fullPath ) )
                {
                    throw new FileSystemException( "LiquidFileSystem Template Not Found", templatePath );
                }
            }

            return File.ReadAllText( fullPath );
        }

        public string FullPath( string templatePath )
        {
            if ( templatePath != null && HttpContext.Current != null )
            {
                if ( templatePath.StartsWith( "~~" ) &&
                    HttpContext.Current.Items != null &&
                    HttpContext.Current.Items.Contains( "Rock:PageId" ) )
                {
                    var rockPage = PageCache.Read( HttpContext.Current.Items["Rock:PageId"].ToString().AsInteger() );
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
                throw new FileSystemException( "LiquidFileSystem Illegal Template Name", templatePath );
            }

            return HttpContext.Current.Server.MapPath( templatePath );
        }
    }
}