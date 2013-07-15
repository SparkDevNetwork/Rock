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
    public partial class NoteBlockShowOptionsAttributes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Notes:Show Private Checkbox
            AddBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Private Checkbox", "ShowPrivateCheckbox", "", "", 0, "True", "D68EE1F5-D29F-404B-945D-AD0BE76594C3" );

            // Attrib for BlockType: Notes:Show Security Button
            AddBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Security Button", "ShowSecurityButton", "", "", 0, "True", "00B6EBFF-786D-453E-8746-119D0B45CB3E" );

            // Attrib for BlockType: Notes:Show Alert Checkbox
            AddBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Alert Checkbox", "ShowAlertCheckbox", "", "", 0, "True", "20243A98-4802-48E2-AF61-83956056AC65" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Notes:Show Alert Checkbox
            DeleteAttribute( "20243A98-4802-48E2-AF61-83956056AC65" );
            
            // Attrib for BlockType: Notes:Show Security Button
            DeleteAttribute( "00B6EBFF-786D-453E-8746-119D0B45CB3E" );
            
            // Attrib for BlockType: Notes:Show Private Checkbox
            DeleteAttribute( "D68EE1F5-D29F-404B-945D-AD0BE76594C3" );
        }
    }
}
