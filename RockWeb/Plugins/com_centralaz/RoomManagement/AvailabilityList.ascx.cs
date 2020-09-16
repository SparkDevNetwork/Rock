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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using com.centralaz.RoomManagement.Model;

namespace RockWeb.Plugins.com_centralaz.RoomManagement
{
    /// <summary>
    /// Block for viewing resource availability
    /// </summary>
    [DisplayName( "Availability List" )]
    [Category( "com_centralaz > Room Management" )]
    [Description( "Block for viewing the availability of resources." )]

    [LinkedPage( "Detail Page" )]
    public partial class AvailabilityList : Rock.Web.UI.RockBlock
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

            gLocations.DataKeyNames = new string[] { "Id" };
            gLocations.Actions.ShowAdd = false;
            gLocations.GridRebind += gReservations_GridRebind;

            gResources.DataKeyNames = new string[] { "Id" };
            gResources.Actions.ShowAdd = false;
            gResources.GridRebind += gReservations_GridRebind;
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
        /// Handles the RowSelected event of the gResources control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gResources_RowSelected( object sender, RowEventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "ResourceId", e.RowKeyValue.ToString() );
            NavigateToLinkedPage( "DetailPage", parms );
        }

        /// <summary>
        /// Handles the RowSelected event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gLocations_RowSelected( object sender, RowEventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "LocationId", e.RowKeyValue.ToString() );
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

        /// <summary>
        /// Gfs the settings_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfSettings_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Selected Entity":
                    {
                        e.Value = string.Empty;
                        break;
                    }
                case "Resource Category":
                    {
                        var resourceCategoryIdList = e.Value.Split( ',' ).AsIntegerList();
                        if ( resourceCategoryIdList.Any() && cpResource.Visible )
                        {
                            var service = new CategoryService( new RockContext() );
                            var categories = service.GetByIds( resourceCategoryIdList );
                            if ( categories != null && categories.Any() )
                            {
                                e.Value = categories.Select( a => a.Name ).ToList().AsDelimited( "," );
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
                case "Parent Location":
                    {
                        if ( gfSettings.GetUserPreference( "Selected Entity" ) == "Location" )
                        {
                            var locationIdList = e.Value.Split( ',' ).AsIntegerList();
                            if ( locationIdList.Any() && lipLocation.Visible )
                            {
                                var service = new LocationService( new RockContext() );
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
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SaveUserPreference( "Selected Entity", rblResourceLocation.SelectedValue );
            gfSettings.SaveUserPreference( "Start Time", dtpStartDateTime.SelectedDateTime.ToString() );
            gfSettings.SaveUserPreference( "End Time", dtpEndDateTime.SelectedDateTime.ToString() );
            gfSettings.SaveUserPreference( "Resource Category", cpResource.SelectedValue.ToString() );
            gfSettings.SaveUserPreference( "Parent Location", lipLocation.SelectedValue.ToString() );
            gfSettings.SaveUserPreference( "Expected Occupants", nbMaxOccupants.Text );
            BindGrid();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblResourceLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblResourceLocation_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( rblResourceLocation.SelectedValue == "Resource" )
            {
                cpResource.Visible = true;
                lipLocation.Visible = false;
            }
            else
            {
                cpResource.Visible = false;
                lipLocation.Visible = true;
            }

            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( "Start Time" ) ) )
            {
                dtpStartDateTime.SelectedDateTime = gfSettings.GetUserPreference( "Start Time" ).AsDateTime();
            }

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( "End Time" ) ) )
            {
                dtpEndDateTime.SelectedDateTime = gfSettings.GetUserPreference( "End Time" ).AsDateTime();
            }

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( "Resource Category" ) ) )
            {
                cpResource.SetValues( gfSettings.GetUserPreference( "Resource Category" ).Split( ',' ).AsIntegerList() );
            }

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( "Parent Location" ) ) )
            {
                lipLocation.SetValues( gfSettings.GetUserPreference( "Parent Location" ).Split( ',' ).AsIntegerList() );
            }

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( "Selected Entity" ) ) )
            {
                rblResourceLocation.SetValue( gfSettings.GetUserPreference( "Selected Entity" ) );
            }

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( "Expected Occupants" ) ) )
            {
                nbMaxOccupants.Text = gfSettings.GetUserPreference( "Expected Occupants" );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            if ( rblResourceLocation.SelectedValue == "Resource" )
            {
                cpResource.Visible = true;
                gResources.Visible = true;

                gLocations.Visible = false;
                lipLocation.Visible = false;
                nbMaxOccupants.Visible = false;

                BindResourcesGrid();
            }
            else
            {
                cpResource.Visible = false;
                gResources.Visible = false;

                gLocations.Visible = true;
                lipLocation.Visible = true;
                nbMaxOccupants.Visible = true;

                BindLocationsGrid();
            }
        }

        /// <summary>
        /// Binds the locations grid.
        /// </summary>
        private void BindLocationsGrid()
        {
            var rockContext = new RockContext();
            var reservationService = new ReservationService( rockContext );
            var qry = reservationService.Queryable().AsNoTracking().Where( r => r.ApprovalState != ReservationApprovalState.Denied );
            var locationService = new LocationService( rockContext );

            List<int> locationIdList = new List<int>();
            List<Location> locationList = new List<Location>();
            if ( lipLocation.SelectedValueAsInt().HasValue )
            {
                locationIdList = locationService.GetAllDescendents( lipLocation.SelectedValueAsInt().Value ).Select( l => l.Id ).ToList();
                locationIdList.Add( lipLocation.SelectedValueAsInt().Value );
                locationList = locationService.Queryable().AsNoTracking().Where( l => locationIdList.Contains( l.Id ) ).ToList();

                if ( locationIdList.Any() )
                {
                    qry = qry.Where( r => r.ReservationLocations.Any( rr => rr.ApprovalState != ReservationLocationApprovalState.Denied && locationIdList.Contains( rr.LocationId ) ) );
                }
            }
            else
            {
                locationList = locationService.Queryable().AsNoTracking().Where( l => l.Name != null && l.Name != string.Empty ).ToList();
            }

            locationList = locationList.Where( l => l.IsActive ).ToList();

            if ( nbMaxOccupants.Text.AsInteger() > 0 )
            {
                locationList = locationList.Where( l => l.FirmRoomThreshold >= nbMaxOccupants.Text.AsInteger() ).ToList();
            }

            var locationIds = locationList.Select( l => l.Id ).ToList();
            var locationResourceList = new ResourceService( rockContext ).Queryable().Where( r => r.LocationId.HasValue && locationIds.Contains( r.LocationId.Value ) ).ToList();


            // Filter by Time
            var today = RockDateTime.Today;
            var filterStartDateTime = dtpStartDateTime.SelectedDateTime ?? today;
            var filterEndDateTime = dtpEndDateTime.SelectedDateTime ?? today.AddMonths( 1 );
            var reservationSummaryList = reservationService.GetReservationSummaries( qry, filterStartDateTime, filterEndDateTime, false );

            // Bind to Grid
            gLocations.DataSource = locationList.Select( l => new
            {
                Id = l.Id,
                Name = String.Format( "{0}<small>{1}{2}</small>",
                            l.Name,
                            locationResourceList.Where( r => r.LocationId.HasValue && r.LocationId == l.Id ).Any() ? "</br>" : String.Empty,
                            locationResourceList.Where( r => r.LocationId.HasValue && r.LocationId == l.Id ).Select( r => r.Name + " (" + r.Quantity + ")" ).ToList().AsDelimited( "</br>" ) ),
                MaxOccupants = l.FirmRoomThreshold,
                IsAvailable = !reservationSummaryList.Any( r => r.ReservationLocations.Any( rl => rl.ApprovalState != ReservationLocationApprovalState.Denied && rl.LocationId == l.Id ) ),
                Availability = reservationSummaryList.Any( r => r.ReservationLocations.Any( rl => rl.ApprovalState != ReservationLocationApprovalState.Denied && rl.LocationId == l.Id ) ) ? reservationSummaryList.Where( r => r.ReservationLocations.Any( rl => rl.ApprovalState != ReservationLocationApprovalState.Denied && rl.LocationId == l.Id ) ).Select( r => r.ReservationName + "</br>" + r.ReservationDateTimeDescription ).ToList().AsDelimited( "</br></br>" ) : "Available"
            } ).OrderBy( l => l.Name ).ToList();
            gLocations.EntityTypeId = EntityTypeCache.Get<Location>().Id;
            gLocations.DataBind();
        }

        /// <summary>
        /// Binds the resources grid.
        /// </summary>
        private void BindResourcesGrid()
        {
            var rockContext = new RockContext();
            var reservationService = new ReservationService( rockContext );
            var resourceService = new ResourceService( rockContext );
            var qry = reservationService.Queryable().AsNoTracking().Where( r => r.ApprovalState != ReservationApprovalState.Denied );

            List<Resource> resourceList = new List<Resource>();
            if ( cpResource.SelectedValueAsInt().HasValue )
            {
                int categoryId = cpResource.SelectedValueAsInt().Value;
                resourceList = resourceService.Queryable().AsNoTracking().Where( r => r.CategoryId == categoryId ).ToList();
                var resourceIdList = resourceList.Select( r => r.Id ).ToList();

                if ( resourceIdList.Any() )
                {
                    qry = qry.Where( r => r.ReservationResources.Any( rr => rr.ApprovalState != ReservationResourceApprovalState.Denied && resourceIdList.Contains( rr.ResourceId ) ) );
                }
            }
            else
            {
                resourceList = resourceService.Queryable().AsNoTracking().ToList();
            }

            // Filter by Time
            var today = RockDateTime.Today;
            var filterStartDateTime = dtpStartDateTime.SelectedDateTime ?? today;
            var filterEndDateTime = dtpEndDateTime.SelectedDateTime ?? today.AddMonths( 1 );
            var reservationSummaryList = reservationService.GetReservationSummaries( qry, filterStartDateTime, filterEndDateTime, false );

            // Bind to Grid
            gResources.DataSource = resourceList.Select( resource =>
            {
                var reservedResources = reservationSummaryList.Where( reservationSummary =>
                     ( reservationSummary.ReservationStartDateTime > filterStartDateTime || reservationSummary.ReservationEndDateTime > filterStartDateTime ) &&
                     ( reservationSummary.ReservationStartDateTime < filterEndDateTime || reservationSummary.ReservationEndDateTime < filterEndDateTime )
                    ).DistinctBy( reservationSummary => reservationSummary.Id ).Sum( reservationSummary => reservationSummary.ReservationResources.Where( rr => rr.ApprovalState != ReservationResourceApprovalState.Denied && rr.ResourceId == resource.Id ).Sum( rr => rr.Quantity ) );
                return new
                {
                    Id = resource.Id,
                    Name = resource.Name,
                    LocationName = ( resource.Location == null ) ? "" : resource.Location.Name,
                    IsAvailable = resource.Quantity - reservedResources > 0,
                    Availability = resource.Quantity - reservedResources > 0 ? String.Format( "{0} Available", resource.Quantity - reservedResources ) : reservationSummaryList.Where( reservation => reservation.ReservationResources.Any( rr => rr.ApprovalState != ReservationResourceApprovalState.Denied && rr.ResourceId == resource.Id ) ).Select( reservation => reservation.ReservationName + "</br>" + reservation.ReservationDateTimeDescription ).ToList().AsDelimited( "</br></br>" )
                };
            } ).OrderBy( l => l.Name ).ToList();
            gResources.EntityTypeId = EntityTypeCache.Get<Reservation>().Id;
            gResources.DataBind();
        }

        #endregion
    }
}