//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web.Http;

using Rock.Data;

namespace Rock.Rest
{
	public abstract class ApiController<T, D> : ApiController
		where T : Rock.Data.Model<T>
		where D : Rock.Data.Dto<T>, new()
	{
		private Service<T, D> _service;

		public ApiController( Service<T, D> service )
		{
			_service = service;
		}

		// GET api/<controller>
		public IEnumerable<D> Get()
		{
			return _service.QueryableDto().ToList();
		}

		// GET api/<controller>/5
		public D Get( int id )
		{
			T model;
			if ( !_service.TryGet( id, out model ) )
				throw new HttpResponseException( HttpStatusCode.NotFound );
			var dto = new D();
			dto.CopyFromModel( model );
			return dto;
		}

		// POST api/<controller>
		public HttpResponseMessage Post( [FromBody]D value )
		{
			var user = CurrentUser();
			if ( user != null )
			{
				T model = _service.CreateNew();
				_service.Add( model, null );

				value.CopyToModel( model );

				if ( !model.IsValid )
					return ControllerContext.Request.CreateErrorResponse(
						HttpStatusCode.BadRequest,
						String.Join( ",", model.ValidationResults.Select( r => r.ErrorMessage ).ToArray() ) );

				_service.Save( model, user.PersonId );

				var response = ControllerContext.Request.CreateResponse( HttpStatusCode.Created );
				//response.Headers.Location = new Uri( Request.RequestUri, "/api/pages/" + page.Id.ToString() );
				return response;
			}

			throw new HttpResponseException( HttpStatusCode.Unauthorized );
		}

		// PUT api/<controller>/5
		public void Put( int id, [FromBody]D value )
		{
			var user = CurrentUser();
			if ( user != null )
			{
				var service = new Service<T, D>();
				T model;
				if ( !service.TryGet( id, out model ) )
					throw new HttpResponseException( HttpStatusCode.NotFound );

				value.CopyToModel( model );

				if ( model.IsValid )
					service.Save( model, user.PersonId );
				else

					throw new HttpResponseException( HttpStatusCode.BadRequest );
			}

			throw new HttpResponseException( HttpStatusCode.Unauthorized );
		}

		// DELETE api/<controller>/5
		public void Delete( int id )
		{
			var user = CurrentUser();
			if ( user != null )
			{
				T model;
				if ( !_service.TryGet( id, out model ) )
					throw new HttpResponseException( HttpStatusCode.NotFound );

				_service.Delete( model, user.PersonId );
				_service.Save( model, user.PersonId );
			}

			throw new HttpResponseException( HttpStatusCode.Unauthorized );
		}

		protected Rock.Cms.User CurrentUser()
		{
			var principal = ControllerContext.Request.GetUserPrincipal();
			if ( principal != null && principal.Identity != null )
			{
				var userService = new Rock.Cms.UserService();
				var user = userService.GetByUserName( principal.Identity.Name );

				if ( user != null )
					return user;
			}

			return null;
		}
	}
}
