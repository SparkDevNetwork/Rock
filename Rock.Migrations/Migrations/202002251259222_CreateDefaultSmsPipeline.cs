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
    public partial class CreateDefaultSmsPipeline : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Communication.SmsActions.SmsActionConversations",
                            "SMS Action Conversations",
                            "Rock.Communication.SmsActions.SmsActionConversations, Rock, Version=1.11.0.10, Culture=neutral, PublicKeyToken=null",
                            false, true, Rock.SystemGuid.EntityType.SMS_ACTION_CONVERSATION );

            AddDefaultSmsPipelineIfNoActionsExists();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RemoveDefaultSmsPipelineIfExists();
        }

        private void AddDefaultSmsPipelineIfNoActionsExists()
        {
            Sql( $@"IF (SELECT COUNT(*) FROM [dbo].[SmsAction]) = 0
                    BEGIN
                        DECLARE @SmsActionComponentEntityTypeId AS INT
                        SELECT TOP 1 @SmsActionComponentEntityTypeId = Id
                        FROM EntityType
                        WHERE GUID = '{Rock.SystemGuid.EntityType.SMS_ACTION_CONVERSATION}'

                        INSERT INTO [dbo].[SmsPipeline]
                            ( [Name]
                            , [IsActive]
                            , [Guid])
                        VALUES 
                            ( 'Default'
                            , 1
                            , 'DAFF8E4A-005E-4960-AE2F-BD50086520B0') --Save for later so we can delete.
                    
                        DECLARE @smsPipelineId AS INT = @@IDENTITY

                        INSERT INTO [dbo].[SmsAction]
                            ( [Name]
                            , [IsActive]
                            , [Order]
                            , [SmsActionComponentEntityTypeId]
                            , [ContinueAfterProcessing]
                            , [CreatedDateTime]
                            , [ModifiedDateTime]
                            , [CreatedByPersonAliasId]
                            , [ModifiedByPersonAliasId]
                            , [Guid]
                            , [ForeignId]
                            , [ForeignGuid]
                            , [ForeignKey]
                            , [SmsPipelineId])
                        VALUES
                            ( 'SMS Conversations'
                            , 1
                            , 0
                            , @SmsActionComponentEntityTypeId
                            , 0
                            , GETDATE()
                            , GETDATE()
                            , NULL
                            , NULL
                            , NEWID() --Convert to string so we can delete later.
                            , NULL
                            , NULL
                            , NULL
                            , @smsPipelineId)

                    END" );
        }

        private void RemoveDefaultSmsPipelineIfExists()
        {
            Sql( @"
                    DECLARE @smsPipelineId AS INT
                    SELECT TOP 1 @smsPipelineId = Id
                    FROM SmsPipeline
                    WHERE [Guid] = 'DAFF8E4A-005E-4960-AE2F-BD50086520B0'

                    DELETE SmsAction WHERE SmsPipelineId = @SmsPipelineId
                    DELETE SmsPipeline WHERE Id = @SmsPipelineId
            " );
        }
    }
}
