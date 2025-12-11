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
using System.Web.Hosting;

using Rock.Attribute;
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

    #endregion Block Attributes

    // was [Rock.SystemGuid.BlockTypeGuid( "327370A5-3CC0-42AD-B236-F45260F6A1EE" )]
    [Rock.SystemGuid.BlockTypeGuid( "0F1DADBC-6B12-4BAA-A828-FD1AA86AA387" )]
    public partial class FileEditor : RockBlockType
    {

        #region Fields

        private string _fileRelativePath = string.Empty;
        private string _worksurfaceLayoutName = "Full Worksurface";

        #endregion Fields

        #region Properties

        private const string FriendlyBlockName = "File Editor";

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
            var filePath = HostingEnvironment.MapPath( _fileRelativePath );

            var bag = new FileEditorBag
            {
                FilePath = filePath,
                FileContents = File.ReadAllText( filePath ),
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
                EditorMode = GetEditorModeFromFileExtension( Path.GetExtension( _fileRelativePath ) ),
                isWorksurfaceLayout = PageCache.Layout.ToString() == _worksurfaceLayoutName,
            };
            return options;
        }

        /// <summary>
        /// Validates that a relative file path is provided and that the file exists at the specified path.
        /// </summary>
        /// <remarks>This method checks whether a relative file path is provided and whether the file
        /// exists at the resolved path. If either condition is not met, an appropriate error message is assigned to
        /// <paramref name="errorMessage"/>.</remarks>
        /// <param name="errorMessage">When the method completes, contains an error message if the validation fails; otherwise, remains unchanged.</param>
        private bool IsRelativeFilePathGood( out string errorMessage )
        {
            errorMessage = string.Empty;

            // Check that we have a relative file path
            if ( !GetRelativePath( out string relativeFilePath ) )
            {
                errorMessage = $"A relative file path was not provided to the {FriendlyBlockName}.";
                return false;
            }

            // Check that the file exists
            var fileUrl = HostingEnvironment.MapPath( relativeFilePath );
            if ( !File.Exists( fileUrl ) )
            {
                errorMessage = $"Invalid relative file path.";
                return false;
            }

            // Check that the file extension is not restricted
            if ( RestrictedFileExtension.Any( a =>
                Path.GetExtension( relativeFilePath )
                .Equals( a, StringComparison.OrdinalIgnoreCase )
            ) )
            {
                errorMessage = $"Viewing/Editing of this file type is not allowed.";
                return false;
            }

            _fileRelativePath = relativeFilePath;

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

            File.WriteAllText( bag.FilePath, bag.FileContents );
            return ActionOk();
        }

        #endregion Block Actions
    }
}