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
    /// A DataFilter that selects groups associated with a set of locations identified by a Location Data View.
    /// </summary>
    [Description( "Filter groups by address using a set of locations identified by a Location Data View" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Location Data View Filter" )]
    public class LocationDataViewFilter : RelatedDataViewFilterBase<Rock.Model.Group, Rock.Model.Location>
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

            // Get the Location Data View that defines the set of candidates from which proximate Locations can be selected.
            var dataView = DataComponentSettingsHelper.GetDataViewForFilterComponent( settings.DataViewGuid, context );

            // Evaluate the Data View that defines the candidate Locations.
            var locationService = new LocationService( context );

            var locationQuery = locationService.Queryable();

            if ( dataView != null )
            {
                locationQuery = DataComponentSettingsHelper.FilterByDataView( locationQuery, dataView, locationService );
            }

            // Get all the Groups that have a Location matching one of the candidate Locations.
            var groupLocationService = new GroupLocationService( context );
            var groupService = new GroupService( context );

            var groupLocationsQuery = groupLocationService.Queryable().Where( gl => locationQuery.Any( l => l.Id == gl.LocationId ) );

            var groupQuery = groupService.Queryable().Where( g => groupLocationsQuery.Any( gl => gl.GroupId == g.Id ) );

            // Retrieve the filter expression.
            var extractedFilterExpression = FilterExpressionExtractor.Extract<Model.Group>( groupQuery, parameterExpression, "g" );

            return extractedFilterExpression;
        }

        #endregion
    }
}