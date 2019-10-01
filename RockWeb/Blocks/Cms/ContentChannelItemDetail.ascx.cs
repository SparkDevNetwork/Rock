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
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;
using System.ComponentModel;
using Rock.Security;
using Newtonsoft.Json;
using Rock.Web;
using System.Web.UI.WebControls;
using System.Data.Entity;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Content Channel Item Detail")]
    [Category("CMS")]
    [Description("Displays the details for a content channel item.")]

    [LinkedPage( "Event Occurrence Page", order: 0, required: false )]
    [BooleanField( "Show Delete Button", "Shows a delete button for the current item.", false, order: 1 )]
    [ContentChannelField("Content Channel", "If set the block will ignore content channel query parameters", false)]
    public partial class ContentChannelItemDetail : RockBlock, IDetailBlock
    {
        #region Fields

        private string _pendingCss = "btn-default";
        private string _approvedCss = "btn-default";
        private string _deniedCss = "btn-default";

        /// <summary>
        /// Gets or sets the pending CSS.
        /// </summary>
        /// <value>
        /// The pending CSS.
        /// </value>
        protected string PendingCss
        {
            get { return _pendingCss; }
            set { _pendingCss = value; }
        }

        /// <summary>
        /// Gets or sets the approved CSS.
        /// </summary>
        /// <value>
        /// The approved CSS.
        /// </value>
        protected string ApprovedCss
        {
            get { return _approvedCss; }
            set { _approvedCss = value; }
        }

        /// <summary>
        /// Gets or sets the denied CSS.
        /// </summary>
        /// <value>
        /// The denied CSS.
        /// </value>
        protected string DeniedCss
        {
            get { return _deniedCss; }
            set { _deniedCss = value; }
        }

        private string _jsScript = @"$('#{0} .btn-toggle').click(function (e) {{

                    e.stopImmediatePropagation();

                    $(this).find('.btn').removeClass('active');
                    $(e.target).addClass('active');

                    $(this).find('a').each(function() {{
                        if ($(this).hasClass('active')) {{
                            $('#{1}').val($(this).attr('data-status'));
                            $(this).removeClass('btn-default');
                            $(this).addClass( $(this).attr('data-active-css') );
                        }} else {{
                            $(this).removeClass( $(this).attr('data-active-css') );
                            $(this).addClass('btn-default');
                        }}
                    }});

                }});

                $(document).ready( function() {{
        
                    window.addEventListener('beforeunload', function(e) {{
                        if ( $('#{2}').val() == 'true' ) {{
                            var timeout = setTimeout( function() {{
                                $('#updateProgress').hide();
                            }}, 1000 );

                            var confirmMessage = 'You have not saved your changes. Are you sure you want to continue?';    
                            ( e || window.event).returnValue = confirmMessage;
                            return confirmMessage;
                        }}
                        return;
                    }});

                    $('.js-item-details').find('input').blur( function() {{
                        $('#{2}').val('true')
                    }});

                    $('.js-item-details').find('textarea').blur( function() {{
                        $('#{2}').val('true')
                    }});

                    $('#{3}').on('summernote.blur', function() {{
                        $('#{2}').val('true')
                    }});
                }});

                function isDirty() {{
                    if ( $('#{2}').val() == 'true' ) {{
                        if ( confirm('You have not saved your changes. Are you sure you want to continue?') ) {{
                            return false;
                        }}
                        return true;
                    }}
                    return false;
                }}";
        #endregion

        #region Control Methods

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

            gChildItems.DataKeyNames = new string[] { "Id" };
            gChildItems.AllowSorting = false;
            gChildItems.Actions.ShowAdd = true;
            gChildItems.IsDeleteEnabled = true;
            gChildItems.Actions.AddClick += gChildItems_Add;
            gChildItems.GridRebind += gChildItems_GridRebind;
            gChildItems.GridReorder += gChildItems_GridReorder;
            gChildItems.EntityTypeId = EntityTypeCache.Get<ContentChannelItem>().Id;

            gParentItems.DataKeyNames = new string[] { "Id" };
            gParentItems.AllowSorting = true;
            gParentItems.Actions.ShowAdd = false;
            gParentItems.IsDeleteEnabled = false;
            gParentItems.GridRebind += gParentItems_GridRebind;
            gParentItems.EntityTypeId = EntityTypeCache.Get<ContentChannelItem>().Id;

            string clearScript = string.Format( "$('#{0}').val('false');", hfIsDirty.ClientID );
            lbSave.OnClientClick = clearScript;
            lbCancel.OnClientClick = clearScript;

            string script = string.Format( _jsScript, pnlStatus.ClientID, hfStatus.ClientID, hfIsDirty.ClientID, htmlContent.ClientID );
            ScriptManager.RegisterStartupScript( pnlStatus, pnlStatus.GetType(), "status-script-" + this.BlockId.ToString(), script, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            RockPage.AddScriptLink( "~/Scripts/Rock/slug.js" );

            if ( !Page.IsPostBack )
            {
                if (string.IsNullOrWhiteSpace(GetAttributeValue("ContentChannel")))
                {
                    ShowDetail(PageParameter("contentItemId").AsInteger(), PageParameter("contentChannelId").AsIntegerOrNull());
                }
                else
                {
                    var contentChannel = GetAttributeValue("ContentChannel").AsGuid();
                    ShowDetail(PageParameter("contentItemId").AsInteger(), new ContentChannelService(new RockContext()).Get(GetAttributeValue("ContentChannel").AsGuid()).Id);
                }
            }
            else
            {
                var rockContext = new RockContext();
                ContentChannelItem item = GetContentItem();
                item.LoadAttributes();

                phAttributes.Controls.Clear();
                Rock.Attribute.Helper.AddEditControls( item, phAttributes, false, BlockValidationGroup, 2 );

                ShowDialog();
            }
        }

        /// <summary>
        /// Gets the bread crumbs.
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            var itemIds = GetNavHierarchy().AsIntegerList();
            int? itemId = PageParameter( pageReference, "contentItemId" ).AsIntegerOrNull();
            if ( itemId != null )
            {
                itemIds.Add( itemId.Value );
            }

            foreach ( var contentItemId in itemIds )
            {
                ContentChannelItem contentItem = new ContentChannelItemService( new RockContext() ).Get( contentItemId );
                if ( contentItem != null )
                {
                    breadCrumbs.Add( new BreadCrumb( contentItem.Title, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Content Item", pageReference ) );
                }
            }

            return breadCrumbs;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            ContentChannelItem contentItem = GetContentItem( rockContext );

            if ( contentItem != null &&
                ( IsUserAuthorized( Authorization.EDIT ) || contentItem.IsAuthorized( Authorization.EDIT, CurrentPerson ) ) )
            {
                contentItem.Title = tbTitle.Text;
                contentItem.Content = htmlContent.Text;
                contentItem.Priority = nbPriority.Text.AsInteger();
                contentItem.ItemGlobalKey = contentItem.Id != 0 ? lblItemGlobalKey.Text : CreateItemGlobalKey();

                // If this is a new item and the channel is manually sorted then we need to set the order to the next number
                if ( contentItem.Id == 0 && new ContentChannelService( rockContext ).IsManuallySorted( contentItem.ContentChannelId ) )
                {
                    contentItem.Order = new ContentChannelItemService( rockContext ).GetNextItemOrderValueForContentChannel( contentItem.ContentChannelId );
                }

                if ( contentItem.ContentChannelType.IncludeTime )
                {
                    contentItem.StartDateTime = dtpStart.SelectedDateTime ?? RockDateTime.Now;
                    contentItem.ExpireDateTime = ( contentItem.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange ) ?
                        dtpExpire.SelectedDateTime : null;
                }
                else
                {
                    contentItem.StartDateTime = dpStart.SelectedDate ?? RockDateTime.Today;
                    contentItem.ExpireDateTime = ( contentItem.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange ) ?
                        dpExpire.SelectedDate : null;
                }

                if ( contentItem.ContentChannelType.DisableStatus )
                {
                    // if DisableStatus == True, just set the status to Approved
                    contentItem.Status = ContentChannelItemStatus.Approved;
                }
                else
                {
                    int newStatusID = hfStatus.Value.AsIntegerOrNull() ?? contentItem.Status.ConvertToInt();
                    int oldStatusId = contentItem.Status.ConvertToInt();
                    if ( newStatusID != oldStatusId && contentItem.IsAuthorized( Authorization.APPROVE, CurrentPerson ) )
                    {
                        contentItem.Status = hfStatus.Value.ConvertToEnum<ContentChannelItemStatus>( ContentChannelItemStatus.PendingApproval );
                        if ( contentItem.Status == ContentChannelItemStatus.PendingApproval )
                        {
                            contentItem.ApprovedDateTime = null;
                            contentItem.ApprovedByPersonAliasId = null;
                        }
                        else
                        {
                            contentItem.ApprovedDateTime = RockDateTime.Now;
                            contentItem.ApprovedByPersonAliasId = CurrentPersonAliasId;
                        }
                    }

                    // remove approved status if they do not have approve access when editing
                    if ( !contentItem.IsAuthorized( Authorization.APPROVE, CurrentPerson ) )
                    {
                        contentItem.ApprovedDateTime = null;
                        contentItem.ApprovedByPersonAliasId = null;
                        contentItem.Status = ContentChannelItemStatus.PendingApproval;
                    }
                }

                contentItem.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phAttributes, contentItem );

                if ( !Page.IsValid || !contentItem.IsValid )
                {
                    // Controls will render the error messages                    
                    return;
                }

                rockContext.WrapTransaction( () =>
                {
                    if ( !string.IsNullOrEmpty( hfSlug.Value ) )
                    {
                        var contentChannelItemSlugService = new ContentChannelItemSlugService( rockContext );
                        contentChannelItemSlugService.SaveSlug( contentItem.Id, hfSlug.Value, null );
                    }

                    rockContext.SaveChanges();
                    contentItem.SaveAttributeValues( rockContext );

                    if ( contentItem.ContentChannel.IsTaggingEnabled )
                    {
                        taglTags.EntityGuid = contentItem.Guid;
                        taglTags.SaveTagValues( CurrentPersonAlias );
                    }

                    int? eventItemOccurrenceId = PageParameter( "EventItemOccurrenceId" ).AsIntegerOrNull();
                    if ( eventItemOccurrenceId.HasValue )
                    {
                        var occurrenceChannelItemService = new EventItemOccurrenceChannelItemService( rockContext );
                        var occurrenceChannelItem = occurrenceChannelItemService
                            .Queryable()
                            .Where( c =>
                                c.ContentChannelItemId == contentItem.Id &&
                                c.EventItemOccurrenceId == eventItemOccurrenceId.Value)
                            .FirstOrDefault();

                        if ( occurrenceChannelItem == null )
                        {
                            occurrenceChannelItem = new EventItemOccurrenceChannelItem();
                            occurrenceChannelItem.ContentChannelItemId = contentItem.Id;
                            occurrenceChannelItem.EventItemOccurrenceId = eventItemOccurrenceId.Value;
                            occurrenceChannelItemService.Add( occurrenceChannelItem );
                            rockContext.SaveChanges();
                        }
                    }
                } );

                ReturnToParentPage();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDelete_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            var contentItemService = new ContentChannelItemService( rockContext );
            ContentChannelItem contentItem = null;

            int contentItemId = hfId.Value.AsInteger();
            if ( contentItemId != 0 )
            {
                contentItem = contentItemService
                    .Queryable( "ContentChannel,ContentChannelType" )
                    .FirstOrDefault( t => t.Id == contentItemId );
            }

            if (contentItem != null )
            {
                contentItemService.Delete( contentItem );
                rockContext.SaveChanges();
            }

            ReturnToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            ReturnToParentPage();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( hfId.ValueAsInt() );
        }

        protected void rSlugs_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var lChannelUrl = e.Item.FindControl( "lChannelUrl" ) as Literal;
            var slug = e.Item.DataItem as ContentChannelItemSlug;

            if ( lChannelUrl != null && slug != null )
            {
                lChannelUrl.Text = GetSlugPrefix( slug.ContentChannelItem.ContentChannel );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbRefreshItemGlobalKey control.
        /// Update the hidden field, value is not saved to the DB until the save button is clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRefreshItemGlobalKey_Click( object sender, EventArgs e )
        {
            lblItemGlobalKey.Text = CreateItemGlobalKey();
        }

        #region Child/Parent List Events

        private void gChildItems_GridRebind( object sender, GridRebindEventArgs e )
        {
            var contentItem = GetContentItem();
            if ( contentItem != null )
            {
                BindChildItemsGrid( contentItem );
            }
        }

        private void gChildItems_Add( object sender, EventArgs e )
        {
            ddlAddNewItemChannel.Items.Clear();
            ddlAddExistingItemChannel.Items.Clear();
            ddlAddExistingItem.Items.Clear();

            ddlAddNewItemChannel.Items.Add( new ListItem() );
            ddlAddExistingItemChannel.Items.Add( new ListItem() );

            var contentItem = GetContentItem();
            if ( contentItem != null && contentItem.ContentChannel != null && contentItem.ContentChannel.ChildContentChannels != null )
            {
                foreach ( var channel in contentItem.ContentChannel.ChildContentChannels
                    .OrderBy( c => c.Name ) )
                {
                    if ( channel.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        ddlAddNewItemChannel.Items.Add( new ListItem( channel.Name, channel.Id.ToString() ) );
                        ddlAddExistingItemChannel.Items.Add( new ListItem( channel.Name, channel.Id.ToString() ) );
                    }
                }
            }

            ShowDialog( "AddChildItem", true );
        }

        private void gChildItems_GridReorder( object sender, GridReorderEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var contentItem = GetContentItem( rockContext );
                if ( contentItem != null )
                {
                    bool isFiltered = false;
                    var items = GetChildItems( contentItem, out isFiltered ).OrderBy( a => a.Order ).ToList();

                    // If the list was filtered due to VIEW security, don't sort it
                    if ( !isFiltered )
                    {
                        var service = new ContentChannelItemService( rockContext );
                        service.Reorder( items, e.OldIndex, e.NewIndex );
                        rockContext.SaveChanges();
                    }
                }

                BindChildItemsGrid( contentItem );
            }
        }

        protected void gChildItems_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToNewItem( e.RowKeyId.ToString() );
        }

        protected void gChildItems_Delete( object sender, RowEventArgs e )
        {
            hfRemoveChildItem.Value = e.RowKeyId.ToString();
            ShowDialog( "RemoveChildItem", true );
        }

        private void gParentItems_GridRebind( object sender, GridRebindEventArgs e )
        {
            var contentItem = GetContentItem();
            if ( contentItem != null )
            {
                BindParentItemsGrid( contentItem );
            }
        }

        protected void gParentItems_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToNewItem( e.RowKeyId.ToString() );
        }

        protected void lbAddNewChildItem_Click( object sender, EventArgs e )
        {
            int? channelId = ddlAddNewItemChannel.SelectedValueAsInt();
            if ( channelId.HasValue )
            {
                var qryParams = new Dictionary<string, string>();
                qryParams.Add( "contentItemId", "0" );
                qryParams.Add( "contentChannelId", channelId.Value.ToString() );

                var hierarchy = GetNavHierarchy();
                hierarchy.Add( hfId.Value );
                qryParams.Add( "Hierarchy", hierarchy.AsDelimited( "," ) );

                NavigateToCurrentPage( qryParams );
            }
        }

        protected void lbAddExistingChildItem_Click( object sender, EventArgs e )
        {
            int? itemId = hfId.Value.AsIntegerOrNull();
            int? childItemId = ddlAddExistingItem.SelectedValueAsInt();

            if ( itemId.HasValue && childItemId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var service = new ContentChannelItemAssociationService( rockContext );
                    var order = service.Queryable().AsNoTracking()
                        .Where( a => a.ContentChannelItemId == itemId.Value )
                        .Select( a => (int?)a.Order )
                        .DefaultIfEmpty()
                        .Max();

                    var assoc = new ContentChannelItemAssociation();
                    assoc.ContentChannelItemId = itemId.Value;
                    assoc.ChildContentChannelItemId = childItemId.Value;
                    assoc.Order = order.HasValue ? order.Value + 1 : 0;
                    service.Add( assoc );

                    rockContext.SaveChanges();
                }
            }

            BindChildItemsGrid( GetContentItem() );

            HideDialog();
        }

        protected void ddlAddExistingItemChannel_SelectedIndexChanged( object sender, EventArgs e )
        {
            ddlAddExistingItem.Items.Clear();
            int? channelId = ddlAddExistingItemChannel.SelectedValueAsInt();
            if ( channelId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var contentItem = GetContentItem( rockContext );

                    var channel = new ContentChannelService( rockContext ).Get( channelId.Value );

                    var items = new List<ContentChannelItem>();
                    foreach ( var item in channel.Items )
                    {
                        if ( !contentItem.ChildItems.Any( i => i.ChildContentChannelItemId == item.Id ) &&
                            item.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        {
                            items.Add( item );
                        }
                    }

                    if ( channel.ItemsManuallyOrdered )
                    {
                        items = items.OrderBy( i => i.Order ).ToList();
                    }
                    else
                    {
                        items = items.OrderByDescending( i => i.StartDateTime ).ToList();
                    }

                    foreach ( var item in items )
                    {
                        ddlAddExistingItem.Items.Add( new ListItem( string.Format( "{0} ({1})", item.Title, item.StartDateTime.ToShortDateString() ), item.Id.ToString() ) );
                    }
                }
            }
        }

        protected void lbRemoveChildItem_Click( object sender, EventArgs e )
        {
            int? itemId = hfId.Value.AsIntegerOrNull();
            int? childItemId = hfRemoveChildItem.Value.AsIntegerOrNull();

            if ( itemId.HasValue && childItemId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var assocService = new ContentChannelItemAssociationService( rockContext );
                    var assoc = assocService.Queryable()
                        .Where( a =>
                            a.ContentChannelItemId == itemId.Value &&
                            a.ChildContentChannelItemId == childItemId.Value )
                        .FirstOrDefault();

                    if ( assoc != null )
                    {
                        assocService.Delete( assoc );
                        rockContext.SaveChanges();
                    }
                }
            }

            BindChildItemsGrid( GetContentItem() );

            HideDialog();
        }

        protected void lbDeleteChildItem_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                int childItemId = hfRemoveChildItem.ValueAsInt();

                var service = new ContentChannelItemService( rockContext );
                var childItem = service.Get( childItemId );
                if ( childItem != null )
                {
                    service.Delete( childItem );
                    rockContext.SaveChanges();
                }
            }

            BindChildItemsGrid( GetContentItem() );

            HideDialog();
        }

        #endregion

        #endregion

        #region Internal Methods

        private string CreateItemGlobalKey()
        {
            using ( var rockContext = new RockContext() )
            {
                var contentChannelItemSlugService = new ContentChannelItemSlugService( rockContext );
                return contentChannelItemSlugService.GetUniqueContentSlug( tbTitle.Text, null );
            }
        }
        /// <summary>
        /// Gets the slug prefix.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <returns></returns>
        private string GetSlugPrefix( ContentChannel channel )
        {
            if ( channel.ItemUrl.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            var itemUrl = channel.ItemUrl.RemoveSpaces();

            if ( itemUrl.EndsWith( "{{Slug}}" ) )
            {
                return itemUrl.Replace( "{{Slug}}", "" );
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the type of the content.
        /// </summary>
        /// <param name="contentItemId">The content type identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private ContentChannelItem GetContentItem( RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var contentItemService = new ContentChannelItemService( rockContext );
            ContentChannelItem contentItem = null;

            int contentItemId = hfId.Value.AsInteger();
            if ( contentItemId != 0 )
            {
                contentItem = contentItemService
                    .Queryable( "ContentChannel,ContentChannelType" )
                    .FirstOrDefault( t => t.Id == contentItemId );
            }

            if ( contentItem == null)
            {
                var contentChannel = new ContentChannelService( rockContext ).Get( hfChannelId.Value.AsInteger() );
                if ( contentChannel != null )
                {
                    contentItem = new ContentChannelItem
                    {
                        ContentChannel = contentChannel,
                        ContentChannelId = contentChannel.Id,
                        ContentChannelType = contentChannel.ContentChannelType,
                        ContentChannelTypeId = contentChannel.ContentChannelType.Id,
                        StartDateTime = RockDateTime.Now
                    };

                    var hierarchy = GetNavHierarchy();
                    if ( hierarchy.Any() )
                    {
                        var parentItem = contentItemService.Get( hierarchy.Last().AsInteger() );
                        if ( parentItem != null &&
                            parentItem.IsAuthorized( Authorization.EDIT, CurrentPerson ) &&
                            parentItem.ContentChannel.ChildContentChannels.Any( c => c.Id == contentChannel.Id ) )
                        {
                            var order = parentItem.ChildItems
                                .Select( a => (int?)a.Order )
                                .DefaultIfEmpty()
                                .Max();

                            var assoc = new ContentChannelItemAssociation();
                            assoc.ContentChannelItemId = parentItem.Id;
                            assoc.Order = order.HasValue ? order.Value + 1 : 0;
                            contentItem.ParentItems.Add( assoc );
                        }
                    }

                    if ( contentChannel.RequiresApproval )
                    {
                        contentItem.Status = ContentChannelItemStatus.PendingApproval;
                    }
                    else
                    {
                        contentItem.Status = ContentChannelItemStatus.Approved;
                        contentItem.ApprovedDateTime = RockDateTime.Now;
                        contentItem.ApprovedByPersonAliasId = CurrentPersonAliasId;
                    }

                    contentItemService.Add( contentItem );
                }
            }

            return contentItem;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="contentItemId">The content item identifier.</param>
        public void ShowDetail( int contentItemId )
        {
            ShowDetail( contentItemId, null );
        }

        public void ShowDetail( int contentItemId, int? contentChannelId )
        {
            bool canEdit = IsUserAuthorized( Authorization.EDIT );
            hfId.Value = contentItemId.ToString();
            hfChannelId.Value = contentChannelId.HasValue ? contentChannelId.Value.ToString() : string.Empty;

            ContentChannelItem contentItem = GetContentItem();

            if ( contentItem == null )
            {
                // this block requires a valid ContentChannel in order to know which channel the ContentChannelItem belongs to, so if ContentChannel wasn't specified, don't show this block
                this.Visible = false;
                return;
            }

            hfContentChannelItemUrl.Value = GetSlugPrefix( contentItem.ContentChannel );

            if ( contentItem.ContentChannel.IsTaggingEnabled )
            {
                taglTags.EntityTypeId = EntityTypeCache.Get( typeof( ContentChannelItem ) ).Id;
                taglTags.CategoryGuid = ( contentItem.ContentChannel != null && contentItem.ContentChannel.ItemTagCategory != null ) ?
                     contentItem.ContentChannel.ItemTagCategory.Guid : (Guid?)null;
                taglTags.EntityGuid = contentItem.Guid;
                taglTags.DelaySave = true;
                taglTags.GetTagValues( CurrentPersonId );
                rcwTags.Visible = true;
            }
            else
            {
                rcwTags.Visible = false;
            }

            pdAuditDetails.SetEntity( contentItem, ResolveRockUrl( "~" ) );

            if ( contentItem != null &&
                contentItem.ContentChannelType != null &&
                contentItem.ContentChannel != null &&
                ( canEdit || contentItem.IsAuthorized( Authorization.EDIT, CurrentPerson ) ) )
            {
                hfIsDirty.Value = "false";

                ShowApproval( contentItem );

                pnlEditDetails.Visible = true;

                // show/hide the delete button
                lbDelete.Visible = GetAttributeValue( "ShowDeleteButton" ).AsBoolean() && contentItem.Id != 0;

                hfId.Value = contentItem.Id.ToString();
                hfChannelId.Value = contentItem.ContentChannelId.ToString();

                string cssIcon = contentItem.ContentChannel.IconCssClass;
                if ( string.IsNullOrWhiteSpace( cssIcon ) )
                {
                    cssIcon = "fa fa-certificate";
                }

                lIcon.Text = string.Format( "<i class='{0}'></i>", cssIcon );

                string title = contentItem.Id > 0 ?
                    ActionTitle.Edit( ContentChannelItem.FriendlyTypeName ) :
                    ActionTitle.Add( ContentChannelItem.FriendlyTypeName );
                lTitle.Text = title.FormatAsHtmlTitle();

                hlContentChannel.Text = contentItem.ContentChannel.Name;

                hlStatus.Visible = contentItem.ContentChannel.RequiresApproval && !contentItem.ContentChannelType.DisableStatus;

                hlStatus.Text = contentItem.Status.ConvertToString();

                hlStatus.LabelType = LabelType.Default;
                switch ( contentItem.Status )
                {
                    case ContentChannelItemStatus.Approved:
                        hlStatus.LabelType = LabelType.Success;
                        break;

                    case ContentChannelItemStatus.Denied:
                        hlStatus.LabelType = LabelType.Danger;
                        break;

                    case ContentChannelItemStatus.PendingApproval:
                        hlStatus.LabelType = LabelType.Warning;
                        break;

                    default:
                        hlStatus.LabelType = LabelType.Default;
                        break;
                }

                var statusDetail = new System.Text.StringBuilder();
                if ( contentItem.ApprovedByPersonAlias != null && contentItem.ApprovedByPersonAlias.Person != null )
                {
                    statusDetail.AppendFormat( "by {0} ", contentItem.ApprovedByPersonAlias.Person.FullName );
                }

                if ( contentItem.ApprovedDateTime.HasValue )
                {
                    statusDetail.AppendFormat( "on {0} at {1}", contentItem.ApprovedDateTime.Value.ToShortDateString(), contentItem.ApprovedDateTime.Value.ToShortTimeString() );
                }

                hlStatus.ToolTip = statusDetail.ToString();

                tbTitle.Text = contentItem.Title;

                rSlugs.DataSource =  contentItem.ContentChannelItemSlugs;
                rSlugs.DataBind();

                htmlContent.Visible = !contentItem.ContentChannelType.DisableContentField;
                htmlContent.Text = contentItem.Content;
                htmlContent.MergeFields.Clear();
                htmlContent.MergeFields.Add( "GlobalAttribute" );
                htmlContent.MergeFields.Add( "Rock.Model.ContentChannelItem|Current Item" );
                htmlContent.MergeFields.Add( "Rock.Model.Person|Current Person" );
                htmlContent.MergeFields.Add( "Campuses" );
                htmlContent.MergeFields.Add( "RockVersion" );

                if ( !string.IsNullOrWhiteSpace( contentItem.ContentChannel.RootImageDirectory ) )
                {
                    htmlContent.DocumentFolderRoot = contentItem.ContentChannel.RootImageDirectory;
                    htmlContent.ImageFolderRoot = contentItem.ContentChannel.RootImageDirectory;
                }

                htmlContent.StartInCodeEditorMode = contentItem.ContentChannel.ContentControlType == ContentControlType.CodeEditor;

                if ( contentItem.ContentChannelType.IncludeTime )
                {
                    dpStart.Visible = false;
                    dpExpire.Visible = false;
                    dtpStart.Visible = contentItem.ContentChannelType.DateRangeType != ContentChannelDateType.NoDates;
                    dtpExpire.Visible = contentItem.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange;

                    dtpStart.SelectedDateTime = contentItem.StartDateTime;
                    dtpStart.Label = contentItem.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange ? "Start" : "Active";
                    dtpExpire.SelectedDateTime = contentItem.ExpireDateTime;
                }
                else
                {
                    dpStart.Visible = contentItem.ContentChannelType.DateRangeType != ContentChannelDateType.NoDates;
                    dpExpire.Visible = contentItem.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange;
                    dtpStart.Visible = false;
                    dtpExpire.Visible = false;

                    dpStart.SelectedDate = contentItem.StartDateTime.Date;
                    dpStart.Label = contentItem.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange ? "Start" : "Active";
                    dpExpire.SelectedDate = contentItem.ExpireDateTime.HasValue ? contentItem.ExpireDateTime.Value.Date : (DateTime?)null;
                }

                lblItemGlobalKey.Text = contentItem.ItemGlobalKey;

                nbPriority.Text = contentItem.Priority.ToString();
                nbPriority.Visible = !contentItem.ContentChannelType.DisablePriority;

                contentItem.LoadAttributes();
                phAttributes.Controls.Clear();
                Rock.Attribute.Helper.AddEditControls( contentItem, phAttributes, true, BlockValidationGroup, 2 );

                phOccurrences.Controls.Clear();
                foreach ( var occurrence in contentItem.EventItemOccurrences
                    .Where( o => o.EventItemOccurrence != null )
                    .Select( o => o.EventItemOccurrence ) )
                {
                    var qryParams = new Dictionary<string, string> { { "EventItemOccurrenceId", occurrence.Id.ToString() } };
                    string url = LinkedPageUrl( "EventOccurrencePage", qryParams );
                    var hlOccurrence = new HighlightLabel();
                    hlOccurrence.LabelType = LabelType.Info;
                    hlOccurrence.ID = string.Format( "hlOccurrence_{0}", occurrence.Id );
                    hlOccurrence.Text = string.Format( "<a href='{0}'><i class='fa fa-calendar-o'></i> {1}</a>", url, occurrence.ToString() );
                    phOccurrences.Controls.Add( hlOccurrence );
                }

                bool canHaveChildren = contentItem.Id > 0 && contentItem.ContentChannel.ChildContentChannels.Any();
                bool canHaveParents = contentItem.Id > 0 && contentItem.ContentChannel.ParentContentChannels.Any();

                pnlChildrenParents.Visible = canHaveChildren || canHaveParents;
                phPills.Visible = canHaveChildren && canHaveParents;
                if ( canHaveChildren && !canHaveParents )
                {
                    lChildrenParentsTitle.Text = "<i class='fa fa-arrow-circle-down'></i> Child Items";
                }

                if ( !canHaveChildren && canHaveParents )
                {
                    lChildrenParentsTitle.Text = "<i class='fa fa-arrow-circle-up'></i> Parent Items";
                    divParentItems.AddCssClass( "active" );
                }

                if ( canHaveChildren )
                {
                    BindChildItemsGrid( contentItem );
                }

                if ( canHaveParents )
                {
                    BindParentItemsGrid( contentItem );
                }
            }
            else
            {
                nbEditModeMessage.Text = EditModeMessage.NotAuthorizedToEdit( ContentChannelItem.FriendlyTypeName );
                pnlEditDetails.Visible = false;
            }
        }

        private void ShowApproval( ContentChannelItem contentItem )
        {
            if ( contentItem != null && contentItem.ContentChannel != null && contentItem.ContentChannel.RequiresApproval )
            {
                if ( contentItem.IsAuthorized( Authorization.APPROVE, CurrentPerson ) )
                {
                    pnlStatus.Visible = true;

                    PendingCss = contentItem.Status == ContentChannelItemStatus.PendingApproval ? "btn-default active" : "btn-default";
                    ApprovedCss = contentItem.Status == ContentChannelItemStatus.Approved ? "btn-success active" : "btn-default";
                    DeniedCss = contentItem.Status == ContentChannelItemStatus.Denied ? "btn-danger active" : "btn-default";
                }
                else
                {
                    pnlStatus.Visible = false;
                }

                hfStatus.Value = contentItem.Status.ConvertToInt().ToString();
            }
            else
            {
                hfStatus.Value = ContentChannelItemStatus.Approved.ToString();
                pnlStatus.Visible = false;
                divStatus.Visible = false;
            }
        }

        private void NavigateToNewItem( string itemId )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "contentItemId", itemId );

            var hierarchy = GetNavHierarchy();
            hierarchy.Add( hfId.Value );

            var newHierarchy = new List<string>();
            foreach ( string existingItemId in hierarchy )
            {
                if ( existingItemId != itemId )
                {
                    newHierarchy.Add( existingItemId );
                }
                else
                {
                    break;
                }
            }

            qryParams.Add( "Hierarchy", newHierarchy.AsDelimited( "," ) );

            NavigateToCurrentPage( qryParams );
        }

        /// <summary>
        /// Returns to parent page.
        /// </summary>
        private void ReturnToParentPage()
        {
            var qryParams = new Dictionary<string, string>();

            int? eventItemOccurrenceId = PageParameter( "EventItemOccurrenceId" ).AsIntegerOrNull();
            if ( eventItemOccurrenceId.HasValue )
            {
                qryParams.Add( "EventCalendarId", PageParameter( "EventCalendarId" ) );
                qryParams.Add( "EventItemId", PageParameter( "EventItemId" ) );
                qryParams.Add( "EventItemOccurrenceId", eventItemOccurrenceId.Value.ToString() );
                qryParams.Add( "ContentChannelId", hfChannelId.Value );
                NavigateToParentPage( qryParams );
            }
            else
            {
                var hierarchy = GetNavHierarchy();

                var newHierarchy = new List<string>();
                foreach ( string itemId in hierarchy )
                {
                    if ( itemId != hfId.Value )
                    {
                        newHierarchy.Add( itemId );
                    }
                    else
                    {
                        break;
                    }
                }

                if ( newHierarchy.Any() )
                {
                    if ( newHierarchy.Count > 1 )
                    {
                        qryParams.Add( "Hierarchy", newHierarchy.Take( newHierarchy.Count() - 1 ).ToList().AsDelimited( "," ) );
                    }

                    qryParams.Add( "ContentItemId", newHierarchy.Last() );
                    NavigateToCurrentPage( qryParams );
                }
                else
                {
                    qryParams.Add( "ContentChannelId", hfChannelId.Value );
                    NavigateToParentPage( qryParams );
                }
            }
        }

        #region Child/Parent List Methods

        private void BindChildItemsGrid( ContentChannelItem contentItem )
        {
            bool isFiltered = false;
            var items = GetChildItems( contentItem, out isFiltered );

            if ( contentItem.ContentChannel.ChildItemsManuallyOrdered && !isFiltered )
            {
                gChildItems.Columns[0].Visible = true;
                gChildItems.AllowSorting = false;
                items = items.OrderBy( i => i.Order ).ToList();
            }
            else
            {
                gChildItems.Columns[0].Visible = false;
                gChildItems.AllowSorting = true;

                SortProperty sortProperty = gChildItems.SortProperty;
                if ( sortProperty != null )
                {
                    items = items.AsQueryable().Sort( sortProperty ).ToList();
                }
                else
                {
                    items = items.OrderByDescending( p => p.StartDateTime ).ToList();
                }
            }

            gChildItems.ObjectList = new Dictionary<string, object>();
            items.ForEach( i => gChildItems.ObjectList.Add( i.Id.ToString(), i ) );

            gChildItems.DataSource = items.Select( i => new
            {
                i.Id,
                i.Guid,
                i.Title,
                i.StartDateTime,
                ExpireDateTime = i.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange ? i.ExpireDateTime : (DateTime?)null,
                Priority = i.ContentChannelType.DisablePriority ? (int?)null : (int?)i.Priority,
                Status = (i.ContentChannel.RequiresApproval && !i.ContentChannelType.DisableStatus) ? DisplayStatus( i.Status ) : string.Empty,
                CreatedBy = i.CreatedByPersonAlias != null && i.CreatedByPersonAlias.Person != null ? i.CreatedByPersonAlias.Person.NickName + " " + i.CreatedByPersonAlias.Person.LastName : string.Empty
            } ).ToList();

            gChildItems.DataBind();
        }

        private void BindParentItemsGrid( ContentChannelItem contentItem )
        {
            var items = GetParentItems( contentItem );

            SortProperty sortProperty = gParentItems.SortProperty;
            if ( sortProperty != null )
            {
                items = items.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                items = items.OrderByDescending( p => p.StartDateTime ).ToList();
            }

            gParentItems.ObjectList = new Dictionary<string, object>();
            items.ForEach( i => gParentItems.ObjectList.Add( i.Id.ToString(), i ) );

            gParentItems.DataSource = items.Select( i => new
            {
                i.Id,
                i.Guid,
                i.Title,
                StartDateTime = i.ContentChannelType.DateRangeType != ContentChannelDateType.NoDates ? i.StartDateTime : (DateTime?)null,
                ExpireDateTime = i.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange ? i.ExpireDateTime : (DateTime?)null,
                Priority = i.ContentChannelType.DisablePriority ? (int?)null : (int?)i.Priority,
                Status = (i.ContentChannel.RequiresApproval && !i.ContentChannelType.DisableStatus) ? DisplayStatus( i.Status ) : string.Empty,
                CreatedBy = i.CreatedByPersonAlias != null && i.CreatedByPersonAlias.Person != null ? i.CreatedByPersonAlias.Person.NickName + " " + i.CreatedByPersonAlias.Person.LastName : string.Empty
            } ).ToList();
            gParentItems.DataBind();
        }

        private List<ContentChannelItem> GetChildItems( ContentChannelItem contentItem, out bool isFiltered )
        {
            isFiltered = false;
            var items = new List<ContentChannelItem>();

            foreach ( var item in contentItem.ChildItems.Select( a => a.ChildContentChannelItem ).ToList() )
            {
                if ( item.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    items.Add( item );
                }
                else
                {
                    isFiltered = true;
                }
            }

            return items;
        }

        private List<ContentChannelItem> GetParentItems( ContentChannelItem contentItem )
        {
            var items = new List<ContentChannelItem>();

            foreach ( var item in contentItem.ParentItems.Select( a => a.ContentChannelItem ).ToList() )
            {
                if ( item.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    items.Add( item );
                }
            }

            return items;
        }

        #endregion

        protected string DisplayStatus( ContentChannelItemStatus contentItemStatus )
        {
            string labelType = "default";
            if ( contentItemStatus == ContentChannelItemStatus.Approved )
            {
                labelType = "success";
            }
            else if ( contentItemStatus == ContentChannelItemStatus.Denied )
            {
                labelType = "danger";
            }

            return string.Format( "<span class='label label-{0}'>{1}</span>", labelType, contentItemStatus.ConvertToString() );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "ADDCHILDITEM":
                    dlgAddChild.Show();
                    break;

                case "REMOVECHILDITEM":
                    dlgRemoveChild.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "ADDCHILDITEM":
                    dlgAddChild.Hide();
                    break;

                case "REMOVECHILDITEM":
                    dlgRemoveChild.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        private List<string> GetNavHierarchy()
        {
            var qryParam = PageParameter( "Hierarchy" );
            if ( !string.IsNullOrWhiteSpace( qryParam ) )
            {
                return qryParam.SplitDelimitedValues( false ).ToList();
            }

            return new List<string>();
        }

        #endregion


    }
}