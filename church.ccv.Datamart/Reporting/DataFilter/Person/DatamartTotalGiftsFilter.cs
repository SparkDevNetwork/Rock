using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using church.ccv.Datamart.Model;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace church.ccv.Datamart.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people based on the TotalGifts value of the Datamart Person record." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Total Gifts Filter" )]
    public class DatamartTotalGiftsFilter : DataFilterComponent
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
            get { return "Datamart Filters"; }
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
            return "Total Gifts";
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
            return @"
function() {
    var result = 'TotalGifts';
    var compareTypeText = $('.js-filter-compare :selected', $content).text();
    if ( $('.js-filter-control', $content).is(':visible') ) {
        var compareValueSingle = $('.js-filter-control', $content).val()    
        result += ' ' + compareTypeText + ' ' + (compareValueSingle || '');
    }
    else if ( $('.js-filter-control-between', $content).is(':visible') ) {
        var compareValueBetween = $('.js-filter-control-between .js-number-range-lower', $content).filter(':visible').val() + ' and ' + $('.js-filter-control-between .js-number-range-upper', $content).filter(':visible').val()
        result += ' ' + compareTypeText + ' ' + compareValueBetween;
    }
    else {
        result += ' ' + compareTypeText;
    }

    return result;
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
            var values = selection.Split( '|' );
            if ( values.Length == 1 )
            {
                return string.Format( "Total Gifts is {0}", values[0] );
            }
            else if ( values.Length >= 2 )
            {
                ComparisonType comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.StartsWith );
                if ( comparisonType == ComparisonType.IsBlank || comparisonType == ComparisonType.IsNotBlank )
                {
                    return string.Format( "Total Gifts {0}", comparisonType.ConvertToString() );
                }
                else if ( comparisonType == ComparisonType.Between )
                {
                    return string.Format( "Total Gifts {0} {1}", comparisonType.ConvertToString(), values[2].Replace( ",", " and " ) );
                }
                else
                {
                    return string.Format( "Total Gifts {0} '{1}'", comparisonType.ConvertToString(), values[1] );
                }
            }
            else
            {
                return "Total Gifts Filter";
            }
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var controls = new List<Control>();

            var ddlIntegerCompare = ComparisonHelper.ComparisonControl( ComparisonHelper.NumericFilterComparisonTypes | ComparisonType.Between );
            ddlIntegerCompare.ID = string.Format( "{0}_{1}", filterControl.ID, "ddlIntegerCompare" );
            ddlIntegerCompare.AddCssClass( "js-filter-compare" );
            filterControl.Controls.Add( ddlIntegerCompare );
            controls.Add( ddlIntegerCompare );

            var numberBox = new NumberBox();
            numberBox.ID = string.Format( "{0}_{1}", filterControl.ID, "numberBox" );
            numberBox.AddCssClass( "js-filter-control" );
            filterControl.Controls.Add( numberBox );
            controls.Add( numberBox );

            numberBox.FieldName = "TotalGifts";

            var numberRangeEditor = new Rock.Web.UI.Controls.NumberRangeEditor();
            numberRangeEditor.ID = string.Format( "{0}_{1}", filterControl.ID, "numberRangeEditor" );
            numberRangeEditor.RangeLabel = "and";
            numberRangeEditor.AddCssClass( "js-filter-control-between" );
            filterControl.Controls.Add( numberRangeEditor );
            controls.Add( numberRangeEditor );

            return controls.ToArray();
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
            DropDownList ddlCompare = controls[0] as DropDownList;
            NumberBox nbValue = controls[1] as NumberBox;
            NumberRangeEditor numberRangeEditor = controls[2] as NumberRangeEditor;

            writer.AddAttribute( "class", "row field-criteria" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.AddAttribute( "class", "col-md-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlCompare.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-8" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            nbValue.RenderControl( writer );
            numberRangeEditor.RenderControl( writer );

            writer.RenderEndTag();

            writer.RenderEndTag();  // row

            RegisterFilterCompareChangeScript( filterControl );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            DropDownList ddlCompare = controls[0] as DropDownList;
            NumberBox nbValue = controls[1] as NumberBox;
            NumberRangeEditor numberRangeEditor = controls[2] as NumberRangeEditor;

            return string.Format( "{0}|{1}|{2}", ddlCompare.SelectedValue, nbValue.Text, numberRangeEditor.DelimitedValues );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var values = selection.Split( '|' );

            DropDownList ddlCompare = controls[0] as DropDownList;
            NumberBox nbValue = controls[1] as NumberBox;
            NumberRangeEditor numberRangeEditor = controls[2] as NumberRangeEditor;

            if ( values.Length >= 2 )
            {
                ddlCompare.SelectedValue = values[0];
                nbValue.Text = values[1];
                if ( values.Length >= 3 )
                {
                    numberRangeEditor.DelimitedValues = values[2];
                }
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
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var values = selection.Split( '|' );

            ComparisonType comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
            int? totalGiftsValue = values[1].AsIntegerOrNull();

            var rockContext = (RockContext)serviceInstance.Context;
            var qryDatamartPerson = new DatamartPersonService( rockContext ).Queryable();

            if ( values.Length >= 3 && comparisonType == ComparisonType.Between )
            {
                var numberRangeEditor = new NumberRangeEditor();
                numberRangeEditor.DelimitedValues = values[2];

                decimal totalGiftValueStart = numberRangeEditor.LowerValue ?? 0;
                decimal totalGiftValueEnd = numberRangeEditor.UpperValue ?? decimal.MaxValue;

                qryDatamartPerson = qryDatamartPerson.Where( p => p.TotalGifts >= totalGiftValueStart );

                qryDatamartPerson = qryDatamartPerson.Where( p => p.TotalGifts <= totalGiftValueEnd );

                var qry = new PersonService( rockContext ).Queryable()
                    .Where( p => qryDatamartPerson.Any( xx => xx.PersonId == p.Id ) );

                Expression result = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

                return result;
            }
            else if ( comparisonType == ComparisonType.NotEqualTo )
            {
                qryDatamartPerson = qryDatamartPerson.Where( p => p.TotalGifts != totalGiftsValue );

                var qry = new PersonService( rockContext ).Queryable()
                    .Where( p => qryDatamartPerson.Any( xx => xx.PersonId == p.Id ) );

                Expression result = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

                return result;
            }
            else if ( comparisonType == ComparisonType.IsBlank )
            {
                qryDatamartPerson = qryDatamartPerson.Where( p => p.TotalGifts == null );

                var qry = new PersonService( rockContext ).Queryable()
                    .Where( p => qryDatamartPerson.Any( xx => xx.PersonId == p.Id ) );

                Expression result = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

                return result;
            }
            else if ( comparisonType == ComparisonType.IsNotBlank )
            {
                qryDatamartPerson = qryDatamartPerson.Where( p => p.TotalGifts != null );

                var qry = new PersonService( rockContext ).Queryable()
                    .Where( p => qryDatamartPerson.Any( xx => xx.PersonId == p.Id ) );

                Expression result = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

                return result;
            }
            else if ( comparisonType == ComparisonType.GreaterThan )
            {
                qryDatamartPerson = qryDatamartPerson.Where( p => p.TotalGifts > totalGiftsValue );

                var qry = new PersonService( rockContext ).Queryable()
                    .Where( p => qryDatamartPerson.Any( xx => xx.PersonId == p.Id ) );

                Expression result = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

                return result;
            }
            else if ( comparisonType == ComparisonType.GreaterThanOrEqualTo )
            {
                qryDatamartPerson = qryDatamartPerson.Where( p => p.TotalGifts >= totalGiftsValue );

                var qry = new PersonService( rockContext ).Queryable()
                    .Where( p => qryDatamartPerson.Any( xx => xx.PersonId == p.Id ) );

                Expression result = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

                return result;
            }
            else if ( comparisonType == ComparisonType.LessThan )
            {
                qryDatamartPerson = qryDatamartPerson.Where( p => p.TotalGifts < totalGiftsValue );

                var qry = new PersonService( rockContext ).Queryable()
                    .Where( p => qryDatamartPerson.Any( xx => xx.PersonId == p.Id ) );

                Expression result = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

                return result;
            }
            else if ( comparisonType == ComparisonType.LessThanOrEqualTo )
            {
                qryDatamartPerson = qryDatamartPerson.Where( p => p.TotalGifts <= totalGiftsValue );

                var qry = new PersonService( rockContext ).Queryable()
                    .Where( p => qryDatamartPerson.Any( xx => xx.PersonId == p.Id ) );

                Expression result = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

                return result;
            }
            else
            {
                var qryTotalGiftsEquals = qryDatamartPerson.Where( p => p.TotalGifts == totalGiftsValue );

                var qry = new PersonService( rockContext ).Queryable()
                    .Where( p => qryTotalGiftsEquals.Any( xx => xx.PersonId == p.Id ) );

                Expression result = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

                return result;
            }
        }

        #endregion
    }
}