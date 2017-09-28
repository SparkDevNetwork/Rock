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
    [MigrationNumber( 13, "1.6.0" )]
    public class QuestionsAndAnswers : Migration
    {
        public override void Up()
        {
            Sql( @"
                CREATE TABLE [dbo].[_com_centralaz_RoomManagement_Question](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
	                [LocationId] [int] NULL,
	                [ResourceId] [int] NULL,
                    [AttributeId] [int] NOT NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_centralaz_RoomManagement_Question] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Question]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_Question_Location] FOREIGN KEY([LocationId])
                REFERENCES [dbo].[Location] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Question] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_Question_Location]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Question]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_Question_Resource] FOREIGN KEY([ResourceId])
                REFERENCES [dbo].[_com_centralaz_RoomManagement_Resource] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Question] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_Question_Resource]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Question]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_Question_AttributeId] FOREIGN KEY([AttributeId])
                REFERENCES [dbo].[Attribute] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Question] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_Question_AttributeId]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Question]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_Question_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Question] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_Question_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Question]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_Question_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Question] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_Question_ModifiedByPersonAliasId]

" );
            RockMigrationHelper.UpdateEntityType( "com.centralaz.RoomManagement.Model.Question", "F42FB8A5-D646-4FA8-AABE-D47B53A9CE35", true, true );

            // Fix bug where setup photos are added as temporary binary files
            var rockContext = new RockContext();
            var photoIds = new ReservationService( rockContext ).Queryable().Select( r => r.SetupPhotoId ).Distinct().ToList();
            var photos = new BinaryFileService( rockContext ).Queryable().Where( bf => photoIds.Contains( bf.Id ) ).ToList();
            foreach ( var photo in photos )
            {
                photo.IsTemporary = false;
            }
            rockContext.SaveChanges();

        }
        public override void Down()
        {
            RockMigrationHelper.DeleteEntityType( "F42FB8A5-D646-4FA8-AABE-D47B53A9CE35" );

            Sql( @"
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Question] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Question_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Question] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Question_CreatedByPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Question] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Question_AttributeId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Question] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Question_Location]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Question] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Question_Resource]
                DROP TABLE [dbo].[_com_centralaz_RoomManagement_Question]
            " );
        }
    }
}
