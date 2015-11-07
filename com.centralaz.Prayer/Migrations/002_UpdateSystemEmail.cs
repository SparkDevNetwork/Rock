using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Plugin;
using Rock.Web.Cache;
namespace com.centralaz.Accountability.Migrations
{
    [MigrationNumber( 2, "1.0.14" )]
    public class UpdateSystemEmail : Migration
    {
        //
        public override void Up()
        {
            RockMigrationHelper.UpdateSystemEmail( "Groups", "Prayer Request Update", "PrayerTeam@centralaz.com", "", "", "", "", "Prayer Request Update", @"{{PrayerRequest.FirstName}},
<br><br>
We've had a great time praying for you this week. One of the things that really motivates a prayer team is frequent updates and answered prayer. Have there been any changes? Please take a moment to update us on any changes that will help us pray more effectively or strategically.
<br><br>
If you wish for us to continue praying for this request or send an update, please click the 'Edit Request' link below.
<br><br>
<a href='{{ 'Global' | Attribute:'PublicApplicationRoot' }}{{MagicUrl}}'>Edit Request</a>
<br><br>
Blessings,<br>
Paul Covert<br>
Prayer Pastor, Central Christian Church<br>
<br><br>
Request:<br>
{{PrayerRequest.Text}}", "4ADFA0F5-E6E1-4208-A6E1-BF69FA29ADF0" );
        }

        public override void Down()
        {

        }
    }
}