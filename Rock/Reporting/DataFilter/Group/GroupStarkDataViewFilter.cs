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
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Group
{
    /// <summary>
    /// Filter groups based on the available capacity in the group
    /// </summary>
    [Description( "Template filter for developers to use to start a new data view filter." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Stark Group Filter" )]
    public class GroupStarkDataViewFilter : DataFilterComponent
    {
        /// <summary>
        /// Gets the entity type that filter applies to.
        /// </summary>
        public override string AppliesToEntityType
        {
            get { return typeof( Rock.Model.Group ).FullName; }
        }

        /// <summary>
        /// Gets the title of this filter.
        /// </summary>
        /// <param name="entityType">The entity type that this filter will be applied to.</param>
        /// <returns></returns>
        public override string GetTitle( Type entityType )
        {
            return "Available Capacity";
        }

        /// <summary>
        /// Gets the section to display this filter in.
        /// </summary>
        public override string Section
        {
            get { return "Template Data View Filters"; }
        }

        /// <summary>
        /// Creates the child controls required by this filter.
        /// </summary>
        /// <returns>An array of controls to be added to the parent.</returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            // Create the standard comparison control, less than, greater than, etc.
            var ddlIntegerCompare = ComparisonHelper.ComparisonControl( ComparisonHelper.NumericFilterComparisonTypes );
            ddlIntegerCompare.Label = "Count";
            ddlIntegerCompare.ID = string.Format( "{0}_ddlIntegerCompare", filterControl.ID );
            ddlIntegerCompare.AddCssClass( "js-filter-compare" );
            filterControl.Controls.Add( ddlIntegerCompare );

            // Use a number box for the user to type in the numerical count to use.
            var nbCapacityCount = new NumberBox();
            nbCapacityCount.Label = "&nbsp;";
            nbCapacityCount.ID = string.Format( "{0}_nbCapacityCount", filterControl.ID );
            nbCapacityCount.AddCssClass( "js-capacity-count" );
            nbCapacityCount.FieldName = "Capacity Count";
            filterControl.Controls.Add( nbCapacityCount );

            return new Control[] { ddlIntegerCompare, nbCapacityCount };
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
            // 1: Get references to the controls we created in CreateChildControls
            var ddlCompare = controls[0] as DropDownList;
            var nbValue = controls[1] as NumberBox;

            // 2: Begin Comparison Row
            writer.AddAttribute( "class", "row field-criteria" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // 3: Render comparison type control
            writer.AddAttribute( "class", "col-md-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlCompare.RenderControl( writer );
            writer.RenderEndTag();

            // 4: Hide or show the NumberBox depending on the comparison type.
            ComparisonType comparisonType = ( ComparisonType ) ddlCompare.SelectedValue.AsInteger();
            nbValue.Style[HtmlTextWriterStyle.Display] = ( comparisonType == ComparisonType.IsBlank || comparisonType == ComparisonType.IsNotBlank ) ? "none" : string.Empty;

            // 5: Render NumberBox control.
            writer.AddAttribute( "class", "col-md-8" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            nbValue.RenderControl( writer );
            writer.RenderEndTag();

            // 6: End comparison row
            writer.RenderEndTag();
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by
        /// the user, the Filterfield control will set the description of the filter to
        /// whatever is returned by this property.  If including script, the controls
        /// parent container can be referenced through a '$content' variable that is
        /// set by the control before referencing this property.
        /// </summary>
        public override string GetClientFormatSelection( Type entityType )
        {
            return @"
function () {
    var result = 'Available Capacity';

    result += ' ' + $('.js-filter-compare', $content).find(':selected').text() + ' ';

    if ($('.js-capacity-count', $content).filter(':visible').length)
    {
        result += $('.js-capacity-count', $content).filter(':visible').val()
    }

    return result;

}
";
        }

        /// <summary>
        /// Gets the selection data from the user selection in the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            SelectionConfig selectionConfig = new SelectionConfig();
            DropDownList ddlCompare = controls[0] as DropDownList;
            NumberBox nbValue = controls[1] as NumberBox;
            selectionConfig.CapacityCompareId = ddlCompare.SelectedValue.ToIntSafe();
            selectionConfig.CapacityNumber = nbValue.Text.ToIntSafe();
            return selectionConfig.ToJson();
        }

        /// <summary>
        /// Sets the control values based on the selection data.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            SelectionConfig selectionConfig = SelectionConfig.Parse( selection );
            DropDownList ddlCompare = controls[0] as DropDownList;
            NumberBox nbValue = controls[1] as NumberBox;

                ComparisonType comparisonType = ( ComparisonType ) selectionConfig.CapacityCompareId;
                ddlCompare.SelectedValue = comparisonType.ConvertToInt().ToString();
                nbValue.Text = selectionConfig.CapacityNumber.ToString();
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
            // 1: Get user provided filter selections.
            SelectionConfig selectionConfig = SelectionConfig.Parse( selection );
            ComparisonType comparisonType = ( ComparisonType ) selectionConfig.CapacityCompareId;

            int? capacityCountValue = selectionConfig.CapacityNumber;

            // 2: Generate generic queryable object of type Group.
            var query = new GroupService( ( RockContext ) serviceInstance.Context ).Queryable();

            // 3: Generate the query for available capacity.
            var capacityCountQuery = query
                .Where( p =>
                    ( ( p.GroupCapacity ?? int.MaxValue ) - p.Members.Count( a => a.GroupMemberStatus == GroupMemberStatus.Active ) )
                    == capacityCountValue );

            // 4: Generate the query for capacity rule.
            var capacityRuleQuery = query.Where( p => p.GroupType.GroupCapacityRule != GroupCapacityRule.Hard );

            // 5: Extract the pure expressions.
            var compareCountExpression = FilterExpressionExtractor.Extract<Rock.Model.Group>( capacityCountQuery, parameterExpression, "p" ) as BinaryExpression;
            var capacityRuleExpression = FilterExpressionExtractor.Extract<Rock.Model.Group>( capacityRuleQuery, parameterExpression, "p" ) as BinaryExpression;

            // 6: Alter the comparison type to match user selection.
            var compareResultExpression = FilterExpressionExtractor.AlterComparisonType( comparisonType, compareCountExpression, 0 );

            // 7: Generate final compound expression.
            BinaryExpression result = Expression.MakeBinary( ExpressionType.Or, compareResultExpression, capacityRuleExpression );
            return result;
        }

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
                // Add values to set defaults / populate upon object creation.
            }

            /// <summary>
            /// 
            /// </summary>
            public int CapacityCompareId { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public int? CapacityNumber { get; set; }

            /// <summary>
            /// Parses the specified selection from a JSON or delimited string.  If a delimited string, position 0 is the capacity compare ID, 1 is the capacity number.
            /// </summary>
            /// <param name="selection">The selection.</param>
            /// <returns></returns>
            public static SelectionConfig Parse( string selection )
            {
                var selectionConfig = selection.FromJsonOrNull<SelectionConfig>();

                // This will only occur when the selection string is not JSON.
                if ( selectionConfig == null )
                {
                    selectionConfig = new SelectionConfig();

                    // If the configuration is a pipe-delimited string, then try to parse it the old-fashioned way.
                    string[] selectionValues = selection.Split( '|' );

                    // Index 0 is the capacity compare ID.
                    // Index 1 is the capacity number.
                    if ( selectionValues.Count() >= 2 )
                    {
                        selectionConfig.CapacityCompareId = selectionValues[0].AsInteger();
                        selectionConfig.CapacityNumber = selectionValues[1].AsIntegerOrNull();
                    }
                    else
                    {
                        // If there are not at least 2 values in the selection string then it is not a valid selection.
                        return null;
                    }
                }

                return selectionConfig;
            }
        }
    }
}