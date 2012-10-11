//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Linq;
using System.Net;
using System.Web.Http;

using Rock.Core;

namespace Rock.Rest.Core
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
				routeTemplate: "api/tags/availablenames/{entity}/{ownerid}/{entityid}/{entityqualifier}/{entityqualifiervalue}",
				defaults: new
				{
					controller = "tags",
					action = "availablenames",
					entityqualifier = RouteParameter.Optional,
					entityqualifiervalue = RouteParameter.Optional
				} );

			routes.MapHttpRoute(
				name: "TagsByEntity",
				routeTemplate: "api/tags/{entity}/{ownerid}/{name}/{entityqualifier}/{entityqualifiervalue}",
				defaults: new
				{
					controller = "tags",
					entityqualifier = RouteParameter.Optional,
					entityqualifiervalue = RouteParameter.Optional
				} );
		}

		[HttpGet]
		public TagDto Get( string entity, int ownerId, string name )
		{
			return Get( entity, ownerId, name, string.Empty, string.Empty );
		}

		[HttpGet]
		public TagDto Get( string entity, int ownerId, string name, string entityQualifier )
		{
			return Get( entity, ownerId, name, entityQualifier, string.Empty );
		}

		[HttpGet]
		public TagDto Get( string entity, int ownerId, string name, string entityQualifier, string entityQualifierValue )
		{
			var service = new TagService();
			var dto = service.QueryableDto( service.GetByEntity( entity, entityQualifier, entityQualifierValue, ownerId ) )
				.Where( t => t.Name == name)
				.FirstOrDefault();

			if ( dto != null )
				return dto;
			else
				throw new HttpResponseException( HttpStatusCode.NotFound );
		}

		[HttpGet]
		public IQueryable<TagDto> AvailableNames( string entity, int ownerId, int entityId )
		{
			return AvailableNames( entity, ownerId, entityId, string.Empty, string.Empty );
		}

		[HttpGet]
		public IQueryable<TagDto> AvailableNames( string entity, int ownerId, int entityId, string entityQualifier )
		{
			return AvailableNames( entity, ownerId, entityId, entityQualifier, string.Empty );
		}

		[HttpGet]
		public IQueryable<TagDto> AvailableNames( string entity, int ownerId, int entityId, string entityQualifier, string entityQualifierValue )
		{
			var service = new TagService();
			var items = service.GetByEntity( entity, entityQualifier, entityQualifierValue, ownerId )
				.Where( t => t.TaggedItems.Select( i => i.EntityId ).Contains( entityId ) == false )
				.OrderBy( t => t.Name );
			return service.QueryableDto( items );
		}

	}
}
