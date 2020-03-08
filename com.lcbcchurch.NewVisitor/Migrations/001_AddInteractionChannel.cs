using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.lcbcchurch.NewVisitor.Migrations
{
    [MigrationNumber( 1, "1.0.7" )]
    public class AddInteractionChannel : Migration
    {
        public override void Up()
        {
            Sql( @"
INSERT INTO [InteractionChannel] (
	[Name]
	,[Guid]
	)
VALUES
(
'Engagement Scoring',
'CFEF6FEE-5E88-4C0B-BB1E-55F8302C01F5'
)
" );
        }

        public override void Down()
        {
        }
    }
}
