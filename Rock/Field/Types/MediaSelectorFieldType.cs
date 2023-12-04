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
using System.Web.UI;
using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Controls;
using Rock.Media;
using Rock.Model;
using Rock.SystemGuid;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    ///  Media Selector Field Type
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.MEDIA_SELECTOR )]
    public class MediaSelectorFieldType : FieldType, ILinkableFieldType
    {
        #region Configuration

        private const string MODE_TYPE = "modetype";
        private const string ITEM_WIDTH = "itemwidth";
        private const string MEDIA_ITEMS = "mediaitems";

        #endregion

        #region Configuration

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string privateValue )
        {
            var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, privateValue );

            if ( privateConfigurationValues.ContainsKey( MEDIA_ITEMS ) )
            {
                var mediaItems = privateConfigurationValues[MEDIA_ITEMS];
                if ( mediaItems.IsNotNullOrWhiteSpace() )
                {
                    var mediaItemsPublicValue = new KeyValueListFieldType().GetPublicEditValue( mediaItems, new Dictionary<string, string>() );
                    publicConfigurationValues[MEDIA_ITEMS] = mediaItemsPublicValue;
                }
            }

            return publicConfigurationValues;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var privateConfigurationValues = base.GetPrivateConfigurationValues( publicConfigurationValues );

            if ( publicConfigurationValues.ContainsKey( MEDIA_ITEMS ) )
            {
                var mediaItems = privateConfigurationValues[MEDIA_ITEMS];
                if ( mediaItems.IsNotNullOrWhiteSpace() )
                {
                    var mediaItemsPrivateValue = new KeyValueListFieldType().GetPrivateEditValue( mediaItems, new Dictionary<string, string>() );
                    privateConfigurationValues[MEDIA_ITEMS] = mediaItemsPrivateValue;
                }
            }

            return privateConfigurationValues;
        }

        #endregion

        #region Formatting

        /// <inheritdoc />
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetPublicEditValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc />
        public override string GetHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var url = string.Empty;
            if ( privateValue.IsNotNullOrWhiteSpace() )
            {
                var keyValuePair = privateValue.Split( new char[] { '^' } );
                if ( keyValuePair.Length == 2 )
                {
                    url = keyValuePair[1];
                }
            }

            return url;
        }

        /// <inheritdoc/>
        public override string GetCondensedTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            // Don't truncate the value.
            return GetTextValue( privateValue, privateConfigurationValues );
        }


        /// <inheritdoc/>
        public override string GetCondensedHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            // Don't truncate the value.
            return GetHtmlValue( privateValue, privateConfigurationValues );
        }

        /// <summary>
        /// Formats the value extended.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public string UrlLink( string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var url = string.Empty;
            if ( value.IsNotNullOrWhiteSpace() )
            {
                if ( configurationValues.ContainsKey( MEDIA_ITEMS ) )
                {
                    string[] keyValuePair = value.Split( new char[] { '^' } );
                    if ( keyValuePair.Length == 2 )
                    {
                        url = keyValuePair[1];
                    }
                }
            }

            return url;
        }


        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetPublicEditValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( publicValue.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            var customValues = privateConfigurationValues[MEDIA_ITEMS].AsDictionary();

            // If there are any custom values, then ensure that all values we
            // got from the public device are valid. If not, ignore them.
            if ( customValues.Any() && customValues.ContainsKey( publicValue ) )
            {
                return $"{publicValue}^{customValues[publicValue]}";
            }
            else
            {
                return string.Empty;
            }
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var publicValue = string.Empty;
            if ( privateValue.IsNotNullOrWhiteSpace() )
            {
                var keyValuePair = privateValue.Split( new char[] { '^' } );
                if ( keyValuePair.Length == 2 )
                {
                    publicValue = keyValuePair[0];
                }
            }

            return publicValue;
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
            configKeys.Add( MODE_TYPE );
            configKeys.Add( ITEM_WIDTH );
            configKeys.Add( MEDIA_ITEMS );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            var ddlModeType = new RockDropDownList();
            ddlModeType.BindToEnum<MediaSelectorMode>();
            ddlModeType.AutoPostBack = true;
            ddlModeType.SelectedIndexChanged += OnQualifierUpdated;
            ddlModeType.Label = "Mode";
            controls.Add( ddlModeType );

            var tbItemWidth = new RockTextBox();
            tbItemWidth.Label = "Item Width";
            tbItemWidth.Help = "The width of each media item in pixels or percentage.";
            tbItemWidth.AutoPostBack = true;
            tbItemWidth.TextChanged += OnQualifierUpdated;
            controls.Add( tbItemWidth );

            var kvlMediaItems = new KeyValueList();
            kvlMediaItems.Label = "Media Items";
            kvlMediaItems.Help = "The items to display. The key will be the name of the item and the value should be the URL to the media file.";
            kvlMediaItems.ValueChanged += OnQualifierUpdated;
            controls.Add( kvlMediaItems );

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
            configurationValues.Add( MODE_TYPE, new ConfigurationValue( "Mode", "", "" ) );
            configurationValues.Add( ITEM_WIDTH, new ConfigurationValue( "Item Width", "The width of each media item in pixels or percentage.", "" ) );
            configurationValues.Add( MEDIA_ITEMS, new ConfigurationValue( "Media Items", "The items to display. The key will be the name of the item and the value should be the URL to the media file.", "" ) );

            if ( controls != null )
            {
                if ( controls.Count > 0 && controls[0] != null && controls[0] is RockDropDownList )
                {
                    configurationValues[MODE_TYPE].Value = ( ( RockDropDownList ) controls[0] ).SelectedValue;
                }

                if ( controls.Count > 1 && controls[1] != null && controls[1] is RockTextBox )
                {
                    configurationValues[ITEM_WIDTH].Value = ( ( RockTextBox ) controls[1] ).Text;
                }

                if ( controls.Count > 2 && controls[2] != null && controls[2] is KeyValueList )
                {
                    configurationValues[MEDIA_ITEMS].Value = ( ( KeyValueList ) controls[2] ).Value;
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
            if ( controls != null && configurationValues != null )
            {
                if ( controls.Count > 0 && controls[0] != null && controls[0] is RockDropDownList && configurationValues.ContainsKey( MODE_TYPE ) )
                {
                    ( ( RockDropDownList ) controls[0] ).SelectedValue = configurationValues[MODE_TYPE].Value;
                }

                if ( controls.Count > 1 && controls[1] != null && controls[1] is RockTextBox && configurationValues.ContainsKey( ITEM_WIDTH ) )
                {
                    ( ( RockTextBox ) controls[1] ).Text = configurationValues[ITEM_WIDTH].Value;
                }

                if ( controls.Count > 2 && controls[2] != null && controls[2] is KeyValueList && configurationValues.ContainsKey( MEDIA_ITEMS ) )
                {
                    ( ( KeyValueList ) controls[2] ).Value = configurationValues[MEDIA_ITEMS].Value;
                }
            }
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
            var control = new Web.UI.Controls.MediaSelector { ID = id };

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( MODE_TYPE ) )
                {
                    control.Mode = configurationValues[MODE_TYPE].Value.ConvertToEnum<MediaSelectorMode>( MediaSelectorMode.Image );
                }

                if ( configurationValues.ContainsKey( MEDIA_ITEMS ) )
                {
                    control.MediaItems = configurationValues[MEDIA_ITEMS].Value.AsDictionary();
                }

                if ( configurationValues.ContainsKey( ITEM_WIDTH ) )
                {
                    control.ItemWidth = configurationValues[ITEM_WIDTH].Value;
                }
            }

            return control;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is MediaSelector )
            {
                var name = ( ( MediaSelector ) control ).Value;
                return GetPrivateEditValue( name, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
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
            if ( value != null )
            {
                string[] nameAndValue = value.Split( new char[] { '^' } );
                if ( nameAndValue.Length == 2 )
                {
                    ( ( MediaSelector ) control ).Value = nameAndValue[0];
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
            return GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )?.EncodeHtml();
        }

#endif
        #endregion
    }
}