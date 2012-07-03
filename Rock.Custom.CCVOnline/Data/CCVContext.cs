
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

using Rock.Data;

namespace Rock.Custom.CCVOnline.Data
{
    public partial class CCVContext : DbContext
    {
        public DbSet<Rock.Custom.CCVOnline.CommandCenter.Recording> Recordings { get; set; }

        protected override void OnModelCreating( DbModelBuilder modelBuilder )
        {
            base.OnModelCreating( modelBuilder );
            modelBuilder.Configurations.Add( new Rock.Custom.CCVOnline.CommandCenter.RecordingConfiguration() );
		}
    }
}

