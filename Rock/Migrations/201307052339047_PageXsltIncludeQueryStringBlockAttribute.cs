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
    public partial class PageXsltIncludeQueryStringBlockAttribute : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Page Xslt Transformation:Include Current QueryString
            AddBlockTypeAttribute( "F49AD5F8-1E45-41E7-A88E-8CD285815BD9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Current QueryString", "IncludeCurrentQueryString", "", "Flag indicating if current page's QueryString should be used when building url for child pages", 0, "False", "09D9AB6B-B2E6-4E9C-AAFE-25758BE2754B" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Page Xslt Transformation:Include Current QueryString
            DeleteAttribute( "09D9AB6B-B2E6-4E9C-AAFE-25758BE2754B" );
        }
    }
}
