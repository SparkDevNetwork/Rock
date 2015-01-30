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

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// returns EntityType.Guid
    /// </summary>
    [Serializable]
    public class EntityTypeFieldType : FieldType, IEntityFieldType
    {

        #region Configuration

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            configKeys.Add( "includeglobal" );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            var cb = new RockCheckBox();
            controls.Add( cb );
            cb.Label = "Include Global Attributes Option";
            cb.Text = "Yes";
            cb.Help = "Should the 'Global Attributes' entity option be included.";
            cb.CheckedChanged += OnQualifierUpdated;

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
            configurationValues.Add( "includeglobal", new ConfigurationValue( "Include Global Option",
                "Should the 'Global Attributes' entity option be included.", "" ) );

            if ( controls != null && controls.Count == 1 )
            {
                if ( controls[0] != null && controls[0] is RockCheckBox )
                    configurationValues["includeglobal"].Value = ( (RockCheckBox)controls[0] ).Checked.ToString();

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
            if ( controls != null && controls.Count == 1 && configurationValues != null )
            {
                if ( controls[0] != null && controls[0] is RockCheckBox && configurationValues.ContainsKey( "includeglobal" ) )
                {
                    var cb = (RockCheckBox)controls[0];
                    cb.Checked = true;

                    bool includeGlobal = false;
                    if ( configurationValues.ContainsKey( "includeglobal" ) &&
                        bool.TryParse( configurationValues["includeglobal"].Value, out includeGlobal ) &&
                        !includeGlobal )
                    {
                        cb.Checked = false;
                    }
                }
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

            Guid guid = Guid.Empty;
            if ( Guid.TryParse( value, out guid ) )
            {
                var entityType = EntityTypeCache.Read( guid );
                if ( entityType != null )
                {
                    formattedValue = entityType.FriendlyName;
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
            var entityTypePicker = new EntityTypePicker { ID = id };
            entityTypePicker.IncludeGlobalOption = true;

            if ( configurationValues != null )
            {
                bool includeGlobal = false;
                if ( configurationValues.ContainsKey( "includeglobal" ) && 
                    bool.TryParse(configurationValues["includeglobal"].Value, out includeGlobal) &&
                    !includeGlobal)
                {
                    entityTypePicker.IncludeGlobalOption = false;
                }
            }

            entityTypePicker.EntityTypes = new EntityTypeService( new RockContext() ).GetEntities().ToList();
            return entityTypePicker;
        }

        /// <summary>
        /// Reads new values entered by the user for the field ( as Guid) 
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            EntityTypePicker entityTypePicker = control as EntityTypePicker;
            if ( entityTypePicker != null && entityTypePicker.SelectedEntityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Read(entityTypePicker.SelectedEntityTypeId.Value);
                if (entityType != null)
                {
                    return entityType.Guid.ToString();
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the value ( as Guid )
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            Guid guid = Guid.Empty;
            if (Guid.TryParse(value, out guid))
            {
                EntityTypePicker entityTypePicker = control as EntityTypePicker;
                if ( entityTypePicker != null )
                {
                    int selectedValue = 0;
                    var entityType = EntityTypeCache.Read(guid);
                    if (entityType != null)
                    {
                        selectedValue = entityType.Id;
                    }
                    entityTypePicker.SelectedEntityTypeId = selectedValue;
                }
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
            var item = new EntityTypeService( new RockContext() ).Get( guid );
            return item != null ? item.Id : (int?)null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var item = new EntityTypeService( new RockContext() ).Get( id ?? 0 );
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
                return new EntityTypeService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

    }
}