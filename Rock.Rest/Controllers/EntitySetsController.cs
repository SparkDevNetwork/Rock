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
using System.Collections.Generic;
using System;
using System.Web.Http;
using Rock.Data;
using Rock.Rest.Filters;
using Rock.Web.Cache;
using System.Linq;
using System.Data.Entity;
using Rock.Model;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// The Entity Sets Controller.
    /// </summary>
    public partial class EntitySetsController
    {
        /// <summary>
        /// Posts the entity set from unique identifier.
        /// </summary>
        /// <param name="entityItemGuids">The entity item guids.</param>
        /// <param name="entityTypeGuid">The entity type unique identifier.</param>
        /// <param name="timeToExpire">The amount of time in minutes before the entity set expires.</param>
        /// <returns>IHttpActionResult.</returns>
        [System.Web.Http.Route( "api/EntitySets/CreateFromItems/{entityTypeGuid:guid}" )]
        [HttpPost]
        [Authenticate, Secured]
        [Rock.SystemGuid.RestActionGuid( "50B248D7-C52A-4698-B4AF-C9DE305394EC" )]
        public IHttpActionResult PostEntitySetFromGuid( [FromBody] List<Guid> entityItemGuids, Guid entityTypeGuid, int timeToExpire = 15 )
        {
            using ( var rockContext = new RockContext() )
            {
                var entitySetGuid = EntitySetService.CreateEntitySetFromItems( entityItemGuids, entityTypeGuid, timeToExpire, rockContext );
                if ( !entitySetGuid.HasValue )
                {
                    return InternalServerError();
                }

                return Ok( entitySetGuid.Value );
            }
        }

        /// <summary>
        /// Posts the entity set from int.
        /// </summary>
        /// <param name="entityItemIds">The entity item ids.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="timeToExpire">The amount of time in minutes before the entity set expires.</param>
        /// <returns>System.Web.Http.IHttpActionResult.</returns>
        [System.Web.Http.Route( "api/EntitySets/CreateFromItems/{entityTypeId:int}" )]
        [HttpPost]
        [Authenticate, Secured]
        [Rock.SystemGuid.RestActionGuid( "32374DFE-6478-41A5-AE7D-43DD58DC6176" )]
        public IHttpActionResult PostEntitySetFromInt( [FromBody] List<int> entityItemIds, int entityTypeId, int timeToExpire = 15 )
        {
            using ( var rockContext = new RockContext() )
            {
                var entitySetId = EntitySetService.CreateEntitySetFromItems( entityItemIds, entityTypeId, timeToExpire, rockContext );
                if ( !entitySetId.HasValue )
                {
                    return InternalServerError();
                }

                return Ok( entitySetId.Value );
            }
        }
    }
}