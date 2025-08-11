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
    /// Migration to add new properties to StepType entity and create Organizational Objective defined type.
    /// </summary>
    public partial class UpdateStepType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateStepTypeColumnsUp();
            UpdateDefinedTypeUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateDefinedTypeDown();
            UpdateStepTypeColumnsDown();
        }

        /// <summary>
        /// Add new columns to StepType table - up.
        /// </summary>
        private void UpdateStepTypeColumnsUp()
        {
            AddColumn( "dbo.StepType", "EngagementType", c => c.Int() );
            AddColumn( "dbo.StepType", "ImpactWeight", c => c.Int() );
            AddColumn( "dbo.StepType", "OrganizationalObjectiveValueId", c => c.Int() );

            AddForeignKey( "dbo.StepType", "OrganizationalObjectiveValueId", "dbo.DefinedValue", "Id" );
        }

        /// <summary>
        /// Add new columns to StepType table - down.
        /// </summary>
        private void UpdateStepTypeColumnsDown()
        {
            DropForeignKey( "dbo.StepType", "OrganizationalObjectiveValueId", "dbo.DefinedValue" );

            DropColumn( "dbo.StepType", "OrganizationalObjectiveValueId" );
            DropColumn( "dbo.StepType", "ImpactWeight" );
            DropColumn( "dbo.StepType", "EngagementType" );
        }

        /// <summary>
        /// Add organizational objective defined type and values - up.
        /// </summary>
        private void UpdateDefinedTypeUp()
        {
            RockMigrationHelper.AddDefinedType(
                category: "Global",
                name: "Organizational Objective",
                description: "This list defines the core objectives that activities within the organization are aligned to. While you can rename these objectives to better fit your context, we highly recommend preserving their original intent. Reporting and analytics are based on the intended purpose of each objective, and changing that intent may lead to misleading results.",
                guid: Rock.SystemGuid.DefinedType.ORGANIZATIONAL_OBJECTIVE_TYPE
            );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.ORGANIZATIONAL_OBJECTIVE_TYPE,
                value: "Outreach",
                description: "Connecting with those outside the church to share the love and message of Christ.",
                guid: Rock.SystemGuid.DefinedValue.ORGANIZATIONAL_OBJECTIVE_TYPE_OUTREACH,
                isSystem: true
            );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.ORGANIZATIONAL_OBJECTIVE_TYPE,
                value: "Discipleship",
                description: "Guiding individuals to grow in their faith and deepen their relationship with Jesus.",
                guid: Rock.SystemGuid.DefinedValue.ORGANIZATIONAL_OBJECTIVE_TYPE_DISCIPLESHIP,
                isSystem: true
            );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.ORGANIZATIONAL_OBJECTIVE_TYPE,
                value: "Activation",
                description: "Empowering individuals to serve others and actively participate in God's mission.",
                guid: Rock.SystemGuid.DefinedValue.ORGANIZATIONAL_OBJECTIVE_TYPE_ACTIVATION,
                isSystem: true
            );
        }

        /// <summary>
        /// Add organizational objective defined type and values - down.
        /// </summary>
        private void UpdateDefinedTypeDown()
        {
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.ORGANIZATIONAL_OBJECTIVE_TYPE_OUTREACH );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.ORGANIZATIONAL_OBJECTIVE_TYPE_DISCIPLESHIP );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.ORGANIZATIONAL_OBJECTIVE_TYPE_ACTIVATION );

            RockMigrationHelper.DeleteDefinedType( Rock.SystemGuid.DefinedType.ORGANIZATIONAL_OBJECTIVE_TYPE );
        }
    }
}
