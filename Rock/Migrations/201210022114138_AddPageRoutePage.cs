namespace Rock.Migrations
{
	/// <summary>
	/// 
	/// </summary>
	public partial class AddPageRoutePage : RockMigration
	{
		/// <summary>
		/// Operations to be performed during the upgrade process.
		/// </summary>
		public override void Up()
		{
			CreateIndex( "cmsPageRoute", "Route", true );
			CreateIndex( "cmsPageRoute", "Guid", true );
			AddBlockType("Page Routes", "Allows for configuration of Page Routes", "~/Blocks/Administration/PageRoutes.ascx", "FEE08A28-B774-4294-9F77-697FE66CA5B5");
			AddPage("B4A24AB7-9369-4055-883F-4F4892C39AE3", "Page Routes", "List of Page Routes", "4A833BE3-7D5E-4C38-AF60-5706260015EA");
			AddBlock("4A833BE3-7D5E-4C38-AF60-5706260015EA", "FEE08A28-B774-4294-9F77-697FE66CA5B5", "Page Route Block", "Content", "09DC13AF-8BF8-4A65-B3DF-77F17C5650D6");
		}

		/// <summary>
		/// Operations to be performed during the downgrade process.
		/// </summary>
		public override void Down()
		{
			DropIndex( "cmsPageRoute", new string[] {"Route"} );
			DropIndex( "cmsPageRoute", new string[] { "Guid" } );
			DeleteBlock( "09DC13AF-8BF8-4A65-B3DF-77F17C5650D6" );
			DeletePage( "4A833BE3-7D5E-4C38-AF60-5706260015EA" );
			DeleteBlockType( "FEE08A28-B774-4294-9F77-697FE66CA5B5" );
		}
	}
}
