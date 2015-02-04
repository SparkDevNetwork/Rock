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
using System.Web.UI;

using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    public class CategoriesFieldType : CategoryFieldType
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
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            string formattedValue = string.Empty;

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                var guids = value.SplitDelimitedValues();
                var categories = new CategoryService( new RockContext() ).Queryable().Where( a => guids.Contains( a.Guid.ToString() ) );
                if ( categories.Any() )
                {
                    formattedValue = string.Join( ", ", ( from category in categories select category.Name ).ToArray() );
                }
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );
        }

        #endregion

        #region EditControl

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var picker = new CategoryPicker { ID = id, AllowMultiSelect = true }; 

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( ENTITY_TYPE_NAME_KEY ) )
                {
                    string entityTypeName = configurationValues[ENTITY_TYPE_NAME_KEY].Value;
                    if ( !string.IsNullOrWhiteSpace( entityTypeName ) && entityTypeName != None.IdValue )
                    {
                        picker.EntityTypeName = entityTypeName;
                        if ( configurationValues.ContainsKey( QUALIFIER_COLUMN_KEY ) )
                        {
                            picker.EntityTypeQualifierColumn = configurationValues[QUALIFIER_COLUMN_KEY].Value;
                            if ( configurationValues.ContainsKey( QUALIFIER_VALUE_KEY ) )
                            {
                                picker.EntityTypeQualifierValue = configurationValues[QUALIFIER_VALUE_KEY].Value;
                            }
                        }
                    }
                }
            }
            return picker;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as CategoryPicker;
            string result = null;

            if ( picker != null )
            {
                var guids = new List<Guid>();
                var ids = picker.SelectedValuesAsInt();
                var categories = new CategoryService( new RockContext() ).Queryable().Where( c => ids.Contains( c.Id ) );

                if ( categories.Any() )
                {
                    guids = categories.Select( c => c.Guid ).ToList();
                }

                result = string.Join( ",", guids );

            }

            return result;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( value != null )
            {
                var picker = control as CategoryPicker;
                var guids = new List<Guid>();

                if ( picker != null )
                {
                    var ids = value.Split( new[] { ',' } );

                    foreach ( var id in ids )
                    {
                        Guid guid;

                        if ( Guid.TryParse( id, out guid ) )
                        {
                            guids.Add( guid );
                        }
                    }

                    var categories = new CategoryService( new RockContext() ).Queryable().Where( c => guids.Contains( c.Guid ) );
                    picker.SetValues( categories );
                }
            }
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Gets the type of the filter comparison.
        /// </summary>
        /// <value>
        /// The type of the filter comparison.
        /// </value>
        public override ComparisonType FilterComparisonType
        {
            get
            {
                return ComparisonHelper.ContainsFilterComparisonTypes;
            }
        }

        #endregion

    }
}
