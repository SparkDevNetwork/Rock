// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web.UI.WebControls;

using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a date value with additional option to use 'current' date
    /// </summary>
    [Serializable]
    public class FilterDateFieldType : DateFieldType
    {

        #region Configuration

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var keys = base.ConfigurationKeys();
            keys.Add( "displayCurrentOption" );
            return keys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override System.Collections.Generic.List<System.Web.UI.Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            var cbDisplayCurrent = new RockCheckBox();
            controls.Add( cbDisplayCurrent );
            cbDisplayCurrent.Label = "Allow 'Current' Option";
            cbDisplayCurrent.Text = "Yes";
            cbDisplayCurrent.Help = "Display option for selecting the 'current' date instead of a specific date";
            return controls;

        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="configurationValues">The configuration values.</param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            base.SetConfigurationValues( controls, configurationValues );

            if ( controls != null )
            {
                if ( controls.Count > 2 && controls[2] != null && controls[2] is CheckBox &&
                    configurationValues.ContainsKey( "displayCurrentOption" ) )
                {
                    ( (CheckBox)controls[2] ).Checked = configurationValues["displayCurrentOption"].Value.AsBoolean( false );
                }

            }
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            var values = base.ConfigurationValues( controls );
            values.Add( "displayCurrentOption", new ConfigurationValue( "Allow 'Current' Option", "Display option for selecting the 'current' date instead of a specific date.", "False" ) );

            if ( controls != null )
            {
                if ( controls.Count > 2 && controls[2] != null && controls[2] is CheckBox )
                {
                    values["displayCurrentOption"].Value = ( (CheckBox)controls[2] ).Checked.ToString();
                }
            }

            return values;
        }

        #endregion

        #region Formatting

        /// <summary>
        /// Formats date display
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            if ( !string.IsNullOrWhiteSpace( value ) && value.StartsWith( "CURRENT", StringComparison.OrdinalIgnoreCase ) )
            {
                var valueParts = value.Split( ':' );
                if ( valueParts.Length > 1 )
                {
                    int daysOffset = valueParts[1].AsInteger();
                    if ( daysOffset != 0 )
                    {
                        return "Current Date " + ( daysOffset > 0 ? " plus " : " minus " ) + Math.Abs( daysOffset ).ToString() + " days";
                    }
                }

                return "Current Date";
            }

            return base.FormatValue( parentControl, value, configurationValues, condensed );
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
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var dp = base.EditControl( configurationValues, id ) as DatePicker;
            if ( dp != null )
            {
                dp.DisplayCurrentOption = configurationValues.ContainsKey( "displayCurrentOption" ) &&
                    configurationValues["displayCurrentOption"].Value.AsBoolean( false );
                return dp;
            }
            return null;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if (control != null)
            {
                var dtp = control as DatePicker;
                if (dtp != null && dtp.IsCurrentDateOffset )
                {
                    return string.Format("CURRENT:{0}", dtp.CurrentDateOffsetDays) ;
                }
            }

            return base.GetEditValue( control, configurationValues );
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( control != null  ) 
            {
                var dtp = control as DatePicker;
                if ( dtp != null )
                {
                    if ( dtp.DisplayCurrentOption && value != null && value.StartsWith( "CURRENT", StringComparison.OrdinalIgnoreCase ) )
                    {
                        dtp.IsCurrentDateOffset = true;
                        var valueParts = value.Split(':');
                        if ( valueParts.Length > 1 )
                        {
                            dtp.CurrentDateOffsetDays = valueParts[1].AsIntegerOrNull() ?? 0;
                        }
                    }
                    else
                    {
                        base.SetEditValue( control, configurationValues, value );
                    }
                }
            }
        }

        #endregion

        #region Filter Control

        public override Control FilterControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            // This field type does not support filtering ( it should only ever be used as a filter )
            return null;
        }
        #endregion

        /// <summary>
        /// Gets information about how to configure a filter UI for this type of field. Used primarily for dataviews
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public override Reporting.EntityField GetFilterConfig( Rock.Web.Cache.AttributeCache attribute )
        {
            var filterConfig = base.GetFilterConfig( attribute );
            filterConfig.FilterFieldType = SystemGuid.FieldType.FILTER_DATE;
            return filterConfig;
        }
    }
}