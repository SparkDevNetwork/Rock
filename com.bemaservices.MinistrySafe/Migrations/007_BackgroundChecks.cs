using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.bemaservices.MinistrySafe.Migrations
{
    [MigrationNumber( 7, "1.9.4" )]
    public partial class BackgroundChecks : Migration
    {
        public override void Up()
        {
            // Page: Ministry Safe
            Sql( @"
                Update BlockType
                Set Path = '~/Plugins/com_bemaservices/MinistrySafe/MinistrySafeTrainingList.ascx',
                    Name = 'Ministry Safe Training List',
                    Description = 'Lists all the Ministry Safe background check requests.'
                Where Path = '~/Plugins/com_bemaservices/MinistrySafe/UserList.ascx'
                " );
        }
        public override void Down()
        {
        }
    }
}
