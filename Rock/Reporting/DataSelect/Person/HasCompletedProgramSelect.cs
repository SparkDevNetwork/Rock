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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
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
        public override string AppliesToEntityType => typeof( Rock.Model.Person ).FullName;

        /// <inheritdoc/>
        public override string Section => "LMS";

        /// <inheritdoc/>
        public override string ColumnPropertyName => "HasCompletedProgram";

        /// <inheritdoc/>
        public override Type ColumnFieldType => typeof( bool );

        /// <inheritdoc/>
        public override string ColumnHeaderText => "Has Completed Program";

        #endregion

        #region Methods

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
            var ddlProgram = new RockDropDownList
            {
                ID = parentControl.GetChildControlInstanceName( _CtlProgram ),
                Label = "Completed Program",
                Help = "The Learning Program that should have been completed during the specified time frame.",
                Required = true
            };

            DataFilter.Person.HasCompletedProgramFilter.SetProgramItems( ddlProgram );

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

            var settings = new SelectionConfig
            {
                LearningProgramGuid = ddlProgram.SelectedValue.AsGuidOrNull(),
                SlidingDateRangeDelimitedValues = slidingDateRangePicker.DelimitedValues
            };

            return settings.ToJson();
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

            var settings = SelectionConfig.Parse( selection ) ?? new SelectionConfig();

            ddlProgram.SetValue( settings.LearningProgramGuid );
            slidingDateRangePicker.DelimitedValues = settings.SlidingDateRangeDelimitedValues;
        }

        /// <inheritdoc/>
        public override Expression GetExpression( RockContext context, MemberExpression entityIdProperty, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection ) ?? new SelectionConfig();
            var selectedProgramGuid = selectionConfig.LearningProgramGuid;
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( selectionConfig.SlidingDateRangeDelimitedValues );

            var completionQuery = DataFilter.Person.HasCompletedProgramFilter.GetFilterQuery( context, selectedProgramGuid, dateRange );

            var personQuery = new PersonService( context ).Queryable()
                .Where( p => completionQuery.Any( c => c.PersonAlias.PersonId == p.Id ) );

            var selectExpression = SelectExpressionExtractor.Extract( personQuery, entityIdProperty, "p" );

            return selectExpression;
        }

        #endregion

        private const string _CtlProgram = "ddlProgram";
        private const string _CtlSlidingDateRange = "slidingDateRangePicker";

        /// <summary>
        /// Get and set the filter settings from DataViewFilter.Selection.
        /// </summary>
        private class SelectionConfig
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
