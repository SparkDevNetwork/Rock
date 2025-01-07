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
using System.Linq;
#if WEBFORMS
using System.Web.UI;
#endif

using Rock.Attribute;
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
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.ENTITYTYPE )]
    public class EntityTypeFieldType : FieldType, IEntityFieldType
    {
        /*
         * 8/5/2022 - DSH
         * 
         * Note: We don't track changes to EntityType because that almost never
         * happens and when it does it is extremely early in the Rock startup
         * process so there is concern this might cause sporadic startup
         * failures. Since the nightly job will get these fixed up if they are
         * out of sync I decided not to track changes. If testing is performed
         * and proven safe then we could later start tracking changes.
         */

        #region Configuration

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guid = privateValue.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return string.Empty;
            }

            return EntityTypeCache.Get( guid.Value )?.FriendlyName ?? string.Empty;
        }

        #endregion

        #region Edit Control

        #endregion

        #region Entity Methods

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

        #region WebForms
#if WEBFORMS

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
                    configurationValues["includeglobal"].Value = ( ( RockCheckBox ) controls[0] ).Checked.ToString();

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
                    var cb = ( RockCheckBox ) controls[0];
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
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

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
                    bool.TryParse( configurationValues["includeglobal"].Value, out includeGlobal ) &&
                    !includeGlobal )
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
                if ( entityTypePicker.SelectedEntityTypeId == 0 && entityTypePicker.IncludeGlobalOption )
                {
                    return Guid.Empty.ToString();
                }
                else
                {
                    var entityType = EntityTypeCache.Get( entityTypePicker.SelectedEntityTypeId.Value );
                    if ( entityType != null )
                    {
                        return entityType.Guid.ToString();
                    }
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
            EntityTypePicker entityTypePicker = control as EntityTypePicker;
            if ( entityTypePicker != null )
            {
                EntityTypeCache entityType = null;
                Guid? guid = value.AsGuidOrNull();
                if ( guid.HasValue )
                {
                    entityType = EntityTypeCache.Get( guid.Value );

                    // If the guid had a value, but the EntityType is null, it's probably the "None (Global Attributes)" entity.
                    if ( entityType == null && entityTypePicker.IncludeGlobalOption )
                    {
                        entityTypePicker.SelectedEntityTypeId = 0;
                    }
                    else
                    {
                        entityTypePicker.SelectedEntityTypeId = entityType?.Id;
                    }
                }
            }
        }

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
            return item != null ? item.Id : ( int? ) null;
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

#endif
        #endregion

    }
}