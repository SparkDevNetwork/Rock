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
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using Rock.Data;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.ReminderType"/> objects.
    /// </summary>
    public partial class ReminderTypeService
    {
        /// <summary>
        /// Gets a list of reminder types for a specific entity type.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <returns></returns>
        /// <param name="authorizedPerson">The authorized person.</param>
        /// <returns></returns>
        public List<ReminderType> GetReminderTypesForEntityType( int entityTypeId, Person authorizedPerson )
        {
            var reminderTypes = this.Queryable()
                .Where( t => t.EntityTypeId == entityTypeId )
                .ToList();

            var authorizedReminderTypes = reminderTypes
                .Where( t => t.IsAuthorized( Rock.Security.Authorization.VIEW, authorizedPerson ) )
                .OrderBy( t => t.Order )
                .ToList();

            return authorizedReminderTypes;
        }
    }
}
