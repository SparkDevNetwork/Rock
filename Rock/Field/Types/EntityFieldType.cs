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
using System.Linq;
using System.Reflection;
#if WEBFORMS
using System.Web.UI;
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) Entity filtered by a selected Entity Type
    /// Stored as EntityType.Guid|EntityId
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.ENTITY )]
    public class EntityFieldType : FieldType, IEntityFieldType
    {

        #region Configuration

        /// <summary>
        /// Configuration value for the help text displayed by the picker
        /// </summary>
        private const string ENTITY_CONTROL_HELP_TEXT_FORMAT = "entityControlHelpTextFormat";

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var entityId = GetEntityIdentifier( privateValue, out EntityTypeCache entityType ).AsIntegerOrNull();

            if ( !entityId.HasValue )
            {
                return string.Empty;
            }

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

            return $"{entityType.FriendlyName}|EntityId:{entityId}";
        }

        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var publicValue = string.Empty;
            var entity = GetEntity( privateValue, out EntityTypeCache entityType );

            if ( entityType != null )
            {
                var fieldType = entityType.SingleValueFieldType;
                if ( fieldType != null )
                {
                    // Use the Entity's field type to get the PublicEditValue since the obsidian EntityPicker uses the
                    // Entity's field type to render the right control.
                    var field = fieldType.Field;
                    publicValue = field.GetPublicEditValue( entity?.Guid.ToStringSafe(), new Dictionary<string, string>() );
                }
            }

            return new EntityFieldValue()
            {
                EntityType = entityType.ToListItemBag(),
                Value = publicValue
            }.ToCamelCaseJson( false, true );
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            // GetPrivateEditValue does not use the entity's fieldType's GetPrivateEditValue because most of the field types
            // return/save the guid value as the private value, however the entityType guid combined with the int Id value is
            // what is required in this instance.

            var entityValue = publicValue.FromJsonOrNull<EntityFieldValue>();

            if ( entityValue != null )
            {
                var jsonValue = entityValue.Value.FromJsonOrNull<ListItemBag>();

                // Some EntityTypes return their Entity value as a ListItemBag, and others return just the guid value (Campus)
                // or a string value. If it is a ListItemBag json we are interested in the actual value at this point.
                if ( jsonValue != null )
                {
                    entityValue.Value = jsonValue.Value;
                }

                // Webforms EntityPicker saves the EntityType Guid along with the Entity Id, so we use the Guid returned from the
                // client to get the EntityId for backwards compatibility.
                var privateValue = $"{entityValue.EntityType?.Value}|{entityValue.Value}";
                var entity = GetEntity( privateValue, out _ );

                return $"{entityValue.EntityType?.Value}|{entity?.Id}";
            }

            return base.GetPrivateEditValue( publicValue, privateConfigurationValues );
        }

        #endregion

        #region Entity Methods

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
            var entityIdentifier = GetEntityIdentifier( value, out EntityTypeCache entityType );

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

            return ( IEntity ) getMethod.Invoke( entityService, new object[] { entityIdentifier } );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the entity identifier as a Guid or Int string, and also returns the EntityType as an out param.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        private string GetEntityIdentifier( string value, out EntityTypeCache entityType )
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

            return values[1];
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        private IEntity GetEntity( string value, out EntityTypeCache entityType )
        {
            var entityIdentifier = GetEntityIdentifier( value, out entityType );

            if ( entityType == null )
            {
                return null;
            }

            IService entityService;
            MethodInfo getMethod;
            object[] parameters;
            Type[] methodParamTypes;
            RockContext rockContext = new RockContext();

            if ( entityIdentifier.AsIntegerOrNull().HasValue )
            {
                methodParamTypes = new Type[] { typeof( int ) };
                parameters = new object[] { entityIdentifier.AsIntegerOrNull() };

                if ( entityType.GetEntityType() == typeof( Person ) )
                {
                    entityService = new PersonAliasService( rockContext );
                    getMethod = entityService.GetType().GetMethod( "GetByAliasId", methodParamTypes );
                }
                else
                {
                    entityService = Reflection.GetServiceForEntityType( entityType.GetEntityType(), rockContext );
                    getMethod = entityService.GetType().GetMethod( "Get", methodParamTypes );
                }
            }
            else
            {
                methodParamTypes = new Type[] { typeof( Guid ) };
                parameters = new object[] { entityIdentifier.AsGuidOrNull() };

                if ( entityType.GetEntityType() == typeof( Person ) )
                {
                    entityService = new PersonAliasService( rockContext );
                    getMethod = entityService.GetType().GetMethod( "GetPerson", methodParamTypes );
                }
                else
                {
                    entityService = Reflection.GetServiceForEntityType( entityType.GetEntityType(), rockContext );
                    getMethod = entityService.GetType().GetMethod( "Get", methodParamTypes );
                }
            }

            return ( IEntity ) getMethod.Invoke( entityService, parameters );
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
            tbHelpText.Help = "Include a {0} in places where you want the EntityType name (Campus, Group, etc) to be included and a {1} in places where you want the pluralized EntityType name (Campuses, Groups, etc) to be included.";

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
                    configurationValues[ENTITY_CONTROL_HELP_TEXT_FORMAT].Value = ( ( RockTextBox ) controls[0] ).Text;
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
                    ( ( RockTextBox ) controls[0] ).Text = configurationValues[ENTITY_CONTROL_HELP_TEXT_FORMAT].Value;
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
                int? entityId = GetEntityIdentifier( value, out EntityTypeCache entityType ).AsIntegerOrNull();

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

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            string editValue = GetEditValue( control, configurationValues );

            if ( string.IsNullOrEmpty( editValue ) )
            {
                return null;
            }

            // we can return the EntityId itself, but it won't do the caller any good unless they already know what the EntityType is
            return GetEntityIdentifier( editValue, out _ ).AsIntegerOrNull();
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

#endif
        #endregion

        #region Helper Class

        /// <summary>
        /// Helper class for the EntityField value.
        /// </summary>
        private sealed class EntityFieldValue
        {
            /// <summary>
            /// Gets or sets the entity value.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public string Value { get; set; }

            /// <summary>
            /// Gets or sets the type of the entity.
            /// </summary>
            /// <value>
            /// The type of the entity.
            /// </value>
            public ListItemBag EntityType { get; set; }
        }

        #endregion
    }
}