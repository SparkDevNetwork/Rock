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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Caching;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using DotLiquid;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Reporting;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Block to display dynamic content channel items
    /// </summary>
    [DisplayName( "Content Channel View" )]
    [Category( "CMS" )]
    [Description( "Block to display dynamic content channel items." )]

    // Block Properties
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this content channel block.", false, order: 0 )]
    [LinkedPage( "Detail Page", "The page to navigate to for details.", false, "", "", 1 )]
    [BooleanField( "Enable Legacy Global Attribute Lava", "This should only be enabled if your lava is using legacy Global Attributes. Enabling this option, will negatively affect the performance of this block.", false, "", 2, "SupportLegacy" )]

    // Custom Settings
    [ContentChannelField( "Channel", "The channel to display items from.", false, "", "CustomSetting" )]
    [EnumsField( "Status", "Include items with the following status.", typeof( ContentChannelItemStatus ), false, "2", "CustomSetting" )]
    [CodeEditorField( "Template", "The template to use when formatting the list of items.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 600, false, @"", "CustomSetting" )]
    [IntegerField( "Count", "The maximum number of items to display.", false, 5, "CustomSetting" )]
    [IntegerField( "Item Cache Duration", "Number of seconds to cache the content items returned by the selected filter.", false, 3600, "CustomSetting", 0, "CacheDuration" )]
    [IntegerField( "Output Cache Duration", "Number of seconds to cache the resolved output. Only cache the output if you are not personalizing the output based on current user, current page, or any other merge field value.", false, 0, "CustomSetting", 0, "OutputCacheDuration" )]
    [CustomCheckboxListField( "Cache Tags", "Cached tags are used to link cached content so that it can be expired as a group", listSource: "", required: false, key: "CacheTags", category: "CustomSetting" )]
    [IntegerField( "Filter Id", "The data filter that is used to filter items", false, 0, "CustomSetting" )]
    [BooleanField( "Query Parameter Filtering", "Determines if block should evaluate the query string parameters for additional filter criteria.", false, "CustomSetting" )]
    [TextField( "Order", "The specifics of how items should be ordered. This value is set through configuration and should not be modified here.", false, "", "CustomSetting" )]
    [BooleanField( "Merge Content", "Should the content data and attribute values be merged using the Lava template engine.", false, "CustomSetting" )]
    [BooleanField( "Set Page Title", "Determines if the block should set the page title with the channel name or content item.", false, "CustomSetting" )]
    [BooleanField( "Rss Autodiscover", "Determines if a RSS autodiscover link should be added to the page head.", false, "CustomSetting" )]
    [TextField( "Meta Description Attribute", "Attribute to use for storing the description attribute.", false, "", "CustomSetting" )]
    [TextField( "Meta Image Attribute", "Attribute to use for storing the image attribute.", false, "", "CustomSetting" )]

    public partial class ContentChannelView : RockBlockCustomSettings
    {
        #region Fields

        private readonly string ITEM_TYPE_NAME = "Rock.Model.ContentChannelItem";
        private readonly string CONTENT_CACHE_KEY = "Content";
        private readonly string TEMPLATE_CACHE_KEY = "Template";
        private readonly string OUTPUT_CACHE_KEY = "Output";

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the duration of the item cache.
        /// </summary>
        /// <value>
        /// The duration of the item cache.
        /// </value>
        public int? ItemCacheDuration { get; set; }

        /// <summary>
        /// Gets or sets the duration of the output cache.
        /// </summary>
        /// <value>
        /// The duration of the output cache.
        /// </value>
        public int? OutputCacheDuration { get; set; }

        /// <summary>
        /// Gets or sets the channel unique identifier.
        /// </summary>
        /// <value>
        /// The channel unique identifier.
        /// </value>
        public Guid? ChannelGuid { get; set; }

        /// <summary>
        /// Gets the settings tool tip.
        /// </summary>
        /// <value>
        /// The settings tool tip.
        /// </value>
        public override string SettingsToolTip
        {
            get
            {
                return "Edit Criteria";
            }
        }

        #endregion Properties

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            ItemCacheDuration = GetAttributeValue( "CacheDuration" ).AsIntegerOrNull();
            OutputCacheDuration = GetAttributeValue( "OutputCacheDuration" ).AsIntegerOrNull();

            this.BlockUpdated += ContentDynamic_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            Button btnTrigger = new Button();
            btnTrigger.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            btnTrigger.ID = "rock-config-cancel-trigger";
            btnTrigger.Style[HtmlTextWriterStyle.Display] = "none";
            btnTrigger.Click += btnTrigger_Click;
            pnlEditModal.Controls.Add( btnTrigger );

            AsyncPostBackTrigger trigger = new AsyncPostBackTrigger();
            trigger.ControlID = "rock-config-cancel-trigger";
            trigger.EventName = "Click";
            upnlContent.Triggers.Add( trigger );
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
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            ChannelGuid = ViewState["ChannelGuid"] as Guid?;

            var rockContext = new RockContext();

            var channel = new ContentChannelService( rockContext ).Queryable( "ContentChannelType" )
                .FirstOrDefault( c => c.Guid.Equals( ChannelGuid.Value ) );
            if ( channel != null )
            {
                CreateFilterControl( channel, DataViewFilter.FromJson( ViewState["DataViewFilter"].ToString() ), false, rockContext );
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["ChannelGuid"] = ChannelGuid;
            ViewState["DataViewFilter"] = ReportingHelper.GetFilterFromControls(phFilters).ToJson();

            return base.SaveViewState();
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the block for when settings are changed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void ContentDynamic_BlockUpdated( object sender, EventArgs e )
        {
            RemoveCacheItem( CONTENT_CACHE_KEY );
            RemoveCacheItem( TEMPLATE_CACHE_KEY );
            // When our cache supports regions, we can call ClearRegion to clear all the output pages.
            RemoveCacheItem( OUTPUT_CACHE_KEY );
            
            ShowView();
        }

        /// <summary>
        /// Handles the Click event of the btnTrigger control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void btnTrigger_Click( object sender, EventArgs e )
        {
            mdEdit.Hide();
            pnlEditModal.Visible = false;

            ShowView();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlChannel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlChannel_SelectedIndexChanged( object sender, EventArgs e )
        {
            ChannelGuid = ddlChannel.SelectedValue.AsGuidOrNull();
            ShowEdit();
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var dataViewFilter = ReportingHelper.GetFilterFromControls( phFilters );

            // update Guids since we are creating a new dataFilter and children and deleting the old one
            SetNewDataFilterGuids( dataViewFilter );

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !dataViewFilter.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            var rockContext = new RockContext();
            DataViewFilterService dataViewFilterService = new DataViewFilterService( rockContext );

            int? dataViewFilterId = hfDataFilterId.Value.AsIntegerOrNull();
            if ( dataViewFilterId.HasValue )
            {
                var oldDataViewFilter = dataViewFilterService.Get( dataViewFilterId.Value );
                DeleteDataViewFilter( oldDataViewFilter, dataViewFilterService );
            }

            dataViewFilterService.Add( dataViewFilter );

            rockContext.SaveChanges();

            SetAttributeValue( "Status", cblStatus.SelectedValuesAsInt.AsDelimited( "," ) );
            SetAttributeValue( "Channel", ddlChannel.SelectedValue );
            SetAttributeValue( "MergeContent", cbMergeContent.Checked.ToString() );
            SetAttributeValue( "Template", ceTemplate.Text );
            SetAttributeValue( "Count", ( nbCount.Text.AsIntegerOrNull() ?? 5 ).ToString() );
            SetAttributeValue( "CacheDuration", ( nbItemCacheDuration.Text.AsIntegerOrNull() ?? 0 ).ToString() );
            SetAttributeValue( "OutputCacheDuration", ( nbOutputCacheDuration.Text.AsIntegerOrNull() ?? 0 ).ToString() );
            SetAttributeValue( "CacheTags", cblCacheTags.SelectedValues.AsDelimited( "," ) );
            SetAttributeValue( "FilterId", dataViewFilter.Id.ToString() );
            SetAttributeValue( "QueryParameterFiltering", cbQueryParamFiltering.Checked.ToString() );
            SetAttributeValue( "Order", kvlOrder.Value );
            SetAttributeValue( "SetPageTitle", cbSetPageTitle.Checked.ToString() );
            SetAttributeValue( "RssAutodiscover", cbSetRssAutodiscover.Checked.ToString() );
            SetAttributeValue( "MetaDescriptionAttribute", ddlMetaDescriptionAttribute.SelectedValue );
            SetAttributeValue( "MetaImageAttribute", ddlMetaImageAttribute.SelectedValue );

            var ppFieldType = new PageReferenceFieldType();
            SetAttributeValue( "DetailPage", ppFieldType.GetEditValue( ppDetailPage, null ) );

            SaveAttributeValues();

            RemoveCacheItem( CONTENT_CACHE_KEY );
            RemoveCacheItem( TEMPLATE_CACHE_KEY );
            // When our cache supports regions, we can call ClearRegion to clear all the output pages.
            RemoveCacheItem( OUTPUT_CACHE_KEY );

            mdEdit.Hide();
            pnlEditModal.Visible = false;
            upnlContent.Update();

            ShowView();
        }

        /// <summary>
        /// Handles the AddFilterClick event of the groupControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void groupControl_AddFilterClick( object sender, EventArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            FilterField filterField = new FilterField();
            Guid? channelGuid = GetAttributeValue( "Channel" ).AsGuidOrNull();
            if ( channelGuid.HasValue )
            {
                var contentChannel = ContentChannelCache.Get( channelGuid.Value );
                if ( contentChannel != null )
                {
                    filterField.Entity = new ContentChannelItem
                    {
                        ContentChannelId = contentChannel.Id,
                        ContentChannelTypeId = contentChannel.ContentChannelTypeId
                    };
                }
            }

            filterField.DataViewFilterGuid = Guid.NewGuid();
            groupControl.Controls.Add( filterField );
            filterField.ID = string.Format( "ff_{0}", filterField.DataViewFilterGuid.ToString( "N" ) );

            // Remove the 'Other Data View' Filter as it doesn't really make sense to have it available in this scenario
            filterField.ExcludedFilterTypes = new string[] { typeof( Rock.Reporting.DataFilter.OtherDataViewFilter ).FullName };
            filterField.FilteredEntityTypeName = groupControl.FilteredEntityTypeName;
            filterField.Expanded = true;

            filterField.DeleteClick += filterControl_DeleteClick;
        }

        /// <summary>
        /// Handles the AddGroupClick event of the groupControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void groupControl_AddGroupClick( object sender, EventArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            FilterGroup childGroupControl = new FilterGroup();
            childGroupControl.DataViewFilterGuid = Guid.NewGuid();
            groupControl.Controls.Add( childGroupControl );
            childGroupControl.ID = string.Format( "fg_{0}", childGroupControl.DataViewFilterGuid.ToString( "N" ) );
            childGroupControl.FilteredEntityTypeName = groupControl.FilteredEntityTypeName;
            childGroupControl.FilterType = FilterExpressionType.GroupAll;

            childGroupControl.AddFilterClick += groupControl_AddFilterClick;
            childGroupControl.AddGroupClick += groupControl_AddGroupClick;
            childGroupControl.DeleteGroupClick += groupControl_DeleteGroupClick;
        }

        /// <summary>
        /// Handles the DeleteClick event of the filterControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void filterControl_DeleteClick( object sender, EventArgs e )
        {
            FilterField fieldControl = sender as FilterField;
            fieldControl.Parent.Controls.Remove( fieldControl );
        }

        /// <summary>
        /// Handles the DeleteGroupClick event of the groupControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void groupControl_DeleteGroupClick( object sender, EventArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            groupControl.Parent.Controls.Remove( groupControl );
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            // Switch does not automatically initialize again after a partial-postback.  This script 
            // looks for any switch elements that have not been initialized and re-initializes them.
            string script = @"
$(document).ready(function() {
    $('.switch > input').each( function () {
        $(this).parent().switch('init');
    });
});
";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "toggle-switch-init", script, true );

            pnlEditModal.Visible = true;
            upnlContent.Update();
            mdEdit.Show();

            var rockContext = new RockContext();
            ddlChannel.DataSource = new ContentChannelService( rockContext ).Queryable()
                .OrderBy( c => c.Name )
                .Where(a => a.ContentChannelType.ShowInChannelList == true)
                .Select( c => new { c.Guid, c.Name } )
                .ToList();
            ddlChannel.DataBind();
            ddlChannel.Items.Insert( 0, new ListItem( "", "" ) );
            ddlChannel.SetValue( GetAttributeValue( "Channel" ) );
            ChannelGuid = ddlChannel.SelectedValue.AsGuidOrNull();

            cblStatus.BindToEnum<ContentChannelItemStatus>();
            foreach ( string status in GetAttributeValue( "Status" ).SplitDelimitedValues() )
            {
                var li = cblStatus.Items.FindByValue( status );
                if ( li != null )
                {
                    li.Selected = true;
                }
            }

            cbMergeContent.Checked = GetAttributeValue( "MergeContent" ).AsBoolean();
            cbSetRssAutodiscover.Checked = GetAttributeValue( "RssAutodiscover" ).AsBoolean();
            cbSetPageTitle.Checked = GetAttributeValue( "SetPageTitle" ).AsBoolean();
            ceTemplate.Text = GetAttributeValue( "Template" );
            nbCount.Text = GetAttributeValue( "Count" );
            nbItemCacheDuration.Text = GetAttributeValue( "CacheDuration" );
            nbOutputCacheDuration.Text = GetAttributeValue( "OutputCacheDuration" );

            DefinedValueService definedValueService = new DefinedValueService( new RockContext() );
            cblCacheTags.DataSource = definedValueService.GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.CACHE_TAGS.AsGuid() ).Select( v => v.Value ).ToList();
            cblCacheTags.DataBind();
            string[] selectedCacheTags = this.GetAttributeValue( "CacheTags" ).SplitDelimitedValues();
            foreach ( ListItem cacheTag in cblCacheTags.Items )
            {
                cacheTag.Selected = selectedCacheTags.Contains( cacheTag.Value );
            }

            hfDataFilterId.Value = GetAttributeValue( "FilterId" );
            cbQueryParamFiltering.Checked = GetAttributeValue( "QueryParameterFiltering" ).AsBoolean();

            var ppFieldType = new PageReferenceFieldType();
            ppFieldType.SetEditValue( ppDetailPage, null, GetAttributeValue( "DetailPage" ) );

            var directions = new Dictionary<string, string>();
            directions.Add( "", "" );
            directions.Add( SortDirection.Ascending.ConvertToInt().ToString(), "Ascending" );
            directions.Add( SortDirection.Descending.ConvertToInt().ToString(), "Descending" );
            kvlOrder.CustomValues = directions;
            kvlOrder.Value = GetAttributeValue( "Order" );
            kvlOrder.Required = true;

            ShowEdit();

            upnlContent.Update();
        }

        /// <summary>
        /// Shows the content channel item or items. If an output cache duration is set,
        /// the content will attempt to be fetched from cache unless any of the following 
        /// settings are enabled or set:
        ///    * MergeContent (bool)
        ///    * SetPageTitle (bool)
        ///    * RssAutodiscover (bool)
        ///    * MetaDescriptionAttribute (string)
        ///    * MetaImageAttribute (string)
        ///    * QueryParameterFiltering (bool)
        /// </summary>
        private void ShowView()
        {
            nbContentError.Visible = false;
            upnlContent.Update();

            string outputContents = null;

            bool isMergeContentEnabled = GetAttributeValue( "MergeContent" ).AsBoolean();
            bool isSetPageTitleEnabled = GetAttributeValue( "SetPageTitle" ).AsBoolean();
            bool isRssAutodiscoverEnabled = GetAttributeValue( "RssAutodiscover" ).AsBoolean();
            bool isQueryParameterFilteringEnabled = GetAttributeValue( "QueryParameterFiltering" ).AsBoolean( false );
            string metaDescriptionAttributeValue = GetAttributeValue( "MetaDescriptionAttribute" );
            string metaImageAttributeValue = GetAttributeValue( "MetaImageAttribute" );
            int pageNumber = PageParameter( "Page" ).AsIntegerOrNull() ?? 1;

            // Try fetching from cache if it's OK to do so. 
            // For now, we'll only cache if pagination is page 1. When our cache supports caching as a region (set)
            // we can then cache all pages and call ClearRegion if the block settings change.
            if ( OutputCacheDuration.HasValue && OutputCacheDuration.Value > 0 && pageNumber == 1 &&
                !( isSetPageTitleEnabled || isSetPageTitleEnabled || isRssAutodiscoverEnabled 
                || isQueryParameterFilteringEnabled || ! string.IsNullOrWhiteSpace( metaDescriptionAttributeValue )
                || ! string.IsNullOrWhiteSpace( metaImageAttributeValue ) ) )
            {
                outputContents = GetCacheItem( OUTPUT_CACHE_KEY ) as string;
            }

            if ( outputContents == null )
            {
                var pageRef = new Rock.Web.PageReference( CurrentPageReference );
                pageRef.Parameters.AddOrReplace( "Page", "PageNum" );

                Dictionary<string, object> linkedPages = new Dictionary<string, object>();
                linkedPages.Add( "DetailPage", LinkedPageRoute( "DetailPage" ) );

                var errorMessages = new List<string>();
                List<ContentChannelItem> contentItemList;
                try
                {
                    contentItemList = GetContent( errorMessages, isQueryParameterFilteringEnabled ) ?? new List<ContentChannelItem>();
                }
                catch ( Exception ex )
                {
                    this.LogException( ex );
                    Exception exception = ex;
                    while ( exception != null )
                    {
                        errorMessages.Add( exception.Message );
                        exception = exception.InnerException;
                    }

                    contentItemList = new List<ContentChannelItem>();
                }

                if ( errorMessages.Any() )
                {
                    nbContentError.Text = "ERROR: There was a problem getting content...<br/> ";
                    nbContentError.NotificationBoxType = NotificationBoxType.Danger;
                    nbContentError.Details = errorMessages.AsDelimited( "<br/>" );
                    nbContentError.Visible = true;
                }

                var pagination = new Pagination();
                pagination.ItemCount = contentItemList.Count();
                pagination.PageSize = GetAttributeValue( "Count" ).AsInteger();
                pagination.CurrentPage = pageNumber;
                pagination.UrlTemplate = pageRef.BuildUrl();
                var currentPageContent = pagination.GetCurrentPageItems( contentItemList );

                var mergeFieldOptions = new Rock.Lava.CommonMergeFieldsOptions();
                mergeFieldOptions.GetLegacyGlobalMergeFields = GetAttributeValue( "SupportLegacy" ).AsBoolean();
                var commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, mergeFieldOptions );

                // Merge content and attribute fields if block is configured to do so.
                if ( isMergeContentEnabled )
                {
                    var itemMergeFields = new Dictionary<string, object>( commonMergeFields );
                    if ( CurrentPerson != null )
                    {
                        // TODO: When support for "Person" is not supported anymore (should use "CurrentPerson" instead), remove this line
                        itemMergeFields.Add( "Person", CurrentPerson );
                    }

                    var enabledCommands = GetAttributeValue( "EnabledLavaCommands" );
                    foreach ( var item in currentPageContent )
                    {
                        itemMergeFields.AddOrReplace( "Item", item );
                        item.Content = item.Content.ResolveMergeFields( itemMergeFields, enabledCommands );
                        foreach ( var attributeValue in item.AttributeValues )
                        {
                            attributeValue.Value.Value = attributeValue.Value.Value.ResolveMergeFields( itemMergeFields, enabledCommands );
                        }
                    }
                }

                var mergeFields = new Dictionary<string, object>( commonMergeFields );
                mergeFields.Add( "Pagination", pagination );
                mergeFields.Add( "LinkedPages", linkedPages );
                mergeFields.Add( "Items", currentPageContent );
                mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );

                // TODO: When support for "Person" is not supported anymore (should use "CurrentPerson" instead), remove this line
                mergeFields.AddOrIgnore( "Person", CurrentPerson );

                // set page title
                if ( isSetPageTitleEnabled && contentItemList.Count > 0 )
                {
                    if ( string.IsNullOrWhiteSpace( PageParameter( "Item" ) ) )
                    {
                        // set title to channel name
                        string channelName = contentItemList.Select( c => c.ContentChannel.Name ).FirstOrDefault();
                        RockPage.BrowserTitle = String.Format( "{0} | {1}", channelName, RockPage.Site.Name );
                        RockPage.PageTitle = channelName;
                        RockPage.Header.Title = String.Format( "{0} | {1}", channelName, RockPage.Site.Name );
                    }
                    else
                    {
                        string itemTitle = contentItemList.Select( c => c.Title ).FirstOrDefault();
                        RockPage.PageTitle = itemTitle;
                        RockPage.BrowserTitle = String.Format( "{0} | {1}", itemTitle, RockPage.Site.Name );
                        RockPage.Header.Title = String.Format( "{0} | {1}", itemTitle, RockPage.Site.Name );
                    }

                    var pageBreadCrumb = RockPage.PageReference.BreadCrumbs.FirstOrDefault();
                    if ( pageBreadCrumb != null )
                    {
                        pageBreadCrumb.Name = RockPage.PageTitle;
                    }
                }

                // set rss auto discover link
                if ( isRssAutodiscoverEnabled && contentItemList.Count > 0 )
                {
                    //<link rel="alternate" type="application/rss+xml" title="RSS Feed for petefreitag.com" href="/rss/" />
                    HtmlLink rssLink = new HtmlLink();
                    rssLink.Attributes.Add( "type", "application/rss+xml" );
                    rssLink.Attributes.Add( "rel", "alternate" );
                    rssLink.Attributes.Add( "title", contentItemList.Select( c => c.ContentChannel.Name ).FirstOrDefault() );

                    var context = HttpContext.Current;
                    string channelRssUrl = string.Format( "{0}://{1}{2}{3}{4}",
                                        context.Request.Url.Scheme,
                                        WebRequestHelper.GetHostNameFromRequest( context ),
                                        context.Request.Url.Port == 80
                                            ? string.Empty
                                            : ":" + context.Request.Url.Port,
                                        RockPage.ResolveRockUrl( "~/GetChannelFeed.ashx?ChannelId=" ),
                                        contentItemList.Select( c => c.ContentChannelId ).FirstOrDefault() );

                    rssLink.Attributes.Add( "href", channelRssUrl );
                    RockPage.Header.Controls.Add( rssLink );
                }

                // set description meta tag
                if ( !string.IsNullOrWhiteSpace( metaDescriptionAttributeValue ) && contentItemList.Count > 0 )
                {
                    string attributeValue = GetMetaValueFromAttribute( metaDescriptionAttributeValue, contentItemList );

                    if ( !string.IsNullOrWhiteSpace( attributeValue ) )
                    {
                        // remove default meta description
                        RockPage.Header.Description = attributeValue.SanitizeHtml( true );
                    }
                }

                // add meta images
                if ( !string.IsNullOrWhiteSpace( metaImageAttributeValue ) && contentItemList.Count > 0 )
                {
                    string attributeValue = GetMetaValueFromAttribute( metaImageAttributeValue, contentItemList );

                    if ( !string.IsNullOrWhiteSpace( attributeValue ) )
                    {
                        HtmlMeta metaDescription = new HtmlMeta();
                        metaDescription.Name = "og:image";
                        metaDescription.Content = string.Format( "{0}://{1}/GetImage.ashx?guid={2}", Request.Url.Scheme, Request.Url.Authority, attributeValue );
                        RockPage.Header.Controls.Add( metaDescription );

                        HtmlLink imageLink = new HtmlLink();
                        imageLink.Attributes.Add( "rel", "image_src" );
                        imageLink.Attributes.Add( "href", string.Format( "{0}://{1}/GetImage.ashx?guid={2}", Request.Url.Scheme, Request.Url.Authority, attributeValue ) );
                        RockPage.Header.Controls.Add( imageLink );
                    }
                }

                var template = GetTemplate();

                if ( template.Registers.ContainsKey( "EnabledCommands" ) )
                {
                    template.Registers["EnabledCommands"] = GetAttributeValue( "EnabledLavaCommands" );
                }
                else // this should never happen
                {
                    template.Registers.Add( "EnabledCommands", GetAttributeValue( "EnabledLavaCommands" ) );
                }

                outputContents = template.Render( Hash.FromDictionary( mergeFields ) );

                if ( OutputCacheDuration.HasValue && OutputCacheDuration.Value > 0 )
                {
                    string cacheTags = GetAttributeValue( "CacheTags" ) ?? string.Empty;
                    // When our cache supports regions, add the pagination page to the cache key and set them all with the same region.
                    AddCacheItem( OUTPUT_CACHE_KEY, outputContents, OutputCacheDuration.Value, cacheTags );
                }
            }

            phContent.Controls.Add( new LiteralControl( outputContents ) );
        }

        /// <summary>
        /// Gets the meta value from attribute.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="content">The content.</param>
        /// <returns>a string value</returns>
        private string GetMetaValueFromAttribute( string input, List<ContentChannelItem> content )
        {
            string attributeEntityType = input.Split( '^' )[0].ToString() ?? "C";
            string attributeKey = input.Split( '^' )[1].ToString() ?? "";

            string attributeValue = string.Empty;

            if ( attributeEntityType == "C" )
            {
                attributeValue = content.FirstOrDefault().ContentChannel.AttributeValues.Where( a => a.Key == attributeKey ).Select( a => a.Value.Value ).FirstOrDefault();
            }
            else
            {
                attributeValue = content.FirstOrDefault().AttributeValues.Where( a => a.Key == attributeKey ).Select( a => a.Value.Value ).FirstOrDefault();
            }

            return attributeValue;
        }

        /// <summary>
        /// Gets the template.
        /// </summary>
        /// <returns>a DotLiquid Template</returns>
        /// <returns>A <see cref="DotLiquid.Template"/></returns>
        private Template GetTemplate()
        {
            Template template = null;

            try
            {
                // only load from the cache if a cacheDuration was specified
                if ( ItemCacheDuration.HasValue && ItemCacheDuration.Value > 0 )
                {
                    template = GetCacheItem( TEMPLATE_CACHE_KEY ) as Template;
                }

                if ( template == null )
                {
                    template = Template.Parse( GetAttributeValue( "Template" ) );

                    if ( ItemCacheDuration.HasValue && ItemCacheDuration.Value > 0 )
                    {
                        string cacheTags = GetAttributeValue( "CacheTags" ) ?? string.Empty;
                        AddCacheItem( TEMPLATE_CACHE_KEY, template, ItemCacheDuration.Value, cacheTags );
                    }
                }
            }
            catch ( Exception ex )
            {
                template = Template.Parse( string.Format( "Lava error: {0}", ex.Message ) );
            }

            return template;
        }

        /// <summary>
        /// Gets the content channel items from the item-cache (if there), or from 
        /// the configured Channel and any given Item id or filter in the query string
        /// if QueryParameterFiltering is enabled.
        /// </summary>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns> a list of <see cref="Rock.Model.ContentChannelItem">ContentChannelItems</see></returns>
        private List<ContentChannelItem> GetContent( List<string> errorMessages, bool isQueryParameterFilteringEnabled )
        {
            List<ContentChannelItem> items = null;

            // only load from the cache if a cacheDuration was specified
            if ( ItemCacheDuration.HasValue && ItemCacheDuration.Value > 0 )
            {
                items = GetCacheItem( CONTENT_CACHE_KEY ) as List<ContentChannelItem>;
            }

            if ( items == null || ( isQueryParameterFilteringEnabled && Request.QueryString.Count > 0 ) )
            {
                Guid? channelGuid = GetAttributeValue( "Channel" ).AsGuidOrNull();
                if ( channelGuid.HasValue )
                {
                    var rockContext = new RockContext();
                    var service = new ContentChannelItemService( rockContext );
                    var itemType = typeof( Rock.Model.ContentChannelItem );

                    ParameterExpression paramExpression = service.ParameterExpression;

                    var contentChannel = new ContentChannelService( rockContext ).Get( channelGuid.Value );
                    if ( contentChannel != null )
                    {
                        items = new List<ContentChannelItem>();

                        var qry = service
                            .Queryable()
                            .Include(a => a.ContentChannel)
                            .Include(a => a.ContentChannelType)
                            .Include(a => a.ContentChannelItemSlugs)
                            .Where( i => i.ContentChannelId == contentChannel.Id );

                        int? itemId = PageParameter( "Item" ).AsIntegerOrNull();
                        if ( isQueryParameterFilteringEnabled && itemId.HasValue )
                        {
                            qry = qry.Where( i => i.Id == itemId.Value );
                        }

                        if ( contentChannel.RequiresApproval && !contentChannel.ContentChannelType.DisableStatus )
                        {
                            // Check for the configured status and limit query to those
                            var statuses = new List<ContentChannelItemStatus>();
                            foreach ( string statusVal in ( GetAttributeValue( "Status" ) ?? "2" ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                            {
                                var status = statusVal.ConvertToEnumOrNull<ContentChannelItemStatus>();
                                if ( status != null )
                                {
                                    statuses.Add( status.Value );
                                }
                            }
                            if ( statuses.Any() )
                            {
                                qry = qry.Where( i => statuses.Contains( i.Status ) );
                            }
                        }

                        int? dataFilterId = GetAttributeValue( "FilterId" ).AsIntegerOrNull();
                        if ( dataFilterId.HasValue )
                        {
                            var dataFilterService = new DataViewFilterService( rockContext );
                            var dataFilter = dataFilterService.Queryable( "ChildFilters" ).FirstOrDefault( a => a.Id == dataFilterId.Value );
                            Expression whereExpression = dataFilter != null ? dataFilter.GetExpression( itemType, service, paramExpression, errorMessages ) : null;

                            qry = qry.Where( paramExpression, whereExpression, null );
                        }

                        // All filtering has been added, now run query, check security and load attributes
                        foreach ( var item in qry.ToList() )
                        {
                            if ( item.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                            {
                                item.LoadAttributes( rockContext );
                                items.Add( item );
                            }
                        }

                        // Order the items
                        SortProperty sortProperty = null;

                        string orderBy = GetAttributeValue( "Order" );
                        if ( !string.IsNullOrWhiteSpace( orderBy ) )
                        {
                            var fieldDirection = new List<string>();
                            foreach ( var itemPair in orderBy.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).Select( a => a.Split( '^' ) ) )
                            {
                                if ( itemPair.Length == 2 && !string.IsNullOrWhiteSpace( itemPair[0] ) )
                                {
                                    var sortDirection = SortDirection.Ascending;
                                    if ( !string.IsNullOrWhiteSpace( itemPair[1] ) )
                                    {
                                        sortDirection = itemPair[1].ConvertToEnum<SortDirection>( SortDirection.Ascending );
                                    }
                                    fieldDirection.Add( itemPair[0] + ( sortDirection == SortDirection.Descending ? " desc" : "" ) );
                                }
                            }

                            sortProperty = new SortProperty();
                            sortProperty.Direction = SortDirection.Ascending;
                            sortProperty.Property = fieldDirection.AsDelimited( "," );

                            string[] columns = sortProperty.Property.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

                            var itemQry = items.AsQueryable();
                            IOrderedQueryable<ContentChannelItem> orderedQry = null;

                            for ( int columnIndex = 0; columnIndex < columns.Length; columnIndex++ )
                            {
                                string column = columns[columnIndex].Trim();

                                var direction = sortProperty.Direction;
                                if ( column.ToLower().EndsWith( " desc" ) )
                                {
                                    column = column.Left( column.Length - 5 );
                                    direction = sortProperty.Direction == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
                                }

                                try
                                {
                                    if ( column.StartsWith( "Attribute:" ) )
                                    {
                                        string attributeKey = column.Substring( 10 );

                                        if ( direction == SortDirection.Ascending )
                                        {
                                            orderedQry = ( columnIndex == 0 ) ?
                                                itemQry.OrderBy( i => i.AttributeValues.Where( v => v.Key == attributeKey ).FirstOrDefault().Value.SortValue ) :
                                                orderedQry.ThenBy( i => i.AttributeValues.Where( v => v.Key == attributeKey ).FirstOrDefault().Value.SortValue );
                                        }
                                        else
                                        {
                                            orderedQry = ( columnIndex == 0 ) ?
                                                itemQry.OrderByDescending( i => i.AttributeValues.Where( v => v.Key == attributeKey ).FirstOrDefault().Value.SortValue ) :
                                                orderedQry.ThenByDescending( i => i.AttributeValues.Where( v => v.Key == attributeKey ).FirstOrDefault().Value.SortValue );
                                        }
                                    }
                                    else
                                    {
                                        if ( direction == SortDirection.Ascending )
                                        {
                                            orderedQry = ( columnIndex == 0 ) ? itemQry.OrderBy( column ) : orderedQry.ThenBy( column );
                                        }
                                        else
                                        {
                                            orderedQry = ( columnIndex == 0 ) ? itemQry.OrderByDescending( column ) : orderedQry.ThenByDescending( column );
                                        }
                                    }
                                }
                                catch { }

                            }

                            try
                            {
                                if ( orderedQry != null )
                                {
                                    items = orderedQry.ToList();
                                }
                            }
                            catch { }

                        }

                        if ( ItemCacheDuration.HasValue && ItemCacheDuration.Value > 0 && !isQueryParameterFilteringEnabled )
                        {
                            string cacheTags = GetAttributeValue( "CacheTags" ) ?? string.Empty;
                            AddCacheItem( CONTENT_CACHE_KEY, items, ItemCacheDuration.Value, cacheTags );
                        }

                        // If items could be filtered by querystring values, check for filters
                        if ( isQueryParameterFilteringEnabled )
                        {
                            var pageParameters = PageParameters();
                            if ( pageParameters.Count > 0 )
                            {
                                var propertyFilter = new Rock.Reporting.DataFilter.PropertyFilter();

                                var itemQry = items.AsQueryable();
                                foreach ( string key in PageParameters().Select( p => p.Key ).ToList() )
                                {
                                    var selection = new List<string>();

                                    // Since there could be many matches by the key name for an attribute we have to construct the unique name used by EntityHelper.FindFromFilterSelection and use that
                                    var attributeService = new AttributeService( rockContext );
                                    var attributeGuid = attributeService
                                        .Queryable()
                                        .Where( a => a.EntityTypeQualifierColumn == "ContentChannelId" )
                                        .Where( a => a.EntityTypeQualifierValue == contentChannel.Id.ToString() )
                                        .Where( a => a.Key == key )
                                        .Select( a => a.Guid )
                                        .FirstOrDefault();

                                    string uniqueName = key;
                                    if( attributeGuid != null )
                                    {
                                        uniqueName = string.Format( "Attribute_{0}_{1}", key, attributeGuid.ToString().Replace("-", string.Empty ) );
                                    }

                                    // Keep using uniquename for attributes since common keys (e.g. "category")will return mutliple values
                                    selection.Add( uniqueName );

                                    var entityField = Rock.Reporting.EntityHelper.FindFromFilterSelection( itemType, uniqueName, false, false );
                                    if ( entityField != null )
                                    {
                                        string value = PageParameter( key );
                                        switch ( entityField.FieldType.Guid.ToString().ToUpper() )
                                        {
                                            case Rock.SystemGuid.FieldType.DAY_OF_WEEK:
                                            case Rock.SystemGuid.FieldType.SINGLE_SELECT:
                                                {
                                                    selection.Add( value );
                                                }
                                                break;
                                            case Rock.SystemGuid.FieldType.MULTI_SELECT:
                                                {
                                                    selection.Add( ComparisonType.Contains.ConvertToInt().ToString() );
                                                    selection.Add( value );
                                                }
                                                break;
                                            default:
                                                {
                                                    selection.Add( ComparisonType.EqualTo.ConvertToInt().ToString() );
                                                    selection.Add( value );
                                                }
                                                break;
                                        }

                                        itemQry = itemQry.Where( paramExpression, propertyFilter.GetExpression( itemType, service, paramExpression, Newtonsoft.Json.JsonConvert.SerializeObject( selection ) ) );
                                    }
                                }

                                items = itemQry.ToList();
                            }
                        }
                    }
                }
            }

            return items;
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        public void ShowEdit()
        {
            int? filterId = hfDataFilterId.Value.AsIntegerOrNull();

            if ( ChannelGuid.HasValue )
            {
                var rockContext = new RockContext();
                var channel = new ContentChannelService( rockContext ).Queryable( "ContentChannelType" )
                    .FirstOrDefault( c => c.Guid.Equals( ChannelGuid.Value ) );
                if ( channel != null )
                {

                    cblStatus.Visible = channel.RequiresApproval && !channel.ContentChannelType.DisableStatus;

                    cbSetRssAutodiscover.Visible = channel.EnableRss;

                    var filterService = new DataViewFilterService( rockContext );
                    DataViewFilter filter = null;

                    if ( filterId.HasValue )
                    {
                        filter = filterService.Get( filterId.Value );
                    }

                    if ( filter == null || filter.ExpressionType == FilterExpressionType.Filter )
                    {
                        filter = new DataViewFilter();
                        filter.Guid = new Guid();
                        filter.ExpressionType = FilterExpressionType.GroupAll;
                    }

                    CreateFilterControl( channel, filter, true, rockContext );

                    kvlOrder.CustomKeys = new Dictionary<string, string>();
                    kvlOrder.CustomKeys.Add( "", "" );
                    kvlOrder.CustomKeys.Add( "Title", "Title" );
                    kvlOrder.CustomKeys.Add( "Priority", "Priority" );
                    kvlOrder.CustomKeys.Add( "Status", "Status" );
                    kvlOrder.CustomKeys.Add( "StartDateTime", "Start" );
                    kvlOrder.CustomKeys.Add( "ExpireDateTime", "Expire" );
                    kvlOrder.CustomKeys.Add( "Order", "Order" );


                    // add attributes to the meta description and meta image attribute list
                    ddlMetaDescriptionAttribute.Items.Clear();
                    ddlMetaImageAttribute.Items.Clear();
                    ddlMetaDescriptionAttribute.Items.Add( "" );
                    ddlMetaImageAttribute.Items.Add( "" );

                    string currentMetaDescriptionAttribute = GetAttributeValue( "MetaDescriptionAttribute" ) ?? string.Empty;
                    string currentMetaImageAttribute = GetAttributeValue( "MetaImageAttribute" ) ?? string.Empty;

                    // add channel attributes
                    channel.LoadAttributes();
                    foreach ( var attribute in channel.Attributes )
                    {
                        var field = attribute.Value.FieldType.Field;
                        string computedKey = "C^" + attribute.Key;

                        ddlMetaDescriptionAttribute.Items.Add( new ListItem( "Channel: " + attribute.Value.ToString(), computedKey ) );

                        if ( field is Rock.Field.Types.ImageFieldType )
                        {
                            ddlMetaImageAttribute.Items.Add( new ListItem( "Channel: " + attribute.Value.ToString(), computedKey ) );
                        }
                    }

                    // add item attributes
                    AttributeService attributeService = new AttributeService( rockContext );
                    var itemAttributes = attributeService.GetByEntityTypeId( new ContentChannelItem().TypeId, false ).AsQueryable()
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

                    foreach ( var attribute in itemAttributes )
                    {
                        string attrKey = "Attribute:" + attribute.Key;
                        if ( !kvlOrder.CustomKeys.ContainsKey( attrKey ) )
                        {
                            kvlOrder.CustomKeys.Add( "Attribute:" + attribute.Key, attribute.Name );

                            string computedKey = "I^" + attribute.Key;
                            ddlMetaDescriptionAttribute.Items.Add( new ListItem( "Item: " + attribute.Name, computedKey ) );

                            var field = attribute.FieldType.Name;

                            if ( field == "Image" )
                            {
                                ddlMetaImageAttribute.Items.Add( new ListItem( "Item: " + attribute.Name, computedKey ) );
                            }
                        }
                    }

                    // select attributes
                    SetListValue( ddlMetaDescriptionAttribute, currentMetaDescriptionAttribute );
                    SetListValue( ddlMetaImageAttribute, currentMetaImageAttribute );

                }
            }
        }

        /// <summary>
        /// Sets the list value.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="value">The value.</param>
        private void SetListValue( ListControl listControl, string value )
        {
            foreach ( ListItem item in listControl.Items )
            {
                item.Selected = ( item.Value == value );
            }
        }

        /// <summary>
        /// Creates the filter control.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        /// <param name="rockContext">The rock context.</param>
        private void CreateFilterControl( ContentChannel channel, DataViewFilter filter, bool setSelection, RockContext rockContext )
        {
            phFilters.Controls.Clear();
            if ( filter != null )
            {
                CreateFilterControl( phFilters, filter, setSelection, rockContext, channel );
            }
        }

        /// <summary>
        /// Creates the filter control.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="contentChannel">The content channel.</param>
        private void CreateFilterControl( Control parentControl, DataViewFilter filter, bool setSelection, RockContext rockContext, ContentChannel contentChannel )
        {
            try
            {
                if ( filter.ExpressionType == FilterExpressionType.Filter )
                {
                    var filterControl = new FilterField();
                    filterControl.Entity = new ContentChannelItem
                    {
                        ContentChannelId = contentChannel.Id,
                        ContentChannelTypeId = contentChannel.ContentChannelTypeId
                    };

                    parentControl.Controls.Add( filterControl );
                    filterControl.DataViewFilterGuid = filter.Guid;
                    filterControl.ID = string.Format( "ff_{0}", filterControl.DataViewFilterGuid.ToString( "N" ) );

                    // Remove the 'Other Data View' Filter as it doesn't really make sense to have it available in this scenario
                    filterControl.ExcludedFilterTypes = new string[] { typeof( Rock.Reporting.DataFilter.OtherDataViewFilter ).FullName };
                    filterControl.FilteredEntityTypeName = ITEM_TYPE_NAME;

                    if ( filter.EntityTypeId.HasValue )
                    {
                        var entityTypeCache = EntityTypeCache.Get( filter.EntityTypeId.Value, rockContext );
                        if ( entityTypeCache != null )
                        {
                            filterControl.FilterEntityTypeName = entityTypeCache.Name;
                        }
                    }

                    filterControl.Expanded = filter.Expanded;
                    if ( setSelection )
                    {
                        try
                        {
                            filterControl.SetSelection( filter.Selection );
                        }
                        catch ( Exception ex )
                        {
                            this.LogException( new Exception( "Exception setting selection for DataViewFilter: " + filter.Guid, ex ) );
                        }
                    }

                    filterControl.DeleteClick += filterControl_DeleteClick;
                }
                else
                {
                    var groupControl = new FilterGroup();
                    parentControl.Controls.Add( groupControl );
                    groupControl.DataViewFilterGuid = filter.Guid;
                    groupControl.ID = string.Format( "fg_{0}", groupControl.DataViewFilterGuid.ToString( "N" ) );
                    groupControl.FilteredEntityTypeName = ITEM_TYPE_NAME;
                    groupControl.IsDeleteEnabled = parentControl is FilterGroup;
                    if ( setSelection )
                    {
                        groupControl.FilterType = filter.ExpressionType;
                    }

                    groupControl.AddFilterClick += groupControl_AddFilterClick;
                    groupControl.AddGroupClick += groupControl_AddGroupClick;
                    groupControl.DeleteGroupClick += groupControl_DeleteGroupClick;
                    foreach ( var childFilter in filter.ChildFilters )
                    {
                        CreateFilterControl( groupControl, childFilter, setSelection, rockContext, contentChannel );
                    }
                }
            }
            catch ( Exception ex )
            {
                this.LogException( new Exception( "Exception creating FilterControl for DataViewFilter: " + filter.Guid, ex ) );
            }
        }

        private void SetNewDataFilterGuids( DataViewFilter dataViewFilter )
        {
            if ( dataViewFilter != null )
            {
                dataViewFilter.Guid = Guid.NewGuid();
                foreach ( var childFilter in dataViewFilter.ChildFilters )
                {
                    SetNewDataFilterGuids( childFilter );
                }
            }
        }

        private void DeleteDataViewFilter( DataViewFilter dataViewFilter, DataViewFilterService service )
        {
            if ( dataViewFilter != null )
            {
                foreach ( var childFilter in dataViewFilter.ChildFilters.ToList() )
                {
                    DeleteDataViewFilter( childFilter, service );
                }

                service.Delete( dataViewFilter );
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// 
        /// </summary>
        public class Pagination : DotLiquid.Drop
        {

            /// <summary>
            /// Gets or sets the item count.
            /// </summary>
            /// <value>
            /// The item count.
            /// </value>
            public int ItemCount { get; set; }

            /// <summary>
            /// Gets or sets the size of the page.
            /// </summary>
            /// <value>
            /// The size of the page.
            /// </value>
            public int PageSize { get; set; }

            /// <summary>
            /// Gets or sets the current page.
            /// </summary>
            /// <value>
            /// The current page.
            /// </value>
            public int CurrentPage { get; set; }

            /// <summary>
            /// Gets the previous page.
            /// </summary>
            /// <value>
            /// The previous page.
            /// </value>
            public int PreviousPage
            {
                get
                {
                    CurrentPage = CurrentPage > TotalPages ? TotalPages : CurrentPage;
                    return ( CurrentPage > 1 ) ? CurrentPage - 1 : -1;
                }
            }

            /// <summary>
            /// Gets the next page.
            /// </summary>
            /// <value>
            /// The next page.
            /// </value>
            public int NextPage
            {
                get
                {
                    CurrentPage = CurrentPage > TotalPages ? TotalPages : CurrentPage;
                    return ( CurrentPage < TotalPages ) ? CurrentPage + 1 : -1;
                }
            }

            /// <summary>
            /// Gets the total pages.
            /// </summary>
            /// <value>
            /// The total pages.
            /// </value>
            public int TotalPages
            {
                get
                {
                    if ( PageSize == 0 )
                    {
                        return 1;
                    }
                    else
                    {
                        return Convert.ToInt32( Math.Abs( ItemCount / PageSize ) ) +
                            ( ( ItemCount % PageSize ) > 0 ? 1 : 0 );
                    }
                }
            }

            public string UrlTemplate { get; set; }

            /// <summary>
            /// Gets or sets the pages.
            /// </summary>
            /// <value>
            /// The pages.
            /// </value>
            public List<PaginationPage> Pages
            {
                get
                {
                    var pages = new List<PaginationPage>();

                    for ( int i = 1; i <= TotalPages; i++ )
                    {
                        pages.Add( new PaginationPage( UrlTemplate, i ) );
                    }

                    return pages;
                }
            }

            /// <summary>
            /// Gets the current page items.
            /// </summary>
            /// <param name="allItems">All items.</param>
            /// <returns></returns>
            public List<ContentChannelItem> GetCurrentPageItems( List<ContentChannelItem> allItems )
            {
                if ( PageSize > 0 )
                {
                    CurrentPage = CurrentPage > TotalPages ? TotalPages : CurrentPage;
                    return allItems.Skip( ( CurrentPage - 1 ) * PageSize ).Take( PageSize ).ToList();
                }

                return allItems;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public class PaginationPage : DotLiquid.Drop
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PaginationPage"/> class.
            /// </summary>
            /// <param name="urlTemplate">The URL template.</param>
            /// <param name="pageNumber">The page number.</param>
            public PaginationPage( string urlTemplate, int pageNumber )
            {
                UrlTemplate = urlTemplate;
                PageNumber = pageNumber;
            }

            private string UrlTemplate { get; set; }

            /// <summary>
            /// Gets the page number.
            /// </summary>
            /// <value>
            /// The page number.
            /// </value>
            public int PageNumber { get; private set; }

            /// <summary>
            /// Gets the page URL.
            /// </summary>
            /// <value>
            /// The page URL.
            /// </value>
            public string PageUrl
            {
                get
                {
                    if ( !string.IsNullOrWhiteSpace( UrlTemplate ) && UrlTemplate.Contains( "{0}" ) )
                    {
                        return string.Format( UrlTemplate, PageNumber );
                    }
                    else
                    {
                        return PageNumber.ToString();
                    }
                }
            }

            #endregion

        }
    }

}