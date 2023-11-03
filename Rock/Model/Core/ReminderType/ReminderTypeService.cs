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
using System.Linq.Dynamic.Core;

using Rock.Attribute;
using Rock.Data;
using Rock.Web.Cache;

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

        /// <summary>
        /// Gets the type of the reminder types for entity.
        /// </summary>
        /// <param name="entityTypeGuid">The entity type unique identifier.</param>
        /// <param name="authorizedPerson">The authorized person.</param>
        /// <returns>List&lt;ReminderType&gt;.</returns>
        public List<ReminderType> GetReminderTypesForEntityType( Guid entityTypeGuid, Person authorizedPerson )
        {
            var entityTypeId = EntityTypeCache.GetId( entityTypeGuid );

            if( entityTypeId == null )
            {
                return null;
            }

            return GetReminderTypesForEntityType( entityTypeId.Value, authorizedPerson );
        }

        /// <summary>
        /// Gets the reminder types and reminders associated as a queryable
        /// for a specific person.
        /// </summary>
        /// <param name="personAliasId"></param>
        /// <returns></returns>
        [RockInternal( "1.16" )]
        internal IQueryable<IGrouping<ReminderType, Reminder>> GetTypesAndRemindersAssignedToPerson( int personAliasId )
        {
            var rockContext = ( RockContext ) this.Context;
            var reminderService = new ReminderService( rockContext ).Queryable();

            // Querying the reminder type and reminder table
            // to find reminder types and the amount of reminders
            // associated with them.
            var reminderTypes = this.Queryable()
                // Join on the reminder type id.
                .Join( reminderService,
                ( rt ) => rt.Id,
                ( r ) => r.ReminderTypeId,
                ( rt, r ) => new
                {
                    ReminderType = rt,
                    Reminder = r
                }
                )
                // Limit to reminders that are assigned
                // to the passed in person.
                .Where( r => r.Reminder.PersonAliasId == personAliasId )
                .GroupBy( rt => rt.ReminderType, r => r.Reminder );

            // We want to be able to build off of this query,
            // so just return the grouping.
            return reminderTypes;
        }
    }
}
