//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace com.ccvonline.Residency.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class GradeBook : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage("82B81403-8A93-4F42-A958-5303C3AF1508","Reports","","Default","5C25CE3F-9AA2-4760-9AC5-0B09350380E9","");
            AddPage("5C25CE3F-9AA2-4760-9AC5-0B09350380E9","Grade Book","","Default","64FB3328-D209-4518-94CD-6FEC6BCB1D85","");
            AddPage("64FB3328-D209-4518-94CD-6FEC6BCB1D85","Residents","","Default","F650B68F-DE1E-4952-9A4D-05D8A6B3F51C","");
            AddPage("F650B68F-DE1E-4952-9A4D-05D8A6B3F51C","Resident Grade Book","","Default","9A3A80AA-A9B0-4824-B81D-68F070131E92","");

            AddBlockType("com_ccvonline - Residency - Competency Person Assessment Report","","~/Plugins/com_ccvonline/Residency/CompetencyPersonAssessmentReport.ascx","0F4672C8-7475-4061-9F6D-13E48193819E");
            
            // Add Block to Page: Grade Book
            AddBlock("64FB3328-D209-4518-94CD-6FEC6BCB1D85","3D7FB6BE-6BBD-49F7-96B4-96310AF3048A","Groups","","Content",0,"E4E626BD-D100-40E3-A6C8-4D68EC40C2F8");

            // Add Block to Page: Residents
            AddBlock("F650B68F-DE1E-4952-9A4D-05D8A6B3F51C","13EE0E6A-BBC6-4D86-9226-E246CFBA11B2","Residency Person List","","Content",0,"169C1FE4-938B-46FC-85EC-EF0EFCD26BD5");

            // Add Block to Page: Resident Grade Book
            AddBlock("9A3A80AA-A9B0-4824-B81D-68F070131E92","0F4672C8-7475-4061-9F6D-13E48193819E","Competency Person Assessment Report","","Content",2,"9E6BA93D-694E-4578-9A85-E0DC32D39470");

            // Attrib Value for Block:Groups, Attribute:Detail Page, Page:Grade Book
            AddBlockAttributeValue("E4E626BD-D100-40E3-A6C8-4D68EC40C2F8","8E57EC42-ABEE-4D35-B7FA-D8513880E8E4","f650b68f-de1e-4952-9a4d-05d8a6b3f51c");

            // Attrib Value for Block:Groups, Attribute:Show User Count, Page:Grade Book
            AddBlockAttributeValue("E4E626BD-D100-40E3-A6C8-4D68EC40C2F8","D7A5D717-6B3F-4033-B707-B92D81D402C2","True");

            // Attrib Value for Block:Groups, Attribute:Show Description, Page:Grade Book
            AddBlockAttributeValue("E4E626BD-D100-40E3-A6C8-4D68EC40C2F8","99AF141C-8F5F-4FB8-8748-837A4BFCFB94","False");

            // Attrib Value for Block:Groups, Attribute:Show Edit, Page:Grade Book
            AddBlockAttributeValue("E4E626BD-D100-40E3-A6C8-4D68EC40C2F8","0EC725C5-F6F7-47DC-ABC2-8A59B6033F45","True");

            // Attrib Value for Block:Groups, Attribute:Limit to Security Role Groups, Page:Grade Book
            AddBlockAttributeValue("E4E626BD-D100-40E3-A6C8-4D68EC40C2F8","1DAD66E3-8859-487E-8200-483C98DE2E07","False");

            // Attrib Value for Block:Groups, Attribute:Group Types, Page:Grade Book
            AddBlockAttributeValue("E4E626BD-D100-40E3-A6C8-4D68EC40C2F8","C3FD6CE3-D37F-4A53-B0D7-AB1B1F252324","00043ce6-eb1b-43b5-a12a-4552b91a3e28");

            // Attrib Value for Block:Groups, Attribute:Show Notification, Page:Grade Book
            AddBlockAttributeValue("E4E626BD-D100-40E3-A6C8-4D68EC40C2F8","D5B9A3DB-DD94-4B7C-A784-28BA691181E0","False");

            // Attrib Value for Block:Groups, Attribute:Show GroupType, Page:Grade Book
            // NOTE: due to bug in Rock Core, this attribute might not exist, so we'll have to set it manually
            //AddBlockAttributeValue("E4E626BD-D100-40E3-A6C8-4D68EC40C2F8","5AF2A432-1A7A-4171-879E-F413D58039C1","False");

            // Attrib Value for Block:Groups, Attribute:Show IsSystem, Page:Grade Book
            // NOTE: due to bug in Rock Core, this attribute might not exist, so we'll have to set it manually
            //AddBlockAttributeValue("E4E626BD-D100-40E3-A6C8-4D68EC40C2F8","8A6E9BEF-F372-495D-816E-86E84E534DD6","False");

            // Attrib for BlockType: com_ccvonline - Residency Person List:Show Add
            AddBlockTypeAttribute( "13EE0E6A-BBC6-4D86-9226-E246CFBA11B2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Add", "ShowAdd", "", "", 0, "True", "9C78BE0D-3199-4B3C-AA35-B43535E70B10" );

            // Attrib for BlockType: com_ccvonline - Residency Person List:Show Delete
            AddBlockTypeAttribute( "13EE0E6A-BBC6-4D86-9226-E246CFBA11B2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Delete", "ShowDelete", "", "", 0, "True", "D55E1E71-9AD2-4B9D-A99B-CD50C9238003" );

            // Attrib Value for Block:Residency Person List, Attribute:Show Add, Page:Residents
            AddBlockAttributeValue( "169C1FE4-938B-46FC-85EC-EF0EFCD26BD5", "9C78BE0D-3199-4B3C-AA35-B43535E70B10", "False" );

            // Attrib Value for Block:Residency Person List, Attribute:Show Delete, Page:Residents
            AddBlockAttributeValue( "169C1FE4-938B-46FC-85EC-EF0EFCD26BD5", "D55E1E71-9AD2-4B9D-A99B-CD50C9238003", "False" );

            // Attrib Value for Block:Residency Person List, Attribute:Detail Page, Page:Residents
            AddBlockAttributeValue("169C1FE4-938B-46FC-85EC-EF0EFCD26BD5","7B1CA61D-0E1A-44C9-9210-DF49D29EECF0","9a3a80aa-a9b0-4824-b81d-68f070131e92");

            // Update new grade book page to use Residency Site
            Sql( @"
DECLARE @SiteId int

SET @SiteId = (select [Id] from [Site] where [Guid] = '960F1D98-891A-4508-8F31-3CF206F5406E')

-- Update new grade book page to use new site (default layout)
UPDATE [Page] SET 
    [SiteId] = @SiteId,
    [Layout] = 'Default'
WHERE [Guid] in (
'9A3A80AA-A9B0-4824-B81D-68F070131E92')

" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: com_ccvonline - Residency Person List:Show Delete
            DeleteAttribute( "D55E1E71-9AD2-4B9D-A99B-CD50C9238003" );
            // Attrib for BlockType: com_ccvonline - Residency Person List:Show Add
            DeleteAttribute( "9C78BE0D-3199-4B3C-AA35-B43535E70B10" );
            // Remove Block: Competency Person Assessment Report, from Page: Resident Grade Book
            DeleteBlock("9E6BA93D-694E-4578-9A85-E0DC32D39470");
            // Remove Block: Residency Person List, from Page: Residents
            DeleteBlock("169C1FE4-938B-46FC-85EC-EF0EFCD26BD5");
            // Remove Block: Groups, from Page: Grade Book
            DeleteBlock("E4E626BD-D100-40E3-A6C8-4D68EC40C2F8");
            DeleteBlockType("0F4672C8-7475-4061-9F6D-13E48193819E"); // com _ccvonline - Residency - Competency Person Assessment Report
            DeletePage("9A3A80AA-A9B0-4824-B81D-68F070131E92"); // Resident Grade Book
            DeletePage("F650B68F-DE1E-4952-9A4D-05D8A6B3F51C"); // Residents
            DeletePage("64FB3328-D209-4518-94CD-6FEC6BCB1D85"); // Grade Book
            DeletePage("5C25CE3F-9AA2-4760-9AC5-0B09350380E9"); // Reports
        }
    }
}
