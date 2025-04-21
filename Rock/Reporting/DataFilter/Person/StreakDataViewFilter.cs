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
    /// A DataFilter to select people that are represented in the entries returned by a specified Step Data View.
    /// </summary>
    [Description( "Select people that are represented in the Streak returned by a specified Data View" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person In Streak Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "A1AE68AE-4E6D-4C09-94BD-7449DEA3F221" )]
    public class StreakDataViewFilter : RelatedDataViewFilterBase<Rock.Model.Person, Rock.Model.Streak>
    {
        #region Overrides
    
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
            var settings = new FilterSettings( selection );

            var context = ( RockContext ) serviceInstance.Context;

            // Get the Data View that defines the set of related records from which entities can be selected.
            var dataView = DataComponentSettingsHelper.GetDataViewForFilterComponent( settings.DataViewGuid, context );

            // Evaluate the Data View that defines the related records.
            var relatedEntityService = new StreakService( context );

            var relatedEntityQuery = relatedEntityService.Queryable();

            if ( dataView != null )
            {
                relatedEntityQuery = DataComponentSettingsHelper.FilterByDataView( relatedEntityQuery, dataView, relatedEntityService );
            }

            // Get all of the People corresponding to the qualifying related records.
            var personService = new PersonService( ( RockContext ) serviceInstance.Context );

            var qry = personService.Queryable()
                        .Where( p => relatedEntityQuery.Any( xx => xx.PersonAlias.PersonId == p.Id ) );

            // Retrieve the Filter Expression.
            var extractedFilterExpression = FilterExpressionExtractor.Extract<Model.Person>( qry, parameterExpression, "p" );

            return extractedFilterExpression;
        }

        #endregion
    }
}