using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.bemaservices.MinistrySafe.Migrations
{
    [MigrationNumber( 6, "1.9.2" )]
    public partial class NamespaceMove : Migration
    {
        public override void Up()
        {
            // Page: Ministry Safe
            Sql( @"
                Update BlockType
                Set Path = '~/Plugins/com_bemaservices/MinistrySafe/UserList.ascx'
                Where Path = '~/Plugins/com_bemaservices/Security/MinistrySafe/UserList.ascx'
                " );
        }
        public override void Down()
        {
        }
    }
}
