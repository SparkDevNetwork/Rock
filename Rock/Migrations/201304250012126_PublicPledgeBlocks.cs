//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

namespace Rock.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PublicPledgeBlocks : RockMigration_5
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddDefinedValue( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Active", "", "41540783-D9EF-4C70-8F1D-C9E83D91ED5F" );
            AddDefinedValue( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Inactive", "", "B91BA046-BC1E-400C-B85D-638C1F4E0CE2" );
            AddDefinedValue( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Web Prospect", "", "368DD475-242C-49C4-A42C-7278BE690CC2" );
            AddDefinedValue( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Unknown", "", "0250B035-7E2C-449F-AFE2-0FDD4A13560C" );
            UpdateFieldType( "Financial Account", "A single Financial Account", "Rock", "Rock.Field.Types.AccountFieldType", "434D7B6F-F8DD-45B7-8C3E-C76EF10BE56A" );
            UpdateFieldType( "Financial Accounts", "A selection of multiple Financial Accounts", "Rock", "Rock.Field.Type.AccountsFieldType", "17033CDD-EF97-4413-A483-7B85A787A87F" );
            AddPage( "1570D2AF-4FE2-4FC7-BED9-F20EBCBE9867", "(Public) Create Pledge", "", "Default", "60051DAF-2986-406D-A78B-1609CBF2256D" );
            AddBlockType( "Finance - Create Pledge", "", "~/Blocks/Finance/CreatePledge.ascx", "20B5568E-A010-4E15-9127-E63CF218D6E5" );
            AddBlock( "60051DAF-2986-406D-A78B-1609CBF2256D", "20B5568E-A010-4E15-9127-E63CF218D6E5", "Create Pledge", "", "Content", 0, "4898B608-4A08-4C1B-97A7-0500D708B5D2" );
            AddBlockTypeAttribute( "20B5568E-A010-4E15-9127-E63CF218D6E5", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "New User Status", "DefaultPersonStatus", "", "Person status to assign to a new user.", 5, "", "88021EF1-853C-4FF7-859B-FDDB4C5E9DDC" );
            AddBlockTypeAttribute( "20B5568E-A010-4E15-9127-E63CF218D6E5", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "DefaultAccounts", "", "The accounts that new pledges will be allocated toward", 0, "", "FEE41040-8849-47FA-820C-242DB0EEAF51" );
            AddBlockTypeAttribute( "20B5568E-A010-4E15-9127-E63CF218D6E5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Legend Text", "LegendText", "", "Custom heading at the top of the form.", 1, "Create a new pledge", "716C4728-7F01-4B4A-B853-CFEACE9E1FC2" );
            AddBlockTypeAttribute( "20B5568E-A010-4E15-9127-E63CF218D6E5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Giving Page", "GivingPage", "", "The page used to set up a person's giving profile.", 2, "", "3D76EC7E-5779-4414-BF06-5975A721FD4D" );
            AddBlockTypeAttribute( "20B5568E-A010-4E15-9127-E63CF218D6E5", "6B6AA175-4758-453F-8D83-FCD8044B5F36", "Start Date", "DefaultStartDate", "", "Date all pledges will begin on.", 3, "", "3C37D459-E0A2-48CF-B3D4-5AEACD4ABA13" );
            AddBlockTypeAttribute( "20B5568E-A010-4E15-9127-E63CF218D6E5", "6B6AA175-4758-453F-8D83-FCD8044B5F36", "End Date", "DefaultEndDate", "", "Date all pledges will end.", 4, "", "1289E02A-862B-4B50-B07A-A625940480C0" );
            // Attrib Value for Create Pledge:New User Status
            AddBlockAttributeValue("4898B608-4A08-4C1B-97A7-0500D708B5D2","88021EF1-853C-4FF7-859B-FDDB4C5E9DDC","61");  
            // Attrib Value for Create Pledge:Accounts
            AddBlockAttributeValue("4898B608-4A08-4C1B-97A7-0500D708B5D2","FEE41040-8849-47FA-820C-242DB0EEAF51","67c6181c-1d8c-44d7-b262-b81e746f06d8,bab250ee-cae6-4a41-9756-ad9327408be0");  
            // Attrib Value for Create Pledge:Legend Text
            AddBlockAttributeValue("4898B608-4A08-4C1B-97A7-0500D708B5D2","716C4728-7F01-4B4A-B853-CFEACE9E1FC2","Create a new pledge");  
            // Attrib Value for Create Pledge:Giving Page
            AddBlockAttributeValue("4898B608-4A08-4C1B-97A7-0500D708B5D2","3D76EC7E-5779-4414-BF06-5975A721FD4D","1570d2af-4fe2-4fc7-bed9-f20ebcbe9867");  
            // Attrib Value for Create Pledge:Start Date
            AddBlockAttributeValue("4898B608-4A08-4C1B-97A7-0500D708B5D2","3C37D459-E0A2-48CF-B3D4-5AEACD4ABA13","1/1/2013");  
            // Attrib Value for Create Pledge:End Date
            AddBlockAttributeValue("4898B608-4A08-4C1B-97A7-0500D708B5D2","1289E02A-862B-4B50-B07A-A625940480C0","12/31/2013");  
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "88021EF1-853C-4FF7-859B-FDDB4C5E9DDC" ); // New User Status
            DeleteAttribute( "FEE41040-8849-47FA-820C-242DB0EEAF51" ); // Accounts
            DeleteAttribute( "716C4728-7F01-4B4A-B853-CFEACE9E1FC2" ); // Legend Text
            DeleteAttribute( "3D76EC7E-5779-4414-BF06-5975A721FD4D" ); // Giving Page
            DeleteAttribute( "3C37D459-E0A2-48CF-B3D4-5AEACD4ABA13" ); // Start Date
            DeleteAttribute( "1289E02A-862B-4B50-B07A-A625940480C0" ); // End Date
            DeleteBlock( "4898B608-4A08-4C1B-97A7-0500D708B5D2" ); // Create Pledge
            DeletePage( "60051DAF-2986-406D-A78B-1609CBF2256D" ); // (Public) Create Pledge
            DeleteBlockType( "20B5568E-A010-4E15-9127-E63CF218D6E5" ); // Finance - Create Pledge
            DeleteFieldType( "17033CDD-EF97-4413-A483-7B85A787A87F" );
            DeleteFieldType( "434D7B6F-F8DD-45B7-8C3E-C76EF10BE56A" );
            DeleteDefinedValue( "41540783-D9EF-4C70-8F1D-C9E83D91ED5F" );
            DeleteDefinedValue( "B91BA046-BC1E-400C-B85D-638C1F4E0CE2" );
            DeleteDefinedValue( "368DD475-242C-49C4-A42C-7278BE690CC2" );
            DeleteDefinedValue( "0250B035-7E2C-449F-AFE2-0FDD4A13560C" );
        }
    }
}
