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
    public partial class ScheduleUsingICal : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Schedule", "iCalendarContent", c => c.String());
            DropColumn("dbo.Schedule", "Frequency");
            DropColumn("dbo.Schedule", "FrequencyQualifier");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Schedule", "FrequencyQualifier", c => c.String(maxLength: 100));
            AddColumn("dbo.Schedule", "Frequency", c => c.Int(nullable: false));
            DropColumn("dbo.Schedule", "iCalendarContent");
        }
    }
}
