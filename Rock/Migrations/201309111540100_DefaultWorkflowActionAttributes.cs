//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class DefaultWorkflowActionAttributes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for LoadGroups: LoadAll
            AddEntityAttribute( "Rock.Model.WorkflowActionType", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "EntityTypeId", "", "Load All", ""
                , "By default groups are only loaded for the selected person, group type, and location.  Select this option to load groups for all the loaded people and group types."
                , 0, "False", "39762EF0-91D5-4B13-BD34-FC3AC3C24897" );

            Sql( @"            
                DECLARE @LoadGroupsAttributeId int
                SELECT @LoadGroupsAttributeId = [Id] FROM EntityType 
                WHERE [Name] = 'Rock.Workflow.Action.CheckIn.LoadGroups'
            
                UPDATE [Attribute] 
                SET [EntityTypeQualifierValue] = @LoadGroupsAttributeId
                WHERE [Guid] = '39762EF0-91D5-4B13-BD34-FC3AC3C24897' 
            " );

            // Attrib for LoadLocations: LoadAll
            AddEntityAttribute( "Rock.Model.WorkflowActionType", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "EntityTypeId", "", "Load All", ""
                , "By default locations are only loaded for the selected person and group type.  Select this option to load locations for all the loaded people and group types."
                , 0, "False", "70203A96-AE70-47AD-A086-FD84792DF2B6" );

            Sql( @"            
                DECLARE @LoadLocationsAttributeId int
                SELECT @LoadLocationsAttributeId = [Id] FROM EntityType 
                WHERE [Name] = 'Rock.Workflow.Action.CheckIn.LoadLocations'
            
                UPDATE [Attribute] 
                SET [EntityTypeQualifierValue] = @LoadLocationsAttributeId
                WHERE [Guid] = '70203A96-AE70-47AD-A086-FD84792DF2B6' 
            " );

            // Attrib for LoadSchedules: LoadAll
            AddEntityAttribute( "Rock.Model.WorkflowActionType", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "EntityTypeId", "", "Load All", ""
                , "By default schedules are only loaded for the selected person, group type, location, and group.  Select this option to load schedules for all the loaded people, group types, locations, and groups."
                , 0, "False", "B222CAF2-DF12-433C-B5D4-A8DB95B60207" );

            Sql( @"            
                DECLARE @LoadSchedulesAttributeId int
                SELECT @LoadSchedulesAttributeId = [Id] FROM EntityType 
                WHERE [Name] = 'Rock.Workflow.Action.CheckIn.LoadSchedules'
            
                UPDATE [Attribute] 
                SET [EntityTypeQualifierValue] = @LoadSchedulesAttributeId
                WHERE [Guid] = 'B222CAF2-DF12-433C-B5D4-A8DB95B60207' 
            " );

            // Attrib for SaveAttendance: Security Code
            AddEntityAttribute( "Rock.Model.WorkflowActionType", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "EntityTypeId", "", "Security Code Length", ""
                , "The number of characters to use for the security code."
                , 0, "3", "D57F42C9-E497-4FEE-8231-4FE2D13DC191" );

            Sql( @"            
                DECLARE @SaveAttendanceAttributeId int
                SELECT @SaveAttendanceAttributeId = [Id] FROM EntityType 
                WHERE [Name] = 'Rock.Workflow.Action.CheckIn.SaveAttendance'
            
                UPDATE [Attribute] 
                SET [EntityTypeQualifierValue] = @SaveAttendanceAttributeId
                WHERE [Guid] = 'D57F42C9-E497-4FEE-8231-4FE2D13DC191' 
            " );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "39762EF0-91D5-4B13-BD34-FC3AC3C24897" ); // LoadGroups: LoadAll
            DeleteAttribute( "70203A96-AE70-47AD-A086-FD84792DF2B6" ); // LoadLocations: LoadAll
            DeleteAttribute( "B222CAF2-DF12-433C-B5D4-A8DB95B60207" ); // LoadSchedules: LoadAll
            DeleteAttribute( "D57F42C9-E497-4FEE-8231-4FE2D13DC191" ); // SavedAttendance: SecurityCodeLength

        }
    }
}
