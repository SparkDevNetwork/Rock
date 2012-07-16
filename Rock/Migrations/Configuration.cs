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
			AutomaticMigrationsEnabled = true;
		}

//        protected override void Seed( Rock.Data.RockContext context )
//        {
//            var groupRoleMember = new GroupRole
//            {
//                Name = "Member",
//                Description = "Member of a group",
//                System = true,
//                Order = 1,
//                Guid = new Guid( "00f3ac1c-71b9-4ee5-a30e-4c48c8a0bf1f" )
//            };

//            context.GroupRoles.AddOrUpdate( gr => gr.Guid, groupRoleMember );

//            var groupType1 = new GroupType
//            {
//                Name = "Type 1",
//                Description = "",
//                System = true,
//                Guid = new Guid( "aece949f-704c-483e-a4fb-93d5e4720c4c" ),
//                DefaultGroupRoleId = groupRoleMember.Id
//            };

//            context.GroupTypes.AddOrUpdate( gt => gt.Guid, groupType1 );

//            var adminRoleGroup = new Group
//            {
//                Name = "Administrative Users",
//                Description = "Group of people who are admins on the site",
//                System = true,
//                IsSecurityRole = true,
//                GroupTypeId = groupType1.Id,
//                Guid = new Guid( "628c51a8-4613-43ed-a18d-4a6fb999273e" )
//            };
//            context.Groups.AddOrUpdate( r => r.Guid, adminRoleGroup );

//            var staffRole = new Group
//            {
//                Name = "Staff Members",
//                Description = "Used to give rights to the organization's staff members.",
//                System = true,
//                IsSecurityRole = true,
//                GroupTypeId = groupType1.Id,
//                Guid = new Guid( "2c112948-ff4c-46e7-981a-0257681eadf4" )
//            };
//            context.Groups.AddOrUpdate( r => r.Guid, staffRole );

//            var webAdminRole = new Group
//            {
//                Name = "Website Administrator",
//                Description = "Group of individuals who administrate portals. They have access to add, remove, update pages and their settings as well as the content on the page.",
//                System = true,
//                IsSecurityRole = true,
//                GroupTypeId = groupType1.Id,
//                Guid = new Guid( "1918e74f-c00d-4ddd-94c4-2e7209ce12c3" )
//            };
//            context.Groups.AddOrUpdate( r => r.Guid, webAdminRole );

//            var webContentEditorRole = new Group
//            {
//                Name = "Website Content Editors",
//                Description = "Group of individuals who have access to edit content on pages, but can not modify settings or add pages.",
//                System = true,
//                IsSecurityRole = true,
//                GroupTypeId = groupType1.Id,
//                Guid = new Guid( "cdf68207-2795-42de-b060-fe01c33beaea" )
//            };
//            context.Groups.AddOrUpdate( r => r.Guid, webContentEditorRole );

//            context.SaveChanges();

//            //
//            // Admin Person
//            //
//            var adminPerson = new Rock.CRM.Person
//            {
//                GivenName = "Admin",
//                LastName = "Admin",
//                Guid = new Guid( "61646D69-6E00-0AD9-999A-D010AD010111" )
//            };
//            context.People.AddOrUpdate( p => p.Guid, adminPerson );

//            context.SaveChanges();

//            //
//            // Admin User Account
//            //
//            var adminUserAccount = new Rock.CMS.User
//            {
//                UserName = "admin",
//                Password = "admin",
//                IsConfirmed = true,
//                AuthenticationType = CMS.AuthenticationType.Database,
//                PersonId = adminPerson.Id,
//                Guid = new Guid( "74686520-6164-6d69-6e20-757365720000" )
//            };
//            UserService us = new UserService();
//            us.ChangePassword( adminUserAccount, "admin" );
//            context.Users.AddOrUpdate( p => p.Guid, adminUserAccount );

//            context.SaveChanges();

//            // 
//            // Admin Group Member
//            //
//            var adminGroupMember = new Rock.Groups.Member
//            {
//                System = true,
//                GroupId = adminRoleGroup.Id,
//                PersonId = adminPerson.Id,
//                GroupRoleId = groupRoleMember.Id,
//                Guid = new Guid( "6F23CACA-6749-4454-85DF-5A55251B644C" )
//            };
//            context.Members.AddOrUpdate( p => p.Guid, adminGroupMember );

//            context.SaveChanges();

//            //
//            // Sites
//            //
//            var defaultSite = new Site
//            {
//                Theme = "RockChMS",
//                Name = "Default",
//                FaviconUrl = "Themes/RockChMS/Assets/Icons/favicon.ico",
//                System = true,
//                AppleTouchIconUrl = "Themes/RockChMS/Assets/Icons/apple-touch.png",
//                FacebookAppId = "201981526511937",
//                Description = "Default Site",
//                Guid = new Guid( "C2D29296-6A87-47A9-A753-EE4E9159C4C4" )
//            };
//            context.Sites.AddOrUpdate( p => p.Name, defaultSite );

//            context.SaveChanges();

//            var defaultDomain = new SiteDomain
//            {
//                System = false,
//                SiteId = defaultSite.Id,
//                Domain = "localhost",
//                Guid = new Guid( "E2A9ACDA-B7E4-4942-B1D0-2CF536783A7A" )
//            };
//            context.SiteDomains.AddOrUpdate( p => p.Domain, defaultDomain );

//            context.SaveChanges();


//            //**************************************************************************
//            // Pages		(AUTO GENERATED BY TOOL)
//            //**************************************************************************

//            var page1_DefaultPage = new Page
//            {
//                SiteId = defaultSite.Id,
//                Name = "Default Page",
//                MenuDisplayChildPages = false,
//                DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
//                Title = "Default Page",
//                MenuDisplayDescription = false,
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = "Default",
//                RequiresEncryption = false,
//                EnableViewState = true,
//                Order = 1,
//                IncludeAdminFooter = true,
//                Guid = new Guid( "85F25819-E948-4960-9DDF-00F54D32444E" ),
//                Description = "This is the description of the default page",
//                MenuDisplayIcon = false
//            };
//            context.Pages.AddOrUpdate( p => p.Guid, page1_DefaultPage );

//            context.SaveChanges();

//            var page12_MainPageTest = new Page
//            {
//                SiteId = defaultSite.Id,
//                Name = "Main Page",
//                MenuDisplayChildPages = true,
//                DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
//                Title = "Rock ChMS",
//                MenuDisplayDescription = false,
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = "Default",
//                RequiresEncryption = false,
//                EnableViewState = false,
//                Order = 8,
//                IncludeAdminFooter = true,
//                Guid = new Guid( "20F97A93-7949-4C2A-8A5E-C756FE8585CA" ),
//                Description = "Main starting page for Rock ChMS",
//                MenuDisplayIcon = false
//            };
//            context.Pages.AddOrUpdate( p => p.Guid, page12_MainPageTest );

//            context.SaveChanges();

//            var page41_Communications = new Page
//            {
//                SiteId = defaultSite.Id,
//                Name = "Communications",
//                ParentPageId = page12_MainPageTest.Id,
//                MenuDisplayChildPages = true,
//                DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
//                Title = "Communications",
//                MenuDisplayDescription = false,
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = "Default",
//                RequiresEncryption = false,
//                EnableViewState = true,
//                Order = 2,
//                IncludeAdminFooter = true,
//                Guid = new Guid( "0319BDC8-8CBF-4673-B4B3-63B2BFCA2C3C" ),
//                MenuDisplayIcon = false
//            };
//            context.Pages.AddOrUpdate( p => p.Guid, page41_Communications );

//            context.SaveChanges();

//            var page44_Administration = new Page
//            {
//                SiteId = defaultSite.Id,
//                Name = "Administration",
//                ParentPageId = page12_MainPageTest.Id,
//                MenuDisplayChildPages = true,
//                DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
//                Title = "Administration",
//                MenuDisplayDescription = false,
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = "Default",
//                RequiresEncryption = false,
//                EnableViewState = true,
//                Order = 5,
//                IncludeAdminFooter = true,
//                Guid = new Guid( "84E12152-E456-478E-AF68-BA640E9CE65B" ),
//                MenuDisplayIcon = false
//            };
//            context.Pages.AddOrUpdate( p => p.Guid, page44_Administration );

//            context.SaveChanges();

//            var page48_Security = new Page
//            {
//                SiteId = defaultSite.Id,
//                Name = "Security",
//                ParentPageId = page44_Administration.Id,
//                MenuDisplayChildPages = false,
//                DisplayInNavWhen = CMS.DisplayInNavWhen.Never,
//                Title = "Security",
//                MenuDisplayDescription = false,
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = "Default",
//                RequiresEncryption = false,
//                EnableViewState = true,
//                Order = 3,
//                IncludeAdminFooter = true,
//                Guid = new Guid( "8C71A7E2-18A8-41C0-AB40-AD85CF90CA81" ),
//                MenuDisplayIcon = false
//            };
//            context.Pages.AddOrUpdate( p => p.Guid, page48_Security );

//            context.SaveChanges();

//            var page49_CMSAdministration = new Page
//            {
//                SiteId = defaultSite.Id,
//                Name = "CMS Administration",
//                ParentPageId = page44_Administration.Id,
//                MenuDisplayChildPages = true,
//                DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
//                Title = "CMS Administration",
//                MenuDisplayDescription = false,
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = "Default",
//                RequiresEncryption = false,
//                EnableViewState = true,
//                Order = 0,
//                IncludeAdminFooter = true,
//                Guid = new Guid( "550A898C-EDEA-48B5-9C58-B20EC13AF13B" ),
//                MenuDisplayIcon = false
//            };
//            context.Pages.AddOrUpdate( p => p.Guid, page49_CMSAdministration );

//            context.SaveChanges();

//            var page2_Sites = new Page
//            {
//                SiteId = defaultSite.Id,
//                Name = "Sites",
//                ParentPageId = page49_CMSAdministration.Id,
//                MenuDisplayChildPages = false,
//                DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
//                Title = "Sites",
//                MenuDisplayDescription = false,
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = "Default",
//                RequiresEncryption = false,
//                EnableViewState = true,
//                Order = 6,
//                IncludeAdminFooter = true,
//                Guid = new Guid( "7596D389-4EAB-4535-8BEE-229737F46F44" ),
//                Description = "Manage Sites",
//                MenuDisplayIcon = false
//            };
//            context.Pages.AddOrUpdate( p => p.Guid, page2_Sites );

//            context.SaveChanges();

//            var page7_Blocks = new Page
//            {
//                SiteId = defaultSite.Id,
//                Name = "Blocks",
//                ParentPageId = page49_CMSAdministration.Id,
//                MenuDisplayChildPages = true,
//                DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
//                Title = "Blocks",
//                MenuDisplayDescription = true,
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = "Default",
//                RequiresEncryption = false,
//                EnableViewState = true,
//                Order = 7,
//                IncludeAdminFooter = true,
//                Guid = new Guid( "5FBE9019-862A-41C6-ACDC-287D7934757D" ),
//                Description = "Manage Blocks",
//                MenuDisplayIcon = true
//            };
//            context.Pages.AddOrUpdate( p => p.Guid, page7_Blocks );

//            context.SaveChanges();

//            var page16_ZoneBlocks = new Page
//            {
//                SiteId = defaultSite.Id,
//                Name = "ZoneBlocks",
//                ParentPageId = page49_CMSAdministration.Id,
//                MenuDisplayChildPages = false,
//                DisplayInNavWhen = CMS.DisplayInNavWhen.Never,
//                Title = "Zone Blocks",
//                MenuDisplayDescription = false,
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = "Dialog",
//                RequiresEncryption = false,
//                EnableViewState = true,
//                Order = 9,
//                IncludeAdminFooter = false,
//                Guid = new Guid( "9F36531F-C1B5-4E23-8FA3-18B6DAFF1B0B" ),
//                Description = "Admin page for administering the blocks in a zone",
//                MenuDisplayIcon = false
//            };
//            context.Pages.AddOrUpdate( p => p.Guid, page16_ZoneBlocks );

//            context.SaveChanges();

//            var page23_BlockProperties = new Page
//            {
//                SiteId = defaultSite.Id,
//                Name = "Block Properties",
//                ParentPageId = page49_CMSAdministration.Id,
//                MenuDisplayChildPages = false,
//                DisplayInNavWhen = CMS.DisplayInNavWhen.Never,
//                Title = "Block Properties",
//                MenuDisplayDescription = false,
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = "Dialog",
//                RequiresEncryption = false,
//                EnableViewState = true,
//                Order = 11,
//                IncludeAdminFooter = false,
//                Guid = new Guid( "F0B34893-9550-4864-ADB4-EE860E4E427C" ),
//                Description = "Lists the attributes for a block instance",
//                MenuDisplayIcon = false
//            };
//            context.Pages.AddOrUpdate( p => p.Guid, page23_BlockProperties );

//            context.SaveChanges();

//            var page29_ChildPages = new Page
//            {
//                SiteId = defaultSite.Id,
//                Name = "ChildPages",
//                ParentPageId = page49_CMSAdministration.Id,
//                MenuDisplayChildPages = false,
//                DisplayInNavWhen = CMS.DisplayInNavWhen.Never,
//                Title = "Child Pages",
//                MenuDisplayDescription = false,
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = "Dialog",
//                RequiresEncryption = false,
//                EnableViewState = true,
//                Order = 14,
//                IncludeAdminFooter = false,
//                Guid = new Guid( "D58F205E-E9CC-4BD9-BC79-F3DA86F6E346" ),
//                Description = "Manage child pages",
//                MenuDisplayIcon = false
//            };
//            context.Pages.AddOrUpdate( p => p.Guid, page29_ChildPages );

//            context.SaveChanges();

//            var page37_PageProperties = new Page
//            {
//                SiteId = defaultSite.Id,
//                Name = "Page Properties",
//                ParentPageId = page49_CMSAdministration.Id,
//                MenuDisplayChildPages = false,
//                DisplayInNavWhen = CMS.DisplayInNavWhen.Never,
//                Title = "Page Properties",
//                MenuDisplayDescription = false,
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = "Dialog",
//                RequiresEncryption = false,
//                EnableViewState = true,
//                Order = 15,
//                IncludeAdminFooter = false,
//                Guid = new Guid( "37759B50-DB4A-440D-A83B-4EF3B4727B1E" ),
//                Description = "Page Properties",
//                MenuDisplayIcon = false
//            };
//            context.Pages.AddOrUpdate( p => p.Guid, page37_PageProperties );

//            context.SaveChanges();

//            var page51_GlobalAttributes = new Page
//            {
//                SiteId = defaultSite.Id,
//                Name = "Global Attributes",
//                ParentPageId = page49_CMSAdministration.Id,
//                MenuDisplayChildPages = false,
//                DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
//                Title = "Global Attributes",
//                MenuDisplayDescription = false,
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = "Default",
//                RequiresEncryption = false,
//                EnableViewState = false,
//                Order = 16,
//                IncludeAdminFooter = true,
//                Guid = new Guid( "A2753E03-96B1-4C83-AA11-FCD68C631571" ),
//                MenuDisplayIcon = false
//            };
//            context.Pages.AddOrUpdate( p => p.Guid, page51_GlobalAttributes );

//            context.SaveChanges();

//            var page52_GlobalValues = new Page
//            {
//                SiteId = defaultSite.Id,
//                Name = "Global Values",
//                ParentPageId = page49_CMSAdministration.Id,
//                MenuDisplayChildPages = false,
//                DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
//                Title = "Global Values",
//                MenuDisplayDescription = false,
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = "Default",
//                RequiresEncryption = false,
//                EnableViewState = false,
//                Order = 17,
//                IncludeAdminFooter = true,
//                Guid = new Guid( "D5550020-0BD0-43E6-806B-25338830F244" ),
//                MenuDisplayIcon = false
//            };
//            context.Pages.AddOrUpdate( p => p.Guid, page52_GlobalValues );

//            context.SaveChanges();

//            var page3_Login = new Page
//            {
//                SiteId = defaultSite.Id,
//                Name = "Login",
//                ParentPageId = page48_Security.Id,
//                MenuDisplayChildPages = false,
//                DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
//                Title = "Login",
//                MenuDisplayDescription = false,
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = "Splash",
//                RequiresEncryption = false,
//                EnableViewState = true,
//                Order = 0,
//                IncludeAdminFooter = true,
//                Guid = new Guid( "03CB988A-138C-448B-A43D-8891844EEB18" ),
//                Description = "Login",
//                MenuDisplayIcon = false
//            };
//            context.Pages.AddOrUpdate( p => p.Guid, page3_Login );

//            context.SaveChanges();

//            var page4_NewAccount = new Page
//            {
//                SiteId = defaultSite.Id,
//                Name = "New Account",
//                ParentPageId = page48_Security.Id,
//                MenuDisplayChildPages = false,
//                DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
//                Title = "Create Account ",
//                MenuDisplayDescription = false,
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = "Default",
//                RequiresEncryption = false,
//                EnableViewState = true,
//                Order = 2,
//                IncludeAdminFooter = true,
//                Guid = new Guid( "7D4E2142-D24E-4DD2-84BC-B34C5C3D0D46" ),
//                Description = "Create Account",
//                MenuDisplayIcon = false
//            };
//            context.Pages.AddOrUpdate( p => p.Guid, page4_NewAccount );

//            context.SaveChanges();

//            var page28_Security = new Page
//            {
//                SiteId = defaultSite.Id,
//                Name = "Security",
//                ParentPageId = page48_Security.Id,
//                MenuDisplayChildPages = false,
//                DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
//                Title = "Manage Security",
//                MenuDisplayDescription = false,
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = "Dialog",
//                RequiresEncryption = false,
//                EnableViewState = true,
//                Order = 13,
//                IncludeAdminFooter = false,
//                Guid = new Guid( "86D5E33E-E351-4CA5-9925-849C6C467257" ),
//                Description = "Used to manage security for an entity",
//                MenuDisplayIcon = false
//            };
//            context.Pages.AddOrUpdate( p => p.Guid, page28_Security );

//            context.SaveChanges();

//            var page54_Confirm = new Page
//            {
//                SiteId = defaultSite.Id,
//                Name = "Confirm",
//                ParentPageId = page48_Security.Id,
//                MenuDisplayChildPages = false,
//                DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
//                Title = "Confirm",
//                MenuDisplayDescription = false,
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = "Default",
//                RequiresEncryption = false,
//                EnableViewState = true,
//                Order = 14,
//                IncludeAdminFooter = true,
//                Guid = new Guid( "D73F83B4-E20E-4F95-9A2C-511FB669F44C" ),
//                MenuDisplayIcon = false
//            };
//            context.Pages.AddOrUpdate( p => p.Guid, page54_Confirm );

//            context.SaveChanges();

//            var page55_ChangePassword = new Page
//            {
//                SiteId = defaultSite.Id,
//                Name = "Change Password",
//                ParentPageId = page48_Security.Id,
//                MenuDisplayChildPages = false,
//                DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
//                Title = "Change Password",
//                MenuDisplayDescription = false,
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = "Default",
//                RequiresEncryption = false,
//                EnableViewState = true,
//                Order = 15,
//                IncludeAdminFooter = true,
//                Guid = new Guid( "4508223C-2989-4592-B764-B3F372B6051B" ),
//                MenuDisplayIcon = false
//            };
//            context.Pages.AddOrUpdate( p => p.Guid, page55_ChangePassword );

