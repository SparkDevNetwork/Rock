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
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using DotLiquid;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Lava;
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

    #region Block Attributes

    // Block Properties
    [LavaCommandsField(
        "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this content channel block.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.EnabledLavaCommands )]
    [LinkedPage(
        "Detail Page",
        Description = "The page to navigate to for details.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.DetailPage )]
    [BooleanField(
        "Enable Legacy Global Attribute Lava",
        Description = "This should only be enabled if your lava is using legacy Global Attributes. Enabling this option, will negatively affect the performance of this block.",
        DefaultBooleanValue = false,
        Order = 2,
        Key = AttributeKey.SupportLegacy )]

    // Custom Settings
    [ContentChannelField(
        "Channel",
        Description = "The channel to display items from.",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.Channel )]
    [EnumsField(
        "Status",
        Description = "Include items with the following status.",
        IsRequired = false,
        EnumSourceType = typeof( ContentChannelItemStatus ),
        DefaultValue = "2",
        Category = "CustomSetting",
        Key = AttributeKey.Status )]
    [CodeEditorField(
        "Template",
        Description = "The template to use when formatting the list of items.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 600,
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.Template )]
    [IntegerField(
        "Count",
        Description = "The maximum number of items to display.",
        IsRequired = false,
        DefaultIntegerValue = 5,
        Category = "CustomSetting",
        Key = AttributeKey.Count )]
    [IntegerField(
        "Item Cache Duration",
        Description = "Number of seconds to cache the content items returned by the selected filter.",
        IsRequired = false,
        DefaultIntegerValue = 3600,
        Category = "CustomSetting",
        Order = 0,
        Key = AttributeKey.CacheDuration )]
    [IntegerField(
        "Output Cache Duration",
        Description = "Number of seconds to cache the resolved output. Only cache the output if you are not personalizing the output based on current user, current page, or any other merge field value.",
        IsRequired = false,
        DefaultIntegerValue = 0,
        Category = "CustomSetting",
        Order = 0,
        Key = AttributeKey.OutputCacheDuration )]
    [CustomCheckboxListField(
        "Cache Tags",
        Description = "Cached tags are used to link cached content so that it can be expired as a group",
        IsRequired = false,
        Key = AttributeKey.CacheTags,
        Category = "CustomSetting" )]
    [IntegerField(
        "Filter Id",
        Description = "The data filter that is used to filter items",
        IsRequired = false,
        DefaultIntegerValue = 0,
        Category = "CustomSetting",
        Key = AttributeKey.FilterId )]
    [BooleanField(
        "Query Parameter Filtering",
        Description = "Determines if block should evaluate the query string parameters for additional filter criteria.",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Key = AttributeKey.QueryParameterFiltering )]
    [TextField(
        "Order",
        Description = "The specifics of how items should be ordered. This value is set through configuration and should not be modified here.",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.Order )]
    [BooleanField(
        "Merge Content",
        Description = "Should the content data and attribute values be merged using the Lava template engine.",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Key = AttributeKey.MergeContent )]
    [BooleanField(
        "Set Page Title",
        Description = "Determines if the block should set the page title with the channel name or content item.",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Key = AttributeKey.SetPageTitle )]
    [BooleanField(
        "Rss Autodiscover",
        Description = "Determines if a RSS autodiscover link should be added to the page head.",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Key = AttributeKey.RssAutodiscover )]
    [TextField(
        "Meta Description Attribute",
        Description = "Attribute to use for storing the description attribute.",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.MetaDescriptionAttribute )]
    [TextField(
        "Meta Image Attribute",
        Description = "Attribute to use for storing the image attribute.",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.MetaImageAttribute )]
    [BooleanField(
        "Enable Tag List",
        Description = "Determines if the ItemTagList lava parameter will be populated.",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Key = AttributeKey.EnableTagList )]
    [BooleanField(
        "Enable Archive Summary",
        Description = "When enabled an additional \"ArchiveSummary\" collection will be available in Lava to help create a summary list of content channel items by month/year. This collection will be cached using the same duration as the Item Cache and will hold the following properties: Month (int), MonthName, Year, Count.",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Key = AttributeKey.EnableArchiveSummary )]
    [EnumField(
        "Personalization",
        Description = "The setting determines how personalization effect the results shown. Ignore will not consider segments or request filters, Prioritize will add items with matching items to the top of the list (in order by the sort order) and Filter will only show items that match the current individuals segments and request filters.",
        EnumSourceType = typeof( PersonalizationFilterType ),
        Category = "CustomSetting",
        Key = AttributeKey.Personalization )]
    [TextField(
        "Context Filter Attribute",
        Description = "Item attribute to compare when filtering items using the block Context. If the block doesn't have a context, this setting will be ignored.",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.ContextAttribute )]

    [ContextAware]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.CONTENT_CHANNEL_VIEW )]
    public partial class ContentChannelView : RockBlockCustomSettings
    {

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string EnabledLavaCommands = "EnabledLavaCommands";
            public const string DetailPage = "DetailPage";
            public const string SupportLegacy = "SupportLegacy";
            public const string Channel = "Channel";
            public const string Status = "Status";
            public const string Template = "Template";
            public const string Count = "Count";
            public const string CacheDuration = "CacheDuration";
            public const string OutputCacheDuration = "OutputCacheDuration";
            public const string CacheTags = "CacheTags";
            public const string FilterId = "FilterId";
            public const string QueryParameterFiltering = "QueryParameterFiltering";
            public const string Order = "Order";
            public const string MergeContent = "MergeContent";
            public const string SetPageTitle = "SetPageTitle";
            public const string RssAutodiscover = "RssAutodiscover";
            public const string MetaDescriptionAttribute = "MetaDescriptionAttribute";
            public const string MetaImageAttribute = "MetaImageAttribute";
            public const string EnableTagList = "EnableTagList";
            public const string EnableArchiveSummary = "EnableArchiveSummary";
            public const string Personalization = "Personalization";
            public const string ContextAttribute = "ContextAttribute";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string Page = "Page";
            public const string Item = "Item";
            public const string Tag = "Tag";
            public const string Year = "Year";
            public const string Month = "Month";
        }

        #endregion

        #region Fields

        private readonly string ITEM_TYPE_NAME = "Rock.Model.ContentChannelItem";
        private readonly string CONTENT_CACHE_KEY = "Content";
        private readonly string TEMPLATE_CACHE_KEY = "Template";
        private readonly string OUTPUT_CACHE_KEY = "Output";
        private readonly string TAG_CACHE_KEY = "Tags";
        private readonly string MONTH_YEAR_CACHE_KEY = "DateFilter";

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

            ItemCacheDuration = GetAttributeValue( AttributeKey.CacheDuration ).AsIntegerOrNull();
            OutputCacheDuration = GetAttributeValue( AttributeKey.OutputCacheDuration ).AsIntegerOrNull();

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
            ViewState["DataViewFilter"] = ReportingHelper.GetFilterFromControls( phFilters ).ToJson();

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

            SetAttributeValue( AttributeKey.Status, cblStatus.SelectedValuesAsInt.AsDelimited( "," ) );
            SetAttributeValue( AttributeKey.Channel, ddlChannel.SelectedValue );
            SetAttributeValue( AttributeKey.MergeContent, cbMergeContent.Checked.ToString() );
            SetAttributeValue( AttributeKey.Template, ceTemplate.Text );
            SetAttributeValue( AttributeKey.Count, ( nbCount.Text.AsIntegerOrNull() ?? 5 ).ToString() );
            SetAttributeValue( AttributeKey.CacheDuration, ( nbItemCacheDuration.Text.AsIntegerOrNull() ?? 0 ).ToString() );
            SetAttributeValue( AttributeKey.OutputCacheDuration, ( nbOutputCacheDuration.Text.AsIntegerOrNull() ?? 0 ).ToString() );
            SetAttributeValue( AttributeKey.CacheTags, cblCacheTags.SelectedValues.AsDelimited( "," ) );
            SetAttributeValue( AttributeKey.FilterId, dataViewFilter.Id.ToString() );
            SetAttributeValue( AttributeKey.QueryParameterFiltering, cbQueryParamFiltering.Checked.ToString() );
            SetAttributeValue( AttributeKey.Order, kvlOrder.Value );
            SetAttributeValue( AttributeKey.SetPageTitle, cbSetPageTitle.Checked.ToString() );
            SetAttributeValue( AttributeKey.RssAutodiscover, cbSetRssAutodiscover.Checked.ToString() );
            SetAttributeValue( AttributeKey.ContextAttribute, ddlContextAttribute.SelectedValue );
            SetAttributeValue( AttributeKey.MetaDescriptionAttribute, ddlMetaDescriptionAttribute.SelectedValue );
            SetAttributeValue( AttributeKey.MetaImageAttribute, ddlMetaImageAttribute.SelectedValue );
            SetAttributeValue( AttributeKey.EnableTagList, cbEnableTags.Checked.ToString() );
            SetAttributeValue( AttributeKey.EnableArchiveSummary, cbEnableArchiveSummary.Checked.ToString() );
            SetAttributeValue( AttributeKey.Personalization, rblPersonalization.SelectedValue );

            var ppFieldType = new PageReferenceFieldType();
            SetAttributeValue( AttributeKey.DetailPage, ppFieldType.GetEditValue( ppDetailPage, null ) );

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
            Guid? channelGuid = GetAttributeValue( AttributeKey.Channel ).AsGuidOrNull();

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
                .Where( a => a.ContentChannelType.ShowInChannelList == true )
                .Select( c => new { c.Guid, c.Name } )
                .ToList();
            ddlChannel.DataBind();
            ddlChannel.Items.Insert( 0, new ListItem( "", "" ) );
            ddlChannel.SetValue( GetAttributeValue( AttributeKey.Channel ) );
            ChannelGuid = ddlChannel.SelectedValue.AsGuidOrNull();

            cblStatus.BindToEnum<ContentChannelItemStatus>();
            foreach ( string status in GetAttributeValue( AttributeKey.Status ).SplitDelimitedValues() )
            {
                var li = cblStatus.Items.FindByValue( status );
                if ( li != null )
                {
                    li.Selected = true;
                }
            }

            cbMergeContent.Checked = GetAttributeValue( AttributeKey.MergeContent ).AsBoolean();
            cbSetRssAutodiscover.Checked = GetAttributeValue( AttributeKey.RssAutodiscover ).AsBoolean();
            cbSetPageTitle.Checked = GetAttributeValue( AttributeKey.SetPageTitle ).AsBoolean();
            ceTemplate.Text = GetAttributeValue( AttributeKey.Template );
            nbCount.Text = GetAttributeValue( AttributeKey.Count );
            nbItemCacheDuration.Text = GetAttributeValue( AttributeKey.CacheDuration );
            nbOutputCacheDuration.Text = GetAttributeValue( AttributeKey.OutputCacheDuration );
            cbEnableTags.Checked = GetAttributeValue( AttributeKey.EnableTagList ).AsBoolean();
            cbEnableArchiveSummary.Checked = GetAttributeValue( AttributeKey.EnableArchiveSummary ).AsBoolean();

            DefinedValueService definedValueService = new DefinedValueService( new RockContext() );
            cblCacheTags.DataSource = definedValueService.GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.CACHE_TAGS.AsGuid() ).Select( v => v.Value ).ToList();
            cblCacheTags.DataBind();
            string[] selectedCacheTags = this.GetAttributeValue( AttributeKey.CacheTags ).SplitDelimitedValues();
            foreach ( ListItem cacheTag in cblCacheTags.Items )
            {
                cacheTag.Selected = selectedCacheTags.Contains( cacheTag.Value );
            }

            hfDataFilterId.Value = GetAttributeValue( AttributeKey.FilterId );
            cbQueryParamFiltering.Checked = GetAttributeValue( AttributeKey.QueryParameterFiltering ).AsBoolean();

            var ppFieldType = new PageReferenceFieldType();
            ppFieldType.SetEditValue( ppDetailPage, null, GetAttributeValue( AttributeKey.DetailPage ) );

            var directions = new Dictionary<string, string>();
            directions.Add( "", "" );
            directions.Add( SortDirection.Ascending.ConvertToInt().ToString(), "Ascending" );
            directions.Add( SortDirection.Descending.ConvertToInt().ToString(), "Descending" );
            kvlOrder.CustomValues = directions;
            kvlOrder.Value = GetAttributeValue( AttributeKey.Order );
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

            // Disable content rendering for configuration mode to improve efficiency.
            // This is also necessary to avoid an issue where Lava content may fail to render if the template
            // uses {% include %} to reference files that do not exist in the filesystem of the current theme.
            if ( this.ConfigurationRenderModeIsEnabled )
            {
                return;
            }

            string outputContents = null;

            bool isMergeContentEnabled = GetAttributeValue( AttributeKey.MergeContent ).AsBoolean();
            bool isSetPageTitleEnabled = GetAttributeValue( AttributeKey.SetPageTitle ).AsBoolean();
            bool isRssAutodiscoverEnabled = GetAttributeValue( AttributeKey.RssAutodiscover ).AsBoolean();
            bool isQueryParameterFilteringEnabled = GetAttributeValue( AttributeKey.QueryParameterFiltering ).AsBoolean( false );
            bool isTagListEnabled = GetAttributeValue( AttributeKey.EnableTagList ).AsBoolean();
            bool isArchiveSummaryEnabled = GetAttributeValue( AttributeKey.EnableArchiveSummary ).AsBoolean();

            string metaDescriptionAttributeValue = GetAttributeValue( AttributeKey.MetaDescriptionAttribute );
            string metaImageAttributeValue = GetAttributeValue( AttributeKey.MetaImageAttribute );
            int pageNumber = PageParameter( PageParameterKey.Page ).AsIntegerOrNull() ?? 1;

            // Try fetching from cache if it's OK to do so. 
            // For now, we'll only cache if pagination is page 1. When our cache supports caching as a region (set)
            // we can then cache all pages and call ClearRegion if the block settings change.
            if ( OutputCacheDuration.HasValue && OutputCacheDuration.Value > 0 && pageNumber == 1 &&
                !( isSetPageTitleEnabled || isSetPageTitleEnabled || isRssAutodiscoverEnabled
                || isQueryParameterFilteringEnabled || !string.IsNullOrWhiteSpace( metaDescriptionAttributeValue )
                || !string.IsNullOrWhiteSpace( metaImageAttributeValue ) ) )
            {
                outputContents = GetCacheItem( OUTPUT_CACHE_KEY, true ) as string;
            }

            if ( outputContents == null )
            {
                var pageRef = new Rock.Web.PageReference( CurrentPageReference );
                pageRef.Parameters.AddOrReplace( "Page", "PageNum" );

                Dictionary<string, object> linkedPages = new Dictionary<string, object>();
                linkedPages.Add( "DetailPage", LinkedPageRoute( AttributeKey.DetailPage ) );

                List<ContentChannelItem> contentItemList = null;
                List<TagModel> tags = null;
                List<ArchiveSummaryModel> archiveSummaries = null;
                try
                {
                    var contentItemResults = GetContent( isQueryParameterFilteringEnabled, isTagListEnabled, isArchiveSummaryEnabled );
                    contentItemList = contentItemResults.Items ?? new List<ContentChannelItem>();
                    tags = contentItemResults.Tags ?? new List<TagModel>();
                    archiveSummaries = contentItemResults.ArchiveSumaries ?? new List<ArchiveSummaryModel>();
                }
                catch ( Exception ex )
                {
                    this.LogException( ex );
                    nbContentError.Text = "ERROR: There was a problem getting content";
                    nbContentError.NotificationBoxType = NotificationBoxType.Danger;
                    nbContentError.Details = ex.Message;
                    nbContentError.Visible = true;

                    // set the contentItemList to an empty list and continue on (but with an empty list of ContentChannelItems to use when rending the Lava)
                    contentItemList = new List<ContentChannelItem>();
                }

                var pagination = new Pagination();
                pagination.ItemCount = contentItemList.Count();
                pagination.PageSize = GetAttributeValue( AttributeKey.Count ).AsInteger();
                pagination.CurrentPage = pageNumber;
                pagination.UrlTemplate = pageRef.BuildUrl();
                var currentPageContent = pagination.GetCurrentPageItems( contentItemList );

                var mergeFieldOptions = new Rock.Lava.CommonMergeFieldsOptions();
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

                    var enabledCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );
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
                mergeFields.Add( "ItemTagList", tags );
                mergeFields.Add( "ArchiveSummary", archiveSummaries );
                mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );

                var tagPageRef = new Rock.Web.PageReference( CurrentPageReference );
                tagPageRef.Parameters.AddOrReplace( PageParameterKey.Tag, "TagTemplate" );

                mergeFields.Add( "CurrentPageUrl", tagPageRef.BuildUrl() );

                var archivePageRef = new Rock.Web.PageReference( CurrentPageReference );
                archivePageRef.Parameters.AddOrReplace( PageParameterKey.Year, "YearTemplate" );
                archivePageRef.Parameters.AddOrReplace( PageParameterKey.Month, "MonthTemplate" );

                mergeFields.Add( "ArchiveSummaryPageUrl", archivePageRef.BuildUrl() );

                // TODO: When support for "Person" is not supported anymore (should use "CurrentPerson" instead), remove this line
                mergeFields.TryAdd( "Person", CurrentPerson );

                // set page title
                if ( isSetPageTitleEnabled && contentItemList.Count > 0 )
                {
                    if ( string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.Item ) ) )
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
                    var proxySafeUrl = context.Request.UrlProxySafe();
                    var proxySafeHostName = WebRequestHelper.GetHostNameFromRequest( context );
                    var proxySafePort = proxySafeUrl.Port == 80 ? string.Empty : ":" + proxySafeUrl.Port;
                    var resolvedRockUrl = RockPage.ResolveRockUrl( "~/GetChannelFeed.ashx?ChannelId=" );
                    var contentChannelItem = contentItemList.Select( c => c.ContentChannelId ).FirstOrDefault();
                    var channelRssUrl = $"{proxySafeUrl.Scheme}://{proxySafeHostName}{proxySafePort}{resolvedRockUrl}{contentChannelItem}";

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
                        var proxySafeUri = Request.UrlProxySafe();

                        var imageUrl = FileUrlHelper.GetImageUrl(
                            attributeValue.AsGuid(),
                            new GetImageUrlOptions
                            {
                                PublicAppRoot = $"{proxySafeUri.Scheme}://{proxySafeUri.Authority}/"
                            }
                        );

                        HtmlMeta metaDescription = new HtmlMeta();
                        metaDescription.Name = "og:image";
                        metaDescription.Content = imageUrl;
                        RockPage.Header.Controls.Add( metaDescription );

                        HtmlLink imageLink = new HtmlLink();
                        imageLink.Attributes.Add( "rel", "image_src" );
                        imageLink.Attributes.Add( "href", imageUrl );
                        RockPage.Header.Controls.Add( imageLink );
                    }
                }

                // Render the Lava content.
                var isRendered = true;
                if ( LavaService.RockLiquidIsEnabled )
                {
                    var template = GetTemplate();
                    outputContents = template.Render( Hash.FromDictionary( mergeFields ) );
                }
                else
                {
                    var template = GetLavaTemplate();
                    var lavaContext = LavaService.NewRenderContext( mergeFields, GetAttributeValue( AttributeKey.EnabledLavaCommands ).SplitDelimitedValues() );

                    var renderResult = LavaService.RenderTemplate( template, lavaContext );
                    isRendered = !renderResult.HasErrors;
                    outputContents = renderResult.Text;
                }

                // Cache the result if caching is enabled and the template was rendered successfully.
                if ( isRendered && OutputCacheDuration.HasValue && OutputCacheDuration.Value > 0 )
                {
                    string cacheTags = GetAttributeValue( AttributeKey.CacheTags ) ?? string.Empty;
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
        /// <returns>a Lava Template</returns>
        /// <returns>A <see cref="Rock.Lava.ILavaTemplate"/></returns>
        private ILavaTemplate GetLavaTemplate()
        {
            ILavaTemplate template = null;

            try
            {
                // only load from the cache if a cacheDuration was specified
                if ( ItemCacheDuration.HasValue && ItemCacheDuration.Value > 0 )
                {
                    template = GetCacheItem( TEMPLATE_CACHE_KEY, true ) as ILavaTemplate;
                }

                if ( template == null )
                {
                    var parseResult = LavaService.ParseTemplate( GetAttributeValue( AttributeKey.Template ) );

                    if ( parseResult.HasErrors )
                    {
                        throw parseResult.GetLavaException();
                    }

                    template = parseResult.Template;

                    if ( ItemCacheDuration.HasValue && ItemCacheDuration.Value > 0 )
                    {
                        string cacheTags = GetAttributeValue( AttributeKey.CacheTags ) ?? string.Empty;
                        AddCacheItem( TEMPLATE_CACHE_KEY, template, ItemCacheDuration.Value, cacheTags );
                    }
                }
            }
            catch ( Exception ex )
            {
                var parseResult = LavaService.ParseTemplate( string.Format( "Lava error: {0}", ex.Message ) );

                template = parseResult.Template;
            }

            return template;
        }

        #region RockLiquid Lava implementation

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
                    template = GetCacheItem( TEMPLATE_CACHE_KEY, true ) as Template;
                }

                if ( template == null )
                {
                    template = LavaHelper.CreateDotLiquidTemplate( GetAttributeValue( AttributeKey.Template ) );

                    LavaHelper.VerifyParseTemplateForCurrentEngine( GetAttributeValue( AttributeKey.Template ) );

                    if ( ItemCacheDuration.HasValue && ItemCacheDuration.Value > 0 )
                    {
                        string cacheTags = GetAttributeValue( AttributeKey.CacheTags ) ?? string.Empty;
                        AddCacheItem( TEMPLATE_CACHE_KEY, template, ItemCacheDuration.Value, cacheTags );
                    }

                    var enabledLavaCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );
                    template.Registers.AddOrReplace( "EnabledCommands", enabledLavaCommands );
                }
            }
            catch ( Exception ex )
            {
                template = Template.Parse( string.Format( "Lava error: {0}", ex.Message ) );
            }

            return template;
        }

        #endregion

        /// <summary>
        /// Gets the content channel items from the item-cache (if there), or from 
        /// the configured Channel and any given Item id or filter in the query string
        /// if QueryParameterFiltering is enabled.
        /// </summary>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns> a list of <see cref="Rock.Model.ContentChannelItem">ContentChannelItems</see></returns>
        private ItemContentResults GetContent( bool isQueryParameterFilteringEnabled, bool isTagListEnabled, bool isArchiveSummaryEnabled )
        {
            List<ContentChannelItem> items = null;
            List<TagModel> tags = null;
            List<ArchiveSummaryModel> archiveSummaries = null;

            // only load from the cache if a cacheDuration was specified
            if ( ItemCacheDuration.HasValue && ItemCacheDuration.Value > 0 )
            {
                items = GetCacheItem( CONTENT_CACHE_KEY, true ) as List<ContentChannelItem>;
                tags = GetCacheItem( TAG_CACHE_KEY, true ) as List<TagModel>;
                archiveSummaries = GetCacheItem( MONTH_YEAR_CACHE_KEY, true ) as List<ArchiveSummaryModel>;
            }

            ContentChannelCache contentChannel = null;
            var channelGuid = GetAttributeValue( AttributeKey.Channel ).AsGuidOrNull();
            if ( channelGuid.HasValue )
            {
                contentChannel = ContentChannelCache.Get( channelGuid.Value );
            }

            if ( items == null || ( isQueryParameterFilteringEnabled && Request.QueryString.Count > 0 ) || ( contentChannel != null && contentChannel.EnablePersonalization ) )
            {
                if ( contentChannel != null )
                {
                    var rockContext = new RockContext();
                    var contentChannelItemService = new ContentChannelItemService( rockContext );
                    var itemId = PageParameter( PageParameterKey.Item ).AsIntegerOrNull();


                    var statuses = new List<ContentChannelItemStatus>();
                    var statusValList = ( GetAttributeValue( AttributeKey.Status ) ?? "2" ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                    var dataFilterId = GetAttributeValue( AttributeKey.FilterId ).AsIntegerOrNull();

                    foreach ( string statusVal in statusValList )
                    {
                        var status = statusVal.ConvertToEnumOrNull<ContentChannelItemStatus>();
                        if ( status != null )
                        {
                            statuses.Add( status.Value );
                        }
                    }

                    var contentChannelItemQuery = GetContentChannelItemQuery( rockContext,
                        contentChannelItemService,
                        channelGuid.Value,
                        itemId,
                        dataFilterId,
                        isQueryParameterFilteringEnabled,
                        statuses );

                    if ( isTagListEnabled )
                    {
                        var tagQuery = new TaggedItemService( rockContext )
                            .Queryable()
                            .AsNoTracking()
                            .Include( ti => ti.Tag );

                        var contentChannelItemTagQuery = tagQuery
                            .Where( ti => contentChannelItemQuery.Any( cci => cci.Guid == ti.EntityGuid ) )
                            .GroupBy( ti => new { ti.Tag.Id, ti.Tag.Guid, ti.Tag.Name } )
                            .Select( group => new { Tag = group.Key, Count = group.Count() } )
                            .Select( tag => new TagModel() { Id = tag.Tag.Id, Guid = tag.Tag.Guid, Name = tag.Tag.Name, Count = tag.Count } );

                        tags = contentChannelItemTagQuery.ToList();

                        var selectedTag = PageParameter( PageParameterKey.Tag );

                        if ( !string.IsNullOrWhiteSpace( selectedTag ) && selectedTag.ToLower() != "all" )
                        {
                            contentChannelItemQuery = contentChannelItemQuery.Where( cci => tagQuery.Any( t => t.Tag.Name == selectedTag && t.EntityGuid == cci.Guid ) );
                        }
                    }

                    if ( isArchiveSummaryEnabled )
                    {
                        archiveSummaries = contentChannelItemQuery
                            .GroupBy( cci => new
                            {
                                cci.StartDateTime.Month,
                                cci.StartDateTime.Year
                            } )
                            .Select( cci => new ArchiveSummaryModel
                            {
                                Month = cci.Key.Month,
                                Year = cci.Key.Year,
                                Count = cci.Count()
                            } )
                            .ToList();

                        archiveSummaries.ForEach( cci => cci.MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName( cci.Month ) );

                        var selectedYear = PageParameter( PageParameterKey.Year ).AsIntegerOrNull();
                        var selectedMonth = PageParameter( PageParameterKey.Month ).AsIntegerOrNull();

                        if ( selectedYear != null )
                        {
                            contentChannelItemQuery = contentChannelItemQuery
                                .Where( cci => cci.StartDateTime.Year == selectedYear.Value );
                        }

                        if ( selectedMonth != null )
                        {
                            contentChannelItemQuery = contentChannelItemQuery
                                .Where( cci => cci.StartDateTime.Month == selectedMonth.Value );
                        }
                    }

                    IQueryable<ContentChannelItem> matchedContentChannelItemQry, nonMatchedContentChannelItemQry = null;
                    var isNonMatchedContentChannelItemExists = false;
                    if ( contentChannel.EnablePersonalization )
                    {
                        /*  08-18-2022 SK
                            The setting determines how personalization effect the results shown.
                            Ignore will not consider segments or request filters,
                            Prioritize will add items with matching items to the top of the list (in order by the sort order) and
                            Filter will only show items that match the current individuals segments and request filters.
                        */
                        var personalizationFilterType = GetAttributeValue( AttributeKey.Personalization ).ConvertToEnum<PersonalizationFilterType>( PersonalizationFilterType.Ignore );
                        var personalizationSegmentIds = new List<int>();
                        if ( RockPage.PersonalizationSegmentIds != null )
                        {
                            //Get all the valid Personalization Segment Ids for the Current User.
                            personalizationSegmentIds = RockPage.PersonalizationSegmentIds.ToList();
                        }

                        var requestFilterIds = new List<int>();
                        if ( RockPage.PersonalizationRequestFilterIds != null )
                        {
                            //Get all the valid Personalization Request Filter Ids for the Current User.
                            requestFilterIds = RockPage.PersonalizationRequestFilterIds.ToList();
                        }

                        if ( personalizationFilterType == PersonalizationFilterType.Ignore )
                        {
                            matchedContentChannelItemQry = contentChannelItemQuery;
                        }
                        else
                        {
                            /*
                                This will return all the entity Ids with PersonalizationType as Segment and Entity Type Id of Content Channel Item
                                which will help further to include content Channel Items that has no Segment associated with it.
                             */
                            var allPersonalizedSegmentEntityIdsQry = GetPersonalizedEntityIdsQry( rockContext, PersonalizationType.Segment );
                            var matchedSegmentEntityIdsQry = GetPersonalizedEntityIdsQry( rockContext, PersonalizationType.Segment, personalizationSegmentIds );

                            /*
                                This will return all the entity Ids with PersonalizationType as RequestFilter and Entity Type Id of Content Channel Item
                                which will help further to include content Channel Items that has no Request Filter associated with it.
                             */
                            var allPersonalizedRequestFilterEntityIdsQry = GetPersonalizedEntityIdsQry( rockContext, PersonalizationType.RequestFilter );
                            var matchedRequestFilterEntityIdsQry = GetPersonalizedEntityIdsQry( rockContext, PersonalizationType.RequestFilter, requestFilterIds );
                            if ( personalizationFilterType == PersonalizationFilterType.Filter )
                            {
                                /*
                                    This will return either the contentChannelItem that has no associated Personalized Segment defined OR
                                    items with matching personalization segments.
                                    Similar filter is also applied related to Request Filter in consequent lines.
                                */
                                contentChannelItemQuery = contentChannelItemQuery.Where( cci => ( !allPersonalizedSegmentEntityIdsQry.Contains( cci.Id ) || matchedSegmentEntityIdsQry.Contains( cci.Id ) ) );
                                contentChannelItemQuery = contentChannelItemQuery.Where( cci => ( !allPersonalizedRequestFilterEntityIdsQry.Contains( cci.Id ) || matchedRequestFilterEntityIdsQry.Contains( cci.Id ) ) );
                                matchedContentChannelItemQry = contentChannelItemQuery;
                            }
                            else
                            {
                                /*
                                    In Prioritize, matching result set will have following items - 
                                    At least one of the Segment as well as Request Filters are matched with person
                                                                    OR
                                    Either Content Channel Item's Segment OR Request Filter are matched with person AND the other Personalization Filter Type which is not matched have nothing selected (Acts as WildCard).
                                    Note:- In Prioritize, we need to include both matching as well as non matching result set. Non matching records has to be appended at the
                                    end of the result set.
                                 */
                                isNonMatchedContentChannelItemExists = true;
                                var matchedPredicate = LinqPredicateBuilder.False<ContentChannelItem>();
                                matchedPredicate = matchedPredicate.Or( cci => matchedSegmentEntityIdsQry.Contains( cci.Id ) && ( matchedRequestFilterEntityIdsQry.Contains( cci.Id ) || !allPersonalizedRequestFilterEntityIdsQry.Contains( cci.Id ) ) );
                                matchedPredicate = matchedPredicate.Or( cci => !allPersonalizedSegmentEntityIdsQry.Contains( cci.Id ) && matchedRequestFilterEntityIdsQry.Contains( cci.Id ) );
                                matchedContentChannelItemQry = contentChannelItemQuery.Where( matchedPredicate );
                                nonMatchedContentChannelItemQry = contentChannelItemQuery.Where( matchedPredicate.Not() );
                            }
                        }
                    }
                    else
                    {
                        matchedContentChannelItemQry = contentChannelItemQuery;
                    }

                    // GetContentChannelItems will return the content channel items after checking authorization and applying all the ordering.
                    items = GetContentChannelItems( rockContext, matchedContentChannelItemQry );

                    /*
                       isNonMatchedContentChannelItemExists variable will only be true if Prioritize is selected as FilterType for either on Segment Or Request Filters.
                       which states to include non matching records at the end.
                    */
                    if ( isNonMatchedContentChannelItemExists )
                    {
                        var nonMatchedContentChannelItems = GetContentChannelItems( rockContext, nonMatchedContentChannelItemQry );
                        if ( nonMatchedContentChannelItems.Any() )
                        {
                            items.AddRange( nonMatchedContentChannelItems );
                        }
                    }

                    if ( ItemCacheDuration.HasValue && ItemCacheDuration.Value > 0 && !isQueryParameterFilteringEnabled && !contentChannel.EnablePersonalization )
                    {
                        string cacheTags = GetAttributeValue( AttributeKey.CacheTags ) ?? string.Empty;
                        AddCacheItem( CONTENT_CACHE_KEY, items, ItemCacheDuration.Value, cacheTags );
                        AddCacheItem( TAG_CACHE_KEY, tags ?? new List<TagModel>(), ItemCacheDuration.Value, cacheTags );
                        AddCacheItem( MONTH_YEAR_CACHE_KEY, archiveSummaries ?? new List<ArchiveSummaryModel>(), ItemCacheDuration.Value, cacheTags );
                    }
                }
            }

            return new ItemContentResults { Items = items, Tags = tags, ArchiveSumaries = archiveSummaries };
        }

        /// <summary>
        /// Gets the content channel items from the content channel item query after checking authorization, applying context filtering and ordering all the items.
        /// </summary>
        private List<ContentChannelItem> GetContentChannelItems( RockContext rockContext, IQueryable<ContentChannelItem> contentChannelItemQuery )
        {
            var items = new List<ContentChannelItem>( contentChannelItemQuery.Count() );
            // All queryable filtering has been added, now run query, check security and load attributes
            foreach ( var item in contentChannelItemQuery )
            {
                if ( item.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    item.LoadAttributes( rockContext );
                    items.Add( item );
                }
            }

            // Apply context filter, now that we've loaded the items' attributes.
            items = ApplyContextFilter( items );

            // Order the items
            string orderBy = GetAttributeValue( AttributeKey.Order );
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

                var sortProperty = new SortProperty();
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

                if ( orderedQry != null )
                {
                    items = orderedQry.ToList();
                }
            }

            return items;
        }

        /// <summary>
        /// Applies the context filter if set and the block has a context entity.
        /// </summary>
        /// <param name="items">The items to filter.</param>
        /// <returns>The filtered items.</returns>
        private List<ContentChannelItem> ApplyContextFilter( List<ContentChannelItem> items )
        {
            var contextFilterAttributeKey = GetAttributeValue( AttributeKey.ContextAttribute );
            if ( contextFilterAttributeKey.IsNullOrWhiteSpace() )
            {
                return items;
            }

            var contextEntityGuid = this.ContextEntity()?.Guid;
            if ( !contextEntityGuid.HasValue )
            {
                return items;
            }

            return items.Where( i =>
                i.AttributeValues.Any( av =>
                {
                    if ( av.Key != contextFilterAttributeKey )
                    {
                        return false;
                    }

                    var guids = av.Value?.Value.SplitDelimitedValues().AsGuidList();

                    return guids?.Any( g => g.Equals( contextEntityGuid ) ) == true;
                } )
            ).ToList();
        }

        /// <summary>
        /// Gets the personalized entity query.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="personalizationType">The personalization type.</param>
        /// <param name="segmentIds">The segment identifiers.</param>
        private IQueryable<int> GetPersonalizedEntityIdsQry( RockContext rockContext, PersonalizationType personalizationType, List<int> segmentIds )
        {
            var entityTypeId = EntityTypeCache.Get<Rock.Model.ContentChannelItem>().Id;
            return ( rockContext ).Set<PersonalizedEntity>()
                .Where( pe => pe.PersonalizationType == personalizationType && pe.EntityTypeId == entityTypeId && segmentIds.Contains( pe.PersonalizationEntityId ) )
                .Select( a => a.EntityId );
        }

        /// <summary>
        /// Gets the personalized entity identifiers query.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="personalizationType">The personalization type.</param>
        private IQueryable<int> GetPersonalizedEntityIdsQry( RockContext rockContext, PersonalizationType personalizationType )
        {
            var entityTypeId = EntityTypeCache.Get<Rock.Model.ContentChannelItem>().Id;
            return ( rockContext ).Set<PersonalizedEntity>()
                .Where( pe => pe.PersonalizationType == personalizationType && pe.EntityTypeId == entityTypeId )
                .Select( a => a.EntityId );
        }

        private IQueryable<ContentChannelItem> GetContentChannelItemQuery( RockContext rockContext,
            ContentChannelItemService contentChannelItemService,
            Guid channelGuid,
            int? itemId,
            int? dataFilterId,
            bool isQueryParameterFilteringEnabled,
            List<ContentChannelItemStatus> statuses
            )
        {
            var contentChannelInfo = new ContentChannelService( rockContext ).GetSelect( channelGuid, s => new { s.Id, s.RequiresApproval, ContentChannelTypeDisableStatus = s.ContentChannelType.DisableStatus } );
            if ( contentChannelInfo == null )
            {
                return null;
            }

            var contentChannelItemQuery = contentChannelItemService
                        .Queryable()
                        .Include( a => a.ContentChannel )
                        .Include( a => a.ContentChannelType )
                        .Include( a => a.ContentChannelItemSlugs )
                        .Where( i => i.ContentChannelId == contentChannelInfo.Id );

            if ( isQueryParameterFilteringEnabled && itemId.HasValue )
            {
                contentChannelItemQuery = contentChannelItemQuery.Where( i => i.Id == itemId.Value );
            }

            if ( contentChannelInfo.RequiresApproval && !contentChannelInfo.ContentChannelTypeDisableStatus )
            {
                if ( statuses.Any() )
                {
                    contentChannelItemQuery = contentChannelItemQuery.Where( i => statuses.Contains( i.Status ) );
                }
            }

            var itemType = typeof( Rock.Model.ContentChannelItem );
            var paramExpression = contentChannelItemService.ParameterExpression;

            try
            {
                if ( dataFilterId.HasValue )
                {
                    var dataFilterService = new DataViewFilterService( rockContext );
                    var dataFilter = dataFilterService.Queryable( "ChildFilters" ).FirstOrDefault( a => a.Id == dataFilterId.Value );
                    Expression whereExpression = dataFilter != null ? dataFilter.GetExpression( itemType, contentChannelItemService, paramExpression ) : null;

                    contentChannelItemQuery = contentChannelItemQuery.Where( paramExpression, whereExpression, null );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                //Don't choke on the filter.
            }

            // If items could be filtered by querystring values, check for filters
            if ( isQueryParameterFilteringEnabled )
            {
                var pageParameters = PageParameters();
                if ( pageParameters.Count > 0 )
                {
                    var propertyFilter = new Rock.Reporting.DataFilter.PropertyFilter();

                    //var itemIdList = items.Select( a => a.Id ).ToList();
                    var queryParameterContentChannelItemQuery = contentChannelItemService
                        .Queryable()
                        .Where( a => contentChannelItemQuery.Any( b => b.Id == a.Id ) );
                    foreach ( string fieldParameterKey in PageParameters().Select( p => p.Key ) )
                    {
                        Expression queryParameterFilteringExpression = null;

                        // Just in case this EntityType has multiple attributes with the same key (or also a property name),
                        // create a OR'd clause for each attribute that has this key, plus any property with a matching name
                        var entityFieldList = Rock.Reporting.EntityHelper.FindFromFieldName( itemType, fieldParameterKey );

                        foreach ( var entityField in entityFieldList )
                        {
                            var selection = new List<string>();
                            selection.Add( entityField.UniqueName );

                            /* 2020-09-11 MDP
                             * We'll get a list of the supported comparison types of the Field (each Rock.Field.IField has a property that defines which comparison types it supports).
                             * - In DataViewDetail this would determine what type of Comparison Control to use (Drop Down, just the word 'is', etc). In the case of a DropDown, the first
                             * one in the drop down is the default comparison type.
                             * - In DynamicReport, the Comparison control is not visible, so that always ends up using the default comparison type of that IFieldType.
                             * 
                             * So for ContentChannelView, we'll use the exact same way to determine the Comparison type (use the first/default comparison type that the field type supports.
                             */

                            var supportedComparisonTypes = entityField.FieldType.Field.FilterComparisonType;
                            ComparisonType defaultComparisonType = ComparisonType.EqualTo;
                            foreach ( ComparisonType comparisonType in typeof( ComparisonType ).GetOrderedValues<ComparisonType>() )
                            {
                                if ( ( supportedComparisonTypes & comparisonType ) == comparisonType )
                                {
                                    defaultComparisonType = comparisonType;
                                    break;
                                }
                            }

                            string value = PageParameter( fieldParameterKey );
                            selection.Add( defaultComparisonType.ConvertToInt().ToString() );
                            selection.Add( value );

                            var entityFieldExpression = propertyFilter.GetExpression( itemType, contentChannelItemService, paramExpression, Newtonsoft.Json.JsonConvert.SerializeObject( selection ) );

                            if ( queryParameterFilteringExpression == null )
                            {
                                queryParameterFilteringExpression = entityFieldExpression;
                            }
                            else
                            {
                                queryParameterFilteringExpression = Expression.OrElse( queryParameterFilteringExpression, entityFieldExpression );
                            }
                        }

                        if ( queryParameterFilteringExpression != null )
                        {
                            queryParameterContentChannelItemQuery = queryParameterContentChannelItemQuery.Where( paramExpression, queryParameterFilteringExpression );
                        }
                    }

                    return queryParameterContentChannelItemQuery;
                }
            }

            return contentChannelItemQuery;
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

                    rblPersonalization.Visible = channel.EnablePersonalization;
                    if ( channel.EnablePersonalization )
                    {
                        rblPersonalization.BindToEnum<PersonalizationFilterType>();
                        rblPersonalization.SetValue( ( int ) GetAttributeValue( AttributeKey.Personalization ).AsInteger() );
                    }

                    // add attributes to the attribute lists
                    ddlContextAttribute.Items.Clear();
                    ddlMetaDescriptionAttribute.Items.Clear();
                    ddlMetaImageAttribute.Items.Clear();
                    ddlContextAttribute.Items.Add( "" );
                    ddlMetaDescriptionAttribute.Items.Add( "" );
                    ddlMetaImageAttribute.Items.Add( "" );

                    var currentContextAttribute = GetAttributeValue( AttributeKey.ContextAttribute ) ?? string.Empty;
                    var currentMetaDescriptionAttribute = GetAttributeValue( AttributeKey.MetaDescriptionAttribute ) ?? string.Empty;
                    var currentMetaImageAttribute = GetAttributeValue( AttributeKey.MetaImageAttribute ) ?? string.Empty;

                    // add channel attributes
                    channel.LoadAttributes();
                    foreach ( var attribute in channel.Attributes )
                    {
                        var field = attribute.Value.FieldType.Field;
                        var computedKey = "C^" + attribute.Key;

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
                        ddlContextAttribute.Items.Add( new ListItem( attribute.Name, attribute.Key ) );

                        var attrKey = "Attribute:" + attribute.Key;
                        if ( !kvlOrder.CustomKeys.ContainsKey( attrKey ) )
                        {
                            kvlOrder.CustomKeys.Add( "Attribute:" + attribute.Key, attribute.Name );

                            var computedKey = "I^" + attribute.Key;
                            ddlMetaDescriptionAttribute.Items.Add( new ListItem( "Item: " + attribute.Name, computedKey ) );

                            var field = attribute.FieldType.Name;

                            if ( field == "Image" )
                            {
                                ddlMetaImageAttribute.Items.Add( new ListItem( "Item: " + attribute.Name, computedKey ) );
                            }
                        }
                    }

                    // select attributes
                    SetListValue( ddlContextAttribute, currentContextAttribute );
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
                        filterControl.SetSelection( filter.Selection );
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
        /// Personalization Filter Type
        /// </summary>
        private enum PersonalizationFilterType
        {
            /// <summary>
            /// The ignore
            /// </summary>
            Ignore = 0,

            /// <summary>
            /// The prioritize
            /// </summary>
            Prioritize = 1,

            /// <summary>
            /// The filter
            /// </summary>
            Filter = 2
        }

        private class TagModel : RockDynamic
        {
            public int Id { get; set; }
            public Guid Guid { get; set; }
            public string Name { get; set; }
            public int Count { get; set; }
        }

        private class ArchiveSummaryModel : DotLiquid.Drop
        {
            public int Month { get; set; }
            public string MonthName { get; set; }
            public int Year { get; set; }
            public int Count { get; set; }
        }

        private class ItemContentResults
        {
            public List<ContentChannelItem> Items { get; set; }
            public List<TagModel> Tags { get; set; }
            public List<ArchiveSummaryModel> ArchiveSumaries { get; set; }
        }

        public class Pagination : RockDynamic
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
        public class PaginationPage : RockDynamic
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