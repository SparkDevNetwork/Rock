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

using Rock.Attribute;
using Rock.Cms.Utm;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.Cms;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.Transactions;
using Rock.Utility;
using Rock.Utility.ExtensionMethods;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.ContentChannelItemView;
using Rock.ViewModels.Cms;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;
using Rock.Web.UI;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Block that displays a specific content channel item using a configurable Lava template.
    /// </summary>
    [DisplayName( "Content Channel Item View" )]
    [Category( "CMS" )]
    [Description( "Block to display a specific content channel item." )]
    [IconCssClass( "ti ti-article" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ConfigurationChangedReload( BlockReloadMode.Block )]

    #region Block Attributes

    [LavaCommandsField(
        "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this content channel item block.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.EnabledLavaCommands )]

    [ContentChannelField(
        "Content Channel",
        Description = "Limits content channel items to a specific channel. In most cases you'll want to provide a Content Channel to limit which channel is shown — especially if you're using non-globally unique slugs.",
        IsRequired = false,
        DefaultValue = "",
        Category = "CustomSetting",
        Key = AttributeKey.ContentChannel )]

    [EnumsField(
        "Status",
        Description = "Include items with the following status.",
        EnumSourceType = typeof( ContentChannelItemStatus ),
        IsRequired = false,
        DefaultValue = "2",
        Category = "CustomSetting",
        Key = AttributeKey.Status )]

    [TextField(
        "Content Channel Query Parameter",
        Description = ContentChannelQueryParameterDescription,
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.ContentChannelQueryParameter )]

    [CodeEditorField(
        "Lava Template",
        Description = "The template to use when formatting the content channel item.",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 200,
        IsRequired = false,
        Category = "CustomSetting",
        DefaultValue = LavaTemplateDefaultValue,
        Key = AttributeKey.LavaTemplate )]

    [IntegerField(
        "Output Cache Duration",
        Description = OutputCacheDurationDescription,
        IsRequired = false,
        Key = AttributeKey.OutputCacheDuration,
        Category = "CustomSetting" )]

    [IntegerField(
        "Item Cache Duration",
        Description = "Number of seconds to cache the content item specified by the parameter.",
        IsRequired = false,
        DefaultIntegerValue = 3600,
        Category = "CustomSetting",
        Key = AttributeKey.ItemCacheDuration )]

    [CustomCheckboxListField(
        "Cache Tags",
        Description = "Cached tags are used to link cached content so that it can be expired as a group",
        IsRequired = false,
        Key = AttributeKey.CacheTags,
        Category = "CustomSetting" )]

    [BooleanField(
        "Merge Content",
        Description = "Should the content data and attribute values be merged using the Lava template engine?",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Key = AttributeKey.MergeContent )]

    [BooleanField(
        "Set Page Title",
        Description = "Determines if the block should set the page title with the channel name or content item. This will also add a breadcrumb with the same name.",
        Category = "CustomSetting",
        Key = AttributeKey.SetPageTitle )]

    [LinkedPage(
        "Detail Page",
        Description = "Page used to view a content item.",
        Order = 1,
        Category = "CustomSetting",
        Key = AttributeKey.DetailPage )]

    [BooleanField(
        "Display Most Recent",
        Description = "Should the most recent item for the configured Content Channel be displayed if no query parameter value is provided?",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.DisplayMostRecent )]

    [BooleanField(
        "Log Interactions",
        Category = "CustomSetting",
        Key = AttributeKey.LogInteractions )]

    [BooleanField(
        "Write Interaction Only If Individual Logged In",
        Description = "Set to true to only write interactions for logged in users, or set to false to write interactions for both logged in and anonymous users.",
        Category = "CustomSetting",
        Key = AttributeKey.WriteInteractionOnlyIfIndividualLoggedIn )]

    [WorkflowTypeField(
        "Workflow Type",
        Description = "The workflow type to launch when the content is viewed.",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.WorkflowType )]

    [BooleanField(
        "Launch Workflow Only If Individual Logged In",
        Description = "Set to true to only launch a workflow for logged in users, or set to false to launch for both logged in and anonymous users.",
        Category = "CustomSetting",
        Key = AttributeKey.LaunchWorkflowOnlyIfIndividualLoggedIn )]

    [EnumField(
        "Launch Workflow Condition",
        EnumSourceType = typeof( LaunchWorkflowCondition ),
        DefaultValue = "1",
        Category = "CustomSetting",
        Key = AttributeKey.LaunchWorkflowCondition )]

    [TextField(
        "Meta Description Attribute",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.MetaDescriptionAttribute )]

    [TextField(
        "Open Graph Type",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.OpenGraphType )]

    [TextField(
        "Open Graph Title Attribute",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.OpenGraphTitleAttribute )]

    [TextField(
        "Open Graph Description Attribute",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.OpenGraphDescriptionAttribute )]

    [TextField(
        "Open Graph Image Attribute",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.OpenGraphImageAttribute )]

    [TextField(
        "Twitter Title Attribute",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.TwitterTitleAttribute )]

    [TextField(
        "Twitter Description Attribute",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.TwitterDescriptionAttribute )]

    [TextField(
        "Twitter Image Attribute",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.TwitterImageAttribute )]

    [TextField(
        "Twitter Card",
        IsRequired = false,
        DefaultValue = "none",
        Category = "CustomSetting",
        Key = AttributeKey.TwitterCard )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "F4484867-C759-4C68-9C76-86CBA3DE4FB4" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "3EF97F63-09E4-418C-B65D-729D975BC223" )]
    [Rock.SystemGuid.BlockTypeGuid( "63659EBE-C5AF-4157-804A-55C7D565110E" )]
    public class ContentChannelItemView : RockBlockType, IHasCustomActions, IBreadCrumbBlock
    {
        #region Keys and Constants

        private static class AttributeKey
        {
            public const string ContentChannel = "ContentChannel";
            public const string Status = "Status";
            public const string ContentChannelQueryParameter = "ContentChannelQueryParameter";
            public const string LavaTemplate = "LavaTemplate";
            public const string OutputCacheDuration = "OutputCacheDuration";
            public const string ItemCacheDuration = "ItemCacheDuration";
            public const string CacheTags = "CacheTags";
            public const string MergeContent = "MergeContent";
            public const string SetPageTitle = "SetPageTitle";
            public const string DetailPage = "DetailPage";
            public const string LogInteractions = "LogInteractions";
            public const string WriteInteractionOnlyIfIndividualLoggedIn = "WriteInteractionOnlyIfIndividualLoggedIn";
            public const string WorkflowType = "WorkflowType";
            public const string LaunchWorkflowCondition = "LaunchWorkflowCondition";
            public const string LaunchWorkflowOnlyIfIndividualLoggedIn = "LaunchWorkflowOnlyIfIndividualLoggedIn";
            public const string MetaDescriptionAttribute = "MetaDescriptionAttribute";
            public const string OpenGraphType = "OpenGraphType";
            public const string OpenGraphTitleAttribute = "OpenGraphTitleAttribute";
            public const string OpenGraphDescriptionAttribute = "OpenGraphDescriptionAttribute";
            public const string OpenGraphImageAttribute = "OpenGraphImageAttribute";
            public const string TwitterTitleAttribute = "TwitterTitleAttribute";
            public const string TwitterDescriptionAttribute = "TwitterDescriptionAttribute";
            public const string TwitterImageAttribute = "TwitterImageAttribute";
            public const string TwitterCard = "TwitterCard";
            public const string EnabledLavaCommands = "EnabledLavaCommands";
            public const string DisplayMostRecent = "DisplayMostRecent";
        }

        private static class PageParameterKey
        {
            public const string PageId = "PageId";
        }

        private static class CacheKey
        {
            public const string CacheKeys = "CacheKeys";
        }

        protected const string LavaTemplateDefaultValue = @"<h1>{{ Item.Title }}</h1>
{{ Item.Content }}";

        private const string ContentChannelQueryParameterDescription = @"Specify the URL parameter to use to determine which Content Channel Item to show, or leave blank to use whatever the first parameter is. The type of the value will determine how the content channel item will be determined as follows:

Integer - ContentChannelItem Id
String - ContentChannelItem Slug
Guid - ContentChannelItem Guid";

        private const string OutputCacheDurationDescription = @"Number of seconds to cache the resolved output. Only cache the output if you are not personalizing the output based on current user, current page, or any other merge field value.";

        /// <summary>
        /// Bounds how long a page-render's interaction token remains usable. Long enough for a
        /// tab left open, short enough to limit replay if a token is captured.
        /// </summary>
        private static readonly TimeSpan InteractionTokenLifetime = TimeSpan.FromHours( 4 );

        private enum LaunchWorkflowCondition
        {
            Always = 0,
            OncePerPersonPerContentChannelItem = 1,
            OncePerPerson = 2
        }

        /// <summary>
        /// The payload encrypted into the interaction token. Lets the registration endpoint
        /// trust the item Id and interaction Guid without re-resolving from page parameters.
        /// </summary>
        private class InteractionTokenPayload
        {
            /// <summary>
            /// Becomes <see cref="Interaction.Guid"/>. Stable across browser-back navigation
            /// so the client can sessionStorage-dedupe.
            /// </summary>
            public Guid Guid { get; set; }

            public int ItemId { get; set; }

            public DateTime ExpiresAt { get; set; }
        }

        #endregion Keys and Constants

        #region RockBlockType Overrides

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            try
            {
                return RenderContent();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );

                return $@"<div class='alert alert-danger'>
    <strong>Content Channel Item View Error</strong><br/>
    {ex.Message.EncodeHtml()}
</div>";
            }
        }

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return BuildInitializationBag();
        }

        #endregion RockBlockType Overrides

        #region Block Actions

        /// <summary>Initial load of the custom settings modal.</summary>
        [BlockAction]
        public BlockActionResult GetCustomSettings()
        {
            return GetSettingsForContentChannel( GetAttributeValue( AttributeKey.ContentChannel ).AsGuidOrNull() );
        }

        /// <summary>
        /// Re-loads the option lists for a different content channel (called when the user changes
        /// the channel dropdown), without persisting anything.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetCustomSettingsForContentChannel( Guid contentChannelGuid )
        {
            return GetSettingsForContentChannel( contentChannelGuid );
        }

        /// <summary>Persists the custom settings modal's values.</summary>
        [BlockAction]
        public BlockActionResult SaveCustomSettings( CustomSettingsBox<ContentChannelItemViewCustomSettingsBag, ContentChannelItemViewCustomSettingsOptionsBag> box )
        {
            if ( !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to edit block settings." );
            }

            var block = new BlockService( RockContext ).Get( BlockId );
            block.LoadAttributes( RockContext );

            box.IfValidProperty( nameof( box.Settings.ContentChannelGuid ),
                () => block.SetAttributeValue( AttributeKey.ContentChannel, box.Settings.ContentChannelGuid.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.ContentChannelItemStatuses ),
                () => block.SetAttributeValue( AttributeKey.Status, box.Settings.ContentChannelItemStatuses.Select( s => ( ( int ) s ).ToString() ).ToList().AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Settings.LavaTemplate ),
                () => block.SetAttributeValue( AttributeKey.LavaTemplate, box.Settings.LavaTemplate ) );

            box.IfValidProperty( nameof( box.Settings.ContentChannelQueryParameter ),
                () => block.SetAttributeValue( AttributeKey.ContentChannelQueryParameter, box.Settings.ContentChannelQueryParameter ) );

            box.IfValidProperty( nameof( box.Settings.IsDisplayMostRecentEnabled ),
                () => block.SetAttributeValue( AttributeKey.DisplayMostRecent, box.Settings.IsDisplayMostRecentEnabled.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.IsPageTitleUpdateEnabled ),
                () => block.SetAttributeValue( AttributeKey.SetPageTitle, box.Settings.IsPageTitleUpdateEnabled.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.IsItemMergeFieldEnabled ),
                () => block.SetAttributeValue( AttributeKey.MergeContent, box.Settings.IsItemMergeFieldEnabled.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.DetailPage ),
                () => block.SetAttributeValue( AttributeKey.DetailPage, box.Settings.DetailPage.ToCommaDelimitedPageRouteValues() ) );

            box.IfValidProperty( nameof( box.Settings.ItemCacheDuration ),
                () => block.SetAttributeValue( AttributeKey.ItemCacheDuration, box.Settings.ItemCacheDuration.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.OutputCacheDuration ),
                () => block.SetAttributeValue( AttributeKey.OutputCacheDuration, box.Settings.OutputCacheDuration.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.CacheTags ),
                () => block.SetAttributeValue( AttributeKey.CacheTags, box.Settings.CacheTags?.AsDelimited( "," ) ) );

            box.IfValidProperty( nameof( box.Settings.IsLogInteractionsEnabled ),
                () => block.SetAttributeValue( AttributeKey.LogInteractions, box.Settings.IsLogInteractionsEnabled.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.IsWriteInteractionOnlyIfIndividualLoggedInEnabled ),
                () => block.SetAttributeValue( AttributeKey.WriteInteractionOnlyIfIndividualLoggedIn, box.Settings.IsWriteInteractionOnlyIfIndividualLoggedInEnabled.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.WorkflowType ),
                () => block.SetAttributeValue( AttributeKey.WorkflowType, box.Settings.WorkflowType?.Value ) );

            box.IfValidProperty( nameof( box.Settings.IsLaunchWorkflowOnlyIfIndividualLoggedInEnabled ),
                () => block.SetAttributeValue( AttributeKey.LaunchWorkflowOnlyIfIndividualLoggedIn, box.Settings.IsLaunchWorkflowOnlyIfIndividualLoggedInEnabled.ToString() ) );

            box.IfValidProperty( nameof( box.Settings.LaunchWorkflowCondition ),
                () => block.SetAttributeValue( AttributeKey.LaunchWorkflowCondition, box.Settings.LaunchWorkflowCondition ) );

            box.IfValidProperty( nameof( box.Settings.MetaDescriptionAttributeValueKey ),
                () => block.SetAttributeValue( AttributeKey.MetaDescriptionAttribute, box.Settings.MetaDescriptionAttributeValueKey ) );

            box.IfValidProperty( nameof( box.Settings.OpenGraphType ),
                () => block.SetAttributeValue( AttributeKey.OpenGraphType, box.Settings.OpenGraphType ) );

            box.IfValidProperty( nameof( box.Settings.OpenGraphTitleAttributeValueKey ),
                () => block.SetAttributeValue( AttributeKey.OpenGraphTitleAttribute, box.Settings.OpenGraphTitleAttributeValueKey ) );

            box.IfValidProperty( nameof( box.Settings.OpenGraphDescriptionAttributeValueKey ),
                () => block.SetAttributeValue( AttributeKey.OpenGraphDescriptionAttribute, box.Settings.OpenGraphDescriptionAttributeValueKey ) );

            box.IfValidProperty( nameof( box.Settings.OpenGraphImageAttributeValueKey ),
                () => block.SetAttributeValue( AttributeKey.OpenGraphImageAttribute, box.Settings.OpenGraphImageAttributeValueKey ) );

            box.IfValidProperty( nameof( box.Settings.TwitterTitleAttributeValueKey ),
                () => block.SetAttributeValue( AttributeKey.TwitterTitleAttribute, box.Settings.TwitterTitleAttributeValueKey ) );

            box.IfValidProperty( nameof( box.Settings.TwitterDescriptionAttributeValueKey ),
                () => block.SetAttributeValue( AttributeKey.TwitterDescriptionAttribute, box.Settings.TwitterDescriptionAttributeValueKey ) );

            box.IfValidProperty( nameof( box.Settings.TwitterImageAttributeValueKey ),
                () => block.SetAttributeValue( AttributeKey.TwitterImageAttribute, box.Settings.TwitterImageAttributeValueKey ) );

            box.IfValidProperty( nameof( box.Settings.TwitterCard ),
                () => block.SetAttributeValue( AttributeKey.TwitterCard, box.Settings.TwitterCard ) );

            block.SaveAttributeValues( RockContext );

            // Settings affect rendered output, so any previously cached output is now stale.
            ClearAllCacheItems();

            return ActionOk();
        }

        /// <summary>
        /// Records a "View" interaction (plus any matching intent interactions) for a content
        /// channel item. Called by the client after the page renders, which avoids logging
        /// bot views since bots typically don't execute JavaScript.
        /// </summary>
        /// <remarks>
        /// Replay note: the View interaction uses the token's Guid, so the bulk-insert dedup
        /// prevents duplicate View records. Intent interactions get fresh Guids per call, so
        /// a visitor replaying their own valid token can inflate their own intent records
        /// during its lifetime. That's analytics noise from a known visitor, not a security
        /// concern, and we rely on edge rate limiting to bound it.
        /// </remarks>
        [BlockAction]
        public BlockActionResult RegisterInteraction( string interactionToken )
        {
            var payload = TryDecryptInteractionToken( interactionToken );
            if ( payload == null )
            {
                return ActionOk();
            }

            // Logging may have been turned off after the token was issued. Other eligibility
            // checks (crawler, logged-in-only) don't need to be re-run because they were
            // already enforced when the encrypted token was issued.
            if ( !GetAttributeValue( AttributeKey.LogInteractions ).AsBoolean() )
            {
                return ActionOk();
            }

            var contentChannelItem = new ContentChannelItemService( RockContext )
                .Queryable()
                .Include( c => c.ContentChannel )
                .FirstOrDefault( c => c.Id == payload.ItemId );

            if ( contentChannelItem == null || !ItemIsApprovedToDisplay( contentChannelItem ) )
            {
                return ActionOk();
            }

            var mediumType = DefinedValueCache.Get( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CONTENTCHANNEL.AsGuid() );
            if ( mediumType == null )
            {
                return ActionOk();
            }

            var info = new InteractionTransactionInfo
            {
                InteractionGuid = payload.Guid,
                GetValuesFromHttpRequest = false,
                ChannelTypeMediumValueId = mediumType.Id,
                ChannelEntityId = contentChannelItem.ContentChannel.Id,
                ChannelName = contentChannelItem.ContentChannel.ToString(),
                ComponentEntityTypeId = contentChannelItem.TypeId,
                ComponentEntityId = contentChannelItem.Id,
                ComponentName = contentChannelItem.ToString(),
                InteractionSummary = contentChannelItem.Title,
                InteractionOperation = "View",
                PersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId,
                UserAgent = RequestContext?.ClientInformation?.UserAgent,
                IPAddress = RequestContext?.ClientInformation?.IpAddress
            };

            // The registration request carries the visitor's UTM cookie (the page-render request
            // strips it from the request collection), so read the UTM values here to attribute them.
            var utmInfo = UtmHelper.GetUtmCookieDataFromRequest( RequestContext );
            UtmHelper.AddUtmInfoToInteractionTransactionInfo( info, utmInfo );

            new InteractionTransaction( info ).Enqueue();

            InteractionService.RegisterIntentInteractions( EntityIntentCache.GetIntentValueIds<ContentChannelItem>( contentChannelItem.Id ) );

            return ActionOk();
        }

        #endregion Block Actions

        #region IHasCustomActions Implementation

        /// <inheritdoc/>
        List<BlockCustomActionBag> IHasCustomActions.GetCustomActions( bool canEdit, bool canAdministrate )
        {
            var actions = new List<BlockCustomActionBag>();

            if ( canAdministrate )
            {
                actions.Add( new BlockCustomActionBag
                {
                    IconCssClass = "ti ti-edit",
                    Tooltip = "Channel Item Configuration",
                    ComponentFileUrl = "/Obsidian/Blocks/Cms/ContentChannelItemView/contentChannelItemViewCustomSettings.obs"
                } );
            }

            return actions;
        }

        #endregion IHasCustomActions Implementation

        #region IBreadCrumbBlock Implementation

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var result = new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb>()
            };

            if ( !GetAttributeValue( AttributeKey.SetPageTitle ).AsBoolean() )
            {
                return result;
            }

            var contentChannelItemParameterValue = GetContentChannelItemParameterValue( pageReference );
            if ( string.IsNullOrEmpty( contentChannelItemParameterValue ) )
            {
                return result;
            }

            // Resolve up front so the page-title cache key matches what RenderContent uses
            // (keyed by item Id, not URL form), enabling cache hits across slug/id/guid URLs.
            var contentChannelItem = ResolveDisplayedItem( contentChannelItemParameterValue );
            if ( contentChannelItem == null || !ItemIsApprovedToDisplay( contentChannelItem ) )
            {
                return result;
            }

            string pageTitle = null;
            var outputCacheDuration = GetAttributeValue( AttributeKey.OutputCacheDuration ).AsIntegerOrNull();

            if ( outputCacheDuration.HasValue && outputCacheDuration.Value > 0 )
            {
                pageTitle = GetCachedItem<string>( GetCacheKey( $"PageTitle_{contentChannelItem.Id}" ) );
            }

            if ( pageTitle == null )
            {
                pageTitle = contentChannelItem.Title;
            }

            if ( pageTitle.IsNotNullOrWhiteSpace() )
            {
                result.BreadCrumbs.Add( new BreadCrumbLink( pageTitle, pageReference ) );
            }

            return result;
        }

        #endregion IBreadCrumbBlock Implementation

        #region Private Methods

        /// <summary>
        /// Issues an encrypted interaction token to eligible visitors. Ineligible requests
        /// (logging disabled, crawler, logged-in-only with no person, no item resolved) get a
        /// null token and the client silently skips registration.
        /// </summary>
        private ContentChannelItemViewOptionsBag BuildInitializationBag()
        {
            var bag = new ContentChannelItemViewOptionsBag();

            if ( !GetAttributeValue( AttributeKey.LogInteractions ).AsBoolean() )
            {
                return bag;
            }

            if ( RequestContext?.ClientInformation?.BrowserInfo?.ClientType == "Crawler" )
            {
                return bag;
            }

            if ( GetAttributeValue( AttributeKey.WriteInteractionOnlyIfIndividualLoggedIn ).AsBoolean()
                 && RequestContext.CurrentPerson == null )
            {
                return bag;
            }

            var contentChannelItem = ResolveDisplayedItem( GetContentChannelItemParameterValue() );
            if ( contentChannelItem == null || !ItemIsApprovedToDisplay( contentChannelItem ) )
            {
                return bag;
            }

            // Encrypting the payload (rather than sending Item Id + Guid as separate fields) means
            // the client cannot point the registration at a different item or extend the lifetime.
            var payload = new InteractionTokenPayload
            {
                Guid = Guid.NewGuid(),
                ItemId = contentChannelItem.Id,
                ExpiresAt = RockDateTime.Now.Add( InteractionTokenLifetime )
            };

            bag.InteractionToken = Rock.Security.Encryption.EncryptString( payload.ToJson() );
            bag.InteractionTokenLifetimeSeconds = ( int ) InteractionTokenLifetime.TotalSeconds;

            return bag;
        }

        /// <summary>
        /// Decrypts and validates an interaction token. Returns <c>null</c> for any tampered,
        /// expired, or otherwise unusable token so callers can treat any failure as "do nothing."
        /// </summary>
        private InteractionTokenPayload TryDecryptInteractionToken( string token )
        {
            if ( token.IsNullOrWhiteSpace() )
            {
                return null;
            }

            string json;
            try
            {
                json = Rock.Security.Encryption.DecryptString( token );
            }
            catch
            {
                return null;
            }

            var payload = json.FromJsonOrNull<InteractionTokenPayload>();
            if ( payload == null || payload.ItemId <= 0 || payload.ExpiresAt < RockDateTime.Now )
            {
                return null;
            }

            return payload;
        }

        private BlockActionResult GetSettingsForContentChannel( Guid? contentChannelGuid )
        {
            var settings = new ContentChannelItemViewCustomSettingsBag();
            var options = new ContentChannelItemViewCustomSettingsOptionsBag();

            options.ContentChannels = ContentChannelCache.All()
                .Where( c => c.ContentChannelType.ShowInChannelList )
                .OrderBy( c => c.Name )
                .ToListItemBagList();

            ContentChannelCache contentChannel = null;
            if ( contentChannelGuid.HasValue && options.ContentChannels.Any( c => c.Value.AsGuidOrNull() == contentChannelGuid ) )
            {
                settings.ContentChannelGuid = contentChannelGuid;
                contentChannel = ContentChannelCache.Get( contentChannelGuid.Value );
                contentChannel?.LoadAttributes();
            }

            // Statuses are only meaningful (and only offered to the user) when the channel
            // requires approval; otherwise every item is implicitly displayable.
            if ( contentChannel != null && contentChannel.RequiresApproval && !contentChannel.ContentChannelType.DisableStatus )
            {
                options.ContentChannelItemStatuses = typeof( ContentChannelItemStatus ).ToEnumListItemBag();
                settings.ContentChannelItemStatuses = GetAttributeValue( AttributeKey.Status ).SplitDelimitedValues().AsEnumList<ContentChannelItemStatus>();
            }

            settings.LavaTemplate = GetAttributeValue( AttributeKey.LavaTemplate );
            settings.ContentChannelQueryParameter = GetAttributeValue( AttributeKey.ContentChannelQueryParameter );
            settings.IsDisplayMostRecentEnabled = GetAttributeValue( AttributeKey.DisplayMostRecent ).AsBoolean();
            settings.IsPageTitleUpdateEnabled = GetAttributeValue( AttributeKey.SetPageTitle ).AsBoolean();
            settings.IsItemMergeFieldEnabled = GetAttributeValue( AttributeKey.MergeContent ).AsBoolean();
            settings.DetailPage = GetAttributeValue( AttributeKey.DetailPage ).ToPageRouteValueBag();
            settings.ItemCacheDuration = GetAttributeValue( AttributeKey.ItemCacheDuration ).AsInteger();
            settings.OutputCacheDuration = GetAttributeValue( AttributeKey.OutputCacheDuration ).AsInteger();

            options.CacheTags = DefinedTypeCache.Get( SystemGuid.DefinedType.CACHE_TAGS.AsGuid() )
                ?.DefinedValues
                .Where( dv => dv.IsActive )
                .Select( dv => new ListItemBag
                {
                    Value = dv.Value,
                    Text = dv.Value
                } )
                .ToList() ?? new List<ListItemBag>();

            if ( options.CacheTags.Any() )
            {
                var selectedCacheTags = GetAttributeValue( AttributeKey.CacheTags ).SplitDelimitedValues();
                var vettedSelectedCacheTags = selectedCacheTags
                    .Where( tag => options.CacheTags.Any( c => c.Value == tag ) )
                    .ToList();

                settings.CacheTags = vettedSelectedCacheTags.Any() ? vettedSelectedCacheTags : null;
            }

            settings.IsLogInteractionsEnabled = GetAttributeValue( AttributeKey.LogInteractions ).AsBoolean();
            settings.IsWriteInteractionOnlyIfIndividualLoggedInEnabled = GetAttributeValue( AttributeKey.WriteInteractionOnlyIfIndividualLoggedIn ).AsBoolean();

            var workflowTypeGuid = GetAttributeValue( AttributeKey.WorkflowType ).AsGuidOrNull();
            if ( workflowTypeGuid.HasValue )
            {
                var workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value );
                if ( workflowType != null )
                {
                    settings.WorkflowType = new ListItemBag
                    {
                        Value = workflowType.Guid.ToString(),
                        Text = workflowType.Name
                    };
                }
            }

            settings.IsLaunchWorkflowOnlyIfIndividualLoggedInEnabled = GetAttributeValue( AttributeKey.LaunchWorkflowOnlyIfIndividualLoggedIn ).AsBoolean();
            options.LaunchWorkflowConditions = typeof( LaunchWorkflowCondition ).ToEnumListItemBag();
            settings.LaunchWorkflowCondition = GetAttributeValue( AttributeKey.LaunchWorkflowCondition );

            // The "C^"/"I^" prefix lets a single dropdown value identify whether the attribute
            // belongs to the channel or to the item, since both can supply meta values.
            var allAttributeOptions = new List<ListItemBag>();
            var imageAttributeOptions = new List<ListItemBag>();

            if ( contentChannel != null )
            {
                foreach ( var channelAttribute in contentChannel.Attributes.Values.OrderBy( a => a.Order ) )
                {
                    var computedKey = $"C^{channelAttribute.Key}";
                    var label = $"Channel: {channelAttribute.Name}";

                    allAttributeOptions.Add( new ListItemBag { Value = computedKey, Text = label } );

                    if ( channelAttribute.FieldType.Name == "Image" )
                    {
                        imageAttributeOptions.Add( new ListItemBag { Value = computedKey, Text = label } );
                    }
                }

                var itemAttributes = AttributeCache.AllForEntityType<ContentChannelItem>()
                    .Where( a => a.IsActive )
                    .Where( a =>
                        ( a.EntityTypeQualifierColumn.Equals( "ContentChannelTypeId", StringComparison.OrdinalIgnoreCase )
                          && a.EntityTypeQualifierValue.Equals( contentChannel.ContentChannelTypeId.ToString() ) )
                        || ( a.EntityTypeQualifierColumn.Equals( "ContentChannelId", StringComparison.OrdinalIgnoreCase )
                             && a.EntityTypeQualifierValue.Equals( contentChannel.Id.ToString() ) ) )
                    .OrderByDescending( a => a.EntityTypeQualifierColumn )
                    .ThenBy( a => a.Order )
                    .GroupBy( a => a.Key )
                    .Select( g => g.First() )
                    .ToList();

                foreach ( var itemAttribute in itemAttributes )
                {
                    var computedKey = $"I^{itemAttribute.Key}";
                    var label = $"Item: {itemAttribute.Name}";

                    allAttributeOptions.Add( new ListItemBag { Value = computedKey, Text = label } );

                    if ( itemAttribute.FieldType.Name == "Image" )
                    {
                        imageAttributeOptions.Add( new ListItemBag { Value = computedKey, Text = label } );
                    }
                }
            }

            options.MetaDescriptionAttributes = allAttributeOptions;
            options.TitleAttributes = allAttributeOptions;
            options.DescriptionAttributes = allAttributeOptions;
            options.ImageAttributes = imageAttributeOptions;

            options.OpenGraphTypes = new List<ListItemBag>
            {
                new ListItemBag { Value = "article", Text = "article" },
                new ListItemBag { Value = "website", Text = "website" },
                new ListItemBag { Value = "book", Text = "book" },
                new ListItemBag { Value = "place", Text = "place" },
                new ListItemBag { Value = "product", Text = "product" },
                new ListItemBag { Value = "profile", Text = "profile" },
                new ListItemBag { Value = "video.episode", Text = "video.episode" },
                new ListItemBag { Value = "video.movie", Text = "video.movie" },
                new ListItemBag { Value = "video.other", Text = "video.other" },
                new ListItemBag { Value = "video.tv_show", Text = "video.tv_show" }
            };

            options.TwitterCards = new List<ListItemBag>
            {
                new ListItemBag { Value = "none", Text = "" },
                new ListItemBag { Value = "summary", Text = "Summary" },
                new ListItemBag { Value = "summary_large_image", Text = "Summary with large image" }
            };

            settings.MetaDescriptionAttributeValueKey = GetAttributeValue( AttributeKey.MetaDescriptionAttribute );
            settings.OpenGraphType = GetAttributeValue( AttributeKey.OpenGraphType );
            settings.OpenGraphTitleAttributeValueKey = GetAttributeValue( AttributeKey.OpenGraphTitleAttribute );
            settings.OpenGraphDescriptionAttributeValueKey = GetAttributeValue( AttributeKey.OpenGraphDescriptionAttribute );
            settings.OpenGraphImageAttributeValueKey = GetAttributeValue( AttributeKey.OpenGraphImageAttribute );
            settings.TwitterTitleAttributeValueKey = GetAttributeValue( AttributeKey.TwitterTitleAttribute );
            settings.TwitterDescriptionAttributeValueKey = GetAttributeValue( AttributeKey.TwitterDescriptionAttribute );
            settings.TwitterImageAttributeValueKey = GetAttributeValue( AttributeKey.TwitterImageAttribute );
            settings.TwitterCard = GetAttributeValue( AttributeKey.TwitterCard );

            return ActionOk( new CustomSettingsBox<ContentChannelItemViewCustomSettingsBag, ContentChannelItemViewCustomSettingsOptionsBag>
            {
                Settings = settings,
                Options = options
            } );
        }

        private string RenderContent()
        {
            var outputCacheDuration = GetAttributeValue( AttributeKey.OutputCacheDuration ).AsIntegerOrNull();
            var setPageTitle = GetAttributeValue( AttributeKey.SetPageTitle ).AsBoolean();
            var isMergeContentEnabled = GetAttributeValue( AttributeKey.MergeContent ).AsBoolean();

            var contentChannelItemParameterValue = GetContentChannelItemParameterValue();

            // Resolving up front lets the output/title caches be keyed by item Id, so the same
            // item reached via /article/easter, /article/123, and /article/<guid> shares one
            // cache entry instead of three. Item_ cache keeps this near-free on warm cache.
            // ResolveDisplayedItem also handles the "Display Most Recent" fallback when the URL
            // either has no item key or has an unresolvable one (e.g. a stray tracking param).
            var contentChannelItem = ResolveDisplayedItem( contentChannelItemParameterValue );

            if ( contentChannelItem == null || !ItemIsApprovedToDisplay( contentChannelItem ) )
            {
                return RenderNoDataFound();
            }

            string outputContents = null;
            string pageTitle = null;

            var outputCacheKey = GetCacheKey( $"Output_{contentChannelItem.Id}" );
            var pageTitleCacheKey = GetCacheKey( $"PageTitle_{contentChannelItem.Id}" );

            if ( outputCacheDuration.HasValue && outputCacheDuration.Value > 0 )
            {
                outputContents = GetCachedItem<string>( outputCacheKey );
                pageTitle = GetCachedItem<string>( pageTitleCacheKey );
            }

            if ( outputContents == null )
            {
                var commonMergeFields = RequestContext.GetCommonMergeFields();

                if ( isMergeContentEnabled )
                {
                    var itemMergeFields = new Dictionary<string, object>( commonMergeFields )
                    {
                        ["Item"] = contentChannelItem
                    };

                    var enabledCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );
                    contentChannelItem.Content = contentChannelItem.Content.ResolveMergeFields( itemMergeFields, enabledCommands );
                    contentChannelItem.LoadAttributes();

                    foreach ( var attributeValue in contentChannelItem.AttributeValues )
                    {
                        attributeValue.Value.Value = attributeValue.Value.Value.ResolveMergeFields( itemMergeFields, enabledCommands );
                    }
                }

                var detailPageRoute = GetAttributeValue( AttributeKey.DetailPage );
                var detailPageId = PageCache.Get( detailPageRoute )?.Id ?? 0;

                var mergeFields = new Dictionary<string, object>( commonMergeFields )
                {
                    ["RockVersion"] = Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber(),
                    ["Item"] = contentChannelItem,
                    ["DetailPage"] = detailPageId,
                    ["DetailPageRoute"] = detailPageRoute
                };

                var lavaTemplate = GetAttributeValue( AttributeKey.LavaTemplate );
                outputContents = lavaTemplate.ResolveMergeFields( mergeFields, GetAttributeValue( AttributeKey.EnabledLavaCommands ) );

                if ( setPageTitle )
                {
                    pageTitle = contentChannelItem.Title;
                }

                if ( outputCacheDuration.HasValue && outputCacheDuration.Value > 0 )
                {
                    var cacheTags = GetAttributeValue( AttributeKey.CacheTags ) ?? string.Empty;

                    AddOutputCacheKey( outputCacheKey, cacheTags );
                    AddOutputCacheKey( pageTitleCacheKey, cacheTags );

                    SetCachedItem( outputCacheKey, outputContents, outputCacheDuration.Value, cacheTags );

                    if ( pageTitle != null )
                    {
                        SetCachedItem( pageTitleCacheKey, pageTitle, outputCacheDuration.Value, cacheTags );
                    }
                }
            }

            if ( setPageTitle && pageTitle.IsNotNullOrWhiteSpace() )
            {
                var siteName = PageCache?.Layout?.Site?.Name;
                ResponseContext.SetPageTitle( pageTitle );
                ResponseContext.SetBrowserTitle( siteName.IsNotNullOrWhiteSpace() ? $"{pageTitle} | {siteName}" : pageTitle );
            }

            // Meta tags must be re-applied on every render, including output-cache hits, because
            // ResponseContext is per-request and the cached output string carries only the body.
            ApplyMetaTags( contentChannelItem );

            LaunchWorkflowIfConfigured( contentChannelItem );

            return outputContents;
        }

        /// <summary>
        /// Surfaces a visible warning to administrators when no item resolves so missing-data
        /// problems are noticeable during page configuration; visitors see nothing.
        /// </summary>
        private string RenderNoDataFound()
        {
            if ( BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return @"<div class='alert alert-warning'>
    404 - No Content. If you did not have Administrate permissions on this block, you would have gotten a real 404 page.
</div>";
            }

            return string.Empty;
        }

        /// <summary>
        /// Resolves the URL parameter (slug, id, or guid) that identifies which content channel
        /// item to display. Accepts an optional <paramref name="pageReference"/> for the
        /// breadcrumb path, which doesn't have access to the live request.
        /// </summary>
        private string GetContentChannelItemParameterValue( PageReference pageReference = null )
        {
            string contentChannelItemKey = null;

            var contentChannelQueryParameter = GetAttributeValue( AttributeKey.ContentChannelQueryParameter );
            if ( !string.IsNullOrEmpty( contentChannelQueryParameter ) )
            {
                contentChannelItemKey = pageReference != null
                    ? pageReference.GetPageParameter( contentChannelQueryParameter )
                    : PageParameter( contentChannelQueryParameter );
            }
            else if ( pageReference != null )
            {
                var key = pageReference.Parameters.Keys.FirstOrDefault();
                if ( key != null )
                {
                    contentChannelItemKey = pageReference.GetPageParameter( key );
                }
            }
            else
            {
                // Prefer route values over query string so trackers like ?fbclid=... don't
                // hijack the lookup. The default "page/{PageId}" route is the exception: its
                // sole route value is "PageId", which would resolve to the page id rather than
                // an item key, so in that case we fall back to the first query string parameter.
                var queryStringKeys = RequestContext?.QueryString?.AllKeys
                    ?.Where( k => k.IsNotNullOrWhiteSpace() )
                    .ToList()
                    ?? new List<string>();

                var routeKeys = ( RequestContext?.GetPageParameters()?.Keys ?? Enumerable.Empty<string>() )
                    .Except( queryStringKeys, StringComparer.OrdinalIgnoreCase )
                    .ToList();

                var isStandardPageRoute = routeKeys.Count == 1
                    && routeKeys[0].Equals( PageParameterKey.PageId, StringComparison.OrdinalIgnoreCase );

                if ( !isStandardPageRoute )
                {
                    var routeKey = routeKeys.LastOrDefault();
                    if ( routeKey.IsNotNullOrWhiteSpace() )
                    {
                        contentChannelItemKey = PageParameter( routeKey );
                    }
                }
                else if ( queryStringKeys.Any() )
                {
                    contentChannelItemKey = PageParameter( queryStringKeys[0] );
                }
            }

            if ( contentChannelItemKey.IsNullOrWhiteSpace() )
            {
                var mostRecentItemId = GetMostRecentContentChannelItemId();
                if ( mostRecentItemId.HasValue )
                {
                    contentChannelItemKey = mostRecentItemId.Value.ToString();
                }
            }

            return contentChannelItemKey;
        }

        /// <summary>
        /// Returns the most recent content channel item Id for the configured channel, gated on
        /// the "Display Most Recent" setting. Used both as the primary fallback when the URL
        /// has no item key, and as a secondary fallback when a key was extracted (e.g. a
        /// tracking query param) but didn't resolve to a real item.
        /// </summary>
        private int? GetMostRecentContentChannelItemId()
        {
            if ( !GetAttributeValue( AttributeKey.DisplayMostRecent ).AsBoolean() )
            {
                return null;
            }

            var contentChannelGuid = GetAttributeValue( AttributeKey.ContentChannel ).AsGuidOrNull();
            if ( !contentChannelGuid.HasValue )
            {
                return null;
            }

            var statuses = GetApprovedStatuses();
            var now = RockDateTime.Now;

            return new ContentChannelItemService( RockContext ).Queryable()
                .Where( i => i.ContentChannel.Guid == contentChannelGuid.Value
                             && i.StartDateTime <= now
                             && ( !i.ContentChannel.RequiresApproval || statuses.Contains( i.Status ) ) )
                .OrderByDescending( c => c.StartDateTime )
                .Select( c => ( int? ) c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Loads the content channel item for the supplied key, falling back to the most recent
        /// item if the lookup returns null. This is what callers should use when they need the
        /// "displayed" item — it handles both an empty key (no parameter on the URL) and a
        /// non-empty key that doesn't resolve (e.g. <c>?utm_source=...</c> on a standard page
        /// route, where the route-detection logic picks up the tracking value as the key).
        /// </summary>
        private ContentChannelItem ResolveDisplayedItem( string contentChannelItemKey )
        {
            var item = GetContentChannelItem( contentChannelItemKey );
            if ( item != null )
            {
                return item;
            }

            var mostRecentItemId = GetMostRecentContentChannelItemId();
            if ( mostRecentItemId.HasValue )
            {
                item = GetContentChannelItem( mostRecentItemId.Value.ToString() );
            }

            return item;
        }

        /// <summary>
        /// Loads the content channel item from a key that may be an integer Id, a Guid, or a slug.
        /// </summary>
        private ContentChannelItem GetContentChannelItem( string contentChannelItemKey )
        {
            if ( string.IsNullOrEmpty( contentChannelItemKey ) )
            {
                return null;
            }

            var itemCacheDuration = GetAttributeValue( AttributeKey.ItemCacheDuration ).AsIntegerOrNull();
            var contentChannelGuid = GetAttributeValue( AttributeKey.ContentChannel ).AsGuidOrNull();
            var itemCacheKey = GetCacheKey( $"Item_{contentChannelGuid}_{contentChannelItemKey}" );

            if ( itemCacheDuration.HasValue && itemCacheDuration.Value > 0 )
            {
                var cachedItem = GetCachedItem<ContentChannelItem>( itemCacheKey );
                if ( cachedItem != null )
                {
                    return cachedItem;
                }
            }

            /*
                5/5/26 - JMH

                A dedicated RockContext (not the block's per-request one) is intentionally
                not disposed here. The cached EF proxy entity holds a reference back to this
                context, keeping it alive for the duration of the cache entry. This matches
                the behavior of the original WebForms block and allows Lava templates to
                lazy-load navigation properties on cached items without throwing an
                ObjectDisposedException.

                Reason: Prevent ObjectDisposedException on cached items during Lava rendering.
            */
            var itemRockContext = new RockContext();
            var query = new ContentChannelItemService( itemRockContext )
                .Queryable()
                .Include( c => c.ContentChannel );

            ContentChannelItem contentChannelItem;

            if ( contentChannelItemKey.AsIntegerOrNull() is int contentChannelItemId )
            {
                contentChannelItem = query.FirstOrDefault( c => c.Id == contentChannelItemId );
            }
            else if ( contentChannelItemKey.AsGuidOrNull() is Guid contentChannelItemGuid )
            {
                contentChannelItem = query.FirstOrDefault( c => c.Guid == contentChannelItemGuid );
            }
            else
            {
                if ( contentChannelGuid.HasValue )
                {
                    query = query.Where( c => c.ContentChannel.Guid == contentChannelGuid.Value );
                }

                contentChannelItem = query
                    .FirstOrDefault( a => a.ContentChannelItemSlugs.Any( s => s.Slug == contentChannelItemKey ) );
            }

            if ( contentChannelItem != null && itemCacheDuration.HasValue && itemCacheDuration.Value > 0 )
            {
                var cacheTags = GetAttributeValue( AttributeKey.CacheTags ) ?? string.Empty;
                AddOutputCacheKey( itemCacheKey, cacheTags );
                SetCachedItem( itemCacheKey, contentChannelItem, itemCacheDuration.Value, cacheTags );
            }

            return contentChannelItem;
        }

        private List<ContentChannelItemStatus> GetApprovedStatuses()
        {
            return ( GetAttributeValue( AttributeKey.Status ) ?? "2" )
                .Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
                .Select( s => s.ConvertToEnumOrNull<ContentChannelItemStatus>() )
                .Where( s => s.HasValue )
                .Select( s => s.Value )
                .ToList();
        }

        private bool ItemIsApprovedToDisplay( ContentChannelItem contentChannelItem )
        {
            if ( contentChannelItem.ContentChannel.RequiresApproval && !GetApprovedStatuses().Contains( contentChannelItem.Status ) )
            {
                return false;
            }

            var channelGuid = GetAttributeValue( AttributeKey.ContentChannel ).AsGuidOrNull();
            if ( channelGuid.HasValue )
            {
                var channel = ContentChannelCache.Get( channelGuid.Value );
                if ( channel != null && channel.Id != contentChannelItem.ContentChannelId )
                {
                    return false;
                }
            }

            return true;
        }

        private void LaunchWorkflowIfConfigured( ContentChannelItem contentChannelItem )
        {
            if ( contentChannelItem == null )
            {
                return;
            }

            var workflowTypeGuid = GetAttributeValue( AttributeKey.WorkflowType ).AsGuidOrNull();
            if ( !workflowTypeGuid.HasValue )
            {
                return;
            }

            var workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value );
            if ( workflowType?.IsActive != true )
            {
                return;
            }

            var launchOnlyIfLoggedIn = GetAttributeValue( AttributeKey.LaunchWorkflowOnlyIfIndividualLoggedIn ).AsBoolean();
            var currentPerson = RequestContext.CurrentPerson;
            if ( launchOnlyIfLoggedIn && currentPerson == null )
            {
                return;
            }

            var launchWorkflowCondition = GetAttributeValue( AttributeKey.LaunchWorkflowCondition ).ConvertToEnum<LaunchWorkflowCondition>();
            if ( launchWorkflowCondition != LaunchWorkflowCondition.Always && currentPerson == null )
            {
                return;
            }

            string alreadyLaunchedKey = null;
            if ( launchWorkflowCondition == LaunchWorkflowCondition.OncePerPersonPerContentChannelItem )
            {
                alreadyLaunchedKey = $"WorkflowLaunched_{workflowType.Id}_{contentChannelItem.Id}";
            }
            else if ( launchWorkflowCondition == LaunchWorkflowCondition.OncePerPerson )
            {
                alreadyLaunchedKey = $"WorkflowLaunched_{workflowType.Id}";
            }

            PersonPreferenceCollection preferences = null;

            if ( alreadyLaunchedKey != null )
            {
                preferences = GetBlockPersonPreferences();
                if ( preferences.GetValue( alreadyLaunchedKey ).AsBooleanOrNull() == true )
                {
                    return;
                }
            }

            var workflowAttributeValues = new Dictionary<string, string>
            {
                ["ContentChannelItem"] = contentChannelItem.Guid.ToString()
            };

            LaunchWorkflowTransaction launchWorkflowTransaction;
            if ( currentPerson != null )
            {
                workflowAttributeValues["Person"] = currentPerson.Guid.ToString();
                launchWorkflowTransaction = new LaunchWorkflowTransaction<Person>( workflowType.Id, null, currentPerson.Id );
            }
            else
            {
                launchWorkflowTransaction = new LaunchWorkflowTransaction( workflowType.Id, null );
            }

            launchWorkflowTransaction.WorkflowAttributeValues = workflowAttributeValues;
            launchWorkflowTransaction.InitiatorPersonAliasId = currentPerson?.PrimaryAliasId;
            launchWorkflowTransaction.Enqueue();

            // Save the preference only after Enqueue() succeeds; otherwise a throwing Enqueue
            // would lock the visitor out of the workflow forever despite it never running.
            if ( preferences != null )
            {
                preferences.SetValue( alreadyLaunchedKey, true.ToString() );
                preferences.Save();
            }
        }

        private void ApplyMetaTags( ContentChannelItem contentChannelItem )
        {
            var metaDescription = GetMetaValueFromAttribute( GetAttributeValue( AttributeKey.MetaDescriptionAttribute ), contentChannelItem );
            if ( metaDescription.IsNotNullOrWhiteSpace() )
            {
                ResponseContext.AddMetaTag( "description", null, metaDescription.SanitizeHtml( true ) );
            }

            AddOpenGraphMeta( "og:type", GetAttributeValue( AttributeKey.OpenGraphType ) );
            AddOpenGraphMeta( "og:title", GetMetaValueFromAttribute( GetAttributeValue( AttributeKey.OpenGraphTitleAttribute ), contentChannelItem ) );
            AddOpenGraphMeta( "og:description", GetMetaValueFromAttribute( GetAttributeValue( AttributeKey.OpenGraphDescriptionAttribute ), contentChannelItem ) );
            AddOpenGraphMeta( "og:image", GetMetaValueFromAttribute( GetAttributeValue( AttributeKey.OpenGraphImageAttribute ), contentChannelItem ) );

            AddTwitterMeta( "twitter:title", GetMetaValueFromAttribute( GetAttributeValue( AttributeKey.TwitterTitleAttribute ), contentChannelItem ) );
            AddTwitterMeta( "twitter:description", GetMetaValueFromAttribute( GetAttributeValue( AttributeKey.TwitterDescriptionAttribute ), contentChannelItem ) );
            AddTwitterMeta( "twitter:image", GetMetaValueFromAttribute( GetAttributeValue( AttributeKey.TwitterImageAttribute ), contentChannelItem ) );

            var twitterCard = GetAttributeValue( AttributeKey.TwitterCard );
            if ( twitterCard.IsNotNullOrWhiteSpace() && twitterCard != "none" )
            {
                AddTwitterMeta( "twitter:card", twitterCard );
            }
        }

        /// <summary>
        /// Open Graph requires the <c>property</c> attribute (not the standard <c>name</c>),
        /// so this needs its own helper rather than going through <see cref="AddTwitterMeta"/>.
        /// </summary>
        private void AddOpenGraphMeta( string property, string content )
        {
            if ( string.IsNullOrEmpty( content ) )
            {
                return;
            }

            var attributes = new Dictionary<string, string>
            {
                ["property"] = property,
                ["content"] = content
            };

            ResponseContext.AddHtmlElement( $"meta-{property}", "meta", null, attributes, Rock.Enums.Net.ResponseElementLocation.Header );
        }

        private void AddTwitterMeta( string name, string content )
        {
            if ( string.IsNullOrEmpty( content ) )
            {
                return;
            }

            ResponseContext.AddMetaTag( name, null, content );
        }

        /// <summary>
        /// Resolves a meta value from a "C^attributeKey" (channel) or "I^attributeKey" (item)
        /// computed key. Renders through Lava so field types like Image format as URLs.
        /// </summary>
        private string GetMetaValueFromAttribute( string computedKey, ContentChannelItem contentChannelItem )
        {
            if ( string.IsNullOrEmpty( computedKey ) )
            {
                return null;
            }

            var parts = computedKey.Split( '^' );
            var entityType = parts.Length > 0 ? parts[0] : "C";
            var attributeKey = parts.Length > 1 ? parts[1] : string.Empty;

            object mergeObject;
            if ( entityType == "C" )
            {
                // ContentChannelCache preloads channel attributes.
                mergeObject = ContentChannelCache.Get( contentChannelItem.ContentChannelId );
            }
            else
            {
                // The item is loaded by GetContentChannelItem without attributes, and the
                // MergeContent path that loads them is conditional. Load on demand here so the
                // Lava Attribute filter has data to read regardless of MergeContent's setting.
                if ( contentChannelItem.AttributeValues == null )
                {
                    contentChannelItem.LoadAttributes( RockContext );
                }
                mergeObject = contentChannelItem;
            }

            var template = $"{{{{ mergeObject | Attribute:'{attributeKey}','Url' }}}}";
            return template.ResolveMergeFields( new Dictionary<string, object> { ["mergeObject"] = mergeObject } );
        }

        /// <summary>
        /// Cache key shape matches <see cref="Rock.Web.UI.RockBlock"/>'s built-in
        /// <c>ItemCacheKey</c> so entries are interchangeable with anything else in Rock that
        /// uses that helper.
        /// </summary>
        private string GetCacheKey( string key )
        {
            var pageOrLayout = BlockCache?.PageId != null
                ? $"Page:{BlockCache.PageId.Value}"
                : $"Layout:{BlockCache?.LayoutId ?? 0}";

            return $"Rock:{pageOrLayout}:Block:{BlockId}:ItemCache:{key}";
        }

        /// <summary>
        /// Tracks every cache key written by this block so <see cref="ClearAllCacheItems"/> can
        /// flush them when settings change.
        /// </summary>
        private void AddOutputCacheKey( string cacheKey, string cacheTags )
        {
            var trackedKeysCacheKey = GetCacheKey( CacheKey.CacheKeys );
            var trackedKeys = GetCachedItem<HashSet<string>>( trackedKeysCacheKey ) ?? new HashSet<string>();
            trackedKeys.Add( cacheKey );
            RockCache.AddOrUpdate( trackedKeysCacheKey, null, trackedKeys, TimeSpan.MaxValue, cacheTags );
        }

        private void ClearAllCacheItems()
        {
            var trackedKeysCacheKey = GetCacheKey( CacheKey.CacheKeys );
            var trackedKeys = GetCachedItem<HashSet<string>>( trackedKeysCacheKey );
            if ( trackedKeys != null )
            {
                foreach ( var key in trackedKeys )
                {
                    RockCache.Remove( key );
                }
            }

            RockCache.Remove( trackedKeysCacheKey );
        }

        private static T GetCachedItem<T>( string key ) where T : class
        {
            return RockCache.Get( key, true ) as T;
        }

        private static void SetCachedItem( string key, object value, int durationInSeconds, string cacheTags )
        {
            RockCache.AddOrUpdate( key, null, value, TimeSpan.FromSeconds( durationInSeconds ), cacheTags ?? string.Empty );
        }

        #endregion Private Methods
    }
}
