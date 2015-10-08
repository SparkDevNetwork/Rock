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
using System.Web.UI.WebControls;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    public class NoteTypesFieldType : CategoryFieldType
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
                var names = new List<string>();

                foreach( string guidString in value.SplitDelimitedValues() )
                {
                    Guid? guid = guidString.AsGuidOrNull();
                    if ( guid.HasValue )
                    {
                        var noteType = NoteTypeCache.Read( guid.Value );
                        if ( noteType != null )
                        {
                            names.Add( noteType.Name );
                        }
                    }
                }

                formattedValue = names.AsDelimited( ", " );
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
            int entityTypeId = 0;
            string qualifierColumn = string.Empty;
            string qualifierValue = string.Empty;

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( ENTITY_TYPE_NAME_KEY ) )
                {
                    string entityTypeName = configurationValues[ENTITY_TYPE_NAME_KEY].Value;
                    if ( !string.IsNullOrWhiteSpace( entityTypeName ) && entityTypeName != None.IdValue )
                    {
                        var entityType = EntityTypeCache.Read( entityTypeName );
                        if ( entityType != null )
                        {
                            entityTypeId = entityType.Id;
                        }
                    }
                }
                if ( configurationValues.ContainsKey( QUALIFIER_COLUMN_KEY ) )
                {
                    qualifierColumn = configurationValues[QUALIFIER_COLUMN_KEY].Value;
                }

                if ( configurationValues.ContainsKey( QUALIFIER_VALUE_KEY ) )
                {
                    qualifierValue = configurationValues[QUALIFIER_VALUE_KEY].Value;
                }
            }

            RockCheckBoxList editControl = new RockCheckBoxList { ID = id };
            editControl.RepeatDirection = RepeatDirection.Horizontal;

            using ( var rockContext = new RockContext() )
            {
                var noteTypes = new NoteTypeService( rockContext ).Get( entityTypeId, qualifierColumn, qualifierValue ).ToList();
                if ( noteTypes.Any() )
                {
                    foreach ( var noteType in noteTypes )
                    {
                        ListItem listItem = new ListItem( noteType.Name, noteType.Guid.ToString().ToUpper() );
                        editControl.Items.Add( listItem );
                    }

                    return editControl;
                }
            }

            return null;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            List<string> values = new List<string>();

            if ( control != null && control is RockCheckBoxList )
            {
                RockCheckBoxList cbl = (RockCheckBoxList)control;
                foreach ( ListItem li in cbl.Items )
                    if ( li.Selected )
                        values.Add( li.Value );
                return values.AsDelimited<string>( "," );
            }

            return null;
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
                List<string> values = new List<string>();
                values.AddRange( value.Split( ',' ) );

                if ( control != null && control is RockCheckBoxList )
                {
                    RockCheckBoxList cbl = (RockCheckBoxList)control;
                    foreach ( ListItem li in cbl.Items )
                        li.Selected = values.Contains( li.Value, StringComparer.OrdinalIgnoreCase );
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
