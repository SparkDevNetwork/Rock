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
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Stored as NoteType.Guid
    /// </summary>
    public class NoteTypeFieldType : FieldType, IEntityFieldType
    {

        #region Configuration

        /// <summary>
        /// Entity Type Name Key
        /// </summary>
        protected const string ENTITY_TYPE_NAME_KEY = "entityTypeName";

        /// <summary>
        /// Qualifier Column Key
        /// </summary>
        protected const string QUALIFIER_COLUMN_KEY = "qualifierColumn";

        /// <summary>
        /// Qualifier Value Key
        /// </summary>
        protected const string QUALIFIER_VALUE_KEY = "qualifierValue";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            configKeys.Add( ENTITY_TYPE_NAME_KEY );
            configKeys.Add( QUALIFIER_COLUMN_KEY );
            configKeys.Add( QUALIFIER_VALUE_KEY );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            var ddl = new RockDropDownList();
            controls.Add( ddl );
            ddl.Items.Add( new ListItem( None.Text, None.IdValue ) );
            foreach ( var entityType in new EntityTypeService( new RockContext() ).GetEntities().OrderBy( e => e.FriendlyName ).ThenBy( e => e.Name ) )
            {
                ddl.Items.Add( new ListItem( entityType.FriendlyName, entityType.Name ) );
            }
            ddl.AutoPostBack = true;
            ddl.SelectedIndexChanged += OnQualifierUpdated;
            ddl.Label = "Entity Type";
            ddl.Help = "The type of entity to display categories for.";

            var tbColumn = new RockTextBox();
            controls.Add( tbColumn );
            tbColumn.AutoPostBack = true;
            tbColumn.TextChanged += OnQualifierUpdated;
            tbColumn.Label = "Qualifier Column";
            tbColumn.Help = "Entity column qualifier.";

            var tbValue = new RockTextBox();
            controls.Add( tbValue );
            tbValue.AutoPostBack = true;
            tbValue.TextChanged += OnQualifierUpdated;
            tbValue.Label = "Qualifier Value";
            tbValue.Help = "Entity column value.";

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( ENTITY_TYPE_NAME_KEY, new ConfigurationValue( "Entity Type", "The type of entity to display categories for", "" ) );
            configurationValues.Add( QUALIFIER_COLUMN_KEY, new ConfigurationValue( "Qualifier Column", "Entity column qualifier", "" ) );
            configurationValues.Add( QUALIFIER_VALUE_KEY, new ConfigurationValue( "Qualifier Value", "Entity column value", "" ) );

            if ( controls != null && controls.Count == 3 )
            {
                if ( controls[0] != null && controls[0] is DropDownList )
                    configurationValues[ENTITY_TYPE_NAME_KEY].Value = ( (DropDownList)controls[0] ).SelectedValue;

                if ( controls[1] != null && controls[1] is TextBox )
                    configurationValues[QUALIFIER_COLUMN_KEY].Value = ( (TextBox)controls[1] ).Text;

                if ( controls[2] != null && controls[2] is TextBox )
                    configurationValues[QUALIFIER_VALUE_KEY].Value = ( (TextBox)controls[2] ).Text;
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls != null && controls.Count == 3 && configurationValues != null )
            {
                if ( controls[0] != null && controls[0] is DropDownList && configurationValues.ContainsKey( ENTITY_TYPE_NAME_KEY ) )
                    ( (DropDownList)controls[0] ).SelectedValue = configurationValues[ENTITY_TYPE_NAME_KEY].Value;

                if ( controls[1] != null && controls[1] is TextBox && configurationValues.ContainsKey( QUALIFIER_COLUMN_KEY ) )
                    ( (TextBox)controls[1] ).Text = configurationValues[QUALIFIER_COLUMN_KEY].Value;

                if ( controls[2] != null && controls[2] is TextBox && configurationValues.ContainsKey( QUALIFIER_VALUE_KEY ) )
                    ( (TextBox)controls[2] ).Text = configurationValues[QUALIFIER_VALUE_KEY].Value;
            }
        }

        #endregion

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
                using ( var rockContext = new RockContext() )
                {
                    var noteType = NoteTypeCache.Read( value.AsGuid() );
                    if ( noteType != null )
                    {
                        formattedValue = noteType.Name;
                    }
                }
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );
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

            var editControl = new RockDropDownList { ID = id };
            editControl.Items.Add( new ListItem() );

            using ( var rockContext = new RockContext() )
            { 
                foreach ( var noteType in new NoteTypeService( rockContext ).Get( entityTypeId, qualifierColumn, qualifierValue ) )
                {
                    editControl.Items.Add( new ListItem( noteType.Name, noteType.Guid.ToString().ToUpper() ) );
                }
            }

            return editControl;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// return value as Category.Guid
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as DropDownList;
            if ( picker != null )
            {
                // picker has value as NoteType.Guid
                return picker.SelectedValue;
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// value is a Category.Guid string
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var picker = control as DropDownList;
            if ( picker != null )
            {
                picker.SelectedValue = value.ToUpper();
            }
        }

        #endregion

        #region Entity Methods

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            var noteType = NoteTypeCache.Read( guid );
            return noteType != null ? noteType.Id : (int?)null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var noteType = NoteTypeCache.Read( id ?? 0 );
            string guidValue = noteType != null ? noteType.Guid.ToString() : string.Empty;
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
                return new NoteTypeService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

    }
}
