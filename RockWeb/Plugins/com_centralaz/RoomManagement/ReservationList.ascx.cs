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
                case FilterSetting.MINISTRY:
                    {
                        int ministryId = 0;
                        var reservationMinistryService = new ReservationMinistryService( new RockContext() );
                        if ( int.TryParse( e.Value, out ministryId ) )
                        {
                            var ministry = reservationMinistryService.Get( ministryId );
                            if ( ministry != null )
                            {
                                e.Value = ministry.Name;
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
                case FilterSetting.APPROVAL_STATE:
                    {
                        var approvalValues = e.Value.Split( ',' ).Select( a => a.ConvertToEnum<ReservationApprovalState>() );
                        if ( approvalValues.Any() )
                        {

                            e.Value = approvalValues.Select( a => a.ConvertToString() ).ToList().AsDelimited( "," );

                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }

                case FilterSetting.START_TIME:
                case FilterSetting.END_TIME:
                    {
                        break;
                    }
                case FilterSetting.CREATED_BY:
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
                case FilterSetting.RESOURCES:
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
                case FilterSetting.LOCATIONS:
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

                        break;
                    }
            }
        }

        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SaveUserPreference( FilterSetting.RESERVATION_NAME, tbName.Text );
            gfSettings.SaveUserPreference( FilterSetting.MINISTRY, ddlMinistry.SelectedValue );
            gfSettings.SaveUserPreference( FilterSetting.APPROVAL_STATE, cblApproval.SelectedValues.AsDelimited( "," ) );

            int personId = ppCreator.PersonId ?? 0;
            gfSettings.SaveUserPreference( FilterSetting.CREATED_BY,  personId.ToString() );

            gfSettings.SaveUserPreference( FilterSetting.START_TIME, dtpStartDateTime.SelectedDateTime.ToString() );
            gfSettings.SaveUserPreference( FilterSetting.END_TIME, dtpEndDateTime.SelectedDateTime.ToString() );
            gfSettings.SaveUserPreference( FilterSetting.RESOURCES, rpResource.SelectedValues.AsIntegerList().AsDelimited(","));
            gfSettings.SaveUserPreference( FilterSetting.LOCATIONS, lipLocation.SelectedValues.AsIntegerList().AsDelimited( "," ) );
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

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( FilterSetting.RESERVATION_NAME ) ) )
            {
                tbName.Text = gfSettings.GetUserPreference( FilterSetting.RESERVATION_NAME );
            }

            var ministries = new ReservationMinistryService( rockContext ).Queryable().ToList();
            ddlMinistry.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
            foreach ( var ministry in ministries )
            {
                ddlMinistry.Items.Add( new ListItem( ministry.Name, ministry.Id.ToString() ) );
            }
            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( FilterSetting.MINISTRY ) ) )
            {
                ddlMinistry.SetValue( gfSettings.GetUserPreference( FilterSetting.MINISTRY ) );
            }

            cblApproval.BindToEnum<ReservationApprovalState>( );
            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( FilterSetting.APPROVAL_STATE ) ) )
            {
                cblApproval.SetValues( gfSettings.GetUserPreference( FilterSetting.APPROVAL_STATE ).SplitDelimitedValues() );
            }

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( FilterSetting.CREATED_BY ) ) )
            {
                int? personId = gfSettings.GetUserPreference( FilterSetting.CREATED_BY ).AsIntegerOrNull();
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

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( FilterSetting.START_TIME ) ) )
            {
                dtpStartDateTime.SelectedDateTime = gfSettings.GetUserPreference(  FilterSetting.START_TIME ).AsDateTime();
            }

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( FilterSetting.END_TIME ) ) )
            {
                dtpEndDateTime.SelectedDateTime = gfSettings.GetUserPreference( FilterSetting.END_TIME ).AsDateTime();
            }

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( FilterSetting.RESOURCES ) ) )
            {
                rpResource.SetValues( gfSettings.GetUserPreference( FilterSetting.RESOURCES ).Split( ',' ).AsIntegerList() );
            }

            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( FilterSetting.LOCATIONS ) ) )
            {
                lipLocation.SetValues( gfSettings.GetUserPreference( FilterSetting.LOCATIONS ).Split( ',' ).AsIntegerList() );
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

            // Filter by Approval
            List<ReservationApprovalState> approvalValues = cblApproval.Items.OfType<ListItem>().Where( l => l.Selected ).Select( a => a.Value.ConvertToEnum<ReservationApprovalState>() ).Where( a => a != null ).ToList();
            if ( approvalValues.Any() )
            {
                qry = qry
                    .Where( r =>
                        approvalValues.Contains( r.ApprovalState ) );
            }

            // Filter by Creator
            if ( ppCreator.PersonId.HasValue )
            {
                qry = qry
                    .Where( r =>
                        r.CreatedByPersonAlias != null &&
                        r.CreatedByPersonAlias.PersonId != null &&
                        r.CreatedByPersonAlias.PersonId == ppCreator.PersonId.Value );
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
            var reservationSummaryList = reservationService.GetReservationSummaries( qry, filterStartDateTime, filterEndDateTime, false );

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
                ReservationDateTimeDescription = r.ReservationDateTimeDescription + 
                    string.Format( " ({0})", ( r.ReservationStartDateTime.Date == r.ReservationEndDateTime.Date ) 
                        ? r.ReservationStartDateTime.DayOfWeek.ToStringSafe().Substring( 0, 3 )
                        : r.ReservationStartDateTime.DayOfWeek.ToStringSafe().Substring( 0, 3 ) + "-" + r.ReservationEndDateTime.DayOfWeek.ToStringSafe().Substring( 0, 3 ) ),
                ApprovalState = r.ApprovalState.ConvertToString()
            } )
            .OrderBy( r => r.ReservationStartDateTime ).ToList();
            gReservations.EntityTypeId = EntityTypeCache.Read<Reservation>().Id;
            gReservations.DataBind();
        }

        #endregion


        #region Filter's User Preference Setting Keys
        /// <summary>
        /// Constant like string-key-settings that are tied to user saved filter preferences.
        /// </summary>
        public static class FilterSetting
        {
            public const string RESERVATION_NAME = "Reservation Name";
            public const string MINISTRY = "Ministry";
            public const string CREATED_BY = "Created By";
            public const string LOCATIONS = "Locations";
            public const string RESOURCES = "Resources";
            public const string START_TIME = "Start Time";
            public const string END_TIME = "End Time";
            public const string APPROVAL_STATE = "Approval State";
        }
        #endregion
    }
}