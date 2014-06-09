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
            //////DONE////////

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
