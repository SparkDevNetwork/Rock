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
    public partial class FileBrowserController : ApiController, IHasCustomRoutes
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
                name: "FileBrowserGetSubFolders",
                routeTemplate: "api/FileBrowser/GetSubFolders/{folderName}/{fileFilter}",
                defaults: new
                {
                    controller = "FileBrowser",
                    action = "GetSubFolders"
                } );

            routes.MapHttpRoute(
                name: "FileBrowserGetFiles",
                routeTemplate: "api/FileBrowser/GetFiles/{folderName}/{fileFilter}",
                defaults: new
                {
                    controller = "FileBrowser",
                    action = "GetFiles"
                } );

            routes.MapHttpRoute(
                name: "FileBrowserGetFileThumbnail",
                routeTemplate: "api/FileBrowser/GetFileThumbnail",
                defaults: new
                {
                    controller = "FileBrowser",
                    action = "GetFileThumbnail",
                    width = System.Web.Http.RouteParameter.Optional,
                    height = System.Web.Http.RouteParameter.Optional
                } );
        }

        /// <summary>
        /// Gets the sub folders.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <param name="fileFilter">The file filter.</param>
        /// <returns></returns>
        [Authenticate]
        public IQueryable<TreeViewItem> GetSubFolders( string folderName, string fileFilter )
        {
            fileFilter = string.IsNullOrWhiteSpace( fileFilter ) ? "*.*" : fileFilter;

            string physicalRootFolder = System.Web.VirtualPathUtility.ToAbsolute( RootContentFolder );
            string contentFolderName = Path.Combine( physicalRootFolder, folderName );
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
                    HasChildren = directoryInfo.EnumerateFiles( fileFilter ).Any(),
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
        public IQueryable<FileItem> GetFiles( string folderName, string fileFilter )
        {
            fileFilter = string.IsNullOrWhiteSpace( fileFilter ) ? "*.*" : fileFilter;

            string physicalRootFolder = System.Web.VirtualPathUtility.ToAbsolute( RootContentFolder );
            string contentFolderName = Path.Combine( physicalRootFolder, folderName );

            List<FileItem> fileList = new List<FileItem>();

            if ( !Directory.Exists( contentFolderName ) )
            {
                // if a non-existent folder was specified, return an empty list
                return fileList.AsQueryable();
            }

            List<string> files = Directory.GetFiles( contentFolderName, fileFilter ).OrderBy( a => a ).ToList();
            foreach ( var file in files )
            {
                FileInfo fileInfo = new FileInfo( file );

                string relativeFilePath = fileInfo.FullName.Substring( 0, RootContentFolder.Length );
                string appPath = System.Web.VirtualPathUtility.ToAbsolute( "~" );

                // construct the thumbNailUrl so that browser will get the thumbnail image from our GetFileThumbnail()
                string thumbNailUrl = string.Format( "api/FileBrowser/GetFileThumbnail?{0}&width=100&height=100", HttpUtility.UrlEncode( relativeFilePath ) );

                FileItem fileItem = new FileItem
                {
                    FileName = fileInfo.Name,
                    RelativeFilePath = relativeFilePath,
                    Size = fileInfo.Length,
                    LastModifiedDateTime = fileInfo.LastWriteTime,
                    ThumbNailUrl = Path.Combine( appPath, thumbNailUrl )
                };

                fileList.Add( fileItem );
            }

            return fileList.AsQueryable();
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
        public HttpResponseMessage GetFileThumbnail( string relativeFilePath, int? width = 100, int? height = 100 )
        {
            string physicalRootFolder = HttpContext.Current.Request.MapPath( RootContentFolder );

            string fullPath = Path.Combine( physicalRootFolder, relativeFilePath.Replace( "/", "\\" ) );

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
                    result.Content = new StreamContent(resizedStream);
                    result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue( mimeType );
                    return result;
                }
            }
            catch
            {
                // intentionally ignore exception and assume it isn't an image, then just return a no-picture as the thumbnail
                string noimageFileName = HttpContext.Current.Request.MapPath( "~/Assets/Images/no-picture.svg" );
                FileStream fs = new FileStream( noimageFileName, FileMode.Open );
                fs.Position = 0;

                HttpResponseMessage result = new HttpResponseMessage( HttpStatusCode.OK );
                result.Content = new StreamContent( fs );
                result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue( "image/svg+xml" );
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class FileItem
        {
            /// <summary>
            /// Gets or sets the name of the file.
            /// </summary>
            /// <value>
            /// The name of the file.
            /// </value>
            public string FileName { get; set; }

            /// <summary>
            /// Gets or sets the thumb nail URL.
            /// </summary>
            /// <value>
            /// The thumb nail URL.
            /// </value>
            public string ThumbNailUrl { get; set; }

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
