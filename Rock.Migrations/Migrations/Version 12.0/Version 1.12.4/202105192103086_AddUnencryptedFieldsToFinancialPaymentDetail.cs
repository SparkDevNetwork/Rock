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

    /// <summary>
    ///
    /// </summary>
    public partial class AddUnencryptedFieldsToFinancialPaymentDetail : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.FinancialPaymentDetail", "NameOnCard", c => c.String() );
            AddColumn( "dbo.FinancialPaymentDetail", "CardExpirationDate", c => c.DateTime() );
            AddColumn( "dbo.FinancialPaymentDetail", "ExpirationMonth", c => c.Int() );
            AddColumn( "dbo.FinancialPaymentDetail", "ExpirationYear", c => c.Int() );

            AddJobToUpdateUnecryptedFields();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.FinancialPaymentDetail", "CardExpirationDate" );
            DropColumn( "dbo.FinancialPaymentDetail", "NameOnCard" );
            DropColumn( "dbo.FinancialPaymentDetail", "ExpirationMonth" );
            DropColumn( "dbo.FinancialPaymentDetail", "ExpirationYear" );

            RemoveJobToUpdateUnecryptedFields();
        }

        private void AddJobToUpdateUnecryptedFields()
        {
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV124DataMigrationUnencryptPaymentDetailFields'
                                AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_124_DECRYPT_FINANCIAL_PAYMENT_DETAILS}'
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
                    ,'Rock Update Helper v12.4 - Decrypt expiration month / year and name on card fields.'
                    ,'This job will decrypt the expiration month / year and the name on card fields.'
                    ,'Rock.Jobs.PostV124DataMigrationUnencryptPaymentDetailFields'
                    ,'0 0 21 1/1 * ? *'
                    ,1
                    ,'{SystemGuid.ServiceJob.DATA_MIGRATIONS_124_DECRYPT_FINANCIAL_PAYMENT_DETAILS}'
                );
            END" );
        }

        private void RemoveJobToUpdateUnecryptedFields()
        {
            Sql( $@"
                DELETE [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV124DataMigrationUnencryptPaymentDetailFields'
                                AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_124_DECRYPT_FINANCIAL_PAYMENT_DETAILS}'
                " );
        }
    }
}
