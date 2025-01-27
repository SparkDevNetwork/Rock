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
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom;
#if WEBFORMS
using System.Web.UI;
#endif
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Web;
using Rock.ViewModels.Utility;
using System;
using Rock.Data;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to show a captcha and validate the user is not a robot.
    /// Stores value of "1" if user is verified as not a robot.
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.System )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.CAPTCHA )]
    public class CaptchaFieldType : FieldType
    {
        #region Configuration

        private const string NOTIFICATION_WARNING_PROPERTY_KEY = "notificationWarning";

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string privateValue )
        {
            var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, privateValue );

            if ( usage == ConfigurationValueUsage.Configure )
            {
                var siteKey = SystemSettings.GetValue( SystemKey.SystemSetting.CAPTCHA_SITE_KEY );
                var secretKey = SystemSettings.GetValue( SystemKey.SystemSetting.CAPTCHA_SECRET_KEY );

                if ( siteKey.IsNullOrWhiteSpace() || secretKey.IsNullOrWhiteSpace() )
                {
                    publicConfigurationValues[NOTIFICATION_WARNING_PROPERTY_KEY] = "Captcha site key or secret key have not been configured yet. Captcha will not work until both keys are set.";
                }
            }

            return publicConfigurationValues;
        }

        /// <inheritdoc />
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            return new Dictionary<string, string>();
        }

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            bool? boolValue = privateValue.AsBooleanOrNull();

            if ( boolValue == true )
            {
                return "Verified";
            }

            return string.Empty;
        }

        #endregion

        #region Edit Control

        /// <summary>
        /// Gets a value indicating whether this field has a control to configure the default value
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has default control; otherwise, <c>false</c>.
        /// </value>
        public override bool HasDefaultControl => false;

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
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
            var configKeys = new List<string>();

            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = new List<Control>();

            var infoBox = new NotificationBox
            {
                NotificationBoxType = NotificationBoxType.Info,
                Text = "The user will be prompted to complete verify they are human each time this field is displayed in edit mode."
            };
            controls.Add( infoBox );

            var siteKey = SystemSettings.GetValue( SystemKey.SystemSetting.CAPTCHA_SITE_KEY );
            var secretKey = SystemSettings.GetValue( SystemKey.SystemSetting.CAPTCHA_SECRET_KEY );

            if ( siteKey.IsNullOrWhiteSpace() || secretKey.IsNullOrWhiteSpace() )
            {
                var nokeysBox = new NotificationBox
                {
                    NotificationBoxType = NotificationBoxType.Warning,
                    Text = "Captcha site key or secret key have not been configured yet. Captcha will not work until both keys are set."
                };
                controls.Add( nokeysBox );
            }

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            var configurationValues = new Dictionary<string, ConfigurationValue>();

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
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
        /// Returns the value using the most appropriate data type
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override object ValueAsFieldType( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return value.AsDoubleOrNull();
        }

        /// <summary>
        /// Returns the value that should be used for sorting, using the most appropriate data type
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override object SortValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            // return ValueAsFieldType which returns the value as a double
            return this.ValueAsFieldType( parentControl, value, configurationValues );
        }

        /// <summary>
        /// Renders the controls necessary for prompting user for a new value and adds them to the parentControl
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new Captcha
            {
                ID = id
            };
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control is Captcha captcha )
            {
                return captcha.IsResponseValid().ToString();
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
        }

#endif
        #endregion
    }
}
