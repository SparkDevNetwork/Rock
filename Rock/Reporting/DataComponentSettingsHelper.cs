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
using System.Collections.Generic;
using System.Linq;

using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Reporting
{
    /// <summary>
    ///     A collection of helper methods for working with settings for Data Components.
    /// </summary>
    public static class DataComponentSettingsHelper
    {
        /// <summary>
        ///     Retrieves and validates a DataView for use in a Filter of another Data View.
        /// </summary>
        /// <param name="dataViewGuid"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static DataView GetDataViewForFilterComponent( Guid? dataViewGuid, RockContext context = null )
        {
            if ( !dataViewGuid.HasValue )
            {
                return null;
            }

            context = context ?? RockApp.Current.CreateRockContext();

            var dsService = new DataViewService( context );

            DataView dataView = dsService.Get( dataViewGuid.Value );

            if ( dataView == null
                || dataView.DataViewFilter == null )
            {
                return null;
            }

            // Verify that the Data View does not contain any references to itself in any of its components.
            // This configuration would cause a circular reference exception during evaluation of the Data View.
            if ( dsService.IsViewInFilter( dataView.Id, dataView.DataViewFilter ) )
            {
                throw new Exception( "Filter issue(s): One of the filters contains a circular reference to the Data View itself." );
            }

            return dataView;
        }

        /// <summary>
        /// Determines whether the person may use every data view referenced by a posted data view filter.
        /// A reference is allowed when the stored filter with the same Guid and entity type already held the
        /// same data view. Any other reference is new or changed, so the person must be able to VIEW that data view.
        /// </summary>
        /// <param name="newFilter">The filter built from the posted controls.</param>
        /// <param name="storedFilter">The filter that is currently stored for the data view, or <c>null</c> when there is none.</param>
        /// <param name="filteredEntityType">The type of the entity the data view filters.</param>
        /// <param name="person">The person who is saving or previewing the data view.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if every new or changed data view reference is allowed; otherwise <c>false</c>.</returns>
        [Rock.Attribute.RockInternal( "17.13" )]
        public static bool IsAuthorizedForNewDataViewReferences( DataViewFilter newFilter, DataViewFilter storedFilter, Type filteredEntityType, Person person, RockContext rockContext )
        {
            /*
                11/09/2026 - MSE

                The data view a filter references comes from a posted value, and the evaluation of the filter
                does not check security on it. A new or changed reference must therefore be one the person
                can VIEW. A reference the stored filter with the same Guid already holds is kept so data views
                built by someone else can still be saved.

                Reason: Stops a saved or previewed filter from using a data view the person cannot VIEW.
            */

            rockContext = rockContext ?? new RockContext();

            var storedFiltersByGuid = new Dictionary<Guid, DataViewFilter>();
            foreach ( var filter in FlattenFilters( storedFilter ) )
            {
                if ( !storedFiltersByGuid.ContainsKey( filter.Guid ) )
                {
                    storedFiltersByGuid.Add( filter.Guid, filter );
                }
            }

            foreach ( var filter in FlattenFilters( newFilter ) )
            {
                var referencedDataViewId = GetReferencedDataViewId( filter, filteredEntityType, rockContext );

                if ( !referencedDataViewId.HasValue )
                {
                    continue;
                }

                if ( storedFiltersByGuid.TryGetValue( filter.Guid, out var stored )
                    && stored.EntityTypeId == filter.EntityTypeId
                    && ( stored.Selection == filter.Selection || GetReferencedDataViewId( stored, filteredEntityType, rockContext ) == referencedDataViewId ) )
                {
                    continue;
                }

                var dataView = new DataViewService( rockContext ).Get( referencedDataViewId.Value );
                if ( dataView == null || !dataView.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the data view id that a filter references. The recorded id is used first. Filters
        /// that do not record it are read from their selection.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="filteredEntityType">The type of the entity the data view filters.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The referenced data view id, or <c>null</c>.</returns>
        private static int? GetReferencedDataViewId( DataViewFilter filter, Type filteredEntityType, RockContext rockContext )
        {
            if ( filter.RelatedDataViewId.HasValue )
            {
                return filter.RelatedDataViewId;
            }

            if ( filter.ExpressionType != FilterExpressionType.Filter || !filter.EntityTypeId.HasValue )
            {
                return null;
            }

            var componentName = EntityTypeCache.Get( filter.EntityTypeId.Value, rockContext )?.Name;

            if ( filteredEntityType != null )
            {
                var componentDataViewId = DataFilterContainer.GetComponent( componentName )?.GetRelatedDataViewId( filteredEntityType, filter.Selection, rockContext );

                if ( componentDataViewId.HasValue )
                {
                    return componentDataViewId;
                }
            }

            if ( componentName == typeof( Rock.Reporting.DataFilter.Group.ContainsPeopleFilter ).FullName )
            {
                return Rock.Reporting.DataFilter.Group.ContainsPeopleFilter.GetReferencedDataViewId( filter.Selection );
            }

            if ( componentName == typeof( Rock.Reporting.DataFilter.Group.ContainsGroupMembersFilter ).FullName )
            {
                return Rock.Reporting.DataFilter.Group.ContainsGroupMembersFilter.GetReferencedDataViewId( filter.Selection );
            }

            return null;
        }

        /// <summary>
        /// Gets the filter and all of its child filters as a flat list.
        /// </summary>
        /// <param name="filter">The filter, or <c>null</c>.</param>
        /// <returns>A list of filters, empty when the filter is <c>null</c>.</returns>
        private static List<DataViewFilter> FlattenFilters( DataViewFilter filter )
        {
            var filters = new List<DataViewFilter>();

            if ( filter != null )
            {
                filters.Add( filter );

                foreach ( var childFilter in filter.ChildFilters )
                {
                    filters.AddRange( FlattenFilters( childFilter ) );
                }
            }

            return filters;
        }

        /// <summary>
        ///     Adds a Data View to a Queryable Expression as a new filter expression.
        /// </summary>
        /// <typeparam name="T">The Entity Type returned by the Queryable Expression.</typeparam>
        /// <param name="query">The query to which the filter will be added.</param>
        /// <param name="dataView">The DataView from which the filter will be created.</param>
        /// <param name="serviceInstance">The Data Service that is used to be build the Query Expression.</param>
        /// <returns></returns>
        public static IQueryable<T> FilterByDataView<T>( IQueryable<T> query, DataView dataView, IService serviceInstance )
        {
            var paramExpression = serviceInstance.ParameterExpression;

            var whereExpression = dataView.GetExpression( serviceInstance, paramExpression );

            return query.Where( paramExpression, whereExpression );
        }

        /// <summary>
        ///     Returns the Guid of the specified DataView.
        /// </summary>
        /// <param name="dataViewId"></param>
        /// <returns>The Guid of the specified DataView, or null if the Data View does not exist.</returns>
        public static Guid? GetDataViewGuid( string dataViewId )
        {
            var id = dataViewId.AsIntegerOrNull();

            if ( id == null || id == 0 )
            {
                return null;
            }

            var dsService = new DataViewService( RockApp.Current.CreateRockContext() );

            return dsService.GetGuid( id.Value );
        }

        /// <summary>
        ///     Returns the Id of the specified DataView.
        /// </summary>
        /// <param name="dataViewGuid">The Guid of an existing Data View.</param>
        /// <returns>The Id of the specified DataView, or null if the Data View does not exist.</returns>
        public static int? GetDataViewId( string dataViewGuid )
        {
            return GetDataViewId( dataViewGuid.AsGuidOrNull() );
        }

        /// <summary>
        ///     Returns the Id of the specified DataView.
        /// </summary>
        /// <param name="dataViewGuid">The Guid of an existing Data View.</param>
        /// <returns>The Id of the specified DataView, or null if the Data View does not exist.</returns>
        public static int? GetDataViewId( Guid? dataViewGuid )
        {
            if ( !dataViewGuid.HasValue )
            {
                return null;
            }

            var dsService = new DataViewService( RockApp.Current.CreateRockContext() );

            return dsService.GetId( dataViewGuid.Value );
        }

        /// <summary>
        ///     Returns the value of a parameter by index from a collection of settings, or a default value if the setting does not
        ///     exist.
        /// </summary>
        /// <param name="parameters">A collection of settings strings.</param>
        /// <param name="parameterIndex">The collection index of the parameter to return.</param>
        /// <param name="defaultValue">A default value to return if the parameter is blank or does not exist.</param>
        /// <returns>A string representing a parameter value.</returns>
        public static string GetParameterOrDefault( IEnumerable<string> parameters, int parameterIndex, string defaultValue = null )
        {
            var value = parameters.ElementAtOrDefault( parameterIndex ).ToStringSafe();

            return string.IsNullOrWhiteSpace( value ) ? defaultValue : value;
        }

        /// <summary>
        ///     Returns the value of a parameter by index from a collection of settings, or an empty string if the setting does not
        ///     exist.
        /// </summary>
        /// <param name="parameters">A collection of settings strings.</param>
        /// <param name="parameterIndex">The collection index of the parameter to return.</param>
        /// <returns>A string representing a parameter value.</returns>
        public static string GetParameterOrEmpty( IEnumerable<string> parameters, int parameterIndex )
        {
            return GetParameterOrDefault( parameters, parameterIndex, string.Empty );
        }

        /// <summary>
        ///     Returns the value of a parameter by index from a collection of settings, parsed to the specified enumerated type.
        ///     If the setting cannot be parsed, the default value is returned.
        /// </summary>
        /// <param name="parameters">A collection of settings strings.</param>
        /// <param name="parameterIndex">The collection index of the parameter to return.</param>
        /// <param name="defaultValue">A default value to return if the parameter is invalid or does not exist.</param>
        /// <returns>An enumerated type value corresponding to the specified parameter value.</returns>
        public static TEnum GetParameterAsEnum<TEnum>( IEnumerable<string> parameters, int parameterIndex, TEnum defaultValue )
            where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            var value = GetParameterOrDefault( parameters, parameterIndex );

            return value.ConvertToEnum<TEnum>( defaultValue );
        }

        /// <summary>
        ///     Returns the value of a parameter by index from a collection of settings, parsed to the specified enumerated type.
        ///     If the setting cannot be parsed, a null value is returned.
        /// </summary>
        /// <param name="parameters">A collection of settings strings.</param>
        /// <param name="parameterIndex">The collection index of the parameter to return.</param>
        /// <returns>An enumerated type value corresponding to the specified parameter value, or null.</returns>
        public static TEnum? GetParameterAsEnum<TEnum>( IEnumerable<string> parameters, int parameterIndex )
            where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            var value = GetParameterOrDefault( parameters, parameterIndex );

            return value.ConvertToEnumOrNull<TEnum>();
        }

        /// <summary>
        /// Gets the parameter as list.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="parameterIndex">Index of the parameter.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <returns></returns>
        public static List<string> GetParameterAsList( IEnumerable<string> parameters, int parameterIndex, string delimiter = "," )
        {
            var value = GetParameterOrDefault( parameters, parameterIndex, string.Empty );

            return value.Split( new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries ).ToList();
        }

        /// <summary>
        /// Create a filter settings object from a Json string.
        /// </summary>
        /// <typeparam name="TSettings"></typeparam>
        /// <param name="selection"></param>
        /// <param name="defaultSettings"></param>
        /// <returns></returns>
        public static TSettings DeserializeFilterSettings<TSettings> ( string selection, TSettings defaultSettings = null )
            where TSettings : class
        {
            var settings = selection.FromJsonOrNull<TSettings>() ?? defaultSettings;
            return settings; 
        }

        /// <summary>
        /// Gets a Json string representation of a filter settings object.
        /// </summary>
        /// <typeparam name="TSettings"></typeparam>
        /// <param name="selection"></param>
        /// <returns></returns>
        public static string SerializeFilterSettings<TSettings>( TSettings selection )
            where TSettings : class
        {
            if ( selection == null )
            {
                return string.Empty;
            }

            var json = selection.ToJson();
            return json;
        }
    }
}