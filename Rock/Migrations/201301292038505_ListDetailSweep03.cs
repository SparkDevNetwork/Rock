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
    public partial class ListDetailSweep03 : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // delete old GroupRoles and PageRoutes blocks
            Sql( @"
delete from [Block] where [Guid] = '1064932B-F0DB-4F39-B438-24703A14198B'
delete from [BlockType] where [Guid] = '89315EBC-D4BD-41E6-B1F1-929D19E66608'
                
delete from [Block] where [Guid] = '09DC13AF-8BF8-4A65-B3DF-77F17C5650D6'
delete from [BlockType] where [Guid] = 'FEE08A28-B774-4294-9F77-697FE66CA5B5'
" 
                );

            AddPage( "BBD61BB9-7BE0-4F16-9615-91D38F3AE9C9", "Group Role Detail", "", "Default", "29BAFA8E-F05A-4468-B2CE-473EEDE7C532" );
            AddPage( "4A833BE3-7D5E-4C38-AF60-5706260015EA", "Page Route Detail", "", "Default", "649A2B1E-7A15-4DA8-AF67-17874B6FE98F" );

            AddBlockType( "Crm - Group Role Detail", "", "~/Blocks/Crm/GroupRoleDetail.ascx", "FAE8AC76-0AF4-4A64-BDF6-FEBE857A74D2" );
            AddBlockType( "Crm - Group Role List", "", "~/Blocks/Crm/GroupRoleList.ascx", "CC3F3EBE-4120-4612-BB02-C79D3604CA99" );
            AddBlockType( "Administration - Page Route Detail", "", "~/Blocks/Administration/PageRouteDetail.ascx", "E6E7333A-C4A6-4DE7-9A37-CC2641320C98" );
            AddBlockType( "Administration - Page Route List", "", "~/Blocks/Administration/PageRouteList.ascx", "E92E3C51-EB14-414D-BC68-9061FEB92A22" );

            AddBlock( "BBD61BB9-7BE0-4F16-9615-91D38F3AE9C9", "CC3F3EBE-4120-4612-BB02-C79D3604CA99", "Group Role List", "", "Content", 0, "7271BD7E-573C-4B72-9AAD-2BB7CDAF9DAA" );
            AddBlock( "29BAFA8E-F05A-4468-B2CE-473EEDE7C532", "FAE8AC76-0AF4-4A64-BDF6-FEBE857A74D2", "Group Role Detail", "", "Content", 0, "F8B0744A-F48C-4965-9C1D-29B548D8974C" );
            AddBlock( "4A833BE3-7D5E-4C38-AF60-5706260015EA", "E92E3C51-EB14-414D-BC68-9061FEB92A22", "Page Route List", "", "Content", 0, "2F1FFF84-E19D-4F2C-9747-8A8B80E376B0" );
            AddBlock( "649A2B1E-7A15-4DA8-AF67-17874B6FE98F", "E6E7333A-C4A6-4DE7-9A37-CC2641320C98", "Page Route Detail", "", "Content", 0, "E01EE6C9-C045-45D4-940A-A2E6BBE8CF00" );

            AddBlockTypeAttribute( "CC3F3EBE-4120-4612-BB02-C79D3604CA99", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "587C4958-65EF-4D1F-AAB4-B264DB203D93" );
            AddBlockTypeAttribute( "E92E3C51-EB14-414D-BC68-9061FEB92A22", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "204869E9-6D4C-4C33-A44D-F711D2EF0378" );

            // Attrib Value for Group Role List:Detail Page Guid
            AddBlockAttributeValue( "7271BD7E-573C-4B72-9AAD-2BB7CDAF9DAA", "587C4958-65EF-4D1F-AAB4-B264DB203D93", "29bafa8e-f05a-4468-b2ce-473eede7c532" );
            // Attrib Value for Page Route List:Detail Page Guid
            AddBlockAttributeValue( "2F1FFF84-E19D-4F2C-9747-8A8B80E376B0", "204869E9-6D4C-4C33-A44D-F711D2EF0378", "649a2b1e-7a15-4da8-af67-17874b6fe98f" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "587C4958-65EF-4D1F-AAB4-B264DB203D93" );
            DeleteAttribute( "204869E9-6D4C-4C33-A44D-F711D2EF0378" );
            DeleteBlock( "7271BD7E-573C-4B72-9AAD-2BB7CDAF9DAA" );
            DeleteBlock( "F8B0744A-F48C-4965-9C1D-29B548D8974C" );
            DeleteBlock( "2F1FFF84-E19D-4F2C-9747-8A8B80E376B0" );
            DeleteBlock( "E01EE6C9-C045-45D4-940A-A2E6BBE8CF00" );
            DeleteBlockType( "43BFD4FE-A7F6-4F65-8B98-3B162DA7BF9B" );
            DeleteBlockType( "FAE8AC76-0AF4-4A64-BDF6-FEBE857A74D2" );
            DeleteBlockType( "CC3F3EBE-4120-4612-BB02-C79D3604CA99" );
            DeleteBlockType( "E6E7333A-C4A6-4DE7-9A37-CC2641320C98" );
            DeleteBlockType( "E92E3C51-EB14-414D-BC68-9061FEB92A22" );
            DeletePage( "29BAFA8E-F05A-4468-B2CE-473EEDE7C532" );
            DeletePage( "649A2B1E-7A15-4DA8-AF67-17874B6FE98F" );
        }
    }
}
