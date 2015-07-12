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
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Block to display content items, html, xml, or transformed xml based on a SQL query or stored procedure.
    /// </summary>
    [DisplayName( "Content Channel View" )]
    [Category( "CMS" )]
    [Description( "Block to display dynamic content channel items." )]

    // Block Properties
    [LinkedPage( "Detail Page", "The page to navigate to for details.", false, "", "", 0 )]

    // Custom Settings
    [ContentChannelField( "Channel", "The channel to display items from.", false, "", "CustomSetting" )]
    [EnumsField( "Status", "Include items with the following status.", typeof( ContentChannelItemStatus ), false, "2", "CustomSetting" )]
    [CodeEditorField( "Template", "The template to use when formatting the list of items.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 600, false, @"", "CustomSetting" )]
    [IntegerField( "Count", "The maximum number of items to display.", false, 5, "CustomSetting" )]
    [IntegerField( "Cache Duration", "Number of seconds to cache the content.", false, 3600, "CustomSetting" )]
    [BooleanField( "Enable Debug", "Enabling debug will display the fields of the first 5 items to help show you wants available for your liquid.", false, "CustomSetting" )]
    [IntegerField( "Filter Id", "The data filter that is used to filter items", false, 0, "CustomSetting" )]
    [BooleanField( "Query Parameter Filtering", "Determines if block should evaluate the query string parameters for additional filter criteria.", false, "CustomSetting" )]
    [TextField( "Order", "The specifics of how items should be ordered. This value is set through configuration and should not be modified here.", false, "", "CustomSetting" )]
    [BooleanField("Merge Content", "Should the content data and attribute values be merged using the liquid template engine.", false, "CustomSetting" )]
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

        #endregion

        #region Properties

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

        #endregion

        #region Base Control Methods

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

            cblStatus.BindToEnum<ContentChannelItemStatus>();

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

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["ChannelGuid"] = ChannelGuid;
            ViewState["DataViewFilter"] = GetFilterControl().ToJson();

            return base.SaveViewState();
        }

        #endregion

        #region Events

        void ContentDynamic_BlockUpdated( object sender, EventArgs e )
        {
            ShowView();
        }

        protected void ddlChannel_SelectedIndexChanged( object sender, EventArgs e )
        {
            ChannelGuid = ddlChannel.SelectedValue.AsGuidOrNull();
            ShowEdit();
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
            SetAttributeValue( "MergeContent", cbMergeContent.Checked.ToString() );
            SetAttributeValue( "Template", ceTemplate.Text );
            SetAttributeValue( "Count", ( nbCount.Text.AsIntegerOrNull() ?? 5 ).ToString() );
            SetAttributeValue( "CacheDuration", ( nbCacheDuration.Text.AsIntegerOrNull() ?? 5 ).ToString() );
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

            FlushCacheItem( CONTENT_CACHE_KEY );
            FlushCacheItem( TEMPLATE_CACHE_KEY );

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
            filterField.DataViewFilterGuid = Guid.NewGuid();
            groupControl.Controls.Add( filterField );
            filterField.ID = string.Format( "ff_{0}", filterField.DataViewFilterGuid.ToString( "N" ) );
            
            // Remove the 'Other Data View' Filter as it doesn't really make sense to have it available in this scenario
            filterField.ExcludedFilterTypes = new string[] { typeof( Rock.Reporting.DataFilter.OtherDataViewFilter ).FullName };
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
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            pnlEditModal.Visible = true;
            upnlContent.Update();
            mdEdit.Show();

            var rockContext = new RockContext();
            ddlChannel.DataSource = new ContentChannelService( rockContext ).Queryable()
                .OrderBy( c => c.Name )
                .Select( c => new { c.Guid, c.Name } )
                .ToList();
            ddlChannel.DataBind();
            ddlChannel.Items.Insert( 0, new ListItem( "", "" ) );
            ddlChannel.SetValue( GetAttributeValue( "Channel" ) );
            ChannelGuid = ddlChannel.SelectedValue.AsGuidOrNull();

            foreach ( string status in GetAttributeValue( "Status" ).SplitDelimitedValues() )
            {
                var li = cblStatus.Items.FindByValue( status );
                if ( li != null )
                {
                    li.Selected = true;
                }
            }

            cbDebug.Checked = GetAttributeValue( "EnableDebug" ).AsBoolean();
            cbMergeContent.Checked = GetAttributeValue( "MergeContent" ).AsBoolean();
            cbSetRssAutodiscover.Checked = GetAttributeValue( "RssAutodiscover" ).AsBoolean();
            cbSetPageTitle.Checked = GetAttributeValue( "SetPageTitle" ).AsBoolean();
            ceTemplate.Text = GetAttributeValue( "Template" );
            nbCount.Text = GetAttributeValue( "Count" );
            nbCacheDuration.Text = GetAttributeValue( "CacheDuration" );
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
        /// Binds the grid.
        /// </summary>
        private void ShowView()
        {
            nbContentError.Visible = false;
            upnlContent.Update();

            var pageRef = CurrentPageReference;
            pageRef.Parameters.AddOrReplace( "Page", "PageNum" );

            Dictionary<string, object> linkedPages = new Dictionary<string, object>();
            linkedPages.Add( "DetailPage", LinkedPageUrl( "DetailPage", null ) );

            var errorMessages = new List<string>();
            List<ContentChannelItem> content;
            try
            {
                content = GetContent( errorMessages ) ?? new List<ContentChannelItem>();
            }
            catch (Exception ex)
            {
                this.LogException( ex );
                Exception exception = ex;
                while ( exception != null )
                {
                    errorMessages.Add( exception.Message );
                    exception = exception.InnerException;
                }
                
                content = new List<ContentChannelItem>();
            }

            if (errorMessages.Any())
            {
                nbContentError.Text = "ERROR: There was a problem getting content...<br/> ";
                nbContentError.NotificationBoxType = NotificationBoxType.Danger;
                nbContentError.Details = errorMessages.AsDelimited( "<br/>" );
                nbContentError.Visible = true;
            }

            var pagination = new Pagination();
            pagination.ItemCount = content.Count();
            pagination.PageSize = GetAttributeValue( "Count" ).AsInteger();
            pagination.CurrentPage = PageParameter( "Page" ).AsIntegerOrNull() ?? 1;
            pagination.UrlTemplate = pageRef.BuildUrl();
            var currentPageContent = pagination.GetCurrentPageItems( content );

            var globalAttributeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );

            // Merge content and attribute fields if block is configured to do so.
            if ( GetAttributeValue( "MergeContent" ).AsBoolean() )
            {
                var itemMergeFields = new Dictionary<string, object>();
                if ( CurrentPerson != null )
                {
                    // TODO: When support for "Person" is not supported anymore (should use "CurrentPerson" instead), remove this line
                    itemMergeFields.Add( "Person", CurrentPerson );
                    itemMergeFields.Add( "CurrentPerson", CurrentPerson );
                }
                globalAttributeFields.ToList().ForEach( d => itemMergeFields.Add( d.Key, d.Value ) );

                foreach ( var item in currentPageContent )
                {
                    itemMergeFields.AddOrReplace( "Item", item );
                    item.Content = item.Content.ResolveMergeFields( itemMergeFields );
                    foreach( var attributeValue in item.AttributeValues )
                    {
                        attributeValue.Value.Value = attributeValue.Value.Value.ResolveMergeFields( itemMergeFields );
                    }
                }
            }

            var mergeFields = new  Dictionary<string, object>();
            mergeFields.Add( "Pagination", pagination );
            mergeFields.Add( "LinkedPages", linkedPages );
            mergeFields.Add( "Items", currentPageContent );
            mergeFields.Add( "Campuses", CampusCache.All() );
            mergeFields.Add( "CurrentPerson", CurrentPerson );

            globalAttributeFields.ToList().ForEach( d => mergeFields.Add( d.Key, d.Value ) );
            mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );

            // enable showing debug info
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                mergeFields["Items"] = currentPageContent.Take( 5 ).ToList();

                lDebug.Visible = true;
                
                lDebug.Text = mergeFields.lavaDebugInfo();

                mergeFields["Items"] = currentPageContent;
            }
            else
            {
                lDebug.Visible = false;
                lDebug.Text = string.Empty;
            }

            // TODO: When support for "Person" is not supported anymore (should use "CurrentPerson" instead), remove this line
            mergeFields.Add( "Person", CurrentPerson );

            // set page title
            if ( GetAttributeValue( "SetPageTitle" ).AsBoolean() && content.Count > 0 )
            {               
                if ( string.IsNullOrWhiteSpace( PageParameter( "Item" ) ) )
                {
                    // set title to channel name
                    string channelName = content.Select( c => c.ContentChannel.Name ).FirstOrDefault();
                    RockPage.BrowserTitle = String.Format( "{0} | {1}", channelName, RockPage.Site.Name );
                    RockPage.PageTitle = channelName;
                    RockPage.Header.Title = String.Format( "{0} | {1}", channelName, RockPage.Site.Name );
                }
                else
                {
                    string itemTitle = content.Select( c => c.Title ).FirstOrDefault();
                    RockPage.PageTitle = itemTitle;
                    RockPage.BrowserTitle = String.Format( "{0} | {1}", itemTitle, RockPage.Site.Name );
                    RockPage.Header.Title = String.Format( "{0} | {1}", itemTitle, RockPage.Site.Name );
                }
            }

            // set rss auto discover link
            if ( GetAttributeValue( "RssAutodiscover" ).AsBoolean() && content.Count > 0 )
            {
                //<link rel="alternate" type="application/rss+xml" title="RSS Feed for petefreitag.com" href="/rss/" />
                HtmlLink rssLink = new HtmlLink();
                rssLink.Attributes.Add("type", "application/rss+xml");
                rssLink.Attributes.Add("rel", "alternate");
                rssLink.Attributes.Add("title", content.Select(c => c.ContentChannel.Name).FirstOrDefault());

                var context = HttpContext.Current;
                string channelRssUrl = string.Format( "{0}://{1}{2}{3}{4}",
                                    context.Request.Url.Scheme,
                                    context.Request.Url.Host,
                                    context.Request.Url.Port == 80
                                        ? string.Empty
                                        : ":" + context.Request.Url.Port,
                                    RockPage.ResolveRockUrl( "~/GetChannelFeed.ashx?ChannelId="),
                                    content.Select(c => c.ContentChannelId).FirstOrDefault());

                rssLink.Attributes.Add( "href", channelRssUrl );
                RockPage.Header.Controls.Add( rssLink );
            }

            // set description meta tag
            string metaDescriptionAttributeValue = GetAttributeValue("MetaDescriptionAttribute");
            if ( !string.IsNullOrWhiteSpace( metaDescriptionAttributeValue ) && content.Count > 0 )
            {
                string attributeValue = GetMetaValueFromAttribute(metaDescriptionAttributeValue, content);

                if ( !string.IsNullOrWhiteSpace( attributeValue ) )
                {
                    // remove default meta description
                    RockPage.Header.Description = attributeValue.SanitizeHtml( true );
                }
            }

            // add meta images
            string metaImageAttributeValue = GetAttributeValue( "MetaImageAttribute" );
            if ( !string.IsNullOrWhiteSpace( metaImageAttributeValue ) && content.Count > 0 )
            {
                string attributeValue = GetMetaValueFromAttribute( metaImageAttributeValue, content );

                if ( !string.IsNullOrWhiteSpace( attributeValue ) )
                {
                    HtmlMeta metaDescription = new HtmlMeta();
                    metaDescription.Name = "og:image";
                    metaDescription.Content = string.Format( "{0}://{1}/GetImage.ashx?guid={2}", Request.Url.Scheme, Request.Url.Authority,  attributeValue );
                    RockPage.Header.Controls.Add( metaDescription );

                    HtmlLink imageLink = new HtmlLink();
                    imageLink.Attributes.Add( "rel", "image_src" );
                    imageLink.Attributes.Add( "href", string.Format( "{0}://{1}/GetImage.ashx?guid={2}", Request.Url.Scheme, Request.Url.Authority,  attributeValue )); 
                    RockPage.Header.Controls.Add( imageLink );
                }
            }

            var template = GetTemplate();

            phContent.Controls.Add( new LiteralControl( template.Render( Hash.FromDictionary( mergeFields ) ) ) );
        }

        private string GetMetaValueFromAttribute( string input, List<ContentChannelItem> content )
        {
            string attributeEntityType = input.Split( '^' )[0].ToString() ?? "C";
            string attributeKey = input.Split( '^' )[1].ToString() ?? "";

            string attributeValue = string.Empty;

            if ( attributeEntityType == "C" )
            {
                attributeValue = content.FirstOrDefault().ContentChannel.AttributeValues.Where( a => a.Key == attributeKey ).Select( a => a.Value.ToString() ).FirstOrDefault();
            }
            else
            {
                attributeValue = content.FirstOrDefault().AttributeValues.Where( a => a.Key == attributeKey ).Select( a => a.Value.ToString() ).FirstOrDefault();
            }

            return attributeValue;
        }

        private Template GetTemplate()
        {
            var template = GetCacheItem( TEMPLATE_CACHE_KEY ) as Template;
            if ( template == null )
            {
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

        private List<ContentChannelItem> GetContent( List<string> errorMessages )
        {
            var items = GetCacheItem( CONTENT_CACHE_KEY ) as List<ContentChannelItem>;
            bool queryParameterFiltering = GetAttributeValue( "QueryParameterFiltering" ).AsBoolean( false );

            if ( items == null || ( queryParameterFiltering && Request.QueryString.Count > 0 ) )
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
                        var entityFields = HackEntityFields( contentChannel, rockContext );

                        if ( items == null )
                        {
                            items = new List<ContentChannelItem>();

                            var qry = service.Queryable( "ContentChannel,ContentChannelType" );

                            int? itemId = PageParameter( "Item" ).AsIntegerOrNull();
                            if ( itemId.HasValue )
                            {
                                qry = qry.Where( i => i.Id == itemId.Value );
                            }
                            else
                            {
                                qry = qry.Where( i => i.ContentChannelId == contentChannel.Id );

                                if ( contentChannel.RequiresApproval )
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
                                                    itemQry.OrderBy( i => i.AttributeValues.Where( v => v.Key == attributeKey ).FirstOrDefault().Value.Value ) :
                                                    orderedQry.ThenBy( i => i.AttributeValues.Where( v => v.Key == attributeKey ).FirstOrDefault().Value.Value );
                                            }
                                            else
                                            {
                                                orderedQry = ( columnIndex == 0 ) ?
                                                    itemQry.OrderByDescending( i => i.AttributeValues.Where( v => v.Key == attributeKey ).FirstOrDefault().Value.Value ) :
                                                    orderedQry.ThenByDescending( i => i.AttributeValues.Where( v => v.Key == attributeKey ).FirstOrDefault().Value.Value );
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

                            int? cacheDuration = GetAttributeValue( "CacheDuration" ).AsInteger();
                            if ( cacheDuration > 0 )
                            {
                                var cacheItemPolicy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddSeconds( cacheDuration.Value ) };
                                AddCacheItem( CONTENT_CACHE_KEY, items, cacheItemPolicy );
                            }
                        }

                        // If items could be filtered by querystring values, check for filters
                        if ( queryParameterFiltering && Request.QueryString.Count > 0 )
                        {
                            var propertyFilter = new Rock.Reporting.DataFilter.PropertyFilter();

                            var itemQry = items.AsQueryable();
                            foreach( string key in Request.QueryString.AllKeys )
                            {
                                var selection = new List<string>();
                                selection.Add( key );

                                var entityField = entityFields.FirstOrDefault( f => f.Name.Equals( key, StringComparison.OrdinalIgnoreCase ) );
                                if ( entityField != null )
                                {
                                    string value = Request.QueryString[key];
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
                                                var values = new List<string>();
                                                values.Add( value );
                                                selection.Add( Newtonsoft.Json.JsonConvert.SerializeObject( values ) );
                                            }
                                            break;
                                        default:
                                            {
                                                selection.Add( ComparisonType.EqualTo.ConvertToInt().ToString());
                                                selection.Add( value);
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

                    cblStatus.Visible = channel.RequiresApproval;

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
                    var itemAttributes = attributeService.GetByEntityTypeId( new ContentChannelItem().TypeId ).AsQueryable()
                                            .Where( a => (
                                                    a.EntityTypeQualifierColumn.Equals( "ContentChannelTypeId", StringComparison.OrdinalIgnoreCase ) &&
                                                    a.EntityTypeQualifierValue.Equals( channel.ContentChannelTypeId.ToString() ) 
                                                ) || (
                                                    a.EntityTypeQualifierColumn.Equals( "ContentChannelId", StringComparison.OrdinalIgnoreCase ) &&
                                                    a.EntityTypeQualifierValue.Equals( channel.Id.ToString() ) 
                                                ) )
                                            .ToList();

                    foreach ( var attribute in itemAttributes )
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
                item.Selected = (item.Value == value);
            }
        }

        /// <summary>
        /// The PropertyFilter checks for it's property/attribute list in a cached items object before recreating 
        /// them using reflection and loading of generic attributes. Because of this, we're going to load them here
        /// and exclude some properties and add additional attributes specific to the channel type, and then save
        /// list to same cached object so that property filter lists our collection of properties/attributes
        /// instead.
        /// </summary>
        private List<Rock.Reporting.EntityField> HackEntityFields( ContentChannel channel, RockContext rockContext )
        {
            if ( channel != null )
            {
                var entityTypeCache = EntityTypeCache.Read( ITEM_TYPE_NAME );
                if ( entityTypeCache != null )
                {
                    var entityType = entityTypeCache.GetEntityType();

                    HttpContext.Current.Items.Remove( string.Format( "EntityHelper:GetEntityFields:{0}", entityType.FullName ) );
                    var entityFields = Rock.Reporting.EntityHelper.GetEntityFields( entityType );

                    if ( entityFields != null )
                    {
                        // Remove the status field
                        var ignoreFields = new List<string>();
                        ignoreFields.Add( "ContentChannelId" );
                        ignoreFields.Add( "Status" );

                        entityFields = entityFields.Where( f => !ignoreFields.Contains( f.Name ) ).ToList();

                        // Add any additional attributes that are specific to channel/type
                        var item = new ContentChannelItem();
                        item.ContentChannel = channel;
                        item.ContentChannelId = channel.Id;
                        item.ContentChannelType = channel.ContentChannelType;
                        item.ContentChannelTypeId = channel.ContentChannelTypeId;
                        item.LoadAttributes( rockContext );
                        foreach ( var attribute in item.Attributes
                            .Where( a =>
                                a.Value.EntityTypeQualifierColumn != "" &&
                                a.Value.EntityTypeQualifierValue != "" )
                            .Select( a => a.Value ) )
                        {
                            Rock.Reporting.EntityHelper.AddEntityFieldForAttribute( entityFields, attribute );
                        }

                        // Re-sort fields
                        int index = 0;
                        var sortedFields = new List<Rock.Reporting.EntityField>();
                        foreach ( var entityProperty in entityFields.OrderBy( p => p.Title ).ThenBy( p => p.Name ) )
                        {
                            entityProperty.Index = index;
                            index++;
                            sortedFields.Add( entityProperty );
                        }

                        // Save new fields to cache ( which report field will use instead of reading them again )
                        HttpContext.Current.Items[string.Format( "EntityHelper:GetEntityFields:{0}", entityType.FullName )] = sortedFields;
                    }

                    return entityFields;
                }
            }

            return null;
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
            HackEntityFields( channel, rockContext );

            phFilters.Controls.Clear();
            if ( filter != null )
            {
                CreateFilterControl( phFilters, filter, setSelection, rockContext );
            }
        }

        /// <summary>
        /// Creates the filter control.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        /// <param name="rockContext">The rock context.</param>
        private void CreateFilterControl( Control parentControl, DataViewFilter filter, bool setSelection, RockContext rockContext )
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
                    var entityTypeCache = Rock.Web.Cache.EntityTypeCache.Read( filter.EntityTypeId.Value, rockContext );
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
                filter.Selection = filterField.GetSelection();
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
                    if ( PageSize == 0 )
                    {
                        return 1;
                    }
                    else
                    {
                        return Convert.ToInt32( Math.Abs( ItemCount / PageSize ) ) +
                            ((ItemCount % PageSize) > 0 ? 1 : 0);
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