namespace Rock.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.Linq;
	using Rock.Groups;
	using Rock.CMS;
	using System.Collections.Generic;

	internal sealed class Configuration : DbMigrationsConfiguration<Rock.Data.RockContext>
	{
		public Configuration()
		{
			AutomaticMigrationsEnabled = true;
		}

		protected override void Seed( Rock.Data.RockContext context )
		{
			var groupRoleMember = new GroupRole
			{
				Name = "Member",
				Description = "Member of a group",
				System = true,
				Order = 1,
				Guid = new Guid( "00f3ac1c-71b9-4ee5-a30e-4c48c8a0bf1f" )
			};

			// Could not get gr.Guid match checking to work... so using Name instead.
			context.GroupRoles.AddOrUpdate( gr => gr.Name, groupRoleMember );

			var groupType1 = new GroupType
			{
				Name = "Type 1",
				Description = "",
				System = true,
				Guid = new Guid( "aece949f-704c-483e-a4fb-93d5e4720c4c" ),
				DefaultGroupRoleId = groupRoleMember.Id
			};

			context.GroupTypes.AddOrUpdate( gt => gt.Name, groupType1 );

			var adminRole = new Group
			{
				Name = "Administrative Users",
				Description = "Group of people who are admins on the site",
				System = true,
				IsSecurityRole = true,
				GroupTypeId = groupType1.Id,
				Guid = new Guid( "628c51a8-4613-43ed-a18d-4a6fb999273e" )
			};

			var staffRole = new Group
			{
				Name = "Staff Members",
				Description = "Used to give rights to the organization's staff members.",
				System = true,
				IsSecurityRole = true,
				GroupTypeId = groupType1.Id,
				Guid = new Guid( "2c112948-ff4c-46e7-981a-0257681eadf4" )
			};

			var webAdminRole = new Group
			{
				Name = "Website Administrator",
				Description = "Group of individuals who administrate portals. They have access to add, remove, update pages and their settings as well as the content on the page.",
				System = true,
				IsSecurityRole = true,
				GroupTypeId = groupType1.Id,
				Guid = new Guid( "1918e74f-c00d-4ddd-94c4-2e7209ce12c3" )
			};

			var webContentEditorRole = new Group
			{
				Name = "Website Content Editors",
				Description = "Group of individuals who have access to edit content on pages, but can not modify settings or add pages.",
				System = true,
				IsSecurityRole = true,
				GroupTypeId = groupType1.Id,
				Guid = new Guid( "cdf68207-2795-42de-b060-fe01c33beaea" )
			};

			context.Groups.AddOrUpdate( r => r.Name, adminRole, staffRole, webAdminRole, webContentEditorRole );

			// Sites
			var defaultSite = new Site
			{
				System = true,
				Name = "Default",
				Description = "Default Site",
				Theme = "RockChMS",
				FaviconUrl = "Themes/Default/Assets/Icons/favicon.ico",
				AppleTouchIconUrl = "Themes/Default/Assets/Icons/apple-touch.png",
				Guid = new Guid( "c2d29296-6a87-47a9-a753-ee4e9159c4c4" )
			};

			context.Sites.AddOrUpdate( s => s.Name, defaultSite );

			// Pages
			var page1 = new Page
			{
				SiteId = defaultSite.Id,
				Name = "Default Page",
				MenuDisplayChildPages = false,
				DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
				Title = "Default Page",
				MenuDisplayDescription = false,
				System = true,
				OutputCacheDuration = 0,
				Layout = "Default",
				RequiresEncryption = false,
				EnableViewState = true,
				Order = 1,
				IncludeAdminFooter = true,
				Guid = new Guid( "85f25819-e948-4960-9ddf-00f54d32444e" ),
				Description = "This is the description of the default page",
				MenuDisplayIcon = false
			};
			context.Pages.AddOrUpdate( p => p.Name, page1 );

			var page12 = new Page
			{
				SiteId = defaultSite.Id,
				Name = "Main Page Test",
				MenuDisplayChildPages = true,
				DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
				Title = "Rock ChMS",
				MenuDisplayDescription = false,
				System = true,
				OutputCacheDuration = 0,
				Layout = "Default",
				RequiresEncryption = false,
				EnableViewState = false,
				Order = 8,
				IncludeAdminFooter = true,
				Guid = new Guid( "20f97a93-7949-4c2a-8a5e-c756fe8585ca" ),
				Description = "Main Rock ChMS",
				MenuDisplayIcon = false
			};
			context.Pages.AddOrUpdate( p => p.Name, page12 );

			var page44 = new Page
			{
				SiteId = defaultSite.Id,
				Name = "Administration",
				ParentPage = page12,
				MenuDisplayChildPages = true,
				DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
				Title = "Administration",
				MenuDisplayDescription = false,
				System = true,
				OutputCacheDuration = 0,
				Layout = "Default",
				RequiresEncryption = false,
				EnableViewState = true,
				Order = 5,
				IncludeAdminFooter = true,
				Guid = new Guid( "84e12152-e456-478e-af68-ba640e9ce65b" ),
				MenuDisplayIcon = false
			};
			context.Pages.AddOrUpdate( p => p.Name, page44 );


			var page48 = new Page
			{
				SiteId = defaultSite.Id,
				Name = "Security",
				ParentPage = page44,
				MenuDisplayChildPages = false,
				DisplayInNavWhen = CMS.DisplayInNavWhen.Never,
				Title = "Security",
				MenuDisplayDescription = false,
				System = true,
				OutputCacheDuration = 0,
				Layout = "Default",
				RequiresEncryption = false,
				EnableViewState = true,
				Order = 3,
				IncludeAdminFooter = true,
				Guid = new Guid( "8c71a7e2-18a8-41c0-ab40-ad85cf90ca81" ),
				MenuDisplayIcon = false
			};
			context.Pages.AddOrUpdate( p => p.Name, page48 );

			var page49 = new Page
			{
				SiteId = defaultSite.Id,
				Name = "CMS Administration",
				ParentPage = page44,
				MenuDisplayChildPages = true,
				DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
				Title = "CMS Administration",
				MenuDisplayDescription = false,
				System = true,
				OutputCacheDuration = 0,
				Layout = "Default",
				RequiresEncryption = false,
				EnableViewState = true,
				Order = 0,
				IncludeAdminFooter = true,
				Guid = new Guid( "550a898c-edea-48b5-9c58-b20ec13af13b" ),
				MenuDisplayIcon = false
			};
			context.Pages.AddOrUpdate( p => p.Name, page49 );

			var page2 = new Page
			{
				SiteId = defaultSite.Id,
				Name = "Sites",
				ParentPage = page49,
				MenuDisplayChildPages = false,
				DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
				Title = "Sites",
				MenuDisplayDescription = false,
				System = true,
				OutputCacheDuration = 0,
				Layout = "Default",
				RequiresEncryption = false,
				EnableViewState = true,
				Order = 6,
				IncludeAdminFooter = true,
				Guid = new Guid( "7596d389-4eab-4535-8bee-229737f46f44" ),
				Description = "Manage Sites",
				MenuDisplayIcon = false
			};
			context.Pages.AddOrUpdate( p => p.Name, page2 );

			var page3 = new Page
			{
				SiteId = defaultSite.Id,
				Name = "Login",
				ParentPage = page48,
				MenuDisplayChildPages = false,
				DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
				Title = "Login",
				MenuDisplayDescription = false,
				System = true,
				OutputCacheDuration = 0,
				Layout = "Splash",
				RequiresEncryption = false,
				EnableViewState = true,
				Order = 0,
				IncludeAdminFooter = true,
				Guid = new Guid( "03cb988a-138c-448b-a43d-8891844eeb18" ),
				Description = "Login",
				MenuDisplayIcon = false
			};
			context.Pages.AddOrUpdate( p => p.Name, page3 );

			var page4 = new Page
			{
				SiteId = defaultSite.Id,
				Name = "New Account",
				ParentPage = page48,
				MenuDisplayChildPages = false,
				DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
				Title = "Create Account ",
				MenuDisplayDescription = false,
				System = true,
				OutputCacheDuration = 0,
				Layout = "Default",
				RequiresEncryption = false,
				EnableViewState = true,
				Order = 2,
				IncludeAdminFooter = true,
				Guid = new Guid( "7d4e2142-d24e-4dd2-84bc-b34c5c3d0d46" ),
				Description = "Create Account",
				MenuDisplayIcon = false
			};
			context.Pages.AddOrUpdate( p => p.Name, page4 );

			var page7 = new Page
			{
				SiteId = defaultSite.Id,
				Name = "Blocks",
				ParentPage = page49,
				MenuDisplayChildPages = true,
				DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
				Title = "Blocks",
				MenuDisplayDescription = true,
				System = true,
				OutputCacheDuration = 0,
				Layout = "Default",
				RequiresEncryption = false,
				EnableViewState = true,
				Order = 7,
				IncludeAdminFooter = true,
				Guid = new Guid( "5fbe9019-862a-41c6-acdc-287d7934757d" ),
				Description = "Manage Blocks",
				MenuDisplayIcon = true
			};
			context.Pages.AddOrUpdate( p => p.Name, page7 );

			var page16 = new Page
			{
				SiteId = defaultSite.Id,
				Name = "ZoneBlocks",
				ParentPage = page49,
				MenuDisplayChildPages = false,
				DisplayInNavWhen = CMS.DisplayInNavWhen.Never,
				Title = "Zone Blocks",
				MenuDisplayDescription = false,
				System = true,
				OutputCacheDuration = 0,
				Layout = "Dialog",
				RequiresEncryption = false,
				EnableViewState = true,
				Order = 9,
				IncludeAdminFooter = false,
				Guid = new Guid( "9f36531f-c1b5-4e23-8fa3-18b6daff1b0b" ),
				Description = "Admin page for administering the blocks in a zone",
				MenuDisplayIcon = false
			};
			context.Pages.AddOrUpdate( p => p.Name, page16 );

			var page23 = new Page
			{
				SiteId = defaultSite.Id,
				Name = "Block Properties",
				ParentPage = page49,
				MenuDisplayChildPages = false,
				DisplayInNavWhen = CMS.DisplayInNavWhen.Never,
				Title = "Block Properties",
				MenuDisplayDescription = false,
				System = true,
				OutputCacheDuration = 0,
				Layout = "Dialog",
				RequiresEncryption = false,
				EnableViewState = true,
				Order = 11,
				IncludeAdminFooter = false,
				Guid = new Guid( "f0b34893-9550-4864-adb4-ee860e4e427c" ),
				Description = "Lists the attributes for a block instance",
				MenuDisplayIcon = false
			};
			context.Pages.AddOrUpdate( p => p.Name, page23 );

			var page28 = new Page
			{
				SiteId = defaultSite.Id,
				Name = "Security",
				ParentPage = page48,
				MenuDisplayChildPages = false,
				DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
				Title = "Manage Security",
				MenuDisplayDescription = false,
				System = true,
				OutputCacheDuration = 0,
				Layout = "Dialog",
				RequiresEncryption = false,
				EnableViewState = true,
				Order = 13,
				IncludeAdminFooter = false,
				Guid = new Guid( "86d5e33e-e351-4ca5-9925-849c6c467257" ),
				Description = "Used to manage security for an entity",
				MenuDisplayIcon = false
			};
			context.Pages.AddOrUpdate( p => p.Name, page28 );

			var page29 = new Page
			{
				SiteId = defaultSite.Id,
				Name = "ChildPages",
				ParentPage = page49,
				MenuDisplayChildPages = false,
				DisplayInNavWhen = CMS.DisplayInNavWhen.Never,
				Title = "Child Pages",
				MenuDisplayDescription = false,
				System = true,
				OutputCacheDuration = 0,
				Layout = "Dialog",
				RequiresEncryption = false,
				EnableViewState = true,
				Order = 14,
				IncludeAdminFooter = false,
				Guid = new Guid( "d58f205e-e9cc-4bd9-bc79-f3da86f6e346" ),
				Description = "Manage child pages",
				MenuDisplayIcon = false
			};
			context.Pages.AddOrUpdate( p => p.Name, page29 );

			var page37 = new Page
			{
				SiteId = defaultSite.Id,
				Name = "Page Properties",
				ParentPage = page49,
				MenuDisplayChildPages = false,
				DisplayInNavWhen = CMS.DisplayInNavWhen.Never,
				Title = "Page Properties",
				MenuDisplayDescription = false,
				System = true,
				OutputCacheDuration = 0,
				Layout = "Dialog",
				RequiresEncryption = false,
				EnableViewState = true,
				Order = 15,
				IncludeAdminFooter = false,
				Guid = new Guid( "37759b50-db4a-440d-a83b-4ef3b4727b1e" ),
				Description = "Page Properties",
				MenuDisplayIcon = false
			};
			context.Pages.AddOrUpdate( p => p.Name, page37 );

			var page41 = new Page
			{
				SiteId = defaultSite.Id,
				Name = "Communications",
				ParentPage = page12,
				MenuDisplayChildPages = true,
				DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
				Title = "Communications",
				MenuDisplayDescription = false,
				System = true,
				OutputCacheDuration = 0,
				Layout = "Default",
				RequiresEncryption = false,
				EnableViewState = true,
				Order = 2,
				IncludeAdminFooter = true,
				Guid = new Guid( "0319bdc8-8cbf-4673-b4b3-63b2bfca2c3c" ),
				MenuDisplayIcon = false
			};
			context.Pages.AddOrUpdate( p => p.Name, page41 );

			var page51 = new Page
			{
				SiteId = defaultSite.Id,
				Name = "Global Attributes",
				ParentPage = page49,
				MenuDisplayChildPages = false,
				DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
				Title = "Global Attributes",
				MenuDisplayDescription = false,
				System = true,
				OutputCacheDuration = 0,
				Layout = "Default",
				RequiresEncryption = false,
				EnableViewState = false,
				Order = 16,
				IncludeAdminFooter = true,
				Guid = new Guid( "a2753e03-96b1-4c83-aa11-fcd68c631571" ),
				MenuDisplayIcon = false
			};
			context.Pages.AddOrUpdate( p => p.Name, page51 );

			var page52 = new Page
			{
				SiteId = defaultSite.Id,
				Name = "Global Values",
				ParentPage = page49,
				MenuDisplayChildPages = false,
				DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
				Title = "Global Values",
				MenuDisplayDescription = false,
				System = true,
				OutputCacheDuration = 0,
				Layout = "Default",
				RequiresEncryption = false,
				EnableViewState = false,
				Order = 17,
				IncludeAdminFooter = true,
				Guid = new Guid( "d5550020-0bd0-43e6-806b-25338830f244" ),
				MenuDisplayIcon = false
			};
			context.Pages.AddOrUpdate( p => p.Name, page52 );

			// Page Routes

			// Blocks

		}
	}
}
