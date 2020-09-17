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
    [MigrationNumber( 10, "1.9.4" )]
    public class ColumnChanges : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            #region Reservation changes
            // AddColumn is currently only available in Rock v7
            //AddColumn( "[dbo].[_com_bemaservices_RoomManagement_Reservation]", "ContactPersonAliasId", c => c.Int() );
            //AddColumn( "[dbo].[_com_bemaservices_RoomManagement_Reservation]", "ContactPhone", c => c.String( maxLength: 50 ) );
            //AddColumn( "[dbo].[_com_bemaservices_RoomManagement_Reservation]", "ContactEmail", c => c.String( maxLength: 400 ) );
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] ADD [ContactPersonAliasId] INT
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] ADD [ContactPhone] NVARCHAR (50)
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] ADD [ContactEmail] NVARCHAR (400)
" );

            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_ContactPersonAliasId] FOREIGN KEY([ContactPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
" );

            #endregion
            
            #region Increase size of the Note column in the Resource table
            Sql( @"
    ALTER TABLE[_com_bemaservices_RoomManagement_Resource]
    ALTER COLUMN[Note] NVARCHAR( 2000 )
" );

            #endregion
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
        }
    }
}
