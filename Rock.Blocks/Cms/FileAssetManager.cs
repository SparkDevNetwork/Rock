﻿// <copyright>
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

using System.ComponentModel;

using Rock.Attribute;
using Rock.Model;
using Rock.Security.SecurityGrantRules;
using Rock.ViewModels.Blocks.Cms.AssetManager;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Manage files stored on a remote server or 3rd party cloud storage
    /// </summary>

    [DisplayName( "File Asset Manager" )]
    [Category( "CMS" )]
    [Description( "Browse and manage files on the web server or stored on a remote server or 3rd party cloud storage" )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [BooleanField(
        "Enable Asset Storage Providers",
        Key = AttributeKey.EnableAssetProviders,
        Description = "Set this to true to enable showing folders and files from your configured asset storage providers.",
        DefaultBooleanValue = false,
        Order = 0
    )]

    [BooleanField(
        "Enable File Manager",
        Key = AttributeKey.EnableFileManager,
        Description = "Set this to true to enable showing folders and files your server's local file system.",
        DefaultBooleanValue = true,
        Order = 1
    )]

    [BooleanField(
        "Use Static Height",
        Key = AttributeKey.IsStaticHeight,
        Description = "Set this to true to be able to set a CSS height value dictating how tall the block will be. Otherwise, it will grow with the content.",
        DefaultBooleanValue = true,
        Order = 2
    )]

    [TextField(
        "Height",
        Key = AttributeKey.Height,
        Description = "If you've selected Yes for \"Use Static Height\", this will be the CSS length value that dictates how tall the block will be.",
        IsRequired = false,
        DefaultValue = "400px",
        Order = 3
    )]

    #region File Manager Options

    [TextField(
        "Root Folder",
        Key = AttributeKey.RootFolder,
        Description = "The root file manager folder to browse",
        IsRequired = true,
        DefaultValue = "~/Content",
        Category = "File Manager Options",
        Order = 4
    )]

    [CustomDropdownListField(
        "Browse Mode",
        Key = AttributeKey.BrowseMode,
        Description = "Select 'image' to show only image files. Select 'doc' to show all files.",
        ListSource = "doc,image",
        IsRequired = true,
        DefaultValue = "doc",
        Category = "File Manager Options",
        Order = 5
    )]

    [LinkedPage(
        "File Editor Page",
        Key = AttributeKey.FileEditorPage,
        Description = "Page used to edit the contents of a file.",
        IsRequired = false,
        Category = "File Manager Options",
        Order = 6
    )]

    [BooleanField(
        "Enable Zip Upload",
        Key = AttributeKey.ZipUploaderEnabled,
        Description = "Set this to true to enable the Zip File uploader.",
        DefaultBooleanValue = false,
        Category = "File Manager Options",
        Order = 7
    )]

    #endregion

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "e357ad54-1725-48b8-997c-23c2587800fb" )]
    [Rock.SystemGuid.BlockTypeGuid( "535500a7-967f-4da3-8fca-cb844203cb3d" )]
    public class FileAssetManager : RockBlockType
    {
        #region Keys

        private static class PreferenceKey
        {
            public const string OpenFolders = "open-folders";
            public const string SelectedFolder = "selected-folder";
        }

        private static class AttributeKey
        {
            public const string EnableAssetProviders = "EnableAssetProviders";
            public const string EnableFileManager = "EnableFileManager";
            public const string IsStaticHeight = "IsStaticHeight";
            public const string Height = "Height";
            public const string RootFolder = "RootFolder";
            public const string BrowseMode = "BrowseMode";
            public const string FileEditorPage = "FileEditorPage";
            public const string ZipUploaderEnabled = "ZipUploaderEnabled";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new AssetManagerOptionsBag
            {
                Title = BlockCache.Name,
                EnableAssetProviders = GetAttributeValue( AttributeKey.EnableAssetProviders ).AsBoolean(),
                EnableFileManager = GetAttributeValue( AttributeKey.EnableFileManager ).AsBoolean(),
                IsStaticHeight = GetAttributeValue( AttributeKey.IsStaticHeight ).AsBoolean(),
                Height = GetAttributeValue( AttributeKey.Height ),
                RootFolder = Rock.Security.Encryption.EncryptString( GetAttributeValue( AttributeKey.RootFolder ) ),
                BrowseMode = GetAttributeValue( AttributeKey.BrowseMode ),
                FileEditorPage = this.GetLinkedPageUrl( AttributeKey.FileEditorPage ),
                EnableZipUploader = GetAttributeValue( AttributeKey.ZipUploaderEnabled ).AsBoolean(),
                SecurityGrantToken = GetSecurityGrantToken()
            };

            return box;
        }

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            return GetSecurityGrantToken();
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken()
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            securityGrant.AddRule( new AssetAndFileManagerSecurityGrantRule( Rock.Security.Authorization.VIEW ) );
            securityGrant.AddRule( new AssetAndFileManagerSecurityGrantRule( Rock.Security.Authorization.EDIT ) );
            securityGrant.AddRule( new AssetAndFileManagerSecurityGrantRule( Rock.Security.Authorization.DELETE ) );

            return securityGrant.ToToken();
        }

        #endregion
    }
}