//            context.SaveChanges();

//            var page56_ForgotUserName = new Page
//            {
//                SiteId = defaultSite.Id,
//                Name = "Forgot User Name",
//                ParentPageId = page48_Security.Id,
//                MenuDisplayChildPages = false,
//                DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
//                Title = "Forgot User Name",
//                MenuDisplayDescription = false,
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = "Default",
//                RequiresEncryption = false,
//                EnableViewState = true,
//                Order = 16,
//                IncludeAdminFooter = true,
//                Guid = new Guid( "C6628FBD-F297-4C23-852E-40F1369C23A8" ),
//                MenuDisplayIcon = false
//            };
//            context.Pages.AddOrUpdate( p => p.Guid, page56_ForgotUserName );

//            context.SaveChanges();

//            var page57_MyAccount = new Page
//            {
//                SiteId = defaultSite.Id,
//                Name = "My Account",
//                ParentPageId = page48_Security.Id,
//                MenuDisplayChildPages = false,
//                DisplayInNavWhen = CMS.DisplayInNavWhen.WhenAllowed,
//                Title = "My Account",
//                MenuDisplayDescription = false,
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = "Default",
//                RequiresEncryption = false,
//                EnableViewState = true,
//                Order = 17,
//                IncludeAdminFooter = true,
//                Guid = new Guid( "290C53DC-0960-484C-B314-8301882A454C" ),
//                MenuDisplayIcon = false
//            };
//            context.Pages.AddOrUpdate( p => p.Guid, page57_MyAccount );

//            context.SaveChanges();

//            context.SaveChanges();

//            //**************************************************************************
//            // Page Routes		(AUTO GENERATED BY TOOL)
//            //**************************************************************************

//            var pageroute_Site_xxActionxx = new PageRoute
//            {
//                Route = @"Site/{Action}",
//                PageId = page2_Sites.Id,
//                Guid = new Guid( "6695D509-5EE5-4498-81A1-D0875AECC223" ),
//                System = true
//            };
//            context.PageRoutes.AddOrUpdate( p => p.Guid, pageroute_Site_xxActionxx );

//            var pageroute_Site_xxActionxx_xxSiteIdxx = new PageRoute
//            {
//                Route = @"Site/{Action}/{SiteId}",
//                PageId = page2_Sites.Id,
//                Guid = new Guid( "1580C995-BA49-4612-B291-C7D3EA35D180" ),
//                System = true
//            };
//            context.PageRoutes.AddOrUpdate( p => p.Guid, pageroute_Site_xxActionxx_xxSiteIdxx );

//            var pageroute_Sites = new PageRoute
//            {
//                Route = @"Sites",
//                PageId = page2_Sites.Id,
//                Guid = new Guid( "2444F430-A4BD-4B1F-B9BC-0C872814F77C" ),
//                System = true
//            };
//            context.PageRoutes.AddOrUpdate( p => p.Guid, pageroute_Sites );

//            var pageroute_Login = new PageRoute
//            {
//                Route = @"Login",
//                PageId = page3_Login.Id,
//                Guid = new Guid( "4257A25E-8F4B-4E6C-9E09-822804C01891" ),
//                System = true
//            };
//            context.PageRoutes.AddOrUpdate( p => p.Guid, pageroute_Login );

//            var pageroute_NewAccount = new PageRoute
//            {
//                Route = @"NewAccount",
//                PageId = page4_NewAccount.Id,
//                Guid = new Guid( "6B940C77-21C7-4F25-BEAC-C05152D30033" ),
//                System = true
//            };
//            context.PageRoutes.AddOrUpdate( p => p.Guid, pageroute_NewAccount );

//            var pageroute_Bloc_xxActionxx = new PageRoute
//            {
//                Route = @"Bloc/{Action}",
//                PageId = page7_Blocks.Id,
//                Guid = new Guid( "9C5F6CD4-E3BA-49B9-8065-129E486F682D" ),
//                System = true
//            };
//            context.PageRoutes.AddOrUpdate( p => p.Guid, pageroute_Bloc_xxActionxx );

//            var pageroute_Bloc_xxActionxx_xxBlockIdxx = new PageRoute
//            {
//                Route = @"Bloc/{Action}/{BlockId}",
//                PageId = page7_Blocks.Id,
//                Guid = new Guid( "D079CEBC-D820-46DB-8572-F36CE47D35BD" ),
//                System = true
//            };
//            context.PageRoutes.AddOrUpdate( p => p.Guid, pageroute_Bloc_xxActionxx_xxBlockIdxx );

//            var pageroute_Blocs = new PageRoute
//            {
//                Route = @"Blocs",
//                PageId = page7_Blocks.Id,
//                Guid = new Guid( "5DCF79F0-CBC3-40E9-BA21-51043BDF8573" ),
//                System = true
//            };
//            context.PageRoutes.AddOrUpdate( p => p.Guid, pageroute_Blocs );

//            var pageroute_ZoneBlocks_xxEditPagexx_xxZoneNamexx = new PageRoute
//            {
//                Route = @"ZoneBlocks/{EditPage}/{ZoneName}",
//                PageId = page16_ZoneBlocks.Id,
//                Guid = new Guid( "1F1A13E4-823C-43F7-B05F-AEBC012B7DDD" ),
//                System = true
//            };
//            context.PageRoutes.AddOrUpdate( p => p.Guid, pageroute_ZoneBlocks_xxEditPagexx_xxZoneNamexx );

//            var pageroute_BlockProperties_xxBlockInstancexx = new PageRoute
//            {
//                Route = @"BlockProperties/{BlockInstance}",
//                PageId = page23_BlockProperties.Id,
//                Guid = new Guid( "6438C940-96F7-4A7E-9DA5-A30FD4FF143A" ),
//                System = true
//            };
//            context.PageRoutes.AddOrUpdate( p => p.Guid, pageroute_BlockProperties_xxBlockInstancexx );

//            var pageroute_Secure_xxEntityTypexx_xxEntityIdxx = new PageRoute
//            {
//                Route = @"Secure/{EntityType}/{EntityId}",
//                PageId = page28_Security.Id,
//                Guid = new Guid( "45BD998E-68F8-4AAF-A094-12E4AEE217A3" ),
//                System = true
//            };
//            context.PageRoutes.AddOrUpdate( p => p.Guid, pageroute_Secure_xxEntityTypexx_xxEntityIdxx );

//            var pageroute_Pages_xxEditPagexx = new PageRoute
//            {
//                Route = @"Pages/{EditPage}",
//                PageId = page29_ChildPages.Id,
//                Guid = new Guid( "926DF96E-55DA-467A-864F-6740C04BA400" ),
//                System = true
//            };
//            context.PageRoutes.AddOrUpdate( p => p.Guid, pageroute_Pages_xxEditPagexx );

//            var pageroute_PageProperties_xxPagexx = new PageRoute
//            {
//                Route = @"PageProperties/{Page}",
//                PageId = page37_PageProperties.Id,
//                Guid = new Guid( "3676BB9B-C96F-4A8D-A418-FBE223020D8D" ),
//                System = true
//            };
//            context.PageRoutes.AddOrUpdate( p => p.Guid, pageroute_PageProperties_xxPagexx );

//            var pageroute_ChangePassword = new PageRoute
//            {
//                Route = @"ChangePassword",
//                PageId = page55_ChangePassword.Id,
//                Guid = new Guid( "5230092F-126D-4169-A060-3B65211DCB71" ),
//                System = true
//            };
//            context.PageRoutes.AddOrUpdate( p => p.Guid, pageroute_ChangePassword );

//            var pageroute_ConfirmAccount = new PageRoute
//            {
//                Route = @"ConfirmAccount",
//                PageId = page54_Confirm.Id,
//                Guid = new Guid( "3C084922-DF00-40E6-971B-72FF4234A54F" ),
//                System = true
//            };
//            context.PageRoutes.AddOrUpdate( p => p.Guid, pageroute_ConfirmAccount );

//            var pageroute_ForgotUserName = new PageRoute
//            {
//                Route = @"ForgotUserName",
//                PageId = page56_ForgotUserName.Id,
//                Guid = new Guid( "A56CF405-CA08-4991-A8D4-E90584E3ADAB" ),
//                System = true
//            };
//            context.PageRoutes.AddOrUpdate( p => p.Guid, pageroute_ForgotUserName );

//            var pageroute_MyAccount = new PageRoute
//            {
//                Route = @"MyAccount",
//                PageId = page57_MyAccount.Id,
//                Guid = new Guid( "8F2692BE-FE9A-4715-AA9F-B7412B2FE69A" ),
//                System = true
//            };
//            context.PageRoutes.AddOrUpdate( p => p.Guid, pageroute_MyAccount );

//            context.SaveChanges();


//            //**************************************************************************
//            // Sites		(AUTO GENERATED BY TOOL)
//            //**************************************************************************

//            var site_Default = new Site
//            {
//                Theme = @"RockChMS",
//                DefaultPageId = page12_MainPageTest.Id,
//                Name = @"Default",
//                FaviconUrl = @"Themes/RockChMS/Assets/Icons/favicon.ico",
//                System = true,
//                AppleTouchIconUrl = @"Themes/RockChMS/Assets/Icons/apple-touch.png",
//                Description = @"Default Site",
//                Guid = new Guid( "C2D29296-6A87-47A9-A753-EE4E9159C4C4" )
//            };
//            context.Sites.AddOrUpdate( p => p.Guid, site_Default );

//            context.SaveChanges();


//            //**************************************************************************
//            // Blocks		(AUTO GENERATED BY TOOL)
//            //**************************************************************************

//            var block_Sites = new Block
//            {
//                Name = @"Sites",
//                System = true,
//                Path = @"~/Blocks/Administration/Sites.ascx",
//                Guid = new Guid( "3E0AFD6E-3E3D-4FF9-9BC6-387AFBF9ACB6" ),
//                Description = @"Site Administration"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_Sites );

//            var block_Login = new Block
//            {
//                Name = @"Login",
//                System = true,
//                Path = @"~/Blocks/Security/Login.ascx",
//                Guid = new Guid( "7B83D513-1178-429E-93FF-E76430E038E4" ),
//                Description = @"Provides ability to login to site."
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_Login );

//            var block_CreateAccount = new Block
//            {
//                Name = @"Create Account",
//                System = true,
//                Path = @"~/Blocks/Security/CreateAccount.ascx",
//                Guid = new Guid( "292D3578-BC27-4DAB-BFC3-6D249E0905E0" ),
//                Description = @"Create new user accounts"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_CreateAccount );

//            var block_HTMLContent = new Block
//            {
//                Name = @"HTML Content",
//                System = true,
//                Path = @"~/Blocks/Cms/HtmlContent.ascx",
//                Guid = new Guid( "19B61D65-37E3-459F-A44F-DEF0089118A3" ),
//                Description = @"A block that displays HTML"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_HTMLContent );

//            var block_Roles = new Block
//            {
//                Name = @"Roles",
//                System = true,
//                Path = @"~/Blocks/Administration/Roles.ascx",
//                Guid = new Guid( "EA5F81B5-9086-4D34-8339-46D26B5F775E" ),
//                Description = @"Role Administration"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_Roles );

//            var block_Blocks = new Block
//            {
//                Name = @"Blocks",
//                System = true,
//                Path = @"~/Blocks/Administration/Blocks.ascx",
//                Guid = new Guid( "0244A072-6216-49F4-92EC-E6B5FFFF03B5" ),
//                Description = @"Block Administration"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_Blocks );

//            var block_BlogPosts = new Block
//            {
//                Name = @"Blog Posts",
//                System = true,
//                Path = @"~/Blocks/Blog/Posts.ascx",
//                Guid = new Guid( "E9918B7E-C108-4813-AE2B-9D5091644AAE" ),
//                Description = @"Blog Posts"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_BlogPosts );

//            var block_BlogCategories = new Block
//            {
//                Name = @"Blog Categories",
//                System = true,
//                Path = @"~/Blocks/Blog/Categories.ascx",
//                Guid = new Guid( "B13D3564-72D8-45F5-AE1D-20D0F600C93F" ),
//                Description = @"Used to list categories for a specific blog."
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_BlogCategories );

//            var block_BlogPostDisplay = new Block
//            {
//                Name = @"Blog Post Display",
//                System = true,
//                Path = @"~/Blocks/Blog/PostDisplay.ascx",
//                Guid = new Guid( "52FC470C-474C-495C-9620-E74A29AE846E" ),
//                Description = @"Used to show a post on a public site and allow for commenting."
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_BlogPostDisplay );

//            var block_PageXsltTransformation = new Block
//            {
//                Name = @"Page Xslt Transformation",
//                System = true,
//                Path = @"~/Blocks/Cms/PageXslt.ascx",
//                Guid = new Guid( "F49AD5F8-1E45-41E7-A88E-8CD285815BD9" ),
//                Description = @"Used for page navigation controls"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_PageXsltTransformation );

//            var block_ZoneBlocks = new Block
//            {
//                Name = @"Zone Blocks",
//                System = true,
//                Path = @"~/Blocks/Administration/ZoneBlocks.ascx",
//                Guid = new Guid( "72CAAF77-A015-45F0-A549-F941B9AB4D75" ),
//                Description = @"~/Blocks/Cms/ZoneBlocks.ascx"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_ZoneBlocks );

//            var block_BlockProperties = new Block
//            {
//                Name = @"Block Properties",
//                System = true,
//                Path = @"~/Blocks/Administration/BlockProperties.ascx",
//                Guid = new Guid( "5EC45388-83D4-4E99-BF25-3FA00327F08B" ),
//                Description = @"~/Blocks/Cms/BlockProperties.ascx"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_BlockProperties );

//            var block_Security = new Block
//            {
//                Name = @"Security",
//                System = true,
//                Path = @"~/Blocks/Administration/Security.ascx",
//                Guid = new Guid( "20474B3D-0DE7-4B63-B7B9-E042DBEF788C" ),
//                Description = @"~/Blocks/Cms/Security.ascx"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_Security );

//            var block_Pages = new Block
//            {
//                Name = @"Pages",
//                System = true,
//                Path = @"~/Blocks/Administration/Pages.ascx",
//                Guid = new Guid( "AEFC2DBE-37B6-4CAB-882C-B214F587BF2E" ),
//                Description = @"~/Blocks/Cms/Pages.ascx"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_Pages );

//            var block_PageProperties = new Block
//            {
//                Name = @"Page Properties",
//                System = true,
//                Path = @"~/Blocks/Administration/PageProperties.ascx",
//                Guid = new Guid( "C7988C3E-822D-4E73-882E-9B7684398BAA" ),
//                Description = @"Gives the user the ability to edit the properties for a page."
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_PageProperties );

//            var block_Geocoding = new Block
//            {
//                Name = @"Geocoding",
//                System = true,
//                Path = @"~/Blocks/Administration/Address/Geocoding.ascx",
//                Guid = new Guid( "340F1474-3403-4426-A8F0-2E33C1B4BF2F" ),
//                Description = @"~/Blocks/Administration/Address/Geocoding.ascx"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_Geocoding );

//            var block_Standardization = new Block
//            {
//                Name = @"Standardization",
//                System = true,
//                Path = @"~/Blocks/Administration/Address/Standardization.ascx",
//                Guid = new Guid( "363C3382-5148-4097-82B2-C85A4910A837" ),
//                Description = @"~/Blocks/Administration/Address/Standardization.ascx"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_Standardization );

//            var block_Redirect = new Block
//            {
//                Name = @"Redirect",
//                System = true,
//                Path = @"~/Blocks/Cms/Redirect.ascx",
//                Guid = new Guid( "B97FB779-5D3E-4663-B3B5-3C2C227AE14A" ),
//                Description = @"Redirects the user to a provided URL."
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_Redirect );

//            var block_LoginStatus = new Block
//            {
//                Name = @"Login Status",
//                System = true,
//                Path = @"~/Blocks/Security/LoginStatus.ascx",
//                Guid = new Guid( "04712F3D-9667-4901-A49D-4507573EF7AD" ),
//                Description = @"~/Blocks/Security/LoginStatus.ascx"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_LoginStatus );

//            var block_Attributes = new Block
//            {
//                Name = @"Attributes",
//                System = true,
//                Path = @"~/Blocks/Administration/Attributes.ascx",
//                Guid = new Guid( "E5EA2F6D-43A2-48E0-B59C-4409B78AC830" ),
//                Description = @"~/Blocks/Administration/Attributes.ascx"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_Attributes );

//            var block_AttributeValues = new Block
//            {
//                Name = @"Attribute Values",
//                System = true,
//                Path = @"~/Blocks/Administration/AttributeValues.ascx",
//                Guid = new Guid( "B084F060-ECE4-462A-B6D0-35B2A30AF3DF" ),
//                Description = @"~/Blocks/Administration/AttributeValues.ascx"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_AttributeValues );

//            var block_ConfirmAccount = new Block
//            {
//                Name = @"Confirm Account",
//                System = true,
//                Path = @"~/Blocks/Security/ConfirmAccount.ascx",
//                Guid = new Guid( "734DFF21-7465-4E02-BFC3-D40F7A65FB60" ),
//                Description = @"~/Blocks/Security/ConfirmAccount.ascx"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_ConfirmAccount );

//            var block_NewAccount = new Block
//            {
//                Name = @"New Account",
//                System = true,
//                Path = @"~/Blocks/Security/NewAccount.ascx",
//                Guid = new Guid( "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3" ),
//                Description = @"~/Blocks/Security/NewAccount.ascx"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_NewAccount );

//            var block_ChangePassword = new Block
//            {
//                Name = @"Change Password",
//                System = true,
//                Path = @"~/Blocks/Security/ChangePassword.ascx",
//                Guid = new Guid( "3C12DE99-2D1B-40F2-A9B8-6FE7C2524B37" ),
//                Description = @"~/Blocks/Security/ChangePassword.ascx"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_ChangePassword );

//            var block_ForgotUserName = new Block
//            {
//                Name = @"Forgot UserName",
//                System = true,
//                Path = @"~/Blocks/Security/ForgotUserName.ascx",
//                Guid = new Guid( "02B3D7D1-23CE-4154-B602-F4A15B321757" ),
//                Description = @"~/Blocks/Security/ForgotUserName.ascx"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_ForgotUserName );

//            var block_Components = new Block
//            {
//                Name = @"Components",
//                System = true,
//                Path = @"~/Blocks/Administration/Components.ascx",
//                Guid = new Guid( "21F5F466-59BC-40B2-8D73-7314D936C3CB" ),
//                Description = @"~/Blocks/Administration/Components.ascx"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_Components );

//            var block_EmailTemplates = new Block
//            {
//                Name = @"Email Templates",
//                System = true,
//                Path = @"~/Blocks/Administration/EmailTemplates.ascx",
//                Guid = new Guid( "10DC44E9-ECC1-4679-8A07-C098A0DCD82E" ),
//                Description = @"~/Blocks/Administration/EmailTemplates.ascx"
//            };
//            context.Blocks.AddOrUpdate( p => p.Guid, block_EmailTemplates );

