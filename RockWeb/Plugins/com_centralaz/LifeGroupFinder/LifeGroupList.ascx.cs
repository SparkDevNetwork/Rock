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
using System.Web.UI.WebControls;
using System.Net;
using System.Xml.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.LifeGroupFinder
{
    [DisplayName( "Life Group List" )]
    [Category( "com_centralaz > Groups" )]
    [Description( "Lists all groups for the configured group types." )]

    [LinkedPage( "Detail Page", "", true, "", "", 0 )]
    [CustomDropdownListField( "Limit to Active Status", "Select which groups to show, based on active status. Select [All] to let the user filter by active status.", "all^[All], active^Active, inactive^Inactive", false, "all", Order = 10 )]
    public partial class LifeGroupList : RockBlock
    {
        #region Fields

        private int _groupTypesCount = 0;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the parameter state.
        /// </summary>
        /// <value>
        /// The state of the parameter.
        /// </value>
        public Dictionary<string, string> ParameterState
        {
            get
            {
                var parameterState = Session["ParameterState"] as Dictionary<string, string>;
                if ( parameterState == null )
                {
                    parameterState = new Dictionary<string, string>();

                    Session["ParameterState"] = parameterState;
                }
                return parameterState;
            }

            set
            {
                Session["ParameterState"] = value;
            }
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            this.AddConfigurationUpdateTrigger( upnlGroupList );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                LoadList();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbReturn control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbReturn_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the list.
        /// </summary>
        private void LoadList()
        {
            // Build qry
            int smallGroupTypeId = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_SMALL_GROUP ).Id;
            RockContext rockContext = new RockContext();
            AttributeValueService attributeValueService = new AttributeValueService( rockContext );
            var qry = new GroupService( rockContext ).Queryable()
                .Where( g => smallGroupTypeId == g.GroupTypeId );

            qry = qry.Where( g => g.Members.Any( m => m.GroupRole.IsLeader == true ) );

            if ( ParameterState["Campus"].AsIntegerOrNull() != null )
            {
                qry = qry.Where( g => g.CampusId == ParameterState["Campus"].AsIntegerOrNull() );
            }

            if ( !String.IsNullOrWhiteSpace( ParameterState["Days"] ) )
            {
                List<DayOfWeek> daysList = ParameterState["Days"].Split( ';' ).ToList().Select( i => (DayOfWeek)( i.AsInteger() ) ).ToList();
                if ( daysList.Any() )
                {
                    //TODO: find out how days works
                    qry = qry.Where( g => daysList.Contains( g.Schedule.WeeklyDayOfWeek.Value ) );
                }
            }

            bool? hasPets = ParameterState["Pets"].AsBooleanOrNull();
            if ( hasPets != null && hasPets.Value )
            {
                var attributeValues = attributeValueService
                                        .Queryable()
                                        .Where( v => v.Attribute.Key == "HasPets" && v.Value == "False" );

                qry = qry.Where( g => attributeValues.Select( v => v.EntityId ).Contains( g.Id ) );
            }

            bool shadeRow = false;

            // Construct List
            var attributeChildValues = attributeValueService
                                        .Queryable()
                                        .Where( v => v.Attribute.Key == "HasChildren" );

            if ( qry.Count() > 0 )
            {
                foreach ( Group group in qry )
                {
                    bool displayGroup = true;
                    if ( !String.IsNullOrWhiteSpace( ParameterState["Children"] ) )
                    {
                        List<String> childrenList = ParameterState["Children"].Split( ';' ).ToList();
                        if ( !childrenList.Intersect( attributeChildValues.FirstOrDefault( v => v.EntityId == group.Id ).Value.Split( ',' ).ToList() ).Any() )
                        {
                            displayGroup = false;
                        }
                    }

                    if ( displayGroup )
                    {
                        if ( shadeRow )
                        {
                            phGroups.Controls.Add( new LiteralControl( "<div class='row' >" ) );
                        }
                        else
                        {
                            phGroups.Controls.Add( new LiteralControl( "<div class='row'>" ) );
                        }

                        phGroups.Controls.Add( new LiteralControl( "<div class='col-md-1'>" ) );
                        phGroups.Controls.Add( new LiteralControl( "<div class='photo'>" ) );
                        GroupMember leader = group.Members.FirstOrDefault( m => m.GroupRole.IsLeader == true );
                        if ( leader != null )
                        {
                            Person person = leader.Person;
                            string imgTag = Rock.Model.Person.GetPhotoImageTag( person.PhotoId, person.Age, person.Gender, 200, 200 );
                            if ( person.PhotoId.HasValue )
                            {
                                phGroups.Controls.Add( new LiteralControl( String.Format( "<a href='{0}'>{1}</a>", person.PhotoUrl, imgTag ) ) );
                            }
                            else
                            {
                                phGroups.Controls.Add( new LiteralControl( "imgTag" ) );
                            }
                        }

                        phGroups.Controls.Add( new LiteralControl( "</div>" ) );
                        phGroups.Controls.Add( new LiteralControl( "</div>" ) );
                        phGroups.Controls.Add( new LiteralControl( "<div class='col-md-1'>" ) );
                        phGroups.Controls.Add( new LiteralControl( "<div class='row'>" ) );
                        group.LoadAttributes();
                        if ( group.GetAttributeValue( "HasPets" ).AsBoolean() )
                        {
                            phGroups.Controls.Add( new LiteralControl( "<i class='fa fa-paw fa-4x text-primary fa-fw'></i>" ) );
                        }
                        else
                        {
                            phGroups.Controls.Add( new LiteralControl( "<i class='fa fa-paw fa-4x text-muted fa-fw'></i>" ) );
                        }

                        phGroups.Controls.Add( new LiteralControl( "</div>" ) );
                        phGroups.Controls.Add( new LiteralControl( "<div class='row'>" ) );

                        if ( group.GetAttributeValues( "HasChildren" ).Any() )
                        {
                            phGroups.Controls.Add( new LiteralControl( "<i class='fa fa-child fa-4x text-primary fa-fw'></i>" ) );
                        }
                        else
                        {
                            phGroups.Controls.Add( new LiteralControl( "<i class='fa fa-child fa-4x text-muted fa-fw'></i>" ) );
                        }

                        phGroups.Controls.Add( new LiteralControl( "</div>" ) );
                        phGroups.Controls.Add( new LiteralControl( "</div>" ) );
                        phGroups.Controls.Add( new LiteralControl( "<div class='col-md-10'>" ) );
                        phGroups.Controls.Add( new LiteralControl( "<div class='row'>" ) );
                        phGroups.Controls.Add( new LiteralControl( "<div class='pull-left'>" ) );
                        phGroups.Controls.Add( new LiteralControl( String.Format( "<u><b><p style='font-size:24px'>{0}</p></b></u>", group.Name ) ) );
                        phGroups.Controls.Add( new LiteralControl( "</div>" ) );
                        phGroups.Controls.Add( new LiteralControl( "<div class='pull-right'>" ) );
                        phGroups.Controls.Add( new LiteralControl( string.Format( "<a href='{0}'>View >></a>", ResolveUrl( string.Format( "~/LifeGroup/{0}", group.Id ) ) ) ) );
                        phGroups.Controls.Add( new LiteralControl( "</div>" ) );
                        phGroups.Controls.Add( new LiteralControl( "</div>" ) );
                        phGroups.Controls.Add( new LiteralControl( "<div class='row'>" ) );
                        phGroups.Controls.Add( new LiteralControl( group.GetAttributeValue( "ListDescription" ) ) );
                        phGroups.Controls.Add( new LiteralControl( "</div>" ) );
                        phGroups.Controls.Add( new LiteralControl( "<hr />" ) );
                        phGroups.Controls.Add( new LiteralControl( "<div class='row'>" ) );
                        phGroups.Controls.Add( new LiteralControl( "<div class='pull-left'>" ) );
                        phGroups.Controls.Add( new LiteralControl( group.Schedule.ToString() ) );
                        phGroups.Controls.Add( new LiteralControl( "</div>" ) );
                        phGroups.Controls.Add( new LiteralControl( "<center>" ) );
                        phGroups.Controls.Add( new LiteralControl( group.GetAttributeValue( "Crossroads" ) ) );
                        phGroups.Controls.Add( new LiteralControl( "</center>" ) );
                        phGroups.Controls.Add( new LiteralControl( "<div class='pull-right'>" ) );
                        phGroups.Controls.Add( new LiteralControl( GetDistance( group ) ) );
                        phGroups.Controls.Add( new LiteralControl( "</div>" ) );
                        phGroups.Controls.Add( new LiteralControl( "</div>" ) );
                        phGroups.Controls.Add( new LiteralControl( "</div>" ) );
                        phGroups.Controls.Add( new LiteralControl( "</div>" ) );
                        phGroups.Controls.Add( new LiteralControl( "</br>" ) );
                        phGroups.Controls.Add( new LiteralControl( "</br>" ) );
                        shadeRow = !shadeRow;
                    }
                }
            }
            else
            {
                phGroups.Controls.Add( new LiteralControl( "<h1>" ) );
                phGroups.Controls.Add( new LiteralControl( "No Results" ) );
                phGroups.Controls.Add( new LiteralControl( "</h1>" ) );
            }

        }

        /// <summary>
        /// Gets the distance.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        protected string GetDistance( Group group )
        {
            RockContext rockContext = new RockContext();
            Location personLocation = new LocationService( rockContext )
                        .Get( ParameterState["StreetAddress1"], ParameterState["StreetAddress2"], ParameterState["City"],
                            ParameterState["State"], ParameterState["PostalCode"], ParameterState["Country"] );
            if ( personLocation.GeoPoint == null )
            {
                SetGeoPointFromAddress( personLocation );
                rockContext.SaveChanges();
            }

            double? closestLocation = null;
            foreach ( var groupLocation in group.GroupLocations
                        .Where( gl => gl.Location.GeoPoint != null ) )
            {

                if ( personLocation != null && personLocation.GeoPoint != null )
                {
                    double meters = groupLocation.Location.GeoPoint.Distance( personLocation.GeoPoint ) ?? 0.0D;
                    double miles = meters * ( 1 / 1609.344 );//TODO: replace with Location.MilesperMeter

                    // If this group already has a distance calculated, see if this location is closer and if so, use it instead
                    if ( closestLocation != null )
                    {
                        if ( closestLocation < miles )
                        {
                            closestLocation = miles;
                        }
                    }
                    else
                    {
                        closestLocation = miles;
                    }
                }
            }

            if ( closestLocation != null )
            {
                return String.Format( "{0}m", closestLocation.Value.ToString( "0.0" ) );
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Sets the geo point from address.
        /// </summary>
        /// <param name="personLocation">The person location.</param>
        private static void SetGeoPointFromAddress( Location personLocation )
        {
            var requestUri = string.Format( "http://maps.googleapis.com/maps/api/geocode/xml?address={0}&sensor=false", Uri.EscapeDataString( personLocation.GetFullStreetAddress() ) );

            var request = WebRequest.Create( requestUri );
            var response = request.GetResponse();
            var xdoc = XDocument.Load( response.GetResponseStream() );

            var result = xdoc.Element( "GeocodeResponse" ).Element( "result" );
            var locationElement = result.Element( "geometry" ).Element( "location" );
            var lat = locationElement.Element( "lat" );
            var lng = locationElement.Element( "lng" );
            personLocation.SetLocationPointFromLatLong( (double)lat, (double)lng );
        }

        #endregion
    }
}