//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a list of options as checkboxes.  Value is saved as a | delimited list
    /// </summary>
    public class SelectSingle : FieldType
    {
        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override Control[] ConfigurationControls()
        {
            Control[] controls = new Control[2];

            TextBox tb = new TextBox();
            tb.TextMode = TextBoxMode.MultiLine;
            tb.Rows = 3;
            controls[0] = tb;

            DropDownList ddl = new DropDownList();
            ddl.Items.Add( new ListItem( "Drop Down List", "ddl" ) );
            ddl.Items.Add( new ListItem( "Radio Buttons", "rb" ) );
            controls[1] = ddl;

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> GetConfigurationValues( Control[] controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( "values", new ConfigurationValue( "Values", "Comma-delimited list of values to display", "" ) );
            configurationValues.Add( "fieldtype", new ConfigurationValue( "Field Type", "Field type to use for selection", "ddl" ) );

            if ( controls != null && controls.Length == 2 )
            {
                if ( controls[0] != null && controls[0] is TextBox )
                    configurationValues["values"].Value = ( ( TextBox )controls[0] ).Text;

                if ( controls[1] != null && controls[1] is DropDownList )
                    if ( ( ( DropDownList )controls[1] ).SelectedValue == "rb" )
                        configurationValues["fieldtype"].Value = "rb";
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( Control[] controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls != null && controls.Length == 2 && configurationValues != null)
            {
                if ( controls[0] != null && controls[0] is TextBox && configurationValues.ContainsKey("values"))
                    ( ( TextBox )controls[0] ).Text = configurationValues["values"].Value;

                if ( controls[1] != null && controls[1] is DropDownList && configurationValues.ContainsKey("fieldtype") )
                    ( ( DropDownList )controls[1] ).SelectedValue = configurationValues["fieldtype"].Value;
            }
        }

        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues )
        {
            ListControl editControl = null;

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( "fieldtype" ) && configurationValues["fieldtype"].Value == "rb" )
                    editControl = new RadioButtonList();
                else
                    editControl = new DropDownList();

                if ( configurationValues.ContainsKey( "values" ) )
                    foreach ( string value in configurationValues["values"].Value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                        editControl.Items.Add( new ListItem( value ) );
            }

            return editControl;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is ListControl )
                return ((ListControl)control).SelectedValue;

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
            if ( control != null && control is ListControl )
                ( ( ListControl )control ).SelectedValue = value;
        }
    }
}