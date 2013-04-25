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
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest
{
    public abstract class ApiController<T> : ApiController 
        where T : Rock.Data.Entity<T>, new()
    {
        private Service<T> _service;

        public ApiController( Service<T> service )
        {
            _service = service;
            _service.Repository.SetConfigurationValue( "ProxyCreationEnabled", "false" );
        }

        // GET api/<controller>
        [Queryable]
        public virtual IQueryable<T> Get()
        {
            var result = _service.Queryable();
            return result;
        }

        // GET api/<controller>/5
        public virtual T Get( int id )
        {
            T model;
            if ( !_service.TryGet( id, out model ) )
                throw new HttpResponseException( HttpStatusCode.NotFound );
            return model;
        }

        // POST api/<controller> (insert)
        [Authenticate]
        public virtual HttpResponseMessage Post( [FromBody]T value )
        {
            var user = CurrentUser();
            if ( user != null )
            {
                _service.Add( value, null );

                if ( !value.IsValid )
                    return ControllerContext.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        String.Join( ",", value.ValidationResults.Select( r => r.ErrorMessage ).ToArray() ) );

                _service.Save( value, user.PersonId );

                var response = ControllerContext.Request.CreateResponse( HttpStatusCode.Created );
                // TODO set response.Headers.Location as per REST POST convention
                //response.Headers.Location = new Uri( Request.RequestUri, "/api/pages/" + page.Id.ToString() );
                return response;
            }

            throw new HttpResponseException( HttpStatusCode.Unauthorized );
        }

        // PUT api/<controller>/5  (update)
        [Authenticate]
        public virtual void Put( int id, [FromBody]T value )
        {
            var user = CurrentUser();
            if ( user != null )
            {
                T existingModel;
                if ( !_service.TryGet( id, out existingModel ) )
                    throw new HttpResponseException( HttpStatusCode.NotFound );

                _service.Attach( value );
                if ( value.IsValid )
                    _service.Save( value, user.PersonId );
                else

                    throw new HttpResponseException( HttpStatusCode.BadRequest );
            }
            else
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
        }

        // DELETE api/<controller>/5
        [Authenticate]
        public virtual void Delete( int id )
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
            else
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
        }


        /// <summary>
        /// Gets a list of objects represented by the selected data view
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [Authenticate]
        [ActionName("DataView")]
        public IQueryable<T> GetDataView( int id )
        {
            var dataView = new DataViewService().Get( id );
            if ( dataView != null && dataView.EntityType.Name == typeof(T).FullName )
            {
                var errorMessages = new List<string>();

                var paramExpression = _service.ParameterExpression;
                var whereExpression = dataView.GetExpression( _service, paramExpression, out errorMessages );

                if ( paramExpression != null && whereExpression != null )
                {
                    return _service.Get( paramExpression, whereExpression );
                }
            }

            return null;
        }

        protected virtual Rock.Model.UserLogin CurrentUser()
        {
            var principal = ControllerContext.Request.GetUserPrincipal();
            if ( principal != null && principal.Identity != null )
            {
                var userLoginService = new Rock.Model.UserLoginService();
                var userLogin = userLoginService.GetByUserName( principal.Identity.Name );

                if ( userLogin != null )
                    return userLogin;
            }

            return null;
        }
    }
}