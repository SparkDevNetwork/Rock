// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Net.Http;
using System.Web.Http;

using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// TaggedItems REST API
    /// </summary>
    public partial class TaggedItemsController : IHasCustomRoutes
    {
        /// <summary>
        /// Add Custom route for flushing cached attributes
        /// </summary>
        /// <param name="routes"></param>
        public void AddRoutes( System.Web.Routing.RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "TaggedItemsByEntity",
                routeTemplate: "api/taggeditems/{entityTypeId}/{ownerid}/{entityguid}/{name}/{entityqualifier}/{entityqualifiervalue}",
                defaults: new
                {
                    controller = "taggeditems",
                    entityqualifier = RouteParameter.Optional,
                    entityqualifiervalue = RouteParameter.Optional
                } );
        }

        [Authenticate, Secured]
        public HttpResponseMessage Post( int entityTypeId, int ownerId, Guid entityGuid, string name )
        {
            return Post( entityTypeId, ownerId, entityGuid, name, string.Empty, string.Empty );
        }

        [Authenticate, Secured]
        public HttpResponseMessage Post( int entityTypeId, int ownerId, Guid entityGuid, string name, string entityQualifier )
        {
            return Post( entityTypeId, ownerId, entityGuid, name, entityQualifier, string.Empty );
        }

        [Authenticate, Secured]
        public HttpResponseMessage Post( int entityTypeId, int ownerId, Guid entityGuid, string name, string entityQualifier, string entityQualifierValue )
        {
            var personAlias = GetPersonAlias();

            using ( new Rock.Data.UnitOfWorkScope() )
            {
                var tagService = new TagService();
                var taggedItemService = new TaggedItemService();

                var tag = tagService.Get( entityTypeId, entityQualifier, entityQualifierValue, ownerId, name );
                if ( tag == null )
                {
                    tag = new Tag();
                    tag.EntityTypeId = entityTypeId;
                    tag.EntityTypeQualifierColumn = entityQualifier;
                    tag.EntityTypeQualifierValue = entityQualifierValue;
                    tag.OwnerId = ownerId;
                    tag.Name = name;
                    tagService.Add( tag, personAlias );
                    tagService.Save( tag, personAlias );
                }

                var taggedItem = taggedItemService.Get( tag.Id, entityGuid );
                if ( taggedItem == null )
                {
                    taggedItem = new TaggedItem();
                    taggedItem.TagId = tag.Id;
                    taggedItem.EntityGuid = entityGuid;
                    taggedItemService.Add( taggedItem, personAlias );
                    taggedItemService.Save( taggedItem, personAlias );
                }
            }

            return ControllerContext.Request.CreateResponse( HttpStatusCode.Created );
        }

        [Authenticate, Secured]
        public void Delete( int entityTypeId, int ownerId, Guid entityGuid, string name )
        {
            Delete( entityTypeId, ownerId, entityGuid, name, string.Empty, string.Empty );
        }

        [Authenticate, Secured]
        public void Delete( int entityTypeId, int ownerId, Guid entityGuid, string name, string entityQualifier )
        {
            Delete( entityTypeId, ownerId, entityGuid, name, entityQualifier, string.Empty );
        }

        [Authenticate, Secured]
        public void Delete( int entityTypeId, int ownerId, Guid entityGuid, string name, string entityQualifier, string entityQualifierValue )
        {
            var personAlias = GetPersonAlias();

            using ( new Rock.Data.UnitOfWorkScope() )
            {
                var tagService = new TagService();
                var taggedItemService = new TaggedItemService();

                if ( name.Contains( '^' ) )
                    name = name.Split( '^' )[0];

                var tag = tagService.Get( entityTypeId, entityQualifier, entityQualifierValue, ownerId, name );
                if ( tag == null )
                    throw new HttpResponseException( HttpStatusCode.NotFound );

                var taggedItem = taggedItemService.Get( tag.Id, entityGuid );
                if ( taggedItem == null )
                    throw new HttpResponseException( HttpStatusCode.NotFound );

                taggedItemService.Delete( taggedItem, personAlias );
                taggedItemService.Save( taggedItem, personAlias );
            }
        }

    }
}
