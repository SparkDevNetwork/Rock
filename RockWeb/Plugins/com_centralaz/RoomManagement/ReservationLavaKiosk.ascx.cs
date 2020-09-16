// <copyright>
// Copyright by the Central Christian Church
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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Store;
using System.Text;
using Rock.Security;

using com.centralaz.RoomManagement.Model;
using com.centralaz.RoomManagement.Web.Cache;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace RockWeb.Plugins.com_centralaz.RoomManagement
{
    [DisplayName( "Reservation Lava Kiosk" )]
    [Category( "com_centralaz > Room Management" )]
    [Description( "Renders the approved reservations in Lava for the given locationId." )]

    [CodeEditorField( "Lava Template", "Lava template to use to display the list of upcoming events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~/Plugins/com_centralaz/RoomManagement/Assets/Lava/ReservationKiosk.lava' %}", "", 1 )]
    [BooleanField( "Enable Debug", "Display a list of merge fields available for lava.", false, "", 99 )]

    public partial class ReservationLavaKiosk : Rock.Web.UI.RockBlock
    {
        #region Fields

        #endregion

        #region Properties

        private string LocationName { get; set; }

        #endregion

        #region Base ControlMethods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            LocationName = ViewState["LocationName"] as string;
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
            base.OnLoad( e );

            nbMessage.Visible = false;
            var locationId = PageParameter( "LocationId" ).AsInteger();

            if ( locationId == 0 )
            {
                nbMessage.Visible = true;
                nbMessage.Text = "Please pass in a valid LocationId";
                return;
            }

            if ( ! Page.IsPostBack )
            {
                var location = new LocationService( new RockContext() ).Get( locationId );
                if ( location != null )
                {
                    LocationName = location.Name;
                }
                BindData( locationId );
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
            ViewState["LocationName"] = LocationName;
            return base.SaveViewState();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindData( PageParameter("LocationId").AsInteger() );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the data.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        private void BindData( int locationId )
        {
            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "CurrentPerson", CurrentPerson );
            mergeFields.Add( "Location", LocationName );

            List<ReservationService.ReservationSummary> reservationSummaryList = GetReservationSummaries( locationId );

            // Bind to Grid
            var reservationSummaries = reservationSummaryList.Select( r => new
            {
                Id = r.Id,
                ReservationType = r.ReservationType,
                ReservationName = r.ReservationName,
                ApprovalState = r.ApprovalState.ConvertToString(),
                Locations = r.ReservationLocations.ToList(),
                Resources = r.ReservationResources.ToList(),
                CalendarDate = r.EventStartDateTime.ToLongDateString(),
                EventStartDateTime = r.EventStartDateTime,
                EventEndDateTime = r.EventEndDateTime,
                ReservationStartDateTime = r.ReservationStartDateTime,
                ReservationEndDateTime = r.ReservationEndDateTime,
                EventDateTimeDescription = r.EventTimeDescription.Replace( "a", " AM" ).Replace( "p", " PM" ),
                ReservationDateTimeDescription = r.ReservationTimeDescription,
                SetupPhotoId = r.SetupPhotoId,
                Note = r.Note,
                RequesterAlias = r.RequesterAlias,
                EventContactPersonAlias = r.EventContactPersonAlias,
                EventContactEmail = r.EventContactEmail,
                EventContactPhoneNumber = r.EventContactPhoneNumber,
                MinistryName = r.ReservationMinistry != null ? r.ReservationMinistry.Name : string.Empty,
            } )
            .OrderBy( r => r.EventStartDateTime )
            .GroupBy( r => r.EventStartDateTime.Date )
            .Select( r => r.ToList() )
            .ToList();

            mergeFields.Add( "ReservationSummaries", reservationSummaries );


            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );

            // show debug info
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                lDebug.Visible = true;
                lDebug.Text = mergeFields.lavaDebugInfo();
            }
            else
            {
                lDebug.Visible = false;
                lDebug.Text = string.Empty;
            }
        }

        /// <summary>
        /// Gets the reservation summaries.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <returns></returns>
        private List<ReservationService.ReservationSummary> GetReservationSummaries( int locationId )
        {
            var rockContext = new RockContext();
            var reservationService = new ReservationService( rockContext );
            var qry = reservationService.Queryable();

            // Filter by Location
                qry = qry
                    .Where( r =>
                        r.ReservationLocations.Any( rl => rl.LocationId == locationId ) );

            // Filter by Approval
            List<ReservationApprovalState> approvalValues = new List<ReservationApprovalState>();
            approvalValues.Add( ReservationApprovalState.Approved );
            if ( approvalValues.Any() )
            {
                qry = qry
                    .Where( r =>
                        approvalValues.Contains( r.ApprovalState ) );
            }

            // Filter by Time
            var today = RockDateTime.Today;
            var filterStartDateTime = today;
            var filterEndDateTime = today;

            var reservationSummaryList = reservationService.GetReservationSummaries( qry, filterStartDateTime, filterEndDateTime, true );
            return reservationSummaryList;
        }

        /// <summary>
        /// Shows the warning.
        /// </summary>
        /// <param name="heading">The heading.</param>
        /// <param name="message">The message.</param>
        private void ShowWarning( string heading, string message )
        {
            nbMessage.Heading = heading;
            nbMessage.Text = string.Format( "<p>{0}</p>", message );
            nbMessage.NotificationBoxType = NotificationBoxType.Danger;
            nbMessage.Visible = true;
        }

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="heading">The heading.</param>
        /// <param name="message">The message.</param>
        private void ShowError( string heading, string message )
        {
            nbMessage.Heading = heading;
            nbMessage.Text = string.Format( "<p>{0}</p>", message );
            nbMessage.NotificationBoxType = NotificationBoxType.Danger;
            nbMessage.Visible = true;
        }

        #endregion

        #region Events
        /// <summary>
        /// Handles the Click event of the btnSubmit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSubmit_Click( object sender, EventArgs e )
        {
            BindData( PageParameter( "LocationId" ).AsInteger() );
        }

        #endregion

    }
}