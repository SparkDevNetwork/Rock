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
    [MigrationNumber( 13, "1.9.4" )]
    public class QuestionsAndAnswers : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
                CREATE TABLE [dbo].[_com_bemaservices_RoomManagement_Question](
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
                 CONSTRAINT [PK__com_bemaservices_RoomManagement_Question] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Question]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Question_Location] FOREIGN KEY([LocationId])
                REFERENCES [dbo].[Location] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Question] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Question_Location]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Question]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Question_Resource] FOREIGN KEY([ResourceId])
                REFERENCES [dbo].[_com_bemaservices_RoomManagement_Resource] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Question] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Question_Resource]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Question]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Question_AttributeId] FOREIGN KEY([AttributeId])
                REFERENCES [dbo].[Attribute] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Question] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Question_AttributeId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Question]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Question_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Question] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Question_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Question]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_Question_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Question] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_Question_ModifiedByPersonAliasId]

" );
            Sql( @"
                    Delete
                    From EntityType
                    Where Name = 'com.bemaservices.RoomManagement.Model.Question'
                " );
            UpdateEntityTypeByGuid( "com.bemaservices.RoomManagement.Model.Question", "F42FB8A5-D646-4FA8-AABE-D47B53A9CE35", true, true );

            // Fix bug where setup photos are added as temporary binary files
            Sql( @"UPDATE [BinaryFile]
                SET [IsTemporary] = 0
                WHERE Id IN (SELECT [SetupPhotoId] FROM [_com_bemaservices_RoomManagement_Reservation])" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteEntityType( "F42FB8A5-D646-4FA8-AABE-D47B53A9CE35" );

            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Question] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_Question_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Question] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_Question_CreatedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Question] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_Question_AttributeId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Question] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_Question_Location]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Question] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_Question_Resource]
                DROP TABLE [dbo].[_com_bemaservices_RoomManagement_Question]
            " );
        }

        public void UpdateEntityTypeByGuid( string name, string guid, bool isEntity, bool isSecured )
        {
            Sql( string.Format( @"
                IF EXISTS ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '{3}' )
                BEGIN
                    UPDATE [EntityType] SET
                        [IsEntity] = {1},
                        [IsSecured] = {2},
                        [Name] = '{0}'
                    WHERE [Guid] = '{3}'
                END
                ELSE
                BEGIN
                    IF EXISTS ( SELECT [Id] FROM [EntityType] WHERE [Name] = '{0}' )
                    BEGIN
                        UPDATE [EntityType] SET
                            [IsEntity] = {1},
                            [IsSecured] = {2},
                            [Guid] = '{3}'
                        WHERE [Name] = '{0}'
                    END
                    ELSE
                    BEGIN
                        INSERT INTO [EntityType] ([Name], [IsEntity], [IsSecured], [IsCommon], [Guid])
                        VALUES ('{0}', {1}, {2}, 0, '{3}')
                    END
                END
",
                name,
                isEntity ? "1" : "0",
                isSecured ? "1" : "0",
                guid ) );
        }
    }
}
