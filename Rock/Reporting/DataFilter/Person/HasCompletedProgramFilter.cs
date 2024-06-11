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
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// The Data View Filter  responsible for determining if a <see cref="Person"/> has completed a <see cref="LearningProgram"/>.
    /// </summary>
    [Description( "Filters for People who've completed the specified learning program within the selected timeframe." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Has Completed Program Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "caf11dd5-4090-44d1-9cde-efe67820cb11" )]
    public class HasCompletedProgramFilter : DataFilterComponent
    {
        #region Properties

        /// <inheritdoc/>
        public override string AppliesToEntityType
        {
            get { return typeof( Rock.Model.Person ).FullName; }
        }

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public override string GetTitle( Type entityType )
        {
            return "Has Completed Program";
        }

        /// <inheritdoc/>
        public override string GetClientFormatSelection( Type entityType )
        {
            return "'Has Completed the ' + " +
                "'\\'' + $('.js-program', $content).find(':selected').text() + '\\' Program ' + " +
                "' in the Date Range: ' + $('.js-slidingdaterange-text-value', $content).val()";
        }

        /// <inheritdoc/>
        public override string FormatSelection( Type entityType, string selection )
        {
            string s = "Completing Program";

            var selectionConfig = SelectionConfig.Parse( selection );
            if ( selectionConfig != null && selectionConfig.LearningProgramId.HasValue )
            {
                var program = new LearningProgramService( new RockContext() ).Get( selectionConfig.LearningProgramId.Value );
                var programName = GetProgramName( program );
                var dateRangeString = string.Empty;
                if ( selectionConfig.SlidingDateRangeDelimitedValues.IsNotNullOrWhiteSpace() )
                {
                    dateRangeString = "in the Date Range: " + SlidingDateRangePicker.FormatDelimitedValues( selectionConfig.SlidingDateRangeDelimitedValues );
                }

                s = string.Format(
                    "Completed the '{0}' program '{1}'",
                    programName,
                    dateRangeString );
            }

            return s;
        }

        /// <inheritdoc/>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var ddlProgram = new RockDropDownList();
            ddlProgram.ID = filterControl.GetChildControlInstanceName( "ddlProgram" );
            ddlProgram.AddCssClass( "js-program" );
            ddlProgram.Label = "Program";
            filterControl.Controls.Add( ddlProgram );
            // Only include active and tracked programs (non-tracked programs won't have LearningProgramCompletion records).
            var programs = new LearningProgramService( new RockContext() ).Queryable().AsNoTracking()
                .Where( p => p.IsActive && p.IsCompletionStatusTracked )
                .ToList()
                .OrderBy( a => a.Name );
            ddlProgram.Items.Clear();
            ddlProgram.Items.Insert( 0, new ListItem() );
            foreach ( var program in programs )
            {
                var programName = GetProgramName( program );
                ddlProgram.Items.Add( new ListItem( programName, program.Guid.ToString() ) );
            }

            var slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.Label = "Date Range";
            slidingDateRangePicker.ID = filterControl.GetChildControlInstanceName( "slidingDateRangePicker" );
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            filterControl.Controls.Add( slidingDateRangePicker );

            var controls = new Control[2] { ddlProgram, slidingDateRangePicker };

            // convert pipe to comma delimited
            var defaultDelimitedValues = slidingDateRangePicker.DelimitedValues.Replace( "|", "," );

            // set the default values in case this is a newly added filter
            var selectionConfig = new SelectionConfig()
            {
                SlidingDateRangeDelimitedValues = defaultDelimitedValues
            };

            SetSelection(
                entityType,
                controls,
                selectionConfig.ToJson() );

            return controls;
        }

        /// <inheritdoc/>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            var ddlProgram = controls[0] as RockDropDownList;
            var slidingDateRangePicker = controls[1] as SlidingDateRangePicker;

            // Row 1
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlProgram.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // Row 2
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-8" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            slidingDateRangePicker.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        /// <inheritdoc/>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var selectionConfig = new SelectionConfig();

            if ( controls.Count() >= 2 )
            {
                var ddlProgram = controls[0] as RockDropDownList;
                var slidingDateRangePicker = controls[1] as SlidingDateRangePicker;

                selectionConfig.LearningProgramId = ddlProgram.SelectedValueAsInt();
                selectionConfig.SlidingDateRangeDelimitedValues = slidingDateRangePicker.DelimitedValues;
            }

            return selectionConfig.ToJson();
        }

        /// <inheritdoc/>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            SelectionConfig selectionConfig = SelectionConfig.Parse( selection );

            var ddlProgram = controls[0] as RockDropDownList;
            var slidingDateRangePicker = controls[1] as SlidingDateRangePicker;

            slidingDateRangePicker.DelimitedValues = selectionConfig.SlidingDateRangeDelimitedValues;
            ddlProgram.SetValue( selectionConfig.LearningProgramId );
        }

        /// <inheritdoc/>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            SelectionConfig selectionConfig = SelectionConfig.Parse( selection );
            var selectedProgramId = selectionConfig.LearningProgramId.ToIntSafe();

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( selectionConfig.SlidingDateRangeDelimitedValues );
            var rockContext = serviceInstance.Context as RockContext;

            var completionQuery = selectedProgramId == 0 ?
                 new List<LearningProgramCompletion>().AsQueryable() :
                 new LearningProgramCompletionService( rockContext ).Queryable()
                    .Include( c => c.PersonAlias )
                    .Where( c => c.LearningProgramId == selectionConfig.LearningProgramId.Value )
                    .Where( c => c.CompletionStatus == Enums.Lms.CompletionStatus.Completed );

            if ( dateRange.Start.HasValue )
            {
                var startDate = dateRange.Start.Value;
                completionQuery = completionQuery.Where( c => c.EndDate >= startDate );
            }

            if ( dateRange.End.HasValue )
            {
                var endDate = dateRange.End.Value;
                completionQuery = completionQuery.Where( c => c.EndDate <= endDate );
            }

            // Create the query that will be used for extracting the Person.
            var personQuery = new PersonService( rockContext ).Queryable().Where( p => completionQuery.Any( c => c.PersonAlias.PersonId == p.Id ) );

            return FilterExpressionExtractor.Extract<Rock.Model.Person>( personQuery, parameterExpression, "p" );
        }

        /// <summary>
        /// Handles nulls while getting the program name.
        /// </summary>
        /// <param name="program">The <see cref="LearningProgram"/> to return the name for.</param>
        /// <returns>The program name or an empty string.</returns>
        private string GetProgramName( LearningProgram program )
        {
            return program?.Name ?? string.Empty;
        }

        #endregion

        /// <summary>
        /// Get and set the filter settings from DataViewFilter.Selection.
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
            /// Gets or sets the learning program identifier to filter to.
            /// </summary>
            /// <value>
            /// The learning program identifiers.
            /// </value>
            public int? LearningProgramId { get; set; }

            /// <summary>
            /// Gets or sets the sliding date range.
            /// </summary>
            /// <value>
            /// The sliding date range.
            /// </value>
            public string SlidingDateRangeDelimitedValues { get; set; }

            /// <summary>
            /// Parses the specified selection from a JSON or delimited string.
            /// </summary>
            /// <param name="selection">The selection.</param>
            /// <returns></returns>
            public static SelectionConfig Parse( string selection )
            {
                return selection.FromJsonOrNull<SelectionConfig>();
            }
        }
    }
}