//            context.SaveChanges();


//            //**************************************************************************
//            // BlockInstance		(AUTO GENERATED BY TOOL)
//            //**************************************************************************

//            var blockinstance_a = new BlockInstance
//            {
//                Guid = new Guid( "B31AE932-F065-4500-8524-2182431CD18C" ),
//                Name = @"a",
//                System = true,
//                OutputCacheDuration = 0,
//                Zone = @"Content",
//                Order = 0,
//                PageId = page2_Sites.Id,
//                BlockId = block_Sites.Id
//            };
//            context.BlockInstances.AddOrUpdate( p => p.Guid, blockinstance_a );

//            var blockinstance_b = new BlockInstance
//            {
//                Guid = new Guid( "3D325BB3-E1C9-4194-8E9B-11BFFC347DC3" ),
//                Name = @"b",
//                System = true,
//                OutputCacheDuration = 0,
//                Zone = @"Content",
//                Order = 0,
//                PageId = page3_Login.Id,
//                BlockId = block_Login.Id
//            };
//            context.BlockInstances.AddOrUpdate( p => p.Guid, blockinstance_b );

//            var blockinstance_d = new BlockInstance
//            {
//                Guid = new Guid( "74B3B85E-33B6-4ACF-8338-CBC12888BC74" ),
//                Name = @"d",
//                System = true,
//                OutputCacheDuration = 0,
//                Zone = @"Content",
//                Order = 0,
//                PageId = page7_Blocks.Id,
//                BlockId = block_Blocks.Id
//            };
//            context.BlockInstances.AddOrUpdate( p => p.Guid, blockinstance_d );

//            var blockinstance_Menu = new BlockInstance
//            {
//                Guid = new Guid( "CC8F4186-870D-4CF3-8226-D49F1A0D0DDF" ),
//                Name = @"Menu",
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = @"Default",
//                Zone = @"Menu",
//                Order = 1,
//                BlockId = block_PageXsltTransformation.Id
//            };
//            context.BlockInstances.AddOrUpdate( p => p.Guid, blockinstance_Menu );

//            var blockinstance_l = new BlockInstance
//            {
//                Guid = new Guid( "A5E0DD78-BB67-41E5-BDDA-73E2277482DA" ),
//                Name = @"l",
//                System = true,
//                OutputCacheDuration = 0,
//                Zone = @"Content",
//                Order = 0,
//                PageId = page16_ZoneBlocks.Id,
//                BlockId = block_ZoneBlocks.Id
//            };
//            context.BlockInstances.AddOrUpdate( p => p.Guid, blockinstance_l );

//            var blockinstance_Html = new BlockInstance
//            {
//                Guid = new Guid( "53E8E3A7-5CE8-493A-96AC-C1A7ECFCA5C7" ),
//                Name = @"Html",
//                System = true,
//                OutputCacheDuration = 0,
//                Zone = @"ContentRight",
//                Order = 0,
//                PageId = page1_DefaultPage.Id,
//                BlockId = block_HTMLContent.Id
//            };
//            context.BlockInstances.AddOrUpdate( p => p.Guid, blockinstance_Html );

//            var blockinstance_BlockProperties = new BlockInstance
//            {
//                Guid = new Guid( "3D128AE6-08DE-4108-A888-F97C66B21996" ),
//                Name = @"Block Properties",
//                System = true,
//                OutputCacheDuration = 0,
//                Zone = @"Content",
//                Order = 0,
//                PageId = page23_BlockProperties.Id,
//                BlockId = block_BlockProperties.Id
//            };
//            context.BlockInstances.AddOrUpdate( p => p.Guid, blockinstance_BlockProperties );

//            var blockinstance_Security = new BlockInstance
//            {
//                Guid = new Guid( "2CA65E58-5ED1-4BBB-B5E1-82F3EBA8EE0C" ),
//                Name = @"Security",
//                System = true,
//                OutputCacheDuration = 0,
//                Zone = @"Content",
//                Order = 0,
//                PageId = page28_Security.Id,
//                BlockId = block_Security.Id
//            };
//            context.BlockInstances.AddOrUpdate( p => p.Guid, blockinstance_Security );

//            var blockinstance_ChildPages = new BlockInstance
//            {
//                Guid = new Guid( "7B13773C-7477-42B6-B6B9-C69A19FB64EA" ),
//                Name = @"Child Pages",
//                System = true,
//                OutputCacheDuration = 0,
//                Zone = @"Content",
//                Order = 0,
//                PageId = page29_ChildPages.Id,
//                BlockId = block_Pages.Id
//            };
//            context.BlockInstances.AddOrUpdate( p => p.Guid, blockinstance_ChildPages );

//            var blockinstance_PageProperties = new BlockInstance
//            {
//                Guid = new Guid( "46376422-C33D-4FFF-8005-E7116781B466" ),
//                Name = @"Page Properties",
//                System = true,
//                OutputCacheDuration = 0,
//                Zone = @"Content",
//                Order = 0,
//                PageId = page37_PageProperties.Id,
//                BlockId = block_PageProperties.Id
//            };
//            context.BlockInstances.AddOrUpdate( p => p.Guid, blockinstance_PageProperties );

//            var blockinstance_Welcome = new BlockInstance
//            {
//                Guid = new Guid( "5F0DBB84-BFEF-43ED-9E51-E245DC85B7B5" ),
//                Name = @"Welcome",
//                System = true,
//                OutputCacheDuration = 0,
//                Zone = @"Content",
//                Order = 0,
//                PageId = page12_MainPageTest.Id,
//                BlockId = block_HTMLContent.Id
//            };
//            context.BlockInstances.AddOrUpdate( p => p.Guid, blockinstance_Welcome );

//            var blockinstance_LoginStatus = new BlockInstance
//            {
//                Guid = new Guid( "C62E5EE6-104E-4741-86DF-B9484F597C2C" ),
//                Name = @"Login Status",
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = @"Default",
//                Zone = @"zHeader",
//                Order = 0,
//                BlockId = block_LoginStatus.Id
//            };
//            context.BlockInstances.AddOrUpdate( p => p.Guid, blockinstance_LoginStatus );

//            var blockinstance_OrgSettings = new BlockInstance
//            {
//                Guid = new Guid( "3CBB177B-DBFB-4FB2-A1A7-957DC6C350EB" ),
//                Name = @"Org Settings",
//                System = true,
//                OutputCacheDuration = 0,
//                Zone = @"Content",
//                Order = 0,
//                PageId = page51_GlobalAttributes.Id,
//                BlockId = block_Attributes.Id
//            };
//            context.BlockInstances.AddOrUpdate( p => p.Guid, blockinstance_OrgSettings );

//            var blockinstance_OrgValues = new BlockInstance
//            {
//                Guid = new Guid( "89FE9265-3087-4334-B990-CE06E91925EE" ),
//                Name = @"Org Values",
//                System = true,
//                OutputCacheDuration = 0,
//                Zone = @"Content",
//                Order = 0,
//                PageId = page52_GlobalValues.Id,
//                BlockId = block_AttributeValues.Id
//            };
//            context.BlockInstances.AddOrUpdate( p => p.Guid, blockinstance_OrgValues );

//            var blockinstance_FooterContent = new BlockInstance
//            {
//                Guid = new Guid( "3FCA7657-A18C-400A-8A5A-29A8680C15E6" ),
//                Name = @"Footer Content",
//                System = true,
//                OutputCacheDuration = 0,
//                Layout = @"Default",
//                Zone = @"Footer",
//                Order = 0,
//                BlockId = block_HTMLContent.Id
//            };
//            context.BlockInstances.AddOrUpdate( p => p.Guid, blockinstance_FooterContent );

//            var blockinstance_NewAccount = new BlockInstance
//            {
//                Guid = new Guid( "0D192BF7-584A-4229-84A6-A02C7CACEEBF" ),
//                Name = @"New Account",
//                System = true,
//                OutputCacheDuration = 0,
//                Zone = @"Content",
//                Order = 0,
//                PageId = page4_NewAccount.Id,
//                BlockId = block_NewAccount.Id
//            };
//            context.BlockInstances.AddOrUpdate( p => p.Guid, blockinstance_NewAccount );

//            var blockinstance_Confirm = new BlockInstance
//            {
//                Guid = new Guid( "3137BF4E-5735-4DB7-B708-3B6F80DA5505" ),
//                Name = @"Confirm",
//                System = true,
//                OutputCacheDuration = 0,
//                Zone = @"Content",
//                Order = 0,
//                PageId = page54_Confirm.Id,
//                BlockId = block_ConfirmAccount.Id
//            };
//            context.BlockInstances.AddOrUpdate( p => p.Guid, blockinstance_Confirm );

//            var blockinstance_RestPassword = new BlockInstance
//            {
//                Guid = new Guid( "150496F4-4798-4BB5-B796-405DE11B5ED1" ),
//                Name = @"Rest Password",
//                System = true,
//                OutputCacheDuration = 0,
//                Zone = @"Content",
//                Order = 0,
//                PageId = page55_ChangePassword.Id,
//                BlockId = block_ChangePassword.Id
//            };
//            context.BlockInstances.AddOrUpdate( p => p.Guid, blockinstance_RestPassword );

//            var blockinstance_ForgotUserName = new BlockInstance
//            {
//                Guid = new Guid( "33C61662-B42A-483B-91F1-C10955C5E5A9" ),
//                Name = @"Forgot User Name",
//                System = true,
//                OutputCacheDuration = 0,
//                Zone = @"Content",
//                Order = 0,
//                PageId = page56_ForgotUserName.Id,
//                BlockId = block_ForgotUserName.Id
//            };
//            context.BlockInstances.AddOrUpdate( p => p.Guid, blockinstance_ForgotUserName );

//            context.SaveChanges();


//            //**************************************************************************
//            // FieldTypes		(AUTO GENERATED BY TOOL)
//            //**************************************************************************

//            var fieldtype_Text = new FieldType
//            {
//                Name = @"Text",
//                System = true,
//                Assembly = @"Rock",
//                Guid = new Guid( "9C204CD0-1233-41C5-818A-C5DA439445AA" ),
//                Class = @"Rock.FieldTypes.Text",
//                Description = @"A Text Field"
//            };
//            context.FieldTypes.AddOrUpdate( p => p.Guid, fieldtype_Text );

//            var fieldtype_MultixxSelect = new FieldType
//            {
//                Name = @"Multi-Select",
//                System = true,
//                Assembly = @"Rock",
//                Guid = new Guid( "BD0D9B57-2A41-4490-89FF-F01DAB7D4904" ),
//                Class = @"Rock.FieldTypes.SelectMulti",
//                Description = @"Renders a list of checkboxes for user to select one or more values from"
//            };
//            context.FieldTypes.AddOrUpdate( p => p.Guid, fieldtype_MultixxSelect );

//            var fieldtype_Boolean = new FieldType
//            {
//                Name = @"Boolean",
//                System = true,
//                Assembly = @"Rock",
//                Guid = new Guid( "1EDAFDED-DFE6-4334-B019-6EECBA89E05A" ),
//                Class = @"Rock.FieldTypes.Boolean",
//                Description = @"True / False"
//            };
//            context.FieldTypes.AddOrUpdate( p => p.Guid, fieldtype_Boolean );

//            var fieldtype_Color = new FieldType
//            {
//                Name = @"Color",
//                System = true,
//                Assembly = @"Rock",
//                Guid = new Guid( "D747E6AE-C383-4E22-8846-71518E3DD06F" ),
//                Class = @"Rock.FieldTypes.Color",
//                Description = @"List of colors to choose from"
//            };
//            context.FieldTypes.AddOrUpdate( p => p.Guid, fieldtype_Color );

//            var fieldtype_SinglexxSelect = new FieldType
//            {
//                Name = @"Single-Select",
//                System = true,
//                Assembly = @"Rock",
//                Guid = new Guid( "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0" ),
//                Class = @"Rock.FieldTypes.SelectSingle",
//                Description = @"Renders either a drop down list, or a list of radio-buttons for user to select one value from"
//            };
//            context.FieldTypes.AddOrUpdate( p => p.Guid, fieldtype_SinglexxSelect );

//            var fieldtype_Interger = new FieldType
//            {
//                Name = @"Interger",
//                System = true,
//                Assembly = @"Rock",
//                Guid = new Guid( "A75DFC58-7A1B-4799-BF31-451B2BBE38FF" ),
//                Class = @"Rock.FieldTypes.Integer",
//                Description = @"An integer value (whole number)."
//            };
//            context.FieldTypes.AddOrUpdate( p => p.Guid, fieldtype_Interger );

//            var fieldtype_PageReference = new FieldType
//            {
//                Name = @"Page Reference",
//                System = true,
//                Assembly = @"Rock",
//                Guid = new Guid( "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108" ),
//                Class = @"Rock.FieldTypes.PageReference",
//                Description = @"A reference to a specific page and route"
//            };
//            context.FieldTypes.AddOrUpdate( p => p.Guid, fieldtype_PageReference );

//            var fieldtype_Image = new FieldType
//            {
//                Name = @"Image",
//                System = true,
//                Assembly = @"Rock",
//                Guid = new Guid( "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D" ),
//                Class = @"Rock.FieldTypes.Image",
//                Description = @"An image stored in the database."
//            };
//            context.FieldTypes.AddOrUpdate( p => p.Guid, fieldtype_Image );

//            var fieldtype_Date = new FieldType
//            {
//                Name = @"Date",
//                System = true,
//                Assembly = @"Rock",
//                Guid = new Guid( "6B6AA175-4758-453F-8D83-FCD8044B5F36" ),
//                Class = @"Rock.FieldTypes.Date",
//                Description = @"A date field"
//            };
//            context.FieldTypes.AddOrUpdate( p => p.Guid, fieldtype_Date );

//            var fieldtype_Video = new FieldType
//            {
//                Name = @"Video",
//                System = true,
//                Assembly = @"Rock",
//                Guid = new Guid( "FA398F9D-5B01-41EA-9A93-112F910A277D" ),
//                Class = @"Rock.FieldTypes.Video",
//                Description = @"A Video field"
//            };
//            context.FieldTypes.AddOrUpdate( p => p.Guid, fieldtype_Video );

//            context.SaveChanges();


//            //**************************************************************************
//            // DefinedTypes		(AUTO GENERATED BY TOOL)
//            //**************************************************************************

//            var definedtype_RecordType = new DefinedType
//            {
//                Description = @"Record Types",
//                Order = 0,
//                Name = @"Record Type",
//                Category = @"Person",
//                Guid = new Guid( "26BE73A6-A9C5-4E94-AE00-3AFDCF8C9275" ),
//                System = true
//            };
//            context.DefinedTypes.AddOrUpdate( p => p.Guid, definedtype_RecordType );

//            var definedtype_RecordStatus = new DefinedType
//            {
//                Description = @"Record Status",
//                Order = 0,
//                Name = @"Record Status",
//                Category = @"Person",
//                Guid = new Guid( "8522BADD-2871-45A5-81DD-C76DA07E2E7E" ),
//                System = true
//            };
//            context.DefinedTypes.AddOrUpdate( p => p.Guid, definedtype_RecordStatus );

//            var definedtype_RecordStatusReason = new DefinedType
//            {
//                Description = @"Record Status Reason",
//                Order = 0,
//                Name = @"Record Status Reason",
//                Category = @"Person",
//                Guid = new Guid( "E17D5988-0372-4792-82CF-9E37C79F7319" ),
//                System = true
//            };
//            context.DefinedTypes.AddOrUpdate( p => p.Guid, definedtype_RecordStatusReason );

//            var definedtype_Status = new DefinedType
//            {
//                Description = @"Status",
//                Order = 0,
//                Name = @"Status",
//                Category = @"Person",
//                Guid = new Guid( "2E6540EA-63F0-40FE-BE50-F2A84735E600" ),
//                System = true
//            };
//            context.DefinedTypes.AddOrUpdate( p => p.Guid, definedtype_Status );

//            var definedtype_Title = new DefinedType
//            {
//                Description = @"Person Title (i.e. Mr., Mrs., Dr., etc)",
//                Order = 0,
//                Name = @"Title",
//                Category = @"Person",
//                Guid = new Guid( "4784CD23-518B-43EE-9B97-225BF6E07846" ),
//                System = true
//            };
//            context.DefinedTypes.AddOrUpdate( p => p.Guid, definedtype_Title );

//            var definedtype_Suffix = new DefinedType
//            {
//                Description = @"Person Suffix (i.e. Sr., Jr. etc)",
//                Order = 0,
//                Name = @"Suffix",
//                Category = @"Person",
//                Guid = new Guid( "16F85B3C-B3E8-434C-9094-F3D41F87A740" ),
//                System = true
//            };
//            context.DefinedTypes.AddOrUpdate( p => p.Guid, definedtype_Suffix );

//            var definedtype_MaritalStatus = new DefinedType
//            {
//                Description = @"Marital Status",
//                Order = 0,
//                Name = @"Marital Status",
//                Category = @"Person",
//                Guid = new Guid( "B4B92C3F-A935-40E1-A00B-BA484EAD613B" ),
//                System = true
//            };
//            context.DefinedTypes.AddOrUpdate( p => p.Guid, definedtype_MaritalStatus );

//            context.SaveChanges();


//            //**************************************************************************
//            // DefinedValues		(AUTO GENERATED BY TOOL)
//            //**************************************************************************

//            var definedvalue_Person = new DefinedValue
//            {
//                Order = 0,
//                Name = @"Person",
//                DefinedTypeId = definedtype_RecordType.Id,
//                Guid = new Guid( "36CF10D6-C695-413D-8E7C-4546EFEF385E" ),
//                Description = @"A Person Record",
//                System = true
//            };
//            context.DefinedValues.AddOrUpdate( p => p.Guid, definedvalue_Person );

//            var definedvalue_Business = new DefinedValue
//            {
//                Order = 1,
//                Name = @"Business",
//                DefinedTypeId = definedtype_RecordType.Id,
//                Guid = new Guid( "BF64ADD3-E70A-44CE-9C4B-E76BBED37550" ),
//                Description = @"A Business Record",
//                System = true
//            };
//            context.DefinedValues.AddOrUpdate( p => p.Guid, definedvalue_Business );

//            var definedvalue_Active = new DefinedValue
//            {
//                Order = 0,
//                Name = @"Active",
//                DefinedTypeId = definedtype_RecordStatus.Id,
//                Guid = new Guid( "618F906C-C33D-4FA3-8AEF-E58CB7B63F1E" ),
//                Description = @"An Active Record",
//                System = true
//            };
//            context.DefinedValues.AddOrUpdate( p => p.Guid, definedvalue_Active );

//            var definedvalue_Inactive = new DefinedValue
//            {
//                Order = 1,
//                Name = @"Inactive",
//                DefinedTypeId = definedtype_RecordStatus.Id,
//                Guid = new Guid( "1DAD99D5-41A9-4865-8366-F269902B80A4" ),
//                Description = @"An Inactive Record",
//                System = true
//            };
//            context.DefinedValues.AddOrUpdate( p => p.Guid, definedvalue_Inactive );

