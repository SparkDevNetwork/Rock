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
using System.Collections.Generic;
using System.Reflection;
using System.Web.UI;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) Entity filtered by a selected Entity Type
    /// Stored as EntityType.Guid|EntityId
    /// </summary>
    public class EntityFieldType : FieldType, IEntityFieldType
    {

        #region Configuration

        /// <summary>
        /// 
        /// </summary>
        private const string ENTITY_CONTROL_HELP_TEXT_FORMAT = "entityControlHelpTextFormat";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            configKeys.Add( ENTITY_CONTROL_HELP_TEXT_FORMAT );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            var tbHelpText = new RockTextBox();
            controls.Add( tbHelpText );
            tbHelpText.Label = "Entity Control Help Text Format";
            tbHelpText.Help = "Include a {0} in places where you want the EntityType name (Campus, Group, etc) to be included and a {1} in places where you the the pluralized EntityType name (Campuses, Groups, etc) to be included.";

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
            configurationValues.Add( ENTITY_CONTROL_HELP_TEXT_FORMAT, new ConfigurationValue( "Entity Control Help Text Format", "", "" ) );

            if ( controls != null && controls.Count == 1 )
            {
                if ( controls[0] != null && controls[0] is RockTextBox )
                {
                    configurationValues[ENTITY_CONTROL_HELP_TEXT_FORMAT].Value = ( (RockTextBox)controls[0] ).Text;
                }
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
                if ( controls[0] != null && controls[0] is RockTextBox && configurationValues.ContainsKey( ENTITY_CONTROL_HELP_TEXT_FORMAT ) )
                {
                    ( (RockTextBox)controls[0] ).Text = configurationValues[ENTITY_CONTROL_HELP_TEXT_FORMAT].Value;
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

            int? entityId = GetEntityId( value, out EntityTypeCache entityType );
            if ( entityType != null )
            {
                // Person is handled differently since it's stored as PersonAlias's EntityType.Guid|PersonAlias.Id
                // (we need to return the Person tied to the PersonAlias instance)
                if ( entityType.GetEntityType() == typeof( PersonAlias ) && entityId.HasValue )
                {
                    entityType = EntityTypeCache.Get( SystemGuid.EntityType.PERSON );
                    using ( var rockContext = new RockContext() )
                    {
                        entityId = new PersonAliasService( rockContext ).GetPersonId( entityId.Value );
                    }
                }

                formattedValue = $"{entityType.FriendlyName}|EntityId:{entityId}";
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
            var entityPicker = new EntityPicker { ID = id };
            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( ENTITY_CONTROL_HELP_TEXT_FORMAT ) )
                {
                    entityPicker.EntityControlHelpTextFormat = configurationValues[ENTITY_CONTROL_HELP_TEXT_FORMAT].Value;
                }
            }
            
            return entityPicker;
        }

        /// <summary>
        /// Reads new values entered by the user for the field ( as Guid) 
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            EntityPicker entityPicker = control as EntityPicker;
            if ( entityPicker != null && entityPicker.EntityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Get( entityPicker.EntityTypeId.Value );
                if ( entityType != null )
                {
                    return $"{entityType.Guid}|{entityPicker.EntityId}";
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
            EntityPicker entityPicker = control as EntityPicker;
            if ( entityPicker != null )
            {
                int? entityId = GetEntityId( value, out EntityTypeCache entityType );

                if ( entityType != null )
                {
                    entityPicker.EntityTypeId = entityType.Id;
                }
                else
                {
                    entityPicker.EntityTypeId = null;

                }

                entityPicker.EntityId = entityId;
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
            string editValue = GetEditValue( control, configurationValues );

            if ( string.IsNullOrEmpty( editValue ))
            {
                return null;
            }

            // we can return the EntityId itself, but it won't do the caller any good unless they already know what the EntityType is
            return GetEntityId( editValue, out _ );
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier of the entity.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            // nothing to do here, as we don't know the EntityType
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value )
        {
            return GetEntity( value, new RockContext() );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value, RockContext rockContext )
        {
            int? entityId = GetEntityId( value, out EntityTypeCache entityType );

            if ( entityType == null )
            {
                return null;
            }

            IService entityService;
            MethodInfo getMethod;
            var methodParamTypes = new Type[] { typeof( int ) };

            // Person is handled differently since it's stored as PersonAlias's EntityType.Guid|PersonAlias.Id
            // (we need to return the Person tied to the PersonAlias instance)
            if ( entityType.GetEntityType() == typeof( PersonAlias ) )
            {
                entityService = new PersonAliasService( rockContext );
                getMethod = entityService.GetType().GetMethod( "GetPerson", methodParamTypes );
            }
            else
            {
                entityService = Reflection.GetServiceForEntityType( entityType.GetEntityType(), rockContext );
                getMethod = entityService.GetType().GetMethod( "Get", methodParamTypes );
            }

            return ( IEntity ) getMethod.Invoke( entityService, new object[] { entityId } );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the entity identifier, and also returns the EntityType as an out param.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        private int? GetEntityId( string value, out EntityTypeCache entityType )
        {
            entityType = null;

            string[] values = ( value ?? string.Empty ).Split( '|' );
            if ( values.Length != 2 )
            {
                return null;
            }

            Guid? entityTypeGuid = values[0].AsGuidOrNull();
            if ( !entityTypeGuid.HasValue )
            {
                return null;
            }

            entityType = EntityTypeCache.Get( entityTypeGuid.Value );

            return values[1].AsIntegerOrNull();
        }

        #endregion
    }
}