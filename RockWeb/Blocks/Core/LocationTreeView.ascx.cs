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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Navigation Tree for named locations
    /// </summary>
    [DisplayName( "Location Tree View" )]
    [Category( "Core" )]
    [Description( "Creates a navigation tree for named locations." )]

    [TextField( "Treeview Title",
        Description = "Location Tree View",
        IsRequired = false,
        Key = AttributeKey.TreeviewTitle )]

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage )]

    public partial class LocationTreeView : RockBlock
    {
        public static class AttributeKey
        {
            public const string TreeviewTitle = "TreeviewTitle";
            public const string DetailPage = "DetailPage";
        }


        #region Fields

        private string _LocationId = string.Empty;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _LocationId = PageParameter( "LocationId" );

            hfPageRouteTemplate.Value = ( this.RockPage.RouteData.Route as System.Web.Routing.Route ).Url;

            bool canEditBlock = IsUserAuthorized( Authorization.EDIT );

            // hide all the actions if user doesn't have EDIT to the block
            divTreeviewActions.Visible = canEditBlock;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            bool canEditBlock = IsUserAuthorized( Authorization.EDIT );

            if ( !Page.IsPostBack )
            {
                if ( string.IsNullOrWhiteSpace( _LocationId ) )
                {
                    // If no location was selected, try to find the first location and redirect
                    // back to current page with that location selected
                    var location = FindFirstLocation();
                    {
                        if ( location != null )
                        {
                            _LocationId = location.Id.ToString();
                            string redirectUrl = this.Request.Url.AbsolutePath + "?LocationId=" + _LocationId.ToString();
                            this.Response.Redirect( redirectUrl, false );
                            Context.ApplicationInstance.CompleteRequest();
                        }
                    }
                }
            }


            if ( !string.IsNullOrWhiteSpace( _LocationId ) )
            {
                hfInitialLocationId.Value = _LocationId;
                hfSelectedLocationId.Value = _LocationId;
                Location Location = ( new LocationService( new RockContext() ) ).Get( int.Parse( _LocationId ) );

                lbAddLocationChild.Enabled = Location != null && canEditBlock;

                // get the parents of the selected item so we can tell the treeview to expand those
                List<string> parentIdList = new List<string>();
                while ( Location != null )
                {
                    Location = Location.ParentLocation;
                    if ( Location != null )
                    {
                        parentIdList.Insert( 0, Location.Id.ToString() );
                    }
                }

                // also get any additional expanded nodes that were sent in the Post
                string postedExpandedIds = this.Request.Params["ExpandedIds"];
                if ( !string.IsNullOrWhiteSpace( postedExpandedIds ) )
                {
                    var postedExpandedIdList = postedExpandedIds.Split( ',' ).ToList();
                    foreach ( var id in postedExpandedIdList )
                    {
                        if ( !parentIdList.Contains( id ) )
                        {
                            parentIdList.Add( id );
                        }
                    }
                }

                hfInitialLocationParentIds.Value = parentIdList.AsDelimited( "," );
            }
            else
            {
                // let the Add button be visible if there is nothing selected
                lbAddLocationChild.Enabled = true;
            }

            // disable add child Location if no Location is selected
            int selectedLocationId = hfSelectedLocationId.ValueAsInt();

            if ( selectedLocationId == 0 )
            {
                lbAddLocationChild.Enabled = false;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbAddLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAddLocationRoot_Click( object sender, EventArgs e )
        {
            Dictionary<string, string> qryParams = new Dictionary<string, string>();
            qryParams.Add( "LocationId", 0.ToString() );
            qryParams.Add( "ParentLocationId", 0.ToString() );
            qryParams.Add( "ExpandedIds", hfInitialLocationParentIds.Value );

            NavigateToLinkedPage( AttributeKey.DetailPage, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the lbAddLocationChild control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddLocationChild_Click( object sender, EventArgs e )
        {
            int locationId = hfSelectedLocationId.ValueAsInt();

            Dictionary<string, string> qryParams = new Dictionary<string, string>();
            qryParams.Add( "LocationId", 0.ToString() );
            qryParams.Add( "ParentLocationId", locationId.ToString() );
            qryParams.Add( "ExpandedIds", hfInitialLocationParentIds.Value );

            NavigateToLinkedPage( AttributeKey.DetailPage, qryParams );
        }

        #endregion

        #region Methods

        private Location FindFirstLocation()
        {
            return new LocationService( new RockContext() ).Queryable()
                .Where( l =>
                    l.Name != null &&
                    l.Name != string.Empty &&
                    !l.ParentLocationId.HasValue )
                .OrderBy( l => l.Name )
                .FirstOrDefault();
        }

        #endregion
    }
}