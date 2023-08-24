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

using System.Collections.Generic;
using System.Web.UI;
using Rock.Attribute;
using Rock.Enums.Controls;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    ///  Media Selector Field Type
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.MEDIA_SELECTOR )]
    public class MediaSelectorFieldType : FieldType
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
            tbItemWidth.Help = "The width of each media item in pixes or percentage.";
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
                return ( ( MediaSelector ) control ).Value;
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
                if ( control != null && control is MediaSelector )
                {
                    ( ( MediaSelector ) control ).Value = value;
                }
            }
        }

#endif
        #endregion
    }
}