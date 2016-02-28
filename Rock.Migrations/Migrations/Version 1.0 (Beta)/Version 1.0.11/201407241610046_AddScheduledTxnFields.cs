// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class AddScheduledTxnFields : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.FinancialScheduledTransaction", "CurrencyTypeValueId", c => c.Int() );
            AddColumn( "dbo.FinancialScheduledTransaction", "CreditCardTypeValueId", c => c.Int() );
            CreateIndex( "dbo.FinancialScheduledTransaction", "CurrencyTypeValueId" );
            CreateIndex( "dbo.FinancialScheduledTransaction", "CreditCardTypeValueId" );
            AddForeignKey( "dbo.FinancialScheduledTransaction", "CreditCardTypeValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.FinancialScheduledTransaction", "CurrencyTypeValueId", "dbo.DefinedValue", "Id" );

            RockMigrationHelper.AddDefinedTypeAttribute( "2BD4FFB0-6C7F-4890-8D08-00F0BB7B43E9", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Batch Name Suffix", "BatchNameSuffix",
                "When a financial transaction is processed by Rock, it will add the transaction to a new or existing batch with a specific name. This name can have a suffix defined specific to the type of credit card being used. This provides control over how specific credit card transactions are grouped into batches.",
                1, "", "B4A01204-FD66-48B3-8F08-135D36CC7EE5" );
            Sql( "UPDATE [Attribute] SET [IsGridColumn] = 1 WHERE [Guid] = 'B4A01204-FD66-48B3-8F08-135D36CC7EE5'" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FC66B5F8-634F-4800-A60D-436964D27B64", "B4A01204-FD66-48B3-8F08-135D36CC7EE5", "VMD" ); // Visa
            RockMigrationHelper.AddDefinedValueAttributeValue( "6373A4B6-4DCA-4EB6-9ADE-B30E8A7F8621", "B4A01204-FD66-48B3-8F08-135D36CC7EE5", "VMD" ); // Mastercard
            RockMigrationHelper.AddDefinedValueAttributeValue( "4B746601-E9EB-4660-BA13-C0B66B24E248", "B4A01204-FD66-48B3-8F08-135D36CC7EE5", "VMD" ); // Discover

            // Add Transaction, Batch Name Prefix Attribute
            RockMigrationHelper.AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Batch Name Prefix", "BatchNamePrefix", "", "The batch prefix name to use when creating a new batch", 2, "Online Giving", "245BDD4E-E8FF-4039-8C0B-C7AC1C185D1D" );
            RockMigrationHelper.AddBlockAttributeValue( "8ADB1C1F-299B-461A-8469-0FF4E2C98216", "245BDD4E-E8FF-4039-8C0B-C7AC1C185D1D", @"Online Giving" );

            RockMigrationHelper.AddPage( "F23C5BF7-4F52-448F-8445-6BAEEE3030AB", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Contribution Detail", "", "F1C3BBD3-EE91-4DDD-8880-1542EBCD8041", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "18C9E5C3-3E28-4AA3-84F6-78CD4EA2DD3C", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Download Payments", "", "720819FC-1730-444A-9DE8-C98D29954170", "fa fa-download" ); // Site:Rock RMS

            RockMigrationHelper.UpdateBlockType( "Scheduled Payment Download", "Block used to download any scheduled payment transactions that were processed by payment gateway during a specified date range.", "~/Blocks/Finance/ScheduledPaymentDownload.ascx", "Finance", "71FF09C3-3E50-4E97-9329-3CD57AACCA53" );

            // Add Block to Page: Contribution Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "F1C3BBD3-EE91-4DDD-8880-1542EBCD8041", "", "5171C4E5-7698-453E-9CC8-088D362296DE", "Giving Profile Detail", "Main", "", "", 0, "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515" );
            // Add Block to Page: Download, Site: Rock RMS
            RockMigrationHelper.AddBlock( "720819FC-1730-444A-9DE8-C98D29954170", "", "71FF09C3-3E50-4E97-9329-3CD57AACCA53", "Scheduled Payment Download", "Main", "", "", 0, "A55A9614-9D89-4D56-A022-D15BD6472C62" );

            // Attrib for BlockType: Scheduled Payment Download:Batch Name Prefix
            RockMigrationHelper.AddBlockTypeAttribute( "71FF09C3-3E50-4E97-9329-3CD57AACCA53", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Batch Name Prefix", "BatchNamePrefix", "", "The batch prefix name to use when creating a new batch", 2, @"Online Giving", "6BF67B6D-D089-47BD-A796-24E1E248F4EE" );

            // Attrib Value for Block:Giving Profile Detail, Attribute:Success Header Page: Contribution Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "7F14DD3C-C761-4AE1-A9E5-021EBF794547", @"
<p>
Thank you for your generous contribution.  Your support is helping {{ OrganizationName }} actively 
achieve our mission.  We are so grateful for your commitment. 
</p>
" );
            // Attrib Value for Block:Giving Profile Detail, Attribute:Add Account Text Page: Contribution Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "77113EE3-27B0-4BE5-9218-7F7EB7DDD193", @"Add Another Account" );
            // Attrib Value for Block:Giving Profile Detail, Attribute:Impersonation Page: Contribution Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "11649364-1508-484A-AA1E-751E5C4F9CD6", @"True" );
            // Attrib Value for Block:Giving Profile Detail, Attribute:Confirmation Footer Page: Contribution Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "E96D2D98-BB52-45E5-88D2-FECC345636D4", @"
<div class='alert alert-info'>
By clicking the 'finish' button below I agree to allow {{ OrganizationName }} to debit the amount above from my account. I acknowledge that I may 
update the transaction information at any time by returning to this website. Please call the Finance Office if you have any additional questions. 
</div>
" );
            // Attrib Value for Block:Giving Profile Detail, Attribute:Success Footer Page: Contribution Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "8F9A8359-2EAF-4390-BFD3-E8361E069F08", @"
" );
            // Attrib Value for Block:Giving Profile Detail, Attribute:Confirmation Header Page: Contribution Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "42F035FE-A1E8-4856-86A6-BA009BC6F33B", @"
<p>
Please confirm the information below. Once you have confirmed that the information is accurate click the 'Finish' button to complete your transaction. 
</p>
" );
            // Attrib Value for Block:Giving Profile Detail, Attribute:Credit Card Gateway Page: Contribution Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "8177CE07-AA1B-43F4-ABCE-23E63DA8EBC8", @"D4A40C4A-336F-49A6-9F44-88F149726126" );
            // Attrib Value for Block:Giving Profile Detail, Attribute:ACH Gateway Page: Contribution Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "FC9DF232-D7B1-4CA9-B348-D139276783BB", @"D4A40C4A-336F-49A6-9F44-88F149726126" );
            // Attrib Value for Block:Giving Profile Detail, Attribute:Accounts Page: Contribution Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "32B27FF4-0EDC-4709-B714-41084F8FE99C", @"4410306f-3fb5-4a57-9a80-09a3f9d40d0c,67c6181c-1d8c-44d7-b262-b81e746f06d8" );
            // Attrib Value for Block:Giving Profile Detail, Attribute:Additional Accounts Page: Contribution Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "DA811408-9985-4D1B-B92C-8C8FCA026B3D", @"True" );

            RockMigrationHelper.AddBlockAttributeValue( "32A7BA7B-968E-4BFD-BEA3-042CF863D751", "78622434-4B4E-42E4-B044-21AEDD315186", "f1c3bbd3-ee91-4ddd-8880-1542ebcd8041" );

            RockMigrationHelper.UpdateSystemEmail_pre201409101843015( "Finance", "Pledge Confirmation", "", "", "", "", "", "Thank you for your commitment", @"
{{ GlobalAttribute.EmailHeader }}

{{ Person.FirstName }},<br/><br/>

Thank you for your commitment to our {{ Account.Name }} account. Your financial gifts help us to continue to reach our mission.

Commitment Amount: ${{ FinancialPledge.TotalAmount }}

If you have any questions, please contact {{ GlobalAttribute.OrganizationEmail }}

Thank-you,<br/>
{{ GlobalAttribute.OrganizationName }}  

{{ GlobalAttribute.EmailFooter }}
", "73E8D035-61BB-495A-A87F-39007B98834C" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteSystemEmail( "73E8D035-61BB-495A-A87F-39007B98834C" );

            RockMigrationHelper.AddBlockAttributeValue( "32A7BA7B-968E-4BFD-BEA3-042CF863D751", "78622434-4B4E-42E4-B044-21AEDD315186", "d360b64f-1267-4518-95cd-99cd5ab87d88" );

            RockMigrationHelper.DeleteBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "7F14DD3C-C761-4AE1-A9E5-021EBF794547" );
            // Attrib Value for Block:Giving Profile Detail, Attribute:Add Account Text Page: Contribution Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "77113EE3-27B0-4BE5-9218-7F7EB7DDD193" );
            // Attrib Value for Block:Giving Profile Detail, Attribute:Impersonation Page: Contribution Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "11649364-1508-484A-AA1E-751E5C4F9CD6" );
            // Attrib Value for Block:Giving Profile Detail, Attribute:Confirmation Footer Page: Contribution Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "E96D2D98-BB52-45E5-88D2-FECC345636D4" );
            // Attrib Value for Block:Giving Profile Detail, Attribute:Success Footer Page: Contribution Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "8F9A8359-2EAF-4390-BFD3-E8361E069F08" );
            // Attrib Value for Block:Giving Profile Detail, Attribute:Confirmation Header Page: Contribution Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "42F035FE-A1E8-4856-86A6-BA009BC6F33B" );
            // Attrib Value for Block:Giving Profile Detail, Attribute:Credit Card Gateway Page: Contribution Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "8177CE07-AA1B-43F4-ABCE-23E63DA8EBC8" );
            // Attrib Value for Block:Giving Profile Detail, Attribute:ACH Gateway Page: Contribution Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "FC9DF232-D7B1-4CA9-B348-D139276783BB" );
            // Attrib Value for Block:Giving Profile Detail, Attribute:Accounts Page: Contribution Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "32B27FF4-0EDC-4709-B714-41084F8FE99C" );
            // Attrib Value for Block:Giving Profile Detail, Attribute:Additional Accounts Page: Contribution Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "DA811408-9985-4D1B-B92C-8C8FCA026B3D" );


            // Attrib for BlockType: Scheduled Payment Download:Batch Name Prefix
            RockMigrationHelper.DeleteAttribute( "6BF67B6D-D089-47BD-A796-24E1E248F4EE" );
            // Remove Block: Scheduled Payment Download, from Page: Download, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "A55A9614-9D89-4D56-A022-D15BD6472C62" );
            // Remove Block: Giving Profile Detail, from Page: Contribution Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515" );

            RockMigrationHelper.DeleteBlockType( "71FF09C3-3E50-4E97-9329-3CD57AACCA53" ); // Scheduled Payment Download

            RockMigrationHelper.DeletePage( "720819FC-1730-444A-9DE8-C98D29954170" ); //  Page: Download, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "F1C3BBD3-EE91-4DDD-8880-1542EBCD8041" ); //  Page: Contribution Detail, Layout: Full Width, Site: Rock RMS

            RockMigrationHelper.DeleteAttribute( "B4A01204-FD66-48B3-8F08-135D36CC7EE5" );

            DropForeignKey( "dbo.FinancialScheduledTransaction", "CurrencyTypeValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.FinancialScheduledTransaction", "CreditCardTypeValueId", "dbo.DefinedValue" );
            DropIndex( "dbo.FinancialScheduledTransaction", new[] { "CreditCardTypeValueId" } );
            DropIndex( "dbo.FinancialScheduledTransaction", new[] { "CurrencyTypeValueId" } );
            DropColumn( "dbo.FinancialScheduledTransaction", "CreditCardTypeValueId" );
            DropColumn( "dbo.FinancialScheduledTransaction", "CurrencyTypeValueId" );
        }
    }
}
