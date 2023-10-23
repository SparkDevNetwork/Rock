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
    public partial class PersistWorkflowIdColumn : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropColumn( "dbo.Workflow", "WorkflowId" );
            AddColumn( "dbo.Workflow", "WorkflowId", c => c.String() );

            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_160_UPDATE_WORKFLOWID_COLUMNS}'
            )
            BEGIN
                INSERT INTO [ServiceJob] (
                      [IsSystem]
                    , [IsActive]
                    , [Name]
                    , [Description]
                    , [Class]
                    , [CronExpression]
                    , [NotificationStatus]
                    , [Guid]
                ) VALUES (
                      1
                    , 1
                    , 'Rock Update Helper v16.0 - Update WorkflowId column.'
                    , 'This job updates all WorkflowId values on the Workflow table using the format specified in ''ufnWorkflow_GetWorkflowId''.'
                    , 'Rock.Jobs.PostV16UpdateWorkflowIds'
                    , '0 0 21 1/1 * ? *'
                    , 1
                    , '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_160_UPDATE_WORKFLOWID_COLUMNS}'
                );
            END" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.Workflow", "WorkflowId" );
            Sql( @"ALTER TABLE dbo.Workflow
    ADD WorkflowId AS [dbo].[ufnWorkflow_GetWorkflowId]([WorkflowTypeId],[WorkflowIdNumber])
" );
        }
    }
}
