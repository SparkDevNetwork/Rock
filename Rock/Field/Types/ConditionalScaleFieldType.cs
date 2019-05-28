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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Field.FieldType" />
    public class ConditionalScaleFieldType : DecimalFieldType
    {
        #region ConfigurationKeys

        private static class ConfigurationKey
        {
            public const string ConfigurationJSON = "ConfigurationJSON";
        }

        #endregion ConfigurationKeys

        #region Configuration

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var keys = base.ConfigurationKeys();
            keys.Add( ConfigurationKey.ConfigurationJSON );
            return keys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            var pnlRulesEditor = new Panel();

            var conditionalScaleRulesControlsRepeater = new Repeater { ID = "conditionalScaleRulesControlsRepeater" };
            conditionalScaleRulesControlsRepeater.ItemTemplate = new ConditionalScaleRangeRuleItemTemplate();
            conditionalScaleRulesControlsRepeater.ItemDataBound += ConditionalScaleRulesControlsRepeater_ItemDataBound;
            pnlRulesEditor.Controls.Add( conditionalScaleRulesControlsRepeater );

            LinkButton btnAddRule = new LinkButton { ID = "btnAddRule", CssClass = "btn btn-action btn-xs margin-b-md", Text = "<i class='fa fa-plus-circle'></i>", CausesValidation = false };
            btnAddRule.Click += BtnAddRule_Click;
            pnlRulesEditor.Controls.Add( btnAddRule );

            controls.Add( pnlRulesEditor );

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

            if ( controls == null || controls.Count < 1 )
            {
                return;
            }

            var pnlRulesEditor = controls[0] as Panel;

            var conditionalScaleRulesControlsRepeater = pnlRulesEditor.FindControl( "conditionalScaleRulesControlsRepeater" ) as Repeater;

            var configurationJSON = configurationValues.GetValueOrNull( ConfigurationKey.ConfigurationJSON );

            List<ConditionalScaleRangeRule> conditionalScaleRangeRuleList = configurationJSON.FromJsonOrNull<List<ConditionalScaleRangeRule>>() ?? new List<ConditionalScaleRangeRule>();
            conditionalScaleRangeRuleList = conditionalScaleRangeRuleList.OrderBy( a => a.RangeIndex ).ThenBy( a => a.Label ).ToList();
            if ( !conditionalScaleRangeRuleList.Any() )
            {
                conditionalScaleRangeRuleList.Add( new ConditionalScaleRangeRule { Guid = Guid.NewGuid() } );
            }

            conditionalScaleRulesControlsRepeater.DataSource = conditionalScaleRangeRuleList;
            conditionalScaleRulesControlsRepeater.DataBind();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the ConditionalScaleRulesControlsRepeater control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        private void ConditionalScaleRulesControlsRepeater_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var repeaterItem = e.Item;

            ConditionalScaleRangeRule conditionalScaleRangeRule = e.Item.DataItem as ConditionalScaleRangeRule;

            HtmlGenericContainer conditionalScaleRangeRuleContainer = repeaterItem.FindControl( "conditionalScaleRangeRuleContainer" ) as HtmlGenericContainer;

            var hfRangeGuid = conditionalScaleRangeRuleContainer.FindControl( "hfRangeGuid" ) as HiddenField;
            var labelTextBox = conditionalScaleRangeRuleContainer.FindControl( "labelTextBox" ) as TextBox;
            var highValueNumberBox = conditionalScaleRangeRuleContainer.FindControl( "highValueNumberBox" ) as NumberBox;
            var colorPicker = conditionalScaleRangeRuleContainer.FindControl( "colorPicker" ) as ColorPicker;
            var lowValueNumberBox = conditionalScaleRangeRuleContainer.FindControl( "lowValueNumberBox" ) as NumberBox;

            hfRangeGuid.Value = conditionalScaleRangeRule.Guid.ToString();

            labelTextBox.Text = conditionalScaleRangeRule.Label;

            // from http://stackoverflow.com/a/216705/1755417 (to trim trailing zeros)
            highValueNumberBox.Text = conditionalScaleRangeRule.HighValue?.ToString( "G29" );
            colorPicker.Text = conditionalScaleRangeRule.Color;
            lowValueNumberBox.Text = conditionalScaleRangeRule.LowValue?.ToString( "G29" );
        }

        /// <summary>
        /// Handles the Click event of the BtnAddRule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void BtnAddRule_Click( object sender, System.EventArgs e )
        {
            LinkButton btnAddRule = sender as LinkButton;
            var pnlRulesEditor = btnAddRule.Parent as Panel;
            var conditionalScaleRulesControlsRepeater = pnlRulesEditor.FindControl( "conditionalScaleRulesControlsRepeater" ) as Repeater;
            List<ConditionalScaleRangeRule> conditionalScaleRangeRuleList = GetRangeRulesListFromRepeaterControls( conditionalScaleRulesControlsRepeater );
            conditionalScaleRangeRuleList.Add( new ConditionalScaleRangeRule() { Guid = Guid.NewGuid() } );
            conditionalScaleRulesControlsRepeater.DataSource = conditionalScaleRangeRuleList;
            conditionalScaleRulesControlsRepeater.DataBind();
        }

        /// <summary>
        /// Gets the range rules list from repeater controls.
        /// </summary>
        /// <param name="conditionalScaleRulesControlsRepeater">The conditional scale rules controls repeater.</param>
        /// <returns></returns>
        private static List<ConditionalScaleRangeRule> GetRangeRulesListFromRepeaterControls( Repeater conditionalScaleRulesControlsRepeater )
        {
            List<ConditionalScaleRangeRule> conditionalScaleRangeRuleList = new List<ConditionalScaleRangeRule>();
            int rangeIndex = 0;
            foreach ( var repeaterItem in conditionalScaleRulesControlsRepeater.Items.OfType<RepeaterItem>() )
            {
                HtmlGenericContainer conditionalScaleRangeRuleContainer = repeaterItem.FindControl( "conditionalScaleRangeRuleContainer" ) as HtmlGenericContainer;
                var hfRangeGuid = conditionalScaleRangeRuleContainer.FindControl( "hfRangeGuid" ) as HiddenField;
                var labelTextBox = conditionalScaleRangeRuleContainer.FindControl( "labelTextBox" ) as TextBox;
                var highValueNumberBox = conditionalScaleRangeRuleContainer.FindControl( "highValueNumberBox" ) as NumberBox;
                var colorPicker = conditionalScaleRangeRuleContainer.FindControl( "colorPicker" ) as ColorPicker;
                var lowValueNumberBox = conditionalScaleRangeRuleContainer.FindControl( "lowValueNumberBox" ) as NumberBox;
                conditionalScaleRangeRuleList.Add( new ConditionalScaleRangeRule
                {
                    RangeIndex = rangeIndex++,
                    Guid = hfRangeGuid.Value.AsGuid(),
                    Label = labelTextBox.Text,
                    Color = colorPicker.Value,
                    HighValue = highValueNumberBox.Text.AsDecimalOrNull(),
                    LowValue = lowValueNumberBox.Text.AsDecimalOrNull()
                } );
            }

            return conditionalScaleRangeRuleList;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            var configurationValue = new ConfigurationValue( "Configuration JSON", "The JSON data used for the conditional formatting rules", string.Empty );
            var pnlRulesEditor = controls[0];
            var conditionalScaleRulesControlsRepeater = pnlRulesEditor.FindControl( "conditionalScaleRulesControlsRepeater" ) as Repeater;

            var rules = GetRangeRulesListFromRepeaterControls( conditionalScaleRulesControlsRepeater );

            configurationValue.Value = rules.ToJson();

            var values = base.ConfigurationValues( controls );
            values.Add( ConfigurationKey.ConfigurationJSON, configurationValue );

            return values;
        }

        #endregion Configuration

        #region Formatting

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary> 
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            decimal? rangeValue = value.AsDecimalOrNull();
            if ( rangeValue == null )
            {
                return string.Empty;
            }

            var configurationJSON = configurationValues.GetValueOrNull( ConfigurationKey.ConfigurationJSON );
            List<ConditionalScaleRangeRule> conditionalScaleRangeRuleList = configurationJSON.FromJsonOrNull<List<ConditionalScaleRangeRule>>() ?? new List<ConditionalScaleRangeRule>();

            var matchingRangeRule = conditionalScaleRangeRuleList.FirstOrDefault( a => ( a.HighValue ?? decimal.MaxValue ) >= rangeValue.Value && rangeValue.Value >= ( a.LowValue ?? decimal.MinValue ) );
            if ( matchingRangeRule != null )
            {
                return $"<span class='label scale-label' style='background-color:{matchingRangeRule.Color}'>{matchingRangeRule.Label}</span>";
            }
            else
            {
                // if out-of-range, display nothing
                return string.Empty;
            }
        }

        #endregion

        #region Classes

        /// <summary>
        /// 
        /// </summary>
        private class ConditionalScaleRangeRule
        {
            // Identification of the Rule (so that DataFilter configs know which RangeRule is referenced ) 
            public Guid Guid { get; set; }

            public int RangeIndex { get; set; }

            public string Label { get; set; }

            public string Color { get; set; }

            public decimal? HighValue { get; set; }

            public decimal? LowValue { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="System.Web.UI.ITemplate" />
        private class ConditionalScaleRangeRuleItemTemplate : ITemplate
        {
            /// <summary>
            /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control" /> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
            /// </summary>
            /// <param name="container">The <see cref="T:System.Web.UI.Control" /> object to contain the instances of controls from the inline template.</param>
            public void InstantiateIn( Control container )
            {
                HtmlGenericContainer conditionalScaleRangeRuleContainer = new HtmlGenericContainer { ID = $"conditionalScaleRangeRuleContainer", CssClass = "row" };

                var hfRangeGuid = new HiddenField { ID = "hfRangeGuid" };
                conditionalScaleRangeRuleContainer.Controls.Add( hfRangeGuid );

                Panel pnlColumn1 = new Panel { ID = "pnlColumn1", CssClass = "col-md-5" };
                var labelTextBox = new RockTextBox { ID = "labelTextBox", Placeholder = "Label", CssClass = "margin-b-md", Required = true };
                pnlColumn1.Controls.Add( labelTextBox );
                labelTextBox.Init += ( object sender, EventArgs e ) =>
                {
                    var parentValidationGroup = ( labelTextBox.FindFirstParentWhere( a => a is IHasValidationGroup ) as IHasValidationGroup )?.ValidationGroup;
                    labelTextBox.ValidationGroup = parentValidationGroup;
                };
                var highValueNumberBox = new NumberBox { ID = "highValueNumberBox", Placeholder = "High Value", CssClass = "margin-b-md" };
                pnlColumn1.Controls.Add( highValueNumberBox );
                conditionalScaleRangeRuleContainer.Controls.Add( pnlColumn1 );

                Panel pnlColumn2 = new Panel { ID = "pnlColumn2", CssClass = "col-md-5" };
                var colorPicker = new ColorPicker { ID = "colorPicker", CssClass = "margin-b-md" };
                pnlColumn2.Controls.Add( colorPicker );
                var lowValueNumberBox = new NumberBox { ID = "lowValueNumberBox", Placeholder = "Low Value", NumberType = ValidationDataType.Double };
                pnlColumn2.Controls.Add( lowValueNumberBox );
                conditionalScaleRangeRuleContainer.Controls.Add( pnlColumn2 );

                Panel pnlColumn3 = new Panel { ID = "pnlColumn3", CssClass = "col-md-2" };
                LinkButton btnDeleteRule = new LinkButton { ID = "btnDeleteRule", CssClass = "btn btn-danger btn-sm", CausesValidation = false, Text = "<i class='fa fa-times'></i>" };
                btnDeleteRule.Click += BtnDeleteRule_Click;
                pnlColumn3.Controls.Add( btnDeleteRule );
                conditionalScaleRangeRuleContainer.Controls.Add( pnlColumn3 );

                container.Controls.Add( conditionalScaleRangeRuleContainer );
                container.Controls.Add( new HtmlGenericControl( "hr" ) );
            }

            /// <summary>
            /// Handles the Click event of the BtnDeleteRule control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
            private void BtnDeleteRule_Click( object sender, System.EventArgs e )
            {
                LinkButton btnDeleteRule = sender as LinkButton;

                var repeaterItem = btnDeleteRule.FirstParentControlOfType<RepeaterItem>();
                var rangeIndexToDelete = repeaterItem.ItemIndex;
                var conditionalScaleRulesControlsRepeater = repeaterItem.NamingContainer as Repeater;
                List<ConditionalScaleRangeRule> conditionalScaleRangeRuleList = GetRangeRulesListFromRepeaterControls( conditionalScaleRulesControlsRepeater );
                conditionalScaleRangeRuleList = conditionalScaleRangeRuleList.Where( a => a.RangeIndex != rangeIndexToDelete ).ToList();
                conditionalScaleRulesControlsRepeater.DataSource = conditionalScaleRangeRuleList;
                conditionalScaleRulesControlsRepeater.DataBind();
            }
        }

        #endregion

        #region Filter Control

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
            var lbl = new Label();
            lbl.ID = string.Format( "{0}_lIs", id );
            lbl.AddCssClass( "data-view-filter-label" );
            lbl.Text = "Is";

            // hide the compare control when in SimpleFilter mode
            lbl.Visible = filterMode != FilterMode.SimpleFilter;

            return lbl;
        }

        /// <summary>
        /// Gets the type of the filter comparison.
        /// </summary>
        /// <value>
        /// The type of the filter comparison.
        /// </value>
        public override ComparisonType FilterComparisonType
        {
            // #TODO#, probably ok
            get { return ComparisonHelper.NumericFilterComparisonTypes; }
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
            var configurationJSON = configurationValues.GetValueOrNull( ConfigurationKey.ConfigurationJSON );
            List<ConditionalScaleRangeRule> conditionalScaleRangeRuleList = configurationJSON.FromJsonOrNull<List<ConditionalScaleRangeRule>>() ?? new List<ConditionalScaleRangeRule>();
            var cblRangeRules = new RockCheckBoxList { ID = $"cblRangeRules_{id}", RepeatDirection = RepeatDirection.Horizontal };
            cblRangeRules.AddCssClass( "js-filter-control" ).AddCssClass( "checkboxlist-group" );
            foreach ( var conditionalScaleRangeRule in conditionalScaleRangeRuleList.OrderBy( a => a.RangeIndex ) )
            {
                cblRangeRules.Items.Add( new ListItem( conditionalScaleRangeRule.Label, conditionalScaleRangeRule.Guid.ToString() ) );
            }

            return cblRangeRules;
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
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var cblRangeRules = control as RockCheckBoxList;
            return cblRangeRules.SelectedValues?.ToJson();
        }

        /// <summary>
        /// Sets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var cblRangeRules = control as RockCheckBoxList;
            var selectedRangeGuidList = value.FromJsonOrNull<List<Guid>>() ?? new List<Guid>();
            cblRangeRules.SetValues( selectedRangeGuidList );
        }

        /// <summary>
        /// Gets the filter format script.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="title">The title.</param>
        /// <returns></returns>
        /// <remarks>
        /// This script must set a javascript variable named 'result' to a friendly string indicating value of filter controls
        /// a '$selectedContent' should be used to limit script to currently selected filter fields
        /// </remarks>
        public override string GetFilterFormatScript( Dictionary<string, ConfigurationValue> configurationValues, string title )
        {
            string titleJs = System.Web.HttpUtility.JavaScriptStringEncode( title );
            var format = "return Rock.reporting.formatFilterForCheckBoxListFilterControl('{0}', $selectedContent);";
            return string.Format( format, titleJs );
        }

        /// <summary>
        /// Formats the filter values.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <returns></returns>
        public override string FormatFilterValues( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues )
        {
            if ( filterValues.Count < 2 )
            {
                return string.Empty;
            }

            var filterCompareType = filterValues[0];
            var filterCompareValues = filterValues[1].FromJsonOrNull<List<Guid>>() ?? new List<Guid>();
            var configurationJSON = configurationValues.GetValueOrNull( ConfigurationKey.ConfigurationJSON );
            List<ConditionalScaleRangeRule> conditionalScaleRangeRuleList = configurationJSON.FromJsonOrNull<List<ConditionalScaleRangeRule>>() ?? new List<ConditionalScaleRangeRule>();

            var selectedLabels = conditionalScaleRangeRuleList.Where( a => filterCompareValues.Contains( a.Guid ) ).Select( a => a.Label ).ToList();
            var selectLabelsText = selectedLabels.AsDelimited( "' OR '" );
            return "Is " + AddQuotes( selectLabelsText );
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
            if ( filterValues.Count < 2 )
            {
                return null;
            }

            var filterCompareValues = filterValues[1].FromJsonOrNull<List<Guid>>() ?? new List<Guid>();

            var configurationJSON = configurationValues.GetValueOrNull( ConfigurationKey.ConfigurationJSON );
            List<ConditionalScaleRangeRule> conditionalScaleRangeRuleList = configurationJSON.FromJsonOrNull<List<ConditionalScaleRangeRule>>() ?? new List<ConditionalScaleRangeRule>();

            MemberExpression propertyExpression = Expression.Property( parameterExpression, propertyName );

            Expression comparison = null;

            var conditionalScaleRangeRuleListToFilter = conditionalScaleRangeRuleList.Where( a => filterCompareValues.Contains( a.Guid ) ).ToList();
            foreach ( var conditionalScaleRangeRule in conditionalScaleRangeRuleListToFilter )
            {
                decimal lowValue = conditionalScaleRangeRule.LowValue ?? decimal.MinValue;
                decimal highValue = conditionalScaleRangeRule.HighValue ?? decimal.MaxValue;

                ConstantExpression constantExpressionLowValue = Expression.Constant( lowValue, typeof( decimal ) );
                ConstantExpression constantExpressionHighValue = Expression.Constant( highValue, typeof( decimal ) );

                var rangeBetweenExpression = ComparisonHelper.ComparisonExpression( ComparisonType.Between, propertyExpression, constantExpressionLowValue, constantExpressionHighValue );

                if ( comparison == null )
                {
                    comparison = rangeBetweenExpression;
                }
                else
                {
                    comparison = Expression.Or( comparison, rangeBetweenExpression );
                }
            }

            return comparison;
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
            string attributeValuePropertyName = this.AttributeValueFieldName;
            Type attributeValueFieldType = this.AttributeValueFieldType;
            var comparison = PropertyFilterExpression( configurationValues, filterValues, parameterExpression, attributeValuePropertyName, attributeValueFieldType );

            if ( comparison == null )
            {
                return new Rock.Data.NoAttributeFilterExpression();
            }

            return comparison;
        }

        #endregion Filter Control
    }
}
