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
    /// <summary>
    /// Adds the PersonSession table, its indexes, and the InteractionSession.PersonSessionId
    /// column that links interaction activity back to the authenticated session.
    /// </summary>
    public partial class AddPersonSessionModel : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.PersonSession",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    PersonAliasId = c.Int( nullable: false ),
                    UserLoginId = c.Int(),
                    IsActive = c.Boolean( nullable: false ),
                    IssuedDateTime = c.DateTime( nullable: false ),
                    InactiveDateTime = c.DateTime(),
                    ExpiresDateTime = c.DateTime(),
                    LastActivityDateTime = c.DateTime( nullable: false ),
                    LastStepUpAuthenticationDateTime = c.DateTime(),
                    LastMultiFactorAuthenticationDateTime = c.DateTime(),
                    IsPersistent = c.Boolean( nullable: false ),
                    InteractionDeviceTypeId = c.Int(),
                    AuthenticationComponentId = c.Int(),
                    CreationSource = c.Int( nullable: false ),
                    AdditionalSettingsJson = c.String(),
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
                .ForeignKey( "dbo.EntityType", t => t.AuthenticationComponentId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.InteractionDeviceType", t => t.InteractionDeviceTypeId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.PersonAliasId )

                // Don't add the default-scaffolded UserLoginId foreign key; we add an
                // ON DELETE SET NULL foreign key in raw SQL below so deleting a UserLogin
                // (also how an API key is revoked) leaves the historical PersonSession
                // row in place rather than cascading the deletion through all of the
                // user's sessions.
                //.ForeignKey( "dbo.UserLogin", t => t.UserLoginId )

                .Index( t => t.PersonAliasId )
                .Index( t => t.UserLoginId )
                .Index( t => t.InteractionDeviceTypeId )
                .Index( t => t.AuthenticationComponentId )
                .Index( t => t.IsActive )
                .Index( t => t.LastActivityDateTime )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            // ON DELETE SET NULL on UserLoginId needs raw SQL: EF's AddForeignKey
            // exposes cascadeDelete: true/false but no SET NULL option.
            //
            // The two filtered unique indexes below back the race-free
            // upsert-with-unique-key pattern that
            // PersonSessionService.FindOrCreateApiKeySession and
            // FindOrCreateLegacyUpgradeSession rely on (mirroring the
            // Guid-unique pattern InteractionService:583 uses for
            // InteractionSession adoption):
            //
            //   * ApiKey  - one active row per UserLogin
            //               (CreationSource = 4, IsActive = 1).
            //   * Legacy  - one row per (UserLogin, IssuedDateTime) tuple
            //               (CreationSource = 5).
            //
            // Component, Impersonation, and UserToken sessions deliberately
            // have NO uniqueness constraint: multiple concurrent sessions per
            // UserLogin are legitimate for those sources (different devices,
            // different browser sessions).
            Sql( """
                ALTER TABLE [dbo].[PersonSession]
                ADD CONSTRAINT [FK_dbo.PersonSession_dbo.UserLogin_UserLoginId] FOREIGN KEY ([UserLoginId])
                REFERENCES [dbo].[UserLogin] ([Id])
                ON DELETE SET NULL;

                CREATE UNIQUE NONCLUSTERED INDEX [IX_ApiKey_UserLoginId] ON [dbo].[PersonSession]
                (
                    [UserLoginId] ASC
                ) WHERE [UserLoginId] IS NOT NULL AND [CreationSource] = 4 AND [IsActive] = 1;

                CREATE UNIQUE NONCLUSTERED INDEX [IX_Legacy_UserLoginId_IssuedDateTime] ON [dbo].[PersonSession]
                (
                    [UserLoginId] ASC,
                    [IssuedDateTime] ASC
                ) WHERE [UserLoginId] IS NOT NULL AND [CreationSource] = 5
                """ );

            AddColumn( "dbo.InteractionSession", "PersonSessionId", c => c.Int() );
            CreateIndex( "dbo.InteractionSession", "PersonSessionId" );
            AddForeignKey( "dbo.InteractionSession", "PersonSessionId", "dbo.PersonSession", "Id" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.InteractionSession", "PersonSessionId", "dbo.PersonSession" );
            DropIndex( "dbo.InteractionSession", new[] { "PersonSessionId" } );
            DropColumn( "dbo.InteractionSession", "PersonSessionId" );

            // The two filtered unique indexes and the UserLoginId foreign key
            // were created in raw SQL during Up(), so they must be dropped in
            // raw SQL too.
            Sql( """
                DROP INDEX [IX_Legacy_UserLoginId_IssuedDateTime] ON [dbo].[PersonSession];
                DROP INDEX [IX_ApiKey_UserLoginId] ON [dbo].[PersonSession];
                ALTER TABLE [dbo].[PersonSession] DROP CONSTRAINT [FK_dbo.PersonSession_dbo.UserLogin_UserLoginId];
                """ );

            DropForeignKey( "dbo.PersonSession", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersonSession", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersonSession", "InteractionDeviceTypeId", "dbo.InteractionDeviceType" );
            DropForeignKey( "dbo.PersonSession", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersonSession", "AuthenticationComponentId", "dbo.EntityType" );

            DropIndex( "dbo.PersonSession", new[] { "Guid" } );
            DropIndex( "dbo.PersonSession", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.PersonSession", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.PersonSession", new[] { "LastActivityDateTime" } );
            DropIndex( "dbo.PersonSession", new[] { "IsActive" } );
            DropIndex( "dbo.PersonSession", new[] { "AuthenticationComponentId" } );
            DropIndex( "dbo.PersonSession", new[] { "InteractionDeviceTypeId" } );
            DropIndex( "dbo.PersonSession", new[] { "UserLoginId" } );
            DropIndex( "dbo.PersonSession", new[] { "PersonAliasId" } );

            DropTable( "dbo.PersonSession" );
        }
    }
}
