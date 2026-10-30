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
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;

using Microsoft.Extensions.DependencyInjection;

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Web.Cache;

#if WEBFORMS
using System.Web.Http.Results;

using IActionResult = System.Web.Http.IHttpActionResult;
#endif

namespace Rock.Rest
{
    /*
     * NOTE: We could have inherited from System.Web.Http.OData.ODataController, but that changes
     * the response format from vanilla REST to OData format. That breaks existing Rock Rest clients.
     *
     */

    /// <summary>
    /// Base ApiController for Rock REST endpoints
    /// Supports ODataV3 Queries and ODataRouting
    /// </summary>
    /// <seealso cref="System.Web.Http.ApiController" />
    [ODataRouting]
    public class ApiControllerBase : ApiController
    {
        /// <summary>
        /// Gets the rock request context that describes the current request
        /// being made.
        /// </summary>
        /// <value>
        /// The rock request context that describes the current request
        /// being made.
        /// </value>
        public RockRequestContext RockRequestContext { get; private set; }

        /// <inheritdoc/>
        public override Task<HttpResponseMessage> ExecuteAsync( HttpControllerContext controllerContext, CancellationToken cancellationToken )
        {
            if ( controllerContext.Request.Properties["RockServiceProvider"] is IServiceProvider serviceProvider )
            {
                RockRequestContext = serviceProvider.GetRequiredService<IRockRequestContextAccessor>().RockRequestContext;
            }

            return base.ExecuteAsync( controllerContext, cancellationToken );
        }

        /// <summary>
        /// Gets the currently logged in Person
        /// </summary>
        /// <returns></returns>
        protected virtual Rock.Model.Person GetPerson()
        {
            return GetPerson( null );
        }

