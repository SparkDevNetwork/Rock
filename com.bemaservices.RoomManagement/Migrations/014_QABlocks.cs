// <copyright>
// Copyright by BEMA Software Services
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
using Rock.Plugin;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 14, "1.9.4" )]
    public class QABlocks : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Page: Resource Detail
            RockMigrationHelper.UpdateBlockTypeByGuid( "Question List", "A list of questions tied to a resource or location", "~/Plugins/com_bemaservices/RoomManagement/QuestionList.ascx", "com_bemaservices > Room Management", "349C4CDB-713E-4E23-9628-17B9938DDFC5" );
            RockMigrationHelper.AddBlock( true, "B75A0C7E-4A15-4892-A857-BADE8B5DD4CA", "", "349C4CDB-713E-4E23-9628-17B9938DDFC5", "Question List", "Main", "", "", 1, "39D13192-23CC-42BE-BAEF-705DEB77EF28" );

            // Page: Named Locations
            RockMigrationHelper.AddBlock( true, "2BECFB85-D566-464F-B6AC-0BE90189A418", "", "349C4CDB-713E-4E23-9628-17B9938DDFC5", "Question List", "Main", "", "", 2, "177C531A-F93D-400C-8923-624D07D7B57D" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( "177C531A-F93D-400C-8923-624D07D7B57D" );
            RockMigrationHelper.DeleteBlock( "39D13192-23CC-42BE-BAEF-705DEB77EF28" );
            RockMigrationHelper.DeleteBlockType( "349C4CDB-713E-4E23-9628-17B9938DDFC5" );
        }
    }
}