//            var definedvalue_Pending = new DefinedValue
//            {
//                Order = 3,
//                Name = @"Pending",
//                DefinedTypeId = definedtype_RecordStatus.Id,
//                Guid = new Guid( "283999EC-7346-42E3-B807-BCE9B2BABB49" ),
//                Description = @"Pending Record Status",
//                System = true
//            };
//            context.DefinedValues.AddOrUpdate( p => p.Guid, definedvalue_Pending );

//            context.SaveChanges();


//            //**************************************************************************
//            // Core Attributes		(AUTO GENERATED BY TOOL)
//            //**************************************************************************

//            var attribute_PrexxText = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Pre-Text",
//                GridColumn = true,
//                MultiValue = false,
//                Key = @"PreText",
//                System = true,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 0,
//                Description = @"HTML text to render before the blocks main content.",
//                Guid = new Guid( "15E874B8-FF76-40FB-8713-6D0C98609734" ),
//                DefaultValue = @"",
//                EntityQualifierValue = block_HTMLContent.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_PrexxText );

//            context.SaveChanges();

//            var attributevalue_8641d846xx0e8axx4921xxaa25xx918bd488c927 = new AttributeValue
//            {
//                AttributeId = attribute_PrexxText.Id,
//                System = false,
//                Guid = new Guid( "8641D846-0E8A-4921-AA25-918BD488C927" ),
//                EntityId = blockinstance_Html.Id
//            };
//            context.AttributeValues.AddOrUpdate( p => p.Guid, attributevalue_8641d846xx0e8axx4921xxaa25xx918bd488c927 );

//            var attributevalue_e0cc4365xxaf31xx4e31xxb3f6xx8fd9bfd50b8a = new AttributeValue
//            {
//                AttributeId = attribute_PrexxText.Id,
//                System = false,
//                Guid = new Guid( "E0CC4365-AF31-4E31-B3F6-8FD9BFD50B8A" ),
//                EntityId = blockinstance_Welcome.Id
//            };
//            context.AttributeValues.AddOrUpdate( p => p.Guid, attributevalue_e0cc4365xxaf31xx4e31xxb3f6xx8fd9bfd50b8a );


//            var attribute_PostxxText = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Post-Text",
//                GridColumn = true,
//                MultiValue = false,
//                Key = @"PostText",
//                System = true,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 1,
//                Description = @"HTML text to render after the blocks main content.",
//                Guid = new Guid( "2E9AF795-68FE-4BD6-AF8B-8848CD796AF5" ),
//                DefaultValue = @"",
//                EntityQualifierValue = block_HTMLContent.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_PostxxText );

//            context.SaveChanges();

//            var attributevalue_fcef61fdxxb5dfxx4935xx9b80xxd46f4c5c211d = new AttributeValue
//            {
//                AttributeId = attribute_PostxxText.Id,
//                System = false,
//                Guid = new Guid( "FCEF61FD-B5DF-4935-9B80-D46F4C5C211D" ),
//                EntityId = blockinstance_Html.Id
//            };
//            context.AttributeValues.AddOrUpdate( p => p.Guid, attributevalue_fcef61fdxxb5dfxx4935xx9b80xxd46f4c5c211d );

//            var attributevalue_538ef869xxee20xx4134xx905dxx5fdf70301ef7 = new AttributeValue
//            {
//                AttributeId = attribute_PostxxText.Id,
//                System = false,
//                Guid = new Guid( "538EF869-EE20-4134-905D-5FDF70301EF7" ),
//                EntityId = blockinstance_Welcome.Id
//            };
//            context.AttributeValues.AddOrUpdate( p => p.Guid, attributevalue_538ef869xxee20xx4134xx905dxx5fdf70301ef7 );


//            var attribute_PostsPerPage = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Posts Per Page",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"PostsPerPage",
//                System = true,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 1,
//                Description = @"The number of posts to display on a single page.",
//                Guid = new Guid( "CE716F50-095B-4F53-9794-57D29C7D490C" ),
//                DefaultValue = @"5",
//                EntityQualifierValue = block_BlogPosts.Id.ToString(),
//                FieldTypeId = fieldtype_Interger.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_PostsPerPage );

//            var attribute_Heading = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Heading",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Heading",
//                System = true,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 1,
//                Description = @"The heading to show above the list of categories.",
//                Guid = new Guid( "427B8BF7-0A43-4C98-99AB-B65595D686FF" ),
//                DefaultValue = @"Categories",
//                EntityQualifierValue = block_BlogCategories.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Heading );

//            var attribute_PostDetailPage = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Post Detail Page",
//                GridColumn = true,
//                MultiValue = false,
//                Key = @"PostDetailPage",
//                System = true,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 2,
//                Description = @"Page reference to the post details page",
//                Guid = new Guid( "E998F606-EBD4-4E7D-8AFE-9A92C145F101" ),
//                DefaultValue = @"-1,-1",
//                EntityQualifierValue = block_BlogPosts.Id.ToString(),
//                FieldTypeId = fieldtype_PageReference.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_PostDetailPage );

//            var attribute_EnableFacebookLogin = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Enable Facebook Login",
//                GridColumn = true,
//                MultiValue = false,
//                Key = @"FacebookEnabled",
//                System = true,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 0,
//                Description = @"Enables the user to login using Facebook.  This assumes that the site is configured with both a Facebook App Id and Secret.",
//                Guid = new Guid( "C5192287-F27E-4B91-97B5-A1C15490A4B9" ),
//                DefaultValue = @"True",
//                EntityQualifierValue = block_Login.Id.ToString(),
//                FieldTypeId = fieldtype_Boolean.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_EnableFacebookLogin );

//            var attribute_RootPage = new Rock.Core.Attribute
//            {
//                Category = @"XML",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Root Page",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"RootPage",
//                System = true,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 1,
//                Description = @"The root page to use for the page collection. Defaults to the current page instance if not set.",
//                Guid = new Guid( "DD516FA7-966E-4C80-8523-BEAC91C8EEDA" ),
//                DefaultValue = @"",
//                EntityQualifierValue = block_PageXsltTransformation.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_RootPage );

//            context.SaveChanges();

//            var attributevalue_12 = new AttributeValue
//            {
//                AttributeId = attribute_RootPage.Id,
//                System = false,
//                Guid = new Guid( "A6C3A68F-407A-4F59-A3DE-A8751A0A0858" ),
//                Value = page12_MainPageTest.ToString(),
//                EntityId = blockinstance_Menu.Id
//            };
//            context.AttributeValues.AddOrUpdate( p => p.Guid, attributevalue_12 );


//            var attribute_XSLTFile = new Rock.Core.Attribute
//            {
//                Category = @"Menu XSLT",
//                EntityQualifierColumn = @"BlockId",
//                Required = true,
//                Name = @"XSLT File",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"XSLTFile",
//                System = true,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 0,
//                Description = @"The path to the XSLT File ",
//                Guid = new Guid( "D8A029F8-83BE-454A-99D3-94D879EBF87C" ),
//                DefaultValue = @"~/Assets/XSLT/PageList.xslt",
//                EntityQualifierValue = block_PageXsltTransformation.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_XSLTFile );

//            context.SaveChanges();

//            var attributevalue_xx_Assets_XSLT_PageNavxxxslt = new AttributeValue
//            {
//                AttributeId = attribute_XSLTFile.Id,
//                System = true,
//                Guid = new Guid( "DD1473B4-9657-4EF4-B499-B8724685A89E" ),
//                Value = @"~/Assets/XSLT/PageNav.xslt",
//                EntityId = blockinstance_Menu.Id
//            };
//            context.AttributeValues.AddOrUpdate( p => p.Guid, attributevalue_xx_Assets_XSLT_PageNavxxxslt );


//            var attribute_NumberofLevels = new Rock.Core.Attribute
//            {
//                Category = @"XML",
//                EntityQualifierColumn = @"BlockId",
//                Required = true,
//                Name = @"Number of Levels",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"NumberofLevels",
//                System = true,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 2,
//                Description = @"Number of parent-child page levels to display. Default 3.",
//                Guid = new Guid( "9909E07F-0E68-43B8-A151-24D03C795093" ),
//                DefaultValue = @"3",
//                EntityQualifierValue = block_PageXsltTransformation.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_NumberofLevels );

//            var attribute_JobPulse = new Rock.Core.Attribute
//            {
//                Category = @"Jobs",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Job Pulse",
//                GridColumn = true,
//                MultiValue = false,
//                Key = @"JobPulse",
//                System = true,
//                Entity = @"",
//                Order = 1,
//                Description = @"Date and time the last job pulse job ran.  This job allows an administrator to be notified if the jobs stop running.",
//                Guid = new Guid( "254F45EE-071C-4337-A522-DFDC20B7966A" ),
//                DefaultValue = @"1/1/1900",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_JobPulse );

//            var attribute_Port = new Rock.Core.Attribute
//            {
//                Category = @"Email Server",
//                EntityQualifierColumn = @"Class",
//                Required = true,
//                Name = @"Port",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"EmailServerPort",
//                System = false,
//                Entity = @"Rock.Util.Job",
//                Order = 1,
//                Description = @"Port of the email server",
//                Guid = new Guid( "8D27E44D-A088-496E-B708-75BE93BA6651" ),
//                DefaultValue = @"25",
//                EntityQualifierValue = @"Rock.Jobs.TestJob",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Port );

//            var attribute_Domain = new Rock.Core.Attribute
//            {
//                Category = @"Email Server",
//                EntityQualifierColumn = @"Class",
//                Required = true,
//                Name = @"Domain",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"EmailServer",
//                System = false,
//                Entity = @"Rock.Util.Job",
//                Order = 0,
//                Description = @"Domain name of your SMTP server",
//                Guid = new Guid( "9C06AA19-EE09-4ABA-A200-FB15443D3BDC" ),
//                DefaultValue = @"smtp.yourdomain.com",
//                EntityQualifierValue = @"Rock.Jobs.TestJob",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Domain );

//            var attribute_LicenseKey = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"License Key",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"LicenseKey",
//                System = false,
//                Entity = @"Rock.Address.Geocode.ServiceObjects",
//                Order = 2,
//                Description = @"The Service Objects License Key",
//                Guid = new Guid( "72CCD974-783D-49F9-AA37-0E609153DB58" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_LicenseKey );

//            var attribute_Order = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Order",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Order",
//                System = false,
//                Entity = @"Rock.Address.Geocode.ServiceObjects",
//                Order = 0,
//                Description = @"The order that this service should be used (priority)",
//                Guid = new Guid( "590F48E8-4B53-497A-956C-69D2813BAEE9" ),
//                DefaultValue = @"0",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Order );

//            var attribute_Order_36 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Order",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Order",
//                System = false,
//                Entity = @"Rock.Address.Geocode.StrikeIron",
//                Order = 0,
//                Description = @"The order that this service should be used (priority)",
//                Guid = new Guid( "5262A57F-B46D-4E69-8570-622FF6A6F0B5" ),
//                DefaultValue = @"0",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Order_36 );

//            var attribute_UserID = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"User ID",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"UserID",
//                System = false,
//                Entity = @"Rock.Address.Geocode.StrikeIron",
//                Order = 1,
//                Description = @"The Strike Iron User ID",
//                Guid = new Guid( "D9C9259C-5CEE-461A-87B6-2DA1DC0718F5" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_UserID );

//            var attribute_Password = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"Password",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Password",
//                System = false,
//                Entity = @"Rock.Address.Geocode.StrikeIron",
//                Order = 2,
//                Description = @"The Strike Iron Password",
//                Guid = new Guid( "50517637-8AD0-4EEB-B991-D22734EF7815" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Password );

//            var attribute_Active = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Active",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Active",
//                System = false,
//                Entity = @"Rock.Address.Geocode.ServiceObjects",
//                Order = 0,
//                Description = @"Should Service be used?",
//                Guid = new Guid( "19E4BF22-8094-4766-9253-9408B521B032" ),
//                DefaultValue = @"False",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Boolean.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Active );

//            var attribute_Active_41 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Active",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Active",
//                System = false,
//                Entity = @"Rock.Address.Geocode.StrikeIron",
//                Order = 0,
//                Description = @"Should Service be used?",
//                Guid = new Guid( "D79C10AB-A799-4311-8F23-914BA7C60533" ),
//                DefaultValue = @"False",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Boolean.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Active_41 );

//            var attribute_UserID_42 = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"User ID",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"UserID",
//                System = false,
//                Entity = @"Rock.Address.Standardize.StrikeIron",
//                Order = 1,
//                Description = @"The Strike Iron User ID",
//                Guid = new Guid( "9301064D-87AE-4690-BD27-4BB010567973" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_UserID_42 );

//            var attribute_Password_43 = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"Password",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Password",
//                System = false,
//                Entity = @"Rock.Address.Standardize.StrikeIron",
//                Order = 2,
//                Description = @"The Strike Iron Password",
//                Guid = new Guid( "34EDAF13-628B-4AF9-822C-C32A5A206A12" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Password_43 );

//            var attribute_Active_44 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Active",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Active",
//                System = false,
//                Entity = @"Rock.Address.Standardize.StrikeIron",
//                Order = 0,
//                Description = @"Should Service be used?",
//                Guid = new Guid( "80388551-C6E9-426A-9493-5ABFE80345A6" ),
//                DefaultValue = @"False",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Boolean.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Active_44 );

//            var attribute_Order_45 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Order",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Order",
//                System = false,
//                Entity = @"Rock.Address.Standardize.StrikeIron",
//                Order = 0,
//                Description = @"The order that this service should be used (priority)",
//                Guid = new Guid( "5C6019A9-3D14-400E-B2AD-F512F52BAD96" ),
//                DefaultValue = @"0",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Order_45 );

//            var attribute_CustomerId = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"Customer Id",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"CustomerId",
//                System = false,
//                Entity = @"Rock.Address.Standardize.MelissaData",
//                Order = 1,
//                Description = @"The Melissa Data Customer ID",
//                Guid = new Guid( "3E8309DF-B3AD-428F-ADBF-2D3BAB03BC7E" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_CustomerId );

//            var attribute_Order_50 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Order",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Order",
//                System = false,
//                Entity = @"Rock.Address.Standardize.MelissaData",
//                Order = 0,
//                Description = @"The order that this service should be used (priority)",
//                Guid = new Guid( "FEC235D7-07A9-4FFB-B8F9-49ABC25B6272" ),
//                DefaultValue = @"0",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Order_50 );

//            var attribute_Active_51 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Active",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Active",
//                System = false,
//                Entity = @"Rock.Address.Standardize.MelissaData",
//                Order = 0,
//                Description = @"Should Service be used?",
//                Guid = new Guid( "BFB6D110-7B0D-43EA-8B8A-BD4BD54243D0" ),
//                DefaultValue = @"False",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Boolean.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Active_51 );

//            var attribute_UserName = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"User Name",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"UserName",
//                System = false,
//                Entity = @"Rock.Address.Geocode.TelaAtlas",
//                Order = 1,
//                Description = @"The Tele Atlas User Name",
//                Guid = new Guid( "B968F46D-BE79-457A-A763-5CF0891863E6" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_UserName );

//            var attribute_Password_53 = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"Password",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Password",
//                System = false,
//                Entity = @"Rock.Address.Geocode.TelaAtlas",
//                Order = 2,
//                Description = @"The Tele Atlas Password",
//                Guid = new Guid( "7485FC34-D23C-4267-B0C7-0097017652F4" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Password_53 );

//            var attribute_Order_54 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Order",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Order",
//                System = false,
//                Entity = @"Rock.Address.Geocode.TelaAtlas",
//                Order = 0,
//                Description = @"The order that this service should be used (priority)",
//                Guid = new Guid( "CD533421-F3E9-480A-B37F-EB19E83E92D2" ),
//                DefaultValue = @"0",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Order_54 );

//            var attribute_Active_55 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Active",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Active",
//                System = false,
//                Entity = @"Rock.Address.Geocode.TelaAtlas",
//                Order = 0,
//                Description = @"Should Service be used?",
//                Guid = new Guid( "7A9209A2-3071-4327-9D7D-AB466D923BB7" ),
//                DefaultValue = @"False",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Boolean.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Active_55 );

//            var attribute_EZxxLocateService = new Rock.Core.Attribute
//            {
//                Category = @"Service",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"EZ-Locate Service",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"EZLocateService",
//                System = false,
//                Entity = @"Rock.Address.Geocode.TelaAtlas",
//                Order = 2,
//                Description = @"The EZ-Locate Service to use (default: USA_Geo_002)",
//                Guid = new Guid( "C72B7EAD-E8A1-4558-B63B-85A8656EF0E3" ),
//                DefaultValue = @"USA_Geo_002",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_EZxxLocateService );

//            var attribute_EntityQualifierColumn = new Rock.Core.Attribute
//            {
//                Category = @"Applies To",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Entity Qualifier Column",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"EntityQualifierColumn",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 1,
//                Description = @"The entity column to evaluate when determining if this attribute applies to the entity",
//                Guid = new Guid( "ECD5B86C-2B48-4548-9FE9-7AC6F6FA8106" ),
//                DefaultValue = @"",
//                EntityQualifierValue = block_Attributes.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_EntityQualifierColumn );

//            var attribute_EntityQualifierValue = new Rock.Core.Attribute
//            {
//                Category = @"Applies To",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Entity Qualifier Value",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"EntityQualifierValue",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 2,
//                Description = @"The entity column value to evaluate.  Attributes will only apply to entities with this value",
//                Guid = new Guid( "FCE1E87D-F816-4AD5-AE60-1E71942C547C" ),
//                DefaultValue = @"",
//                EntityQualifierValue = block_Attributes.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_EntityQualifierValue );

//            var attribute_Entity = new Rock.Core.Attribute
//            {
//                Category = @"Applies To",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Entity",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Entity",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 0,
//                Description = @"Entity Name",
//                Guid = new Guid( "5B33FE25-6BF0-4890-91C6-49FB1629221E" ),
//                DefaultValue = @"",
//                EntityQualifierValue = block_Attributes.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Entity );

//            var attribute_EntityQualifierValue_60 = new Rock.Core.Attribute
//            {
//                Category = @"Applies To",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Entity Qualifier Value",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"EntityQualifierValue",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 2,
//                Description = @"The entity column value to evaluate.  Attributes will only apply to entities with this value",
//                Guid = new Guid( "26280214-AAA0-475D-B3E2-F887085551C5" ),
//                DefaultValue = @"",
//                EntityQualifierValue = block_AttributeValues.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_EntityQualifierValue_60 );

//            var attribute_Entity_61 = new Rock.Core.Attribute
//            {
//                Category = @"Applies To",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Entity",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Entity",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 0,
//                Description = @"Entity Name",
//                Guid = new Guid( "981D5D1C-504D-4A0B-8DC7-6D01F4E51AF8" ),
//                DefaultValue = @"",
//                EntityQualifierValue = block_AttributeValues.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Entity_61 );

