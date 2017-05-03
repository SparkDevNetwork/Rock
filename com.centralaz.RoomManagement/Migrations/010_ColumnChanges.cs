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
using Rock.Plugin;

namespace com.centralaz.RoomManagement.Migrations
{
    [MigrationNumber( 10, "1.6.0" )]
    public class ColumnChanges : Migration
    {
        public override void Up()
        {
            #region Reservation changes
            AddColumn( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "ContactPersonAliasId", c => c.Int() );
            AddColumn( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "ContactPhone", c => c.String( maxLength: 50 ) );
            AddColumn( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "ContactEmail", c => c.String( maxLength: 400 ) );

            Sql( @"
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] WITH CHECK ADD CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_ContactPersonAliasId] FOREIGN KEY([ContactPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
" );

            #endregion
            
            #region Increase size of the Note column in the Resource table
            Sql( @"
    ALTER TABLE[_com_centralaz_RoomManagement_Resource]
    ALTER COLUMN[Note] NVARCHAR( 2000 )
" );

            #endregion
        }

        public override void Down()
        {
        }
    }
}
