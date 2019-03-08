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
using System.Data.Entity.Migrations;

using Rock;

namespace Rock.Migrations
{
    /// <summary>
    ///
    /// </summary>
    public partial class AttendancedCheckedInBy : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Attendance", "CheckedInByPersonAliasId", c => c.Int());

            // Update the "Scanned Id" and "Family Id" options to not be displayed in check-in configuratoin (they are always supported).
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.CHECKIN_SEARCH_TYPE, SystemGuid.FieldType.BOOLEAN, "User Selectable", "UserSelectable", "Should this search type be displayed as an available option when configuring check-in?", 0, "true", "72C3C999-D243-4500-8EB0-2913069E2B28" );
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_SCANNED_ID, "72C3C999-D243-4500-8EB0-2913069E2B28", "false" );
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_FAMILY_ID, "72C3C999-D243-4500-8EB0-2913069E2B28", "false" );

            // Add an 'Alternate Id' person search key and configure it to not be available in the advanced settings of person detail
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.PERSON_SEARCH_KEYS, SystemGuid.FieldType.BOOLEAN, "User Selectable", "UserSelectable", "Should user be able to add a search value of this type on person edit page?", 0, "true", "15C419AA-76A9-4105-AB99-8384AB0E9B44" );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.PERSON_SEARCH_KEYS, "Alternate Id", "An alternate identifier is used for things like check-in (bar-code, fingerprint id, etc.)", SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID );
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID, "15C419AA-76A9-4105-AB99-8384AB0E9B44", "false" );

            // Update attributes to show in grid
            Sql( @"
    UPDATE 
		[Attribute]
    SET 
		[IsGridColumn] = 1
    WHERE 
		[Guid] IN ( '72C3C999-D243-4500-8EB0-2913069E2B28', '15C419AA-76A9-4105-AB99-8384AB0E9B44' )
" );

            // Update any checkin configs that were using scanned or family id search type to use name/phone (that's what it would have been using anyway).
            Sql( $@"
    DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'BBF88ADA-3C2E-4A0B-A9AE-5B07D4557129' )
    UPDATE
        [AttributeValue]
    SET
        [Value] = '{SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME_AND_PHONE.ToLower()}'
    WHERE
        [AttributeId] = @AttributeId
        AND ( [Value] = '{SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_SCANNED_ID}' OR [Value] = '{SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_FAMILY_ID}' )
" );

            // Job for Migrating Family Check-in Identifiers to the head-of-household (schedule for 9pm to avoid conflict with AppPoolRecycle)
            Sql( $@"
    INSERT INTO [dbo].[ServiceJob]
           ([IsSystem]
           ,[IsActive]
           ,[Name]
           ,[Description]
           ,[Class]
           ,[CronExpression]
           ,[NotificationStatus]
           ,[Guid])
     VALUES
        (0	
         ,1	
         ,'Move Family Check-in Identifiers to Person'
         ,'Moves family check-in identifiers to Person alternate ids. All the family ids will be associated with the families head of houshold.'
         ,'Rock.Jobs.MigrateFamilyAlternateId'
         ,'0 0 21 1/1 * ? *'
         ,3
         ,'{ SystemGuid.ServiceJob.MIGRATE_FAMILY_CHECKIN_IDS }')" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Attendance", "CheckedInByPersonAliasId");
        }
    }
}