//            var attribute_EntityId = new Rock.Core.Attribute
//            {
//                Category = @"Entity",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Entity Id",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"EntityId",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 2,
//                Description = @"The entity id that values apply to",
//                Guid = new Guid( "680ED2DB-049B-4958-B36A-F4F0CCDF6DAA" ),
//                DefaultValue = @"",
//                EntityQualifierValue = block_AttributeValues.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_EntityId );

//            var attribute_EntityQualifierColumn_63 = new Rock.Core.Attribute
//            {
//                Category = @"Applies To",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Entity Qualifier Column",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"EntityQualifierColumn",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 1,
//                Description = @"The entity column to evaluate when determining if this attribute applies to the entity",
//                Guid = new Guid( "070A37AB-1F87-4F21-B27E-BCCD83F5DB7E" ),
//                DefaultValue = @"",
//                EntityQualifierValue = block_AttributeValues.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_EntityQualifierColumn_63 );

//            var attribute_SentLogin = new Rock.Core.Attribute
//            {
//                Category = @"Captions",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Sent Login",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"SentLoginCaption",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 4,
//                Description = @"",
//                Guid = new Guid( "4A2C2A1B-F0CE-483A-82B6-54EC740AE0EE" ),
//                DefaultValue = @"Your username has been emailed to you.  If you've forgotten your password, the email includes a link to reset your password.",
//                EntityQualifierValue = block_NewAccount.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_SentLogin );

//            context.SaveChanges();

//            var attributevalue_YourusernamehasbeenemailedtoyouxxIfyouxxveforgotte = new AttributeValue
//            {
//                AttributeId = attribute_SentLogin.Id,
//                System = true,
//                Guid = new Guid( "64A1BD51-C8ED-4CE9-83D6-1B02187EFB7E" ),
//                Value = @"Your username has been emailed to you.  If you've forgotten your password, the email includes a link to reset your password.",
//                EntityId = blockinstance_NewAccount.Id
//            };
//            context.AttributeValues.AddOrUpdate( p => p.Guid, attributevalue_YourusernamehasbeenemailedtoyouxxIfyouxxveforgotte );


//            var attribute_Confirm = new Rock.Core.Attribute
//            {
//                Category = @"Captions",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Confirm",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"ConfirmCaption",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 5,
//                Description = @"",
//                Guid = new Guid( "27A35511-8263-41E1-88F8-F284E2339248" ),
//                DefaultValue = @"Because you've selected an existing person, we need to have you confirm the email address you entered belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue.",
//                EntityQualifierValue = block_NewAccount.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Confirm );

//            context.SaveChanges();

//            var attributevalue_Becauseyouxxveselectedanexistingpersonxxweneedtoha = new AttributeValue
//            {
//                AttributeId = attribute_Confirm.Id,
//                System = true,
//                Guid = new Guid( "DF12A2E5-E7C1-4D13-95FA-D9A2B6433D3A" ),
//                Value = @"Because you've selected an existing person, we need to have you confirm the email address you entered belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue.",
//                EntityId = blockinstance_NewAccount.Id
//            };
//            context.AttributeValues.AddOrUpdate( p => p.Guid, attributevalue_Becauseyouxxveselectedanexistingpersonxxweneedtoha );


//            var attribute_Success = new Rock.Core.Attribute
//            {
//                Category = @"Captions",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Success",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"SuccessCaption",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 6,
//                Description = @"",
//                Guid = new Guid( "9C8FF3E7-2B9A-4652-AB7D-BD7D570AC68F" ),
//                DefaultValue = @"{0}, Your account has been created",
//                EntityQualifierValue = block_NewAccount.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Success );

//            context.SaveChanges();

//            var attributevalue_xx0xxxxYouraccounthasbeencreated = new AttributeValue
//            {
//                AttributeId = attribute_Success.Id,
//                System = true,
//                Guid = new Guid( "603AC593-F9AA-4BB5-9C8E-E6C3DABD72C5" ),
//                Value = @"{0}, Your account has been created",
//                EntityId = blockinstance_NewAccount.Id
//            };
//            context.AttributeValues.AddOrUpdate( p => p.Guid, attributevalue_xx0xxxxYouraccounthasbeencreated );


//            var attribute_CheckforDuplicates = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Check for Duplicates",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Duplicates",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 0,
//                Description = @"Should people with the same email and last name be presented as a possible pre-existing record for user to choose from.",
//                Guid = new Guid( "029DA832-10EC-49C9-8A16-B10126759A9A" ),
//                DefaultValue = @"true",
//                EntityQualifierValue = block_NewAccount.Id.ToString(),
//                FieldTypeId = fieldtype_Boolean.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_CheckforDuplicates );

//            context.SaveChanges();

//            var attributevalue_True_60 = new AttributeValue
//            {
//                AttributeId = attribute_CheckforDuplicates.Id,
//                System = false,
//                Guid = new Guid( "C3B3E4CA-577B-4417-BBD1-9302DE921B72" ),
//                Value = @"True",
//                EntityId = blockinstance_NewAccount.Id
//            };
//            context.AttributeValues.AddOrUpdate( p => p.Guid, attributevalue_True_60 );


//            var attribute_FoundDuplicate = new Rock.Core.Attribute
//            {
//                Category = @"Captions",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Found Duplicate",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"FoundDuplicateCaption",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 2,
//                Description = @"",
//                Guid = new Guid( "65FCFD11-9FEE-42F5-9CF5-A6237462D2CB" ),
//                DefaultValue = @"There are already one or more people in our system that have the same email address and last name as you do.  Are any of these people you?",
//                EntityQualifierValue = block_NewAccount.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_FoundDuplicate );

//            context.SaveChanges();

//            var attributevalue_Therearealreadyoneormorepeopleinoursystemthathavet = new AttributeValue
//            {
//                AttributeId = attribute_FoundDuplicate.Id,
//                System = true,
//                Guid = new Guid( "F2A02D0B-37BE-46D4-AA83-0A767CAB8630" ),
//                Value = @"There are already one or more people in our system that have the same email address and last name as you do.  Are any of these people you?",
//                EntityId = blockinstance_NewAccount.Id
//            };
//            context.AttributeValues.AddOrUpdate( p => p.Guid, attributevalue_Therearealreadyoneormorepeopleinoursystemthathavet );


//            var attribute_ExistingAccount = new Rock.Core.Attribute
//            {
//                Category = @"Captions",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Existing Account",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"ExistingAccountCaption",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 3,
//                Description = @"",
//                Guid = new Guid( "3465DB21-3139-4EAA-933D-FB40CC5B2AB7" ),
//                DefaultValue = @"{0}, you already have an existing account.  Would you like us to email you the username?",
//                EntityQualifierValue = block_NewAccount.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_ExistingAccount );

//            context.SaveChanges();

//            var attributevalue_xx0xxxxyoualreadyhaveanexistingaccountxxWouldyouli = new AttributeValue
//            {
//                AttributeId = attribute_ExistingAccount.Id,
//                System = true,
//                Guid = new Guid( "B788129C-0164-4FAF-9A56-67D1E8D3598A" ),
//                Value = @"{0}, you already have an existing account.  Would you like us to email you the username?",
//                EntityId = blockinstance_NewAccount.Id
//            };
//            context.AttributeValues.AddOrUpdate( p => p.Guid, attributevalue_xx0xxxxyoualreadyhaveanexistingaccountxxWouldyouli );


//            var attribute_OrganizationName = new Rock.Core.Attribute
//            {
//                Category = @"Organization",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Organization Name",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"OrganizationName",
//                System = false,
//                Entity = @"",
//                Order = 0,
//                Description = @"The name of your organization",
//                Guid = new Guid( "410BF494-0714-4E60-AFBD-AD65899A12BE" ),
//                DefaultValue = @"Our Organization Name",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_OrganizationName );

//            var attribute_Invalid = new Rock.Core.Attribute
//            {
//                Category = @"Captions",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Invalid",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"InvalidCaption",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 5,
//                Description = @"",
//                Guid = new Guid( "ADD1758C-31EA-4E75-8F33-468181D2ECDE" ),
//                DefaultValue = @"The confirmation code you've entered is not valid.  Please enter a valid confirmation code or <a href='{0}'>create a new account</a>",
//                EntityQualifierValue = block_ConfirmAccount.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Invalid );

//            var attribute_Delete = new Rock.Core.Attribute
//            {
//                Category = @"Captions",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Delete",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"DeleteCaption",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 3,
//                Description = @"",
//                Guid = new Guid( "72386CE2-DF26-4FBB-BA42-5C5EAFCAAC94" ),
//                DefaultValue = @"Are you sure you want to delete the '{0}' account?",
//                EntityQualifierValue = block_ConfirmAccount.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Delete );

//            var attribute_Deleted = new Rock.Core.Attribute
//            {
//                Category = @"Captions",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Deleted",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"DeletedCaption",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 4,
//                Description = @"",
//                Guid = new Guid( "9279A85F-1443-4F42-960C-33CB0F608111" ),
//                DefaultValue = @"The account has been deleted.",
//                EntityQualifierValue = block_ConfirmAccount.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Deleted );

//            var attribute_Confirmed = new Rock.Core.Attribute
//            {
//                Category = @"Captions",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Confirmed",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"ConfirmedCaption",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 0,
//                Description = @"",
//                Guid = new Guid( "22FCB059-AA40-45D9-BECB-C22D15A3D41A" ),
//                DefaultValue = @"{0}, Your account has been confirmed.  Thank you for creating the account",
//                EntityQualifierValue = block_ConfirmAccount.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Confirmed );

//            var attribute_SMTPServer = new Rock.Core.Attribute
//            {
//                Category = @"EmailConfig",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"SMTP Server",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"SMTPServer",
//                System = false,
//                Entity = @"",
//                Order = 10,
//                Description = @"The server to use for relaying SMTP Messages",
//                Guid = new Guid( "1C4E71DD-ED38-4586-93CF-A847003EC594" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_SMTPServer );

//            var attribute_SMTPPort = new Rock.Core.Attribute
//            {
//                Category = @"EmailConfig",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"SMTP Port",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"SMTPPort",
//                System = false,
//                Entity = @"",
//                Order = 11,
//                Description = @"The Port to use for SMTP Relaying",
//                Guid = new Guid( "3C5F2BF8-8D8A-46D4-9182-2A25D32851EA" ),
//                DefaultValue = @"25",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Interger.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_SMTPPort );

//            var attribute_SMTPUserName = new Rock.Core.Attribute
//            {
//                Category = @"EmailConfig",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"SMTP User Name",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"SMTPUserName",
//                System = false,
//                Entity = @"",
//                Order = 12,
//                Description = @"The Username to use when relaying SMTP Messages",
//                Guid = new Guid( "40690F08-1433-4046-8F22-B4B16075F1CF" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_SMTPUserName );

//            var attribute_SMTPPassword = new Rock.Core.Attribute
//            {
//                Category = @"EmailConfig",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"SMTP Password",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"SMTPPassword",
//                System = false,
//                Entity = @"",
//                Order = 13,
//                Description = @"The password to use when relaying SMTP Messages",
//                Guid = new Guid( "996B04C9-45E5-4DC1-A84B-27D14B53DCC6" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_SMTPPassword );

//            var attribute_SMTPUseSSL = new Rock.Core.Attribute
//            {
//                Category = @"EmailConfig",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"SMTP Use SSL",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"SMTPUseSSL",
//                System = false,
//                Entity = @"",
//                Order = 14,
//                Description = @"Should SSL be used when relaying SMTP Messages",
//                Guid = new Guid( "10DD8248-DC68-4206-ABFD-DA4E8BB849E3" ),
//                DefaultValue = @"False",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Boolean.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_SMTPUseSSL );

//            var attribute_ConfirmRoute = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"BlockId",
//                Required = true,
//                Name = @"Confirm Route",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"ConfirmRoute",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 1,
//                Description = @"The URL Route for Confirming an account",
//                Guid = new Guid( "B9E9EE84-7B64-4AC2-9C61-8228700954BA" ),
//                DefaultValue = @"",
//                EntityQualifierValue = block_NewAccount.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_ConfirmRoute );

//            context.SaveChanges();

//            var attributevalue_Page_54 = new AttributeValue
//            {
//                AttributeId = attribute_ConfirmRoute.Id,
//                System = false,
//                Guid = new Guid( "334CDE25-6067-44F3-8C03-11DFBDB03EFA" ),
//                Value = @"Page/54",
//                EntityId = blockinstance_NewAccount.Id
//            };
//            context.AttributeValues.AddOrUpdate( p => p.Guid, attributevalue_Page_54 );


//            var attribute_Success_81 = new Rock.Core.Attribute
//            {
//                Category = @"Captions",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Success",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"SuccessCaption",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 2,
//                Description = @"",
//                Guid = new Guid( "727FE7DA-C624-4590-94E1-C206324725CB" ),
//                DefaultValue = @"Your password has been changed",
//                EntityQualifierValue = block_ChangePassword.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Success_81 );

//            var attribute_InvalidUserName = new Rock.Core.Attribute
//            {
//                Category = @"Captions",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Invalid UserName",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"InvalidUserNameCaption",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 0,
//                Description = @"",
//                Guid = new Guid( "B254930D-9BBD-4BB5-B25F-84088A9DCE28" ),
//                DefaultValue = @"The User Name/Password combination is not valid.",
//                EntityQualifierValue = block_ChangePassword.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_InvalidUserName );

//            var attribute_InvalidPassword = new Rock.Core.Attribute
//            {
//                Category = @"Captions",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Invalid Password",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"InvalidPasswordCaption",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 1,
//                Description = @"",
//                Guid = new Guid( "979A3DCA-146E-47E7-BF08-DC7025DC8E22" ),
//                DefaultValue = @"The User Name/Password combination is not valid.",
//                EntityQualifierValue = block_ChangePassword.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_InvalidPassword );

//            var attribute_ResetPassword = new Rock.Core.Attribute
//            {
//                Category = @"Captions",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Reset Password",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"ResetPasswordCaption",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 1,
//                Description = @"",
//                Guid = new Guid( "C3FB6A2B-7711-4C52-AF10-0B1511198CDD" ),
//                DefaultValue = @"{0}, Enter a new password for your '{1}' account",
//                EntityQualifierValue = block_ConfirmAccount.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_ResetPassword );

//            var attribute_PasswordReset = new Rock.Core.Attribute
//            {
//                Category = @"Captions",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Password Reset",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"PasswordResetCaption",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 2,
//                Description = @"",
//                Guid = new Guid( "040D281F-B535-452E-B59E-7C45985C2937" ),
//                DefaultValue = @"{0}, The password for your '{1}' account has been changed",
//                EntityQualifierValue = block_ConfirmAccount.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_PasswordReset );

//            var attribute_Success_86 = new Rock.Core.Attribute
//            {
//                Category = @"Captions",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Success",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"SuccessCaption",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 2,
//                Description = @"",
//                Guid = new Guid( "488E438F-3BA3-4D3B-A1B0-D11D85752E06" ),
//                DefaultValue = @"Your user name has been sent to the email address you entered",
//                EntityQualifierValue = block_ForgotUserName.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Success_86 );

//            var attribute_Heading_87 = new Rock.Core.Attribute
//            {
//                Category = @"Captions",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Heading",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"HeadingCaption",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 0,
//                Description = @"",
//                Guid = new Guid( "6EFAF3CD-327A-4472-AA20-09AF1EF8BC78" ),
//                DefaultValue = @"Enter your email address below and we'll send you your account user name",
//                EntityQualifierValue = block_ForgotUserName.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Heading_87 );

//            var attribute_InvalidEmail = new Rock.Core.Attribute
//            {
//                Category = @"Captions",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Invalid Email",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"InvalidEmailCaption",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 1,
//                Description = @"",
//                Guid = new Guid( "87E7485A-FF22-48E7-BB4A-58E66B305D62" ),
//                DefaultValue = @"There are not any accounts for the email address you entered",
//                EntityQualifierValue = block_ForgotUserName.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_InvalidEmail );

//            var attribute_Password_89 = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"Password",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Password",
//                System = false,
//                Entity = @"Rock.MEF.Geocode.TelaAtlas",
//                Order = 2,
//                Description = @"The Tele Atlas Password",
//                Guid = new Guid( "90432AF9-F419-412E-87C4-23D0F91AC02A" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Password_89 );

//            var attribute_EZxxLocateService_90 = new Rock.Core.Attribute
//            {
//                Category = @"Service",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"EZ-Locate Service",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"EZLocateService",
//                System = false,
//                Entity = @"Rock.MEF.Geocode.TelaAtlas",
//                Order = 2,
//                Description = @"The EZ-Locate Service to use (default: USA_Geo_002)",
//                Guid = new Guid( "51F322BA-6B8F-43E8-B78D-37AD75640997" ),
//                DefaultValue = @"USA_Geo_002",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_EZxxLocateService_90 );

//            var attribute_UserName_91 = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"User Name",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"UserName",
//                System = false,
//                Entity = @"Rock.MEF.Geocode.TelaAtlas",
//                Order = 1,
//                Description = @"The Tele Atlas User Name",
//                Guid = new Guid( "D36AE1C2-1A2B-484B-B0A2-50AE37ACC3D6" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_UserName_91 );

//            var attribute_Active_92 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Active",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Active",
//                System = false,
//                Entity = @"Rock.MEF.Geocode.TelaAtlas",
//                Order = 0,
//                Description = @"Should Service be used?",
//                Guid = new Guid( "EDF90832-4F9C-41C4-BDD1-367BF0887D8D" ),
//                DefaultValue = @"False",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Boolean.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Active_92 );

//            var attribute_Order_93 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Order",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Order",
//                System = false,
//                Entity = @"Rock.MEF.Geocode.TelaAtlas",
//                Order = 0,
//                Description = @"The order that this service should be used (priority)",
//                Guid = new Guid( "5DA149E7-1043-40B4-A288-266271F5CC6D" ),
//                DefaultValue = @"0",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Order_93 );

//            var attribute_LicenseKey_94 = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"License Key",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"LicenseKey",
//                System = false,
//                Entity = @"Rock.MEF.Geocode.ServiceObjects",
//                Order = 2,
//                Description = @"The Service Objects License Key",
//                Guid = new Guid( "44E19CEC-ED1C-4997-A7B0-F30A2C83CB89" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_LicenseKey_94 );

//            var attribute_Active_95 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Active",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Active",
//                System = false,
//                Entity = @"Rock.MEF.Geocode.ServiceObjects",
//                Order = 0,
//                Description = @"Should Service be used?",
//                Guid = new Guid( "BC31ED74-D1EF-45F6-809C-CE5F5788DFA3" ),
//                DefaultValue = @"False",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Boolean.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Active_95 );

//            var attribute_Order_96 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Order",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Order",
//                System = false,
//                Entity = @"Rock.MEF.Geocode.ServiceObjects",
//                Order = 0,
//                Description = @"The order that this service should be used (priority)",
//                Guid = new Guid( "6EC59763-50BF-4EEA-8424-ACDA293F01C5" ),
//                DefaultValue = @"0",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Order_96 );

//            var attribute_UserID_97 = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"User ID",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"UserID",
//                System = false,
//                Entity = @"Rock.MEF.Geocode.StrikeIron",
//                Order = 1,
//                Description = @"The Strike Iron User ID",
//                Guid = new Guid( "82F06C17-F314-4086-8FF8-2E75A5C68A99" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_UserID_97 );

