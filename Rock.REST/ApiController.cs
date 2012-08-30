using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Http;
using System.Web.Http;

using Rock.Data;

namespace Rock.Rest
{
    public abstract class ApiController<T, D>  : ApiController 
        where T : Rock.Data.Model<T>
        where D : Rock.Data.Dto<T>, new()
    {
        private Service<T, D> _service;

        public ApiController( Service<T, D> service )
        {
            _service = service;
        }

        // GET api/<controller>
        public IQueryable<D> Get()
        {
            return _service.QueryableDto();
        }

        // GET api/<controller>/5
        public D Get( int id )
        {
            T model;
            if ( !_service.TryGet( id, out model ) )
                throw new HttpResponseException( HttpStatusCode.NotFound );
            var dto = new D();
            dto.CopyFromModel(model);
            return dto;
        }

        // POST api/<controller>
        public HttpResponseMessage Post( [FromBody]D value )
        {
            if ( !ModelState.IsValid )
            {
            }

            T model = _service.CreateNew();
            _service.Add( model, null );

            value.CopyToModel(model);

            if ( !model.IsValid )
                throw new HttpResponseException( HttpStatusCode.BadRequest );

            _service.Save( model, null );

            var response = ControllerContext.Request.CreateResponse( HttpStatusCode.Created );
            //response.Headers.Location = new Uri( Request.RequestUri, "/api/pages/" + page.Id.ToString() );
            return response;
            
        }

        // PUT api/<controller>/5
        public void Put( int id, [FromBody]D value )
        {
            var service = new Service<T,D>();
            T model;
            if ( !service.TryGet( id, out model ) )
                throw new HttpResponseException( HttpStatusCode.NotFound );

            value.CopyToModel( model );

            if ( model.IsValid )
                service.Save( model, null );
            else
                throw new HttpResponseException( HttpStatusCode.BadRequest );
        }

        // DELETE api/<controller>/5
        public void Delete( int id )
        {
            T model;
            if ( !_service.TryGet( id, out model ) )
                throw new HttpResponseException( HttpStatusCode.NotFound );

            _service.Delete( model, null );
            _service.Save( model, null );
        }
    }
}
