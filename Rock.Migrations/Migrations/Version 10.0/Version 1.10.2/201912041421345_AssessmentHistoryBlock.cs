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
    public partial class AssessmentHistoryBlock : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateBlockType( "Assessment History", "Displays Assessment History on the Person Profile's History tab. Allows a person to see and delete (if needed) pending assessment requests.", "~/Blocks/Crm/AssessmentHistory.ascx", "CRM", "E7EB1E42-FEA7-4735-83FE-A618BD2616BF" );
            // Add Block to Page: History Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "E7EB1E42-FEA7-4735-83FE-A618BD2616BF".AsGuid(), "Assessment History", "SectionC1", @"", @"", 3, "121C12D7-A0A7-43EB-935D-3D4D80EDD650" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '27F84ADB-AA13-439E-A130-FBF73698B172'" );  // Page: History,  Zone: SectionC1,  Block: Communication History
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '2D99AB97-4B9C-4D72-B207-8F36AE90D495'" );  // Page: History,  Zone: SectionC1,  Block: Attendance History
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = 'F5BE2D28-C886-4357-9D2E-FE4D5BBBA700'" );  // Page: History,  Zone: SectionC1,  Block: History Log
            Sql( @"UPDATE [Block] SET [Order] = 3 WHERE [Guid] = '121C12D7-A0A7-43EB-935D-3D4D80EDD650'" );  // Page: History,  Zone: SectionC1,  Block: Assessment History
            Sql( @"UPDATE [Block] SET [Order] = 4 WHERE [Guid] = '69E78065-3BFC-452E-97C3-C104497CF7EB'" );  // Page: History,  Zone: SectionC1,  Block: Signature Document List            
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Assessment History, from Page: History, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "121C12D7-A0A7-43EB-935D-3D4D80EDD650" );
            RockMigrationHelper.DeleteBlockType( "E7EB1E42-FEA7-4735-83FE-A618BD2616BF" ); // Assessment History
        }
    }
}
