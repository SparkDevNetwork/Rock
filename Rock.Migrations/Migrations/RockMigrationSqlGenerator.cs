using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Data.Entity.SqlServer;

namespace Rock.Migrations
{
    public class RockSqlServerMigrationSqlGenerator : SqlServerMigrationSqlGenerator
    {
        public override IEnumerable<MigrationStatement> Generate( IEnumerable<MigrationOperation> migrationOperations, string providerManifestToken )
        {
            var migrationStatements = base.Generate( migrationOperations, providerManifestToken );
            return migrationStatements;
        }
    }
}
