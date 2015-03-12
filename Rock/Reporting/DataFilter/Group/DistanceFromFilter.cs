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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Group
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter group within a specified distance from a location" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Group Distance From Filter" )]
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
            get { return typeof( Rock.Model.Group ).FullName; }
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

            if ( selectionValues.Length >= 3 )
            {
                Guid? locationGuid = selectionValues[1].AsGuidOrNull();
                if ( locationGuid.HasValue )
                {
                    var location = new LocationService( new RockContext() ).Get( locationGuid.Value );
                    double miles = selectionValues[2].AsDoubleOrNull() ?? 0;

                    result = string.Format( "Within {0} miles from location: {1}", miles, location != null ? location.ToString() : string.Empty );
                }
            }

            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            RockDropDownList groupLocationTypeList = new RockDropDownList();
            groupLocationTypeList.Items.Clear();
            foreach ( var value in Rock.Web.Cache.DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.GROUP_LOCATION_TYPE.AsGuid() ).DefinedValues.OrderBy( a => a.Order ).ThenBy( a => a.Value ) )
            {
                groupLocationTypeList.Items.Add( new ListItem( value.Value, value.Guid.ToString() ) );
            }

            groupLocationTypeList.Items.Insert( 0, Rock.Constants.None.ListItem );

            groupLocationTypeList.ID = filterControl.ID + "_groupLocationTypeList";
            groupLocationTypeList.Label = "Location Type";
            filterControl.Controls.Add( groupLocationTypeList );            
            
            LocationPicker locationPicker = new LocationPicker();
            locationPicker.ID = filterControl.ID + "_locationPicker";
            locationPicker.Label = "Location";

            filterControl.Controls.Add( locationPicker );

            NumberBox numberBox = new NumberBox();
            numberBox.ID = filterControl.ID + "_numberBox";
            numberBox.NumberType = ValidationDataType.Double;
            numberBox.Label = "Miles";
            numberBox.AddCssClass( "number-box-miles" );
            filterControl.Controls.Add( numberBox );

            return new Control[3] { groupLocationTypeList, locationPicker, numberBox };
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
            Guid? groupLocationTypeGuid = null;
            Guid? groupLocationGuid = null;
            RockDropDownList dropDownList = controls[0] as RockDropDownList;
            if ( dropDownList != null )
            {
                groupLocationTypeGuid = dropDownList.SelectedValue.AsGuidOrNull();
            }
            
            var location = ( controls[1] as LocationPicker ).Location;
            if ( location != null )
            {
                groupLocationGuid = location.Guid;
            }

            var value2 = ( controls[2] as NumberBox ).Text;
            return groupLocationTypeGuid.ToString() + "|" + groupLocationGuid.ToString() + "|" + value2;
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
            if ( selectionValues.Length >= 3 )
            {
                var groupLocationType = controls[0] as RockDropDownList;
                Guid? groupLocationTypeGuid = selectionValues[0].AsGuidOrNull();
                groupLocationType.SetValue( groupLocationTypeGuid.ToString()  );

                var locationPicker = controls[1] as LocationPicker;
                var selectedLocation = new LocationService( new RockContext() ).Get( selectionValues[1].AsGuid() );
                locationPicker.CurrentPickerMode = locationPicker.GetBestPickerModeForLocation( selectedLocation );
                locationPicker.Location = selectedLocation;
                
                var numberBox = controls[2] as NumberBox;
                numberBox.Text = selectionValues[2];
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
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 2 )
            {
                Guid? groupLocationTypeValueGuid = selectionValues[0].AsGuidOrNull();
                
                Location location = new LocationService( (RockContext)serviceInstance.Context ).Get( selectionValues[1].AsGuid() );

                if ( location == null )
                {
                    return null;
                }

                var selectedLocationGeoPoint = location.GeoPoint;
                double miles = selectionValues[2].AsDoubleOrNull() ?? 0;
                double meters = miles * Location.MetersPerMile;

                GroupService groupService = new GroupService( (RockContext)serviceInstance.Context );

                var qry = groupService.Queryable()
                    .Where( p => p.GroupLocations
                        .Where( l => l.GroupLocationTypeValue.Guid == groupLocationTypeValueGuid )
                        .Where( l => l.IsMappedLocation ).Any() );

                // limit to distance LessThan specified distance (dbGeography uses meters for distance units)
                qry = qry
                    .Where( p => p.GroupLocations.Any( l => l.Location.GeoPoint.Distance( selectedLocationGeoPoint ) <= meters ) );

                Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Group>( qry, parameterExpression, "p" );

                return extractedFilterExpression;
            }

            return null;
        }

        #endregion
    }
}