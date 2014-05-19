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
namespace com.ccvonline.Residency.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class CreateResidencySite : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Residency Security
            AddPage( "82B81403-8A93-4F42-A958-5303C3AF1508", "Security", "", "Default", "FB6EC263-9C20-41ED-9BCC-F53BEE50CBA8", "" );

            // Hide Security Page from Navigation
            Sql( "update [Page] set [DisplayInNavWhen] = 2 where [Guid] = 'FB6EC263-9C20-41ED-9BCC-F53BEE50CBA8'" );

            // Residency Login
            AddPage( "FB6EC263-9C20-41ED-9BCC-F53BEE50CBA8", "Login", "", "Splash", "07770489-9C8D-43FA-85B3-E99BB54D3353", "" );

            AddBlock( "07770489-9C8D-43FA-85B3-E99BB54D3353", "7B83D513-1178-429E-93FF-E76430E038E4", "Login", "", "Content", 0, "C88645F1-BDB6-4A08-A581-587DCCB40A3B" );

            // Create site for Residency and Update residency pages
            Sql( @"
DECLARE @PageId int
DECLARE @SiteId int
DECLARE @LoginPageReference NVARCHAR(260)

-- 'Resident Home' page is default page for site
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '826C0BFF-C831-4427-98F9-57FF462D82F5')

SET @LoginPageReference = (select '~/page/' + CONVERT(nvarchar(10), [Id]) from [Page] where [Guid] = '07770489-9C8D-43FA-85B3-E99BB54D3353')


INSERT INTO [Site] (IsSystem, Name, Description, Theme, DefaultPageId, LoginPageReference, Guid)
    VALUES (0, 'Residency', 'The site for the Residency Resident pages', 'Residency', @PageId, @LoginPageReference, '960F1D98-891A-4508-8F31-3CF206F5406E')

SET @SiteId = SCOPE_IDENTITY()

-- Update Resident pages to use new site (default layout)
UPDATE [Page] SET 
    [SiteId] = @SiteId,
    [Layout] = 'Default'
WHERE [Guid] in (
'130FA92D-9D5F-45D1-84AA-B399F2E868E6',
'83DBB422-38C5-44F3-9FDE-3737AC8CF2A7',
'0DF59029-C17B-474D-8DD1-ED312B734202',
'4827C8D3-B0FA-4AA4-891F-1F27C7D76606',
'A4BE6749-0190-4655-B3F6-0CEEC2DDD5C4',
'A16C4B0F-66C6-4CF0-8B54-B232DDF553B9',
'5D729D30-8E33-4913-A56F-98F803479C6D',
'56F3E462-28EF-4EC5-A58C-C5FDE48356E0',
'ADE663B9-386B-479C-ABD9-3349E1B4B827',
'826C0BFF-C831-4427-98F9-57FF462D82F5',
'07770489-9C8D-43FA-85B3-E99BB54D3353')

-- Update Resident Login page to use new site (splash layout)
UPDATE [Page] SET 
    [SiteId] = @SiteId,
    [Layout] = 'Splash'
WHERE [Guid] in (
'07770489-9C8D-43FA-85B3-E99BB54D3353')

" );


            // Set Page Security for Residency Login Page to Allow All Users
            Sql( @"
DECLARE @entityTypeId int
DECLARE @pageId int

set @entityTypeId = (select [Id] from [EntityType] where [Name] = 'Rock.Model.Page')
set @pageId = (select [Id] from [Page] where [Guid] = '07770489-9C8D-43FA-85B3-E99BB54D3353')

delete from [Auth] where [Guid] = '69246000-8944-471B-A848-91E16D6486BE'

INSERT INTO [Auth]
           ([EntityTypeId]
           ,[EntityId]
           ,[Order]
           ,[Action]
           ,[AllowOrDeny]
           ,[SpecialRole]
           ,[PersonId]
           ,[GroupId]
           ,[Guid])
     VALUES
           (@entityTypeId
           ,@pageId
           ,0
           ,'View'
           ,'A'
           ,1
           ,NULL
           ,NULL
           ,'69246000-8944-471B-A848-91E16D6486BE')
" );
        }


        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"

update [Page] set [SiteId] = 1 where [Guid] in 
(
'130FA92D-9D5F-45D1-84AA-B399F2E868E6',
'83DBB422-38C5-44F3-9FDE-3737AC8CF2A7',
'0DF59029-C17B-474D-8DD1-ED312B734202',
'4827C8D3-B0FA-4AA4-891F-1F27C7D76606',
'A4BE6749-0190-4655-B3F6-0CEEC2DDD5C4',
'A16C4B0F-66C6-4CF0-8B54-B232DDF553B9',
'5D729D30-8E33-4913-A56F-98F803479C6D',
'56F3E462-28EF-4EC5-A58C-C5FDE48356E0',
'ADE663B9-386B-479C-ABD9-3349E1B4B827',
'826C0BFF-C831-4427-98F9-57FF462D82F5',
'07770489-9C8D-43FA-85B3-E99BB54D3353')

delete from [Site] where [Guid] = '960F1D98-891A-4508-8F31-3CF206F5406E';

delete from [Auth] where [Guid] = '69246000-8944-471B-A848-91E16D6486BE'

" );

            DeletePage( "07770489-9C8D-43FA-85B3-E99BB54D3353" ); // Residency Login
            DeletePage( "FB6EC263-9C20-41ED-9BCC-F53BEE50CBA8" ); // Residency Security

            DeleteBlock( "C88645F1-BDB6-4A08-A581-587DCCB40A3B" ); // Residency Login 
        }
    }
}
