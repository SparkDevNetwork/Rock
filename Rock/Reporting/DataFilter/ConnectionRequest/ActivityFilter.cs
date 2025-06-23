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
using Rock.Net;
using Rock.ViewModels.Controls;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.ConnectionRequest
{
    /// <summary>
    ///
    /// </summary>
    [Description( "Would allow filtering by activity types." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Activity Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "1511AFEF-1E60-4056-B861-5EBED0362BE4" )]
    public class ActivityFilter : DataFilterComponent
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
            get { return typeof( Rock.Model.ConnectionRequest ).FullName; }
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

        #region Configuration

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Reporting/DataFilters/ConnectionRequest/activityFilter.obs" )
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var config = SelectionConfig.Parse( selection );

            var activityTypeOptions = new ConnectionActivityTypeService( new RockContext() ).Queryable( "ConnectionType" )
                .AsNoTracking()
                .Where( a => a.IsActive )
                .OrderBy( a => a.ConnectionTypeId.HasValue )
                .ThenBy( a => a.Name )
                .ToList()
                .Select( a => a.ToListItemBag() )
                .ToList();

            var isBlank = selection.Trim() == string.Empty;

            return new Dictionary<string, string>
            {
                { "activityType", config?.ConnectionActivityTypeGuid?.ToString() },
                { "activityTypeOptions", activityTypeOptions.ToCamelCaseJson(false, true) },
                { "comparisonType", (isBlank ? ComparisonType.GreaterThanOrEqualTo : config?.IntegerCompare).ConvertToInt().ToString() },
                { "minimumCount", isBlank ? "1" : config?.MinimumCount.ToString() },
                { "dateRange", config?.SlidingDateRangeDelimitedValues },
            };
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            var selectionConfig = new SelectionConfig
            {
                ConnectionActivityTypeGuid = data.GetValueOrNull( "activityType" )?.AsGuid(),
                IntegerCompare = data.GetValueOrDefault( "comparisonType", ComparisonType.GreaterThanOrEqualTo.ConvertToInt().ToString() ).ConvertToEnum<ComparisonType>(),
                MinimumCount = data.GetValueOrDefault( "minimumCount", "1" ).AsInteger(),
                SlidingDateRangeDelimitedValues = data.GetValueOrDefault( "dateRange", "All||||" ),
            };

            return selectionConfig.ToJson();
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
            return "Activity";
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
            return "'Activity ' + " +
                "'\\'' + $('.js-activity-type', $content).find(':selected').text() + '\\' ' + " +
                "$('.js-filter-compare', $content).find(':selected').text() + ' ' + " +
                "$('.js-count', $content).val() + ' times. Date Range: ' + " +
                "$('.js-slidingdaterange-text-value', $content).val()";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string s = "Activity";

            var selectionConfig = SelectionConfig.Parse( selection );
            if ( selectionConfig != null && selectionConfig.ConnectionActivityTypeGuid.HasValue )
            {
                var activityType = new ConnectionActivityTypeService( new RockContext() ).Get( selectionConfig.ConnectionActivityTypeGuid.Value );
                var activityName = GetActivityName( activityType );
                var dateRangeString = string.Empty;
                if ( selectionConfig.SlidingDateRangeDelimitedValues.IsNotNullOrWhiteSpace() )
                {
                    var dateRange = SlidingDateRangePicker.FormatDelimitedValues( selectionConfig.SlidingDateRangeDelimitedValues );
                    if ( dateRangeString.IsNotNullOrWhiteSpace() )
                    {
                        dateRangeString += $" Date Range: {dateRangeString}";
                    }
                }

                s = string.Format(
                    "Activity '{0}' {1} {2} times. {3}",
                    activityName,
                    selectionConfig.IntegerCompare.ConvertToString(),
                    selectionConfig.MinimumCount,
                    dateRangeString );
            }

            return s;
        }

