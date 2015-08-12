using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;
namespace com.centralaz.Finance.Migrations
{
    [MigrationNumber( 1, "1.1.1" )]
    public class ReplaceExternalGivingHistoryBlock : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.DeleteBlock( "8A5E5144-3054-4FC9-AD8A-B0F4813C94E4" );

            RockMigrationHelper.AddBlock( "621E03C5-6586-450B-A340-CC7D9727B69A", "", "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "Dynamic Data", "Main", "", "", 0, "E42FEC25-3FBE-414B-8F43-C91983988A9B" );

            RockMigrationHelper.AddBlockAttributeValue( "E42FEC25-3FBE-414B-8F43-C91983988A9B", "B0EC41B9-37C0-48FD-8E4E-37A8CA305012", @"CurrentPersonId=1" ); // Query Params

            RockMigrationHelper.AddBlockAttributeValue( "E42FEC25-3FBE-414B-8F43-C91983988A9B", "71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356", @"select top 1 *
from Location as l
inner join GroupLocation as gl
	on l.Id = gl.LocationId
inner join [Group] as g
	on g.Id = gl.GroupId
inner join [GroupType] as gt
	on g.GroupTypeId =	gt.id
inner join [GroupMember] as gm
	on gm.GroupId = g.Id
Where gm.PersonId = @CurrentPersonId
and gt.Guid = '790E3215-3B10-442B-AF69-616C0DCB998E'" ); // Query

            RockMigrationHelper.AddBlockAttributeValue( "E42FEC25-3FBE-414B-8F43-C91983988A9B", "6A233402-446C-47E9-94A5-6A247C29BC21", @"

<p>{{ GlobalAttribute.OrganizationName }}<br />
{{ GlobalAttribute.OrganizationAddress }}</p>

{%for row in rows %}
<p>
{{row.Street1}} {{row.Street2}}</br>
{{row.City}}, {{row.State}} {{row.PostalCode}}
</p>
{%endfor%}

<p><span style='line-height: 1.6em;'>Dear {{Person.FullName}},</span></p>

<p>The report below will allow you to view your previous giving. Use the date and account filters to adjust the display. On behalf of Central Christian Church, thank you for your faithful giving.</p>

<hr />" ); // Formatted Output

        }
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "6A233402-446C-47E9-94A5-6A247C29BC21" );
            RockMigrationHelper.DeleteAttribute( "71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356" );
            RockMigrationHelper.DeleteAttribute( "B0EC41B9-37C0-48FD-8E4E-37A8CA305012" );
            RockMigrationHelper.DeleteBlock( "E42FEC25-3FBE-414B-8F43-C91983988A9B" );

            RockMigrationHelper.AddBlock( "621E03C5-6586-450B-A340-CC7D9727B69A", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Transaction Report Intro Text", "Main", "", "", 0, "8A5E5144-3054-4FC9-AD8A-B0F4813C94E4" );

        }
    }
}
