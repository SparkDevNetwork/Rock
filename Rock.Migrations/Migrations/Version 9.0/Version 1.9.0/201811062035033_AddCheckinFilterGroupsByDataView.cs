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
    public partial class AddCheckinFilterGroupsByDataView : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddGroupType( "Check in by Data View", "", "Group", "Member", false, false, false, "", 0, "", 0, "6BCED84C-69AD-4F5A-9197-5C0F9C02DD34", "DC7ED2FD-5F88-4760-B3C8-494EDB1CFC06", true );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "DC7ED2FD-5F88-4760-B3C8-494EDB1CFC06", SystemGuid.FieldType.DATAVIEWS, "Data View", "The data view a person must be included in to be able to check into this group.", 0, "", "E8F8498F-5C51-4216-AC81-875349D6C2D0" );
            RockMigrationHelper.AddAttributeQualifier( "E8F8498F-5C51-4216-AC81-875349D6C2D0", "entityTypeName", "Rock.Model.Person", "D59BAD04-4C23-497C-810A-328B760B7C6B" );

            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CheckIn.FilterGroupsByDataView", "E6490F9B-21C6-4D0F-AD15-9729AC22C094", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "E6490F9B-21C6-4D0F-AD15-9729AC22C094", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0B1FF2A0-D0CE-42A5-95BC-C65F02D14399" );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "E6490F9B-21C6-4D0F-AD15-9729AC22C094", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "869473D0-AC93-4F8F-B164-2A332571E779" );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "E6490F9B-21C6-4D0F-AD15-9729AC22C094", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Remove", "Remove", "Select 'Yes' if groups should be removed.  Select 'No' if they should just be marked as excluded.", 0, @"True", "5EE8CC8E-455D-4115-88F4-81D224A30A28" );

            RockMigrationHelper.UpdateWorkflowActionType( "EB744DF1-E454-482C-B111-80A54EF8A674", "Filter Groups by DataView", 0, "E6490F9B-21C6-4D0F-AD15-9729AC22C094", true, false, "", "66EF6CB1-1A96-2F81-4534-3BCA5C33D4CD", 1, "False", "31F407AA-50BB-4915-BB22-7AF463989D98" );

            Sql( @"
    -- Fix the ordering of the Unattended Check-in Id: 10 workflow activity so that the 'Filter By Data View' activity is immediately after 'Filter By Gender'
    DECLARE @ActivityTypeId int = ( SELECT TOP 1 [Id] FROM [WorkflowActivityType] WHERE [Guid] = 'EB744DF1-E454-482C-B111-80A54EF8A674' )
    DECLARE @EntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Workflow.Action.CheckIn.FilterGroupsByGender' )
    DECLARE @Order int = ISNULL( ( SELECT TOP 1 [Order] FROM [WorkflowActionType] WHERE [ActivityTypeId] = @ActivityTypeId AND [EntityTypeId] = @EntityTypeId ), 0)
    IF @Order IS NOT NULL AND @Order > 0
    BEGIN
        UPDATE [WorkflowActionType] SET [Order] = [Order] + 1 WHERE [ActivityTypeId] = @ActivityTypeId AND [Order] > @Order
        UPDATE [WorkflowActionType] SET [Order] = @Order + 1 WHERE [Guid] = '31F407AA-50BB-4915-BB22-7AF463989D98'
    END
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // delete workflow action entity attributes
            RockMigrationHelper.DeleteAttribute( "0B1FF2A0-D0CE-42A5-95BC-C65F02D14399" );
            RockMigrationHelper.DeleteAttribute( "869473D0-AC93-4F8F-B164-2A332571E779" );
            RockMigrationHelper.DeleteAttribute( "5EE8CC8E-455D-4115-88F4-81D224A30A28" );

            RockMigrationHelper.DeleteWorkflowActionType( "31F407AA-50BB-4915-BB22-7AF463989D98" ); // workflow action type
            RockMigrationHelper.DeleteEntityType( "E6490F9B-21C6-4D0F-AD15-9729AC22C094" ); // workflow action (entity type)
            RockMigrationHelper.DeleteAttribute( "E8F8498F-5C51-4216-AC81-875349D6C2D0" ); // group type attribute
            RockMigrationHelper.DeleteGroupType( "DC7ED2FD-5F88-4760-B3C8-494EDB1CFC06" ); // group type
        }
    }
}
