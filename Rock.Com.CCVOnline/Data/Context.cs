
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

using Rock.Data;

namespace Rock.Com.CCVOnline.Data
{
    public partial class Context : DbContext
    {
        public Context()
            : base( "RockContext" )
        {
            Database.SetInitializer<Context>( null );
        }

        public DbSet<Rock.Com.CCVOnline.Service.Recording> Recordings { get; set; }

        protected override void OnModelCreating( DbModelBuilder modelBuilder )
        {
            modelBuilder.Configurations.Add( new Rock.Com.CCVOnline.Service.RecordingConfiguration() );
        }
    }
}