//            var attribute_Password_98 = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"Password",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Password",
//                System = false,
//                Entity = @"Rock.MEF.Geocode.StrikeIron",
//                Order = 2,
//                Description = @"The Strike Iron Password",
//                Guid = new Guid( "48CB3154-B2BC-43A2-9F1E-CEA9E81C1C18" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Password_98 );

//            var attribute_Active_99 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Active",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Active",
//                System = false,
//                Entity = @"Rock.MEF.Geocode.StrikeIron",
//                Order = 0,
//                Description = @"Should Service be used?",
//                Guid = new Guid( "342FB5B8-B551-415D-93F3-1AA89B3E5BC5" ),
//                DefaultValue = @"False",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Boolean.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Active_99 );

//            var attribute_Order_100 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Order",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Order",
//                System = false,
//                Entity = @"Rock.MEF.Geocode.StrikeIron",
//                Order = 0,
//                Description = @"The order that this service should be used (priority)",
//                Guid = new Guid( "16FA8CC4-FDF8-4E1B-B7BD-BEC602E6C563" ),
//                DefaultValue = @"0",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Order_100 );

//            var attribute_Password_101 = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"Password",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Password",
//                System = false,
//                Entity = @"Rock.MEF.Standardize.StrikeIron",
//                Order = 2,
//                Description = @"The Strike Iron Password",
//                Guid = new Guid( "F108D456-5C77-42D0-A7EB-EA1189B6EF2A" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Password_101 );

//            var attribute_UserID_102 = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"User ID",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"UserID",
//                System = false,
//                Entity = @"Rock.MEF.Standardize.StrikeIron",
//                Order = 1,
//                Description = @"The Strike Iron User ID",
//                Guid = new Guid( "BEC1B322-09E0-4333-943D-B9170EDBDDBB" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_UserID_102 );

//            var attribute_Active_103 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Active",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Active",
//                System = false,
//                Entity = @"Rock.MEF.Standardize.StrikeIron",
//                Order = 0,
//                Description = @"Should Service be used?",
//                Guid = new Guid( "0B8E0E4B-4A6C-4DA8-BB65-E3D2B38BC44E" ),
//                DefaultValue = @"False",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Boolean.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Active_103 );

//            var attribute_Order_104 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Order",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Order",
//                System = false,
//                Entity = @"Rock.MEF.Standardize.StrikeIron",
//                Order = 0,
//                Description = @"The order that this service should be used (priority)",
//                Guid = new Guid( "84C1E686-B9A0-41ED-87F4-FF2482103B7E" ),
//                DefaultValue = @"0",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Order_104 );

//            var attribute_CustomerId_105 = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"Customer Id",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"CustomerId",
//                System = false,
//                Entity = @"Rock.MEF.Standardize.MelissaData",
//                Order = 1,
//                Description = @"The Melissa Data Customer ID",
//                Guid = new Guid( "B728AC57-729B-4596-833A-64914D609096" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_CustomerId_105 );

//            var attribute_Active_106 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Active",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Active",
//                System = false,
//                Entity = @"Rock.MEF.Standardize.MelissaData",
//                Order = 0,
//                Description = @"Should Service be used?",
//                Guid = new Guid( "BF0424E3-26E2-46F7-B289-D050AD733294" ),
//                DefaultValue = @"False",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Boolean.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Active_106 );

//            var attribute_Order_107 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Order",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Order",
//                System = false,
//                Entity = @"Rock.MEF.Standardize.MelissaData",
//                Order = 0,
//                Description = @"The order that this service should be used (priority)",
//                Guid = new Guid( "0F607291-E63E-4766-BDF6-C5BB88B57DF9" ),
//                DefaultValue = @"0",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Order_107 );

//            var attribute_EZxxLocateService_108 = new Rock.Core.Attribute
//            {
//                Category = @"Service",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"EZ-Locate Service",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"EZLocateService",
//                System = false,
//                Entity = @"Rock.Component.Geocode.TelaAtlas",
//                Order = 2,
//                Description = @"The EZ-Locate Service to use (default: USA_Geo_002)",
//                Guid = new Guid( "A7F71469-5739-460F-9ADB-DC399CD9B1A5" ),
//                DefaultValue = @"USA_Geo_002",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_EZxxLocateService_108 );

//            var attribute_UserName_109 = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"User Name",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"UserName",
//                System = false,
//                Entity = @"Rock.Component.Geocode.TelaAtlas",
//                Order = 1,
//                Description = @"The Tele Atlas User Name",
//                Guid = new Guid( "AD5FA137-5622-49AF-844E-6A973EC0403A" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_UserName_109 );

//            var attribute_Password_110 = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"Password",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Password",
//                System = false,
//                Entity = @"Rock.Component.Geocode.TelaAtlas",
//                Order = 2,
//                Description = @"The Tele Atlas Password",
//                Guid = new Guid( "7F251CFF-1FEF-407D-9722-8840B6918C30" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Password_110 );

//            var attribute_Order_111 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Order",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Order",
//                System = false,
//                Entity = @"Rock.Component.Geocode.TelaAtlas",
//                Order = 0,
//                Description = @"The order that this service should be used (priority)",
//                Guid = new Guid( "0B4688A9-37D6-48AC-BEF8-64FF63971CC9" ),
//                DefaultValue = @"0",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Order_111 );

//            var attribute_Active_112 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Active",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Active",
//                System = false,
//                Entity = @"Rock.Component.Geocode.TelaAtlas",
//                Order = 0,
//                Description = @"Should Service be used?",
//                Guid = new Guid( "EA32844C-C0D1-456A-B1B9-110A271CE9A8" ),
//                DefaultValue = @"False",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Boolean.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Active_112 );

//            var attribute_LicenseKey_113 = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"License Key",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"LicenseKey",
//                System = false,
//                Entity = @"Rock.Component.Geocode.ServiceObjects",
//                Order = 2,
//                Description = @"The Service Objects License Key",
//                Guid = new Guid( "0CE7689E-FB9F-4A48-8542-744384290320" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_LicenseKey_113 );

//            var attribute_Order_114 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Order",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Order",
//                System = false,
//                Entity = @"Rock.Component.Geocode.ServiceObjects",
//                Order = 0,
//                Description = @"The order that this service should be used (priority)",
//                Guid = new Guid( "0F9D7966-9D0A-420C-A0AE-260C82613A3D" ),
//                DefaultValue = @"0",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Order_114 );

//            var attribute_Active_115 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Active",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Active",
//                System = false,
//                Entity = @"Rock.Component.Geocode.ServiceObjects",
//                Order = 0,
//                Description = @"Should Service be used?",
//                Guid = new Guid( "0627AE3B-F2D6-44FA-8B6F-1441F2E46B8D" ),
//                DefaultValue = @"False",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Boolean.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Active_115 );

//            var attribute_UserID_116 = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"User ID",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"UserID",
//                System = false,
//                Entity = @"Rock.Component.Geocode.StrikeIron",
//                Order = 1,
//                Description = @"The Strike Iron User ID",
//                Guid = new Guid( "ECACB606-0F86-41DE-928A-BB995113714D" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_UserID_116 );

//            var attribute_Password_117 = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"Password",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Password",
//                System = false,
//                Entity = @"Rock.Component.Geocode.StrikeIron",
//                Order = 2,
//                Description = @"The Strike Iron Password",
//                Guid = new Guid( "93987B6A-343A-42BF-92C7-EA7C9533B092" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Password_117 );

//            var attribute_Order_118 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Order",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Order",
//                System = false,
//                Entity = @"Rock.Component.Geocode.StrikeIron",
//                Order = 0,
//                Description = @"The order that this service should be used (priority)",
//                Guid = new Guid( "FF93823D-0ACD-4E45-8B90-3ADD4A20B79C" ),
//                DefaultValue = @"0",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Order_118 );

//            var attribute_Active_119 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Active",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Active",
//                System = false,
//                Entity = @"Rock.Component.Geocode.StrikeIron",
//                Order = 0,
//                Description = @"Should Service be used?",
//                Guid = new Guid( "04B0C668-5725-4180-9342-26C7862D7577" ),
//                DefaultValue = @"False",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Boolean.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Active_119 );

//            var attribute_UserID_120 = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"User ID",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"UserID",
//                System = false,
//                Entity = @"Rock.Component.Standardize.StrikeIron",
//                Order = 1,
//                Description = @"The Strike Iron User ID",
//                Guid = new Guid( "01BB9E72-AC57-4D95-922D-2DBC1DEE311E" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_UserID_120 );

//            var attribute_Password_121 = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"Password",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Password",
//                System = false,
//                Entity = @"Rock.Component.Standardize.StrikeIron",
//                Order = 2,
//                Description = @"The Strike Iron Password",
//                Guid = new Guid( "DF56D543-9465-4F27-9A42-0F094DB92299" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Password_121 );

//            var attribute_Order_122 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Order",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Order",
//                System = false,
//                Entity = @"Rock.Component.Standardize.StrikeIron",
//                Order = 0,
//                Description = @"The order that this service should be used (priority)",
//                Guid = new Guid( "E93F399F-EE85-4CCC-85A7-4938E92234EA" ),
//                DefaultValue = @"0",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Order_122 );

//            var attribute_Active_123 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Active",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Active",
//                System = false,
//                Entity = @"Rock.Component.Standardize.StrikeIron",
//                Order = 0,
//                Description = @"Should Service be used?",
//                Guid = new Guid( "C0168A17-FA44-45BB-B12A-F6A24230A15E" ),
//                DefaultValue = @"False",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Boolean.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Active_123 );

//            var attribute_CustomerId_124 = new Rock.Core.Attribute
//            {
//                Category = @"Security",
//                EntityQualifierColumn = @"",
//                Required = true,
//                Name = @"Customer Id",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"CustomerId",
//                System = false,
//                Entity = @"Rock.Component.Standardize.MelissaData",
//                Order = 1,
//                Description = @"The Melissa Data Customer ID",
//                Guid = new Guid( "8774A579-894C-46FF-83B5-BCC21CB6FF30" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_CustomerId_124 );

//            var attribute_Order_125 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Order",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Order",
//                System = false,
//                Entity = @"Rock.Component.Standardize.MelissaData",
//                Order = 0,
//                Description = @"The order that this service should be used (priority)",
//                Guid = new Guid( "3DC7BD74-F7E3-4735-A6D2-5921ADAF28E6" ),
//                DefaultValue = @"0",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Order_125 );

//            var attribute_Active_126 = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Active",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Active",
//                System = false,
//                Entity = @"Rock.Component.Standardize.MelissaData",
//                Order = 0,
//                Description = @"Should Service be used?",
//                Guid = new Guid( "7137CE8A-85E0-4811-A6DF-699BDA2301CC" ),
//                DefaultValue = @"False",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Boolean.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Active_126 );

//            var attribute_ComponentContainer = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"BlockId",
//                Required = true,
//                Name = @"Component Container",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"ComponentContainer",
//                System = false,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 0,
//                Description = @"The Rock Extension Component Container to manage",
//                Guid = new Guid( "259AF14D-0214-4BE4-A7BF-40423EA07C99" ),
//                DefaultValue = @"",
//                EntityQualifierValue = block_Components.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_ComponentContainer );

//            var attribute_EmailHeaderLogo = new Rock.Core.Attribute
//            {
//                Category = @"EmailFormat",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Email Header Logo",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"EmailHeaderLogo",
//                System = true,
//                Entity = @"",
//                Order = 1,
//                Description = @"Logo image to be used at the top off all emails.",
//                Guid = new Guid( "B95C446D-6A3C-4672-8718-CF988C447D0D" ),
//                DefaultValue = @"assets/images/email-header.jpg",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Image.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_EmailHeaderLogo );

//            var attribute_EmailBackgroundColor = new Rock.Core.Attribute
//            {
//                Category = @"EmailFormat",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Email Background Color",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"EmailBackgroundColor",
//                System = true,
//                Entity = @"",
//                Order = 2,
//                Description = @"Background color (format #ffffff) that will be used for default emails.",
//                Guid = new Guid( "56C8EC3F-1F7B-410A-A742-42EA217E3302" ),
//                DefaultValue = @"#cccccc",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_EmailBackgroundColor );

//            var attribute_OrganizationEmail = new Rock.Core.Attribute
//            {
//                Category = @"Organization",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Organization Email",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"OrganizationEmail",
//                System = false,
//                Entity = @"",
//                Order = 0,
//                Description = @"The primary email address for the organization.",
//                Guid = new Guid( "6837554F-93B3-4D46-BA48-A4059FA1766F" ),
//                DefaultValue = @"info@organizationname.com",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_OrganizationEmail );

//            var attribute_OrganizationWebsite = new Rock.Core.Attribute
//            {
//                Category = @"Organization",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Organization Website",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"OrganizationWebsite",
//                System = false,
//                Entity = @"",
//                Order = 0,
//                Description = @"The primary website for the organization.",
//                Guid = new Guid( "118A083B-3F28-4D17-8B19-CC6859F89F33" ),
//                DefaultValue = @"www.organization.com",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_OrganizationWebsite );

//            var attribute_OrganizationPhone = new Rock.Core.Attribute
//            {
//                Category = @"Organization",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Organization Phone",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"OrganizationPhone",
//                System = false,
//                Entity = @"",
//                Order = 0,
//                Description = @"The primary phone number of the organization.",
//                Guid = new Guid( "85716596-6AEA-4887-830F-744D22E28A0D" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_OrganizationPhone );

//            var attribute_EmailBodyTextColor = new Rock.Core.Attribute
//            {
//                Category = @"EmailFormat",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Email Body Text Color",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"EmailBodyTextColor",
//                System = false,
//                Entity = @"",
//                Order = 0,
//                Description = @"The text color (format #000000) to use for the body font of the email.",
//                Guid = new Guid( "724FA692-8E3E-4F43-B3CA-0D6767AAD53A" ),
//                DefaultValue = @"#000000",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_EmailBodyTextColor );

//            var attribute_EmailBodyTextLinkColor = new Rock.Core.Attribute
//            {
//                Category = @"EmailFormat",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Email Body Text Link Color",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"EmailBodyTextLinkColor",
//                System = false,
//                Entity = @"",
//                Order = 0,
//                Description = @"The text link color (format #000000) to use for the HTML anchors in the body of the email.",
//                Guid = new Guid( "A910C483-0BE0-400F-B25D-131D78F124A0" ),
//                DefaultValue = @"#006699",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_EmailBodyTextLinkColor );

//            var attribute_EmailFooterTextColor = new Rock.Core.Attribute
//            {
//                Category = @"EmailFormat",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Email Footer Text Color",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"EmailFooterTextColor",
//                System = false,
//                Entity = @"",
//                Order = 0,
//                Description = @"The text color (format #000000) to use for the footer font of the email.",
//                Guid = new Guid( "61E60E2A-EA86-4769-9029-F803B06849DE" ),
//                DefaultValue = @"#4f4e4e",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_EmailFooterTextColor );

//            var attribute_EmailFooterTextLinkColor = new Rock.Core.Attribute
//            {
//                Category = @"EmailFormat",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Email Footer Text Link Color",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"EmailFooterTextLinkColor",
//                System = false,
//                Entity = @"",
//                Order = 0,
//                Description = @"The text link color (format #000000) to use for the HTML anchors in the footer of the email.",
//                Guid = new Guid( "D64C84BF-972D-4458-A2C5-B8E05002D833" ),
//                DefaultValue = @"#212937",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_EmailFooterTextLinkColor );

//            var attribute_HourstoKeepUnconfirmedAccounts = new Rock.Core.Attribute
//            {
//                Category = @"General",
//                EntityQualifierColumn = @"Class",
//                Required = false,
//                Name = @"Hours to Keep Unconfirmed Accounts",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"HoursKeepUnconfirmedAccounts",
//                System = false,
//                Entity = @"Rock.Util.Job",
//                Order = 0,
//                Description = @"The number of hours to keep user accounts that have not been confirmed (default is 48 hours.)",
//                Guid = new Guid( "6C0E84A3-E2F8-480A-AB96-34D24C2ACF40" ),
//                DefaultValue = @"48",
//                EntityQualifierValue = @"Rock.Jobs.RockCleanup",
//                FieldTypeId = fieldtype_Interger.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_HourstoKeepUnconfirmedAccounts );

//            var attribute_EmailBodyBackgroundColor = new Rock.Core.Attribute
//            {
//                Category = @"EmailFormat",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Email Body Background Color",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"EmailBodyBackgroundColor",
//                System = false,
//                Entity = @"",
//                Order = 0,
//                Description = @"The background color of the body of the email.",
//                Guid = new Guid( "DC2A8545-D61C-4D45-B593-A7326E3BE3A4" ),
//                DefaultValue = @"#fff",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_EmailBodyBackgroundColor );

//            var attribute_EmailHeader = new Rock.Core.Attribute
//            {
//                Category = @"EmailFormat",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Email Header",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"EmailHeader",
//                System = false,
//                Entity = @"",
//                Order = 0,
//                Description = @"The standard email header that wraps the email text and shows a header image.",
//                Guid = new Guid( "EBC67F76-7305-4108-AD32-E2531EAB1637" ),
//                DefaultValue = @"<style>  	body { 		background-color: {EmailBodyBackgroundColor};           		font-family: Verdana, Arial, Helvetica, sans-serif;           		font-size: 12px;           		line-height: 1.3em;           		margin: 0; 		padding: 0; 	} 	 	a { 		color: {EmailBodyTextLinkColor}; 	}   </style>    <table style=""text-align: center; background-color: {EmailBackgroundColor}; margin: 0pt"" border=0 cellSpacing=0 cellPadding=0 width=""100%"" align=center>           <tbody>               	<tr>                   		<td style=""background-color: {EmailBackgroundColor}; margin: 0pt auto"" valign=top align=middle> 			<!-- Begin Layout -->                    			<table style=""text-align: left; background-color: {EmailBodyBackgroundColor}; margin: 0px auto; width: 550px"" border=0 cellspacing=0 cellpadding=0 width=550> 			<tbody>                           				<tr>                               					<td valign=top align=left> 						<!-- Header Start -->                               						<table style=""width: 100%"" border=0 cellSpacing=0 cellPadding=0 width=""100%"">                                   						<tbody>                                       							<tr>                                           								<td style=""height: 51px""> 									<img style=""border-bottom: medium none; border-left: medium none; padding-bottom: 0pt; margin: 0px; padding-left: 0pt; padding-right: 0pt; border-top: medium none; border-right: medium none; padding-top: 0pt"" src=""{Config:BaseUrl}{EmailHeaderLogo}"">  								</td>                                       							</tr>                                   						</tbody>                               						</table>                               						<!-- Header End -->  						  						                               						<table style=""padding-bottom: 18px; width: 100%; background-color: {EmailBodyBackgroundColor}; "" cellspacing=0 cellpadding=20 >                                   						<tbody>                                       							<tr>                                           								<td>                                          								<!-- Main Text Start -->",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_EmailHeader );

