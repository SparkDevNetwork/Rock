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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PrayerRequestsController
    {
        [Authenticate, Secured]
        [HttpGet]
        [EnableQuery]
        [System.Web.Http.Route( "api/PrayerRequests/Public" )]
        public IQueryable<PrayerRequest> Public()
        {
            var now = RockDateTime.Now;
            return base.Get()
                .Where( p =>
                    ( p.IsActive.HasValue && p.IsActive.Value == true ) &&
                    ( p.IsPublic.HasValue && p.IsPublic.Value == true ) &&
                    ( p.IsApproved.HasValue && p.IsApproved == true ) &&
                    ( !p.ExpirationDate.HasValue || p.ExpirationDate.Value > now )
                );
        }

        /// <summary>
        /// Gets Prayer Requests for the specified top-level category
        /// Prayer Requests that are in categories that are decendents of the specified category will also be included
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [EnableQuery]
        [System.Web.Http.Route( "api/PrayerRequests/GetByCategory/{categoryId}" )]
        public IQueryable<PrayerRequest> GetByCategory( int categoryId )
        {
            var rockContext = ( this.Service.Context as RockContext ) ?? new RockContext();
            var decendentsCategoriesQry = new CategoryService( rockContext ).GetAllDescendents( categoryId ).Select( a => a.Id );
            return this.Get().Where( a => a.CategoryId.HasValue ).Where( a => decendentsCategoriesQry.Contains( a.CategoryId.Value ) || ( a.CategoryId.Value == categoryId ) );
        }

        /// <summary>
        /// Increment the prayer count for a prayer request
        /// </summary>
        /// <param name="prayerId">The prayer identifier.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <exception cref="System.Web.Http.OData.IEdmEntityObject"></exception>
        [Authenticate, Secured]
        [HttpPut]
        [System.Web.Http.Route( "api/PrayerRequests/Prayed/{prayerId}/{personAliasId}" )]
        public virtual void Prayed( int prayerId, int personAliasId )
        {
            SetProxyCreation( true );

            PrayerRequest prayerRequest;
            if ( !Service.TryGet( prayerId, out prayerRequest ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            prayerRequest.PrayerCount = ( prayerRequest.PrayerCount ?? 0 ) + 1;

            System.Web.HttpContext.Current.Items.Add( "CurrentPerson", GetPerson() );
            Service.Context.SaveChanges();
        }


        /// <summary>
        /// Flags the specified prayer request.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="System.Web.Http.OData.IEdmEntityObject"></exception>
        [Authenticate, Secured]
        [HttpPut]
        [System.Web.Http.Route( "api/PrayerRequests/Flag/{id}" )]
        public virtual void Flag( int id )
        {
            SetProxyCreation( true );

            PrayerRequest prayerRequest;
            if ( !Service.TryGet( id, out prayerRequest ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            prayerRequest.FlagCount = ( prayerRequest.FlagCount ?? 0 ) + 1;
            if ( prayerRequest.FlagCount >= 1 )
            {
                prayerRequest.IsApproved = false;
            }

            Service.Context.SaveChanges();
        }
        
        /// <summary>
        /// Increment the prayer count for a prayer request
        /// </summary>
        /// <param name="id">The prayer identifier.</param>
        /// <exception cref="System.Web.Http.OData.IEdmEntityObject"></exception>
        [Authenticate, Secured]
        [HttpPut]
        [System.Web.Http.Route( "api/PrayerRequests/Prayed/{id}" )]
        public virtual void Prayed( int id )
        {
            SetProxyCreation( true );

            PrayerRequest prayerRequest;
            if ( !Service.TryGet( id, out prayerRequest ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            prayerRequest.PrayerCount = ( prayerRequest.PrayerCount ?? 0 ) + 1;

            Service.Context.SaveChanges();
        }
    }
}
