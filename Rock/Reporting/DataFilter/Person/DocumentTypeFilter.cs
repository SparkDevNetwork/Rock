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
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using static Rock.Web.UI.Controls.SlidingDateRangePicker;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people based on their document types within a specific date range" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Document Type Filter" )]
    public class DocumentTypeFilter : DataFilterComponent
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

        #endregion Properties

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
            return "Document Type";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the FilterField control
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
  
  var result = 'Document Type';

  var docType = $('.js-document-type option:selected', $content).text();
  if (docType) {
     result = result + ': ' + docType;

                    var dateRangeText = $('.js-slidingdaterange-text-value', $content).val();
                    if( dateRangeText ) {{
                        result +=  "", Date Range: "" + dateRangeText
                    }}
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
            StringBuilder result = new StringBuilder();
            result.Append( "Document Type" );

            var selectionConfig = SelectionConfig.Parse( selection );

            if ( selectionConfig != null )
            {
                var documentTypeService = new DocumentTypeService( new RockContext() ).Get( selectionConfig.DocumentTypeId );

                if ( documentTypeService != null )
                {
                    result.Append( ": " + documentTypeService.Name );

                    if ( selectionConfig.SlidingDateRangePickerDelimitedValues.IsNotNullOrWhiteSpace() )
                    {
                        var dateRangeString = SlidingDateRangePicker.FormatDelimitedValues( selectionConfig.SlidingDateRangePickerDelimitedValues );
                        if ( dateRangeString.IsNotNullOrWhiteSpace() )
                        {
                            result.Append( ", Date Range: " + dateRangeString );
                        }
                    }
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var ddlDocumentType = new RockDropDownList();
            ddlDocumentType.ID = filterControl.ID + "_ddlDocumentType";
            ddlDocumentType.Label = "Document Type";
            ddlDocumentType.CssClass = "js-document-type";
            ddlDocumentType.Required = true;
            ddlDocumentType.AutoPostBack = true;
            ddlDocumentType.EnhanceForLongLists = true;
            filterControl.Controls.Add( ddlDocumentType );

            var documentTypeService = new DocumentTypeService( new RockContext() );
            var documentTypes = documentTypeService.Queryable().AsNoTracking()
                .OrderBy( a => a.Order ).ThenBy( a => a.Name ).Select( a => new
                {
                    a.Id,
                    a.Name
                } ).ToList();

            ddlDocumentType.Items.Clear();
            ddlDocumentType.Items.Add( new ListItem() );
            ddlDocumentType.Items.AddRange( documentTypes.Select( a => new ListItem( a.Name, a.Id.ToString() ) ).ToArray() );

            int? documentTypeId = filterControl.Page.Request.Params[ddlDocumentType.UniqueID].AsIntegerOrNull();
            ddlDocumentType.SetValue( documentTypeId );

            SlidingDateRangePicker slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.ID = filterControl.ID + "_slidingDateRangePicker";
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            slidingDateRangePicker.Label = "Date Range";
            slidingDateRangePicker.Help = "The date range of the documents";
            filterControl.Controls.Add( slidingDateRangePicker );

            var controls = new Control[2] { ddlDocumentType, slidingDateRangePicker };

            return controls;
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
        /// Implement this version of GetSelection if your DataFilterComponent works the same in all FilterModes
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="controls">The collection of controls used to set the filter values.</param>
        /// <returns>A formatted string representing the filter settings.</returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            DropDownList ddlDocumentType = controls[0] as DropDownList;
            SlidingDateRangePicker slidingDateRangePicker = controls[1] as SlidingDateRangePicker;

            var selectionConfig = new SelectionConfig
            {
                DocumentTypeId = ddlDocumentType.SelectedValue.AsInteger(),
                SlidingDateRangePickerDelimitedValues = slidingDateRangePicker.DelimitedValues,
            };

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

            var documentType = new DocumentTypeService( new RockContext() ).Get( selectionConfig.DocumentTypeId );
            var ddlDocumentType = controls[0] as RockDropDownList;
            var slidingDateRangePicker = controls[1] as SlidingDateRangePicker;
            if ( documentType != null )
            {
                ddlDocumentType.SetValue( selectionConfig.DocumentTypeId );
            }

            slidingDateRangePicker.DelimitedValues = selectionConfig.SlidingDateRangePickerDelimitedValues;
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
            var rockContext = ( RockContext ) serviceInstance.Context;

            SelectionConfig selectionConfig = SelectionConfig.Parse( selection );
            if ( selectionConfig == null )
            {
                return null;
            }

            var context = ( RockContext ) serviceInstance.Context;

            var selectedDocumentType = DocumentTypeCache.Get( selectionConfig.DocumentTypeId );

            var documentService = new DocumentService( rockContext );
            var documentQuery = documentService.Queryable().AsNoTracking()
                .Where( d => d.DocumentType.Id == selectedDocumentType.Id );

            if ( selectionConfig.SlidingDateRangePickerDelimitedValues.IsNotNullOrWhiteSpace() )
            {
                var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( selectionConfig.SlidingDateRangePickerDelimitedValues );
                if ( dateRange.Start.HasValue )
                {
                    documentQuery = documentQuery.Where( d => d.CreatedDateTime >= dateRange.Start.Value );
                }

                if ( dateRange.End.HasValue )
                {
                    documentQuery = documentQuery.Where( d => d.CreatedDateTime <= dateRange.End.Value );
                }
            }

            // Get all of the people corresponding to the qualifying documents.
            var qry = new PersonService( context ).Queryable().AsNoTracking()
                .Where( p => documentQuery.Any( d => d.EntityId == p.Id ) );

            // Retrieve the Filter Expression.
            var extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

            return extractedFilterExpression;
        }

        #endregion Public Methods

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
            /// Gets or sets the Document Type.
            /// </summary>
            /// <value>
            /// The Document Type.
            /// </value>
            public int DocumentTypeId { get; set; }

            /// <summary>
            /// Gets or sets the sliding date range picker delimited values.
            /// </summary>
            /// <value>The sliding date range picker delimited values.</value>
            public string SlidingDateRangePickerDelimitedValues { get; set; }

            /// <summary>
            /// Gets or sets the date range mode.
            /// </summary>
            /// <value>
            /// The date range mode.
            /// </value>
            public SlidingDateRangeType DateRangeMode { get; set; }

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
            public TimeUnitType TimeUnit { get; set; }

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
            /// Gets or sets a value indicating whether [use Sunday date].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [use Sunday date]; otherwise, <c>false</c>.
            /// </value>
            public bool UseSundayDate { get; set; }

            /// <summary>
            /// Parses the specified selection from a JSON or delimited string.
            /// </summary>
            /// <param name="selection">The selection.</param>
            /// <returns></returns>
            public static SelectionConfig Parse( string selection )
            {
                var selectionConfig = selection.FromJsonOrNull<SelectionConfig>();
                if ( selectionConfig == null )
                {
                    selectionConfig = new SelectionConfig();

                    // If the configuration is a delimited string then try to parse it the old fashioned way
                    string[] selectionValues = selection.Split( '|' );

                    // Index 0 is the document type.
                    if ( selectionValues.Count() > 0 )
                    {
                        selectionConfig.DocumentTypeId = selectionValues[0].AsInteger();
                    }
                    else
                    {
                        // If there are not at least one value in the selection string then it is not valid.
                        return null;
                    }

                    // Index 1 is the date range values
                    if ( selectionValues.Count() > 1 )
                    {
                        string[] dateRangeValues = selectionValues[1].Split( ',' );
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

                return selectionConfig;
            }
        }
    }
}
