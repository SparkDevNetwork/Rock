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

using Rock.Constants;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    public class CategoryFieldType : FieldType
    {
        /// <summary>
        /// Entity Type Name Key
        /// </summary>
        protected const string ENTITY_TYPE_NAME_KEY = "entityTypeName";

        /// <summary>
        /// Qualifier Column Key
        /// </summary>
        protected const string QUALIFIER_COLUMN_KEY = "qualifierColumn";

        /// <summary>
        /// Qualifier Value Key
        /// </summary>
        protected const string QUALIFIER_VALUE_KEY = "qualifierValue";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            configKeys.Add( ENTITY_TYPE_NAME_KEY );
            configKeys.Add( QUALIFIER_COLUMN_KEY );
            configKeys.Add( QUALIFIER_VALUE_KEY );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            DropDownList ddl = new DropDownList();
            controls.Add( ddl );
            ddl.Items.Add( new ListItem( None.Text, None.IdValue ) );
            foreach ( var entityType in new EntityTypeService().GetEntities().OrderBy( e => e.FriendlyName ).ThenBy( e => e.Name ) )
            {
                ddl.Items.Add( new ListItem( entityType.FriendlyName, entityType.Name ) );
            }
            controls.Add( ddl );
            ddl.AutoPostBack = true;
            ddl.SelectedIndexChanged += OnQualifierUpdated;

            TextBox tbColumn = new TextBox();
            controls.Add( tbColumn );
            tbColumn.AutoPostBack = true;
            tbColumn.TextChanged += OnQualifierUpdated;

            TextBox tbValue = new TextBox();
            controls.Add( tbValue );
            tbValue.AutoPostBack = true;
            tbValue.TextChanged += OnQualifierUpdated;

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
            configurationValues.Add( ENTITY_TYPE_NAME_KEY, new ConfigurationValue( "Entity Type", "The type of entity to display categories for", "" ) );
            configurationValues.Add( QUALIFIER_COLUMN_KEY, new ConfigurationValue( "Qualifier Column", "Entity column qualifier", "" ) );
            configurationValues.Add( QUALIFIER_VALUE_KEY, new ConfigurationValue( "Qualifier Value", "Entity column value", "" ) );

            if ( controls != null && controls.Count == 3 )
            {
                if ( controls[0] != null && controls[0] is DropDownList )
                    configurationValues[ENTITY_TYPE_NAME_KEY].Value = ( (DropDownList)controls[0] ).SelectedValue;

                if ( controls[1] != null && controls[1] is TextBox )
                    configurationValues[QUALIFIER_COLUMN_KEY].Value = ( (TextBox)controls[1] ).Text;

                if ( controls[2] != null && controls[2] is TextBox )
                    configurationValues[QUALIFIER_VALUE_KEY].Value = ( (TextBox)controls[2] ).Text;
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
            if ( controls != null && controls.Count == 3 && configurationValues != null )
            {
                if ( controls[0] != null && controls[0] is DropDownList && configurationValues.ContainsKey( ENTITY_TYPE_NAME_KEY ) )
                    ( (DropDownList)controls[0] ).SelectedValue = configurationValues[ENTITY_TYPE_NAME_KEY].Value;

                if ( controls[1] != null && controls[1] is TextBox && configurationValues.ContainsKey( QUALIFIER_COLUMN_KEY ) )
                    ( (TextBox)controls[1] ).Text = configurationValues[QUALIFIER_COLUMN_KEY].Value;

                if ( controls[2] != null && controls[2] is TextBox && configurationValues.ContainsKey( QUALIFIER_VALUE_KEY ) )
                    ( (TextBox)controls[2] ).Text = configurationValues[QUALIFIER_VALUE_KEY].Value;
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
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                var category = CategoryCache.Read( new Guid( value ) );
                if ( category != null )
                {
                    return category.Name;
                }
            }

            return string.Empty;
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
            var picker = new CategoryPicker { ID = id }; 

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( ENTITY_TYPE_NAME_KEY ) )
                {
                    string entityTypeName = configurationValues[ENTITY_TYPE_NAME_KEY].Value;
                    if ( !string.IsNullOrWhiteSpace( entityTypeName ) && entityTypeName != None.IdValue )
                    {
                        picker.EntityTypeName = entityTypeName;
                        if ( configurationValues.ContainsKey( QUALIFIER_COLUMN_KEY ) )
                        {
                            picker.EntityTypeQualifierColumn = configurationValues[QUALIFIER_COLUMN_KEY].Value;
                            if ( configurationValues.ContainsKey( QUALIFIER_VALUE_KEY ) )
                            {
                                picker.EntityTypeQualifierValue = configurationValues[QUALIFIER_VALUE_KEY].Value;
                            }
                        }
                    }
                }
            }
            return picker;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as CategoryPicker;
            string result = null;

            if ( picker != null )
            {
                var guid = Guid.Empty;
                var id = picker.ItemId.AsInteger();
                var category = CategoryCache.Read( id ?? 0 );

                if ( category != null )
                {
                    guid = category.Guid;
                }

                result = guid.ToString();
            }

            return result;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( value != null )
            {
                var picker = control as CategoryPicker;

                if ( picker != null )
                {
                    Guid guid;
                    Guid.TryParse( value, out guid );
                    var category = new CategoryService().Get( guid );
                    picker.SetValue( category );
                }
            }
        }
    }
}
