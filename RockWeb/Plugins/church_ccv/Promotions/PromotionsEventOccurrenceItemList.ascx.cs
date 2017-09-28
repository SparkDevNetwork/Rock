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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using church.ccv.Promotions.Data;
using church.ccv.Promotions.Model;
using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.church_ccv.Promotions
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Promotions Event Occurrence Item List" )]
    [Category( "CCV > Promotions" )]
    [Description("Lists the content channel item promotional items associated with this Occurence, as well as their state.")]
    [SystemEmailField( "Event Changed Email", "Email that will be sent if this event is altered after a promotion has been created for it.", false)]
    public partial class PromotionsEventOccurrenceItemList : RockBlock, ISecondaryBlock
    {

        #region Properties
        
        private int EventItemOccurrenceId { get; set; }
        private int EventCalendarId { get; set; }
        private int EventItemId { get; set; }
        
        private List<ContentChannel> ContentChannels { get; set; }
        Grid gItems { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            EventItemOccurrenceId = (int)ViewState["EventItemOccurrenceId"];

            string json = ViewState["ContentChannels"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                ContentChannels = new List<ContentChannel>();
            }
            else
            {
                ContentChannels = JsonConvert.DeserializeObject<List<ContentChannel>>( json );
            }

            CreateGrids( new RockContext() );
            BindGrids();
        }

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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                var rockContext = new RockContext();
                
                EventItemOccurrenceId = PageParameter( "EventItemOccurrenceId" ).AsInteger( );
                EventCalendarId = PageParameter( "EventCalendarId" ).AsInteger( );
                EventItemId = PageParameter( "EventItemId" ).AsInteger( );

                ContentChannels = new List<ContentChannel>();

                var channels = new Dictionary<int, ContentChannel>();

                var eventItemOccurrence = new EventItemOccurrenceService( rockContext ).Get( EventItemOccurrenceId );
                if ( eventItemOccurrence != null && eventItemOccurrence.EventItem != null && eventItemOccurrence.EventItem.EventCalendarItems != null )
                {
                    eventItemOccurrence.EventItem.EventCalendarItems
                        .SelectMany( i => i.EventCalendar.ContentChannels )
                        .Select( c => c.ContentChannel )
                        .ToList()
                        .ForEach( c => channels.AddOrIgnore( c.Id, c ) );
                }

                foreach( var channel in channels )
                {
                    // JHM: 4-7 - WE want to allow them to request for ANY channel type.
                    //if ( channel.Value.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        ContentChannels.Add( channel.Value );
                    }
                }

                CreateGrids( rockContext );
                BindGrids();
            }

            TrySendEventChangedEmail( );

            base.OnLoad( e );
        }

        void TrySendEventChangedEmail( )
        {
            // if the modified date of this event is greater than the content item modified date,
            // we should send an email.
            RockContext rockContext = new RockContext( );
            var eventItemOccurrence = new EventItemOccurrenceService( rockContext ).Get( EventItemOccurrenceId );
            if( eventItemOccurrence == null )
            {
                // if it's null, send an email. the event was deleted.
            }
            // otherwise, as long as it has a future start date, check
            else if( eventItemOccurrence.NextStartDateTime >= RockDateTime.Now )
            {
                // does it have any promotion occurrences where this event has been modified AFTER?
                PromotionsService<PromotionRequest> promoRequestService = new PromotionsService<PromotionRequest>( rockContext );
                var promoRequestList = promoRequestService.Queryable( ).Where( pr => pr.EventItemOccurrenceId == EventItemOccurrenceId && pr.EventLastModifiedTime < eventItemOccurrence.ModifiedDateTime ).ToList( );

                if( promoRequestList.Count > 0 )
                {
                    // update their modified time
                    foreach( PromotionRequest promoRequest in promoRequestList )
                    {
                        promoRequest.EventLastModifiedTime = eventItemOccurrence.ModifiedDateTime;
                    }
                    rockContext.SaveChanges( );

                    var promoRequestIds = promoRequestList.Select( pr => pr.Id ).ToList( );

                    // get all the promotion occurrences tied to this event.
                    PromotionsService<PromotionOccurrence> promoOccurrenceService = new PromotionsService<PromotionOccurrence>( rockContext );
                    var promoOccurrenceList = promoOccurrenceService.Queryable( ).Where( po => po.PromotionRequestId.HasValue && promoRequestIds.Contains( po.PromotionRequestId.Value ) ).ToList( );
                    
                    // send an email
                    var eventChangedEmailTemplateGuid = GetAttributeValue( "EventChangedEmail" ).AsGuidOrNull();
                    if ( eventChangedEmailTemplateGuid.HasValue )
                    {
                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                        mergeFields.Add( "Event", eventItemOccurrence );
                        mergeFields.Add( "EventCalendarId", EventCalendarId );

                        var contentChannelItemList = promoOccurrenceList.Select( po => po.ContentChannelItemId ).ToList( );
                        mergeFields.Add( "ContentChannelItemList", contentChannelItemList );
                        
                        // get the email service and email
                        SystemEmailService emailService = new SystemEmailService( rockContext );
                        SystemEmail reassignEmail = emailService.Get( eventChangedEmailTemplateGuid.Value );

                        // build a recipient list using the "To" from the system email
                        var recipients = new List<Rock.Communication.RecipientData>();

                        // add person and the mergeObjects (same mergeobjects as receipt)
                        recipients.Add( new Rock.Communication.RecipientData( reassignEmail.To, mergeFields ) );

                        Rock.Communication.Email.Send( eventChangedEmailTemplateGuid.Value, recipients, ResolveRockUrl( "~/" ), ResolveRockUrl( "~~/" ) );
                    }
                }
            }

        }

        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["EventItemOccurrenceId"] = EventItemOccurrenceId;
            ViewState["ContentChannels"] = JsonConvert.SerializeObject( ContentChannels, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Add event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gItems_Add( object sender, EventArgs e )
        {
            var grid = ( (Control)sender ).DataKeysContainer;
            if ( grid != null )
            {
                int contentChannelId = grid.ID.Substring( 7 ).AsInteger();
                NavigateToDetailPage( 0, contentChannelId );
            }
        }

        /// <summary>
        /// Handles the Edit event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gItems_Edit( object sender, RowEventArgs e )
        {
            
        }

        /// <summary>
        /// Handles the Delete event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gItems_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            ContentChannelItemService contentItemService = new ContentChannelItemService( rockContext );

            ContentChannelItem contentItem = contentItemService.Get( e.RowKeyId );

            if ( contentItem != null )
            {
                string errorMessage;
                if ( !contentItemService.CanDelete( contentItem, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                contentItemService.Delete( contentItem );
                rockContext.SaveChanges();
            }

            BindGrids();
        }

        /// <summary>
        /// Handles the GridRebind event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gItems_GridRebind( object sender, EventArgs e )
        {
            BindGrids();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrids();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on it's page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }
        
        private void CreateGrids( RockContext rockContext )
        {
            if ( ContentChannels.Any() )
            {
                this.Visible = true;

                phContentChannelGrids.Controls.Clear();

                // create a new section for each content channel type
                var section = new HtmlGenericControl( "section" );
                phContentChannelGrids.Controls.Add( section );
                section.AddCssClass( "panel panel-widget rock-panel-widget" );
                section.ID = string.Format( "contentChannels_section" );

                // create the header that shows the name of the content channel
                var header = new HtmlGenericControl( "header" );
                section.Controls.Add( header );
                header.AddCssClass( "panel-heading clearfix" );

                // create an inner container that will store the entry
                var innerDiv = new HtmlGenericControl( "div" );
                phContentChannelGrids.Controls.Add( innerDiv );
                section.Controls.Add( innerDiv );
                innerDiv.AddCssClass( "panel-body grid grid-panel" );
                innerDiv.AddCssClass( "grid" );
                innerDiv.AddCssClass( "grid-panel" );

                gItems = new Grid();
                innerDiv.Controls.Add( gItems );
                gItems.ID = "gItems_contentChannels";
                gItems.DataKeyNames = new string[] { "Id" };
                gItems.RowItemText = "Item";
                gItems.AllowSorting = false;
                gItems.IsDeleteEnabled = false;
                gItems.GridRebind += gItems_GridRebind;
                gItems.RowCommand += gItems_RowCommand;
                gItems.Actions.Visible = false;
                
                var typeField = new RockBoundField();
                gItems.Columns.Add( typeField );
                typeField.DataField = "Type";
                typeField.HeaderText = "Type";
                typeField.HtmlEncode = false;
                
                var statusField = new ButtonField();
                gItems.Columns.Add( statusField );
                statusField.DataTextField = "Status";
                statusField.HeaderText = "Status";
                statusField.SortExpression = "Status";
            }
            else
            {
                this.Visible = false;
            }
        }

        private void gItems_RowCommand( object sender, GridViewCommandEventArgs e )
        {
            RockContext rockContext = new RockContext( );

            // take the row index, and get its bound key value.
            int rowIndex = Convert.ToInt32( e.CommandArgument );
            int rowKeyValueId = (int)gItems.DataKeys[rowIndex].Value;

            // get the content channel we want to request a promo for.
            ContentChannel contentChannel = ContentChannels.Where( cc => cc.Id == rowKeyValueId ).SingleOrDefault( );
            
            // in the promo request table, create the request.
            PromotionsService<PromotionRequest> promoService = new PromotionsService<PromotionRequest>( rockContext );
            PromotionRequest promoRequest = promoService.Queryable( ).Where( pr => pr.EventItemOccurrenceId == EventItemOccurrenceId && 
                                                                                   pr.ContentChannelId == contentChannel.Id ).SingleOrDefault( );

            var eventItemOccurrence = new EventItemOccurrenceService( new RockContext( ) ).Get( EventItemOccurrenceId );

            // Based on the promo's state, our click action changes.

            // if it doesn't have a future occurrence, don't allow promoting it.
            if( eventItemOccurrence.NextStartDateTime >= RockDateTime.Now )
            {
                // If the request doesn't exist, then create the request, because they clicked "Request'
                if ( promoRequest == null )
                {
                    // create the promo request, linking the content channel type and the event occurrence
                    promoRequest = new PromotionRequest();
                    promoRequest.EventItemOccurrenceId = EventItemOccurrenceId;
                    promoRequest.EventLastModifiedTime = eventItemOccurrence.ModifiedDateTime;
                    promoRequest.ContentChannelId = contentChannel.Id;
                    promoRequest.IsActive = true;
                    
                    promoService.Add( promoRequest );
                    rockContext.SaveChanges();

                    BindGrids();
                }
                // if it exists and isn't active, activate it.
                else if ( promoRequest.IsActive == false )
                {
                    promoRequest.IsActive = true;
                    rockContext.SaveChanges();

                    BindGrids();
                }
                // it exists and IS active, so deactivate it.
                else
                {
                    promoRequest.IsActive = false;
                    rockContext.SaveChanges();

                    BindGrids();
                }
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrids()
        {
            if ( ContentChannels.Any() )
            {
                RockContext rockContext = new RockContext( );

                // ok, first get all the promo requests for this event occurrence
                var promoRequests = new PromotionsService<PromotionRequest>( rockContext )
                    .Queryable().Where( pr => pr.EventItemOccurrenceId == EventItemOccurrenceId );
                
                // get the UI grid for the content channel types
                var pwItems = phContentChannelGrids.FindControl( "contentChannels_section" ) as HtmlGenericControl;
                if ( pwItems != null )
                {
                    var gItems = pwItems.FindControl( "gItems_contentChannels" ) as Grid;
                    if ( gItems != null )
                    {
                        gItems.DataSource = ContentChannels.Select( i => new
                        {
                            Id = i.Id,
                            Guid = i.Guid,
                            Type = string.Format( "<span class='{0}'></span> {1}", i.IconCssClass, i.Name ),
                            Status = DisplayStatus( i, promoRequests )
                        } ).ToList( );
                        gItems.DataBind();
                    }
                }
            }
        }

        private string DisplayStatus ( ContentChannel contentChannel, IQueryable<PromotionRequest> promoRequestQuery )
        {
            // first, see if this item occurrence has any future events. If not, there's nothing to promote.
            var eventItemOccurrence = new EventItemOccurrenceService( new RockContext( ) ).Get( EventItemOccurrenceId );

            if ( eventItemOccurrence.NextStartDateTime >= RockDateTime.Now )
            {
                // now, see what the state of the content channel is within the promoRequest.
                PromotionRequest promoReq = promoRequestQuery.Where( pr => pr.ContentChannelId == contentChannel.Id ).SingleOrDefault();
                if ( promoReq == null || promoReq.IsActive == false )
                {
                    return "Click to Promote";
                }
                else
                {
                    return "Promoting (Click to Stop)";
                }
            }
            else
            {
                return "The Event is Over. There's Nothing to Promote.";
            }
        }

        private void NavigateToDetailPage( int contentItemId, int? contentChannelId = null )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "EventCalendarId", PageParameter( "EventCalendarId" ) );
            qryParams.Add( "EventItemId", PageParameter( "EventItemId" ) );
            qryParams.Add( "EventItemOccurrenceId", PageParameter( "EventItemOccurrenceId" ) );
            qryParams.Add( "ContentItemId", contentItemId.ToString() );
            if ( contentChannelId.HasValue )
            {
                qryParams.Add( "ContentChannelId", contentChannelId.Value.ToString() );
            }
            
            NavigateToLinkedPage( "DetailPage", qryParams );
        }

        #endregion
    }
}