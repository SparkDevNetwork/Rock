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
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Enums.Lms;
using Rock.Model;
using Rock.Utility;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataSelect.Group
{
    /// <summary>
    /// The Data Select responsible for including completed learning program details.
    /// </summary>
    [Description( "Select the Person's Completed Learning Program" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Completed Learning Program" )]
    [Rock.SystemGuid.EntityTypeGuid( "fa3f52b5-fa6b-4fbf-b0e3-bd6c186b45a7" )]
    public class HasCompletedProgramSelect : DataSelectComponent
    {
        #region Properties

        /// <inheritdoc/>
        public override string AppliesToEntityType
        {
            get
            {
                return typeof( Rock.Model.Person ).FullName;
            }
        }

        /// <inheritdoc/>
        public override string Section
        {
            get
            {
                return "LMS";
            }
        }

        /// <inheritdoc/>
        public override string ColumnPropertyName
        {
            get
            {
                return "HasCompletedProgram";
            }
        }

        /// <inheritdoc/>
        public override Type ColumnFieldType
        {
            get { return typeof( ProgramCompletionData ); }
        }

        /// <inheritdoc/>
        public override string ColumnHeaderText
        {
            get
            {
                return "Has Completed Program";
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override System.Web.UI.WebControls.DataControlField GetGridField( Type entityType, string selection )
        {
            var result = new CallbackField();

            result.OnFormatDataValue += ( sender, e ) =>
            {
                var programData = e.DataValue as ProgramCompletionData;

                if ( programData == null )
                {
                    e.FormattedValue = string.Empty;
                }
                else
                {
                    switch ( programData.CompletionStatus )
                    {
                        case CompletionStatus.Pending:
                            e.FormattedValue = $"Pending Completion";
                            break;
                        case CompletionStatus.Completed:
                            if ( programData.EndDate.HasValue )
                            {
                                e.FormattedValue = $"Completed on {programData.EndDate.Value.ToShortDateString()}";
                            }
                            else
                            {
                                e.FormattedValue = "Completed";
                            }
                            break;
                        case CompletionStatus.Expired:
                            if ( programData.EndDate.HasValue )
                            {
                                e.FormattedValue = $"Expired on {programData.EndDate.Value.ToShortDateString()}";
                            }
                            else
                            {
                                e.FormattedValue = "Expired";
                            }
                            break;
                    }
                }
            };

            return result;
        }

        /// <inheritdoc/>
        public override string GetTitle( Type entityType )
        {
            return "Has Completed Program";
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            // Define Control: Output Format DropDown List
            var ddlProgram = new RockDropDownList();
            ddlProgram.ID = parentControl.GetChildControlInstanceName( _CtlProgram );
            ddlProgram.Label = "Completed Program";
            ddlProgram.Help = "The Learning Program that should have been completed during the specified time frame.";

            var programs = new LearningProgramService( new RockContext() ).Queryable().AsNoTracking()
               .Where( p => p.IsActive )
               .Select( p => new
               {
                   p.Id,
                   p.Name
               } )
               .ToList()
               .OrderBy( a => a.Name );
            ddlProgram.Items.Clear();
            ddlProgram.Items.Insert( 0, new ListItem() );
            foreach ( var program in programs )
            {
                var programName = program.Name ?? string.Empty;
                ddlProgram.Items.Add( new ListItem( programName, program.Id.ToString() ) );
            }

            parentControl.Controls.Add( ddlProgram );

            var slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.Label = "Date Range";
            slidingDateRangePicker.ID = parentControl.GetChildControlInstanceName( _CtlSlidingDateRange );
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            parentControl.Controls.Add( slidingDateRangePicker );

            return new System.Web.UI.Control[] { ddlProgram, slidingDateRangePicker };
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( System.Web.UI.Control parentControl, System.Web.UI.HtmlTextWriter writer, System.Web.UI.Control[] controls )
        {
            base.RenderControls( parentControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( System.Web.UI.Control[] controls )
        {
            var ddlProgram = controls.GetByName<DropDownList>( _CtlProgram );
            var slidingDateRangePicker = controls.GetByName<SlidingDateRangePicker>( _CtlSlidingDateRange );

            var settings = new HasCompletedProgramSelectSettings
            {
                LearningProgramId = ddlProgram.SelectedValue.AsIntegerOrNull(),
                SlidingDateRangeDelimitedValues = slidingDateRangePicker.DelimitedValues
            };

            return settings.ToSelectionString();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( System.Web.UI.Control[] controls, string selection )
        {
            var ddlProgram = controls.GetByName<RockDropDownList>( _CtlProgram );
            var slidingDateRangePicker = controls.GetByName<SlidingDateRangePicker>( _CtlSlidingDateRange );

            var settings = new HasCompletedProgramSelectSettings( selection );

            if ( !settings.IsValid )
            {
                return;
            }

            ddlProgram.SelectedValue = settings.LearningProgramId.ToString();
            slidingDateRangePicker.DelimitedValues = settings.SlidingDateRangeDelimitedValues;
        }

        /// <inheritdoc/>
        public override Expression GetExpression( RockContext context, MemberExpression entityIdProperty, string selection )
        {
            var settings = new HasCompletedProgramSelectSettings( selection );
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( settings.SlidingDateRangeDelimitedValues );

            var programQuery = settings.LearningProgramId.HasValue ?
                new LearningProgramCompletionService( context ).Queryable()
                    .AsNoTracking()
                    .Include( c => c.PersonAlias )
                    .Where( c => c.LearningProgramId == settings.LearningProgramId ) :
                new List<LearningProgramCompletion>().AsQueryable();

            if ( dateRange.Start.HasValue || dateRange.End.HasValue )
            {
                programQuery = programQuery.Where( c => c.EndDate.HasValue );

                if ( dateRange.Start.HasValue )
                {
                    var startDate = dateRange.Start.Value;
                    programQuery = programQuery.Where( c => c.EndDate >= startDate );
                }

                if ( dateRange.End.HasValue )
                {
                    var endDate = dateRange.End.Value;
                    programQuery = programQuery.Where( c => c.EndDate <= endDate );
                }
            }
            var personQuery = new PersonService( context ).Queryable()
                .AsNoTracking()
                .Select( p => programQuery
                    .Where( c => c.PersonAlias.PersonId == p.Id )
                    .Select( c => new ProgramCompletionData
                    {
                        EndDate = c.EndDate,
                        CompletionStatus = c.CompletionStatus
                    } )
                    .FirstOrDefault()
                );

            var selectExpression = SelectExpressionExtractor.Extract( personQuery, entityIdProperty, "p" );

            return selectExpression;
        }

        #endregion

        private const string _CtlProgram = "ddlProgram";
        private const string _CtlSlidingDateRange = "slidingDateRangePicker";

        private class ProgramCompletionData
        {
            public DateTime? EndDate { get; set; }
            public CompletionStatus CompletionStatus { get; set; }
        }

        private class HasCompletedProgramSelectSettings : SettingsStringBase
        {
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

            public HasCompletedProgramSelectSettings()
            {

            }

            public HasCompletedProgramSelectSettings( string settingsString )
            {
                FromSelectionString( settingsString );
            }

            protected override IEnumerable<string> OnGetParameters()
            {
                var settings = new List<string>();

                settings.Add( LearningProgramId.ToStringSafe() );
                settings.Add( SlidingDateRangeDelimitedValues.Replace( "|", ";" ).ToStringSafe() );

                return settings;
            }

            protected override void OnSetParameters( int version, IReadOnlyList<string> parameters )
            {
                LearningProgramId = DataComponentSettingsHelper.GetParameterOrDefault( parameters, 0, string.Empty ).AsIntegerOrNull();
                SlidingDateRangeDelimitedValues = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 1 ).Replace( ";", "|" );
            }
        }
    }
}
