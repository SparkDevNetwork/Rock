namespace Rock.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.Linq;
	using Rock.Groups;
	using Rock.CMS;
	using Rock.Util;
	using System.Collections.Generic;
	using Rock.Core;
	using Rock.CRM;

	internal sealed class Configuration : DbMigrationsConfiguration<Rock.Data.RockContext>
	{
		public Configuration()
		{
			AutomaticMigrationsEnabled = false;
		}
	}
}
