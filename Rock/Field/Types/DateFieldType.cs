//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a date value
    /// </summary>
    [Serializable]
    public class DateFieldType : FieldType
    {
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
            string formattedValue = string.Empty;

            DateTime dateValue = DateTime.MinValue;
            if ( DateTime.TryParse( value, out dateValue ) )
            {
                formattedValue = dateValue.ToShortDateString();

                if ( configurationValues != null &&
                    configurationValues.ContainsKey( "format" ) &&
                    !String.IsNullOrWhiteSpace( configurationValues["format"].Value ) )
                {
                    try
                    {
                        formattedValue = dateValue.ToString( configurationValues["format"].Value );
                    }
                    catch
                    {
                        formattedValue = dateValue.ToShortDateString();
                    }
                }

                if ( !condensed )
                {
                    if ( configurationValues != null &&
                        configurationValues.ContainsKey( "displayDiff" ) )
                    {
                        bool displayDiff = false;
                        if ( bool.TryParse( configurationValues["displayDiff"].Value, out displayDiff ) && displayDiff )
                            formattedValue += " " + dateValue.ToElapsedString( true, false );
                    }
                }
            }

            return formattedValue;
        }

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var keys = base.ConfigurationKeys();
            keys.Add( "format" );
            keys.Add( "displayDiff" );
            return keys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override System.Collections.Generic.List<System.Web.UI.Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            var textbox = new RockTextBox();
            controls.Add( textbox );
            textbox.Label = "Date Format";
            textbox.Help = "The format string to use for date (default is system short date)";

            var cbDisplayDiff = new RockCheckBox();
            controls.Add( cbDisplayDiff );
            cbDisplayDiff.Label = "Display Date Span";
            cbDisplayDiff.Text = "Yes";
            cbDisplayDiff.Help = "Display the number of years between value and current date";
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

            if ( controls != null && controls.Count >= 2 )
            {
                int i = controls.Count - 2;
                if ( controls[i] != null && controls[i] is TextBox &&
                    configurationValues.ContainsKey( "format" ) )
                {
                    ( (TextBox)controls[i] ).Text = configurationValues["format"].Value ?? string.Empty;
                }

                i++;
                if ( controls[i] != null && controls[i] is CheckBox &&
                    configurationValues.ContainsKey( "displayDiff" ) )
                {
                    bool displayDiff = false;
                    if ( !bool.TryParse( configurationValues["displayDiff"].Value ?? "False", out displayDiff ) )
                        displayDiff = false;

                    ( (CheckBox)controls[i] ).Checked = displayDiff;
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
            values.Add( "format", new ConfigurationValue( "Date Format", "The format string to use for date (default is system short date)", "" ) );
            values.Add( "displayDiff", new ConfigurationValue( "Display Date Span", "Display the number of years between value and current date", "False" ) );

            if ( controls != null && controls.Count >= 2 )
            {
                int i = controls.Count - 2;
                if ( controls[i] != null && controls[i] is TextBox )
                {
                    values["format"].Value = ( (TextBox)controls[i] ).Text;
                }
                i++;
                if ( controls[i] != null && controls[i] is CheckBox )
                {
                    values["displayDiff"].Value = ( (CheckBox)controls[i] ).Checked.ToString();
                }
            }

            return values;
        }

        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var dp = new DatePicker { ID = id }; 
            return dp;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if (control != null && control is DatePicker)
            {
                var dtp = control as DatePicker;
                if (dtp != null && dtp.SelectedDate.HasValue)
                {
                    return dtp.SelectedDate.Value.ToShortDateString();
                }
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
            var dt = DateTime.MinValue;
            if ( DateTime.TryParse( value, out dt ) )
            {

                if ( control != null && control is DatePicker ) 
                {
                    var dtp = control as DatePicker;
                    if ( dtp != null )
                    {
                        dtp.SelectedDate = dt;
                    }
                }
            }
        }

    }
}