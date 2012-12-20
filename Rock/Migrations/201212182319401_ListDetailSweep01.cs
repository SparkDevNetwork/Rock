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
    public partial class ListDetailSweep01 : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Old Campus Block
            DeleteBlock( "CB71352B-C10B-453A-8879-0EFF8707355A" );
            DeleteBlockType( "0D0DC731-E282-44EA-AD1E-89D16AB20192" );

            // Old ScheduledJob Block
            DeleteBlock( "01727331-4F96-43CD-8585-791B35D86487" );
            DeleteBlockType( "ED2063B5-9839-46D1-8419-FE36D3B54708" );

            // Old Sites Block
            DeleteBlock( "B31AE932-F065-4500-8524-2182431CD18C" );
            DeleteBlockType( "3E0AFD6E-3E3D-4FF9-9BC6-387AFBF9ACB6" );

            // Old GroupTypes Block
            DeleteBlock( "F3B2FC30-5ACE-4D1D-87F3-9712723D903F" );
            DeleteBlockType( "C443D72B-1A9E-41E7-8E70-4E9D39AE6AC3" );

            AddPage( "5EE91A54-C750-48DC-9392-F1F0F0581C3A", "Campus Detail", "", "Default", "BDD7B906-4D42-43C0-8DBB-B89A566734D8" );
            AddPage( "C58ADA1A-6322-4998-8FED-C3565DE87EFA", "Scheduled Job Detail", "", "Default", "E18AC09D-45CD-49CF-8874-157B32556B7D" );
            AddPage( "7596D389-4EAB-4535-8BEE-229737F46F44", "Site Detail", "", "Default", "A2991117-0B85-4209-9008-254929C6E00F" );
            AddPage( "40899BCD-82B0-47F2-8F2A-B6AA3877B445", "Group Type Detail", "", "Default", "5CD8E024-710B-4EDE-8C8C-4C9E15E6AFAB" );

            AddBlockType( "Campus Detail", "~/Blocks/Administration/CampusDetail.ascx", "~/Blocks/Administration/CampusDetail.ascx", "E30354A1-A1B8-4BE5-ADCE-43EEDDEF6C65" );
            AddBlockType( "Campus List", "~/Blocks/Administration/CampusList.ascx", "~/Blocks/Administration/CampusList.ascx", "C93D614A-6EBC-49A1-A80D-F3677D2B86A0" );
            AddBlockType( "Scheduled Job Detail", "~/Blocks/Administration/ScheduledJobDetail.ascx", "~/Blocks/Administration/ScheduledJobDetail.ascx", "C5EC90C9-26C4-493A-84AC-4B5DEF9EA472" );
            AddBlockType( "Scheduled Job List", "~/Blocks/Administration/ScheduledJobList.ascx", "~/Blocks/Administration/ScheduledJobList.ascx", "6D3F924E-BDD0-4C78-981E-B698351E75AD" );
            AddBlockType( "Site Detail", "~/Blocks/Administration/SiteDetail.ascx", "~/Blocks/Administration/SiteDetail.ascx", "2AC06C36-869F-45F7-8C14-802781C5F70E" );
            AddBlockType( "Site List", "~/Blocks/Administration/SiteList.ascx", "~/Blocks/Administration/SiteList.ascx", "441D5A71-C250-4FF5-90C3-DEEAD3AC028D" );
            AddBlockType( "Group Type Detail", "~/Blocks/Crm/GroupTypeDetail.ascx", "~/Blocks/Crm/GroupTypeDetail.ascx", "78B8EE69-71A7-43C1-B00B-ED13828FE104" );
            AddBlockType( "Group Type List", "~/Blocks/Crm/GroupTypeList.ascx", "~/Blocks/Crm/GroupTypeList.ascx", "80306BB1-FE4B-436F-AC7A-691CF0BC0F5E" );

            AddBlock( "5EE91A54-C750-48DC-9392-F1F0F0581C3A", "C93D614A-6EBC-49A1-A80D-F3677D2B86A0", "Campus List", "Content", "B84A7CFD-1E1E-4FDF-8342-9EE1ECCE56F8", 0 );
            AddBlock( "BDD7B906-4D42-43C0-8DBB-B89A566734D8", "E30354A1-A1B8-4BE5-ADCE-43EEDDEF6C65", "Campus Detail", "Content", "176FFC6F-6B55-4319-A781-A2F7F1F85F24", 0 );
            AddBlock( "C58ADA1A-6322-4998-8FED-C3565DE87EFA", "6D3F924E-BDD0-4C78-981E-B698351E75AD", "Scheduled Job List", "Content", "191F7008-38D7-409B-A2AF-B48A340A7C78", 0 );
            AddBlock( "E18AC09D-45CD-49CF-8874-157B32556B7D", "C5EC90C9-26C4-493A-84AC-4B5DEF9EA472", "Scheduled Job Detail", "Content", "2C710931-8FC0-41F2-8F08-8E5C84F1858E", 0 );
            AddBlock( "7596D389-4EAB-4535-8BEE-229737F46F44", "441D5A71-C250-4FF5-90C3-DEEAD3AC028D", "Site List", "Content", "9EAAC77C-3B75-428E-A393-F51B2D29097F", 0 );
            AddBlock( "A2991117-0B85-4209-9008-254929C6E00F", "2AC06C36-869F-45F7-8C14-802781C5F70E", "Site Detail", "Content", "FC3EBB17-B73E-41C7-8A0D-ACE3FCFDB40A", 0 );
            AddBlock( "40899BCD-82B0-47F2-8F2A-B6AA3877B445", "80306BB1-FE4B-436F-AC7A-691CF0BC0F5E", "Group Type List", "Content", "24332630-10F5-4B09-90B5-0958660C6540", 0 );
            AddBlock( "5CD8E024-710B-4EDE-8C8C-4C9E15E6AFAB", "78B8EE69-71A7-43C1-B00B-ED13828FE104", "Group Type Detail", "Content", "308CFDEC-374F-491D-AFE3-5F08DF7DBB8E", 0 );

            AddBlockTypeAttribute( "C93D614A-6EBC-49A1-A80D-F3677D2B86A0", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "2DD44E0D-BC55-447E-8707-EA7994712D17" );
            AddBlockTypeAttribute( "6D3F924E-BDD0-4C78-981E-B698351E75AD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "98EC3F61-745C-483C-BD53-F2EA2A589380" );
            AddBlockTypeAttribute( "441D5A71-C250-4FF5-90C3-DEEAD3AC028D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "BE7EEE0F-9B2B-4CF5-8714-29166025B3DD" );
            AddBlockTypeAttribute( "80306BB1-FE4B-436F-AC7A-691CF0BC0F5E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "54731F4A-041F-47BD-B908-91ECC592D679" );

            // Attrib Value for Campus List:Detail Page Guid
            AddBlockAttributeValue( "B84A7CFD-1E1E-4FDF-8342-9EE1ECCE56F8", "2DD44E0D-BC55-447E-8707-EA7994712D17", "bdd7b906-4d42-43c0-8dbb-b89a566734d8" );
            // Attrib Value for Scheduled Job List:Detail Page Guid
            AddBlockAttributeValue( "191F7008-38D7-409B-A2AF-B48A340A7C78", "98EC3F61-745C-483C-BD53-F2EA2A589380", "e18ac09d-45cd-49cf-8874-157b32556b7d" );
            // Attrib Value for Site List:Detail Page Guid
            AddBlockAttributeValue( "9EAAC77C-3B75-428E-A393-F51B2D29097F", "BE7EEE0F-9B2B-4CF5-8714-29166025B3DD", "a2991117-0b85-4209-9008-254929c6e00f" );
            // Attrib Value for Group Type List:Detail Page Guid
            AddBlockAttributeValue( "24332630-10F5-4B09-90B5-0958660C6540", "54731F4A-041F-47BD-B908-91ECC592D679", "5cd8e024-710b-4ede-8c8c-4c9e15e6afab" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "2DD44E0D-BC55-447E-8707-EA7994712D17" );
            DeleteAttribute( "98EC3F61-745C-483C-BD53-F2EA2A589380" );
            DeleteAttribute( "BE7EEE0F-9B2B-4CF5-8714-29166025B3DD" );
            DeleteAttribute( "54731F4A-041F-47BD-B908-91ECC592D679" );
            DeleteBlock( "B84A7CFD-1E1E-4FDF-8342-9EE1ECCE56F8" );
            DeleteBlock( "176FFC6F-6B55-4319-A781-A2F7F1F85F24" );
            DeleteBlock( "191F7008-38D7-409B-A2AF-B48A340A7C78" );
            DeleteBlock( "2C710931-8FC0-41F2-8F08-8E5C84F1858E" );
            DeleteBlock( "9EAAC77C-3B75-428E-A393-F51B2D29097F" );
            DeleteBlock( "FC3EBB17-B73E-41C7-8A0D-ACE3FCFDB40A" );
            DeleteBlock( "24332630-10F5-4B09-90B5-0958660C6540" );
            DeleteBlock( "308CFDEC-374F-491D-AFE3-5F08DF7DBB8E" );
            DeleteBlockType( "E30354A1-A1B8-4BE5-ADCE-43EEDDEF6C65" );
            DeleteBlockType( "C93D614A-6EBC-49A1-A80D-F3677D2B86A0" );
            DeleteBlockType( "C5EC90C9-26C4-493A-84AC-4B5DEF9EA472" );
            DeleteBlockType( "6D3F924E-BDD0-4C78-981E-B698351E75AD" );
            DeleteBlockType( "2AC06C36-869F-45F7-8C14-802781C5F70E" );
            DeleteBlockType( "441D5A71-C250-4FF5-90C3-DEEAD3AC028D" );
            DeleteBlockType( "78B8EE69-71A7-43C1-B00B-ED13828FE104" );
            DeleteBlockType( "80306BB1-FE4B-436F-AC7A-691CF0BC0F5E" );
            DeletePage( "BDD7B906-4D42-43C0-8DBB-B89A566734D8" );
            DeletePage( "E18AC09D-45CD-49CF-8874-157B32556B7D" );
            DeletePage( "A2991117-0B85-4209-9008-254929C6E00F" );
            DeletePage( "5CD8E024-710B-4EDE-8C8C-4C9E15E6AFAB" );
        }
    }
}