//            var attribute_EmailFooter = new Rock.Core.Attribute
//            {
//                Category = @"EmailFormat",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Email Footer",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"EmailFooter",
//                System = false,
//                Entity = @"",
//                Order = 0,
//                Description = @"Standard footer text that contains the name of the organization, etc.",
//                Guid = new Guid( "ED326066-4A91-412A-805C-40DEDAE8F61A" ),
//                DefaultValue = @"<!-- Main Text End --> 								</td>                           							</tr>                           							                  						</tbody>                   						</table> 						 						<!-- Footer Start -->                               						<table cellpadding=20 align=center style=""background-color: {EmailBackgroundColor}; width: 100%;"">                                   						<tbody>                                       							<tr>                                           								<td>                                           									<p style=""text-align: center; color: {EmailFooterTextColor}""><span style=""font-size: 16px"">{OrganizationName} | {OrganizationPhone} <br>   									<a href=""mailto:{OrganizationEmail}"">{OrganizationEmail}</A> | <span style=""color: {EmailFooterTextLink Color};""><a style=""color: {EmailFooterTextLinkColor}"" href=""{OrganizationWebsite}"">{OrganizationWebsite}</A></span></span></p>                                           								</td>                                       							</tr>                                   						</tbody>                               						</table>                               									<!-- Footer End --> 				<!-- End Layout --> 				</td>               				</tr>           			</tbody>       			</table>",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_EmailFooter );

//            var attribute_DaystoKeepExceptionsinLog = new Rock.Core.Attribute
//            {
//                Category = @"General",
//                EntityQualifierColumn = @"Class",
//                Required = false,
//                Name = @"Days to Keep Exceptions in Log",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"DaysKeepExceptions",
//                System = false,
//                Entity = @"Rock.Util.Job",
//                Order = 0,
//                Description = @"The number of days to keep exceptions in the exception log (default is 14 days.)",
//                Guid = new Guid( "643902EA-2534-4D5D-AC5B-CEFF53D98EA8" ),
//                DefaultValue = @"14",
//                EntityQualifierValue = @"Rock.Jobs.RockCleanup",
//                FieldTypeId = fieldtype_Interger.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_DaystoKeepExceptionsinLog );

//            var attribute_Log404sAsExceptions = new Rock.Core.Attribute
//            {
//                Category = @"Config",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Log 404s As Exceptions",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"Log404AsException",
//                System = false,
//                Entity = @"",
//                Order = 0,
//                Description = @"Add 404 (File Not Found) errors as exceptions in the exception log. Warning this will impact performance.",
//                Guid = new Guid( "B4947CE4-3E1B-4679-8B7D-B44D0D4A7D97" ),
//                DefaultValue = @"False",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Boolean.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_Log404sAsExceptions );

//            var attribute_EmailExceptionsList = new Rock.Core.Attribute
//            {
//                Category = @"Config",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Email Exceptions List",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"EmailExceptionsList",
//                System = false,
//                Entity = @"",
//                Order = 0,
//                Description = @"Comma separated list of email addresses to send exception notifications to.",
//                Guid = new Guid( "F7D2FE87-537D-4452-B503-3991D15BD242" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_EmailExceptionsList );

//            var attribute_SendGridUsername = new Rock.Core.Attribute
//            {
//                Category = @"EmailConfig",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"SendGrid Username",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"SendGridUsername",
//                System = false,
//                Entity = @"",
//                Order = 0,
//                Description = @"Username of the SendGrid account.",
//                Guid = new Guid( "E49FE2ED-F67A-4E60-B297-A7C5220C056C" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_SendGridUsername );

//            var attribute_SendGridPassword = new Rock.Core.Attribute
//            {
//                Category = @"EmailConfig",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"SendGrid Password",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"SendGridPassword",
//                System = false,
//                Entity = @"",
//                Order = 0,
//                Description = @"SendGrid account password.",
//                Guid = new Guid( "A96616C8-EFC1-4E7D-A6E1-76FCA2E5AB52" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_SendGridPassword );

//            var attribute_TestAttribute = new Rock.Core.Attribute
//            {
//                Category = @"[All]",
//                EntityQualifierColumn = @"",
//                Required = false,
//                Name = @"Test Attribute",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"testattr",
//                System = false,
//                Entity = @"",
//                Order = 0,
//                Description = @"this is a test",
//                Guid = new Guid( "EE186365-E21C-447C-B4F9-C0733282780C" ),
//                DefaultValue = @"",
//                EntityQualifierValue = @"",
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_TestAttribute );

//            var attribute_CacheDuration = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Cache Duration",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"CacheDuration",
//                System = true,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 2,
//                Description = @"Number of seconds to cache the content.",
//                Guid = new Guid( "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4" ),
//                DefaultValue = @"0",
//                EntityQualifierValue = block_HTMLContent.Id.ToString(),
//                FieldTypeId = fieldtype_Interger.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_CacheDuration );

//            context.SaveChanges();

//            var attributevalue_0 = new AttributeValue
//            {
//                Order = 0,
//                AttributeId = attribute_CacheDuration.Id,
//                System = false,
//                Guid = new Guid( "D26D86B9-C8E0-4104-B472-B22B6EB85692" ),
//                Value = @"0",
//                EntityId = blockinstance_Welcome.Id
//            };
//            context.AttributeValues.AddOrUpdate( p => p.Guid, attributevalue_0 );


//            var attribute_RequireApproval = new Rock.Core.Attribute
//            {
//                Category = @"Advanced",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Require Approval",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"RequireApproval",
//                System = true,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 6,
//                Description = @"Require that content be approved?",
//                Guid = new Guid( "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A" ),
//                DefaultValue = @"False",
//                EntityQualifierValue = block_HTMLContent.Id.ToString(),
//                FieldTypeId = fieldtype_Boolean.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_RequireApproval );

//            context.SaveChanges();

//            var attributevalue_False = new AttributeValue
//            {
//                Order = 0,
//                AttributeId = attribute_RequireApproval.Id,
//                System = false,
//                Guid = new Guid( "C024B060-C801-49EB-8FDA-DCADDDC42752" ),
//                Value = @"False",
//                EntityId = blockinstance_Welcome.Id
//            };
//            context.AttributeValues.AddOrUpdate( p => p.Guid, attributevalue_False );


//            var attribute_SupportVersions = new Rock.Core.Attribute
//            {
//                Category = @"Advanced",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Support Versions",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"SupportVersions",
//                System = true,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 5,
//                Description = @"Support content versioning?",
//                Guid = new Guid( "7C1CE199-86CF-4EAE-8AB3-848416A72C58" ),
//                DefaultValue = @"False",
//                EntityQualifierValue = block_HTMLContent.Id.ToString(),
//                FieldTypeId = fieldtype_Boolean.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_SupportVersions );

//            context.SaveChanges();

//            var attributevalue_True_113 = new AttributeValue
//            {
//                Order = 0,
//                AttributeId = attribute_SupportVersions.Id,
//                System = false,
//                Guid = new Guid( "2E894676-CE3B-45A4-B91A-0F2A5595F135" ),
//                Value = @"True",
//                EntityId = blockinstance_Welcome.Id
//            };
//            context.AttributeValues.AddOrUpdate( p => p.Guid, attributevalue_True_113 );


//            var attribute_ContextParameter = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Context Parameter",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"ContextParameter",
//                System = true,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 3,
//                Description = @"Query string parameter to use for 'personalizing' content based on unique values.",
//                Guid = new Guid( "3FFC512D-A576-4289-B648-905FD7A64ABB" ),
//                DefaultValue = @"",
//                EntityQualifierValue = block_HTMLContent.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_ContextParameter );

//            context.SaveChanges();

//            var attributevalue_c4529048xx488cxx4480xx8b8exx88a823b500db = new AttributeValue
//            {
//                Order = 0,
//                AttributeId = attribute_ContextParameter.Id,
//                System = false,
//                Guid = new Guid( "C4529048-488C-4480-8B8E-88A823B500DB" ),
//                EntityId = blockinstance_Welcome.Id
//            };
//            context.AttributeValues.AddOrUpdate( p => p.Guid, attributevalue_c4529048xx488cxx4480xx8b8exx88a823b500db );


//            var attribute_ContextName = new Rock.Core.Attribute
//            {
//                Category = @"",
//                EntityQualifierColumn = @"BlockId",
//                Required = false,
//                Name = @"Context Name",
//                GridColumn = false,
//                MultiValue = false,
//                Key = @"ContextName",
//                System = true,
//                Entity = @"Rock.CMS.BlockInstance",
//                Order = 4,
//                Description = @"Name to use to further 'personalize' content.  Blocks with the same name, and referenced with the same context parameter will share html values.",
//                Guid = new Guid( "466993F7-D838-447A-97E7-8BBDA6A57289" ),
//                DefaultValue = @"",
//                EntityQualifierValue = block_HTMLContent.Id.ToString(),
//                FieldTypeId = fieldtype_Text.Id
//            };
//            context.Attributes.AddOrUpdate( p => p.Guid, attribute_ContextName );

//            context.SaveChanges();

//            var attributevalue_f01dccd9xxbebbxx447fxx956dxxe75840af18d2 = new AttributeValue
//            {
//                Order = 0,
//                AttributeId = attribute_ContextName.Id,
//                System = false,
//                Guid = new Guid( "F01DCCD9-BEBB-447F-956D-E75840AF18D2" ),
//                EntityId = blockinstance_Welcome.Id
//            };
//            context.AttributeValues.AddOrUpdate( p => p.Guid, attributevalue_f01dccd9xxbebbxx447fxx956dxxe75840af18d2 );


//            context.SaveChanges();


//            //**************************************************************************
//            // CoreAttribute Qualifiers		(AUTO GENERATED BY TOOL)
//            //**************************************************************************


//            //**************************************************************************
//            // Email Templates		(AUTO GENERATED BY TOOL)
//            //**************************************************************************

//            var emailtemplate_ForgotUsernames = new Rock.CRM.EmailTemplate
//            {
//                Category = @"Security",
//                Subject = @"Login Request",
//                Title = @"Forgot Usernames",
//                Guid = new Guid( "113593FF-620E-4870-86B1-7A0EC0409208" ),
//                From = @"rock@sparkdevnetwork.com",
//                System = true,
//                Body = @"{EmailHeader}
//
//Below are your current usernames at {OrganizationName} {Person:RepeatBegin}<br/><br/>For {Person:FirstName} {Person:LastName},<br/><br/>{User:RepeatBegin}{User:Username} <a href='{ConfirmAccountUrl}?cc={User:ConfirmationCodeEncoded}&action=reset'>Reset Password</a><br/>{User:RepeatEnd}<br/>{Person:RepeatEnd}
//
//{EmailFooter}"
//            };
//            context.EmailTemplates.AddOrUpdate( p => p.Guid, emailtemplate_ForgotUsernames );

//            var emailtemplate_ConfirmAccount = new Rock.CRM.EmailTemplate
//            {
//                Category = @"Security",
//                Subject = @"Account Confirmation",
//                Title = @"Confirm Account",
//                Guid = new Guid( "17AACEEF-15CA-4C30-9A3A-11E6CF7E6411" ),
//                From = @"rock@sparkdevnetwork.com",
//                System = true,
//                Body = @"{EmailHeader}
//
//{Person:FirstName},<br/><br/>Thank-you for creating an account at {OrganizationName}. Please <a href='{ConfirmAccountUrl}?cc={User:ConfirmationCodeEncoded}&action=confirm'>confirm</a> that you are the owner of this email address.<br/><br/>If you did not create this account, you can <a href='{ConfirmAccountUrl}?cc={User:ConfirmationCodeEncoded}&action=delete'>Delete It</a>.<br/><br/>If the above links do not work, you can also go to {ConfirmAccountUrl} and enter the following confirmation code:<br/>{User:ConfirmationCode}<br/><br/>Thank-you,<br/>{OrganizationName}
//
//{EmailFooter}
//
//
//                         			
//	<!-- Main Text End -->
//								</td>                          
//							</tr>                          
//							                 
//						</tbody>                  
//						</table>
//						
//						<!-- Footer Start -->                              
//						<table cellpadding=20 align=center style=""background-color: {EmailBackgroundColor}; width: 100%;"">                                  
//						<tbody>                                      
//							<tr>                                          
//								<td>                                          
//									<p style=""text-align: center; color: {EmailFooterTextColor}""><span style=""font-size: 16px"">{OrganizationName} | {OrganizationPhone} <br>  
//									<a href=""mailto:{OrganizationEmail}"">{OrganizationEmail}</A> | <span style=""color: {EmailFooterTextLink Color};""><a style=""color: {EmailFooterTextLinkColor}"" href=""{OrganizationWebsite}"">{OrganizationWebsite}</A></span></span></p>                                          
//								</td>                                      
//							</tr>                                  
//						</tbody>                              
//						</table>                              
//									<!-- Footer End -->
//				<!-- End Layout -->
//				</td>              
//				</tr>          
//			</tbody>      
//			</table>							"
//            };
//            context.EmailTemplates.AddOrUpdate( p => p.Guid, emailtemplate_ConfirmAccount );

//            var emailtemplate_AccountCreated = new Rock.CRM.EmailTemplate
//            {
//                Category = @"Security",
//                Subject = @"Account Created",
//                Title = @"Account Created",
//                Guid = new Guid( "84E373E9-3AAF-4A31-B3FB-A8E3F0666710" ),
//                From = @"Rock@SparkDevNetwork",
//                System = true,
//                Body = @"{EmailHeader}
//
//{Person:FirstName},<br/><br/>
//
//Thank-you for creating a new account at {OrganizationName}.  Your '{User:UserName}' username is now active and can be used to login to our site and access your information.<br/><br/>
//
//If you did not create this account you can <a href='{ConfirmAccountUrl}?cc={User:ConfirmationCodeEncoded}&action=delete'>Delete it here</a><br/><br/>
//
//Thanks.
//
//{EmailFooter}"
//            };
//            context.EmailTemplates.AddOrUpdate( p => p.Guid, emailtemplate_AccountCreated );

//            context.SaveChanges();

//            //**************************************************************************
//            // HTML Content		(AUTO GENERATED BY TOOL)
//            //**************************************************************************

//            var htmlcontent_31 = new HtmlContent
//            {
//                Guid = new Guid( "553BED67-9E53-4D2B-A1C9-1499FF76356D" ),
//                Approved = true,
//                BlockId = blockinstance_Html.Id,
//                Version = 0,
//                Content = @"<p>
//	The Right Side - yoyo</p>
//"
//            };
//            context.HtmlContents.AddOrUpdate( p => p.Guid, htmlcontent_31 );

//            var htmlcontent_32 = new HtmlContent
//            {
//                Guid = new Guid( "9CD7DC41-4D32-4AD2-8EB4-D57EE96E3A30" ),
//                Approved = true,
//                BlockId = blockinstance_Html.Id,
//                Version = 1,
//                Content = @"."
//            };
//            context.HtmlContents.AddOrUpdate( p => p.Guid, htmlcontent_32 );

//            var htmlcontent_33 = new HtmlContent
//            {
//                Guid = new Guid( "BA6B5D30-BDBA-41BB-AB22-540D998E8C84" ),
//                Approved = true,
//                BlockId = blockinstance_Welcome.Id,
//                Version = 0,
//                Content = @"<h2>
//	&nbsp;</h2>
//<h1>
//	Welcome to the default page of Rock ChMS</h1>
//<p>
//	This is the default page.&nbsp; The navigation menu is also now active.&nbsp; If you don&#39;t see an Administration option above (and you&#39;re an administrator), make sure to login.</p>
//<p>
//	&nbsp;</p>
//<p>
//	&nbsp;</p>
//<p>
//	&nbsp;</p>
//"
//            };
//            context.HtmlContents.AddOrUpdate( p => p.Guid, htmlcontent_33 );

//            var htmlcontent_34 = new HtmlContent
//            {
//                Guid = new Guid( "CBE60B6E-8667-46FD-A37E-4185C7002241" ),
//                Approved = true,
//                BlockId = blockinstance_FooterContent.Id,
//                Version = 0,
//                Content = @"<p>
//	Copyright&nbsp; 2012 Spark Development Network</p>
//"
//            };
//            context.HtmlContents.AddOrUpdate( p => p.Guid, htmlcontent_34 );

//            var htmlcontent_40 = new HtmlContent
//            {
//                Guid = new Guid( "1ECA64A8-4D46-41C8-960F-F232805AD76E" ),
//                Approved = true,
//                BlockId = blockinstance_Welcome.Id,
//                Version = 1,
//                Content = @"
//<h2>Welcome to Rock ChMS</h2>
//
//<p>You must be logged in now because only authenticated users can see this page. The navigation menu is also now working but there is very little in there at the moment. If you don&#39;t see an Administration option above (and you&#39;re an administrator), there must be a problem with your authorization settings.</p>
//<p><small>If you're a developer you may want to <a href=""http://sparkdevnetwork.github.com/Rock-ChMS/"">start over on our developer pages</a>.</small></p>
//<p>
//	v1</p>
//<p>
//	&nbsp;</p>
//<p>
//	&nbsp;</p>
//"
//            };
//            context.HtmlContents.AddOrUpdate( p => p.Guid, htmlcontent_40 );

//            var htmlcontent_41 = new HtmlContent
//            {
//                Guid = new Guid( "D7EA2FD8-890C-49D1-83A5-7BF49993FFCC" ),
//                Approved = true,
//                BlockId = blockinstance_Welcome.Id,
//                Version = 2,
//                Content = @"<h2>
//	Welcome to the default page of Rock ChMS</h2>
//<p>
//	This is the default page.&nbsp; The navigation menu is also now active.&nbsp; If you don&#39;t see an Administration option above (and you&#39;re an administrator), make sure to login.</p>
//<p>
//	v2</p>
//<p>
//	&nbsp;</p>
//<p>
//	&nbsp;</p>
//<p>
//	&nbsp;</p>
//"
//            };
//            context.HtmlContents.AddOrUpdate( p => p.Guid, htmlcontent_41 );

//            context.SaveChanges();


//            //**************************************************************************
//            // CMS Auth		(AUTO GENERATED BY TOOL)
//            //**************************************************************************

//            var auth_281 = new Auth
//            {
//                Guid = new Guid( "372E79DE-4190-41DC-9AC4-7E0758A35338" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = 0,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"Global"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_281 );

//            var auth_282 = new Auth
//            {
//                Guid = new Guid( "3A246749-7FDE-4B55-8499-CF0DE57D6729" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = 0,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"Global"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_282 );

//            var auth_421 = new Auth
//            {
//                Guid = new Guid( "85E7DBF4-EBE9-4D16-9D8E-7F9B56947297" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page1_DefaultPage.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_421 );

//            var auth_423 = new Auth
//            {
//                Guid = new Guid( "DEE02C2A-F0F0-4ACB-8541-D0CF1960D1B4" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page12_MainPageTest.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_423 );

//            // Allow Autheticated users to see the main page
//            var auth_Allow_Authenticated_Users = new Auth
//            {
//                Guid = new Guid( "33fc0743-2d6a-477c-a453-878bb7582504" ),
//                Action = @"View",
//                SpecialRole = Rock.CMS.SpecialRole.AllAuthenticatedUsers,
//                AllowOrDeny = @"A",
//                EntityId = page12_MainPageTest.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_Allow_Authenticated_Users );

