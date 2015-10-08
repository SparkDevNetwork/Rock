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
using System.Collections.ObjectModel;
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
                routeTemplate: "api/taggeditems/{entityTypeId}/{ownerid}/{entityguid}/{entityqualifier}/{entityqualifiervalue}",
                defaults: new
                {
                    controller = "taggeditems",
                    entityqualifier = RouteParameter.Optional,
                    entityqualifiervalue = RouteParameter.Optional
                } );
        }

        /// <summary>
        /// Posts the specified entity type identifier.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        public HttpResponseMessage Post( int entityTypeId, int ownerId, Guid entityGuid, string name )
        {
            return Post( entityTypeId, ownerId, entityGuid, name, string.Empty, string.Empty );
        }

        /// <summary>
        /// Posts the specified entity type identifier.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="entityQualifier">The entity qualifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        public HttpResponseMessage Post( int entityTypeId, int ownerId, Guid entityGuid, string name, string entityQualifier )
        {
            return Post( entityTypeId, ownerId, entityGuid, name, entityQualifier, string.Empty );
        }

        /// <summary>
        /// Posts the specified entity type identifier.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="entityQualifier">The entity qualifier.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        public HttpResponseMessage Post( int entityTypeId, int ownerId, Guid entityGuid, string name, string entityQualifier, string entityQualifierValue )
        {
            SetProxyCreation( true );

            var personAlias = GetPersonAlias();

            var tagService = new TagService( (Rock.Data.RockContext)Service.Context );

            var tag = tagService.Get( entityTypeId, entityQualifier, entityQualifierValue, ownerId, name );
            if ( tag == null )
            {
                tag = new Tag();
                tag.EntityTypeId = entityTypeId;
                tag.EntityTypeQualifierColumn = entityQualifier;
                tag.EntityTypeQualifierValue = entityQualifierValue;
                tag.OwnerPersonAliasId = new PersonAliasService( (Rock.Data.RockContext)Service.Context ).GetPrimaryAliasId( ownerId );
                tag.Name = name;
                tagService.Add( tag );
            }

            tag.TaggedItems = tag.TaggedItems ?? new Collection<TaggedItem>();

            var taggedItem = tag.TaggedItems.FirstOrDefault( i => i.EntityGuid.Equals( entityGuid ) );
            if ( taggedItem == null )
            {
                taggedItem = new TaggedItem();
                taggedItem.Tag = tag;
                taggedItem.EntityGuid = entityGuid;
                tag.TaggedItems.Add( taggedItem );
            }

            System.Web.HttpContext.Current.Items.Add( "CurrentPerson", GetPerson() );
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
        [Authenticate, Secured]
        public void Delete( int entityTypeId, int ownerId, Guid entityGuid, string name )
        {
            Delete( entityTypeId, ownerId, entityGuid, name, string.Empty, string.Empty );
        }

        /// <summary>
        /// Deletes the specified entity type identifier.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="entityQualifier">The entity qualifier.</param>
        [Authenticate, Secured]
        public void Delete( int entityTypeId, int ownerId, Guid entityGuid, string name, string entityQualifier )
        {
            Delete( entityTypeId, ownerId, entityGuid, name, entityQualifier, string.Empty );
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
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        public void Delete( int entityTypeId, int ownerId, Guid entityGuid, string name, string entityQualifier, string entityQualifierValue )
        {
            SetProxyCreation( true );

            var personAlias = GetPersonAlias();

            if ( name.Contains( '^' ) )
            {
                name = name.Split( '^' )[0];
            }

            var taggedItem = Service.Queryable()
                .FirstOrDefault( i =>
                    ( i.Tag.EntityTypeId == entityTypeId ) &&
                    ( i.Tag.EntityTypeQualifierColumn == null || i.Tag.EntityTypeQualifierColumn == string.Empty || i.Tag.EntityTypeQualifierColumn == entityQualifier ) &&
                    ( i.Tag.EntityTypeQualifierValue == null || i.Tag.EntityTypeQualifierValue == string.Empty || i.Tag.EntityTypeQualifierValue == entityQualifierValue ) &&
                    ( i.Tag.OwnerPersonAlias == null || i.Tag.OwnerPersonAlias.PersonId == ownerId ) &&
                    ( i.Tag.Name == name ) &&
                    i.EntityGuid.Equals( entityGuid ) );

            if ( taggedItem == null )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            CheckCanEdit( taggedItem );

            Service.Delete( taggedItem );

            Service.Context.SaveChanges();
        }
    }
}