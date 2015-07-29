// <copyright>
// Copyright 2013 by the Spark Development Network
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Rock.Data;
using Rock.Model;

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
            if (!dataViewGuid.HasValue)
            {
                return null;
            }

            context = context ?? new RockContext();

            var dsService = new DataViewService( context );

            DataView dataView = dsService.Get( dataViewGuid.Value );

            if (dataView == null
                || dataView.DataViewFilter == null)
            {
                return null;
            }

            // Verify that the Data View does not contain any references to itself in any of its components.
            // This configuration would cause a circular reference excpetion during evaluation of the Data View.
            if (dsService.IsViewInFilter( dataView.Id, dataView.DataViewFilter ))
            {
                throw new Exception( "Filter issue(s): One of the filters contains a circular reference to the Data View itself." );
            }

            return dataView;
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

            List<string> errorMessages;

            var whereExpression = dataView.GetExpression( serviceInstance, paramExpression, out errorMessages );

            if (errorMessages.Any())
            {
                throw new Exception( "Filter issue(s): " + errorMessages.AsDelimited( "; " ) );
            }

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

            if (id == null)
            {
                return null;
            }

            var dsService = new DataViewService( new RockContext() );

            var dataView = dsService.Get( id.Value );

            return dataView.Guid;
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
            if (!dataViewGuid.HasValue)
            {
                return null;
            }

            var dsService = new DataViewService( new RockContext() );

            var dataView = dsService.Get( dataViewGuid.Value );

            return ( dataView != null ) ? (int?)dataView.Id : null;
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
    }
}