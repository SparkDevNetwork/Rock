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
    public partial class AddAttendanceOccurrenceAttendanceTypeValueId : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddAttendanceTypeColumn();
            AddDefinedTypeAndValues();
            AddBlockAttributeData();
        }

        private void AddAttendanceTypeColumn()
        {
            AddColumn( "dbo.AttendanceOccurrence", "AttendanceTypeValueId", c => c.Int() );
            AddForeignKey( "dbo.AttendanceOccurrence", "AttendanceTypeValueId", "dbo.DefinedValue", "Id", cascadeDelete: false );
        }

        private void AddDefinedTypeAndValues()
        {
            RockMigrationHelper.AddDefinedType( "Check-in",
                "Attendance Types",
                "List of possible attendance types an attendance occurrence can have.",
                SystemGuid.DefinedType.CHECK_IN_ATTENDANCE_TYPES,
                string.Empty );

            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.CHECK_IN_ATTENDANCE_TYPES,
                "Physical",
                string.Empty,
                SystemGuid.DefinedValue.CHECK_IN_ATTENDANCE_TYPE_PHYSICAL,
                true );

            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.CHECK_IN_ATTENDANCE_TYPES,
                "Virtual",
                string.Empty,
                SystemGuid.DefinedValue.CHECK_IN_ATTENDANCE_TYPE_VIRTUAL,
                true );
        }

        private void AddBlockAttributeData()
        {

            // Attribute for BlockType: Group Attendance Detail:Attendance Type Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( SystemGuid.BlockType.GROUP_ATTENDANCE_DETAIL,
                SystemGuid.FieldType.TEXT,
                "Attendance Type Label",
                "AttendanceTypeLabel",
                "Attendance Type Label",
                "The label that will be shown for the attendance types section.",
                15,
                "Attendance Location",
                SystemGuid.Attribute.ATTENDANCE_TYPE_LABEL );

            // Attribute for BlockType: Group Attendance Detail:Configured Attendance Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( SystemGuid.BlockType.GROUP_ATTENDANCE_DETAIL,
                SystemGuid.FieldType.DEFINED_VALUE,
                "Configured Attendance Types",
                "AttendanceTypes",
                "Configured Attendance Types",
                "The Attendance types that an occurrence can have. If no or one Attendance types selected none will be shown.",
                14,
                string.Empty,
                SystemGuid.Attribute.CONFIGURED_ATTENDANCE_TYPES );

            // Attribute for BlockType: Group Attendance List:Display Attendance Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( SystemGuid.BlockType.GROUP_ATTENDANCE_LIST,
                SystemGuid.FieldType.BOOLEAN,
                "Display Attendance Type",
                "DisplayAttendanceType",
                "Display Attendance Type",
                "Should the Attendance Type column be displayed?",
                4,
                "True",
                SystemGuid.Attribute.DISPLAY_ATTENDANCE_TYPE );

            // Attribute for BlockType: Attendance Self Entry:Configured Attendance Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( SystemGuid.BlockType.ATTENDANCE_SELF_ENTRY,
                SystemGuid.FieldType.DEFINED_VALUE,
                "Configured Attendance Type",
                "AttendanceType",
                "Configured Attendance Type",
                "The Attendance type that an occurrence will have.",
                28,
                SystemGuid.DefinedValue.CHECK_IN_ATTENDANCE_TYPE_VIRTUAL,
                SystemGuid.Attribute.CONFIGURED_ATTENDANCE_TYPE );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RemoveAttendanceTypeColumn();
            RemoveDefinedTypeAndValues();
            RemoveBlockAttributeData();
        }

        private void RemoveAttendanceTypeColumn()
        {
            DropForeignKey( "dbo.AttendanceOccurrence", "AttendanceTypeValueId", "dbo.DefinedValue" );
            DropColumn( "dbo.AttendanceOccurrence", "AttendanceTypeValueId" );
        }

        private void RemoveDefinedTypeAndValues()
        {
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.CHECK_IN_ATTENDANCE_TYPE_PHYSICAL );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.CHECK_IN_ATTENDANCE_TYPE_VIRTUAL );
            RockMigrationHelper.DeleteDefinedType( Rock.SystemGuid.DefinedType.CHECK_IN_ATTENDANCE_TYPES );
        }

        private void RemoveBlockAttributeData()
        {

            // Display Attendance Type Attribute for BlockType: Group Attendance List
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.DISPLAY_ATTENDANCE_TYPE );

            // Attendance Type Label Attribute for BlockType: Group Attendance Detail
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.ATTENDANCE_TYPE_LABEL );

            // Configured Attendance Types Attribute for BlockType: Group Attendance Detail
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.CONFIGURED_ATTENDANCE_TYPES );

            // Configured Attendance Type Attribute for BlockType: Attendance Self Entry
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.CONFIGURED_ATTENDANCE_TYPE );
        }
    }
}
