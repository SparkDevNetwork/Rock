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
    public partial class PaymentGateway : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( "DELETE FinancialPersonSavedAccount" );

            DropForeignKey( "dbo.FinancialGateway", "EntityTypeId", "dbo.EntityType" );
            DropForeignKey( "dbo.FinancialTransaction", "GatewayId", "dbo.FinancialGateway" );
            DropForeignKey( "dbo.FinancialPersonSavedAccount", "GatewayId", "dbo.FinancialGateway" );
            DropForeignKey( "dbo.FinancialScheduledTransaction", "GatewayId", "dbo.FinancialGateway" );
            DropIndex( "dbo.FinancialGateway", new[] { "EntityTypeId" } );
            DropIndex( "dbo.FinancialTransaction", new[] { "GatewayId" } );
            DropIndex( "dbo.FinancialPersonSavedAccount", new[] { "GatewayId" } );
            DropIndex( "dbo.FinancialScheduledTransaction", new[] { "GatewayId" } );
            AddColumn( "dbo.FinancialBatch", "BatchStartDateTime", c => c.DateTime() );
            AddColumn( "dbo.FinancialBatch", "BatchEndDateTime", c => c.DateTime() );
            AddColumn( "dbo.FinancialTransaction", "GatewayEntityTypeId", c => c.Int() );
            AddColumn( "dbo.FinancialTransaction", "ScheduledTransactionId", c => c.Int() );
            AddColumn( "dbo.FinancialPersonSavedAccount", "FinancialTransactionId", c => c.Int( nullable: false ) );
            AddColumn( "dbo.FinancialScheduledTransaction", "NextPaymentDate", c => c.DateTime( storeType: "date" ) );
            AddColumn( "dbo.FinancialScheduledTransaction", "LastStatusUpdateDateTime", c => c.DateTime( storeType: "date" ) );
            AddColumn( "dbo.FinancialScheduledTransaction", "GatewayEntityTypeId", c => c.Int() );
            AddColumn( "dbo.FinancialScheduledTransaction", "GatewayScheduleId", c => c.String() );
            CreateIndex( "dbo.FinancialTransaction", "GatewayEntityTypeId" );
            CreateIndex( "dbo.FinancialScheduledTransaction", "GatewayEntityTypeId" );
            CreateIndex( "dbo.FinancialTransaction", "ScheduledTransactionId" );
            CreateIndex( "dbo.FinancialPersonSavedAccount", "FinancialTransactionId" );
            AddForeignKey( "dbo.FinancialTransaction", "GatewayEntityTypeId", "dbo.EntityType", "Id" );
            AddForeignKey( "dbo.FinancialScheduledTransaction", "GatewayEntityTypeId", "dbo.EntityType", "Id" );
            AddForeignKey( "dbo.FinancialTransaction", "ScheduledTransactionId", "dbo.FinancialScheduledTransaction", "Id" );
            AddForeignKey( "dbo.FinancialPersonSavedAccount", "FinancialTransactionId", "dbo.FinancialTransaction", "Id", cascadeDelete: true );
            DropColumn( "dbo.FinancialBatch", "BatchDate" );
            DropColumn( "dbo.FinancialTransaction", "GatewayId" );
            DropColumn( "dbo.FinancialPersonSavedAccount", "GatewayId" );
            DropColumn( "dbo.FinancialPersonSavedAccount", "PaymentMethod" );
            DropColumn( "dbo.FinancialPersonSavedAccount", "TransactionCode" );
            DropColumn( "dbo.FinancialScheduledTransaction", "GatewayId" );
            DropTable( "dbo.FinancialGateway" );

            // Delete Payment Type defined type (not needed)
            DeleteDefinedType( "23E80D98-017E-47B9-BAF3-AC442A1EC3EE" );

            // Combine the pledge and transaction frequency types into one
            Sql( @"
    DECLARE @FrequencyValueTypeId int
    SET @FrequencyValueTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '1F645CFB-5BBD-4465-B9CA-0D2104A1479B')

    UPDATE P SET [PledgeFrequencyValueId] = NV.[Id]
    FROM [FinancialPledge] P
    INNER JOIN [DefinedValue] OV ON OV.[Id] = P.[PledgeFrequencyValueId]
    INNER JOIN [DefinedValue] NV ON NV.[DefinedTypeId] = @FrequencyValueTypeId AND NV.[Name] = OV.[Name]
