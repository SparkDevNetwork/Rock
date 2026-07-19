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
    public partial class RemoveLegacyCheckInManagerLocationsBlockType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Delete the legacy WebForms Check-in Manager "Locations (Obsolete)" block
            // It has been replaced by the new Obsidian Check-in Manager blocks (Roster, LiveMetrics, RoomSettings).
            Sql( @"
    DECLARE @LocationsBlockTypeId INT = ( SELECT TOP (1) [Id] FROM [BlockType] WHERE [Path] = '~/Blocks/CheckIn/Manager/Locations.ascx' OR [Guid] = '00FC1DEA-FE34-41E3-BC0A-2EE9138091EC' );

    IF @LocationsBlockTypeId IS NOT NULL
    BEGIN
        DELETE FROM [Block]
        WHERE [BlockTypeId] = @LocationsBlockTypeId;

        DELETE FROM [BlockType]
        WHERE [Id] = @LocationsBlockTypeId;
    END
    " );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
