//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Net;
using System.Web.Http;

using Rock.Cms;
using Rock.Rest.Filters;

namespace Rock.Rest.Cms
{
	/// <summary>
	/// BlockInstances REST API
	/// </summary>
	public partial class BlockInstancesController : IHasCustomRoutes 
	{
		/// <summary>
		/// Add Custom route needed for block move
		/// </summary>
		/// <param name="routes"></param>
		public void AddRoutes( System.Web.Routing.RouteCollection routes )
		{
			routes.MapHttpRoute(
				name: "BlockInstanceMove",
				routeTemplate: "api/blockinstances/move/{id}",
				defaults: new
				{
					controller = "blockinstances",
					action = "move"
				} );
		}

		/// <summary>
		/// Moves a block instance from one zone to another
		/// </summary>
		/// <param name="blockInstance"></param>
		/// <returns></returns>
		[HttpPut]
		[Authenticate]
		public void Move( int id, BlockInstanceDto blockInstance )
		{
			var user = CurrentUser();
			if ( user != null )
			{
				var service = new BlockInstanceService();
				BlockInstance model;
				if ( !service.TryGet( id, out model ) )
					throw new HttpResponseException( HttpStatusCode.NotFound );

				if ( !model.IsAuthorized( "Edit", user.Person ) )
					throw new HttpResponseException( HttpStatusCode.Unauthorized );

				if ( model.IsValid )
				{
					if ( model.Layout != null && model.Layout != blockInstance.Layout )
						Rock.Web.Cache.Page.FlushLayoutBlockInstances( model.Layout );

					if (blockInstance.Layout != null)
						Rock.Web.Cache.Page.FlushLayoutBlockInstances( blockInstance.Layout);
					else
					{
						var page = Rock.Web.Cache.Page.Read( blockInstance.PageId.Value );
						page.FlushBlockInstances();
					}

					blockInstance.CopyToModel( model );

					service.Move( model );
					service.Save( model, user.PersonId );
				}
				else
					throw new HttpResponseException( HttpStatusCode.BadRequest );
			}
			else
				throw new HttpResponseException( HttpStatusCode.Unauthorized );
		}
	}
}