" );
            DeleteDefinedType( "9E358FBE-2321-4C54-895F-C888E29298AE" );    // Batch Type
            DeleteDefinedType( "059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F" );    // Frequency Type
            DeleteDefinedValue( "A5A12067-322E-44A4-94C4-561312F9913C" );   // One-Time - Future

            AddDefinedValue( "1F645CFB-5BBD-4465-B9CA-0D2104A1479B", "Quarterly", "Every Quarter", "BF08EA03-C52A-4364-B142-12EBCA7CA14A" );
            AddDefinedValue( "1F645CFB-5BBD-4465-B9CA-0D2104A1479B", "Twice a Year", "Every Six Months", "691BB8AB-5F96-4E88-847C-CB970D9E87FA" );
            AddDefinedValue( "1F645CFB-5BBD-4465-B9CA-0D2104A1479B", "Yearly", "Every Year", "AC88C37A-901E-4CBB-947B-11348C208192" );

            AddDefinedValue( "1D1304DE-E83A-44AF-B11D-0C66DD600B81", "Credit Card", "Credit Card", "928A2E04-C77B-4282-888F-EC549CEE026A" );
            AddDefinedValue( "1D1304DE-E83A-44AF-B11D-0C66DD600B81", "ACH", "Bank Account (ACH)", "DABEE8FD-AEDF-43E1-8547-4C97FA14D9B6" );

            // Financial Gateways Page
            AddPage( "C831428A-6ACD-4D49-9B2D-046D399E3123", "Financial Gateways", "", "Default", "6F8EC649-FDED-4805-B7AF-42A6901C197F", "icon-credit-card" );
            AddBlock( "6F8EC649-FDED-4805-B7AF-42A6901C197F", "21F5F466-59BC-40B2-8D73-7314D936C3CB", "Gateways", "", "Content", 0, "8C707818-ECB1-4E40-8F2C-6E9802E6BA73" );
            AddBlockAttributeValue( "8C707818-ECB1-4E40-8F2C-6E9802E6BA73", "259AF14D-0214-4BE4-A7BF-40423EA07C99", "Rock.Financial.GatewayContainer, Rock" );

            UpdateFieldType( "Time Field Type", "", "Rock", "Rock.Field.Types.TimeFieldType", "2F8F5EC4-57FA-4F6C-AB15-9D6616994580" );

            UpdateEntityType( "Rock.PayFlowPro.Gateway", "Gateway", "Rock.PayFlowPro.Gateway, Rock.PayFlowPro, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", false, false, "d4a40c4a-336f-49a6-9f44-88f149726126" );
            AddEntityAttribute( "Rock.PayFlowPro.Gateway", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "", "Should Service be used?", 0, "False", "ABBABC4C-1C9D-4B75-9A8F-B1C86205F3F1" );
            AddEntityAttribute( "Rock.PayFlowPro.Gateway", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "", "", "Order", "", "The order that this service should be used (priority)", 0, "", "4F8C4047-DFB2-46FE-86B6-2002E3DDF6DE" );
            AddEntityAttribute( "Rock.PayFlowPro.Gateway", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "", "", "Mode", "", "Mode to use for transactions", 1, "Live", "EEF31B3A-92EF-4E2B-81BB-184741291DEC" );
            AddEntityAttribute( "Rock.PayFlowPro.Gateway", "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "PayPal Partner", "", "", 2, "", "5EB5E3F5-AD7C-4B3B-B779-FB3FA37D98D4" );
            AddEntityAttribute( "Rock.PayFlowPro.Gateway", "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "PayPal Merchant Login", "", "", 3, "", "ECB1048F-6930-4B53-86D9-611E49469BFA" );
            AddEntityAttribute( "Rock.PayFlowPro.Gateway", "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "PayPal User", "", "", 4, "", "298E7FA1-45C8-46A0-9266-CA75465B66FD" );
            AddEntityAttribute( "Rock.PayFlowPro.Gateway", "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "PayPal Password", "", "", 5, "", "9F792F64-A4C0-4FE3-AE4E-8F869503101B" );
            AddEntityAttribute( "Rock.PayFlowPro.Gateway", "2F8F5EC4-57FA-4F6C-AB15-9D6616994580", "", "", "Batch Process Time", "", "The Paypal Batch processing cut-off time.  When batches are created by Rock, they will use this for the start/stop when creating new batches", 6, "00:00:00", "5DF40338-3DA1-4959-AF81-B2B3A32C9C9D" );
            AddAttributeValue("ABBABC4C-1C9D-4B75-9A8F-B1C86205F3F1", 0, "True", "BD8D3D0A-A535-4CBE-9342-8027C5665CCB" );

            // Add Transaction Page
            DeleteBlock( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96" );
            AddBlockType( "Finance - Add Transaction", "", "~/Blocks/Finance/AddTransaction.ascx", "74EE3481-3E5A-4971-A02E-D463ABB45591" );
            AddBlock( "1615E090-1889-42FF-AB18-5F7BE9F24498", "74EE3481-3E5A-4971-A02E-D463ABB45591", "Contributions", "", "Main", 0, "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96" );

            // Attrib for BlockType: Finance - Add Transaction:Credit Card Gateway
            AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "A7486B0E-4CA2-4E00-A987-5544C7DABA76", "Credit Card Gateway", "CCGateway", "", "The payment gateway to use for Credit Card transactions", 0, "", "3D478949-1F85-4E81-A403-22BBA96B8F69" );
            // Attrib for BlockType: Finance - Add Transaction:ACH Card Gateway
            AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "A7486B0E-4CA2-4E00-A987-5544C7DABA76", "ACH Card Gateway", "ACHGateway", "", "The payment gateway to use for ACH (bank account) transactions", 1, "", "D6429E78-E8F0-4EF2-9D18-DFDDE4ECC6A7" );
            // Attrib for BlockType: Finance - Add Transaction:Batch Name Prefix
            AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Batch Name Prefix", "BatchNamePrefix", "", "The batch prefix name to use when creating a new batch", 2, "Online Giving - ", "245BDD4E-E8FF-4039-8C0B-C7AC1C185D1D" );
            // Attrib for BlockType: Finance - Add Transaction:Source
            AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Source", "Source", "", "The Financial Source Type to use when creating transactions", 3, "", "5C54E6E7-1C21-4959-98EA-FB1C2D0A0D61" );
            // Attrib for BlockType: Finance - Add Transaction:Address Type
            AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Address Type", "AddressType", "", "The location type to use for the person's address", 4, "8C52E53C-2A66-435A-AE6E-5EE307D9A0DC", "DBF313AB-0488-4BF7-A11D-1998D7A3476D" );
            // Attrib for BlockType: Finance - Add Transaction:Layout Style
            AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Layout Style", "LayoutStyle", "", "How the sections of this page should be displayed", 5, "Vertical", "23B31F2F-9366-446D-9D3E-4CA68A6876D1" );
            // Attrib for BlockType: Finance - Add Transaction:Accounts
            AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "Accounts", "", "The accounts to display.  By default all active accounts with a Public Name will be displayed", 6, "", "DAB27F0A-D0C0-4275-93F4-DEF227F6B1A2" );
            // Attrib for BlockType: Finance - Add Transaction:Allow Other Accounts
            AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Other Accounts", "AllowOtherAccounts", "", "Should users be allowed to select additional accounts?  If so, any active account with a Public Name value will be available", 7, "True", "D63727D8-BE34-4935-A0C4-31CBE1BD0982" );
            // Attrib for BlockType: Finance - Add Transaction:Add Account Text
            AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Add Account Text", "AddAccountText", "", "The button text to display for adding an additional account", 8, "Add Another Account", "1133170C-8E4C-4020-B795-F799F893D70D" );
            // Attrib for BlockType: Finance - Add Transaction:Allow Scheduled Transactions
            AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Scheduled Transactions", "AllowScheduled", "", "If the selected gateway(s) allow scheduled transactions, should that option be provided to user", 9, "True", "63CA1F26-6942-48F4-9A15-F0A2D40E3FAB" );
            // Attrib for BlockType: Finance - Add Transaction:Allow Impersonation
            AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Impersonation", "AllowImpersonation", "", "Should the current user be able to view and edit other people's transactions?  IMPORTANT: This should only be enabled on an internal page that is secured to trusted users", 10, "False", "2C9BF7FD-26F6-4122-B8DB-E6FEE8BF607D" );
            // Attrib for BlockType: Finance - Add Transaction:Prompt for Phone
            AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Prompt for Phone", "DisplayPhone", "", "Should the user be prompted for their phone number?", 11, "False", "8A572D6B-5CC1-4357-BFD5-8D887433A0AB" );
            // Attrib for BlockType: Finance - Add Transaction:Prompt for Email
            AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Prompt for Email", "DisplayEmail", "", "Should the user be prompted for their email address?", 12, "True", "8B67B723-1C71-44EF-81F0-F4225CE7039B" );
            // Attrib for BlockType: Finance - Add Transaction:Confirmation Header
            AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Confirmation Header", "ConfirmationHeader", "", "The text (HTML) to display at the top of the confirmation section?", 13, @"
<p>
Please confirm the information below. Once you have confirmed that the information is accurate click the ''Finish'' button to complete your transaction. 
</p>
", "FA6300AD-9268-47FD-BBBE-BB6A415B0002" );
            // Attrib for BlockType: Finance - Add Transaction:Confirmation Footer
            AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Confirmation Footer", "ConfirmationFooter", "", "The text (HTML) to display at the bottom of the confirmation section?", 14, @"
<div class=''alert alert-info''>
By clicking the ''finish'' button below I agree to allow {{ OrganizationName }} to debit the amount above from my account. I acknowledge that I may 
update the transaction information at any time by returning to this website. Please call the Finance Office if you have any additional questions. 
</div>
", "B1F9196D-B51D-4ECD-A7BE-89F34431D736" );
            // Attrib for BlockType: Finance - Add Transaction:Success Header
            AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Success Header", "SuccessHeader", "", "The text (HTML) to display at the top of the confirmation section?", 15, @"
<p>
Thank-you for your generous contribution.  Your support is helping {{ OrganizationName }} actively 
achieve our mission.  We are so grateful for your commitment. 
</p>
", "1597A542-E6EB-4E29-A435-E5C23785251E" );
            // Attrib for BlockType: Finance - Add Transaction:Success Footer
            AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Success Footer", "SuccessFooter", "", "The text (HTML) to display at the bottom of the confirmation section?", 16, @"
", "188C6D55-CC08-4019-AA5F-706251509696" );

            // Attrib Value for Block:Contributions, Attribute:Credit Card Gateway, Page:Give Now
            AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "3D478949-1F85-4E81-A403-22BBA96B8F69", "d4a40c4a-336f-49a6-9f44-88f149726126" );
            // Attrib Value for Block:Contributions, Attribute:ACH Card Gateway, Page:Give Now
            AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "D6429E78-E8F0-4EF2-9D18-DFDDE4ECC6A7", "d4a40c4a-336f-49a6-9f44-88f149726126" );
            // Attrib Value for Block:Contributions, Attribute:Accounts, Page:Give Now
            AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "DAB27F0A-D0C0-4275-93F4-DEF227F6B1A2", "4410306f-3fb5-4a57-9a80-09a3f9d40d0c,67c6181c-1d8c-44d7-b262-b81e746f06d8" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlockAttributeValue( "8C707818-ECB1-4E40-8F2C-6E9802E6BA73", "ABBABC4C-1C9D-4B75-9A8F-B1C86205F3F1" );

            DeleteAttribute("3D478949-1F85-4E81-A403-22BBA96B8F69");
            DeleteAttribute("D6429E78-E8F0-4EF2-9D18-DFDDE4ECC6A7");
            DeleteAttribute("245BDD4E-E8FF-4039-8C0B-C7AC1C185D1D");
            DeleteAttribute("5C54E6E7-1C21-4959-98EA-FB1C2D0A0D61");
            DeleteAttribute("DBF313AB-0488-4BF7-A11D-1998D7A3476D");
            DeleteAttribute("23B31F2F-9366-446D-9D3E-4CA68A6876D1");
            DeleteAttribute("DAB27F0A-D0C0-4275-93F4-DEF227F6B1A2");
            DeleteAttribute("D63727D8-BE34-4935-A0C4-31CBE1BD0982");
            DeleteAttribute("1133170C-8E4C-4020-B795-F799F893D70D");
            DeleteAttribute("63CA1F26-6942-48F4-9A15-F0A2D40E3FAB");
            DeleteAttribute("2C9BF7FD-26F6-4122-B8DB-E6FEE8BF607D");
            DeleteAttribute("8A572D6B-5CC1-4357-BFD5-8D887433A0AB");
            DeleteAttribute("8B67B723-1C71-44EF-81F0-F4225CE7039B");
            DeleteAttribute("FA6300AD-9268-47FD-BBBE-BB6A415B0002");
            DeleteAttribute("B1F9196D-B51D-4ECD-A7BE-89F34431D736");
            DeleteAttribute("1597A542-E6EB-4E29-A435-E5C23785251E");
            DeleteAttribute("188C6D55-CC08-4019-AA5F-706251509696");

            DeleteAttribute( "5EB5E3F5-AD7C-4B3B-B779-FB3FA37D98D4" );    // Rock.PayFlowPro.Gateway: PayPal Partner
            DeleteAttribute( "4F8C4047-DFB2-46FE-86B6-2002E3DDF6DE" );    // Rock.PayFlowPro.Gateway: Order
            DeleteAttribute( "ABBABC4C-1C9D-4B75-9A8F-B1C86205F3F1" );    // Rock.PayFlowPro.Gateway: Active
            DeleteAttribute( "ECB1048F-6930-4B53-86D9-611E49469BFA" );    // Rock.PayFlowPro.Gateway: PayPal Merchant Login
            DeleteAttribute( "298E7FA1-45C8-46A0-9266-CA75465B66FD" );    // Rock.PayFlowPro.Gateway: PayPal User
            DeleteAttribute( "9F792F64-A4C0-4FE3-AE4E-8F869503101B" );    // Rock.PayFlowPro.Gateway: PayPal Password
            DeleteAttribute( "EEF31B3A-92EF-4E2B-81BB-184741291DEC" );    // Rock.PayFlowPro.Gateway: Mode
            DeleteAttribute( "5DF40338-3DA1-4959-AF81-B2B3A32C9C9D" );    // Rock.PayFlowPro.Gateway: Batch Process Time

            // Remove Block: Gateways, from Page: Financial Gateways
            DeleteBlock("8C707818-ECB1-4E40-8F2C-6E9802E6BA73");

            // Remove Block: Contributions, from Page: Give Now
            DeleteBlock("20C12A0F-BEC1-4620-9273-EEFE4CFB1D96");
            AddBlock( "1615E090-1889-42FF-AB18-5F7BE9F24498", "B343E2B7-0AD0-49B8-B78E-E47BD42171A7", "Contributions", "", "Main", 0, "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96" );

            DeleteBlockType("74EE3481-3E5A-4971-A02E-D463ABB45591"); // Finance - Add Transaction

            DeletePage("6F8EC649-FDED-4805-B7AF-42A6901C197F"); // Financial Gateways

            DeleteDefinedValue( "DABEE8FD-AEDF-43E1-8547-4C97FA14D9B6" );
            DeleteDefinedValue( "928A2E04-C77B-4282-888F-EC549CEE026A" );

            DeleteDefinedValue( "AC88C37A-901E-4CBB-947B-11348C208192" );
            DeleteDefinedValue( "691BB8AB-5F96-4E88-847C-CB970D9E87FA" );
            DeleteDefinedValue( "BF08EA03-C52A-4364-B142-12EBCA7CA14A" );

            AddDefinedValue( "1F645CFB-5BBD-4465-B9CA-0D2104A1479B", "One-Time (Future)", "One Time Future", "A5A12067-322E-44A4-94C4-561312F9913C" );

            AddDefinedType( "Financial", "Frequency Type", "Types of payment frequencies", "059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F" );
            AddDefinedValue( "059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F", "Weekly", "Every Week", "53957842-DE28-498C-AC61-65B32E8034CB" );
            AddDefinedValue( "059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F", "Bi-Weekly", "Every Two Weeks", "FBD9315C-5E0B-49D8-9D28-27EBF268E67B" );
            AddDefinedValue( "059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F", "Monthly", "Once a Month", "C53509B1-FC2B-46C8-A00E-58392FBE9408" );
            AddDefinedValue( "059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F", "Twice a Month", "Twice a Month", "CA25B6D3-9BA4-4E88-9A5A-BF44B2898383" );

            Sql( @"
    DECLARE @FrequencyValueTypeId int
    SET @FrequencyValueTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F')

    UPDATE P SET [PledgeFrequencyValueId] = NV.[Id]
    FROM [FinancialPledge] P
    INNER JOIN [DefinedValue] OV ON OV.[Id] = P.[PledgeFrequencyValueId]
    INNER JOIN [DefinedValue] NV ON NV.[DefinedTypeId] = @FrequencyValueTypeId AND NV.[Name] = OV.[Name]
" );

            AddDefinedType( "Financial", "Payment Type", "The type of payment associated with a transaction", "23E80D98-017E-47B9-BAF3-AC442A1EC3EE" );
            AddDefinedValue( "23E80D98-017E-47B9-BAF3-AC442A1EC3EE", "Credit Card", "Credit Card payment type", "09412338-AAAA-4644-BA2A-4CADBE653468" );
            AddDefinedValue( "23E80D98-017E-47B9-BAF3-AC442A1EC3EE", "Checking/ACH", "Checking/ACH payment type", "FFAD975C-7504-418F-8959-30BD0C62CD30" );

            AddDefinedType( "Financial", "Batch Type", "Batch Types", "9E358FBE-2321-4C54-895F-C888E29298AE" );
            AddDefinedValue( "9E358FBE-2321-4C54-895F-C888E29298AE", "ACH", "ACH", "E6F877F3-D2CC-443E-976A-4402502F544F" );
            AddDefinedValue( "9E358FBE-2321-4C54-895F-C888E29298AE", "Visa", "Visa", "24CC2E82-B2B6-4037-87AE-39EEAFE06712" );
            AddDefinedValue( "9E358FBE-2321-4C54-895F-C888E29298AE", "MasterCard", "MasterCard", "50F625F8-F1BE-4FA0-B99F-3FA852D87DD1" );
            AddDefinedValue( "9E358FBE-2321-4C54-895F-C888E29298AE", "Discover", "Discover", "18DF8254-0C68-4FE0-973E-C0B1767EFD3F" );
            AddDefinedValue( "9E358FBE-2321-4C54-895F-C888E29298AE", "Amex", "Amex", "378D8EAD-7FA6-4D0D-862D-ED6E04B17770" );
            AddDefinedValue( "9E358FBE-2321-4C54-895F-C888E29298AE", "PayPal", "PayPal", "4832DA18-DD18-477F-BFDB-ABFC28FE5743" );

            CreateTable(
                "dbo.FinancialGateway",
                c => new
                    {
                        Id = c.Int( nullable: false, identity: true ),
                        Name = c.String( nullable: false, maxLength: 50 ),
                        Description = c.String(),
                        EntityTypeId = c.Int( nullable: false ),
                        Guid = c.Guid( nullable: false ),
                    } )
                .PrimaryKey( t => t.Id );

            AddColumn( "dbo.FinancialScheduledTransaction", "GatewayId", c => c.Int() );
            AddColumn( "dbo.FinancialPersonSavedAccount", "TransactionCode", c => c.String( nullable: false, maxLength: 50 ) );
            AddColumn( "dbo.FinancialPersonSavedAccount", "PaymentMethod", c => c.Int( nullable: false ) );
            AddColumn( "dbo.FinancialPersonSavedAccount", "GatewayId", c => c.Int() );
            AddColumn( "dbo.FinancialTransaction", "GatewayId", c => c.Int() );
            AddColumn( "dbo.FinancialBatch", "BatchDate", c => c.DateTime( storeType: "date" ) );
            DropForeignKey( "dbo.FinancialPersonSavedAccount", "FinancialTransactionId", "dbo.FinancialTransaction" );
            DropForeignKey( "dbo.FinancialTransaction", "ScheduledTransactionId", "dbo.FinancialScheduledTransaction" );
            DropForeignKey( "dbo.FinancialScheduledTransaction", "GatewayEntityTypeId", "dbo.EntityType" );
            DropForeignKey( "dbo.FinancialTransaction", "GatewayEntityTypeId", "dbo.EntityType" );
            DropIndex( "dbo.FinancialPersonSavedAccount", new[] { "FinancialTransactionId" } );
            DropIndex( "dbo.FinancialTransaction", new[] { "ScheduledTransactionId" } );
            DropIndex( "dbo.FinancialScheduledTransaction", new[] { "GatewayEntityTypeId" } );
            DropIndex( "dbo.FinancialTransaction", new[] { "GatewayEntityTypeId" } );
            DropColumn( "dbo.FinancialScheduledTransaction", "GatewayScheduleId" );
            DropColumn( "dbo.FinancialScheduledTransaction", "GatewayEntityTypeId" );
            DropColumn( "dbo.FinancialScheduledTransaction", "LastStatusUpdateDateTime" );
            DropColumn( "dbo.FinancialScheduledTransaction", "NextPaymentDate" );
            DropColumn( "dbo.FinancialPersonSavedAccount", "FinancialTransactionId" );
            DropColumn( "dbo.FinancialTransaction", "ScheduledTransactionId" );
            DropColumn( "dbo.FinancialTransaction", "GatewayEntityTypeId" );
            DropColumn( "dbo.FinancialBatch", "BatchEndDateTime" );
            DropColumn( "dbo.FinancialBatch", "BatchStartDateTime" );
            CreateIndex( "dbo.FinancialScheduledTransaction", "GatewayId" );
            CreateIndex( "dbo.FinancialPersonSavedAccount", "GatewayId" );
            CreateIndex( "dbo.FinancialTransaction", "GatewayId" );
            CreateIndex( "dbo.FinancialGateway", "EntityTypeId" );
            AddForeignKey( "dbo.FinancialScheduledTransaction", "GatewayId", "dbo.FinancialGateway", "Id" );
            AddForeignKey( "dbo.FinancialPersonSavedAccount", "GatewayId", "dbo.FinancialGateway", "Id" );
            AddForeignKey( "dbo.FinancialTransaction", "GatewayId", "dbo.FinancialGateway", "Id" );
            AddForeignKey( "dbo.FinancialGateway", "EntityTypeId", "dbo.EntityType", "Id" );
        }
    }
}
