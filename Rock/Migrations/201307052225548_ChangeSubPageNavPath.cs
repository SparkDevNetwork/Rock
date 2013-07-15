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
    public partial class ChangeSubPageNavPath : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib Value for Sub Page Menu:XSLT File
            AddBlockAttributeValue( "F82E5FF2-F412-405C-9CC5-BF6E0401EB38", "D8A029F8-83BE-454A-99D3-94D879EBF87C", "~/Themes/RockChMS/Assets/Xslt/SubPageNav.xslt" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddBlockAttributeValue( "F82E5FF2-F412-405C-9CC5-BF6E0401EB38", "D8A029F8-83BE-454A-99D3-94D879EBF87C", "~/Themes/RockChMS/Assets/Xslt/PersonDetail/SubPageNav.xslt" );
        }
    }
}
