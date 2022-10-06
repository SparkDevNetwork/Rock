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
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Reporting.DataFilter
{
    /// <summary>
    /// Operates against giving alerts
    /// </summary>
    [Description( "Filter people based on their giving alert options" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Giving Alerts Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "735AC377-7E41-4602-AB54-5B624B559145" )]
    public class GivingAlertsFilter : DataFilterComponent
    {
        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return "Rock.Model.Person"; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Additional Filters"; }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Gets the user-friendly title used to identify the filter component.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <returns>
        /// The name of the filter.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override string GetTitle( Type entityType )
        {
            return "Giving Alerts";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before
        /// referencing this property.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <returns>
        /// The client format script.
        /// </returns>
        public override string GetClientFormatSelection( Type entityType )
        {
            return @"
function() {
    var result = 'Giving Alerts of type: ';

    var alertTypes = $('.js-giving-alerts', $content).find(':selected');
    if ( alertTypes.length > 0 ) { 
        var alertTypesDelimitedList = alertTypes.map(function() {{ return $(this).text() }}).get().join(', ');
        result += alertTypesDelimitedList +"". "";
    }

    let comparisonSelect = document.querySelector('.js-filter-compare');
    let comparisonText = comparisonSelect.options[comparisonSelect.selectedIndex].text;

    if(comparisonText) {
        result += comparisonText;
    }

    let amountNumberBox = document.querySelector('.js-amount');
    let amount = amountNumberBox.querySelector('.form-control').value;

    if(amount){
        result += "" $"" + amount;
    }

    var dateRangeText = $('.js-slidingdaterange-text-value', $content).val();
    if( dateRangeText ) {{
        result +=  "" from: "" + dateRangeText
    }}

    return result;
}
";
        }

        /// <summary>
        /// Provides a user-friendly description of the specified filter values.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        /// <returns>
        /// A string containing the user-friendly description of the settings.
        /// </returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );

            var result = "Giving Alerts of type";

            if ( selectionConfig != null )
            {
                if ( selectionConfig.TransactionAlertTypeIds.Count > 0 )
                {
                    var alertTypeNames = new List<string>();
                    foreach ( var transactionAlertTypeId in selectionConfig.TransactionAlertTypeIds )
                    {
                        var transactionAlertType = new FinancialTransactionAlertTypeService( new RockContext() )
                            .Get( transactionAlertTypeId );
                        if ( transactionAlertType != null )
                        {
                            alertTypeNames.Add( transactionAlertType.Name );
                        }
                    }

                    result += string.Format( ": {0}.", alertTypeNames.AsDelimited( "," ) );
                }

                var comparisonType = selectionConfig.ComparisonValue.ConvertToEnumOrNull<ComparisonType>();
                result += $" {comparisonType.ConvertToString()}: ${selectionConfig.Amount}";

                if ( selectionConfig.DelimitedDateRangeValues.IsNotNullOrWhiteSpace() )
                {
                    var dateRangeString = SlidingDateRangePicker.FormatDelimitedValues( selectionConfig.DelimitedDateRangeValues );
                    if ( dateRangeString.IsNotNullOrWhiteSpace() )
                    {
                        result += $" from: {dateRangeString}";
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Creates the model representation of the child controls used to display and edit the filter settings.
        /// Implement this version of CreateChildControls if your DataFilterComponent works the same in all filter modes
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="filterControl">The control that serves as the container for the filter controls.</param>
        /// <returns>
        /// The array of new controls created to implement the filter.
        /// </returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var controls = new List<Control>();
            var rockContext = new RockContext();

            var rlbGivingAlerts = new RockListBox();
            rlbGivingAlerts.Label = "Alert Name";
            rlbGivingAlerts.Required = true;
            rlbGivingAlerts.ID = filterControl.GetChildControlInstanceName( "rlbGivingAlerts" );
            rlbGivingAlerts.CssClass = "js-giving-alerts";
            rlbGivingAlerts.Items.Clear();
            rlbGivingAlerts.Items.AddRange( GetGivingAlertsListItems( rockContext ).ToArray() );
            filterControl.Controls.Add( rlbGivingAlerts );
            controls.Add( rlbGivingAlerts );

            var ddlIntegerCompare = ComparisonHelper.ComparisonControl( ComparisonHelper.NumericFilterComparisonTypes | ComparisonType.StartsWith );
            ddlIntegerCompare.ID = string.Format( "{0}_{1}", filterControl.ID, "ddlIntegerCompare" );
            ddlIntegerCompare.AddCssClass( "js-filter-compare" );
            filterControl.Controls.Add( ddlIntegerCompare );
            controls.Add( ddlIntegerCompare );

            var currencyBox = new CurrencyBox();
            currencyBox.Label = "Amount";
            currencyBox.ID = string.Format( "{0}_{1}", filterControl.ID, "numberBox" );
            currencyBox.AddCssClass( "js-amount" );
            filterControl.Controls.Add( currencyBox );
            controls.Add( currencyBox );

            currencyBox.FieldName = "Amount";

            // Date Started
            var slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.ID = filterControl.GetChildControlInstanceName( "slidingDateRangePicker" );
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            slidingDateRangePicker.Label = "Date Range";
            slidingDateRangePicker.Help = "The date range that the alert occurred.";
            slidingDateRangePicker.Required = true;
            filterControl.Controls.Add( slidingDateRangePicker );
            controls.Add( slidingDateRangePicker );

            return controls.ToArray();
        }

        private List<ListItem> GetGivingAlertsListItems( RockContext rockContext )
        {
            var channels = new FinancialTransactionAlertTypeService( rockContext )
                .Queryable()
                .Select( ic => new ListItem() { Text = ic.Name, Value = ic.Id.ToString() } )
                .ToList();

            return channels.OrderBy( m => m.Text ).ToList();
        }

        /// <summary>
        /// Renders the child controls used to display and edit the filter settings for HTML presentation.
        /// Implement this version of RenderControls if your DataFilterComponent works the same in all FilterModes
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="filterControl">The control that serves as the container for the controls being rendered.</param>
        /// <param name="writer">The writer being used to generate the HTML for the output page.</param>
        /// <param name="controls">The model representation of the child controls for this component.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            RockListBox rlbGivingAlerts = controls[0] as RockListBox;
            DropDownList ddlCompare = controls[1] as DropDownList;
            CurrencyBox cbAmount = controls[2] as CurrencyBox;
            SlidingDateRangePicker slidingDateRangePicker = controls[3] as SlidingDateRangePicker;

            writer.AddAttribute( "class", "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div ); // row

            writer.AddAttribute( "class", "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div ); // col-md-6

            rlbGivingAlerts.RenderControl( writer ); // rlbGivingAlerts

            ddlCompare.RenderControl( writer ); // ddlCompare

            cbAmount.RenderControl( writer ); // cbAmount

            slidingDateRangePicker.RenderControl( writer ); // dateRange

            writer.RenderEndTag(); // col-md-6

            writer.RenderEndTag();  // row
        }

        /// <summary>
        /// Gets the selection.
        /// Implement this version of GetSelection if your DataFilterComponent works the same in all FilterModes
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="controls">The collection of controls used to set the filter values.</param>
        /// <returns>
        /// A formatted string representing the filter settings.
        /// </returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            RockListBox rlbGivingAlerts = controls[0] as RockListBox;
            DropDownList ddlCompare = controls[1] as DropDownList;
            CurrencyBox cbAmount = controls[2] as CurrencyBox;
            SlidingDateRangePicker dateRange = controls[3] as SlidingDateRangePicker;

            var selectionConfig = new SelectionConfig();
            selectionConfig.ComparisonValue = ddlCompare.SelectedValue;
            selectionConfig.DelimitedDateRangeValues = dateRange.DelimitedValues;
            selectionConfig.Amount = cbAmount.IntegerValue ?? 0;
            selectionConfig.TransactionAlertTypeIds = rlbGivingAlerts.SelectedValuesAsInt;

            return selectionConfig.ToJson();
        }

        /// <summary>
        /// Sets the selection.
        /// Implement this version of SetSelection if your DataFilterComponent works the same in all FilterModes
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );

            RockListBox rlbWebsites = controls[0] as RockListBox;
            DropDownList ddlCompare = controls[1] as DropDownList;
            CurrencyBox cbAmount = controls[2] as CurrencyBox;
            SlidingDateRangePicker dateRange = controls[3] as SlidingDateRangePicker;

            ddlCompare.SelectedValue = selectionConfig.ComparisonValue;
            cbAmount.IntegerValue = selectionConfig.Amount;
            rlbWebsites.SetValues( selectionConfig.TransactionAlertTypeIds );
            dateRange.DelimitedValues = selectionConfig.DelimitedDateRangeValues;
        }

        /// <summary>
        /// Creates a Linq Expression that can be applied to an IQueryable to filter the result set.
        /// </summary>
        /// <param name="entityType">The type of entity in the result set.</param>
        /// <param name="serviceInstance">A service instance that can be queried to obtain the result set.</param>
        /// <param name="parameterExpression">The input parameter that will be injected into the filter expression.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        /// <returns>
        /// A Linq Expression that can be used to filter an IQueryable.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );
            var comparisonType = selectionConfig.ComparisonValue.ConvertToEnumOrNull<ComparisonType>();
            var rockContext = ( RockContext ) serviceInstance.Context;
            var qry = new FinancialTransactionAlertService( rockContext ).Queryable();

            if ( selectionConfig.TransactionAlertTypeIds.Count > 0 )
            {
                qry = qry.Where( ft => selectionConfig.TransactionAlertTypeIds.Contains( ft.AlertTypeId ) );
            }

            if ( selectionConfig.DelimitedDateRangeValues.IsNotNullOrWhiteSpace() )
            {
                var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( selectionConfig.DelimitedDateRangeValues );
                if ( dateRange.Start.HasValue )
                {
                    qry = qry.Where( n => n.AlertDateTime >= dateRange.Start.Value );
                }

                if ( dateRange.End.HasValue )
                {
                    qry = qry.Where( n => n.AlertDateTime <= dateRange.End.Value );
                }
            }

            if ( comparisonType != null && selectionConfig.Amount > 0 )
            {
                switch ( comparisonType )
                {
                    case ComparisonType.EqualTo:
                        qry = qry.Where( ft => ft.Amount == selectionConfig.Amount );
                        break;
                    case ComparisonType.LessThan:
                        qry = qry.Where( ft => ft.Amount < selectionConfig.Amount );
                        break;
                    case ComparisonType.LessThanOrEqualTo:
                        qry = qry.Where( ft => ft.Amount <= selectionConfig.Amount );
                        break;
                    case ComparisonType.GreaterThan:
                        qry = qry.Where( ft => ft.Amount > selectionConfig.Amount );
                        break;
                    case ComparisonType.GreaterThanOrEqualTo:
                        qry = qry.Where( ft => ft.Amount >= selectionConfig.Amount );
                        break;
                }
            }

            var transactionPersonsKey = qry.Select( a => a.PersonAliasId );
            var personQry = new PersonService( rockContext )
                .Queryable()
                .Where( p => p.Aliases.Any( pa => transactionPersonsKey.Contains( pa.Id ) ) );

            return FilterExpressionExtractor.Extract<Rock.Model.Person>( personQry, parameterExpression, "p" );
        }

        #endregion Public Methods

        /// <summary>
        /// Get and set the filter settings from DataViewFilter.Selection
        /// </summary>
        protected class SelectionConfig
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SelectionConfig"/> class.
            /// </summary>
            public SelectionConfig()
            {
            }

            /// <summary>
            /// Gets or sets the note type identifiers.
            /// </summary>
            /// <value>
            /// The note type identifiers.
            /// </value>
            public List<int> TransactionAlertTypeIds { get; set; }

            /// <summary>
            /// Gets a pipe delimited string of the property values. This is to use the SlidingDateRangePicker's existing logic.
            /// </summary>
            /// <value>
            /// The delimited values.
            /// </value>
            public string DelimitedDateRangeValues { get; set; }

            /// <summary>
            /// Gets or sets the minimum count.
            /// </summary>
            /// <value>
            /// The minimum count.
            /// </value>
            public int Amount { get; set; }

            /// <summary>
            /// The date range comparison value
            /// </summary>
            public string ComparisonValue { get; set; }

            /// <summary>
            /// Parses the specified selection from a JSON or delimited string.
            /// </summary>
            /// <param name="selection">The selection.</param>
            /// <returns></returns>
            public static SelectionConfig Parse( string selection )
            {
                return selection.FromJsonOrNull<SelectionConfig>() ?? new SelectionConfig();
            }
        }
    }
}