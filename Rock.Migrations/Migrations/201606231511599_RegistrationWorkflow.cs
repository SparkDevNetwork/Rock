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
    public partial class RegistrationWorkflow : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Attendance", "SearchValue", c => c.String());
            AddColumn("dbo.Attendance", "SearchResultGroupId", c => c.Int());
            AddColumn("dbo.RegistrationInstance", "RegistrationWorkflowTypeId", c => c.Int());
            AddColumn("dbo.RegistrationTemplate", "RegistrationWorkflowTypeId", c => c.Int());
            CreateIndex("dbo.Attendance", "SearchResultGroupId");
            CreateIndex("dbo.RegistrationInstance", "RegistrationWorkflowTypeId");
            CreateIndex("dbo.RegistrationTemplate", "RegistrationWorkflowTypeId");
            AddForeignKey("dbo.RegistrationTemplate", "RegistrationWorkflowTypeId", "dbo.WorkflowType", "Id");
            AddForeignKey("dbo.RegistrationInstance", "RegistrationWorkflowTypeId", "dbo.WorkflowType", "Id");
            AddForeignKey("dbo.Attendance", "SearchResultGroupId", "dbo.Group", "Id");

            // MP: Address/Country Hide Address Line 2
            RockMigrationHelper.AddDefinedTypeAttribute( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", Rock.SystemGuid.FieldType.BOOLEAN, "Show Address Line 2", "ShowAddressLine2", "Show Address Line 2 when editing an address", 4, "True", "360F05E4-E55B-4313-BB9F-9DCE96833571" );
            RockMigrationHelper.AddDefinedValueAttributeValueByValue( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "US", "ShowAddressLine2", @"False" ); // United States ShowAddressLine2 (False), all other countries will default to true

            // DT: V5 hotfix: 002_CheckinGradeRequired
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "GroupTypePurposeValueId", "", "Grade Required", "", 0, "False", "A4899874-9EDF-4549-B054-4F593F4C4362", "core_checkin_GradeRequired" );
            RockMigrationHelper.UpdateAttributeQualifier( "A4899874-9EDF-4549-B054-4F593F4C4362", "falsetext", "No", "B61ED891-C631-4172-A05D-D86265CA2A1D" );
            RockMigrationHelper.UpdateAttributeQualifier( "A4899874-9EDF-4549-B054-4F593F4C4362", "truetext", "Yes", "D4C52849-6ED4-414A-95D7-2F7F805CF9A3" );

            // DT: Add back some defined value foreign key indexes (so defined value delete can use them)
            Sql( MigrationSQL._201606231511599_RegistrationWorkflow_Indexes );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Attendance", "SearchResultGroupId", "dbo.Group");
            DropForeignKey("dbo.RegistrationInstance", "RegistrationWorkflowTypeId", "dbo.WorkflowType");
            DropForeignKey("dbo.RegistrationTemplate", "RegistrationWorkflowTypeId", "dbo.WorkflowType");
            DropIndex("dbo.RegistrationTemplate", new[] { "RegistrationWorkflowTypeId" });
            DropIndex("dbo.RegistrationInstance", new[] { "RegistrationWorkflowTypeId" });
            DropIndex("dbo.Attendance", new[] { "SearchResultGroupId" });
            DropColumn("dbo.RegistrationTemplate", "RegistrationWorkflowTypeId");
            DropColumn("dbo.RegistrationInstance", "RegistrationWorkflowTypeId");
            DropColumn("dbo.Attendance", "SearchResultGroupId");
            DropColumn("dbo.Attendance", "SearchValue");
        }
    }
}
