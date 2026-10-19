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

using Rock.Attribute;
using Rock.Configuration;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.FileEditor;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Manage files stored on a remote server or 3rd party cloud storage
    /// </summary>

    [DisplayName( "File Editor" )]
    [Category( "CMS" )]
    [Description( "Edit files directly via an editor" )]
    [IconCssClass( "ti ti-edit" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [TextField(
        "Relative File Path",
        Description = "The relative path to file",
        IsRequired = false,
        Key = AttributeKey.RelativeFilePath,
        Order = 0
    )]

    [TextField(
        "Root Folder",
        Key = AttributeKey.RootFolder,
        Description = "The folder that limits which files can be passed in the URL. Files outside it cannot be opened or saved. The file set in Relative File Path is always allowed. Must start with ~/. Uses ~/Content when blank.",
        IsRequired = false,
        DefaultValue = "~/Content",
        Order = 1 )]

    #endregion Block Attributes

    // was [Rock.SystemGuid.BlockTypeGuid( "327370A5-3CC0-42AD-B236-F45260F6A1EE" )]
    [Rock.SystemGuid.BlockTypeGuid( "0F1DADBC-6B12-4BAA-A828-FD1AA86AA387" )]
    [Rock.SystemGuid.EntityTypeGuid( "EFD19035-70F5-4135-9D76-A1EE024D7D7B")]
    public partial class FileEditor : RockBlockType
    {

        #region Fields

        private string _fileRelativePath = string.Empty;
        private string _filePhysicalPath = string.Empty;
        private string _worksurfaceLayoutName = "Full Worksurface";

        #endregion Fields

        #region Properties

        private const string FriendlyBlockName = "File Editor";
        private const string DefaultRootFolder = "~/Content";
        private const string InvalidPathMessage = "Invalid relative file path.";
        private const string FileTypeNotAllowedMessage = "Viewing/Editing of this file type is not allowed.";
        private const string InvalidRootFolderMessage = "The File Editor Root Folder setting is not valid.";

        // These apply whatever the Root Folder is, so widening the root
        // never exposes server code, configuration or application data.
        private static readonly string[] RestrictedFolders = { "Bin", "App_Code", "App_Data", "App_Browsers", "App_GlobalResources", "App_LocalResources" };
        private static readonly string[] RestrictedCodeFileExtensions = { ".cs", ".vb", ".ascx", ".aspx", ".ashx", ".asmx", ".asax", ".master", ".svc", ".cshtml", ".vbhtml", ".config", ".dll", ".exe", ".skin", ".browser" };

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

        #endregion Properties

        #region Keys

        private static class AttributeKey
        {
            public const string RelativeFilePath = "RelativeFilePath";
            public const string RootFolder = "RootFolder";
        }

        private static class PageParameterKey
        {
            public const string RelativeFilePath = "RelativeFilePath";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        private static class EditorMode
        {
            public const string Text = "text";
            public const string Css = "css";
            public const string Html = "html";
            public const string Lava = "lava";
            public const string Javascript = "javascript";
            public const string Less = "less";
            public const string Powershell = "powershell";
            public const string Sql = "sql";
            public const string Typescript = "typescript";
            public const string CSharp = "csharp";
            public const string Markdown = "markdown";
            public const string Xml = "xml";
            public const string Json = "json";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            string errorMessage = string.Empty;

            if ( !IsPersonAuthorized( out errorMessage ) )
            {
                return new CustomBlockBox<FileEditorBag, FileEditorOptionsBag>
                {
                    ErrorMessage = errorMessage
                };
            }

            if ( !IsRelativeFilePathGood( out errorMessage ) )
            {
                return new CustomBlockBox<FileEditorBag, FileEditorOptionsBag>
                {
                    ErrorMessage = errorMessage
                };
            }

            return GetBoxInitialState();
        }

        /// <summary>
        /// Gets the initial state of the box. Populates the CustomBag or
        /// ErrorMessage properties depending on the data and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private CustomBlockBox<FileEditorBag, FileEditorOptionsBag> GetBoxInitialState()
        {
            var box = new CustomBlockBox<FileEditorBag, FileEditorOptionsBag>
            {
                Bag = GetBoxBag(),
                NavigationUrls = GetBoxNavigationUrls(),
                Options = GetBoxOptions(),
            };

            return box;
        }

        /// <summary>
        /// Creates and returns a <see cref="FileEditorBag"/> containing the file path and contents of the file
        /// specified by the relative file path.
        /// </summary>
        /// <remarks>The file path is resolved using the application's hosting environment. The method
        /// reads the entire contents of the file into memory, so it is not suitable for very large files.</remarks>
        /// <returns>A <see cref="FileEditorBag"/> containing the resolved file path and the file's contents.</returns>
        private FileEditorBag GetBoxBag()
        {
            var bag = new FileEditorBag
            {
                FilePath = _filePhysicalPath,
                FileContents = File.ReadAllText( _filePhysicalPath ),
            };

            return bag;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl(
                    new Dictionary<string, string>
                    {
                        ["RelativeFilePath"] = PageParameter( PageParameterKey.RelativeFilePath ),
                    }
                ),
            };
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private FileEditorOptionsBag GetBoxOptions()
        {
            var options = new FileEditorOptionsBag
            {
                IsEditable = IsPersonEditOrAdminAuthorized(),
                EditorMode = GetEditorModeFromFileExtension( Path.GetExtension( _filePhysicalPath ).ToLower() ),
                isWorksurfaceLayout = PageCache.Layout.ToString() == _worksurfaceLayoutName,
            };
            return options;
        }

        /// <summary>
        /// Validates that a relative file path was provided and names a file the editor may open and save.
        /// </summary>
        /// <param name="errorMessage">The message to show when validation fails.</param>
        /// <returns><c>true</c> if the file may be edited; otherwise <c>false</c>.</returns>
        /// <remarks>On success, sets the relative and physical paths that <see cref="GetBoxBag"/> and <see cref="Save"/> use.</remarks>
        private bool IsRelativeFilePathGood( out string errorMessage )
        {
            errorMessage = string.Empty;

            // Check that we have a relative file path
            if ( !GetRelativePath( out string relativeFilePath ) )
            {
                errorMessage = $"A relative file path was not provided to the {FriendlyBlockName}.";
                return false;
            }

            if ( !TryGetValidatedFilePath( relativeFilePath, out var physicalPath, out errorMessage ) )
            {
                return false;
            }

            _fileRelativePath = relativeFilePath;
            _filePhysicalPath = physicalPath;

            return true;
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

            // MapPath returns absolute, drive and UNC paths unchanged, and a ":"
            // would also allow NTFS stream names.
            if ( relativeFilePath.IsNullOrWhiteSpace() || !relativeFilePath.StartsWith( "~/" ) || relativeFilePath.Contains( ":" ) )
            {
                return false;
            }

            var rootFolder = GetAttributeValue( AttributeKey.RootFolder );
            if ( rootFolder.IsNullOrWhiteSpace() )
            {
                rootFolder = DefaultRootFolder;
            }

            if ( !rootFolder.StartsWith( "~/" ) || rootFolder.Contains( ":" ) )
            {
                errorMessage = InvalidRootFolderMessage;
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

                physicalWebRoot = Path.GetFullPath( RockApp.Current.MapPath( "~/" ) ).TrimEnd( '/', '\\' );
                physicalRoot = Path.GetFullPath( RockApp.Current.MapPath( rootFolder ) );
                fullPath = Path.GetFullPath( RockApp.Current.MapPath( relativeFilePath ) );
            }
            catch ( Exception )
            {
                return false;
            }

            if ( !Rock.Utility.FileUtilities.IsPathWithinFolder( physicalRoot, physicalWebRoot ) )
            {
                errorMessage = InvalidRootFolderMessage;
                return false;
            }

            if ( !Rock.Utility.FileUtilities.IsPathWithinFolder( fullPath, physicalWebRoot ) )
            {
                return false;
            }

            /*
                10/19/26 - JMH

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
        /// Determines whether the current person is authorized to access
        /// the specified block and returns an error message if not.
        /// </summary>
        /// <returns>A string containing an error message if the person is
        /// not authorized to view the block; otherwise, an empty
        /// string.</returns>
        private bool IsPersonAuthorized( out string errorMessage )
        {
            errorMessage = string.Empty;
            // Check for any Authorization
            if ( !IsPersonEditOrAdminAuthorized() )
            {
                if ( !IsPersonViewAuthorized() )
                {
                    errorMessage = $"You are not authorized to view the {FriendlyBlockName}.";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether the current Person is authorized to view the specified content.
        /// </summary>
        /// <returns><see langword="true"/> if the current Person has view authorization; otherwise, <see langword="false"/>.</returns>
        private bool IsPersonViewAuthorized()
        {
            return BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Determines whether the current Person has either edit or administrative authorization for the block.
        /// </summary>
        /// <remarks>This method checks the current Person's permissions against the block's authorization
        /// settings  for the "Edit" and "Administrate" roles.</remarks>
        /// <returns><see langword="true"/> if the current Person is authorized with either edit or administrative permissions; 
        /// otherwise, <see langword="false"/>.</returns>
        private bool IsPersonEditOrAdminAuthorized()
        {
            var currentPerson = RequestContext.CurrentPerson;
            var allowedAuthorizations = new[] { Authorization.EDIT, Authorization.ADMINISTRATE };

            if ( allowedAuthorizations.Any( auth => BlockCache.IsAuthorized( auth, currentPerson ) ) )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to retrieve the relative file path from the specified attributes or page parameters.
        /// </summary>
        /// <remarks>This method first attempts to retrieve the relative file path from the attribute
        /// specified by <c>AttributeKey.RelativeFilePath</c>. If the attribute value is null, empty, or consists only
        /// of white-space characters, it then attempts to retrieve the value from the page parameter specified by
        /// <c>PageParameterKey.RelativeFilePath</c>.</remarks>
        /// <param name="relativeFilePath">When this method returns, contains the relative file path if found; otherwise, an empty string.</param>
        /// <returns><see langword="true"/> if a non-empty relative file path was successfully retrieved; otherwise, <see
        /// langword="false"/>.</returns>
        private bool GetRelativePath( out string relativeFilePath )
        {
            relativeFilePath = GetAttributeValue( AttributeKey.RelativeFilePath );

            if ( string.IsNullOrWhiteSpace( relativeFilePath ) )
            {
                relativeFilePath = PageParameter( PageParameterKey.RelativeFilePath );
            }

            return relativeFilePath.IsNotNullOrWhiteSpace();
        }

        /// <summary>
        /// Determines the appropriate editor mode for a given file extension.
        /// </summary>
        /// <remarks>This method maps common file extensions to predefined editor modes. The returned
        /// editor mode  can be used to configure syntax highlighting or other editor-specific features.</remarks>
        /// <param name="extension">The file extension, including the leading period (e.g., ".html", ".css").</param>
        /// <returns>A string representing the editor mode corresponding to the specified file extension.  If the file extension
        /// is not recognized, the default editor mode for plain text is returned.</returns>
        private string GetEditorModeFromFileExtension( string extension )
        {
            switch ( extension )
            {
                case ".html":
                case ".htm":
                    return EditorMode.Html;
                case ".css":
                    return EditorMode.Css;
                case ".lava":
                    return EditorMode.Lava;
                case ".js":
                    return EditorMode.Javascript;
                case ".less":
                    return EditorMode.Less;
                case ".ps1":
                    return EditorMode.Powershell;
                case ".sql":
                    return EditorMode.Sql;
                case ".ts":
                case ".tsx":
                    return EditorMode.Typescript;
                case ".cs":
                    return EditorMode.CSharp;
                case ".md":
                    return EditorMode.Markdown;
                case ".xml":
                    return EditorMode.Xml;
                case ".json":
                    return EditorMode.Json;
                default:
                    return EditorMode.Text;
            }
        }

        #endregion Methods

        #region Block Actions

        [BlockAction]
        public BlockActionResult Save( FileEditorBag bag )
        {
            if ( !IsPersonEditOrAdminAuthorized() )
            {
                return ActionForbidden( "You do not have permission to modify this block." );
            }

            if ( bag.FileContents == null )
            {
                return ActionBadRequest( "Error while saving the content back to file." );
            }

            // The file path in the bag is posted back by the browser, so run the
            // same checks that init used and write to the path derived from them instead.
            if ( !IsRelativeFilePathGood( out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            // Init sends this same validated path down, so it must match exactly.
            var isSameFileAsInit = string.Equals( bag.FilePath, _filePhysicalPath, StringComparison.OrdinalIgnoreCase );

            if ( !isSameFileAsInit )
            {
                return ActionBadRequest( InvalidPathMessage );
            }

            File.WriteAllText( _filePhysicalPath, bag.FileContents );
            return ActionOk();
        }

        #endregion Block Actions
    }
}