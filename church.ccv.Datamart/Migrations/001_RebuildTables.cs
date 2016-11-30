using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace church.ccv.Datamart.Migrations
{
    [MigrationNumber( 1, "1.2.0" )]
    public class RebuildTables : Rock.Plugin.Migration
    {
        public override void Up()
        {
            Sql( MigrationSQL._001_RebuildTables_Rebuild_church_ccv_Datamart_ERALoss );
            LongSql( MigrationSQL._001_RebuildTables_Rebuild_church_ccv_Datamart_EstimatedRegularAttendees, 180 );
            LongSql( MigrationSQL._001_RebuildTables_Rebuild_church_ccv_Datamart_Family, 180 );
            Sql( MigrationSQL._001_RebuildTables_Rebuild_church_ccv_Datamart_NearestGroup );
            Sql( MigrationSQL._001_RebuildTables_Rebuild_church_ccv_Datamart_Neighborhood );
            LongSql( MigrationSQL._001_RebuildTables_Rebuild_church_ccv_Datamart_Person, 180 );
        }

        private void LongSql( string sql, int commandTimeoutSeconds)
        {
            using ( SqlCommand sqlCommand = new SqlCommand( sql, SqlConnection, SqlTransaction ) )
            {
                sqlCommand.CommandTimeout = commandTimeoutSeconds;
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.ExecuteNonQuery();
            }
        }

        public override void Down()
        {
            //
        }
    }
}
