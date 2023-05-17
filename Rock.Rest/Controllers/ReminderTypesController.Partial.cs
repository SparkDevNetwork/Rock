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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// ReminderTypes REST API
    /// </summary>
    public partial class ReminderTypesController
    {
        /// <summary>
        /// Checks to see if there are reminder types available for a specified entity type with
        /// View authorization for authenticated user.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/ReminderTypes/ReminderTypesExistForEntityType" )]
        [Rock.SystemGuid.RestActionGuid( "4D2E2784-BDA4-4C37-8098-626F2971C040" )]
        public bool ReminderTypesExistForEntityType( int entityTypeId )
        {
            var reminderTypeService = ( ReminderTypeService ) Service;
            var authorizedReminderTypeCount = reminderTypeService.Queryable()
                .Where( t => t.IsActive && t.EntityTypeId == entityTypeId )
                .ToList() // Execute EF query so LINQ can use IsAuthorized().
                .Where( t => t.IsAuthorized( Rock.Security.Authorization.VIEW, RockRequestContext.CurrentPerson ) )
                .Count();

            return ( authorizedReminderTypeCount > 0 );
        }
    }
}