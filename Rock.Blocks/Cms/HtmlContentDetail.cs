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

using Rock;
using Rock.Attribute;
using Rock.Enums.Cms;
using Rock.Lava;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.Utility;
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
            try
            {
                return GetContentHtml();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );

                return $@"<div class='alert alert-danger'>
                    <strong>HTML Content Error</strong><br/>
                    {ex.Message.EncodeHtml()}
                </div>";
            }
        }

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new HtmlContentDetailOptionsBag();
        }

        #endregion RockBlockType Overrides

        #region Fields

        private RenderedContent _renderedContent;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Renders the content once per request and captures any failure so the
        /// error can be reported through the options bag instead of the HTML.
        /// </summary>
        /// <returns>The rendered HTML or the error message.</returns>
        private RenderedContent GetRenderedContent()
        {
            // The framework builds the options bag before it asks for the initial HTML, so both read from this one result.
            if ( _renderedContent != null )
            {
                return _renderedContent;
            }

            try
            {
                _renderedContent = new RenderedContent { Html = GetContentHtml() };
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                _renderedContent = new RenderedContent { ErrorMessage = ex.Message };
            }

            return _renderedContent;
        }

        /// <summary>
        /// Gets the HTML to display for the current request, serving it from
        /// the content cache when a cache duration is configured.
        /// </summary>
        /// <returns>The rendered HTML, or an empty string when no content is active.</returns>
        private string GetContentHtml()
        {
            var entityValue = GetEntityValue();
            var cacheDuration = GetAttributeValue( AttributeKey.CacheDuration ).AsInteger();
            var isCachingEnabled = cacheDuration > 0;

            if ( isCachingEnabled )
            {
                var cachedHtml = HtmlContentService.GetCachedContent( BlockId, entityValue );

                if ( cachedHtml != null )
                {
                    return cachedHtml;
                }
            }

            var html = RenderActiveContent( entityValue );

            if ( isCachingEnabled )
            {
                var cacheTags = GetAttributeValue( AttributeKey.CacheTags ) ?? string.Empty;
                HtmlContentService.AddCachedContent( BlockId, entityValue, html, cacheDuration, cacheTags );
            }

            return html;
        }

        /// <summary>
        /// Loads the active content for the entity value, resolves any Lava,
        /// and expands Rock's relative URL tokens.
        /// </summary>
        /// <param name="entityValue">The entity value that scopes the content.</param>
        /// <returns>The rendered HTML, or an empty string when no content is active.</returns>
        private string RenderActiveContent( string entityValue )
        {
            var content = new HtmlContentService( RockContext ).GetActiveContentHtml( BlockId, entityValue );

            if ( content == null )
            {
                return string.Empty;
            }

            var html = content.IsLavaTemplate()
                ? content.ResolveMergeFields( GetContentMergeFields(), GetAttributeValue( AttributeKey.EnabledLavaCommands ) )
                : content;

            return ResolveRockUrlTokens( html );
        }

        /// <summary>
        /// Replaces the "~~/" theme and "~/" application URL tokens. The theme
        /// token is replaced first so the application token does not consume it.
        /// </summary>
        /// <param name="html">The HTML containing URL tokens.</param>
        /// <returns>The HTML with absolute URLs.</returns>
        private string ResolveRockUrlTokens( string html )
        {
            var themeRoot = RequestContext.ResolveRockUrl( "~~/" );
            var appRoot = RequestContext.ResolveRockUrl( "~/" );

            return html.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );
        }

        /// <summary>
        /// Builds the merge fields available to the content's Lava. This is the
        /// common set plus the fields the HTML Content block has always exposed.
        /// </summary>
        /// <returns>The merge fields for rendering the content.</returns>
        private Dictionary<string, object> GetContentMergeFields()
        {
            var currentPerson = RequestContext.CurrentPerson;
            var mergeFields = RequestContext.GetCommonMergeFields();

            mergeFields.Add( "CurrentPage", PageCache );
            mergeFields.Add( "CurrentVisitor", GetCurrentVisitor() );
            mergeFields.Add( "CurrentBrowser", new CurrentBrowserMergeField( RequestContext.ClientInformation?.BrowserInfo ) );
            mergeFields.Add( "RockVersion", VersionInfo.VersionInfo.GetRockProductVersionNumber() );
            mergeFields.Add( "CurrentPersonCanEdit", BlockCache.IsAuthorized( Authorization.EDIT, currentPerson ) );
            mergeFields.Add( "CurrentPersonCanAdministrate", BlockCache.IsAuthorized( Authorization.ADMINISTRATE, currentPerson ) );

            // "Person" is a legacy alias kept for existing templates; new templates should use "CurrentPerson".
            if ( currentPerson != null )
            {
                mergeFields.TryAdd( "Person", currentPerson );
            }

            return mergeFields;
        }

        /// <summary>
        /// Gets the person alias representing the current visitor, preferring
        /// the anonymous visitor alias and falling back to the signed-in person.
        /// </summary>
        /// <returns>The visitor's person alias, or null when there is none.</returns>
        private PersonAlias GetCurrentVisitor()
        {
            var visitorAliasId = RequestContext.CurrentVisitorId ?? RequestContext.CurrentPerson?.PrimaryAliasId;

            if ( !visitorAliasId.HasValue )
            {
                return null;
            }

            return new PersonAliasService( RockContext ).Get( visitorAliasId.Value );
        }

        /// <summary>
        /// Builds the entity value that scopes this block's content. It combines
        /// the Context Parameter (a page parameter or context entity Id) and the
        /// Context Name, and is empty when neither setting is configured.
        /// </summary>
        /// <returns>The entity value used to look up HtmlContent rows.</returns>
        private string GetEntityValue()
        {
            var contextParameter = GetAttributeValue( AttributeKey.ContextParameter );
            var contextName = GetAttributeValue( AttributeKey.ContextName );
            var entityValue = string.Empty;

            if ( contextParameter.IsNotNullOrWhiteSpace() )
            {
                entityValue = $"{contextParameter}={GetContextParameterValue( contextParameter )}";
            }

            // The leading "&" when Context Name stands alone is intentional; the service matches on the literal "&ContextName=".
            if ( contextName.IsNotNullOrWhiteSpace() )
            {
                entityValue += $"&ContextName={contextName}";
            }

            return entityValue;
        }

        /// <summary>
        /// Gets the value for the configured Context Parameter, preferring the
        /// page parameter and falling back to the page's context entity Id.
        /// </summary>
        /// <param name="contextParameter">The Context Parameter block setting.</param>
        /// <returns>The parameter value, or an empty string when neither source has one.</returns>
        private string GetContextParameterValue( string contextParameter )
        {
            var pageParameterValue = PageParameter( contextParameter );

            if ( pageParameterValue.IsNotNullOrWhiteSpace() )
            {
                return pageParameterValue;
            }

            return GetContextEntity()?.Id.ToString() ?? string.Empty;
        }

        #endregion Methods

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

        #region Support Classes

        /// <summary>
        /// The outcome of rendering the block's content for the current request.
        /// </summary>
        private sealed class RenderedContent
        {
            /// <summary>
            /// The rendered HTML, or an empty string when rendering failed.
            /// </summary>
            public string Html { get; set; } = string.Empty;

            /// <summary>
            /// The error message when rendering failed, otherwise null.
            /// </summary>
            public string ErrorMessage { get; set; }
        }

        #endregion Support Classes

        #region CurrentBrowser Merge Field Wrapper

        /*
            9/9/2026 - CLAUDE

            These four classes are ported verbatim from the WebForms HtmlContentDetail block.
            Their shape is an end-user Lava contract (CurrentBrowser.IsMobile,
            CurrentBrowser.BrowserInfo.OS.Family, and so on) defined by the User Agent Helper
            spec (specs/completed/core/260506-rock-user-agent-helper.md, Phase 5 step 23).
            They intentionally keep RockDynamic instead of LavaDataObject so existing
            templates see exactly the same values, including the empty-string rendering of
            missing version segments.

            Reason: Preserve the CurrentBrowser Lava merge field shape exactly.
        */

        /// <summary>
        /// Lava-safe wrapper exposing the legacy <c>BrowserClient</c> shape
        /// for the <c>CurrentBrowser</c> merge field. Built on top of the
        /// public <see cref="UserAgentInfo"/> surface so it survives removal
        /// of the obsolete deprecation-window holdover.
        /// </summary>
        [LavaType]
        private sealed class CurrentBrowserMergeField : RockDynamic
        {
            private readonly UserAgentInfo _userAgentInfo;
            private CurrentBrowserInfo _browserInfo;

            public string ClientType => _userAgentInfo?.ClientType;

            public bool IsMobile => _userAgentInfo?.ClientType == "Mobile";

            public CurrentBrowserInfo BrowserInfo
            {
                get
                {
                    // Built lazily because BrowserInfo is almost never accessed from templates.
                    if ( _browserInfo == null && _userAgentInfo != null )
                    {
                        _browserInfo = new CurrentBrowserInfo( _userAgentInfo );
                    }

                    return _browserInfo;
                }
            }

            public CurrentBrowserMergeField( UserAgentInfo info )
            {
                _userAgentInfo = info;
            }
        }

        /// <summary>
        /// Lava-safe wrapper for <c>BrowserClient.BrowserInfo</c>.
        /// </summary>
        [LavaType]
        private sealed class CurrentBrowserInfo : RockDynamic
        {
            private readonly UserAgentInfo _userAgentInfo;

            public string String => _userAgentInfo?.ToString();

            public CurrentBrowserVersion OS { get; }

            public CurrentBrowserDevice Device { get; }

            public CurrentBrowserVersion UserAgent { get; }

            public CurrentBrowserInfo( UserAgentInfo info )
            {
                _userAgentInfo = info;

                OS = new CurrentBrowserVersion( info.OSFamily, info.OSVersion, info.GetOSFamilyVersion() );
                Device = new CurrentBrowserDevice( info );
                UserAgent = new CurrentBrowserVersion( info.BrowserFamily, info.BrowserVersion, info.GetBrowserFamilyVersion() );
            }

            public override string ToString() => _userAgentInfo?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Lava-safe wrapper for <c>BrowserClient.BrowserInfo.OS</c>. Renders
        /// non-numeric version segments as an empty string (see spec).
        /// </summary>
        [LavaType]
        private sealed class CurrentBrowserVersion : RockDynamic
        {
            private readonly UserAgentVersion _userAgentVersion;

            private readonly string _description;

            public string Family { get; }

            public string Major => _userAgentVersion?.Major?.ToString() ?? string.Empty;

            public string Minor => _userAgentVersion?.Minor?.ToString() ?? string.Empty;

            public string Patch => _userAgentVersion?.Patch?.ToString() ?? string.Empty;

            public string PatchMinor => _userAgentVersion?.PatchMinor?.ToString() ?? string.Empty;

            public CurrentBrowserVersion( string family, UserAgentVersion version, string description )
            {
                Family = family ?? string.Empty;
                _userAgentVersion = version;
                _description = description ?? string.Empty;
            }

            public override string ToString() => _description;
        }

        /// <summary>
        /// Lava-safe wrapper for <c>BrowserClient.BrowserInfo.Device</c>.
        /// </summary>
        [LavaType]
        private sealed class CurrentBrowserDevice : RockDynamic
        {
            private readonly UserAgentInfo _userAgentInfo;

            public string Family => _userAgentInfo?.DeviceFamily ?? string.Empty;

            public string Brand => _userAgentInfo?.DeviceBrand ?? string.Empty;

            public string Model => _userAgentInfo?.DeviceModel ?? string.Empty;

            public CurrentBrowserDevice( UserAgentInfo info )
            {
                _userAgentInfo = info;
            }

            public override string ToString() => $"{Brand} {Family} {Model}";
        }

        #endregion CurrentBrowser Merge Field Wrapper
    }
}
