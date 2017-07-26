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
using System.Linq;
using System.Net;
using System.Web.Http;

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
            var tag = ( (TagService)Service )
                .Get( entityTypeId, entityQualifier, entityQualifierValue, ownerId ).FirstOrDefault( t => t.Name == tagName );

            if ( tag != null )
            {
                return tag;
            }
            else
            {
                // NOTE: This exception is expected when adding a new Tag.  The Javascript responds to the NotFound error by prompting them to create a new tag
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }
        }

        /// <summary>
        /// Queryable GET of Tags
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        public IQueryable<Tag> AvailableNames( int entityTypeId, int ownerId, Guid entityGuid, string name )
        {
            return AvailableNames( entityTypeId, ownerId, entityGuid, name, string.Empty, string.Empty );
        }

        /// <summary>
        /// Queryable GET of Tags
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="entityQualifier">The entity qualifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        public IQueryable<Tag> AvailableNames( int entityTypeId, int ownerId, Guid entityGuid, string name, string entityQualifier )
        {
            return AvailableNames( entityTypeId, ownerId, entityGuid, name, entityQualifier, string.Empty );
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
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        public IQueryable<Tag> AvailableNames( int entityTypeId, int ownerId, Guid entityGuid, string name, string entityQualifier, string entityQualifierValue )
        {
            return ( (TagService)Service )
                .Get( entityTypeId, entityQualifier, entityQualifierValue, ownerId )
                .Where( t =>
                    t.Name.StartsWith( name ) &&
                    !t.TaggedItems.Any( i => i.EntityGuid == entityGuid ) )
                .OrderBy( t => t.Name );
        }
    }
}
