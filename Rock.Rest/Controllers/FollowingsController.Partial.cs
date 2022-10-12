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
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding.Binders;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Followings REST API
    /// </summary>
    public partial class FollowingsController
    {
        /// <summary>
        /// Deletes following of the specified entity by the specified user.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="purposeKey">The custom purpose to identify the type of following.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Followings/{entityTypeId}/{entityId}/{personId:int}" )]
        [Rock.SystemGuid.RestActionGuid( "386BCB43-5F2A-4034-9032-850440CBAB7F" )]
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
        /// Deletes following of the specified entity by the specified user.
        /// </summary>
        /// <param name="entityTypeGuid">The entity type identifier.</param>
        /// <param name="entityGuid">The entity identifier.</param>
        /// <param name="personGuid">The person identifier.</param>
        /// <param name="purposeKey">The custom purpose to identify the type of following.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Followings/{entityTypeGuid}/{entityGuid}/{personGuid:guid}" )]
        [Rock.SystemGuid.RestActionGuid( "1071DB79-DB8D-4310-B11C-2637F1A6A630" )]
        public virtual void Delete( Guid entityTypeGuid, Guid entityGuid, Guid personGuid, [FromUri( BinderType = typeof( TypeConverterModelBinder ) )] string purposeKey = null )
        {
            // Convert and validate the input parameters.
            var entityTypeId = EntityTypeCache.GetId( entityTypeGuid ) ?? 0;
            AssertEntityIdentifierParameterIsValid( entityTypeId != 0, "Entity Type", nameof( entityTypeGuid ), entityTypeGuid );

            var entityId = Reflection.GetEntityIdForEntityType( entityTypeGuid, entityGuid ) ?? 0;
            AssertEntityIdentifierParameterIsValid( entityId != 0, "Entity", nameof( entityTypeGuid ), entityTypeGuid );

            var personId = Reflection.GetEntityIdForEntityType( SystemGuid.EntityType.PERSON.AsGuid(), personGuid ) ?? 0;
            AssertEntityIdentifierParameterIsValid( personId != 0, "Person", nameof( entityTypeGuid ), entityTypeGuid );

            // Process the action.
            Delete( entityTypeId, entityId, personId, purposeKey );
        }

        /// <summary>
        /// Deletes following of the specified entity by the current user.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="purposeKey">The custom purpose to identify the type of following.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Followings/{entityTypeId}/{entityId:int}" )]
        [System.Web.Http.HttpDelete]
        [Rock.SystemGuid.RestActionGuid( "AAB5800B-A429-40D2-A402-D3DE7E15776E" )]
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
        /// Deletes following of the specified entity by the current user.
        /// </summary>
        /// <param name="entityTypeGuid">The entity type identifier.</param>
        /// <param name="entityGuid">The entity identifier.</param>
        /// <param name="purposeKey">The custom purpose to identify the type of following.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Followings/{entityTypeGuid}/{entityGuid:guid}" )]
        [Rock.SystemGuid.RestActionGuid( "A3358897-942F-4332-B060-12718DC87CE6" )]
        public virtual void Delete( Guid entityTypeGuid, Guid entityGuid, [FromUri( BinderType = typeof( TypeConverterModelBinder ) )] string purposeKey = null )
        {
            // Convert and validate the input parameters.
            var entityTypeId = EntityTypeCache.GetId( entityTypeGuid ) ?? 0;
            AssertEntityIdentifierParameterIsValid( entityTypeId != 0, "Entity Type", nameof( entityTypeGuid ), entityTypeGuid );

            var entityId = Reflection.GetEntityIdForEntityType( entityTypeGuid, entityGuid ) ?? 0;
            AssertEntityIdentifierParameterIsValid( entityId != 0, "Entity", nameof( entityTypeGuid ), entityTypeGuid );

            // Process the action.
            Delete( entityTypeId, entityId, purposeKey );
        }

        /// <summary>
        /// Adds a Following of the specified entity for the current user.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="purposeKey">The custom purpose to identify the type of following.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Followings/{entityTypeId}/{entityId:int}" )]
        [System.Web.Http.HttpPost]
        [Rock.SystemGuid.RestActionGuid( "1C1F80FE-2567-463E-8BFE-E49ECB8450C7" )]
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

        /// <summary>
        /// Adds a Following of the specified entity for the current user.
        /// </summary>
        /// <param name="entityTypeGuid">The entity type identifier.</param>
        /// <param name="entityGuid">The entity identifier.</param>
        /// <param name="purposeKey">The custom purpose to identify the type of following.</param>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Followings/{entityTypeGuid}/{entityGuid:guid}" )]
        [Rock.SystemGuid.RestActionGuid( "3D09FD68-06F9-4860-9110-60A015004071" )]
        [System.Web.Http.HttpPost]
        public virtual HttpResponseMessage Follow( Guid entityTypeGuid, Guid entityGuid, [FromUri( BinderType = typeof( TypeConverterModelBinder ) )] string purposeKey = null )
        {
            // Convert and validate the input parameters.
            var entityTypeId = EntityTypeCache.GetId( entityTypeGuid ) ?? 0;
            AssertEntityIdentifierParameterIsValid( entityTypeId != 0, "Entity Type", nameof( entityTypeGuid ), entityTypeGuid );

            var entityId = Reflection.GetEntityIdForEntityType( entityTypeGuid, entityGuid ) ?? 0;
            AssertEntityIdentifierParameterIsValid( entityId != 0, "Entity", nameof( entityGuid ), entityGuid );

            // Process the action.
            var response = Follow( entityTypeId, entityId, purposeKey );
            return response;
        }

        #region Support methods

        /// <summary>
        /// Verify that the specified input parameter refers to a valid entity.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="entityTypeName"></param>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        private void AssertEntityIdentifierParameterIsValid( bool predicate, string entityTypeName, string parameterName, object parameterValue )
        {
            if ( !predicate )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, $"Invalid parameter value. No matching {entityTypeName} can be found for '{parameterName}={parameterValue}'." );
                throw new HttpResponseException( errorResponse );
            }
        }

        #endregion
    }
}