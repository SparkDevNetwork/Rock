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

        /// <summary>
        /// Gets the grade label from GlobalAttributes
        /// </summary>
        /// <returns></returns>
        private string GetGlobalGradeLabel()
        {
            var value = GlobalAttributesCache.Read().GetValue( "core.GradeLabel" );
            return string.IsNullOrWhiteSpace( value ) ? "Grade" : value;
        }

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
            return GetGlobalGradeLabel();
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
            return string.Format( @"
function() {{
  var compareText = $('.js-filter-compare', $content).find(':selected').text();
  var compareValue = $('.js-filter-control', $content).filter(':visible').length ? $('.js-filter-control', $content).find(':selected').text() : '';
  var result = '{0} ' + compareText + ' ' + compareValue;
  
  return result;
}}
", GetGlobalGradeLabel().EscapeQuotes());
            
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
                return string.Format( "{0} is {1}", GetGlobalGradeLabel(), values[0] );
            }
            else if ( values.Length >= 2 )
            {
                var gradeNameValue = DefinedValueCache.Read( values[1].AsGuid() );
                string gradeDescription = gradeNameValue != null ? gradeNameValue.Description : "??";
                ComparisonType comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.StartsWith );
                if ( comparisonType == ComparisonType.IsBlank || comparisonType == ComparisonType.IsNotBlank )
                {
                    return string.Format( "{0} {1}", GetGlobalGradeLabel(), comparisonType.ConvertToString() );
                }
                else
                {
                    return string.Format( "{0} {1} '{2}'", GetGlobalGradeLabel(), comparisonType.ConvertToString(), gradeDescription );
                }
            }
            else
            {
                return GetGlobalGradeLabel() + " Filter";
            }
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var controls = new List<Control>();

            var ddlIntegerCompare = ComparisonHelper.ComparisonControl( ComparisonHelper.NumericFilterComparisonTypes );
            ddlIntegerCompare.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
            ddlIntegerCompare.AddCssClass( "js-filter-compare" );
            filterControl.Controls.Add( ddlIntegerCompare );
            controls.Add( ddlIntegerCompare );

            var ddlGradeDefinedValue = new RockDropDownList();
            ddlGradeDefinedValue.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
            ddlGradeDefinedValue.AddCssClass( "js-filter-control" );

            ddlGradeDefinedValue.Items.Clear();

            // add blank item as first item
            ddlGradeDefinedValue.Items.Add( new ListItem() );

            var schoolGrades = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() );
            if ( schoolGrades != null )
            {
                foreach ( var schoolGrade in schoolGrades.DefinedValues.OrderByDescending( a => a.Value.AsInteger() ) )
                {
                    ddlGradeDefinedValue.Items.Add( new ListItem( schoolGrade.Description, schoolGrade.Guid.ToString() ) );
                }
            }

            filterControl.Controls.Add( ddlGradeDefinedValue );
            controls.Add( ddlGradeDefinedValue );

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
            RockDropDownList ddlGradeDefinedValue = controls[1] as RockDropDownList;

            writer.AddAttribute( "class", "row field-criteria" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.AddAttribute( "class", "col-md-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlCompare.RenderControl( writer );
            writer.RenderEndTag();

            ComparisonType comparisonType = (ComparisonType)ddlCompare.SelectedValue.AsInteger();
            ddlGradeDefinedValue.Style[HtmlTextWriterStyle.Display] = ( comparisonType == ComparisonType.IsBlank || comparisonType == ComparisonType.IsNotBlank ) ? "none" : string.Empty;

            writer.AddAttribute( "class", "col-md-8" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlGradeDefinedValue.RenderControl( writer );
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
            RockDropDownList ddlGradeDefinedValue = controls[1] as RockDropDownList;

            return string.Format( "{0}|{1}", ddlCompare.SelectedValue, ddlGradeDefinedValue.SelectedValue );
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
            RockDropDownList ddlGradeDefinedValue = controls[1] as RockDropDownList;

            if ( values.Length == 2 )
            {
                ddlCompare.SelectedValue = values[0];
                ddlGradeDefinedValue.SetValue( values[1] );
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

            var values = selection.Split( '|' );
            ComparisonType comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
            Guid? gradeDefinedValueGuid = values[1].AsGuidOrNull();
            DefinedTypeCache gradeDefinedType = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() );
            DefinedValueCache gradeDefinedValue = gradeDefinedType.DefinedValues.FirstOrDefault( a => a.Guid == gradeDefinedValueGuid );
            int? gradeOffset = gradeDefinedValue != null ? gradeDefinedValue.Value.AsIntegerOrNull() : null;

            var personGradeQuery = new PersonService( (RockContext)serviceInstance.Context ).Queryable();

            // if the next MM/DD of a graduation isn't until next year, treat next year as the current school year
            int currentYearAdjustor = 0;
            if ( gradeTransitionDate.HasValue && !( RockDateTime.Now < gradeTransitionDate ) )
            {
                currentYearAdjustor = 1;
            }

            int currentSchoolYear = RockDateTime.Now.AddYears( currentYearAdjustor ).Year;

            if ( gradeTransitionDate.HasValue && gradeOffset.HasValue )
            {
                /*
                 * example (assuming defined values are the stock values):
                 * Billy graduates in 2020, the transition date is 6/1
                 * In other words, Billy graduates on 6/1/2020
                 * and current date is Feb 1, 2015.  
                 
                 * Stock Example:
                 * 9th Grade offset is 3
                 * 8th Grade offset is 4
                 * 7th Grade offset is 5
                 * 6th Grade offset is 6
                 * Billy graduates on 6/1/2020 and current date is Feb 1, 2015
                 * Therefore, his current grade offset is 5 yrs, which would mean he is in 7th grade
                 *                  * 
                 * If the filter is: 
                 *      Equal to 7th grade...
                 *          7th Graders would be included. 
                 *          Grade offset must be LessThanOrEqualTo 5 and GreaterThan 4
                 *      Not-Equal to 7th grade...
                 *          7th Graders would not be included. 
                 *          Grade offset must be LessThanOrEqualTo 4 or GreaterThan 5
                 *      Less than 7th grade..
                 *          7th Graders would not be included, 6th and younger would be included. 
                 *          Grade offset must be GreaterThan 5
                 *      Less than or Equal to 7th grade..
                 *          7th Graders and younger would be included. 
                 *          Grade offset must be GreaterThan 4
                 *      Greater than 7th grade..
                 *          7th Graders would not be included, 8th Graders and older would be included. 
                 *          Grade offset must be LessThanOrEqualTo 4
                 *      Greater than or Equal to 7th grade..
                 *          7th Graders and older would be included. 
                 *          Grade offset must be LessThanOrEqualTo 5
                 *          
                 * Combined Example:
                 * High School offset is 3
                 * Jr High offset is 5
                 * K-6 offset is 12
                 * Billy graduates on 6/1/2020 and current date is Feb 1, 2015
                 * Therefore, his current grade offset is 5 yrs, which would mean he is in Jr High
                 * 
                 * If the filter is: 
                 *      Equal to Jr High...
                 *          Jr High would be included. 
                 *          Grade offset must be LessThanOrEqualTo 5 and GreaterThan 3
                 *      Not-Equal to Jr High...
                 *          Jr High would not be included. 
                 *          Grade offset must be LessThanOrEqualTo 3 or GreaterThan 5
                 *      Less than Jr High..
                 *          Jr High would not be included, K-6 and younger would be included. 
                 *          Grade offset must be GreaterThan 5
                 *      Less than or Equal to Jr High..
                 *          Jr High and younger would be included. 
                 *          Grade offset must be GreaterThan 3
                 *      Greater than Jr High..
                 *          Jr High would not be included, High School and older would be included. 
                 *          Grade offset must be LessThanOrEqualTo 3
                 *      Greater than or Equal to Jr High..
                 *          Jr High and older would be included. 
                 *          Grade offset must be LessThanOrEqualTo 5
                 */

                DefinedValueCache nextGradeDefinedValue = gradeDefinedType.DefinedValues
                        .OrderByDescending( a => a.Value.AsInteger() ).Where( a => a.Value.AsInteger() < gradeOffset ).FirstOrDefault();
                int nextGradeOffset = nextGradeDefinedValue != null ? nextGradeDefinedValue.Value.AsInteger() : -1;

                switch ( comparisonType )
                { 
                    case ComparisonType.EqualTo:
                        // Include people who have have a grade offset LessThanOrEqualTo selected grade's offset, but GreaterThan the next grade's offset
                        personGradeQuery = personGradeQuery.Where( p => p.GraduationYear - currentSchoolYear <= gradeOffset
                            && p.GraduationYear - currentSchoolYear > nextGradeOffset );
                        break;

                    case ComparisonType.NotEqualTo:
                        // Include people who have have a grade offset LessThanOrEqualTo next grade's offset, or GreaterThan the selected grade's offset (and not already graduated)
                        personGradeQuery = personGradeQuery.Where( p => ( p.GraduationYear - currentSchoolYear <= nextGradeOffset
                            || p.GraduationYear - currentSchoolYear > gradeOffset )
                            && p.GraduationYear - currentSchoolYear >= 0 );
                        break;

                    case ComparisonType.LessThan:
                        // Grade offset must be GreaterThan selected grade's offset
                        personGradeQuery = personGradeQuery.Where( p => p.GraduationYear - currentSchoolYear > gradeOffset );
                        break;

                    case ComparisonType.LessThanOrEqualTo:
                        // Grade offset must be GreaterThan next grade's offset
                        personGradeQuery = personGradeQuery.Where( p => p.GraduationYear - currentSchoolYear > nextGradeOffset );
                        break;

                    case ComparisonType.GreaterThan:
                        // Grade offset must be LessThanOrEqualTo next grade's offset (and not already graduated)
                        personGradeQuery = personGradeQuery.Where( p => p.GraduationYear - currentSchoolYear <= nextGradeOffset
                            && p.GraduationYear - currentSchoolYear >= 0 );
                        break;

                    case ComparisonType.GreaterThanOrEqualTo:
                        // Grade offset must be LessThanOrEqualTo selected grade's offset (and not already graduated)
                        personGradeQuery = personGradeQuery.Where( p => p.GraduationYear - currentSchoolYear <= gradeOffset
                            && p.GraduationYear - currentSchoolYear >= 0 );
                        break;

                    case ComparisonType.IsBlank:
                        // only return people that don't have a graduation year, or have already graduated
                        personGradeQuery = personGradeQuery.Where( p => !p.GraduationYear.HasValue || (p.GraduationYear - currentSchoolYear) < 0 );
                        break;

                    case ComparisonType.IsNotBlank:
                        // only return people that have a graduation date, and haven't graduated yet
                        personGradeQuery = personGradeQuery.Where( p => p.GraduationYear.HasValue && ( p.GraduationYear - currentSchoolYear ) >= 0 );
                        break;
                }

                Expression result = FilterExpressionExtractor.Extract<Rock.Model.Person>( personGradeQuery, parameterExpression, "p" );
                return result;
            }
            else
            {
                if ( !gradeTransitionDate.HasValue )
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
                }
                else
                {
                    // there is a grade transition date, but the selected gradeOffset is null
                    if ( comparisonType == ComparisonType.IsBlank )
                    {
                        // if trying to find people without a Grade only include people that don't have a graduation date or already graduated
                        personGradeQuery = personGradeQuery.Where( p => !p.GraduationYear.HasValue || p.GraduationYear.Value < currentSchoolYear );
                    }
                    else if ( comparisonType == ComparisonType.IsNotBlank )
                    {
                        // if trying to find people with a Grade only include people that have a graduation date and haven't already graduated
                        personGradeQuery = personGradeQuery.Where( p => p.GraduationYear.HasValue && !( p.GraduationYear.Value < currentSchoolYear ) );
                    }
                    else
                    {
                        // if no grade selected and they are comparing return false (nobody meets the condition since the condition is invalid)
                        personGradeQuery = personGradeQuery.Where( p => false );
                    }
                }

                return FilterExpressionExtractor.Extract<Rock.Model.Person>( personGradeQuery, parameterExpression, "p" );
            }
        }

        #endregion
    }
}