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

using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a pair of integer values
    /// Stored as a comma-delimited pair of integers
    /// </summary>
    [Serializable]
    public class IntegerRangeFieldType : FieldType
    {

        #region Formatting

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary> 
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            string formattedValue = string.Empty;

            if ( value != null )
            {
                string[] valuePair = value.Split( new char[] { ',' }, StringSplitOptions.None );
                if ( valuePair.Length == 2 )
                {
                    string lowerValue = string.IsNullOrWhiteSpace( valuePair[0] ) ? Rock.Constants.None.TextHtml : valuePair[0];
                    string upperValue = string.IsNullOrWhiteSpace( valuePair[1] ) ? Rock.Constants.None.TextHtml : valuePair[1];
                    if ( !string.IsNullOrWhiteSpace( lowerValue ) || !string.IsNullOrWhiteSpace( upperValue ) )
                    {
                        return string.Format( "{0} to {1}", lowerValue, upperValue );
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }

            // something unexpected.  Let the base format it
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
        public override System.Web.UI.Control EditControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var numberRangeEditor = new NumberRangeEditor { ID = id };
            numberRangeEditor.NumberType = System.Web.UI.WebControls.ValidationDataType.Integer;
            return numberRangeEditor;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues )
        {
            NumberRangeEditor editor = control as NumberRangeEditor;
            if ( editor != null )
            {
                return string.Format( "{0},{1}", editor.LowerValue, editor.UpperValue );
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            NumberRangeEditor editor = control as NumberRangeEditor;
            if ( editor != null )
            {
                if ( value != null )
                {
                    string[] valuePair = value.Split( new char[] { ',' }, StringSplitOptions.None );
                    if ( valuePair.Length == 2 )
                    {
                        int result;

                        if ( int.TryParse( valuePair[0], out result ) )
                        {
                            editor.LowerValue = result;
                        }

                        if ( int.TryParse( valuePair[1], out result ) )
                        {
                            editor.UpperValue = result;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tests the value to ensure that it is a valid value.  If not, message will indicate why
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="message">The message.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is valid; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsValid( string value, bool required, out string message )
        {
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                int result;
                string[] valuePair = value.Split( new char[] { ',' }, StringSplitOptions.None );
                if ( valuePair.Length <= 2 )
                {
                    foreach ( string v in valuePair )
                    {
                        if ( !string.IsNullOrWhiteSpace( v ) )
                        {
                            if ( !string.IsNullOrWhiteSpace( v ) )
                            {
                                if ( !int.TryParse( v, out result ) )
                                {
                                    message = "The input provided contains invalid integer values";
                                    return false;
                                }
                            }
                        }
                    }
                }
                else
                {
                    message = "The input provided is not a valid integer range.";
                    return false;
                }
            }

            return base.IsValid( value, required, out message );
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

    }
}