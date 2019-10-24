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
using Rock.Data;
using Rock.Model;

namespace Rock.Reporting.DataFilter.Group
{
    /// <summary>
    ///     A DataFilter that selects people associated with a set of GroupTypes identified by a GroupType Data View.
    /// </summary>
    [Description( "Filter groups using a set of GroupTypes identified by a Group Type Data View" )]
    [Export( typeof(DataFilterComponent) )]
    [ExportMetadata( "ComponentName", "Group Type Data View Filter" )]
    public class GroupTypeDataViewFilter : RelatedDataViewFilterBase<Rock.Model.Group, Rock.Model.GroupType>
    {
        #region Overrides

        /// <summary>
        /// Creates a Linq Expression that can be applied to an IQueryable to filter the result set.
        /// </summary>
        /// <param name="entityType">The type of entity in the result set.</param>
        /// <param name="serviceInstance">A service instance that can be queried to obtain the result set.</param>
        /// <param name="parameterExpression">The input parameter that will be injected into the filter expression.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        /// <returns>
        /// A Linq Expression that can be used to filter an IQueryable.
        /// </returns>
        /// <exception cref="System.Exception">Filter issue(s):  + errorMessages.AsDelimited( ;  )</exception>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var settings = new FilterSettings( selection );

            var context = (RockContext)serviceInstance.Context;

            // Get the GroupType Data View that defines the set of candidates from which proximate GroupTypes can be selected.
            var dataView = DataComponentSettingsHelper.GetDataViewForFilterComponent( settings.DataViewGuid, context );

            // Evaluate the Data View that defines the candidate GroupTypes.
            var GroupTypeService = new GroupTypeService( context );

            var GroupTypeQuery = GroupTypeService.Queryable();

            if ( dataView != null )
            {
                GroupTypeQuery = DataComponentSettingsHelper.FilterByDataView( GroupTypeQuery, dataView, GroupTypeService );
            }

            // Get all of the Groups corresponding to the qualifying Group Types.
            var qry = new GroupService( context ).Queryable()
                                                  .Where( g => GroupTypeQuery.Any( gt => gt.Id == g.GroupTypeId ) );

            // Retrieve the Filter Expression.
            var extractedFilterExpression = FilterExpressionExtractor.Extract<Model.Group>( qry, parameterExpression, "g" );

            return extractedFilterExpression;
        }

        #endregion
    }
}