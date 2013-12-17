using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
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
        private const string rootContentFolder = "~/Content";

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

            string physicalRootFolder = System.Web.VirtualPathUtility.ToAbsolute( rootContentFolder );
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

            string physicalRootFolder = System.Web.VirtualPathUtility.ToAbsolute( rootContentFolder );
            string contentFolderName = Path.Combine( physicalRootFolder, folderName );

            List<FileItem> fileList = new List<FileItem>();

            if ( !Directory.Exists( contentFolderName ) )
            {
                // if a non-existent folder was specified, return an empty list
                return fileList.AsQueryable();
            }

            List<string> files = Directory.GetFiles( contentFolderName, fileFilter ).OrderBy( a => a ).ToList();
            foreach (var file in files)
            {
                FileInfo fileInfo = new FileInfo( file );

                FileItem fileItem = new FileItem
                {
                    FileName = fileInfo.Name,
                    RelativeFilePath = fileInfo.FullName.Substring( 0, rootContentFolder.Length ),
                    Bytes = fileInfo.Length,
                    LastModifiedDateTime = fileInfo.LastWriteTime
                };

                fileList.Add(fileItem);
            }



            return fileList.AsQueryable();
        }

        /// <summary>
        /// Gets the file thumbnail
        /// </summary>
        /// <example>
        /// <![CDATA[ <img src='api/FileBrowser/GetFile?%5cExternal+Site%5cMarketing%5cFunnyCat.gif ]]>
        /// </example>
        /// <param name="relativeFilePath">The relative file path.</param>
        /// <returns></returns>
        public Stream GetFileThumbnail( string relativeFilePath, int width, int height)
        {
            string physicalRootFolder = System.Web.VirtualPathUtility.ToAbsolute( rootContentFolder );
            string fullPath = Path.Combine(physicalRootFolder, relativeFilePath);

            // default width/height to 100 if not specified
            width = width <= 0 ? 100 : width;
            height = height <= 0 ? 100 : height;


            // return Null if the file doesn't exist
            if (!File.Exists(fullPath))
            {
                return null;
            }

            try
            {
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap( fullPath );
                
                // TODO get a resized thumbnail image
            }
            catch
            {
                // intentionally ignore exception and return null
                return null;
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
            /// Gets or sets the bytes.
            /// </summary>
            /// <value>
            /// The bytes.
            /// </value>
            public long Bytes { get; set; }
            
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
