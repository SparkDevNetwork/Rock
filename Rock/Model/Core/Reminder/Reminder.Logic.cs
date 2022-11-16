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

using System;

namespace Rock.Model
{
    public partial class Reminder
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether this is a renewing reminder.  If a reminder has a null value
        /// in RenewMaxCount it is perpetually renewing.  A reminder must also have a non-null, non-zero
        /// value in RenewPeriodDays to be renewing.
        /// </summary>
        public bool IsRenewing
        {
            get
            {
                if ( this.RenewMaxCount == 0 )
                {
                    return false;
                }
                else if ( this.RenewPeriodDays == null || this.RenewPeriodDays == 0 )
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this reminder has future occurrences.  Returns false if the
        /// reminder is non-renewing.
        /// </summary>
        public bool HasFutureOccurrences
        {
            get
            {
                if ( !IsRenewing )
                {
                    // Non-renewing reminders don't have future occurrences.
                    return false;
                }

                if ( RenewMaxCount == null )
                {
                    // If RenewMaxCount is null, this reminder is perpetually renewing.
                    return true;
                }

                return ( RenewMaxCount > RenewCurrentCount );
            }
        }

        /// <summary>
        /// Gets a value indicating whether this reminder is active.
        /// </summary>
        public bool IsActive
        {
            get
            {
                if ( this.IsComplete )
                {
                    return false;
                }
                else if ( this.ReminderDate >= RockDateTime.Now )
                {
                    return false;
                }

                return true;
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Marks this reminder as complete and adjusts the reminder date and recurrence count.  Changes
        /// should be saved to the database after calling this method.
        /// </summary>
        public void CompleteReminder()
        {
            var currentDate = RockDateTime.Now;
            this.IsComplete = true;
            if ( this.IsRenewing )
            {
                if ( this.HasFutureOccurrences )
                {
                    this.ReminderDate = this.GetNextDate( currentDate ).Value;
                    this.IsComplete = false;
                    this.RenewCurrentCount++;
                }
            }
        }

        /// <summary>
        /// Reset a completed reminder.
        /// </summary>
        public void ResetCompletedReminder()
        {
            var currentDate = RockDateTime.Now;
            this.IsComplete = false;
            this.RenewCurrentCount = 0;
            if ( this.ReminderDate < currentDate )
            {
                this.ReminderDate = this.GetNextDate( currentDate ).Value;
            }
        }

        /// <summary>
        /// Cancels reoccurrence.  Changes should be saved to the database after calling this method.
        /// </summary>
        public void CancelReoccurrence()
        {
            this.RenewMaxCount = 0;
        }

        /// <summary>
        /// Gets the next reminder date.
        /// Note:  The next date is determined from the time a reminder is completed, not the prior
        /// reminder date.
        /// </summary>
        /// <param name="completedDateTime">The current date time.</param>
        /// <returns></returns>
        public DateTime? GetNextDate( DateTime completedDateTime )
        {
            if ( !this.IsRenewing )
            {
                return null;
            }

            if ( !this.HasFutureOccurrences )
            {
                return null;
            }

            return completedDateTime.AddDays( this.RenewPeriodDays.Value );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Reminder: {this.ReminderType.Name}";
        }

        #endregion Public Methods
    }
}
