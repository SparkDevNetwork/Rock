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
    public partial class AddDateKeyToSelectTables : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Registration", "CreatedDateKey", c => c.Int());

            // Just in case it takes a while, PostV110DataMigrationsUpdateDateKeyValues will change this column to not null
            AddColumn( "dbo.AttendanceOccurrence", "OccurrenceDateKey", c => c.Int());
            
            AddColumn("dbo.Step", "StartDateKey", c => c.Int());
            AddColumn("dbo.Step", "EndDateKey", c => c.Int());
            AddColumn("dbo.Step", "CompletedDateKey", c => c.Int());

            // Just in case it takes a while, PostV110DataMigrationsUpdateDateKeyValues will change this column to not null
            AddColumn( "dbo.BenevolenceRequest", "RequestDateKey", c => c.Int());
            
            AddColumn("dbo.Communication", "SendDateKey", c => c.Int());
            AddColumn("dbo.ConnectionRequest", "CreatedDateKey", c => c.Int());
            AddColumn("dbo.FinancialTransaction", "TransactionDateKey", c => c.Int());
            AddColumn("dbo.FinancialTransaction", "SettledDateKey", c => c.Int());

            // Just in case it takes a while, PostV110DataMigrationsUpdateDateKeyValues will change this column to not null
            AddColumn("dbo.FinancialPledge", "StartDateKey", c => c.Int());

            // Just in case it takes a while, PostV110DataMigrationsUpdateDateKeyValues will change this column to not null
            AddColumn( "dbo.FinancialPledge", "EndDateKey", c => c.Int());

            // Just in case it takes a while, PostV110DataMigrationsUpdateDateKeyValues will change this column to not null            
            AddColumn( "dbo.Interaction", "InteractionDateKey", c => c.Int());
            
            AddColumn("dbo.MetricValue", "MetricValueDateKey", c => c.Int());


            //  Just in case these take a while,  PostV110DataMigrationsUpdateDateKeyValues will take care of creating indexes
            /*
            CreateIndex( "dbo.Registration", "CreatedDateKey" );
            CreateIndex( "dbo.AttendanceOccurrence", "OccurrenceDateKey" );
            CreateIndex( "dbo.Step", "StartDateKey" );
            CreateIndex( "dbo.Step", "EndDateKey" );
            CreateIndex( "dbo.Step", "CompletedDateKey" );
            CreateIndex( "dbo.BenevolenceRequest", "RequestDateKey" );
            CreateIndex( "dbo.Communication", "SendDateKey" );
            CreateIndex( "dbo.ConnectionRequest", "CreatedDateKey" );
            CreateIndex( "dbo.FinancialTransaction", "TransactionDateKey" );
            CreateIndex( "dbo.FinancialTransaction", "SettledDateKey" );
            CreateIndex( "dbo.FinancialPledge", "StartDateKey" );
            CreateIndex( "dbo.FinancialPledge", "EndDateKey" );
            CreateIndex( "dbo.Interaction", "InteractionDateKey" );
            CreateIndex( "dbo.MetricValue", "MetricValueDateKey" );
            */

            AddJobToUpdateDateKeyValues();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropDateKeyIndexesIfExist();

            DropColumn("dbo.MetricValue", "MetricValueDateKey");
            DropColumn("dbo.Interaction", "InteractionDateKey");
            DropColumn("dbo.FinancialPledge", "EndDateKey");
            DropColumn("dbo.FinancialPledge", "StartDateKey");
            DropColumn("dbo.FinancialTransaction", "SettledDateKey");
            DropColumn("dbo.FinancialTransaction", "TransactionDateKey");
            DropColumn("dbo.ConnectionRequest", "CreatedDateKey");
            DropColumn("dbo.Communication", "SendDateKey");
            DropColumn("dbo.BenevolenceRequest", "RequestDateKey");
            DropColumn("dbo.Step", "CompletedDateKey");
            DropColumn("dbo.Step", "EndDateKey");
            DropColumn("dbo.Step", "StartDateKey");
            DropColumn("dbo.AttendanceOccurrence", "OccurrenceDateKey");
            DropColumn("dbo.Registration", "CreatedDateKey");

            RemoveJobToUpdateDateKeyValues();
        }


        /// <summary>
        /// Drops the date key indexes if exist.
        /// </summary>
        private void DropDateKeyIndexesIfExist()
        {
            Sql( @"
IF EXISTS (
        SELECT *
        FROM sys.indexes
        WHERE name = 'IX_CreatedDateKey' AND object_id = OBJECT_ID('Registration')
        )
BEGIN
    DROP INDEX [IX_CreatedDateKey] ON [dbo].[Registration]
END

IF EXISTS (
        SELECT *
        FROM sys.indexes
        WHERE name = 'IX_OccurrenceDateKey' AND object_id = OBJECT_ID('AttendanceOccurrence')
        )
BEGIN
    DROP INDEX [IX_OccurrenceDateKey] ON [dbo].[AttendanceOccurrence]
END

IF EXISTS (
        SELECT *
        FROM sys.indexes
        WHERE name = 'IX_StartDateKey' AND object_id = OBJECT_ID('Step')
        )
BEGIN
    DROP INDEX [IX_StartDateKey] ON [dbo].[Step]
END

IF EXISTS (
        SELECT *
        FROM sys.indexes
        WHERE name = 'IX_EndDateKey' AND object_id = OBJECT_ID('Step')
        )
BEGIN
    DROP INDEX [IX_EndDateKey] ON [dbo].[Step]
END

IF EXISTS (
        SELECT *
        FROM sys.indexes
        WHERE name = 'IX_CompletedDateKey' AND object_id = OBJECT_ID('Step')
        )
BEGIN
    DROP INDEX [IX_CompletedDateKey] ON [dbo].[Step]
END

IF EXISTS (
        SELECT *
        FROM sys.indexes
        WHERE name = 'IX_RequestDateKey' AND object_id = OBJECT_ID('BenevolenceRequest')
        )
BEGIN
    DROP INDEX [IX_RequestDateKey] ON [dbo].[BenevolenceRequest]
END

IF EXISTS (
        SELECT *
        FROM sys.indexes
        WHERE name = 'IX_SendDateKey' AND object_id = OBJECT_ID('Communication')
        )
BEGIN
    DROP INDEX [IX_SendDateKey] ON [dbo].[Communication]
END

IF EXISTS (
        SELECT *
        FROM sys.indexes
        WHERE name = 'IX_CreatedDateKey' AND object_id = OBJECT_ID('ConnectionRequest')
        )
BEGIN
    DROP INDEX [IX_CreatedDateKey] ON [dbo].[ConnectionRequest]
END

IF EXISTS (
        SELECT *
        FROM sys.indexes
        WHERE name = 'IX_TransactionDateKey' AND object_id = OBJECT_ID('FinancialTransaction')
        )
BEGIN
    DROP INDEX [IX_TransactionDateKey] ON [dbo].[FinancialTransaction]
END

IF EXISTS (
        SELECT *
        FROM sys.indexes
        WHERE name = 'IX_SettledDateKey' AND object_id = OBJECT_ID('FinancialTransaction')
        )
BEGIN
    DROP INDEX [IX_SettledDateKey] ON [dbo].[FinancialTransaction]
END

IF EXISTS (
        SELECT *
        FROM sys.indexes
        WHERE name = 'IX_StartDateKey' AND object_id = OBJECT_ID('FinancialPledge')
        )
BEGIN
    DROP INDEX [IX_StartDateKey] ON [dbo].[FinancialPledge]
END

IF EXISTS (
        SELECT *
        FROM sys.indexes
        WHERE name = 'IX_EndDateKey' AND object_id = OBJECT_ID('FinancialPledge')
        )
BEGIN
    DROP INDEX [IX_EndDateKey] ON [dbo].[FinancialPledge]
END

IF EXISTS (
        SELECT *
        FROM sys.indexes
        WHERE name = 'IX_InteractionDateKey' AND object_id = OBJECT_ID('Interaction')
        )
BEGIN
    DROP INDEX [IX_InteractionDateKey] ON [dbo].[Interaction]
END
" );
        }

        private void AddJobToUpdateDateKeyValues()
        {
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV110DataMigrationsUpdateDateKeyValues'
                                AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_110_POPULATE_DATE_KEYS}'
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
                    ,'Rock Update Helper v11.0 - Add DateKey Fields'
                    ,'Runs data updates to set the DateKey fields added as part of the v11.0 update.'
                    ,'Rock.Jobs.PostV110DataMigrationsUpdateDateKeyValues'
                    ,'0 0 21 1/1 * ? *'
                    ,1
                    ,'{SystemGuid.ServiceJob.DATA_MIGRATIONS_110_POPULATE_DATE_KEYS}'
                );
            END" );
        }

        private void RemoveJobToUpdateDateKeyValues()
        {
            Sql( $@"
                DELETE [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV110DataMigrationsUpdateDateKeyValues'
                                AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_110_POPULATE_DATE_KEYS}'
                " );
        }
    }
}
