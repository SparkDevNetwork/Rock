
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Rock.Custom.CCVOnline.Data
{
    public partial class CCVContext : DbContext
    {
        public DbSet<Rock.Custom.CCVOnline.CommandCenter.Recording> Recordings { get; set; }

        protected override void OnModelCreating( DbModelBuilder modelBuilder )
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Configurations.Add( new Rock.Custom.CCVOnline.CommandCenter.RecordingConfiguration() );
		}
    }
}

