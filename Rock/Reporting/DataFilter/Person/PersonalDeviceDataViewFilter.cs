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
    /// Select people that are represented in the Personal Device entries returned by a specified Data View
    /// </summary>
    [Description( "Select people that are represented in the Personal Device entries returned by a specified Data View" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person In Personal Device Filter" )]
    public class PersonalDeviceDataViewFilter : RelatedDataViewFilterBase<Rock.Model.Person, Rock.Model.PersonalDevice>
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
            var personalDeviceService = new PersonalDeviceService( context );

            var personalDeviceQuery = personalDeviceService.Queryable();

            if ( dataView != null )
            {
                personalDeviceQuery = DataComponentSettingsHelper.FilterByDataView( personalDeviceQuery, dataView, personalDeviceService );
            }

            // Get all of the People corresponding to the Personal Devices records.
            var qry = new PersonService( ( RockContext ) serviceInstance.Context ).Queryable()
                        .Where( p => personalDeviceQuery.Any( xx => xx.PersonAlias.PersonId == p.Id ) );
            // Retrieve the Filter Expression.
            var extractedFilterExpression = FilterExpressionExtractor.Extract<Model.Person>( qry, parameterExpression, "p" );

            return extractedFilterExpression;
        }

        #endregion
    }
}
