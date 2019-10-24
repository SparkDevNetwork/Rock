// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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

using Rock.Data;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a pair of dropdown lists of Defined Values for a specific Defined Type
    /// Stored as a comma-delimited pair of DefinedValue.Guids: lowerGuid,upperGuid
    /// </summary>
    [Serializable]
    public class DefinedValueRangeFieldType : FieldType
    {
        #region Configuration

        private const string DEFINED_TYPE_KEY = "definedtype";
        private const string DISPLAY_DESCRIPTION = "displaydescription";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( DEFINED_TYPE_KEY );
            configKeys.Add( DISPLAY_DESCRIPTION );
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

            Rock.Model.DefinedTypeService definedTypeService = new Model.DefinedTypeService( new RockContext() );
            ddl.Items.Add( new ListItem() );
            foreach ( var definedType in definedTypeService.Queryable().OrderBy( d => d.Name ) )
            {
                ddl.Items.Add( new ListItem( definedType.Name, definedType.Guid.ToString() ) );
            }

            // option to show descriptions instead of values
            var cbDescription = new RockCheckBox();
            controls.Add( cbDescription );
            cbDescription.AutoPostBack = true;
            cbDescription.CheckedChanged += OnQualifierUpdated;
            cbDescription.Label = "Display Descriptions";
            cbDescription.Text = "Yes";
            cbDescription.Help = "When set, the defined value descriptions will be displayed instead of the values.";
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
            configurationValues.Add( DEFINED_TYPE_KEY, new ConfigurationValue( "Defined Type", "The Defined Type to select values from", string.Empty ) );
            configurationValues.Add( DISPLAY_DESCRIPTION, new ConfigurationValue( "Display Descriptions", "When set, the defined value descriptions will be displayed instead of the values.", string.Empty ) );

            if ( controls != null )
            {
                DropDownList ddlDefinedType = controls.Count > 0 ? controls[0] as DropDownList : null;
                CheckBox cbDisplayDescription = controls.Count > 1 ? controls[1] as CheckBox : null;
                if ( ddlDefinedType != null )
                {
                    configurationValues[DEFINED_TYPE_KEY].Value = ddlDefinedType.SelectedValue;
                }

                if ( cbDisplayDescription != null )
                {
                    configurationValues[DISPLAY_DESCRIPTION].Value = cbDisplayDescription.Checked.ToString();
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
                DropDownList ddlDefinedType = controls.Count > 0 ? controls[0] as DropDownList : null;
                CheckBox cbDisplayDescription = controls.Count > 1 ? controls[1] as CheckBox : null;
                if ( ddlDefinedType != null )
                {
                    ddlDefinedType.SelectedValue = configurationValues.GetValueOrNull( DEFINED_TYPE_KEY );
                }

                if ( cbDisplayDescription != null )
                {
                    cbDisplayDescription.Checked = configurationValues.GetValueOrNull( DISPLAY_DESCRIPTION ).AsBoolean();
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
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            if ( value != null )
            {
                string[] valuePair = value.Split( new char[] { ',' }, StringSplitOptions.None );
                if ( valuePair.Length == 2 )
                {
                    bool useDescription = false;
                    if ( !condensed && configurationValues.GetValueOrNull( DISPLAY_DESCRIPTION ).AsBoolean() )
                    {
                        useDescription = true;
                    }

                    var lowerDefinedValue = DefinedValueCache.Get( valuePair[0].AsGuid() );
                    var upperDefinedValue = DefinedValueCache.Get( valuePair[1].AsGuid() );
                    if ( lowerDefinedValue != null || upperDefinedValue != null )
                    {
                        if ( useDescription )
                        {
                            return string.Format(
                                "{0} to {1}",
                                lowerDefinedValue != null ? lowerDefinedValue.Description : string.Empty,
                                upperDefinedValue != null ? upperDefinedValue.Description : string.Empty );
                        }
                        else
                        {
                            return string.Format(
                                "{0} to {1}",
                                lowerDefinedValue != null ? lowerDefinedValue.Value : string.Empty,
                                upperDefinedValue != null ? upperDefinedValue.Value : string.Empty );
                        }
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
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            Panel pnlRange = new Panel { ID = id, CssClass = "form-control-group" };
            //Panel pnlCol1 = new Panel { CssClass = "col-md-6" };
            //Panel pnlCol2 = new Panel { CssClass = "col-md-6" };

            RockDropDownList lowerValueControl = new RockDropDownList { ID = string.Format( "{0}_ddlLower", id ) };
            lowerValueControl.CssClass = "input-width-md";
            RockDropDownList upperValueControl = new RockDropDownList { ID = string.Format( "{0}_ddlUpper", id ) };
            upperValueControl.CssClass = "input-width-md";
            pnlRange.Controls.Add( lowerValueControl );
            pnlRange.Controls.Add( new Label { CssClass = "to", Text = " to " } );
            pnlRange.Controls.Add( upperValueControl );

            if ( configurationValues != null && configurationValues.ContainsKey( DEFINED_TYPE_KEY ) )
            {
                Guid definedTypeGuid = configurationValues.GetValueOrNull( DEFINED_TYPE_KEY ).AsGuid();
                DefinedTypeCache definedType = DefinedTypeCache.Get( definedTypeGuid );

                if ( definedType != null )
                {
                    var definedValues = definedType.DefinedValues;
                    if ( definedValues.Any() )
                    {
                        bool useDescription = configurationValues.GetValueOrNull( DISPLAY_DESCRIPTION ).AsBoolean();

                        lowerValueControl.Items.Add( new ListItem() );
                        upperValueControl.Items.Add( new ListItem() );

                        foreach ( var definedValue in definedValues)
                        {
                            lowerValueControl.Items.Add( new ListItem( useDescription ? definedValue.Description : definedValue.Value, definedValue.Guid.ToString() ) );
                            upperValueControl.Items.Add( new ListItem( useDescription ? definedValue.Description : definedValue.Value, definedValue.Guid.ToString() ) );
                        }
                    }

                    return pnlRange;
                }
            }

            return null;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Panel pnlRange = control as Panel;
            if ( pnlRange != null )
            {
                RockDropDownList lowerValueControl = pnlRange.Controls.OfType<RockDropDownList>().FirstOrDefault( a => a.ID.EndsWith( "_ddlLower" ) );
                RockDropDownList upperValueControl = pnlRange.Controls.OfType<RockDropDownList>().FirstOrDefault( a => a.ID.EndsWith( "_ddlUpper" ) );

                if ( !string.IsNullOrEmpty( lowerValueControl.SelectedValue ) || !string.IsNullOrEmpty( upperValueControl.SelectedValue ) )
                {
                    return string.Format( "{0},{1}", lowerValueControl.SelectedValue, upperValueControl.SelectedValue );
                }
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
            Panel pnlRange = control as Panel;
            if ( pnlRange != null )
            {
                RockDropDownList lowerValueControl = pnlRange.Controls.OfType<RockDropDownList>().FirstOrDefault( a => a.ID.EndsWith( "_ddlLower" ) );
                RockDropDownList upperValueControl = pnlRange.Controls.OfType<RockDropDownList>().FirstOrDefault( a => a.ID.EndsWith( "_ddlUpper" ) );

                if ( value != null )
                {
                    string[] valuePair = value.Split( new char[] { ',' }, StringSplitOptions.None );
                    if ( valuePair.Length == 2 )
                    {
                        lowerValueControl.SetValue( valuePair[0].AsGuidOrNull() );
                        upperValueControl.SetValue( valuePair[1].AsGuidOrNull() );
                    }
                }
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
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override System.Web.UI.Control FilterControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, Rock.Reporting.FilterMode filterMode )
        {
            // This field type does not support filtering
            return null;
        }

        /// <summary>
        /// Gets the filter compare control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterCompareControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            // This fieldtype does not support filtering
            return null;
        }

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return false;
        }

        #endregion
    }
}