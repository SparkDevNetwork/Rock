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
    using Rock.Security;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20231005 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            NoteMentionSearchSecurityUp();
            PrayerRequestsPrayedMobileSecurityUp();
            AddViewProtectinoProfileToPersonBioSummaryBlock();
            UpdateFamilyPreRegistrationSuccessHtmlContent();
            UpdateReminderNotificationSystemCommunicationBody();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            PrayerRequestsPrayedMobileSecurityDown();
            NoteMentionSearchSecurityDown();
            RevertFamilyPreRegistrationSuccessHtmlContent();
        }

        /// <summary>
        /// DH: Fix default security on new Mention Search API endpoint
        /// </summary>
        private void NoteMentionSearchSecurityUp()
        {
            RockMigrationHelper.AddRestAction( "dca338b6-9749-427e-8238-1686c9587d16", "Controls", "Rock.Rest.v2.ControlsController" );

            RockMigrationHelper.AddSecurityAuthForRestAction( "dca338b6-9749-427e-8238-1686c9587d16",
                0,
                Rock.Security.Authorization.EDIT, // For POST method
                true,
                string.Empty,
                Rock.Model.SpecialRole.AllUsers,
                "11615413-430e-4579-8e6c-26537e6fd473" );

            RockMigrationHelper.AddSecurityAuthForRestAction( "dca338b6-9749-427e-8238-1686c9587d16",
                0,
                "FullSearch",
                true,
                Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS,
                Rock.Model.SpecialRole.None,
                "0764dae6-8915-452a-95d3-f2cdf5999892" );

            RockMigrationHelper.AddSecurityAuthForRestAction( "dca338b6-9749-427e-8238-1686c9587d16",
                0,
                "FullSearch",
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_USERS,
                Rock.Model.SpecialRole.None,
                "575c92ac-1602-4d75-9c02-1a8259dc2ca9" );

            RockMigrationHelper.AddSecurityAuthForRestAction( "dca338b6-9749-427e-8238-1686c9587d16",
                0,
                "FullSearch",
                true,
                Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
                Rock.Model.SpecialRole.None,
                "0858c8b1-1359-49d8-b3a6-6c6e03fa3643" );

            RockMigrationHelper.AddSecurityAuthForRestAction( "dca338b6-9749-427e-8238-1686c9587d16",
                0,
                "FullSearch",
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                Rock.Model.SpecialRole.None,
                "7a3dc6a3-c9d7-48c9-ac36-f64ada7aaf07" );

            RockMigrationHelper.AddSecurityAuthForRestAction( "dca338b6-9749-427e-8238-1686c9587d16",
                0,
                "FullSearch",
                false,
                string.Empty,
                Rock.Model.SpecialRole.AllUsers,
                "de33af64-a72a-4ba7-a8ee-9825ba6e73f4" );
        }

        /// <summary>
        /// DH: Fix default security on new Mention Search API endpoint
        /// </summary>
        private void NoteMentionSearchSecurityDown()
        {
            RockMigrationHelper.DeleteSecurityAuth( "de33af64-a72a-4ba7-a8ee-9825ba6e73f4" );
            RockMigrationHelper.DeleteSecurityAuth( "7a3dc6a3-c9d7-48c9-ac36-f64ada7aaf07" );
            RockMigrationHelper.DeleteSecurityAuth( "0858c8b1-1359-49d8-b3a6-6c6e03fa3643" );
            RockMigrationHelper.DeleteSecurityAuth( "575c92ac-1602-4d75-9c02-1a8259dc2ca9" );
            RockMigrationHelper.DeleteSecurityAuth( "0764dae6-8915-452a-95d3-f2cdf5999892" );
            RockMigrationHelper.DeleteSecurityAuth( "11615413-430e-4579-8e6c-26537e6fd473" );
        }

        /// <summary>
        /// BC: Set default security on Prayed API endpoint
        /// </summary>
        private void PrayerRequestsPrayedMobileSecurityUp()
        {
            RockMigrationHelper.AddRestAction( "9696DB0A-4CCC-4530-BC8D-E4E54A438BFA", "PrayerRequests", "Rock.Rest.Controllers.PrayerRequestsController" );

            RockMigrationHelper.AddSecurityAuthForRestAction( "9696DB0A-4CCC-4530-BC8D-E4E54A438BFA",
                0,
                Rock.Security.Authorization.EDIT, // For PUT method
                true,
                Rock.SystemGuid.Group.GROUP_MOBILE_APPLICATION_USERS,
                Rock.Model.SpecialRole.None,
                "9be860e6-158f-40f4-a53d-cfb8c8b3bb56" );
        }

        /// <summary>
        /// BC: Set default security on Prayed API endpoint
        /// </summary>
        private void PrayerRequestsPrayedMobileSecurityDown()
        {
            RockMigrationHelper.DeleteSecurityAuth( "9be860e6-158f-40f4-a53d-cfb8c8b3bb56" );
        }

        /// <summary>
        /// PA: Add View Protection Profile To Person Bio Summary Block
        /// </summary>
        private void AddViewProtectinoProfileToPersonBioSummaryBlock()
        {
            // Removing the ViewProtectionProfile Auth which were added previously
            Sql( @"DECLARE @EntityTypeId INT = (
            		SELECT TOP 1 [Id]
            		FROM [EntityType]
            		WHERE [Name] = 'Rock.Model.Block'
            		)
            DECLARE @PersonBioBlockId INT = (
            		SELECT TOP 1 [Id]
            		FROM [Block]
            		WHERE [Guid] = '1E6AF671-9C1A-4C6C-8156-36B6D7589F34'
            		)
            DECLARE @PersonEditBlockId INT = (
            		SELECT TOP 1 [Id]
            		FROM [Block]
            		WHERE [Guid] = '59C7EA79-2073-4EA9-B439-7E74F06E8F5B'
            		)
            DECLARE @RockAdminGroupId INT = (
            		SELECT TOP 1 [Id]
            		FROM [Group]
            		WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E'
            		)
            DECLARE @StaffWorkersGroupId INT = (
            		SELECT TOP 1 [Id]
            		FROM [Group]
            		WHERE [Guid] = '2C112948-FF4C-46E7-981A-0257681EADF4'
            		)
            DECLARE @StaffLikeWorkersGroupId INT = (
            		SELECT TOP 1 [Id]
            		FROM [Group]
            		WHERE [Guid] = '300BA2C8-49A3-44BA-A82A-82E3FD8C3745'
            		)

            UPDATE [Auth] SET [AllowOrDeny] = 'A' WHERE [Action] = 'ViewProtectionProfile' 
                AND [SpecialRole] = 1 AND [Groupid] IS NULL
                AND [EntityTypeId] = 9 AND [EntityId] IN (@PersonBioBlockId, @PersonEditBlockId);
            
            DELETE FROM [Auth]
                WHERE [Auth].[GroupId] IN (@RockAdminGroupId, @StaffWorkersGroupId, @StaffLikeWorkersGroupId)
                AND [Auth].[Action] = 'ViewProtectionProfile' AND [Auth].[EntityTypeId] = @EntityTypeId 
                AND [Auth].[EntityId] IN (@PersonBioBlockId, @PersonEditBlockId);" );

            // Add Auth as Allow for All Users for Person Summary Bio Block
            RockMigrationHelper.AddSecurityAuthForBlock( "C9523ABF-7FFA-4F43-ACEE-EE20D5D2C9E5", 1, Authorization.VIEW_PROTECTION_PROFILE, true, null, Model.SpecialRole.AllUsers, "746A89DD-D4BC-40F2-9802-7D00B78D87C1" );
        }

        /// <summary>
        /// JMH: Update the Family Pre-Registration Success HTML content.
        /// </summary>
        private void UpdateFamilyPreRegistrationSuccessHtmlContent()
        {
            Sql( $@"DECLARE @BlockId AS INT = (SELECT [Id] FROM [Block] WHERE [Guid] = 'DC006503-C69E-49CC-B384-EB199AFED5BD')

UPDATE [HtmlContent]
   SET [Content] = REPLACE([Content], N'<h4>We''re excited to see you on {{{{ when | Date:''dddd'' }}}}!</h4>', N'//- CHANGE:3CB90F70-C3E4-49EB-A1B5-058E7EC1C640 BEGIN
{{%- if when == null or when == '''' -%}}
<h4>We''re excited to see you!</h4>
{{%- else -%}}
    //- Truncate the time.
    {{%- assign whenDate = when | Date:''MM-dd-yyyy'' -%}}
    {{%- assign dayDiff = ''now'' | Date:''MM-dd-yyyy'' | DateDiff:whenDate,''d'' | AsInteger -%}}
    {{%- if dayDiff == 0 -%}}
<h4>We''re excited to see you today!</h4>
    {{%- elseif dayDiff > 0 and dayDiff < 7 -%}}
<h4>We''re excited to see you on {{{{ when | Date:''dddd'' }}}}!</h4>
    {{%- elseif dayDiff >= 7 -%}}
<h4>We''re excited to see you on {{{{ when | Date:''MMMM'' }}}} {{{{ when | Date:''d'' | NumberToOrdinal }}}}!</h4>
    {{%- endif -%}}
{{%- endif -%}}
//- CHANGE:3CB90F70-C3E4-49EB-A1B5-058E7EC1C640 END ')
  FROM [HtmlContent] 
 WHERE [BlockId] = @BlockId
       AND [Content] NOT LIKE N'%CHANGE:3CB90F70-C3E4-49EB-A1B5-058E7EC1C640%'" );
        }

        /// <summary>
        /// JMH: Revert the Family Pre-Registration Success HTML content.
        /// </summary>
        private void RevertFamilyPreRegistrationSuccessHtmlContent()
        {
            RockMigrationHelper.UpdateHtmlContentBlock(
                "DC006503-C69E-49CC-B384-EB199AFED5BD",
                @"{% assign when = PageParameter.When %}
<h2>Thank-you for Registering!</h2>
<h4>We''re excited to see you on {{ when | Date:''dddd'' }}!</h4>
<br/><br/>
<h4>Now What?</h4>
<p>When you arrive, just head to the Children''s Ministry Check-in Desk to check-in your children.</p>
<p>If you have any questions when you are trying to check in children, please see a volunteer to help you.</p>
<p>You will receive a tag to place on each child, as well as a tag for you to use to pick up your children after the service.</p>
<p>Then, just take your children to the room listed on their tag.</p>
<p>When the service is over, return to the same room where you dropped off your children and present your other tag to check them out.</p>",
                "D99BFE10-72A8-4349-A838-860EC34516D8" );
        }

        /// <summary>
        /// PA: Update the Body of Reminder Notification System Communication
        /// </summary>
        private void UpdateReminderNotificationSystemCommunicationBody()
        {
            Sql( $@"UPDATE [SystemCommunication] SET [Body]=REPLACE([Body], 'Below are {{ MaxRemindersPerEntityType }} of the most recent reminders', 'Below are the most recent reminders') WHERE [Guid] = '7899958C-BC2F-499E-A5CC-11DE1EF8DF20'" );
        }
    }
}
