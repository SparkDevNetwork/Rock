//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Linq;
using System.Net;
using System.Web.Http;

using Rock.Model;

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
                routeTemplate: "api/tags/availablenames/{entityTypeId}/{ownerid}/{entityguid}/{entityqualifier}/{entityqualifiervalue}",
                defaults: new
                {
                    controller = "tags",
                    action = "availablenames",
                    entityqualifier = RouteParameter.Optional,
                    entityqualifiervalue = RouteParameter.Optional
                } );

            routes.MapHttpRoute(
                name: "TagsByEntity",
                routeTemplate: "api/tags/{entityTypeId}/{ownerid}/{name}/{entityqualifier}/{entityqualifiervalue}",
                defaults: new
                {
                    controller = "tags",
                    entityqualifier = RouteParameter.Optional,
                    entityqualifiervalue = RouteParameter.Optional
                } );
        }

        [HttpGet]
        public Tag Get( int entityTypeId, int ownerId, string name )
        {
            return Get( entityTypeId, ownerId, name, string.Empty, string.Empty );
        }

        [HttpGet]
        public Tag Get( int entityTypeId, int ownerId, string name, string entityQualifier )
        {
            return Get( entityTypeId, ownerId, name, entityQualifier, string.Empty );
        }

        [HttpGet]
        public Tag Get( int entityTypeId, int ownerId, string name, string entityQualifier, string entityQualifierValue )
        {
            var service = new TagService();
            var tag = service.Get( entityTypeId, entityQualifier, entityQualifierValue, ownerId ).FirstOrDefault(t => t.Name == name);

            if ( tag != null )
                return tag;
            else
                throw new HttpResponseException( HttpStatusCode.NotFound );
        }

        [HttpGet]
        public IQueryable<Tag> AvailableNames( int entityTypeId, int ownerId, Guid entityGuid )
        {
            return AvailableNames( entityTypeId, ownerId, entityGuid, string.Empty, string.Empty );
        }

        [HttpGet]
        public IQueryable<Tag> AvailableNames( int entityTypeId, int ownerId, Guid entityGuid, string entityQualifier )
        {
            return AvailableNames( entityTypeId, ownerId, entityGuid, entityQualifier, string.Empty );
        }

        [HttpGet]
        public IQueryable<Tag> AvailableNames( int entityTypeId, int ownerId, Guid entityGuid, string entityQualifier, string entityQualifierValue )
        {
            var service = new TagService();
            return service.Get( entityTypeId, entityQualifier, entityQualifierValue, ownerId )
                .Where( t => t.TaggedItems.Select( i => i.EntityGuid ).Contains( entityGuid ) == false )
                .OrderBy( t => t.Name );
        }

    }
}
