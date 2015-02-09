// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class UpdateSecuritySettingsPageNames : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //
            // Rename some admin pages (all v3.0 books have been updated that had text references to these pages) 
            //

            // rename 'Communication Settings -> Communications'
            Sql( @"UPDATE [Page]
	                SET [PageTitle] = 'Communications'
		                , [BrowserTitle] = 'Communications'
		                , [InternalName] = 'Communications'
	                WHERE [Guid] = '199DC522-F4D6-4D82-AF44-3C16EE9D2CDA'" );

            // rename 'Check-in Settings -> Check-in'
            Sql( @"UPDATE [Page]
	                SET [PageTitle] = 'Check-in'
		                , [BrowserTitle] = 'Check-in'
		                , [InternalName] = 'Check-in'
	                WHERE [Guid] = '66C5DD58-094C-4FF9-9AFB-44801FCFCC2D'" );

            //
            // Move the 'Photo Request' page under 'Admin Tools > Communications'
            //
            Sql( @"DECLARE @CommunicationsAdminPageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '199DC522-F4D6-4D82-AF44-3C16EE9D2CDA')
                    UPDATE [Page]
	                    SET [ParentPageId] = @CommunicationsAdminPageId
		                    , [Order] = 9
	                    WHERE [Guid] = '325B50D6-545D-461A-9CB7-72B001E82F21'" );

            //
            // add explicit view rules to all admin pages
            //

            // general settings
            RockMigrationHelper.AddSecurityAuthForPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", 0, "View", true, "628C51A8-4613-43ED-A18D-4A6FB999273E", 0, "31a9d077-3097-7395-42bb-4ec8e3fd884a" ); // allow admin view
            RockMigrationHelper.AddSecurityAuthForPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", 1, "View", false, null, 1, "e78d695d-7476-2c89-4834-a56f78933faa" ); // deny all

            // cms
            RockMigrationHelper.AddSecurityAuthForPage( "B4A24AB7-9369-4055-883F-4F4892C39AE3", 0, "View", true, "628C51A8-4613-43ED-A18D-4A6FB999273E", 0, "37eb9790-6425-d0a0-4a18-6f66e05f080c" ); // allow admin view
            RockMigrationHelper.AddSecurityAuthForPage( "B4A24AB7-9369-4055-883F-4F4892C39AE3", 1, "View", false, null, 1, "c842d9cf-b37b-059f-49c7-edee02aeb276" ); // deny all

            // security
            RockMigrationHelper.AddSecurityAuthForPage( "91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F", 0, "View", true, "628C51A8-4613-43ED-A18D-4A6FB999273E", 0, "82a94ba7-7098-c9ba-4b9a-8e0f07507fdc" ); // allow admin view
            RockMigrationHelper.AddSecurityAuthForPage( "91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F", 1, "View", false, null, 1, "ecfec2cc-c578-688c-4d50-12afbb83a6f9" ); // deny all

            // communications
            RockMigrationHelper.AddSecurityAuthForPage( "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", 0, "View", true, "628C51A8-4613-43ED-A18D-4A6FB999273E", 0, "094ff105-1454-dba5-406c-688bc8736f98" ); // allow admin view
            RockMigrationHelper.AddSecurityAuthForPage( "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", 1, "View", false, null, 1, "22469ab2-15c7-6299-4b69-a80575c6303d" ); // deny all

            // power tools
            RockMigrationHelper.AddSecurityAuthForPage( "7F1F4130-CB98-473B-9DE1-7A886D2283ED", 0, "View", true, "628C51A8-4613-43ED-A18D-4A6FB999273E", 0, "27bb4ed9-cb81-5f8a-4f3c-4049100337e1" ); // allow admin view
            RockMigrationHelper.AddSecurityAuthForPage( "7F1F4130-CB98-473B-9DE1-7A886D2283ED", 1, "View", false, null, 1, "332549e8-2092-5c8b-4756-5c31229db9c6" ); // deny all

            // system settings
            RockMigrationHelper.AddSecurityAuthForPage( "C831428A-6ACD-4D49-9B2D-046D399E3123", 0, "View", true, "628C51A8-4613-43ED-A18D-4A6FB999273E", 0, "74d3d1a1-7b57-4f90-4e09-55d8ada5181e" ); // allow admin view
            RockMigrationHelper.AddSecurityAuthForPage( "C831428A-6ACD-4D49-9B2D-046D399E3123", 1, "View", false, null, 1, "ce1f30c9-8402-97a3-4f06-67656221aaeb" ); // deny all

            // check-in
            RockMigrationHelper.AddSecurityAuthForPage( "66C5DD58-094C-4FF9-9AFB-44801FCFCC2D", 0, "View", true, "628C51A8-4613-43ED-A18D-4A6FB999273E", 0, "82ae62a7-7806-2db8-4b1b-3e8ed2c50da9" ); // allow admin view
            RockMigrationHelper.AddSecurityAuthForPage( "66C5DD58-094C-4FF9-9AFB-44801FCFCC2D", 1, "View", false, null, 1, "3050df7b-8f08-b5a8-4e04-e280b0b75ffc" ); // deny all

            // rock shop
            RockMigrationHelper.AddSecurityAuthForPage( "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA", 0, "View", true, "628C51A8-4613-43ED-A18D-4A6FB999273E", 0, "dac981df-0387-788c-4c19-aa32640dc96d" ); // allow admin view
            RockMigrationHelper.AddSecurityAuthForPage( "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA", 1, "View", false, null, 1, "62b84704-3c69-11ba-4641-eb66379d97e8" ); // deny all


            // add access to Admin Tools & Communications pages for Communication Administrators role
            RockMigrationHelper.AddSecurityAuthForPage( "84E12152-E456-478E-AF68-BA640E9CE65B", 0, "View", true, "B1906B7D-1A1E-41B9-BBA4-F4482CECAF7B", 0, "565db4ae-719c-98b8-4059-366bab9c61f8" ); // admin tools
            
            RockMigrationHelper.AddSecurityAuthForPage( "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", 0, "View", true, "B1906B7D-1A1E-41B9-BBA4-F4482CECAF7B", 0, "99b8f35b-4e7d-65a3-4c83-1a1ae0d50ef7" ); // communications view
            RockMigrationHelper.AddSecurityAuthForPage( "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", 0, "Edit", true, "B1906B7D-1A1E-41B9-BBA4-F4482CECAF7B", 0, "c19d56e3-06e7-4f97-4176-9131c46b52fd" ); // communications edit
            RockMigrationHelper.AddSecurityAuthForPage( "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", 0, "Administrate", true, "B1906B7D-1A1E-41B9-BBA4-F4482CECAF7B", 0, "5de8c15f-da6e-e0ba-4eaa-2f25f0d226a0" ); // communications admin


            // add access to Admin Tools & CMS Settings pages for Website Administrators role
            RockMigrationHelper.AddSecurityAuthForPage( "84E12152-E456-478E-AF68-BA640E9CE65B", 0, "View", true, "1918E74F-C00D-4DDD-94C4-2E7209CE12C3", 0, "5bfa6aea-c24c-c39c-4240-c1babe517f36" ); // admin tools
            
            RockMigrationHelper.AddSecurityAuthForPage( "B4A24AB7-9369-4055-883F-4F4892C39AE3", 0, "View", true, "B1906B7D-1A1E-41B9-BBA4-F4482CECAF7B", 0, "5e2f609b-0b11-86bf-430d-8fff7b864163" ); // cms settings view
            RockMigrationHelper.AddSecurityAuthForPage( "B4A24AB7-9369-4055-883F-4F4892C39AE3", 0, "Edit", true, "B1906B7D-1A1E-41B9-BBA4-F4482CECAF7B", 0, "c02e8409-2cd7-4088-4ead-f58008d740ae" ); // cms settings edit
            RockMigrationHelper.AddSecurityAuthForPage( "B4A24AB7-9369-4055-883F-4F4892C39AE3", 0, "Administrate", true, "B1906B7D-1A1E-41B9-BBA4-F4482CECAF7B", 0, "edad9191-0abb-5590-440e-0401060c8024" ); // cms settings admin
        
            //
            // give web administrators edit access to the external website ads content channel
            //

            Sql( @"  DECLARE @ExternalSiteAdChannelId int = (SELECT TOP 1 [Id] FROM [ContentChannel] WHERE [Guid] = '8E213BB1-9E6F-40C1-B468-B3F8A60D5D24')
  DECLARE @ChannelEntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.ContentChannel')
  DECLARE @WebAdminGroupId int = (SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '1918E74F-C00D-4DDD-94C4-2E7209CE12C3')

  DECLARE @SecurityExists int = (SELECT COUNT(*) FROM AUTH WHERE [EntityTypeId] = @ChannelEntityTypeId AND [EntityId] = @ExternalSiteAdChannelId AND [GroupId] = @WebAdminGroupId AND [Action] = 'Edit' AND [AllowOrDeny] = 'A')

  IF (@SecurityExists = 0)
  BEGIN
	INSERT INTO [Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
		VALUES
			(@ChannelEntityTypeId, @ExternalSiteAdChannelId, 0, 'Edit', 'A', 0, @WebAdminGroupId, 'efc74512-a659-75b5-4add-7a3b2b039569')
END

SET @SecurityExists = (SELECT COUNT(*) FROM AUTH WHERE [EntityTypeId] = @ChannelEntityTypeId AND [EntityId] = @ExternalSiteAdChannelId AND [GroupId] = @WebAdminGroupId AND [Action] = 'Administrate' AND [AllowOrDeny] = 'A')

  IF (@SecurityExists = 0)
  BEGIN
	INSERT INTO [Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
		VALUES
			(@ChannelEntityTypeId, @ExternalSiteAdChannelId, 0, 'Administrate', 'A', 0, @WebAdminGroupId, 'c6c72677-060f-b4bf-4e94-2fe5e96e278b')
END" );

            //
            // Move the Content Channel admin pages under 'Admin Tools > CMS Configuration'
            //
            Sql( @"DECLARE @CmsConfigPageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'B4A24AB7-9369-4055-883F-4F4892C39AE3')
                    UPDATE [Page]
	                    SET [ParentPageId] = @CmsConfigPageId
		                    , [Order] = 12
	                    WHERE [Guid] = '37E3D602-5D7D-4818-BCAA-C67EBB301E55'

                    UPDATE [Page]
	                    SET [ParentPageId] = @CmsConfigPageId
		                    , [Order] = 13
	                    WHERE [Guid] = '8ADCE4B2-8E95-4FA3-89C4-06A883E8145E'
                    " );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //
            // Rename some admin pages (all v3.0 books have been updated that had text references to these pages) 
            //

            // rename 'Communication Settings -> Communications'
            Sql( @"UPDATE [Page]
	                SET [PageTitle] = 'Communications Settings'
		                , [BrowserTitle] = 'Communications Settings'
		                , [InternalName] = 'Communications Settings'
	                WHERE [Guid] = '199DC522-F4D6-4D82-AF44-3C16EE9D2CDA'" );

            // rename 'Check-in Settings -> Check-in'
            Sql( @"UPDATE [Page]
	                SET [PageTitle] = 'Check-in Settings'
		                , [BrowserTitle] = 'Check-in Settings'
		                , [InternalName] = 'Check-in Settings'
	                WHERE [Guid] = '66C5DD58-094C-4FF9-9AFB-44801FCFCC2D'" );

            //
            // Move the 'Photo Request' page under 'Admin Tools > Communications'
            //
            Sql( @"DECLARE @PeoplePageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '97ECDC48-6DF6-492E-8C72-161F76AE111B')
                    UPDATE [Page]
	                    SET [ParentPageId] = @PeoplePageId
		                    , [Order] = 9
	                    WHERE [Guid] = '325B50D6-545D-461A-9CB7-72B001E82F21'" );

            //
            // Move the Content Channel admin pages under 'Admin Tools > Communications'
            //
            Sql( @"DECLARE @CommPageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '199DC522-F4D6-4D82-AF44-3C16EE9D2CDA')
                    UPDATE [Page]
	                    SET [ParentPageId] = @CommPageId
		                    , [Order] = 9
	                    WHERE [Guid] = '37E3D602-5D7D-4818-BCAA-C67EBB301E55'

                    UPDATE [Page]
	                    SET [ParentPageId] = @CommPageId
		                    , [Order] = 9
	                    WHERE [Guid] = '8ADCE4B2-8E95-4FA3-89C4-06A883E8145E'
                    " );


            //
            // remove added security
            //

            Sql( @"DELETE FROM [Auth] WHERE [Guid] IN
                    ('efc74512-a659-75b5-4add-7a3b2b039569', 'c6c72677-060f-b4bf-4e94-2fe5e96e278b', '31a9d077-3097-7395-42bb-4ec8e3fd884a','e78d695d-7476-2c89-4834-a56f78933faa','37eb9790-6425-d0a0-4a18-6f66e05f080c', 'c842d9cf-b37b-059f-49c7-edee02aeb276', '82a94ba7-7098-c9ba-4b9a-8e0f07507fdc', 'ecfec2cc-c578-688c-4d50-12afbb83a6f9', '094ff105-1454-dba5-406c-688bc8736f98', '22469ab2-15c7-6299-4b69-a80575c6303d','27bb4ed9-cb81-5f8a-4f3c-4049100337e1', '332549e8-2092-5c8b-4756-5c31229db9c6', '74d3d1a1-7b57-4f90-4e09-55d8ada5181e', 'ce1f30c9-8402-97a3-4f06-67656221aaeb', '82ae62a7-7806-2db8-4b1b-3e8ed2c50da9', '3050df7b-8f08-b5a8-4e04-e280b0b75ffc', 'dac981df-0387-788c-4c19-aa32640dc96d', '62b84704-3c69-11ba-4641-eb66379d97e8', '565db4ae-719c-98b8-4059-366bab9c61f8', '99b8f35b-4e7d-65a3-4c83-1a1ae0d50ef7', 'c19d56e3-06e7-4f97-4176-9131c46b52fd', '5de8c15f-da6e-e0ba-4eaa-2f25f0d226a0', '5bfa6aea-c24c-c39c-4240-c1babe517f36', '5e2f609b-0b11-86bf-430d-8fff7b864163', 'c02e8409-2cd7-4088-4ead-f58008d740ae', 'edad9191-0abb-5590-440e-0401060c8024')" );
        }
    }
}
