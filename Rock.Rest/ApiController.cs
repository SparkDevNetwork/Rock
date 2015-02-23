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
using System.Web.Http.OData;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;

namespace Rock.Rest
{
    /// <summary>
    /// ApiController for Rock REST Entity endpoints
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ApiController<T> : ApiControllerBase
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiController{T}"/> class.
        /// </summary>
        /// <param name="service">The service.</param>
        public ApiController( Service<T> service )
        {
            Service = service;

            // Turn off proxy creation by default so that when querying objects through rest, EF does not automatically navigate all child properties for requested objects
            // When adding, updating, or deleting objects, the proxy should be enabled to properly track relationships that should or shouldn't be updated
            SetProxyCreation( false );
        }

        // GET api/<controller>
        [Authenticate, Secured]
        [EnableQuery]
        public virtual IQueryable<T> Get()
        {
            var result = Service.Queryable();
            return result;
        }

        // GET api/<controller>/5
        [Authenticate, Secured]
        [ActionName( "GetById" )]
        public virtual T GetById( int id )
        {
            T model;
            if ( !Service.TryGet( id, out model ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            return model;
        }

        // GET api/<controller>(5)
        [Authenticate, Secured]
        [EnableQuery]
        public virtual T Get( [FromODataUri] int key )
        {
            T model;
            if ( !Service.TryGet( key, out model ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            return model;
        }

        // POST api/<controller> (insert)
        [Authenticate, Secured]
        public virtual HttpResponseMessage Post( [FromBody]T value )
        {
            SetProxyCreation( true );

            CheckCanEdit( value );

            Service.Add( value );

            if ( !value.IsValid )
            {
                return ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    string.Join( ",", value.ValidationResults.Select( r => r.ErrorMessage ).ToArray() ) );
            }

            System.Web.HttpContext.Current.Items.Add( "CurrentPerson", GetPerson() );
            Service.Context.SaveChanges();

            var response = ControllerContext.Request.CreateResponse( HttpStatusCode.Created );

            // TODO set response.Headers.Location as per REST POST convention
            //response.Headers.Location = new Uri( Request.RequestUri, "/api/pages/" + page.Id.ToString() );
            return response;
        }

        // PUT api/<controller>/5  (update)
        [Authenticate, Secured]
        public virtual void Put( int id, [FromBody]T value )
        {
            SetProxyCreation( true );

            T targetModel;
            if ( !Service.TryGet( id, out targetModel ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            CheckCanEdit( targetModel );

            Service.SetValues( value, targetModel );

            if ( targetModel.IsValid )
            {
                System.Web.HttpContext.Current.Items.Add( "CurrentPerson", GetPerson() );
                Service.Context.SaveChanges();
            }
            else
            {
                var response = ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    string.Join( ",", targetModel.ValidationResults.Select( r => r.ErrorMessage ).ToArray() ) );
                throw new HttpResponseException( response );
            }
        }

        // DELETE api/<controller>/5
        [Authenticate, Secured]
        public virtual void Delete( int id )
        {
            SetProxyCreation( true );

            T model;
            if ( !Service.TryGet( id, out model ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            CheckCanEdit( model );

            Service.Delete( model );
            Service.Context.SaveChanges();
        }

        /// <summary>
        /// Gets a list of objects represented by the selected data view
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [ActionName( "DataView" )]
        [EnableQuery]
        public IQueryable<T> GetDataView( int id )
        {
            var dataView = new DataViewService( new RockContext() ).Get( id );

            CheckCanEdit( dataView );
            SetProxyCreation( false );

            if ( dataView != null && dataView.EntityType.Name == typeof( T ).FullName )
            {
                var errorMessages = new List<string>();

                var paramExpression = Service.ParameterExpression;
                var whereExpression = dataView.GetExpression( Service, paramExpression, out errorMessages );

                if ( paramExpression != null )
                {
                    return Service.Get( paramExpression, whereExpression );
                }
            }

            return null;
        }

        /// <summary>
        /// Checks the can edit.
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected virtual void CheckCanEdit( T entity )
        {
            if ( entity is ISecured )
            {
                CheckCanEdit( (ISecured)entity );
            }
        }

        /// <summary>
        /// Checks the can edit.
        /// </summary>
        /// <param name="securedModel">The secured model.</param>
        protected virtual void CheckCanEdit( ISecured securedModel )
        {
            CheckCanEdit( securedModel, GetPerson() );
        }

        /// <summary>
        /// Checks the can edit.
        /// </summary>
        /// <param name="securedModel">The secured model.</param>
        /// <param name="person">The person.</param>
        /// <exception cref="System.Web.Http.HttpResponseException">
        /// </exception>
        protected virtual void CheckCanEdit( ISecured securedModel, Person person )
        {
            if ( securedModel != null )
            {
                if ( IsProxy( securedModel ) )
                {
                    if ( !securedModel.IsAuthorized( Rock.Security.Authorization.EDIT, person ) )
                    {
                        throw new HttpResponseException( HttpStatusCode.Unauthorized );
                    }
                }
                else
                {
                    // Need to reload using service with a proxy enabled so that if model has custom
                    // parent authorities, those properties can be lazy-loaded and checked for authorization
                    SetProxyCreation( true );
                    ISecured reloadedModel = (ISecured)Service.Get( securedModel.Id );
                    if ( reloadedModel != null && !reloadedModel.IsAuthorized( Rock.Security.Authorization.EDIT, person ) )
                    {
                        throw new HttpResponseException( HttpStatusCode.Unauthorized );
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [enable proxy creation].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable proxy creation]; otherwise, <c>false</c>.
        /// </value>
        protected void SetProxyCreation( bool enabled )
        {
            Service.Context.Configuration.ProxyCreationEnabled = enabled;
        }

        /// <summary>
        /// Determines whether the specified type is proxy.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        protected bool IsProxy( object type )
        {
            return type != null && System.Data.Entity.Core.Objects.ObjectContext.GetObjectType( type.GetType() ) != type.GetType();
        }

    }
}