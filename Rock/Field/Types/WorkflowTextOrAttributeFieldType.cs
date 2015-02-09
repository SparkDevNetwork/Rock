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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a textbox and a dropdown list of attributes for a specific workflow Type
    /// </summary>
    [Serializable]
    public class WorkflowTextOrAttributeFieldType : FieldType
    {

        #region Configuration

        private const string ATTRIBUTE_FIELD_TYPES_KEY = "attributefieldtypes";
        private const string WORKFLOW_TYPE_ATTRIBUTES_KEY = "WorkflowTypeAttributes";
        private const string ACTIVITY_TYPE_ATTRIBUTES_KEY = "ActivityTypeAttributes";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( ATTRIBUTE_FIELD_TYPES_KEY );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            var tbCustomValues = new RockTextBox();
            controls.Add( tbCustomValues );
            tbCustomValues.TextMode = TextBoxMode.MultiLine;
            tbCustomValues.Rows = 3;
            tbCustomValues.AutoPostBack = true;
            tbCustomValues.TextChanged += OnQualifierUpdated;
            tbCustomValues.Label = "Limit Attributes by Field Type";
            tbCustomValues.Help = "Optional list of field type classes for limiting selection to attributes using those field types (e.g. 'Rock.Field.Types.PersonFieldType|Rock.Field.Types.GroupFieldType').";

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
            configurationValues.Add( ATTRIBUTE_FIELD_TYPES_KEY, new ConfigurationValue( "Limit Attributes by Field Type", "Optional list of field type classes for limiting selection to attributes using those field types (e.g. 'Rock.Field.Types.PersonFieldType|Rock.Field.Types.GroupFieldType').", "" ) );

            if ( controls != null && controls.Count == 1 )
            {
                if ( controls[0] != null && controls[0] is RockTextBox )
                {
                    configurationValues[ATTRIBUTE_FIELD_TYPES_KEY].Value = ( (RockTextBox)controls[0] ).Text;
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
            if ( controls != null && controls.Count == 1 && configurationValues != null )
            {
                if ( controls[0] != null && controls[0] is RockTextBox && configurationValues.ContainsKey( ATTRIBUTE_FIELD_TYPES_KEY ) )
                {
                    ( (RockTextBox)controls[0] ).Text = configurationValues[ATTRIBUTE_FIELD_TYPES_KEY].Value;
                }
            }
        }

        #endregion

        #region Formating

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
            string formattedValue = value;

            Guid guid = value.AsGuid();
            if ( !guid.IsEmpty() )
            {
                var attributes = GetContextAttributes();
                if ( attributes != null && attributes.ContainsKey( guid ) )
                {
                    formattedValue = attributes[guid].Name;
                }

                if ( string.IsNullOrWhiteSpace( formattedValue ) )
                {
                    var attributeCache = AttributeCache.Read( guid );
                    if ( attributeCache != null )
                    {
                        formattedValue = attributeCache.Name;
                    }
                }
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );

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
            var filteredFieldTypes = new List<string>();

            if ( configurationValues != null &&
                configurationValues.ContainsKey( ATTRIBUTE_FIELD_TYPES_KEY ) )
            {
                filteredFieldTypes = configurationValues[ATTRIBUTE_FIELD_TYPES_KEY].Value
                    .Split( "|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries ).ToList();
            }

            var editControl = new Rock.Web.UI.Controls.RockTextOrDropDownList { ID = id };
            editControl.ValidateRequestMode = ValidateRequestMode.Disabled;

            editControl.DropDownList.Items.Add( new ListItem() );

            var attributes = GetContextAttributes();
            if ( attributes != null )
            {
                foreach ( var attribute in attributes )
                {
                    var fieldType = FieldTypeCache.Read( attribute.Value.FieldTypeId );
                    if ( !filteredFieldTypes.Any() || filteredFieldTypes.Contains( fieldType.Class, StringComparer.OrdinalIgnoreCase ) )
                    {
                        editControl.DropDownList.Items.Add( new ListItem( attribute.Value.Name, attribute.Key.ToString() ) );
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
            if ( control != null && control is RockTextOrDropDownList )
            {
                return ( (RockTextOrDropDownList)control ).SelectedValue;
            }

            return string.Empty;
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
                if ( control != null && control is RockTextOrDropDownList )
                {
                    ( (RockTextOrDropDownList)control ).SelectedValue = value;
                }
            }
        }

        private Dictionary<Guid, Rock.Model.Attribute> GetContextAttributes()
        {
            var httpContext = System.Web.HttpContext.Current;
            if ( httpContext != null && httpContext.Items != null )
            {
                var workflowAttributes = httpContext.Items[WORKFLOW_TYPE_ATTRIBUTES_KEY] as Dictionary<Guid, Rock.Model.Attribute>;
                var activityAttributes = httpContext.Items[ACTIVITY_TYPE_ATTRIBUTES_KEY] as Dictionary<Guid, Rock.Model.Attribute>;

                if ( workflowAttributes != null && activityAttributes != null )
                {
                    return workflowAttributes.Concat( activityAttributes ).ToDictionary( x => x.Key, x => x.Value );
                }
            }

            return null;
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