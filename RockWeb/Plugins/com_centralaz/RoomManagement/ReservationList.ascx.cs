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
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

using com.centralaz.RoomManagement.Model;
using System.Web.UI.WebControls;

namespace RockWeb.Plugins.com_centralaz.RoomManagement
{
    /// <summary>
    /// Block for viewing list of reservations
    /// </summary>
    [DisplayName( "Reservation List" )]
    [Category( "com_centralaz > Room Management" )]
    [Description( "Block for viewing a list of reservations." )]

    [LinkedPage( "Detail Page" )]
    public partial class ReservationList : Rock.Web.UI.RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;
            gfSettings.DisplayFilterValue += gfSettings_DisplayFilterValue;

            gReservations.DataKeyNames = new string[] { "Id" };
            gReservations.Actions.ShowAdd = false;
            gReservations.GridRebind += gReservations_GridRebind;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Edit event of the gReservations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gReservations_Edit( object sender, RowEventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "ReservationId", e.RowKeyValue.ToString() );
            NavigateToLinkedPage( "DetailPage", parms );
        }


        /// <summary>
        /// Handles the GridRebind event of the gReservations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gReservations_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        protected void gfSettings_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Ministry":
                case "Status":
                    {
                        int definedValueId = 0;
                        if ( int.TryParse( e.Value, out definedValueId ) )
                        {
                            var definedValue = DefinedValueCache.Read( definedValueId );
                            if ( definedValue != null )
                            {
                                e.Value = definedValue.Value;
                            }
                        }
                        break;
                    }

                case "Start Time":
                case "End Time":
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }
                case "Created By":
                    {
                        string personName = string.Empty;

                        int? personId = e.Value.AsIntegerOrNull();
                        if ( personId.HasValue )
                        {
                            var personService = new PersonService( new RockContext() );
                            var person = personService.Get( personId.Value );
                            if ( person != null )
                            {
                                personName = person.FullName;
                            }
                        }

                        e.Value = personName;

                        break;
                    }
                case "Resources":
                    {
                        var resourceIdList = e.Value.Split( ',' ).AsIntegerList();
                        if ( resourceIdList.Any() && rpResource.Visible )
                        {
                            var service = new ResourceService( new RockContext() );
                            var resources = service.GetByIds( resourceIdList );
                            if ( resources != null && resources.Any() )
                            {
                                e.Value = resources.Select( a => a.Name ).ToList().AsDelimited( "," );
                            }
                            else
                            {
                                e.Value = string.Empty;
                            }
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }
                case "Locations":
                    {
                        var locationIdList = e.Value.Split( ',' ).AsIntegerList();
                        if ( locationIdList.Any() && lipLocation.Visible )
                        {
                            var service = new FinancialAccountService( new RockContext() );
                            var locations = service.GetByIds( locationIdList );
                            if ( locations != null && locations.Any() )
                            {
                                e.Value = locations.Select( a => a.Name ).ToList().AsDelimited( "," );
                            }
                            else
                            {
                                e.Value = string.Empty;
                            }
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }
            }
        }

        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SaveUserPreference( "Reservation Name", tbName.Text );
            gfSettings.SaveUserPreference( "Ministry", ddlMinistry.SelectedValue );
            gfSettings.SaveUserPreference( "Status", ddlStatus.SelectedValue );

            int personId = ppCreator.PersonId ?? 0;
            gfSettings.SaveUserPreference( "Created By", UserCanAdministrate ? personId.ToString() : "" );

            gfSettings.SaveUserPreference( "Start Time", dtpStartDateTime.ToString() );
            gfSettings.SaveUserPreference( "End Time", dtpEndDateTime.ToString() );
            gfSettings.SaveUserPreference( "Resources", rpResource.SelectedValues.ToString() );
            gfSettings.SaveUserPreference( "Locations", lipLocation.SelectedValues.ToString() );
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            var rockContext = new RockContext();

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( "Reservation Name" ) ) )
            {
                tbName.Text = gfSettings.GetUserPreference( "Reservation Name" );
            }

            var ministries = new ReservationMinistryService( rockContext ).Queryable().ToList();
            ddlMinistry.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
            foreach ( var ministry in ministries )
            {
                ddlMinistry.Items.Add( new ListItem( ministry.Name, ministry.Id.ToString() ) );
            }
            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( "Ministry" ) ) )
            {
                ddlMinistry.SetValue( gfSettings.GetUserPreference( "Ministry" ) );
            }

            var statuses = new ReservationStatusService( rockContext ).Queryable().ToList();
            ddlStatus.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
            foreach ( var status in statuses )
            {
                ddlStatus.Items.Add( new ListItem( status.Name, status.Id.ToString() ) );
            }
            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( "Status" ) ) )
            {
                ddlStatus.SetValue( gfSettings.GetUserPreference( "Status" ) );
            }

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( "Created By" ) ) )
            {
                int? personId = gfSettings.GetUserPreference( "Created By" ).AsIntegerOrNull();
                if ( personId.HasValue && personId.Value != 0 )
                {
                    var personService = new PersonService( new RockContext() );
                    var person = personService.Get( personId.Value );
                    if ( person != null )
                    {
                        ppCreator.SetValue( person );
                    }
                }
            }

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( "Start Time" ) ) )
            {
                dtpStartDateTime.SelectedDateTime = gfSettings.GetUserPreference( "Start Time" ).AsDateTime();
            }

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( "End Time" ) ) )
            {
                dtpEndDateTime.SelectedDateTime = gfSettings.GetUserPreference( "End Time" ).AsDateTime();
            }

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( "Resource" ) ) )
            {
                rpResource.SetValues( gfSettings.GetUserPreference( "Resource" ).Split( ',' ).AsIntegerList() );
            }

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( "Location" ) ) )
            {
                lipLocation.SetValues( gfSettings.GetUserPreference( "Location" ).Split( ',' ).AsIntegerList() );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var reservationService = new ReservationService( rockContext );
            var qry = reservationService.Queryable();

            // Filter by Name
            if ( !String.IsNullOrWhiteSpace( tbName.Text ) )
            {
                qry = qry.Where( r => r.Name.Contains( tbName.Text ) );
            }

            // Filter by Ministry
            var ministryValueId = ddlMinistry.SelectedValueAsInt();
            if ( ministryValueId.HasValue )
            {
                qry = qry.Where( r => r.ReservationMinistryId == ministryValueId );
            }

            // Filter by Status
            var statusValueId = ddlStatus.SelectedValueAsInt();
            if ( statusValueId.HasValue )
            {
                qry = qry.Where( r => r.ReservationStatusId == statusValueId );
            }

            // Filter by Creator
            if ( ppCreator.PersonId.HasValue )
            {
                qry = qry
                    .Where( r =>
                        r.CreatedByPersonId != null &&
                        r.CreatedByPersonId == ppCreator.PersonId.Value );
            }

            // Filter by Resources
            var resourceIdList = rpResource.SelectedValuesAsInt().ToList();
            if ( resourceIdList.Where( r => r != 0 ).Any() )
            {
                qry = qry.Where( r => r.ReservationResources.Any( rr => resourceIdList.Contains( rr.ResourceId ) ) );
            }

            // Filter by Locations
            var locationIdList = lipLocation.SelectedValuesAsInt().ToList();
            if ( locationIdList.Where( r => r != 0 ).Any() )
            {
                qry = qry.Where( r => r.ReservationLocations.Any( rr => locationIdList.Contains( rr.LocationId ) ) );
            }

            // Filter by Time
            var today = RockDateTime.Today;
            var filterStartDateTime = dtpStartDateTime.SelectedDateTime ?? today;
            var filterEndDateTime = dtpEndDateTime.SelectedDateTime ?? today.AddMonths( 1 );
            var reservationSummaryList = reservationService.GetReservationSummaries( qry, filterStartDateTime, filterEndDateTime );

            // Bind to Grid
            gReservations.DataSource = reservationSummaryList.Select( r => new
            {
                Id = r.Id,
                ReservationName = r.ReservationName,
                Locations = r.ReservationLocations.Select( rl => rl.Location.Name ).ToList().AsDelimited( ", " ),
                Resources = r.ReservationResources.Select( rr => rr.Resource.Name ).ToList().AsDelimited( ", " ),
                EventStartDateTime = r.EventStartDateTime,
                EventEndDateTime = r.EventEndDateTime,
                ReservationStartDateTime = r.ReservationStartDateTime,
                ReservationEndDateTime = r.ReservationEndDateTime,
                EventDateTimeDescription = r.EventDateTimeDescription,
                ReservationDateTimeDescription = r.ReservationDateTimeDescription
            } )
            .OrderBy( r => r.ReservationStartDateTime ).ToList();
            gReservations.EntityTypeId = EntityTypeCache.Read<Reservation>().Id;
            gReservations.DataBind();
        }

        #endregion
    }
}