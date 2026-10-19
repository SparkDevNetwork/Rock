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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "File Manager" )]
    [Category( "CMS" )]
    [Description( "Block that can be used to browse and manage files on the web server" )]

    #region Block Attributes

    [TextField(
        "Relative File Path",
        Description = "The relative path to file",
        IsRequired = false,
        Key = AttributeKey.RelativeFilePath )]

    [TextField(
        "Root Folder",
        Key = AttributeKey.RootFolder,
        Description = "The folder that limits which files can be passed in the URL. Files outside it cannot be opened or saved. The file set in Relative File Path is always allowed. Must start with ~/. Uses ~/Content when blank.",
        IsRequired = false,
        DefaultValue = "~/Content",
        Order = 1 )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "0F1DADBC-6B12-4BAA-A828-FD1AA86AA387" )]
    public partial class FileEditor : RockBlock
    {
        #region Page Parameter Keys

        /// <summary>
        /// Just in case something passes in specific configuration in the URL
        /// </summary>
        private static class PageParameterKey
        {
            public const string RelativeFilePath = "RelativeFilePath";
        }

        #endregion Page Parameter Keys

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string RelativeFilePath = "RelativeFilePath";
            public const string RootFolder = "RootFolder";
        }

        #endregion Attribute Keys

        #region Fields

        string _fileRelativePath = string.Empty;
        string _filePhysicalPath = string.Empty;

        private const string DefaultRootFolder = "~/Content";
        private const string InvalidPathMessage = "Invalid relative file path.";
        private const string FileTypeNotAllowedMessage = "Viewing/Editing of this file type is not allowed.";

        // These apply whatever the Root Folder is, so widening the root
        // never exposes server code, configuration or application data.
        private static readonly string[] RestrictedFolders = { "Bin", "App_Code", "App_Data", "App_Browsers", "App_GlobalResources", "App_LocalResources" };
        private static readonly string[] RestrictedCodeFileExtensions = { ".cs", ".vb", ".ascx", ".aspx", ".ashx", ".asmx", ".asax", ".master", ".svc", ".cshtml", ".vbhtml", ".config", ".dll", ".exe", ".skin", ".browser" };

        #endregion

        #region Properties

        private Dictionary<string, CodeEditorMode> EditorMode
        {
            get
            {
                return new Dictionary<string, CodeEditorMode>()
                {
                    { ".ascx", CodeEditorMode.Html},
                    { ".cs",CodeEditorMode.CSharp},
                    { ".html",CodeEditorMode.Html},
                    {".less",CodeEditorMode.Css },
                    {".css",CodeEditorMode.Css },
                    {".lava",CodeEditorMode.Lava }
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

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            _fileRelativePath = GetRelativePath();

            if ( !Page.IsPostBack )
            {
                ShowDetail();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( !IsUserAuthorized( Rock.Security.Authorization.EDIT ) )
            {
                ShowError( "You are not authorized to edit this file." );
                return;
            }

            var fileRelativePath = hfRelativePath.Value;
            if ( string.IsNullOrWhiteSpace( fileRelativePath ) || !string.Equals( fileRelativePath, _fileRelativePath, StringComparison.Ordinal ) )
            {
                ShowError( "Error while saving the content back to file" );
                return;
            }

            if ( !TryGetValidatedFilePath( _fileRelativePath, out var physicalPath, out var errorMessage ) )
            {
                ShowError( errorMessage );
                return;
            }

            File.WriteAllText( physicalPath, ceFilerEditor.Text );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            Dictionary<string, string> queryString = new Dictionary<string, string>();
            var relativePath = PageParameter( PageParameterKey.RelativeFilePath );
            if ( relativePath.IsNotNullOrWhiteSpace() )
            {
                queryString.Add( PageParameterKey.RelativeFilePath, relativePath );
            }
            NavigateToParentPage( queryString );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            nbWarningMessage.Visible = false;
            pnlEditDetails.Visible = false;

            if ( !TryGetValidatedFilePath( _fileRelativePath, out var physicalPath, out var errorMessage ) )
            {
                ShowError( errorMessage );
                return;
            }

            _filePhysicalPath = physicalPath;

            string ext = Path.GetExtension( _filePhysicalPath ).ToLower();

            hfRelativePath.Value = _fileRelativePath;
            pnlEditDetails.Visible = true;
            if ( EditorMode.ContainsKey( ext ) )
            {
                ceFilerEditor.EditorMode = EditorMode[ext];
            }
            else
            {
                ceFilerEditor.EditorMode = CodeEditorMode.Text;
            }

            var canEdit = IsUserAuthorized( Rock.Security.Authorization.EDIT );
            btnSave.Visible = canEdit;
            ceFilerEditor.ReadOnly = !canEdit;

            string content = File.ReadAllText( _filePhysicalPath );

            if ( !string.IsNullOrEmpty( content ) )
            {
                ceFilerEditor.Text = content;
            }
        }

        /// <summary>
        /// Determines which item to edit based on either the configuration or the relativeFilePath that was passed in.
        /// </summary>
        private string GetRelativePath()
        {
            string relativePath = GetAttributeValue( AttributeKey.RelativeFilePath );

            // A configured defined type takes precedence over any definedTypeId param value that is passed in.
            if ( string.IsNullOrWhiteSpace( relativePath ) )
            {
                relativePath = PageParameter( PageParameterKey.RelativeFilePath );
            }

            return relativePath;
        }

        /// <summary>
        /// Resolves a relative file path to a physical path the editor is allowed to open and save.
        /// </summary>
        /// <param name="relativeFilePath">The virtual path of the file, starting with ~/.</param>
        /// <param name="physicalPath">The full physical path of the file when it is allowed.</param>
        /// <param name="errorMessage">The message to show when the file is not allowed.</param>
        /// <returns><c>true</c> if the file exists, is within the Root Folder or is the configured Relative File Path, and may be edited; otherwise <c>false</c>.</returns>
        private bool TryGetValidatedFilePath( string relativeFilePath, out string physicalPath, out string errorMessage )
        {
            physicalPath = null;
            errorMessage = InvalidPathMessage;

            // A ":" would allow drive letters, UNC paths and NTFS stream names.
            if ( relativeFilePath.IsNullOrWhiteSpace() || !relativeFilePath.StartsWith( "~/" ) || relativeFilePath.Contains( ":" ) )
            {
                return false;
            }

            var rootFolder = GetAttributeValue( AttributeKey.RootFolder );
            if ( rootFolder.IsNullOrWhiteSpace() )
            {
                rootFolder = DefaultRootFolder;
            }

            if ( !rootFolder.StartsWith( "~/" ) )
            {
                errorMessage = "The File Editor Root Folder setting is not valid.";
                return false;
            }

            string physicalWebRoot;
            string physicalRoot;
            string fullPath;

            try
            {
                if ( !Rock.Utility.FileUtilities.IsFileTypeAllowed( Path.GetFileName( relativeFilePath ) ) )
                {
                    errorMessage = FileTypeNotAllowedMessage;
                    return false;
                }

                // Server.MapPath throws for paths that climb above the application root.
                physicalWebRoot = Path.GetFullPath( Server.MapPath( "~/" ) ).TrimEnd( '/', '\\' );
                physicalRoot = Path.GetFullPath( Server.MapPath( rootFolder ) );
                fullPath = Path.GetFullPath( Server.MapPath( relativeFilePath ) );
            }
            catch ( Exception )
            {
                return false;
            }

            if ( !Rock.Utility.FileUtilities.IsPathWithinFolder( physicalRoot, physicalWebRoot ) )
            {
                errorMessage = "The File Editor Root Folder setting is not valid.";
                return false;
            }

            if ( !Rock.Utility.FileUtilities.IsPathWithinFolder( fullPath, physicalWebRoot ) )
            {
                return false;
            }

            /*
                10/16/26 - JMH

                A file set in the Relative File Path block setting was chosen by an
                administrator, so it is trusted even when it sits outside the Root Folder.
                This keeps existing blocks pinned to files outside ~/Content working.
                The Root Folder only guards paths passed in the URL.

                Reason: Confine URL-supplied paths without breaking configured blocks.
            */
            var isConfiguredFile = string.Equals( relativeFilePath, GetAttributeValue( AttributeKey.RelativeFilePath ), StringComparison.Ordinal );
            if ( !isConfiguredFile && !Rock.Utility.FileUtilities.IsPathWithinFolder( fullPath, physicalRoot ) )
            {
                return false;
            }

            // Short (8.3) names such as APP_CO~1 contain "~" and would not match the restricted folder names.
            var webRootRelativePath = fullPath.Substring( physicalWebRoot.Length );
            if ( webRootRelativePath.Contains( "~" )
                || RestrictedFolders.Any( f => Rock.Utility.FileUtilities.IsPathWithinFolder( fullPath, Path.Combine( physicalWebRoot, f ) ) ) )
            {
                return false;
            }

            var ext = Path.GetExtension( fullPath );
            if ( RestrictedFileExtension.Any( a => ext.Equals( a, StringComparison.OrdinalIgnoreCase ) )
                || RestrictedCodeFileExtensions.Any( a => ext.Equals( a, StringComparison.OrdinalIgnoreCase ) ) )
            {
                errorMessage = FileTypeNotAllowedMessage;
                return false;
            }

            // Checked last so the response can't be used to probe for files outside the root.
            if ( !File.Exists( fullPath ) )
            {
                return false;
            }

            physicalPath = fullPath;
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Shows an error message in the block's notification box.
        /// </summary>
        /// <param name="message">The message to show.</param>
        private void ShowError( string message )
        {
            nbWarningMessage.Text = message;
            nbWarningMessage.NotificationBoxType = NotificationBoxType.Danger;
            nbWarningMessage.Visible = true;
        }

        #endregion
    }
}