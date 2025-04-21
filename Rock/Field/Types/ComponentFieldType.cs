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
using System.Web.UI.WebControls;
#endif

using Rock.Attribute;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of MEF Components of a specific type
    /// Stored as EntityType.Guid
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.System )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.COMPONENT )]
    public class ComponentFieldType : FieldType
    {
        #region Configuration

        private const string Container = "container";

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( privateValue.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            var entityTypeGuid = privateValue.AsGuid();

            if ( entityTypeGuid != Guid.Empty )
            {
                var entityType = EntityTypeCache.Get( entityTypeGuid );

                if ( entityType != null )
                {
                    return entityType.FriendlyName;
                }
            }

            return string.Empty;
        }

        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc />
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( privateConfigurationValues.ContainsKey( Container ) )
            {
                var containerType = privateConfigurationValues[Container];
                var guid = privateValue.AsGuidOrNull();

                if ( guid.HasValue && !string.IsNullOrWhiteSpace( containerType ) )
                {
                    var resolvedContainerType = Rock.Utility.Container.ResolveContainer( containerType );

                    if ( resolvedContainerType != null )
                    {
                        var instanceProperty = resolvedContainerType.GetProperty( "Instance" );

                        if ( instanceProperty != null )
                        {
                            var container = instanceProperty.GetValue( null, null ) as Rock.Extension.IContainer;
                            var componentDictionary = container?.Dictionary;

                            var component = componentDictionary.FirstOrDefault( c => c.Value.Value.TypeGuid == guid );

                            var componentName = component.Value.Key;

                            // If the component name already has a space then trust
                            // that they are using the exact name formatting they want.
                            if ( componentName.IsNotNullOrWhiteSpace() && !componentName.Contains( ' ' ) )
                            {
                                componentName = componentName.SplitCase();
                            }

                            return new ListItemBag
                            {
                                Text = componentName,
                                Value = privateValue.ToUpper()
                            }.ToCamelCaseJson( false, true );
                        }
                    }
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

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( "container" );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            var tb = new RockTextBox();
            controls.Add( tb );
            tb.AutoPostBack = true;
            tb.TextChanged += OnQualifierUpdated;
            tb.Label = "Container Assembly Name";
            tb.Help = "The assembly name of the MEF container to show components of.";
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
            configurationValues.Add( "container", new ConfigurationValue( "Container Assembly Name", "The assembly name of the MEF container to show components of", "" ) );

            if ( controls != null && controls.Count == 1 &&
                controls[0] != null && controls[0] is TextBox )
            {
                configurationValues["container"].Value = ( ( TextBox ) controls[0] ).Text;
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
            if ( controls != null && controls.Count == 1 && configurationValues != null &&
                controls[0] != null && controls[0] is TextBox && configurationValues.ContainsKey( "container" ) )
            {
                ( ( TextBox ) controls[0] ).Text = configurationValues["container"].Value;
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
            try
            {
                ComponentPicker editControl = new ComponentPicker { ID = id };

                if ( configurationValues != null && configurationValues.ContainsKey( "container" ) )
                {
                    editControl.ContainerType = configurationValues["container"].Value;
                }

                return editControl;
            }
            catch ( SystemException ex )
            {
                return new LiteralControl( ex.Message );
            }
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as ComponentPicker;
            if ( picker != null )
            {
                // NOTE: ComponentPicker uses the Entity.Guid as the ListItem value
                var guid = picker.SelectedValue.AsGuidOrNull();
                if ( guid.HasValue )
                {
                    return guid.Value.ToString();
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var picker = control as ComponentPicker;
            if ( picker != null )
            {
                Guid? guid = value.AsGuidOrNull();
                picker.SetValue( guid );
            }
        }

#endif
        #endregion
    }
}