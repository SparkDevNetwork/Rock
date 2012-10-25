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
    public partial class AddGroupPageBlock1 : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddBlockType( "Group Types", "Allows for configuration of Group Types", "~/Blocks/Group/GroupTypes.ascx", "C443D72B-1A9E-41E7-8E70-4E9D39AE6AC3" );
            AddPage( "91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F", "Group Types", "Manage Group Types", "40899BCD-82B0-47F2-8F2A-B6AA3877B445" );
            AddBlock( "40899BCD-82B0-47F2-8F2A-B6AA3877B445", "C443D72B-1A9E-41E7-8E70-4E9D39AE6AC3", "Group Types", "Content", "F3B2FC30-5ACE-4D1D-87F3-9712723D903F" ); 
            
            AddBlockType( "Groups", "Allows for the configuration fo Groups", "~/Blocks/Group/Groups.ascx", "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A" );
            AddPage( "91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F", "Groups", "Manage Groups and Group Members", "4D7624FB-A9AE-40BD-82CB-84C22F64343E" );
            AddBlock( "4D7624FB-A9AE-40BD-82CB-84C22F64343E", "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "Groups", "Content", "52B774FE-9ABF-4852-9496-6FAD4646F949" );
             
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Groups
            DeleteBlock( "52B774FE-9ABF-4852-9496-6FAD4646F949" );
            DeletePage( "4D7624FB-A9AE-40BD-82CB-84C22F64343E" );
            DeleteBlockType( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A" );

            // Group Types
            DeleteBlock( "F3B2FC30-5ACE-4D1D-87F3-9712723D903F" );
            DeletePage( "40899BCD-82B0-47F2-8F2A-B6AA3877B445" );
            DeleteBlockType( "C443D72B-1A9E-41E7-8E70-4E9D39AE6AC3" );
        }
    }
}
