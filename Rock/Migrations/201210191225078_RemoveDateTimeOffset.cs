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
    public partial class RemoveDateTimeOffset : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //AlterColumn("dbo.cmsFile", "LastModifiedTime", c => c.DateTime());
            //AlterColumn("dbo.coreAudit", "DateTime", c => c.DateTime());
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //AlterColumn("dbo.coreAudit", "DateTime", c => c.DateTimeOffset());
            //AlterColumn("dbo.cmsFile", "LastModifiedTime", c => c.DateTimeOffset());
        }
    }
}
