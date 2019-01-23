// <copyright>
// Copyright by LCBC Church
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Model;
using Rock.Plugin;
namespace com.lcbcchurch.Care.Migrations
{
    [MigrationNumber( 4, "1.0.14" )]
    public class AddMigratedContactsNoteType : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.UpdateNoteType( "Migrated Contacts", "Rock.Model.Person", false, "59DD19B4-C7C1-4575-AF70-05EDE1290B82", false ); // Create `Migrated Contacts` NoteType
            RockMigrationHelper.AddSecurityAuthForNoteType( "59DD19B4-C7C1-4575-AF70-05EDE1290B82", 2, Rock.Security.Authorization.VIEW, false, null, (int)Rock.Model.SpecialRole.AllUsers, "B3B0BD2E-BA33-461D-8B7B-EE880B566DA4" ); // [All Users] - Deny
            RockMigrationHelper.AddSecurityAuthForNoteType( "59DD19B4-C7C1-4575-AF70-05EDE1290B82", 1, Rock.Security.Authorization.VIEW, true, "2C112948-FF4C-46E7-981A-0257681EADF4", (int)Rock.Model.SpecialRole.None, "4A4C741C-3793-4822-A0D1-5D13D892656A" );  // [RSR - Staff Workers] - Allow
            RockMigrationHelper.AddSecurityAuthForNoteType( "59DD19B4-C7C1-4575-AF70-05EDE1290B82", 0, Rock.Security.Authorization.VIEW, true, "628C51A8-4613-43ED-A18D-4A6FB999273E", (int)Rock.Model.SpecialRole.None, "29FD9EF0-85ED-4471-84FE-7682B65F9851" );  // [RSR - Rock Administration] - Allow

            // 'Migrated Contacts' needs to be User Selectable and Allow Note Replies
            Sql( @"
UPDATE [NoteType]
SET [UserSelectable] = 1, [AllowsReplies] = 1
WHERE [Name] = 'Migrated Contacts'
" );

        }
        public override void Down()
        {
            RockMigrationHelper.DeleteByGuid( "59DD19B4-C7C1-4575-AF70-05EDE1290B82", "NoteType" );
            RockMigrationHelper.DeleteSecurityAuth( "B3B0BD2E-BA33-461D-8B7B-EE880B566DA4" );
            RockMigrationHelper.DeleteSecurityAuth( "4A4C741C-3793-4822-A0D1-5D13D892656A" );
            RockMigrationHelper.DeleteSecurityAuth( "29FD9EF0-85ED-4471-84FE-7682B65F9851" );
        }
    }
}
