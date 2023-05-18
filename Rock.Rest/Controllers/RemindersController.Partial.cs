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
    /// Reminders REST API
    /// </summary>
    public partial class RemindersController
    {
        /// <summary>
        /// Gets the reminder count for the authenticated user.
        /// </summary>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/Reminders/GetReminderCount" )]
        [Rock.SystemGuid.RestActionGuid( "7D1FB560-CBF9-450A-8332-E787D03A8929" )]
        public int? GetReminderCount()
        {
            return RockRequestContext.CurrentPerson.ReminderCount;
        }
    }
}