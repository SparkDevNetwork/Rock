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
            
            AddBlockType( "Finance - Giving Profile Detail", "Giving profile details UI", "~/Blocks/Finance/GivingProfileDetail.ascx", "B343E2B7-0AD0-49B8-B78E-E47BD42171A7" );
            AddBlockType( "Finance - Giving Profile List", "", "~/Blocks/Finance/GivingProfileList.ascx", "694FF260-8C6F-4A59-93C9-CF3793FE30E6" );
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
                      
            // Attrib for BlockType: Finance - Giving Profile Detail:Summary Message
            AddBlockTypeAttribute("B343E2B7-0AD0-49B8-B78E-E47BD42171A7","9C204CD0-1233-41C5-818A-C5DA439445AA","Summary Message","SummaryMessage","Messages","What text should be displayed on the transaction summary?",3
                ,"{{ Date }}<br/> {{ TotalContribution }} given by {{ Person.FullName }} using a {{ PaymentType }} ending in {{ PaymentLastFour }}.","337A4DB8-C7D7-4181-93EB-D112F898E6F2");

            // Attrib for BlockType: Finance - Giving Profile Detail:Show Campuses
            AddBlockTypeAttribute("B343E2B7-0AD0-49B8-B78E-E47BD42171A7","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Show Campuses","ShowCampuses","UI Options","Should giving be associated with a specific campus?",1,"False","D4EE15C2-1BF0-4865-89BE-97840F560D87");

            // Attrib for BlockType: Finance - Giving Profile Detail:Show Checking/ACH
            AddBlockTypeAttribute("B343E2B7-0AD0-49B8-B78E-E47BD42171A7","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Show Checking/ACH","ShowChecking/ACH","UI Options","Allow users to give using a checking account?",3,"True","A7E9F837-6527-403C-9BFE-40DF33502507");

            // Attrib for BlockType: Finance - Giving Profile Detail:Show Frequencies
            AddBlockTypeAttribute("B343E2B7-0AD0-49B8-B78E-E47BD42171A7","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Show Frequencies","ShowFrequencies","UI Options","Allow users to give recurring gifts?",4,"True","E1A03D80-0741-40E3-A143-8C58F6E08532");

            // Attrib for BlockType: Finance - Giving Profile Detail:Require Phone
            AddBlockTypeAttribute("B343E2B7-0AD0-49B8-B78E-E47BD42171A7","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Require Phone","RequirePhone","UI Options","Should financial contributions require a user's phone number?",5,"True","D01AA4B6-547D-4FF7-8BF0-1CEA97B426B0");

            // Attrib for BlockType: Finance - Giving Profile Detail:Confirmation Message
            AddBlockTypeAttribute("B343E2B7-0AD0-49B8-B78E-E47BD42171A7","9C204CD0-1233-41C5-818A-C5DA439445AA","Confirmation Message","ConfirmationMessage","Messages","What text should be displayed on the confirmation page?",0,
                @"{{ ContributionConfirmationHeader }}<br/><br/>
                {{ Person.FullName }},<br/><br/>
                You're about to give a total of <strong>{{ TotalContribution }}</strong> using your {{ PaymentType }} ending in {{ PaymentLastFour }}.<br/><br/>
                If this is correct, please press Give.  Otherwise, click Back to edit.<br/>
                Thank you,<br/><br/>
                {{ OrganizationName }}<br/>  
                {{ ContributionFooter }}","1AF10F92-C93C-4532-9216-E51E9A2D4E3A");

            // Attrib for BlockType: Finance - Giving Profile Detail:Receipt Message
            AddBlockTypeAttribute("B343E2B7-0AD0-49B8-B78E-E47BD42171A7","9C204CD0-1233-41C5-818A-C5DA439445AA","Receipt Message","ReceiptMessage","Messages","What text should be displayed on the receipt page?",1,
                @"{{ ContributionReceiptHeader }}<br/>
                {{ Person.FullName }},<br/><br/>
                Thank you for your generosity! You just gave a total of {{ TotalContribution }} to {{ OrganizationName }}.<br/><br/>        
                {{ ReceiptFooter }}","4A792174-2B6C-43DE-97D6-177EC9FCDE54");

            // Attrib for BlockType: Finance - Giving Profile Detail:Checking/ACH Provider
            AddBlockTypeAttribute("B343E2B7-0AD0-49B8-B78E-E47BD42171A7","BD0D9B57-2A41-4490-89FF-F01DAB7D4904","Checking/ACH Provider","Checking/ACHProvider","Payments","Which payment processor should be used for checking/ACH?",1,"","542188C1-0AFB-485A-8898-31AAB8B24F43");

            // Attrib for BlockType: Finance - Giving Profile Detail:Show Credit Card
            AddBlockTypeAttribute("B343E2B7-0AD0-49B8-B78E-E47BD42171A7","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Show Credit Card","ShowCreditCard","UI Options","Allow users to give using a credit card?",2,"True","F87BE9AE-CA62-439F-9AEB-1977C8B83EE9");

            // Attrib for BlockType: Finance - Giving Profile Detail:Credit Card Provider
            AddBlockTypeAttribute("B343E2B7-0AD0-49B8-B78E-E47BD42171A7","BD0D9B57-2A41-4490-89FF-F01DAB7D4904","Credit Card Provider","CreditCardProvider","Payments","Which payment processor should be used for credit cards?",0,"","09099684-BCE6-442C-B7F5-212BD89B1400");

            // Attrib for BlockType: Finance - Giving Profile Detail:Default Accounts
            AddBlockTypeAttribute("B343E2B7-0AD0-49B8-B78E-E47BD42171A7","B4A28DA2-C3DF-4106-9DE9-03E6E68E4122","Default Accounts","DefaultAccounts","Payments","Which accounts should be displayed by default?",2,"","0182E96B-DC87-4CA8-8029-24EB82889631");

            // Attrib for BlockType: Finance - Giving Profile Detail:Show Vertical Layout
            AddBlockTypeAttribute("B343E2B7-0AD0-49B8-B78E-E47BD42171A7","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Show Vertical Layout","ShowVerticalLayout","UI Options","Should the giving page display vertically or horizontally?",0,"True","8A42DBED-BE06-4A56-A83B-7E6B1DA28AFC");

            // Attrib for BlockType: Finance - Giving Profile List:Detail Page
            AddBlockTypeAttribute("694FF260-8C6F-4A59-93C9-CF3793FE30E6","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Detail Page","DetailPageGuid","","",0,"","C4CF0479-2F51-4106-95DC-BDCA78850694");

            // Attrib for BlockType: Finance - Giving Profile Detail:Linked Page
            AddBlockTypeAttribute( "B343E2B7-0AD0-49B8-B78E-E47BD42171A7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "New Account", "NewAccount", "", "", 0, "", "A4D2C7B0-D79A-457B-AFEC-90C69CF4F3B1" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "C4CF0479-2F51-4106-95DC-BDCA78850694" ); // Detail Page
            DeleteAttribute( "A4D2C7B0-D79A-457B-AFEC-90C69CF4F3B1" ); // Linked Page
            DeleteAttribute( "8A42DBED-BE06-4A56-A83B-7E6B1DA28AFC" ); // Show Vertical Layout
            DeleteAttribute( "0182E96B-DC87-4CA8-8029-24EB82889631" ); // Default Accounts
            DeleteAttribute( "09099684-BCE6-442C-B7F5-212BD89B1400" ); // Credit Card Provider
            DeleteAttribute( "F87BE9AE-CA62-439F-9AEB-1977C8B83EE9" ); // Show Credit Card
            DeleteAttribute( "542188C1-0AFB-485A-8898-31AAB8B24F43" ); // Checking/ACH Provider            
            DeleteAttribute( "4A792174-2B6C-43DE-97D6-177EC9FCDE54" ); // Receipt Message
            DeleteAttribute( "1AF10F92-C93C-4532-9216-E51E9A2D4E3A" ); // Confirmation Message
            DeleteAttribute( "D01AA4B6-547D-4FF7-8BF0-1CEA97B426B0" ); // Require Phone
            DeleteAttribute( "E1A03D80-0741-40E3-A143-8C58F6E08532" ); // Show Frequencies
            DeleteAttribute( "A7E9F837-6527-403C-9BFE-40DF33502507" ); // Show Checking/ACH
            DeleteAttribute( "D4EE15C2-1BF0-4865-89BE-97840F560D87" ); // Show Campuses
            DeleteAttribute( "337A4DB8-C7D7-4181-93EB-D112F898E6F2" ); // Summary Message
            
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
