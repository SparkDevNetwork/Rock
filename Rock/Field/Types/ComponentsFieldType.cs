//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Extension;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a checkbox list of MEF Components of a specific type
    /// </summary>
    [Serializable]
    public class ComponentsFieldType : FieldType
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
            var names = new List<string>();

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                int entityTypeId = int.MinValue;
                if ( int.TryParse( value, out entityTypeId ) )
                {
                    var entityType = EntityTypeCache.Read( entityTypeId );
                    if ( entityType != null )
                    {
                        names.Add( entityType.FriendlyName );
                    }
                }
            }

            return names.AsDelimited( ", " );
        }

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( "container" );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            TextBox tb = new TextBox();
            controls.Add( tb );

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
            configurationValues.Add( "container", new ConfigurationValue( "Container Assembly Name", "The assembly name of the MEF container to show components of", "" ) );

            if ( controls != null && controls.Count == 1 &&
                controls[0] != null && controls[0] is TextBox )
            {
                configurationValues["container"].Value = ( (TextBox)controls[0] ).Text;
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
            if ( controls != null && controls.Count == 1 && configurationValues != null &&
                controls[0] != null && controls[0] is TextBox && configurationValues.ContainsKey( "container" ) )
            {
                ( (TextBox)controls[0] ).Text = configurationValues["container"].Value;
            }
        }

        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            try
            {
                ComponentsPicker editControl = new ComponentsPicker { ID = id }; 

                if ( configurationValues != null && configurationValues.ContainsKey( "container" ) )
                {
                    editControl.ContainerType = configurationValues["container"].Value;
                }

                return editControl;
            }
            catch ( SystemException ex )
            {
                return new LiteralControl( ex.Message );
            }
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is ComponentsPicker )
            {
                return ( (ComponentsPicker)control ).SelectedComponents.AsDelimited("|");
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
            if ( value != null )
            {
                if ( control != null && control is ComponentsPicker )
                {
                    var selectedGuids = new List<Guid>();
                    value.SplitDelimitedValues().ToList().ForEach( v => selectedGuids.Add( Guid.Parse( v ) ) );
                    ( (ComponentsPicker)control ).SelectedComponents = selectedGuids;
                }
            }
        }

    }
}