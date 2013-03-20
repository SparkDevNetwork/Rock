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
    public partial class PledgeBlocks : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "142627AE-6590-48E3-BFCA-3669260B8CF2", "Pledge List", "List of all pledges in the system", "Default", "1570D2AF-4FE2-4FC7-BED9-F20EBCBE9867" );
            AddPage( "1570D2AF-4FE2-4FC7-BED9-F20EBCBE9867", "Pledge Detail", "Details of a given pledge.", "Default", "EF7AA296-CA69-49BC-A28B-901A8AAA9466" );
            AddBlockType( "Finance - Administration - Pledge List", "Generic list of all Pledges in the system", "~/Blocks/Finance/Administration/PledgeList.ascx", "7011E792-A75F-4F22-B17E-D3A58C0EDB6D" );
            AddBlockType( "Finance - Administration - Pledge Detail", "Details of a given pledge", "~/Blocks/Finance/Administration/PledgeDetail.ascx", "E08504ED-B84C-4115-A058-03AAB8E8A307" );
            AddBlock( "1570D2AF-4FE2-4FC7-BED9-F20EBCBE9867", "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "Pledge List", "", "Content", 0, "ABEE9BA4-55E4-435E-8CA2-7D1626C57847" );
            AddBlock( "EF7AA296-CA69-49BC-A28B-901A8AAA9466", "E08504ED-B84C-4115-A058-03AAB8E8A307", "Pledge Detail", "", "Content", 0, "1FA87E65-1731-483C-A5DC-7DB8336CEED8" );
            AddBlockTypeAttribute( "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "", "", 0, "", "3E26B7DF-7A7F-4829-987F-47304C0F845E" );
            // Attrib Value for Pledge List:Detail Page Guid
            AddBlockAttributeValue("ABEE9BA4-55E4-435E-8CA2-7D1626C57847","3E26B7DF-7A7F-4829-987F-47304C0F845E","ef7aa296-ca69-49bc-a28b-901a8aaa9466");

            AddDefinedType( "Financial", "Frequency Type", "Types of payment frequencies", "059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F" );
            AddDefinedValue( "059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F", "Weekly", "Every Week", "53957842-DE28-498C-AC61-65B32E8034CB" );
            AddDefinedValue( "059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F", "Bi-Weekly", "Every Two Weeks", "FBD9315C-5E0B-49D8-9D28-27EBF268E67B" );
            AddDefinedValue( "059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F", "Monthly", "Once a Month", "C53509B1-FC2B-46C8-A00E-58392FBE9408" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "3E26B7DF-7A7F-4829-987F-47304C0F845E" ); // Detail Page Guid
            DeleteBlock( "ABEE9BA4-55E4-435E-8CA2-7D1626C57847" ); // Pledge List
            DeleteBlock( "1FA87E65-1731-483C-A5DC-7DB8336CEED8" ); // Pledge Detail
            DeleteBlockType( "7011E792-A75F-4F22-B17E-D3A58C0EDB6D" ); // Finance - Administration - Pledge List
            DeleteBlockType( "E08504ED-B84C-4115-A058-03AAB8E8A307" ); // Finance - Administration - Pledge Detail
            DeletePage( "1570D2AF-4FE2-4FC7-BED9-F20EBCBE9867" ); // Pledge List
            DeletePage( "EF7AA296-CA69-49BC-A28B-901A8AAA9466" ); // Pledge Detail

            DeleteDefinedType( "059F69C0-BF9B-4D53-B7CD-2D3B7C647C5F" );
        }
    }
}
