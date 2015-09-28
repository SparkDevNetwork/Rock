using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;
using Rock.Web.Cache;

namespace com.centralaz.SpiritualGifts.Migrations
{
    [MigrationNumber( 5, "1.0.14" )]
    public class BadgeDetailPageFix : Migration
    {
        public static readonly string SPIRITUAL_GIFT_BADGE = "88F61BA1-35AA-4080-AC56-CF4B1145C51F";

        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {

            // add the badge to PersonBadge SpiritualGift Result Detail page guid
            RockMigrationHelper.AddPersonBadgeAttributeValue( SPIRITUAL_GIFT_BADGE, "56D4946D-DD70-4C06-A382-3CF34B56A8BE", "A2EA46FF-6B39-4A0E-A1A4-AAEC6ADFC068" );
            RockMigrationHelper.DeleteAttribute( "26770BE6-350C-406F-AD3D-104275D73BD0" );
            RockMigrationHelper.AddBlockTypeAttribute( "AA78F8DC-B8AB-4300-B6FA-A8A227E422D9", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Minimum Days To Retake", "MinimumDaysToRetake", "", "The number of days that must pass before the test can be taken again.", 0, @"30", "26770BE6-350C-406F-AD3D-104275D73BD0" );
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DATE, "12D8E61F-ED07-41D9-BE0B-43C73907896D", "Last Spiritual Save Date", "LastSpiritualSaveDate", "fa fa-gift", "The date the person took the spiritual gifts test.", 9, string.Empty, "E0841055-FC6C-47DB-9CF3-36BD8DB66151" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {

        }
    }
}