        /// <summary>
        /// Gets the currently logged in Person
        /// </summary>
        /// <param name="controller">The ApiController instance that is looking up the current person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        internal static Rock.Model.Person GetPerson( ApiController controller, RockContext rockContext )
        {
            if ( controller.Request.Properties.Keys.Contains( "Person" ) )
            {
                return controller.Request.Properties["Person"] as Person;
            }

            var principal = controller.ControllerContext.Request.GetUserPrincipal();
            if ( principal != null && principal.Identity != null )
            {
                if ( principal.Identity.Name.StartsWith( "rckipid=" ) )
                {
                    var personService = new Model.PersonService( rockContext ?? new RockContext() );
                    Rock.Model.Person impersonatedPerson = personService.GetByImpersonationToken( principal.Identity.Name.Substring( 8 ), false, null );
                    if ( impersonatedPerson != null )
                    {
                        return impersonatedPerson;
                    }
                }
                else
                {
                    var userLoginService = new Rock.Model.UserLoginService( rockContext ?? new RockContext() );
                    var userLogin = userLoginService.GetByUserName( principal.Identity.Name );

                    if ( userLogin != null )
                    {
                        var person = userLogin.Person;
                        controller.Request.Properties.Add( "Person", person );
                        return userLogin.Person;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the currently logged in Person
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        protected virtual Rock.Model.Person GetPerson( RockContext rockContext )
        {
            return GetPerson( this, rockContext );
        }

        /// <summary>
        /// Gets the primary person alias of the currently logged in person
        /// </summary>
        /// <returns></returns>
        protected virtual Rock.Model.PersonAlias GetPersonAlias()
        {
            return GetPersonAlias( null );
        }

        /// <summary>
        /// Gets the primary person alias of the currently logged in person
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        protected virtual Rock.Model.PersonAlias GetPersonAlias( RockContext rockContext )
        {
            var person = GetPerson( rockContext );
            if ( person != null )
            {
                return person.PrimaryAlias;
            }

            return null;
        }

        /// <summary>
        /// Gets the primary person alias ID of the currently logged in person
        /// </summary>
        /// <returns></returns>
        protected virtual int? GetPersonAliasId()
        {
            return GetPersonAliasId( null );
        }

        /// <summary>
        /// Gets the primary person alias ID of the currently logged in person
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        protected virtual int? GetPersonAliasId( RockContext rockContext )
        {
            var currentPersonAlias = GetPersonAlias( rockContext );
            return currentPersonAlias == null ? ( int? ) null : currentPersonAlias.Id;
        }

        /// <summary>
        /// Determines whether the current person has is authorized for the
        /// security action on the current controller action. This will return
        /// <c>false</c> if the current rest action does not have a
        /// <see cref="SystemGuid.RestActionGuidAttribute"/>.
        /// </summary>
        /// <param name="securityAction">The security action to be authorized.</param>
        /// <returns><c>true</c> if the current person has access; otherwise, <c>false</c>.</returns>
        protected bool IsCurrentPersonAuthorized( string securityAction )
        {
            if ( ActionContext.ActionDescriptor is ReflectedHttpActionDescriptor actionDescriptor )
            {
                var restGuid = actionDescriptor.MethodInfo.GetCustomAttribute<SystemGuid.RestActionGuidAttribute>()?.Guid;

                if ( restGuid.HasValue )
                {
                    var restAction = RestActionCache.Get( restGuid.Value );

                    return restAction?.IsAuthorized( securityAction, RockRequestContext.CurrentPerson ) ?? false;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the person may launch workflows of the workflow type.
        /// VIEW on the workflow type (or its category) is the launch right, the same
        /// rule used by the Workflow Entry and Workflow Launch blocks.
        /// </summary>
        /// <param name="workflowType">The workflow type to be launched.</param>
        /// <param name="person">The person launching the workflow, or <c>null</c> for an anonymous caller.</param>
        /// <returns><c>true</c> if the person may launch the workflow type; otherwise, <c>false</c>.</returns>
        protected static bool CanLaunchWorkflowType( Rock.Security.ISecured workflowType, Rock.Model.Person person )
        {
            return workflowType != null && workflowType.IsAuthorized( Rock.Security.Authorization.VIEW, person );
        }

        /// <summary>
        /// The navigation properties of each entity type that can carry related records, cached
        /// because they are read on every POST.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _relatedRecordPropertiesByType = new ConcurrentDictionary<Type, PropertyInfo[]>();

        /// <summary>
        /// Gets the name of the first navigation property on the entity that holds a related
        /// record which is not tracked by the context. Adding the entity to the context would
        /// insert that related record too, without any authorization check of its own.
        /// </summary>
        /// <param name="entity">The posted entity that is about to be added.</param>
        /// <param name="context">The context the entity will be added to, or <c>null</c> if it will be added to a new context.</param>
        /// <param name="isCollectionAllowed">Returns <c>true</c> if the related records in the named collection may be inserted with the entity. May be <c>null</c>.</param>
        /// <returns>The property name, or <c>null</c> if no untracked related records were found.</returns>
        internal static string GetUntrackedRelatedEntityPropertyName( object entity, DbContext context, Func<string, bool> isCollectionAllowed )
        {
            if ( entity == null )
            {
                return null;
            }

            foreach ( var property in GetRelatedRecordProperties( entity.GetType() ) )
            {
                var value = property.GetValue( entity );

                if ( value == null )
                {
                    continue;
                }

                if ( IsMappedEntityType( property.PropertyType ) )
                {
                    if ( !IsTrackedEntity( value, context ) )
                    {
                        return property.Name;
                    }

                    continue;
                }

                var isAllowed = isCollectionAllowed?.Invoke( property.Name ) == true;

                foreach ( var relatedItem in ( ( IEnumerable ) value ).Cast<object>() )
                {
                    if ( relatedItem == null || IsTrackedEntity( relatedItem, context ) )
                    {
                        continue;
                    }

                    if ( !isAllowed )
                    {
                        return property.Name;
                    }

                    // An allowed child may still not carry related records of its own.
                    var childPropertyName = GetUntrackedRelatedEntityPropertyName( relatedItem, context, null );
                    if ( childPropertyName != null )
                    {
                        return $"{property.Name}.{childPropertyName}";
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether the property is a navigation property that holds a related
        /// record or a collection of related records.
        /// </summary>
        /// <remarks>
        /// This is broader than <see cref="Utility.ExcludeNavigationPropertiesContractResolver.IsNavigationPropertyType(Type)"/>,
        /// which only looks for <see cref="IEntity"/> and cache types.
        /// </remarks>
        /// <param name="property">The property.</param>
        /// <returns><c>true</c> if the property is a navigation property; otherwise <c>false</c>.</returns>
        internal static bool IsNavigationProperty( PropertyInfo property )
        {
            if ( property == null || !property.CanWrite || property.GetIndexParameters().Length != 0 )
            {
                return false;
            }

            return IsMappedEntityType( property.PropertyType )
                || IsMappedEntityType( GetCollectionItemType( property.PropertyType ) );
        }

        /// <summary>
        /// Determines whether the type is a database entity, which EF inserts when it is
        /// reachable from an entity being added.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the type is a database entity; otherwise <c>false</c>.</returns>
        internal static bool IsMappedEntityType( Type type )
        {
            /*
                10/15/2026 - MSE

                Some mapped tables (such as GroupLocationScheduleConfig and AnalyticsSourceDate)
                do not implement IEntity, so any class with a [Table] attribute is also treated
                as an entity. EF metadata is not used because the v2 API has no context at this
                point and the check must stay cheap.

                Reason: Catch every related record that EF would insert.
            */
            return type != null
                && type.IsClass
                && ( typeof( IEntity ).IsAssignableFrom( type ) || System.Attribute.IsDefined( type, typeof( TableAttribute ), true ) );
        }

        /// <summary>
        /// Gets the navigation properties of the entity type that are saved to the database.
        /// </summary>
        /// <param name="type">The entity type.</param>
        /// <returns>The navigation properties.</returns>
        private static PropertyInfo[] GetRelatedRecordProperties( Type type )
        {
            return _relatedRecordPropertiesByType.GetOrAdd( type, t => t.GetProperties( BindingFlags.Public | BindingFlags.Instance )
                .Where( p => IsNavigationProperty( p ) && !System.Attribute.IsDefined( p, typeof( NotMappedAttribute ), true ) )
                .ToArray() );
        }

        /// <summary>
        /// Gets the item type of a generic collection or array type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The item type, or <c>null</c> if the type is not a collection.</returns>
        private static Type GetCollectionItemType( Type type )
        {
            if ( type.IsArray )
            {
                return type.GetElementType();
            }

            if ( type.IsGenericType && typeof( IEnumerable ).IsAssignableFrom( type ) && type.GetGenericArguments().Length == 1 )
            {
                return type.GetGenericArguments()[0];
            }

            return null;
        }

        /// <summary>
        /// Determines whether the entity is tracked by the context, which means adding a
        /// related entity will not insert it.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="context">The context, or <c>null</c>.</param>
        /// <returns><c>true</c> if the entity is tracked by the context; otherwise <c>false</c>.</returns>
        private static bool IsTrackedEntity( object entity, DbContext context )
        {
            return context != null && context.Entry( entity ).State != System.Data.Entity.EntityState.Detached;
        }

        #region .NET Core compatible methods

        /// <summary>
        /// Returns a response object to indicate that no content was generated.
        /// </summary>
        /// <returns>The response object.</returns>
        protected IActionResult NoContent()
        {
            return new StatusCodeResult( HttpStatusCode.NoContent, this );
        }

        /// <summary>
        /// Creates a not found response with the given error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>The response object.</returns>
        protected IActionResult NotFound( string errorMessage )
        {
            var error = new HttpError( errorMessage );

            return new NegotiatedContentResult<HttpError>( HttpStatusCode.NotFound, error, this );
        }

        /// <summary>
        /// Creates an unauthorized response with the given error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>The response object.</returns>
        protected IActionResult Unauthorized( string errorMessage )
        {
            var error = new HttpError( errorMessage );

            return new NegotiatedContentResult<HttpError>( HttpStatusCode.Unauthorized, error, this );
        }

        /// <summary>
        /// Creates an internals erver error response with the given error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>The response object.</returns>
        protected IActionResult InternalServerError( string errorMessage )
        {
            var error = new HttpError( errorMessage );

            return new NegotiatedContentResult<HttpError>( HttpStatusCode.InternalServerError, error, this );
        }

        #endregion
    }
}
