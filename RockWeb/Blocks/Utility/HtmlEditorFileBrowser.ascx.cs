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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;

using Rock;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Utility
{
    /// <summary>
    /// </summary>
    [DisplayName( "HtmlEditor FileBrowser" )]
    [Category( "Utility" )]
    [Description( "Block to be used as part of the RockFileBrowser HtmlEditor Plugin" )]
    [Rock.SystemGuid.BlockTypeGuid( "17A1687B-A2C7-4160-BF2B-2424DF69E9D5" )]
    public partial class HtmlEditorFileBrowser : RockBlock
    {
        #region Page Parameter Keys

        private static class PageParameterKey
        {
            /// <summary>
            /// The relative file path
            /// </summary>
            public const string RelativeFilePath = "RelativeFilePath";

            /// <summary>
            /// The root folder encrypted
            /// </summary>
            public const string RootFolderEncrypted = "rootFolder";

            /// <summary>
            /// The zip uploader enabled encrypted
            /// </summary>
            public const string ZipUploaderEnabledEncrypted = "ZipUploaderEnabled";

            /// <summary>
            /// The modal mode
            /// </summary>
            public const string ModalMode = "ModalMode";

            /// <summary>
            /// The title
            /// </summary>
            public const string Title = "Title";

            /// <summary>
            /// The browser mode
            /// </summary>
            public const string BrowserMode = "BrowserMode";

            /// <summary>
            /// The edit file page
            /// </summary>
            public const string EditFilePage = "editFilePage";
        }

        #endregion Page Parameter Keys

        #region Properties

        private List<string> RestrictedFolders
        {
            get
            {
                return new List<string>()
                {
                    "bin",
                    "App_Data",
                    "App_Code",
                    "App_Browsers",
                    "Assets",
                    "Blocks",
                    "Content",
                    "Plugins",
                    "Scripts",
                    "SqlServerTypes",
                    "Styles",
                    "Themes",
                    "Webhooks"
                };
            }
        }

        private List<string> HiddenFolders
        {
            get
            {
                return new List<string>()
                {
                    "Content\\ASM_Thumbnails"
                };
            }
        }

        private List<string> UploadRestrictedFolders
        {
            get
            {
                return new List<string>()
                {
                    "bin",
                    "App_Code"
                };
            }
        }

        private List<string> RestrictedFileExtension
        {
            get
            {
                return new List<string>()
                {
                    ".bin",
                    ".png",
                    ".jpg",
                    ".ico",
                    ".jpeg",
                    ".config",
                    ".eot",
                    ".woff",
                    ".woff2"
                };
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // only enable the zip file uploaded if we can trust that it was explicitly enabled
            var zipUploaderEnabled = IsZipUploaderEnabled();

            btnUploadZipFile.Visible = zipUploaderEnabled;

            // NOTE: this will return a Status 400 if a valid rootfolder is not specified in the URL
            fuprFileUpload.RootFolder = GetRootFolderPath();

            string submitScriptFormat = @"
    // include the selected folder in the post to ~/FileUploader.ashx
    var selectedFolderPath = $('#{0}').val();
    data.formData = {{ folderPath: selectedFolderPath }};
";

            // setup javascript for when a file is submitted
            fuprFileUpload.SubmitFunctionClientScript = string.Format( submitScriptFormat, hfSelectedFolder.ClientID );

            string doneScriptFormat = @"
    // reselect the node to refresh the list of files
    var selectedFolderPath = $('#{0}').val();
    var foldersTree = $('.js-folder-treeview .treeview').data('rockTree');
    foldersTree.$el.trigger('rockTree:selected', selectedFolderPath);
";

            // setup javascript for when a file is done uploading
            fuprFileUpload.DoneFunctionClientScript = string.Format( doneScriptFormat, hfSelectedFolder.ClientID );

            if ( PageParameter( PageParameterKey.BrowserMode ) == "image" )
            {
                // point file uploads to ImageUploader.ashx which has special rules for file uploads
                fuprFileUpload.UploadUrl = "ImageUploader.ashx";
            }
            else
            {
                // use the default fileUploader url
                fuprFileUpload.UploadUrl = null;
            }
        }

        /// <summary>
        /// Determines whether [is zip uploader enabled].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is zip uploader enabled]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsZipUploaderEnabled()
        {
            bool zipUploaderEnabled = false;
            string zipUploaderEnabledEncrypted = this.PageParameter( PageParameterKey.ZipUploaderEnabledEncrypted );
            if ( zipUploaderEnabledEncrypted.IsNotNullOrWhiteSpace() )
            {
                try
                {
                    zipUploaderEnabled = Rock.Security.Encryption.DecryptString( zipUploaderEnabledEncrypted )?.AsBoolean() ?? false;
                }
                catch
                {
                    zipUploaderEnabled = false;
                }
            }

            return zipUploaderEnabled;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbErrorMessage.Visible = false;

            if ( !this.IsPostBack )
            {
                // handle custom ajax post
                if ( Request.Params["getSelectedFileResult"].AsBoolean() )
                {
                    string fileSelectedResult = getSelectedFileResult( this.Request.Form["selectedFileId"] );

                    Response.Write( fileSelectedResult );
                    base.OnLoad( e );
                    Response.End();
                }

                pnlFileBrowser.CssClass = "is-postback";
                pnlModalHeader.Visible = PageParameter( PageParameterKey.ModalMode ).AsBoolean();
                pnlModalFooterActions.Visible = PageParameter( PageParameterKey.ModalMode ).AsBoolean();
                lTitle.Text = PageParameter( PageParameterKey.Title );

                if ( PageParameter( PageParameterKey.RelativeFilePath ).IsNotNullOrWhiteSpace() )
                {
                    string fileUrl = Server.MapPath( PageParameter( PageParameterKey.RelativeFilePath ) );
                    string physicalRootFolder = this.Request.MapPath( GetRootFolderPath() );
                    if ( File.Exists( fileUrl ) && fileUrl.Contains( physicalRootFolder ) )
                    {
                        string directoryPath = Path.GetDirectoryName( fileUrl );
                        string relativeFolderPath = directoryPath.Replace( physicalRootFolder, string.Empty );
                        hfSelectedFolder.Value = relativeFolderPath;
                    }
                }

                BuildFolderTreeView();
            }

            // handle custom postback events
            string postbackArgs = Request.Params["__EVENTARGUMENT"];
            if ( !string.IsNullOrWhiteSpace( postbackArgs ) )
            {
                string[] nameValue = postbackArgs.Split( new char[] { ':' } );
                if ( nameValue.Count() == 2 )
                {
                    string eventParam = nameValue[0];
                    if ( eventParam.Equals( "folder-selected" ) )
                    {
                        string folderPath = nameValue[1].Replace( @"/", @"\" );
                        hfSelectedFolder.Value = folderPath;
                        ListFolderContents( folderPath );
                    }
                    else if ( eventParam.Equals( "file-delete" ) )
                    {
                        string fileRelativePath = nameValue[1].Replace( @"/", @"\" );
                        DeleteFile( fileRelativePath );
                    }
                }
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Initializes the tree views.
        /// </summary>
        private void BuildFolderTreeView()
        {
            string physicalRootFolder = this.Request.MapPath( GetRootFolderPath() );

            if ( !Directory.Exists( physicalRootFolder ) )
            {
                try
                {
                    Directory.CreateDirectory( physicalRootFolder );
                }
                catch
                {
                    // intentionally ignore the exception (we will show the Warning below)
                }
            }

            if ( Directory.Exists( physicalRootFolder ) && !HiddenFolders.Any( a => physicalRootFolder.IndexOf( a, StringComparison.OrdinalIgnoreCase ) > 0 ) )
            {
                var sb = new StringBuilder();
                sb.AppendLine( "<ul id=\"treeview\">" );
                sb.Append( DirectoryNode( physicalRootFolder, physicalRootFolder ) );
                sb.AppendLine( "</ul>" );

                lblFolders.Text = sb.ToString();
                upnlFolders.Update();
                ListFolderContents( hfSelectedFolder.Value );
            }
            else
            {
                nbWarning.Title = "Warning";
                nbWarning.Text = "Folder does not exist: " + physicalRootFolder;
                nbWarning.Visible = true;
            }
        }

        /// <summary>
        /// Gets the root folder path.
        /// </summary>
        /// <returns></returns>
        private string GetRootFolderPath()
        {
            //// the rootFolder param is encrypted to help prevent the web user from specifying a folder
            //// and must be provided to help prevent directly browsing to this page and getting access to the filesystem
            //// we'll return Http 400 if someone is attempting to get to this page directly without a valid (encrypted) rootFolder specified
            string rootFolderEncrypted = PageParameter( PageParameterKey.RootFolderEncrypted );
            string rootFolder = null;
            if ( !string.IsNullOrWhiteSpace( rootFolderEncrypted ) )
            {
                try
                {
                    rootFolder = Rock.Security.Encryption.DecryptString( rootFolderEncrypted );
                }
                catch ( Exception )
                {
                    // respond with BadRequest if they somehow provided an encrypted rootFolder value that isn't valid
                    Response.StatusCode = 400;
                    Response.End();
                    return null;
                }
            }
            else
            {
                // respond with BadRequest if they did not specify rootFolder
                Response.StatusCode = 400;
                Response.End();
                return null;
            }

            if ( string.IsNullOrWhiteSpace( rootFolder ) )
            {
                // respond with BadRequest if they did not specify rootFolder (or decrypted to emptystring)
                Response.StatusCode = 400;
                Response.End();
                return null;
            }

            // ensure that the folder is formatted to be relative to web root
            if ( !rootFolder.StartsWith( "~/" ) )
            {
                rootFolder = "~/" + rootFolder;
            }

            return rootFolder;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get a list of folders the represent the entire tree at the physical root folder.
        /// </summary>
        /// <param name="directoryPath">The current directory being processed.</param>
        /// <param name="physicalRootFolder">The root directory to process from.</param>
        /// <returns>A collection of strings representing relative path names.</returns>
        protected List<string> GetRecursiveFolders( string directoryPath, string physicalRootFolder )
        {
            DirectoryInfo directoryInfo = new DirectoryInfo( directoryPath );
            string relativeFolderPath = directoryPath.Replace( physicalRootFolder, string.Empty );

            var folders = new List<string>();
            folders.Add( string.IsNullOrEmpty( relativeFolderPath ) ? "\\" : relativeFolderPath );

            try
            {
                List<string> subDirectoryList = Directory.GetDirectories( directoryPath ).OrderBy( a => a ).ToList();

                foreach ( var subDirectoryPath in subDirectoryList )
                {
                    folders.AddRange( GetRecursiveFolders( subDirectoryPath, physicalRootFolder ) );
                }

                return folders;
            }
            catch ( Exception ex )
            {
                ShowErrorMessage( ex, string.Format( "Unable to access folder {0}. Contact your administrator.", directoryPath ) );
                return null;
            }
        }

        /// <summary>
        /// Builds a Directory treenode.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <param name="physicalRootFolder">The physical root folder.</param>
        /// <returns></returns>
        protected string DirectoryNode( string directoryPath, string physicalRootFolder )
        {
            var sb = new StringBuilder();

            DirectoryInfo directoryInfo = new DirectoryInfo( directoryPath );
            string relativeFolderPath = directoryPath.Replace( physicalRootFolder, string.Empty );
            bool dataExpanded = hfSelectedFolder.Value.StartsWith( relativeFolderPath );
            bool selected = hfSelectedFolder.Value == relativeFolderPath;

            sb.AppendFormat( "<li data-expanded='{2}' data-id='{0}'><span class='{3}'> {1}</span> \n", HttpUtility.HtmlEncode( relativeFolderPath ), directoryInfo.Name, dataExpanded.ToTrueFalse().ToLower(), selected ? "selected" : string.Empty );

            try
            {
                List<string> subDirectoryList = Directory.GetDirectories( directoryPath ).OrderBy( a => a ).ToList();

                if ( subDirectoryList.Any() )
                {
                    sb.AppendLine( "<ul>" );

                    foreach ( var subDirectoryPath in subDirectoryList )
                    {
                        if ( !HiddenFolders.Any( a => subDirectoryPath.IndexOf( a, StringComparison.OrdinalIgnoreCase ) > 0 ) )
                        {
                            sb.Append( DirectoryNode( subDirectoryPath, physicalRootFolder ) );
                        }
                    }

                    sb.AppendLine( "</ul>" );
                }

                sb.AppendLine( "</li>" );

                return sb.ToString();
            }
            catch ( Exception ex )
            {
                ShowErrorMessage( ex, string.Format( "Unable to access folder {0}. Contact your administrator.", directoryPath ) );
                return string.Empty;
            }
        }

        /// <summary>
        /// Lists the folder contents.
        /// </summary>
        /// <param name="relativeFolderPath">The folder path.</param>
        protected void ListFolderContents( string relativeFolderPath )
        {
            try
            {
                string rootFolder = GetRootFolderPath();
                string physicalRootFolder = this.MapPath( rootFolder );
                string physicalFolder = Path.Combine( physicalRootFolder, relativeFolderPath.TrimStart( '/', '\\' ) );

                bool isRestricted = false;
                bool isUploadRestricted = false;

                if ( RestrictedFolders.Contains( relativeFolderPath.TrimStart( '/', '\\' ), StringComparer.OrdinalIgnoreCase ) )
                {
                    isRestricted = true;
                }

                if ( UploadRestrictedFolders.Any( a => relativeFolderPath.TrimStart( '/', '\\' ).StartsWith( a, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    isUploadRestricted = true;
                }

                hfIsRestrictedFolder.Value = isRestricted.ToString();
                hfIsUploadRestrictedFolder.Value = isUploadRestricted.ToString();

                var fileTypeWhiteList = "*.*";

                if ( PageParameter( PageParameterKey.BrowserMode ) == "image" )
                {
                    string imageFileTypeWhiteList = GlobalAttributesCache.Get().GetValue( "ContentImageFiletypeWhitelist" );
                    if ( imageFileTypeWhiteList.IsNotNullOrWhiteSpace() )
                    {
                        fileTypeWhiteList = imageFileTypeWhiteList;
                    }
                }

                // Directory.GetFiles doesn't support multiple patterns, so we'll do one at a time
                List<string> fileFilters = fileTypeWhiteList.Split( new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries )
                    .Select( s => s = "*." + s.TrimStart( new char[] { '*', ' ' } ).TrimStart( '.' ) ) // ensure that the filter starts with '*.'
                    .ToList();

                List<string> fileList = new List<string>();
                foreach ( var filter in fileFilters )
                {
                    fileList.AddRange( Directory.GetFiles( physicalFolder, filter ).OrderBy( a => a ).ToList() );
                }

                lbNoFilesFound.Visible = !fileList.Any();
                if ( !fileList.Any() )
                {
                    lblFiles.Text = string.Empty;
                    return;
                }

                var sb = new StringBuilder();
                sb.AppendLine( "<ul class='js-rocklist rocklist'>" );

                string editFilePage = PageParameter( PageParameterKey.EditFilePage );

                foreach ( var filePath in fileList )
                {
                    string ext = Path.GetExtension( filePath );
                    string fileName = Path.GetFileName( filePath ).Replace( "'", "&#39;" );
                    string relativeFilePath = filePath.Replace( physicalRootFolder, string.Empty );
                    string imagePath = rootFolder.TrimEnd( '/', '\\' ) + "/" + relativeFilePath.TrimStart( '/', '\\' ).Replace( "\\", "/" );
                    string imageUrl = this.ResolveUrl( "~/api/FileBrowser/GetFileThumbnail?relativeFilePath=" + HttpUtility.UrlEncode( imagePath ) );

                    string editHtml = string.Empty;
                    if ( !RestrictedFileExtension.Any( a => ext.Equals( a, StringComparison.OrdinalIgnoreCase ) ) && !string.IsNullOrWhiteSpace( editFilePage ) )
                    {
                        string url = editFilePage + "?RelativeFilePath=" + HttpUtility.UrlEncode( imagePath );

                        editHtml = $"<a data-href='{url}' title='Edit' class='btn btn-xs btn-square btn-default js-edit-file action'><i class='fa fa-pencil'></i></a>";
                    }

                    string nameHtmlFormat = @"
<li class='js-rocklist-item rocklist-item' data-id='{0}' title='{2}'>
    <div class='rollover-container'>
        <div class='rollover-item actions'>
            <a title='Delete' class='btn btn-xs btn-square btn-danger js-delete-file action'>
                <i class='fa fa-times'></i>
            </a>
            <a href='{3}' target='_blank' rel='noopener noreferrer' title='Download' class='btn btn-xs btn-square btn-default js-download-file action'>
                <i class='fa fa-download'></i>
            </a>
            {4}
        </div>

        <img src='{1}' class='file-browser-image' />
        <br />
        <span class='file-name'>{2}</span>
    </div>
</li>
";

                    // put the file timestamp as part of the URL to that changed files are loaded from the server instead of the browser cache
                    var fileDateTime = File.GetLastWriteTimeUtc( filePath );
                    imageUrl += "&timeStamp=" + fileDateTime.Ticks.ToString();

                    string nameHtml = string.Format(
                        nameHtmlFormat,
                        HttpUtility.HtmlEncode( relativeFilePath ),
                        imageUrl,
                        fileName,
                        HttpUtility.HtmlEncode( this.ResolveUrl( imagePath ) ),
                        editHtml );

                    sb.AppendLine( nameHtml );
                }

                sb.AppendLine( "</ul>" );

                lblFiles.Text = sb.ToString();
            }
            catch ( Exception ex )
            {
                ShowErrorMessage( ex, string.Format( "Unable to list contents of folder {0}. Contact your administrator.", relativeFolderPath ) );
            }
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="relativeFilePath">The relative file path.</param>
        protected void DeleteFile( string relativeFilePath )
        {
            try
            {
                string rootFolder = GetRootFolderPath();
                string physicalRootFolder = this.MapPath( rootFolder );
                string physicalFilePath = Path.Combine( physicalRootFolder, relativeFilePath.TrimStart( '\\', '/' ) );
                File.Delete( physicalFilePath );
                ListFolderContents( Path.GetDirectoryName( relativeFilePath ) );
            }
            catch ( Exception ex )
            {
                this.ShowErrorMessage( ex, "An error occurred when attempting to  file " + relativeFilePath );
            }
        }

        /// <summary>
        /// Files the selected.
        /// </summary>
        /// <param name="relativeFilePath">The relative file path.</param>
        protected string getSelectedFileResult( string relativeFilePath )
        {
            string rootFolder = GetRootFolderPath();
            string imageUrl = rootFolder.TrimEnd( '\\', '/' ) + '/' + relativeFilePath.TrimStart( '\\', '/' ).Replace( '\\', '/' );

            // pipe delimit in the format 'imageSrcUrl|imageAltText' (use Pipe instead of comma since comma is a valid char for filenames)
            string result = string.Format( "{0}|{1}", imageUrl.TrimStart( '~', '/', '\\' ), Path.GetFileName( relativeFilePath ) );
            return result;
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the lbCreateFolder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCreateFolder_Click( object sender, EventArgs e )
        {
            // truncate dir name if it's really long
            if ( hfSelectedFolder.Value.Length > 20 )
            {
                int startingPoint = hfSelectedFolder.Value.Length - 19;
                tbNewFolderName.PrependText = "..." + hfSelectedFolder.Value.Substring( startingPoint ).TrimEnd( '\\' ) + "\\";
            }
            else
            {
                tbNewFolderName.PrependText = hfSelectedFolder.Value.TrimEnd( '\\' ) + "\\";
            }

            tbNewFolderName.Text = string.Empty;
            mdCreateFolder.Show();
        }

        /// <summary>
        /// Handles the Click event of the lbDeleteFolder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDeleteFolder_Click( object sender, EventArgs e )
        {
            if ( string.IsNullOrWhiteSpace( hfSelectedFolder.Value ) )
            {
                return;
            }

            if ( IsRestrictedFolder( hfSelectedFolder.Value ) )
            {
                // If case they got this far, even though this is a restricted folder, jump out 
                return;
            }

            try
            {
                string selectedPhysicalFolder = GetSelectedPhysicalFolder();
                Directory.Delete( selectedPhysicalFolder, true );

                string rootFolder = GetRootFolderPath();
                string physicalRootFolder = this.MapPath( rootFolder );
                string relativeFolder = selectedPhysicalFolder.Replace( physicalRootFolder, string.Empty );

                // set selected folder to deleted folder's parent
                hfSelectedFolder.Value = Path.GetDirectoryName( relativeFolder );

                BuildFolderTreeView();
            }
            catch ( Exception ex )
            {
                string relativeFolderPath = hfSelectedFolder.Value;
                this.ShowErrorMessage( ex, "An error occurred when attempting to delete folder " + relativeFolderPath );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbRenameFolder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRenameFolder_Click( object sender, EventArgs e )
        {
            if ( string.IsNullOrWhiteSpace( hfSelectedFolder.Value ) )
            {
                return;
            }

            if ( IsRestrictedFolder( hfSelectedFolder.Value ) )
            {
                // If case they got this far, even though this is a restricted folder, jump out 
                return;
            }

            tbOrigFolderName.Description = hfSelectedFolder.Value;
            tbRenameFolderName.PrependText = Path.GetDirectoryName( hfSelectedFolder.Value ) + "\\";
            tbRenameFolderName.Text = string.Empty;

            if ( tbRenameFolderName.PrependText == "\\\\" )
            {
                tbRenameFolderName.PrependText = "\\";
            }

            mdRenameFolder.Show();
        }

        /// <summary>
        /// Handles the Click event of the lbArchive control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbArchive_Click( object sender, EventArgs e )
        {
            if ( !IsZipUploaderEnabled() )
            {
                // just in case they got this far, even thought it isn't enabled
                return;
            }

            if ( string.IsNullOrWhiteSpace( hfSelectedFolder.Value ) )
            {
                return;
            }

            if ( IsRestrictedFolder( hfSelectedFolder.Value ) )
            {
                // If case they got this far, even though this is a restricted folder, jump out 
                return;
            }

            mdArchive.Show();
        }

        /// <summary>
        /// Determines whether [is restricted folder] [the specified folder name].
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <returns>
        ///   <c>true</c> if [is restricted folder] [the specified folder name]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsRestrictedFolder( string folderName )
        {
            return RestrictedFolders.Contains( hfSelectedFolder.Value.TrimStart( '/', '\\' ), StringComparer.OrdinalIgnoreCase );
        }

        /// <summary>
        /// Handles the Click event of the lbMoveFolder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbMoveFolder_Click( object sender, EventArgs e )
        {
            string physicalRootFolder = this.Request.MapPath( GetRootFolderPath() );
            var folders = GetRecursiveFolders( physicalRootFolder, physicalRootFolder );

            if ( string.IsNullOrWhiteSpace( hfSelectedFolder.Value ) )
            {
                return;
            }

            if ( IsRestrictedFolder( hfSelectedFolder.Value ) )
            {
                // If case they got this far, even though this is a restricted folder, jump out 
                return;
            }

            if ( folders != null )
            {
                tbMoveOrigFolderName.Description = hfSelectedFolder.Value;
                var currentFolder = Path.GetDirectoryName( hfSelectedFolder.Value );

                ddlMoveFolderTarget.Items.Clear();
                foreach ( var folder in folders )
                {
                    if ( !folder.StartsWith( hfSelectedFolder.Value ) )
                    {
                        ddlMoveFolderTarget.Items.Add( folder );
                    }
                }

                ddlMoveFolderTarget.SelectedValue = currentFolder;

                mdMoveFolder.Show();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbRefresh control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRefresh_Click( object sender, EventArgs e )
        {
            BuildFolderTreeView();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdRenameFolder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdRenameFolder_SaveClick( object sender, EventArgs e )
        {
            if ( IsRestrictedFolder( hfSelectedFolder.Value ) )
            {
                // If case they got this far, even though this is a restricted folder, jump out 
                return;
            }

            var renameFolderName = tbRenameFolderName.Text;
            if ( IsValidFolderName( renameFolderName ) )
            {
                mdRenameFolder.Hide();
                try
                {
                    string selectedPhysicalFolder = GetSelectedPhysicalFolder();
                    string renamedPhysicalFolder = Path.Combine( Path.GetDirectoryName( selectedPhysicalFolder ), tbRenameFolderName.Text );
                    Directory.Move( selectedPhysicalFolder, renamedPhysicalFolder );

                    // set selected folder to renamed folder
                    hfSelectedFolder.Value = Path.Combine( Path.GetDirectoryName( hfSelectedFolder.Value ), tbRenameFolderName.Text );

                    BuildFolderTreeView();
                }
                catch ( Exception ex )
                {
                    string relativeFolderPath = hfSelectedFolder.Value;
                    this.ShowErrorMessage( ex, "An error occurred when attempting to rename folder " + relativeFolderPath );
                }
            }
            else
            {
                tbRenameFolderName.ShowErrorMessage( "Invalid Folder Name" );
                mdRenameFolder.Show();
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the mdMoveFolder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdMoveFolder_SaveClick( object sender, EventArgs e )
        {
            var targetFolder = ddlMoveFolderTarget.SelectedValue;
            mdMoveFolder.Hide();

            try
            {
                string selectedPhysicalFolder = GetSelectedPhysicalFolder();
                string targetPhysicalFolder = Path.Combine( GetPhysicalFolder( targetFolder ), Path.GetFileName( selectedPhysicalFolder ) );

                if ( !Directory.Exists( targetPhysicalFolder ) && !File.Exists( targetPhysicalFolder ) )
                {
                    Directory.Move( selectedPhysicalFolder, targetPhysicalFolder );

                    // set selected folder to moved folder
                    hfSelectedFolder.Value = Path.Combine( targetFolder, Path.GetFileName( selectedPhysicalFolder ) );

                    BuildFolderTreeView();
                }
                else
                {
                    nbErrorMessage.Text = "A file or folder already exists at that path.";
                    nbErrorMessage.Visible = true;
                }
            }
            catch ( Exception ex )
            {
                string relativeFolderPath = hfSelectedFolder.Value;
                this.ShowErrorMessage( ex, "An error occurred when attempting to rename folder " + relativeFolderPath );
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the mdArchive control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdArchive_SaveClick( object sender, EventArgs e )
        {
            mdArchive.Hide();

            if ( !IsZipUploaderEnabled() )
            {
                // just in case they got this far, even thought it isn't enabled
                return;
            }

            if ( IsRestrictedFolder( hfSelectedFolder.Value ) )
            {
                // If case they got this far, even though this is a restricted folder, jump out 
                return;
            }

            try
            {
                var physicalZipFile = this.Request.MapPath( fupZipUpload.UploadedContentFilePath );
                if ( File.Exists( physicalZipFile ) )
                {
                    string selectedPhysicalFolder = GetSelectedPhysicalFolder();
                    FileInfo fileInfo = new FileInfo( physicalZipFile );
                    if ( fileInfo.Extension.Equals( ".zip", StringComparison.OrdinalIgnoreCase ) )
                    {
                        using ( ZipArchive archive = ZipFile.OpenRead( physicalZipFile ) )
                        {
                            foreach ( ZipArchiveEntry file in archive.Entries )
                            {
                                string completeFileName = Path.Combine( selectedPhysicalFolder, file.FullName );
                                if ( file.Name == string.Empty )
                                {
                                    // Assuming Empty for Directory
                                    Directory.CreateDirectory( Path.GetDirectoryName( completeFileName ) );
                                    continue;
                                }

                                file.ExtractToFile( completeFileName, true );
                            }
                        }
                    }
                    else
                    {
                        nbErrorMessage.Text = "Invalid File Uploaded.";
                        nbErrorMessage.Visible = true;
                    }
                }
                else
                {
                    nbErrorMessage.Text = "Error Uploading the File.";
                    nbErrorMessage.Visible = true;
                }

                File.Delete( physicalZipFile );
                BuildFolderTreeView();
            }
            catch ( Exception ex )
            {
                string relativeFolderPath = hfSelectedFolder.Value;
                this.ShowErrorMessage( ex, "An error occurred when attempting to rename folder " + relativeFolderPath );
            }
        }

        /// <summary>
        /// Determines whether [is valid folder name] [the specified rename folder name].
        /// </summary>
        /// <param name="renameFolderName">Name of the rename folder.</param>
        /// <returns></returns>
        private static bool IsValidFolderName( string renameFolderName )
        {
            var invalidChars = Path.GetInvalidPathChars().ToList();
            invalidChars.Add( '\\' );
            invalidChars.Add( '/' );
            invalidChars.Add( '~' );

            // ensure that folder is a simple folder name (no backslashs, invalidchars, etc)
            var validFolderName = !( renameFolderName.ToList().Any( a => invalidChars.Contains( a ) ) || renameFolderName.StartsWith( ".." ) || renameFolderName.EndsWith( "." ) );
            return validFolderName;
        }

        /// <summary>
        /// Handles the SaveClick event of the mdCreateFolder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdCreateFolder_SaveClick( object sender, EventArgs e )
        {
            if ( IsValidFolderName( tbNewFolderName.Text ) )
            {
                mdCreateFolder.Hide();
                try
                {
                    string selectedPhysicalFolder = GetSelectedPhysicalFolder();
                    Directory.CreateDirectory( Path.Combine( selectedPhysicalFolder, tbNewFolderName.Text ) );
                    BuildFolderTreeView();
                }
                catch ( Exception ex )
                {
                    string relativeFolderPath = hfSelectedFolder.Value;
                    ShowErrorMessage( ex, "An error occurred when attempting to create folder " + relativeFolderPath );
                }
            }
            else
            {
                tbNewFolderName.ShowErrorMessage( "Invalid Folder Name" );
                mdCreateFolder.Show();
            }
        }

        /// <summary>
        /// Shows the error message.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="friendlyMessage">The friendly message.</param>
        private void ShowErrorMessage( Exception ex, string friendlyMessage )
        {
            nbErrorMessage.Text = friendlyMessage;
            nbErrorMessage.Visible = true;
            ExceptionLogService.LogException( ex, this.Context );
        }

        /// <summary>
        /// Gets the physical folder of the specified virtual folder.
        /// </summary>
        /// <returns></returns>
        private string GetPhysicalFolder( string relativeFolderPath )
        {
            string rootFolder = GetRootFolderPath();
            string physicalRootFolder = this.MapPath( rootFolder );
            string selectedPhysicalFolder = Path.Combine( physicalRootFolder, relativeFolderPath.TrimStart( '/', '\\' ) );

            return selectedPhysicalFolder;
        }

        /// <summary>
        /// Gets the selected physical folder.
        /// </summary>
        /// <returns></returns>
        private string GetSelectedPhysicalFolder()
        {
            return GetPhysicalFolder( hfSelectedFolder.Value );
        }
    }
}