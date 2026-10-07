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
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    public partial class GroupSyncsController
    {
        /// <summary>
        /// POST endpoint. Use this to INSERT a new GroupSync.
        /// </summary>
        /// <param name="value">The GroupSync to add.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        public override HttpResponseMessage Post( [FromBody] GroupSync value )
        {
            if ( value == null )
            {
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }

            CheckGroupSyncTarget( value, null );

            return base.Post( value );
        }

        /// <summary>
        /// Validates the GroupSync's target after PUT or PATCH applies the caller's values.
        /// </summary>
        /// <param name="entity">The tracked GroupSync with the caller's values applied.</param>
        protected override void CheckCanEditAfterUpdate( GroupSync entity )
        {
            base.CheckCanEditAfterUpdate( entity );

            var originalGroupId = Service.Context.Entry( entity ).Property( s => s.GroupId ).OriginalValue;

            CheckGroupSyncTarget( entity, originalGroupId );
        }

        /// <summary>
        /// Blocks security role groups and requires EDIT on the group and VIEW on the data view.
        /// </summary>
        /// <param name="groupSync">The GroupSync with the caller's values applied.</param>
        /// <param name="originalGroupId">The stored GroupId for an update, or <c>null</c> for an insert.</param>
        /// <exception cref="HttpResponseException"></exception>
        private void CheckGroupSyncTarget( GroupSync groupSync, int? originalGroupId )
        {
            var person = GetPerson();

            using ( var rockContext = new RockContext() )
            {
                var groupService = new GroupService( rockContext );

                if ( originalGroupId.HasValue && originalGroupId.Value != groupSync.GroupId )
                {
                    var originalGroup = groupService.Get( originalGroupId.Value );
                    if ( originalGroup != null && originalGroup.IsSecurityRoleOrSecurityGroupType() )
                    {
                        ThrowSecurityRoleForbidden();
                    }
                }

                var group = groupService.Get( groupSync.GroupId );
                if ( group == null )
                {
                    throw new HttpResponseException( HttpStatusCode.BadRequest );
                }

                if ( group.IsSecurityRoleOrSecurityGroupType() )
                {
                    ThrowSecurityRoleForbidden();
                }

                if ( !group.IsAuthorized( Rock.Security.Authorization.EDIT, person ) )
                {
                    throw new HttpResponseException( HttpStatusCode.Unauthorized );
                }
            }

            var dataView = DataViewCache.Get( groupSync.SyncDataViewId );
            if ( dataView == null || !dataView.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }
        }

        /// <summary>
        /// Throws a 403 for security role groups.
        /// </summary>
        /// <exception cref="HttpResponseException"></exception>
        private void ThrowSecurityRoleForbidden()
        {
            var response = ControllerContext.Request.CreateErrorResponse(
                HttpStatusCode.Forbidden,
                "Security role groups cannot be synced through this endpoint." );
            throw new HttpResponseException( response );
        }
    }
}
