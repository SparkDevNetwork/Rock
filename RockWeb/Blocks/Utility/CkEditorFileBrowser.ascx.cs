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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Utility
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "CkEditor FileBrowser" )]
    [Category( "Utility" )]
    [Description( "Block to be used as part of the RockFileBrowser CKEditor Plugin" )]
    public partial class CkEditorFileBrowser : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            iupFileBrowser.RootFolder = GetRootFolderPath();

            // setup javascript for when a file is submitted
            iupFileBrowser.SubmitFunctionClientScript = string.Format( @"
    var selectedFolderPath = $('#{0}').val();
    if (selectedFolderPath) {{
        // include the selected folder in the post to ~/ImageUploader.ashx
        data.formData = {{ folderPath: selectedFolderPath }};
    }}
    else {{
        // no directory selected
        return false;
    }}
", hfSelectedFolder.ClientID);

            // setup javascript for when a file is done uploading
            iupFileBrowser.DoneFunctionClientScript = string.Format( @"
    var selectedFolderPath = $('#{0}').val();
    var foldersTree = $('.js-folder-treeview .treeview').data('rockTree');
    if (selectedFolderPath) {{
        // reselect the node to refresh the list of files
        foldersTree.$el.trigger('rockTree:selected', selectedFolderPath);
    }}
    else {{
        // no directory selected
        return false;
    }}
", hfSelectedFolder.ClientID );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !this.IsPostBack )
            {
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
                        string folderPath = nameValue[1];
                        hfSelectedFolder.Value = folderPath;
                        ListFolderContents( folderPath );
                    }
                    else if (eventParam.Equals("file-delete"))
                    {
                        string fileRelativePath = nameValue[1];
                        DeleteFile( fileRelativePath );
                    }
                }
            }

            // handle custom ajax post
            if (Request.Params["getSelectedFileResult"].AsBoolean())
            {
                string fileSelectedResult = getSelectedFileResult( this.Request.Form["selectedFileId"] );
                
                Response.Write(fileSelectedResult);
                Response.End();
            }
        }

        /// <summary>
        /// Initializes the tree views.
        /// </summary>
        private void BuildFolderTreeView()
        {
            var sb = new StringBuilder();
            sb.AppendLine( "<ul id=\"treeview\">" );
            string physicalRootFolder = this.Request.MapPath( GetRootFolderPath() );

            if ( Directory.Exists( physicalRootFolder ) )
            {
                List<string> directoryList = Directory.GetDirectories( physicalRootFolder ).OrderBy( a => a ).ToList();

                foreach ( string directoryPath in directoryList )
                {
                    sb.Append( DirectoryNode( directoryPath, physicalRootFolder ) );
                }

                sb.AppendLine( "</ul>" );

                lblFolders.Text = sb.ToString();
                upnlFolders.Update();
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
            // the rootFolder param is encrypted to help prevent the web user from specifying a folder
            string rootFolder = PageParameter( "rootFolder" );
            if ( !string.IsNullOrWhiteSpace( rootFolder ) )
            {
                rootFolder = Rock.Security.Encryption.DecryptString( rootFolder );
            }

            // default to the "~/Content" if the rootFolder is not specified
            if ( string.IsNullOrWhiteSpace( rootFolder ) )
            {
                rootFolder = "~/Content";
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

            sb.AppendFormat( "<li data-expanded='{2}' data-id='{0}'><span class='{3}'><i class=\"fa fa-folder\"></i> {1}</span> \n", relativeFolderPath, directoryInfo.Name, dataExpanded.ToTrueFalse().ToLower(), selected ? "selected" : string.Empty );

            List<string> subDirectoryList = Directory.GetDirectories( directoryPath ).OrderBy( a => a ).ToList();

            if ( subDirectoryList.Any() )
            {
                sb.AppendLine( "<ul>" );

                foreach ( var subDirectoryPath in subDirectoryList )
                {
                    sb.Append( DirectoryNode( subDirectoryPath, physicalRootFolder ) );
                }

                sb.AppendLine( "</ul>" );
            }

            sb.AppendLine( "</li>" );

            return sb.ToString();
        }

        /// <summary>
        /// Lists the folder contents.
        /// </summary>
        /// <param name="relativeFolderPath">The folder path.</param>
        protected void ListFolderContents( string relativeFolderPath )
        {
            string rootFolder = GetRootFolderPath();
            string physicalRootFolder = this.MapPath( rootFolder );
            string physicalFolder = Path.Combine( physicalRootFolder, relativeFolderPath.TrimStart( '/', '\\' ) );

            var sb = new StringBuilder();
            sb.AppendLine( "<ul>" );

            var fileList = Directory.GetFiles( physicalFolder, "*.*" ).OrderBy( a => a ).ToList();
            foreach ( var filePath in fileList )
            {

                string nameHtmlFormat = @"
<li data-id='{2}' data-expanded='true'>
    <span class='rocktree-name'>
        <div class='rollover-container'>
          <div class='rollover-item pull-right'>
            <a title='delete' class='btn btn-xs btn-danger js-delete-file'>
              <i class='fa fa-times'></i>
            </a>
          </div>
          <img src='{0}' class='file-browser-image' />
          <br />
          <span class='file-name'>{1}</span>
        </div>
    </span>
</li>
";

                string fileName = Path.GetFileName( filePath );
                string relativeFilePath = filePath.Replace( physicalRootFolder, string.Empty );
                string imagePath = rootFolder.TrimEnd( '/', '\\' ) + "/" + relativeFilePath.TrimStart( '/', '\\' ).Replace( "\\", "/" );
                string imageUrl = this.ResolveUrl( "~/api/FileBrowser/GetFileThumbnail?relativeFilePath=" + HttpUtility.UrlEncode( imagePath ) + "&width=100&height=100" );

                sb.AppendLine( string.Format( nameHtmlFormat, imageUrl, fileName, relativeFilePath ) );
            }

            sb.AppendLine( "</ul>" );

            lblFiles.Text = sb.ToString();
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="relativeFilePath">The relative file path.</param>
        protected void DeleteFile(string relativeFilePath)
        {
            string rootFolder = GetRootFolderPath();
            string physicalRootFolder = this.MapPath( rootFolder );
            string physicalFilePath = Path.Combine( physicalRootFolder, relativeFilePath.TrimStart('\\', '/') );
            File.Delete( physicalFilePath );
            ListFolderContents( Path.GetDirectoryName( relativeFilePath ) );
        }

        /// <summary>
        /// Files the selected.
        /// </summary>
        /// <param name="relativeFilePath">The relative file path.</param>
        protected string getSelectedFileResult( string relativeFilePath )
        {
            string rootFolder = GetRootFolderPath();
            string imageUrl = rootFolder.TrimEnd('\\', '/') + '/' + relativeFilePath.TrimStart('\\', '/').Replace('\\', '/');
            return string.Format( "{0},{1}", imageUrl, relativeFilePath );
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the lbCreateFolder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCreateFolder_Click( object sender, EventArgs e )
        {
            tbNewFolderName.PrependText = hfSelectedFolder.Value.TrimEnd( '\\' ) + "\\";
            tbNewFolderName.Text = "";
            mdCreateFolder.Show();
        }

        /// <summary>
        /// Handles the Click event of the lbDeleteFolder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDeleteFolder_Click( object sender, EventArgs e )
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

        /// <summary>
        /// Handles the Click event of the lbRenameFolder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRenameFolder_Click( object sender, EventArgs e )
        {
            tbOrigFolderName.Text = hfSelectedFolder.Value;
            tbRenameFolderName.PrependText = Path.GetDirectoryName( hfSelectedFolder.Value ) + "\\";
            tbRenameFolderName.Text = "";
            mdRenameFolder.Show();
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
            var renameFolderName = tbRenameFolderName.Text;
            if ( IsValidFolderName( renameFolderName ) )
            {

                string selectedPhysicalFolder = GetSelectedPhysicalFolder();
                string renamedPhysicalFolder = Path.Combine( Path.GetDirectoryName( selectedPhysicalFolder ), tbRenameFolderName.Text );
                Directory.Move( selectedPhysicalFolder, renamedPhysicalFolder );

                // set selected folder to renamed folder
                hfSelectedFolder.Value = Path.Combine( Path.GetDirectoryName( hfSelectedFolder.Value ), tbRenameFolderName.Text );

                BuildFolderTreeView();
            }
            else
            {
                tbRenameFolderName.ShowErrorMessage( "Invalid Folder Name" );
                mdRenameFolder.Show();
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
            var validFolderName = !( renameFolderName.ToList().Any( a => invalidChars.Contains( a ) ) || renameFolderName.StartsWith( ".." ) );
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
                string selectedPhysicalFolder = GetSelectedPhysicalFolder();
                Directory.CreateDirectory( Path.Combine( selectedPhysicalFolder, tbNewFolderName.Text ) );
                BuildFolderTreeView();
            }
            else
            {
                tbNewFolderName.ShowErrorMessage( "Invalid Folder Name" );
                mdCreateFolder.Show();
            }
        }

        /// <summary>
        /// Gets the selected physical folder.
        /// </summary>
        /// <returns></returns>
        private string GetSelectedPhysicalFolder()
        {
            string relativeFolderPath = hfSelectedFolder.Value;
            string rootFolder = GetRootFolderPath();
            string physicalRootFolder = this.MapPath( rootFolder );
            string selectedPhysicalFolder = Path.Combine( physicalRootFolder, relativeFolderPath.TrimStart( '/', '\\' ) );

            return selectedPhysicalFolder;
        }
    }
}