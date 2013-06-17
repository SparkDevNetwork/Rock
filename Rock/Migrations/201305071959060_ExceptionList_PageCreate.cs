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
    public partial class ExceptionList_PageCreate : RockMigration_5
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "Exception List", "", "Default", "21DA6141-0A03-4F00-B0A8-3B110FBE2438" );
            AddBlockType( "Administration - Exception List", "", "~/Blocks/Administration/ExceptionList.ascx", "6302B319-9830-4BE3-A402-17801C88F7E4" );
            AddBlock( "21DA6141-0A03-4F00-B0A8-3B110FBE2438", "6302B319-9830-4BE3-A402-17801C88F7E4", "Exception List", "", "Content", 0, "557E75A4-1841-4CBE-B976-F36DF209AA17" );
            AddBlockTypeAttribute( "6302B319-9830-4BE3-A402-17801C88F7E4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "", "", 0, "", "A742376A-0148-4777-B704-E47841879337" );
            AddBlockTypeAttribute( "6302B319-9830-4BE3-A402-17801C88F7E4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Summary Count Days", "SummaryCountDays", "", "Summary field for exceptions that have occurred within the last x days. Default value is 7.", 0, "7", "7944E9DE-F676-421A-B2EE-80446EFC3D58" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "A742376A-0148-4777-B704-E47841879337" ); // Detail Page Guid
            DeleteAttribute( "7944E9DE-F676-421A-B2EE-80446EFC3D58" ); // Summary Count Days
            DeleteBlock( "557E75A4-1841-4CBE-B976-F36DF209AA17" ); // Exception List
            DeleteBlockType( "6302B319-9830-4BE3-A402-17801C88F7E4" ); // Administration - Exception List
            DeletePage( "21DA6141-0A03-4F00-B0A8-3B110FBE2438" ); // Exception List
        }
    }
}
