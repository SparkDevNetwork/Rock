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
using System.Web.Http;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// TaggedItems REST API
    /// </summary>
    public partial class TagsController : IHasCustomRoutes
    {
        /// <summary>
        /// Add Custom route for flushing cached attributes
        /// </summary>
        /// <param name="routes"></param>
        public void AddRoutes( System.Web.Routing.RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "TagNamesAvail",
                routeTemplate: "api/tags/availablenames/{entityTypeId}/{ownerid}/{name}/{entityguid}/{entityqualifier}/{entityqualifiervalue}",
                defaults: new
                {
                    controller = "tags",
                    action = "availablenames",
                    entityqualifier = RouteParameter.Optional,
                    entityqualifiervalue = RouteParameter.Optional
                } );

            routes.MapHttpRoute(
                name: "TagsByEntityName",
                routeTemplate: "api/tags/{entityTypeId}/{ownerid}/{entityqualifier}/{entityqualifiervalue}",
                defaults: new
                {
                    controller = "tags",
                    entityqualifier = RouteParameter.Optional,
                    entityqualifiervalue = RouteParameter.Optional
                } );
        }

        /// <summary>
        /// GET a specific Tag
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        public Tag Get( int entityTypeId, int ownerId, string name )
        {
            return Get( entityTypeId, ownerId, name, string.Empty, string.Empty );
        }

        /// <summary>
        /// GET a specific Tag
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="entityQualifier">The entity qualifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        public Tag Get( int entityTypeId, int ownerId, string name, string entityQualifier )
        {
            return Get( entityTypeId, ownerId, name, entityQualifier, string.Empty );
        }

        /// <summary>
        /// GET a specific Tag
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="entityQualifier">The entity qualifier.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpGet]
        public Tag Get( int entityTypeId, int ownerId, string name, string entityQualifier, string entityQualifierValue )
        {
            string tagName = WebUtility.UrlDecode( name );
            var tag = ( ( TagService ) Service ).Get( entityTypeId, entityQualifier, entityQualifierValue, ownerId, name );

            if ( tag == null )
            {
                // NOTE: This exception is expected when adding a new Tag.  The Javascript responds to the NotFound error by prompting them to create a new tag
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            return tag;

        }

        /// <summary>
        /// Queryable GET of Tags
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="categoryIds">The delimited list of category id </param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        public IQueryable<Tag> AvailableNames( int entityTypeId, int ownerId, Guid entityGuid, string name, string categoryIds=null )
        {
            return AvailableNames( entityTypeId, ownerId, entityGuid, name, string.Empty, string.Empty, categoryIds );
        }

        /// <summary>
        /// Queryable GET of Tags
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="entityQualifier">The entity qualifier.</param>
        /// <param name="categoryIds">The delimited list of category id </param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        public IQueryable<Tag> AvailableNames( int entityTypeId, int ownerId, Guid entityGuid, string name, string entityQualifier, string categoryIds = null )
        {
            return AvailableNames( entityTypeId, ownerId, entityGuid, name, entityQualifier, string.Empty, categoryIds );
        }

        /// <summary>
        /// Queryable GET of Tags
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="entityQualifier">The entity qualifier.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="categoryIds">The delimited list of category id </param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        public IQueryable<Tag> AvailableNames( int entityTypeId, int ownerId, Guid entityGuid, string name, string entityQualifier, string entityQualifierValue, string categoryIds = null )
        {
            var tags = ( ( TagService ) Service )
                .Get( entityTypeId, entityQualifier, entityQualifierValue, ownerId )
                .Where( t =>
                    t.Name.StartsWith( name ) &&
                    !t.TaggedItems.Any( i => i.EntityGuid == entityGuid ) && t.IsActive );

            var categoryGuids = new List<Guid>();
            if ( !string.IsNullOrEmpty( categoryIds ) )
            {
                categoryGuids = categoryIds.SplitDelimitedValues().AsGuidList();

                var cateogryInts = new CategoryService( this.Service.Context as RockContext )
                        .GetByGuids( categoryGuids )
                        .Select( a => a.Id ).ToList();

                tags = tags.Where( a => a.CategoryId.HasValue && cateogryInts.Contains( a.CategoryId.Value ) );
            }

            var person = GetPerson();
            var tagItems = new List<Tag>();

            foreach ( var tag in tags.OrderBy( t => t.Name ) )
            {
                if ( tag.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                {
                    tagItems.Add( tag );
                }
            }

            return tagItems.AsQueryable<Tag>();
        }
    }
}
