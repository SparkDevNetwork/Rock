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
    public partial class AddOidcModelsAndPages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.AuthClaim",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    IsActive = c.Boolean( nullable: false ),
                    IsSystem = c.Boolean( nullable: false ),
                    Name = c.String( nullable: false, maxLength: 50 ),
                    PublicName = c.String( maxLength: 100 ),
                    ScopeId = c.Int( nullable: false ),
                    Value = c.String(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.AuthScope", t => t.ScopeId, cascadeDelete: true )
                .Index( t => t.Name, unique: true )
                .Index( t => t.ScopeId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.AuthScope",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    IsActive = c.Boolean( nullable: false ),
                    IsSystem = c.Boolean( nullable: false ),
                    Name = c.String( nullable: false, maxLength: 50 ),
                    PublicName = c.String( maxLength: 100 ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.Name, unique: true )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.AuthClient",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    AllowUserApiAccess = c.Boolean( nullable: false ),
                    AllowedClaims = c.String(),
                    AllowedScopes = c.String(),
                    IsActive = c.Boolean( nullable: false ),
                    Name = c.String( nullable: false ),
                    ClientId = c.String( nullable: false, maxLength: 50 ),
                    ClientSecretHash = c.String(),
                    RedirectUri = c.String( nullable: false ),
                    PostLogoutRedirectUri = c.String( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.ClientId, unique: true )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            AddScopes();
            AddClaims();
            AddBlockPagesAttributes();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.AuthClient", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AuthClient", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AuthClaim", "ScopeId", "dbo.AuthScope" );
            DropForeignKey( "dbo.AuthScope", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AuthScope", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AuthClaim", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AuthClaim", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.AuthClient", new[] { "Guid" } );
            DropIndex( "dbo.AuthClient", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.AuthClient", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.AuthClient", new[] { "ClientId" } );
            DropIndex( "dbo.AuthScope", new[] { "Guid" } );
            DropIndex( "dbo.AuthScope", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.AuthScope", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.AuthScope", new[] { "Name" } );
            DropIndex( "dbo.AuthClaim", new[] { "Guid" } );
            DropIndex( "dbo.AuthClaim", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.AuthClaim", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.AuthClaim", new[] { "ScopeId" } );
            DropIndex( "dbo.AuthClaim", new[] { "Name" } );
            DropTable( "dbo.AuthClient" );
            DropTable( "dbo.AuthScope" );
            DropTable( "dbo.AuthClaim" );
        }

        private void AddScopes()
        {
            var sql = $@"INSERT INTO AuthScope ([IsActive], [IsSystem], [Name], [PublicName], [CreatedDateTime], [ModifiedDateTime], [Guid])
                        VALUES (1, 1, 'address', 'Address', GETDATE(), GETDATE(), '{Rock.SystemGuid.AuthScope.ADDRESS}')
                        , (1, 1, 'email', 'Email Address', GETDATE(), GETDATE(), '{Rock.SystemGuid.AuthScope.EMAIL}')
                        , (1, 1, 'offline_access', 'Allows the use of refresh tokens.', GETDATE(), GETDATE(), '{Rock.SystemGuid.AuthScope.OFFLINE}')
                        , (1, 1, 'phone', 'Phone Number', GETDATE(), GETDATE(), '{Rock.SystemGuid.AuthScope.PHONE}')
                        , (1, 1, 'profile', 'Profile Information', GETDATE(), GETDATE(), '{Rock.SystemGuid.AuthScope.PROFILE}')";
            Sql( sql );
        }

        private void AddClaims()
        {
            var sql = $@"INSERT INTO AuthClaim ([IsActive], [IsSystem], [Name], [PublicName], [ScopeId], [CreatedDateTime], [ModifiedDateTime], [Guid])
                        VALUES (1, 1, 'address', 'Address', (SELECT TOP 1 Id FROM AuthScope WHERE [Guid] = '{Rock.SystemGuid.AuthScope.ADDRESS}'), GETDATE(), GETDATE(), NEWID())
                        , (1, 1, 'email', 'Email Address', (SELECT TOP 1 Id FROM AuthScope WHERE [Guid] = '{Rock.SystemGuid.AuthScope.EMAIL}'), GETDATE(), GETDATE(), NEWID())
                        , (1, 1, 'phone_number', 'Phone Number', (SELECT TOP 1 Id FROM AuthScope WHERE [Guid] = '{Rock.SystemGuid.AuthScope.PHONE}'), GETDATE(), GETDATE(), NEWID())
                        , (1, 1, 'name', 'Full Name', (SELECT TOP 1 Id FROM AuthScope WHERE [Guid] = '{Rock.SystemGuid.AuthScope.PROFILE}'), GETDATE(), GETDATE(), NEWID())
                        , (1, 1, 'family_name', 'Last Name', (SELECT TOP 1 Id FROM AuthScope WHERE [Guid] = '{Rock.SystemGuid.AuthScope.PROFILE}'), GETDATE(), GETDATE(), NEWID())
                        , (1, 1, 'given_name', 'First Name', (SELECT TOP 1 Id FROM AuthScope WHERE [Guid] = '{Rock.SystemGuid.AuthScope.PROFILE}'), GETDATE(), GETDATE(), NEWID())
                        , (1, 1, 'middle_name', 'Middle Name', (SELECT TOP 1 Id FROM AuthScope WHERE [Guid] = '{Rock.SystemGuid.AuthScope.PROFILE}'), GETDATE(), GETDATE(), NEWID())
                        , (1, 1, 'nickname', 'Nickname', (SELECT TOP 1 Id FROM AuthScope WHERE [Guid] = '{Rock.SystemGuid.AuthScope.PROFILE}'), GETDATE(), GETDATE(), NEWID())
                        , (1, 1, 'preferred_username', 'Full Name', (SELECT TOP 1 Id FROM AuthScope WHERE [Guid] = '{Rock.SystemGuid.AuthScope.PROFILE}'), GETDATE(), GETDATE(), NEWID())
                        , (1, 1, 'picture', 'Photo', (SELECT TOP 1 Id FROM AuthScope WHERE [Guid] = '{Rock.SystemGuid.AuthScope.PROFILE}'), GETDATE(), GETDATE(), NEWID())
                        , (1, 1, 'gender', 'Gender', (SELECT TOP 1 Id FROM AuthScope WHERE [Guid] = '{Rock.SystemGuid.AuthScope.PROFILE}'), GETDATE(), GETDATE(), NEWID())";
            Sql( sql );
        }

        private void AddBlockPagesAttributes()
        {

            // Add Page Give Permission to Site:External Website
            RockMigrationHelper.AddPage( true, Rock.SystemGuid.Page.SECURITY_ADMIN_TOOLS, Rock.SystemGuid.Layout.FULL_WIDTH, "Give Permission", "", Rock.SystemGuid.Page.OIDC_GIVE_PERMISSION, "" );
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.OIDC_GIVE_PERMISSION, 0, Rock.Security.Authorization.VIEW, true, null, ( int ) Rock.Model.SpecialRole.AllAuthenticatedUsers, "AB7172FB-F504-4772-B6BD-62F5600C0EB9" );

            // Add Page Logout to Site:External Website
            RockMigrationHelper.AddPage( true, Rock.SystemGuid.Page.SUPPORT_PAGES_EXTERNAL_SITE, "55E19934-762D-48E5-BD07-ACB1249ACBDC", "Logout", "", Rock.SystemGuid.Page.OIDC_LOGOUT, "" );

            // Add Page OpenID Connect Client to Site:Rock RMS
            RockMigrationHelper.AddPage( true, Rock.SystemGuid.Page.SECURITY_ROCK_SETTINGS, Rock.SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "OpenID Connect Clients", "", Rock.SystemGuid.Page.OIDC_CLIENT_LIST, "fa fa-openid" );

            // Add Page OpenID Connect Scopes to Site:Rock RMS
            RockMigrationHelper.AddPage( true, Rock.SystemGuid.Page.OIDC_CLIENT_LIST, Rock.SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "OpenID Connect Scopes", "", Rock.SystemGuid.Page.OIDC_SCOPE_LIST, "" );

            // Add Page OpenID Connect Scope Detail to Site:Rock RMS
            RockMigrationHelper.AddPage( true, Rock.SystemGuid.Page.OIDC_SCOPE_LIST, Rock.SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "OpenID Connect Scope Detail", "", Rock.SystemGuid.Page.OIDC_SCOPE_DETAIL, "" );

            // Add Page OpenID Connect Client Detail to Site:Rock RMS
            RockMigrationHelper.AddPage( true, Rock.SystemGuid.Page.OIDC_CLIENT_LIST, Rock.SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "OpenID Connect Client Detail", "", Rock.SystemGuid.Page.OIDC_CLIENT_DETAIL, "" );

#pragma warning disable CS0618 // Type or member is obsolete
            // Add Page Route for Give Permission
            RockMigrationHelper.AddPageRoute( Rock.SystemGuid.Page.OIDC_GIVE_PERMISSION, "Auth/Authorize", Rock.SystemGuid.PageRoute.OIDC_AUTHORIZE );

            // Add Page Route for Logout
            RockMigrationHelper.AddPageRoute( Rock.SystemGuid.Page.OIDC_LOGOUT, "Auth/Logout", Rock.SystemGuid.PageRoute.OIDC_LOGOUT );
#pragma warning restore CS0618 // Type or member is obsolete

            // Add/Update BlockType Authorize
            RockMigrationHelper.UpdateBlockType( "Authorize", "Choose to authorize the auth client to access the user's data.", "~/Blocks/Security/Oidc/Authorize.ascx", "Oidc", Rock.SystemGuid.BlockType.OIDC_AUTHORIZE );

            // Add/Update BlockType Logout
            RockMigrationHelper.UpdateBlockType( "Logout", "Choose to authorize the auth client to access the user's data.", "~/Blocks/Security/Oidc/Logout.ascx", "Oidc", Rock.SystemGuid.BlockType.OIDC_LOGOUT );

            // Add/Update BlockType OpenID Connect Scopes
            RockMigrationHelper.UpdateBlockType( "OpenID Connect Scopes", "Block for displaying and editing available OpenID Connect scopes.", "~/Blocks/Security/Oidc/AuthScopeList.ascx", "Oidc", Rock.SystemGuid.BlockType.OIDC_SCOPE_LIST );

            // Add/Update BlockType OpenID Connect Scope Detail
            RockMigrationHelper.UpdateBlockType( "OpenID Connect Scope Detail", "Displays the details of the given OpenID Connect Scope.", "~/Blocks/Security/Oidc/AuthScopeDetail.ascx", "Oidc", Rock.SystemGuid.BlockType.OIDC_SCOPE_DETAIL );

            // Add/Update BlockType OpenID Connect Scopes
            RockMigrationHelper.UpdateBlockType( "OpenID Connect Scopes", "Block for displaying and editing available OpenID Connect scopes.", "~/Blocks/Security/Oidc/AuthClaims.ascx", "Oidc", Rock.SystemGuid.BlockType.OIDC_CLAIMS );

            // Add/Update BlockType OpenID Connect Clients
            RockMigrationHelper.UpdateBlockType( "OpenID Connect Clients", "Block for displaying and editing available OpenID Connect clients.", "~/Blocks/Security/Oidc/AuthClientList.ascx", "Oidc", Rock.SystemGuid.BlockType.OIDC_CLIENT_LIST );

            // Add/Update BlockType OpenID Connect Client Detail
            RockMigrationHelper.UpdateBlockType( "OpenID Connect Client Detail", "Displays the details of the given OpenID Connect Client.", "~/Blocks/Security/Oidc/AuthClientDetail.ascx", "Oidc", Rock.SystemGuid.BlockType.OIDC_CLIENT_DETAIL );

            // Add Block Authorize to Page: Give Permission, Site: External Website
            RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.OIDC_GIVE_PERMISSION.AsGuid(), null, Rock.SystemGuid.Site.EXTERNAL_SITE.AsGuid(), Rock.SystemGuid.BlockType.OIDC_AUTHORIZE.AsGuid(), "Authorize", "Main", @"", @"", 0, Rock.SystemGuid.Block.OIDC_AUTHORIZE );

            // Add Block Logout to Page: Logout, Site: External Website
            RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.OIDC_LOGOUT.AsGuid(), null, Rock.SystemGuid.Site.EXTERNAL_SITE.AsGuid(), Rock.SystemGuid.BlockType.OIDC_LOGOUT.AsGuid(), "Logout", "Main", @"", @"", 0, Rock.SystemGuid.Block.OIDC_LOGOUT );

            // Add Block OpenID Connect Scope List to Page: OpenID Connect Scopes, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.OIDC_SCOPE_LIST.AsGuid(), null, Rock.SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), Rock.SystemGuid.BlockType.OIDC_SCOPE_LIST.AsGuid(), "OpenID Connect Scope List", "Main", @"", @"", 0, Rock.SystemGuid.Block.OIDC_SCOPE_LIST );

            // Add Block OpenID Connect Scope Detail to Page: OpenID Connect Scope Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.OIDC_SCOPE_DETAIL.AsGuid(), null, Rock.SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), Rock.SystemGuid.BlockType.OIDC_SCOPE_DETAIL.AsGuid(), "OpenID Connect Scope Detail", "Main", @"", @"", 0, Rock.SystemGuid.Block.OIDC_SCOPE_DETAIL );

            // Add Block OpenID Connect Claims 2 to Page: OpenID Connect Scope Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.OIDC_SCOPE_DETAIL.AsGuid(), null, Rock.SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), Rock.SystemGuid.BlockType.OIDC_CLAIMS.AsGuid(), "OpenID Connect Claims", "Main", @"", @"", 2, Rock.SystemGuid.Block.OIDC_CLAIMS );

            // Add Block OpenID Connect Clients to Page: OpenID Connect Client, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.OIDC_CLIENT_LIST.AsGuid(), null, Rock.SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), Rock.SystemGuid.BlockType.OIDC_CLIENT_LIST.AsGuid(), "OpenID Connect Clients", "Main", @"", @"", 0, Rock.SystemGuid.Block.OIDC_CLIENT_LIST );

            // Add Block OpenID Connect Client Detail to Page: OpenID Connect Client Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.OIDC_CLIENT_DETAIL.AsGuid(), null, Rock.SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), Rock.SystemGuid.BlockType.OIDC_CLIENT_DETAIL.AsGuid(), "OpenID Connect Client Detail", "Main", @"", @"", 0, Rock.SystemGuid.Block.OIDC_CLIENT_DETAIL );

            // update block order for pages with new blocks if the page,zone has multiple blocks

            // Update Order for Page: OpenID Connect Scope Detail,  Zone: Main,  Block: OpenID Connect Claims 2
            Sql( $"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '{Rock.SystemGuid.BlockType.OIDC_CLAIMS}'" );

            // Update Order for Page: OpenID Connect Scope Detail,  Zone: Main,  Block: OpenID Connect Scope Detail
            Sql( $"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '{Rock.SystemGuid.BlockType.OIDC_SCOPE_DETAIL}'" );

            // Attribute for BlockType: OpenID Connect Scopes:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( Rock.SystemGuid.BlockType.OIDC_SCOPE_LIST, Rock.SystemGuid.FieldType.PAGE_REFERENCE, "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", Rock.SystemGuid.Attribute.OIDC_SCOPE_LIST_DETAIL_PAGE );

            // Attribute for BlockType: OpenID Connect Clients:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( Rock.SystemGuid.BlockType.OIDC_CLIENT_LIST, Rock.SystemGuid.FieldType.PAGE_REFERENCE, "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", Rock.SystemGuid.Attribute.OIDC_CLIENT_LIST_DETAIL_PAGE );

            // Attribute for BlockType: OpenID Connect Clients:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( Rock.SystemGuid.BlockType.OIDC_CLIENT_LIST, Rock.SystemGuid.FieldType.PAGE_REFERENCE, "OpenID Connect Scopes Page", "ScopePage", "OpenID Connect Scopes Page", @"", 0, @"", Rock.SystemGuid.Attribute.OIDC_CLIENT_LIST_SCOPE_PAGE );

            // Block Attribute Value for OpenID Connect Scope List ( Page: OpenID Connect Scopes, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( Rock.SystemGuid.Block.OIDC_SCOPE_LIST, Rock.SystemGuid.Attribute.OIDC_SCOPE_LIST_DETAIL_PAGE, @Rock.SystemGuid.Page.OIDC_SCOPE_DETAIL );

            // Block Attribute Value for OpenID Connect Clients ( Page: OpenID Connect Clients, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( Rock.SystemGuid.Block.OIDC_CLIENT_LIST, Rock.SystemGuid.Attribute.OIDC_CLIENT_LIST_DETAIL_PAGE, @Rock.SystemGuid.Page.OIDC_CLIENT_DETAIL );

            // Block Attribute Value for OpenID Connect Clients ( Page: OpenID Connect Clients, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( Rock.SystemGuid.Block.OIDC_CLIENT_LIST, Rock.SystemGuid.Attribute.OIDC_CLIENT_LIST_SCOPE_PAGE, @Rock.SystemGuid.Page.OIDC_SCOPE_LIST );
        }
    }
}
