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
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Block for viewing list of tags
    /// </summary>
    [DisplayName( "Location List" )]
    [Category( "Core" )]
    [Description( "Block for viewing a list of locations." )]

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.BlockTypeGuid( "5144ED5B-89A9-4D77-B0E5-695070BE0C8E" )]
    public partial class LocationList : RockBlock, ICustomGridColumns
    {
        public static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

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
            NavigateToLinkedPage( AttributeKey.DetailPage, parms );
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
            rFilter.SetFilterPreference( "Street Address", txtStreetAddress1.Text );
            rFilter.SetFilterPreference( "City", txtCity.Text );
            rFilter.SetFilterPreference( "Not Geocoded", cbNotGeocoded.Checked.ToString() );
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            if ( !string.IsNullOrWhiteSpace( rFilter.GetFilterPreference( "Street Address" ) ) )
            {
                txtStreetAddress1.Text = rFilter.GetFilterPreference( "Street Address" );
            }

            if ( !string.IsNullOrWhiteSpace( rFilter.GetFilterPreference( "City" ) ) )
            {
                txtCity.Text = rFilter.GetFilterPreference( "City" );
            }

            if ( !string.IsNullOrWhiteSpace( rFilter.GetFilterPreference( "Not Geocoded" ) ) )
            {
                cbNotGeocoded.Checked = Convert.ToBoolean( rFilter.GetFilterPreference( "Not Geocoded" ) );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            //rGrid.Columns[0].Visible = true

            var locations = GetLocations();
            if ( locations != null )
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

                rGrid.EntityTypeId = EntityTypeCache.Get<Rock.Model.Location>().Id;

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

            if ( !string.IsNullOrWhiteSpace( rFilter.GetFilterPreference( "Street Address" ) ) )
            {
                string streetAddress1 = rFilter.GetFilterPreference( "Street Address" );
                queryable = queryable.Where( l => l.Street1.StartsWith( streetAddress1 ) );
                filterCount++;
            }

            if ( !string.IsNullOrWhiteSpace( rFilter.GetFilterPreference( "City" ) ) )
            {
                string city = rFilter.GetFilterPreference( "City" );
                queryable = queryable.Where( l => l.City.StartsWith( city ) );
                filterCount++;
            }

            if ( !string.IsNullOrWhiteSpace( rFilter.GetFilterPreference( "Not Geocoded" ) ) )
            {
                bool notGeocoded = Convert.ToBoolean( rFilter.GetFilterPreference( "Not Geocoded" ) );
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