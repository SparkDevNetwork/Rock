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
using Rock.Web.Cache;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    ///     A DataFilter that selects People by their presence in the set of records returned by a History Data View.
    /// </summary>
    [Description( "Select people that are represented in the History entries returned by a specified Data View" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "History Data View" )]
    [Rock.SystemGuid.EntityTypeGuid( "EE2E4C8E-0A0C-4270-9DD0-00BB061820F2" )]
    public class HistoryDataViewFilter : RelatedDataViewFilterBase<Rock.Model.Person, Rock.Model.History>
    {
        #region Overrides

        /// <summary>
        /// Get the name of the related entity that is used to filter the result set.
        /// </summary>
        /// <returns></returns>
        protected override string GetRelatedEntityName()
        {
            return "History Record";
        }

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
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var settings = new FilterSettings( selection );

            var context = ( RockContext ) serviceInstance.Context;

            // Get the Data View that defines the set of related records from which entities can be selected.
            var dataView = DataComponentSettingsHelper.GetDataViewForFilterComponent( settings.DataViewGuid, context );

            // Evaluate the Data View that defines the related records.
            var historyService = new HistoryService( context );

            var historyQuery = historyService.Queryable();

            if ( dataView != null )
            {
                historyQuery = DataComponentSettingsHelper.FilterByDataView( historyQuery, dataView, historyService );
            }

            // Select only those History records that are either related to a Person, or affect a Person.
            int personEntityTypeId = EntityTypeCache.GetId( typeof( Model.Person ) ).GetValueOrDefault();

            historyQuery = historyQuery.Where( x => x.EntityTypeId == personEntityTypeId );

            // Get all of the People corresponding to the qualifying History records.
            var qry = new PersonService( context )
                .Queryable()
                .Where( p =>
                    historyQuery.Any( h =>
                        h.EntityTypeId == personEntityTypeId
                        && h.EntityId == p.Id
                        || ( h.RelatedEntityTypeId == personEntityTypeId && h.RelatedEntityId == p.Id )
                    )
                );

            // Retrieve the Filter Expression.
            var extractedFilterExpression = FilterExpressionExtractor.Extract<Model.Person>( qry, parameterExpression, "p" );

            return extractedFilterExpression;
        }

        #endregion
    }
}