//            // Deny UnAutheticated users from seeing the main page
//            var auth_Deny_UnAuthenticated_Users = new Auth
//            {
//                Guid = new Guid( "6b1ee024-29f4-4c77-bf95-c91411a40f75" ),
//                Action = @"View",
//                SpecialRole = Rock.CMS.SpecialRole.AllUnAuthenticatedUsers,
//                AllowOrDeny = @"D",
//                EntityId = page12_MainPageTest.Id,
//                Order = 1,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_Allow_Authenticated_Users );

//            var auth_426 = new Auth
//            {
//                Guid = new Guid( "7485CCAA-C26C-4BB2-8AC0-91F503388C98" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page41_Communications.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_426 );

//            var auth_429 = new Auth
//            {
//                Guid = new Guid( "C837CBCE-BF2D-4B4F-8EA0-6160E747745A" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page44_Administration.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_429 );

//            var auth_438 = new Auth
//            {
//                Guid = new Guid( "35C8B275-4FEB-443D-84DE-D81B0C908E30" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page48_Security.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_438 );

//            var auth_439 = new Auth
//            {
//                Guid = new Guid( "63918643-7344-4BB2-8DFA-8D681FE5216A" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page49_CMSAdministration.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_439 );

//            var auth_446 = new Auth
//            {
//                Guid = new Guid( "75F4DB76-3A18-4F2D-992A-8471A950B260" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page3_Login.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_446 );

//            var auth_447 = new Auth
//            {
//                Guid = new Guid( "8C6F5555-4401-45DC-8BB6-19ABEA83E33E" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page4_NewAccount.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_447 );

//            var auth_448 = new Auth
//            {
//                Guid = new Guid( "8C0A7255-F6F7-4C6C-99D7-6E0F0CDA3605" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page28_Security.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_448 );

//            var auth_449 = new Auth
//            {
//                Guid = new Guid( "56334C96-E4C5-4B5F-80FA-DE771EAD9DAD" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page2_Sites.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_449 );

//            var auth_450 = new Auth
//            {
//                Guid = new Guid( "12EDC29A-A00C-4CB5-A582-50EC0334E353" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page7_Blocks.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_450 );

//            var auth_451 = new Auth
//            {
//                Guid = new Guid( "94532445-8D87-4109-A375-B9D8D8158D2A" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page16_ZoneBlocks.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_451 );

//            var auth_452 = new Auth
//            {
//                Guid = new Guid( "1F6E6E6A-4D52-49C3-8B50-9065A1249406" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page23_BlockProperties.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_452 );

//            var auth_453 = new Auth
//            {
//                Guid = new Guid( "5BB3213F-C661-42F8-9311-97C97FABF676" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page29_ChildPages.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_453 );

//            var auth_454 = new Auth
//            {
//                Guid = new Guid( "69914B52-318F-42F3-A201-B5AAF140385F" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page37_PageProperties.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_454 );

//            var auth_455 = new Auth
//            {
//                Guid = new Guid( "847021EB-54C3-437F-8C00-7C464A537E7A" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page51_GlobalAttributes.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_455 );

//            var auth_456 = new Auth
//            {
//                Guid = new Guid( "228DB32F-DF56-403E-B31F-4163C6829119" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page52_GlobalValues.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_456 );

//            var auth_457 = new Auth
//            {
//                Guid = new Guid( "2935C7A1-55C6-404A-B87A-159DEA65CDA6" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page1_DefaultPage.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_457 );

//            var auth_459 = new Auth
//            {
//                Guid = new Guid( "D82132FD-73F6-4ACC-BEC5-66541A5BCC5C" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page12_MainPageTest.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_459 );

//            var auth_462 = new Auth
//            {
//                Guid = new Guid( "A776F466-0619-40DD-9323-CE0484368BCE" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page41_Communications.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_462 );

//            var auth_465 = new Auth
//            {
//                Guid = new Guid( "BF72E3B4-6AFE-4F54-840B-C03E57433D15" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page44_Administration.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_465 );

//            var auth_474 = new Auth
//            {
//                Guid = new Guid( "709ACEC4-CAF6-4DC2-A70A-B31384A2E32C" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page48_Security.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_474 );

//            var auth_475 = new Auth
//            {
//                Guid = new Guid( "0341282F-1C68-471D-A839-DEDAB85F7246" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page49_CMSAdministration.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_475 );

//            var auth_482 = new Auth
//            {
//                Guid = new Guid( "B0BF8636-69FA-4EDC-A13C-198049A0F1E7" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page3_Login.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_482 );

//            var auth_483 = new Auth
//            {
//                Guid = new Guid( "2339E2F2-DB76-46F8-9422-D23DCAC55271" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page4_NewAccount.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_483 );

//            var auth_484 = new Auth
//            {
//                Guid = new Guid( "373BAE73-FBA6-4F26-A73A-422B0D1D0BA3" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page28_Security.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_484 );

//            var auth_485 = new Auth
//            {
//                Guid = new Guid( "DA1DE249-3C2E-4C2E-9BFB-E786B0BE5D06" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page2_Sites.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_485 );

//            var auth_486 = new Auth
//            {
//                Guid = new Guid( "97D3000B-8466-4982-9E66-C564A74807F6" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page7_Blocks.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_486 );

//            var auth_487 = new Auth
//            {
//                Guid = new Guid( "F04CCB12-5DCC-42F2-8EC3-8F292B33774D" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page16_ZoneBlocks.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_487 );

//            var auth_488 = new Auth
//            {
//                Guid = new Guid( "9F37B54F-803D-4BAB-AE67-4A2A492B7071" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page23_BlockProperties.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_488 );

//            var auth_489 = new Auth
//            {
//                Guid = new Guid( "C856676E-E807-4F9D-87D4-E2F6B012BD30" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page29_ChildPages.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_489 );

//            var auth_490 = new Auth
//            {
//                Guid = new Guid( "94854054-373C-4FC1-A743-878715E8EAC2" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page37_PageProperties.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_490 );

//            var auth_491 = new Auth
//            {
//                Guid = new Guid( "90CF80A3-4BAB-4875-92B2-A395D641F645" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page51_GlobalAttributes.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_491 );

//            var auth_492 = new Auth
//            {
//                Guid = new Guid( "1CEACC65-4140-4682-9D48-77F3D7FE1EDB" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page52_GlobalValues.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_492 );

//            var auth_495 = new Auth
//            {
//                Guid = new Guid( "8FDF6739-8E39-4827-8A85-9CCA5C458F20" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_Html.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_495 );

//            var auth_496 = new Auth
//            {
//                Guid = new Guid( "96BF7A74-31BE-4784-A1C3-4CDD9A1B86B8" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_a.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_496 );

//            var auth_497 = new Auth
//            {
//                Guid = new Guid( "141FC2F9-5643-4901-BBA3-D8D185075890" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_b.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_497 );

//            var auth_498 = new Auth
//            {
//                Guid = new Guid( "BBEB07FB-0B84-444C-BFC4-71EF69FCDEF9" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_NewAccount.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_498 );

//            var auth_501 = new Auth
//            {
//                Guid = new Guid( "66CB6BDC-D0BA-4E3D-A545-4DDE8ABB2EB8" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_d.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_501 );

//            var auth_507 = new Auth
//            {
//                Guid = new Guid( "5A8CAF7F-CD32-4008-A64A-BC306DCA743C" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_Welcome.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_507 );

//            var auth_509 = new Auth
//            {
//                Guid = new Guid( "98236B45-7EE9-4F90-B116-9F90D482C031" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_l.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_509 );

//            var auth_511 = new Auth
//            {
//                Guid = new Guid( "FE461AAB-EBAE-495A-8037-402AE2582DBB" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_BlockProperties.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_511 );

//            var auth_512 = new Auth
//            {
//                Guid = new Guid( "F9488ED6-E0D6-43D4-AFDB-92FB1201D465" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_Security.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_512 );

//            var auth_513 = new Auth
//            {
//                Guid = new Guid( "B509716C-1763-4C16-B2F3-B4C8DB84F6D8" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_ChildPages.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_513 );

//            var auth_514 = new Auth
//            {
//                Guid = new Guid( "B32E86FE-625B-4198-9ACD-30FFF5DF11C7" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_PageProperties.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_514 );

//            var auth_517 = new Auth
//            {
//                Guid = new Guid( "713DEBFC-96EF-4BBC-BAE0-8F810F5A49A8" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_OrgSettings.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_517 );

//            var auth_518 = new Auth
//            {
//                Guid = new Guid( "6D5D85F9-003D-4CC0-B6DB-08A9B487629E" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_OrgValues.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_518 );

//            var auth_519 = new Auth
//            {
//                Guid = new Guid( "E0F2F1DD-7D6D-4EEE-8E0F-062951BE9C54" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_FooterContent.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_519 );

//            var auth_520 = new Auth
//            {
//                Guid = new Guid( "AE6348E6-A0D5-46D7-9B60-6DFA05E3F0C0" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_LoginStatus.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_520 );

//            var auth_521 = new Auth
//            {
//                Guid = new Guid( "3BF5CFEC-078F-4B7A-95FE-330EB834425A" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_Menu.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_521 );

//            var auth_524 = new Auth
//            {
//                Guid = new Guid( "1010E2C1-98EF-41A3-B608-C9D680FCC496" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_Html.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_524 );

//            var auth_525 = new Auth
//            {
//                Guid = new Guid( "E2C562A7-7695-4574-826D-7A56DB28BE26" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_a.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_525 );

//            var auth_526 = new Auth
//            {
//                Guid = new Guid( "765FEDE1-6280-4487-8F16-9B0FF29662E8" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_b.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_526 );

//            var auth_527 = new Auth
//            {
//                Guid = new Guid( "BAD13F5D-E7B6-4522-BF4C-72F08589C3E4" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_NewAccount.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_527 );

//            var auth_530 = new Auth
//            {
//                Guid = new Guid( "A33957A8-7D2A-43DA-9AC7-57CAFA430386" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_d.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_530 );

//            var auth_536 = new Auth
//            {
//                Guid = new Guid( "0F1C3B0E-72F8-4A87-A878-2365437A9D7A" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_Welcome.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_536 );

//            var auth_538 = new Auth
//            {
//                Guid = new Guid( "EA9417E8-616A-473B-ACAA-CB594EA821F7" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_l.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_538 );

//            var auth_540 = new Auth
//            {
//                Guid = new Guid( "87AE0C9D-6CCD-4477-B7CF-83154FED97A3" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_BlockProperties.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_540 );

//            var auth_541 = new Auth
//            {
//                Guid = new Guid( "1F8FC7B5-2313-43CB-A6BE-FE22DFE7D595" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_Security.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_541 );

//            var auth_542 = new Auth
//            {
//                Guid = new Guid( "D60A9200-A28B-4903-B28E-F0381223F9DB" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_ChildPages.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_542 );

//            var auth_543 = new Auth
//            {
//                Guid = new Guid( "BADF8248-47C4-4890-98BE-A5DAA78592A2" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_PageProperties.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_543 );

//            var auth_546 = new Auth
//            {
//                Guid = new Guid( "6915069B-E079-4D08-A2EB-523206DA2A4C" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_OrgSettings.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_546 );

//            var auth_547 = new Auth
//            {
//                Guid = new Guid( "8C067E40-EAB6-4EB1-8026-626339096019" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_OrgValues.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_547 );

//            var auth_548 = new Auth
//            {
//                Guid = new Guid( "4F6C75EF-9A0C-45F5-909E-28A973D2CAB2" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_FooterContent.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_548 );

//            var auth_549 = new Auth
//            {
//                Guid = new Guid( "907A5F4C-6C0C-45FA-8FEA-111066D73012" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_LoginStatus.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_549 );

//            var auth_550 = new Auth
//            {
//                Guid = new Guid( "20D0D89F-EB75-40B7-83AC-91DE33E95AE3" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_Menu.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_550 );

//            var auth_551 = new Auth
//            {
//                Guid = new Guid( "4D375C24-07F3-4FA3-90A6-F106AC67CB3F" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page54_Confirm.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_551 );

//            var auth_552 = new Auth
//            {
//                Guid = new Guid( "4C1D579C-BBDE-4698-9B75-6A1EC1F01E30" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page54_Confirm.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 1,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_552 );

//            var auth_553 = new Auth
//            {
//                Guid = new Guid( "B470E340-EF5E-425C-B928-37279EA07E57" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_Confirm.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_553 );

//            var auth_554 = new Auth
//            {
//                Guid = new Guid( "01ED396D-1D60-44F7-9B22-8A17325391B0" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_Confirm.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 1,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_554 );

//            var auth_555 = new Auth
//            {
//                Guid = new Guid( "C04C839E-D14B-40F5-A2AF-63A2F3EF3D36" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page55_ChangePassword.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_555 );

//            var auth_556 = new Auth
//            {
//                Guid = new Guid( "6E1056AA-EE85-4C85-9352-AB366DF68835" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page55_ChangePassword.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 1,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_556 );

//            var auth_557 = new Auth
//            {
//                Guid = new Guid( "8F2895CC-9905-4233-BBA2-EF5B62090E39" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_RestPassword.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_557 );

//            var auth_558 = new Auth
//            {
//                Guid = new Guid( "85BDEA04-3E16-497B-BDB3-6215DC972984" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_RestPassword.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 1,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_558 );

//            var auth_559 = new Auth
//            {
//                Guid = new Guid( "BFCEA7B5-3052-40C5-A0FE-F4CE17A1D44B" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page56_ForgotUserName.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_559 );

//            var auth_560 = new Auth
//            {
//                Guid = new Guid( "A6A370C3-2E8B-4B1F-95E8-3C493D0E6A99" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page56_ForgotUserName.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 1,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_560 );

//            var auth_561 = new Auth
//            {
//                Guid = new Guid( "8B0EB5A1-31A1-487C-BCFB-0E3766B4BA39" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_ForgotUserName.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_561 );

//            var auth_562 = new Auth
//            {
//                Guid = new Guid( "AAEFC62C-EB5B-4EEF-9E95-7AE6EF17796E" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_ForgotUserName.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 1,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_562 );

//            var auth_563 = new Auth
//            {
//                Guid = new Guid( "3952352B-0BC2-4621-B332-B644A8FFFA77" ),
//                Action = @"Configure",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page57_MyAccount.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_563 );

//            var auth_564 = new Auth
//            {
//                Guid = new Guid( "0934758F-1D24-45A5-8C1B-83ED3225FFC0" ),
//                Action = @"Edit",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page57_MyAccount.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 1,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_564 );

//            var auth_599 = new Auth
//            {
//                Guid = new Guid( "9B512011-3869-4EE5-94DC-F07A5534391C" ),
//                Action = @"View",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = page44_Administration.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 1,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_599 );

//            var auth_600 = new Auth
//            {
//                Guid = new Guid( "C7EE94F0-5587-4244-8FB4-EE9B69312E33" ),
//                Action = @"View",
//                SpecialRole = Rock.CMS.SpecialRole.AllUsers,
//                AllowOrDeny = @"D",
//                EntityId = page44_Administration.Id,
//                Order = 2,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_600 );

//            var auth_601 = new Auth
//            {
//                Guid = new Guid( "EA61416D-BBA7-459D-9B23-A87DF4082118" ),
//                Action = @"View",
//                SpecialRole = Rock.CMS.SpecialRole.AllUsers,
//                AllowOrDeny = @"A",
//                EntityId = page48_Security.Id,
//                Order = 0,
//                EntityType = @"CMS.Page"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_601 );

//            var auth_654 = new Auth
//            {
//                Guid = new Guid( "AB3FE61A-93D8-417C-8003-0BC9A21D2117" ),
//                Action = @"Approve",
//                SpecialRole = Rock.CMS.SpecialRole.None,
//                AllowOrDeny = @"A",
//                EntityId = blockinstance_Welcome.Id,
//                GroupId = adminRoleGroup.Id,
//                Order = 0,
//                EntityType = @"CMS.BlockInstance"
//            };
//            context.Auths.AddOrUpdate( p => p.Guid, auth_654 );

//            context.SaveChanges();


//            //**************************************************************************
//            // Utility: Jobs		(AUTO GENERATED BY TOOL)
//            //**************************************************************************

//            var job_JobPulse = new Job
//            {
//                NotificationStatus = Rock.Util.JobNotificationStatus.None,
//                Name = @"Job Pulse",
//                Class = @"Rock.Jobs.JobPulse",
//                LastStatus = @"Success",
//                System = true,
//                LastRunSchedulerName = @"RockSchedulerIIS",
//                LastRunDuration = 0,
//                CronExpression = @"30 * * ? * SUN,MON,TUE,WED,THU,FRI,SAT",
//                Active = true,
//                Guid = new Guid( "CB24FF2A-5AD3-4976-883F-DAF4EFC1D7C7" ),
//                Description = @"System job that allows Rock to monitor the jobs engine."
//            };
//            context.Jobs.AddOrUpdate( p => p.Guid, job_JobPulse );

//            var job_RockCleanup = new Job
//            {
//                NotificationStatus = Rock.Util.JobNotificationStatus.None,
//                Name = @"Rock Cleanup",
//                Class = @"Rock.Jobs.RockCleanup",
//                LastStatus = @"Success",
//                System = true,
//                LastRunSchedulerName = @"RockSchedulerIIS",
//                LastRunDuration = 0,
//                CronExpression = @"0 0 1 * * ?",
//                Active = true,
//                Guid = new Guid( "1A8238B1-038A-4295-9FDE-C6D93002A5D7" ),
//                Description = @"General job to clean up various areas of Rock."
//            };
//            context.Jobs.AddOrUpdate( p => p.Guid, job_RockCleanup );

//            context.SaveChanges();

//            //
//            // Some stuff not easily handling via the automated tool.
//            // Stuff such as AttributeValue values -- because these values sometimes
//            // refer to some other (nearly impossible to programatically determine)
//            // entity.  Example: The RootPage attribute's attribute value will
//            // refer to a pageId. We know this because we coded it that way.
//            // Perhaps that Attribute should be named RootPageId so that the Page entity
//            // could be inferred or perhaps the Attribute / AttributeValue should be of
//            // type "page".
//            //

//            attributevalue_12.Value = page12_MainPageTest.Id.ToString();
//            context.AttributeValues.AddOrUpdate( p => p.Guid, attributevalue_12 );
//            context.SaveChanges();

//            //foreach ( Page page in context.Pages )
//            //{
//            //    var auth = new Auth
//            //    {
//            //        EntityType = "CMS.Page",
//            //        EntityId = page.Id,
//            //        Action = "Configure",
//            //        GroupId = adminRoleGroup.Id,
//            //        AllowOrDeny = "A",
//            //        Guid = Guid.NewGuid()
//            //    };
//            //    //context.Auths.AddOrUpdate( p => deleg2, auth );
//            //    context.Auths.AddOrUpdate( p => new { p.EntityType, p.EntityId , p.Action}, auth );
//            //}
//            //context.SaveChanges();

//        }
	}
}
