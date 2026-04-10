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

    /// <summary>
    ///
    /// </summary>
    public partial class RemoveLegacySystemEmailPagesBlocksEtc : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Remove Block: "System Email Detail", from Page: System Email Details
            RockMigrationHelper.DeleteBlock( "707A99EB-C24A-46BB-9230-8607E674246C" );

            // Remove Block: "System Email List", from Page: System Emails
            RockMigrationHelper.DeleteBlock( "68F10E30-BD74-49F5-B63F-DA671E31DA90" );

            // Remove Block: legacy 'System Email' Categories block
            RockMigrationHelper.DeleteBlock( "624AE4BC-0A47-46EA-A078-E020BF3EF683" );

            // Remove BlockType: ~/Blocks/Communication/SystemEmailDetail.ascx 244
            RockMigrationHelper.DeleteBlockType( "82B00455-B8CF-4673-ACF5-641B961DF59F" ); // should cascade if other instances exist

            // Remove BlockType: ~/Blocks/Communication/SystemEmailList.ascx 245
            RockMigrationHelper.DeleteBlockType( "2645A264-D5E5-43E8-8FE2-D351F3D5435B" ); // should cascade if other instances exist

            // Remove route for System Emails (Legacy) [the list]
            RockMigrationHelper.DeletePageRoute( "4E800BB8-5769-65CB-65E0-C545E4C06E1D" );

            // Remove route for System Email Details (Legacy)
            RockMigrationHelper.DeletePageRoute( "3928CD60-6B98-A597-492C-554A86083B8F" );

            // Remove route for legacy System Email Categories 
            RockMigrationHelper.DeletePageRoute( "553ACDA8-6BEF-4918-958A-AEF3163681CD" );

            // Remove Page: 'System Email Categories' 361
            RockMigrationHelper.DeletePage( "66FAF7A6-7523-475C-A88D-51C75178A785" );

            // Remove Page: System Email Details (Legacy) 269
            RockMigrationHelper.DeletePage( "588C72A8-7DEC-405F-BA4A-FE64F87CB817" );

            // Remove Page: System Emails (Legacy) [the list] 61
            RockMigrationHelper.DeletePage( "89B7A631-EA6F-4DA3-9380-04EE67B63E9E" );

            // Group Sync references to SystemEmail
            DropForeignKey( "dbo.GroupSync", "ExitSystemEmailId", "dbo.SystemEmail" );
            DropForeignKey( "dbo.GroupSync", "WelcomeSystemEmailId", "dbo.SystemEmail" );

            DropIndex( "dbo.GroupSync", new[] { "ExitSystemEmailId" } );
            DropIndex( "dbo.GroupSync", new[] { "WelcomeSystemEmailId" } );

            DropColumn( "dbo.GroupSync", "ExitSystemEmailId" );
            DropColumn( "dbo.GroupSync", "WelcomeSystemEmailId" );

            // GroupType references to SystemEmail
            DropForeignKey( "dbo.GroupType", "ScheduleConfirmationSystemEmailId", "dbo.SystemEmail" );
            DropForeignKey( "dbo.GroupType", "ScheduleReminderSystemEmailId", "dbo.SystemEmail" );

            DropIndex( "dbo.GroupType", new[] { "ScheduleConfirmationSystemEmailId" } );
            DropIndex( "dbo.GroupType", new[] { "ScheduleReminderSystemEmailId" } );

            DropColumn( "dbo.GroupType", "ScheduleConfirmationSystemEmailId" );
            DropColumn( "dbo.GroupType", "ScheduleReminderSystemEmailId" );

            // SignatureDocumentTemplate references to SystemEmail
            DropForeignKey( "dbo.SignatureDocumentTemplate", "InviteSystemEmailId", "dbo.SystemEmail" );
            DropIndex( "dbo.SignatureDocumentTemplate", new[] { "InviteSystemEmailId" } );
            DropColumn( "dbo.SignatureDocumentTemplate", "InviteSystemEmailId" );

            // WorkflowActionForm references to SystemEmail
            DropForeignKey( "dbo.WorkflowActionForm", "NotificationSystemEmailId", "dbo.SystemEmail" );
            DropIndex( "dbo.WorkflowActionForm", new[] { "NotificationSystemEmailId" } );
            DropColumn( "dbo.WorkflowActionForm", "NotificationSystemEmailId" );

            /*
                 2/10/2026 - NA

                 We are intentionally NOT dropping dbo.SystemEmail at this time, even though Rock core no longer
                 references it after removing the related model properties and foreign keys/columns.

                 Some RockShop plugins still have older migrations that run against existing databases and
                 expect dbo.SystemEmail to be present. Those migrations query the table directly (ie. with
                 INNER JOINs). If we drop dbo.SystemEmail, those plugin migrations can fail hard during install
                 or upgrade because the table no longer exists, which blocks the entire migration pipeline.

                 Keeping the table (even if unused by core) preserves backward compatibility for RockShop
                 plugin migrations that have not yet been updated. Once we have reasonable confidence that
                 active plugins no longer depend on dbo.SystemEmail (or RockShop has been updated to prevent
                 running legacy migrations against newer schemas), we can revisit dropping the table in a
                 future major version.

                 Reason: Avoid breaking RockShop plugin installs/upgrades that still reference dbo.SystemEmail
                 in legacy migrations.
            */
            // DropTable( "dbo.SystemEmail" ); // DO NOT DROP this table at this time.


            // Remove all but one record from the SystemEmail table
            Sql( @"
DECLARE @SystemEmailGuid UNIQUEIDENTIFIER = '75CB0A4A-B1C5-4958-ADEB-8621BD231520';

-- Remove any other legacy records (We are commenting this out due to potential plugins that are still referencing records in this table)
-- DELETE FROM [SystemEmail]
-- WHERE [Guid] <> @SystemEmailGuid;

-- Update the retained compatibility record
UPDATE [SystemEmail]
SET 
    [CategoryId] = NULL,
    [Title] = 'SystemEmail Table Obsolete (Retained for Compatibility)',
    [Subject] = 'The SystemEmail table is no longer used by Rock core but has been retained to prevent breaking existing RockShop plugins.',
    [Body] = 'The SystemEmail table has been obsoleted and is no longer referenced by Rock core functionality. However, it has not been removed from the database.

Some RockShop plugins include legacy migrations that still reference this table. Dropping it could cause those plugins to fail during installation or upgrade. For that reason, the table has been intentionally retained out of an abundance of caution.

This table should be considered deprecated and will be removed in a future major version once compatibility concerns have been resolved.'
-- WHERE [Guid] = @SystemEmailGuid;  NOTE: We are doing this for all remaining SystemEmail records.
" );

            // Remove the legacy EntityType and any associated Categories
            Sql( $@"
-- First, fix bad SystemCommunication that is using a SystemEmail category
DECLARE @SystemCommunicationEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '{Rock.SystemGuid.EntityType.SYSTEM_COMMUNICATION}');

DECLARE @SystemCommunicationForSystemCategoryId INT = (SELECT [Id] FROM [Category] WHERE [Name] = 'System' AND [EntityTypeId] = @SystemCommunicationEntityTypeId );
UPDATE [SystemCommunication] SET [CategoryId] = @SystemCommunicationForSystemCategoryId  WHERE [Guid] = 'FAEA9DE5-62CE-4EEE-960B-C06103E97AA9';

-- Now, clear any category values associated to the legacy SystemEmail entity type
DECLARE @SystemEmailEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'B21FD119-893E-46C0-B42D-E4CDD5C8C49D');

-- Also NULL out any other SystemCommunication records that reference Categories
-- incorrectly tied to the legacy SystemEmail entity type (which might have come from Plugins)
UPDATE sc
SET sc.[CategoryId] = NULL
FROM [SystemCommunication] sc
INNER JOIN [Category] c ON sc.[CategoryId] = c.[Id]
WHERE c.[EntityTypeId] = @SystemEmailEntityTypeId;

DELETE [Category] WHERE [EntityTypeId] = @SystemEmailEntityTypeId;

-- Finally, remove the legacy SystemEmail entity type
DELETE FROM [EntityType]
WHERE [Id] = @SystemEmailEntityTypeId
    AND [Name] = 'Rock.Model.SystemEmail';

-- Now, look for any attributes that were using the legacy FieldType and convert them to text
DECLARE @TextFieldTypeId INT = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA' ); -- Text
DECLARE @SystemEmailFieldTypeId INT = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF' ); -- Rock.Field.Types.SystemEmailFieldType
UPDATE [Attribute] SET [FieldTypeId] = @TextFieldTypeId WHERE [FieldTypeId] = @SystemEmailFieldTypeId;
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // We are not supporting the re-addition of the SystemEmail entity.
        }
    }
}
