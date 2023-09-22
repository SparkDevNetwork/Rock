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
using System.Net;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

using IActionResult = System.Web.Http.IHttpActionResult;

namespace Rock.Rest
{
    /// <summary>
    /// Helper class for providing standard REST API actions to various
    /// entity controllers.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TService">The type of the service class.</typeparam>
    internal class RestApiHelper<TEntity, TService>
        where TEntity : Entity<TEntity>, new()
        where TService : Service<TEntity>
    {
        private readonly ApiControllerBase _controller;

        /// <summary>
        /// <para>
        /// Gets or sets a value indicating whether security is ignored. When
        /// security is not ignored the entity will be checked for either VIEW
        /// or EDIT permissions depending on the operation. If that fails then
        /// the current REST action will be checked for ADMINISTRATE permission.
        /// </para>
        /// <para>
        /// In other words, having ADMINISTRATE authorization to the REST
        /// action will cause entity security to be bypassed.
        /// </para>
        /// </summary>
        /// <value><c>true</c> if security is ignored; otherwise, <c>false</c>.</value>
        public bool IsSecurityIgnored { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestApiHelper{TEntity, TService}"/> class.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public RestApiHelper( ApiControllerBase controller )
        {
            _controller = controller;
        }

        #region API Methods

        /// <summary>
        /// POST endpoint. Use this to INSERT a new <typeparamref name="TEntity"/> entity.
        /// </summary>
        /// <param name="entity">The entity to be created.</param>
        /// <returns>The response that should be sent back.</returns>
        public IActionResult Create( TEntity entity )
        {
            try
            {
                if ( entity == null )
                {
                    return BadRequest( "Item to be created cannot be null.");
                }

                if ( !entity.IsValid )
                {
                    var errorMessage = entity.ValidationResults.Select( r => r.ErrorMessage ).JoinStrings( ", " );

                    return BadRequest( errorMessage );
                }

                if ( !CheckAuthorized( Security.Authorization.EDIT, entity, out var authorizationResult ) )
                {
                    return authorizationResult;
                }

                using ( var rockContext = new RockContext() )
                {
                    new Service<TEntity>( rockContext ).Add( entity );

                    // TODO: Do something with this.
                    // System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", GetPerson() );

                    rockContext.SaveChanges();
                }

                var locationUri = new Uri( $"/api/v2/models/{typeof( TEntity ).Name.Pluralize().ToLower()}/{entity.Id}", UriKind.Relative );
                var response = new CreatedAtResult
                {
                    Id = entity.Id,
                    Guid = entity.Guid,
                    IdKey = entity.IdKey
                };

                return new CreatedNegotiatedContentResult<CreatedAtResult>( locationUri, response, _controller );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );

                return InternalServerError( ex.Message );
            }
        }

        /// <summary>
        /// GET endpoint. Use this to get an existing <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="key">The identifier, unique identifier or IdKey of the item.</param>
        /// <returns>The response that should be sent back.</returns>
        public IActionResult Get( string key )
        {
            if ( key.IsNullOrWhiteSpace() )
            {
                return BadRequest( "Invalid key specified." );
            }

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    var service = ( Service<TEntity> ) Activator.CreateInstance( typeof( TService ), rockContext );
                    var entity = service.Get( key );

                    if ( entity == null )
                    {
                        return NotFound( "The item was not found." );
                    }

                    if ( !CheckAuthorized( Security.Authorization.VIEW, entity, out var authorizationResult ) )
                    {
                        return authorizationResult;
                    }

                    return new OkNegotiatedContentResult<TEntity>( entity, _controller );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );

                return InternalServerError( ex.Message );
            }
        }

        /// <summary>
        /// PUT endpoint. Use this to update an existing <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="key">The identifier, unique identifier or IdKey of the item.</param>
        /// <param name="entity">The entity data to update the existing entity with.</param>
        /// <returns>The response that should be sent back.</returns>
        public IActionResult Update( string key, TEntity entity )
        {
            try
            {
                if ( entity == null )
                {
                    return BadRequest( "Item to be updated cannot be null." );
                }

                using ( var rockContext = new RockContext() )
                {
                    var service = ( Service<TEntity> ) Activator.CreateInstance( typeof( TService ), rockContext );
                    var targetEntity = service.Get( key );

                    if ( targetEntity == null )
                    {
                        return NotFound( "The item was not found." );
                    }

                    if ( entity.Id != targetEntity.Id )
                    {
                        return BadRequest( "Item identifiers do not match." );
                    }

                    if ( !CheckAuthorized( Security.Authorization.EDIT, targetEntity, out var authorizationResult ) )
                    {
                        return authorizationResult;
                    }

                    service.SetValues( entity, targetEntity );

                    if ( !targetEntity.IsValid )
                    {
                        var errorMessage = targetEntity.ValidationResults.Select( r => r.ErrorMessage ).JoinStrings( ", " );

                        return BadRequest( errorMessage );
                    }

                    // TODO: Do something with this.
                    // System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", GetPerson() );

                    rockContext.SaveChanges();

                    return NoContent();
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );

                return InternalServerError( ex.Message );
            }
        }

        /// <summary>
        /// DELETE endpoint. Use this to delete an existing <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="key">The identifier, unique identifier or IdKey of the item.</param>
        /// <returns>The response that should be sent back.</returns>
        public IActionResult Delete( string key )
        {
            try
            {
                using ( var rockContext = new RockContext() )
                {
                    var service = ( Service<TEntity> ) Activator.CreateInstance( typeof( TService ), rockContext );
                    var entity = service.Get( key );

                    if ( entity == null )
                    {
                        return NotFound( "The item was not found." );
                    }

                    if ( !CheckAuthorized( Security.Authorization.EDIT, entity, out var authorizationResult ) )
                    {
                        return authorizationResult;
                    }

                    service.Delete( entity );

                    rockContext.SaveChanges();

                    return NoContent();
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );

                return InternalServerError( ex.Message );
            }
        }

        /// <summary>
        /// PATCH endpoint. Use this to perform a partial update to an
        /// existing <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="key">The identifier, unique identifier or IdKey of the item.</param>
        /// <param name="values">The new values to be set on the entity.</param>
        /// <returns>The response that should be sent back.</returns>
        public IActionResult Patch( string key, Dictionary<string, object> values )
        {
            try
            {
                // Check that something was sent in the body
                if ( values == null || !values.Keys.Any() )
                {
                    return BadRequest( "No values were sent in the body." );
                }
                else if ( values.ContainsKey( "Id" ) || values.ContainsKey( "id" ) )
                {
                    return BadRequest( "Cannot set Id." );
                }

                using ( var rockContext = new RockContext() )
                {
                    var service = ( Service<TEntity> ) Activator.CreateInstance( typeof( TService ), rockContext );
                    var entity = service.Get( key );

                    if ( entity == null )
                    {
                        return NotFound( "The item was not found." );
                    }

                    if ( !CheckAuthorized( Security.Authorization.EDIT, entity, out var authorizationResult ) )
                    {
                        return authorizationResult;
                    }

                    var type = entity.GetType();
                    var properties = type.GetProperties( BindingFlags.Public | BindingFlags.Instance ).ToList();

                    // Loop over every key in the values dictionary and look
                    // for  matching property.
                    foreach ( var propKey in values.Keys )
                    {
                        var property = properties.FirstOrDefault( p => p.Name.Equals( propKey, StringComparison.OrdinalIgnoreCase ) );

                        if ( property == null )
                        {
                            return BadRequest( $"{typeof( TEntity ).Name} does not have property {propKey}" );

                        }

                        var propertyType = Nullable.GetUnderlyingType( property.PropertyType ) ?? property.PropertyType;
                        var newValue = values[propKey];

                        if ( property.GetValue( entity ) == newValue )
                        {
                            continue;
                        }
                        else if ( !property.CanWrite )
                        {
                            return BadRequest( $"Cannot write {property.Name}" );
                        }

                        if ( newValue == null )
                        {
                            // No need to parse anything
                            property.SetValue( entity, null );
                        }
                        else if ( propertyType == typeof( int ) || propertyType == typeof( int? ) || propertyType.IsEnum )
                        {
                            // By default, objects that hold integer values, hold int64, so coerce to int32
                            try
                            {
                                var int32 = Convert.ToInt32( newValue );
                                property.SetValue( entity, int32 );
                            }
                            catch ( OverflowException )
                            {
                                return BadRequest( $"Cannot cast {property.Name} to int32." );
                            }
                        }
                        else
                        {
                            var castedValue = Convert.ChangeType( newValue, propertyType );

                            property.SetValue( entity, castedValue );
                        }
                    }

                    // Verify model is valid before saving
                    if ( !entity.IsValid )
                    {
                        var errorMessage = entity.ValidationResults.Select( r => r.ErrorMessage ).JoinStrings( ", " );

                        return BadRequest( errorMessage );
                    }

                    // TODO: Do something with this.
                    // System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", GetPerson() );

                    rockContext.SaveChanges();

                    return NoContent();
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );

                return InternalServerError( ex.Message );
            }
        }

        /// <summary>
        /// Get all the attribute values for the <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="key">The identifier, unique identifier or IdKey of the item.</param>
        /// <returns>The response that should be sent back.</returns>
        public IActionResult GetAttributeValues( string key )
        {
            if ( key.IsNullOrWhiteSpace() )
            {
                return BadRequest( "Invalid key specified." );
            }

            if ( !typeof( IHasAttributes ).IsAssignableFrom( typeof( TEntity ) ) )
            {
                return BadRequest( "This item does not support attributes." );
            }

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    var service = ( Service<TEntity> ) Activator.CreateInstance( typeof( TService ), rockContext );
                    var entity = service.Get( key );

                    if ( entity == null )
                    {
                        return NotFound( "The item was not found." );
                    }

                    if ( !CheckAuthorized( Security.Authorization.VIEW, entity, out var authorizationResult ) )
                    {
                        return authorizationResult;
                    }

                    var attributedEntity = ( IHasAttributes ) entity;
                    var values = new Dictionary<string, object>();

                    attributedEntity.LoadAttributes( rockContext );

                    foreach ( var attributeKey in attributedEntity.Attributes.Keys)
                    {
                        values.AddOrIgnore( attributeKey, new
                        {
                            Value = attributedEntity.GetAttributeValue( attributeKey ),
                            TextValue = attributedEntity.GetAttributeTextValue( attributeKey  ),
                            HtmlValue = attributedEntity.GetAttributeHtmlValue( attributeKey ),
                            CondensedTextValue = attributedEntity.GetAttributeCondensedTextValue( attributeKey ),
                            CondensedHtmlValue = attributedEntity.GetAttributeCondensedHtmlValue( attributeKey ),
                            NullValue = (string)null
                        } );
                    }

                    return new OkNegotiatedContentResult<Dictionary<string, object>>( values, _controller );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );

                return InternalServerError( ex.Message );
            }
        }

        /// <summary>
        /// PATCH endpoint. Use this to perform a partial update of attribute
        /// values to an existing <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="key">The identifier, unique identifier or IdKey of the item.</param>
        /// <param name="values">The new values to be set on the entity.</param>
        /// <returns>The response that should be sent back.</returns>
        public IActionResult PatchAttributeValues( string key, Dictionary<string, string> values )
        {
            if ( key.IsNullOrWhiteSpace() )
            {
                return BadRequest( "Invalid key specified." );
            }

            if ( !typeof( IHasAttributes ).IsAssignableFrom( typeof( TEntity ) ) )
            {
                return BadRequest( "This item does not support attributes." );
            }

            try
            {
                // Check that something was sent in the body
                if ( values == null || !values.Keys.Any() )
                {
                    return BadRequest( "No values were sent in the body." );
                }

                using ( var rockContext = new RockContext() )
                {
                    var service = ( Service<TEntity> ) Activator.CreateInstance( typeof( TService ), rockContext );
                    var entity = service.Get( key );

                    if ( entity == null )
                    {
                        return NotFound( "The item was not found." );
                    }

                    if ( !CheckAuthorized( Security.Authorization.EDIT, entity, out var authorizationResult ) )
                    {
                        return authorizationResult;
                    }

                    var attributedEntity = ( IHasAttributes ) entity;

                    attributedEntity.LoadAttributes( rockContext );

                    // Loop over every key in the values dictionary and look
                    // for  matching attribute.
                    foreach ( var attrKey in values.Keys )
                    {
                        if ( !attributedEntity.Attributes.ContainsKey( attrKey ) )
                        {
                            return BadRequest( $"{typeof( TEntity ).Name} does not have attribute {attrKey}" );

                        }

                        attributedEntity.SetAttributeValue( attrKey, values[attrKey] ?? string.Empty );
                    }

                    // TODO: Do something with this.
                    // System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", GetPerson() );

                    attributedEntity.SaveAttributeValues( rockContext );

                    return NoContent();
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );

                return InternalServerError( ex.Message );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Check if the current person is authorized to the entity with the
        /// type of action to be performed.
        /// </summary>
        /// <param name="action">The action to be performed.</param>
        /// <param name="entity">The entity to be checked against.</param>
        /// <param name="errorResult">The error result if <c>false</c> is returned; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if the person is authorized or security is disabled or not supported, <c>false</c> otherwise.</returns>
        private bool CheckAuthorized( string action, IEntity entity, out IActionResult errorResult )
        {
            if ( !IsSecurityIgnored && entity is ISecured securedEntity )
            {
                var isAuthorized = securedEntity.IsAuthorized( action, _controller.RockRequestContext.CurrentPerson );

                // If they were not authorized by the entity, check if they have
                // Administrate authorization to the API, which grants them access
                // anyway.
                if ( !isAuthorized && _controller.ActionContext.ActionDescriptor is ReflectedHttpActionDescriptor actionDescriptor )
                {
                    var restGuid = actionDescriptor.MethodInfo.GetCustomAttribute<SystemGuid.RestActionGuidAttribute>()?.Guid;

                    if ( restGuid.HasValue )
                    {
                        var restAction = RestActionCache.Get( restGuid.Value );

                        isAuthorized = restAction?.IsAuthorized( Security.Authorization.ADMINISTRATE, _controller.RockRequestContext.CurrentPerson ) ?? false;
                    }
                }

                if ( !isAuthorized )
                {
                    var error = new HttpError( $"You are not authorized to {action.SplitCase().ToLower()} this item." );

                    errorResult = new NegotiatedContentResult<HttpError>( HttpStatusCode.Unauthorized, error, _controller );

                    return false;
                }
            }

            errorResult = null;

            return true;
        }

        /// <summary>
        /// Returns a response object to indicate that no content was generated.
        /// </summary>
        /// <returns>The response object.</returns>
        private IActionResult NoContent()
        {
            return new StatusCodeResult( HttpStatusCode.NoContent, _controller );
        }

        /// <summary>
        /// Creates a not found response with the given error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>The response object.</returns>
        private IActionResult NotFound( string errorMessage )
        {
            var error = new HttpError( errorMessage );

            return new NegotiatedContentResult<HttpError>( HttpStatusCode.NotFound, error, _controller );
        }

        /// <summary>
        /// Creates a bad request response with the given error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>The response object.</returns>
        private IActionResult BadRequest( string errorMessage )
        {
            return new BadRequestErrorMessageResult( errorMessage, _controller );
        }

        /// <summary>
        /// Creates an internals erver error response with the given error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>The response object.</returns>
        private IActionResult InternalServerError( string errorMessage )
        {
            var error = new HttpError( errorMessage );

            return new NegotiatedContentResult<HttpError>( HttpStatusCode.InternalServerError, error, _controller );
        }

        #endregion

        /// <summary>
        /// The response to be sent for a newly created item.
        /// </summary>
        private class CreatedAtResult
        {
            /// <summary>
            /// Gets or sets the integer identifier.
            /// </summary>
            /// <value>The integer identifier.</value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            /// <value>The unique identifier.</value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the identifier key.
            /// </summary>
            /// <value>The identifier key.</value>
            public string IdKey { get; set; }
        }
    }
}
