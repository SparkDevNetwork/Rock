// <copyright>
// Copyright by the Spark Development Network
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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class AddSystemCommunicationIdColumn : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.Communication", "SystemCommunicationId", c => c.Int() );
            CreateIndex( "dbo.Communication", "SystemCommunicationId" );

            // Instead of the scaffolded AddForeignKey("dbo.Communication", "SystemCommunicationId", "dbo.SystemCommunication", "Id");
            // we want a ON DELETE NULL (cascade null).
            Sql( @"ALTER TABLE [dbo].[Communication]
ADD CONSTRAINT [FK_dbo.Communication_dbo.SystemCommunication_SystemCommunicationId] FOREIGN KEY ([SystemCommunicationId])
REFERENCES [dbo].[SystemCommunication] ([Id])
ON DELETE SET NULL;" );


            AddJobToAddCommunicationSystemCommunicationIdIndex();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.Communication", "SystemCommunicationId", "dbo.SystemCommunication" );
            DropIndex( "dbo.Communication", new[] { "SystemCommunicationId" } );
            DropColumn( "dbo.Communication", "SystemCommunicationId" );

            RemoveJobToAddCommunicationSystemCommunicationIdIndex();
        }

        private void AddJobToAddCommunicationSystemCommunicationIdIndex()
        {
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV125DataMigrationsAddSystemCommunicationIndexToCommunication'
                                AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_125_ADD_COMMUNICATION_SYSTEM_COMMUNICATION_ID_INDEX}'
            )
            BEGIN
                INSERT INTO [ServiceJob] (
                    [IsSystem]
                    ,[IsActive]
                    ,[Name]
                    ,[Description]
                    ,[Class]
                    ,[CronExpression]
                    ,[NotificationStatus]
                    ,[Guid]
                ) VALUES (
                    1
                    ,1
                    ,'Rock Update Helper v12.5 - Add index for Communication SystemCommunicationId.'
                    ,'This job will add an index for Communication SystemCommunicationId column.'
                    ,'Rock.Jobs.PostV125DataMigrationsAddSystemCommunicationIndexToCommunication'
                    ,'0 0 21 1/1 * ? *'
                    ,1
                    ,'{SystemGuid.ServiceJob.DATA_MIGRATIONS_125_ADD_COMMUNICATION_SYSTEM_COMMUNICATION_ID_INDEX}'
                );
            END" );
        }

        private void RemoveJobToAddCommunicationSystemCommunicationIdIndex()
        {
            Sql( $@"
                DELETE [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV125DataMigrationsAddSystemCommunicationIndexToCommunication'
                                AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_125_ADD_COMMUNICATION_SYSTEM_COMMUNICATION_ID_INDEX}'
                " );
        }
    }
}
