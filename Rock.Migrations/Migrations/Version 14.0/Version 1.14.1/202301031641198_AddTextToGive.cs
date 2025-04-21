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

    using Rock.SystemGuid;
    
    /// <summary>
    /// Adds configuration necessary for Text To Give functionality.
    /// </summary>
    public partial class AddTextToGive : Rock.Migrations.RockMigration
    {
        private const string TEXT_TO_GIVE_PAGE_GUID = "42CEEE52-ADEC-48BB-AF90-496DB2B272C7";
        private const string TEXT_TO_GIVE_PAGE_ROUTE_GUID = "A5DE71C3-5C98-40D1-BD27-32B298AC4577";
        private const string UTILITY_PAYMENT_ENTRY_BLOCKTYPE_GUID = "4CCC45A5-4AB9-4A36-BF8D-A6E316790004";
        private const string UTILITY_PAYMENT_ENTRY_BLOCKINSTANCE_GUID = "9684D991-8B26-4D39-BAD5-B520F91D27B8";
        private const string SHOW_CONFIRMATION_BLOCKATTRIBUTE_GUID = "64C005B4-5DEE-49CD-A639-1732769F9B34";
        private const string SHOW_HEADINGS_BLOCKATTRIBUTE_GUID = "7CBFC16B-5F24-47DA-BC89-60E460B839CE";
        private const string ALLOW_ACCOUNTS_BLOCKATTRIBUTE_GUID = "8D8DADDA-ECC2-497D-840B-02FB246696D6";
        private const string ENABLE_SETUP_BLOCKATTRIBUTE_GUID = "7213DAF7-909F-4A1D-9F4B-74F2A8757ACF";
        private const string FINISH_TEMPLATE_BLOCKATTRIBUTE_GUID = "6BEE06A9-969E-4704-9DC7-6B881D7280E3";
        private const string ADVANCED_BLOCK_ATTRIBUTE_CATEGORY_GUID = "171E45E4-74EC-4962-9AEA-56D899217AFB";
        private const string TEXT_OPTIONS_ATTRIBUTE_CATEGORY_GUID = "2569E660-39CA-4F8F-9C38-330E0896F387";

        private const string DEFAULT_FINISH_LAVA_TEMPLATE = @"
{% if Transaction.ScheduledTransactionDetails %}
    {% assign transactionDetails = Transaction.ScheduledTransactionDetails %}
{% else %}
    {% assign transactionDetails = Transaction.TransactionDetails %}
{% endif %}

<h1>Thank You!</h1>

<p>Your support is helping {{ 'Global' | Attribute:'OrganizationName' }} actively achieve our
mission. We are so grateful for your commitment.</p>

<dl>
    <dt>Confirmation Code</dt>
    <dd>{{ Transaction.TransactionCode }}</dd>
    <dd></dd>

    <dt>Name</dt>
    <dd>{{ Person.FullName }}</dd>
    <dd></dd>
    <dd>{{ Person.Email }}</dd>
    <dd>{{ BillingLocation.Street }} {{ BillingLocation.City }}, {{ BillingLocation.State }} {{ BillingLocation.PostalCode }}</dd>
</dl>

<dl class='dl-horizontal'>
    {% for transactionDetail in transactionDetails %}
        <dt>{{ transactionDetail.Account.PublicName }}</dt>
        <dd>{{ transactionDetail.Amount | Minus: transactionDetail.FeeCoverageAmount | FormatAsCurrency }}</dd>
    {% endfor %}
    {% if Transaction.TotalFeeCoverageAmount %}
        <dt>Fee Coverage</dt>
        <dd>{{ Transaction.TotalFeeCoverageAmount | FormatAsCurrency }}</dd>
    {% endif %}
    <dd></dd>

    <dt>Payment Method</dt>
    <dd>{{ PaymentDetail.CurrencyTypeValue.Description}}</dd>

    {% if PaymentDetail.AccountNumberMasked  != '' %}
        <dt>Account Number</dt>
        <dd>{{ PaymentDetail.AccountNumberMasked }}</dd>
    {% endif %}

    <dt>When<dt>
    <dd>

    {% if Transaction.TransactionFrequencyValue %}
        {{ Transaction.TransactionFrequencyValue.Value }} starting on {{ Transaction.NextPaymentDate | Date:'sd' }}
    {% else %}
        Today
    {% endif %}
    </dd>
</dl>
";
        private const string TEXTTOGIVESETUP_FINISH_LAVA_TEMPLATE = @"
<p>Thank you for your gift. Your next gift can be completed by texting the word ""give"" followed by the dollar amount (e.g. ""give $100"").</p>

<dl>
    <dt>Confirmation Code</dt>
    <dd>{{ Transaction.TransactionCode }}</dd>
    <dd></dd>

    <dt>Name</dt>
    <dd>{{ Person.FullName }}</dd>
    <dd></dd>
    <dd>{{ Person.Email }}</dd>
    <dd>{{ BillingLocation.Street }} {{ BillingLocation.City }}, {{ BillingLocation.State }} {{ BillingLocation.PostalCode }}</dd>
</dl>

<dl class='dl-horizontal'>
    {% for transactionDetail in transactionDetails %}
        <dt>{{ transactionDetail.Account.PublicName }}</dt>
        <dd>{{ transactionDetail.Amount | Minus: transactionDetail.FeeCoverageAmount | FormatAsCurrency }}</dd>
    {% endfor %}
    {% if Transaction.TotalFeeCoverageAmount %}
        <dt>Fee Coverage</dt>
        <dd>{{ Transaction.TotalFeeCoverageAmount | FormatAsCurrency }}</dd>
    {% endif %}
    <dd></dd>

    <dt>Payment Method</dt>
    <dd>{{ PaymentDetail.CurrencyTypeValue.Description}}</dd>

    {% if PaymentDetail.AccountNumberMasked  != '' %}
        <dt>Account Number</dt>
        <dd>{{ PaymentDetail.AccountNumberMasked }}</dd>
    {% endif %}

    <dt>When<dt>
    <dd>

    {% if Transaction.TransactionFrequencyValue %}
        {{ Transaction.TransactionFrequencyValue.Value }} starting on {{ Transaction.NextPaymentDate | Date:'sd' }}
    {% else %}
        Today
    {% endif %}
    </dd>
</dl>
";


        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Create Page.
            RockMigrationHelper.AddPage(
                Page.GIVE,
                Layout.LEFT_SIDEBAR,
                "Text To Give Setup",
                string.Empty,
                TEXT_TO_GIVE_PAGE_GUID );
#pragma warning disable CS0618 // Type or member is obsolete
            // Create PageRoute.
            RockMigrationHelper.AddPageRoute( TEXT_TO_GIVE_PAGE_GUID, "give/setup", TEXT_TO_GIVE_PAGE_ROUTE_GUID );
#pragma warning restore CS0618 // Type or member is obsolete

            // Set "Utility Payment Entry" BlockType Guid to known value.
            RockMigrationHelper.UpdateBlockType(
                "Utility Payment Entry",
                "Creates a new financial transaction or scheduled transaction.",
                "~/Blocks/Finance/UtilityPaymentEntry.ascx",
                "Finance",
                UTILITY_PAYMENT_ENTRY_BLOCKTYPE_GUID );

            // Add Block to Page.
            RockMigrationHelper.AddBlock(
                TEXT_TO_GIVE_PAGE_GUID,
                string.Empty,
                UTILITY_PAYMENT_ENTRY_BLOCKTYPE_GUID,
                "Utility Payment Entry",
                "Main",
                string.Empty,
                string.Empty,
                0,
                UTILITY_PAYMENT_ENTRY_BLOCKINSTANCE_GUID );


            // Set "Show Confirmation Page" Attribute Guid to a known value.
            RockMigrationHelper.AddBlockTypeAttribute(
                UTILITY_PAYMENT_ENTRY_BLOCKTYPE_GUID,
                
                FieldType.BOOLEAN,
                "Show Confirmation Page",
                "ShowConfirmationPage",
                string.Empty,
                "Show a confirmation page before processing the transaction.",
                7,
                "True",
                SHOW_CONFIRMATION_BLOCKATTRIBUTE_GUID );

            // Set correct attribute category for "Show Confirmation Page" Attribute.
            Sql( $@"
                DECLARE @AttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{SHOW_CONFIRMATION_BLOCKATTRIBUTE_GUID}');
                DECLARE @CategoryId INT = (SELECT [Id] FROM [Category] WHERE [Guid] = '{TEXT_OPTIONS_ATTRIBUTE_CATEGORY_GUID}');
                INSERT INTO [AttributeCategory] (AttributeId, CategoryId) VALUES (@AttributeId, @CategoryId);" );

            // Set "Show Confirmation Page" Block Attribute Value.
            RockMigrationHelper.AddBlockAttributeValue(
                UTILITY_PAYMENT_ENTRY_BLOCKINSTANCE_GUID,
                SHOW_CONFIRMATION_BLOCKATTRIBUTE_GUID,
                "False" );


            // Set "Allow Account Options In URL" Attribute Guid to a known value.
            RockMigrationHelper.AddBlockTypeAttribute(
                UTILITY_PAYMENT_ENTRY_BLOCKTYPE_GUID,
                FieldType.BOOLEAN,
                "Allow Account Options In URL",
                "AllowAccountOptionsInURL",
                string.Empty,
                "Set to true to allow account options to be set via URL. To simply set allowed accounts, the allowed accounts can be specified as a comma-delimited list of AccountIds or AccountGlCodes. Example: ?AccountIds=1,2,3 or ?AccountGlCodes=40100,40110. The default amount for each account and whether it is editable can also be specified. Example:?AccountIds=1^50.00^false,2^25.50^false,3^35.00^true or ?AccountGlCodes=40100^50.00^false,40110^42.25^true",
                1,
                "False",
                ALLOW_ACCOUNTS_BLOCKATTRIBUTE_GUID );

            // Set correct attribute category for "Show Panel Heading" Attribute.
            Sql( $@"
                DECLARE @AttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{ALLOW_ACCOUNTS_BLOCKATTRIBUTE_GUID}');
                DECLARE @CategoryId INT = (SELECT [Id] FROM [Category] WHERE [Guid] = '{ADVANCED_BLOCK_ATTRIBUTE_CATEGORY_GUID}');
                INSERT INTO [AttributeCategory] (AttributeId, CategoryId) VALUES (@AttributeId, @CategoryId);" );

            // Set "Allow Account Options In URL" Block Attribute Value.
            RockMigrationHelper.AddBlockAttributeValue(
                UTILITY_PAYMENT_ENTRY_BLOCKINSTANCE_GUID,
                ALLOW_ACCOUNTS_BLOCKATTRIBUTE_GUID,
                "True" );


            // Set "Show Panel Headings" Attribute Guid to a known value.
            RockMigrationHelper.AddBlockTypeAttribute(
                UTILITY_PAYMENT_ENTRY_BLOCKTYPE_GUID,
                FieldType.BOOLEAN,
                "Show Panel Headings",
                "ShowPanelHeadings",
                string.Empty,
                "Show the text headings at the top of the block and in panel sections.",
                11,
                "True",
                SHOW_HEADINGS_BLOCKATTRIBUTE_GUID );

            // Set correct attribute category for "Show Panel Headings" Attribute.
            Sql( $@"
                DECLARE @AttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{SHOW_HEADINGS_BLOCKATTRIBUTE_GUID}');
                DECLARE @CategoryId INT = (SELECT [Id] FROM [Category] WHERE [Guid] = '{ADVANCED_BLOCK_ATTRIBUTE_CATEGORY_GUID}');
                INSERT INTO [AttributeCategory] (AttributeId, CategoryId) VALUES (@AttributeId, @CategoryId);" );

            // Set "Show Panel Headings" Block Attribute Value.
            RockMigrationHelper.AddBlockAttributeValue(
                UTILITY_PAYMENT_ENTRY_BLOCKINSTANCE_GUID,
                SHOW_HEADINGS_BLOCKATTRIBUTE_GUID,
                "False" );


            // Set "Enable Text To Give Setup" Attribute Guid to a known value.
            RockMigrationHelper.AddBlockTypeAttribute(
                UTILITY_PAYMENT_ENTRY_BLOCKTYPE_GUID,
                FieldType.BOOLEAN,
                "Enable Text To Give Setup",
                "EnableTextToGiveSetup",
                string.Empty,
                "This setting enables specific behavior for setting up Text To Give accounts.",
                12,
                "False",
                ENABLE_SETUP_BLOCKATTRIBUTE_GUID );

            // Set correct attribute category for "Enable Text To Give Setup" Attribute.
            Sql( $@"
                DECLARE @AttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{ENABLE_SETUP_BLOCKATTRIBUTE_GUID}');
                DECLARE @CategoryId INT = (SELECT [Id] FROM [Category] WHERE [Guid] = '{ADVANCED_BLOCK_ATTRIBUTE_CATEGORY_GUID}');
                INSERT INTO [AttributeCategory] (AttributeId, CategoryId) VALUES (@AttributeId, @CategoryId);" );

            // Set "Enable Text To Give Setup" Block Attribute Value.
            RockMigrationHelper.AddBlockAttributeValue(
                UTILITY_PAYMENT_ENTRY_BLOCKINSTANCE_GUID,
                ENABLE_SETUP_BLOCKATTRIBUTE_GUID,
                "True" );


            // Set "Finish Lava Template" Attribute Guid to a known value.
            RockMigrationHelper.AddBlockTypeAttribute(
                UTILITY_PAYMENT_ENTRY_BLOCKTYPE_GUID,
                FieldType.CODE_EDITOR,
                "Finish Lava Template",
                "FinishLavaTemplate",
                string.Empty,
                "The text (HTML) to display on the success page. <span class='tip tip-lava'></span>",
                11,
                DEFAULT_FINISH_LAVA_TEMPLATE,
                FINISH_TEMPLATE_BLOCKATTRIBUTE_GUID );

            // Set correct attribute category for "Finish Lava Template" Attribute.
            Sql( $@"
                DECLARE @AttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{FINISH_TEMPLATE_BLOCKATTRIBUTE_GUID}');
                DECLARE @CategoryId INT = (SELECT [Id] FROM [Category] WHERE [Guid] = '{TEXT_OPTIONS_ATTRIBUTE_CATEGORY_GUID}');
                INSERT INTO [AttributeCategory] (AttributeId, CategoryId) VALUES (@AttributeId, @CategoryId);" );

            // Set "Finish Lava Template" Block Attribute Value.
            RockMigrationHelper.AddBlockAttributeValue(
                UTILITY_PAYMENT_ENTRY_BLOCKINSTANCE_GUID,
                FINISH_TEMPLATE_BLOCKATTRIBUTE_GUID,
                TEXTTOGIVESETUP_FINISH_LAVA_TEMPLATE );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
