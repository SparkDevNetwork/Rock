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
    public partial class SecurityRoleBlocksPages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddBlock( "48AAD428-A9C9-4BBB-A80F-B85F28D31240", "88B7EFA9-7419-4D05-9F88-38B936E61EDD", "Group Member List", "", "Content", 1, "E71D3062-286A-49D2-A0BB-84B385EFAD50" );

            AddPage( "48AAD428-A9C9-4BBB-A80F-B85F28D31240", "Group Member Detail", "", "Default", "45899E6A-7CEC-44EC-8DBA-BD8850262C04", "" );
            
            AddBlock( "45899E6A-7CEC-44EC-8DBA-BD8850262C04", "AAE2E5C3-9279-4AB0-9682-F4D19519D678", "Group Member Detail", "", "Content", 0, "07850B9E-D3D3-4E26-B579-83541645512D" );

            // Attrib for BlockType: Group List:Show GroupType
            AddBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show GroupType", "ShowGroupType", "", "", 0, "True", "5AF2A432-1A7A-4171-879E-F413D58039C1" );

            // Attrib for BlockType: Group List:Show IsSystem
            AddBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show IsSystem", "ShowIsSystem", "", "", 0, "True", "8A6E9BEF-F372-495D-816E-86E84E534DD6" );

            // Attrib Value for Group Member List:Detail Page
            AddBlockAttributeValue( "E71D3062-286A-49D2-A0BB-84B385EFAD50", "E4CCB79C-479F-4BEE-8156-969B2CE05973", "45899e6a-7cec-44ec-8dba-bd8850262c04" );

            // Attrib Value for Group Member List:Group
            AddBlockAttributeValue( "E71D3062-286A-49D2-A0BB-84B385EFAD50", "9F2D3674-B780-4CD3-B4AB-3DF3EA21905A", "0" );

            // Attrib Value for Groups:Show GroupType
            AddBlockAttributeValue( "52B774FE-9ABF-4852-9496-6FAD4646F949", "5AF2A432-1A7A-4171-879E-F413D58039C1", "False" );

            // Attrib Value for Groups:Show IsSystem
            AddBlockAttributeValue( "52B774FE-9ABF-4852-9496-6FAD4646F949", "8A6E9BEF-F372-495D-816E-86E84E534DD6", "False" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Group List:Show IsSystem
            DeleteAttribute( "8A6E9BEF-F372-495D-816E-86E84E534DD6" );
            // Attrib for BlockType: Group List:Show GroupType
            DeleteAttribute( "5AF2A432-1A7A-4171-879E-F413D58039C1" );

            DeleteBlock( "07850B9E-D3D3-4E26-B579-83541645512D" ); // Group Member Detail
            DeleteBlock( "E71D3062-286A-49D2-A0BB-84B385EFAD50" ); // Group Member List
            DeletePage( "45899E6A-7CEC-44EC-8DBA-BD8850262C04" ); // Group Member Detail
        }
    }
}
