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

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Reminder SaveHook.
    /// </summary>
    public partial class Reminder
    {
        internal class SaveHook : EntitySaveHook<Reminder>
        {
            protected override void PostSave()
            {
                var reminder = this.Entity;

                var person = reminder.PersonAlias?.Person;
                if ( person == null )
                {
                    person = new PersonAliasService( RockContext ).GetPerson( reminder.PersonAliasId );
                }

                if ( this.State == EntityContextState.Added )
                {
                    HandleAddedReminderCount( person );
                }
                else if ( this.State == EntityContextState.Modified )
                {
                    HandleModifiedReminderCount( person );
                }
                else if ( this.State == EntityContextState.Deleted )
                {
                    HandleDeletedReminderCount();
                }

                base.PostSave();
            }

            #region Reminder Count Handling

            /// <summary>
            /// Checks to see if a reminder was previously active.  Note that this shares similar logic with <see cref="Reminder.IsActive"/>.
            /// </summary>
            /// <returns></returns>
            private bool ReminderWasActive()
            {
                var wasComplete = ( bool ) this.OriginalValues[nameof( this.Entity.IsComplete )];
                if ( wasComplete )
                {
                    return false;
                }

                var reminderDate = ( DateTime ) this.OriginalValues[nameof( this.Entity.ReminderDate )];
                if ( reminderDate >= RockDateTime.Now )
                {
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Updates a Person's reminder count when appropriate after a reminder has been added.
            /// </summary>
            private void HandleAddedReminderCount( Person person )
            {
                var reminder = this.Entity;

                var reminderCount = ( person.ReminderCount ?? 0 );

                if ( reminder.IsActive )
                {
                    // If an active reminder was added, increment the counter.
                    person.ReminderCount = reminderCount + 1;
                    this.RockContext.SaveChanges( true );
                }
            }

            /// <summary>
            /// Updates a Person's reminder count when appropriate after a reminder has been modified.
            /// </summary>
            private void HandleModifiedReminderCount( Person person )
            {
                var reminder = this.Entity;
                var reminderCount = ( person.ReminderCount ?? 0 );

                bool isActive = reminder.IsActive;
                bool wasActive = ReminderWasActive();

                var originalPersonAliasId = ( int ) this.OriginalValues[nameof( this.Entity.PersonAliasId )];
                bool personAssignmentChanged = ( this.Entity.PersonAliasId != originalPersonAliasId );

                if ( isActive && wasActive && personAssignmentChanged )
                {
                    // If an active reminder was re-assigned to another person, the original person's reminder
                    // count should be decremented and the new person's reminder count should be increased.
                    person.ReminderCount = reminderCount + 1;
                    var originalPersonAlias = new PersonAliasService( this.RockContext ).Get( originalPersonAliasId );
                    if ( originalPersonAlias.Person.ReminderCount != null && originalPersonAlias.Person.ReminderCount > 0 )
                    {
                        originalPersonAlias.Person.ReminderCount -= 1;
                    }
                    this.RockContext.SaveChanges();
                }
                else if ( isActive && !wasActive )
                {
                    // If a reminder was modified so that it is now active but was not previously active,
                    // increment the counter for the person (even if the assignment changed).
                    person.ReminderCount = reminderCount + 1;
                    RockContext.SaveChanges();
                }
                else if ( !isActive && wasActive )
                {
                    // If a reminder was modified so that it is no longer active but was previously active,
                    // decrement the counter for the original person (whether or not it's the new person).
                    var originalPersonAlias = new PersonAliasService( this.RockContext ).Get( originalPersonAliasId );
                    if ( originalPersonAlias.Person.ReminderCount != null && originalPersonAlias.Person.ReminderCount > 0 )
                    {
                        originalPersonAlias.Person.ReminderCount -= 1;
                    }
                    this.RockContext.SaveChanges();
                }
            }

            /// <summary>
            /// Updates a Person's reminder count to decrement the value when appropriate after a reminder has been
            /// deleted.  Note that this shares similar logic with Reminder.IsActive.
            /// </summary>
            private void HandleDeletedReminderCount()
            {
                if ( !ReminderWasActive() )
                {
                    // If this was not an active reminder, we don't need to worry about the counter.
                    return;
                }

                // If an active reminder was deleted, decrement the counter.
                var personAliasId = ( int ) this.OriginalValues[nameof( this.Entity.PersonAliasId )];
                var personAlias = new PersonAliasService( this.RockContext ).Get( personAliasId );

                if ( personAlias.Person.ReminderCount != null && personAlias.Person.ReminderCount > 0 )
                {
                    personAlias.Person.ReminderCount -= 1;
                }

                this.RockContext.SaveChanges();
            }

            #endregion Reminder Count Handling
        }
    }
}
