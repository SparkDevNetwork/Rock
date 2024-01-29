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
using System.Linq.Expressions;
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;

#endif
using Rock.Attribute;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to save a boolean value. Stored as "True" or "False"
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.Common )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><path d=""M1,5.18H2.67v6.67H3.78V5.18H5.44V4.07H1Z""/><path d=""M15,5.18V4.07H10.56v7.78h1.11V8.52h2.77V7.4H11.67V5.18Z""/><rect x=""7.44"" y=""3"" width=""1.11"" height=""10""/></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.BOOLEAN )]
    public class BooleanFieldType : FieldType
    {
        /// <summary>
        /// Boolean FieldType Configuration Keys
        /// </summary>
        public static class ConfigurationKey
        {
            /// <summary>
            /// The key for true text
            /// </summary>
            public const string TrueText = "truetext";
            /// <summary>
            /// The key for false text
            /// </summary>
            public const string FalseText = "falsetext";
            /// <summary>
            /// The key for the boolean control type
            /// </summary>
            public const string BooleanControlType = "BooleanControlType";
        }

        #region Configuration

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            bool? boolValue = privateValue.AsBooleanOrNull();

            if ( !boolValue.HasValue )
            {
                return string.Empty;
            }

            if ( boolValue.Value )
            {
                var trueText = privateConfigurationValues.GetValueOrDefault( ConfigurationKey.TrueText, string.Empty );
                if ( trueText.IsNullOrWhiteSpace() )
                {
                    return "Yes";

                }
                return trueText;
            }
            else
            {
                var falseText = privateConfigurationValues.GetValueOrDefault( ConfigurationKey.FalseText, string.Empty );
                if ( falseText.IsNullOrWhiteSpace() )
                {
                    return "No";

                }
                return falseText;
            }
        }

        /// <inheritdoc/>
        public override string GetCondensedTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            bool? boolValue = privateValue.AsBooleanOrNull();

            if ( !boolValue.HasValue )
            {
                return string.Empty;
            }

            // A condensed boolean value simply returns "Y" or "N" regardless
            // of the other configuration values.
            return boolValue.Value ? "Y" : "N";
        }

        /// <inheritdoc/>
        public override string GetCondensedHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetCondensedTextValue( privateValue, privateConfigurationValues );
        }

        #endregion

        #region Edit Control



        /// <summary>
        /// The type of the control (DropDown, CheckBox, Toggle, or Switch) to use to edit the value
        /// </summary>
        public enum BooleanControlType
        {
            /// <summary>
            /// Use a RockDropDown with TrueText and FalseTest as the options
            /// </summary>
            DropDown,

            /// <summary>
            /// Use a RockCheckBox
            /// </summary>
            Checkbox,

            /// <summary>
            /// Use a Rock:Toggle with TrueText and FalseTest as the buttons text
            /// </summary>
            Toggle
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
            bool? boolValue = value.AsBooleanOrNull();
            if ( required && !boolValue.HasValue )
            {
                message = "Invalid boolean value";
                return false;
            }

            return base.IsValid( value, required, out message );
        }

        #endregion

        #region Filter Control



        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return true;
        }

        /// <summary>
        /// Formats the filter value value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override string FormatFilterValueValue( Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            string formattedValue = FormatValue( null, value, configurationValues, false );
            return AddQuotes( formattedValue );
        }

        /// <inheritdoc/>
        public override ComparisonValue GetPublicFilterValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var values = privateValue.FromJsonOrNull<List<string>>();

            if ( values?.Count == 1 )
            {
                // NOTE: this is for backwards compatibility for filters that
                // were saved when Boolean DataFilters didn't have a Compare Option.
                return new ComparisonValue
                {
                    ComparisonType = ComparisonType.EqualTo,
                    Value = GetPublicEditValue( values[0], privateConfigurationValues )
                };
            }
            else
            {
                return base.GetPublicFilterValue( privateValue, privateConfigurationValues );
            }
        }

        /// <summary>
        /// Gets a filter expression for an entity property value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns></returns>
        public override Expression PropertyFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, Expression parameterExpression, string propertyName, Type propertyType )
        {
            if ( filterValues.Count == 1 )
            {
                // NOTE: this is for backwards compatibility for filters that were saved when Boolean DataFilters didn't have a Compare Option
                MemberExpression propertyExpression = Expression.Property( parameterExpression, propertyName );
                ConstantExpression constantExpression = Expression.Constant( bool.Parse( filterValues[0] ) );
                ComparisonType comparisonType = ComparisonType.EqualTo;
                return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, constantExpression );
            }
            else
            {
                return base.PropertyFilterExpression( configurationValues, filterValues, parameterExpression, propertyName, propertyType );
            }
        }

        /// <summary>
        /// Gets the type of the filter comparison.
        /// </summary>
        /// <value>
        /// The type of the filter comparison.
        /// </value>
        public override ComparisonType FilterComparisonType
        {
            get
            {
                return ComparisonType.EqualTo | ComparisonType.NotEqualTo;
            }
        }

        /// <summary>
        /// Gets a filter expression for an attribute value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns></returns>
        public override Expression AttributeFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, ParameterExpression parameterExpression )
        {
            if ( filterValues.Count == 1 )
            {
                // NOTE: this is for backwards compatibility for filters that were saved when Boolean DataFilters didn't have a Compare Option
                MemberExpression propertyExpression = Expression.Property( parameterExpression, "Value" );
                ConstantExpression constantExpression = Expression.Constant( filterValues[0] );
                ComparisonType comparisonType = ComparisonType.EqualTo;
                return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, constantExpression );
            }
            else
            {
                return base.AttributeFilterExpression( configurationValues, filterValues, parameterExpression );
            }

        }

        /// <summary>
        /// Attributes the constant expression.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override ConstantExpression AttributeConstantExpression( string value )
        {
            var booleanValue = value.AsBoolean();
            return Expression.Constant( booleanValue, typeof( bool ) );
        }

        /// <summary>
        /// Gets the name of the attribute value field that should be bound to (Value, ValueAsDateTime, ValueAsBoolean, or ValueAsNumeric)
        /// </summary>
        /// <value>
        /// The name of the attribute value field.
        /// </value>
        public override string AttributeValueFieldName
        {
            get
            {
                return "ValueAsBoolean";
            }
        }

        /// <summary>
        /// Gets the type of the attribute value field.
        /// </summary>
        /// <value>
        /// The type of the attribute value field.
        /// </value>
        public override Type AttributeValueFieldType
        {
            get
            {
                return typeof( bool? );
            }
        }

        #endregion

        #region Persistence

        /// <inheritdoc/>
        public override bool IsPersistedValueInvalidated( Dictionary<string, string> oldPrivateConfigurationValues, Dictionary<string, string> newPrivateConfigurationValues )
        {
            var oldTrueText = oldPrivateConfigurationValues.GetValueOrNull( ConfigurationKey.TrueText ) ?? string.Empty;
            var oldFalseText = oldPrivateConfigurationValues.GetValueOrNull( ConfigurationKey.FalseText ) ?? string.Empty;
            var newTrueText = newPrivateConfigurationValues.GetValueOrNull( ConfigurationKey.TrueText ) ?? string.Empty;
            var newFalseText = newPrivateConfigurationValues.GetValueOrNull( ConfigurationKey.FalseText ) ?? string.Empty;

            if ( oldTrueText != newTrueText )
            {
                return true;
            }

            if ( oldFalseText != newFalseText )
            {
                return true;
            }

            return false;
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            configKeys.Add( ConfigurationKey.TrueText );
            configKeys.Add( ConfigurationKey.FalseText );
            configKeys.Add( ConfigurationKey.BooleanControlType );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            RockTextBox tbTrueText = new RockTextBox();
            controls.Add( tbTrueText );
            tbTrueText.AutoPostBack = true;
            tbTrueText.TextChanged += OnQualifierUpdated;
            tbTrueText.Label = "True Text";
            tbTrueText.Text = "Yes";
            tbTrueText.Help = "The text to display when value is true.";

            RockTextBox tbFalseText = new RockTextBox();
            controls.Add( tbFalseText );
            tbFalseText.AutoPostBack = true;
            tbFalseText.TextChanged += OnQualifierUpdated;
            tbFalseText.Label = "False Text";
            tbFalseText.Text = "No";
            tbFalseText.Help = "The text to display when value is false.";

            RockDropDownList ddlBooleanControlType = new RockDropDownList();
            controls.Add( ddlBooleanControlType );
            ddlBooleanControlType.BindToEnum<BooleanControlType>();
            ddlBooleanControlType.Label = "Control Type";
            ddlBooleanControlType.Help = "The type of control to use when editing the value";
            ddlBooleanControlType.AutoPostBack = true;
            ddlBooleanControlType.SelectedIndexChanged += OnQualifierUpdated;

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
            configurationValues.Add( ConfigurationKey.TrueText, new ConfigurationValue( "True Text",
                "The text to display when value is true (default is 'Yes').", "Yes" ) );
            configurationValues.Add( ConfigurationKey.FalseText, new ConfigurationValue( "False Text",
                "The text to display when value is false (default is 'No').", "No" ) );

            configurationValues.Add( ConfigurationKey.BooleanControlType, new ConfigurationValue( "Control Type",
                "The type of control to use when editing the value", BooleanControlType.DropDown.ConvertToString( false ) ) );

            if ( controls != null && controls.Count == 3 )
            {
                var tbTrueText = controls[0] as TextBox;
                var tbFalseText = controls[1] as TextBox;
                var ddlBooleanControlType = controls[2] as RockDropDownList;
                configurationValues[ConfigurationKey.TrueText].Value = tbTrueText?.Text;
                configurationValues[ConfigurationKey.FalseText].Value = tbFalseText?.Text;
                configurationValues[ConfigurationKey.BooleanControlType].Value = ddlBooleanControlType?.SelectedValue;
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
                var tbTrueText = controls[0] as TextBox;
                var tbFalseText = controls[1] as TextBox;
                var ddlBooleanControlType = controls[2] as RockDropDownList;
                if ( configurationValues.ContainsKey( ConfigurationKey.TrueText ) )
                {
                    tbTrueText.Text = configurationValues[ConfigurationKey.TrueText].Value;
                }

                if ( configurationValues.ContainsKey( ConfigurationKey.FalseText ) )
                {
                    tbFalseText.Text = configurationValues[ConfigurationKey.FalseText].Value;
                }

                if ( configurationValues.ContainsKey( ConfigurationKey.BooleanControlType ) )
                {
                    ddlBooleanControlType.SetValue( configurationValues[ConfigurationKey.BooleanControlType].Value );
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
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <summary>
        /// Returns the value using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override object ValueAsFieldType( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return value.AsBoolean();
        }

        /// <summary>
        /// Returns the value that should be used for sorting, using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override object SortValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            // return ValueAsFieldType which returns the value as a bool
            return this.ValueAsFieldType( parentControl, value, configurationValues );
        }

        /// <summary>
        /// Renders the controls necessary for prompting user for a new value and adds them to the parentControl
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var booleanControlType = configurationValues.GetValueOrNull( ConfigurationKey.BooleanControlType )?.ConvertToEnum<BooleanControlType>() ?? BooleanControlType.DropDown;

            return CreateBooleanEditControl( configurationValues, id, booleanControlType );
        }

        /// <summary>
        /// Creates the boolean edit control using the specified control type
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="booleanControlType">Type of the boolean control.</param>
        /// <returns></returns>
        private Control CreateBooleanEditControl( Dictionary<string, ConfigurationValue> configurationValues, string id, BooleanControlType booleanControlType )
        {

            string yesText = "Yes";
            string noText = "No";

            if ( configurationValues != null )
            {
                yesText = configurationValues.GetValueOrNull( ConfigurationKey.TrueText );
                noText = configurationValues.GetValueOrNull( ConfigurationKey.FalseText );
            }

            if ( yesText.IsNullOrWhiteSpace() )
            {
                yesText = "Yes";
            }

            if ( noText.IsNullOrWhiteSpace() )
            {
                noText = "No";
            }

            Control editControl;

            if ( booleanControlType == BooleanControlType.Checkbox )
            {
                var checkboxEditControl = new RockCheckBox { ID = id };
                editControl = checkboxEditControl;
            }
            else if ( booleanControlType == BooleanControlType.Toggle )
            {
                var toggleEditControl = new Toggle
                {
                    ID = id,
                    OnText = yesText,
                    OffText = noText,
                };

                editControl = toggleEditControl;
            }
            else
            {
                var dropDownEditControl = new RockDropDownList { ID = id };
                dropDownEditControl.Items.Add( new ListItem() );
                dropDownEditControl.Items.Add( new ListItem( noText, "False" ) );
                dropDownEditControl.Items.Add( new ListItem( yesText, "True" ) );

                editControl = dropDownEditControl;
            }

            return editControl;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var editControlAsDropDownList = control as RockDropDownList;
            var editControlAsCheckbox = control as RockCheckBox;
            var editControlAsToggle = control as Toggle;

            if ( editControlAsDropDownList != null )
            {
                return editControlAsDropDownList.SelectedValue;
            }

            if ( editControlAsCheckbox != null )
            {
                return editControlAsCheckbox.Checked.ToTrueFalse();
            }

            if ( editControlAsToggle != null )
            {
                return editControlAsToggle.Checked.ToTrueFalse();
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
            var editControlAsDropDownList = control as RockDropDownList;
            var editControlAsCheckbox = control as RockCheckBox;
            var editControlAsToggle = control as Toggle;

            if ( editControlAsDropDownList != null )
            {
                editControlAsDropDownList.SetValue( value.AsBooleanOrNull()?.ToString() ?? string.Empty );
            }
            else if ( editControlAsCheckbox != null )
            {
                editControlAsCheckbox.Checked = value.AsBoolean();
            }
            else if ( editControlAsToggle != null )
            {
                editControlAsToggle.Checked = value.AsBoolean();
            }
        }

        /// <summary>
        /// Gets the filter value control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            string yesText = "Yes";
            string noText = "No";

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( ConfigurationKey.TrueText ) )
                {
                    yesText = configurationValues[ConfigurationKey.TrueText].Value;
                }
                if ( configurationValues.ContainsKey( ConfigurationKey.FalseText ) )
                {
                    noText = configurationValues[ConfigurationKey.FalseText].Value;
                }
            }

            // NOTE: Use the RockDropDown for the filter control even if the ControlType of the EditControl is set to something else
            ListControl filterValueControl = new RockDropDownList();

            filterValueControl.ID = string.Format( "{0}_ctlCompareValue", id );
            filterValueControl.AddCssClass( "js-filter-control" );

            if ( !required )
            {
                filterValueControl.Items.Add( new ListItem() );
            }

            filterValueControl.Items.Add( new ListItem( yesText, "True" ) );
            filterValueControl.Items.Add( new ListItem( noText, "False" ) );
            return filterValueControl;
        }

        /// <summary>
        /// Gets the filter compare control with the specified FilterMode
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterCompareControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            if ( filterMode == FilterMode.SimpleFilter )
            {
                // hide the compare control for SimpleFilter mode
                RockDropDownList ddlCompare = ComparisonHelper.ComparisonControl( FilterComparisonType, required );
                ddlCompare.ID = string.Format( "{0}_ddlCompare", id );
                ddlCompare.AddCssClass( "js-filter-compare" );
                ddlCompare.Visible = false;
                return ddlCompare;
            }
            else
            {
                return base.FilterCompareControl( configurationValues, id, required, filterMode );
            }
        }

        /// <summary>
        /// Sets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var filterValueControl = control as ListControl;
            if ( filterValueControl != null )
            {
                filterValueControl.SetValue( value );
            }
        }

        /// <summary>
        /// Gets the filter compare value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override string GetFilterCompareValue( Control control, FilterMode filterMode )
        {
            if ( filterMode == FilterMode.SimpleFilter )
            {
                // hard code to EqualTo when in SimpleFilter mode (the comparison control is not visible)
                return ComparisonType.EqualTo.ConvertToInt().ToString();
            }
            else
            {
                return base.GetFilterCompareValue( control, filterMode );
            }
        }

        /// <summary>
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var filterValueControl = control as ListControl;
            if ( filterValueControl != null )
            {
                // Return the filter value only if a value has been selected.
                return string.IsNullOrEmpty( filterValueControl.SelectedValue ) ? null : filterValueControl.SelectedValue;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the filter value.
        /// </summary>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override List<string> GetFilterValues( Control filterControl, Dictionary<string, ConfigurationValue> configurationValues, FilterMode filterMode )
        {
            if ( filterMode == FilterMode.SimpleFilter )
            {
                var values = new List<string>();

                if ( filterControl != null )
                {
                    try
                    {
                        // hard code to EqualTo when in SimpleFilter mode (the comparison control is not visible)
                        string compare = ComparisonType.EqualTo.ConvertToInt().ToString();
                        string value = GetFilterValueValue( filterControl.Controls[1].Controls[0], configurationValues );

                        if ( value != null )
                        {
                            // since SimpleFilter is limit to just IsEqual, only return FilterValues if they actually picked something
                            values.Add( compare );
                            values.Add( value );
                        }
                    }
                    catch
                    {
                        // intentionally ignore error
                    }
                }

                return values;
            }
            else
            {
                return base.GetFilterValues( filterControl, configurationValues, filterMode );
            }
        }

#endif
        #endregion
    }
}