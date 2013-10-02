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
    public partial class DefinedTypeRefactor : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add BlockType Defined Value List
            AddBlockType( "Administration - Defined Value List", "Block for viewing values for a defined type.", "~/Blocks/Administration/DefinedValueList.ascx", "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE" );
            
            // Add Block to Page: Defined Type Detail
            AddBlock( "60C0C193-61CF-4B34-A0ED-67EF8FD44867", "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "Defined Value List", "", "Content", 1, "71260814-0750-4302-8ED5-2B6AFA1181A9" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlock( "71260814-0750-4302-8ED5-2B6AFA1181A9" ); // Remove Block: Defined Value List, from Page: Defined Type Detail
            DeleteBlockType( "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE" ); // Administration - Defined Value List
        }
    }
}
