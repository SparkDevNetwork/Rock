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

namespace Rock.Reporting.DataFilter.BenevolenceRequest
{
    /// <summary>
    ///     A Data Filter to select Benevolence Requests by their Benevolence Results from a Benevolence Result View.
    /// </summary>
    [Description( "Select Benevolence Requests by their Benevolence results from a Benevolence Result View." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Benevolence Result View" )]
    public class BenevolenceResultDataViewFilter : RelatedDataViewFilterBase<Rock.Model.BenevolenceRequest, Rock.Model.BenevolenceResult>
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

            var context = ( RockContext ) serviceInstance.Context;

            // Get the Benevolence Request Data View.
            var dataView = DataComponentSettingsHelper.GetDataViewForFilterComponent( settings.DataViewGuid, context );

            // Evaluate the Data View that defines the Benevolence Result.
            var benevolenceResultService = new BenevolenceResultService( context );

            var benevolenceResultQuery = benevolenceResultService.Queryable();

            if ( dataView != null )
            {
                benevolenceResultQuery = DataComponentSettingsHelper.FilterByDataView( benevolenceResultQuery, dataView, benevolenceResultService );
            }

            var benevolenceRequestKey = benevolenceResultQuery.Select( a => a.BenevolenceRequestId );
            // Get all of the result corresponding to the qualifying Benevolence Requests.
            var qry = new BenevolenceRequestService( context ).Queryable()
                                                  .Where( g => benevolenceRequestKey.Contains( g.Id ) );

            // Retrieve the Filter Expression.
            var extractedFilterExpression = FilterExpressionExtractor.Extract<Model.BenevolenceRequest>( qry, parameterExpression, "g" );

            return extractedFilterExpression;
        }

        #endregion
    }
}
