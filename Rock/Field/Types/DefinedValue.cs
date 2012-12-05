//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of Defined Values for a specific Defined Type
    /// </summary>
    [Serializable]
    public class DefinedValue : FieldType
    {
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
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                try
                {
                    var definedValue = Rock.Web.Cache.DefinedValueCache.Read( Int32.Parse( value ) );
                    if ( definedValue != null )
                        return definedValue.Name;
                }
                catch { }

                return "Unknown Defined Value: " + value;
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( "definedtype" );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            DropDownList ddl = new DropDownList();

            Rock.Model.DefinedTypeService definedTypeService = new Model.DefinedTypeService();
            foreach ( var definedType in definedTypeService.Queryable().OrderBy( d => d.Order ) )
                ddl.Items.Add( new ListItem( definedType.Name, definedType.Id.ToString() ) );

            controls.Add( ddl );

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
            configurationValues.Add( "definedtype", new ConfigurationValue( "Defined Type", "The Defined Type to select values from", "" ) );

            if ( controls != null && controls.Count == 1 &&
                controls[0] != null && controls[0] is DropDownList )
                configurationValues["definedtype"].Value = ( ( DropDownList )controls[0] ).SelectedValue;
         
            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls != null && controls.Count == 1 && configurationValues != null &&
                controls[0] != null && controls[0] is DropDownList && configurationValues.ContainsKey("definedtype") )
                    ( ( DropDownList )controls[0] ).SelectedValue = configurationValues["definedtype"].Value;
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
            ListControl editControl = new DropDownList();

            if ( configurationValues != null && configurationValues.ContainsKey( "definedtype" ) )
            {
                int definedTypeId = 0;
                if ( Int32.TryParse( configurationValues["definedtype"].Value, out definedTypeId ) )
                {
                    Rock.Model.DefinedValueService definedValueService = new Model.DefinedValueService();
                    foreach ( var definedValue in definedValueService.GetByDefinedTypeId( definedTypeId ) )
                        editControl.Items.Add( new ListItem( definedValue.Name, definedValue.Id.ToString() ) );
                }
            }
            return editControl;
        }
    }
}