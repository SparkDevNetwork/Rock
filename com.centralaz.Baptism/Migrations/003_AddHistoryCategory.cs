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
    [MigrationNumber( 3, "1.0.14" )]
    public class AddHistoryCategory : Migration
    {
        public override void Up()
        {
            // Add a Baptism Changes category (for the 'history' entity)
            RockMigrationHelper.UpdateCategory( "546D5F43-1184-47C9-8265-2D7BF4E1BCA5", "Baptism Changes", "fa fa-tint", "Anything related to a person's baptism.", com.centralaz.Baptism.SystemGuid.Category.HISTORY_PERSON_BAPTISM_CHANGES );
        }

        public override void Down()
        {
            // Delete the Baptism Changes category
            RockMigrationHelper.DeleteCategory( com.centralaz.Baptism.SystemGuid.Category.HISTORY_PERSON_BAPTISM_CHANGES );
        }
    }
}