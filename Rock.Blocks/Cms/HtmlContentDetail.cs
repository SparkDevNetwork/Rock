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

using System.Collections.Generic;
using System.ComponentModel;

using Rock.Attribute;
using Rock.Enums.Cms;
using Rock.Security;
using Rock.ViewModels.Blocks.Cms.HtmlContentDetail;
using Rock.ViewModels.Cms;
using Rock.Web.UI;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Adds an editable HTML fragment to the page.
    /// </summary>
    [DisplayName( "HTML Content" )]
    [Category( "CMS" )]
    [Description( "Adds an editable HTML fragment to the page." )]
    [IconCssClass( "ti ti-code" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ConfigurationChangedReload( BlockReloadMode.Block )]

    #region Block Attributes

    [SecurityAction(
        Authorization.EDIT,
        "The roles and/or users that can edit the HTML content." )]

    [SecurityAction(
        Authorization.APPROVE,
        "The roles and/or users that have access to approve HTML content." )]

    [BooleanField(
        "Code Editor by Default",
        Description = "Opens the editor in code view instead of the visual editor.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.Editor,
        Order = 0,
        Key = AttributeKey.UseCodeEditor )]

    [TextField(
        "Document Root Folder",
        Description = "The root folder used when browsing or uploading documents.",
        IsRequired = false,
        DefaultValue = "~/Content",
        Category = AttributeCategory.Editor,
        Order = 1,
        Key = AttributeKey.DocumentRootFolder )]

    [TextField(
        "Image Root Folder",
        Description = "The root folder used when browsing or uploading images.",
        IsRequired = false,
        DefaultValue = "~/Content",
        Category = AttributeCategory.Editor,
        Order = 2,
        Key = AttributeKey.ImageRootFolder )]

    [BooleanField(
        "User Specific Folders",
        Description = "Whether the document and image root folders are scoped to the current user.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.Editor,
        Order = 3,
        Key = AttributeKey.UserSpecificFolders )]

    [BooleanField(
        "Validate Markup",
        Description = "Validates the HTML markup for mismatched tags before saving.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.Editor,
        Order = 4,
        Key = AttributeKey.ValidateMarkup )]

    [BooleanField(
        "Enable Versioning",
        Description = "Preserves previous versions of the content. Required for approval to be enabled.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.VersioningAndApproval,
        Order = 5,
        Key = AttributeKey.SupportVersions )]

    [BooleanField(
        "Require Approval",
        Description = "Whether content changes must be approved before they display. Requires versioning to be enabled.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.VersioningAndApproval,
        Order = 6,
        Key = AttributeKey.RequireApproval )]

    [TextField(
        "Context Parameter",
        Description = "The query string parameter used to personalize content for a specific value.",
        IsRequired = false,
        Category = AttributeCategory.Personalization,
        Order = 7,
        Key = AttributeKey.ContextParameter )]

    [TextField(
        "Context Name",
        Description = "A name that further scopes personalized content. Blocks sharing the same name and context parameter share the same values.",
        IsRequired = false,
        Category = AttributeCategory.Personalization,
        Order = 8,
        Key = AttributeKey.ContextName )]

    [LavaCommandsField(
        "Enabled Lava Commands",
        Description = "The Lava commands enabled for this block's content.",
        IsRequired = false,
        Category = AttributeCategory.Behavior,
        Order = 9,
        Key = AttributeKey.EnabledLavaCommands )]

    [IntegerField(
        "Cache Duration",
        Description = "The number of seconds to cache the rendered content. Leave at 0 to disable caching.",
        IsRequired = false,
        DefaultIntegerValue = 0,
        Category = AttributeCategory.Behavior,
        Order = 10,
        Key = AttributeKey.CacheDuration )]

    [CustomCheckboxListField(
        "Cache Tags",
        Description = "Tags that group this block's cached content with other cached content so it can be expired together.",
        ListSource = AttributeStrings.CacheTagListSource,
        IsRequired = false,
        Category = AttributeCategory.Behavior,
        Order = 11,
        Key = AttributeKey.CacheTags )]

    [ContextAware]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "37985D9E-A685-4110-8AB8-AB166DC9C33E" )]
    // was [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.HTML_CONTENT )]
    [Rock.SystemGuid.BlockTypeGuid( "17E49D62-95F5-43AD-99E9-9366995D56A2" )]
    public class HtmlContentDetail : RockBlockType, IHasCustomActions
    {
        #region Keys

        private static class AttributeKey
        {
            public const string EnabledLavaCommands = "EnabledLavaCommands";
            public const string UseCodeEditor = "UseCodeEditor";
            public const string DocumentRootFolder = "DocumentRootFolder";
            public const string ImageRootFolder = "ImageRootFolder";
            public const string UserSpecificFolders = "UserSpecificFolders";
            public const string CacheDuration = "CacheDuration";
            public const string ContextParameter = "ContextParameter";
            public const string ContextName = "ContextName";
            public const string SupportVersions = "SupportVersions";
            public const string RequireApproval = "RequireApproval";
            public const string CacheTags = "CacheTags";
            public const string ValidateMarkup = "ValidateMarkup";
        }

        private static class AttributeCategory
        {
            public const string Editor = "Editor";
            public const string VersioningAndApproval = "Versioning & Approval";
            public const string Personalization = "Personalization";
            public const string Behavior = "Behavior";
        }

        #endregion Keys

        #region Attribute Strings

        private static class AttributeStrings
        {
            /// <summary>
            /// Supplies the Cache Tags setting with the values of the Cache Tags defined type.
            /// </summary>
            public const string CacheTagListSource = @"
                SELECT CAST( [dv].[Value] AS VARCHAR ) AS [Value], [dv].[Value] AS [Text]
                FROM [DefinedType] AS [dt]
                INNER JOIN [DefinedValue] AS [dv] ON [dv].[DefinedTypeId] = [dt].[Id]
                WHERE [dt].[Guid] = 'BDF73089-9154-40C1-90E4-74518E9937DC'";
        }

        #endregion Attribute Strings

        #region RockBlockType Overrides

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            return string.Empty;
        }

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new HtmlContentDetailOptionsBag();
        }

        #endregion RockBlockType Overrides

        #region IHasCustomActions Implementation

        /// <inheritdoc/>
        List<BlockCustomActionBag> IHasCustomActions.GetCustomActions( bool canEdit, bool canAdministrate )
        {
            var actions = new List<BlockCustomActionBag>();

            if ( canEdit )
            {
                actions.Add( new BlockCustomActionBag
                {
                    IconCssClass = "ti ti-edit",
                    Tooltip = "Edit HTML",
                    ComponentFileUrl = "/Obsidian/Blocks/Cms/HtmlContentDetail/htmlContentDetailEdit.obs"
                } );
            }

            return actions;
        }

        #endregion IHasCustomActions Implementation
    }
}
