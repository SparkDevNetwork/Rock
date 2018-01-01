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
    public partial class UrlMap : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.SiteUrlMap",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SiteId = c.Int(nullable: false),
                        Token = c.String(nullable: false, maxLength: 50),
                        Url = c.String(nullable: false, maxLength: 200),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.Site", t => t.SiteId, cascadeDelete: true)
                .Index(t => t.SiteId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.SiteDomain", "Order", c => c.Int(nullable: false));

            // Create a global attribute for default short link site and default to site with lowest default page id (public site)
            RockMigrationHelper.AddGlobalAttribute( SystemGuid.FieldType.SITE, "", "", "Default Short Link Site", "The default site to use when adding new short links.", 0, "", "DD0E0757-2A01-47BB-A74A-F6E69B0399C8", "DefaultShortLinkSite" );
            Sql( @"
    DECLARE @SiteId int = ( SELECT TOP 1 [Id] FROM [Site] ORDER BY [DefaultPageId] )
    UPDATE [Attribute] SET [DefaultValue] = @SiteId WHERE [Guid] = 'DD0E0757-2A01-47BB-A74A-F6E69B0399C8' 
" );
            // Add new interaction medium
            RockMigrationHelper.UpdateEntityType( "Rock.Model.SiteUrlMap", "C225FE29-F4FA-4A60-996C-56B0F081042E", true, true );
            RockMigrationHelper.UpdateDefinedValue( "9BF5777A-961F-49A8-A834-45E5C2077967", "Url Shortner", "Used for tracking views using a short link.", "371066D5-C5F9-4783-88C8-D9AC8DC67468" );

            RockMigrationHelper.AddPage( "E7BD353C-91A6-4C15-A6C8-F44D0B16D16E", "7CFA101B-2D20-4523-9EC5-3F30502797A5", "Short Link", "", "A9188D7A-80D9-4865-9C77-9F90E992B65C", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "B4A24AB7-9369-4055-883F-4F4892C39AE3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Short Links", "", "8C0114FF-31CF-443E-9278-3F9E6087140C", "fa fa-link" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "8C0114FF-31CF-443E-9278-3F9E6087140C", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Link", "", "47D5293B-A041-43A4-915A-FB1D156F265E", "fa fa-link" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "A9188D7A-80D9-4865-9C77-9F90E992B65C", "ShortLink/{Page}", "38E68E37-DC86-487A-93A2-8A9F3EBC9768" );// for Page:Short Link

            Sql( @"
    UPDATE [Page] SET [IncludeAdminFooter] = 0 WHERE [Guid] = 'A9188D7A-80D9-4865-9C77-9F90E992B65C'
" );
            RockMigrationHelper.UpdateBlockType( "Shortened Links", "Displays a dialog for adding a short link to the current page.", "~/Blocks/Administration/ShortLink.ascx", "Administration", "86FB6B0E-E426-4581-96C0-A7654D6A5C7D" );
            RockMigrationHelper.UpdateBlockType( "Short Link List", "Lists all the short Links .", "~/Blocks/Cms/ShortLinkList.ascx", "CMS", "D6D87CCC-DB6D-4138-A4B5-30F0707A5300" );
            RockMigrationHelper.UpdateBlockType( "Short Link Detail", "Displays the details for a specific short link.", "~/Blocks/Cms/ShortLinkDetail.ascx", "CMS", "794C564C-6395-4303-812F-3BFBD1057443" );
            RockMigrationHelper.UpdateBlockType( "Short Link Click List", "Lists cliks for a particular short link.", "~/Blocks/Cms/ShortLinkClickList.ascx", "CMS", "1D7B8095-9E5B-4A9A-A519-69E1746140DD" );

            // Add Block to Page: Short Link, Site: Rock RMS
            RockMigrationHelper.AddBlock( "A9188D7A-80D9-4865-9C77-9F90E992B65C", "", "86FB6B0E-E426-4581-96C0-A7654D6A5C7D", "Shortened Links", "Main", @"", @"", 0, "B21DFC9B-FB22-4FA1-B7F9-82CE7B4506F1" );
            // Add Block to Page: Short Links, Site: Rock RMS
            RockMigrationHelper.AddBlock( "8C0114FF-31CF-443E-9278-3F9E6087140C", "", "D6D87CCC-DB6D-4138-A4B5-30F0707A5300", "Short Link List", "Main", @"", @"", 0, "A0ECC10B-BB01-4D5D-BAC2-76192C231A20" );
            // Add Block to Page: Link, Site: Rock RMS
            RockMigrationHelper.AddBlock( "47D5293B-A041-43A4-915A-FB1D156F265E", "", "794C564C-6395-4303-812F-3BFBD1057443", "Short Link Detail", "Main", @"", @"", 0, "2550B79E-8BBE-4C02-B395-EC1D182EAEA1" );
            // Add Block to Page: Link, Site: Rock RMS
            RockMigrationHelper.AddBlock( "47D5293B-A041-43A4-915A-FB1D156F265E", "", "1D7B8095-9E5B-4A9A-A519-69E1746140DD", "Short Link Click List", "Main", @"", @"", 1, "7B5CFF37-02CE-4E57-9FEC-C5092D24B6DC" );

            // Attrib for BlockType: Shortened Links:Minimum Token Length
            RockMigrationHelper.UpdateBlockTypeAttribute( "86FB6B0E-E426-4581-96C0-A7654D6A5C7D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Minimum Token Length", "MinimumTokenLength", "", "The minimum number of characters for the token.", 0, @"7", "B13D4F65-E00A-4C80-B9E0-A18575985F9B" );
            // Attrib for BlockType: Short Link List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "D6D87CCC-DB6D-4138-A4B5-30F0707A5300", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "C857D4CC-3BA0-45FC-B73B-C00D89C8865B" );
            // Attrib for BlockType: Short Link Detail:Minimum Token Length
            RockMigrationHelper.UpdateBlockTypeAttribute( "794C564C-6395-4303-812F-3BFBD1057443", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Minimum Token Length", "MinimumTokenLength", "", "The minimum number of characters for the token.", 0, @"7", "A8431AE5-78E9-46F0-A355-FDF8204B984E" );

            // Attrib Value for Block:Short Link List, Attribute:Detail Page Page: Short Links, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A0ECC10B-BB01-4D5D-BAC2-76192C231A20", "C857D4CC-3BA0-45FC-B73B-C00D89C8865B", @"47d5293b-a041-43a4-915a-fb1d156f265e" );

            // DT: New Failed Payment Email
            Sql( @"
IF EXISTS ( SELECT [Id] FROM [SystemEmail] WHERE [Guid] = '449232B5-9C6B-480E-A881-E317D0BC307E' AND [Body] = '
{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>
    {{ Transaction.AuthorizedPersonAlias.Person.NickName }}, 
</p>
<p>
    We just wanted to make you aware that your gift to {{ ''Global'' | Attribute:''OrganizationName'' }} that was scheduled for {{ Transaction.TransactionDateTime | Date:''M/d/yyyy'' }} in the amount of 
    {{ Transaction.ScheduledTransaction.TotalAmount | FormatAsCurrency }} did not process successfully. If you''d like, you can update your giving profile at 
    <a href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Give"">{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Give</a>.
</p>

<p>
    Below are the details of your transaction that we were unable to process.
</p>

<p>
<strong>Txn Code:</strong> {{ Transaction.TransactionCode }}<br/>
<strong>Status:</strong> {{ Transaction.Status }}<br/>
<strong>Status Message:</strong> {{ Transaction.StatusMessage }}
</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}
' )
BEGIN
	UPDATE [SystemEmail] SET
		[Subject] = 'Unsuccessful Payment to {{ ''Global'' | Attribute:''OrganizationName'' }}',
		[Body] = '{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>
    {{ Transaction.AuthorizedPersonAlias.Person.NickName }}, 
</p>
<p>
{% if Transaction.ScheduledTransaction %}
    We just wanted to make you aware that your gift to {{ ''Global'' | Attribute:''OrganizationName'' }} that was scheduled for {{ Transaction.TransactionDateTime | Date:''M/d/yyyy'' }} in the amount of 
    {{ Transaction.ScheduledTransaction.TotalAmount | FormatAsCurrency }} did not process successfully. If you''d like, you can update your giving profile at 
    <a href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Give"">{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Give</a>.
{% else %}
    {% assign amount = Transaction.TotalAmount %}
    {% if amount < 0 %}{% assign amount = 0 | Minus:amount %}{% endif %}
    We just wanted to make you aware that your {{ Transaction.TransactionTypeValue.Value | Downcase }} payment on {{ Transaction.TransactionDateTime | Date:''M/d/yyyy'' }} in the amount of 
    {{ amount | FormatAsCurrency }} did not process successfully. 
    {% if Transaction.TransactionTypeValue.Value == ''Contribution'' %}
        If you''d like, you can re-submit your contribution at  
        <a href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Give"">{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Give</a>.
    {% endif %}
{% endif %}
</p>
<p>
    Below are the details of your transaction that we were unable to process.
</p>

<p>
<strong>Txn Code:</strong> {{ Transaction.TransactionCode }}<br/>
<strong>Status:</strong> {{ Transaction.Status }}<br/>
<strong>Status Message:</strong> {{ Transaction.StatusMessage }}
</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}
'
	WHERE [Guid] = '449232B5-9C6B-480E-A881-E317D0BC307E'
END
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Short Link Detail:Minimum Token Length
            RockMigrationHelper.DeleteAttribute( "A8431AE5-78E9-46F0-A355-FDF8204B984E" );
            // Attrib for BlockType: Short Link List:Detail Page
            RockMigrationHelper.DeleteAttribute( "C857D4CC-3BA0-45FC-B73B-C00D89C8865B" );
            // Attrib for BlockType: Shortened Links:Minimum Token Length
            RockMigrationHelper.DeleteAttribute( "B13D4F65-E00A-4C80-B9E0-A18575985F9B" );

            // Remove Block: Short Link Click List, from Page: Link, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "7B5CFF37-02CE-4E57-9FEC-C5092D24B6DC" );
            // Remove Block: Short Link Detail, from Page: Link, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "2550B79E-8BBE-4C02-B395-EC1D182EAEA1" );
            // Remove Block: Short Link List, from Page: Short Links, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "A0ECC10B-BB01-4D5D-BAC2-76192C231A20" );
            // Remove Block: Shortened Links, from Page: Short Link, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B21DFC9B-FB22-4FA1-B7F9-82CE7B4506F1" );

            RockMigrationHelper.DeleteBlockType( "1D7B8095-9E5B-4A9A-A519-69E1746140DD" ); // Short Link Click List
            RockMigrationHelper.DeleteBlockType( "794C564C-6395-4303-812F-3BFBD1057443" ); // Short Link Detail
            RockMigrationHelper.DeleteBlockType( "D6D87CCC-DB6D-4138-A4B5-30F0707A5300" ); // Short Link List
            RockMigrationHelper.DeleteBlockType( "86FB6B0E-E426-4581-96C0-A7654D6A5C7D" ); // Shortened Links

            RockMigrationHelper.DeletePage( "47D5293B-A041-43A4-915A-FB1D156F265E" ); //  Page: Link, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "8C0114FF-31CF-443E-9278-3F9E6087140C" ); //  Page: Short Links, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "A9188D7A-80D9-4865-9C77-9F90E992B65C" ); //  Page: Short Link, Layout: Dialog, Site: Rock RMS

            DropForeignKey("dbo.SiteUrlMap", "SiteId", "dbo.Site");
            DropForeignKey("dbo.SiteUrlMap", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.SiteUrlMap", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.SiteUrlMap", new[] { "Guid" });
            DropIndex("dbo.SiteUrlMap", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.SiteUrlMap", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.SiteUrlMap", new[] { "SiteId" });
            DropColumn("dbo.SiteDomain", "Order");
            DropTable("dbo.SiteUrlMap");
        }
    }
}
