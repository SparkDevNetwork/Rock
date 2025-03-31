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
    ///
    /// </summary>
    public partial class AddHistoryLoginModel : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.HistoryLogin",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    UserName = c.String( maxLength: 255 ),
                    UserLoginId = c.Int(),
                    PersonAliasId = c.Int(),
                    LoginAttemptDateTime = c.DateTime( nullable: false ),
                    ClientIpAddress = c.String( maxLength: 45 ),
                    AuthClientClientId = c.String( maxLength: 50 ),
                    ExternalSource = c.String( maxLength: 200 ),
                    SourceSiteId = c.Int(),
                    RelatedDataJson = c.String(),
                    DestinationUrl = c.String( maxLength: 2048 ),
                    WasLoginSuccessful = c.Boolean( nullable: false ),
                    LoginFailureReason = c.Int(),
                    LoginFailureMessage = c.String(),
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
                .ForeignKey( "dbo.PersonAlias", t => t.PersonAliasId )

                // Don't add these default-scaffolded foreign keys; we'll add ON DELETE SET NULL foreign keys instead.
                //.ForeignKey("dbo.Site", t => t.SourceSiteId)
                //.ForeignKey("dbo.UserLogin", t => t.UserLoginId)

                // Don't add these default-scaffolded indexes; we'll add more targeted indexes instead.
                //.Index( t => t.UserLoginId )
                //.Index( t => t.PersonAliasId )
                //.Index( t => t.SourceSiteId )

                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            Sql( @"
ALTER TABLE [dbo].[HistoryLogin]
ADD CONSTRAINT [FK_dbo.HistoryLogin_dboSite_SourceSiteId] FOREIGN KEY ([SourceSiteId])
REFERENCES [dbo].[Site] ([Id])
ON DELETE SET NULL;

ALTER TABLE [dbo].[HistoryLogin]
ADD CONSTRAINT [FK_dbo.HistoryLogin_dboUserLogin_UserLoginId] FOREIGN KEY ([UserLoginId])
REFERENCES [dbo].[UserLogin] ([Id])
ON DELETE SET NULL;

CREATE NONCLUSTERED INDEX [IX_LoginAttemptDateTime] ON [dbo].[HistoryLogin]
(
    [LoginAttemptDateTime] ASC,
    [SourceSiteId] ASC
)
INCLUDE([PersonAliasId],[AuthClientClientId],[LoginFailureReason]);

CREATE NONCLUSTERED INDEX [IX_PersonAliasId_LoginAttemptDateTime] ON [dbo].[HistoryLogin]
(
    [PersonAliasId] ASC,
    [LoginAttemptDateTime] ASC
)
INCLUDE([SourceSiteId],[AuthClientClientId],[LoginFailureReason]);

CREATE NONCLUSTERED INDEX [IX_SourceSiteId_LoginAttemptDateTime] ON [dbo].[HistoryLogin]
(
    [SourceSiteId] ASC,
    [LoginAttemptDateTime] ASC
)
INCLUDE([PersonAliasId],[AuthClientClientId],[LoginFailureReason]);" );

            // This was already applied in a previous migration, but EF saw it as a change.
            // https://github.com/SparkDevNetwork/Rock/blob/4423274bac1cb56471d00a212a88bb8f59f9e358/Rock.Migrations/Migrations/Version%2017.0/Version%2017.0/202503131624313_UpdateCommunicationRecipientModel.cs#L34
            //AlterColumn( "dbo.CommunicationRecipient", "UnsubscribeLevel", c => c.Int() );

            // Fix for issue #6241 to wrap the footer fragment in a conditional statement to prevent it from rendering when the RenderMedium is not 'Html'.
            string oldHtmlFragmentValue = "{ \"HtmlFragment\": \"<table style=''width: 100%; margin-left: 5mm; margin-right: 5mm;''>\\n    <tr>\\n        <td style=\\\"text-align:left; font-size:8px; opacity:.5\\\">\\n            {{ Salutation }}\\n        </td>\\n        <td style=\\\"text-align:right; font-size:8px; opacity:.5\\\">\\n            Page <span class=''pageNumber''></span> of <span class=''totalPages''></span>\\n        </td>\\n    </tr>\\n</table>\\n\\n\\n\" }";
            string newHtmlFragmentValue = "{ \"HtmlFragment\": \"{% if RenderMedium != ''Html'' %}<table style=''width: 100%; margin-left: 5mm; margin-right: 5mm;''>\\n    <tr>\\n        <td style=\\\"text-align:left; font-size:8px; opacity:.5\\\">\\n            {{ Salutation }}\\n        </td>\\n        <td style=\\\"text-align:right; font-size:8px; opacity:.5\\\">\\n            Page <span class=''pageNumber''></span> of <span class=''totalPages''></span>\\n        </td>\\n    </tr>\\n</table>{% endif %}\\n\\n\\n\" }";

            Sql( $@"
            UPDATE [dbo].[FinancialStatementTemplate] 
            SET [FooterSettingsJson] = REPLACE( [FooterSettingsJson], '{oldHtmlFragmentValue}', '{newHtmlFragmentValue}')
            WHERE [Guid] = '4B93657A-DD5F-4D8A-A13F-1B4E9ADBDAD0'" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // This was already applied in a previous migration, but EF saw it as a change.
            // https://github.com/SparkDevNetwork/Rock/blob/4423274bac1cb56471d00a212a88bb8f59f9e358/Rock.Migrations/Migrations/Version%2017.0/Version%2017.0/202503131624313_UpdateCommunicationRecipientModel.cs#L42
            //AlterColumn( "dbo.CommunicationRecipient", "UnsubscribeLevel", c => c.Int( nullable: false ) );

            DropForeignKey( "dbo.HistoryLogin", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.HistoryLogin", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.HistoryLogin", "CreatedByPersonAliasId", "dbo.PersonAlias" );

            DropIndex( "dbo.HistoryLogin", new[] { "Guid" } );
            DropIndex( "dbo.HistoryLogin", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.HistoryLogin", new[] { "CreatedByPersonAliasId" } );

            Sql( @"
ALTER TABLE [dbo].[HistoryLogin] DROP CONSTRAINT [FK_dbo.HistoryLogin_dboUserLogin_UserLoginId];
ALTER TABLE [dbo].[HistoryLogin] DROP CONSTRAINT [FK_dbo.HistoryLogin_dboSite_SourceSiteId];

DROP INDEX [IX_SourceSiteId_LoginAttemptDateTime] ON [dbo].[HistoryLogin];
DROP INDEX [IX_PersonAliasId_LoginAttemptDateTime] ON [dbo].[HistoryLogin];
DROP INDEX [IX_LoginAttemptDateTime] ON [dbo].[HistoryLogin];" );

            DropTable( "dbo.HistoryLogin" );
        }
    }
}
