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
    public partial class RockControlGallery : RockMigration_5
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "B4A24AB7-9369-4055-883F-4F4892C39AE3", "Rock Control Gallery", "These are most of the Rock Controls that can be used to build Blocks. Click Toggle Labels to see how that look without headings and labels.", "Default", "F8B18AA5-A85C-42D4-87D4-C8F7E2FB0159", "icon-magic" );
            AddBlockType( "Examples - Rock Control Gallery", "", "~/Blocks/Examples/RockControlGallery.ascx", "55468258-18B9-4FAE-90E8-F173F7704E23" );
            AddBlock( "F8B18AA5-A85C-42D4-87D4-C8F7E2FB0159", "55468258-18B9-4FAE-90E8-F173F7704E23", "Rock Control Gallery", "", "Content", 0, "3FA43C1C-2590-4297-8C0E-8A67F14A85DF" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlock( "3FA43C1C-2590-4297-8C0E-8A67F14A85DF" ); // Rock Control Gallery
            DeleteBlockType( "55468258-18B9-4FAE-90E8-F173F7704E23" ); // Examples - Rock Control Gallery
            DeletePage( "F8B18AA5-A85C-42D4-87D4-C8F7E2FB0159" ); // Rock Control Gallery
        }
    }
}
