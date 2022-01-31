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
using System.Linq;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Follow;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Follow
{
    /// <summary>
    /// Block for editing following event types.
    /// </summary>
    [DisplayName( "Event Detail" )]
    [Category( "Follow" )]
    [Description( "Block for editing following event types." )]
    public partial class EventDetail : RockBlock
    {

        #region Properties

        public int EventId { get; set; }
        public int? EventEntityTypeId { get; set; }

        #endregion

        #region Control Methods

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            EventId = ViewState["EventId"] as int? ?? 0;
            EventEntityTypeId = ViewState["EventEntityTypeId"] as int?;

            var followingEvent = new FollowingEventType { Id = EventId, EntityTypeId = EventEntityTypeId };
            BuildDynamicControls( followingEvent, false );
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
                ShowDetail( PageParameter( "EventId" ).AsInteger() );
            }
        }

        protected override object SaveViewState()
        {
            ViewState["EventId"] = EventId;
            ViewState["EventEntityTypeId"] = EventEntityTypeId;
            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                FollowingEventType followingEvent = null;

                var eventService = new Rock.Model.FollowingEventTypeService( rockContext );

                if ( EventId != 0 )
                {
                    followingEvent = eventService.Get( EventId );
                }

                if ( followingEvent == null )
                {
                    followingEvent = new Rock.Model.FollowingEventType();
                    eventService.Add( followingEvent );
                }

                followingEvent.Name = tbName.Text;
                followingEvent.IsActive = cbIsActive.Checked;
                followingEvent.Description = tbDescription.Text;
                followingEvent.EntityTypeId = cpEventType.SelectedEntityTypeId;
                followingEvent.SendOnWeekends = !cbSendOnFriday.Checked;

                var eventEntityTypeGuid = EntityTypeCache.Get( cpEventType.SelectedEntityTypeId.Value ).Guid.ToString();
                if ( string.Equals( eventEntityTypeGuid, Rock.SystemGuid.EntityType.PERSON_PRAYER_REQUEST, StringComparison.OrdinalIgnoreCase ) )
                {
                    followingEvent.IncludeNonPublicRequests = cbIncludeNonPublicRequests.Checked;
                }

                followingEvent.IsNoticeRequired = cbRequireNotification.Checked;
                followingEvent.EntityNotificationFormatLava = ceNotificationFormat.Text;

                followingEvent.FollowedEntityTypeId = null;
                var eventComponent = followingEvent.GetEventComponent();
                if ( eventComponent != null )
                {
                    var followedEntityType = EntityTypeCache.Get( eventComponent.FollowedType );
                    if ( followedEntityType != null )
                    {
                        followingEvent.FollowedEntityTypeId = followedEntityType.Id;
                    }
                }

                rockContext.SaveChanges();

                followingEvent.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phAttributes, followingEvent );
                followingEvent.SaveAttributeValues( rockContext );
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpEventType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpEventType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var followingEvent = new FollowingEventType { Id = EventId, EntityTypeId = cpEventType.SelectedEntityTypeId };

            if ( cpEventType.SelectedEntityTypeId.HasValue )
            {
                var eventEntityTypeGuid = EntityTypeCache.Get( cpEventType.SelectedEntityTypeId.Value ).Guid.ToString();
                if ( string.Equals( eventEntityTypeGuid, Rock.SystemGuid.EntityType.PERSON_PRAYER_REQUEST, StringComparison.OrdinalIgnoreCase ) )
                {
                    cbIncludeNonPublicRequests.Visible = true;
                    cbIncludeNonPublicRequests.Checked = followingEvent.IncludeNonPublicRequests;
                }
                else
                {
                    cbIncludeNonPublicRequests.Visible = false;
                    cbIncludeNonPublicRequests.Checked = false;
                }
            }

            BuildDynamicControls( followingEvent, true );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        public void ShowDetail( int eventId )
        {
            FollowingEventType followingEvent = null;

            bool editAllowed = IsUserAuthorized( Authorization.EDIT );

            if ( !eventId.Equals( 0 ) )
            {
                followingEvent = new FollowingEventTypeService( new RockContext() ).Get( eventId );
                editAllowed = editAllowed || followingEvent.IsAuthorized( Authorization.EDIT, CurrentPerson );
                pdAuditDetails.SetEntity( followingEvent, ResolveRockUrl( "~" ) );
            }

            if ( followingEvent == null )
            {
                followingEvent = new FollowingEventType { Id = 0, IsActive = true };
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            EventId = followingEvent.Id;

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed || !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( FollowingEventType.FriendlyTypeName );
            }

            if ( readOnly )
            {
                ShowReadonlyDetails( followingEvent );
            }
            else
            {
                ShowEditDetails( followingEvent );
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="event">The event.</param>
        private void ShowEditDetails( FollowingEventType followingEvent )
        {
            if ( followingEvent.Id == 0 )
            {
                lActionTitle.Text = ActionTitle.Add( FollowingEventType.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = followingEvent.Name.FormatAsHtmlTitle();
            }

            hlInactive.Visible = !followingEvent.IsActive;

            SetEditMode( true );

            tbName.Text = followingEvent.Name;
            cbIsActive.Checked = followingEvent.IsActive;
            tbDescription.Text = followingEvent.Description;
            cpEventType.SetValue( followingEvent.EntityType != null ? followingEvent.EntityType.Guid.ToString().ToUpper() : string.Empty );
            cbSendOnFriday.Checked = !followingEvent.SendOnWeekends;
            cbIncludeNonPublicRequests.Visible = false;
            cbIncludeNonPublicRequests.Checked = false;

            if ( followingEvent.EntityType != null )
            {
                var eventEntityTypeGuid = EntityTypeCache.Get( followingEvent.EntityType.Id )?.Guid.ToString();
                if ( string.Equals( eventEntityTypeGuid, Rock.SystemGuid.EntityType.PERSON_PRAYER_REQUEST, StringComparison.OrdinalIgnoreCase ) )
                {
                    cbIncludeNonPublicRequests.Visible = true;
                    cbIncludeNonPublicRequests.Checked = followingEvent.IncludeNonPublicRequests;
                }
            }

            cbRequireNotification.Checked = followingEvent.IsNoticeRequired;
            ceNotificationFormat.Text = followingEvent.EntityNotificationFormatLava;

            BuildDynamicControls( followingEvent, true );
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="event">The event.</param>
        private void ShowReadonlyDetails( FollowingEventType followingEvent )
        {
            SetEditMode( false );

            lActionTitle.Text = followingEvent.Name.FormatAsHtmlTitle();
            hlInactive.Visible = !followingEvent.IsActive;
            lEventDescription.Text = followingEvent.Description;

            DescriptionList descriptionList = new DescriptionList();

            if ( followingEvent.EntityType != null )
            {
                descriptionList.Add( "Event Type", followingEvent.EntityType.Name );
            }

            descriptionList.Add( "Require Notification", followingEvent.IsNoticeRequired ? "Yes" : "No" );
            descriptionList.Add( "Send Weekend Notices on Friday", followingEvent.SendOnWeekends ? "No" : "Yes" );

            var eventEntityTypeGuid = EntityTypeCache.Get( followingEvent.EntityType.Id ).Guid.ToString();
            if ( followingEvent.EntityType != null && string.Equals( eventEntityTypeGuid, Rock.SystemGuid.EntityType.PERSON_PRAYER_REQUEST, StringComparison.OrdinalIgnoreCase ) )
            {
                descriptionList.Add( "Include Non-Public Requests", followingEvent.IncludeNonPublicRequests ? "Yes" : "No" );
            }

            lblMainDetails.Text = descriptionList.Html;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        private void BuildDynamicControls( FollowingEventType followingEvent, bool SetValues )
        {
            if ( SetValues )
            {
                EventEntityTypeId = followingEvent.EntityTypeId;
                if ( followingEvent.EntityTypeId.HasValue )
                {
                    var EventComponentEntityType = EntityTypeCache.Get( followingEvent.EntityTypeId.Value );
                    var EventEntityType = EntityTypeCache.Get( "Rock.Model.FollowingEventType" );
                    if ( EventComponentEntityType != null && EventEntityType != null )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            Rock.Attribute.Helper.UpdateAttributes( EventComponentEntityType.GetEntityType(), EventEntityType.Id, "EntityTypeId", EventComponentEntityType.Id.ToString(), rockContext );
                        }
                    }
                }
            }

            if ( followingEvent.Attributes == null )
            {
                followingEvent.LoadAttributes();
            }

            phAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( followingEvent, phAttributes, SetValues, BlockValidationGroup, new List<string> { "Active", "Order" } );
        }

        #endregion
    }
}