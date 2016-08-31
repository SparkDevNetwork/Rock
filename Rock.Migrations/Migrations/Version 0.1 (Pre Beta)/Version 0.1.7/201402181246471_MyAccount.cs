// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
    public partial class MyAccount : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "C0854F84-2E8B-479C-A3FB-6B47BE89B795", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Edit My Account", "", "4A4655D1-BDD9-4ECE-A3F6-B655F0BDF9F5", "" ); // Site:Rock Solid Church
            AddBlockType( "My Account", "Public block for user to manager their account", "~/Blocks/Security/MyAccount.ascx", "B37DC24D-9DE4-4E94-B8E1-9BCB03A835F1" );
            AddBlockType( "Edit My Account", "Allows a person to edit their account information.", "~/Blocks/Security/EditMyAccount.ascx", "F501AB3F-1F41-4C06-9BC2-57C42E702995" );

            // Add Block to Page: My Account, Site: Rock Solid Church
            AddBlock( "C0854F84-2E8B-479C-A3FB-6B47BE89B795", "", "B37DC24D-9DE4-4E94-B8E1-9BCB03A835F1", "My Account", "Main", "", "", 0, "54DA7430-6884-4C2E-9A28-123DA211E02E" );
            // Add Block to Page: My Account, Site: Rock Solid Church
            AddBlock( "C0854F84-2E8B-479C-A3FB-6B47BE89B795", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Actions", "Sidebar1", "", "", 0, "87068AAB-16A7-42CC-8A31-5A957D6C4DD5" );
            
            // Add Block to Page: Edit My Account, Site: Rock Solid Church
            AddBlock( "4A4655D1-BDD9-4ECE-A3F6-B655F0BDF9F5", "", "F501AB3F-1F41-4C06-9BC2-57C42E702995", "Edit My Account", "Main", "", "", 0, "539A5E9C-DE5F-4991-BCC4-9C4AC3151C7B" );
            
            // Attrib for BlockType: My Account:Detail Page
            AddBlockTypeAttribute( "B37DC24D-9DE4-4E94-B8E1-9BCB03A835F1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Page to edit account details", 0, @"", "534190B5-3EC9-4497-B394-31122154FCE5" );
            // Attrib Value for Block:My Account, Attribute:Detail Page Page: My Account, Site: Rock Solid Church
            AddBlockAttributeValue( "54DA7430-6884-4C2E-9A28-123DA211E02E", "534190B5-3EC9-4497-B394-31122154FCE5", @"4a4655d1-bdd9-4ece-a3f6-b655f0bdf9f5" );

            // Attrib Value for Block:Actions, Attribute:Use Code Editor Page: My Account, Site: Rock Solid Church
            AddBlockAttributeValue( "87068AAB-16A7-42CC-8A31-5A957D6C4DD5", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );
            // Attrib Value for Block:Actions, Attribute:Document Root Folder Page: My Account, Site: Rock Solid Church
            AddBlockAttributeValue( "87068AAB-16A7-42CC-8A31-5A957D6C4DD5", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );
            // Attrib Value for Block:Actions, Attribute:Image Root Folder Page: My Account, Site: Rock Solid Church
            AddBlockAttributeValue( "87068AAB-16A7-42CC-8A31-5A957D6C4DD5", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );
            // Attrib Value for Block:Actions, Attribute:User Specific Folders Page: My Account, Site: Rock Solid Church
            AddBlockAttributeValue( "87068AAB-16A7-42CC-8A31-5A957D6C4DD5", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );
            // Attrib Value for Block:Actions, Attribute:Cache Duration Page: My Account, Site: Rock Solid Church
            AddBlockAttributeValue( "87068AAB-16A7-42CC-8A31-5A957D6C4DD5", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );
            // Attrib Value for Block:Actions, Attribute:Context Parameter Page: My Account, Site: Rock Solid Church
            AddBlockAttributeValue( "87068AAB-16A7-42CC-8A31-5A957D6C4DD5", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" );
            // Attrib Value for Block:Actions, Attribute:Context Name Page: My Account, Site: Rock Solid Church
            AddBlockAttributeValue( "87068AAB-16A7-42CC-8A31-5A957D6C4DD5", "466993F7-D838-447A-97E7-8BBDA6A57289", @"" );
            // Attrib Value for Block:Actions, Attribute:Require Approval Page: My Account, Site: Rock Solid Church
            AddBlockAttributeValue( "87068AAB-16A7-42CC-8A31-5A957D6C4DD5", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );
            // Attrib Value for Block:Actions, Attribute:Support Versions Page: My Account, Site: Rock Solid Church
            AddBlockAttributeValue( "87068AAB-16A7-42CC-8A31-5A957D6C4DD5", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );

            Sql( @"
    DECLARE @LayoutId int = (SELECT [Id] FROM [Layout] WHERE [Guid] = 'ADC7866C-D0B3-4B97-9AB2-EACB36EA24ED')
    UPDATE [Page] SET [LayoutId] = @LayoutId WHERE [Guid] = 'C0854F84-2E8B-479C-A3FB-6B47BE89B795'

    DECLARE @BlockId int = (SELECT [Id] FROM [Block] WHERE [Guid] = '87068AAB-16A7-42CC-8A31-5A957D6C4DD5')
    INSERT INTO
        [HtmlContent] (
            [BlockId],[EntityValue],[Version],[Content],[IsApproved],[Guid]
        )
    VALUES (
         @BlockId
        , ''
        , 1
        , 
'<div class=''panel panel-default''>
    <div class=''panel-heading''>Account Info</div>
    <div class=''panel-body''>
        <a href=''~/ChangePassword''>Change Password</a>
    </div>
</div>'
        ,1
        ,'A1D209C1-96C2-43B9-9C75-F6445A1FACC6'
    )
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    DELETE [HtmlContent] WHERE [Guid] = 'A1D209C1-96C2-43B9-9C75-F6445A1FACC6'
" );

            // Attrib for BlockType: My Account:Detail Page
            DeleteAttribute( "534190B5-3EC9-4497-B394-31122154FCE5" );

            // Remove Block: Actions, from Page: My Account, Site: Rock Solid Church
            DeleteBlock( "87068AAB-16A7-42CC-8A31-5A957D6C4DD5" );
            // Remove Block: My Account, from Page: My Account, Site: Rock Solid Church
            DeleteBlock( "54DA7430-6884-4C2E-9A28-123DA211E02E" );

            // Remove Block: Edit My Account, from Page: Edit My Account, Site: Rock Solid Church
            DeleteBlock( "539A5E9C-DE5F-4991-BCC4-9C4AC3151C7B" );

            DeleteBlockType( "F501AB3F-1F41-4C06-9BC2-57C42E702995" ); // Edit My Account
            DeleteBlockType( "B37DC24D-9DE4-4E94-B8E1-9BCB03A835F1" ); // My Account
            
            DeletePage( "4A4655D1-BDD9-4ECE-A3F6-B655F0BDF9F5" ); // Page: Edit My AccountLayout: FullWidth, Site: Rock Solid Church
        }
    }
}
