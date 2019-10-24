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

using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// TaggedItems REST API
    /// </summary>
    public partial class TagsController 
    {
        /// <summary>
        /// GET a specific Tag
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="entityQualifier">The entity qualifier.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="categoryGuid">The category unique identifier.</param>
        /// <param name="includeInactive">The include inactive.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpGet]
        public Tag Get( int entityTypeId, int ownerId, string name, string entityQualifier = null, string entityQualifierValue = null, string categoryGuid = null, bool? includeInactive = false )
        {
            string tagName = WebUtility.UrlDecode( name );
            var tag = ( ( TagService ) Service ).Get( entityTypeId, entityQualifier, entityQualifierValue, ownerId, name, categoryGuid.AsGuidOrNull(), includeInactive );

            if ( tag == null || !tag.IsAuthorized( Rock.Security.Authorization.TAG, GetPerson() ) )
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
        /// <param name="entityQualifier">The entity qualifier.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="categoryGuid">The category unique identifier.</param>
        /// <param name="includeInactive">The include inactive.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/Tags/AvailableNames" )]
        public IQueryable<Tag> AvailableNames( int entityTypeId, int ownerId, Guid entityGuid, string name = null, string entityQualifier = null, string entityQualifierValue = null, Guid? categoryGuid = null, bool? includeInactive = null )
        {
            var tags = ( ( TagService ) Service )
                .Get( entityTypeId, entityQualifier, entityQualifierValue, ownerId, categoryGuid, includeInactive )
                .Where( t =>
                    t.Name.StartsWith( name ) &&
                    !t.TaggedItems.Any( i => i.EntityGuid == entityGuid ) && 
                    t.IsActive );

            if ( categoryGuid.HasValue )
            {
                var category = CategoryCache.Get( categoryGuid.Value );
                if ( category != null )
                {
                    tags = tags
                        .Where( a => 
                            a.CategoryId.HasValue && 
                            a.CategoryId.Value == category.Id );
                }
            }

            var person = GetPerson();
            var tagItems = new List<Tag>();

            foreach ( var tag in tags.OrderBy( t => t.Name ) )
            {
                if ( tag.IsAuthorized( Rock.Security.Authorization.TAG, person ) )
                {
                    tagItems.Add( tag );
                }
            }

            return tagItems.AsQueryable<Tag>();
        }
    }
}
