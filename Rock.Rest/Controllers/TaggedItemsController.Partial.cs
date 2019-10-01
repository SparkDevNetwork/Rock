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
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// TaggedItems REST API
    /// </summary>
    public partial class TaggedItemsController
    {

        /// <summary>
        /// Posts the specified entity type identifier.
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
        public HttpResponseMessage Post( int entityTypeId, int ownerId, Guid entityGuid, string name, string entityQualifier = null, string entityQualifierValue = null, Guid? categoryGuid = null, bool? includeInactive = null )
        {
            SetProxyCreation( true );

            var person = GetPerson( ( Rock.Data.RockContext ) Service.Context );

            var tagService = new TagService( ( Rock.Data.RockContext ) Service.Context );

            var tag = tagService.Get( entityTypeId, entityQualifier, entityQualifierValue, ownerId, name, categoryGuid, includeInactive );
            if ( tag == null || !tag.IsAuthorized( Rock.Security.Authorization.TAG, person ) )
            {
                int? categoryId = null;
                if ( categoryGuid.HasValue )
                {
                    var category = CategoryCache.Get( categoryGuid.Value );
                    categoryId = category != null ? category.Id : (int?)null;
                }

                tag = new Tag();
                tag.EntityTypeId = entityTypeId;
                tag.CategoryId = categoryId;
                tag.EntityTypeQualifierColumn = entityQualifier;
                tag.EntityTypeQualifierValue = entityQualifierValue;
                tag.OwnerPersonAliasId = new PersonAliasService( ( Rock.Data.RockContext ) Service.Context ).GetPrimaryAliasId( ownerId );
                tag.Name = name;
                tagService.Add( tag );
            }

            tag.TaggedItems = tag.TaggedItems ?? new Collection<TaggedItem>();

            var taggedItem = tag.TaggedItems.FirstOrDefault( i => i.EntityGuid.Equals( entityGuid ) );
            if ( taggedItem == null )
            {
                taggedItem = new TaggedItem();
                taggedItem.Tag = tag;
                taggedItem.EntityTypeId = entityTypeId;
                taggedItem.EntityGuid = entityGuid;
                tag.TaggedItems.Add( taggedItem );
            }

            System.Web.HttpContext.Current.Items.Add( "CurrentPerson", person );
            Service.Context.SaveChanges();

            return ControllerContext.Request.CreateResponse( HttpStatusCode.Created, tag.Id );
        }

        /// <summary>
        /// Deletes the specified entity type identifier.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="entityQualifier">The entity qualifier.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="categoryGuid">The category unique identifier.</param>
        /// <param name="includeInactive">The include inactive.</param>
        /// <exception cref="HttpResponseException">
        /// </exception>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        public void Delete( int entityTypeId, int ownerId, Guid entityGuid, string name, string entityQualifier = null, string entityQualifierValue = null, Guid? categoryGuid = null, bool? includeInactive = null )
        {
            SetProxyCreation( true );

            if ( name.Contains( '^' ) )
            {
                name = name.Split( '^' )[0];
            }

            TaggedItem taggedItem = null;

            var tagService = new TagService( ( Rock.Data.RockContext ) Service.Context );
            var tag = tagService.Get( entityTypeId, entityQualifier, entityQualifierValue, ownerId, name, categoryGuid, includeInactive );
            if ( tag != null )
            {
                taggedItem = tag.TaggedItems.Where( i => i.EntityGuid == entityGuid ).FirstOrDefault();
            }

            if ( taggedItem == null )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            if ( !taggedItem.IsAuthorized( Rock.Security.Authorization.TAG, GetPerson( ( Rock.Data.RockContext ) Service.Context ) ))
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }

            Service.Delete( taggedItem );

            Service.Context.SaveChanges();
        }
    }
}