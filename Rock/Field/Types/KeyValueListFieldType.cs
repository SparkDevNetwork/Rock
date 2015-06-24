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
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a key/value list
    /// </summary>
    [Serializable]
    public class KeyValueListFieldType : ValueListFieldType
    {

        #region Configuration

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Insert(0, "keyprompt" );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            var tbKeyPrompt = new RockTextBox();
            controls.Insert(0, tbKeyPrompt );
            tbKeyPrompt.AutoPostBack = true;
            tbKeyPrompt.TextChanged += OnQualifierUpdated;
            tbKeyPrompt.Label = "Key Prompt";
            tbKeyPrompt.Help = "The text to display as a prompt in the key textbox.";

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
            configurationValues.Add( "keyprompt", new ConfigurationValue( "Key Prompt", "The text to display as a prompt in the key textbox.", "" ) );
            configurationValues.Add( "valueprompt", new ConfigurationValue( "Label Prompt", "The text to display as a prompt in the label textbox.", "" ) );
            configurationValues.Add( "definedtype", new ConfigurationValue( "Defined Type", "Optional Defined Type to select values from, otherwise values will be free-form text fields", "" ) );
            configurationValues.Add( "customvalues", new ConfigurationValue( "Custom Values", "Optional list of options to use for the values.  Format is either 'value1,value2,value3,...', or 'value1:text1,value2:text2,value3:text3,...'.", "" ) );

            if ( controls != null )
            {
                if ( controls.Count > 0 && controls[0] != null && controls[0] is RockTextBox   )
                {
                    configurationValues["keyprompt"].Value = ( (RockTextBox)controls[0] ).Text;
                }
                if ( controls.Count > 1 && controls[1] != null && controls[1] is RockTextBox  )
                {
                    configurationValues["valueprompt"].Value = ( (RockTextBox)controls[1] ).Text;
                }
                if ( controls.Count > 2 && controls[2] != null && controls[2] is RockDropDownList  )
                {
                    configurationValues["definedtype"].Value = ( (RockDropDownList)controls[2] ).SelectedValue;
                }
                if ( controls.Count > 3 && controls[3] != null && controls[3] is RockTextBox )
                {
                    configurationValues["customvalues"].Value = ( (RockTextBox)controls[3] ).Text;
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
                if ( controls.Count > 0 && controls[0] != null && controls[0] is RockTextBox && configurationValues.ContainsKey( "keyprompt" ) )
                {
                    ( (RockTextBox)controls[0] ).Text = configurationValues["keyprompt"].Value;
                }
                if ( controls.Count > 1 && controls[1] != null && controls[1] is RockTextBox && configurationValues.ContainsKey( "valueprompt" ) )
                {
                    ( (RockTextBox)controls[1] ).Text = configurationValues["valueprompt"].Value;
                }
                if ( controls.Count > 2 && controls[2] != null && controls[2] is RockDropDownList && configurationValues.ContainsKey( "definedtype" ) )
                {
                    ( (RockDropDownList)controls[2] ).SelectedValue = configurationValues["definedtype"].Value;
                }
                if ( controls.Count > 3 && controls[3] != null && controls[3] is RockTextBox && configurationValues.ContainsKey( "customvalues" ) )
                {
                   ( (RockTextBox)controls[3] ).Text = configurationValues["customvalues"].Value;
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
        /// <param name="configurationValues"></param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            bool isDefinedType = configurationValues != null && configurationValues.ContainsKey( "definedtype" );

            var values = new List<string>();
            string[] nameValues = value.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
            foreach ( string nameValue in nameValues )
            {
                string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                if ( nameAndValue.Length == 2 && isDefinedType )
                {
                    var definedValue = DefinedValueCache.Read( nameAndValue[1].AsInteger() );
                    if ( definedValue != null )
                    {
                        nameAndValue[1] = definedValue.Value;
                    }
                }

                values.Add( string.Format( "{0}: {1}", nameAndValue[0], nameAndValue[1] ) );
            }

            return values.AsDelimited( ", " );
        }

        #endregion

        #region Edit Control

        /// <summary>
        /// Edits the control.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override ValueList EditControl( string id )
        {
            return new KeyValueList { ID = id };
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
            var control = base.EditControl( configurationValues, id ) as KeyValueList;

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( "keyprompt" ) )
                {
                    control.KeyPrompt = configurationValues["keyprompt"].Value;
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
            if ( control != null && control is KeyValueList )
            {
                return ( (KeyValueList)control ).Value;
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
            if ( value!= null && control != null && control is KeyValueList )
            {
                ( (KeyValueList)control ).Value = value;
            }
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <returns></returns>
        public override System.Web.UI.Control FilterControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id, bool required )
        {
            // This fieldtype does not support filtering
            return null;
        }

        #endregion

        /// <summary>
        /// Gets the values from string.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public List<KeyValuePair<string, object>> GetValuesFromString( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            List<KeyValuePair<string, object>> values = new List<KeyValuePair<string, object>>();

            bool isDefinedType = configurationValues != null && configurationValues.ContainsKey( "definedtype" );

            string[] nameValues = value.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
            foreach ( string nameValue in nameValues )
            {
                string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                if ( nameAndValue.Length == 2 && isDefinedType )
                {
                    var definedValue = DefinedValueCache.Read( nameAndValue[1].AsInteger() );
                    if ( definedValue != null )
                    {
                        values.Add( new KeyValuePair<string, object>(nameAndValue[0], definedValue));
                    }
                    else
                    {
                        values.Add(  new KeyValuePair<string, object>(nameAndValue[0], nameAndValue[1]) );
                    }
                }
            }

            return values;
        }
      
    }
}