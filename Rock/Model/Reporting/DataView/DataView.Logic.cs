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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

using Rock.Data;
using Rock.Logging;
using Rock.Reporting;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a filterable DataView in Rock.
    /// </summary>
    public partial class DataView : Model<DataView>, ICategorized
    {
        /// <summary>
        /// Gets the parent security authority for the DataView which is its Category
        /// </summary>
        /// <value>
        /// The parent authority of the DataView.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                if ( this.Category != null )
                {
                    return this.Category;
                }

                return base.ParentAuthority;
            }
        }

        /// <summary>
        /// Determines whether [is authorized for all data view components] [the specified data view].
        /// </summary>
        /// <param name="dataViewAction">The data view action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="authorizationMessage">The authorization message.</param>
        /// <returns></returns>
        public bool IsAuthorizedForAllDataViewComponents( string dataViewAction, Person person, RockContext rockContext, out string authorizationMessage )
        {
            bool isAuthorized = true;
            authorizationMessage = string.Empty;

            // can't edit an existing DataView if not authorized for that DataView
            if ( this.Id != 0 && !this.IsAuthorized( dataViewAction, person ) )
            {
                isAuthorized = false;
                authorizationMessage = Rock.Constants.EditModeMessage.ReadOnlyEditActionNotAllowed( DataView.FriendlyTypeName );
            }

            if ( this.EntityType != null && !this.EntityType.IsAuthorized( Authorization.VIEW, person ) )
            {
                isAuthorized = false;
                authorizationMessage = "INFO: Data view uses an entity type that you do not have access to view.";
            }

            if ( this.DataViewFilter != null && !this.DataViewFilter.IsAuthorized( Authorization.VIEW, person ) )
            {
                isAuthorized = false;
                authorizationMessage = "INFO: Data view contains a filter that you do not have access to view.";
            }

            if ( this.TransformEntityTypeId != null )
            {
                string dataTransformationComponentTypeName = EntityTypeCache.Get( this.TransformEntityTypeId ?? 0 ).GetEntityType().FullName;
                var dataTransformationComponent = Rock.Reporting.DataTransformContainer.GetComponent( dataTransformationComponentTypeName );
                if ( dataTransformationComponent != null )
                {
                    if ( !dataTransformationComponent.IsAuthorized( Authorization.VIEW, person ) )
                    {
                        isAuthorized = false;
                        authorizationMessage = "INFO: Data view contains a data transformation that you do not have access to view.";
                    }
                }
            }

            return isAuthorized;
        }

        /// <summary>
        /// Gets the most appropriate database context for this DataView's EntityType
        /// </summary>
        /// <returns></returns>
        public System.Data.Entity.DbContext GetDbContext()
        {
            if ( EntityTypeId.HasValue )
            {
                var cachedEntityType = EntityTypeCache.Get( EntityTypeId.Value );
                if ( cachedEntityType != null && cachedEntityType.AssemblyName != null )
                {
                    Type entityType = cachedEntityType.GetEntityType();

                    if ( entityType != null )
                    {
                        return Reflection.GetDbContextForEntityType( entityType );
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the most appropriate service instance for this DataView's EntityType
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <returns></returns>
        public IService GetServiceInstance( System.Data.Entity.DbContext dbContext )
        {
            if ( EntityTypeId.HasValue )
            {
                var cachedEntityType = EntityTypeCache.Get( EntityTypeId.Value );
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

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="paramExpression">The parameter expression.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        [RockObsolete( "1.12" )]
        [Obsolete( "Use GetExpression( IService serviceInstance, ParameterExpression paramExpression )" )]
        public Expression GetExpression( IService serviceInstance, ParameterExpression paramExpression, out List<string> errorMessages )
        {
            return this.GetExpression( serviceInstance, paramExpression, null, out errorMessages );
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="paramExpression">The parameter expression.</param>
        /// <param name="dataViewFilterOverrides">The data view filter overrides.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        [RockObsolete( "1.12" )]
        [Obsolete( "Use GetExpression( IService serviceInstance, ParameterExpression paramExpression )" )]
        public Expression GetExpression( IService serviceInstance, ParameterExpression paramExpression, DataViewFilterOverrides dataViewFilterOverrides, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            return GetExpression( serviceInstance, paramExpression, dataViewFilterOverrides );
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="paramExpression">The parameter expression.</param>
        /// <returns></returns>
        public Expression GetExpression( IService serviceInstance, ParameterExpression paramExpression )
        {
            return this.GetExpression( serviceInstance, paramExpression, null );
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
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
        public Expression GetExpression( IService serviceInstance, ParameterExpression paramExpression, DataViewFilterOverrides dataViewFilterOverrides )
        {
            var dataViewEntityTypeCache = EntityTypeCache.Get( EntityTypeId.Value );

            if ( dataViewEntityTypeCache?.AssemblyName == null )
            {
                throw new RockDataViewFilterExpressionException( this.DataViewFilter, $"Unable to determine DataView Assembly for EntityTypeId { EntityTypeId }" );
            }

            Type dataViewEntityTypeType = dataViewEntityTypeCache.GetEntityType();
            if ( dataViewEntityTypeType == null )
            {
                throw new RockDataViewFilterExpressionException( this.DataViewFilter, $"Unable to determine DataView EntityType for { dataViewEntityTypeCache }." );
            }

            // DataViews must have a DataViewFilter (something has gone wrong it doesn't have one)
            // Note that DataViewFilterId might be null even though DataViewFilter is not null
            // This is because the DataViewFilter might be just in memory and not saved to the database (for example, a Preview or a DynamicReport)
            if ( this.DataViewFilter == null )
            {
                throw new RockDataViewFilterExpressionException( this.DataViewFilter, $"DataViewFilter is null for DataView { this.Name } ({this.Id})." );
            }

            bool usePersistedValues = this.PersistedScheduleIntervalMinutes.HasValue && this.PersistedLastRefreshDateTime.HasValue;
            if ( dataViewFilterOverrides != null )
            {
                // don't use persisted values if this DataView in the list of DataViews that should not be persisted due to override
                usePersistedValues = usePersistedValues && !dataViewFilterOverrides.IgnoreDataViewPersistedValues.Contains( this.Id );
            }

            // If dataViewFilterOverrides is null assume true in order to preserve current functionality.
            RockLogger.Log.Debug( RockLogDomains.Reporting, "{methodName} dataViewFilterOverrides: {@dataViewFilterOverrides} DataviewId: {DataviewId}", nameof( GetExpression ), dataViewFilterOverrides, DataViewFilter.DataViewId );
            if ( dataViewFilterOverrides == null || dataViewFilterOverrides.ShouldUpdateStatics )
            {
                DataViewService.AddRunDataViewTransaction( Id );
            }

            // We need to call GetExpression regardless of whether or not usePresistedValues is true so the child queries get their stats updated.
            var filterExpression = DataViewFilter != null ? DataViewFilter.GetExpression( dataViewEntityTypeType, serviceInstance, paramExpression, dataViewFilterOverrides ) : null;

            if ( usePersistedValues )
            {
                // If this is a persisted DataView, get the ids for the expression by querying DataViewPersistedValue instead of evaluating all the filters
                var rockContext = serviceInstance.Context as RockContext;
                if ( rockContext == null )
                {
                    rockContext = new RockContext();
                }

                var persistedValuesQuery = rockContext.DataViewPersistedValues.Where( a => a.DataViewId == this.Id );
                var ids = persistedValuesQuery.Select( v => v.EntityId );
                MemberExpression propertyExpression = Expression.Property( paramExpression, "Id" );
                if ( !( serviceInstance.Context is RockContext ) )
                {
                    // if this DataView doesn't use a RockContext get the EntityIds into memory as a List<int> then back into IQueryable<int> so that we aren't use multiple dbContexts
                    ids = ids.ToList().AsQueryable();
                }

                var idsExpression = Expression.Constant( ids.AsQueryable(), typeof( IQueryable<int> ) );

                Expression expression = Expression.Call( typeof( Queryable ), "Contains", new Type[] { typeof( int ) }, idsExpression, propertyExpression );

                return expression;
            }
            else
            {
                if ( dataViewEntityTypeCache.Id == EntityTypeCache.Get( typeof( Rock.Model.Person ) ).Id )
                {
                    var qry = new PersonService( ( RockContext ) serviceInstance.Context ).Queryable( this.IncludeDeceased );
                    Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, paramExpression, "p" );
                    if ( filterExpression == null )
                    {
                        filterExpression = extractedFilterExpression;
                    }
                    else
                    {
                        filterExpression = Expression.AndAlso( filterExpression, extractedFilterExpression );
                    }
                }

                if ( this.TransformEntityTypeId.HasValue )
                {

                    Expression transformedExpression = GetTransformExpression( this.TransformEntityTypeId.Value, serviceInstance, paramExpression, filterExpression );
                    if ( transformedExpression == null )
                    {
                        // if TransformEntityTypeId is defined, but we got null back, we'll get unexpected results, so throw an exception
                        throw new RockDataViewFilterExpressionException( this.DataViewFilter, $"Unable to determine transform expression for TransformEntityTypeId: {TransformEntityTypeId}" );
                    }

                    return transformedExpression;
                }

                return filterExpression;
            }
        }

        /// <summary>
        /// Persists the DataView to the database by updating the DataViewPersistedValues for this DataView. Returns true if successful
        /// </summary>
        /// <param name="databaseTimeoutSeconds">The database timeout seconds.</param>
        public void PersistResult( int? databaseTimeoutSeconds = null )
        {
            using ( var dbContext = this.GetDbContext() )
            {
                var persistStopwatch = Stopwatch.StartNew();
                var dataViewFilterOverrides = new DataViewFilterOverrides();

                dataViewFilterOverrides.ShouldUpdateStatics = false;

                // set an override so that the Persisted Values aren't used when rebuilding the values from the DataView Query
                dataViewFilterOverrides.IgnoreDataViewPersistedValues.Add( this.Id );
                var dataViewGetQueryArgs = new DataViewGetQueryArgs
                {
                    DbContext = dbContext,
                    DataViewFilterOverrides = dataViewFilterOverrides,
                    DatabaseTimeoutSeconds = databaseTimeoutSeconds,
                };

                var qry = this.GetQuery( dataViewGetQueryArgs );

                RockContext rockContext = dbContext as RockContext;
                if ( rockContext == null )
                {
                    rockContext = new RockContext();
                }

                rockContext.Database.CommandTimeout = databaseTimeoutSeconds;
                var savedDataViewPersistedValues = rockContext.DataViewPersistedValues.Where( a => a.DataViewId == this.Id );

                var updatedEntityIdsQry = qry.Select( a => a.Id );

                if ( !( rockContext is RockContext ) )
                {
                    // if this DataView doesn't use a RockContext get the EntityIds into memory as a List<int> then back into IQueryable<int> so that we aren't use multiple dbContexts
                    updatedEntityIdsQry = updatedEntityIdsQry.ToList().AsQueryable();
                }

                var persistedValuesToRemove = savedDataViewPersistedValues.Where( a => !updatedEntityIdsQry.Any( x => x == a.EntityId ) );
                var persistedEntityIdsToInsert = updatedEntityIdsQry.Where( x => !savedDataViewPersistedValues.Any( a => a.EntityId == x ) ).ToList();

                var removeCount = persistedValuesToRemove.Count();
                if ( removeCount > 0 )
                {
                    // increase the batch size if there are a bunch of rows (and this is a narrow table with no references to it)
                    int? deleteBatchSize = removeCount > 50000 ? 25000 : ( int? ) null;

                    int rowRemoved = rockContext.BulkDelete( persistedValuesToRemove, deleteBatchSize );
                }

                if ( persistedEntityIdsToInsert.Any() )
                {
                    List<DataViewPersistedValue> persistedValuesToInsert = persistedEntityIdsToInsert.OrderBy( a => a )
                        .Select( a =>
                        new DataViewPersistedValue
                        {
                            DataViewId = this.Id,
                            EntityId = a
                        } ).ToList();

                    rockContext.BulkInsert( persistedValuesToInsert );
                }

                persistStopwatch.Stop();

                // Update the Persisted Refresh information.
                PersistedLastRefreshDateTime = RockDateTime.Now;
                PersistedLastRunDurationMilliseconds = Convert.ToInt32( persistStopwatch.Elapsed.TotalMilliseconds );
            }
        }

        /// <summary>
        /// Gets the transform expression.
        /// </summary>
        /// <param name="transformEntityTypeId">The transform entity type identifier.</param>
        /// <param name="service">The service.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <returns></returns>
        private Expression GetTransformExpression( int transformEntityTypeId, IService service, ParameterExpression parameterExpression, Expression whereExpression )
        {
            var entityType = EntityTypeCache.Get( transformEntityTypeId );

            if ( entityType == null )
            {
                // if we can't determine EntityType, throw an exception so we don't return incorrect results
                throw new RockDataViewFilterExpressionException( this.DataViewFilter, $"Unable to determine TransformEntityType {entityType.Name}" );
            }


            var component = Rock.Reporting.DataTransformContainer.GetComponent( entityType.Name );
            if ( component == null )
            {
                // if we can't determine component, throw an exception so we don't return incorrect results
                throw new RockDataViewFilterExpressionException( this.DataViewFilter, $"Unable to determine transform component for {entityType.Name}" );
            }

            return component.GetExpression( service, parameterExpression, whereExpression );
        }
    }
}
