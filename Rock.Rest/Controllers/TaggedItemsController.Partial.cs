//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
                routeTemplate: "api/taggeditems/{entityTypeId}/{ownerid}/{entityid}/{name}/{entityqualifier}/{entityqualifiervalue}",
                defaults: new
                {
                    controller = "taggeditems",
                    entityqualifier = RouteParameter.Optional,
                    entityqualifiervalue = RouteParameter.Optional
                } );
        }

        [Authenticate]
        public HttpResponseMessage Post( int entityTypeId, int ownerId, int entityId, string name )
        {
            return Post( entityTypeId, ownerId, entityId, name, string.Empty, string.Empty );
        }

        [Authenticate]
        public HttpResponseMessage Post( int entityTypeId, int ownerId, int entityId, string name, string entityQualifier )
        {
            return Post( entityTypeId, ownerId, entityId, name, entityQualifier, string.Empty );
        }

        [Authenticate]
        public HttpResponseMessage Post( int entityTypeId, int ownerId, int entityId, string name, string entityQualifier, string entityQualifierValue )
        {
            var user = CurrentUser();
            if ( user != null )
            {
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
                        tagService.Add( tag, user.PersonId );
                        tagService.Save( tag, user.PersonId );
                    }

                    var taggedItem = taggedItemService.Get( tag.Id, entityId );
                    if ( taggedItem == null )
                    {
                        taggedItem = new TaggedItem();
                        taggedItem.TagId = tag.Id;
                        taggedItem.EntityId = entityId;
                        taggedItemService.Add( taggedItem, user.PersonId );
                        taggedItemService.Save( taggedItem, user.PersonId );
                    }
                }

                return ControllerContext.Request.CreateResponse( HttpStatusCode.Created );
            }

            throw new HttpResponseException( HttpStatusCode.Unauthorized );
        }

        [Authenticate]
        public void Delete( int entityTypeId, int ownerId, int entityId, string name )
        {
            Delete( entityTypeId, ownerId, entityId, name, string.Empty, string.Empty );
        }

        [Authenticate]
        public void Delete( int entityTypeId, int ownerId, int entityId, string name, string entityQualifier )
        {
            Delete( entityTypeId, ownerId, entityId, name, entityQualifier, string.Empty );
        }

        [Authenticate]
        public void Delete( int entityTypeId, int ownerId, int entityId, string name, string entityQualifier, string entityQualifierValue )
        {
            var user = CurrentUser();
            if ( user != null )
            {
                using ( new Rock.Data.UnitOfWorkScope() )
                {
                    var tagService = new TagService();
                    var taggedItemService = new TaggedItemService();

                    if ( name.Contains( '^' ) )
                        name = name.Split( '^' )[0];

                    var tag = tagService.Get( entityTypeId, entityQualifier, entityQualifierValue, ownerId, name );
                    if ( tag == null )
                        throw new HttpResponseException( HttpStatusCode.NotFound );

                    var taggedItem = taggedItemService.Get( tag.Id, entityId );
                    if ( taggedItem == null )
                        throw new HttpResponseException( HttpStatusCode.NotFound );

                    taggedItemService.Delete( taggedItem, user.PersonId );
                    taggedItemService.Save( taggedItem, user.PersonId );
                }
            }
            else
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
        }

    }
}
