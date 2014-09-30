// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Caching;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using DotLiquid;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Block to display content items, html, xml, or transformed xml based on a SQL query or stored procedure.
    /// </summary>
    [DisplayName( "Content Channel Dynamic" )]
    [Category( "CMS" )]
    [Description( "Block to display dynamic content channel items." )]

    [LinkedPage( "Detail Page", "The page to navigate to for details.", false, "", "", 0 )]
    [ContentChannelField( "Channel", "The channel to display items from.", false, "", "Advanced", 1 )]
    [CodeEditorField( "Template", "The template to use when formatting the list of items.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 600, false, @"", "Advanced", 2 )]
    [IntegerField( "Count", "The maximum number of items to display.", false, 5, "Advanced", 3 )]
    [IntegerField( "Cache Duration", "Number of seconds to cache the content.", false, 3600, "Advanced", 4 )]
    [BooleanField( "Enable Debug", "Enabling debug will display the fields of the first 5 items to help show you wants available for your liquid.", false, "Advanced", 5 )]
    [IntegerField( "Filter Id", "The data filter that is used to filter items", false, 0, "Advanced", 6 )]

    public partial class ContentChannelDynamic : RockBlock
    {
        #region Fields

        private readonly string ITEM_TYPE_NAME = "Rock.Model.ContentChannelItem";
        private readonly string CONTENT_CACHE_KEY = "Content";
        private readonly string TEMPLATE_CACHE_KEY = "Template";

        #endregion

        #region Properties

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Switch does not automatically initialize again after a partial-postback.  This script 
            // looks for any switch elements that have not been initialized and re-intializes them.
            string script = @"
$(document).ready(function() {
    $('.switch > input').each( function () {
        $(this).parent().switch('init');
    });
});
";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "toggle-switch-init", script, true );

            this.BlockUpdated += ContentDynamic_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
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

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            RockContext rockContext = new RockContext();
            CreateFilterControl( DataViewFilter.FromJson( ViewState["DataViewFilter"].ToString() ), false, rockContext );
        }

        protected override object SaveViewState()
        {
            ViewState["DataViewFilter"] = GetFilterControl().ToJson();
            return base.SaveViewState();
        }

        public override List<Control> GetAdministrateControls( bool canConfig, bool canEdit )
        {
            List<Control> configControls = new List<Control>();

            if ( IsUserAuthorized( Authorization.EDIT ) )
            {
                LinkButton lbEdit = new LinkButton();
                lbEdit.CssClass = "edit";
                lbEdit.ToolTip = "Edit Criteria";
                lbEdit.Click += lbEdit_Click;
                configControls.Add( lbEdit );
                HtmlGenericControl iEdit = new HtmlGenericControl( "i" );
                lbEdit.Controls.Add( iEdit );
                lbEdit.CausesValidation = false;
                iEdit.Attributes.Add( "class", "fa fa-pencil-square-o" );

                // will toggle the block config so they are no longer showing
                lbEdit.Attributes["onclick"] = "Rock.admin.pageAdmin.showBlockConfig()";

                ScriptManager.GetCurrent( this.Page ).RegisterAsyncPostBackControl( lbEdit );
            }

            configControls.AddRange( base.GetAdministrateControls( canConfig, canEdit ) );

            return configControls;
        }

        #endregion

        #region Events

        protected void lbEdit_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            ddlChannel.DataSource = new ContentChannelService( rockContext ).Queryable()
                .OrderBy( c => c.Name )
                .Select( c => new { c.Guid, c.Name } )
                .ToList();
            ddlChannel.DataBind();
            ddlChannel.SetValue( GetAttributeValue( "Channel" ) );

            cbDebug.Checked = GetAttributeValue( "EnableDebug" ).AsBoolean();
            ceQuery.Text = GetAttributeValue( "Template" );
            nbCount.Text = GetAttributeValue( "Count" );
            nbCacheDuration.Text = GetAttributeValue( "CacheDuration" );

            hfChannelGuid.Value = ddlChannel.SelectedValue;
            ShowEdit( GetAttributeValue( "FilterId" ) );

            pnlView.Visible = false;
            pnlEdit.Visible = true;
            upnlContent.Update();
        }

        void ContentDynamic_BlockUpdated( object sender, EventArgs e )
        {
            ShowView();
        }

        protected void ddlChannel_SelectedIndexChanged( object sender, EventArgs e )
        {
            hfChannelGuid.Value = ddlChannel.SelectedValue;
            ShowEdit( hfDataFilterId.Value );
        }

        protected void lbSave_Click( object sender, EventArgs e )
        {

            var dataViewFilter = GetFilterControl();

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

            SetAttributeValue( "Channel", ddlChannel.SelectedValue );
            SetAttributeValue( "EnableDebug", cbDebug.Checked.ToString() );
            SetAttributeValue( "Template", ceQuery.Text );
            SetAttributeValue( "Count", ( nbCount.Text.AsIntegerOrNull() ?? 5 ).ToString() );
            SetAttributeValue( "CacheDuration", ( nbCacheDuration.Text.AsIntegerOrNull() ?? 5 ).ToString() );
            SetAttributeValue( "FilterId", dataViewFilter.Id.ToString() );
            SaveAttributeValues();

            FlushCacheItem( CONTENT_CACHE_KEY );
            FlushCacheItem( TEMPLATE_CACHE_KEY );

            ShowView();
        }

        protected void lbCancel_Click( object sender, EventArgs e )
        {
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
            filterField.DataViewFilterGuid = Guid.NewGuid();
            groupControl.Controls.Add( filterField );
            filterField.ID = string.Format( "ff_{0}", filterField.DataViewFilterGuid.ToString( "N" ) );
            filterField.FilteredEntityTypeName = groupControl.FilteredEntityTypeName;
            filterField.Expanded = true;
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

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void ShowView()
        {
            pnlEdit.Visible = false;
            pnlView.Visible = true;
            upnlContent.Update();

            var pageRef = CurrentPageReference;
            pageRef.Parameters.AddOrReplace( "Page", "{0}" );

            Dictionary<string, object> linkedPages = new Dictionary<string, object>();
            linkedPages.Add( "DetailPage", LinkedPageUrl( "DetailPage", null ) );
                
            var content = GetContent();
            var pagination = new Pagination();
            pagination.ItemCount = content.Count();
            pagination.PageSize = GetAttributeValue( "Count" ).AsInteger();
            pagination.CurrentPage = PageParameter( "Page" ).AsIntegerOrNull() ?? 1;
            pagination.UrlTemplate = pageRef.BuildUrl();
            var currentPageContent = pagination.GetCurrentPageItems( content );

            var globalAttributeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );
            var mergeFields = new Dictionary<string, object>();
            if ( CurrentPerson != null )
            {
                mergeFields.Add( "Pagination", pagination );
                mergeFields.Add( "LinkedPages", linkedPages );
                mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );
                mergeFields.Add( "Items", currentPageContent );
                mergeFields.Add( "Campuses", CampusCache.All() );
                mergeFields.Add( "Person", CurrentPerson );
                globalAttributeFields.ToList().ForEach( d => mergeFields.Add( d.Key, d.Value ) );
            }

            // enable showing debug info
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() )
            {
                var debugFields = new Dictionary<string, object>();
                if ( CurrentPerson != null )
                {
                    debugFields.Add( "Pagination", pagination );
                    debugFields.Add( "LinkedPages", linkedPages );
                    debugFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );
                    debugFields.Add( "Items", currentPageContent.Take( 5 ).ToList() );
                    debugFields.Add( "Campuses", CampusCache.All() );
                    debugFields.Add( "Person", CurrentPerson );
                    globalAttributeFields.ToList().ForEach( d => debugFields.Add( d.Key, d.Value ) );
                }

                lDebug.Visible = true;
                StringBuilder debugInfo = new StringBuilder();
                debugInfo.Append( "<div class='alert alert-info'><h4>Debug Info</h4>" );
                debugInfo.Append( "<p><em>Showing first 5 groups.</em></p>" );
                debugInfo.Append( "<pre>" + debugFields.LiquidizeChildren().ToJson() + "</pre>" );
                debugInfo.Append( "</div" );
                lDebug.Text = debugInfo.ToString();
            }
            else
            {
                lDebug.Visible = false;
                lDebug.Text = string.Empty;
            }

            var template = GetTemplate();

            phContent.Controls.Add( new LiteralControl( template.Render( Hash.FromDictionary( mergeFields ) ) ) );
        }

        private Template GetTemplate()
        {
            var template = GetCacheItem( TEMPLATE_CACHE_KEY ) as Template;
            if ( template == null )
            {
                Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
                template = Template.Parse( GetAttributeValue( "Template" ) );

                int? cacheDuration = GetAttributeValue( "CacheDuration" ).AsInteger();
                if ( cacheDuration > 0 )
                {
                    var cacheItemPolicy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddSeconds( cacheDuration.Value ) };
                    AddCacheItem( TEMPLATE_CACHE_KEY, template, cacheItemPolicy );
                }
            }

            return template;
        }

        private List<ContentChannelItem> GetContent()
        {
            var items = GetCacheItem( CONTENT_CACHE_KEY ) as List<ContentChannelItem>;

            if ( items == null )
            {
                items = new List<ContentChannelItem>();

                Guid? channelGuid = GetAttributeValue( "Channel" ).AsGuidOrNull();
                if ( channelGuid.HasValue )
                {
                    var rockContext = new RockContext();
                    var service = new ContentChannelItemService( rockContext );

                    var contentChannel = new ContentChannelService( rockContext ).Get( channelGuid.Value );
                    if ( contentChannel != null )
                    {
                        var qry = service.Queryable( "ContentChannel,ContentChannelType" );

                        int? itemId = PageParameter( "Item" ).AsIntegerOrNull();
                        if ( itemId.HasValue )
                        {
                            qry = qry.Where( i => i.Id == itemId.Value );
                        }
                        else
                        {
                            int? dataFilterId = GetAttributeValue( "FilterId" ).AsIntegerOrNull();
                            if ( dataFilterId.HasValue )
                            {
                                var dataFilterService = new DataViewFilterService( rockContext );
                                var dataFilter = dataFilterService.Get( dataFilterId.Value );

                                var errorMessages = new List<string>();
                                ParameterExpression paramExpression = service.ParameterExpression;
                                Expression whereExpression = dataFilter != null ? dataFilter.GetExpression( typeof( Rock.Model.ContentChannelItem ), service, paramExpression, errorMessages ) : null;

                                qry = qry.Where( paramExpression, whereExpression );
                            }
                            else
                            {
                                // Create a default query that approved, active items of this channel type 
                                var now = RockDateTime.Now;
                                qry = qry.Where( i =>
                                        i.Status == ContentChannelItemStatus.Approved &&
                                        i.ContentChannelId == contentChannel.Id &&
                                        i.StartDateTime.CompareTo( now ) <= 0 &&
                                        ( !i.ExpireDateTime.HasValue || i.ExpireDateTime.Value.CompareTo( now ) > 0 )
                                    );
                            }
                        }

                        // All filtering has been added, now run query and then check security and load attributes
                        foreach ( var item in qry.ToList() )
                        {
                            if ( item.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                            {
                                item.LoadAttributes( rockContext );
                                items.Add( item );
                            }
                        }

                        return items
                            .OrderBy( i => i.Priority )
                            .ThenByDescending( i => i.ExpireDateTime )
                            .ThenByDescending( i => i.StartDateTime )
                            .ToList();
                    }
                }

                int? cacheDuration = GetAttributeValue( "CacheDuration" ).AsInteger();
                if ( cacheDuration > 0 )
                {
                    var cacheItemPolicy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddSeconds( cacheDuration.Value ) };
                    AddCacheItem( CONTENT_CACHE_KEY, items, cacheItemPolicy );
                }
            }

            return items;
        }

        public void ShowEdit( string filterId )
        {
            var rockContext = new RockContext();
            var filterService = new DataViewFilterService( rockContext );
            DataViewFilter filter = null;

            int? dataFilterId = filterId.AsIntegerOrNull();
            if ( dataFilterId.HasValue )
            {
                filter = filterService.Get( dataFilterId.Value );
            }

            if ( filter == null || filter.ExpressionType == FilterExpressionType.Filter )
            {
                filter = new DataViewFilter();
                filter.Guid = new Guid();
                filter.ExpressionType = FilterExpressionType.GroupAll;
            }

            CreateFilterControl( filter, true, rockContext );
        }

        private void CreateFilterControl( DataViewFilter filter, bool setSelection, RockContext rockContext )
        {
            phFilters.Controls.Clear();
            if ( filter != null )
            {
                CreateFilterControl( phFilters, filter, setSelection, rockContext );
            }
        }

        private void CreateFilterControl( Control parentControl, DataViewFilter filter, bool setSelection, RockContext rockContext )
        {
            if ( filter.ExpressionType == FilterExpressionType.Filter )
            {
                var filterControl = new FilterField();
                parentControl.Controls.Add( filterControl );
                filterControl.DataViewFilterGuid = filter.Guid;
                filterControl.ID = string.Format( "ff_{0}", filterControl.DataViewFilterGuid.ToString( "N" ) );
                filterControl.FilteredEntityTypeName = ITEM_TYPE_NAME;
                if ( filter.EntityTypeId.HasValue )
                {
                    var entityTypeCache = Rock.Web.Cache.EntityTypeCache.Read( filter.EntityTypeId.Value, rockContext );
                    if ( entityTypeCache != null )
                    {
                        filterControl.FilterEntityTypeName = entityTypeCache.Name;
                    }
                }

                filterControl.Expanded = filter.Expanded;
                if ( setSelection )
                {
                    filterControl.Selection = filter.Selection;
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
                    CreateFilterControl( groupControl, childFilter, setSelection, rockContext );
                }
            }
        }

        private DataViewFilter GetFilterControl()
        {
            if ( phFilters.Controls.Count > 0 )
            {
                return GetFilterControl( phFilters.Controls[0] );
            }

            return null;
        }

        private DataViewFilter GetFilterControl( Control control )
        {
            FilterGroup groupControl = control as FilterGroup;
            if ( groupControl != null )
            {
                return GetFilterGroupControl( groupControl );
            }

            FilterField filterControl = control as FilterField;
            if ( filterControl != null )
            {
                return GetFilterFieldControl( filterControl );
            }

            return null;
        }

        private DataViewFilter GetFilterGroupControl( FilterGroup filterGroup )
        {
            DataViewFilter filter = new DataViewFilter();
            filter.Guid = filterGroup.DataViewFilterGuid;
            filter.ExpressionType = filterGroup.FilterType;
            foreach ( Control control in filterGroup.Controls )
            {
                DataViewFilter childFilter = GetFilterControl( control );
                if ( childFilter != null )
                {
                    filter.ChildFilters.Add( childFilter );
                }
            }

            return filter;
        }

        private DataViewFilter GetFilterFieldControl( FilterField filterField )
        {
            DataViewFilter filter = new DataViewFilter();
            filter.Guid = filterField.DataViewFilterGuid;
            filter.ExpressionType = FilterExpressionType.Filter;
            filter.Expanded = filterField.Expanded;
            if ( filterField.FilterEntityTypeName != null )
            {
                filter.EntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( filterField.FilterEntityTypeName ).Id;
                filter.Selection = filterField.Selection;
            }

            return filter;
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
                    return Convert.ToInt32( Math.Abs( ItemCount / PageSize ) ) +
                        ( ( ItemCount % PageSize ) > 0 ? 1 : 0 );
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
            public PaginationPage( string urlTemplate, int pageNumber)
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
                    if ( !string.IsNullOrWhiteSpace( UrlTemplate ) && UrlTemplate.Contains( "{0}"))
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