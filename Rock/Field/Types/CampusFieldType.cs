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
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) CampusFieldType
    /// Stored as Campus's Guid
    /// </summary>
    public class CampusFieldType : FieldType, IEntityFieldType
    {

        #region Formatting

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            string formattedValue = value;

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                var campus = CampusCache.Read( value.AsGuid() );
                if ( campus != null )
                {
                    formattedValue = campus.Name;
                }
            }

            return base.FormatValue( parentControl, formattedValue, configurationValues, condensed );
        }

        #endregion

        #region Edit Control

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var campusPicker = new CampusPicker { ID = id };

            var campusList = CampusCache.All();

            if ( campusList.Any() )
            {
                campusPicker.Campuses = campusList;
                return campusPicker;
            }

            return null;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// returns Campus.Guid as string
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            CampusPicker campusPicker = control as CampusPicker;

            if ( campusPicker != null )
            {
                int? campusId = campusPicker.SelectedCampusId;
                if (campusId.HasValue)
                {
                    var campus = CampusCache.Read( campusId.Value );
                    if (campus != null )
                    {
                        return campus.Guid.ToString();
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the value.
        /// Expects value as a Campus.Guid as string
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            CampusPicker campusPicker = control as CampusPicker;

            if ( campusPicker != null )
            {
                Guid guid = value.AsGuid();

                // get the item (or null) and set it
                var campus = CampusCache.Read( guid );
                campusPicker.SetValue( campus == null ? "0" : campus.Id.ToString() );
            }
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Control FilterControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var cbList = new RockCheckBoxList();
            cbList.ID = string.Format( "{0}_cbList", id );
            cbList.AddCssClass( "js-filter-control" );
            cbList.RepeatDirection = RepeatDirection.Horizontal;

            var campusList = CampusCache.All();
            if ( campusList.Any() )
            {
                foreach ( var campus in campusList )
                {
                    ListItem listItem = new ListItem( campus.Name, campus.Guid.ToString() );
                    cbList.Items.Add( listItem );
                }

                return cbList;
            }

            return null;
        }

        /// <summary>
        /// Gets the filter value.
        /// </summary>
        /// <param name="filterControl"></param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override List<string> GetFilterValues( Control filterControl, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var values = new List<string>();

            List<string> cblValues = new List<string>();

            if ( filterControl != null && filterControl is RockCheckBoxList )
            {
                CheckBoxList cbl = (CheckBoxList)filterControl;
                foreach ( ListItem li in cbl.Items )
                {
                    if ( li.Selected )
                    {
                        cblValues.Add( li.Value );
                    }
                }

                values.Add( cblValues.AsDelimited<string>( "," ) );
            }

            return values;
        }

        /// <summary>
        /// Sets the filter value.
        /// </summary>
        /// <param name="filterControl"></param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues"></param>
        public override void SetFilterValues( Control filterControl, Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues )
        {
            if ( filterControl != null && filterControl is CheckBoxList && filterValues.Any() )
            {
                string value = filterValues[0];
                if ( value != null )
                {
                    List<string> values = new List<string>();
                    values.AddRange( value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) );

                    CheckBoxList cbl = (CheckBoxList)filterControl;
                    foreach ( ListItem li in cbl.Items )
                    {
                        li.Selected = values.Contains( li.Value );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the filters expression.
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <returns></returns>
        public override Expression FilterExpression( IService serviceInstance, ParameterExpression parameterExpression, List<string> filterValues )
        {
            if ( filterValues.Count == 1 )
            {
                MemberExpression propertyExpression = Expression.Property( parameterExpression, "Value" );

                List<string> selectedValues = filterValues[0].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                if ( selectedValues.Any() )
                {
                    ConstantExpression constantExpression = Expression.Constant( selectedValues, typeof( List<string> ) );
                    return Expression.Call( constantExpression, typeof( List<string> ).GetMethod( "Contains", new Type[] { typeof( string ) } ), propertyExpression );
                }
            }

            return null;
        }

        #endregion

        #region Entity Methods

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            var item = CampusCache.Read( guid );
            return item != null ? item.Id : (int?)null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            CampusCache item = null;
            if ( id.HasValue )
            {
                item = CampusCache.Read( id.Value );
            }
            string guidValue = item != null ? item.Guid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value )
        {
            return GetEntity( value, null );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value, RockContext rockContext )
        {
            Guid? guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                rockContext = rockContext ?? new RockContext();
                return new CampusService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

    }
}