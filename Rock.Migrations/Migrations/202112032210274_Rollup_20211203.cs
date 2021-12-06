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
    public partial class Rollup_20211203 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateRegistrationTemplateLavaTemplateContactName_Up();
            UpdatedPreSelectedOptionsFormatLava_Up();
            AddDaysSinceTransactionOption_Up();
            UpdateOnSiteCollectionDefinedValue_Up();
            CreditCardValueIconImageUp();
        }

        /// <summary>
        /// SK: Fixed Registration Template related lava template to replace the old ContactName with ContactPersonAlias.Person.FullName
        /// </summary>
        private void UpdateRegistrationTemplateLavaTemplateContactName_Up()
        {
            string newValue = "{{ RegistrationInstance.ContactPersonAlias.Person.FullName }}";
            string oldValue = "{{ RegistrationInstance.ContactName }}";

            Sql( $@"
                    DECLARE @defaultReminderEmailAttributeId INT = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = '10fed7fa-8e42-4a28-b13f-0dc65d1f7be5'),
                    @defaultPaymentReminderEmailAttributeId INT = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = 'c8ab59c0-3074-418e-8493-2bced16d5034'),
                    @defaultWaitListTransitionEmailAttributeId INT = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = 'e50ac4c6-8c6c-46ec-85d0-0ed1e91ea099')
            
                    UPDATE
                        [dbo].[Attribute] 
                    SET [DefaultValue] = REPLACE([DefaultValue], '{oldValue}', '{newValue}')
                    WHERE
                        [DefaultValue] LIKE '%{oldValue}%'
                    AND [Id] In (@defaultReminderEmailAttributeId, @defaultPaymentReminderEmailAttributeId, @defaultWaitListTransitionEmailAttributeId)

                    UPDATE
	                    [dbo].[RegistrationTemplate]
                    SET [ConfirmationEmailTemplate] = REPLACE([ConfirmationEmailTemplate], '{oldValue}', '{newValue}'),
	                    [ReminderEmailTemplate] = REPLACE([ReminderEmailTemplate], '{oldValue}', '{newValue}'),
	                    [PaymentReminderEmailTemplate] = REPLACE([PaymentReminderEmailTemplate], '{oldValue}', '{newValue}'),
	                    [WaitListTransitionEmailTemplate] = REPLACE([WaitListTransitionEmailTemplate], '{oldValue}', '{newValue}')" );
        }

        /// <summary>
        /// SK: Updated Pre Selected Options Format Lava
        /// </summary>
        public void UpdatedPreSelectedOptionsFormatLava_Up()
        {
            string newValue = "{{ Schedule.Name }} - {{ Group.Name }} - {{ Location.Name }} {% if DisplayLocationCount == true %} <span class='ml-3'>Count: {{ LocationCount }}</span> {% endif %}".Replace( "'", "''" );
            string oldValue = "{{ Schedule.Name }} - {{ Group.Name }} - {{ Location.Name }}".Replace( "'", "''" );

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Value" );

            Sql( $@"DECLARE @attributeId INT = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = '55580865-E792-469F-B45C-45713477D033')
UPDATE [dbo].[AttributeValue] 
SET [Value] = REPLACE({targetColumn}, '{oldValue}', '{newValue}')
WHERE {targetColumn} NOT LIKE '%{newValue}%' AND [AttributeId] = @attributeId" );
        }

        /// <summary>
        /// MP: Add 'Show Days Since Last Transaction' block setting
        /// </summary>
        private void AddDaysSinceTransactionOption_Up()
        {
            // Attribute for BlockType
            //   BlockType: Transaction List
            //   Category: Finance
            //   Attribute: Show Days Since Last Transaction
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Days Since Last Transaction", "ShowDaysSinceLastTransaction", "Show Days Since Last Transaction", @"Show the number of days between the transaction and the transaction listed next to the transaction", 12, @"False", "D9A0DB03-1E45-46EE-A352-DA04E9A1F96A" );

            // Set ShowDaysSinceLastTransaction to true only the Person Contribution Tab
            RockMigrationHelper.UpdateBlockAttributeValue( "9382B285-3EF6-47F7-94BB-A47C498196A3", "D9A0DB03-1E45-46EE-A352-DA04E9A1F96A", "True" );
        }

        /// <summary>
        /// MP: Update the Value of 'On-Site Collection' to 'On-Site'. Only update it hasn't changed from 'On-Site Collection' to something custom.
        /// </summary>
        private void UpdateOnSiteCollectionDefinedValue_Up()
        {
            Sql( @"
UPDATE [DefinedValue]
SET [Value] = 'On-Site'
WHERE [Guid] = 'BE7ECF50-52BC-4774-808D-574BA842DB98' AND [Value] = 'On-Site Collection'" );
        }

        private void CreditCardValueIconImageUp()
        {
            RockMigrationHelper.AddDefinedTypeAttribute( Rock.SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE,
                Rock.SystemGuid.FieldType.URL_LINK,
                "Icon Image",
                Rock.SystemKey.CreditCardTypeAttributeKey.IconImage,
                "When displaying payment information for this card type, this image may be used to provide a visual reference of the card. An SVG is strongly recommended.",
                2,
                string.Empty,
                "84FA697A-64CF-44F7-9836-FC439E61634B" );

            RockMigrationHelper.AddDefinedValueAttributeValue( Rock.SystemGuid.DefinedValue.CREDITCARD_TYPE_AMEX,
                "84FA697A-64CF-44F7-9836-FC439E61634B",
                "/Assets/Images/currency/amex.svg" );

            RockMigrationHelper.AddDefinedValueAttributeValue( Rock.SystemGuid.DefinedValue.CREDITCARD_TYPE_DINERS_CLUB,
                "84FA697A-64CF-44F7-9836-FC439E61634B",
                "/Assets/Images/currency/diners.svg" );

            RockMigrationHelper.AddDefinedValueAttributeValue( Rock.SystemGuid.DefinedValue.CREDITCARD_TYPE_DISCOVER,
                "84FA697A-64CF-44F7-9836-FC439E61634B",
                "/Assets/Images/currency/discover.svg" );

            RockMigrationHelper.AddDefinedValueAttributeValue( Rock.SystemGuid.DefinedValue.CREDITCARD_TYPE_JCB,
                "84FA697A-64CF-44F7-9836-FC439E61634B",
                "/Assets/Images/currency/jcb.svg" );

            RockMigrationHelper.AddDefinedValueAttributeValue( Rock.SystemGuid.DefinedValue.CREDITCARD_TYPE_MASTERCARD,
                "84FA697A-64CF-44F7-9836-FC439E61634B",
                "/Assets/Images/currency/mastercard.svg" );

            RockMigrationHelper.AddDefinedValueAttributeValue( Rock.SystemGuid.DefinedValue.CREDITCARD_TYPE_VISA,
                "84FA697A-64CF-44F7-9836-FC439E61634B",
                "/Assets/Images/currency/visa.svg" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CreditCardValueIconImageDown();
        }

        private void CreditCardValueIconImageDown()
        {
            RockMigrationHelper.DeleteAttribute( "84FA697A-64CF-44F7-9836-FC439E61634B" );
        }
    }
}
