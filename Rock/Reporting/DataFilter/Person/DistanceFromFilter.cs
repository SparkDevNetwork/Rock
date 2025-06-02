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
using System.ComponentModel.Composition;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people on home address within a specified distance from a location" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Distance From Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "0B08B4E8-5C3D-479A-A53A-0889189413AC" )]
    public class DistanceFromFilter : DataFilterComponent
    {
        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return typeof( Rock.Model.Person ).FullName; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Additional Filters"; }
        }

        #endregion

        #region Configuration

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Reporting/DataFilters/Person/distanceFromFilter.obs" )
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var data = new Dictionary<string, string>();
            string[] selectionValues = selection.Split( '|' );

            if ( selectionValues.Length >= 2 )
            {
                var selectedLocation = new LocationService( rockContext ).Get( selectionValues[0].AsGuid() );

                if ( selectedLocation != null )
                {
                    if ( selectedLocation.IsNamedLocation )
                    {
                        var locationBag = selectedLocation.ToListItemBag();
                        data.AddOrReplace( "location", locationBag.ToCamelCaseJson( false, true ) );
                    }
                    else if ( !string.IsNullOrWhiteSpace( selectedLocation.GetFullStreetAddress().Replace( ",", string.Empty ) ) )
                    {
                        var address = new AddressControlBag
                        {
                            Street1 = selectedLocation.Street1,
                            Street2 = selectedLocation.Street2,
                            City = selectedLocation.City,
                            Locality = selectedLocation.County,
                            State = selectedLocation.State,
                            PostalCode = selectedLocation.PostalCode,
                            Country = selectedLocation.Country
                        };
                        data.AddOrReplace( "location", address.ToCamelCaseJson( false, true ) );
                    }
                    else if ( selectedLocation.GeoPoint != null )
                    {
                        data.AddOrReplace( "location", selectedLocation.GeoPoint.AsText().ToJson() );
                    }
                    else if ( selectedLocation.GeoFence != null )
                    {
                        data.AddOrReplace( "location", selectedLocation.GeoFence.AsText().ToJson() );
                    }
                }

                var miles = selectionValues[1];
                data.AddOrReplace( "miles", miles );
            }

            return data;
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            var locationGuid = "";
            var locationRaw = data.GetValueOrNull( "location" );
            var miles = data.GetValueOrNull( "miles" );

            if ( !string.IsNullOrWhiteSpace( locationRaw ) )
            {
                // ListItemBag or AddressControlBag
                if ( locationRaw.StartsWith( "{" ) )
                {
                    var locationBag = locationRaw.FromJsonOrNull<ListItemBag>();
                    var addressBag = locationRaw.FromJsonOrNull<AddressControlBag>();

                    if ( locationBag != null && locationBag.Value.IsNotNullOrWhiteSpace() )
                    {
                        locationGuid = locationBag.Value;
                    }
                    else if ( addressBag != null && addressBag.Street1.IsNotNullOrWhiteSpace() )
                    {
                        var orgCountryCode = GlobalAttributesCache.Get().OrganizationCountry;
                        var defaultCountryCode = string.IsNullOrWhiteSpace( orgCountryCode ) ? "US" : orgCountryCode;

                        var location = new Location
                        {
                            Street1 = addressBag.Street1,
                            Street2 = addressBag.Street2,
                            City = addressBag.City,
                            County = addressBag.Locality,
                            State = addressBag.State,
                            PostalCode = addressBag.PostalCode,
                            Country = addressBag.Country.IsNotNullOrWhiteSpace() ? addressBag.Country : defaultCountryCode
                        };

                        string validationMessage;
                        var isValid = LocationService.ValidateLocationAddressRequirements( location, out validationMessage );

                        if ( isValid )
                        {
                            var locationService = new LocationService( rockContext );
                            location = locationService.Get( location.Street1, location.Street2, location.City, location.State, location.County, location.PostalCode, location.Country, null );
                            locationGuid = location.Guid.ToString();
                        }
                    }
                }
                else
                {
                    // GeoFence or GeoPoint
                    var geoString = locationRaw.FromJsonOrNull<string>();
                    var locationService = new LocationService( rockContext );

                    if ( geoString.StartsWith( "POLYGON" ) )
                    {
                        // GeoFence
                        var location = locationService.GetByGeoFence( DbGeography.PolygonFromText( geoString, DbGeography.DefaultCoordinateSystemId ) );
                        locationGuid = location.Guid.ToString();
                    }
                    else if ( geoString.StartsWith( "POINT" ) )
                    {
                        // GeoPoint
                        var location = locationService.GetByGeoPoint( DbGeography.FromText( geoString, DbGeography.DefaultCoordinateSystemId ) );
                        locationGuid = location.Guid.ToString();
                    }
                }
            }

            return $"{locationGuid}|{miles}";
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "Distance From";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before 
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            return @"
function() {
  var locationName = $('.picker-label', $content).find('span').text();
  var miles = $('.number-box', $content).find('input').val();

  return 'Within ' + miles + ' from location: ' + locationName;
}
";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = "Distance From";
            string[] selectionValues = selection.Split( '|' );

            if ( selectionValues.Length >= 2 )
            {
                Guid locationGuid = selectionValues[0].AsGuid();
                var location = new LocationService( new RockContext() ).Get( locationGuid );
                double miles = selectionValues[1].AsDoubleOrNull() ?? 0;

                result = string.Format( "Within {0} miles from location: {1}", miles, location != null ? location.ToString() : string.Empty );
            }

            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            LocationPicker locationPicker = new LocationPicker();
            locationPicker.ID = filterControl.ID + "_0";
            locationPicker.Label = "Location";

            filterControl.Controls.Add( locationPicker );

            NumberBox numberBox = new NumberBox();
            numberBox.ID = filterControl.ID + "_1";
            numberBox.NumberType = ValidationDataType.Double;
            numberBox.Label = "Miles";
            numberBox.AddCssClass( "number-box-miles" );
            filterControl.Controls.Add( numberBox );

            return new Control[2] { locationPicker, numberBox };
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            base.RenderControls( entityType, filterControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var location = ( controls[0] as LocationPicker ).Location;
            var value1 = string.Empty;
            if ( location != null )
            {
                value1 = location.Guid.ToString();
            }

            var value2 = ( controls[1] as NumberBox ).Text;
            return value1 + "|" + value2;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 2 )
            {
                var locationPicker = controls[0] as LocationPicker;
                var selectedLocation = new LocationService( new RockContext() ).Get( selectionValues[0].AsGuid() );
                locationPicker.SetBestPickerModeForLocation( selectedLocation );
                locationPicker.Location = selectedLocation;
                var numberBox = controls[1] as NumberBox;
                numberBox.Text = selectionValues[1];
            }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var rockContext = ( RockContext ) serviceInstance.Context;

            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 2 )
            {
                Location location = new LocationService( rockContext ).Get( selectionValues[0].AsGuid() );

                if ( location == null )
                {
                    return null;
                }

                GroupMemberService groupMemberService = new GroupMemberService( rockContext );
                var groupTypeFamilyId = GroupTypeCache.GetFamilyGroupType().Id;

                // limit to Family's Home Addresses that have are a real location (not a PO Box)
                var groupMemberServiceQry = groupMemberService.Queryable( true )
                    .Where( xx => xx.Group.GroupTypeId == groupTypeFamilyId );

                int groupLocationTypeHomeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() ).Id;

                // if a specific point was selected (whether a marker, or an address), we'll do a radial search
                if ( location.GeoPoint != null )
                {
                    // limit to distance LessThan specified distance (dbGeography uses meters for distance units)
                    double miles = selectionValues[1].AsDoubleOrNull() ?? 0;
                    double meters = miles * Location.MetersPerMile;

                    groupMemberServiceQry = groupMemberServiceQry
                        .Where( xx =>
                            xx.Group.GroupLocations.Any( l =>
                                l.GroupLocationTypeValue.Id == groupLocationTypeHomeId
                                && l.IsMappedLocation
                                && location.GeoPoint.Buffer( meters ).Intersects( l.Location.GeoPoint )
                                ) );
                }
                // otherwise if a geo fence was drawn, see what points intersect within it
                else if ( location.GeoFence != null )
                {
                    groupMemberServiceQry = groupMemberServiceQry
                        .Where( xx =>
                            xx.Group.GroupLocations.Any( l =>
                                l.GroupLocationTypeValue.Id == groupLocationTypeHomeId
                                && l.IsMappedLocation
                                && l.Location.GeoPoint.Intersects( location.GeoFence )
                                ) );
                }

                var qry = new PersonService( rockContext ).Queryable()
                    .Where( p => groupMemberServiceQry.Any( xx => xx.PersonId == p.Id ) );

                Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

                return extractedFilterExpression;
            }

            return null;
        }

        #endregion
    }
}