// <copyright>
// Copyright by the Central Christian Church
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
using System.Linq;
using com.centralaz.RoomManagement.Model;
using Rock.Data;
using Rock.Model;
using Rock.Plugin;

namespace com.centralaz.RoomManagement.Migrations
{
    [MigrationNumber( 14, "1.6.0" )]
    public class QABlocks : Migration
    {
        public override void Up()
        {
            // Page: Resource Detail
            RockMigrationHelper.UpdateBlockType( "Question List", "A list of questions tied to a resource or location", "~/Plugins/com_centralaz/RoomManagement/QuestionList.ascx", "com_centralaz > Room Management", "349C4CDB-713E-4E23-9628-17B9938DDFC5" );
            RockMigrationHelper.AddBlock( "B75A0C7E-4A15-4892-A857-BADE8B5DD4CA", "", "349C4CDB-713E-4E23-9628-17B9938DDFC5", "Question List", "Main", "", "", 1, "39D13192-23CC-42BE-BAEF-705DEB77EF28" );

            // Page: Named Locations
            RockMigrationHelper.AddBlock( "96501070-BB46-4432-AA3C-A8C496691629", "", "349C4CDB-713E-4E23-9628-17B9938DDFC5", "Question List", "Main", "", "", 2, "177C531A-F93D-400C-8923-624D07D7B57D" );

        }
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( "177C531A-F93D-400C-8923-624D07D7B57D" );
            RockMigrationHelper.DeleteBlock( "39D13192-23CC-42BE-BAEF-705DEB77EF28" );
            RockMigrationHelper.DeleteBlockType( "349C4CDB-713E-4E23-9628-17B9938DDFC5" );
        }
    }
}
