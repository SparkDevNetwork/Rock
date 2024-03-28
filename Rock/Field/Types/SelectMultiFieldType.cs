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
using System.Data;
using System.Linq;
using System.Linq.Expressions;
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a list of options as checkboxes.  Value is saved as a comma-delimited list
    /// </summary>
    [Serializable]
    [FieldTypeUsage( FieldTypeUsage.Common )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><g><path d=""M13,1H3A2,2,0,0,0,1,3V13a2,2,0,0,0,2,2H13a2,2,0,0,0,2-2V3A2,2,0,0,0,13,1Zm.5,12a.5.5,0,0,1-.5.5H3a.5.5,0,0,1-.5-.5V3A.5.5,0,0,1,3,2.5H13a.5.5,0,0,1,.5.5Zm-3-7.53L7,8.94,5.5,7.47A.75.75,0,0,0,4.44,8.53l2,2a.87.87,0,0,0,.56.22.74.74,0,0,0,.53-.22l4-4a.75.75,0,0,0-1.06-1.06Z""/></g></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.MULTI_SELECT )]
    public class SelectMultiFieldType : FieldType, ISplitMultiValueFieldType
    {
        #region Configuration

        private const string VALUES_KEY = "values";
        private const string ENHANCED_SELECTION_KEY = "enhancedselection";
        private const string REPEAT_COLUMNS = "repeatColumns";
        private const string REPEAT_DIRECTION = "repeatDirection";

        private const string CUSTOM_VALUES_PUBLIC_KEY = "customValues";

        #endregion

        #region Formatting

        /// <summary>
        /// Gets the text value from the configured values.
        /// </summary>
        /// <param name="privateValue">The private (database) value.</param>
        /// <param name="configuredValues">The key-value pairs that describe which values can be displayed.</param>
        /// <returns>A plain string of text.</returns>
        private string GetTextValueFromConfiguredValues( string privateValue, Dictionary<string, string> configuredValues )
        {
            var selectedValues = privateValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

            return configuredValues
                .Where( v => selectedValues.Contains( v.Key ) )
                .Select( v => v.Value )
                .ToList()
                .AsDelimited( ", " );
        }

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( !string.IsNullOrWhiteSpace( privateValue ) && privateConfigurationValues.ContainsKey( VALUES_KEY ) )
            {
                var configuredValues = Helper.GetConfiguredValues( privateConfigurationValues );

                return GetTextValueFromConfiguredValues( privateValue, configuredValues );
            }

            return base.GetTextValue( privateValue, privateConfigurationValues );
        }

        #endregion

        #region Edit Control

        #endregion

        #region Filter Control

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
                return ComparisonHelper.ContainsFilterComparisonTypes;
            }
        }

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return true;
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
            // probably won't happen since MultiFieldType would only be a Attribute FieldType
            return base.PropertyFilterExpression( configurationValues, filterValues, parameterExpression, propertyName, propertyType );
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
            Expression comparison = null;
            if ( filterValues.Count > 1 )
            {
                //// OR up the where clauses for each of the selected values
                // and make sure to wrap commas around things so we don't collide with partial matches
                // so it'll do something like this:
                //
                // WHERE ',' + Value + ',' like '%,bacon,%'
                // OR ',' + Value + ',' like '%,lettuce,%'
                // OR ',' + Value + ',' like '%,tomato,%'

                // should be either "Contains" or "Not Contains" or "IsBlank"
                ComparisonType comparisonType = filterValues[0].ConvertToEnum<ComparisonType>( ComparisonType.Contains );

                if ( comparisonType == ComparisonType.EqualTo )
                {
                    // If EqualTo was specified, treat it as Contains
                    comparisonType = ComparisonType.Contains;
                }

                if ( comparisonType == ComparisonType.NotEqualTo )
                {
                    // If NotEqualTo was specified, treat it as DoesNotContain
                    comparisonType = ComparisonType.DoesNotContain;
                }

                // No comparison value was specified, so we can filter if the Comparison Type using no value still makes sense
                if ( ( ComparisonType.IsBlank | ComparisonType.IsNotBlank ).HasFlag( comparisonType ) )
                {
                    // Just checking if IsBlank or IsNotBlank, so let ComparisonExpression do its thing
                    MemberExpression propertyExpression = Expression.Property( parameterExpression, this.AttributeValueFieldName );
                    return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, AttributeConstantExpression( string.Empty ) );
                }

                List<string> selectedValues = filterValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

                foreach ( var selectedValue in selectedValues )
                {
                    var searchValue = "," + selectedValue + ",";
                    var qryToExtract = new AttributeValueService( new Data.RockContext() ).Queryable().Where( a => ( "," + a.Value + "," ).Contains( searchValue ) );
                    var valueExpression = FilterExpressionExtractor.Extract<AttributeValue>( qryToExtract, parameterExpression, "a" );

                    if ( comparisonType != ComparisonType.Contains )
                    {
                        valueExpression = Expression.Not( valueExpression );
                    }

                    if ( comparison == null )
                    {
                        comparison = valueExpression;
                    }
                    else
                    {
                        comparison = Expression.Or( comparison, valueExpression );
                    }
                }
            }

            if ( comparison == null )
            {
                return new NoAttributeFilterExpression();
            }

            return comparison;
        }

        #endregion

        #region Persistence

        /// <inheritdoc/>
        public override bool IsPersistedValueSupported( Dictionary<string, string> privateConfigurationValues )
        {
            var values = privateConfigurationValues.GetValueOrNull( VALUES_KEY ) ?? string.Empty;

            return !values.IsStrictLavaTemplate();
        }

        /// <inheritdoc/>
        public override bool IsPersistedValueInvalidated( Dictionary<string, string> oldPrivateConfigurationValues, Dictionary<string, string> newPrivateConfigurationValues )
        {
            var oldValues = oldPrivateConfigurationValues.GetValueOrNull( VALUES_KEY ) ?? string.Empty;
            var newValues = newPrivateConfigurationValues.GetValueOrNull( VALUES_KEY ) ?? string.Empty;

            if ( oldValues != newValues )
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public override bool IsPersistedValueVolatile( Dictionary<string, string> privateConfigurationValues )
        {
            var values = privateConfigurationValues.GetValueOrNull( VALUES_KEY ) ?? string.Empty;

            // No need to resolve lava fields since we don't support lava.
            values = values.ToUpper();

            // If the source is a SQL query then it is volatile since the results
            // of the query might change at any time.
            return values.Contains( "SELECT" ) && values.Contains( "FROM" );
        }

        /// <inheritdoc/>
        public override PersistedValues GetPersistedValues( string privateValue, Dictionary<string, string> privateConfigurationValues, IDictionary<string, object> cache )
        {
            if ( string.IsNullOrWhiteSpace( privateValue ) || !privateConfigurationValues.ContainsKey( VALUES_KEY ) )
            {
                return new PersistedValues
                {
                    TextValue = privateValue,
                    CondensedTextValue = privateValue,
                    HtmlValue = privateValue,
                    CondensedHtmlValue = privateValue
                };
            }

            if ( !( cache?.GetValueOrNull( "configuredValues" ) is Dictionary<string, string> configuredValues ) )
            {
                configuredValues = Helper.GetConfiguredValues( privateConfigurationValues );

                cache?.AddOrReplace( "configuredValues", configuredValues );
            }

            var textValue = GetTextValueFromConfiguredValues( privateValue, configuredValues ) ?? string.Empty;
            var condensedTextValue = textValue.Truncate( CondensedTruncateLength );

            return new PersistedValues
            {
                TextValue = textValue,
                CondensedTextValue = condensedTextValue,
                HtmlValue = textValue,
                CondensedHtmlValue = condensedTextValue
            };
        }

        #endregion

        #region ISplitMultiValueFieldType

        /// <inheritdoc/>
        public ICollection<string> SplitMultipleValues( string privateValue )
        {
            return privateValue.Split( ',' );
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
            configKeys.Add( VALUES_KEY );
            configKeys.Add( ENHANCED_SELECTION_KEY );
            configKeys.Add( REPEAT_COLUMNS );
            configKeys.Add( REPEAT_DIRECTION );
            return configKeys;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string privateValue )
        {
            var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, privateValue );

            if ( publicConfigurationValues.ContainsKey( VALUES_KEY ) )
            {
                if ( usage == ConfigurationValueUsage.Configure )
                {
                    // customValues contains the actual raw string that comprises the values.
                    // is used while editing the configuration values only.
                    publicConfigurationValues[CUSTOM_VALUES_PUBLIC_KEY] = publicConfigurationValues[VALUES_KEY];
                }

                var options = Helper.GetConfiguredValues( privateConfigurationValues )
                    .Select( kvp => new
                    {
                        value = kvp.Key,
                        text = kvp.Value
                    } );

                if ( usage == ConfigurationValueUsage.View )
                {
                    var selectedValues = privateValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                    options = options.Where( o => selectedValues.Contains( o.value ) );
                }

                publicConfigurationValues[VALUES_KEY] = options.ToCamelCaseJson( false, true );
            }
            else
            {
                publicConfigurationValues[VALUES_KEY] = "[]";
            }

            return publicConfigurationValues;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var privateConfigurationValues = base.GetPrivateConfigurationValues( publicConfigurationValues );

            // Don't allow them to provide the actual value items.
            if ( privateConfigurationValues.ContainsKey( VALUES_KEY ) )
            {
                privateConfigurationValues.Remove( VALUES_KEY );
            }

            // Convert the custom values string into the values to be stored.
            if ( privateConfigurationValues.ContainsKey( CUSTOM_VALUES_PUBLIC_KEY ) )
            {
                privateConfigurationValues[VALUES_KEY] = privateConfigurationValues[CUSTOM_VALUES_PUBLIC_KEY];
                privateConfigurationValues.Remove( CUSTOM_VALUES_PUBLIC_KEY );
            }
            else
            {
                privateConfigurationValues[VALUES_KEY] = "";
            }

            return privateConfigurationValues;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            var tbValues = new RockTextBox();
            tbValues.TextMode = TextBoxMode.MultiLine;
            tbValues.Rows = 3;
            tbValues.AutoPostBack = true;
            tbValues.TextChanged += OnQualifierUpdated;
            tbValues.Label = "Values";
            tbValues.Help = "The source of the values to display in a list.  Format is either 'value1,value2,value3,...', 'value1^text1,value2^text2,value3^text3,...', or a SQL Select statement that returns result set with a 'Value' and 'Text' column <span class='tip tip-lava'></span>.";
            controls.Add( tbValues );

            // option for Displaying an enhanced 'chosen' value picker
            var cbEnhanced = new RockCheckBox();
            cbEnhanced.AutoPostBack = true;
            cbEnhanced.CheckedChanged += OnQualifierUpdated;
            cbEnhanced.Label = "Enhance For Long Lists";
            cbEnhanced.Text = "Yes";
            cbEnhanced.Help = "When set, will render a searchable selection of options.";
            controls.Add( cbEnhanced );

            var tbRepeatColumns = new NumberBox();
            tbRepeatColumns.Label = "Columns";
            tbRepeatColumns.Help = "Select how many columns the list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no upper limit enforced here however the block this is used in might add constraints due to available space.";
            tbRepeatColumns.MinimumValue = "0";
            tbRepeatColumns.AutoPostBack = true;
            tbRepeatColumns.TextChanged += OnQualifierUpdated;
            controls.Add( tbRepeatColumns );

            var ddlRepeatDirection = new RockDropDownList();
            ddlRepeatDirection.Label = "Repeat Direction";
            ddlRepeatDirection.Help = "The direction that the list options will be displayed.";
            ddlRepeatDirection.BindToEnum<RepeatDirection>();
            ddlRepeatDirection.AutoPostBack = true;
            ddlRepeatDirection.TextChanged += OnQualifierUpdated;
            controls.Add( ddlRepeatDirection );

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = base.ConfigurationValues( controls );

            string description = "The source of the values to display in a list.  Format is either 'value1,value2,value3,...', 'value1^text1,value2^text2,value3^text3,...', or a SQL Select statement that returns result set with a 'Value' and 'Text' column <span class='tip tip-lava'></span>.";
            configurationValues.Add( VALUES_KEY, new ConfigurationValue( "Values", description, string.Empty ) );

            description = "When set, will render a searchable selection of options.";
            configurationValues.Add( ENHANCED_SELECTION_KEY, new ConfigurationValue( "Enhance For Long Lists", description, string.Empty ) );

            description = "Select how many columns the list should use before going to the next row. If blank 4 is used.";
            configurationValues.Add( REPEAT_COLUMNS, new ConfigurationValue( "Repeat Columns", description, string.Empty ) );

            description = "The direction that the list options will be displayed.";
            configurationValues.Add( REPEAT_DIRECTION, new ConfigurationValue( "Repeat Direction", description, string.Empty ) );

            if ( controls != null && controls.Count > 3 )
            {
                var tbValues = controls[0] as RockTextBox;
                var cbEnhanced = controls[1] as RockCheckBox;
                var tbRepeatColumns = controls[2] as NumberBox;
                var ddlRepeatDirection = controls[3] as RockDropDownList;

                tbRepeatColumns.Visible = !cbEnhanced.Checked;
                ddlRepeatDirection.Visible = !cbEnhanced.Checked;

                configurationValues[VALUES_KEY].Value = tbValues.Text;
                configurationValues[ENHANCED_SELECTION_KEY].Value = cbEnhanced.Checked.ToString();
                configurationValues[REPEAT_COLUMNS].Value = tbRepeatColumns.Visible ? tbRepeatColumns.Text : string.Empty;
                configurationValues[REPEAT_DIRECTION].Value = ddlRepeatDirection.SelectedValue;
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
            base.SetConfigurationValues( controls, configurationValues );

            if ( controls != null && controls.Count > 3 && configurationValues != null )
            {
                var tbValues = controls[0] as RockTextBox;
                var cbEnhanced = controls[1] as RockCheckBox;
                var tbRepeatColumns = controls[2] as NumberBox;
                var ddlRepeatDirection = controls[3] as RockDropDownList;

                tbValues.Text = configurationValues.ContainsKey( VALUES_KEY ) ? configurationValues[VALUES_KEY].Value : string.Empty;
                cbEnhanced.Checked = configurationValues.ContainsKey( ENHANCED_SELECTION_KEY ) ? configurationValues[ENHANCED_SELECTION_KEY].Value.AsBoolean() : cbEnhanced.Checked;
                tbRepeatColumns.Text = configurationValues.ContainsKey( REPEAT_COLUMNS ) ? configurationValues[REPEAT_COLUMNS].Value : string.Empty;
                tbRepeatColumns.Visible = !cbEnhanced.Checked;
                ddlRepeatDirection.SetValue( configurationValues.GetValueOrNull( REPEAT_DIRECTION ) );
                ddlRepeatDirection.Visible = !cbEnhanced.Checked;
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
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

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
            if ( configurationValues != null )
            {
                ListControl editControl = null;

                if ( configurationValues.ContainsKey( ENHANCED_SELECTION_KEY ) && configurationValues[ENHANCED_SELECTION_KEY].Value.AsBoolean() )
                {
                    editControl = new RockListBox { ID = id };
                    ( ( RockListBox ) editControl ).DisplayDropAsAbsolute = true;
                }
                else
                {
                    editControl = new RockCheckBoxList { ID = id };
                    var rockCheckBoxList = ( RockCheckBoxList ) editControl;

                    if ( configurationValues.ContainsKey( REPEAT_COLUMNS ) )
                    {
                        rockCheckBoxList.RepeatColumns = configurationValues[REPEAT_COLUMNS].Value.AsInteger();
                    }

                    if ( configurationValues.ContainsKey( REPEAT_DIRECTION ) )
                    {
                        rockCheckBoxList.RepeatDirection = configurationValues[REPEAT_DIRECTION].Value.ConvertToEnumOrNull<RepeatDirection>() ?? RepeatDirection.Horizontal;
                    }
                    else
                    {
                        rockCheckBoxList.RepeatDirection = RepeatDirection.Horizontal;
                    }
                }

                foreach ( var keyVal in Helper.GetConfiguredValues( configurationValues ) )
                {
                    editControl.Items.Add( new ListItem( keyVal.Value, keyVal.Key ) );
                }

                if ( editControl.Items.Count > 0 )
                {
                    return editControl;
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
            List<string> values = new List<string>();

            if ( control != null && control is ListControl )
            {
                ListControl cbl = ( ListControl ) control;
                foreach ( ListItem li in cbl.Items )
                {
                    if ( li.Selected )
                    {
                        values.Add( li.Value );
                    }
                }

                return values.AsDelimited<string>( "," );
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
                List<string> values = new List<string>();

                /*
                     01/23/2024 - NA

                     Because the "value" used here could be coming from a Lava "| Attribute: <key>" filter, it is possibly
                     going to be the "FormattedValue" -- which would be comma-space delmited (", ").  That has been the
                     case since v8.
                     (see https://github.com/SparkDevNetwork/Rock/commit/8f0a49f5e668a39e88891d76585c197c73bf24a0#diff-9cebf8ebf183910a1dfc469e485ed5f53ea3cbda27b6c1baf38cea7e36a85d14R155)

                     Reason: Fix issue #5706
                */
                values.AddRange( value.Split( ',' ).Select( s => s.Trim() ) );

                if ( control != null && control is ListControl )
                {
                    ListControl cbl = ( ListControl ) control;
                    foreach ( ListItem li in cbl.Items )
                    {
                        li.Selected = values.Contains( li.Value );
                    }
                }
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
            // call the base which render SelectMulti's List
            return base.FilterValueControl( configurationValues, id, required, filterMode );
        }

        /// <summary>
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            // call the base which will get SelectMulti's List values
            return base.GetFilterValueValue( control, configurationValues );
        }

        /// <summary>
        /// Sets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            // call the base which will set SelectMulti's List values
            base.SetFilterValueValue( control, configurationValues, value );
        }

#endif
        #endregion
    }
}