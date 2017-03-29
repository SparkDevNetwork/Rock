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
    [MigrationNumber( 6, "1.4.5" )]
    public class ResourceApproval : Migration
    {
        public override void Up()
        {
            Sql( @"
                ALTER TABLE [_com_centralaz_RoomManagement_Resource] Add ApprovalGroupId [int] NULL;

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource] WITH CHECK ADD CONSTRAINT [FK__com_centralaz_RoomManagement_Resource_ApprovalGroup] FOREIGN KEY([ApprovalGroupId])
                REFERENCES [dbo].[Group] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_Resource_ApprovalGroup]
            " );
}
        public override void Down()
        {
            Sql( @"
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Resource_ApprovalGroup]
                ALTER TABLE [_com_centralaz_RoomManagement_Resource] Drop Column ApprovalGroupId
            " );
        }
    }
}
