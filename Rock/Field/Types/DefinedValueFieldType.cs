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
using Newtonsoft.Json;

using Rock;
using Rock.Constants;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of Defined Values for a specific Defined Type
    /// </summary>
    [Serializable]
    public class DefinedValueFieldType : FieldType
    {
        private const string DEFINED_TYPE_KEY = "definedtype";
        private const string ALLOW_MULTIPLE_KEY = "allowmultiple";

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
                var names = new List<string>();
                foreach ( string guidValue in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                {
                    Guid guid = Guid.Empty;
                    if ( Guid.TryParse( guidValue, out guid ) )
                    {
                        var definedValue = Rock.Web.Cache.DefinedValueCache.Read( guid );
                        if ( definedValue != null )
                        {
                            names.Add( definedValue.Name );
                        }
                    }
                }

                return names.AsDelimited( ", " );
            }

            return string.Empty;

        }

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( DEFINED_TYPE_KEY );
            configKeys.Add( ALLOW_MULTIPLE_KEY );
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
            var ddl = new RockDropDownList();
            controls.Add( ddl );
            ddl.AutoPostBack = true;
            ddl.SelectedIndexChanged += OnQualifierUpdated;
            ddl.Label = "Defined Type";
            ddl.Help = "The Defined Type to select values from.";

            Rock.Model.DefinedTypeService definedTypeService = new Model.DefinedTypeService();
            foreach ( var definedType in definedTypeService.Queryable().OrderBy( d => d.Order ) )
            {
                ddl.Items.Add( new ListItem( definedType.Name, definedType.Id.ToString() ) );
            }

            // Add checkbox for deciding if the defined values list is renedered as a drop
            // down list or a checkbox list.
            var cb = new RockCheckBox();
            controls.Add( cb );
            cb.AutoPostBack = true;
            cb.CheckedChanged += OnQualifierUpdated;
            cb.Label = "Allow Multiple Values";
            cb.Text = "Yes";
            cb.Help = "When set, allows multiple defined type values to be selected.";
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
            configurationValues.Add( DEFINED_TYPE_KEY, new ConfigurationValue( "Defined Type", "The Defined Type to select values from", "" ) );
            configurationValues.Add( ALLOW_MULTIPLE_KEY, new ConfigurationValue( "Allow Multiple Values", "When set, allows multiple defined type values to be selected.", "" ) );

            if ( controls != null && controls.Count == 2 )
            {
                if ( controls[0] != null && controls[0] is DropDownList )
                {
                    configurationValues[DEFINED_TYPE_KEY].Value = ( (DropDownList)controls[0] ).SelectedValue;
                }

                if ( controls[1] != null && controls[1] is CheckBox )
                {
                    configurationValues[ ALLOW_MULTIPLE_KEY ].Value = ( (CheckBox)controls[1] ).Checked.ToString();
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
            if ( controls != null && controls.Count == 2 && configurationValues != null )
            {
                if ( controls[0] != null && controls[0] is DropDownList && configurationValues.ContainsKey( DEFINED_TYPE_KEY ) )
                {
                    ( (DropDownList)controls[0] ).SelectedValue = configurationValues[DEFINED_TYPE_KEY].Value;
                }

                if ( controls[1] != null && controls[1] is CheckBox && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) )
                {
                    ( (CheckBox)controls[1] ).Checked = configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();
                }
            }
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
            ListControl editControl;

            if ( configurationValues != null && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ ALLOW_MULTIPLE_KEY ].Value.AsBoolean() )
            {
                editControl = new Rock.Web.UI.Controls.RockCheckBoxList { ID = id }; 
                editControl.AddCssClass( "checkboxlist-group" );
            }
            else
            {
                editControl = new Rock.Web.UI.Controls.RockDropDownList { ID = id }; 
                editControl.Items.Add( new ListItem() );
            }

            if ( configurationValues != null && configurationValues.ContainsKey( DEFINED_TYPE_KEY ) )
            {
                int definedTypeId = 0;
                if ( Int32.TryParse( configurationValues[DEFINED_TYPE_KEY].Value, out definedTypeId ) )
                {
                    Rock.Model.DefinedValueService definedValueService = new Model.DefinedValueService();
                    foreach ( var definedValue in definedValueService.GetByDefinedTypeId( definedTypeId ) )
                    {
                        editControl.Items.Add( new ListItem( definedValue.Name, definedValue.Id.ToString() ) );
                    }
                }
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
            var ids = new List<string>();

            if ( control != null && control is ListControl )
            {
                if ( control is Rock.Web.UI.Controls.RockDropDownList )
                {
                    ids.Add( ( (ListControl)control ).SelectedValue );
                }
                else if ( control is Rock.Web.UI.Controls.RockCheckBoxList )
                {
                    var cblControl = control as Rock.Web.UI.Controls.RockCheckBoxList;

                    ids.AddRange( cblControl.Items.Cast<ListItem>()
                        .Where( i => i.Selected )
                        .Select( i => i.Value ) );
                }
            }

            var guids = new List<string>();

            foreach ( string id in ids )
            {
                int definedValueId = int.MinValue;
                if ( int.TryParse( id, out definedValueId ) )
                {
                    var definedValue = Rock.Web.Cache.DefinedValueCache.Read( definedValueId );
                    if ( definedValue != null )
                    {
                        guids.Add( definedValue.Guid.ToString() );
                    }
                }
            }

            return guids.AsDelimited( "," );
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
                if ( control != null && control is ListControl )
                {
                    var ids = new List<string>();
                    foreach ( string guidValue in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                    {
                        Guid guid = Guid.Empty;
                        if ( Guid.TryParse( guidValue, out guid ) )
                        {
                            var definedValue = Rock.Web.Cache.DefinedValueCache.Read( guid );
                            if ( definedValue != null )
                            {
                                ids.Add( definedValue.Id.ToString() );
                            }
                        }
                    }

                    var listControl = control as ListControl;
                    foreach ( ListItem li in listControl.Items )
                    {
                        li.Selected = ids.Contains( li.Value );
                    }
                }
            }
        }

    }
}