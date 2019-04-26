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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    ///
    /// </summary>
    [Description( "Filter people on based on the current age in years" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Age" )]
    public class AgeFilter : DataFilterComponent
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
            return "Age";
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
    var result = 'Age';
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
                return string.Format( "Age is {0}", values[0] );
            }
            else if ( values.Length >= 2 )
            {
                ComparisonType comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.StartsWith );
                if ( comparisonType == ComparisonType.IsBlank || comparisonType == ComparisonType.IsNotBlank )
                {
                    return string.Format( "Age {0}", comparisonType.ConvertToString() );
                }
                else if (comparisonType == ComparisonType.Between)
                {
                    return string.Format( "Age {0} {1}", comparisonType.ConvertToString(), values[2].Replace(",", " and ") );
                }
                else
                {
                    return string.Format( "Age {0} '{1}'", comparisonType.ConvertToString(), values[1] );
                }
            }
            else
            {
                return "Age Filter";
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

            numberBox.FieldName = "Age";

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

            writer.AddAttribute( "class", "row form-row field-criteria" );
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
            DateTime currentDate = RockDateTime.Today;
            int currentDayOfYear = currentDate.DayOfYear;

            var values = selection.Split( '|' );

            ComparisonType comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
            int? ageValue = values[1].AsIntegerOrNull();

            var rockContext = (RockContext)serviceInstance.Context;

            var personAgeQuery = new PersonService( rockContext ).Queryable();
            MemberExpression idExpression = Expression.Property( parameterExpression, "Id" );
            Expression ageSelect = new Rock.Reporting.DataSelect.Person.AgeSelect().GetExpression( rockContext, idExpression, "" );

            if ( values.Length >= 3 && comparisonType == ComparisonType.Between )
            {
                var numberRangeEditor = new NumberRangeEditor();
                numberRangeEditor.DelimitedValues = values[2];

                decimal ageValueStart = numberRangeEditor.LowerValue ?? 0;
                decimal ageValueEnd = numberRangeEditor.UpperValue ?? decimal.MaxValue;
                var personAgeBetweenQuery = personAgeQuery.Where(
                  p => ( ( p.BirthDate > SqlFunctions.DateAdd( "year", -SqlFunctions.DateDiff( "year", p.BirthDate, currentDate ), currentDate )
                        ? SqlFunctions.DateDiff( "year", p.BirthDate, currentDate ) - 1
                        : SqlFunctions.DateDiff( "year", p.BirthDate, currentDate ) )
                    >= ageValueStart ) && ( ( p.BirthDate > SqlFunctions.DateAdd( "year", -SqlFunctions.DateDiff( "year", p.BirthDate, currentDate ), currentDate )
                        ? SqlFunctions.DateDiff( "year", p.BirthDate, currentDate ) - 1
                        : SqlFunctions.DateDiff( "year", p.BirthDate, currentDate ) )
                    <= ageValueEnd ) );

                BinaryExpression result = FilterExpressionExtractor.Extract<Rock.Model.Person>( personAgeBetweenQuery, parameterExpression, "p" ) as BinaryExpression;
                return result;
            }
            else
            {
                var personAgeEqualQuery = personAgeQuery.Where(
                          p => ( p.BirthDate > SqlFunctions.DateAdd( "year", -SqlFunctions.DateDiff( "year", p.BirthDate, currentDate ), currentDate )
                                ? SqlFunctions.DateDiff( "year", p.BirthDate, currentDate ) - 1
                                : SqlFunctions.DateDiff( "year", p.BirthDate, currentDate ) )
                            == ageValue );

                BinaryExpression compareEqualExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( personAgeEqualQuery, parameterExpression, "p" ) as BinaryExpression;
                BinaryExpression result = FilterExpressionExtractor.AlterComparisonType( comparisonType, compareEqualExpression, null );
                return result;
            }
        }

        #endregion
    }
}