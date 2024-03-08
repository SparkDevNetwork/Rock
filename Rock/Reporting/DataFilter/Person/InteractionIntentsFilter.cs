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
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// A Data Filter to select People based on their interactions with content having the specified intents.
    /// </summary>
    [Description( "Filter people based on their interactions with content having the specified intents." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Interaction Intents Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "1B32C42D-176E-449F-A86F-F2BF21169878" )]
    public class InteractionIntentsFilter : DataFilterComponent
    {
        #region Settings

        /// <summary>
        /// Get and set the filter settings from DataViewFilter.Selection.
        /// </summary>
        private class SelectionConfig
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SelectionConfig"/> class.
            /// </summary>
            public SelectionConfig()
            {
                // Add values to set defaults / populate upon object creation.
            }

            /// <summary>
            /// Gets or sets the interaction count comparison type.
            /// </summary>
            /// <value>
            /// The interaction count comparison type
            /// </value>
            public string InteractionCountComparisonType { get; set; }

            /// <summary>
            /// Gets or sets the interaction count.
            /// </summary>
            /// <value>
            /// The interaction count.
            /// </value>
            public int InteractionCount { get; set; }

            /// <summary>
            /// Gets or sets the interaction intent value identifiers.
            /// </summary>
            /// <value>
            /// The interaction intent value identifiers.
            /// </value>
            public List<int> InteractionIntentValueIds { get; set; }

            /// <summary>
            /// Gets or sets the delimited date range values.
            /// </summary>
            /// <value>
            /// The delimited date range values.
            /// </value>
            public string DelimitedDateRangeValues { get; set; }

            /// <summary>
            /// Parses the specified selection from a JSON string.
            /// </summary>
            /// <param name="selection">The filter selection control.</param>
            /// <returns></returns>
            public static SelectionConfig Parse( string selection )
            {
                return selection.FromJsonOrNull<SelectionConfig>();
            }
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public override string AppliesToEntityType
        {
            get { return "Rock.Model.Person"; }
        }

        /// <inheritdoc/>
        public override string Section
        {
            get { return "Additional Filters"; }
        }

        #endregion Properties

        #region Public Methods

        /// <inheritdoc/>
        public override string GetTitle( Type entityType )
        {
            return "Interaction Intents";
        }

        /// <inheritdoc/>
        public override string GetClientFormatSelection( Type entityType )
        {
            return @"
function() {
    var comparisonType = $('.js-comparison-type', $content).find(':selected').text();
    var count = $('.js-count', $content).val();
    if (!count) {
        count = '0';
    }

    result = comparisonType ? comparisonType + ' ' + count + ' Interactions' : 'Interactions';

    var intentNames = $('.js-intents + .chosen-container', $content).find('.chosen-choices .search-choice span:first-child');
    if (intentNames.length > 0) {
        var intentNamesDelimitedList = intentNames.map(function() {
            return $(this).text();
        }).get().join(', ');
        result += ' with: ' + intentNamesDelimitedList + '.';
    }

    var dateRangeText = $('.js-date-range', $content).find('.js-slidingdaterange-text-value').val();
    if(dateRangeText) {
        result +=  ' Date Range: ' + dateRangeText
    }

    return result;
}
";
        }

        /// <inheritdoc/>
        public override string FormatSelection( Type entityType, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );

            var resultSb = new StringBuilder( "Interactions" );

            if ( selectionConfig != null )
            {
                var comparisonType = selectionConfig.InteractionCountComparisonType.ConvertToEnumOrNull<ComparisonType>();
                if ( comparisonType != null )
                {
                    resultSb.Clear();
                    resultSb.Append( $"{comparisonType.ConvertToString()} {selectionConfig.InteractionCount} Interactions" );
                }

                if ( selectionConfig.InteractionIntentValueIds?.Any() == true )
                {
                    var intentNames = new List<string>();
                    foreach ( var intentValueId in selectionConfig.InteractionIntentValueIds )
                    {
                        var intentValue = DefinedValueCache.Get( intentValueId );
                        if ( intentValue?.Value.IsNotNullOrWhiteSpace() == true )
                        {
                            intentNames.Add( intentValue.Value );
                        }
                    }

                    if ( intentNames.Any() )
                    {
                        resultSb.Append( $" with: {intentNames.JoinStrings( ", " )}." );
                    }
                }

                if ( selectionConfig.DelimitedDateRangeValues.IsNotNullOrWhiteSpace() )
                {
                    var dateRangeString = SlidingDateRangePicker.FormatDelimitedValues( selectionConfig.DelimitedDateRangeValues );
                    if ( dateRangeString.IsNotNullOrWhiteSpace() )
                    {
                        resultSb.Append( $" Date Range: {dateRangeString}" );
                    }
                }
            }

            return resultSb.ToString();
        }

        /// <inheritdoc/>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var controls = new List<Control>();

            // Define control: interaction count comparison type drop down list.
            var ComparisonTypes =
                ComparisonType.EqualTo |
                ComparisonType.LessThan |
                ComparisonType.LessThanOrEqualTo |
                ComparisonType.GreaterThan |
                ComparisonType.GreaterThanOrEqualTo;
            var ddlInteractionCountComparisonType = ComparisonHelper.ComparisonControl( ComparisonTypes );
            ddlInteractionCountComparisonType.ID = filterControl.GetChildControlInstanceName( "_interactionCountComparisonType" );
            ddlInteractionCountComparisonType.AddCssClass( "js-comparison-type" );
            filterControl.Controls.Add( ddlInteractionCountComparisonType );
            controls.Add( ddlInteractionCountComparisonType );

            // Define control: interaction count number box.
            var nbInteractionCount = new NumberBox();
            nbInteractionCount.ID = filterControl.GetChildControlInstanceName( "_interactionCount" );
            nbInteractionCount.AddCssClass( "js-count" );
            nbInteractionCount.FieldName = "Count";
            filterControl.Controls.Add( nbInteractionCount );
            controls.Add( nbInteractionCount );

            // Define control: "interactions with the" label.
            var lblInteractionsWith = new Label();
            lblInteractionsWith.Text = " interactions with the ";
            filterControl.Controls.Add( lblInteractionsWith );
            controls.Add( lblInteractionsWith );

            // Define control: Interaction Intents Defined Values picker.
            var dvpInteractionIntents = new DefinedValuesPickerEnhanced();
            dvpInteractionIntents.ID = filterControl.GetChildControlInstanceName( "_interactionIntents" );
            dvpInteractionIntents.AddCssClass( "js-intents" );
            dvpInteractionIntents.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.INTERACTION_INTENT ) )?.Id;
            filterControl.Controls.Add( dvpInteractionIntents );
            controls.Add( dvpInteractionIntents );

            // Define control: "intent(s)" label.
            var lblIntents = new Label();
            lblIntents.Text = " intent(s) ";
            filterControl.Controls.Add( lblIntents );
            controls.Add( lblIntents );

            // Define control: "in the following date range" label.
            var lblDateRange = new Label();
            lblDateRange.Text = " in the following date range ";
            filterControl.Controls.Add( lblDateRange );
            controls.Add( lblDateRange );

            // Define control: Date range sliding date range picker.
            var sdrpDateRange = new SlidingDateRangePicker();
            sdrpDateRange.ID = filterControl.GetChildControlInstanceName( "_dateRange" );
            sdrpDateRange.AddCssClass( "js-date-range" );
            sdrpDateRange.Label = "Date Range";
            sdrpDateRange.Help = "The date range within which the interaction took place";
            filterControl.Controls.Add( sdrpDateRange );
            controls.Add( sdrpDateRange );

            return controls.ToArray();
        }

        /// <inheritdoc/>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            // Get references to the controls we created in CreateChildControls.
            var ddlInteractionCountComparisonType = controls[0] as DropDownList;
            var nbInteractionCount = controls[1] as NumberBox;
            var lblInteractionsWith = controls[2] as Label;
            var dvpInteractionIntents = controls[3] as DefinedValuesPickerEnhanced;
            var lblIntents = controls[4] as Label;
            var lblDateRange = controls[5] as Label;
            var sdrpDateRange = controls[6] as SlidingDateRangePicker;

            writer.AddAttribute( "class", "row field-criteria" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div ); // begin first row

            writer.AddAttribute( "class", "col-md-3" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div ); // interaction count comparison type drop down list
            ddlInteractionCountComparisonType.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-1" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div ); // interaction count number box
            nbInteractionCount.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-2 mt-1" );
            lblInteractionsWith.RenderControl( writer ); // "interactions with the" label

            writer.AddAttribute( "class", "col-md-5" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div ); // Interaction Intents Defined Values picker
            dvpInteractionIntents.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-1 mt-1" );
            lblIntents.RenderControl( writer ); // "intent(s)" label

            writer.RenderEndTag(); // end first row

            writer.AddAttribute( "class", "row mt-3" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div ); // begin second row

            writer.AddAttribute( "class", "col-md-3 mt-4" );
            lblDateRange.RenderControl( writer ); // "in the following date range" label

            sdrpDateRange.RenderControl( writer ); // Date range sliding date range picker

            writer.AddAttribute( "class", "col-md-2" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div ); // whiteSpace
            writer.RenderEndTag();

            writer.RenderEndTag(); // end second row
        }

        /// <inheritdoc/>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            // Get references to the controls we created in CreateChildControls.
            var ddlInteractionCountComparisonType = controls[0] as DropDownList;
            var nbInteractionCount = controls[1] as NumberBox;
            var dvpInteractionIntents = controls[3] as DefinedValuesPickerEnhanced;
            var sdrpDateRange = controls[6] as SlidingDateRangePicker;

            var selectionConfig = new SelectionConfig();
            selectionConfig.InteractionCountComparisonType = ddlInteractionCountComparisonType.SelectedValue;
            selectionConfig.InteractionCount = nbInteractionCount.IntegerValue ?? 1;
            selectionConfig.InteractionIntentValueIds = dvpInteractionIntents.SelectedValuesAsInt;
            selectionConfig.DelimitedDateRangeValues = sdrpDateRange.DelimitedValues;

            return selectionConfig.ToJson();
        }

        /// <inheritdoc/>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );

            // Get references to the controls we created in CreateChildControls.
            var ddlInteractionCountComparisonType = controls[0] as DropDownList;
            var nbInteractionCount = controls[1] as NumberBox;
            var dvpInteractionIntents = controls[3] as DefinedValuesPickerEnhanced;
            var sdrpDateRange = controls[6] as SlidingDateRangePicker;

            ddlInteractionCountComparisonType.SelectedValue = selectionConfig.InteractionCountComparisonType;
            nbInteractionCount.IntegerValue = selectionConfig.InteractionCount;
            dvpInteractionIntents.SetValues( selectionConfig.InteractionIntentValueIds );
            sdrpDateRange.DelimitedValues = selectionConfig.DelimitedDateRangeValues;
        }

        /// <inheritdoc/>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var rockContext = ( RockContext ) serviceInstance.Context;
            var selectionConfig = SelectionConfig.Parse( selection );
            var comparisonType = selectionConfig
                .InteractionCountComparisonType
                .ConvertToEnumOrNull<ComparisonType>();

            var channelTypeMediumValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_INTERACTION_INTENTS )?.Id;

            var interactionQry = new InteractionService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( i =>
                    i.InteractionComponent.EntityId.HasValue
                    && selectionConfig.InteractionIntentValueIds.Contains( i.InteractionComponent.EntityId.Value )
                    && i.InteractionComponent.InteractionChannel.ChannelTypeMediumValueId == channelTypeMediumValueId
                );

            if ( selectionConfig.DelimitedDateRangeValues.IsNotNullOrWhiteSpace() )
            {
                var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( selectionConfig.DelimitedDateRangeValues );
                if ( dateRange.Start.HasValue )
                {
                    interactionQry = interactionQry.Where( n => n.CreatedDateTime >= dateRange.Start.Value );
                }

                if ( dateRange.End.HasValue )
                {
                    interactionQry = interactionQry.Where( n => n.CreatedDateTime <= dateRange.End.Value );
                }
            }

            var personQry = new PersonService( rockContext ).Queryable();

            if ( comparisonType != null )
            {
                switch ( comparisonType )
                {
                    case ComparisonType.EqualTo:
                        personQry = personQry.Where( p => interactionQry.Where( xx => xx.PersonAlias.PersonId == p.Id ).Count() == selectionConfig.InteractionCount );
                        break;
                    case ComparisonType.LessThan:
                        personQry = personQry.Where( p => interactionQry.Where( xx => xx.PersonAlias.PersonId == p.Id ).Count() < selectionConfig.InteractionCount );
                        break;
                    case ComparisonType.LessThanOrEqualTo:
                        personQry = personQry.Where( p => interactionQry.Where( xx => xx.PersonAlias.PersonId == p.Id ).Count() <= selectionConfig.InteractionCount );
                        break;
                    case ComparisonType.GreaterThan:
                        personQry = personQry.Where( p => interactionQry.Where( xx => xx.PersonAlias.PersonId == p.Id ).Count() > selectionConfig.InteractionCount );
                        break;
                    case ComparisonType.GreaterThanOrEqualTo:
                        personQry = personQry.Where( p => interactionQry.Where( xx => xx.PersonAlias.PersonId == p.Id ).Count() >= selectionConfig.InteractionCount );
                        break;
                }
            }

            return FilterExpressionExtractor.Extract<Rock.Model.Person>( personQry, parameterExpression, "p" );
        }

        #endregion Public Methods
    }
}
