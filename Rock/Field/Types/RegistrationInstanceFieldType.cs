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
using System.Data.Entity;
using System.Linq;
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
    /// Field Type to select a single (or null) registration instance filtered by a selected registration template
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.System )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.REGISTRATION_INSTANCE )]
    public class RegistrationInstanceFieldType : FieldType, IEntityFieldType, IEntityReferenceFieldType
    {
        #region Configuration

        /// <summary>
        /// Configuration Key for Registration Template
        /// </summary>
        public static readonly string REGISTRATION_TEMPLATE_KEY = "registrationtemplate";

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string value, Dictionary<string, string> configurationValues )
        {
            string formattedValue = string.Empty;

            Guid guid = Guid.Empty;
            if ( Guid.TryParse( value, out guid ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    var registrationInstance = new RegistrationInstanceService( rockContext ).GetNoTracking( guid );
                    if ( registrationInstance != null )
                    {
                        formattedValue = registrationInstance.Name;
                    }
                }
            }
            return formattedValue;
        }

        #endregion

        #region Edit Control

        /// <inheritdoc />
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc />
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guid = privateValue.AsGuidOrNull();

            if ( guid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var registrationTemplate = new RegistrationInstanceService( rockContext ).GetSelect( guid.Value, r => new ListItemBag()
                    {
                        Text = r.Name,
                        Value = r.Guid.ToString()
                    } );

                    return registrationTemplate.ToCamelCaseJson( false, true );
                }
            }

            return base.GetPublicEditValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc />
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var jsonValue = publicValue.FromJsonOrNull<ListItemBag>();

            if ( jsonValue != null )
            {
                return jsonValue.Value;
            }

            return base.GetPrivateEditValue( publicValue, privateConfigurationValues );
        }

        /// <inheritdoc />
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string value )
        {
            var configurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, value );

            if ( usage != ConfigurationValueUsage.View && configurationValues.ContainsKey( REGISTRATION_TEMPLATE_KEY ) )
            {
                var id = configurationValues[REGISTRATION_TEMPLATE_KEY].AsIntegerOrNull();
                if ( id.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var registrationTemplate = new RegistrationTemplateService( rockContext ).GetSelect( id.Value, r => new ListItemBag()
                        {
                            Text = r.Name,
                            Value = r.Guid.ToString()
                        } );

                        configurationValues[REGISTRATION_TEMPLATE_KEY] = registrationTemplate.ToCamelCaseJson( false, true );
                    }
                }
            }

            return configurationValues;
        }

        /// <inheritdoc />
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var configurationValues = base.GetPrivateConfigurationValues( publicConfigurationValues );

            if ( configurationValues.ContainsKey( REGISTRATION_TEMPLATE_KEY ) )
            {
                var jsonValue = configurationValues[REGISTRATION_TEMPLATE_KEY].FromJsonOrNull<ListItemBag>();
                if ( jsonValue != null && Guid.TryParse( jsonValue.Value, out Guid guid ) )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var registrationTemplate = new RegistrationTemplateService( rockContext ).GetId( guid );
                        configurationValues[REGISTRATION_TEMPLATE_KEY] = registrationTemplate.ToString();
                    }
                }
            }

            return configurationValues;
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
                return new RegistrationInstanceService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            Guid? guid = privateValue.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var registrationInstanceId = new RegistrationInstanceService( rockContext ).GetId( guid.Value );

                if ( !registrationInstanceId.HasValue )
                {
                    return null;
                }

                return new List<ReferencedEntity>
                {
                    new ReferencedEntity( EntityTypeCache.GetId<RegistrationInstance>().Value, registrationInstanceId.Value )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a Registration Instance and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<RegistrationInstance>().Value, nameof( RegistrationInstance.Name ) )
            };
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
            configKeys.Add( REGISTRATION_TEMPLATE_KEY );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // build a drop down list of defined types (the one that gets selected is
            // used to build a list of defined values) 
            var rtp = new RegistrationTemplatePicker();
            controls.Add( rtp );
            rtp.SelectItem += OnQualifierUpdated;
            rtp.Label = "Registration Template";
            rtp.Help = "Registration Template to select items from, if left blank any registration template's instance can be selected.";

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
            configurationValues.Add( REGISTRATION_TEMPLATE_KEY, new ConfigurationValue( "Registration Template", "Registration Template to select items from, if left blank any registration template's instance can be selected.", "" ) );

            if ( controls != null && controls.Count == 1 )
            {
                if ( controls[0] != null && controls[0] is RegistrationTemplatePicker )
                {
                    configurationValues[REGISTRATION_TEMPLATE_KEY].Value = ( ( RegistrationTemplatePicker ) controls[0] ).SelectedValue;
                }
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="configurationValues">The configuration values.</param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls != null && controls.Count == 1 && configurationValues != null )
            {
                if ( controls[0] != null && controls[0] is RegistrationTemplatePicker && configurationValues.ContainsKey( REGISTRATION_TEMPLATE_KEY ) )
                {
                    ( ( RegistrationTemplatePicker ) controls[0] ).SetValue( configurationValues[REGISTRATION_TEMPLATE_KEY].Value.AsIntegerOrNull() );
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
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            RegistrationInstancePicker editControl = new RegistrationInstancePicker { ID = id };

            if ( configurationValues != null && configurationValues.ContainsKey( REGISTRATION_TEMPLATE_KEY ) )
            {
                int registionTemplateId = 0;
                if ( Int32.TryParse( configurationValues[REGISTRATION_TEMPLATE_KEY].Value, out registionTemplateId ) && registionTemplateId > 0 )
                {
                    editControl.RegistrationTemplateId = registionTemplateId;
                }
            }

            return editControl;
        }

        /// <summary>
        /// Reads new values entered by the user for the field ( as Guid )
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as RegistrationInstancePicker;
            if ( picker != null )
            {
                int? itemId = picker.RegistrationInstanceId;
                Guid? itemGuid = null;
                if ( itemId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        itemGuid = new RegistrationInstanceService( rockContext ).Queryable().AsNoTracking().Where( a => a.Id == itemId.Value ).Select( a => ( Guid? ) a.Guid ).FirstOrDefault();
                    }
                }

                return itemGuid?.ToString() ?? string.Empty;
            }

            return null;
        }

        /// <summary>
        /// Sets the value. ( as Guid )
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var picker = control as RegistrationInstancePicker;
            if ( picker != null )
            {
                int? itemId = null;
                Guid? itemGuid = value.AsGuidOrNull();
                if ( itemGuid.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        itemId = new RegistrationInstanceService( rockContext ).Queryable().Where( a => a.Guid == itemGuid.Value ).Select( a => ( int? ) a.Id ).FirstOrDefault();
                    }
                }

                picker.RegistrationInstanceId = itemId;
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
            var item = new RegistrationInstanceService( new RockContext() ).Get( guid );
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
            var item = new RegistrationInstanceService( new RockContext() ).Get( id ?? 0 );
            string guidValue = item != null ? item.Guid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }

#endif
        #endregion
    }
}