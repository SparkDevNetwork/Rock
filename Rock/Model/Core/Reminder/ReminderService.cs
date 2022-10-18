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
using System.Data.Entity;
using System.Linq;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.Reminder"/> entity objects.
    /// </summary>
    public partial class ReminderService
    {
        /// <summary>
        /// Recalculate the reminder count for a person.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        public void RecalculateReminderCount( int personId )
        {
            var rockContext = this.Context as RockContext;
            var person = new PersonService( rockContext ).Get( personId );
            int reminderCount = GetByPerson( person.Id ).Count();
            person.ReminderCount = reminderCount;
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets the appropriate set of reminders for the provided input (must have a person identifier, at minimum).
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="reminderTypeId">The entity type identifier.</param>
        /// <returns></returns>
        public IQueryable<Reminder> GetReminders( int personId, int? entityTypeId, int? entityId, int? reminderTypeId )
        {
            if ( entityTypeId.HasValue )
            {
                if ( entityId.HasValue )
                {
                    if ( reminderTypeId.HasValue )
                    {
                        return GetByReminderTypeEntityAndPerson( reminderTypeId.Value, entityTypeId.Value, entityId.Value, personId );
                    }

                    return GetByEntityAndPerson( entityTypeId.Value, entityId.Value, personId );

                }
                else if ( reminderTypeId.HasValue )
                {
                    return GetByReminderTypeEntityTypeAndPerson( reminderTypeId.Value, entityTypeId.Value, personId );
                }

                return GetByEntityTypeAndPerson( entityTypeId.Value, personId );
            }

            return GetByPerson( personId );
        }

        /// <summary>
        /// Gets reminders by person.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IQueryable<Reminder> GetByPerson( int personId )
        {
            return this.Queryable()
                .Where( r => r.PersonAlias.PersonId == personId );
        }

        /// <summary>
        /// Gets reminders by entity type and person.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IQueryable<Reminder> GetByEntityTypeAndPerson( int entityTypeId, int personId )
        {
            return this.Queryable()
                .Where( r => r.ReminderType.EntityTypeId == entityTypeId
                    && r.PersonAlias.PersonId == personId );
        }

        /// <summary>
        /// Gets reminders by entity and person.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IQueryable<Reminder> GetByEntityAndPerson( int entityTypeId, int entityId, int personId )
        {
            return this.Queryable()
                .Where( r => r.ReminderType.EntityTypeId == entityTypeId
                    && r.PersonAlias.PersonId == personId
                    && r.EntityId == entityId );
        }

        /// <summary>
        /// Gets reminders by reminder type, entity type and person.
        /// </summary>
        /// <param name="reminderTypeId">The entity type identifier.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IQueryable<Reminder> GetByReminderTypeEntityTypeAndPerson( int reminderTypeId, int entityTypeId, int personId )
        {
            return this.Queryable()
                .Where( r => r.ReminderType.EntityTypeId == entityTypeId
                    && r.ReminderTypeId == reminderTypeId
                    && r.PersonAlias.PersonId == personId );
        }

        /// <summary>
        /// Gets reminders by reminder type, entity and person.
        /// </summary>
        /// <param name="reminderTypeId">The entity type identifier.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IQueryable<Reminder> GetByReminderTypeEntityAndPerson( int reminderTypeId, int entityTypeId, int entityId, int personId )
        {
            return this.Queryable()
                .Where( r => r.ReminderType.EntityTypeId == entityTypeId
                    && r.ReminderTypeId == reminderTypeId
                    && r.PersonAlias.PersonId == personId
                    && r.EntityId == entityId );
        }

        /// <summary>
        /// Gets reminders by person.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IQueryable<EntityType> GetReminderEntityTypesByPerson( int personId )
        {
            var entityTypeIds = GetByPerson( personId ).Select( r => r.ReminderType.EntityTypeId ).Distinct();
            var entityTypes = new EntityTypeService( this.Context as RockContext ).Queryable()
                .Where( t => entityTypeIds.Contains( t.Id ) );
            return entityTypes;
        }

        /// <summary>
        /// Gets active reminders based on the current date.
        /// </summary>
        /// <param name="currentDate">The current date (e.g., RockDateTime.Now).</param>
        /// <returns></returns>
        public IQueryable<Reminder> GetActiveReminders( DateTime currentDate )
        {
            var activeReminders = this
                .Queryable().AsNoTracking()                 // Get reminders that:
                .Where( r => r.ReminderType.IsActive        //   have active reminder types;
                        && !r.IsComplete                    //   are not complete; and
                        && r.ReminderDate <= currentDate )  //   are active (i.e., reminder date has passed).
                .Include( r => r.ReminderType )             // Make sure to include the ReminderType
                .Include( r => r.PersonAlias.Person );      // and Person.

            return activeReminders;
        }
    }
}
