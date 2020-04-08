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

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    ///     A Data Filter to select Donor by their Connection Requests from a Connection Request View.
    /// </summary>
    [Description( "Select Person by their Connection Requests from a Benevolence Connection View." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Connection Request View" )]
    public class ConnectionRequestDataViewFilter : RelatedDataViewFilterBase<Rock.Model.Person, Rock.Model.ConnectionRequest>
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

            // Get the Connection Request Data View.
            var dataView = DataComponentSettingsHelper.GetDataViewForFilterComponent( settings.DataViewGuid, context );

            // Evaluate the Data View that defines the Person's Connection Request.
            var connectionRequestService = new ConnectionRequestService( context );

            var connectionRequestQuery = connectionRequestService.Queryable();

            if ( dataView != null )
            {
                connectionRequestQuery = DataComponentSettingsHelper.FilterByDataView( connectionRequestQuery, dataView, connectionRequestService );
            }

            var connectionRequestPersonsKey = connectionRequestQuery.Select( a => a.PersonAliasId );
            // Get all of the Person corresponding to the qualifying Benevolence Requests.
            var qry = new PersonService( context ).Queryable()
                                                  .Where( g => g.Aliases.Any( k => connectionRequestPersonsKey.Contains( k.Id ) ) );

            // Retrieve the Filter Expression.
            var extractedFilterExpression = FilterExpressionExtractor.Extract<Model.Person>( qry, parameterExpression, "g" );

            return extractedFilterExpression;
        }

        #endregion

    }
}
