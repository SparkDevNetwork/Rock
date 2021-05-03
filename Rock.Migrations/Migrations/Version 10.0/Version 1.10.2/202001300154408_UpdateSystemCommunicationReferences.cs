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
    using System.Collections.Generic;

    /// <summary>
    ///
    /// </summary>
    public partial class UpdateSystemCommunicationReferences : Rock.Migrations.RockMigration
    {
        private List<Guid> _WellKnownEntryGuidList;

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            InitializeLocalContext();

            UpdateSystemCommunicationWorkflowCategoryGuid();

            CopyEmailKeysToCommunicationKeysUp( "dbo.GroupType", "ScheduleConfirmationSystemEmailId", "ScheduleConfirmationSystemCommunicationId" );
            CopyEmailKeysToCommunicationKeysUp( "dbo.GroupType", "ScheduleReminderSystemEmailId", "ScheduleReminderSystemCommunicationId" );
            CopyEmailKeysToCommunicationKeysUp( "dbo.GroupSync", "WelcomeSystemEmailId", "WelcomeSystemCommunicationId" );
            CopyEmailKeysToCommunicationKeysUp( "dbo.GroupSync", "ExitSystemEmailId", "ExitSystemCommunicationId" );
            CopyEmailKeysToCommunicationKeysUp( "dbo.SignatureDocumentTemplate", "InviteSystemEmailId", "InviteSystemCommunicationId" );
            CopyEmailKeysToCommunicationKeysUp( "dbo.WorkflowActionForm", "NotificationSystemEmailId", "NotificationSystemCommunicationId" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // No actions.
        }

        /// <summary>
        /// Set the System Communication Category "Workflow" to a new well-known guid.
        /// </summary>
        private void UpdateSystemCommunicationWorkflowCategoryGuid()
        {
            string sql;

            sql = $@"
UPDATE [Category]
    SET [Guid] = '{ Rock.SystemGuid.Category.SYSTEM_COMMUNICATION_WORKFLOW }'
WHERE [Name] = 'Workflow'
    AND [EntityTypeId] IN ( SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.SystemCommunication' )
            ";

            Sql( sql );
        }

        /// <summary>
        /// Initialize the local execution context for this migration.
        /// </summary>
        private void InitializeLocalContext()
        {
            // Get the list of SystemCommunication identifiers that are well-known by Rock.
            // These identifiers are the same for the corresponding SystemEmail template.
            _WellKnownEntryGuidList = new List<Guid>()
            {
                SystemGuid.SystemCommunication.ASSESSMENT_REQUEST.AsGuid(),
                SystemGuid.SystemCommunication.ATTENDANCE_NOTIFICATION.AsGuid(),
                SystemGuid.SystemCommunication.CONFIG_EXCEPTION_NOTIFICATION.AsGuid(),
                SystemGuid.SystemCommunication.CONFIG_JOB_NOTIFICATION.AsGuid(),
                SystemGuid.SystemCommunication.FINANCE_PLEDGE_CONFIRMATION.AsGuid(),
                SystemGuid.SystemCommunication.GROUP_ATTENDANCE_REMINDER.AsGuid(),
                SystemGuid.SystemCommunication.NOTE_WATCH_NOTIFICATION.AsGuid(),
                SystemGuid.SystemCommunication.NOTE_APPROVAL_NOTIFICATION.AsGuid(),
                SystemGuid.SystemCommunication.PRAYER_REQUEST_COMMENTS_NOTIFICATION.AsGuid(),
                SystemGuid.SystemCommunication.REGISTRATION_NOTIFICATION.AsGuid(),
                SystemGuid.SystemCommunication.SCHEDULING_CONFIRMATION.AsGuid(),
                SystemGuid.SystemCommunication.SCHEDULING_REMINDER.AsGuid(),
                SystemGuid.SystemCommunication.SCHEDULING_RESPONSE.AsGuid(),
                SystemGuid.SystemCommunication.SECURITY_ACCOUNT_CREATED.AsGuid(),
                SystemGuid.SystemCommunication.SECURITY_CONFIRM_ACCOUNT.AsGuid(),
                SystemGuid.SystemCommunication.SECURITY_FORGOT_USERNAME.AsGuid(),
                SystemGuid.SystemCommunication.SPARK_DATA_NOTIFICATION.AsGuid(),
                SystemGuid.SystemCommunication.WORKFLOW_FORM_NOTIFICATION.AsGuid(),
                SystemGuid.SystemCommunication.COMMUNICATION_QUEUE.AsGuid(),
                SystemGuid.SystemCommunication.FINANCE_EXPIRING_CREDIT_CARD.AsGuid(),
                SystemGuid.SystemCommunication.FINANCE_FAILED_PAYMENT.AsGuid(),
                SystemGuid.SystemCommunication.FINANCE_GIVING_RECEIPT.AsGuid(),
                SystemGuid.SystemCommunication.FOLLOWING_EVENT.AsGuid(),
                SystemGuid.SystemCommunication.FOLLOWING_SUGGESTION.AsGuid(),
                SystemGuid.SystemCommunication.GROUP_MEMBER_ABSENCE.AsGuid(),
                SystemGuid.SystemCommunication.GROUP_REQUIREMENTS.AsGuid(),
                SystemGuid.SystemCommunication.GROUP_PENDING_MEMBERS.AsGuid(),
                SystemGuid.SystemCommunication.GROUP_SYNC_WELCOME.AsGuid(),
                SystemGuid.SystemCommunication.GROUP_SYNC_EXIT.AsGuid(),
                SystemGuid.SystemCommunication.KIOSK_INFO_UPDATE.AsGuid()
            };
        }

        /// <summary>
        /// Copy foreign key references from an EmailId field to a CommunicationId field for well-known system templates.
        /// [Id] values are identical for records in both the SystemEmail and SystemCommunication tables.
        /// This fixes an issue with the previous migration which attempted this incorrectly.
        /// </summary>
        private void CopyEmailKeysToCommunicationKeysUp( string tableName, string emailIdField, string communicationIdField )
        {
            var sql = $@"
UPDATE { tableName }
        SET { communicationIdField } = { emailIdField }
WHERE {emailIdField } IN ( SELECT [Id] FROM [SystemEmail] WHERE [Guid] IN ( '{ _WellKnownEntryGuidList.AsDelimited( "','" ) }' ) )
            ";

            Sql( sql );
        }

    }
}
