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
    /// Field Type to select a single (or null) component filtered by a channel
    /// Stored as "Channel.Guid|Component.Guid"
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.INTERACTION_CHANNEL_INTERACTION_COMPONENT )]
    public class InteractionChannelInteractionComponentFieldType : FieldType, IEntityReferenceFieldType
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

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var configurationValues = base.GetPrivateConfigurationValues( publicConfigurationValues );

            if ( configurationValues.TryGetValue( ConfigKey.DefaultInteractionChannelGuid, out string interactionChannelJson ) )
            {
                var jsonValue = interactionChannelJson.FromJsonOrNull<ListItemBag>();

                if ( jsonValue != null )
                {
                    var interactionChannel = InteractionChannelCache.Get( jsonValue.Value.AsGuid() );
                    configurationValues[ConfigKey.DefaultInteractionChannelGuid] = interactionChannel?.Id.ToStringSafe();
                }
            }

            return configurationValues;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string value )
        {
            var configurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, value );

            if ( usage != ConfigurationValueUsage.View && configurationValues.TryGetValue( ConfigKey.DefaultInteractionChannelGuid, out string interactionChannelId ) )
            {
                var interactionChannel = InteractionChannelCache.Get( interactionChannelId.AsInteger() );

                if ( interactionChannel != null )
                {
                    configurationValues[ConfigKey.DefaultInteractionChannelGuid] = interactionChannel.ToListItemBag().ToCamelCaseJson( false, true );
                }
            }

            return configurationValues;
        }

        #endregion Configuration

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var formattedValue = string.Empty;

            GetModelsFromAttributeValue( privateValue, out var channel, out var component );

            if ( component != null )
            {
                formattedValue = "Interaction Component: " + component.Name;
            }

            if ( channel != null )
            {
                formattedValue = "Interaction Channel: " + channel.Name;
            }

            return formattedValue;
        }

        #endregion Formatting

        #region Edit Control

        #endregion Edit Control

        /// <inheritdoc />
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc />
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( !string.IsNullOrWhiteSpace( privateValue ) )
            {
                GetModelsFromAttributeValue( privateValue, out var interactionChannel, out var interactionComponent );

                var jsonValue = new JsonValue
                {
                    InteractionChannel = interactionChannel?.ToListItemBag(),
                    InteractionComponent = interactionComponent?.ToListItemBag()
                };

                return jsonValue.ToCamelCaseJson( false, true );
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var jsonValue = publicValue.FromJsonOrNull<JsonValue>();
            return jsonValue != null ? $"{jsonValue.InteractionChannel?.Value}|{jsonValue.InteractionComponent?.Value}" : string.Empty;
        }

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

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            ParseDelimitedGuids( privateValue, out var interactionChannelGuid, out var interactionComponentGuid );

            if ( !interactionChannelGuid.HasValue && !interactionComponentGuid.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var entityReferences = new List<ReferencedEntity>();

                if ( interactionChannelGuid.HasValue )
                {
                    var interactionChannelId = InteractionChannelCache.GetId( interactionChannelGuid.Value );

                    if ( interactionChannelId.HasValue )
                    {
                        entityReferences.Add( new ReferencedEntity( EntityTypeCache.GetId<InteractionChannel>().Value, interactionChannelId.Value ) );
                    }
                }

                if ( interactionComponentGuid.HasValue )
                {
                    var interactionComponentId = InteractionComponentCache.GetId( interactionComponentGuid.Value );

                    if ( interactionComponentId.HasValue )
                    {
                        entityReferences.Add( new ReferencedEntity( EntityTypeCache.GetId<InteractionComponent>().Value, interactionComponentId.Value ) );
                    }
                }

                return entityReferences;
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of InteractionChannel
            // and InteractionComponent and should have its persisted values
            // updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<InteractionChannel>().Value, nameof( InteractionChannel.Name ) ),
                new ReferencedProperty( EntityTypeCache.GetId<InteractionComponent>().Value, nameof( InteractionComponent.Name ) )
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

#endif
        #endregion

        #region Helper Classes

        /// <summary>
        /// Data sent to the obsidian client.
        /// </summary>
        private sealed class JsonValue
        {
            public ListItemBag InteractionChannel { get; set; }
            public ListItemBag InteractionComponent { get; set; }
        }

        #endregion
    }
}