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
    public partial class GivingBlocks : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "142627AE-6590-48E3-BFCA-3669260B8CF2", "One Time Gift", "", "Default", "9800CE96-C99B-4C70-ADD9-DF22E89378D4" );
            AddPage( "142627AE-6590-48E3-BFCA-3669260B8CF2", "Recurring Gift", "", "Default", "FF73E611-2674-4BA1-A75F-E9291FAC0E19" );
            AddBlockType( "Finance - One Time Gift", "", "~/Blocks/Finance/OneTimeGift.ascx", "4A2AA794-A968-4CCD-973A-C90FD589996F" );
            AddBlockType( "Finance - Recurring Gift", "", "~/Blocks/Finance/RecurringGift.ascx", "F679692F-133E-4F57-9072-D87C675C3283" );            
            AddBlock( "9800CE96-C99B-4C70-ADD9-DF22E89378D4", "4A2AA794-A968-4CCD-973A-C90FD589996F", "One Time Gift", "", "Content", 0, "3BFFEDFD-2198-4A13-827A-4FD1A774949E" );
            AddBlock( "FF73E611-2674-4BA1-A75F-E9291FAC0E19", "F679692F-133E-4F57-9072-D87C675C3283", "Recurring Gift", "", "Content", 0, "0F17BF49-A6D5-47C3-935A-B050127EA939" );
            
            AddBlockTypeAttribute( "4A2AA794-A968-4CCD-973A-C90FD589996F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Credit Card giving", "ShowCreditCardgiving", "UI Options", "Allow users to give using a credit card?", 4, "True", "73DA7706-40D8-4427-B666-CC2220848BBE" );
            AddBlockTypeAttribute( "4A2AA794-A968-4CCD-973A-C90FD589996F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Checking/ACH giving", "ShowChecking/ACHgiving", "UI Options", "Allow users to give using a checking account?", 5, "True", "87E78EF4-761E-43B8-AD61-A7CABBF38708" );
            AddBlockTypeAttribute( "4A2AA794-A968-4CCD-973A-C90FD589996F", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Credit Card Provider", "CreditCardProvider", "Payments", "Which payment processor should be used for credit cards?", 1, "", "6BB568E5-5505-4072-B5D7-D47C5DFD7499" );
            AddBlockTypeAttribute( "4A2AA794-A968-4CCD-973A-C90FD589996F", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Checking/ACH Provider", "Checking/ACHProvider", "Payments", "Which payment processor should be used for checking/ACH?", 2, "", "4D88D9C9-C3EB-4E1B-8894-DB4D41815580" );
            AddBlockTypeAttribute( "4A2AA794-A968-4CCD-973A-C90FD589996F", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Default Funds to display", "DefaultFundstodisplay", "Payments", "Which funds should be displayed by default?", 3, "", "3D4D15BA-A258-428A-990C-6AE5668BCF17" );
            AddBlockTypeAttribute( "4A2AA794-A968-4CCD-973A-C90FD589996F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Stack layout vertically", "Stacklayoutvertically", "UI Options", "Should giving UI be stacked vertically or horizontally?", 2, "True", "487CA28D-E768-4CE7-8B1D-1C418C3650D2" );
            AddBlockTypeAttribute( "4A2AA794-A968-4CCD-973A-C90FD589996F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus selection", "ShowCampusselection", "UI Options", "Should giving be associated with a specific campus?", 3, "False", "09F53EC0-0A6D-4834-91F5-E27BC13B3AA2" );
                        
            AddBlockTypeAttribute( "F679692F-133E-4F57-9072-D87C675C3283", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Credit Card giving", "ShowCreditCardgiving", "UI Options", "Allow users to give using a credit card?", 4, "True", "FB8CB110-C599-4A49-A1E5-6D23EDB05A64" );
            AddBlockTypeAttribute( "F679692F-133E-4F57-9072-D87C675C3283", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Checking/ACH giving", "ShowChecking/ACHgiving", "UI Options", "Allow users to give using a checking account?", 5, "True", "A050195C-F832-4528-9753-ECB1993D7133" );
            AddBlockTypeAttribute( "F679692F-133E-4F57-9072-D87C675C3283", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Credit Card Provider", "CreditCardProvider", "Payments", "Which payment processor should be used for credit cards?", 1, "", "2BA3EF55-D44F-405C-AF50-F070CA16B781" );
            AddBlockTypeAttribute( "F679692F-133E-4F57-9072-D87C675C3283", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Checking/ACH Provider", "Checking/ACHProvider", "Payments", "Which payment processor should be used for checking/ACH?", 2, "", "6B484886-04BA-4AE8-AD19-355CC149BCAE" );
            AddBlockTypeAttribute( "F679692F-133E-4F57-9072-D87C675C3283", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Default Funds to display", "DefaultFundstodisplay", "Payments", "Which funds should be displayed by default?", 3, "", "739EF6E1-288D-4751-AFE8-3C32DF111AE4" );
            AddBlockTypeAttribute( "F679692F-133E-4F57-9072-D87C675C3283", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Stack layout vertically", "Stacklayoutvertically", "UI Options", "Should giving UI be stacked vertically or horizontally?", 2, "True", "1B68CD07-883C-4FC8-BC51-06DF0C0BB1EB" );
            AddBlockTypeAttribute( "F679692F-133E-4F57-9072-D87C675C3283", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus selection", "ShowCampusselection", "UI Options", "Should giving be associated with a specific campus?", 3, "False", "C77F965A-B537-4966-9203-C12F447C8054" );

            // One Time Gift Block Attributes
            // Attrib Value for One Time Gift:Show Credit Card giving
            AddBlockAttributeValue( "3BFFEDFD-2198-4A13-827A-4FD1A774949E", "73DA7706-40D8-4427-B666-CC2220848BBE", "True" );
            // Attrib Value for One Time Gift:Show Checking/ACH giving
            AddBlockAttributeValue( "3BFFEDFD-2198-4A13-827A-4FD1A774949E", "87E78EF4-761E-43B8-AD61-A7CABBF38708", "True" );
            // Attrib Value for One Time Gift:Credit Card Provider
            AddBlockAttributeValue( "3BFFEDFD-2198-4A13-827A-4FD1A774949E", "6BB568E5-5505-4072-B5D7-D47C5DFD7499", "" );
            // Attrib Value for One Time Gift:Checking/ACH Provider
            AddBlockAttributeValue( "3BFFEDFD-2198-4A13-827A-4FD1A774949E", "4D88D9C9-C3EB-4E1B-8894-DB4D41815580", "" );
            // Attrib Value for One Time Gift:Default Funds to display
            AddBlockAttributeValue( "3BFFEDFD-2198-4A13-827A-4FD1A774949E", "3D4D15BA-A258-428A-990C-6AE5668BCF17", "" );
            // Attrib Value for One Time Gift:Stack layout vertically
            AddBlockAttributeValue( "3BFFEDFD-2198-4A13-827A-4FD1A774949E", "487CA28D-E768-4CE7-8B1D-1C418C3650D2", "False" );
            // Attrib Value for One Time Gift:Show Campus selection
            AddBlockAttributeValue( "3BFFEDFD-2198-4A13-827A-4FD1A774949E", "09F53EC0-0A6D-4834-91F5-E27BC13B3AA2", "False" );
            
            // Recurring Gift Block Attributes
            // Attrib Value for One Time Gift:Show Credit Card giving
            AddBlockAttributeValue( "0F17BF49-A6D5-47C3-935A-B050127EA939", "FB8CB110-C599-4A49-A1E5-6D23EDB05A64", "True" );
            // Attrib Value for One Time Gift:Show Checking/ACH giving
            AddBlockAttributeValue( "0F17BF49-A6D5-47C3-935A-B050127EA939", "A050195C-F832-4528-9753-ECB1993D7133", "True" );
            // Attrib Value for One Time Gift:Credit Card Provider
            AddBlockAttributeValue( "0F17BF49-A6D5-47C3-935A-B050127EA939", "2BA3EF55-D44F-405C-AF50-F070CA16B781", "" );
            // Attrib Value for One Time Gift:Checking/ACH Provider
            AddBlockAttributeValue( "0F17BF49-A6D5-47C3-935A-B050127EA939", "6B484886-04BA-4AE8-AD19-355CC149BCAE", "" );
            // Attrib Value for One Time Gift:Default Funds to display
            AddBlockAttributeValue( "0F17BF49-A6D5-47C3-935A-B050127EA939", "739EF6E1-288D-4751-AFE8-3C32DF111AE4", "" );
            // Attrib Value for One Time Gift:Stack layout vertically
            AddBlockAttributeValue( "0F17BF49-A6D5-47C3-935A-B050127EA939", "1B68CD07-883C-4FC8-BC51-06DF0C0BB1EB", "False" );
            // Attrib Value for One Time Gift:Show Campus selection
            AddBlockAttributeValue( "0F17BF49-A6D5-47C3-935A-B050127EA939", "C77F965A-B537-4966-9203-C12F447C8054", "False" );            

            // Move Financial Transactions page to Finance section
            DeleteBlock( "B447AB11-3A19-4527-921A-2266A6B4E181" );
            DeleteBlockType( "18EE7010-E8CF-4B61-BFDA-E014CCFC9E6D" );
            AddBlockType( "Finance - Transactions", "View and search financial transactions", "~/Blocks/Finance/Transactions.ascx", "E04320BC-67C3-452D-9EF6-D74D8C177154" );
            AddBlock( "7CA317B5-5C47-465D-B407-7D614F2A568F", "E04320BC-67C3-452D-9EF6-D74D8C177154", "Financial Transaction", "", "Content", 0, "B447AB11-3A19-4527-921A-2266A6B4E181" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Move Financial Transactions page back to Administration section
            DeleteBlock( "B447AB11-3A19-4527-921A-2266A6B4E181" );
            DeleteBlockType( "E04320BC-67C3-452D-9EF6-D74D8C177154" );
            AddBlockType( "Financial Transactions", "View and search financial transactions", "~/Blocks/Administration/Financials.ascx", "18ee7010-e8cf-4b61-bfda-e014ccfc9e6d" );
            AddBlock( "7CA317B5-5C47-465D-B407-7D614F2A568F", "18ee7010-e8cf-4b61-bfda-e014ccfc9e6d", "Financial Transactions", "", "Content", 0, "B447AB11-3A19-4527-921A-2266A6B4E181" );

            DeleteAttribute( "73DA7706-40D8-4427-B666-CC2220848BBE" ); // Show Credit Card giving
            DeleteAttribute( "4D88D9C9-C3EB-4E1B-8894-DB4D41815580" ); // Checking/ACH Provider
            DeleteAttribute( "3D4D15BA-A258-428A-990C-6AE5668BCF17" ); // Default Funds to display
            DeleteAttribute( "487CA28D-E768-4CE7-8B1D-1C418C3650D2" ); // Stack layout vertically
            DeleteAttribute( "09F53EC0-0A6D-4834-91F5-E27BC13B3AA2" ); // Show Campus selection
            DeleteAttribute( "6BB568E5-5505-4072-B5D7-D47C5DFD7499" ); // Credit Card Provider
            DeleteAttribute( "87E78EF4-761E-43B8-AD61-A7CABBF38708" ); // Show Checking/ACH giving
            DeleteAttribute( "6B484886-04BA-4AE8-AD19-355CC149BCAE" ); // Checking/ACH Provider
            DeleteAttribute( "FB8CB110-C599-4A49-A1E5-6D23EDB05A64" ); // Show Credit Card giving
            DeleteAttribute( "A050195C-F832-4528-9753-ECB1993D7133" ); // Show Checking/ACH giving
            DeleteAttribute( "2BA3EF55-D44F-405C-AF50-F070CA16B781" ); // Credit Card Provider
            DeleteAttribute( "739EF6E1-288D-4751-AFE8-3C32DF111AE4" ); // Default Funds to display
            DeleteAttribute( "1B68CD07-883C-4FC8-BC51-06DF0C0BB1EB" ); // Stack layout vertically
            DeleteAttribute( "C77F965A-B537-4966-9203-C12F447C8054" ); // Show Campus selection
            DeleteBlock( "3BFFEDFD-2198-4A13-827A-4FD1A774949E" ); // One Time Gift
            DeleteBlock( "0F17BF49-A6D5-47C3-935A-B050127EA939" ); // Recurring Gift
            DeleteBlockType( "4A2AA794-A968-4CCD-973A-C90FD589996F" ); // Finance - One Time Gift
            DeleteBlockType( "F679692F-133E-4F57-9072-D87C675C3283" ); // Finance - Recurring Gift
            DeletePage( "9800CE96-C99B-4C70-ADD9-DF22E89378D4" ); // One Time Gift
            DeletePage( "FF73E611-2674-4BA1-A75F-E9291FAC0E19" ); // Recurring Gift
        }
    }
}
