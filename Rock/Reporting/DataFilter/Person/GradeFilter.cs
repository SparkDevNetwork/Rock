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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people on based on the current grade" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Grade" )]
    public class GradeFilter : DataFilterComponent
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
            return "Grade";
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
            return @"'Grade ' + $('select', $content).find(':selected').text() + ( $('input', $content).filter(':visible').length ?  (' \'' +  $('input', $content).filter(':visible').val()  + '\'') : '' ) ";
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
                return string.Format( "Grade is {0}", values[0] );
            }
            else if ( values.Length >= 2 )
            {
                ComparisonType comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.StartsWith );
                if ( comparisonType == ComparisonType.IsBlank || comparisonType == ComparisonType.IsNotBlank )
                {
                    return string.Format( "Grade {0}", comparisonType.ConvertToString() );
                }
                else
                {
                    return string.Format( "Grade {0} '{1}'", comparisonType.ConvertToString(), values[1] );
                }
            }
            else
            {
                return "Grade Filter";
            }
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var controls = new List<Control>();

            var ddlIntegerCompare = ComparisonControl( NumericFilterComparisonTypes );
            ddlIntegerCompare.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
            ddlIntegerCompare.AddCssClass( "js-filter-compare" );
            filterControl.Controls.Add( ddlIntegerCompare );
            controls.Add( ddlIntegerCompare );

            var numberBox = new NumberBox();
            numberBox.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
            numberBox.AddCssClass( "js-filter-control" );
            filterControl.Controls.Add( numberBox );
            controls.Add( numberBox );

            numberBox.FieldName = "Grade";

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
            
            writer.AddAttribute( "class", "row field-criteria" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.AddAttribute( "class", "col-md-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlCompare.RenderControl( writer );
            writer.RenderEndTag();

            ComparisonType comparisonType = (ComparisonType)( ddlCompare.SelectedValue.AsInteger() );
            nbValue.Style[HtmlTextWriterStyle.Display] = ( comparisonType == ComparisonType.IsBlank || comparisonType == ComparisonType.IsNotBlank ) ? "none" : string.Empty;

            writer.AddAttribute( "class", "col-md-8" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            nbValue.RenderControl( writer );
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

            return string.Format( "{0}|{1}", ddlCompare.SelectedValue, nbValue.Text );
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

            if ( values.Length == 2 )
            {
                ddlCompare.SelectedValue = values[0];
                nbValue.Text = values[1];
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
            // GradeTransitionDate is stored as just MM/DD so it'll resolve to the current year
            DateTime? gradeTransitionDate = GlobalAttributesCache.Read().GetValue( "GradeTransitionDate" ).AsDateTime();
            int? gradeMaxFactorReactor = null;
            int currentYear = RockDateTime.Now.Year;
            var values = selection.Split( '|' );
            ComparisonType comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
            int? gradeValue = values[1].AsIntegerOrNull();
            var personGradeQuery = new PersonService( (RockContext)serviceInstance.Context ).Queryable();

            if ( gradeTransitionDate.HasValue )
            {
                gradeMaxFactorReactor = ( RockDateTime.Now < gradeTransitionDate ) ? 12 : 13;
                var personEqualGradeQuery = personGradeQuery.Where( p => ( gradeMaxFactorReactor - ( SqlFunctions.DatePart( "year", p.GraduationDate ) - currentYear ) == gradeValue ) );
                BinaryExpression compareEqualExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( personEqualGradeQuery, parameterExpression, "p" ) as BinaryExpression;
                BinaryExpression result = FilterExpressionExtractor.AlterComparisonType( comparisonType, compareEqualExpression, null );
                return result;
            }
            else
            {
                if ( comparisonType == ComparisonType.IsBlank )
                {
                    // if no gradeTransitionDate, return true (everybody has a blank grade)
                    personGradeQuery = personGradeQuery.Where( p => true );
                }
                else
                {
                    // if no gradeTransitionDate, return false (nobody has a grade)
                    personGradeQuery = personGradeQuery.Where( p => false );
                }

                return FilterExpressionExtractor.Extract<Rock.Model.Person>( personGradeQuery, parameterExpression, "p" );
            }
        }

        #endregion
    }
}