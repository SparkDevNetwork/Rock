using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace church.ccv.SampleProject.Migrations
{
    [MigrationNumber( 2, "1.0.8" )]
    public class AddSystemData : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( "7F2581A1-941E-4D51-8A9D-5BE9B881B003", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Referral Agencies", "", "223AC4F2-CBED-4733-807A-188CFBBFA0C8", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "223AC4F2-CBED-4733-807A-188CFBBFA0C8", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Referral Agency Details", "", "4BF8FA57-AE86-4103-B07E-80ECE0000AEE", "" ); // Site:Rock RMS

            // Since the Referral Agency Details block handles displaying the breadcrumb for the page, we need to turn off the default breadcrumb rendered by the page
            Sql( @"
    UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '4BF8FA57-AE86-4103-B07E-80ECE0000AEE'
" );

            RockMigrationHelper.UpdateBlockType( "Referral Agency Detail", "Displays the details of a Referral Agency.", "~/Plugins/com_ccvonline/SampleProject/ReferralAgencyDetail.ascx", "CCV > Sample Project", "2F130DF6-1EE4-45CE-9410-CBB0517EB33E" );
            RockMigrationHelper.UpdateBlockType( "Referral Agency List", "Lists all the Referral Agencies.", "~/Plugins/com_ccvonline/SampleProject/ReferralAgencyList.ascx", "CCV > Sample Project", "53F447CE-4B91-470A-A15D-B60DCAAB29CB" );
            
            // Add Block to Page: Referral Agencies, Site: Rock RMS
            RockMigrationHelper.AddBlock( "223AC4F2-CBED-4733-807A-188CFBBFA0C8", "", "53F447CE-4B91-470A-A15D-B60DCAAB29CB", "Referral Agency List", "Main", "", "", 0, "A0B53736-4132-4D1B-8300-9F9FB1A5DC21" );
            // Add Block to Page: Referral Agency Details, Site: Rock RMS
            RockMigrationHelper.AddBlock( "4BF8FA57-AE86-4103-B07E-80ECE0000AEE", "", "2F130DF6-1EE4-45CE-9410-CBB0517EB33E", "Referral Agency Detail", "Main", "", "", 0, "B69EBD0E-A1B4-47C5-AAE7-B40BEA37965F" );
            // Attrib for BlockType: Referral Agency List:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "53F447CE-4B91-470A-A15D-B60DCAAB29CB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "5B480350-663C-4789-BF4D-33EC8DF882E8" );
            // Attrib Value for Block:Referral Agency List, Attribute:Detail Page Page: Referral Agencies, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A0B53736-4132-4D1B-8300-9F9FB1A5DC21", "5B480350-663C-4789-BF4D-33EC8DF882E8", @"4bf8fa57-ae86-4103-b07e-80ece0000aee" );

            RockMigrationHelper.AddDefinedType( "Global", "Referral Agency Type", "The type of agency (e.g. Counseling, Food, Financial Assistance, etc.)", "150478D4-3709-4543-906F-1F9496B4E7D0" );
            RockMigrationHelper.AddDefinedValue( "150478D4-3709-4543-906F-1F9496B4E7D0", "Counseling and Therapy", "", "83F9A59C-DBE5-4E1A-B33C-F701FA8175E1", false );
            RockMigrationHelper.AddDefinedValue( "150478D4-3709-4543-906F-1F9496B4E7D0", "Financial Assistance or Counseling", "", "7A30D312-996E-4823-B1FF-AA27C1806521", false );
            RockMigrationHelper.AddDefinedValue( "150478D4-3709-4543-906F-1F9496B4E7D0", "24 Hour Crisis Hotlines", "", "EDBE6DCE-313F-4648-8D97-A39520A54BFC", false );
            RockMigrationHelper.AddDefinedValue( "150478D4-3709-4543-906F-1F9496B4E7D0", "Youth Resources", "", "BB666FA1-5391-40B1-B334-3A27575AD9D5", false );
            RockMigrationHelper.AddDefinedValue( "150478D4-3709-4543-906F-1F9496B4E7D0", "Food and Clothing", "", "E15AE7DE-3555-437B-99B0-B28601C4EA2D", false );
            RockMigrationHelper.AddDefinedValue( "150478D4-3709-4543-906F-1F9496B4E7D0", "Homeless Resources/Housing", "", "F6E4D78C-E05A-4AEF-AF8C-09B3B8FDDEBF", false );
            RockMigrationHelper.AddDefinedValue( "150478D4-3709-4543-906F-1F9496B4E7D0", "Substance Abuse", "", "AD01D370-7CB6-4261-ACF6-8EE21CB353AA", false );
            RockMigrationHelper.AddDefinedValue( "150478D4-3709-4543-906F-1F9496B4E7D0", "Residential Drug Treatment Centers", "", "57F4BCC8-B80F-48E5-93E2-A76E3F572C0C", false );
            RockMigrationHelper.AddDefinedValue( "150478D4-3709-4543-906F-1F9496B4E7D0", "Domestic Violence Resources", "", "AE95FD8A-FD9E-4EDD-9689-5491725FEFE6", false );
            RockMigrationHelper.AddDefinedValue( "150478D4-3709-4543-906F-1F9496B4E7D0", "Mediation", "", "40C66BE2-CE13-4E7D-980A-A2D66968CE57", false );
            RockMigrationHelper.AddDefinedValue( "150478D4-3709-4543-906F-1F9496B4E7D0", "Miscellaneous", "", "62AB4A35-6E72-4BCD-BF6A-5B0D2052BACA", false );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            Sql( @"
    UPDATE [_com_ccvonline_SampleProject_ReferralAgency] SET [AgencyTypeValueId] = NULL
" );

            RockMigrationHelper.DeleteDefinedType( "150478D4-3709-4543-906F-1F9496B4E7D0" );

            // Attrib for BlockType: Referral Agency List:Detail Page
            RockMigrationHelper.DeleteAttribute( "5B480350-663C-4789-BF4D-33EC8DF882E8" );
            
            // Remove Block: Referral Agency Detail, from Page: Referral Agency Details, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B69EBD0E-A1B4-47C5-AAE7-B40BEA37965F" );
            // Remove Block: Referral Agency List, from Page: Referral Agencies, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "A0B53736-4132-4D1B-8300-9F9FB1A5DC21" );

            RockMigrationHelper.DeleteBlockType( "53F447CE-4B91-470A-A15D-B60DCAAB29CB" ); // Referral Agency List
            RockMigrationHelper.DeleteBlockType( "2F130DF6-1EE4-45CE-9410-CBB0517EB33E" ); // Referral Agency Detail

            RockMigrationHelper.DeletePage( "4BF8FA57-AE86-4103-B07E-80ECE0000AEE" ); // Page: Referral Agency DetailsLayout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "223AC4F2-CBED-4733-807A-188CFBBFA0C8" ); // Page: Referral AgenciesLayout: Full Width, Site: Rock RMS

        }
    }
}
