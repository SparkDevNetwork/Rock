﻿// <copyright>
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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Shows the distance of the person's home from a location" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Person Distance" )]
    public class DistanceFromSelect : DataSelectComponent
    {
        #region Properties

        /// <summary>
        /// Gets the name of the entity type. Filter should be an empty string
        /// if it applies to all entities
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        public override string AppliesToEntityType
        {
            get
            {
                return typeof( Rock.Model.Person ).FullName;
            }
        }

        /// <summary>
        /// Gets the section that this will appear in in the Field Selector
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get
            {
                return base.Section;
            }
        }

        /// <summary>
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public override string ColumnPropertyName
        {
            get
            {
                return "Distance";
            }
        }

        /// <summary>
        /// Gets the type of the column field.
        /// </summary>
        /// <value>
        /// The type of the column field.
        /// </value>
        public override Type ColumnFieldType
        {
            get { return typeof( double? ); }
        }

        /// <summary>
        /// Gets the default column header text.
        /// </summary>
        /// <value>
        /// The default column header text.
        /// </value>
        public override string ColumnHeaderText
        {
            get
            {
                return "Distance (miles)";
            }
        }

        #endregion

        #region Methods

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
            return "Distance";
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( RockContext context, MemberExpression entityIdProperty, string selection )
        {
            string[] selectionValues = selection.Split( '|' );

            Location selectedLocation = null;
            int? locationTypeValueId = null;
            if ( selectionValues.Count() >= 2 )
            {
                // the selected Location 
                selectedLocation = new LocationService( context ).Get( selectionValues[0].AsGuid() );

                // which address type (home, work, previous) to use as the person's location
                var locationTypeValueGuid = selectionValues[1].AsGuidOrNull();
                if (locationTypeValueGuid.HasValue)
                {
                    var locationTypeValue = DefinedValueCache.Get( locationTypeValueGuid.Value );
                    if ( locationTypeValue != null )
                    {
                        locationTypeValueId = locationTypeValue.Id;
                    }
                }
                
            }

            Guid familyGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
            int familyGroupTypeId = new GroupTypeService( context ).Get( familyGuid ).Id;

            var groupMemberQuery = new GroupMemberService( context ).Queryable();

            IQueryable<double?> personLocationDistanceQuery;

            if ( selectedLocation != null )
            {
                personLocationDistanceQuery = new PersonService( context ).Queryable()
                    .Select( p =>
                        groupMemberQuery
                        .Where( m => m.Group.GroupTypeId == familyGroupTypeId && m.PersonId == p.Id )
                        .SelectMany( m => m.Group.GroupLocations )
                        .Where( gl => gl.GroupLocationTypeValueId == locationTypeValueId )
                        .Where( gl => gl.Location.GeoPoint != null )
                        .Select( s => DbFunctions.Truncate(s.Location.GeoPoint.Distance( selectedLocation.GeoPoint ) * Location.MilesPerMeter, 2) )
                        .FirstOrDefault() );
            }
            else
            {
                personLocationDistanceQuery = new PersonService( context ).Queryable()
                    .Select( p => (double?)null );
            }

            var selectExpression = SelectExpressionExtractor.Extract( personLocationDistanceQuery, entityIdProperty, "p" );

            return selectExpression;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            LocationPicker locationPicker = new LocationPicker();
            locationPicker.ID = parentControl.ID + "_0";
            locationPicker.Label = "Location";
            parentControl.Controls.Add( locationPicker );

            RockDropDownList locationTypeList = new RockDropDownList();
            locationTypeList.Items.Clear();
            foreach ( var value in DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.GROUP_LOCATION_TYPE.AsGuid() ).DefinedValues.OrderBy( a => a.Order ).ThenBy( a => a.Value ) )
            {
                locationTypeList.Items.Add( new ListItem( value.Value, value.Guid.ToString() ) );
            }

            locationTypeList.Items.Insert( 0, Rock.Constants.None.ListItem );

            locationTypeList.ID = parentControl.ID + "_grouplocationType";
            locationTypeList.Label = "Address Type";
            parentControl.Controls.Add( locationTypeList );

            return new System.Web.UI.Control[] { locationPicker, locationTypeList };
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( System.Web.UI.Control parentControl, System.Web.UI.HtmlTextWriter writer, System.Web.UI.Control[] controls )
        {
            base.RenderControls( parentControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( System.Web.UI.Control[] controls )
        {
            if ( controls.Count() == 2 )
            {
                Guid? locationGuid = null;
                Guid? locationTypeGuid = null;
                LocationPicker locationPicker = controls[0] as LocationPicker;
                Location location = locationPicker.Location;
                if ( location != null )
                {
                    locationGuid = location.Guid;
                }

                RockDropDownList dropDownList = controls[1] as RockDropDownList;
                if ( dropDownList != null )
                {
                    locationTypeGuid = dropDownList.SelectedValue.AsGuidOrNull();
                }

                return string.Format( "{0}|{1}", locationGuid, locationTypeGuid );
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( System.Web.UI.Control[] controls, string selection )
        {
            if ( controls.Count() == 2 )
            {
                string[] selectionValues = selection.Split( '|' );
                if ( selectionValues.Length >= 2 )
                {
                    var locationPicker = controls[0] as LocationPicker;
                    var selectedLocation = new LocationService( new RockContext() ).Get( selectionValues[0].AsGuid() );
                    locationPicker.SetBestPickerModeForLocation( selectedLocation );
                    locationPicker.Location = selectedLocation;

                    RockDropDownList dropDownList = controls[1] as RockDropDownList;
                    dropDownList.SetValue( selectionValues[1] );
                }
            }
        }

        #endregion
    }
}
