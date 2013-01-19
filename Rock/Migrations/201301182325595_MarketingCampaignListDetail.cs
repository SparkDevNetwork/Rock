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
    public partial class MarketingCampaignListDetail : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // remove old MarketingCampaigns Block
            Sql( @"
delete from Block where Guid = 'C26C8EB3-0BF3-4D5E-A685-BC6C9B9246D8'
delete from BlockType where Guid = 'D99313E1-EFF0-4339-8296-4FA4922B48B7'
" );

            AddPage( "74345663-5BCA-493C-A2FB-80DC9CC8E70C", "Ad Campaign Detail", "", "Default", "64521E8E-3BA7-409C-A18F-4ACAAC6758CE" );
            AddPage( "B4A24AB7-9369-4055-883F-4F4892C39AE3", "Events", "Manage Events", "Default", "0D5796D3-A486-46EC-9E98-111646DB2CFF" );
            AddPage( "0D5796D3-A486-46EC-9E98-111646DB2CFF", "Event Detail", "", "Default", "D8365E1D-1FDA-404E-88A1-843E5684F457" );

            AddBlockType( "Administration - Marketing Campaign Detail", "", "~/Blocks/Administration/MarketingCampaignDetail.ascx", "841A0529-31A6-4065-854F-E52C516B9237" );
            AddBlockType( "Administration - Marketing Campaign List", "", "~/Blocks/Administration/MarketingCampaignList.ascx", "F6592890-1FD0-4BF4-AC57-F71FA900FCB5" );

            AddBlock( "74345663-5BCA-493C-A2FB-80DC9CC8E70C", "F6592890-1FD0-4BF4-AC57-F71FA900FCB5", "Marketing Campaigns", "", "Content", 0, "0B3C8E47-6E91-4A9E-BB1C-ECEEA3FC42C8" );
            AddBlock( "64521E8E-3BA7-409C-A18F-4ACAAC6758CE", "841A0529-31A6-4065-854F-E52C516B9237", "Marketing Campaign Detail", "", "Content", 0, "045E23A5-138F-4DF1-B0B0-C28745875E10" );
            AddBlock( "0D5796D3-A486-46EC-9E98-111646DB2CFF", "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "Event List", "", "Content", 0, "A007FDC8-F3DE-457E-9097-68CD1355A454" );
            AddBlock( "D8365E1D-1FDA-404E-88A1-843E5684F457", "582BEEA1-5B27-444D-BC0A-F60CEB053981", "Event Detail", "", "Content", 0, "68D40E7D-EF38-4B53-8BD7-1D798F1C5B22" );

            AddBlockTypeAttribute( "F6592890-1FD0-4BF4-AC57-F71FA900FCB5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "D6D1AD4A-8467-44E9-8F24-FE7AA64D48EE" );
            AddBlockTypeAttribute( "841A0529-31A6-4065-854F-E52C516B9237", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Event Page", "EventPageGuid", "Advanced", "", 0, "", "3BEA9889-6C0D-4059-AA7E-554621B41649" );

            // Attrib Value for Event List:Limit to Security Role Groups
            AddBlockAttributeValue( "A007FDC8-F3DE-457E-9097-68CD1355A454", "1DAD66E3-8859-487E-8200-483C98DE2E07", "False" );
            // Attrib Value for Event List:Group Types
            AddBlockAttributeValue( "A007FDC8-F3DE-457E-9097-68CD1355A454", "C3FD6CE3-D37F-4A53-B0D7-AB1B1F252324", "13" );
            // Attrib Value for Event List:Detail Page Guid
            AddBlockAttributeValue( "A007FDC8-F3DE-457E-9097-68CD1355A454", "8E57EC42-ABEE-4D35-B7FA-D8513880E8E4", "d8365e1d-1fda-404e-88a1-843e5684f457" );
            // Attrib Value for Event List:Show User Count
            AddBlockAttributeValue( "A007FDC8-F3DE-457E-9097-68CD1355A454", "D7A5D717-6B3F-4033-B707-B92D81D402C2", "True" );
            // Attrib Value for Event List:Show Description
            AddBlockAttributeValue( "A007FDC8-F3DE-457E-9097-68CD1355A454", "99AF141C-8F5F-4FB8-8748-837A4BFCFB94", "True" );
            // Attrib Value for Event List:Show Edit
            AddBlockAttributeValue( "A007FDC8-F3DE-457E-9097-68CD1355A454", "0EC725C5-F6F7-47DC-ABC2-8A59B6033F45", "True" );
            // Attrib Value for Event List:Show Notification
            AddBlockAttributeValue( "A007FDC8-F3DE-457E-9097-68CD1355A454", "D5B9A3DB-DD94-4B7C-A784-28BA691181E0", "False" );
            // Attrib Value for Marketing Campaigns:Detail Page Guid
            AddBlockAttributeValue( "0B3C8E47-6E91-4A9E-BB1C-ECEEA3FC42C8", "D6D1AD4A-8467-44E9-8F24-FE7AA64D48EE", "64521e8e-3ba7-409c-a18f-4acaac6758ce" );
            // Attrib Value for Marketing Campaign Detail:Event Page
            AddBlockAttributeValue( "045E23A5-138F-4DF1-B0B0-C28745875E10", "3BEA9889-6C0D-4059-AA7E-554621B41649", "d8365e1d-1fda-404e-88a1-843e5684f457" );
            // Attrib Value for Event Detail:Group Types
            AddBlockAttributeValue( "68D40E7D-EF38-4B53-8BD7-1D798F1C5B22", "15AC7A62-7BF2-44B7-93CD-EA8F96BF529A", "13" );
            // Attrib Value for Event Detail:Show Edit
            AddBlockAttributeValue( "68D40E7D-EF38-4B53-8BD7-1D798F1C5B22", "50C7E223-459E-4A1C-AE3C-2892CBD40D22", "True" );
            // Attrib Value for Event Detail:Limit to Security Role Groups
            AddBlockAttributeValue( "68D40E7D-EF38-4B53-8BD7-1D798F1C5B22", "12295C7E-08F4-4AC5-8A34-C829620FC0B1", "False" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "D6D1AD4A-8467-44E9-8F24-FE7AA64D48EE" );
            DeleteAttribute( "3BEA9889-6C0D-4059-AA7E-554621B41649" );
            DeleteBlock( "0B3C8E47-6E91-4A9E-BB1C-ECEEA3FC42C8" );
            DeleteBlock( "045E23A5-138F-4DF1-B0B0-C28745875E10" );
            DeleteBlock( "A007FDC8-F3DE-457E-9097-68CD1355A454" );
            DeleteBlock( "68D40E7D-EF38-4B53-8BD7-1D798F1C5B22" );
            DeleteBlockType( "841A0529-31A6-4065-854F-E52C516B9237" );
            DeleteBlockType( "F6592890-1FD0-4BF4-AC57-F71FA900FCB5" );

            DeletePage( "D8365E1D-1FDA-404E-88A1-843E5684F457" );
            DeletePage( "64521E8E-3BA7-409C-A18F-4ACAAC6758CE" );
            DeletePage( "0D5796D3-A486-46EC-9E98-111646DB2CFF" );
        }
    }
}
