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
    public partial class AddEventRegistrationPaymentPlansSupport : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.RegistrationInstance", "PaymentDeadlineDate", c => c.DateTime( storeType: "date" ));
            AddColumn("dbo.Registration", "PaymentPlanFinancialScheduledTransactionId", c => c.Int());
            AddColumn("dbo.RegistrationTemplate", "IsPaymentPlanAllowed", c => c.Boolean(nullable: false));
            AddColumn("dbo.RegistrationTemplate", "PaymentPlanFrequencyValueIds", c => c.String(maxLength: 50));
            AddColumn("dbo.RegistrationTemplate", "ConnectionStatusValueId", c => c.Int());
            AddColumn("dbo.RegistrationTemplateFormField", "IsLockedIfValuesExist", c => c.Boolean(nullable: false));
            CreateIndex("dbo.Registration", "PaymentPlanFinancialScheduledTransactionId");
            CreateIndex("dbo.RegistrationTemplate", "ConnectionStatusValueId");
            AddForeignKey("dbo.Registration", "PaymentPlanFinancialScheduledTransactionId", "dbo.FinancialScheduledTransaction", "Id");
            AddForeignKey("dbo.RegistrationTemplate", "ConnectionStatusValueId", "dbo.DefinedValue", "Id");

            // Add Financial Frequency DefinedType > Interval Days DefinedValue attribute and values.
            RockMigrationHelper.AddDefinedTypeAttribute( 
                SystemGuid.DefinedType.FINANCIAL_FREQUENCY, 
                SystemGuid.FieldType.INTEGER,
                "Interval Days", // Name
                "IntervalDays",  // Key
                "The number of days between payment events. In some cases, this value will be approximate.", 
                0,               // Order
                true,            // IsGridColumn
                string.Empty,    // DefaultValue
                false,           // IsMultiValue
                false,           // IsRequired
                SystemGuid.Attribute.DEFINED_TYPE_FINANCIAL_FREQUENCY_INTERVAL_DAYS );
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME, SystemGuid.Attribute.DEFINED_TYPE_FINANCIAL_FREQUENCY_INTERVAL_DAYS, "0" );
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY, SystemGuid.Attribute.DEFINED_TYPE_FINANCIAL_FREQUENCY_INTERVAL_DAYS, "7" );
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_BIWEEKLY, SystemGuid.Attribute.DEFINED_TYPE_FINANCIAL_FREQUENCY_INTERVAL_DAYS, "14" );
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_FIRST_AND_FIFTEENTH, SystemGuid.Attribute.DEFINED_TYPE_FINANCIAL_FREQUENCY_INTERVAL_DAYS, "15" );
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TWICEMONTHLY, SystemGuid.Attribute.DEFINED_TYPE_FINANCIAL_FREQUENCY_INTERVAL_DAYS, "16" );
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY, SystemGuid.Attribute.DEFINED_TYPE_FINANCIAL_FREQUENCY_INTERVAL_DAYS, "31" );
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_QUARTERLY, SystemGuid.Attribute.DEFINED_TYPE_FINANCIAL_FREQUENCY_INTERVAL_DAYS, "92" );
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TWICEYEARLY, SystemGuid.Attribute.DEFINED_TYPE_FINANCIAL_FREQUENCY_INTERVAL_DAYS, "183" );
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_YEARLY, SystemGuid.Attribute.DEFINED_TYPE_FINANCIAL_FREQUENCY_INTERVAL_DAYS, "365" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Delete Financial Frequency DefinedType > Interval Days DefinedValue attribute and values.
            Sql( $@"
                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{SystemGuid.Attribute.DEFINED_TYPE_FINANCIAL_FREQUENCY_INTERVAL_DAYS}')

                DELETE [AttributeValue]
                WHERE [AttributeId] = @AttributeId" );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.DEFINED_TYPE_FINANCIAL_FREQUENCY_INTERVAL_DAYS );

            DropForeignKey("dbo.RegistrationTemplate", "ConnectionStatusValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.Registration", "PaymentPlanFinancialScheduledTransactionId", "dbo.FinancialScheduledTransaction");
            DropIndex("dbo.RegistrationTemplate", new[] { "ConnectionStatusValueId" });
            DropIndex("dbo.Registration", new[] { "PaymentPlanFinancialScheduledTransactionId" });
            DropColumn("dbo.RegistrationTemplateFormField", "IsLockedIfValuesExist");
            DropColumn("dbo.RegistrationTemplate", "ConnectionStatusValueId");
            DropColumn("dbo.RegistrationTemplate", "PaymentPlanFrequencyValueIds");
            DropColumn("dbo.RegistrationTemplate", "IsPaymentPlanAllowed");
            DropColumn("dbo.Registration", "PaymentPlanFinancialScheduledTransactionId");
            DropColumn("dbo.RegistrationInstance", "PaymentDeadlineDate");
        }
    }
}
