using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using com.mychurch.MyFirstRockLibrary.Model;

namespace com.mychurch.MyFirstRockLibrary.Data
{
    public partial class MyFirstRockLibraryContext : DbContext
    {
        public MyFirstRockLibraryContext()
            : base( "RockContext" )
        {
            // intentionally left blank
        }

        protected override void OnModelCreating( DbModelBuilder modelBuilder )
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            #region Model Configurations

            // List of modelBuilder.Configurations.Add(...) go here

            modelBuilder.Configurations.Add( new PotluckDinnerConfiguration() );
            modelBuilder.Configurations.Add( new PotluckDishConfiguration() );

            #endregion
        }

        #region Models

        // Model DbSet classes go here

        public DbSet<PotluckDinner> PotluckDinners { get; set; }
        public DbSet<PotluckDish> PotluckDish { get; set; }

        #endregion
    }
}
