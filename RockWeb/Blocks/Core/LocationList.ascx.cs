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
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Block for viewing list of tags
    /// </summary>
    [DisplayName( "Location List" )]
    [Category( "Core" )]
    [Description( "Block for viewing a list of locations." )]

    [LinkedPage( "Detail Page" )]
    public partial class LocationList : Rock.Web.UI.RockBlock
    {
        #region Fields


        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;

            rGrid.DataKeyNames = new string[] { "Id" };
            rGrid.Actions.ShowAdd = false;
            rGrid.GridRebind += rGrid_GridRebind;
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
        /// Handles the Edit event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "LocationId", e.RowKeyValue.ToString() );
            NavigateToLinkedPage( "DetailPage", parms );
        }


        /// <summary>
        /// Handles the GridRebind event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }


        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            
            if ( e.Key != "Street Address" && e.Key != "City" && e.Key != "Not Geocoded" )
            {
                e.Value = string.Empty;
            }

            if ( e.Key == "Not Geocoded" )
            {
                if ( e.Value == "False" )
                {
                    e.Value = String.Empty;
                }
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Street Address", txtStreetAddress1.Text );
            rFilter.SaveUserPreference( "City", txtCity.Text );
            rFilter.SaveUserPreference( "Not Geocoded", cbNotGeocoded.Checked.ToString() );
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            if ( ! string.IsNullOrWhiteSpace( rFilter.GetUserPreference( "Street Address" ) ) )
            {
                txtStreetAddress1.Text = rFilter.GetUserPreference( "Street Address" );
            }

            if ( ! string.IsNullOrWhiteSpace( rFilter.GetUserPreference( "City" ) ) )
            {
                txtCity.Text = rFilter.GetUserPreference( "City" );
            }

            if ( ! string.IsNullOrWhiteSpace( rFilter.GetUserPreference( "Not Geocoded" ) ) )
            {
                cbNotGeocoded.Checked = Convert.ToBoolean(rFilter.GetUserPreference( "Not Geocoded" ));
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            //rGrid.Columns[0].Visible = true

            var locations = GetLocations();
            if (locations != null)
            {
                rGrid.DataSource = locations.Select( t => new
                {
                    Id = t.Id,
                    t.Street1,
                    t.City,
                    t.State,
                    t.PostalCode,
                    t.Country
                } ).ToList();

                rGrid.EntityTypeId = EntityTypeCache.Read<Rock.Model.Location>().Id;

                rGrid.DataBind();
            }
            else
            {
                rGrid.DataSource = null;
                rGrid.DataBind();
            }
        }

        /// <summary>
        /// Gets the tags.
        /// </summary>
        /// <returns></returns>
        private IQueryable<Location> GetLocations()
        {
            int filterCount = 0;
            var queryable = new Rock.Model.LocationService( new RockContext() ).Queryable()
                                .Where( l => l.Street1 != null && l.Street1 != string.Empty );

            if ( !string.IsNullOrWhiteSpace(rFilter.GetUserPreference( "Street Address" )) )
            {
                string streetAddress1 = rFilter.GetUserPreference( "Street Address" );
                queryable = queryable.Where( l => l.Street1.StartsWith( streetAddress1 ) );
                filterCount++;
            }

            if ( !string.IsNullOrWhiteSpace( rFilter.GetUserPreference( "City" ) ) )
            {
                string city = rFilter.GetUserPreference( "City" );
                queryable = queryable.Where( l => l.City.StartsWith( city ) );
                filterCount++;
            }

            if ( !string.IsNullOrWhiteSpace( rFilter.GetUserPreference( "Not Geocoded" ) ) )
            {
                bool notGeocoded = Convert.ToBoolean(rFilter.GetUserPreference( "Not Geocoded" ));
                if ( notGeocoded )
                {
                    queryable = queryable.Where( l => l.GeoPoint == null );
                    filterCount++;
                }
            }

            if ( filterCount == 0 )
            {
                return null;
            }
            else
            {
                return queryable;
            }
        }

        #endregion

}
}