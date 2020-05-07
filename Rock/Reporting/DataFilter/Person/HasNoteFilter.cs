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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

// This is to get the enums without the prefix
using static Rock.Web.UI.Controls.SlidingDateRangePicker;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people based on notes" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Has Note Filter" )]
    public class HasNoteFilter : DataFilterComponent
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
            return "Person Note";
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
                    var noteTypeName = $('.js-notetype', $content).find(':selected').text()
                    var containsText = $('.js-notecontains', $content).val();
                    var result = ""Has a "" + noteTypeName + "" note containing '"" + containsText + ""'"";

                    var dateRangeText = $('.js-slidingdaterange-text-value', $content).val();
                    if(dateRageText){
                        result +=  "" Date Range: "" + dateRangeText
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
            string result = "Person Note";

            var selectionConfig = SelectionConfig.Parse( selection );

            if ( selectionConfig != null )
            {
                NoteTypeCache selectedNoteType = null;
                if ( selectionConfig.NoteTypeId.HasValue )
                {
                    selectedNoteType = NoteTypeCache.Get( selectionConfig.NoteTypeId.Value );
                }
                if ( selectedNoteType != null )
                {
                    result = $"Has a {selectedNoteType.Name} note";
                }
                else
                {
                    result = "Has a note";
                }

                var containingText = selectionConfig.NoteContains;
                if ( containingText.IsNotNullOrWhiteSpace() )
                {
                    result += $" containing '{containingText}'";
                }

                if ( selectionConfig.DateRangeMode != null )
                {
                    var dateRangeString = SlidingDateRangePicker.FormatDelimitedValues( selectionConfig.DelimitedValues );
                    if ( dateRangeString.IsNotNullOrWhiteSpace() )
                    {
                        result += $" Date Range: {dateRangeString}";
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            RockDropDownList ddlNoteType = new RockDropDownList();
            ddlNoteType.ID = filterControl.ID + "_ddlNoteType";
            ddlNoteType.CssClass = "js-notetype";
            ddlNoteType.Label = "Note Type";
            filterControl.Controls.Add( ddlNoteType );

            var noteTypeService = new NoteTypeService( new RockContext() );
            var entityTypeIdPerson = EntityTypeCache.GetId<Rock.Model.Person>();
            var noteTypes = noteTypeService.Queryable().Where( a => a.EntityTypeId == entityTypeIdPerson )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Select( a => new
                {
                    a.Id,
                    a.Name
                } ).ToList();

            ddlNoteType.Items.Clear();
            ddlNoteType.Items.Add( new ListItem() );
            ddlNoteType.Items.AddRange( noteTypes.Select( a => new ListItem( a.Name, a.Id.ToString() ) ).ToArray() );

            var tbContains = new RockTextBox();
            tbContains.Label = "Contains";
            tbContains.ID = filterControl.ID + "_tbContains";
            tbContains.CssClass = "js-notecontains";
            filterControl.Controls.Add( tbContains );

            var slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.ID = filterControl.ID + "_slidingDateRangePicker";
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            slidingDateRangePicker.Label = "Date Range";
            slidingDateRangePicker.Help = "The date range of the note's creation date";
            slidingDateRangePicker.Required = false;
            filterControl.Controls.Add( slidingDateRangePicker );

            return new System.Web.UI.Control[3] { ddlNoteType, tbContains, slidingDateRangePicker };
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
            base.RenderControls( entityType, filterControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            SelectionConfig selectionConfig = new SelectionConfig();

            if ( controls.Count() >= 3 )
            {
                RockDropDownList ddlNoteType = controls[0] as RockDropDownList;
                RockTextBox tbContains = controls[1] as RockTextBox;
                SlidingDateRangePicker slidingDateRange = controls[2] as SlidingDateRangePicker;

                selectionConfig.NoteTypeId = ddlNoteType.SelectedValue.AsIntegerOrNull();
                selectionConfig.NoteContains = tbContains.Text;
                selectionConfig.StartDate = slidingDateRange.DateRangeModeStart;
                selectionConfig.EndDate = slidingDateRange.DateRangeModeEnd;
                selectionConfig.DateRangeMode = slidingDateRange.SlidingDateRangeMode;
                selectionConfig.TimeUnit = slidingDateRange.TimeUnit;
                selectionConfig.NumberOfTimeUnits = slidingDateRange.NumberOfTimeUnits;
            }

            return selectionConfig.ToJson();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            SelectionConfig selectionConfig = SelectionConfig.Parse( selection );
            if ( selectionConfig != null )
            {
                RockDropDownList ddlNoteType = controls[0] as RockDropDownList;
                RockTextBox tbContains = controls[1] as RockTextBox;
                SlidingDateRangePicker slidingDateRange = controls[2] as SlidingDateRangePicker;

                ddlNoteType.SelectedValue = selectionConfig.NoteTypeId.HasValue ? selectionConfig.NoteTypeId.ToString() : "";
                tbContains.Text = selectionConfig.NoteContains;
                slidingDateRange.DelimitedValues = selectionConfig.DelimitedValues;
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
            SelectionConfig selectionConfig = SelectionConfig.Parse( selection );
            if ( selectionConfig != null )
            {
                var entityTypeIdPerson = EntityTypeCache.GetId<Rock.Model.Person>();
                var noteQry = new NoteService( ( RockContext ) serviceInstance.Context ).Queryable()
                    .Where( x => x.NoteType.EntityTypeId == entityTypeIdPerson );

                if ( selectionConfig.NoteTypeId.HasValue )
                {
                    noteQry = noteQry.Where( x => x.NoteTypeId == selectionConfig.NoteTypeId.Value );
                }

                if ( selectionConfig.NoteContains.IsNotNullOrWhiteSpace() )
                {
                    noteQry = noteQry.Where( a => a.Text.Contains( selectionConfig.NoteContains ) );
                }

                if ( selectionConfig.DateRangeMode != null )
                {
                    DateRange dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( selectionConfig.DelimitedValues );
                    if ( dateRange.Start.HasValue )
                    {
                        noteQry = noteQry.Where( n => n.CreatedDateTime >= dateRange.Start.Value );
                    }

                    if ( dateRange.End.HasValue )
                    {
                        noteQry = noteQry.Where( n => n.CreatedDateTime <= dateRange.End.Value );
                    }
                }

                var qry = new PersonService( ( RockContext ) serviceInstance.Context ).Queryable()
                    .Where( p => noteQry.Any( x => x.EntityId == p.Id ) );

                return FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );
            }

            return null;
        }

        #endregion

        /// <summary>
        /// Get and set the filter settings from DataViewFilter.Selection
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
            /// Gets or sets the note type identifier.
            /// </summary>
            /// <value>
            /// The note type identifier.
            /// </value>
            public int? NoteTypeId { get; set; }

            /// <summary>
            /// Gets or sets the note contains text.
            /// </summary>
            /// <value>
            /// The note contains.
            /// </value>
            public string NoteContains { get; set; }

            /// <summary>
            /// Gets a pipe delimited string of the property values. This is to use the SlidingDateRangePicker's existing logic.
            /// </summary>
            /// <value>
            /// The delimited values.
            /// </value>
            public string DelimitedValues
            {
                get
                {
                    return CreateSlidingDateRangePickerDelimitedValues();
                }
            }

            /// <summary>
            /// Gets or sets the date range mode.
            /// </summary>
            /// <value>
            /// The date range mode.
            /// </value>
            public SlidingDateRangeType? DateRangeMode { get; set; }

            /// <summary>
            /// Gets or sets the number of time units.
            /// </summary>
            /// <value>
            /// The number of time units.
            /// </value>
            public int? NumberOfTimeUnits { get; set; }

            /// <summary>
            /// Gets or sets the time unit.
            /// </summary>
            /// <value>
            /// The time unit.
            /// </value>
            public TimeUnitType? TimeUnit { get; set; }

            /// <summary>
            /// Gets or sets the start date.
            /// </summary>
            /// <value>
            /// The start date.
            /// </value>
            public DateTime? StartDate { get; set; }

            /// <summary>
            /// Gets or sets the end date.
            /// </summary>
            /// <value>
            /// The end date.
            /// </value>
            public DateTime? EndDate { get; set; }

            /// <summary>
            /// Parses the specified selection from a JSON or delimited string.
            /// </summary>
            /// <param name="selection">The selection.</param>
            /// <returns></returns>
            public static SelectionConfig Parse( string selection )
            {
                SelectionConfig selectionConfig = selection.FromJsonOrNull<SelectionConfig>();

                if ( selectionConfig == null )
                {
                    selectionConfig = new SelectionConfig();
                    string[] selectionValues = selection.Split( '|' );

                    if ( selectionValues.Count() >= 2 )
                    {
                        selectionConfig.NoteTypeId = selectionValues[0].AsIntegerOrNull();
                        selectionConfig.NoteContains = selectionValues[1];

                        if ( selectionValues.Count() >= 3 )
                        {
                            string[] dateRangeValues = selectionValues[3].Split( ',' );
                            if ( dateRangeValues.Count() > 3 )
                            {
                                // DateRange index 0 is the mode
                                selectionConfig.DateRangeMode = dateRangeValues[0].ConvertToEnum<SlidingDateRangeType>();

                                // DateRange index 1 is the number of time units
                                selectionConfig.NumberOfTimeUnits = dateRangeValues[1].AsIntegerOrNull();

                                // DateRange index 2 is the time unit
                                selectionConfig.TimeUnit = dateRangeValues[2].ConvertToEnum<TimeUnitType>();

                                // DateRange index 3 is the start date
                                selectionConfig.StartDate = dateRangeValues[3].AsDateTime();

                                // DateRange index 4 is the end date if it exists
                                if ( dateRangeValues.Count() > 4 )
                                {
                                    selectionConfig.EndDate = dateRangeValues[4].AsDateTime();
                                }
                            }
                            else if ( dateRangeValues.Any() )
                            {
                                // Try to get a DateRange from what we have
                                selectionConfig.DateRangeMode = SlidingDateRangeType.DateRange;
                                selectionConfig.StartDate = dateRangeValues[0].AsDateTime();

                                if ( dateRangeValues.Count() > 1 )
                                {
                                    selectionConfig.EndDate = dateRangeValues[1].AsDateTime();
                                    if ( selectionConfig.EndDate.HasValue )
                                    {
                                        // This value would have been from the DatePicker which does not automatically add a day.
                                        selectionConfig.EndDate.Value.AddDays( 1 );
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        return null;
                    }
                }

                return selectionConfig;
            }

            /// <summary>
            /// Creates the sliding date range picker delimited values.
            /// </summary>
            /// <returns></returns>
            private string CreateSlidingDateRangePickerDelimitedValues()
            {
                return string.Format(
                    "{0}|{1}|{2}|{3}|{4}",
                    this.DateRangeMode,
                    ( SlidingDateRangeType.Last | SlidingDateRangeType.Previous | SlidingDateRangeType.Next | SlidingDateRangeType.Upcoming ).HasFlag( DateRangeMode ) ? this.NumberOfTimeUnits : ( int? ) null,
                    ( SlidingDateRangeType.Last | SlidingDateRangeType.Previous | SlidingDateRangeType.Next | SlidingDateRangeType.Upcoming | SlidingDateRangeType.Current ).HasFlag( this.DateRangeMode ) ? this.TimeUnit : ( TimeUnitType? ) null,
                    this.DateRangeMode == SlidingDateRangeType.DateRange ? this.StartDate : null,
                    this.DateRangeMode == SlidingDateRangeType.DateRange ? this.EndDate : null );
            }
        }
    }
}