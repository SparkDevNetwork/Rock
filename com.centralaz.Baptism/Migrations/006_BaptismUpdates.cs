using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Plugin;
using Rock.Web.Cache;
namespace com.centralaz.Baptism.Migrations
{
    [MigrationNumber( 6, "1.0.14" )]
    public class BaptismUpdates : Migration
    {
        public override void Up()
        {
            Sql( @"
        --Baptisms Page
        UPDATE [Page] SET
            [BreadCrumbDisplayName] = 0 
        WHERE [GUID] = 'B248D7E3-AD38-4E83-9E3C-3CC6D3814AB4'

UPDATE [dbo].[GroupType]
   SET [ShowInNavigation] = 0    
 WHERE [GUID] = '32F8592C-AE11-44A7-A053-DE43789811D9'


" );

        }

        public override void Down()
        {

        }
    }
}