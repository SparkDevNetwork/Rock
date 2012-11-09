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
    public partial class SiteMapPage : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddBlockType( "Site Map", "Shows a map of Pages and Blocks", "~/Blocks/Administration/SiteMap.ascx", "2700A1B8-BD1A-40F1-A660-476DA86D0432" );
            AddPage( "B4A24AB7-9369-4055-883F-4F4892C39AE3", "Site Map", "Current Pages and Blocks", "EC7A06CD-AAB5-4455-962E-B4043EA2440E" );
            AddBlock( "EC7A06CD-AAB5-4455-962E-B4043EA2440E", "2700A1B8-BD1A-40F1-A660-476DA86D0432", "Site Map", "Content", "68192536-3CE8-433B-9DF8-A895EF037FD7" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlock( "68192536-3CE8-433B-9DF8-A895EF037FD7" );
            DeletePage( "EC7A06CD-AAB5-4455-962E-B4043EA2440E" );
            DeleteBlockType( "2700A1B8-BD1A-40F1-A660-476DA86D0432" );
        }
    }
}
