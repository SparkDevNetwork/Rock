//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataTransform.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people based on their total contribution amount" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Giving Amount Filter" )]
    public class GivingAmountFilter : DataFilterComponent
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
            get { return typeof( Rock.Model.Person ).FullName; }
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

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "Giving Amount";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before 
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            // Giving amount total {0} {1} between {2} and {3}
            return @"
function() {
    var comparisonText = $('select:first', $content).find(':selected').text();
    var totalAmount = $('.number-box', $content).find('input').val();
    var startDate = $('.date-range-picker', $content).find('input:first').val();
    var endDate = $('.date-range-picker', $content).find('input:last').val();

    return 'Giving amount total ' + comparisonText.toLowerCase() + ' $' + totalAmount + ' between ' + startDate + ' and ' + endDate;
}
";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = "Giving Amount";
            string[] selectionValues = selection.Split( '|' );

            if ( selectionValues.Length >= 4 )
            {
                ComparisonType comparisonType = selectionValues[0].ConvertToEnum<ComparisonType>( ComparisonType.GreaterThanOrEqualTo );
                decimal amount = selectionValues[1].AsDecimal() ?? 0.00M;
                DateTime startDate = selectionValues[2].AsDateTime() ?? DateTime.MinValue;
                DateTime endDate = selectionValues[3].AsDateTime() ?? DateTime.MaxValue;


                result = string.Format( "Giving amount total {0} {1} between {2} and {3}", comparisonType.ConvertToString().ToLower(), amount.ToString("C"), startDate.ToShortDateString(), endDate.ToShortDateString() );
            }

            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var comparisonControl = this.ComparisonControl( ComparisonType.LessThan | ComparisonType.GreaterThanOrEqualTo );
            comparisonControl.ID = filterControl.ID + "_0";
            filterControl.Controls.Add( comparisonControl );

            NumberBox numberBoxAmount = new NumberBox();
            numberBoxAmount.PrependText = "$";
            numberBoxAmount.NumberType = ValidationDataType.Currency;
            numberBoxAmount.ID = filterControl.ID + "_1";
            numberBoxAmount.Label = "Amount";

            filterControl.Controls.Add( numberBoxAmount );

            DateRangePicker dateRangePicker = new DateRangePicker();
            dateRangePicker.ID = filterControl.ID + "_2";
            dateRangePicker.Label = "Date Range";
            dateRangePicker.Required = true;
            filterControl.Controls.Add( dateRangePicker );

            var controls = new Control[3] { comparisonControl, numberBoxAmount, dateRangePicker };

            SetSelection( entityType, controls, string.Format( "{0}|||", ComparisonType.GreaterThanOrEqualTo.ConvertToInt().ToString() ) );

            return controls;
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            controls[0].RenderControl( writer );
            controls[1].RenderControl( writer );
            controls[2].RenderControl( writer );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            string comparisonType = ( (DropDownList)controls[0] ).SelectedValue;
            decimal? amount = ( controls[1] as NumberBox ).Text.AsDecimal();
            DateRangePicker dateRangePicker = controls[2] as DateRangePicker;
            return string.Format( "{0}|{1}|{2}|{3}", comparisonType, amount, dateRangePicker.LowerValue, dateRangePicker.UpperValue );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 4 )
            {
                var comparisonControl = controls[0] as DropDownList;
                var numberBox = controls[1] as NumberBox;
                var dateRangePicker = controls[2] as DateRangePicker;

                comparisonControl.SetValue( selectionValues[0] );
                decimal? amount = selectionValues[1].AsDecimal();
                if (amount.HasValue)
                {
                    numberBox.Text = amount.Value.ToString( "F2" );
                }
                else
                {
                    numberBox.Text = string.Empty;
                }
                
                dateRangePicker.LowerValue = selectionValues[2].AsDateTime();
                dateRangePicker.UpperValue = selectionValues[3].AsDateTime();
            }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, object serviceInstance, Expression parameterExpression, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length != 4 )
            {
                return null;
            }

            ComparisonType comparisonType = selectionValues[0].ConvertToEnum<ComparisonType>( ComparisonType.GreaterThanOrEqualTo );
            decimal amount = selectionValues[1].AsDecimal() ?? 0.00M;
            DateTime startDate = selectionValues[2].AsDateTime() ?? DateTime.MinValue;
            DateTime endDate = selectionValues[3].AsDateTime() ?? DateTime.MaxValue;

            var financialTransactionQry = new FinancialTransactionService().Queryable()
                .Where( xx => xx.TransactionDateTime >= startDate && xx.TransactionDateTime < endDate )
                .GroupBy( xx => xx.AuthorizedPersonId ).Select( xx =>
                    new
                    {
                        PersonId = xx.Key,
                        TotalAmount = xx.Sum( ss => ss.Amount )
                    } );

            if ( comparisonType == ComparisonType.LessThan )
            {
                financialTransactionQry = financialTransactionQry.Where( xx => xx.TotalAmount < amount );
            }
            else
            {
                financialTransactionQry = financialTransactionQry.Where( xx => xx.TotalAmount >= amount );
            }

            var innerQry = financialTransactionQry.Select( xx => xx.PersonId ?? 0 ).AsQueryable();

            var qry = new Rock.Data.Service<Rock.Model.Person>().Queryable()
                .Where( p => innerQry.Any( xx => xx == p.Id ) );

            Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

            return extractedFilterExpression;
        }

        #endregion
    }
}