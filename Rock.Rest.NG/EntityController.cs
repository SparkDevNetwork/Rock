using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Rock.Rest
{
    /// <summary>
    /// Provides basic CRUD operations for an entity API.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="Rock.Rest.ApiControllerBase" />
    [ApiController]
    public abstract class EntityController<TEntity> : ApiControllerBase
        where TEntity : Rock.Data.Entity<TEntity>, new()
    {
        #region Properties

        /// <summary>
        /// Gets the entity service handling access to this entity type.
        /// </summary>
        /// <value>
        /// The entity service handling access to this entity type.
        /// </value>
        protected Rock.Data.Service<TEntity> Service { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is currently
        /// ignoring security checks.
        /// </summary>
        /// <value>
        ///   <c>true</c> if security is ignored; otherwise, <c>false</c>.
        /// </value>
        protected bool IsSecurityIgnored { get; set; } = true;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityController{TEntity}"/> class.
        /// </summary>
        /// <param name="dependencies">The required dependencies.</param>
        public EntityController( EntityControllerDependencies dependencies )
            : base( dependencies.RockContext )
        {
            Service = dependencies.EntityServiceFactory.GetEntityService<TEntity>( DataContext );
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets a collection of all entities the user has access to.
        /// </summary>
        /// <param name="sort">The optional property name to sort on.</param>
        /// <param name="sortAscending">If <c>true</c> then sorting will be in ascending order.</param>
        /// <param name="cursor">The cursor returned by a previous request if loading another page of results.</param>
        /// <returns>The collection of entities.</returns>
        [HttpGet]
        [Route( "" )]
        [Produces( MediaTypeApplicationJson )]
        [ProducesResponseType( StatusCodes.Status400BadRequest )]
        [ProducesResponseType( StatusCodes.Status200OK )]
        public virtual ActionResult<PaginatedResult<TEntity>> GetEntities( string sort = null, bool sortAscending = true, string cursor = null )
        {
            var qry = GetQueryable();

            PaginationCursor currentCursor;

            if ( cursor.IsNotNullOrWhiteSpace() )
            {
                currentCursor = PaginationCursor.Decode( cursor );

                if ( currentCursor == null || currentCursor.LastId == 0 )
                {
                    return BadRequest();
                }
            }
            else
            {
                currentCursor = new PaginationCursor
                {
                    OrderByProperty = sort,
                    OrderByAscending = sortAscending
                };
            }

            int pageSize = 100;

            var paginatedResult = GetPaginatedResult( qry, pageSize, currentCursor );

            return Ok( paginatedResult );
        }

        /// <summary>
        /// Gets the entity as specified by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity.</param>
        /// <returns>The requested entity.</returns>
        [HttpGet]
        [Route( "{id:int}" )]
        [Produces( MediaTypeApplicationJson )]
        [ProducesResponseType( StatusCodes.Status404NotFound )]
        [ProducesResponseType( StatusCodes.Status401Unauthorized )]
        [ProducesResponseType( StatusCodes.Status200OK )]
        public virtual ActionResult<TEntity> GetEntityById( int id )
        {
            var entity = GetById( id );

            if ( entity == null )
            {
                return NotFound();
            }

            if ( !IsAuthorizedToView( entity ) )
            {
                return Unauthorized();
            }

            return Ok( entity );
        }

        /// <summary>
        /// Gets the entity as specified by its unique identifier.
        /// </summary>
        /// <param name="guid">The unique identifier of the entity.</param>
        /// <returns>The requested entity.</returns>
        [HttpGet]
        [Route( "{guid:guid}" )]
        [Produces( MediaTypeApplicationJson )]
        [ProducesResponseType( StatusCodes.Status404NotFound )]
        [ProducesResponseType( StatusCodes.Status401Unauthorized )]
        [ProducesResponseType( StatusCodes.Status200OK )]
        public virtual ActionResult<TEntity> GetEntityByGuid( Guid guid )
        {
            var entity = GetByGuid( guid );

            if ( entity == null )
            {
                return NotFound();
            }

            if ( !IsAuthorizedToView( entity ) )
            {
                return Unauthorized();
            }

            return Ok( entity );
        }

        /// <summary>
        /// Creates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The newly created entity.</returns>
        [HttpPost]
        [Route( "" )]
        [Consumes( "application/json" )]
        [Produces( MediaTypeApplicationJson )]
        [ProducesResponseType( StatusCodes.Status400BadRequest )]
        [ProducesResponseType( StatusCodes.Status401Unauthorized )]
        [ProducesResponseType( StatusCodes.Status200OK )]
        public virtual ActionResult<TEntity> CreateEntity( [FromBody] TEntity entity )
        {
            if ( entity.Id != 0 )
            {
                return BadRequest();
            }

            if ( !IsAuthorizedToEdit( entity ) )
            {
                return Unauthorized();
            }

            Service.Add( entity );
            DataContext.SaveChanges();

            return CreatedAtAction( nameof( GetEntityById ), new { id = entity.Id }, entity );
        }

        /// <summary>
        /// Replaces the entity specified by its identifier with the values
        /// provided.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="entity">The entity values.</param>
        /// <returns>The modified entity.</returns>
        [HttpPut]
        [Route( "{id:int}" )]
        [Consumes( "application/json" )]
        [Produces( MediaTypeApplicationJson )]
        [ProducesResponseType( StatusCodes.Status401Unauthorized )]
        [ProducesResponseType( StatusCodes.Status404NotFound )]
        [ProducesResponseType( StatusCodes.Status200OK )]
        public virtual ActionResult<TEntity> ReplaceEntityById( int id, TEntity entity )
        {
            var targetEntity = GetById( id );
            if ( targetEntity == null )
            {
                return NotFound();
            }

            return ReplaceEntity( targetEntity, entity );
        }

        /// <summary>
        /// Replaces the entity specified by its unique identifier with
        /// the values provided.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="entity">The entity values.</param>
        /// <returns>The modified entity.</returns>
        [HttpPut]
        [Route( "{guid:guid}" )]
        [Consumes( "application/json" )]
        [Produces( MediaTypeApplicationJson )]
        [ProducesResponseType( StatusCodes.Status400BadRequest )]
        [ProducesResponseType( StatusCodes.Status401Unauthorized )]
        [ProducesResponseType( StatusCodes.Status404NotFound )]
        [ProducesResponseType( StatusCodes.Status200OK )]
        public virtual ActionResult<TEntity> ReplaceEntityByGuid( Guid guid, TEntity entity )
        {
            var targetEntity = GetByGuid( guid );
            if ( targetEntity == null )
            {
                return NotFound();
            }

            return ReplaceEntity( targetEntity, entity );
        }

        /// <summary>
        /// Update an existing entity from the JSON Patch details.
        /// </summary>
        /// <param name="id">The identifier of the entity to be modified.</param>
        /// <param name="patch">The properties and conditions to use when updating the entity.</param>
        /// <returns>The updated entity.</returns>
        [HttpPatch]
        [Route( "{id:int}" )]
        [Consumes( "application/json" )]
        [Produces( MediaTypeApplicationJson )]
        [ProducesResponseType( StatusCodes.Status400BadRequest )]
        [ProducesResponseType( StatusCodes.Status401Unauthorized )]
        [ProducesResponseType( StatusCodes.Status404NotFound )]
        [ProducesResponseType( StatusCodes.Status200OK )]
        public virtual ActionResult<TEntity> PatchEntityById( int id, [FromBody] JsonPatchDocument<TEntity> patch )
        {
            if ( patch == null )
            {
                return BadRequest();
            }

            var entity = GetById( id );
            if ( entity == null )
            {
                return NotFound();
            }

            return PatchEntity( entity, patch );
        }

        /// <summary>
        /// Update an existing entity from the JSON Patch details.
        /// </summary>
        /// <param name="guid">The unique identifier of the entity to be modified.</param>
        /// <param name="patch">The properties and conditions to use when updating the entity.</param>
        /// <returns>The updated entity.</returns>
        [HttpPatch]
        [Route( "{guid:guid}" )]
        [Consumes( "application/json" )]
        [Produces( MediaTypeApplicationJson )]
        [ProducesResponseType( StatusCodes.Status400BadRequest )]
        [ProducesResponseType( StatusCodes.Status401Unauthorized )]
        [ProducesResponseType( StatusCodes.Status404NotFound )]
        [ProducesResponseType( StatusCodes.Status200OK )]
        public virtual ActionResult<TEntity> PatchEntityByGuid( Guid guid, [FromBody] JsonPatchDocument<TEntity> patch )
        {
            if ( patch == null )
            {
                return BadRequest();
            }

            var entity = GetByGuid( guid );
            if ( entity == null )
            {
                return NotFound();
            }

            return PatchEntity( entity, patch );
        }

        /// <summary>
        /// Deletes the entity specified by its identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route( "{id:int}" )]
        [Produces( MediaTypeApplicationJson )]
        [ProducesResponseType( StatusCodes.Status401Unauthorized )]
        [ProducesResponseType( StatusCodes.Status404NotFound )]
        [ProducesResponseType( StatusCodes.Status200OK )]
        public virtual IActionResult DeleteEntityById( int id )
        {
            var entity = GetById( id );

            if ( entity == null )
            {
                return NotFound();
            }

            if ( !IsAuthorizedToEdit( entity ) )
            {
                return Unauthorized();
            }

            Service.Delete( entity );
            DataContext.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Deletes the entity specified by its unique identifier.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route( "{guid:guid}" )]
        [Produces( MediaTypeApplicationJson )]
        [ProducesResponseType( StatusCodes.Status401Unauthorized )]
        [ProducesResponseType( StatusCodes.Status404NotFound )]
        [ProducesResponseType( StatusCodes.Status200OK )]
        public virtual IActionResult DeleteEntityByGuid( Guid guid )
        {
            var entity = GetByGuid( guid );

            if ( entity == null )
            {
                return NotFound();
            }

            if ( !IsAuthorizedToEdit( entity ) )
            {
                return Unauthorized();
            }

            Service.Delete( entity );
            DataContext.SaveChanges();

            return Ok();
        }

        #endregion

        #region Non Action Methods

        /// <summary>
        /// Gets the entity by its identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The entity.</returns>
        protected virtual TEntity GetById( int id )
        {
            return Service.Get( id );
        }

        /// <summary>
        /// Gets the entity by its unique identifier.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns>The entity.</returns>
        protected virtual TEntity GetByGuid( Guid guid )
        {
            return Service.Get( guid );
        }

        /// <summary>
        /// Gets an IQueryable that can be used to enumerate entities of this type.
        /// </summary>
        /// <returns>The <see cref="IQueryable{T}"/> for these entities.</returns>
        protected virtual IQueryable<TEntity> GetQueryable()
        {
            return Service.Queryable();
        }

        /// <summary>
        /// Replaces an entity in the database with the values from another
        /// entity that the user has provided.
        /// </summary>
        /// <param name="targetEntity">The target entity.</param>
        /// <param name="sourceEntity">The source entity.</param>
        /// <returns>The result of the action.</returns>
        protected virtual ActionResult<TEntity> ReplaceEntity( TEntity targetEntity, TEntity sourceEntity )
        {
            if ( !IsAuthorizedToEdit( targetEntity ) )
            {
                return Unauthorized();
            }

            Service.SetValues( sourceEntity, targetEntity );

            if ( !targetEntity.IsValid )
            {
                return BadRequest();
            }

            // Check security again as they might be trying to change a
            // property that affects their authorization level.
            if ( !IsAuthorizedToEdit( targetEntity ) )
            {
                return BadRequest();
            }

            DataContext.SaveChanges();

            return Ok( targetEntity );
        }

        /// <summary>
        /// Patches the entity with the given patch document that describes
        /// the conditions and changes to make.
        /// </summary>
        /// <param name="targetEntity">The target entity.</param>
        /// <param name="patch">The patches to make.</param>
        /// <returns>The result of the action.</returns>
        protected virtual ActionResult<TEntity> PatchEntity( TEntity targetEntity, JsonPatchDocument<TEntity> patch )
        {
            if ( !IsAuthorizedToEdit( targetEntity ) )
            {
                return Unauthorized();
            }

            patch.ApplyTo( targetEntity, ModelState );

            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            if ( !IsAuthorizedToEdit( targetEntity ) )
            {
                return Unauthorized();
            }

            DataContext.SaveChanges();

            return Ok( targetEntity );
        }

        /// <summary>
        /// Gets the <see cref="PaginatedResult{T}"/> object for the query and
        /// the current pagination values.
        /// </summary>
        /// <param name="query">The query object.</param>
        /// <param name="pageSize">Number of items per result set.</param>
        /// <param name="currentCursor">The current cursor that describes what items to get.</param>
        /// <returns>A <see cref="PaginatedResult{T}"/> that contains the resulting items.</returns>
        protected virtual PaginatedResult<TEntity> GetPaginatedResult( IQueryable<TEntity> query, int pageSize, PaginationCursor currentCursor )
        {
            var paginatedResult = new PaginatedResult<TEntity>
            {
                Items = new List<TEntity>()
            };

            int foundItems = 0;

            // Configure sorting behavior.
            if ( currentCursor.OrderByProperty.IsNotNullOrWhiteSpace() )
            {
                if ( currentCursor.OrderByAscending )
                {
                    query = query.OrderBy( currentCursor.OrderByProperty )
                        .ThenBy( a => a.Id );
                }
                else
                {
                    query = query.OrderByDescending( currentCursor.OrderByProperty )
                        .ThenByDescending( a => a.Id );
                }
            }

            // If this is a "next page" request then update the query to give
            // us results from the next page.
            if ( currentCursor.LastId > 0 )
            {
                query = GetNextPageQueryable( query, currentCursor );
            }

            // Query twice as many items as we are going to return on the
            // assumption some will be filtered out.
            int querySize = pageSize * 2;
            for ( int offset = 0; paginatedResult.Items.Count < pageSize; offset += querySize )
            {
                var items = query.Skip( offset ).Take( querySize ).ToList();

                if ( items.Count == 0 )
                {
                    break;
                }

                foreach ( var entity in items )
                {
                    if ( IsAuthorizedToView( entity ) )
                    {
                        paginatedResult.Items.Add( entity );
                        foundItems += 1;

                        if ( paginatedResult.Items.Count >= pageSize )
                        {
                            break;
                        }
                    }
                }
            }

            // If we didn't get a full page then we are done.
            if ( foundItems < pageSize )
            {
                return paginatedResult;
            }

            // Build the cursor that will get them to the next page.
            var lastItem = paginatedResult.Items.Last();
            var lastValue = currentCursor.OrderByProperty.IsNotNullOrWhiteSpace() ? lastItem.GetPropertyValue( currentCursor.OrderByProperty ) : null;

            var nextCursor = new PaginationCursor
            {
                OrderByProperty = currentCursor.OrderByProperty,
                OrderByAscending = currentCursor.OrderByAscending,
                LastOrderValue = lastValue != null ? Newtonsoft.Json.Linq.JToken.FromObject( lastValue ) : null,
                LastId = lastItem.Id
            };

            paginatedResult.NextPage = PaginationCursor.Encode( nextCursor );

            return paginatedResult;
        }

        /// <summary>
        /// Gets the queryable for the next page of results.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="currentCursor">The current cursor.</param>
        /// <returns></returns>
        /// <remarks>
        /// NOTE: This has not been thoroughly tested. Just enough to proof of
        /// concept that this will indeed work as a potential solution.
        /// </remarks>
        protected virtual IQueryable<TEntity> GetNextPageQueryable( IQueryable<TEntity> query, PaginationCursor currentCursor )
        {
            if ( currentCursor.OrderByProperty.IsNotNullOrWhiteSpace() )
            {
                var type = typeof( TEntity );
                var property = type.GetProperty( currentCursor.OrderByProperty );
                var propertyNullable = property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof( Nullable<> );
                var parameter = Expression.Parameter( type, "p" );
                var lastValue = currentCursor.LastOrderValue?.ToObject( property.PropertyType );

                Expression valueEqualExpression;
                Expression valueOrderExpression = null;

                // Get the expressions that have to do with the property name.
                // We have a number of possible situations:
                // lastValue could be null or not-null.
                // Database property could be either nullable or not nullable.
                // Sorting descending or ascending.
                // All combinations need to be tested. Only a few are tested so far.
                // Rather than these complicated if statements, might be better to
                // whiteboard all possible combinations and then check for each combination
                // and construct the logic expression for it.
                if ( lastValue == null )
                {
                    if ( currentCursor.OrderByAscending )
                    {
                        valueEqualExpression = ExpressionHelper.IsNullExpression( type, parameter, currentCursor.OrderByProperty );
                        valueOrderExpression = ExpressionHelper.IsNotNullExpression( type, parameter, currentCursor.OrderByProperty );
                    }
                    else
                    {
                        valueEqualExpression = ExpressionHelper.IsNullExpression( type, parameter, currentCursor.OrderByProperty );
                    }
                }
                else
                {
                    valueEqualExpression = ExpressionHelper.ComparisonExpression( type, parameter, property.Name, lastValue, 0 );

                    valueOrderExpression = ExpressionHelper.ComparisonExpression( type, parameter, property.Name, lastValue, currentCursor.OrderByAscending ? 1 : -1 );

                    if ( propertyNullable && !currentCursor.OrderByAscending )
                    {
                        valueOrderExpression = Expression.OrElse( valueOrderExpression, ExpressionHelper.IsNullExpression( type, parameter, property.Name ) );
                    }
                }

                // Get the expression for the Id value to be combined with
                // the 'property == oldValue' expression.
                var idExpression = ExpressionHelper.ComparisonExpression( type, parameter, "Id", (int) currentCursor.LastId, currentCursor.OrderByAscending ? 1 : -1 );

                Expression logicExpression;

                // Build the final expression:
                if ( valueOrderExpression != null )
                {
                    // (property == oldValue && Id > lastId) OR property > oldValue
                    logicExpression = Expression.OrElse( valueOrderExpression, Expression.AndAlso( valueEqualExpression, idExpression ) );
                }
                else
                {
                    // property == oldValue && Id > lastId
                    logicExpression = Expression.AndAlso( valueEqualExpression, idExpression );
                }

                // Turn the logic expression into a where clause.
                var resultExpression = Expression.Call( typeof( Queryable ),
                    "Where",
                    new Type[] { type },
                    query.Expression,
                    Expression.Quote( Expression.Lambda( logicExpression, parameter ) ) );

                return query.Provider.CreateQuery<TEntity>( resultExpression );
            }
            else
            {
                if ( currentCursor.OrderByAscending )
                {
                    return query.Where( a => a.Id > currentCursor.LastId );
                }
                else
                {
                    return query.Where( a => a.Id < currentCursor.LastId );
                }
            }
        }

        /// <summary>
        /// Determines whether the current person is authorized to view the
        /// entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        ///   <c>true</c> if the person is authorized; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsAuthorizedToView( TEntity entity )
        {
            return IsPersonAuthorizedToView( GetCurrentPerson(), entity );
        }

        /// <summary>
        /// Determines whether the person is authorized to view the
        /// entity.
        /// </summary>
        /// <param name="person">The person to check authorization for.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>
        ///   <c>true</c> if the person is authorized; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsPersonAuthorizedToView( Rock.Model.Person person, TEntity entity )
        {
            if ( !IsSecurityIgnored && entity is Rock.Security.ISecured securedEntity )
            {
                return securedEntity.IsAuthorized( Rock.Security.Authorization.VIEW, person );
            }

            return true;
        }

        /// <summary>
        /// Determines whether the current person is authorized to edit the
        /// entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        ///   <c>true</c> if the person is authorized; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsAuthorizedToEdit( TEntity entity )
        {
            return IsPersonAuthorizedToEdit( GetCurrentPerson(), entity );
        }

        /// <summary>
        /// Determines whether the person is authorized to edit the
        /// entity.
        /// </summary>
        /// <param name="person">The person to check authorization for.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>
        ///   <c>true</c> if the person is authorized; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsPersonAuthorizedToEdit( Rock.Model.Person person, TEntity entity )
        {
            if ( !IsSecurityIgnored && entity is Rock.Security.ISecured securedEntity )
            {
                return securedEntity.IsAuthorized( Rock.Security.Authorization.EDIT, person );
            }

            return true;
        }

        #endregion
    }
}
