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
    /// Select people through their associated interactions.
    /// </summary>
    /// <seealso cref="Rock.Reporting.DataFilterComponent" />
    [Description( "Select Person through their Interaction." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Interaction View" )]
    public class InteractionDataViewFilter : RelatedDataViewFilterBase<Rock.Model.Person, Rock.Model.Interaction>
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

            // Get the Financial Transaction Data View.
            var dataView = DataComponentSettingsHelper.GetDataViewForFilterComponent( settings.DataViewGuid, context );

            // Evaluate the Data View that defines the Person's Interaction.
            var interactionService = new InteractionService( context );

            var interactionQuery = interactionService.Queryable();

            if ( dataView != null )
            {
                interactionQuery = DataComponentSettingsHelper.FilterByDataView( interactionQuery, dataView, interactionService );
            }

            var interactionPersonsKey = interactionQuery.Where(a => a.PersonAliasId.HasValue).Select( a => a.PersonAliasId.Value );
            // Get all of the Person corresponding to the qualifying Financial Transactions.
            var qry = new PersonService( context ).Queryable()
                                                  .Where( g => g.Aliases.Any( k => interactionPersonsKey.Contains( k.Id ) ) );

            // Retrieve the Filter Expression.
            var extractedFilterExpression = FilterExpressionExtractor.Extract<Model.Person>( qry, parameterExpression, "g" );

            return extractedFilterExpression;
        }

        #endregion
    }
}
