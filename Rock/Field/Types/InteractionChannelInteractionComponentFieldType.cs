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
using System.Web.UI;

using Rock.Attribute;
using Rock.Data;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) component filtered by a channel
    /// Stored as "Channel.Guid|Component.Guid"
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    public class InteractionChannelInteractionComponentFieldType : FieldType
    {
        #region Keys

        /// <summary>
        /// Keys for the config values
        /// </summary>
        public static class ConfigKey
        {
            /// <summary>
            /// The default interaction channel unique identifier
            /// </summary>
            public const string DefaultInteractionChannelGuid = "DefaultInteractionChannelGuid";
        }

        #endregion Keys

        #region Configuration

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            return new List<string>
            {
                ConfigKey.DefaultInteractionChannelGuid
            };
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var channelPicker = new InteractionChannelPicker
            {
                Label = "Default Interaction Channel",
                Help = "The default interaction channel selection"
            };

            InteractionChannelPicker.LoadDropDownItems( channelPicker, true );
            return new List<Control> { channelPicker };
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            var configurationValues = new Dictionary<string, ConfigurationValue>
            {
                { ConfigKey.DefaultInteractionChannelGuid, new ConfigurationValue( "Default Interaction Channel Guid", "The default interaction channel.", string.Empty ) }
            };

            if ( controls != null && controls.Count == 1 )
            {
                var channelPicker = controls[0] as InteractionChannelPicker;

                if ( channelPicker != null )
                {
                    configurationValues[ConfigKey.DefaultInteractionChannelGuid].Value = channelPicker.SelectedValue;
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
            if ( controls != null && controls.Count == 1 && configurationValues != null && configurationValues.ContainsKey( ConfigKey.DefaultInteractionChannelGuid ) )
            {
                var channelPicker = controls[0] as InteractionChannelPicker;

                if ( channelPicker != null )
                {
                    channelPicker.SelectedValue = configurationValues[ConfigKey.DefaultInteractionChannelGuid].Value;
                }
            }
        }

        #endregion Configuration

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
            var formattedValue = string.Empty;
            GetModelsFromAttributeValue( value, out var channel, out var component );

            if ( component != null )
            {
                formattedValue = "Interaction Component: " + component.Name;
            }

            if ( channel != null )
            {
                formattedValue = "Interaction Channel: " + channel.Name;
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );
        }

        #endregion Formatting

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
            var editControl = new InteractionChannelInteractionComponentPicker { ID = id };

            if ( configurationValues != null && configurationValues.ContainsKey( ConfigKey.DefaultInteractionChannelGuid ) )
            {
                var channelGuid = configurationValues[ConfigKey.DefaultInteractionChannelGuid].Value.AsGuidOrNull();

                if ( channelGuid.HasValue )
                {
                    var channel = InteractionChannelCache.Get( channelGuid.Value );
                    editControl.DefaultInteractionChannelId = channel?.Id;
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
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var interactionChannelInteractionComponentPicker = control as InteractionChannelInteractionComponentPicker;

            if ( interactionChannelInteractionComponentPicker != null )
            {
                var rockContext = new RockContext();
                Guid? interactionChannelGuid = null;
                Guid? interactionComponentGuid = null;

                if ( interactionChannelInteractionComponentPicker.InteractionChannelId.HasValue )
                {
                    var channel = InteractionChannelCache.Get( interactionChannelInteractionComponentPicker.InteractionChannelId.Value );

                    if ( channel != null )
                    {
                        interactionChannelGuid = channel.Guid;
                    }
                }

                if ( interactionChannelInteractionComponentPicker.InteractionComponentId.HasValue )
                {
                    var component = InteractionComponentCache.Get( interactionChannelInteractionComponentPicker.InteractionComponentId.Value );

                    if ( component != null )
                    {
                        interactionComponentGuid = component.Guid;
                    }
                }

                if ( interactionChannelGuid.HasValue || interactionComponentGuid.HasValue )
                {
                    return string.Format( "{0}|{1}", interactionChannelGuid, interactionComponentGuid );
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
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var interactionChannelInteractionComponentPicker = control as InteractionChannelInteractionComponentPicker;

            if ( interactionChannelInteractionComponentPicker != null )
            {
                GetModelsFromAttributeValue( value, out var interactionChannel, out var interactionComponent );
                interactionChannelInteractionComponentPicker.InteractionChannelId = interactionChannel?.Id;
                interactionChannelInteractionComponentPicker.InteractionComponentId = interactionComponent?.Id;
            }
        }

        #endregion Edit Control

        #region Parse Helpers

        /// <summary>
        /// Gets the models from the delimited values.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="interactionChannelGuid">The channel unique identifier.</param>
        /// <param name="interactionComponentGuid">The component unique identifier.</param>
        public static void ParseDelimitedGuids( string value, out Guid? interactionChannelGuid, out Guid? interactionComponentGuid )
        {
            var parts = ( value ?? string.Empty ).Split( '|' );

            if ( parts.Length == 1 )
            {
                // If there is only one guid, assume it is the type
                interactionChannelGuid = null;
                interactionComponentGuid = parts[0].AsGuidOrNull();
                return;
            }

            interactionChannelGuid = parts.Length > 0 ? parts[0].AsGuidOrNull() : null;
            interactionComponentGuid = parts.Length > 1 ? parts[1].AsGuidOrNull() : null;
        }

        /// <summary>
        /// Gets the models from the delimited values.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="interactionChannel">The interactionChannel</param>
        /// <param name="interactionComponent">The interactionComponent</param>
        private void GetModelsFromAttributeValue( string value, out InteractionChannelCache interactionChannel, out InteractionComponentCache interactionComponent )
        {
            interactionChannel = null;
            interactionComponent = null;

            ParseDelimitedGuids( value, out var interactionChannelGuid, out var interactionComponentGuid );

            if ( interactionChannelGuid.HasValue || interactionComponentGuid.HasValue )
            {
                if ( interactionChannelGuid.HasValue )
                {
                    interactionChannel = InteractionChannelCache.Get( interactionChannelGuid.Value );
                }

                if ( interactionComponentGuid.HasValue )
                {
                    interactionComponent = InteractionComponentCache.Get( interactionComponentGuid.Value );
                }
            }
        }

        #endregion Parse Helpers
    }
}