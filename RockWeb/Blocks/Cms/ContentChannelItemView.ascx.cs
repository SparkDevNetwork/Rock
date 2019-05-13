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
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Transactions;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Field.Types;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Block to display a specific content channel item.
    /// </summary>
    [DisplayName( "Content Channel Item View" )]
    [Category( "CMS" )]
    [Description( "Block to display a specific content channel item." )]

    [LavaCommandsField( "Enabled Lava Commands", description: "The Lava commands that should be enabled for this content channel item block.", required: false )]

    [ContentChannelField( "Content Channel", description: "Limits content channel items to a specific channel, or leave blank to leave unrestricted.", required: false, defaultValue: "", category: "CustomSetting" )]
    [EnumsField( "Status", description: "Include items with the following status.", enumSourceType: typeof( ContentChannelItemStatus ), required: false, defaultValue: "2", category: "CustomSetting" )]
    [TextField( "Content Channel Query Parameter", description: CONTENT_CHANNEL_QUERY_PARAMETER_DESCRIPTION, required: false, category: "CustomSetting" )]

    [CodeEditorField( "Lava Template", description: "The template to use when formatting the content channel item.", mode: CodeEditorMode.Lava, theme: CodeEditorTheme.Rock, height: 200, required: false, category: "CustomSetting", defaultValue: @"
<h1>{{ Item.Title }}</h1>
{{ Item.Content }}" )]

    [IntegerField( "Output Cache Duration", OUTPUT_CACHE_DURATION_DESCRIPTION, required: false, key: "OutputCacheDuration", category: "CustomSetting" )]
    [IntegerField( "Item Cache Duration", description: "Number of seconds to cache the content item specified by the parameter.", required: false, defaultValue: 3600, category: "CustomSetting", order: 0, key: "ItemCacheDuration" )]
    [CustomCheckboxListField( "Cache Tags", description: "Cached tags are used to link cached content so that it can be expired as a group", listSource: "", required: false, key: "CacheTags", category: "CustomSetting" )]

    [BooleanField( "Set Page Title", description: "Determines if the block should set the page title with the channel name or content item.", category: "CustomSetting" )]
    [LinkedPage( "Detail Page", description: "Page used to view a content item.", order: 1, category: "CustomSetting", key: "DetailPage" )]

    [BooleanField( "Log Interactions", category: "CustomSetting" )]
    [BooleanField( "Write Interaction Only If Individual Logged In", description: "Set to true to only write interactions for logged in users, or set to false to write interactions for both logged in and anonymous users.", category: "CustomSetting" )]

    [WorkflowTypeField( "Workflow Type", description: "The workflow type to launch when the content is viewed.", category: "CustomSetting" )]
    [BooleanField( "Launch Workflow Only If Individual Logged In", description: "Set to true to only launch a workflow for logged in users, or set to false to launch for both logged in and anonymous users.", category: "CustomSetting" )]
    [EnumField( "Launch Workflow Condition", "", enumSourceType: typeof( LaunchWorkflowCondition ), defaultValue: "1", category: "CustomSetting" )]

    [TextField( "Meta Description Attribute", required: false, category: "CustomSetting" )]
    [TextField( "Open Graph Type", required: false, category: "CustomSetting" )]
    [TextField( "Open Graph Title Attribute", required: false, category: "CustomSetting" )]
    [TextField( "Open Graph Description Attribute", required: false, category: "CustomSetting" )]
    [TextField( "Open Graph Image Attribute", required: false, category: "CustomSetting" )]
    [TextField( "Twitter Title Attribute", required: false, category: "CustomSetting" )]
    [TextField( "Twitter Description Attribute", required: false, category: "CustomSetting" )]
    [TextField( "Twitter Image Attribute", required: false, category: "CustomSetting" )]
    [TextField( "Twitter Card", required: false, defaultValue: "none", category: "CustomSetting" )]
    public partial class ContentChannelItemView : RockBlockCustomSettings
    {
        #region Block Property Constants
        private const string CONTENT_CHANNEL_QUERY_PARAMETER_DESCRIPTION = @"
Specify the URL parameter to use to determine which Content Channel Item to show, or leave blank to use whatever the first parameter is.
The type of the value will determine how the content channel item will be determined as follows:

Integer - ContentChannelItem Id
String - ContentChannelItem Slug
Guid - ContentChannelItem Guid

";
        private const string OUTPUT_CACHE_DURATION_DESCRIPTION = @"Number of seconds to cache the resolved output. Only cache the output if you are not personalizing the output based on current user, current page, or any other merge field value.";

        #endregion Block Property Constants

        #region Fields

        /// <summary>
        /// The output cache key prefix
        /// </summary>
        private const string OUTPUT_CACHE_KEY_PREFIX = "Output_";

        /// <summary>
        /// The item cache key prefix
        /// </summary>
        private const string ITEM_CACHE_KEY_PREFIX = "Item_";

        /// <summary>
        /// The pagetitle cache key prefix
        /// </summary>
        private const string PAGETITLE_CACHE_KEY_PREFIX = "PageTitle_";

        /// <summary>
        /// The cache key to store a list of CacheKeys that have been used by this block
        /// </summary>
        private const string CACHEKEYS_CACHE_KEY = "CacheKeys";

        /// <summary>
        ///
        /// </summary>
        private enum LaunchWorkflowCondition
        {
            Always = 0,
            OncePerPersonPerContentChannelItem = 1,
            OncePerPerson = 2
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            // Create a hidden button that will show the view if they cancel the Settings modal
            Button btnTrigger = new Button();
            btnTrigger.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            btnTrigger.ID = "rock-config-cancel-trigger";
            btnTrigger.Click += btnTrigger_Click;
            btnTrigger.Style[HtmlTextWriterStyle.Display] = "none";
            pnlSettings.Controls.Add( btnTrigger );

            AsyncPostBackTrigger trigger = new AsyncPostBackTrigger();
            trigger.ControlID = "rock-config-cancel-trigger";
            trigger.EventName = "Click";
            upnlContent.Triggers.Add( trigger );

            base.OnInit( e );
        }

        /// <summary>
        /// Handles the Click event of the btnTrigger control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnTrigger_Click( object sender, EventArgs e )
        {
            mdSettings.Hide();
            pnlSettings.Visible = false;

            ShowView();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowView();
            }
        }

        /// <summary>
        /// Shows or hides the workflow settings controls based on the selection in the workflowpicker.
        /// </summary>
        protected void ShowHideControls()
        {
            if ( wtpWorkflowType.SelectedValue == "0" )
            {
                cbLaunchWorkflowOnlyIfIndividualLoggedIn.Visible = false;
                ddlLaunchWorkflowCondition.Visible = false;
            }
            else
            {
                cbLaunchWorkflowOnlyIfIndividualLoggedIn.Visible = true;
                ddlLaunchWorkflowCondition.Visible = true;
            }
        }

        #endregion Base Control Methods

        #region Settings

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            pnlSettings.Visible = true;
            ddlContentChannel.Items.Clear();
            ddlContentChannel.Items.Add( new ListItem() );
            foreach ( var contentChannel in ContentChannelCache.All().OrderBy( a => a.Name ) )
            {
                ddlContentChannel.Items.Add( new ListItem( contentChannel.Name, contentChannel.Guid.ToString() ) );
            }

            ddlLaunchWorkflowCondition.Items.Clear();
            foreach ( LaunchWorkflowCondition launchWorkflowCondition in Enum.GetValues( typeof( LaunchWorkflowCondition ) ) )
            {
                ddlLaunchWorkflowCondition.Items.Add( new ListItem( launchWorkflowCondition.ConvertToString( true ), launchWorkflowCondition.ConvertToInt().ToString() ) );
            }

            var channelGuid = this.GetAttributeValue( "ContentChannel" ).AsGuidOrNull();
            ddlContentChannel.SetValue( channelGuid );
            UpdateSocialMediaDropdowns( channelGuid );

            cblStatus.BindToEnum<ContentChannelItemStatus>();
            foreach ( string status in GetAttributeValue( "Status" ).SplitDelimitedValues() )
            {
                var li = cblStatus.Items.FindByValue( status );
                if ( li != null )
                {
                    li.Selected = true;
                }
            }

            var ppFieldType = new PageReferenceFieldType();
            ppFieldType.SetEditValue( ppDetailPage, null, GetAttributeValue( "DetailPage" ) );
            tbContentChannelQueryParameter.Text = this.GetAttributeValue( "ContentChannelQueryParameter" );
            ceLavaTemplate.Text = this.GetAttributeValue( "LavaTemplate" );
            nbOutputCacheDuration.Text = this.GetAttributeValue( "OutputCacheDuration" );
            nbItemCacheDuration.Text = this.GetAttributeValue( "ItemCacheDuration" );

            DefinedValueService definedValueService = new DefinedValueService( new RockContext() );
            cblCacheTags.DataSource = definedValueService.GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.CACHE_TAGS.AsGuid() ).Select( v => v.Value ).ToList();
            cblCacheTags.DataBind();
            string[] selectedCacheTags = this.GetAttributeValue( "CacheTags" ).SplitDelimitedValues();
            foreach ( ListItem cacheTag in cblCacheTags.Items )
            {
                cacheTag.Selected = selectedCacheTags.Contains( cacheTag.Value );
            }

            cbSetPageTitle.Checked = this.GetAttributeValue( "SetPageTitle" ).AsBoolean();

            if ( this.GetAttributeValue( "LogInteractions" ).AsBoolean() )
            {
                cbLogInteractions.Checked = true;
                cbWriteInteractionOnlyIfIndividualLoggedIn.Visible = true;
                cbWriteInteractionOnlyIfIndividualLoggedIn.Checked = this.GetAttributeValue( "WriteInteractionOnlyIfIndividualLoggedIn" ).AsBoolean();
            }
            else
            {
                cbLogInteractions.Checked = false;
                cbWriteInteractionOnlyIfIndividualLoggedIn.Visible = false;
                cbWriteInteractionOnlyIfIndividualLoggedIn.Checked = false;
            }

            var rockContext = new RockContext();

            // Workflow
            Guid? workflowTypeGuid = this.GetAttributeValue( "WorkflowType" ).AsGuidOrNull();
            if ( workflowTypeGuid.HasValue )
            {
                wtpWorkflowType.SetValue( new WorkflowTypeService( rockContext ).GetNoTracking( workflowTypeGuid.Value ) );
            }
            else
            {
                wtpWorkflowType.SetValue( null );
            }

            ShowHideControls();

            cbLaunchWorkflowOnlyIfIndividualLoggedIn.Checked = this.GetAttributeValue( "LaunchWorkflowOnlyIfIndividualLoggedIn" ).AsBoolean();
            ddlLaunchWorkflowCondition.SetValue( this.GetAttributeValue( "LaunchWorkflowCondition" ) );

            // Social Media
            ddlMetaDescriptionAttribute.SetValue( this.GetAttributeValue( "MetaDescriptionAttribute" ) );
            ddlOpenGraphType.SetValue( this.GetAttributeValue( "OpenGraphType" ) );
            ddlOpenGraphTitleAttribute.SetValue( this.GetAttributeValue( "OpenGraphTitleAttribute" ) );
            ddlOpenGraphDescriptionAttribute.SetValue( this.GetAttributeValue( "OpenGraphDescriptionAttribute" ) );
            ddlOpenGraphImageAttribute.SetValue( this.GetAttributeValue( "OpenGraphImageAttribute" ) );

            ddlTwitterTitleAttribute.SetValue( this.GetAttributeValue( "TwitterTitleAttribute" ) );
            ddlTwitterDescriptionAttribute.SetValue( this.GetAttributeValue( "TwitterDescriptionAttribute" ) );
            ddlTwitterImageAttribute.SetValue( this.GetAttributeValue( "TwitterImageAttribute" ) );
            ddlTwitterCard.SetValue( this.GetAttributeValue( "TwitterCard" ) );

            mdSettings.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdSettings_SaveClick( object sender, EventArgs e )
        {
            this.SetAttributeValue( "ContentChannel", ddlContentChannel.SelectedValue );
            this.SetAttributeValue( "Status", cblStatus.SelectedValuesAsInt.AsDelimited( "," ) );
            var ppFieldType = new PageReferenceFieldType();
            this.SetAttributeValue( "DetailPage", ppFieldType.GetEditValue( ppDetailPage, null ) );
            this.SetAttributeValue( "ContentChannelQueryParameter", tbContentChannelQueryParameter.Text );
            this.SetAttributeValue( "LavaTemplate", ceLavaTemplate.Text );
            this.SetAttributeValue( "OutputCacheDuration", nbOutputCacheDuration.Text );
            this.SetAttributeValue( "ItemCacheDuration", nbItemCacheDuration.Text );
            this.SetAttributeValue( "CacheTags", cblCacheTags.SelectedValues.AsDelimited( "," ) );
            this.SetAttributeValue( "SetPageTitle", cbSetPageTitle.Checked.ToString() );
            this.SetAttributeValue( "LogInteractions", cbLogInteractions.Checked.ToString() );
            this.SetAttributeValue( "WriteInteractionOnlyIfIndividualLoggedIn", cbWriteInteractionOnlyIfIndividualLoggedIn.Checked.ToString() );
            int? selectedWorkflowTypeId = wtpWorkflowType.SelectedValueAsId();
            Guid? selectedWorkflowTypeGuid = null;
            if ( selectedWorkflowTypeId.HasValue )
            {
                selectedWorkflowTypeGuid = WorkflowTypeCache.Get( selectedWorkflowTypeId.Value ).Guid;
            }

            this.SetAttributeValue( "WorkflowType", selectedWorkflowTypeGuid.ToString() );
            this.SetAttributeValue( "LaunchWorkflowOnlyIfIndividualLoggedIn", cbLaunchWorkflowOnlyIfIndividualLoggedIn.Checked.ToString() );
            this.SetAttributeValue( "LaunchWorkflowCondition", ddlLaunchWorkflowCondition.SelectedValue );
            this.SetAttributeValue( "MetaDescriptionAttribute", ddlMetaDescriptionAttribute.SelectedValue );
            this.SetAttributeValue( "OpenGraphType", ddlOpenGraphType.SelectedValue );
            this.SetAttributeValue( "OpenGraphTitleAttribute", ddlOpenGraphTitleAttribute.SelectedValue );
            this.SetAttributeValue( "OpenGraphDescriptionAttribute", ddlOpenGraphDescriptionAttribute.SelectedValue );
            this.SetAttributeValue( "OpenGraphImageAttribute", ddlOpenGraphImageAttribute.SelectedValue );
            this.SetAttributeValue( "TwitterTitleAttribute", ddlTwitterTitleAttribute.SelectedValue );
            this.SetAttributeValue( "TwitterDescriptionAttribute", ddlTwitterDescriptionAttribute.SelectedValue );
            this.SetAttributeValue( "TwitterImageAttribute", ddlTwitterImageAttribute.SelectedValue );
            this.SetAttributeValue( "TwitterCard", ddlTwitterCard.SelectedValue );

            SaveAttributeValues();

            var cacheKeys = GetCacheItem( CACHEKEYS_CACHE_KEY ) as HashSet<string>;
            if ( cacheKeys != null )
            {
                foreach ( var cacheKey in cacheKeys )
                {
                    RemoveCacheItem( cacheKey );
                }
            }

            RemoveCacheItem( CACHEKEYS_CACHE_KEY );

            mdSettings.Hide();
            pnlSettings.Visible = false;

            // reload the page to make sure we have a clean load
            NavigateToCurrentPageReference();
        }

        #endregion Settings

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlContentChannel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlContentChannel_SelectedIndexChanged( object sender, EventArgs e )
        {
            var channelGuid = ddlContentChannel.SelectedValue.AsGuidOrNull();
            UpdateSocialMediaDropdowns( channelGuid );
        }

        /// <summary>
        /// Handles the SelectItem event of the wtpWorkflowType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void wtpWorkflowType_SelectItem( object sender, EventArgs e )
        {
            ShowHideControls();
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Shows the no data found.
        /// </summary>
        private void ShowNoDataFound()
        {
            if ( IsUserAuthorized( Rock.Security.Authorization.ADMINISTRATE ) )
            {
                nbAlert.Text = "404 - No Content. If you did not have Administrate permissions on this block, you would have gotten a real 404 page.";
            }
            else
            {
                this.Response.StatusCode = 404;
            }
        }

        /// <summary>
        /// Shows the view.
        /// </summary>
        private void ShowView()
        {
            int? outputCacheDuration = GetAttributeValue( "OutputCacheDuration" ).AsIntegerOrNull();

            string outputContents = null;
            string pageTitle = null;

            var contentChannelItemParameterValue = GetContentChannelItemParameterValue();
            if ( string.IsNullOrEmpty( contentChannelItemParameterValue ) )
            {
                // No item specified, so don't show anything
                ShowNoDataFound();
                return;
            }

            string outputCacheKey = OUTPUT_CACHE_KEY_PREFIX + contentChannelItemParameterValue;
            string pageTitleCacheKey = PAGETITLE_CACHE_KEY_PREFIX + contentChannelItemParameterValue;

            if ( outputCacheDuration.HasValue && outputCacheDuration.Value > 0 )
            {
                outputContents = GetCacheItem( outputCacheKey ) as string;
                pageTitle = GetCacheItem( pageTitleCacheKey ) as string;
            }

            bool setPageTitle = GetAttributeValue( "SetPageTitle" ).AsBoolean();

            if ( outputContents == null )
            {
                ContentChannelItem contentChannelItem = GetContentChannelItem( contentChannelItemParameterValue );

                if ( contentChannelItem == null )
                {
                    ShowNoDataFound();
                    return;
                }

                if ( contentChannelItem.ContentChannel.RequiresApproval )
                {
                    var statuses = new List<ContentChannelItemStatus>();
                    foreach ( var status in ( GetAttributeValue( "Status" ) ?? "2" ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                    {
                        var statusEnum = status.ConvertToEnumOrNull<ContentChannelItemStatus>();
                        if ( statusEnum != null )
                        {
                            statuses.Add( statusEnum.Value );
                        }
                    }

                    if ( !statuses.Contains( contentChannelItem.Status ) )
                    {
                        ShowNoDataFound();
                        return;
                    }
                }

                // if a Channel was specified, verify that the ChannelItem is part of the channel
                var channelGuid = this.GetAttributeValue( "ContentChannel" ).AsGuidOrNull();
                if ( channelGuid.HasValue )
                {
                    var channel = ContentChannelCache.Get( channelGuid.Value );
                    if ( channel != null )
                    {
                        if ( contentChannelItem.ContentChannelId != channel.Id )
                        {
                            ShowNoDataFound();
                            return;
                        }
                    }
                }

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
                mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );
                mergeFields.Add( "Item", contentChannelItem );
                int detailPage = 0;
                var page = PageCache.Get( GetAttributeValue( "DetailPage" ) );
                if ( page != null )
                {
                    detailPage = page.Id;
                }

                mergeFields.Add( "DetailPage", detailPage );

                string metaDescriptionValue = GetMetaValueFromAttribute( this.GetAttributeValue( "MetaDescriptionAttribute" ), contentChannelItem );

                if ( !string.IsNullOrWhiteSpace( metaDescriptionValue ) )
                {
                    // remove default meta description
                    RockPage.Header.Description = metaDescriptionValue.SanitizeHtml( true );
                }

                AddHtmlMeta( "og:type", this.GetAttributeValue( "OpenGraphType" ) );
                AddHtmlMeta( "og:title", GetMetaValueFromAttribute( this.GetAttributeValue( "OpenGraphTitleAttribute" ), contentChannelItem ) ?? contentChannelItem.Title );
                AddHtmlMeta( "og:description", GetMetaValueFromAttribute( this.GetAttributeValue( "OpenGraphDescriptionAttribute" ), contentChannelItem ) );
                AddHtmlMeta( "og:image", GetMetaValueFromAttribute( this.GetAttributeValue( "OpenGraphImageAttribute" ), contentChannelItem ) );
                AddHtmlMeta( "twitter:title", GetMetaValueFromAttribute( this.GetAttributeValue( "TwitterTitleAttribute" ), contentChannelItem ) ?? contentChannelItem.Title );
                AddHtmlMeta( "twitter:description", GetMetaValueFromAttribute( this.GetAttributeValue( "TwitterDescriptionAttribute" ), contentChannelItem ) );
                AddHtmlMeta( "twitter:image", GetMetaValueFromAttribute( this.GetAttributeValue( "TwitterImageAttribute" ), contentChannelItem ) );
                var twitterCard = this.GetAttributeValue( "TwitterCard" );
                if ( twitterCard.IsNotNullOrWhiteSpace() && twitterCard != "none" )
                {
                    AddHtmlMeta( "twitter:card", twitterCard );
                }
                string lavaTemplate = this.GetAttributeValue( "LavaTemplate" );
                string enabledLavaCommands = this.GetAttributeValue( "EnabledLavaCommands" );
                outputContents = lavaTemplate.ResolveMergeFields( mergeFields, enabledLavaCommands );

                if ( setPageTitle )
                {
                    pageTitle = contentChannelItem.Title;
                }

                if ( outputCacheDuration.HasValue && outputCacheDuration.Value > 0 )
                {
                    string cacheTags = GetAttributeValue( "CacheTags" ) ?? string.Empty;
                    var cacheKeys = GetCacheItem( CACHEKEYS_CACHE_KEY ) as HashSet<string> ?? new HashSet<string>();
                    cacheKeys.Add( outputCacheKey );
                    cacheKeys.Add( pageTitleCacheKey );
                    AddCacheItem( CACHEKEYS_CACHE_KEY, cacheKeys, TimeSpan.MaxValue, cacheTags );
                    AddCacheItem( outputCacheKey, outputContents, outputCacheDuration.Value, cacheTags );

                    if ( pageTitle != null )
                    {
                        AddCacheItem( pageTitleCacheKey, pageTitle, outputCacheDuration.Value, cacheTags );
                    }
                }
            }

            lContentOutput.Text = outputContents;

            if ( setPageTitle && pageTitle != null )
            {
                RockPage.PageTitle = pageTitle;
                RockPage.BrowserTitle = string.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
                RockPage.Header.Title = string.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );

                var pageBreadCrumb = RockPage.PageReference.BreadCrumbs.FirstOrDefault();
                if ( pageBreadCrumb != null )
                {
                    pageBreadCrumb.Name = RockPage.PageTitle;
                }
            }

            LaunchWorkflow();

            LaunchInteraction();
        }

        /// <summary>
        /// Gets the content channel item using the first page parameter or ContentChannelQueryParameter
        /// </summary>
        /// <returns></returns>
        private ContentChannelItem GetContentChannelItem( string contentChannelItemKey )
        {
            int? itemCacheDuration = GetAttributeValue( "ItemCacheDuration" ).AsIntegerOrNull();
            Guid? contentChannelGuid = GetAttributeValue( "ContentChannel" ).AsGuidOrNull();

            ContentChannelItem contentChannelItem = null;

            if ( string.IsNullOrEmpty( contentChannelItemKey ) )
            {
                // nothing specified, so don't show anything
                return null;
            }

            string itemCacheKey = ITEM_CACHE_KEY_PREFIX + contentChannelGuid + "_" + contentChannelItemKey;

            if ( itemCacheDuration.HasValue && itemCacheDuration.Value > 0 )
            {
                contentChannelItem = GetCacheItem( itemCacheKey ) as ContentChannelItem;
                if ( contentChannelItem != null )
                {
                    return contentChannelItem;
                }
            }

            // look up the ContentChannelItem from either the Id, Guid, or Slug depending on the datatype of the ContentChannelQueryParameter value
            int? contentChannelItemId = contentChannelItemKey.AsIntegerOrNull();
            Guid? contentChannelItemGuid = contentChannelItemKey.AsGuidOrNull();

            var rockContext = new RockContext();
            if ( contentChannelItemId.HasValue )
            {
                contentChannelItem = new ContentChannelItemService( rockContext ).Get( contentChannelItemId.Value );
            }
            else if ( contentChannelItemGuid.HasValue )
            {
                contentChannelItem = new ContentChannelItemService( rockContext ).Get( contentChannelItemGuid.Value );
            }
            else
            {
                var contentChannelQuery = new ContentChannelItemService( rockContext ).Queryable();
                if ( contentChannelGuid.HasValue )
                {

                    contentChannelQuery = contentChannelQuery.Where( c => c.ContentChannel.Guid == contentChannelGuid );
                }

                contentChannelItem = contentChannelQuery
                    .Where( a => a.ContentChannelItemSlugs.Any( s => s.Slug == contentChannelItemKey ) )
                    .FirstOrDefault();
            }

            if ( contentChannelItem != null && itemCacheDuration.HasValue && itemCacheDuration.Value > 0 )
            {
                string cacheTags = GetAttributeValue( "CacheTags" ) ?? string.Empty;
                var cacheKeys = GetCacheItem( CACHEKEYS_CACHE_KEY ) as HashSet<string> ?? new HashSet<string>();
                cacheKeys.Add( itemCacheKey );
                AddCacheItem( CACHEKEYS_CACHE_KEY, cacheKeys, TimeSpan.MaxValue, cacheTags );
                AddCacheItem( itemCacheKey, contentChannelItem, itemCacheDuration.Value, cacheTags );
            }

            return contentChannelItem;
        }

        /// <summary>
        /// Gets the content channel item parameter using the first page parameter or ContentChannelQueryParameter
        /// </summary>
        /// <returns></returns>
        private string GetContentChannelItemParameterValue()
        {
            string contentChannelItemKey = null;

            // Determine the ContentChannelItem from the ContentChannelQueryParameter or the first parameter
            string contentChannelQueryParameter = this.GetAttributeValue( "ContentChannelQueryParameter" );
            if ( !string.IsNullOrEmpty( contentChannelQueryParameter ) )
            {
                contentChannelItemKey = this.PageParameter( contentChannelQueryParameter );
            }
            else
            {
                var currentRoute = ( ( System.Web.Routing.Route ) Page.RouteData.Route );
                // if this is the standard "page/{PageId}" route, don't grab the Item from the route since it would just be the pageId
                if ( currentRoute == null || currentRoute.Url != "page/{PageId}" )
                {
                    // if no specific Parameter was specified, get whatever the last Parameter in the Route is
                    var key = this.Page.RouteData.Values.Keys.LastOrDefault();
                    if ( key.IsNotNullOrWhiteSpace() )
                    {
                        contentChannelItemKey = this.Page.RouteData.Values[key].ToString();
                    }
                }
                else if ( Request.QueryString.HasKeys() )
                {
                    contentChannelItemKey = this.PageParameter( Request.QueryString.Keys[0] );
                }
            }

            return contentChannelItemKey;
        }

        /// <summary>
        /// Launches the interaction if configured
        /// </summary>
        private void LaunchInteraction()
        {
            bool logInteractions = this.GetAttributeValue( "LogInteractions" ).AsBoolean();
            if ( !logInteractions )
            {
                return;
            }

            bool writeInteractionOnlyIfIndividualLoggedIn = this.GetAttributeValue( "WriteInteractionOnlyIfIndividualLoggedIn" ).AsBoolean();
            if ( writeInteractionOnlyIfIndividualLoggedIn && this.CurrentPerson == null )
            {
                // don't log interaction if WriteInteractionOnlyIfIndividualLoggedIn = true and nobody is logged in
                return;
            }

            var contentChannelItem = GetContentChannelItem( GetContentChannelItemParameterValue() );

            var interactionTransaction = new InteractionTransaction(
                DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CONTENTCHANNEL.AsGuid() ),
                contentChannelItem.ContentChannel,
                contentChannelItem );

            interactionTransaction.InteractionSummary = contentChannelItem.Title;

            interactionTransaction.Enqueue();
        }

        /// <summary>
        /// Launches the workflow if configured
        /// </summary>
        private void LaunchWorkflow()
        {
            // Check to see if a workflow should be launched when viewed
            WorkflowTypeCache workflowType = null;
            Guid? workflowTypeGuid = GetAttributeValue( "WorkflowType" ).AsGuidOrNull();
            if ( !workflowTypeGuid.HasValue )
            {
                return;
            }

            workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value );

            if ( workflowType == null || ( workflowType.IsActive != true ) )
            {
                return;
            }

            bool launchWorkflowOnlyIfIndividualLoggedIn = this.GetAttributeValue( "LaunchWorkflowOnlyIfIndividualLoggedIn" ).AsBoolean();
            if ( launchWorkflowOnlyIfIndividualLoggedIn && this.CurrentPerson == null )
            {
                // don't launch a workflow if LaunchWorkflowOnlyIfIndividualLoggedIn = true and nobody is logged in
                return;
            }

            var launchWorkflowCondition = this.GetAttributeValue( "LaunchWorkflowCondition" ).ConvertToEnum<LaunchWorkflowCondition>();

            if ( launchWorkflowCondition != LaunchWorkflowCondition.Always && this.CurrentPerson == null )
            {
                // don't launch a workflow if LaunchWorkflowOnlyIfIndividualLoggedIn = true and nobody is logged in
                return;
            }

            var contentChannelItem = GetContentChannelItem( this.GetContentChannelItemParameterValue() );

            // use BlockUserPreference to store whether the Workflow was already launched depending on LaunchWorkflowCondition
            string alreadyLaunchedKey = null;
            if ( launchWorkflowCondition == LaunchWorkflowCondition.OncePerPersonPerContentChannelItem )
            {
                alreadyLaunchedKey = string.Format( "WorkflowLaunched_{0}_{1}", workflowType.Id, contentChannelItem.Id );
            }
            else if ( launchWorkflowCondition == LaunchWorkflowCondition.OncePerPerson )
            {
                alreadyLaunchedKey = string.Format( "WorkflowLaunched_{0}", workflowType.Id );
            }

            if ( alreadyLaunchedKey != null )
            {
                var alreadyLaunched = this.GetBlockUserPreference( alreadyLaunchedKey ).AsBooleanOrNull();
                if ( alreadyLaunched == true )
                {
                    return;
                }
                else
                {
                    this.SetBlockUserPreference( alreadyLaunchedKey, true.ToString(), true );
                }
            }

            var workflowAttributeValues = new Dictionary<string, string>();
            workflowAttributeValues.Add( "ContentChannelItem", contentChannelItem.Guid.ToString() );

            LaunchWorkflowTransaction launchWorkflowTransaction;
            if ( this.CurrentPersonId.HasValue )
            {
                workflowAttributeValues.Add( "Person", this.CurrentPerson.Guid.ToString() );
                launchWorkflowTransaction = new Rock.Transactions.LaunchWorkflowTransaction<Person>( workflowType.Id, null, this.CurrentPersonId.Value );
            }
            else
            {
                launchWorkflowTransaction = new Rock.Transactions.LaunchWorkflowTransaction( workflowType.Id, null );
            }

            if ( workflowAttributeValues != null )
            {
                launchWorkflowTransaction.WorkflowAttributeValues = workflowAttributeValues;
            }

            launchWorkflowTransaction.InitiatorPersonAliasId = this.CurrentPersonAliasId;
            launchWorkflowTransaction.Enqueue();
        }

        /// <summary>
        /// Adds the HTML meta from attribute value.
        /// </summary>
        /// <param name="metaName">Name of the meta.</param>
        /// <param name="metaContent">Content of the meta.</param>
        private void AddHtmlMeta( string metaName, string metaContent )
        {
            if ( string.IsNullOrEmpty( metaContent ) )
            {
                return;
            }

            RockPage.Header.Controls.Add( new HtmlMeta
            {
                Name = metaName,
                Content = metaContent
            } );
        }

        /// <summary>
        /// Gets the meta value from attribute.
        /// </summary>
        /// <param name="attributeComputedKey">The attribute computed key.</param>
        /// <param name="contentChannelItem">The content channel item.</param>
        /// <returns>
        /// a string value
        /// </returns>
        private string GetMetaValueFromAttribute( string attributeComputedKey, ContentChannelItem contentChannelItem )
        {
            if ( string.IsNullOrEmpty( attributeComputedKey ) )
            {
                return null;
            }

            string attributeEntityType = attributeComputedKey.Split( '^' )[0].ToString() ?? "C";
            string attributeKey = attributeComputedKey.Split( '^' )[1].ToString() ?? string.Empty;

            string attributeValue = string.Empty;

            object mergeObject;

            if ( attributeEntityType == "C" )
            {
                mergeObject = ContentChannelCache.Get( contentChannelItem.ContentChannelId );
            }
            else
            {
                mergeObject = contentChannelItem;
            }

            // use Lava to get the Attribute value formatted for the MetaValue, and specify the Url param in case the Attribute supports rendering the value as a Url (for example, Image)
            string metaTemplate = string.Format( "{{{{ mergeObject | Attribute:'{0}':'Url' }}}}", attributeKey );

            string resolvedValue = metaTemplate.ResolveMergeFields( new Dictionary<string, object> { { "mergeObject", mergeObject } } );

            return resolvedValue;
        }

        /// <summary>
        /// Updates the social media dropdowns.
        /// </summary>
        /// <param name="channelGuid">The channel unique identifier.</param>
        private void UpdateSocialMediaDropdowns( Guid? channelGuid )
        {
            List<AttributeCache> channelAttributes = new List<AttributeCache>();
            List<AttributeCache> itemAttributes = new List<AttributeCache>();

            if ( channelGuid.HasValue )
            {
                var rockContext = new RockContext();
                var channel = new ContentChannelService( rockContext ).GetNoTracking( channelGuid.Value );

                // add channel attributes
                channel.LoadAttributes();
                channelAttributes = channel.Attributes.Select( a => a.Value ).ToList();

                // add item attributes
                AttributeService attributeService = new AttributeService( rockContext );
                itemAttributes = attributeService.GetByEntityTypeId( new ContentChannelItem().TypeId, false ).AsQueryable()
                                        .Where( a => (
                                                a.EntityTypeQualifierColumn.Equals( "ContentChannelTypeId", StringComparison.OrdinalIgnoreCase ) &&
                                                a.EntityTypeQualifierValue.Equals( channel.ContentChannelTypeId.ToString() )
                                            ) || (
                                                a.EntityTypeQualifierColumn.Equals( "ContentChannelId", StringComparison.OrdinalIgnoreCase ) &&
                                                a.EntityTypeQualifierValue.Equals( channel.Id.ToString() )
                                            ) )
                                        .OrderByDescending( a => a.EntityTypeQualifierColumn )
                                        .ThenBy( a => a.Order )
                                        .ToAttributeCacheList();
            }

            RockDropDownList[] attributeDropDowns = new RockDropDownList[]
            {
                ddlMetaDescriptionAttribute,
                ddlOpenGraphTitleAttribute,
                ddlOpenGraphDescriptionAttribute,
                ddlOpenGraphImageAttribute,
                ddlTwitterTitleAttribute,
                ddlTwitterDescriptionAttribute,
                ddlTwitterImageAttribute,
                ddlMetaDescriptionAttribute
            };

            RockDropDownList[] attributeDropDownsImage = new RockDropDownList[]
            {
                ddlOpenGraphImageAttribute,
                ddlTwitterImageAttribute,
            };

            foreach ( var attributeDropDown in attributeDropDowns )
            {
                attributeDropDown.Items.Clear();
                attributeDropDown.Items.Add( new ListItem() );
                foreach ( var attribute in channelAttributes )
                {
                    string computedKey = "C^" + attribute.Key;
                    if ( attributeDropDownsImage.Contains( attributeDropDown ) )
                    {
                        if ( attribute.FieldType.Name == "Image" )
                        {
                            attributeDropDown.Items.Add( new ListItem( "Channel: " + attribute.Name, computedKey ) );
                        }
                    }
                    else
                    {
                        attributeDropDown.Items.Add( new ListItem( "Channel: " + attribute.Name, computedKey ) );
                    }
                }

                // get all the possible Item attributes for items in this Content Channel and add those as options too
                foreach ( var attribute in itemAttributes.DistinctBy( a => a.Key ).ToList() )
                {
                    string computedKey = "I^" + attribute.Key;
                    if ( attributeDropDownsImage.Contains( attributeDropDown ) )
                    {
                        if ( attribute.FieldType.Name == "Image" )
                        {
                            attributeDropDown.Items.Add( new ListItem( "Item: " + attribute.Name, computedKey ) );
                        }
                    }
                    else
                    {
                        attributeDropDown.Items.Add( new ListItem( "Item: " + attribute.Name, computedKey ) );
                    }
                }
            }
        }

        #endregion Methods

        /// <summary>
        /// Handles the CheckedChanged event of the cbLogInteractions control.
        /// If log interactions is not enabled then don't allow write interaction setting.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbLogInteractions_CheckedChanged( object sender, EventArgs e )
        {
            if ( cbLogInteractions.Checked )
            {
                cbWriteInteractionOnlyIfIndividualLoggedIn.Visible = true;
                cbWriteInteractionOnlyIfIndividualLoggedIn.Checked = true;
            }
            else
            {
                cbWriteInteractionOnlyIfIndividualLoggedIn.Visible = false;
                cbWriteInteractionOnlyIfIndividualLoggedIn.Checked = false;
            }
        }
    }
}