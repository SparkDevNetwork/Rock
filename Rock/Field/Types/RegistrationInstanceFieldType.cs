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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) registration instance filtered by a selected registration template
    /// </summary>
    public class RegistrationInstanceFieldType : FieldType
    {

        #region Configuration

        /// <summary>
        /// Configuration Key for Registration Template
        /// </summary>
        public static readonly string REGISTRATION_TEMPLATE_KEY = "registrationtemplate";

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

        #endregion
    }
}