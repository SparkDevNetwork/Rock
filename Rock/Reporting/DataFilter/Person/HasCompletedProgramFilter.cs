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
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Net;
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

        /// <inheritdoc/>
        public override string ObsidianFileUrl => "~/Obsidian/Reporting/DataFilters/Person/hasCompletedProgramFilter.obs";

        #endregion

        #region Configuration

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var config = SelectionConfig.Parse( selection );

            var learningProgramOptions = new LearningProgramService( new RockContext() )
                .Queryable()
                .Where( lp => lp.IsActive && lp.IsCompletionStatusTracked )
                .OrderBy( lp => lp.Name )
                .ToList()
                .Select( lp => lp.ToListItemBag() )
                .ToList();

            var data = new Dictionary<string, string>
            {
                { "learningProgramOptions", learningProgramOptions.ToCamelCaseJson(false, true) },
                { "learningProgram", config?.LearningProgramGuid?.ToString() },
                { "dateRange", config?.SlidingDateRangeDelimitedValues },
            };

            return data;
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            var selectionConfig = new SelectionConfig
            {
                LearningProgramGuid = data.GetValueOrNull( "learningProgram" )?.AsGuidOrNull(),
                SlidingDateRangeDelimitedValues = data.GetValueOrNull( "dateRange" ),
            };

            return selectionConfig.ToJson();
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
            var selectionConfig = SelectionConfig.Parse( selection );

            if ( selectionConfig != null && selectionConfig.LearningProgramGuid.HasValue )
            {
                var program = new LearningProgramService( new RockContext() ).Get( selectionConfig.LearningProgramGuid.Value );
                var dateRangeString = SlidingDateRangePicker.FormatDelimitedValues( selectionConfig.SlidingDateRangeDelimitedValues );

                return dateRangeString.IsNotNullOrWhiteSpace()
                    ? $"Completed the '{program?.Name}' program in the date range '{dateRangeString}'"
                    : $"Completed the '{program?.Name}' program";
            }

            return "Completed Program";
        }

#if WEBFORMS

        /// <inheritdoc/>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var ddlProgram = new RockDropDownList
            {
                ID = filterControl.GetChildControlInstanceName( "ddlProgram" ),
                Label = "Program",
                Required = true
            };

            ddlProgram.AddCssClass( "js-program" );
            filterControl.Controls.Add( ddlProgram );
            SetProgramItems( ddlProgram );

            var slidingDateRangePicker = new SlidingDateRangePicker
            {
                ID = filterControl.GetChildControlInstanceName( "slidingDateRangePicker" ),
                Label = "Date Range"
            };
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            filterControl.Controls.Add( slidingDateRangePicker );

            return new Control[2] { ddlProgram, slidingDateRangePicker };
        }

        internal static void SetProgramItems( DropDownList ddlProgram )
        {
            // Only include active and tracked programs (non-tracked programs won't have LearningProgramCompletion records).
            var programs = new LearningProgramService( new RockContext() )
                .Queryable()
                .Where( lp => lp.IsActive && lp.IsCompletionStatusTracked )
                .OrderBy( lp => lp.Name )
                .Select( lp => new
                {
                    lp.Guid,
                    lp.Name
                } );

            ddlProgram.Items.Clear();
            ddlProgram.Items.Insert( 0, new ListItem() );

            foreach ( var program in programs )
            {
                ddlProgram.Items.Add( new ListItem( program.Name, program.Guid.ToString() ) );
            }
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

                selectionConfig.LearningProgramGuid = ddlProgram.SelectedValueAsGuid();
                selectionConfig.SlidingDateRangeDelimitedValues = slidingDateRangePicker.DelimitedValues;
            }

            return selectionConfig.ToJson();
        }

        /// <inheritdoc/>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            SelectionConfig selectionConfig = SelectionConfig.Parse( selection ) ?? new SelectionConfig();

            var ddlProgram = controls[0] as RockDropDownList;
            var slidingDateRangePicker = controls[1] as SlidingDateRangePicker;

            slidingDateRangePicker.DelimitedValues = selectionConfig.SlidingDateRangeDelimitedValues;
            ddlProgram.SetValue( selectionConfig.LearningProgramGuid );
        }

#endif

        /// <inheritdoc/>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection ) ?? new SelectionConfig();
            var selectedProgramGuid = selectionConfig.LearningProgramGuid;
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( selectionConfig.SlidingDateRangeDelimitedValues );
            var rockContext = serviceInstance.Context as RockContext;

            var completionQuery = GetFilterQuery( rockContext, selectedProgramGuid, dateRange );

            // Create the query that will be used for extracting the Person.
            var personQuery = new PersonService( rockContext )
                .Queryable()
                .Where( p => completionQuery.Any( lpc => lpc.PersonAlias.PersonId == p.Id ) );

            return FilterExpressionExtractor.Extract<Rock.Model.Person>( personQuery, parameterExpression, "p" );
        }

        /// <summary>
        /// Gets the query with the settings applied. This method allows us to
        /// ensure we use the same logic in the Data Filter and the Data Select.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="selectedProgramGuid">The unique identifier of the program.</param>
        /// <param name="dateRange">The date range to limit results to.</param>
        /// <returns>A queryable that matches the parameters.</returns>
        internal static IQueryable<LearningProgramCompletion> GetFilterQuery( RockContext rockContext, Guid? selectedProgramGuid, DateRange dateRange )
        {
            var completionQuery = new LearningProgramCompletionService( rockContext ).Queryable();

            if ( selectedProgramGuid.HasValue )
            {
                completionQuery = completionQuery
                    .Where( c => c.LearningProgram.Guid == selectedProgramGuid.Value
                        && c.CompletionStatus == Enums.Lms.CompletionStatus.Completed
                        && c.EndDate.HasValue );
            }
            else
            {
                // Make sure we get no results if not properly configured.
                completionQuery = completionQuery.Where( _ => false );
            }

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

            return completionQuery;
        }

        #endregion

        /// <summary>
        /// Get and set the filter settings from DataViewFilter.Selection.
        /// </summary>
        protected class SelectionConfig
        {
            /// <summary>
            /// Gets or sets the learning program unique identifier to filter to.
            /// </summary>
            /// <value>
            /// The learning program unique identifier.
            /// </value>
            public Guid? LearningProgramGuid { get; set; }

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