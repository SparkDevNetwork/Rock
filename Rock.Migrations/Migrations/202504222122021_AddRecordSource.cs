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

    /// <summary>
    ///
    /// </summary>
    public partial class AddRecordSource : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JPH_AddRecordSourceColumns_20250407_Up();
            JPH_AddRecordSourceDefinedTypeAndValues_20250407_Up();
            JPH_AddRecordSourceBlockAttributes_20250407_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JPH_AddRecordSourceColumns_20250407_Down();
            JPH_AddRecordSourceDefinedTypeAndValues_20250407_Down();
            JPH_AddRecordSourceBlockAttributes_20250407_Down();
        }

        /// <summary>
        /// JPH: Add record source columns - up.
        /// </summary>
        private void JPH_AddRecordSourceColumns_20250407_Up()
        {
            AddColumn( "dbo.Person", "RecordSourceValueId", c => c.Int() );
            AddColumn( "dbo.Group", "GroupMemberRecordSourceValueId", c => c.Int() );
            AddColumn( "dbo.GroupType", "GroupMemberRecordSourceValueId", c => c.Int() );
            AddColumn( "dbo.GroupType", "AllowGroupSpecificRecordSource", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.WorkflowActionForm", "PersonEntryRecordSourceValueId", c => c.Int() );
            AddColumn( "dbo.RegistrationInstance", "RegistrantRecordSourceValueId", c => c.Int() );
            AddColumn( "dbo.RegistrationTemplate", "RegistrantRecordSourceValueId", c => c.Int() );

            AddForeignKey( "dbo.Group", "GroupMemberRecordSourceValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.WorkflowActionForm", "PersonEntryRecordSourceValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.GroupType", "GroupMemberRecordSourceValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.RegistrationInstance", "RegistrantRecordSourceValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.RegistrationTemplate", "RegistrantRecordSourceValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.Person", "RecordSourceValueId", "dbo.DefinedValue", "Id" );
        }

        /// <summary>
        /// JPH: Add record source columns - down.
        /// </summary>
        private void JPH_AddRecordSourceColumns_20250407_Down()
        {
            DropForeignKey( "dbo.Person", "RecordSourceValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.RegistrationTemplate", "RegistrantRecordSourceValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.RegistrationInstance", "RegistrantRecordSourceValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.GroupType", "GroupMemberRecordSourceValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.WorkflowActionForm", "PersonEntryRecordSourceValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Group", "GroupMemberRecordSourceValueId", "dbo.DefinedValue" );

            DropColumn( "dbo.RegistrationTemplate", "RegistrantRecordSourceValueId" );
            DropColumn( "dbo.RegistrationInstance", "RegistrantRecordSourceValueId" );
            DropColumn( "dbo.WorkflowActionForm", "PersonEntryRecordSourceValueId" );
            DropColumn( "dbo.GroupType", "AllowGroupSpecificRecordSource" );
            DropColumn( "dbo.GroupType", "GroupMemberRecordSourceValueId" );
            DropColumn( "dbo.Group", "GroupMemberRecordSourceValueId" );
            DropColumn( "dbo.Person", "RecordSourceValueId" );
        }

        /// <summary>
        /// JPH: Add record source defined type and values - up.
        /// </summary>
        private void JPH_AddRecordSourceDefinedTypeAndValues_20250407_Up()
        {
            RockMigrationHelper.AddDefinedType(
                category: "Global",
                name: "Record Source",
                description: "Describes how newly-created records are coming into Rock (e.g. Person records created as a result of Event Registration).",
                guid: Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE
            );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE,
                value: "External Website",
                description: "Describes a record that was created from interaction with an external website.",
                guid: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_EXTERNAL_WEBSITE,
                isSystem: true
            );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE,
                value: "Prayer",
                description: "Describes a record that was created from a prayer request submission.",
                guid: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_PRAYER,
                isSystem: true
            );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE,
                value: "Event Registration",
                description: "Describes a record that was created from an event registration.",
                guid: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_EVENT_REGISTRATION,
                isSystem: true
            );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE,
                value: "Workflow",
                description: "Describes a record that was created from a workflow.",
                guid: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_WORKFLOW,
                isSystem: true
            );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE,
                value: "Group Registration",
                description: "Describes a record that was created from a group registration.",
                guid: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_GROUP_REGISTRATION,
                isSystem: true
            );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE,
                value: "Giving",
                description: "Describes a record that was created from a giving submission.",
                guid: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_GIVING,
                isSystem: true
            );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE,
                value: "Pledge",
                description: "Describes a record that was created from a pledge submission.",
                guid: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_PLEDGE,
                isSystem: true
            );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE,
                value: "Sign-up",
                description: "Describes a record that was created from a sign-up registration.",
                guid: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_SIGN_UP,
                isSystem: true
            );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE,
                value: "Check-in",
                description: "Describes a record that was created from check-in.",
                guid: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_CHECK_IN,
                isSystem: true
            );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE,
                value: "Family Registration",
                description: "Describes a record that was created from a family registration.",
                guid: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_FAMILY_REGISTRATION,
                isSystem: true
            );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE,
                value: "Mobile App",
                description: "Describes a record that was created from interaction with a mobile app.",
                guid: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_MOBILE_APP,
                isSystem: true
            );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE,
                value: "Apple TV App",
                description: "Describes a record that was created from interaction with an Apple TV app.",
                guid: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_APPLE_TV_APP,
                isSystem: true
            );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE,
                value: "Roku TV App",
                description: "Describes a record that was created from interaction with a Roku TV app.",
                guid: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_ROKU_TV_APP,
                isSystem: true
            );
        }

        /// <summary>
        /// JPH: Add record source defined type and values - down.
        /// </summary>
        private void JPH_AddRecordSourceDefinedTypeAndValues_20250407_Down()
        {
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_EXTERNAL_WEBSITE );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_PRAYER );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_EVENT_REGISTRATION );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_WORKFLOW );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_GROUP_REGISTRATION );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_GIVING );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_PLEDGE );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_SIGN_UP );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_CHECK_IN );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_FAMILY_REGISTRATION );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_MOBILE_APP );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_APPLE_TV_APP );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_ROKU_TV_APP );

            RockMigrationHelper.DeleteDefinedType( Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE );
        }

        /// <summary>
        /// JPH: Add record source block attributes - up.
        /// </summary>
        private void JPH_AddRecordSourceBlockAttributes_20250407_Up()
        {
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                blockTypeGuid: "E5C34503-DDAD-4881-8463-0E1E20B1675D", // AccountEntry (Obsidian)
                fieldTypeGuid: Rock.SystemGuid.FieldType.DEFINED_VALUE,
                name: "Record Source",
                key: "RecordSource",
                abbreviatedName: "Record Source",
                description: "The record source to use for new individuals (default = 'External Website'). If a 'RecordSource' page parameter is found, it will be used instead.",
                order: 15,
                defaultValue: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_EXTERNAL_WEBSITE,
                guid: "11975898-16E5-41A2-A077-015E7CD92B8F"
            );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                blockTypeGuid: "C03CE9ED-8572-4BE5-AB2A-FF7498494905", // FamilyPreRegistration (Obsidian)
                fieldTypeGuid: Rock.SystemGuid.FieldType.DEFINED_VALUE,
                name: "Record Source",
                key: "RecordSource",
                abbreviatedName: "Record Source",
                description: "The record source to use for new individuals (default = 'Family Registration'). If a 'RecordSource' page parameter is found, it will be used instead.",
                order: 13,
                defaultValue: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_FAMILY_REGISTRATION,
                guid: "1A9D8D16-4AB4-431A-B095-5547CE99340C"
            );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                blockTypeGuid: "0455ecbd-d54d-4485-bf4d-f469048ae10f", // FinancialPledgeEntry (Obsidian)
                fieldTypeGuid: Rock.SystemGuid.FieldType.DEFINED_VALUE,
                name: "New Record Source", // Name/key variation, to align with existing, similar block setting.
                key: "NewRecordSource",
                abbreviatedName: "New Record Source",
                description: "The record source to use for new individuals (default = 'Pledge'). If a 'RecordSource' page parameter is found, it will be used instead.",
                order: 4,
                defaultValue: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_PLEDGE,
                guid: "40FDD991-32C7-4207-9F3D-7DF79E0BF9F7"
            );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                blockTypeGuid: Rock.SystemGuid.BlockType.MOBILE_GROUPS_GROUP_REGISTRATION, // GroupRegistration (Mobile)
                fieldTypeGuid: Rock.SystemGuid.FieldType.DEFINED_VALUE,
                name: "Record Source",
                key: "DefaultRecordSource", // Key variation, to align with existing, similar block setting.
                abbreviatedName: "Record Source",
                description: "The record source to use for new individuals (default = 'Group Registration'). Can be overridden by a record source specified on the group or its parent group type.",
                order: 9,
                defaultValue: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_GROUP_REGISTRATION,
                guid: "68FDDECF-65E5-4544-A983-4FF6457F1735"
            );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                blockTypeGuid: "5e000376-ff90-4962-a053-ec1473da5c45", // GroupRegistration (Obsidian)
                fieldTypeGuid: Rock.SystemGuid.FieldType.DEFINED_VALUE,
                name: "Record Source",
                key: "RecordSource",
                abbreviatedName: "Record Source",
                description: "The record source to use for new individuals (default = 'Group Registration'). Can be overridden by a record source specified on the group or its parent group type. If a 'RecordSource' page parameter is found, it will be used instead.",
                order: 9,
                defaultValue: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_GROUP_REGISTRATION,
                guid: "D812FD48-8184-432F-B621-5B6394FA75CD"
            );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                blockTypeGuid: "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47", // OnboardPerson (Mobile)
                fieldTypeGuid: Rock.SystemGuid.FieldType.DEFINED_VALUE,
                name: "Default Record Source", // Name/key variation, to align with existing, similar block settings.
                key: "DefaultRecordSource",
                abbreviatedName: "Default Record Source",
                description: "The record source to use for new individuals (default = 'Mobile App').",
                order: 3,
                defaultValue: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_MOBILE_APP,
                guid: "B432BA76-7BBA-4C89-B816-9CB07B7D435E"
            );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                blockTypeGuid: "5AA30F53-1B7D-4CA9-89B6-C10592968870", // PrayerRequestEntry (Obsidian)
                fieldTypeGuid: Rock.SystemGuid.FieldType.DEFINED_VALUE,
                name: "Record Source",
                key: "RecordSource",
                abbreviatedName: "Record Source",
                description: "The record source to use for new individuals (default = 'Prayer'). If a 'RecordSource' page parameter is found, it will be used instead.",
                order: 20,
                defaultValue: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_PRAYER,
                guid: "A914A16B-CBF0-4906-81BF-E2602777ADEA"
            );

            RockMigrationHelper.AddBlockTypeAttributeToCategoryIfNotAlreadyAdded(
                blockTypeAttributeGuid: "A914A16B-CBF0-4906-81BF-E2602777ADEA",
                categoryName: "Features"
            );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                blockTypeGuid: "2A71FDA2-5204-418F-858E-693A1F4E9A49", // Register (Mobile)
                fieldTypeGuid: Rock.SystemGuid.FieldType.DEFINED_VALUE,
                name: "Record Source",
                key: "RecordSource",
                abbreviatedName: "Record Source",
                description: "The record source to use for new individuals (default = 'Mobile App').",
                order: 13,
                defaultValue: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_MOBILE_APP,
                guid: "C7C1A2E1-584B-496E-BF41-8AFD48DC2CD4"
            );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                blockTypeGuid: "161587D9-7B74-4D61-BF8E-3CDB38F16A12", // SignUpRegister (Obsidian)
                fieldTypeGuid: Rock.SystemGuid.FieldType.DEFINED_VALUE,
                name: "Record Source",
                key: "RecordSource",
                abbreviatedName: "Record Source",
                description: "The record source to use for new individuals (default = 'Sign-up'). Can be overridden by a record source specified on the sign-up project or its parent group type. If a 'RecordSource' page parameter is found, it will be used instead.",
                order: 8,
                defaultValue: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_SIGN_UP,
                guid: "73CBB8FA-A2D0-4953-B42D-7DB68E516A6C"
            );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                blockTypeGuid: "74EE3481-3E5A-4971-A02E-D463ABB45591", // TransactionEntry (Legacy)
                fieldTypeGuid: Rock.SystemGuid.FieldType.DEFINED_VALUE,
                name: "Record Source",
                key: "RecordSource",
                abbreviatedName: "Record Source",
                description: "The record source to use for new individuals (default = 'Giving'). If a 'RecordSource' page parameter is found, it will be used instead.",
                order: 15,
                defaultValue: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_GIVING,
                guid: "210E6CD9-0128-4210-A83A-FCE86E950BBD"
            );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                blockTypeGuid: "6316D801-40C0-4EED-A2AD-55C13870664D", // TransactionEntryV2 (Legacy)
                fieldTypeGuid: Rock.SystemGuid.FieldType.DEFINED_VALUE,
                name: "Record Source",
                key: "PersonRecordSource", // Key variation, to align with existing, similar block setting.
                abbreviatedName: "Record Source",
                description: "The record source to use for new individuals (default = 'Giving'). If a 'RecordSource' page parameter is found, it will be used instead.",
                order: 6,
                defaultValue: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_GIVING,
                guid: "332F8C78-00B3-4D05-B545-53BD07A12591"
            );

            RockMigrationHelper.AddBlockTypeAttributeToCategoryIfNotAlreadyAdded(
                blockTypeAttributeGuid: "332F8C78-00B3-4D05-B545-53BD07A12591",
                categoryName: "Person Options"
            );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                blockTypeGuid: "4CCC45A5-4AB9-4A36-BF8D-A6E316790004", // UtilityPaymentEntry (Legacy)
                fieldTypeGuid: Rock.SystemGuid.FieldType.DEFINED_VALUE,
                name: "Record Source",
                key: "RecordSource",
                abbreviatedName: "Record Source",
                description: "The record source to use for new individuals (default = 'Giving'). If a 'RecordSource' page parameter is found, it will be used instead.",
                order: 24,
                defaultValue: Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_GIVING,
                guid: "57A60C1E-8FBC-48DF-ACC1-7270593609B2"
            );
        }

        /// <summary>
        /// JPH: Add record source block attributes - down.
        /// </summary>
        private void JPH_AddRecordSourceBlockAttributes_20250407_Down()
        {
            RockMigrationHelper.DeleteAttribute( "11975898-16E5-41A2-A077-015E7CD92B8F" ); // AccountEntry (Obsidian)
            RockMigrationHelper.DeleteAttribute( "1A9D8D16-4AB4-431A-B095-5547CE99340C" ); // FamilyPreRegistration (Obsidian)
            RockMigrationHelper.DeleteAttribute( "40FDD991-32C7-4207-9F3D-7DF79E0BF9F7" ); // FinancialPledgeEntry (Obsidian)
            RockMigrationHelper.DeleteAttribute( "68FDDECF-65E5-4544-A983-4FF6457F1735" ); // GroupRegistration (Mobile)
            RockMigrationHelper.DeleteAttribute( "D812FD48-8184-432F-B621-5B6394FA75CD" ); // GroupRegistration (Obsidian)
            RockMigrationHelper.DeleteAttribute( "B432BA76-7BBA-4C89-B816-9CB07B7D435E" ); // OnboardPerson (Mobile)
            RockMigrationHelper.DeleteAttribute( "A914A16B-CBF0-4906-81BF-E2602777ADEA" ); // PrayerRequestEntry (Obsidian)
            RockMigrationHelper.DeleteAttribute( "C7C1A2E1-584B-496E-BF41-8AFD48DC2CD4" ); // Register (Mobile)
            RockMigrationHelper.DeleteAttribute( "73CBB8FA-A2D0-4953-B42D-7DB68E516A6C" ); // SignUpRegister (Obsidian)
            RockMigrationHelper.DeleteAttribute( "210E6CD9-0128-4210-A83A-FCE86E950BBD" ); // TransactionEntry (Legacy)
            RockMigrationHelper.DeleteAttribute( "332F8C78-00B3-4D05-B545-53BD07A12591" ); // TransactionEntryV2 (Legacy)
            RockMigrationHelper.DeleteAttribute( "57A60C1E-8FBC-48DF-ACC1-7270593609B2" ); // UtilityPaymentEntry (Legacy)
        }
    }
}
