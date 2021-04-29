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
using System.Web.Http.ModelBinding.Binders;

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
        /// <param name="purposeKey">The custom purpose to identify the type of following.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Followings/{entityTypeId}/{entityId}/{personId}" )]
        public virtual void Delete( int entityTypeId, int entityId, int personId, [FromUri( BinderType = typeof( TypeConverterModelBinder ) )] string purposeKey = null )
        {
            /*
             4/23/2021 - Daniel
            The weird BinderType thing allows the user to do ?purposeKey=
            to pass a blank purpose key, otherwise it throws a required value
            error even though it's not required.
            */
            SetProxyCreation( true );

            purposeKey = purposeKey ?? string.Empty;

            // If PurposeKey is null then the provided purposeKey must be
            // empty.
            // If PurposeKey is not null then it must match the provided
            // purposeKey.
            var followings = Service.Queryable()
                .Where( f =>
                    f.EntityTypeId == entityTypeId &&
                    f.EntityId == entityId &&
                    f.PersonAlias.PersonId == personId &&
                    ( ( f.PurposeKey == null && purposeKey == "" ) || f.PurposeKey == purposeKey ) );

            foreach ( var following in followings )
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
        /// <param name="purposeKey">The custom purpose to identify the type of following.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Followings/{entityTypeId}/{entityId}" )]
        [System.Web.Http.HttpDelete]
        public virtual void Delete( int entityTypeId, int entityId, [FromUri( BinderType = typeof( TypeConverterModelBinder ) )] string purposeKey = null )
        {
            /*
             4/23/2021 - Daniel
            The weird BinderType thing allows the user to do ?purposeKey=
            to pass a blank purpose key, otherwise it throws a required value
            error even though it's not required.
            */
            var person = GetPerson();

            if ( person == null )
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }

            SetProxyCreation( true );

            purposeKey = purposeKey ?? string.Empty;

            var followings = Service.Queryable()
                .Where( f =>
                    f.EntityTypeId == entityTypeId &&
                    f.EntityId == entityId &&
                    f.PersonAlias.PersonId == person.Id &&
                    ( ( f.PurposeKey == null && purposeKey == "" ) || f.PurposeKey == purposeKey ) );
            foreach ( var following in followings )
            {
                // Don't check security here because a person is allowed to un-follow/delete something they previously followed
                Service.Delete( following );
            }

            Service.Context.SaveChanges();
        }

        /// <summary>
        /// Follows the specified entity.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="purposeKey">The custom purpose to identify the type of following.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Followings/{entityTypeId}/{entityId}" )]
        [System.Web.Http.HttpPost]
        public virtual HttpResponseMessage Follow( int entityTypeId, int entityId, [FromUri( BinderType = typeof( TypeConverterModelBinder ) )] string purposeKey = null )
        {
            /*
             4/23/2021 - Daniel
            The weird BinderType thing allows the user to do ?purposeKey=
            to pass a blank purpose key, otherwise it throws a required value
            error even though it's not required.
            */
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
                PersonAliasId = GetPerson().PrimaryAliasId.Value,
                PurposeKey = purposeKey
            };

            Service.Add( following );

            if ( !following.IsValid )
            {
                return ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    string.Join( ",", following.ValidationResults.Select( r => r.ErrorMessage ).ToArray() ) );
            }

            System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", GetPerson() );

            Service.Context.SaveChanges();

            var response = ControllerContext.Request.CreateResponse( HttpStatusCode.Created, following.Id );

            return response;
        }
    }
}
