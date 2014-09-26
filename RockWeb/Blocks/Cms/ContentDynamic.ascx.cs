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
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using DotLiquid;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Block to display content items, html, xml, or transformed xml based on a SQL query or stored procedure.
    /// </summary>
    [DisplayName( "Content" )]
    [Category( "CMS" )]
    [Description( "Block to display content channel items." )]

    [ContentChannelField( "Channel", "The channel to display items from.", false, "", "", 0 )]
    [EnumsField( "Status", "Include items with the following status.", typeof( ContentItemStatus ), false, "2", "", 1 )]
    [MemoField( "Filters", "The filters to use when quering items.", false, "", "", 2 )]
    [TextField( "Order", "The specifics of how items should be ordered. This value is set through configuration and should not be modified here.", false, "Priority^ASC|Expire^DESC|Start^DESC", "", 3 )]
    [IntegerField( "Count", "The maximum number of items to display.", false, 5, "", 5 )]
    [IntegerField( "Cache Duration", "Number of seconds to cache the content.", false, 3600, "", 6 )]
    [LinkedPage( "Detail Page", "The page to navigate to for details.", false, "", "", 7 )]
    [CodeEditorField( "Template", "The template to use when formatting the list of items.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 600, false, @"
", "", 8 )]
    [BooleanField( "Enable Debug", "Enabling debug will display the fields of the first 5 items to help show you wants available for your liquid.", false, "", 9 )]

    public partial class ContentDynamic : RockBlock
    {
        #region Fields

        private readonly string contentCacheKey = "Content";
        private readonly string templateCacheKey = "Template";

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
            else
            {
                if ( pnlEdit.Visible )
                {
                    BuildDynamicEditControls( false );
                }
            }
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

            cblStatus.BindToEnum<ContentItemStatus>();
            foreach ( string status in GetAttributeValue( "Status" ).SplitDelimitedValues() )
            {
                var li = cblStatus.Items.FindByValue( status );
                if ( li != null )
                {
                    li.Selected = true;
                }
            }

            nbCount.Text = GetAttributeValue( "Count" );
            nbCacheDuration.Text = GetAttributeValue( "CacheDuration" );

            kvlFilter.Value = GetAttributeValue( "Filters" );

            var directions = new Dictionary<string, string>();
            directions.Add( "ASC", "Ascending" );
            directions.Add( "DESC", "Descending" );
            kvlOrder.CustomValues = directions;
            kvlOrder.Value = GetAttributeValue( "Order" );

            ceQuery.Text = GetAttributeValue( "Template" );
            cbDebug.Checked = GetAttributeValue( "EnableDebug" ).AsBoolean();

            hfChannelGuid.Value = ddlChannel.SelectedValue;
            BuildDynamicEditControls( true );

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
            BuildDynamicEditControls( true );
        }

        protected void lbSave_Click( object sender, EventArgs e )
        {
            SetAttributeValue( "Channel", ddlChannel.SelectedValue );
            SetAttributeValue( "Template", ceQuery.Text );
            SetAttributeValue( "EnableDebug", cbDebug.Checked.ToString() );
            SetAttributeValue( "Count", ( nbCount.Text.AsIntegerOrNull() ?? 5 ).ToString() );
            SetAttributeValue( "CacheDuration", ( nbCacheDuration.Text.AsIntegerOrNull() ?? 5 ).ToString() );
            SetAttributeValue( "Status", cblStatus.SelectedValues.AsDelimited( "," ) );
            SetAttributeValue( "Filters", kvlFilter.Value );
            SetAttributeValue( "Order", kvlOrder.Value );

            SaveAttributeValues();

            FlushCacheItem( contentCacheKey );
            FlushCacheItem( templateCacheKey );

            ShowView();
        }

        protected void lbCancel_Click( object sender, EventArgs e )
        {
            ShowView();
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
            var template = GetCacheItem( templateCacheKey ) as Template;
            if ( template == null )
            {
                Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
                template = Template.Parse( GetAttributeValue( "Template" ) );

                int? cacheDuration = GetAttributeValue( "CacheDuration" ).AsInteger();
                if ( cacheDuration > 0 )
                {
                    var cacheItemPolicy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddSeconds( cacheDuration.Value ) };
                    AddCacheItem( templateCacheKey, template, cacheItemPolicy );
                }
            }

            return template;
        }

        private List<ContentItem> GetContent()
        {
            var items = GetCacheItem( contentCacheKey ) as List<ContentItem>;

            if ( items == null )
            {
                items = new List<ContentItem>();

                Guid? channelGuid = GetAttributeValue( "Channel" ).AsGuidOrNull();
                if ( channelGuid.HasValue )
                {
                    var rockContext = new RockContext();
                    var service = new ContentItemService( rockContext );

                    var contentChannel = new ContentChannelService( rockContext ).Get( channelGuid.Value );
                    if ( contentChannel != null )
                    {
                        // Create query that gets items of this channel type and that are active
                        var now = RockDateTime.Now;
                        var qry = service.Queryable( "ContentChannel,ContentType" )
                            .Where( i =>
                                i.ContentChannelId == contentChannel.Id &&
                                i.StartDateTime.CompareTo( now ) <= 0 &&
                                ( !i.ExpireDateTime.HasValue || i.ExpireDateTime.Value.CompareTo( now ) > 0 )
                            );

                        // Check for the configured status and limit query to those
                        var statuses = new List<ContentItemStatus>();
                        foreach ( string statusVal in GetAttributeValue( "Status" ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                        {
                            var status = statusVal.ConvertToEnumOrNull<ContentItemStatus>();
                            if ( status != null )
                            {
                                statuses.Add( status.Value );
                            }
                        }
                        if ( statuses.Any() )
                        {
                            qry = qry.Where( i => statuses.Contains( i.Status ) );
                        }

                        var filters = new Dictionary<string, string>();

                        // Create a generic item and load it's attributes so that we can tell what 
                        // attributes exist for items of this channel/content type
                        var genericItem = new ContentItem
                        {
                            Id = 0,
                            ContentChannelId = contentChannel.Id,
                            ContentTypeId = contentChannel.ContentTypeId
                        };
                        genericItem.LoadAttributes();

                        // If attributes exist for items of this channel/content type, look for filters
                        if ( genericItem.Attributes != null && genericItem.Attributes.Any() )
                        {
                            // First check query string parameters for any attribute value
                            foreach ( var pageParameter in PageParameters() )
                            {
                                if ( genericItem.Attributes.ContainsKey( pageParameter.Key ) )
                                {
                                    filters.Add( pageParameter.Key, pageParameter.Value.ToString() );
                                }
                            }

                            // Then check the block settings (replacing any querystring values)
                            var settingFilters = GetAttributeValue( "Filters" ).Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                            foreach ( string filter in settingFilters )
                            {
                                var filterParts = filter.Split( new char[] { ',' } );
                                if ( filterParts.Length == 2 )
                                {
                                    filters.AddOrReplace( filterParts[0], filterParts[1] );
                                }
                            }
                        }

                        // Update qry to limit it to any attribute filters
                        foreach ( var filter in filters )
                        {
                            qry = qry.WhereAttributeValue( rockContext, filter.Key, filter.Value );
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

                        //var sortColumns = new List<string>();
                        //var orderSettings = GetAttributeValue("Order").Split( new char[] { '|'}, StringSplitOptions.RemoveEmptyEntries);
                        //foreach( string orderSetting in orderSettings)
                        //{
                        //    var orderParts = orderSetting.Split( new char[] { ','} );
                        //    if (orderSetting.Length == 2)
                        //    {
                        //        switch( orderParts[0] )
                        //        filters.AddOrReplace( filterParts[0], filterParts[1] )
                        //    }
                        //}     

                        var sortProperty = new SortProperty();
                        sortProperty.Direction = SortDirection.Ascending;
                        sortProperty.Property = "Status DESC";

                        var dynamicItems = new List<dynamic>();
                        foreach ( var item in items )
                        {
                            dynamic dynItem = new ExpandoObject();
                            dynItem.Id = item.Id;
                            dynItem.Guid = item.Guid;
                            dynItem.Title = item.Title;
                            dynItem.Priority = item.Priority;
                            dynItem.Status = item.Status;
                            dynItem.ApprovedByPersonAliasId = item.ApprovedByPersonAliasId;
                            dynItem.ApprovedDateTime = item.ApprovedDateTime;
                            dynItem.StartDateTime = item.StartDateTime;
                            dynItem.ExpireDateTime = item.ExpireDateTime;

                            foreach ( var attributeValue in item.AttributeValues )
                            {
                                ( (Dictionary<string, object>)dynItem ).Add( attributeValue.Key, attributeValue.Value.Value );
                            }

                            dynamicItems.Add( dynItem );
                        }

                        //var orderedItems = new List<ContentItem>();
                        //foreach ( var item in dynamicItems.AsQueryable().Sort(sortProperty) )
                        //{
                        //    orderedItems.Add( items.FirstOrDefault( i => i.Id == item.Id) );
                        //}

                        //return orderedItems;
                    }
                }

                int? cacheDuration = GetAttributeValue( "CacheDuration" ).AsInteger();
                if ( cacheDuration > 0 )
                {
                    var cacheItemPolicy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddSeconds( cacheDuration.Value ) };
                    AddCacheItem( contentCacheKey, items, cacheItemPolicy );
                }
            }

            return items;
        }

        private void BuildDynamicEditControls( bool setValues )
        {
            kvlFilter.CustomKeys = new Dictionary<string, string>();
            kvlOrder.CustomKeys = new Dictionary<string, string>();

            kvlOrder.CustomKeys.Add( "Priority", "Priority" );
            kvlOrder.CustomKeys.Add( "Title", "Title" );
            kvlOrder.CustomKeys.Add( "Status", "Status" );
            kvlOrder.CustomKeys.Add( "StartDateTime", "Start" );
            kvlOrder.CustomKeys.Add( "ExpireDateTime", "Expire" );

            var fields = new Dictionary<string, string>();
            Guid? channelGuid = hfChannelGuid.Value.AsGuidOrNull();
            if ( channelGuid.HasValue )
            {
                var rockContext = new RockContext();
                var channel = new ContentChannelService( rockContext ).Get( channelGuid.Value );
                if ( channel != null )
                {
                    var channelItem = new ContentItem();
                    channelItem.ContentTypeId = channel.ContentTypeId;
                    channelItem.LoadAttributes( rockContext );

                    foreach ( var attribute in channelItem.Attributes.Select( a => a.Value ) )
                    {
                        kvlFilter.CustomKeys.Add( attribute.Key.ToString(), attribute.Name );
                        kvlOrder.CustomKeys.Add( "Attribute_" + attribute.Key.ToString(), attribute.Name );
                    }
                }
            }

            kvlFilter.Visible = kvlFilter.CustomKeys.Any();
        }

        #endregion

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
            public List<ContentItem> GetCurrentPageItems( List<ContentItem> allItems )
            {
                if ( PageSize > 0 )
                {
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

        }
    }

}