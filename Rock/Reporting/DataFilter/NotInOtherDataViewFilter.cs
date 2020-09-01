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
using System.Linq.Expressions;

using Rock.Data;
using Rock.Model;

namespace Rock.Reporting.DataFilter
{
    /// <summary>
    /// Exclude entities using another dataview
    /// </summary>
    [Description( "Exclude entities that are in another dataview" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Not In Other Data View Filter" )]
    public class NotInOtherDataViewFilter : OtherDataViewFilter, IDataFilterWithOverrides
    {
        #region Public Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public override string GetTitle( Type entityType )
        {
            return "Not In Existing Data View";
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
            var title = "Not Included In Data View:";
            return $@"Rock.reporting.formatFilterForOtherDataViewFilter('{title}', $content)";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );

            int? dataviewId = selectionConfig.DataViewId;
            if ( dataviewId.HasValue && dataviewId > 0 )
            {
                var dataView = new DataViewService( new RockContext() ).GetNoTracking( dataviewId.Value );
                if ( dataView != null )
                {
                    return $"Not Included in '{dataView.Name}' Data View";
                }
                else
                {
                    // a DataView id was specified, but we couldn't find it. So we have a problem.
                    return $"Error. DataView not found for DataViewId: {dataviewId }";
                }
            }

            // no dataview was specified, so let's show...
            return "Not In Another Data View";
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Filter issue(s):  + errorMessages.AsDelimited( ;  )</exception>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            return GetExpressionWithOverrides( entityType, serviceInstance, parameterExpression, null, selection );
        }

        /// <summary>
        /// Gets the expression with overrides.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="dataViewFilterOverrides">The data view filter overrides.</param>
        /// <param name="selection">A formatted string representing the filter settings: FieldName, <see cref="ComparisonType">Comparison Type</see>, (optional) Comparison Value(s)</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Filter issue(s): " + errorMessages.AsDelimited( "; " )</exception>
        public new Expression GetExpressionWithOverrides( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, DataViewFilterOverrides dataViewFilterOverrides, string selection )
        {
            // first get the 'IN other dataview expression' from the inherited from OtherDataViewFilter
            var baseExpression = base.GetExpressionWithOverrides( entityType, serviceInstance, parameterExpression, dataViewFilterOverrides, selection );

            if ( baseExpression != null )
            {
                return Expression.Not( baseExpression );
            }
            else
            {
                return baseExpression;
            }
        }

        #endregion
    }
}