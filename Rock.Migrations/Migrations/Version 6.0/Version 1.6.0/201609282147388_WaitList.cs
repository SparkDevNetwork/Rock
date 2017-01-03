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
    
    /// <summary>
    ///
    /// </summary>
    public partial class WaitList : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.RegistrationRegistrant", "OnWaitList", c => c.Boolean(nullable: false));
            AddColumn("dbo.RegistrationTemplate", "WaitListEnabled", c => c.Boolean(nullable: false));

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.EventCalendarItem", "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D", "EventCalendarId", "1", "Twitter Photo", "Recommended size 440px x 220px.", 0, "", "8D9E95E2-D648-4091-8CCC-1C20A04852C3", "core_calendar_TwitterPhoto" );
            RockMigrationHelper.UpdateAttributeQualifier( "8D9E95E2-D648-4091-8CCC-1C20A04852C3", "binaryFileType", "c1142570-8cd6-4a20-83b1-acb47c1cd377", "2A489607-B61B-4F2B-B836-FEF85132981C" );
            RockMigrationHelper.UpdateAttributeQualifier( "8D9E95E2-D648-4091-8CCC-1C20A04852C3", "formatAsLink", "False", "BC15C93F-08AB-44A8-AC67-2C0D644C3C9C" );

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.EventCalendarItem", "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D", "EventCalendarId", "1", "Facebook Photo", "Recommended Size 1200px x 630px.", 1, "", "7B13726E-259A-49C8-97F9-F55094E32046", "core_calendar_FacebookPhoto" );
            RockMigrationHelper.UpdateAttributeQualifier( "7B13726E-259A-49C8-97F9-F55094E32046", "binaryFileType", "c1142570-8cd6-4a20-83b1-acb47c1cd377", "E485E4D2-E1E6-4736-9ADB-CAA3AAE1CC75" );
            RockMigrationHelper.UpdateAttributeQualifier( "7B13726E-259A-49C8-97F9-F55094E32046", "formatAsLink", "False", "95798E17-8EB0-4A1A-B3D4-E126E82F5464" );

            Sql( @"
    DECLARE @PublicCalendarId int = (SELECT TOP 1 [Id] FROM [EventCalendar] WHERE [Guid] = '8A444668-19AF-4417-9C74-09F842572974' )
    UPDATE [Attribute] SET [EntityTypeQualifierValue] = CAST( @PublicCalendarId AS varchar )
    WHERE [Key] IN ( 'core_calendar_TwitterPhoto', 'core_calendar_FacebookPhoto' )
" );

            // Fix any non-unique RockInstanceIds
            Sql( @"
    UPDATE [Attribute] SET [Guid] = NEWID()
    WHERE [Key] = 'RockInstanceId'
    AND [Guid] IN ( 
        '5AC1BAF1-3687-40C2-ACFA-FF8591ED51F2', -- 5.1 Install
        '2F4A4BA9-A62F-4EE7-A34F-660F11D9D6E7', -- 4.1 Install
        '624AC46B-8EAA-41D1-95BE-831B5200E48B', -- 3.1 Install
        '4989608F-8479-418B-90EE-3373400344BE'  -- ?
    )
" );
            // clear migration table
            Sql( @"
    UPDATE [__MigrationHistory] SET [Model] = 0x
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.RegistrationTemplate", "WaitListEnabled");
            DropColumn("dbo.RegistrationRegistrant", "OnWaitList");
        }
    }
}
