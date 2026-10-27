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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.ViewModels.Core;
using Rock.ViewModels.Rest.Models;
using Rock.Core.EntitySearch;

#if WEBFORMS
using System.Web.Http.Results;

using HttpError = System.Web.Http.HttpError;
using IActionResult = System.Web.Http.IHttpActionResult;
using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;
#endif

namespace Rock.Rest
{
    /// <summary>
    /// Helper class for providing standard CRUD API actions to various
    /// entity controllers.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TService">The type of the service class.</typeparam>
    public class CrudEndpointHelper<TEntity, TService>
        where TEntity : class, IEntity, new()
        where TService : Service<TEntity>
    {
        private readonly ApiControllerBase _controller;

        /// <summary>
        /// The audit properties that are set by Rock. Only callers that have
        /// security ignored (EXECUTE_UNRESTRICTED_WRITE) may write them.
        /// </summary>
        private static readonly HashSet<string> ProtectedAuditProperties = new HashSet<string>( StringComparer.OrdinalIgnoreCase )
        {
            nameof( IModel.CreatedByPersonAliasId ),
            nameof( IModel.ModifiedByPersonAliasId ),
            nameof( IModel.CreatedDateTime ),
            nameof( IModel.ModifiedDateTime ),
            nameof( IModel.ModifiedAuditValuesAlreadyUpdated )
        };

        /// <summary>
        /// A value indicating whether security is ignored. When security is
        /// not ignored the entity will be checked for either VIEW or EDIT
        /// permissions depending on the operation.
        /// </summary>
        public bool IsSecurityIgnored { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CrudEndpointHelper{TEntity, TService}"/> class.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public CrudEndpointHelper( ApiControllerBase controller )
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
                    return BadRequest( "Item to be created cannot be null." );
                }

                /*
                    10/15/2026 - MSE

                    Adding the entity also inserts every related record it carries, and only the
                    entity itself is authorized. The v2 formatter drops IEntity navigation
                    properties but keeps related records of mapped types that are not IEntity
                    (such as GroupLocationScheduleConfigs). It is also chosen by the request
                    path, so a site in a virtual directory reads the body with the v1
                    formatter, which keeps them all.

                    Reason: Authorize every record a POST inserts.
                */
                var relatedPropertyName = ApiControllerBase.GetUntrackedRelatedEntityPropertyName( entity, null, null );
                if ( relatedPropertyName != null )
                {
                    return BadRequest( $"Related records in '{relatedPropertyName}' cannot be saved with this item. Save them separately." );
                }

                if ( !entity.IsValid )
                {
                    var errorMessage = entity.ValidationResults.Select( r => r.ErrorMessage ).JoinStrings( ", " );

                    return BadRequest( errorMessage );
                }

                using ( var rockContext = new RockContext() )
                {
                    var service = ( Service<TEntity> ) Activator.CreateInstance( typeof( TService ), rockContext );
                    var entitySet = rockContext.Set<TEntity>();

                    /*
                        10/27/2026 - MSE

                        Attach the record with the DbSet here, not service.Add. SetValues needs the
                        clone to be tracked, but some services run their own rules in Add (for example
                        StepService checks the StepTypeId), and those rules must see the final values
                        and only run for an authorized caller. So Create attaches with the DbSet and
                        calls service.Add once below, after the authorization check, so the service
                        rules run on the populated record.

                        Reason: Creating a Step through the REST API failed for a valid step type.
                    */

                    // If the entity is not a dynamic proxy entity, then make it
                    // one so that lazy loading will work during the security
                    // check.
                    if ( !entity.IsDynamicProxyEntity() )
                    {
                        var clone = entitySet.Create();

                        entitySet.Add( clone );
                        service.SetValues( entity, clone );

                        entity = clone;
                    }
                    else
                    {
                        entitySet.Add( entity );
                    }

                    /*
                        10/15/2026 - MSE

                        Some IsAuthorized overrides grant EDIT to the record's author (for example
                        Note). The entity here is the row that will be saved and holds the posted
                        Id and audit values, so a caller could name themselves as author to skip
                        the parent check. Clear the Id and audit values for the check, then put the
                        posted audit values back so the saved row is the same as before this change.
                        The Guid is not changed because this is the saved row and clients may post it.

                        Reason: Do not let posted audit values grant EDIT on a new record.
                    */
                    entity.Id = 0;

                    var auditModel = entity as IModel;
                    var postedCreatedByPersonAliasId = auditModel?.CreatedByPersonAliasId;
                    var postedModifiedByPersonAliasId = auditModel?.ModifiedByPersonAliasId;
                    var postedModifiedAuditValuesAlreadyUpdated = auditModel?.ModifiedAuditValuesAlreadyUpdated ?? false;

                    if ( auditModel != null )
                    {
                        auditModel.CreatedByPersonAliasId = null;
                        auditModel.ModifiedByPersonAliasId = null;
                        auditModel.ModifiedAuditValuesAlreadyUpdated = false;
                    }

                    if ( !CheckAuthorized( Security.Authorization.EDIT, entity, out var authorizationResult ) )
                    {
                        return authorizationResult;
                    }

                    if ( auditModel != null )
                    {
                        auditModel.CreatedByPersonAliasId = postedCreatedByPersonAliasId;
                        auditModel.ModifiedByPersonAliasId = postedModifiedByPersonAliasId;
                        auditModel.ModifiedAuditValuesAlreadyUpdated = postedModifiedAuditValuesAlreadyUpdated;
                    }

                    if ( CheckIsSystem( entity, out var isSystemResult ) )
                    {
                        return isSystemResult;
                    }

                    // Run the service's own add rules (such as the StepService
                    // AllowMultiple and prerequisite checks) now that the values
                    // are set and the caller is authorized. The entity is already
                    // tracked as Added, so the DbSet add inside service.Add does
                    // not add it a second time.
                    service.Add( entity );

                    /*
                        10/19/2026 - MSE

                        A new record never takes the storage columns (such as BinaryFile Path) from
                        the client. The FileSystem provider trusts a stored Path that starts with ~,
                        so a posted Path could point the record at any file under the web root.
                        Clearing them lets Rock set them from the file type when the record is saved.
                        Nothing may read ContentStream on the posted entity before this clear, because
                        the cached stream would survive it and be written by the storage provider.

                        Reason: Clients must not choose where a stored file lives.
                    */
                    NotClientWritableAttribute.ClearValues( rockContext, entity, typeof( TEntity ) );

                    rockContext.SaveChanges();
                }

                var routePrefixAttribute = _controller.GetType().GetCustomAttribute<RoutePrefixAttribute>();
                var locationUri = new Uri( $"{routePrefixAttribute.Prefix}/{entity.Id}", UriKind.Relative );
                var response = new CreatedAtResponseBag
                {
                    Id = entity.Id,
                    Guid = entity.Guid,
                    IdKey = entity.IdKey,
                    Location = $"{_controller.RockRequestContext.RootUrlPath}{locationUri}"
                };

                return new CreatedNegotiatedContentResult<CreatedAtResponseBag>( locationUri, response, _controller );
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
        /// <param name="id">The identifier, unique identifier or IdKey of the item.</param>
        /// <returns>The response that should be sent back.</returns>
        public IActionResult Get( string id )
        {
            if ( id.IsNullOrWhiteSpace() )
            {
                return BadRequest( "Invalid key specified." );
            }

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    var service = ( Service<TEntity> ) Activator.CreateInstance( typeof( TService ), rockContext );
                    var entity = service.Get( id );

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
        /// <param name="id">The identifier, unique identifier or IdKey of the item.</param>
        /// <param name="entity">The entity data to update the existing entity with.</param>
        /// <returns>The response that should be sent back.</returns>
        public IActionResult Update( string id, TEntity entity )
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
                    var targetEntity = service.Get( id );

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

                    if ( CheckIsSystem( entity, out var isSystemResult ) || CheckIsSystem( targetEntity, out isSystemResult ) )
                    {
                        return isSystemResult;
                    }

                    /*
                        10/15/2026 - MSE

                        SetValues copies every posted value, including the audit columns and the
                        ModifiedAuditValuesAlreadyUpdated flag. A restricted caller could name
                        themselves as author (which some IsAuthorized overrides, such as Note, treat
                        as EDIT permission), back date the record, or keep a forged modified by
                        person. Restore the stored created values and clear the flag so Rock stamps
                        the modified values on save. We restore instead of rejecting so clients that
                        echo the whole record keep working.

                        Reason: Restricted callers must not write audit columns.
                    */
                    var storedModel = !IsSecurityIgnored ? targetEntity as IModel : null;
                    var storedCreatedByPersonAliasId = storedModel?.CreatedByPersonAliasId;
                    var storedCreatedDateTime = storedModel?.CreatedDateTime;

                    service.SetValues( entity, targetEntity );

                    if ( storedModel != null )
                    {
                        storedModel.CreatedByPersonAliasId = storedCreatedByPersonAliasId;
                        storedModel.CreatedDateTime = storedCreatedDateTime;
                        storedModel.ModifiedAuditValuesAlreadyUpdated = false;
                    }

                    /*
                        10/19/2026 - MSE

                        PUT copies every mapped column, including the storage columns (such as
                        BinaryFile Path) that only Rock may set. The FileSystem provider trusts a
                        stored Path that starts with ~, so a posted Path could point the record at
                        any file under the web root. We put the stored values back instead of
                        rejecting the request so clients that echo the whole record keep working.

                        Reason: Clients must not choose where a stored file lives.
                    */
                    NotClientWritableAttribute.RestoreOriginalValues( rockContext, targetEntity, typeof( TEntity ) );

                    // The check above authorized the record as it is stored. Authorize it again with the
                    // posted values applied, since they may change who is allowed to edit it.
                    if ( !CheckAuthorizedAfterUpdate( targetEntity, out authorizationResult ) )
                    {
                        return authorizationResult;
                    }

                    if ( !targetEntity.IsValid )
                    {
                        var errorMessage = targetEntity.ValidationResults.Select( r => r.ErrorMessage ).JoinStrings( ", " );

                        return BadRequest( errorMessage );
                    }

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
        /// <param name="id">The identifier, unique identifier or IdKey of the item.</param>
        /// <returns>The response that should be sent back.</returns>
        public IActionResult Delete( string id )
        {
            try
            {
                using ( var rockContext = new RockContext() )
                {
                    var service = ( Service<TEntity> ) Activator.CreateInstance( typeof( TService ), rockContext );
                    var entity = service.Get( id );

                    if ( entity == null )
                    {
                        return NotFound( "The item was not found." );
                    }

                    if ( !CheckAuthorized( Security.Authorization.EDIT, entity, out var authorizationResult ) )
                    {
                        return authorizationResult;
                    }

                    if ( CheckIsSystem( entity, out var isSystemResult ) )
                    {
                        return isSystemResult;
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
        /// <param name="id">The identifier, unique identifier or IdKey of the item.</param>
        /// <param name="values">The new values to be set on the entity.</param>
        /// <returns>The response that should be sent back.</returns>
        public IActionResult Patch( string id, Dictionary<string, object> values )
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
                    var entity = service.Get( id );

                    if ( entity == null )
                    {
                        return NotFound( "The item was not found." );
                    }

                    if ( !CheckAuthorized( Security.Authorization.EDIT, entity, out var authorizationResult ) )
                    {
                        return authorizationResult;
                    }

                    if ( CheckIsSystem( entity, out var isSystemResult ) )
                    {
                        return isSystemResult;
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
                            return BadRequest( $"{typeof( TEntity ).Name} does not have property '{propKey}'." );

                        }
                        else if ( property.Name == "IsSystem" )
                        {
                            return BadRequest( $"Modifying property '{propKey}' is not permitted." );
                        }
                        else if ( NotClientWritableAttribute.GetPropertyNames( typeof( TEntity ) ).Contains( property.Name ) )
                        {
                            /*
                                10/19/2026 - MSE

                                The storage columns (such as BinaryFile Path) are set only by Rock. The
                                FileSystem provider trusts a stored Path that starts with ~, so a patched
                                Path could point the record at any file under the web root. This applies
                                to every caller, like the IsSystem rule.

                                Reason: Clients must not choose where a stored file lives.
                            */
                            return BadRequest( $"Modifying property '{propKey}' is not permitted." );
                        }
                        else if ( !IsSecurityIgnored && ProtectedAuditProperties.Contains( property.Name ) )
                        {
                            /*
                                10/15/2026 - MSE

                                The audit columns are set by Rock. A restricted caller could otherwise
                                name themselves as author (which some IsAuthorized overrides, such as
                                Note, treat as EDIT permission), back date the record, or forge the
                                modified by person.

                                Reason: Restricted callers must not write audit columns.
                            */
                            return BadRequest( $"Modifying property '{propKey}' is not permitted." );
                        }
                        else if ( ApiControllerBase.IsNavigationProperty( property ) )
                        {
                            // Related records are changed by their foreign key properties, so the
                            // permission check below sees the change. See ApiController.Patch.
                            return BadRequest( $"Cannot write '{property.Name}'" );
                        }

                        var propertyType = Nullable.GetUnderlyingType( property.PropertyType ) ?? property.PropertyType;
                        var newValue = values[propKey];

                        if ( property.GetValue( entity ) == newValue )
                        {
                            continue;
                        }
                        else if ( !property.CanWrite )
                        {
                            return BadRequest( $"Cannot write '{property.Name}'" );
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
                                return BadRequest( $"Cannot cast '{property.Name}' to int32." );
                            }
                        }
                        else
                        {
                            var castedValue = Convert.ChangeType( newValue, propertyType );

                            property.SetValue( entity, castedValue );
                        }
                    }

                    // The check above authorized the record as it is stored. Authorize it again with the
                    // patched values applied, since they may change who is allowed to edit it.
                    if ( !CheckAuthorizedAfterUpdate( entity, out authorizationResult ) )
                    {
                        return authorizationResult;
                    }

                    // Verify model is valid before saving
                    if ( !entity.IsValid )
                    {
                        var errorMessage = entity.ValidationResults.Select( r => r.ErrorMessage ).JoinStrings( ", " );

                        return BadRequest( errorMessage );
                    }

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
        /// <param name="id">The identifier, unique identifier or IdKey of the item.</param>
        /// <returns>The response that should be sent back.</returns>
        public IActionResult GetAttributeValues( string id )
        {
            if ( id.IsNullOrWhiteSpace() )
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
                    var entity = service.Get( id );

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

                    foreach ( var attribute in attributedEntity.Attributes )
                    {
                        if ( !IsSecurityIgnored )
                        {
                            // The AuthorizedForEntity method will check explicit
                            // permissions on the entity. No inherited permissions
                            // will be considered.
                            var authorized = Security.Authorization.AuthorizedForEntity( attribute.Value, Security.Authorization.VIEW, _controller.RockRequestContext.CurrentPerson );

                            // If this attribute explicitly denies the person access
                            // then do not include the value. If it is explicitly
                            // allowed or not set either way then include it.
                            if ( authorized == false )
                            {
                                continue;
                            }
                        }

                        values.TryAdd( attribute.Key, new ModelAttributeValueBag
                        {
                            Value = attributedEntity.GetAttributeValue( attribute.Key ),
                            TextValue = attributedEntity.GetAttributeTextValue( attribute.Key ),
                            HtmlValue = attributedEntity.GetAttributeHtmlValue( attribute.Key ),
                            CondensedTextValue = attributedEntity.GetAttributeCondensedTextValue( attribute.Key ),
                            CondensedHtmlValue = attributedEntity.GetAttributeCondensedHtmlValue( attribute.Key )
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
        /// <param name="id">The identifier, unique identifier or IdKey of the item.</param>
        /// <param name="values">The new values to be set on the entity.</param>
        /// <returns>The response that should be sent back.</returns>
        public IActionResult PatchAttributeValues( string id, Dictionary<string, string> values )
        {
            if ( id.IsNullOrWhiteSpace() )
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
                    var entity = service.Get( id );

                    if ( entity == null )
                    {
                        return NotFound( "The item was not found." );
                    }

                    var entityAuthorized = CheckAuthorized( Security.Authorization.EDIT, entity, out _ );
                    var attributedEntity = ( IHasAttributes ) entity;

                    attributedEntity.LoadAttributes( rockContext );

                    // Loop over every key in the values dictionary and look
                    // for  matching attribute.
                    foreach ( var attrKey in values.Keys )
                    {
                        if ( !attributedEntity.Attributes.TryGetValue( attrKey, out var attribute ) )
                        {
                            return BadRequest( $"{typeof( TEntity ).Name} does not have attribute {attrKey}" );
                        }

                        if ( !IsSecurityIgnored )
                        {
                            // The AuthorizedForEntity method will check explicit
                            // permissions on the entity. No inherited permissions
                            // will be considered.
                            var authorized = Security.Authorization.AuthorizedForEntity( attribute, Security.Authorization.EDIT, _controller.RockRequestContext.CurrentPerson );

                            // If the attribute explicitly denied them edit
                            // permission then return an error.
                            if ( authorized == false )
                            {
                                return Unauthorized( $"You are not authorized to Edit attribute '{attrKey}'." );
                            }

                            // If the attribute did not explicitly grant them
                            // edit permission then check if they have edit
                            // permission on the entity, and if not return an
                            // error.
                            if ( authorized != true && !entityAuthorized )
                            {
                                return Unauthorized( $"You are not authorized to Edit attribute '{attrKey}'." );
                            }
                        }

                        attributedEntity.SetAttributeValue( attrKey, values[attrKey] ?? string.Empty );
                    }

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

        /// <summary>
        /// POST endpoint. Use this to perform a query via a user supplied
        /// entity search query. This should be considered an administrative
        /// level search since no security is checked and no limitations are
        /// set by the system.
        /// </summary>
        /// <param name="query">The custom user query options.</param>
        /// <returns>The response that should be sent back.</returns>
        public IActionResult Search( EntitySearchQueryBag query )
        {
            try
            {
                var entityType = EntityTypeCache.Get<TEntity>();
                var systemQuery = new EntitySearchSystemQuery
                {
                    CurrentPerson = _controller.RockRequestContext.CurrentPerson,
                    IsRefinementAllowed = true
                };

                using ( var rockContext = new RockContext() )
                {
                    var queryable = ( IQueryable<TEntity> ) Reflection.GetQueryableForEntityType( typeof( TEntity ), rockContext )
                        ?? throw new Exception( $"Entity type '{typeof( TEntity ).FullName}' does not support entity search." );

                    var results = EntitySearchHelper.GetSearchResults( queryable, systemQuery, query, rockContext );

                    // Because of the way the search results come back, it basically
                    // looks like dictionaries, so make sure the serializer knows to
                    // encode those since we wouldn't normally. This should not cause
                    // a problem for attribute value keys because those are not
                    // included in these responses.
                    var formatter = Utility.ApiPickerJsonMediaTypeFormatter.CreateV2Formatter();
                    if ( formatter.SerializerSettings.ContractResolver is Newtonsoft.Json.Serialization.DefaultContractResolver defaultContractResolver )
                    {
                        defaultContractResolver.NamingStrategy.ProcessDictionaryKeys = true;
                    }

                    return new FormattedContentResult<EntitySearchResultsBag>( HttpStatusCode.OK, results, formatter, null, _controller );
                }
            }
            catch ( System.Exception ex )
            {
                ExceptionLogService.LogException( ex );

                return InternalServerError( ex.Message );
            }
        }

        /// <summary>
        /// GET and POST endpoint. Use this to perform a query via a defined
        /// Entity Search.
        /// </summary>
        /// <param name="searchKey">The search key to use for the query.</param>
        /// <param name="query">The custom user query options.</param>
        /// <returns>The response that should be sent back.</returns>
        public IActionResult Search( string searchKey, EntitySearchQueryBag query )
        {
            try
            {
                var entityType = EntityTypeCache.Get<TEntity>();
                var entitySearch = EntitySearchCache.GetByEntityTypeAndKey( entityType, searchKey );

                if ( entitySearch == null )
                {
                    return NotFound( "Search key was not found." );
                }

                if ( !CheckAuthorized( Security.Authorization.EXECUTE, entitySearch, out var authorizationResult ) )
                {
                    return authorizationResult;
                }

                if ( !entitySearch.IsRefinementAllowed && query != null )
                {
                    return BadRequest( "Custom query options have been disabled." );
                }

                var results = EntitySearchService.GetSearchResults( entitySearch, query, _controller.RockRequestContext.CurrentPerson );

                // Because of the way the search results come back, it basically
                // looks like dictionaries, so make sure the serializer knows to
                // encode those since we wouldn't normally. This should not cause
                // a problem for attribute value keys because those are not
                // included in these responses.
                var formatter = Utility.ApiPickerJsonMediaTypeFormatter.CreateV2Formatter();
                if ( formatter.SerializerSettings.ContractResolver is Newtonsoft.Json.Serialization.DefaultContractResolver defaultContractResolver )
                {
                    defaultContractResolver.NamingStrategy.ProcessDictionaryKeys = true;
                }

                return new FormattedContentResult<EntitySearchResultsBag>( HttpStatusCode.OK, results, formatter, null, _controller );
            }
            catch ( System.Exception ex )
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

                if ( !isAuthorized )
                {
                    errorResult = Unauthorized( $"You are not authorized to {action.SplitCase().ToLower()} this item." );

                    return false;
                }
            }

            errorResult = null;

            return true;
        }

        /// <summary>
        /// Check if the current person is authorized to the entity with the
        /// type of action to be performed.
        /// </summary>
        /// <param name="action">The action to be performed.</param>
        /// <param name="entity">The entity to be checked against.</param>
        /// <param name="errorResult">The error result if <c>false</c> is returned; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if the person is authorized or security is disabled or not supported, <c>false</c> otherwise.</returns>
        private bool CheckAuthorized( string action, IEntityCache entity, out IActionResult errorResult )
        {
            if ( !IsSecurityIgnored && entity is ISecured securedEntity )
            {
                var isAuthorized = securedEntity.IsAuthorized( action, _controller.RockRequestContext.CurrentPerson );

                if ( !isAuthorized )
                {
                    errorResult = Unauthorized( $"You are not authorized to {action.SplitCase().ToLower()} this item." );

                    return false;
                }
            }

            errorResult = null;

            return true;
        }

        /// <summary>
        /// Checks that the current person can still EDIT the entity after the caller's values have been applied.
        /// If the values move the entity under a different parent authority, the person must also be able to
        /// EDIT it as a new record under that parent, the same rule a create applies.
        /// </summary>
        /// <param name="entity">The tracked entity with the caller's values applied.</param>
        /// <param name="errorResult">The error result if <c>false</c> is returned; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if the person is authorized or security is disabled or not supported, <c>false</c> otherwise.</returns>
        private bool CheckAuthorizedAfterUpdate( TEntity entity, out IActionResult errorResult )
        {
            if ( IsSecurityIgnored || !( entity is ISecured ) )
            {
                errorResult = null;

                return true;
            }

            using ( var rockContext = new RockContext() )
            {
                rockContext.Configuration.ProxyCreationEnabled = true;

                // Use a fresh copy so the parent authority and any related records load from the new
                // foreign keys, not the ones already loaded on the tracked entity. This context is never saved.
                var service = ( Service<TEntity> ) Activator.CreateInstance( typeof( TService ), rockContext );
                var updatedEntity = service.Get( entity.Id );

                if ( updatedEntity == null )
                {
                    errorResult = Unauthorized( "You are not authorized to edit this item." );

                    return false;
                }

                service.SetValues( entity, updatedEntity );

                // Must run while this context is open so related records can lazy-load.
                if ( !CheckAuthorized( Security.Authorization.EDIT, updatedEntity, out errorResult ) )
                {
                    return false;
                }

                /*
                    10/15/2026 - MSE

                    The copy above keeps the stored Id and author, so IsAuthorized overrides that
                    grant EDIT to the author (such as Note) still pass after the record is moved
                    under a parent the person could never add to. A record moved under a different
                    parent must meet the same bar as creating it there.

                    Reason: Authorize a parent change like a create under the new parent.
                */
                if ( GetParentAuthorityKey( updatedEntity as ISecured ) == GetStoredParentAuthorityKey( entity.Id ) )
                {
                    return true;
                }

                return CheckAuthorizedAsNewRecord( entity, out errorResult );
            }
        }

        /// <summary>
        /// Gets a key that identifies the parent authorities of the secured item.
        /// </summary>
        /// <param name="secured">The secured item.</param>
        /// <returns>A string made from the type and identifier of each parent authority.</returns>
        private static string GetParentAuthorityKey( ISecured secured )
        {
            if ( secured == null )
            {
                return string.Empty;
            }

            return $"{secured.ParentAuthority?.TypeId}:{secured.ParentAuthority?.Id}|{secured.ParentAuthorityPre?.TypeId}:{secured.ParentAuthorityPre?.Id}";
        }

        /// <summary>
        /// Gets the parent authority key of the entity as it is stored in the database.
        /// The caller's tracked entity is not saved yet, so this reads the stored parent
        /// without touching the tracked entity's navigation properties.
        /// </summary>
        /// <param name="id">The identifier of the entity.</param>
        /// <returns>The parent authority key of the stored entity.</returns>
        private string GetStoredParentAuthorityKey( int id )
        {
            using ( var storedContext = new RockContext() )
            {
                storedContext.Configuration.ProxyCreationEnabled = true;

                var storedService = ( Service<TEntity> ) Activator.CreateInstance( typeof( TService ), storedContext );

                return GetParentAuthorityKey( storedService.Get( id ) as ISecured );
            }
        }

        /// <summary>
        /// Checks that the current person could EDIT the entity's values as a new record.
        /// A proxy copy with no Id, no author and a new Guid is built in a separate context
        /// that is never saved, so parent authorities lazy-load from the new foreign keys and
        /// author or Guid based permissions do not apply.
        /// </summary>
        /// <param name="entity">The tracked entity with the caller's values applied.</param>
        /// <param name="errorResult">The error result if <c>false</c> is returned; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if the person is authorized, <c>false</c> otherwise.</returns>
        private bool CheckAuthorizedAsNewRecord( TEntity entity, out IActionResult errorResult )
        {
            using ( var newContext = new RockContext() )
            {
                newContext.Configuration.ProxyCreationEnabled = true;

                var newService = ( Service<TEntity> ) Activator.CreateInstance( typeof( TService ), newContext );
                var asNew = newContext.Set<TEntity>().Create();

                /*
                    10/27/2026 - MSE

                    Attach the copy with the DbSet instead of newService.Add. SetValues needs the
                    copy to be tracked, but some services run their own rules in Add (for example
                    StepService checks the StepTypeId), and the copy is still empty at this point,
                    so those rules fail for valid values. This copy is never saved.

                    Reason: Changing the step type of a Step through the REST API failed.
                */
                newContext.Set<TEntity>().Add( asNew );
                newService.SetValues( entity, asNew );

                // Clear these before any navigation property is read.
                asNew.Id = 0;
                asNew.Guid = Guid.NewGuid();
                if ( asNew is IModel asNewModel )
                {
                    asNewModel.CreatedByPersonAliasId = null;
                }

                return CheckAuthorized( Security.Authorization.EDIT, asNew, out errorResult );
            }
        }

        /// <summary>
        /// Check if the entity is classified as a "system" entity. These do
        /// not allow any editing actions performed on them.
        /// </summary>
        /// <param name="entity">The entity to be checked against.</param>
        /// <param name="errorResult">The error result if <c>true</c> is returned; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if the entity is considered "system" and should not be modified, <c>false</c> otherwise.</returns>
        private bool CheckIsSystem( IEntity entity, out IActionResult errorResult )
        {
            var isSystemProperty = entity.GetType().GetProperty( "IsSystem" );

            if ( isSystemProperty != null && ( bool ) isSystemProperty.GetValue( entity ) == true )
            {
                errorResult = BadRequest( "Not allowed to modify items marked as system." );
                return true;
            }

            errorResult = null;
            return false;
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
        /// Creates an unauthorized response with the given error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>The response object.</returns>
        private IActionResult Unauthorized( string errorMessage )
        {
            var error = new HttpError( errorMessage );

            return new NegotiatedContentResult<HttpError>( HttpStatusCode.Unauthorized, error, _controller );
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
    }
}
