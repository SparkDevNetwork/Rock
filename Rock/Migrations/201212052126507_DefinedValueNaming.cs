//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    /// 
    /// </summary>
    public partial class DefinedValueNaming : RockMigration_1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey( "dbo.Person", "MaritalStatusId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Person", "PersonStatusId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Person", "RecordStatusId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Person", "RecordStatusReasonId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Person", "RecordTypeId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Person", "SuffixId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Person", "TitleId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.PhoneNumber", "NumberTypeId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.GroupLocation", "LocationTypeId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.GroupTypeLocationType", "LocationTypeId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Pledge", "FrequencyTypeId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Fund", "FundTypeId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.FinancialTransaction", "CurrencyTypeId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.FinancialTransaction", "CreditCardTypeId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.FinancialTransaction", "GatewayId", "dbo.PaymentGateway" );
            DropForeignKey( "dbo.FinancialTransaction", "SourceTypeId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Metric", "CollectionFrequencyId", "dbo.DefinedValue" );
            DropIndex( "dbo.Person", new[] { "MaritalStatusId" } );
            DropIndex( "dbo.Person", new[] { "PersonStatusId" } );
            DropIndex( "dbo.Person", new[] { "RecordStatusId" } );
            DropIndex( "dbo.Person", new[] { "RecordStatusReasonId" } );
            DropIndex( "dbo.Person", new[] { "RecordTypeId" } );
            DropIndex( "dbo.Person", new[] { "SuffixId" } );
            DropIndex( "dbo.Person", new[] { "TitleId" } );
            DropIndex( "dbo.PhoneNumber", new[] { "NumberTypeId" } );
            DropIndex( "dbo.GroupLocation", new[] { "LocationTypeId" } );
            DropIndex( "dbo.GroupTypeLocationType", new[] { "LocationTypeId" } );
            DropIndex( "dbo.Pledge", new[] { "FrequencyTypeId" } );
            DropIndex( "dbo.Fund", new[] { "FundTypeId" } );
            DropIndex( "dbo.FinancialTransaction", new[] { "CurrencyTypeId" } );
            DropIndex( "dbo.FinancialTransaction", new[] { "CreditCardTypeId" } );
            DropIndex( "dbo.FinancialTransaction", new[] { "GatewayId" } );
            DropIndex( "dbo.FinancialTransaction", new[] { "SourceTypeId" } );
            DropIndex( "dbo.Metric", new[] { "CollectionFrequencyId" } );
            AddColumn( "dbo.Person", "RecordTypeValueId", c => c.Int() );
            AddColumn( "dbo.Person", "RecordStatusValueId", c => c.Int() );
            AddColumn( "dbo.Person", "RecordStatusReasonValueId", c => c.Int() );
            AddColumn( "dbo.Person", "PersonStatusValueId", c => c.Int() );
            AddColumn( "dbo.Person", "SuffixValueId", c => c.Int() );
            AddColumn( "dbo.Person", "TitleValueId", c => c.Int() );
            AddColumn( "dbo.Person", "MaritalStatusValueId", c => c.Int() );
            AddColumn( "dbo.PhoneNumber", "NumberTypeValueId", c => c.Int() );
            AddColumn( "dbo.GroupLocation", "LocationTypeValueId", c => c.Int() );
            AddColumn( "dbo.GroupTypeLocationType", "LocationTypeValueId", c => c.Int( nullable: false ) );
            AddColumn( "dbo.Pledge", "FrequencyTypeValueId", c => c.Int() );
            AddColumn( "dbo.Fund", "FundTypeValueId", c => c.Int() );
            AddColumn( "dbo.FinancialTransaction", "TransactionDateTime", c => c.DateTime() );
            AddColumn( "dbo.FinancialTransaction", "CurrencyTypeValueId", c => c.Int() );
            AddColumn( "dbo.FinancialTransaction", "CreditCardTypeValueId", c => c.Int() );
            AddColumn( "dbo.FinancialTransaction", "PaymentGatewayId", c => c.Int() );
            AddColumn( "dbo.FinancialTransaction", "SourceTypeValueId", c => c.Int() );
            AddColumn( "dbo.Metric", "CollectionFrequencyValueId", c => c.Int() );
            DropPrimaryKey( "dbo.GroupTypeLocationType", new[] { "GroupTypeId", "LocationTypeId" } );
            AddPrimaryKey( "dbo.GroupTypeLocationType", new[] { "GroupTypeId", "LocationTypeValueId" } );
            AddForeignKey( "dbo.Person", "MaritalStatusValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.Person", "PersonStatusValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.Person", "RecordStatusValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.Person", "RecordStatusReasonValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.Person", "RecordTypeValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.Person", "SuffixValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.Person", "TitleValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.PhoneNumber", "NumberTypeValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.GroupLocation", "LocationTypeValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.GroupTypeLocationType", "LocationTypeValueId", "dbo.DefinedValue", "Id", cascadeDelete: true );
            AddForeignKey( "dbo.Pledge", "FrequencyTypeValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.Fund", "FundTypeValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.FinancialTransaction", "CurrencyTypeValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.FinancialTransaction", "CreditCardTypeValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.FinancialTransaction", "PaymentGatewayId", "dbo.PaymentGateway", "Id" );
            AddForeignKey( "dbo.FinancialTransaction", "SourceTypeValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.Metric", "CollectionFrequencyValueId", "dbo.DefinedValue", "Id" );
            CreateIndex( "dbo.Person", "MaritalStatusValueId" );
            CreateIndex( "dbo.Person", "PersonStatusValueId" );
            CreateIndex( "dbo.Person", "RecordStatusValueId" );
            CreateIndex( "dbo.Person", "RecordStatusReasonValueId" );
            CreateIndex( "dbo.Person", "RecordTypeValueId" );
            CreateIndex( "dbo.Person", "SuffixValueId" );
            CreateIndex( "dbo.Person", "TitleValueId" );
            CreateIndex( "dbo.PhoneNumber", "NumberTypeValueId" );
            CreateIndex( "dbo.GroupLocation", "LocationTypeValueId" );
            CreateIndex( "dbo.GroupTypeLocationType", "LocationTypeValueId" );
            CreateIndex( "dbo.Pledge", "FrequencyTypeValueId" );
            CreateIndex( "dbo.Fund", "FundTypeValueId" );
            CreateIndex( "dbo.FinancialTransaction", "CurrencyTypeValueId" );
            CreateIndex( "dbo.FinancialTransaction", "CreditCardTypeValueId" );
            CreateIndex( "dbo.FinancialTransaction", "PaymentGatewayId" );
            CreateIndex( "dbo.FinancialTransaction", "SourceTypeValueId" );
            CreateIndex( "dbo.Metric", "CollectionFrequencyValueId" );
            DropColumnMoveDataUp( "dbo.Person", "RecordTypeId", "RecordTypeValueId" );
            DropColumnMoveDataUp( "dbo.Person", "RecordStatusId", "RecordStatusValueId" );
            DropColumnMoveDataUp( "dbo.Person", "RecordStatusReasonId", "RecordStatusReasonValueId" );
            DropColumnMoveDataUp( "dbo.Person", "PersonStatusId", "PersonStatusValueId" );
            DropColumnMoveDataUp( "dbo.Person", "SuffixId", "SuffixValueId" );
            DropColumnMoveDataUp( "dbo.Person", "TitleId", "TitleValueId" );
            DropColumnMoveDataUp( "dbo.Person", "MaritalStatusId", "MaritalStatusValueId" );
            DropColumnMoveDataUp( "dbo.PhoneNumber", "NumberTypeId", "NumberTypeValueId" );
            DropColumnMoveDataUp( "dbo.GroupLocation", "LocationTypeId", "LocationTypeValueId" );
            DropColumnMoveDataUp( "dbo.GroupTypeLocationType", "LocationTypeId", "LocationTypeValueId" );
            DropColumnMoveDataUp( "dbo.Pledge", "FrequencyTypeId", "FrequencyTypeValueId" );
            DropColumnMoveDataUp( "dbo.Fund", "FundTypeId", "FundTypeValueId" );
            DropColumnMoveDataUp( "dbo.FinancialTransaction", "TransactionDate", "TransactionDateTime" );
            DropColumnMoveDataUp( "dbo.FinancialTransaction", "CurrencyTypeId", "CurrencyTypeValueId" );
            DropColumnMoveDataUp( "dbo.FinancialTransaction", "CreditCardTypeId", "CreditCardTypeValueId" );
            DropColumnMoveDataUp( "dbo.FinancialTransaction", "GatewayId", "PaymentGatewayId" );
            DropColumnMoveDataUp( "dbo.FinancialTransaction", "SourceTypeId", "SourceTypeValueId" );
            DropColumnMoveDataUp( "dbo.Metric", "CollectionFrequencyId", "CollectionFrequencyValueId" );
        }

        /// <summary>
        /// Drops the column move data up.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="oldColumn">The old column.</param>
        /// <param name="newColumn">The new column.</param>
        private void DropColumnMoveDataUp( string tableName, string oldColumn, string newColumn )
        {
            string updateSql = "UPDATE {0} set {2} = {1}";
            Sql( string.Format( updateSql, tableName, oldColumn, newColumn ) );

            DropColumn( tableName, oldColumn );
        }

        /// <summary>
        /// Drops the column move data down.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="oldColumn">The old column.</param>
        /// <param name="newColumn">The new column.</param>
        private void DropColumnMoveDataDown( string tableName, string oldColumn, string newColumn )
        {
            string updateSql = "UPDATE {0} set {1} = {2}";
            Sql( string.Format( updateSql, tableName, oldColumn, newColumn ) );

            DropColumn( tableName, newColumn );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn( "dbo.Metric", "CollectionFrequencyId", c => c.Int() );
            AddColumn( "dbo.FinancialTransaction", "SourceTypeId", c => c.Int() );
            AddColumn( "dbo.FinancialTransaction", "GatewayId", c => c.Int() );
            AddColumn( "dbo.FinancialTransaction", "CreditCardTypeId", c => c.Int() );
            AddColumn( "dbo.FinancialTransaction", "CurrencyTypeId", c => c.Int() );
            AddColumn( "dbo.FinancialTransaction", "TransactionDate", c => c.DateTime() );
            AddColumn( "dbo.Fund", "FundTypeId", c => c.Int() );
            AddColumn( "dbo.Pledge", "FrequencyTypeId", c => c.Int() );
            AddColumn( "dbo.GroupTypeLocationType", "LocationTypeId", c => c.Int( nullable: false ) );
            AddColumn( "dbo.GroupLocation", "LocationTypeId", c => c.Int() );
            AddColumn( "dbo.PhoneNumber", "NumberTypeId", c => c.Int() );
            AddColumn( "dbo.Person", "MaritalStatusId", c => c.Int() );
            AddColumn( "dbo.Person", "SuffixId", c => c.Int() );
            AddColumn( "dbo.Person", "TitleId", c => c.Int() );
            AddColumn( "dbo.Person", "PersonStatusId", c => c.Int() );
            AddColumn( "dbo.Person", "RecordStatusReasonId", c => c.Int() );
            AddColumn( "dbo.Person", "RecordStatusId", c => c.Int() );
            AddColumn( "dbo.Person", "RecordTypeId", c => c.Int() );
            DropIndex( "dbo.Metric", new[] { "CollectionFrequencyValueId" } );
            DropIndex( "dbo.FinancialTransaction", new[] { "SourceTypeValueId" } );
            DropIndex( "dbo.FinancialTransaction", new[] { "PaymentGatewayId" } );
            DropIndex( "dbo.FinancialTransaction", new[] { "CreditCardTypeValueId" } );
            DropIndex( "dbo.FinancialTransaction", new[] { "CurrencyTypeValueId" } );
            DropIndex( "dbo.Fund", new[] { "FundTypeValueId" } );
            DropIndex( "dbo.Pledge", new[] { "FrequencyTypeValueId" } );
            DropIndex( "dbo.GroupTypeLocationType", new[] { "LocationTypeValueId" } );
            DropIndex( "dbo.GroupLocation", new[] { "LocationTypeValueId" } );
            DropIndex( "dbo.PhoneNumber", new[] { "NumberTypeValueId" } );
            DropIndex( "dbo.Person", new[] { "SuffixValueId" } );
            DropIndex( "dbo.Person", new[] { "TitleValueId" } );
            DropIndex( "dbo.Person", new[] { "RecordTypeValueId" } );
            DropIndex( "dbo.Person", new[] { "RecordStatusReasonValueId" } );
            DropIndex( "dbo.Person", new[] { "RecordStatusValueId" } );
            DropIndex( "dbo.Person", new[] { "PersonStatusValueId" } );
            DropIndex( "dbo.Person", new[] { "MaritalStatusValueId" } );
            DropForeignKey( "dbo.Metric", "CollectionFrequencyValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.FinancialTransaction", "SourceTypeValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.FinancialTransaction", "PaymentGatewayId", "dbo.PaymentGateway" );
            DropForeignKey( "dbo.FinancialTransaction", "CreditCardTypeValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.FinancialTransaction", "CurrencyTypeValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Fund", "FundTypeValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Pledge", "FrequencyTypeValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.GroupTypeLocationType", "LocationTypeValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.GroupLocation", "LocationTypeValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.PhoneNumber", "NumberTypeValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Person", "SuffixValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Person", "TitleValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Person", "RecordTypeValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Person", "RecordStatusReasonValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Person", "RecordStatusValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Person", "PersonStatusValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.Person", "MaritalStatusValueId", "dbo.DefinedValue" );
            DropPrimaryKey( "dbo.GroupTypeLocationType", new[] { "GroupTypeId", "LocationTypeValueId" } );
            AddPrimaryKey( "dbo.GroupTypeLocationType", new[] { "GroupTypeId", "LocationTypeId" } );

            DropColumnMoveDataDown( "dbo.Person", "RecordTypeId", "RecordTypeValueId" );
            DropColumnMoveDataDown( "dbo.Person", "RecordStatusId", "RecordStatusValueId" );
            DropColumnMoveDataDown( "dbo.Person", "RecordStatusReasonId", "RecordStatusReasonValueId" );
            DropColumnMoveDataDown( "dbo.Person", "PersonStatusId", "PersonStatusValueId" );
            DropColumnMoveDataDown( "dbo.Person", "SuffixId", "SuffixValueId" );
            DropColumnMoveDataDown( "dbo.Person", "TitleId", "TitleValueId" );
            DropColumnMoveDataDown( "dbo.Person", "MaritalStatusId", "MaritalStatusValueId" );
            DropColumnMoveDataDown( "dbo.PhoneNumber", "NumberTypeId", "NumberTypeValueId" );
            DropColumnMoveDataDown( "dbo.GroupLocation", "LocationTypeId", "LocationTypeValueId" );
            DropColumnMoveDataDown( "dbo.GroupTypeLocationType", "LocationTypeId", "LocationTypeValueId" );
            DropColumnMoveDataDown( "dbo.Pledge", "FrequencyTypeId", "FrequencyTypeValueId" );
            DropColumnMoveDataDown( "dbo.Fund", "FundTypeId", "FundTypeValueId" );
            DropColumnMoveDataDown( "dbo.FinancialTransaction", "TransactionDate", "TransactionDateTime" );
            DropColumnMoveDataDown( "dbo.FinancialTransaction", "CurrencyTypeId", "CurrencyTypeValueId" );
            DropColumnMoveDataDown( "dbo.FinancialTransaction", "CreditCardTypeId", "CreditCardTypeValueId" );
            DropColumnMoveDataDown( "dbo.FinancialTransaction", "GatewayId", "PaymentGatewayId" );
            DropColumnMoveDataDown( "dbo.FinancialTransaction", "SourceTypeId", "SourceTypeValueId" );
            DropColumnMoveDataDown( "dbo.Metric", "CollectionFrequencyId", "CollectionFrequencyValueId" );

            CreateIndex( "dbo.Metric", "CollectionFrequencyId" );
            CreateIndex( "dbo.FinancialTransaction", "SourceTypeId" );
            CreateIndex( "dbo.FinancialTransaction", "GatewayId" );
            CreateIndex( "dbo.FinancialTransaction", "CreditCardTypeId" );
            CreateIndex( "dbo.FinancialTransaction", "CurrencyTypeId" );
            CreateIndex( "dbo.Fund", "FundTypeId" );
            CreateIndex( "dbo.Pledge", "FrequencyTypeId" );
            CreateIndex( "dbo.GroupTypeLocationType", "LocationTypeId" );
            CreateIndex( "dbo.GroupLocation", "LocationTypeId" );
            CreateIndex( "dbo.PhoneNumber", "NumberTypeId" );
            CreateIndex( "dbo.Person", "SuffixId" );
            CreateIndex( "dbo.Person", "TitleId" );
            CreateIndex( "dbo.Person", "RecordTypeId" );
            CreateIndex( "dbo.Person", "RecordStatusReasonId" );
            CreateIndex( "dbo.Person", "RecordStatusId" );
            CreateIndex( "dbo.Person", "PersonStatusId" );
            CreateIndex( "dbo.Person", "MaritalStatusId" );
            AddForeignKey( "dbo.Metric", "CollectionFrequencyId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.FinancialTransaction", "SourceTypeId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.FinancialTransaction", "GatewayId", "dbo.PaymentGateway", "Id" );
            AddForeignKey( "dbo.FinancialTransaction", "CreditCardTypeId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.FinancialTransaction", "CurrencyTypeId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.Fund", "FundTypeId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.Pledge", "FrequencyTypeId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.GroupTypeLocationType", "LocationTypeId", "dbo.DefinedValue", "Id", cascadeDelete: true );
            AddForeignKey( "dbo.GroupLocation", "LocationTypeId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.PhoneNumber", "NumberTypeId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.Person", "SuffixId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.Person", "TitleId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.Person", "RecordTypeId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.Person", "RecordStatusReasonId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.Person", "RecordStatusId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.Person", "PersonStatusId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.Person", "MaritalStatusId", "dbo.DefinedValue", "Id" );
        }
    }
}
