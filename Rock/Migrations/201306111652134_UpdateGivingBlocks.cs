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
    public partial class UpdateGivingBlocks : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DeleteBlock( "3BFFEDFD-2198-4A13-827A-4FD1A774949E" ); // One Time Gift
            DeleteBlock( "0F17BF49-A6D5-47C3-935A-B050127EA939" ); // Recurring Gift
            DeleteBlockType( "4A2AA794-A968-4CCD-973A-C90FD589996F" ); // Finance - One Time Gift
            DeleteBlockType( "F679692F-133E-4F57-9072-D87C675C3283" ); // Finance - Recurring Gift
            DeletePage( "9800CE96-C99B-4C70-ADD9-DF22E89378D4" ); // One Time Gift
            DeletePage( "FF73E611-2674-4BA1-A75F-E9291FAC0E19" ); // Recurring Gift            

            AddPage( "142627AE-6590-48E3-BFCA-3669260B8CF2", "Scheduled Contributions", "", "Default", "F23C5BF7-4F52-448F-8445-6BAEEE3030AB" );
            AddPage( "142627AE-6590-48E3-BFCA-3669260B8CF2", "Make a Contribution", "", "Default", "1615E090-1889-42FF-AB18-5F7BE9F24498" );

            AddBlockType( "Finance - Giving Profile Detail", "Giving profile details", "~/Blocks/Finance/GivingProfileDetail.ascx", "B343E2B7-0AD0-49B8-B78E-E47BD42171A7" );
            AddBlockType( "Finance - Giving Profile List", "Giving profile list", "~/Blocks/Finance/GivingProfileList.ascx", "694FF260-8C6F-4A59-93C9-CF3793FE30E6" );
            AddBlock( "F23C5BF7-4F52-448F-8445-6BAEEE3030AB", "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "Scheduled Contributions", "", "Content", 0, "32A7BA7B-968E-4BFD-BEA3-042CF863D751" );
            AddBlock( "1615E090-1889-42FF-AB18-5F7BE9F24498", "B343E2B7-0AD0-49B8-B78E-E47BD42171A7", "Contributions", "", "Content", 0, "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96" );

            AddDefinedType( "Financial", "Payment Type", "The type of payment associated with a transaction", "23E80D98-017E-47B9-BAF3-AC442A1EC3EE" );
            AddDefinedValue( "23E80D98-017E-47B9-BAF3-AC442A1EC3EE", "Credit Card", "Credit Card payment type", "09412338-AAAA-4644-BA2A-4CADBE653468" );
            AddDefinedValue( "23E80D98-017E-47B9-BAF3-AC442A1EC3EE", "Checking/ACH", "Checking/ACH payment type", "FFAD975C-7504-418F-8959-30BD0C62CD30" );

            // Remove current transaction frequency types
            DeleteDefinedValue( "35711E44-131B-4534-B0B2-F0A749292362" );
            DeleteDefinedValue( "72990023-0D43-4554-8D32-28461CAB8920" );
            DeleteDefinedValue( "1400753C-A0F9-4A45-8A1D-81C98450BD1F" );
            DeleteDefinedValue( "791C863D-2600-445B-98F8-3E5B66A3DEC4" );

            // Add & re-order transaction frequency types
            AddDefinedValue( "1F645CFB-5BBD-4465-B9CA-0D2104A1479B", "One-Time", "One Time", "82614683-7FB4-4F16-9087-6F85945A7B16" );
            AddDefinedValue( "1F645CFB-5BBD-4465-B9CA-0D2104A1479B", "One-Time (Future)", "One Time Future", "A5A12067-322E-44A4-94C4-561312F9913C" );
            AddDefinedValue( "1F645CFB-5BBD-4465-B9CA-0D2104A1479B", "Weekly", "Every Week", "35711E44-131B-4534-B0B2-F0A749292362" );
            AddDefinedValue( "1F645CFB-5BBD-4465-B9CA-0D2104A1479B", "Bi-Weekly", "Every Two Weeks", "72990023-0D43-4554-8D32-28461CAB8920" );
            AddDefinedValue( "1F645CFB-5BBD-4465-B9CA-0D2104A1479B", "Twice a Month", "Twice a Month", "791C863D-2600-445B-98F8-3E5B66A3DEC4" );
            AddDefinedValue( "1F645CFB-5BBD-4465-B9CA-0D2104A1479B", "Monthly", "Once a Month", "1400753C-A0F9-4A45-8A1D-81C98450BD1F" );

            // Fix for field types
            //UpdateFieldType( "Accounts Field Type", "", "Rock", "Rock.Field.Types.AccountsFieldType", "1629FF21-8491-41B5-B0FF-C424608E50E0" );

            // Attrib for BlockType: Finance - Giving Profile Detail:Confirmation Message
            AddBlockTypeAttribute( "B343E2B7-0AD0-49B8-B78E-E47BD42171A7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Confirmation Message", "ConfirmationMessage", "Message Options", "What text should be displayed on the confirmation page?", 0
                , @"{{ ContributionConfirmationHeader }}<br/><br/>
	            {{ Person.FullName }},<br/><br/>
	            You are about to give a total of <strong>{{ TotalContribution }}</strong> using your {{ PaymentType }} ending in {{ PaymentLastFour }}.<br/><br/>
	            If this is correct, please press Give.  Otherwise, click Back to edit.<br/>
	            Thank you,<br/><br/>
	            {{ OrganizationName }}<br/>  
	            {{ ContributionConfirmationFooter }}"
                , "D396F18E-3A84-43C6-93AC-9282460B6A17" );

            // Attrib for BlockType: Finance - Giving Profile Detail:Receipt Message
            AddBlockTypeAttribute( "B343E2B7-0AD0-49B8-B78E-E47BD42171A7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Receipt Message", "ReceiptMessage", "Message Options", "What text should be displayed on the receipt page?", 1
                , @"{{ ContributionReceiptHeader }}<br/>
	            {{ Person.FullName }},<br/><br/>
	            Thank you for your generosity! You just gave a total of {{ TotalContribution }} to {{ OrganizationName }}.<br/><br/>        
	            {{ ContributionReceiptFooter }}"
                , "FB1292E0-B0FA-4A1E-953B-CACD2FC65983" );

            // Attrib for BlockType: Finance - Giving Profile Detail:Summary Message
            AddBlockTypeAttribute( "B343E2B7-0AD0-49B8-B78E-E47BD42171A7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Summary Message", "SummaryMessage", "Message Options", "What text should be displayed on the transaction summary?", 3
                , "{{ Date }}<br/> {{ TotalContribution }} given by {{ Person.FullName }} using a {{ PaymentType }} ending in {{ PaymentLastFour }}.", "12EB04DB-664F-4B50-85F5-22860BFDE340" );

            // Attrib for BlockType: Finance - Giving Profile List:Detail Page
            AddBlockTypeAttribute( "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPageGuid", "", "", 0, "", "D9775A5E-A271-439C-B7A3-00141195A0F2" );

            // Attrib for BlockType: Finance - Giving Profile Detail:Show Campuses
            AddBlockTypeAttribute( "B343E2B7-0AD0-49B8-B78E-E47BD42171A7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campuses", "ShowCampuses", "UI Options", "Should giving be associated with a specific campus?", 1, "False", "F4C4BB2B-2492-4DDC-BCA1-F8279BF23CA3" );

            // Attrib for BlockType: Finance - Giving Profile Detail:Show Credit Card
            AddBlockTypeAttribute( "B343E2B7-0AD0-49B8-B78E-E47BD42171A7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Credit Card", "ShowCreditCard", "UI Options", "Allow users to give using a credit card?", 3, "True", "C295CDA1-8010-438B-85A2-1CC76AA6D231" );

            // Attrib for BlockType: Finance - Giving Profile Detail:Show Checking/ACH
            AddBlockTypeAttribute( "B343E2B7-0AD0-49B8-B78E-E47BD42171A7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Checking/ACH", "ShowChecking/ACH", "UI Options", "Allow users to give using a checking account?", 4, "True", "14896278-80A6-475E-A543-F96FB5683482" );

            // Attrib for BlockType: Finance - Giving Profile Detail:Show Frequencies
            AddBlockTypeAttribute( "B343E2B7-0AD0-49B8-B78E-E47BD42171A7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Frequencies", "ShowFrequencies", "UI Options", "Allow users to give recurring gifts?", 5, "True", "9EEDD59A-AC64-4ACC-8C72-35F01F341505" );

            // Attrib for BlockType: Finance - Giving Profile Detail:Require Phone
            AddBlockTypeAttribute( "B343E2B7-0AD0-49B8-B78E-E47BD42171A7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Phone", "RequirePhone", "UI Options", "Should financial contributions require a user's phone number?", 6, "True", "9E95E73D-80BF-404D-B5D3-B4A1146E1C9C" );

            // Attrib for BlockType: Finance - Giving Profile Detail:New Accounts
            AddBlockTypeAttribute( "B343E2B7-0AD0-49B8-B78E-E47BD42171A7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "New Accounts", "NewAccounts", "Financial", "What page should users redirect to when creating a new account?", 0, "7D4E2142-D24E-4DD2-84BC-B34C5C3D0D46", "1328AE22-8FFE-41B6-90E3-2E5A5159348B" );

            // Attrib for BlockType: Finance - Giving Profile Detail:Summary Message
            AddBlockTypeAttribute( "B343E2B7-0AD0-49B8-B78E-E47BD42171A7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Summary Message", "SummaryMessage", "Message Options", "What text should be displayed on the transaction summary?", 2, "{{ Date }}: {{ TotalContribution }} given by {{ Person.FullName }} using a {{ PaymentType }} ending in {{ PaymentLastFour }}.", "12EB04DB-664F-4B50-85F5-22860BFDE340" );

            // Attrib for BlockType: Finance - Giving Profile Detail:Checking/ACH Provider
            AddBlockTypeAttribute( "B343E2B7-0AD0-49B8-B78E-E47BD42171A7", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Checking/ACH Provider", "Checking/ACHProvider", "Payment Options", "Which payment processor should be used for checking/ACH?", 1, "", "1BD00EB8-E86B-4EC1-BAC3-FEDFEDFF9EDA" );

            // Attrib for BlockType: Finance - Giving Profile Detail:Show Additional Accounts
            AddBlockTypeAttribute( "B343E2B7-0AD0-49B8-B78E-E47BD42171A7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Additional Accounts", "ShowAdditionalAccounts", "UI Options", "Should users be allowed to give to additional accounts?", 2, "True", "015325DA-34AE-4E70-A1BD-9D657AD8C67B" );

            // Attrib for BlockType: Finance - Giving Profile Detail:Credit Card Provider
            AddBlockTypeAttribute( "B343E2B7-0AD0-49B8-B78E-E47BD42171A7", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Credit Card Provider", "CreditCardProvider", "Payment Options", "Which payment processor should be used for credit cards?", 0, "", "5DD98BDC-4E8F-4452-A0D0-4AD7F378E581" );

            // Attrib for BlockType: Finance - Giving Profile Detail:Default Accounts
            AddBlockTypeAttribute( "B343E2B7-0AD0-49B8-B78E-E47BD42171A7", "17033CDD-EF97-4413-A483-7B85A787A87F", "Default Accounts", "DefaultAccounts", "Payment Options", "Which accounts should be displayed by default?", 2, "", "B5986804-EC11-45E4-AFF6-326B58385BB0" );

            // Attrib for BlockType: Finance - Giving Profile Detail:Show Vertical Layout
            AddBlockTypeAttribute( "B343E2B7-0AD0-49B8-B78E-E47BD42171A7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Vertical Layout", "ShowVerticalLayout", "UI Options", "Should the giving page display vertically or horizontally?", 0, "True", "0E559CB5-9A27-433C-80C6-0DBCFB5E1FA3" );

            // Attrib Value for Scheduled Contributions:Detail Page
            AddBlockAttributeValue( "32A7BA7B-968E-4BFD-BEA3-042CF863D751", "D9775A5E-A271-439C-B7A3-00141195A0F2", "1615E090-1889-42FF-AB18-5F7BE9F24498" );

            // Attrib Value for Contributions:Show Campuses
            AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "F4C4BB2B-2492-4DDC-BCA1-F8279BF23CA3", "True" );

            // Attrib Value for Contributions:Show Credit Card
            AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "C295CDA1-8010-438B-85A2-1CC76AA6D231", "True" );

            // Attrib Value for Contributions:Show Checking/ACH
            AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "14896278-80A6-475E-A543-F96FB5683482", "True" );

            // Attrib Value for Contributions:Show Frequencies
            AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "9EEDD59A-AC64-4ACC-8C72-35F01F341505", "True" );

            // Attrib Value for Contributions:Require Phone
            AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "9E95E73D-80BF-404D-B5D3-B4A1146E1C9C", "True" );
            
            // Attrib Value for Contributions:Show Vertical Layout
            AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "0E559CB5-9A27-433C-80C6-0DBCFB5E1FA3", "True" );

            // Attrib Value for Contributions:Checking/ACH Provider
            AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "1BD00EB8-E86B-4EC1-BAC3-FEDFEDFF9EDA", "" );

            // Attrib Value for Contributions:Show Additional Accounts
            AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "015325DA-34AE-4E70-A1BD-9D657AD8C67B", "True" );

            // Attrib Value for Contributions:Credit Card Provider
            AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "5DD98BDC-4E8F-4452-A0D0-4AD7F378E581", "" );

            // Attrib Value for Contributions:New Accounts
            AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "1328AE22-8FFE-41B6-90E3-2E5A5159348B", "7d4e2142-d24e-4dd2-84bc-b34c5c3d0d46" );

            // Attrib Value for Contributions:Default Accounts
            AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "B5986804-EC11-45E4-AFF6-326B58385BB0", "4410306f-3fb5-4a57-9a80-09a3f9d40d0c,67c6181c-1d8c-44d7-b262-b81e746f06d8" );

            // Attrib Value for Contributions:Confirmation Message
            AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "D396F18E-3A84-43C6-93AC-9282460B6A17"
                , @"{{ ContributionConfirmationHeader }}<br/><br/>        
                {{ Person.FullName }},<br/><br/>        
                You are about to give a total of <strong>{{ TotalContribution }}</strong> 
                using your {{ PaymentType }} ending in {{ PaymentLastFour }}.<br/><br/>        
                If this is correct, please press Give.  Otherwise, click Back to edit.<br/>        
                Thank you,<br/><br/>        {{ OrganizationName }}<br/>          
                {{ ContributionConfirmaFooter }}" );

            // Attrib Value for Contributions:Receipt Message
            AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "FB1292E0-B0FA-4A1E-953B-CACD2FC65983"
                , @"{{ ContributionReceiptHeader }}<br/>        
                {{ Person.FullName }},<br/><br/>        
                Thank you for your generosity! You just gave a total of {{ TotalContribution }} to {{ OrganizationName }}.<br/><br/>                
                {{ ContributionReceiptFooter }}" );

            // Attrib Value for Contributions:Summary Message
            AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "12EB04DB-664F-4B50-85F5-22860BFDE340"
                , @"{{ Date }}: {{ TotalContribution }} given by {{ Person.FullName }} 
                using a {{ PaymentType }} ending in {{ PaymentLastFour }}." );

            
            

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "0E559CB5-9A27-433C-80C6-0DBCFB5E1FA3" ); // Show Vertical Layout
            DeleteAttribute( "B5986804-EC11-45E4-AFF6-326B58385BB0" ); // Default Accounts
            DeleteAttribute( "5DD98BDC-4E8F-4452-A0D0-4AD7F378E581" ); // Credit Card Provider
            DeleteAttribute( "015325DA-34AE-4E70-A1BD-9D657AD8C67B" ); // Show Additional Accounts
            DeleteAttribute( "1BD00EB8-E86B-4EC1-BAC3-FEDFEDFF9EDA" ); // Checking/ACH Provider
            DeleteAttribute( "1328AE22-8FFE-41B6-90E3-2E5A5159348B" ); // New Accounts
            DeleteAttribute( "9E95E73D-80BF-404D-B5D3-B4A1146E1C9C" ); // Require Phone
            DeleteAttribute( "9EEDD59A-AC64-4ACC-8C72-35F01F341505" ); // Show Frequencies
            DeleteAttribute( "14896278-80A6-475E-A543-F96FB5683482" ); // Show Checking/ACH
            DeleteAttribute( "C295CDA1-8010-438B-85A2-1CC76AA6D231" ); // Show Credit Card
            DeleteAttribute( "F4C4BB2B-2492-4DDC-BCA1-F8279BF23CA3" ); // Show Campuses
            DeleteAttribute( "D396F18E-3A84-43C6-93AC-9282460B6A17" ); // Confirmation Message
            DeleteAttribute( "FB1292E0-B0FA-4A1E-953B-CACD2FC65983" ); // Receipt Message
            DeleteAttribute( "12EB04DB-664F-4B50-85F5-22860BFDE340" ); // Summary Message
            DeleteAttribute( "D9775A5E-A271-439C-B7A3-00141195A0F2" ); // Detail Page

            DeleteDefinedType( "23E80D98-017E-47B9-BAF3-AC442A1EC3EE" ); // Payment Type
            DeleteDefinedValue( "09412338-AAAA-4644-BA2A-4CADBE653468" );
            DeleteDefinedValue( "FFAD975C-7504-418F-8959-30BD0C62CD30" );

            DeleteDefinedValue( "82614683-7FB4-4F16-9087-6F85945A7B16" ); // Frequency Type
            DeleteDefinedValue( "A5A12067-322E-44A4-94C4-561312F9913C" );

            DeleteBlock( "32A7BA7B-968E-4BFD-BEA3-042CF863D751" ); // Scheduled Contributions
            DeleteBlock( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96" ); // Contributions
            DeleteBlockType( "B343E2B7-0AD0-49B8-B78E-E47BD42171A7" ); // Finance - Giving Profile Detail
            DeleteBlockType( "694FF260-8C6F-4A59-93C9-CF3793FE30E6" ); // Finance - Giving Profile List
            DeletePage( "F23C5BF7-4F52-448F-8445-6BAEEE3030AB" ); // Scheduled Contributions
            DeletePage( "1615E090-1889-42FF-AB18-5F7BE9F24498" ); // Make a Contribution

            AddPage( "142627AE-6590-48E3-BFCA-3669260B8CF2", "One Time Gift", "", "Default", "9800CE96-C99B-4C70-ADD9-DF22E89378D4" );
            AddPage( "142627AE-6590-48E3-BFCA-3669260B8CF2", "Recurring Gift", "", "Default", "FF73E611-2674-4BA1-A75F-E9291FAC0E19" );
            AddBlockType( "Finance - One Time Gift", "", "~/Blocks/Finance/OneTimeGift.ascx", "4A2AA794-A968-4CCD-973A-C90FD589996F" );
            AddBlockType( "Finance - Recurring Gift", "", "~/Blocks/Finance/RecurringGift.ascx", "F679692F-133E-4F57-9072-D87C675C3283" );
            AddBlock( "9800CE96-C99B-4C70-ADD9-DF22E89378D4", "4A2AA794-A968-4CCD-973A-C90FD589996F", "One Time Gift", "", "Content", 0, "3BFFEDFD-2198-4A13-827A-4FD1A774949E" );
            AddBlock( "FF73E611-2674-4BA1-A75F-E9291FAC0E19", "F679692F-133E-4F57-9072-D87C675C3283", "Recurring Gift", "", "Content", 0, "0F17BF49-A6D5-47C3-935A-B050127EA939" );
        }
    }
}
