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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using Rock.Data;
    using Rock.Model;
    
    /// <summary>
    /// Adds an RSVP Reminder SystemCommunication to the database.
    /// </summary>
    public partial class AddRsvpReminderSystemCommunication : Rock.Migrations.RockMigration
    {
        #region Reminder Values
        private const string RsvpReminderGuid = "6F48C40C-92E9-4B11-ADCB-59F474A8B4B2";
        private const string RsvpReminderSubject = "RSVP Reminder for {{ Occurrence.OccurrenceDate | Date:'dddd, MMMM d, yyyy' }}";
        private const string RsvpReminderSmsMessage = "This is just a reminder from {{ 'Global' | Attribute:'OrganizationName' }} that you have RSVP'd for {{ Occurrence.Name }} on {{ Occurrence.OccurrenceDate | Date:'dddd, MMMM d, yyyy' }}.";
        private const string RsvpReminderBody = @"
{{ 'Global' | Attribute:'EmailHeader' }}

<h1>RSVP Reminder</h1>

<p>Hi {{  Person.NickName  }}!</p>

<p>This is just a reminder that you have RSVP'd for {{ Occurrence.Name }} on {{ Occurrence.OccurrenceDate | Date:'dddd, MMMM d, yyyy' }}.</p>

<p>Thanks!</p>

<p>{{ 'Global' | Attribute:'OrganizationName' }}</p>

{{ 'Global' | Attribute:'EmailFooter' }}
";
        #endregion Reminder Values

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            using ( var rockContext = new RockContext() )
            {
                var categoryService = new CategoryService( rockContext );
                var rsvpConfirmationCategory = categoryService.Get( Rock.SystemGuid.Category.SYSTEM_COMMUNICATION_RSVP_CONFIRMATION.AsGuid() );

                RockMigrationHelper.UpdateSystemCommunication(
                    rsvpConfirmationCategory.Name, "RSVP Reminder", "", "", "", "", "",
                    RsvpReminderSubject, RsvpReminderBody, RsvpReminderGuid, true, RsvpReminderSmsMessage );
            }
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
