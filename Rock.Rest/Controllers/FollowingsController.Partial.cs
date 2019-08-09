// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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

using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Followings REST API
    /// </summary>
    public partial class FollowingsController
    {
        /// <summary>
        /// Deletes the following record(s).
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="personId">The person identifier.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Followings/{EntityTypeId}/{EntityId}/{PersonId}" )]
        public virtual void Delete( int entityTypeId, int entityId, int personId )
        {
            SetProxyCreation( true );

            foreach ( var following in Service.Queryable()
                .Where( f =>
                    f.EntityTypeId == entityTypeId &&
                    f.EntityId == entityId &&
                    f.PersonAlias.PersonId == personId ) )
            {
                CheckCanEdit( following );
                Service.Delete( following );
            }

            Service.Context.SaveChanges();
        }

        /// <summary>
        /// Deletes the following record(s).
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Followings/{EntityTypeId}/{EntityId}" )]
        [System.Web.Http.HttpDelete]
        public virtual void Delete( int entityTypeId, int entityId )
        {
            var person = GetPerson();

            if ( person == null )
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }

            SetProxyCreation( true );

            foreach ( var following in Service.Queryable()
                .Where( f =>
                    f.EntityTypeId == entityTypeId &&
                    f.EntityId == entityId &&
                    f.PersonAlias.PersonId == person.Id ) )
            {
                CheckCanEdit( following );
                Service.Delete( following );
            }

            Service.Context.SaveChanges();
        }

        /// <summary>
        /// Follows the specified entity.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Followings/{EntityTypeId}/{EntityId}" )]
        [System.Web.Http.HttpPost]
        public virtual HttpResponseMessage Follow( int entityTypeId, int entityId )
        {
            var person = GetPerson();

            if ( person == null )
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }

            SetProxyCreation( true );

            var following = new Following
            {
                EntityTypeId = entityTypeId,
                EntityId = entityId,
                PersonAliasId = GetPerson().PrimaryAliasId.Value
            };

            Service.Add( following );

            if ( !following.IsValid )
            {
                return ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    string.Join( ",", following.ValidationResults.Select( r => r.ErrorMessage ).ToArray() ) );
            }

            if ( !System.Web.HttpContext.Current.Items.Contains( "CurrentPerson" ) )
            {
                System.Web.HttpContext.Current.Items.Add( "CurrentPerson", GetPerson() );
            }

            Service.Context.SaveChanges();

            var response = ControllerContext.Request.CreateResponse( HttpStatusCode.Created, following.Id );

            return response;
        }
    }
}
