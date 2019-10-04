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
    public partial class CampusStatus : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumnsToCampusUp();
            CreateCampusDefinedTypesUp();
            UpdateAddedCampusColumns();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumnsToCampusDown();
            CreateCampusDefinedTypesDown();
        }

        /// <summary>
        /// ED: Add the CampusStatusValueId and CampusTypeValueId columns (Up)
        /// </summary>
        private void AddColumnsToCampusUp()
        {
            AddColumn( "dbo.Campus", "CampusStatusValueId", c => c.Int() );
            AddColumn( "dbo.Campus", "CampusTypeValueId", c => c.Int() );
            CreateIndex( "dbo.Campus", "CampusStatusValueId" );
            CreateIndex( "dbo.Campus", "CampusTypeValueId" );
            AddForeignKey( "dbo.Campus", "CampusStatusValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.Campus", "CampusTypeValueId", "dbo.DefinedValue", "Id" );
        }

        /// <summary>
        /// ED: Add the CampusStatusValueId and CampusTypeValueId columns (Down)
        /// </summary>
        private void AddColumnsToCampusDown()
        {
            DropForeignKey( "dbo.Campus", "CampusTypeValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Campus", "CampusStatusValueId", "dbo.DefinedValue" );
            DropIndex( "dbo.Campus", new[] { "CampusTypeValueId" } );
            DropIndex( "dbo.Campus", new[] { "CampusStatusValueId" } );
            DropColumn( "dbo.Campus", "CampusTypeValueId" );
            DropColumn( "dbo.Campus", "CampusStatusValueId" );
        }

        /// <summary>
        /// ED: Creates the campus defined types and defined values (Up).
        /// </summary>
        private void CreateCampusDefinedTypesUp()
        {
            RockMigrationHelper.UpdateCategory( Rock.SystemGuid.EntityType.DEFINED_TYPE, "Campus", "fa fa-building", "", Rock.SystemGuid.Category.DEFINEDTYPE_CAMPUS, 0 );
            RockMigrationHelper.AddDefinedType( "Campus", "Campus Status", "List of possible statuses a campus can have.", Rock.SystemGuid.DefinedType.CAMPUS_STATUS, @"" );
            RockMigrationHelper.AddDefinedType( "Campus", "Campus Type", "List of different types of campuses.", Rock.SystemGuid.DefinedType.CAMPUS_TYPE, @"" );
            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.CAMPUS_STATUS, "Closed", "", Rock.SystemGuid.DefinedValue.CAMPUS_STATUS_CLOSED, true );
            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.CAMPUS_STATUS, "Open", "", Rock.SystemGuid.DefinedValue.CAMPUS_STATUS_OPEN, true );
            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.CAMPUS_STATUS, "Pending", "", Rock.SystemGuid.DefinedValue.CAMPUS_STATUS_PENDING, true );
            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.CAMPUS_TYPE, "Online", "", Rock.SystemGuid.DefinedValue.CAMPUS_TYPE_ONLINE, true );
            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.CAMPUS_TYPE, "Physical", "", Rock.SystemGuid.DefinedValue.CAMPUS_TYPE_PHYSICAL, true );
        }

        private void CreateCampusDefinedTypesDown()
        {
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.CAMPUS_STATUS_PENDING ); // Pending
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.CAMPUS_TYPE_ONLINE ); // Online
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.CAMPUS_STATUS_OPEN ); // Open
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.CAMPUS_TYPE_PHYSICAL ); // Physical
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.CAMPUS_STATUS_CLOSED ); // Closed
            RockMigrationHelper.DeleteDefinedType( Rock.SystemGuid.DefinedType.CAMPUS_STATUS ); // Campus Status
            RockMigrationHelper.DeleteDefinedType( Rock.SystemGuid.DefinedType.CAMPUS_TYPE ); // Campus Type
            RockMigrationHelper.DeleteCategory( Rock.SystemGuid.Category.DEFINEDTYPE_CAMPUS );
        }

        private void UpdateAddedCampusColumns()
        {
            Sql( $@"
                DECLARE @openDefinedValueId INT = (SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = '{Rock.SystemGuid.DefinedValue.CAMPUS_STATUS_OPEN}')
                DECLARE @pendingDefinedValueId INT = (SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = '{Rock.SystemGuid.DefinedValue.CAMPUS_STATUS_PENDING}')
                DECLARE @onlineDefinedValueId INT = (SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = '{Rock.SystemGuid.DefinedValue.CAMPUS_TYPE_ONLINE}')
                DECLARE @physicalDefinedValueId INT = (SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = '{Rock.SystemGuid.DefinedValue.CAMPUS_TYPE_PHYSICAL}')

                UPDATE [dbo].[Campus]
                SET [CampusStatusValueId] = @openDefinedValueId
                WHERE [IsActive] = 1

                UPDATE [dbo].[Campus]
                SET [CampusStatusValueId] = @pendingDefinedValueId
                WHERE [IsActive] = 0

                UPDATE [dbo].[Campus]
                SET [CampusTypeValueId] = @onlineDefinedValueId
                WHERE [Name] LIKE '%online%' OR [Name] LIKE '%on-line%'

                UPDATE [dbo].[Campus]
                SET [CampusTypeValueId] = @physicalDefinedValueId
                WHERE CampusTypeValueId IS NULL" );
        }
    }
}
