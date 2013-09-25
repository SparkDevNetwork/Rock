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
    public partial class RefactorMetrics : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DeleteBlock( "CCD4F459-2E0A-40C6-8DE3-AD512AE9CA74" );
            
            AddPage("84DB9BA0-2725-40A5-A3CA-9A1C043C31B0","Metric Detail","","Default","574544A8-4831-4DAB-8BCE-B2C9B3D188AF","");      
            AddBlockType("Administration - Metric List","","~/Blocks/Administration/MetricList.ascx","AE70E6C4-A34C-4D05-A13C-CE0ABE2A6B5B");
            AddBlockType("Administration - Metric Detail","","~/Blocks/Administration/MetricDetail.ascx","CE769F0A-722F-4745-A6A8-7F00548CD1D2");            
            AddBlockType("Administration - Metric Value List","","~/Blocks/Administration/MetricValueList.ascx","819C3597-4A93-4974-B1E9-4D065989EA25");
           
            // Add Block to Page: Metrics
            AddBlock( "84DB9BA0-2725-40A5-A3CA-9A1C043C31B0", "AE70E6C4-A34C-4D05-A13C-CE0ABE2A6B5B", "Metric List", "", "Content", 0, "9126CFA2-9B26-4FBB-BB87-F76514221DBE" );

            // Add Block to Page: Metric Detail
            AddBlock("574544A8-4831-4DAB-8BCE-B2C9B3D188AF","CE769F0A-722F-4745-A6A8-7F00548CD1D2","Metric Detail","","Content",0,"816B856A-FDC8-4832-88E7-FA330AFC5D6E");

            // Add Block to Page: Metric Detail
            AddBlock("574544A8-4831-4DAB-8BCE-B2C9B3D188AF","819C3597-4A93-4974-B1E9-4D065989EA25","Metric Value","","Content",1,"1B9850ED-CD78-47E2-A076-A3BA5D8808EC");

            // Attrib for BlockType: Administration - Metric List:Detail Page
            AddBlockTypeAttribute("AE70E6C4-A34C-4D05-A13C-CE0ABE2A6B5B","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Detail Page","DetailPage","","",0,"","A94901D2-F91B-49D7-84EE-F046F3DAF144");
            
            // Attrib Value for Block:Metrics, Attribute:Detail Page, Page:Metrics
            AddBlockAttributeValue("9126CFA2-9B26-4FBB-BB87-F76514221DBE","A94901D2-F91B-49D7-84EE-F046F3DAF144","574544a8-4831-4dab-8bce-b2c9b3d188af");
 
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Administration - Metric List:Detail Page
            DeleteAttribute("A94901D2-F91B-49D7-84EE-F046F3DAF144");
            // Remove Block: Metric Value, from Page: Metric Detail
            DeleteBlock("1B9850ED-CD78-47E2-A076-A3BA5D8808EC");
            // Remove Block: Metric Detail, from Page: Metric Detail
            DeleteBlock("816B856A-FDC8-4832-88E7-FA330AFC5D6E");
            // Remove Block: Metric List, from Page: Metrics
            DeleteBlock("9126CFA2-9B26-4FBB-BB87-F76514221DBE");
            
            DeleteBlockType("819C3597-4A93-4974-B1E9-4D065989EA25"); // Administration - Metric Value List
            DeleteBlockType("AE70E6C4-A34C-4D05-A13C-CE0ABE2A6B5B"); // Administration - Metric List
            DeleteBlockType("CE769F0A-722F-4745-A6A8-7F00548CD1D2"); // Administration - Metric Detail
            DeletePage("574544A8-4831-4DAB-8BCE-B2C9B3D188AF"); // Metric Detail            
        }
    }
}
