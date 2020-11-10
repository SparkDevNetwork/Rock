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
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people whether they have created notes" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Created Note Filter" )]
    public class CreatedNotesFilter : DataFilterComponent
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

        private const string _CtlNoteTypes = "cblNoteTypes";
        private const string _CtlSlidingDateRangePicker = "slidingDateRangePicker";
        private const string _CtlMinimumCount = "nbMinimumCount";

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
            return "Created Person Note";
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
                function() {{
                    var minimumCount = $('.number-box', $content).find('input').val();
                    var result = ""Created "" + minimumCount + "" or more Person "";

                    if( minimumCount > 1 ) {{
                        result +=  ""Notes"";
                    }} else {{
                        result +=  ""Note"";
                    }}

                    var noteTypeNames = $('.rock-list-box .js-notetypes', $content).find(':selected');
                    if ( noteTypeNames.length > 0 ) {{
                        var noteTypesDelimitedList = noteTypeNames.map(function() {{ return $(this).text() }}).get().join(', ');
                        result += "" with note types: "" + noteTypesDelimitedList +""."";
                    }}

                    var dateRangeText = $('.js-slidingdaterange-text-value', $content).val();
                    if( dateRangeText ) {{
                        result +=  "" Date Range: "" + dateRangeText
                    }}
                    return result;
                }}
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
            var result = "Created Person Note";

            var selectionConfig = SelectionConfig.Parse( selection );

            if ( selectionConfig != null )
            {
                result = $"Created {selectionConfig.MinimumCount} or more Person {"Note".PluralizeIf( selectionConfig.MinimumCount > 1 )}";

                if ( selectionConfig.NoteTypeIds.Any() )
                {
                    var noteTypeNames = new List<string>();
                    foreach ( var noteTypeId in selectionConfig.NoteTypeIds )
                    {
                        var noteType = NoteTypeCache.Get( noteTypeId );
                        if ( noteType != null )
                        {
                            noteTypeNames.Add( noteType.Name );
                        }
                    }

                    result += string.Format( " with note types: {0}.", noteTypeNames.AsDelimited( "," ) );
                }

                if ( selectionConfig.DelimitedValues.IsNotNullOrWhiteSpace() )
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
            var cblNoteTypes = new RockListBox();
            cblNoteTypes.ID = filterControl.GetChildControlInstanceName( _CtlNoteTypes );
            cblNoteTypes.CssClass = "js-notetypes";
            cblNoteTypes.Label = "Note Types";
            cblNoteTypes.Help = "The type of note to filter by. Leave blank to include all note types.";
            filterControl.Controls.Add( cblNoteTypes );

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

            cblNoteTypes.Items.Clear();
            cblNoteTypes.Items.AddRange( noteTypes.Select( a => new ListItem( a.Name, a.Id.ToString() ) ).ToArray() );

            var slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.ID = filterControl.GetChildControlInstanceName( _CtlSlidingDateRangePicker );
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            slidingDateRangePicker.Label = "Date Range";
            slidingDateRangePicker.Help = "The date range that the note was created during.";
            slidingDateRangePicker.Required = false;
            filterControl.Controls.Add( slidingDateRangePicker );

            var nbMinimumCount = new NumberBox();
            nbMinimumCount.ID = filterControl.GetChildControlInstanceName( _CtlMinimumCount );
            nbMinimumCount.NumberType = ValidationDataType.Integer;
            nbMinimumCount.MinimumValue = "1";
            nbMinimumCount.Label = "Minimum Count";
            nbMinimumCount.Help = "The minimum number of notes created during the date range to be considered a match.";
            nbMinimumCount.Required = true;
            nbMinimumCount.AddCssClass( "js-minimum-count" );
            filterControl.Controls.Add( nbMinimumCount );

            return new System.Web.UI.Control[3] { cblNoteTypes, slidingDateRangePicker, nbMinimumCount };
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var selectionConfig = new SelectionConfig();

            if ( controls.Count() >= 3 )
            {
                var cblNoteTypes = controls[0] as RockListBox;
                var slidingDateRangePicker = controls[1] as SlidingDateRangePicker;
                var nbMinimumCount = controls[2] as NumberBox;

                selectionConfig.NoteTypeIds = cblNoteTypes.SelectedValuesAsInt;
                selectionConfig.DelimitedValues = slidingDateRangePicker.DelimitedValues;
                selectionConfig.MinimumCount = nbMinimumCount.IntegerValue ?? 1;
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
            var selectionConfig = SelectionConfig.Parse( selection );
            if ( selectionConfig != null )
            {
                var cblNoteTypes = controls[0] as RockListBox;
                var slidingDateRangePicker = controls[1] as SlidingDateRangePicker;
                var nbMinimumCount = controls[2] as NumberBox;

                cblNoteTypes.SetValues( selectionConfig.NoteTypeIds );
                slidingDateRangePicker.DelimitedValues = selectionConfig.DelimitedValues;
                nbMinimumCount.IntegerValue = selectionConfig.MinimumCount;
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
            var selectionConfig = SelectionConfig.Parse( selection );
            if ( selectionConfig != null )
            {
                var entityTypeIdPerson = EntityTypeCache.GetId<Rock.Model.Person>();
                var noteQry = new NoteService( ( RockContext ) serviceInstance.Context ).Queryable()
                    .Where( x => x.NoteType.EntityTypeId == entityTypeIdPerson && x.CreatedByPersonAliasId.HasValue );

                if ( selectionConfig.NoteTypeIds != null && selectionConfig.NoteTypeIds.Any() )
                {
                    noteQry = noteQry.Where( x => selectionConfig.NoteTypeIds.Contains( x.NoteTypeId ) );
                }

                if ( selectionConfig.DelimitedValues.IsNotNullOrWhiteSpace() )
                {
                    var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( selectionConfig.DelimitedValues );
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
                    .Where( p => noteQry.Where( xx => xx.CreatedByPersonAlias.PersonId == p.Id ).Count() >= selectionConfig.MinimumCount );

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
            /// Gets or sets the note type identifiers.
            /// </summary>
            /// <value>
            /// The note type identifiers.
            /// </value>
            public List<int> NoteTypeIds { get; set; }

            /// <summary>
            /// Gets a pipe delimited string of the property values. This is to use the SlidingDateRangePicker's existing logic.
            /// </summary>
            /// <value>
            /// The delimited values.
            /// </value>
            public string DelimitedValues { get; set; }

            /// <summary>
            /// Gets or sets the minimum count.
            /// </summary>
            /// <value>
            /// The minimum count.
            /// </value>
            public int MinimumCount { get; set; }

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