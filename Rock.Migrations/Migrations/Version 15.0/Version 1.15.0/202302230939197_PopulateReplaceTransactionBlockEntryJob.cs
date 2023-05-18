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
    public partial class PopulateReplaceTransactionBlockEntryJob : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_150_REPLACE_TRANSACTION_ENTRY_BLOCKS_WITH_UTILITY_PAYMENT_ENTRY_BLOCK}'
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
                    , 'Rock Update Helper v15.0 - Replace TransactionEntry Blocks with UtilityPaymentEntry block.'
                    , 'This job will replace all existing instances of the TransactionEntryBlock with a new instance of the UtilityPayment block whiles preserving block settings.'
                    , 'Rock.Jobs.PostV15DataMigrationsReplaceTransactionEntryBlocksWithUtilityPaymentBlock'
                    , '0 0 21 1/1 * ? *'
                    , 1
                    , '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_150_REPLACE_TRANSACTION_ENTRY_BLOCKS_WITH_UTILITY_PAYMENT_ENTRY_BLOCK}'
                );
            END" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //
        }
    }
}
