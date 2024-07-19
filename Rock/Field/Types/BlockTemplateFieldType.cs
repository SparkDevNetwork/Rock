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
    /// Field Type to select a template to use in a block
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.BLOCK_TEMPLATE )]
    public class BlockTemplateFieldType : FieldType, IEntityReferenceFieldType
    {
        #region Configuration

        /// <summary>
        /// The template block key
        /// </summary>
        public static readonly string TEMPLATE_BLOCK_KEY = "templateblock";

        private static readonly Guid _CustomGuid = new Guid( "ffffffff-ffff-ffff-ffff-ffffffffffff" );

        /// <inheritdoc />
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string value )
        {
            var configurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, value );

            if ( usage != ConfigurationValueUsage.View && configurationValues.TryGetValue( TEMPLATE_BLOCK_KEY, out string templateBlockGuidString ) && Guid.TryParse( templateBlockGuidString, out Guid templateBlockGuid ) )
            {
                var definedValue = DefinedValueCache.Get( templateBlockGuid );

                if ( definedValue != null )
                {
                    configurationValues[TEMPLATE_BLOCK_KEY] = definedValue.ToListItemBag().ToCamelCaseJson( false, true );
                }
            }

            return configurationValues;
        }

        /// <inheritdoc />
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var configurationValues = base.GetPrivateConfigurationValues( publicConfigurationValues );

            if ( configurationValues.TryGetValue( TEMPLATE_BLOCK_KEY, out string templateBlockJsonString ) )
            {
                var jsonValue = templateBlockJsonString.FromJsonOrNull<ListItemBag>();

                if ( jsonValue != null )
                {
                    configurationValues[TEMPLATE_BLOCK_KEY] = jsonValue.Value;
                }
            }

            return configurationValues;
        }

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( TryGetValueParts( privateValue, out Guid? templateGuid, out string templateValue ) )
            {
                if ( templateGuid.HasValue )
                {
                    if ( templateGuid.Value == _CustomGuid )
                    {
                        return "Template: Custom";
                    }
                    else
                    {
                        var definedValue = DefinedValueCache.Get( templateGuid.Value );
                        if ( definedValue != null )
                        {
                            return "Template: " + definedValue.Value;
                        }
                    }
                }
            }

            return string.Empty;
        }

        private bool TryGetValueParts( string privateValue, out Guid? templateGuid, out string templateValue )
        {
            string[] parts = ( privateValue ?? string.Empty ).Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
            if ( parts.Length > 0 )
            {
                templateGuid = parts[0].AsGuidOrNull();
                templateValue = parts.Length > 1 ? parts[1] : null;
            }
            else
            {
                templateGuid = null;
                templateValue = null;
            }

            return templateGuid.HasValue || !string.IsNullOrWhiteSpace( templateValue );
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
            string[] parts = ( privateValue ?? string.Empty ).Split( new[] { '|' }, 2 );
            if ( parts.Length >= 1 )
            {
                var templateGuid = parts[0];
                if ( templateGuid == _CustomGuid.ToString() )
                {
                    templateGuid = Guid.Empty.ToString();
                    if ( parts.Length >= 2 )
                    {
                        return $"{templateGuid}|{parts[1]}";
                    }
                }
                else
                {
                    return $"{templateGuid}|";
                }
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            string[] parts = ( publicValue ?? string.Empty ).Split( new[] { '|' }, 2 );
            if ( parts.Length >= 1 )
            {
                var templateGuid = parts[0];
                if ( templateGuid == Guid.Empty.ToString() )
                {
                    templateGuid = _CustomGuid.ToString();
                    if ( parts.Length >= 2 )
                    {
                        return $"{templateGuid}|{parts[1]}";
                    }
                }
                else
                {
                    return $"{templateGuid}|";
                }
            }

            return string.Empty;
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( TryGetValueParts( privateValue, out Guid? templateGuid, out string templateValue ) )
            {
                if ( templateGuid.HasValue && !templateGuid.Value.IsEmpty() )
                {
                    var definedValue = DefinedValueCache.Get( templateGuid.Value );

                    if ( definedValue != null )
                    {
                        return new List<ReferencedEntity>
                        {
                            new ReferencedEntity( EntityTypeCache.GetId<DefinedValue>().Value, definedValue.Id )
                        };
                    }
                }
            }

            return null;
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<DefinedValue>().Value, nameof( DefinedValue.Value ) )
            };
        }

        #endregion

        /// <summary>
        /// Gets the template value from either the pre-defined template or the custom template content.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The content of the selected template.</returns>
        public static string GetTemplateContent( string value )
        {
            var values = value.Split( new[] { '|' }, 2 );

            if ( values.Length >= 1 )
            {
                if ( values[0].AsGuid() == _CustomGuid && values.Length >= 2 )
                {
                    return values[1];
                }
                else
                {
                    return DefinedValueCache.Get( values[0].AsGuid() )?.Description ?? string.Empty;
                }
            }

            return string.Empty;
        }

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( TEMPLATE_BLOCK_KEY );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            var dvpTemplateBlock = new DefinedValuePicker();
            dvpTemplateBlock.DisplayDescriptions = true;
            controls.Add( dvpTemplateBlock );
            dvpTemplateBlock.AutoPostBack = true;
            dvpTemplateBlock.SelectedIndexChanged += OnQualifierUpdated;

            var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.TEMPLATE_BLOCK.AsGuid() );
            dvpTemplateBlock.DefinedTypeId = definedType?.Id;
            dvpTemplateBlock.Label = "Template Block";
            dvpTemplateBlock.Help = "An optional setting to select template block from.";

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
            configurationValues.Add( TEMPLATE_BLOCK_KEY, new ConfigurationValue( "Template Block", "Type of Template Block to select.", "" ) );

            if ( controls != null && controls.Count == 1 )
            {
                if ( controls[0] != null && controls[0] is DefinedValuePicker )
                {
                    int? definedValueId = ( ( DefinedValuePicker ) controls[0] ).SelectedValueAsInt();
                    if ( definedValueId.HasValue )
                    {
                        var definedValue = DefinedValueCache.Get( definedValueId.Value );
                        if ( definedValue != null )
                        {
                            configurationValues[TEMPLATE_BLOCK_KEY].Value = definedValue.Guid.ToString();
                        }
                    }
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
                if ( controls[0] != null && controls[0] is DefinedValuePicker && configurationValues.ContainsKey( TEMPLATE_BLOCK_KEY ) )
                {
                    Guid? definedValueGuid = configurationValues[TEMPLATE_BLOCK_KEY].Value.AsGuidOrNull();
                    if ( definedValueGuid.HasValue )
                    {
                        var definedValue = DefinedValueCache.Get( definedValueGuid.Value );
                        if ( definedValue != null )
                        {
                            ( ( DefinedValuePicker ) controls[0] ).SetValue( definedValue.Id.ToString() );
                        }
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
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            BlockTemplatePicker editControl = new BlockTemplatePicker { ID = id };

            if ( configurationValues != null && configurationValues.ContainsKey( TEMPLATE_BLOCK_KEY ) )
            {
                Guid blockTemplateDefinedValueGuid = Guid.Empty;
                if ( Guid.TryParse( configurationValues[TEMPLATE_BLOCK_KEY].Value, out blockTemplateDefinedValueGuid ) && blockTemplateDefinedValueGuid != Guid.Empty )
                {
                    var definedValue = DefinedValueCache.Get( blockTemplateDefinedValueGuid );
                    if ( definedValue != null )
                    {
                        editControl.TemplateBlockValueId = definedValue.Id;
                    }
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
            BlockTemplatePicker blockTemplatePicker = control as BlockTemplatePicker;
            if ( blockTemplatePicker != null )
            {
                if ( blockTemplatePicker.TemplateKey.HasValue )
                {
                    return string.Format( "{0}|{1}", blockTemplatePicker.TemplateKey, blockTemplatePicker.TemplateValue );
                }
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
            BlockTemplatePicker blockTemplatePicker = control as BlockTemplatePicker;
            if ( blockTemplatePicker != null )
            {
                // initialize in case the value isn't set
                blockTemplatePicker.TemplateKey = null;
                blockTemplatePicker.TemplateValue = string.Empty;

                string[] parts = ( value ?? string.Empty ).Split( new[] { '|' }, 2 );
                if ( parts.Length >= 1 )
                {
                    var templateGuid = parts[0].AsGuid();
                    if ( templateGuid == _CustomGuid )
                    {
                        blockTemplatePicker.TemplateKey = templateGuid;
                        if ( parts.Length >= 2 )
                        {
                            blockTemplatePicker.TemplateValue = parts[1];
                        }
                    }
                    else
                    {
                        var definedValue = DefinedValueCache.Get( templateGuid );
                        if ( definedValue != null )
                        {
                            blockTemplatePicker.TemplateKey = definedValue.Guid;
                        }
                    }
                }
            }
        }

#endif
        #endregion
    }
}