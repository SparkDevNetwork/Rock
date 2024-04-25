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
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI.WebControls;

using EF6.TagWith;

using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting
{
    /// <summary>
    /// Provides methods to build queries from Data Views.
    /// This component implements caching of DataView and DataViewFilter objects to reduce the overhead of retrieving
    /// their definitions from the data store when building often-used queries.
    /// </summary>
    /// <remarks>
    /// This should not be used directly except by DataView, DataViewCache and
    /// unit testing code.
    /// </remarks>
    internal class DataViewQueryBuilder
    {
        #region Instancing

        private static readonly Lazy<DataViewQueryBuilder> _dataManager = new Lazy<DataViewQueryBuilder>();

        /// <summary>
        /// The global singleton instance.
        /// </summary>
        public static DataViewQueryBuilder Instance => _dataManager.Value;

        #endregion

        #region Data Views

        /// <summary>
        /// Get the definition for the specified Data View.
        /// </summary>
        /// <param name="dataViewId"></param>
        /// <returns></returns>
        public IDataViewDefinition GetDataViewDefinition( int dataViewId )
        {
            return DataViewCache.Get( dataViewId );
        }

        /// <summary>
        /// Get the definition for the specified Data View.
        /// </summary>
        /// <param name="dataViewGuid"></param>
        /// <returns></returns>
        public IDataViewDefinition GetDataViewDefinition( Guid dataViewGuid )
        {
            return DataViewCache.Get( dataViewGuid );
        }

        /// <summary>
        /// Gets the query for the specified Data View.
        /// </summary>
        /// <param name="dataViewId"></param>
        /// <returns></returns>
        public IQueryable<IEntity> GetDataViewQuery( int dataViewId )
        {
            var dataView = DataViewCache.Get( dataViewId ) as IDataViewDefinition;
            var query = GetDataViewQuery( dataView, null );
            return query;
        }

        /// <summary>
        /// Gets the query for the specified Data View Filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataViewFilterId"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public IQueryable<T> GetDataViewFilterQuery<T>( int dataViewFilterId, GetQueryableOptions args )
            where T : class, IEntity
        {
            var filter = DataViewFilterCache.Get( dataViewFilterId );

            var query = GetDataViewFilterQuery( filter, typeof( T ), args );
            return query as IQueryable<T>;
        }

        /// <summary>
        /// Gets the query for the specified Data View Filter.
        /// </summary>
        /// <param name="dataViewFilter"></param>
        /// <param name="queryEntityType"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public IQueryable<IEntity> GetDataViewFilterQuery( IDataViewFilterDefinition dataViewFilter, Type queryEntityType, GetQueryableOptions args = null )
        {
            args = args ?? new GetQueryableOptions();

            var rockContext = new RockContext();

            var service = Reflection.GetServiceForEntityType( queryEntityType, rockContext );
            var paramExpression = service.ParameterExpression;

            Expression whereExpression = null;
            if ( dataViewFilter != null )
            {
                whereExpression = this.GetDataFilterExpression( dataViewFilter,
                    queryEntityType,
                    service,
                    paramExpression,
                    args.DataViewFilterOverrides );
            }

            // Get the specific, underlying IEntity System.Type implementation - for example, Rock.Model.Group
            // The Where expression requires a strongly-typed input parameter to allow filtering on Entity-specific properties.
            var getMethod = service.GetType().GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ), typeof( SortProperty ) } );
            if ( getMethod == null )
            {
                throw new RockDataViewFilterExpressionException( dataViewFilter, $"Unable to determine IService.Get()" );
            }

            var query = getMethod.Invoke( service, new object[] { paramExpression, whereExpression, null } ) as IQueryable<IEntity>;

            return query;
        }

        /// <summary>
        /// Gets the query for the specified Data View.
        /// </summary>
        /// <param name="dataView"></param>
        /// <param name="dataViewGetQueryArgs">The arguments.</param>
        /// <returns></returns>
        /// <exception cref="Rock.Reporting.RockReportingException">
        /// Unable to determine DbContext for {this}
        /// or
        /// Unable to determine ServiceInstance for {this}
        /// or
        /// Unable to determine IService.Get for {this}
        /// </exception>
        public IQueryable<IEntity> GetDataViewQuery( IDataViewDefinition dataView, GetQueryableOptions dataViewGetQueryArgs = null )
        {
            dataViewGetQueryArgs = dataViewGetQueryArgs ?? new GetQueryableOptions();

            var dbContext = dataViewGetQueryArgs.DbContext;
            if ( dbContext == null )
            {
                dbContext = GetDbContextForDataView( dataView );
            }

            var entityTypeId = dataView.EntityTypeId ?? 0;

            var serviceInstance = GetServiceForEntityType( entityTypeId, dbContext );
            if ( serviceInstance == null )
            {
                var entityTypeCache = EntityTypeCache.Get( dataView.EntityTypeId ?? 0 );
                throw new RockDataViewFilterExpressionException( dataView.DataViewFilter, $"Unable to determine ServiceInstance from DataView EntityType {entityTypeCache} for {this}" );
            }

            var databaseTimeoutSeconds = dataViewGetQueryArgs.DatabaseTimeoutSeconds;
            if ( databaseTimeoutSeconds.HasValue )
            {
                dbContext.Database.CommandTimeout = databaseTimeoutSeconds.Value;
            }

            var dataViewFilterOverrides = dataViewGetQueryArgs.DataViewFilterOverrides;
            var paramExpression = serviceInstance.ParameterExpression;
            var whereExpression = GetDataViewExpression( dataView, serviceInstance, paramExpression, dataViewFilterOverrides );

            var sortProperty = dataViewGetQueryArgs.SortProperty;

            if ( sortProperty == null )
            {
                // if no sorting is specified, just sort by Id
                sortProperty = new SortProperty { Direction = SortDirection.Ascending, Property = "Id" };
            }

            Type returnType = null;
            IQueryable<IEntity> dataViewQuery;
            var personEntityTypeId = EntityTypeCache.GetId<Rock.Model.Person>();
            if ( dataView.EntityTypeId.HasValue && dataView.EntityTypeId.Value == personEntityTypeId && serviceInstance is PersonService personService )
            {
                /* 05/25/2022 MDP
                We have a option in DataViews that are based on Person on whether Deceased individuals should be included. That requires the
                PersonService.Querable( bool includeDeceased ) method, so we'll use that.
                */

                returnType = typeof( Person );
                dataViewQuery = personService.Queryable( dataView.IncludeDeceased ).Where( paramExpression, whereExpression, sortProperty );
            }
            else
            {
                var getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ), typeof( SortProperty ) } );
                if ( getMethod == null )
                {
                    throw new RockDataViewFilterExpressionException( dataView.DataViewFilter, $"Unable to determine IService.Get for Report: {this}" );
                }

                // Get the specific, underlying IEntity type implementation (i.e. Group).
                var genericArgs = getMethod.ReturnType.GetGenericArguments();
                if ( genericArgs.Length > 0 )
                {
                    returnType = genericArgs.First();
                }

                dataViewQuery = getMethod.Invoke( serviceInstance, new object[] { paramExpression, whereExpression, sortProperty } ) as IQueryable<IEntity>;
            }

            // Add a comment to the query with the data view id for debugging.
            /*
                6/21/2023 - JPH
                When calling the TagWith() method without explicitly specifying the underlying IEntity implementation type,
                it will add a ParameterExpression of type IEntity to the query, which prevents the successful casting of the
                returned IQueryable<IEntity> to a specific IEntity implementation (i.e. IQueryable<Group>). Many callers of
                this GetQuery() method have a tendency to do exactly that: cast the resulting IQueryable to a specific type.
                Rather than change all of those callers' handling of this return type, we'll use reflection below to ensure
                we're passing the correct underlying type to the TagWith() method, so it doesn't handcuff our usage of the
                returned IQueryable.
                Reason: TagWith() in certain situations is causing query to be null
                (https://app.asana.com/0/474497188512037/1204855716691596/f)
             */
            if ( returnType != null )
            {
                var tagWithMethod = typeof( TagWithExtensions ).GetMethod( "TagWith" );
                if ( tagWithMethod != null )
                {
                    tagWithMethod = tagWithMethod.MakeGenericMethod( returnType );
                    dataViewQuery = tagWithMethod.Invoke( null, new object[]
                    {
                        dataViewQuery,
                        $"Data View Id: {dataView.Id}" // Here is where we're specifying the "tag" to be added.
                    } ) as IQueryable<IEntity>;
                }
            }

            return dataViewQuery;
        }

        /// <summary>
        /// Returns a flag indicating if the specified user has permission to view the results of a Data View.
        /// </summary>
        /// <param name="dataView"></param>
        /// <param name="CurrentPerson"></param>
        /// <remarks>To be removed if we decide we don't need this.</remarks>
        /// <returns></returns>
        private bool IsUserAuthorizedToViewResults( IDataViewDefinition dataView, Person CurrentPerson )
        {
            if ( dataView == null )
            {
                return false;
            }

            bool isAuthorized;
            if ( dataView is ISecured securedDataView )
            {
                isAuthorized = Authorization.Authorized( securedDataView, Authorization.VIEW, CurrentPerson );
            }
            else
            {
                // This Data View implementation does not support ISecured, so we need to check security on the associated persisted Data View instead.
                if ( dataView.Id == 0 )
                {
                    // This implementation does not support ISecured and is not associated with a persisted Data View, so we can assume it is unsecured.
                    isAuthorized = true;
                }
                else
                {
                    // Check security on the persisted Data View associated with this implementation.
                    var rockContext = new RockContext();
                    var dataViewService = new DataViewService( rockContext );
                    var dataViewEntity = dataViewService.Get( dataView.Id );

                    isAuthorized = Authorization.Authorized( dataViewEntity, Authorization.VIEW, CurrentPerson );
                }
            }

            return isAuthorized;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="dataView"></param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="paramExpression">The parameter expression.</param>
        /// <param name="dataViewFilterOverrides">The data view filter overrides.</param>
        /// <returns></returns>
        /// <exception cref="Rock.Reporting.RockReportingException">
        /// Unable to determine Assembly for EntityTypeId { EntityTypeId }
        /// or
        /// Unable to determine DataView EntityType for { dataViewEntityTypeCache }.
        /// or
        /// Unable to determine transform expression for TransformEntityTypeId: {TransformEntityTypeId}
        /// </exception>
        public Expression GetDataViewExpression( IDataViewDefinition dataView, IService serviceInstance, ParameterExpression paramExpression, DataViewFilterOverrides dataViewFilterOverrides )
        {
            var dataViewEntityTypeCache = EntityTypeCache.Get( dataView.EntityTypeId.Value );

            var dataViewFilter = dataView.DataViewFilter;

            if ( dataViewEntityTypeCache?.AssemblyName == null )
            {
                throw new RockDataViewFilterExpressionException( dataViewFilter, $"Unable to determine DataView Assembly for EntityTypeId {dataView.EntityTypeId}" );
            }

            var resultEntityType = dataViewEntityTypeCache.GetEntityType();
            if ( resultEntityType == null )
            {
                throw new RockDataViewFilterExpressionException( dataViewFilter, $"Unable to determine DataView EntityType for {dataViewEntityTypeCache}." );
            }

            // DataViews must have a DataViewFilter (something has gone wrong it doesn't have one)
            // Note that DataViewFilterId might be null even though DataViewFilter is not null
            // This is because the DataViewFilter might be just in memory and not saved to the database (for example, a Preview or a DynamicReport)
            if ( dataView.DataViewFilter == null )
            {
                throw new RockDataViewFilterExpressionException( dataViewFilter, $"DataViewFilter is null for DataView {dataView.Name} ({dataView.Id})." );
            }

            var usePersistedValues = ( dataView.PersistedScheduleIntervalMinutes.HasValue || dataView.PersistedScheduleId.HasValue )
                && dataView.PersistedLastRefreshDateTime.HasValue;
            if ( dataViewFilterOverrides != null )
            {
                // don't use persisted values if this DataView in the list of DataViews that should not be persisted due to override
                usePersistedValues = usePersistedValues && !dataViewFilterOverrides.IgnoreDataViewPersistedValues.Contains( dataView.Id );
            }

            if ( usePersistedValues )
            {
                // If this is a persisted DataView, get the ids for the expression by querying DataViewPersistedValue instead of evaluating all the filters
                var rockContext = serviceInstance.Context ?? new RockContext();

                var persistedValuesQuery = rockContext.Set<DataViewPersistedValue>().Where( a => a.DataViewId == dataView.Id );
                var ids = persistedValuesQuery.Select( v => v.EntityId );
                var propertyExpression = Expression.Property( paramExpression, "Id" );
                var idsExpression = Expression.Constant( ids, typeof( IQueryable<int> ) );

                Expression expression = Expression.Call( typeof( Queryable ), "Contains", new Type[] { typeof( int ) }, idsExpression, propertyExpression );

                return expression;
            }
            else
            {
                // It was decided on 2024-02-26 during a review with PO that we
                // only update statistics if the data view actually executes.
                // That means we don't update statistics if we are using the
                // persisted values.

                // If dataViewFilterOverrides is null assume true in order to preserve current functionality.
                if ( dataViewFilterOverrides == null || dataViewFilterOverrides.ShouldUpdateStatics )
                {
                    DataViewService.AddRunDataViewTransaction( dataView.Id );
                }

                var filterExpression = dataViewFilter != null
                    ? GetDataFilterExpression( dataViewFilter, resultEntityType, serviceInstance, paramExpression, dataViewFilterOverrides )
                    : null;

                if ( dataView.TransformEntityTypeId.HasValue )
                {
                    Expression transformedExpression = GetTransformExpression( dataView, dataView.TransformEntityTypeId.Value, serviceInstance, paramExpression, filterExpression );
                    if ( transformedExpression == null )
                    {
                        // if TransformEntityTypeId is defined, but we got null back, we'll get unexpected results, so throw an exception
                        throw new RockDataViewFilterExpressionException( dataViewFilter, $"Unable to determine transform expression for TransformEntityTypeId: {dataView.TransformEntityTypeId}" );
                    }

                    return transformedExpression;
                }

                return filterExpression;
            }
        }

        /// <summary>
        /// Gets the transform expression.
        /// </summary>
        /// <param name="dataView"></param>
        /// <param name="transformEntityTypeId">The transform entity type identifier.</param>
        /// <param name="service">The service.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <returns></returns>
        private Expression GetTransformExpression( IDataViewDefinition dataView, int transformEntityTypeId, IService service, ParameterExpression parameterExpression, Expression whereExpression )
        {
            var entityType = EntityTypeCache.Get( transformEntityTypeId );

            if ( entityType == null )
            {
                // if we can't determine EntityType, throw an exception so we don't return incorrect results
                throw new RockDataViewFilterExpressionException( dataView.DataViewFilter, $"Unable to determine TransformEntityType {entityType.Name}" );
            }

            var component = Rock.Reporting.DataTransformContainer.GetComponent( entityType.Name );
            if ( component == null )
            {
                // if we can't determine component, throw an exception so we don't return incorrect results
                throw new RockDataViewFilterExpressionException( dataView.DataViewFilter, $"Unable to determine transform component for {entityType.Name}" );
            }

            return component.GetExpression( service, parameterExpression, whereExpression );
        }

        #endregion

        #region Data View Filters

        /// <summary>
        /// Get the definition for the specified Data View Filter.
        /// </summary>
        /// <param name="dataViewFilterId"></param>
        /// <returns></returns>
        public IDataViewFilterDefinition GetDataViewFilterDefinition( int dataViewFilterId )
        {
            return DataViewFilterCache.Get( dataViewFilterId );
        }

        /// <summary>
        /// Get the definition for the specified Data View Filter.
        /// </summary>
        /// <param name="dataViewFilterGuid"></param>
        /// <returns></returns>
        public IDataViewFilterDefinition GetDataViewFilterDefinition( Guid dataViewFilterGuid )
        {
            return DataViewFilterCache.Get( dataViewFilterGuid );
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="dataViewFilterId"></param>
        /// <param name="resultEntityType">The type of Entity returned in the result set of the filter.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public Expression GetDataViewFilterExpression( int dataViewFilterId, Type resultEntityType, IService serviceInstance, ParameterExpression parameter )
        {
            return GetDataViewFilterExpression( dataViewFilterId, resultEntityType, serviceInstance, parameter, new DataViewFilterOverrides() );
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="dataViewFilterId"></param>
        /// <param name="resultEntityType">The type of Entity returned in the result set of the filter.</param>
        /// <param name="serviceInstance">A service instance for the resultEntityType.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="dataViewFilterOverrides">The data view filter overrides.</param>
        /// <returns></returns>
        /// <exception cref="Rock.Reporting.RockReportingException">
        /// EntityTypeId not defined for {this}
        /// or
        /// Unable to determine EntityType not defined for EntityTypeId {EntityTypeId}
        /// or
        /// Unable to determine Component for EntityType {entityType.Name}
        /// or
        /// unable to determine expression for {filter}
        /// or
        /// Unexpected FilterExpressionType {ExpressionType}
        /// </exception>
        public Expression GetDataViewFilterExpression( int dataViewFilterId, Type resultEntityType, IService serviceInstance, ParameterExpression parameter, DataViewFilterOverrides dataViewFilterOverrides )
        {
            var dataViewFilter = DataViewFilterCache.Get( dataViewFilterId ) as IDataViewFilterDefinition;

            return GetDataFilterExpression( dataViewFilter, resultEntityType, serviceInstance, parameter, dataViewFilterOverrides );
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="dataViewFilter"></param>
        /// <param name="resultEntityType">The type of Entity returned in the result set of the filter.</param>
        /// <param name="serviceInstance">A service instance for the resultEntityType.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="dataViewFilterOverrides">The data view filter overrides.</param>
        /// <returns></returns>
        /// <exception cref="Rock.Reporting.RockReportingException">
        /// EntityTypeId not defined for {this}
        /// or
        /// Unable to determine EntityType not defined for EntityTypeId {EntityTypeId}
        /// or
        /// Unable to determine Component for EntityType {entityType.Name}
        /// or
        /// unable to determine expression for {filter}
        /// or
        /// Unexpected FilterExpressionType {ExpressionType}
        /// </exception>

        private Expression GetDataFilterExpression( IDataViewFilterDefinition dataViewFilter, Type resultEntityType, IService serviceInstance, ParameterExpression parameter, DataViewFilterOverrides dataViewFilterOverrides )
        {
            switch ( dataViewFilter.ExpressionType )
            {
                case FilterExpressionType.Filter:
                    // Get the Entity Type of the component that is used to construct the filter.
                    var filterComponentEntityType = EntityTypeCache.Get( dataViewFilter.EntityTypeId.Value );
                    if ( filterComponentEntityType == null )
                    {
                        // if this happens, we want to throw an exception to prevent incorrect results
                        throw new RockDataViewFilterExpressionException( dataViewFilter, $"Unable to determine EntityType for Data View Filter {dataViewFilter.Id}" );
                    }

                    var component = Rock.Reporting.DataFilterContainer.GetComponent( filterComponentEntityType.Name );
                    if ( component == null )
                    {
                        // if this happens, we want to throw an exception to prevent incorrect results
                        throw new RockDataViewFilterExpressionException( dataViewFilter, $"Unable to determine Component for EntityType {filterComponentEntityType.Name}" );
                    }

                    string selection; // A formatted string representing the filter settings: FieldName, <see cref="ComparisonType">Comparison Type</see>, (optional) Comparison Value(s)
                    var dataViewFilterOverride = dataViewFilterOverrides?.GetOverride( dataViewFilter.Guid ?? Guid.NewGuid() );
                    if ( dataViewFilterOverride != null )
                    {
                        if ( dataViewFilterOverride.IncludeFilter == false )
                        {
                            /*
                            1/15/2021 - Shaun
                            This should not assume that returning Expression.Constant( true ) is equivalent to not filtering as this predicate
                            may be joined to other predicates and the AND/OR logic may result in an inappropriate filter.  Instead, we simply
                            return null and allow the caller to handle this in a manner appropriate to the given filter.
                            */

                            // If the dataview filter should not be included, don't have this filter filter anything. 
                            return null;
                        }
                        else
                        {
                            selection = dataViewFilterOverride.Selection;
                        }
                    }
                    else
                    {
                        selection = dataViewFilter.Selection;
                    }

                    Expression expression;

                    try
                    {
                        if ( component is IDataFilterWithOverrides )
                        {
                            expression = ( component as IDataFilterWithOverrides ).GetExpressionWithOverrides( resultEntityType, serviceInstance, parameter, dataViewFilterOverrides, selection );
                        }
                        else
                        {
                            expression = component.GetExpression( resultEntityType, serviceInstance, parameter, selection );
                        }
                    }
                    catch ( RockDataViewFilterExpressionException dex )
                    {
                        // components don't know which DataView/DataFilter they are working with, so if there was a RockDataViewFilterExpressionException, let's tell it what DataViewFilter/DataView it was using
                        dex.SetDataFilterIfNotSet( dataViewFilter );
                        throw;
                    }

                    if ( expression == null )
                    {
                        // If a DataFilter component returned a null expression, that probably means that it decided not to filter anything. So, we'll interpret that as "Don't Filter"
                        expression = Expression.Constant( true );
                    }

                    return expression;

                case FilterExpressionType.GroupAll:
                case FilterExpressionType.GroupAnyFalse:

                    Expression andExp = null;
                    foreach ( var filter in dataViewFilter.ChildFilters )
                    {
                        var exp = GetDataFilterExpression( filter, resultEntityType, serviceInstance, parameter, dataViewFilterOverrides );
                        if ( exp == null )
                        {
                            // If a DataFilter component returned a null expression, that probably means that it decided not to filter anything. So, we'll interpret that as "Don't Filter"
                            exp = Expression.Constant( true );
                        }

                        if ( andExp == null )
                        {
                            andExp = exp;
                        }
                        else
                        {
                            andExp = Expression.AndAlso( andExp, exp );
                        }
                    }

                    if ( dataViewFilter.ExpressionType == FilterExpressionType.GroupAnyFalse
                         && andExp != null )
                    {
                        // If only one of the conditions must be false, invert the expression so that it becomes the logical equivalent of "NOT ALL".
                        andExp = Expression.Not( andExp );
                    }

                    if ( andExp == null )
                    {
                        // If there aren't any child filters for a GroupAll/GroupAnyFalse. That is OK, so just don't filter anything.
                        return Expression.Constant( true );
                    }

                    return andExp;

                case FilterExpressionType.GroupAny:
                case FilterExpressionType.GroupAllFalse:

                    Expression orExp = null;
                    foreach ( var filter in dataViewFilter.ChildFilters )
                    {
                        var exp = GetDataFilterExpression( filter, resultEntityType, serviceInstance, parameter, dataViewFilterOverrides );
                        if ( exp == null )
                        {
                            /*
                            1/15/2021 - Shaun
                            Filter expressions of these types (GroupAny/GroupAllFalse) are joined with an OR clause,
                            so they must either be defaulted to false or excluded from the where expression altogether
                            (otherwise they will return every Person record in the database, because a "True OrElse
                            <anything>" predicate will always be true).
                            Therefore, if this child filter is null, we can simply ignore it and move on to the next one.
                            Reason: Correcting behavior of dynamic reports where a group is deselected at run time.
                            */

                            continue;
                        }

                        if ( orExp == null )
                        {
                            orExp = exp;
                        }
                        else
                        {
                            orExp = Expression.OrElse( orExp, exp );
                        }
                    }

                    if ( dataViewFilter.ExpressionType == FilterExpressionType.GroupAllFalse
                         && orExp != null )
                    {
                        // If all of the conditions must be false, invert the expression so that it becomes the logical equivalent of "NOT ANY".
                        orExp = Expression.Not( orExp );
                    }

                    if ( orExp == null )
                    {
                        // If there aren't any child filters for a GroupAny/GroupAllFalse. That is OK, so just don't filter anything.
                        return Expression.Constant( true );
                    }

                    return orExp;
                default:
                    throw new RockDataViewFilterExpressionException( dataViewFilter, $"Unexpected FilterExpressionType {dataViewFilter.ExpressionType} " );
            }
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Gets the most appropriate database context for this DataView's EntityType
        /// </summary>
        /// <returns></returns>
        private System.Data.Entity.DbContext GetDbContextForDataView( IDataViewDefinition dataView )
        {
            if ( dataView.DisableUseOfReadOnlyContext )
            {
                return new RockContext();
            }
            else
            {
                return new RockContextReadOnly();
            }
        }

        /// <summary>
        /// Gets the most appropriate service instance for this DataView's EntityType
        /// </summary>
        /// <param name="entityTypeId"></param>
        /// <param name="dbContext">The database context.</param>
        /// <returns></returns>
        private IService GetServiceForEntityType( int? entityTypeId, System.Data.Entity.DbContext dbContext )
        {
            if ( entityTypeId.HasValue )
            {
                var cachedEntityType = EntityTypeCache.Get( entityTypeId.Value );
                if ( cachedEntityType != null && cachedEntityType.AssemblyName != null )
                {
                    Type entityType = cachedEntityType.GetEntityType();

                    if ( entityType != null )
                    {
                        if ( dbContext != null )
                        {
                            return Reflection.GetServiceForEntityType( entityType, dbContext );
                        }
                    }
                }
            }

            return null;
        }

        #endregion
    }
}
