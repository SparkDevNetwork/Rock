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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web.Http;
using System.Web.Http.OData.Query;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;

namespace Rock.Rest
{
    public abstract class ApiController<T> : ApiController
        where T : Rock.Data.Entity<T>, new()
    {
        /// <summary>
        /// Gets or sets the service.
        /// </summary>
        /// <value>
        /// The service.
        /// </value>
        protected Service<T> Service
        {
            get { return _service; }
            set { _service = value; }
        }
        private Service<T> _service;

        public ApiController( Service<T> service )
        {
            _service = service;
            _service.Repository.SetConfigurationValue( "ProxyCreationEnabled", "false" );
        }

        // GET api/<controller>
        [Authenticate, Secured]
        [Queryable( AllowedQueryOptions = AllowedQueryOptions.All )]
        public virtual IQueryable<T> Get()
        {
            var result = _service.Queryable();
            return result;
        }

        // GET api/<controller>/5
        [Authenticate, Secured]
        public virtual T Get( int id )
        {
            T model;
            if ( !_service.TryGet( id, out model ) )
                throw new HttpResponseException( HttpStatusCode.NotFound );
            return model;
        }

        // POST api/<controller> (insert)
        [Authenticate, Secured]
        public virtual HttpResponseMessage Post( [FromBody]T value )
        {
            CheckCanEdit( value );

            var personAlias = GetPersonAlias();

            _service.Add( value, personAlias );

            if ( !value.IsValid )
                return ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    String.Join( ",", value.ValidationResults.Select( r => r.ErrorMessage ).ToArray() ) );

            _service.Save( value, personAlias );

            var response = ControllerContext.Request.CreateResponse( HttpStatusCode.Created );
            // TODO set response.Headers.Location as per REST POST convention
            //response.Headers.Location = new Uri( Request.RequestUri, "/api/pages/" + page.Id.ToString() );
            return response;
        }

        // PUT api/<controller>/5  (update)
        [Authenticate, Secured]
        public virtual void Put( int id, [FromBody]T value )
        {
            T targetModel;
            if ( !_service.TryGet( id, out targetModel ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            CheckCanEdit( targetModel );

            _service.SetValues( value, targetModel );

            if ( targetModel.IsValid )
            {
                _service.Save( targetModel, GetPersonAlias() );
            }
            else
            {
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }
        }

        // DELETE api/<controller>/5
        [Authenticate, Secured]
        public virtual void Delete( int id )
        {
            T model;
            if ( !_service.TryGet( id, out model ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            CheckCanEdit( model );

            var personAlias = GetPersonAlias();
            _service.Delete( model, personAlias );
            _service.Save( model, personAlias );
        }


        /// <summary>
        /// Gets a list of objects represented by the selected data view
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [ActionName( "DataView" )]
        public IQueryable<T> GetDataView( int id )
        {
            var dataView = new DataViewService().Get( id );

            CheckCanEdit( dataView );

            if ( dataView != null && dataView.EntityType.Name == typeof( T ).FullName )
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

        /// <summary>
        /// Gets the peron alias.
        /// </summary>
        /// <returns></returns>
        protected virtual Rock.Model.Person GetPerson()
        {
            if (Request.Properties.Keys.Contains("Person"))
            {
                return Request.Properties["Person"] as Person;
            }

            var principal = ControllerContext.Request.GetUserPrincipal();
            if ( principal != null && principal.Identity != null )
            {
                var userLoginService = new Rock.Model.UserLoginService();
                var userLogin = userLoginService.GetByUserName( principal.Identity.Name );

                if ( userLogin != null )
                {
                    var person = userLogin.Person;
                    Request.Properties.Add( "Person", person );
                    return userLogin.Person;
                }
            }

            return null;
        }

        protected virtual Rock.Model.PersonAlias GetPersonAlias()
        {
            var person = GetPerson();
            if (person != null)
            {
                return person.PrimaryAlias;
            }

            return null;
        }

        protected void CheckCanEdit( T entity )
        {
            if ( entity is ISecured )
            {
                CheckCanEdit( (ISecured)entity );
            }
        }

        protected void CheckCanEdit( ISecured securedModel )
        {
            CheckCanEdit( securedModel, GetPerson() );
        }

        protected void CheckCanEdit( ISecured securedModel, Person person )
        {
            if ( securedModel != null )
            {
                // Need to reload using service with a proxy enabled so that if model has custom
                // parent authorities, those properties can be lazy-loaded and checked for authorization
                ISecured reloadedModel = (ISecured)new Service<T>().Get( securedModel.Id );
                if ( reloadedModel != null && !reloadedModel.IsAuthorized( Rock.Security.Authorization.EDIT, person ) )
                {
                    throw new HttpResponseException( HttpStatusCode.Unauthorized );
                }
            }
        }
    }
}