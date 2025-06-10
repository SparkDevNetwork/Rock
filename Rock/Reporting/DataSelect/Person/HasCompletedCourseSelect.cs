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
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Enums.Lms;
using Rock.Model;
using Rock.Reporting.DataFilter.Person;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataSelect.Group
{
    /// <summary>
    /// The Data Select responsible for including completed learning course details.
    /// </summary>
    [Description( "Select the Person's completed Learning Course" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Completed Learning Course" )]
    [Rock.SystemGuid.EntityTypeGuid( "63b0c1e0-2800-4459-9415-8f27b5710414" )]
    public class HasCompletedCourseSelect : DataSelectComponent
    {
        #region Properties

        /// <inheritdoc/>
        public override string AppliesToEntityType => typeof( Rock.Model.Person ).FullName;

        /// <inheritdoc/>
        public override string Section => "LMS";

        /// <inheritdoc/>
        public override string ColumnPropertyName => "HasCompletedCourse";

        /// <inheritdoc/>
        public override Type ColumnFieldType => typeof( bool );

        /// <inheritdoc/>
        public override string ColumnHeaderText => "Has Completed Course";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override string GetTitle( Type entityType )
        {
            return "Has Completed Course";
        }

#if REVIEW_WEBFORMS
        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            // Create the program drop down.
            var ddlProgram = new RockDropDownList
            {
                ID = parentControl.GetChildControlInstanceName( _CtlProgram ),
                Label = "Program",
                Required = true,
                AutoPostBack = true
            };

            ddlProgram.AddCssClass( "js-program" );
            ddlProgram.SelectedIndexChanged += Program_SelectedIndexChanged;
            parentControl.Controls.Add( ddlProgram );
            HasCompletedProgramFilter.SetProgramItems( ddlProgram );

            // This is a hack. We don't know why this has to be done this way
            // because Data Filters work fine without it. But with selects if
            // you have a conditional control then you have to access the raw
            // postback data like this so that the SetCourseItems below can
            // populate the control for the current selection to "stick".
            var selectedProgramGuid = parentControl.Page.Request.Params[ddlProgram.UniqueID].AsGuidOrNull();
            ddlProgram.SetValue( selectedProgramGuid );

            // Create the course drop down.
            var ddlCourse = new RockDropDownList
            {
                ID = parentControl.GetChildControlInstanceName( "ddlCourse" ),
                Label = "Course",
                Required = true
            };

            ddlCourse.AddCssClass( "js-course" );
            parentControl.Controls.Add( ddlCourse );
            HasCompletedCourseFilter.SetCourseItems( ddlProgram, ddlCourse );

            var slidingDateRangePicker = new SlidingDateRangePicker
            {
                Label = "Date Range",
                ID = parentControl.GetChildControlInstanceName( _CtlSlidingDateRange )
            };
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            parentControl.Controls.Add( slidingDateRangePicker );

            var cblStatuses = new RockCheckBoxList();
            cblStatuses.Label = "with Course Status (optional)";
            cblStatuses.ID = parentControl.GetChildControlInstanceName( _CtlStatuses );
            cblStatuses.CssClass = "js-statuses";
            parentControl.Controls.Add( cblStatuses );

            cblStatuses.Items.Clear();
            var completionStatusType = typeof( LearningCompletionStatus );

            foreach ( LearningCompletionStatus enumValue in HasCompletedCourseFilter.CompletionStatusesWithCompletedDateValue )
            {
                cblStatuses.Items.Add( new ListItem( Enum.GetName( completionStatusType, enumValue ), enumValue.ToString() ) );
            }

            return new System.Web.UI.Control[] { ddlProgram, ddlCourse, slidingDateRangePicker, cblStatuses };
        }

        private void Program_SelectedIndexChanged( object sender, EventArgs e )
        {
            var container = ( sender as System.Web.UI.Control ).Parent;
            var ddlProgram = container.ControlsOfTypeRecursive<DropDownList>().FirstOrDefault( a => a.HasCssClass( "js-program" ) );
            var ddlCourse = container.ControlsOfTypeRecursive<DropDownList>().FirstOrDefault( a => a.HasCssClass( "js-course" ) );

            HasCompletedCourseFilter.SetCourseItems( ddlProgram, ddlCourse );
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
            var ddlCourse = controls.GetByName<DropDownList>( _CtlCourse );
            var slidingDateRangePicker = controls.GetByName<SlidingDateRangePicker>( _CtlSlidingDateRange );
            var cblStatuses = controls.GetByName<CheckBoxList>( _CtlStatuses );

            var selectionConfig = new SelectionConfig
            {
                LearningCourseGuid = ddlCourse.SelectedValue.AsGuidOrNull(),
                LearningCompletionStatuses = new List<LearningCompletionStatus>(),
                SlidingDateRangeDelimitedValues = slidingDateRangePicker.DelimitedValues
            };

            foreach ( ListItem item in cblStatuses.Items )
            {
                if ( item.Selected && Enum.TryParse<LearningCompletionStatus>( item.Value, out var enumValue ) )
                {
                    selectionConfig.LearningCompletionStatuses.Add( enumValue );
                }
            }

            return selectionConfig.ToJson();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( System.Web.UI.Control[] controls, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection ) ?? new SelectionConfig();

            var ddlProgram = controls.GetByName<RockDropDownList>( _CtlProgram );
            var ddlCourse = controls.GetByName<RockDropDownList>( _CtlCourse );
            var slidingDateRangePicker = controls.GetByName<SlidingDateRangePicker>( _CtlSlidingDateRange );
            var cblStatuses = controls.GetByName<CheckBoxList>( _CtlStatuses );

            var programGuid = new LearningCourseService( new RockContext() )
                .Queryable()
                .Where( lc => lc.Guid == selectionConfig.LearningCourseGuid )
                .Select( lc => ( Guid? ) lc.LearningProgram.Guid )
                .FirstOrDefault();

            ddlProgram.SetValue( programGuid );

            HasCompletedCourseFilter.SetCourseItems( ddlProgram, ddlCourse );
            ddlCourse.SetValue( selectionConfig.LearningCourseGuid );

            slidingDateRangePicker.DelimitedValues = selectionConfig.SlidingDateRangeDelimitedValues;

            foreach ( ListItem item in cblStatuses.Items )
            {
                var isSelected = false;
                if ( selectionConfig.LearningCompletionStatuses != null && Enum.TryParse<LearningCompletionStatus>( item.Value, out var enumValue ) )
                {
                    isSelected = selectionConfig.LearningCompletionStatuses.Contains( enumValue );
                }

                item.Selected = isSelected;
            }
        }
#endif

        /// <inheritdoc/>
        public override Expression GetExpression( RockContext context, MemberExpression entityIdProperty, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection ) ?? new SelectionConfig();
            var selectedCourseGuid = selectionConfig.LearningCourseGuid;
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( selectionConfig.SlidingDateRangeDelimitedValues );
            var completionStatuses = selectionConfig.LearningCompletionStatuses;

            var participantQuery = HasCompletedCourseFilter.GetFilterQuery( context, selectedCourseGuid, dateRange, completionStatuses );

            var personQuery = new PersonService( context )
                .Queryable()
                .Where( p => participantQuery.Any( lp => lp.PersonId == p.Id ) );

            var selectExpression = SelectExpressionExtractor.Extract( personQuery, entityIdProperty, "p" );

            return selectExpression;
        }

        #endregion

        private const string _CtlProgram = "ddlProgram";
        private const string _CtlCourse = "ddlCourse";
        private const string _CtlSlidingDateRange = "slidingDateRangePicker";
        private const string _CtlStatuses = "cblStatuses";

        /// <summary>
        /// Get and set the filter settings from DataViewFilter.Selection.
        /// </summary>
        protected class SelectionConfig
        {
            /// <summary>
            /// Gets or sets the learning course identifier to filter to.
            /// </summary>
            /// <value>
            /// The learning course identifiers.
            /// </value>
            public Guid? LearningCourseGuid { get; set; }

            /// <summary>
            /// Gets or sets the completion statuses to filter to.
            /// </summary>
            /// <value>
            /// The <see cref="LearningCompletionStatus" /> to filter to.
            /// </value>
            public List<LearningCompletionStatus> LearningCompletionStatuses { get; set; }

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
