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
    public partial class CacheViewerBlock : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateBlockType( "Cache Viewer", "Block used to view a cached entity for debugging purposes.", "~/Blocks/Cms/CacheViewer.ascx", "CMS", "7D54B356-D0AF-4D10-96BE-26DE4F38CEBF" );
            // Add Block to Page: Cache Manager Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "4B8691C7-537F-4B6E-9ED1-E3BA3FA0051E".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "7D54B356-D0AF-4D10-96BE-26DE4F38CEBF".AsGuid(), "Cache Viewer", "Main", @"", @"", 1, "D67B5DBF-DDC0-4017-9B99-21813B391CFD" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'C7107FFF-DE8D-41DD-B5EA-9C9C80B98C3C'" );  // Page: Cache Manager,  Zone: Main,  Block: Cache Manager
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'D67B5DBF-DDC0-4017-9B99-21813B391CFD'" );  // Page: Cache Manager,  Zone: Main,  Block: Cache Viewer            
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Cache Viewer, from Page: Cache Manager, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "D67B5DBF-DDC0-4017-9B99-21813B391CFD" );
            RockMigrationHelper.DeleteBlockType( "7D54B356-D0AF-4D10-96BE-26DE4F38CEBF" ); // Cache Viewer
        }
    }
}
