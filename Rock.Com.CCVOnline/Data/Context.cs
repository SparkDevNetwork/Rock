//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

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
			Rock.Data.ContextHelper.AddConfigurations( modelBuilder );

			ContextHelper.AddConfigurations(modelBuilder);
        }
    }

	public static class ContextHelper
	{
		public static void AddConfigurations( DbModelBuilder modelBuilder )
		{
			modelBuilder.Configurations.Add( new Rock.Com.CCVOnline.Service.RecordingConfiguration() );
		}
	}
}

