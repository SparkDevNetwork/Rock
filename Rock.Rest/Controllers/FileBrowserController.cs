using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using ImageResizer;
using Rock.Rest.Filters;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class FileBrowserController : ApiController, IHasCustomRoutes
    {
        /// <summary>
        /// The root content folder
        /// </summary>
        private const string RootContentFolder = "~/Content";

        /// <summary>
        /// Adds the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public void AddRoutes( RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "GetSubFolders",
                routeTemplate: "api/FileBrowser/GetSubFolders",
                defaults: new
                {
                    controller = "FileBrowser",
                    action = "GetSubFolders"
                } );

            routes.MapHttpRoute(
                name: "GetFiles",
                routeTemplate: "api/FileBrowser/GetFiles",
                defaults: new
                {
                    controller = "FileBrowser",
                    action = "GetFiles"
                } );

            routes.MapHttpRoute(
                name: "DeleteFile",
                routeTemplate: "api/FileBrowser/DeleteFile",
                defaults: new
                {
                    controller = "FileBrowser",
                    action = "DeleteFile"
                } );

            routes.MapHttpRoute(
                name: "CreateFolder",
                routeTemplate: "api/FileBrowser/CreateFolder",
                defaults: new
                {
                    controller = "FileBrowser",
                    action = "CreateFolder"
                } );

            routes.MapHttpRoute(
                name: "GetFileThumbnail",
                routeTemplate: "api/FileBrowser/GetFileThumbnail",
                defaults: new
                {
                    controller = "FileBrowser",
                    action = "GetFileThumbnail"
                } );
        }

        /// <summary>
        /// Gets the sub folders.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <param name="fileFilter">The file filter.</param>
        /// <returns></returns>
        [Authenticate]
        [HttpGet]
        public IQueryable<TreeViewItem> GetSubFolders( string folderName = "", string fileFilter = "*.*" )
        {
            fileFilter = string.IsNullOrWhiteSpace( fileFilter ) ? "*.*" : fileFilter;

            string physicalRootFolder = HttpContext.Current.Request.MapPath( RootContentFolder );
            string contentFolderName = Path.Combine( physicalRootFolder, folderName.TrimStart( new char[] { '/', '\\' } ) );
            List<TreeViewItem> directoryFileList = new List<TreeViewItem>();

            if ( !Directory.Exists( contentFolderName ) )
            {
                // if a non-existent folder was specified, return an empty list
                return directoryFileList.AsQueryable();
            }

            // list the folders that are in the selected directory
            List<string> directoryList = Directory.GetDirectories( contentFolderName ).OrderBy( a => a ).ToList();
            foreach ( string directoryPath in directoryList )
            {
                DirectoryInfo directoryInfo = new DirectoryInfo( directoryPath );
                TreeViewItem directoryNode = new TreeViewItem
                {
                    HasChildren = directoryInfo.EnumerateFiles( fileFilter ).Any() || directoryInfo.EnumerateDirectories().Any(),
                    IconCssClass = "fa fa-folder",
                    Id = directoryInfo.FullName.Remove( 0, physicalRootFolder.Length ),
                    Name = directoryInfo.Name
                };

                directoryFileList.Add( directoryNode );
            }

            return directoryFileList.AsQueryable();
        }

        /// <summary>
        /// Gets the files.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <param name="fileFilter">The file filter.</param>
        /// <returns></returns>
        [Authenticate]
        [HttpGet]
        public IQueryable<FileItem> GetFiles( string folderName = "", string fileFilter = "*.*" )
        {
            fileFilter = string.IsNullOrWhiteSpace( fileFilter ) ? "*.*" : fileFilter;

            string physicalRootFolder = HttpContext.Current.Request.MapPath( RootContentFolder );
            string contentFolderName = Path.Combine( physicalRootFolder, folderName.TrimStart( new char[] { '/', '\\' } ) );

            List<FileItem> fileList = new List<FileItem>();

            if ( !Directory.Exists( contentFolderName ) )
            {
                // if a non-existent folder was specified, return an empty list
                return fileList.AsQueryable();
            }

            List<string> files = Directory.GetFiles( contentFolderName, fileFilter ).OrderBy( a => a ).ToList();
            string apiUrl = VirtualPathUtility.ToAbsolute( "~/api/FileBrowser/GetFileThumbnail" );
            foreach ( var file in files )
            {
                FileInfo fileInfo = new FileInfo( file );

                string relativeFilePath = fileInfo.FullName.Substring( physicalRootFolder.Length );

                // construct the thumbNailUrl so that browser will get the thumbnail image from our GetFileThumbnail()
                string thumbNailUrl = apiUrl + string.Format( "?relativeFilePath={0}&width=100&height=100", HttpUtility.UrlEncode( relativeFilePath ) );

                string deleteScriptFormat = @"$.ajax({{ type: ""DELETE"", url: Rock.settings.get(""baseUrl"") + ""api/FileBrowser/DeleteFile?relativeFilePath={0}""}}); $(this).closest("".rocktree-item"").fadeOut();";

                string deleteScript = string.Format( deleteScriptFormat, HttpUtility.UrlEncode( relativeFilePath ) );

                string nameHtmlFormat = @"
<div class='rollover-container'>
  <div class='rollover-item pull-right'>
    <a title='delete' class='btn btn-xs btn-danger' onclick='{2}'>
      <i class='fa fa-times'></i>
    </a>
  </div>
  <img src='{0}' class='file-browser-image' />
  <br />
  <span class='file-name'>{1}</span>
</div>
";

                FileItem fileItem = new FileItem
                {
                    Id = relativeFilePath,
                    Name = string.Format( nameHtmlFormat, thumbNailUrl, fileInfo.Name, deleteScript ),
                    IconSmallUrl = thumbNailUrl,
                    RelativeFilePath = relativeFilePath,
                    Size = fileInfo.Length,
                    LastModifiedDateTime = fileInfo.LastWriteTime
                };

                fileList.Add( fileItem );
            }

            return fileList.AsQueryable();
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="relativeFilePath">The relative file path.</param>
        [Authenticate]
        [HttpDelete]
        public void DeleteFile( string relativeFilePath )
        {
            string physicalRootFolder = HttpContext.Current.Request.MapPath( RootContentFolder );
            string fullPath = Path.Combine( physicalRootFolder, relativeFilePath.Replace( "/", "\\" ).TrimStart( new char[] { '/', '\\' } ) );
            File.Delete( fullPath );
        }

        /// <summary>
        /// Creates the folder.
        /// </summary>
        /// <param name="relativeFolderPath">The relative folder path.</param>
        [Authenticate]
        [HttpPost]
        public void CreateFolder( string relativeFolderPath )
        {
            // TODO: get CreateFolder to work, test, etc
            string physicalRootFolder = HttpContext.Current.Request.MapPath( RootContentFolder );
            string fullFolderPath = Path.Combine( physicalRootFolder, relativeFolderPath.Replace( "/", "\\" ).TrimStart( new char[] { '/', '\\' } ) );
            Directory.CreateDirectory( fullFolderPath );
        }

        /// <summary>
        /// Gets the file thumbnail
        /// </summary>
        /// <param name="relativeFilePath">The relative file path.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        /// <example>
        ///   <![CDATA[ <img src='api/FileBrowser/GetFileThumbnail?relativeFilePath=External+Site%5cMarketing%5cFunnyCat.gif&width=100&height=100 ]]>
        /// </example>
        [Authenticate]
        [HttpGet]
        public HttpResponseMessage GetFileThumbnail( string relativeFilePath, int? width = 100, int? height = 100 )
        {
            string physicalRootFolder = HttpContext.Current.Request.MapPath( RootContentFolder );
            string fullPath = Path.Combine( physicalRootFolder, relativeFilePath.Replace( "/", "\\" ).TrimStart( new char[] { '/', '\\' } ) );

            // default width/height to 100 if they specified a zero or negative param
            width = width <= 0 ? 100 : width;
            height = height <= 0 ? 100 : height;

            // return a 404 if the file doesn't exist
            if ( !File.Exists( fullPath ) )
            {
                throw new HttpResponseException( new System.Net.Http.HttpResponseMessage( HttpStatusCode.NotFound ) );
            }

            try
            {
                return ResizeAndSendImage( width, height, fullPath );
            }
            catch
            {
                // intentionally ignore exception and assume it isn't an image, then just return a no-picture as the thumbnail

                //TODO ask about picture
                string noimageFileName = HttpContext.Current.Request.MapPath( "~/Assets/Images/no-picture.svg" );

                return ResizeAndSendImage( width, height, noimageFileName );
            }
        }

        /// <summary>
        /// Resizes the and send image.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="fullPath">The full path.</param>
        /// <returns></returns>
        private static HttpResponseMessage ResizeAndSendImage( int? width, int? height, string fullPath )
        {
            if ( Path.GetExtension( fullPath ).Equals( ".svg", StringComparison.OrdinalIgnoreCase ) )
            {
                HttpResponseMessage result = new HttpResponseMessage( HttpStatusCode.OK );
                result.Content = new StreamContent( new FileStream( fullPath, FileMode.Open, FileAccess.Read ) );
                result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue( "image/svg+xml" );
                return result;
            }
            else
            {
                using ( Image image = Image.FromFile( fullPath ) )
                {
                    string mimeType = string.Empty;

                    // try to figure out the MimeType by using the ImageCodeInfo class
                    var codecs = ImageCodecInfo.GetImageEncoders();
                    ImageCodecInfo codecInfo = codecs.FirstOrDefault( a => a.FormatID == image.RawFormat.Guid );
                    if ( codecInfo != null )
                    {
                        mimeType = codecInfo.MimeType;
                    }

                    // load the image into a stream, then use ImageResizer to resize it to the specified width and height (same technique as RockWeb GetImage.ashx.cs)
                    var origImageStream = new MemoryStream();
                    image.Save( origImageStream, image.RawFormat );
                    origImageStream.Position = 0;
                    var resizedStream = new MemoryStream();

                    ImageBuilder.Current.Build( origImageStream, resizedStream, new ResizeSettings { Width = width ?? 100, Height = height ?? 100 } );

                    HttpResponseMessage result = new HttpResponseMessage( HttpStatusCode.OK );
                    resizedStream.Position = 0;
                    result.Content = new StreamContent( resizedStream );
                    result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue( mimeType );
                    return result;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class FileItem : TreeViewItem
        {
            /// <summary>
            /// Gets or sets the relative file path.
            /// </summary>
            /// <value>
            /// The relative file path.
            /// </value>
            public string RelativeFilePath { get; set; }

            /// <summary>
            /// Gets or sets the size of the file in bytes
            /// </summary>
            /// <value>
            /// The size.
            /// </value>
            public long Size { get; set; }

            /// <summary>
            /// Gets or sets the last modified date time.
            /// </summary>
            /// <value>
            /// The last modified date time.
            /// </value>
            public DateTime LastModifiedDateTime { get; set; }
        }
    }
}
