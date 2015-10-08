// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    
    /// <summary>
    ///
    /// </summary>
    public partial class RegistrationReminder : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.RegistrationInstance", "SendReminderDateTime", c => c.DateTime());
            AddColumn("dbo.RegistrationInstance", "ReminderSent", c => c.Boolean(nullable: false));
            AddColumn("dbo.RegistrationTemplate", "ConfirmationFromName", c => c.String(maxLength: 200));
            AddColumn("dbo.RegistrationTemplate", "ConfirmationFromEmail", c => c.String(maxLength: 200));
            AddColumn("dbo.RegistrationTemplate", "ConfirmationSubject", c => c.String(maxLength: 200));
            AddColumn("dbo.RegistrationTemplate", "ReminderFromName", c => c.String(maxLength: 200));
            AddColumn("dbo.RegistrationTemplate", "ReminderFromEmail", c => c.String(maxLength: 200));
            AddColumn("dbo.RegistrationTemplate", "ReminderSubject", c => c.String(maxLength: 200));
            DropColumn("dbo.RegistrationInstance", "ReminderSentDateTime");
            DropColumn("dbo.RegistrationInstance", "ConfirmationSentDateTime");
            DropColumn("dbo.RegistrationTemplate", "UseDefaultConfirmationEmail");

            // Remove confirmation system email if it happened to get created ( was removed from previous migration )
            RockMigrationHelper.DeleteSystemEmail( "7B0F4F06-69BD-4CB4-BD04-8DA3779D5259" );

            Sql( @"INSERT INTO [ServiceJob] ( [IsSystem], [IsActive], [Name], [Description], [Assembly], [Class], [CronExpression], [Guid], [NotificationStatus] )
                 VALUES (0,1,'Registration Reminder','Send any registration reminders that are due to be sent.','','Rock.Jobs.SendRegistrationReminders','0 0 0/1 1/1 * ? *','56B1390A-0E12-40F0-BE5F-0445FF4AA0E0',3)" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            Sql( "DELETE [ServiceJob] WHERE [Guid] = '56B1390A-0E12-40F0-BE5F-0445FF4AA0E0'" );

            AddColumn( "dbo.RegistrationTemplate", "UseDefaultConfirmationEmail", c => c.Boolean( nullable: false ) );
            AddColumn("dbo.RegistrationInstance", "ConfirmationSentDateTime", c => c.DateTime());
            AddColumn("dbo.RegistrationInstance", "ReminderSentDateTime", c => c.DateTime());
            DropColumn("dbo.RegistrationTemplate", "ReminderSubject");
            DropColumn("dbo.RegistrationTemplate", "ReminderFromEmail");
            DropColumn("dbo.RegistrationTemplate", "ReminderFromName");
            DropColumn("dbo.RegistrationTemplate", "ConfirmationSubject");
            DropColumn("dbo.RegistrationTemplate", "ConfirmationFromEmail");
            DropColumn("dbo.RegistrationTemplate", "ConfirmationFromName");
            DropColumn("dbo.RegistrationInstance", "ReminderSent");
            DropColumn("dbo.RegistrationInstance", "SendReminderDateTime");
        }
    }
}
