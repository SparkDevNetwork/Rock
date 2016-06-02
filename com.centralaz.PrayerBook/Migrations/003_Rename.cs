// <copyright>
// Copyright by Central Christian Church
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
using Rock.Plugin;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Constants;

namespace com.centralaz.Prayerbook.Migrations
{
    [MigrationNumber( 3, "1.1.0" )]
    public class Rename : Migration
    {
        public override void Up()
        {
            Sql( String.Format( @"
            UPDATE [BlockType]
            SET Name = 'Homepage Sidebar'
            WHERE Guid = '{0}'

            UPDATE [BlockType]
            SET Name = 'Edit Entry'
            WHERE Guid = '{1}'

            UPDATE [BlockType]
            SET Name = 'Book Management'
            WHERE Guid = '{2}'

            UPDATE [Attribute]
            SET Name = 'IsActive', [Key] = 'IsActive'
            WHERE Guid = '{3}'

            UPDATE [Attribute]
            SET Name = 'IsActive', [Key] = 'IsActive'
            WHERE Guid = '{4}'

            UPDATE [Attribute]
            SET Name = 'IsOpen', [Key] = 'IsOpen'
            WHERE Guid = '{5}'

            UPDATE [Attribute]
            SET Name = 'IsPublished', [Key] = 'IsPublished'
            WHERE Guid = '{6}';

            UPDATE [Attribute]
            SET Name = 'OpenDate', [Key] = 'OpenDate'
            WHERE Guid = '{7}';

            UPDATE [Attribute]
            SET Name = 'CloseDate', [Key] = 'CloseDate'
            WHERE Guid = '{8}'
            ",
              com.centralaz.Prayerbook.SystemGuid.BlockType.HOMEPAGE_SIDEBAR_BLOCKTYPE,//       0
              com.centralaz.Prayerbook.SystemGuid.BlockType.EDIT_ENTRY_BLOCKTYPE,//             1
              com.centralaz.Prayerbook.SystemGuid.BlockType.BOOK_MANAGEMENT_BLOCKTYPE,//        2
              com.centralaz.Prayerbook.SystemGuid.Attribute.MINISTRY_ISACTIVE_ATTRIBTUTE,//     3
              com.centralaz.Prayerbook.SystemGuid.Attribute.SUBMINISTRY_ISACTIVE_ATTRIBTUTE,//  4
              "F438BA68-FE87-41CA-BFD1-D6912866407F",//                                         5
              "2277DA7E-3B5B-44CF-9664-29B2578AA368",//                                         6
              "6D5C6677-FE19-4417-BE24-4DF81D6EBE45",//                                         7
              "D1E1FC2A-D8F1-4874-BE1B-F408A8134ED8"//                                          8
              ) );
        }

        public override void Down()
        {
        }
    }
}
