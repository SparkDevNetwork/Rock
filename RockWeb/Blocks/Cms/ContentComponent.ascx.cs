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
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Content Component" )]
    [Category( "CMS" )]
    [Description( "Block to manage and display content." )]

    #region Block Attributes

    [ContentChannelField(
        "Content Channel",
        Category = "CustomSetting",
        Key = AttributeKey.ContentChannel )]

    [IntegerField(
        "Item Cache Duration",
        Description = "Number of seconds to cache the content item specified by the parameter.",
        IsRequired = false,
        DefaultIntegerValue = 0,
        Category = "CustomSetting",
        Order = 0,
        Key = AttributeKey.ItemCacheDuration )]

    [DefinedValueField(
        "Content Component Template",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CONTENT_COMPONENT_TEMPLATE,
        Key = AttributeKey.ContentComponentTemplate )]

    [BooleanField(
        "Allow Multiple Content Items",
        Category = "CustomSetting",
        Key = AttributeKey.AllowMultipleContentItems )]

    [IntegerField(
        "Output Cache Duration",
        Description = "Number of seconds to cache the resolved output. Only cache the output if you are not personalizing the output based on current user, current page, or any other merge field value.",
        IsRequired = false,
        Key = AttributeKey.OutputCacheDuration,
        Category = "CustomSetting" )]

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

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.CONTENT_COMPONENT )]
    public partial class ContentComponent : RockBlockCustomSettings
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ContentChannel = "ContentChannel";
            public const string ItemCacheDuration = "ItemCacheDuration";
            public const string ContentComponentTemplate = "ContentComponentTemplate";
            public const string AllowMultipleContentItems = "AllowMultipleContentItems";
            public const string OutputCacheDuration = "OutputCacheDuration";
            public const string FilterId = "FilterId";
            public const string CacheTags = "CacheTags";
        }

        #endregion Attribute Keys

        #region Fields

        /// <summary>
        /// The output cache key
        /// </summary>
        private const string OUTPUT_CACHE_KEY = "Output";

        /// <summary>
        /// The item cache key
        /// </summary>
        private const string ITEM_CACHE_KEY = "Item";

        /// <summary>
        /// The Text of btnSaveItem when not in 'AllowMultipleContentItems' mode
        /// </summary>
        private const string CONTENT_CHANNEL_ITEM_SAVE_TEXT = "Save";

        /// <summary>
        /// The Text of btnSaveItem when in 'AllowMultipleContentItems' mode
        /// </summary>
        private const string CONTENT_CHANNEL_ITEM_CLOSE_MODAL_TEXT = "Close";

        /// <summary>
        /// The content channel type identifier
        /// </summary>
        private int ContentChannelTypeId = new ContentChannelTypeService( new RockContext() ).GetId( Rock.SystemGuid.ContentChannelType.CONTENT_COMPONENT.AsGuid() ) ?? 0;

        #endregion Fields

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            gContentChannelItems.DataKeyNames = new string[] { "Id" };
            gContentChannelItems.Actions.ShowAdd = true;
            gContentChannelItems.Actions.AddClick += gContentChannelItems_AddClick;
            gContentChannelItems.GridRebind += gContentChannelItems_GridRebind;
            gContentChannelItems.GridReorder += gContentChannelItems_GridReorder;
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

            var rockContext = new RockContext();
            CreateFilterControl( this.ContentChannelTypeId, DataViewFilter.FromJson( ViewState["DataViewFilter"].ToString() ), false, rockContext );
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["DataViewFilter"] = ReportingHelper.GetFilterFromControls( phFilters ).ToJson();
            return base.SaveViewState();
        }

        #endregion

        #region overrides

        /// <summary>
        /// Adds icons to the configuration area of a <see cref="T:Rock.Model.Block" /> instance.  Can be overridden to
        /// add additional icons
        /// </summary>
        /// <param name="canConfig">A <see cref="T:System.Boolean" /> flag that indicates if the user can configure the <see cref="T:Rock.Model.Block" /> instance.
        /// This value will be <c>true</c> if the user is allowed to configure the <see cref="T:Rock.Model.Block" /> instance; otherwise <c>false</c>.</param>
        /// <param name="canEdit">A <see cref="T:System.Boolean" /> flag that indicates if the user can edit the <see cref="T:Rock.Model.Block" /> instance.
        /// This value will be <c>true</c> if the user is allowed to edit the <see cref="T:Rock.Model.Block" /> instance; otherwise <c>false</c>.</param>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.List`1" /> containing all the icon <see cref="T:System.Web.UI.Control">controls</see>
        /// that will be available to the user in the configuration area of the block instance.
        /// </returns>
        public override List<Control> GetAdministrateControls( bool canConfig, bool canEdit )
        {
            // don't show the Edit option until the block is configured
            canEdit = canEdit && this.GetContentChannel() != null;

            List<Control> configControls = base.GetAdministrateControls( canConfig, canEdit );

            // remove the "aBlockProperties" control since we'll be taking care of all that with our "lbConfigure" button
            var aBlockProperties = configControls.FirstOrDefault( a => a.ID == "aBlockProperties" );
            if ( aBlockProperties != null )
            {
                configControls.Remove( aBlockProperties );
            }

            if ( canConfig )
            {
                LinkButton lbConfigure = new LinkButton();
                lbConfigure.ID = "lbConfigure";
                lbConfigure.CssClass = "edit";
                lbConfigure.ToolTip = "Configure";
                lbConfigure.Click += lbConfigure_Click;
                configControls.Add( lbConfigure );
                HtmlGenericControl iConfigure = new HtmlGenericControl( "i" );
                iConfigure.Attributes.Add( "class", "fa fa-cog" );

                lbConfigure.Controls.Add( iConfigure );
                lbConfigure.CausesValidation = false;

                // will toggle the block config so they are no longer showing
                lbConfigure.Attributes["onclick"] = "Rock.admin.pageAdmin.showBlockConfig()";

                ScriptManager.GetCurrent( this.Page ).RegisterAsyncPostBackControl( lbConfigure );
            }

            return configControls;
        }

        protected override void ShowSettings()
        {
            ContentChannelCache contentChannel = this.GetContentChannel();
            if ( contentChannel == null )
            {
                // shouldn't happen. This button isn't visible unless there is a contentChannel configured
                return;
            }

            pnlContentComponentEditContentChannelItems.Visible = true;
            mdContentComponentEditContentChannelItems.Show();

            var allowMultipleContentItems = this.GetAttributeValue( AttributeKey.AllowMultipleContentItems ).AsBoolean();
            pnlContentChannelItemsList.Visible = allowMultipleContentItems;
            pnlContentChannelItemEdit.CssClass = allowMultipleContentItems ? "col-md-8" : "col-md-12";

            var rockContext = new RockContext();
            var contentChannelItemId = new ContentChannelItemService( rockContext ).Queryable().Where( a => a.ContentChannelId == contentChannel.Id ).OrderBy( a => a.Order ).ThenBy( a => a.Title ).Select( a => ( int? ) a.Id ).FirstOrDefault();

            EditContentChannelItem( contentChannelItemId );

            if ( allowMultipleContentItems )
            {
                // hide the SaveButton
                mdContentComponentEditContentChannelItems.SaveButtonText = CONTENT_CHANNEL_ITEM_CLOSE_MODAL_TEXT;
                mdContentComponentEditContentChannelItems.SaveButtonCausesValidation = false;
                mdContentComponentEditContentChannelItems.CloseLinkVisible = true;
                mdContentComponentEditContentChannelItems.CancelLinkVisible = false;
            }
            else
            {
                mdContentComponentEditContentChannelItems.SaveButtonText = CONTENT_CHANNEL_ITEM_SAVE_TEXT;
                mdContentComponentEditContentChannelItems.SaveButtonCausesValidation = true;
                mdContentComponentEditContentChannelItems.CloseLinkVisible = false;
                mdContentComponentEditContentChannelItems.CancelLinkVisible = true;
            }

            btnSaveItem.Visible = allowMultipleContentItems;
        }

        #endregion overrides

        #region Shared Methods

        /// <summary>
        /// Shows the view.
        /// </summary>
        private void ShowView()
        {
            // Disable content rendering for configuration mode to improve efficiency.
            // This is also necessary to avoid an issue where Lava content may fail to render if the template
            // uses {% include %} to reference files that do not exist in the filesystem of the current theme.
            if ( this.ConfigurationRenderModeIsEnabled )
            {
                return;
            }

            int? outputCacheDuration = GetAttributeValue( AttributeKey.OutputCacheDuration ).AsIntegerOrNull();
            int? itemCacheDuration = GetAttributeValue( AttributeKey.ItemCacheDuration ).AsIntegerOrNull();

            string outputContents = null;

            string outputCacheKey = OUTPUT_CACHE_KEY;

            if ( outputCacheDuration.HasValue && outputCacheDuration.Value > 0 )
            {
                outputContents = GetCacheItem( outputCacheKey ) as string;
            }

            if ( outputContents == null )
            {
                var contentChannelItems = GetContentChannelItems( ITEM_CACHE_KEY, itemCacheDuration );

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new Rock.Lava.CommonMergeFieldsOptions() );
                mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );

                mergeFields.Add( "Items", contentChannelItems );
                mergeFields.Add( "ContentChannel", this.GetContentChannel() );
                mergeFields.Add( "CurrentPage", this.PageCache );

                DefinedValueCache contentComponentTemplate = null;
                var contentComponentTemplateValueGuid = this.GetAttributeValue( AttributeKey.ContentComponentTemplate ).AsGuidOrNull();
                if ( contentComponentTemplateValueGuid.HasValue )
                {
                    contentComponentTemplate = DefinedValueCache.Get( contentComponentTemplateValueGuid.Value );
                }

                if ( contentComponentTemplate == null )
                {
                    return;
                }

                string lavaTemplate = contentComponentTemplate.GetAttributeValue( "DisplayLava" );

                // run LavaMerge on lavaTemplate
                outputContents = lavaTemplate.ResolveMergeFields( mergeFields );

                // run LavaMerge again in case there is lava in the MergeFields
                if ( Rock.Lava.LavaHelper.IsLavaTemplate( outputContents ) )
                {
                    outputContents = outputContents.ResolveMergeFields( mergeFields );
                }

                if ( outputCacheDuration.HasValue && outputCacheDuration.Value > 0 )
                {
                    string cacheTags = GetAttributeValue( AttributeKey.CacheTags ) ?? string.Empty;
                    AddCacheItem( outputCacheKey, outputContents, outputCacheDuration.Value, cacheTags );
                }
            }

            lContentOutput.Text = outputContents;
        }

        /// <summary>
        /// Gets the content channel items from the Cache or from Database if not cached
        /// </summary>
        /// <param name="itemCacheKey">The item cache key.</param>
        /// <param name="itemCacheDuration">Duration of the item cache.</param>
        /// <returns></returns>
        public List<ContentChannelItem> GetContentChannelItems( string itemCacheKey, int? itemCacheDuration )
        {
            List<ContentChannelItem> contentChannelItems = null;
            try
            {
                if ( itemCacheDuration.HasValue && itemCacheDuration.Value > 0 )
                {
                    contentChannelItems = GetCacheItem( itemCacheKey ) as List<ContentChannelItem>;
                    if ( contentChannelItems != null )
                    {
                        return contentChannelItems;
                    }
                }

                ContentChannelCache contentChannel = GetContentChannel();

                if ( contentChannel == null )
                {
                    return null;
                }

                var rockContext = new RockContext();
                var contentChannelItemService = new ContentChannelItemService( rockContext );
                IQueryable<ContentChannelItem> contentChannelItemsQuery = contentChannelItemService.Queryable().Where( a => a.ContentChannelId == contentChannel.Id ).OrderBy( a => a.Order ).ThenBy( a => a.Title );

                var allowMultipleContentItems = this.GetAttributeValue( AttributeKey.AllowMultipleContentItems ).AsBoolean();
                if ( !allowMultipleContentItems )
                {
                    // if allowMultipleContentItems = false, just get the first one
                    // if it was configured for allowMultipleContentItems previously, there might be more, but they won't show until allowMultipleContentItems is enabled again
                    contentChannelItemsQuery = contentChannelItemsQuery.Take( 1 );
                }

                int? dataFilterId = GetAttributeValue( AttributeKey.FilterId ).AsIntegerOrNull();
                if ( dataFilterId.HasValue )
                {
                    var dataFilterService = new DataViewFilterService( rockContext );
                    ParameterExpression paramExpression = contentChannelItemService.ParameterExpression;
                    var itemType = typeof( Rock.Model.ContentChannelItem );
                    var dataFilter = dataFilterService.Queryable( "ChildFilters" ).FirstOrDefault( a => a.Id == dataFilterId.Value );
                    Expression whereExpression = dataFilter != null ? dataFilter.GetExpression( itemType, contentChannelItemService, paramExpression ) : null;

                    contentChannelItemsQuery = contentChannelItemsQuery.Where( paramExpression, whereExpression, null );
                }

                contentChannelItems = contentChannelItemsQuery.ToList();

                if ( contentChannelItems != null && itemCacheDuration.HasValue && itemCacheDuration.Value > 0 )
                {
                    string cacheTags = GetAttributeValue( AttributeKey.CacheTags ) ?? string.Empty;
                    AddCacheItem( itemCacheKey, contentChannelItems, itemCacheDuration.Value, cacheTags );
                }

                return contentChannelItems;
            }
            catch ( Exception ex )
            {
                this.LogException( ex );
                nbContentError.Text = "ERROR: There was a problem getting content";
                nbContentError.NotificationBoxType = NotificationBoxType.Danger;
                nbContentError.Details = ex.Message;
                nbContentError.Visible = true;

                // set the contentItemList to an empty list and continue on (but with an empty list of ContentChannelItems to use when rending the Lava)
                contentChannelItems = new List<ContentChannelItem>();
                return contentChannelItems;
            }
        }

        /// <summary>
        /// Gets the content channel cache object.
        /// </summary>
        /// <returns></returns>
        public ContentChannelCache GetContentChannel()
        {
            Guid? contentChannelGuid = this.GetAttributeValue( AttributeKey.ContentChannel ).AsGuidOrNull();
            ContentChannelCache contentChannel = null;
            if ( contentChannelGuid.HasValue )
            {
                contentChannel = ContentChannelCache.Get( contentChannelGuid.Value );
            }

            return contentChannel;
        }

        #endregion Shared Methods

        #region Content Component - Config

        /// <summary>
        /// Handles the Click event of the lbConfigure control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void lbConfigure_Click( object sender, EventArgs e )
        {
            pnlContentComponentConfig.Visible = true;
            mdContentComponentConfig.Show();

            Guid? contentChannelGuid = this.GetAttributeValue( AttributeKey.ContentChannel ).AsGuidOrNull();
            ContentChannel contentChannel = null;
            if ( contentChannelGuid.HasValue )
            {
                contentChannel = new ContentChannelService( new RockContext() ).Get( contentChannelGuid.Value );
            }

            if ( contentChannel == null )
            {
                contentChannel = new ContentChannel { ContentChannelTypeId = this.ContentChannelTypeId };
            }

            tbComponentName.Text = contentChannel.Name;
            contentChannel.LoadAttributes();
            avcContentChannelAttributes.NumberOfColumns = 2;
            avcContentChannelAttributes.ValidationGroup = mdContentComponentConfig.ValidationGroup;
            avcContentChannelAttributes.AddEditControls( contentChannel );

            nbItemCacheDuration.Text = this.GetAttributeValue( AttributeKey.ItemCacheDuration );

            DefinedTypeCache contentComponentTemplateType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.CONTENT_COMPONENT_TEMPLATE.AsGuid() );
            if ( contentComponentTemplateType != null )
            {
                dvpContentComponentTemplate.DefinedTypeId = contentComponentTemplateType.Id;
            }

            DefinedValueCache contentComponentTemplate = null;
            var contentComponentTemplateValueGuid = this.GetAttributeValue( AttributeKey.ContentComponentTemplate ).AsGuidOrNull();
            if ( contentComponentTemplateValueGuid.HasValue )
            {
                contentComponentTemplate = DefinedValueCache.Get( contentComponentTemplateValueGuid.Value );
            }

            dvpContentComponentTemplate.SetValue( contentComponentTemplate );

            cbAllowMultipleContentItems.Checked = this.GetAttributeValue( AttributeKey.AllowMultipleContentItems ).AsBoolean();

            nbOutputCacheDuration.Text = this.GetAttributeValue( AttributeKey.OutputCacheDuration );

            // Cache Tags
            cblCacheTags.DataSource = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.CACHE_TAGS.AsGuid() ).DefinedValues.Select( v => v.Value ).ToList();
            cblCacheTags.DataBind();
            cblCacheTags.Visible = cblCacheTags.Items.Count > 0;
            string[] selectedCacheTags = this.GetAttributeValue( AttributeKey.CacheTags ).SplitDelimitedValues();
            foreach ( ListItem cacheTag in cblCacheTags.Items )
            {
                cacheTag.Selected = selectedCacheTags.Contains( cacheTag.Value );
            }

            cePreHtml.Text = this.BlockCache.PreHtml;
            cePostHtml.Text = this.BlockCache.PostHtml;

            hfDataFilterId.Value = GetAttributeValue( AttributeKey.FilterId );

            int? filterId = hfDataFilterId.Value.AsIntegerOrNull();
            var rockContext = new RockContext();

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

            CreateFilterControl( this.ContentChannelTypeId, filter, true, rockContext );
        }

        /// <summary>
        /// Handles the SaveClick event of the mdContentComponentConfig control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdContentComponentConfig_SaveClick( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            var dataViewFilter = ReportingHelper.GetFilterFromControls( phFilters );
            if ( dataViewFilter != null )
            {
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

                DataViewFilterService dataViewFilterService = new DataViewFilterService( rockContext );

                int? dataViewFilterId = hfDataFilterId.Value.AsIntegerOrNull();
                if ( dataViewFilterId.HasValue )
                {
                    var oldDataViewFilter = dataViewFilterService.Get( dataViewFilterId.Value );
                    DeleteDataViewFilter( oldDataViewFilter, dataViewFilterService );
                }

                dataViewFilterService.Add( dataViewFilter );
            }

            rockContext.SaveChanges();

            ContentChannelService contentChannelService = new ContentChannelService( rockContext );
            Guid? contentChannelGuid = this.GetAttributeValue( AttributeKey.ContentChannel ).AsGuidOrNull();
            ContentChannel contentChannel = null;

            if ( contentChannelGuid.HasValue )
            {
                contentChannel = contentChannelService.Get( contentChannelGuid.Value );
            }

            if ( contentChannel == null )
            {
                contentChannel = new ContentChannel();
                contentChannel.ContentChannelTypeId = this.ContentChannelTypeId;
                contentChannelService.Add( contentChannel );
            }

            contentChannel.LoadAttributes( rockContext );
            avcContentChannelAttributes.GetEditValues( contentChannel );

            contentChannel.Name = tbComponentName.Text;
            rockContext.SaveChanges();
            contentChannel.SaveAttributeValues( rockContext );

            this.SetAttributeValue( "ContentChannel", contentChannel.Guid.ToString() );

            this.SetAttributeValue( "ItemCacheDuration", nbItemCacheDuration.Text );

            int? contentComponentTemplateValueId = dvpContentComponentTemplate.SelectedValue.AsInteger();
            Guid? contentComponentTemplateValueGuid = null;
            if ( contentComponentTemplateValueId.HasValue )
            {
                var contentComponentTemplate = DefinedValueCache.Get( contentComponentTemplateValueId.Value );
                if ( contentComponentTemplate != null )
                {
                    contentComponentTemplateValueGuid = contentComponentTemplate.Guid;
                }
            }

            this.SetAttributeValue( "ContentComponentTemplate", contentComponentTemplateValueGuid.ToString() );
            this.SetAttributeValue( "AllowMultipleContentItems", cbAllowMultipleContentItems.Checked.ToString() );
            this.SetAttributeValue( "OutputCacheDuration", nbOutputCacheDuration.Text );
            this.SetAttributeValue( "CacheTags", cblCacheTags.SelectedValues.AsDelimited( "," ) );
            if ( dataViewFilter != null )
            {
                this.SetAttributeValue( "FilterId", dataViewFilter.Id.ToString() );
            }
            else
            {
                this.SetAttributeValue( "FilterId", null );
            }

            this.SaveAttributeValues();

            var block = new BlockService( rockContext ).Get( this.BlockId );
            block.PreHtml = cePreHtml.Text;
            block.PostHtml = cePostHtml.Text;
            rockContext.SaveChanges();

            mdContentComponentConfig.Hide();
            pnlContentComponentConfig.Visible = false;

            RemoveCacheItem( OUTPUT_CACHE_KEY );
            RemoveCacheItem( ITEM_CACHE_KEY );

            // reload the page to make sure we have a clean load
            NavigateToCurrentPageReference();
        }

        #endregion Content Component - Config

        #region Content Component - Edit Content

        /// <summary>
        /// Edits the content channel item.
        /// </summary>
        /// <param name="contentChannelItemId">The content channel item identifier.</param>
        private void EditContentChannelItem( int? contentChannelItemId )
        {
            var allowMultipleContentItems = this.GetAttributeValue( AttributeKey.AllowMultipleContentItems ).AsBoolean();
            var rockContext = new RockContext();
            ContentChannelItem contentChannelItem = null;
            if ( contentChannelItemId.HasValue )
            {
                contentChannelItem = new ContentChannelItemService( rockContext ).Get( contentChannelItemId.Value );
            }

            if ( contentChannelItem == null )
            {
                contentChannelItem = new ContentChannelItem();
                var contentChannel = this.GetContentChannel();
                contentChannelItem.ContentChannelTypeId = contentChannel.ContentChannelTypeId;
                contentChannelItem.ContentChannelId = contentChannel.Id;
            }

            hfContentChannelItemId.Value = contentChannelItem.Id.ToString();
            tbContentChannelItemTitle.Text = contentChannelItem.Title;
            htmlContentChannelItemContent.Text = contentChannelItem.Content;

            contentChannelItem.LoadAttributes();

            AttributeCache[] includedAttributes = new AttributeCache[0];
            var contentComponentTemplateValueGuid = this.GetAttributeValue( AttributeKey.ContentComponentTemplate ).AsGuidOrNull();
            if ( contentComponentTemplateValueGuid.HasValue )
            {
                var contentComponentTemplate = DefinedValueCache.Get( contentComponentTemplateValueGuid.Value );
                if ( contentComponentTemplate != null )
                {
                    var contentChannelItemAttributes = contentChannelItem.Attributes.Select( a => a.Value );

                    // Special case for Content Components: Only show attributes that don't have a Category, or have a Category that has the same name as the Content Component Template
                    includedAttributes = contentChannelItemAttributes.Where( a => !a.Categories.Any() || a.Categories.Any( c => c.Name == contentComponentTemplate.Value ) ).ToArray();
                }
            }

            avcContentChannelItemAttributes.ShowCategoryLabel = false;
            avcContentChannelItemAttributes.NumberOfColumns = 2;
            avcContentChannelItemAttributes.IncludedAttributes = includedAttributes;
            avcContentChannelItemAttributes.ValidationGroup = mdContentComponentEditContentChannelItems.ValidationGroup;
            avcContentChannelItemAttributes.AddEditControls( contentChannelItem );

            if ( allowMultipleContentItems )
            {
                BindContentChannelItemsGrid();
            }
        }

        /// <summary>
        /// Binds the content channel items grid.
        /// </summary>
        private void BindContentChannelItemsGrid()
        {
            ContentChannelCache contentChannel = this.GetContentChannel();
            if ( contentChannel != null )
            {
                IQueryable<ContentChannelItem> contentChannelItemsQuery = new ContentChannelItemService( new RockContext() ).Queryable().Where( a => a.ContentChannelId == contentChannel.Id ).OrderBy( a => a.Order ).ThenBy( a => a.Title );

                gContentChannelItems.DataSource = contentChannelItemsQuery.AsNoTracking().ToList();
                gContentChannelItems.DataBind();
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gContentChannelItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gContentChannelItems_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var currentContentChannelItemId = hfContentChannelItemId.Value.AsInteger();
            ContentChannelItem contentChannelItem = e.Row.DataItem as ContentChannelItem;
            if (contentChannelItem != null && contentChannelItem.Id == currentContentChannelItemId )
            {
                e.Row.CssClass = "row-highlight";
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSaveItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveItem_Click( object sender, EventArgs e )
        {
            SaveContentChannelItem();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdContentComponentEditContentChannelItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdContentComponentEditContentChannelItems_SaveCloseClick( object sender, EventArgs e )
        {
            if ( mdContentComponentEditContentChannelItems.SaveButtonText == CONTENT_CHANNEL_ITEM_SAVE_TEXT )
            {
                SaveContentChannelItem();
            }

            mdContentComponentEditContentChannelItems.Hide();
            pnlContentComponentEditContentChannelItems.Visible = false;

            // reload the page to make sure we have a clean load
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Saves the content channel item.
        /// </summary>
        private void SaveContentChannelItem()
        {
            RockContext rockContext = new RockContext();
            ContentChannelItemService contentChannelItemService = new ContentChannelItemService( rockContext );
            ContentChannelItem contentChannelItem = null;
            int contentChannelItemId = hfContentChannelItemId.Value.AsInteger();
            if ( contentChannelItemId != 0 )
            {
                contentChannelItem = contentChannelItemService.Get( contentChannelItemId );
            }

            if ( contentChannelItem == null )
            {
                ContentChannelCache contentChannel = this.GetContentChannel();

                contentChannelItem = new ContentChannelItem();
                contentChannelItem.ContentChannelTypeId = contentChannel.ContentChannelTypeId;
                contentChannelItem.ContentChannelId = contentChannel.Id;
                contentChannelItem.Order = ( contentChannelItemService.Queryable().Where( a => a.ContentChannelId == contentChannel.Id ).Max( a => ( int? ) a.Order ) ?? 0 ) + 1;
                contentChannelItemService.Add( contentChannelItem );
            }

            contentChannelItem.LoadAttributes( rockContext );
            avcContentChannelItemAttributes.GetEditValues( contentChannelItem );

            contentChannelItem.Title = tbContentChannelItemTitle.Text;
            contentChannelItem.Content = htmlContentChannelItemContent.Text;

            rockContext.SaveChanges();

            // just in case this is a new contentChannelItem, set the hfContentChannelItemId to the Id after SaveChanges.
            hfContentChannelItemId.Value = contentChannelItem.Id.ToString();

            contentChannelItem.SaveAttributeValues( rockContext );

            RemoveCacheItem( OUTPUT_CACHE_KEY );
            RemoveCacheItem( ITEM_CACHE_KEY );

            BindContentChannelItemsGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gContentChannelItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        private void gContentChannelItems_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindContentChannelItemsGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gContentChannelItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gContentChannelItems_AddClick( object sender, EventArgs e )
        {
            EditContentChannelItem( null );
        }

        /// <summary>
        /// Handles the RowSelected event of the gContentChannelItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gContentChannelItems_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            EditContentChannelItem( e.RowKeyId );
        }

        /// <summary>
        /// Handles the GridReorder event of the gContentChannelItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gContentChannelItems_GridReorder( object sender, Rock.Web.UI.Controls.GridReorderEventArgs e )
        {
            var contentChannel = this.GetContentChannel();
            if ( contentChannel != null )
            {
                var rockContext = new RockContext();
                ContentChannelItemService contentChannelItemService = new ContentChannelItemService( rockContext );
                var contentChannelItems = contentChannelItemService.Queryable().Where( a => a.ContentChannelId == contentChannel.Id ).OrderBy( a => a.Order ).ThenBy( a => a.Title ).ToList();
                contentChannelItemService.Reorder( contentChannelItems, e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
                BindContentChannelItemsGrid();
            }
        }

        /// <summary>
        /// Handles the DeleteClick event of the gContentChannelItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gContentChannelItems_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var contentItemService = new ContentChannelItemService( rockContext );
            var contentItemAssociationService = new ContentChannelItemAssociationService( rockContext );
            var contentItemSlugService = new ContentChannelItemSlugService( rockContext );

            ContentChannelItem contentItem = contentItemService.Get( e.RowKeyId );

            if ( contentItem != null )
            {
                string errorMessage;
                if ( !contentItemService.CanDelete( contentItem, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                rockContext.WrapTransaction( () =>
                {
                    contentItemAssociationService.DeleteRange( contentItem.ChildItems );
                    contentItemAssociationService.DeleteRange( contentItem.ParentItems );
                    contentItemService.Delete( contentItem );
                    rockContext.SaveChanges();
                } );
            }

            BindContentChannelItemsGrid();

            // edit whatever the first item is, or create a new one
            var contentChannel = GetContentChannel();
            var contentChannelItemId = new ContentChannelItemService( rockContext ).Queryable().Where( a => a.ContentChannelId == contentChannel.Id ).OrderBy( a => a.Order ).ThenBy( a => a.Title ).Select( a => ( int? ) a.Id ).FirstOrDefault();

            EditContentChannelItem( contentChannelItemId );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowView();
        }

        #endregion

        #region Filter Related stuff

        #endregion

        /// <summary>
        /// Creates the filter control.
        /// </summary>
        /// <param name="contentChannelTypeId">The content channel type identifier.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        /// <param name="rockContext">The rock context.</param>
        private void CreateFilterControl( int contentChannelTypeId, DataViewFilter filter, bool setSelection, RockContext rockContext )
        {
            phFilters.Controls.Clear();
            if ( filter != null )
            {
                CreateFilterControl( phFilters, filter, setSelection, rockContext, contentChannelTypeId );
            }
        }

        /// <summary>
        /// Creates the filter control.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        /// <param name="rockContext">The rock context.</param>
        private void CreateFilterControl( Control parentControl, DataViewFilter filter, bool setSelection, RockContext rockContext, int contentChannelTypeId )
        {
            try
            {
                if ( filter.ExpressionType == FilterExpressionType.Filter )
                {
                    var filterControl = AddFilterField( parentControl, filter.Guid, contentChannelTypeId );

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
                }
                else
                {
                    var groupControl = new FilterGroup();
                    parentControl.Controls.Add( groupControl );
                    groupControl.DataViewFilterGuid = filter.Guid;
                    groupControl.ID = string.Format( "fg_{0}", groupControl.DataViewFilterGuid.ToString( "N" ) );
                    groupControl.FilteredEntityTypeName = typeof( Rock.Model.ContentChannelItem ).FullName;
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
                        CreateFilterControl( groupControl, childFilter, setSelection, rockContext, contentChannelTypeId );
                    }
                }
            }
            catch ( Exception ex )
            {
                this.LogException( new Exception( "Exception creating FilterControl for DataViewFilter: " + filter.Guid, ex ) );
            }
        }

        /// <summary>
        /// Sets the new data filter guids.
        /// </summary>
        /// <param name="dataViewFilter">The data view filter.</param>
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

        /// <summary>
        /// Deletes the data view filter.
        /// </summary>
        /// <param name="dataViewFilter">The data view filter.</param>
        /// <param name="service">The service.</param>
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

        /// <summary>
        /// Handles the AddFilterClick event of the groupControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void groupControl_AddFilterClick( object sender, EventArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            FilterField filterField = AddFilterField( groupControl, Guid.NewGuid(), this.ContentChannelTypeId );
            filterField.Expanded = true;
        }

        /// <summary>
        /// Creates the filter field.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="dataViewFilterGuid">The data view filter unique identifier.</param>
        /// <param name="contentChannelTypeId">The content channel type identifier.</param>
        /// <returns></returns>
        private FilterField AddFilterField( Control parentControl, Guid dataViewFilterGuid, int contentChannelTypeId )
        {
            FilterField filterField = new FilterField();

            filterField.DataViewFilterGuid = dataViewFilterGuid;

            parentControl.Controls.Add( filterField );

            filterField.ID = string.Format( "ff_{0}", filterField.DataViewFilterGuid.ToString( "N" ) );

            // Remove the 'Other Data View' and ContentChannel/ContentChannelType Filters as it doesn't really make sense to have it available in this scenario
            filterField.ExcludedFilterTypes = new string[]
            {
                typeof( Rock.Reporting.DataFilter.OtherDataViewFilter ).FullName,
                typeof( Rock.Reporting.DataFilter.NotInOtherDataViewFilter ).FullName,
                typeof( Rock.Reporting.DataFilter.ContentChannelItem.ContentChannelItemAttributesFilter ).FullName,
                typeof( Rock.Reporting.DataFilter.ContentChannelItem.ContentChannel ).FullName,
                typeof( Rock.Reporting.DataFilter.ContentChannelItem.ContentChannelType ).FullName,
            };

            filterField.DeleteClick += filterControl_DeleteClick;
            filterField.FilteredEntityTypeName = typeof( Rock.Model.ContentChannelItem ).FullName;

            return filterField;
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


    }
}