#if WEBFORMS

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var ddlActivityType = new RockDropDownList();
            ddlActivityType.ID = filterControl.ID + "_ddlActivityType";
            ddlActivityType.AddCssClass( "js-activity-type" );
            ddlActivityType.Label = "Activity Type";
            filterControl.Controls.Add( ddlActivityType );
            var activityTypes = new ConnectionActivityTypeService( new RockContext() ).Queryable( "ConnectionType" ).AsNoTracking().Where( a => a.IsActive )
                .OrderBy( a => a.ConnectionTypeId.HasValue )
                .ThenBy( a => a.Name )
                .ToList();
            ddlActivityType.Items.Clear();
            ddlActivityType.Items.Insert( 0, new ListItem() );
            foreach ( var activityType in activityTypes )
            {
                var activityName = GetActivityName( activityType );
                ddlActivityType.Items.Add( new ListItem( activityName, activityType.Guid.ToString() ) );
            }

            var ddlIntegerCompare = ComparisonHelper.ComparisonControl( ComparisonHelper.NumericFilterComparisonTypes );
            ddlIntegerCompare.Label = "Count";
            ddlIntegerCompare.ID = filterControl.ID + "_ddlIntegerCompare";
            ddlIntegerCompare.AddCssClass( "js-filter-compare" );
            filterControl.Controls.Add( ddlIntegerCompare );

            var nbCount = new NumberBox();
            nbCount.ID = filterControl.ID + "_nbCount";
            nbCount.Label = "&nbsp;"; // give it whitespace label so it lines up nicely
            nbCount.AddCssClass( "js-count" );
            filterControl.Controls.Add( nbCount );

            var slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.Label = "Date Range";
            slidingDateRangePicker.ID = filterControl.ID + "_slidingDateRangePicker";
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            filterControl.Controls.Add( slidingDateRangePicker );

            var controls = new Control[4] { ddlActivityType, ddlIntegerCompare, nbCount, slidingDateRangePicker };

            // convert pipe to comma delimited
            var defaultDelimitedValues = slidingDateRangePicker.DelimitedValues.Replace( "|", "," );
            var defaultCount = 1;

            // set the default values in case this is a newly added filter
            var selectionConfig = new SelectionConfig()
            {
                IntegerCompare = ComparisonType.GreaterThanOrEqualTo,
                MinimumCount = defaultCount,
                SlidingDateRangeDelimitedValues = defaultDelimitedValues
            };

            SetSelection(
                entityType,
                controls,
                selectionConfig.ToJson() );

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
            var ddlActivityType = controls[0] as RockDropDownList;
            var ddlIntegerCompare = controls[1] as RockDropDownList;
            var nbCount = controls[2] as NumberBox;
            var slidingDateRangePicker = controls[3] as SlidingDateRangePicker;

            // Row 1
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlActivityType.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // Row 2
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlIntegerCompare.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-1" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            nbCount.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-7" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            slidingDateRangePicker.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
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

            if ( controls.Count() >= 4 )
            {
                var ddlActivityType = controls[0] as RockDropDownList;
                var ddlIntegerCompare = controls[1] as RockDropDownList;
                var nbCount = controls[2] as NumberBox;
                var slidingDateRangePicker = controls[3] as SlidingDateRangePicker;

                selectionConfig.ConnectionActivityTypeGuid = ddlActivityType.SelectedValueAsGuid();
                selectionConfig.IntegerCompare = ddlIntegerCompare.SelectedValueAsEnum<ComparisonType>( ComparisonType.GreaterThanOrEqualTo );
                selectionConfig.SlidingDateRangeDelimitedValues = slidingDateRangePicker.DelimitedValues;
                selectionConfig.MinimumCount = nbCount.IntegerValue ?? 1;
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

            var ddlActivityType = controls[0] as RockDropDownList;
            var ddlIntegerCompare = controls[1] as RockDropDownList;
            var nbCount = controls[2] as NumberBox;
            var slidingDateRangePicker = controls[3] as SlidingDateRangePicker;

            ddlIntegerCompare.SelectedValue = selectionConfig.IntegerCompare.ConvertToInt().ToString();
            nbCount.Text = selectionConfig.MinimumCount.ToString();
            slidingDateRangePicker.DelimitedValues = selectionConfig.SlidingDateRangeDelimitedValues;
            ddlActivityType.SetValue( selectionConfig.ConnectionActivityTypeGuid );
        }

#endif

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
            if ( !selectionConfig.ConnectionActivityTypeGuid.HasValue )
            {
                return null;
            }

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( selectionConfig.SlidingDateRangeDelimitedValues );
            var rockContext = serviceInstance.Context as RockContext;
            var connectionRequestActivityQry = new ConnectionRequestActivityService( rockContext ).Queryable();

            if ( dateRange.Start.HasValue )
            {
                var startDate = dateRange.Start.Value;
                connectionRequestActivityQry = connectionRequestActivityQry.Where( a => a.CreatedDateTime >= startDate );
            }

            if ( dateRange.End.HasValue )
            {
                var endDate = dateRange.End.Value;
                connectionRequestActivityQry = connectionRequestActivityQry.Where( a => a.CreatedDateTime < endDate );
            }

            connectionRequestActivityQry = connectionRequestActivityQry.Where( a => a.ConnectionActivityType.Guid == selectionConfig.ConnectionActivityTypeGuid.Value );

            var qry = new ConnectionRequestService( rockContext ).Queryable()
                  .Where( p => connectionRequestActivityQry.Where( xx => xx.ConnectionRequestId == p.Id ).Count() == selectionConfig.MinimumCount );

            BinaryExpression compareEqualExpression = FilterExpressionExtractor.Extract<Rock.Model.ConnectionRequest>( qry, parameterExpression, "p" ) as BinaryExpression;
            BinaryExpression result = FilterExpressionExtractor.AlterComparisonType( selectionConfig.IntegerCompare, compareEqualExpression, null );

            return result;
        }

        private string GetActivityName( ConnectionActivityType activityType )
        {
            var activityName = string.Empty;
            if ( activityType != null )
            {
                activityName = activityType.Name;
                if ( activityType.ConnectionType != null )
                {
                    activityName += $" ( {activityType.ConnectionType.Name} )";
                }
            }

            return activityName;
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
            /// Gets or sets the connection activity type identifiers.
            /// </summary>
            /// <value>
            /// The connection activity type identifiers.
            /// </value>
            public Guid? ConnectionActivityTypeGuid { get; set; }

            /// <summary>
            /// Gets or sets the integer compare.
            /// </summary>
            /// <value>
            /// The integer compare.
            /// </value>
            public ComparisonType IntegerCompare { get; set; }

            /// <summary>
            /// Gets or sets the minimum count.
            /// </summary>
            /// <value>
            /// The minimum count.
            /// </value>
            public int MinimumCount { get; set; }

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