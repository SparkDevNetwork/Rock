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
using System.Data.Entity;
using System.Linq;

using HtmlAgilityPack;

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

    [CustomDropdownListField(
        "Versioning & Approval",
        Description = "Preserves previous versions of the content and, optionally, requires changes to be approved before they display. Approval requires versioning.",
        ListSource = AttributeStrings.VersioningModeListSource,
        IsRequired = true,
        DefaultValue = VersioningMode.Off,
        Category = AttributeCategory.VersioningAndApproval,
        Order = 5,
        Key = AttributeKey.VersioningAndApprovalMode )]

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
            public const string VersioningAndApprovalMode = "VersioningAndApprovalMode";
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

        /// <summary>
        /// The stored values of the Versioning &amp; Approval setting.
        /// </summary>
        private static class VersioningMode
        {
            public const string Off = "Off";
            public const string VersioningOnly = "VersioningOnly";
            public const string VersioningWithApproval = "VersioningWithApproval";
        }

        #endregion Keys

        #region Attribute Strings

        private static class AttributeStrings
        {
            /// <summary>
            /// The choices offered by the Versioning &amp; Approval setting.
            /// </summary>
            public const string VersioningModeListSource = VersioningMode.Off + "^Off,"
                + VersioningMode.VersioningOnly + "^Versioning Only,"
                + VersioningMode.VersioningWithApproval + "^Versioning with Approval";

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

        #region Properties

        /// <summary>
        /// Gets the configured Versioning &amp; Approval mode, one of the
        /// <see cref="VersioningMode"/> values.
        /// </summary>
        private string CurrentVersioningMode => GetAttributeValue( AttributeKey.VersioningAndApprovalMode );

        /// <summary>
        /// Gets a value indicating whether previous versions are preserved.
        /// </summary>
        private bool IsVersioningEnabled => CurrentVersioningMode == VersioningMode.VersioningOnly || IsApprovalRequired;

        /// <summary>
        /// Gets a value indicating whether content changes must be approved
        /// before they display.
        /// </summary>
        private bool IsApprovalRequired => CurrentVersioningMode == VersioningMode.VersioningWithApproval;

        #endregion Properties

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

        #endregion RockBlockType Overrides

        #region Fields

        /// <summary>
        /// The merge fields the editor's picker always offers, in the picker's
        /// "Field^Type|Label" format. Context entities are appended per request.
        /// </summary>
        private static readonly string[] _standardEditorMergeFields = new[]
        {
            "GlobalAttribute",
            "CurrentPerson^Rock.Model.Person|Current Person",
            "Campuses",
            "PageParameter",
            "RockVersion",
            "Date",
            "Time",
            "DayOfWeek"
        };

        #endregion Fields

        #region Methods

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

        /// <summary>
        /// Determines whether the current person is authorized for the given
        /// security action on this block.
        /// </summary>
        /// <returns><see langword="true"/> if authorized; otherwise <see langword="false"/>.</returns>
        private bool IsCurrentPersonAuthorized( string action )
        {
            return BlockCache.IsAuthorized( action, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Determines whether a content row belongs to this block and context,
        /// using the same filter the service applies when reading content.
        /// </summary>
        /// <returns><see langword="true"/> if the row is within scope; otherwise <see langword="false"/>.</returns>
        private bool IsContentInScope( HtmlContent htmlContent, string entityValue )
        {
            return GetScopedContentQuery( entityValue ).Any( c => c.Id == htmlContent.Id );
        }

        /// <summary>
        /// Gets a query of every content row that belongs to this block and
        /// context, using the same filter the service applies when reading
        /// content so shared Context Name rows are included.
        /// </summary>
        /// <returns>The scoped content query.</returns>
        private IQueryable<HtmlContent> GetScopedContentQuery( string entityValue )
        {
            var htmlContentService = new HtmlContentService( RockContext );

            return htmlContentService.AddFilterLogic( htmlContentService.Queryable(), BlockId, entityValue );
        }

        /// <summary>
        /// Gets the content row the editor was loaded from, identified by its
        /// version number within this block's scope.
        /// </summary>
        /// <returns>The matching row, or null when the editor started from a blank block.</returns>
        private HtmlContent GetContentVersion( string entityValue, int version )
        {
            return GetScopedContentQuery( entityValue )
                .Where( c => c.Version == version )
                .OrderByDescending( c => c.ModifiedDateTime )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the version number a newly created row should receive.
        /// </summary>
        /// <returns>One more than the highest existing version, or 1 when versioning is off or nothing exists.</returns>
        private int GetNextVersion( string entityValue )
        {
            if ( !IsVersioningEnabled )
            {
                return 1;
            }

            var maxVersion = GetScopedContentQuery( entityValue ).Max( c => ( int? ) c.Version ) ?? 0;

            return maxVersion + 1;
        }

        /// <summary>
        /// Parses the content with HtmlAgilityPack and returns the reasons for
        /// any mismatched or malformed tags. Lava is neutralized first so its
        /// tags are not reported as HTML errors.
        /// </summary>
        /// <returns>The warning reasons, empty when the markup is clean.</returns>
        private static List<string> GetMarkupWarnings( string content )
        {
            var document = new HtmlDocument();
            document.LoadHtml( content.IsLavaTemplate() ? content.SanitizeLava() : content );

            return document.ParseErrors
                .Select( error => error.Reason )
                .ToList();
        }

        /// <summary>
        /// Applies the WebForms approval rules onto the content's IsApproved
        /// flag and approver fields, extended with the Denied state the
        /// redesigned status toggle and tooltip need.
        /// </summary>
        /// <param name="htmlContent">The row being saved.</param>
        /// <param name="requestedStatus">The status the editor asked for.</param>
        /// <param name="isContentChanged">Whether the content text differs from the loaded version.</param>
        private void ApplyApprovalStatus( HtmlContent htmlContent, HtmlContentApprovalStatus requestedStatus, bool isContentChanged )
        {
            if ( !IsApprovalRequired )
            {
                SetApprovalStatus( htmlContent, HtmlContentApprovalStatus.Approved );
                return;
            }

            if ( IsCurrentPersonAuthorized( Authorization.APPROVE ) )
            {
                SetApprovalStatus( htmlContent, requestedStatus );
                return;
            }

            // As in WebForms, a non-approver only sends the version back for review when the text itself changed.
            if ( isContentChanged )
            {
                SetApprovalStatus( htmlContent, HtmlContentApprovalStatus.PendingApproval );
            }
        }

        /// <summary>
        /// Writes a three-state status onto the entity's columns: approved and
        /// denied both record the current person as the reviewer, pending
        /// records nobody. Pending must clear the reviewer because an unapproved
        /// row that still names one reads back as Denied.
        /// </summary>
        private void SetApprovalStatus( HtmlContent htmlContent, HtmlContentApprovalStatus status )
        {
            htmlContent.IsApproved = status == HtmlContentApprovalStatus.Approved;

            if ( status == HtmlContentApprovalStatus.PendingApproval )
            {
                htmlContent.ApprovedByPersonAliasId = null;
                htmlContent.ApprovedDateTime = null;
                return;
            }

            htmlContent.ApprovedByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;
            htmlContent.ApprovedDateTime = RockDateTime.Now;
        }

        /// <summary>
        /// Derives the three-state approval status from the entity's IsApproved
        /// flag and approver fields. A denied version is unapproved but records
        /// who denied it, while a pending version records nobody.
        /// </summary>
        /// <returns>The derived approval status.</returns>
        private static HtmlContentApprovalStatus GetApprovalStatus( HtmlContent htmlContent )
        {
            if ( htmlContent.IsApproved )
            {
                return HtmlContentApprovalStatus.Approved;
            }

            return htmlContent.ApprovedByPersonAliasId.HasValue
                ? HtmlContentApprovalStatus.Denied
                : HtmlContentApprovalStatus.PendingApproval;
        }

        /// <summary>
        /// Builds the edit bag for a version of the content, or for a brand new
        /// version when no content exists yet.
        /// </summary>
        /// <returns>The bag the editor binds to.</returns>
        private HtmlContentEditBag GetEditBag( HtmlContent htmlContent, int? maxVersion )
        {
            if ( htmlContent == null )
            {
                return new HtmlContentEditBag
                {
                    ApprovalStatus = IsApprovalRequired
                        ? HtmlContentApprovalStatus.PendingApproval
                        : HtmlContentApprovalStatus.Approved
                };
            }

            return new HtmlContentEditBag
            {
                Version = htmlContent.Version,
                MaxVersion = maxVersion,
                Content = htmlContent.Content,
                StartDateTime = htmlContent.StartDateTime?.ToString( "s" ),
                ExpireDateTime = htmlContent.ExpireDateTime?.ToString( "s" ),
                ApprovalStatus = GetApprovalStatus( htmlContent ),
                ApprovedByName = htmlContent.ApprovedByPersonAlias?.Person?.FullName,
                ApprovedDateTime = htmlContent.ApprovedDateTime?.ToString( "s" )
            };
        }

        /// <summary>
        /// Builds the block-derived options that decide which editor features
        /// are shown and how the editor is configured.
        /// </summary>
        /// <returns>The edit options.</returns>
        private HtmlContentEditOptionsBag GetEditOptions()
        {
            return new HtmlContentEditOptionsBag
            {
                IsVersioningEnabled = IsVersioningEnabled,
                IsApprovalRequired = IsApprovalRequired,
                IsCurrentPersonApprover = IsCurrentPersonAuthorized( Authorization.APPROVE ),
                IsCodeEditorDefault = GetAttributeValue( AttributeKey.UseCodeEditor ).AsBoolean(),
                EncryptedDocumentRootFolder = Encryption.EncryptString( GetAttributeValue( AttributeKey.DocumentRootFolder ) ),
                EncryptedImageRootFolder = Encryption.EncryptString( GetAttributeValue( AttributeKey.ImageRootFolder ) ),
                IsUserSpecificRoot = GetAttributeValue( AttributeKey.UserSpecificFolders ).AsBoolean(),
                MergeFields = GetEditorMergeFields()
            };
        }

        /// <summary>
        /// Gets the merge fields offered by the editor's merge field picker: the
        /// block's standard set plus one entry per context entity on the page.
        /// </summary>
        /// <returns>Merge field definitions in the picker's "Field^Type|Label" format.</returns>
        private List<string> GetEditorMergeFields()
        {
            var mergeFields = _standardEditorMergeFields.ToList();

            foreach ( var contextEntityType in RequestContext.GetContextEntityTypes() )
            {
                if ( LavaHelper.IsLavaDataObject( RequestContext.GetContextEntity( contextEntityType ) ) )
                {
                    mergeFields.Add( $"Context.{contextEntityType.Name}^{contextEntityType.FullName}|Current {contextEntityType.Name} (Context)|Context" );
                }
            }

            return mergeFields;
        }

        /// <summary>
        /// Builds the Version History rows for the block's content, newest first.
        /// Content bodies are deliberately left out so a long history stays cheap.
        /// </summary>
        /// <returns>The version rows.</returns>
        private List<HtmlContentVersionBag> GetVersionBags( string entityValue, int? currentVersion )
        {
            var versions = new HtmlContentService( RockContext )
                .GetContent( BlockId, entityValue )
                .ThenByDescending( c => c.ModifiedDateTime )
                .Select( c => new
                {
                    c.Id,
                    c.Version,
                    c.ModifiedDateTime,
                    ModifiedByPerson = c.ModifiedByPersonAlias.Person,
                    c.IsApproved,
                    ApprovedByPerson = c.ApprovedByPersonAlias.Person,
                    c.StartDateTime,
                    c.ExpireDateTime
                } )
                .ToList();

            return versions
                .Select( v => new HtmlContentVersionBag
                {
                    IdKey = IdHasher.Instance.GetHash( v.Id ),
                    Version = v.Version,
                    VersionText = $"Version {v.Version}",
                    ModifiedDateTime = v.ModifiedDateTime?.ToString( "s" ),
                    ModifiedByName = v.ModifiedByPerson?.FullName,
                    IsApproved = v.IsApproved,
                    // A denied version still records who acted on it, but that person is not an approver.
                    ApprovedByName = v.IsApproved ? v.ApprovedByPerson?.FullName : null,
                    StartDateTime = v.StartDateTime?.ToString( "s" ),
                    ExpireDateTime = v.ExpireDateTime?.ToString( "s" ),
                    IsCurrent = v.Version == currentVersion
                } )
                .ToList();
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets everything the Edit HTML modal needs when it opens: the latest
        /// version for the editor, the block-derived options, and the version
        /// history when versioning is enabled.
        /// </summary>
        /// <returns>The edit box, or a forbidden result when the person cannot edit.</returns>
        [BlockAction]
        public BlockActionResult GetEditContent()
        {
            if ( !IsCurrentPersonAuthorized( Authorization.EDIT ) )
            {
                return ActionForbidden( "You are not authorized to edit this content." );
            }

            var entityValue = GetEntityValue();
            var latestVersion = new HtmlContentService( RockContext ).GetLatestVersion( BlockId, entityValue );

            return ActionOk( new HtmlContentEditBox
            {
                Content = GetEditBag( latestVersion, latestVersion?.Version ),
                Options = GetEditOptions(),
                Versions = IsVersioningEnabled
                    ? GetVersionBags( entityValue, latestVersion?.Version )
                    : null
            } );
        }

        /// <summary>
        /// Gets a specific version of the content so the editor can load it,
        /// typically from the Select button in Version History.
        /// </summary>
        /// <returns>The edit bag for that version, or a not-found result when it does not belong to this block.</returns>
        [BlockAction]
        public BlockActionResult GetVersion( string idKey )
        {
            if ( !IsCurrentPersonAuthorized( Authorization.EDIT ) )
            {
                return ActionForbidden( "You are not authorized to edit this content." );
            }

            var entityValue = GetEntityValue();
            var htmlContentService = new HtmlContentService( RockContext );
            var htmlContent = htmlContentService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );

            // Treat another block's version as not found so a guessed key cannot expose foreign content.
            if ( htmlContent == null || !IsContentInScope( htmlContent, entityValue ) )
            {
                return ActionNotFound( "The requested version could not be found." );
            }

            var maxVersion = htmlContentService.GetLatestVersion( BlockId, entityValue )?.Version;

            return ActionOk( GetEditBag( htmlContent, maxVersion ) );
        }

        /// <summary>
        /// Saves the editor's content as a new version or over the loaded one,
        /// applying markup validation and the approval rules.
        /// </summary>
        /// <returns>The save outcome, including any markup warnings that blocked it.</returns>
        [BlockAction]
        public BlockActionResult Save( SaveHtmlContentRequestBag request )
        {
            if ( !IsCurrentPersonAuthorized( Authorization.EDIT ) )
            {
                return ActionForbidden( "You are not authorized to edit this content." );
            }

            if ( request == null )
            {
                return ActionBadRequest( "The content to save is required." );
            }

            var newContent = request.Content ?? string.Empty;
            var isMarkupValidated = GetAttributeValue( AttributeKey.ValidateMarkup ).AsBoolean();

            // Warn once about bad markup; a second save with the warning acknowledged proceeds.
            if ( isMarkupValidated && !request.IsMarkupWarningAcknowledged )
            {
                var markupWarnings = GetMarkupWarnings( newContent );

                if ( markupWarnings.Any() )
                {
                    return ActionOk( new SaveHtmlContentResponseBag
                    {
                        IsSaved = false,
                        MarkupWarnings = markupWarnings
                    } );
                }
            }

            var entityValue = GetEntityValue();
            var htmlContent = GetContentVersion( entityValue, request.Version );

            var isContentChanged = htmlContent == null || htmlContent.Content != newContent;
            var isNewVersionRequired = htmlContent == null
                || ( isContentChanged && IsVersioningEnabled && !request.IsOverwriteCurrentVersion );

            if ( isNewVersionRequired )
            {
                htmlContent = new HtmlContent
                {
                    BlockId = BlockId,
                    EntityValue = entityValue,
                    Version = GetNextVersion( entityValue )
                };

                new HtmlContentService( RockContext ).Add( htmlContent );
            }

            htmlContent.Content = newContent;
            htmlContent.StartDateTime = request.StartDateTime.AsDateTime();
            htmlContent.ExpireDateTime = request.ExpireDateTime.AsDateTime();

            ApplyApprovalStatus( htmlContent, request.ApprovalStatus, isContentChanged );

            RockContext.SaveChanges();
            HtmlContentService.FlushCachedContent( BlockId, entityValue );

            return ActionOk( new SaveHtmlContentResponseBag
            {
                IsSaved = true,

                // Only new content that is being held back from display is worth warning about.
                IsApprovalPending = isContentChanged && !htmlContent.IsApproved
            } );
        }

        #endregion Block Actions

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
