using System.Data.Entity.Migrations;

namespace RockCoreMigrator
{
    // the only the purpose of this project is to let you Update-Database RockCore from Package Manager Console
    internal sealed class Configuration : DbMigrationsConfiguration<Rock.Data.RockContext>
    {
        public Configuration()
        {
            this.MigrationsAssembly = typeof( Rock.Data.RockContext ).Assembly;
            this.MigrationsNamespace = "Rock.Migrations";
            this.ContextKey = "Rock.Migrations.Configuration";
        }
    }
}
