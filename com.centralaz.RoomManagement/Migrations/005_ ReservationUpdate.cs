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
    [MigrationNumber( 5, "1.4.5" )]
    public class ReservationUpdate : Migration
    {
        public override void Up()
        {

            Sql( @"
                DECLARE @WorkflowId int = (Select Top 1 Id From WorkflowType Where Guid = '543D4FCD-310B-4048-BFCB-BAE582CBB890')

INSERT [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger] ([WorkflowTypeId], [TriggerType], [QualifierValue], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [Guid], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES (@WorkflowId, 0, N'|||', CAST(N'2017-03-20 14:02:11.953' AS DateTime), CAST(N'2017-03-20 14:02:11.953' AS DateTime), 10, 10, N'5339e1c4-ac09-4bd5-9416-628dba200ba5', NULL, NULL, NULL)
INSERT [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger] ([WorkflowTypeId], [TriggerType], [QualifierValue], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [Guid], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES (@WorkflowId, 2, N'|||', CAST(N'2017-03-20 14:02:11.953' AS DateTime), CAST(N'2017-03-20 14:02:11.953' AS DateTime), 10, 10, N'68f6de62-cdbb-4ec0-8440-8b1740c21e65', NULL, NULL, NULL)

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflow_WorkflowId]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflow_WorkflowId] FOREIGN KEY([WorkflowId])
                REFERENCES [dbo].[Workflow] ([Id])
                ON DELETE CASCADE" );
        }
        public override void Down()
        {

        }
    }
}
