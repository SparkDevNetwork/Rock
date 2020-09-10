﻿// <copyright>
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

namespace Rock.Reporting.DataFilter.PrayerRequest
{
    /// <summary>
    /// Retrieve the prayer requests involving a set of people in a Data View.
    /// </summary>
    [Description( "Retrieve the prayer requests involving a set of people in a Data View." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Contains People" )]
    public class ContainsPeopleFilter : RelatedDataViewFilterBase<Rock.Model.PrayerRequest, Rock.Model.Person>
    {
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

            //
            // Define Candidate People.
            //
            var dataView = DataComponentSettingsHelper.GetDataViewForFilterComponent( settings.DataViewGuid, context );

            var personService = new PersonService( context );

            var personQuery = personService.Queryable();

            if ( dataView != null )
            {
                personQuery = DataComponentSettingsHelper.FilterByDataView( personQuery, dataView, personService );
            }

            var personKeys = personQuery.Select( x => x.Id );

            //
            // Construct the Query to return the list of Prayer Requests matching the filter conditions.
            //
            var prayerRequestQuery = new PrayerRequestService( context ).Queryable();

            prayerRequestQuery = prayerRequestQuery.Where( p => personKeys.Contains( p.RequestedByPersonAlias.PersonId ) );

            var result = FilterExpressionExtractor.Extract<Rock.Model.PrayerRequest>( prayerRequestQuery, parameterExpression, "p" );

            return result;
        }
    }
}