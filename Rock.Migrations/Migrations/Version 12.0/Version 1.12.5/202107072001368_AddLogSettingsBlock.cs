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
    public partial class AddLogSettingsBlock : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update BlockType Logs              
            RockMigrationHelper.UpdateBlockType( "Logs", "Block to edit system log settings.", "~/Blocks/Administration/LogSettings.ascx", "Administration", "6ABC44FD-C4D7-4E30-8537-3A065B493453" );
            
            // Add Block Log Settings to Page: Rock Logs, Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "82EC7718-6549-4531-A0AB-7957919AE71C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "6ABC44FD-C4D7-4E30-8537-3A065B493453".AsGuid(), "Log Settings", "Main", @"", @"", 0, "B31E49B6-2349-4378-98A4-19A8DDD42DF9" );
            
            // Update Order for Page: Rock Logs,  Zone: Main,  Block: Log Settings              
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'B31E49B6-2349-4378-98A4-19A8DDD42DF9'" );
            
            // Update Order for Page: Rock Logs,  Zone: Main,  Block: Rock Logs              
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'BDEF9AA0-55FC-4A66-8938-2AB2E075521B'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
