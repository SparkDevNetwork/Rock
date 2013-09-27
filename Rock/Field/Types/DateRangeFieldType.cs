//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a pair of date values
    /// </summary>
    [Serializable]
    public class DateRangeFieldType : FieldType
    {
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
                DateTime result;
                string[] valuePair = value.Split( new char[] { ',' }, StringSplitOptions.None );
                if ( valuePair.Length <= 2 )
                {
                    foreach ( string v in valuePair )
                    {
                        if ( !string.IsNullOrWhiteSpace( v ) )
                        {
                            if ( !string.IsNullOrWhiteSpace( v ) )
                            {
                                if ( !DateTime.TryParse( v, out result ) )
                                {
                                    message = "The input provided contains invalid date values";
                                    return false;
                                }
                            }
                        }
                    }
                }
                else
                {
                    message = "The input provided is not a valid date range.";
                    return false;
                }
            }

            return base.IsValid( value, required, out message );
        }

        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control EditControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var dateRangePicker = new DateRangePicker { ID = id };
            return dateRangePicker;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues )
        {
            DateRangePicker editor = control as DateRangePicker;
            if ( editor != null )
            {
                return string.Format( "{0:d},{1:d}", editor.LowerValue, editor.UpperValue );
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
            DateRangePicker editor = control as DateRangePicker;
            if ( editor != null )
            {
                if ( value != null )
                {
                    string[] valuePair = value.Split( new char[] { ',' }, StringSplitOptions.None );
                    if ( valuePair.Length == 2 )
                    {
                        DateTime result;

                        if ( DateTime.TryParse( valuePair[0], out result ) )
                        {
                            editor.LowerValue = result;
                        }

                        if ( DateTime.TryParse( valuePair[1], out result ) )
                        {
                            editor.UpperValue = result;
                        }
                    }
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
        public override string FormatValue( System.Web.UI.Control parentControl, string value, System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            if ( value != null )
            {
                string[] valuePair = value.Split( new char[] { ',' }, StringSplitOptions.None );
                if ( valuePair.Length == 2 )
                {
                    string lowerValue = string.IsNullOrWhiteSpace( valuePair[0] ) ? Rock.Constants.None.TextHtml : DateTime.Parse( valuePair[0] ).Date.ToShortDateString();
                    string upperValue = string.IsNullOrWhiteSpace( valuePair[1] ) ? Rock.Constants.None.TextHtml : DateTime.Parse( valuePair[1] ).Date.ToShortDateString();
                    return string.Format( "{0} to {1}", lowerValue, upperValue );
                }
            }

            // something unexpected.  Let the base format it
            return base.FormatValue( parentControl, value, configurationValues, condensed );

        }
    }
}