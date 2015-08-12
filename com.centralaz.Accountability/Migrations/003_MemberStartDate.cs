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
    [MigrationNumber( 3, "1.0.14" )]
    public class MemberStartDate : Migration
    {
        //
        public override void Up()
        {
            GroupTypeCache theGroupType = GroupTypeCache.Read( "DC99BF69-8A1A-411F-A267-1AE75FDC2341" );
            int id = theGroupType.Id;
            String theId = id.ToString();
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.GroupMember", Rock.SystemGuid.FieldType.DATE, "GroupTypeId", theId, "Member Start Date", "", "The date the first report is due for this member", 0, "10/1/2014", "1751DA07-F665-48F6-A409-E193FCB1C86D" );            
        }

        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "1751DA07-F665-48F6-A409-E193FCB1C86D" );
        }
    }
}