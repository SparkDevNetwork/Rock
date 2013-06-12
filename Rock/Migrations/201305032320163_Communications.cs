//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class Communications : RockMigration_5
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Communication", "FutureSendDateTime", c => c.DateTime());
            AddColumn("dbo.Communication", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.Communication", "ReviewerPersonId", c => c.Int());
            AddColumn("dbo.Communication", "ReviewedDateTime", c => c.DateTime());
            AddColumn("dbo.Communication", "ReviewerNote", c => c.String());
            AddColumn("dbo.Communication", "ChannelEntityTypeId", c => c.Int());
            AddColumn("dbo.Communication", "ChannelDataJson", c => c.String());
            AddColumn("dbo.CommunicationRecipient", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.CommunicationRecipient", "StatusNote", c => c.String());
            AddForeignKey("dbo.Communication", "ReviewerPersonId", "dbo.Person", "Id");
            AddForeignKey("dbo.Communication", "ChannelEntityTypeId", "dbo.EntityType", "Id");
            CreateIndex("dbo.Communication", "ReviewerPersonId");
            CreateIndex("dbo.Communication", "ChannelEntityTypeId");
            DropColumn("dbo.Communication", "IsTemporary");

            UpdateFieldType( "Component Field Type", "", "Rock", "Rock.Field.Types.ComponentFieldType", "A7486B0E-4CA2-4E00-A987-5544C7DABA76" );

            // Email Channel
            AddEntityAttribute( "Rock.Communication.Channel.Email", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "", "", "Order", "", "The order that this service should be used (priority)", 0, "", "20749375-E866-4E62-AE2C-0FAEFBE1BC49" );
            AddEntityAttribute( "Rock.Communication.Channel.Email", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "", "Should Service be used?", 0, "False", "78067A0F-5B68-4CB9-9204-ED61C5F9934F" );
            AddEntityAttribute( "Rock.Communication.Channel.Email", "A7486B0E-4CA2-4E00-A987-5544C7DABA76", "", "", "Transport Container", "", "", 0, "", "C1B0F779-C53B-4735-BEEE-1B2BBC3C3816" );
            AddAttributeValue("20749375-E866-4E62-AE2C-0FAEFBE1BC49", 0, "0", "E503369D-01B4-4B2A-B536-F1E655B70917");
            AddAttributeValue("78067A0F-5B68-4CB9-9204-ED61C5F9934F", 0, "True", "FFAF5D21-EF7E-4DF2-8F61-6D790A41CB67");

            Sql( @"
                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'C1B0F779-C53B-4735-BEEE-1B2BBC3C3816')
                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Communication.Transport.SMTP')

                IF NOT EXISTS(Select * FROM [AttributeValue] WHERE [Guid] = 'CA87AA12-93A8-42EC-B4CD-96E9884B63FC')
                    INSERT INTO [AttributeValue] (
                        [IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
                    VALUES(
                        1,@AttributeId,0,0,@EntityTypeId,'CA87AA12-93A8-42EC-B4CD-96E9884B63FC')
");

            AddEntityAttribute( "Rock.Communication.Channel.Sms", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "", "", "Order", "", "The order that this service should be used (priority)", 0, "", "21EBFA4E-17DD-4A0D-A464-93A71313E069" );
            AddEntityAttribute( "Rock.Communication.Channel.Sms", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "", "Should Service be used?", 0, "False", "7366162A-0E74-4727-AE3A-C4C60FD3E145" );
            AddEntityAttribute( "Rock.Communication.Channel.Email", "A7486B0E-4CA2-4E00-A987-5544C7DABA76", "", "", "Transport Container", "", "", 0, "", "8C25B7FE-12E1-4074-BA8A-45BEF08A6C44" );
            AddAttributeValue("21EBFA4E-17DD-4A0D-A464-93A71313E069", 0, "1", "6CB987A9-CB7A-47CB-A95D-33E7CD94C0B8");
            AddAttributeValue("7366162A-0E74-4727-AE3A-C4C60FD3E145", 0, "True", "B1E5418A-4D37-4FA5-B8A3-CA6681F011EC");

            // SMTP Transport
            AddEntityAttribute("Rock.Communication.Transport.SMTP", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "", "", "Order", "", "The order that this service should be used (priority)", 0, "", "E7A26D9B-4A11-4653-8AC3-D57E2634B4ED");
            AddEntityAttribute( "Rock.Communication.Transport.SMTP", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "", "Should Service be used?", 0, "False", "9ED9A07A-7074-4DD1-8B73-384E52C6DDE3" );
            AddEntityAttribute( "Rock.Communication.Transport.SMTP", "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Server", "", "", 0, "", "6CFFDF99-E93A-49B8-B440-0EF93878A51F" );
            AddEntityAttribute( "Rock.Communication.Transport.SMTP", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "", "", "Port", "", "", 1, "25", "C6B13F15-9D6F-45B2-BDB9-E77D29A32EBF" );
            AddEntityAttribute( "Rock.Communication.Transport.SMTP", "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "UserName", "", "", 2, "", "2CE8D3AC-F851-462C-93D5-DB82F48DDBFD" );
            AddEntityAttribute( "Rock.Communication.Transport.SMTP", "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Password", "", "", 3, "", "D3641DA0-9E50-4C98-A994-978AF308E745" );
            AddEntityAttribute( "Rock.Communication.Transport.SMTP", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Use SSL", "", "", 4, "False", "B3B2308B-6CD2-4853-8220-C80D861F5D3C" );
            AddAttributeValue( "E7A26D9B-4A11-4653-8AC3-D57E2634B4ED", 0, "0", "1CB667D5-C3C0-46A5-96E1-F0A62E24FA78" );
            AddAttributeValue( "9ED9A07A-7074-4DD1-8B73-384E52C6DDE3", 0, "True", "FCF41882-BE77-4817-BF40-5B01B9213602" );

            AddPage( "CADB44F2-2453-4DB5-AB11-DADA5162AB79", "New Communication", "Communications are used to notify people of information through a selected communication channel.", "Default", "2A22D08D-73A8-4AAF-AC7E-220E8B2E7857", "icon-comment" );
            AddPage( "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", "Communication Channels", "The different channels of communication that can be used to communicate information to people", "Default", "6FF35C53-F89F-4601-8543-2E2328C623F8", "icon-comment" );
            AddPage( "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", "Communication Transports", "The method used to facilitate the sending of information for a particular communication channel", "Default", "29CC8A0B-6476-4200-8B93-DC9BA8767D59", "icon-truck" );
            AddPage( "F7105BFE-B28C-41B6-9CE6-F1018D77DD8F", "Communication List", "View communications that have been saved", "Default", "CADB44F2-2453-4DB5-AB11-DADA5162AB79", "icon-comments" );
            AddPage( "F7105BFE-B28C-41B6-9CE6-F1018D77DD8F", "New Communication", "", "Default", "8F3BD682-E2D5-4B7D-A7A8-519AC6B1514A" );
            AddPageRoute( "2A22D08D-73A8-4AAF-AC7E-220E8B2E7857", "Communication/{CommunicationId}" );

            AddBlockType( "Core - Communication", "", "~/Blocks/Core/Communication.ascx", "D9834641-7F39-4CFA-8CB2-E64068127565" );
            AddBlockType( "Core - Communication List", "", "~/Blocks/Core/CommunicationList.ascx", "56ABBD0F-8F62-4094-88B3-161E71F21419" );
            AddBlock( "2A22D08D-73A8-4AAF-AC7E-220E8B2E7857", "D9834641-7F39-4CFA-8CB2-E64068127565", "Communication", "", "Content", 0, "BD9B2F32-AB18-4761-80C9-FDA4DBEEA9EC" );
            AddBlock( "6FF35C53-F89F-4601-8543-2E2328C623F8", "21F5F466-59BC-40B2-8D73-7314D936C3CB", "Channels", "", "Content", 0, "190A276D-81B5-4EF6-B1EB-1ABC68B4D770" );
            AddBlock( "29CC8A0B-6476-4200-8B93-DC9BA8767D59", "21F5F466-59BC-40B2-8D73-7314D936C3CB", "Transports", "", "Content", 0, "FA8EA948-150A-4668-BDEF-E3669FAC695E" );
            AddBlock( "CADB44F2-2453-4DB5-AB11-DADA5162AB79", "56ABBD0F-8F62-4094-88B3-161E71F21419", "Communication List", "", "Content", 0, "4578A82D-0C8A-4316-9047-7FEF2C13190B" );
            AddBlock( "8F3BD682-E2D5-4B7D-A7A8-519AC6B1514A", "B97FB779-5D3E-4663-B3B5-3C2C227AE14A", "Redirect", "", "Content", 0, "A4F04794-9C7E-4B98-BC74-0D4470B8C84A" );
            AddBlockTypeAttribute( "D9834641-7F39-4CFA-8CB2-E64068127565", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Display Count", "DisplayCount", "", "The initial number of recipients to display prior to expanding list", 0, "", "39B75C2A-879F-4C5A-A7D7-B18710B4681C" );
            AddBlockTypeAttribute( "D9834641-7F39-4CFA-8CB2-E64068127565", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Maximum Recipients", "MaximumRecipients", "", "The maximum number of recipients allowed before communication will need to be approved", 0, "", "EA534449-2BD0-4AA3-836C-627FA22576E4" );
            AddBlockTypeAttribute( "D9834641-7F39-4CFA-8CB2-E64068127565", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Send When Approved", "SendWhenApproved", "", "Should communication be sent once it's approved (vs. just being queued for scheduled job to send)?", 0, "False", "04C3D08F-CAB5-49DC-91B4-3F81CCF40211" );
            AddBlockTypeAttribute( "56ABBD0F-8F62-4094-88B3-161E71F21419", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "", "", 0, "", "AF388013-8DA9-4CD9-B707-082A401D464E" );
            AddBlockTypeAttribute( "B97FB779-5D3E-4663-B3B5-3C2C227AE14A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Url", "Url", "", "The path to redirect to", 0, "", "964D33F4-27D0-4715-86BE-D30CEB895044" );


            // Attrib Value for Channels:Component Container
            AddBlockAttributeValue( "190A276D-81B5-4EF6-B1EB-1ABC68B4D770", "259AF14D-0214-4BE4-A7BF-40423EA07C99", "Rock.Communication.ChannelContainer, Rock" );
            // Attrib Value for Transports:Component Container
            AddBlockAttributeValue( "FA8EA948-150A-4668-BDEF-E3669FAC695E", "259AF14D-0214-4BE4-A7BF-40423EA07C99", "Rock.Communication.TransportContainer, Rock" );
            // Attrib Value for Communication:Display Count
            AddBlockAttributeValue( "BD9B2F32-AB18-4761-80C9-FDA4DBEEA9EC", "39B75C2A-879F-4C5A-A7D7-B18710B4681C", "20" );
            // Attrib Value for Communication:Maximum Recipients
            AddBlockAttributeValue( "BD9B2F32-AB18-4761-80C9-FDA4DBEEA9EC", "EA534449-2BD0-4AA3-836C-627FA22576E4", "300" );
            // Attrib Value for Communication:Send When Approved
            AddBlockAttributeValue( "BD9B2F32-AB18-4761-80C9-FDA4DBEEA9EC", "04C3D08F-CAB5-49DC-91B4-3F81CCF40211", "True" );
            // Attrib Value for Communication List:Detail Page Guid
            AddBlockAttributeValue( "4578A82D-0C8A-4316-9047-7FEF2C13190B", "AF388013-8DA9-4CD9-B707-082A401D464E", "2a22d08d-73a8-4aaf-ac7e-220e8b2e7857" );
            // Attrib Value for Redirect:Url
            AddBlockAttributeValue( "A4F04794-9C7E-4B98-BC74-0D4470B8C84A", "964D33F4-27D0-4715-86BE-D30CEB895044", "~/Communication/0" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "E7A26D9B-4A11-4653-8AC3-D57E2634B4ED" );
            DeleteAttribute( "9ED9A07A-7074-4DD1-8B73-384E52C6DDE3" );
            DeleteAttribute( "6CFFDF99-E93A-49B8-B440-0EF93878A51F" );
            DeleteAttribute( "C6B13F15-9D6F-45B2-BDB9-E77D29A32EBF" );
            DeleteAttribute( "2CE8D3AC-F851-462C-93D5-DB82F48DDBFD" );
            DeleteAttribute( "D3641DA0-9E50-4C98-A994-978AF308E745" );
            DeleteAttribute( "B3B2308B-6CD2-4853-8220-C80D861F5D3C" );
            DeleteAttribute( "78067A0F-5B68-4CB9-9204-ED61C5F9934F" );
            DeleteAttribute( "20749375-E866-4E62-AE2C-0FAEFBE1BC49" );
            DeleteAttribute( "C1B0F779-C53B-4735-BEEE-1B2BBC3C3816" );
            DeleteAttribute( "7366162A-0E74-4727-AE3A-C4C60FD3E145" );
            DeleteAttribute( "21EBFA4E-17DD-4A0D-A464-93A71313E069" );
            DeleteAttribute( "8C25B7FE-12E1-4074-BA8A-45BEF08A6C44" );

            DeleteAttribute( "39B75C2A-879F-4C5A-A7D7-B18710B4681C" ); // Display Count
            DeleteAttribute( "EA534449-2BD0-4AA3-836C-627FA22576E4" ); // Maximum Recipients
            DeleteAttribute( "04C3D08F-CAB5-49DC-91B4-3F81CCF40211" ); // Send When Approved
            DeleteAttribute( "A385456F-25E4-4356-B45A-CCECC77F2056" ); // Default Accounts to display
            DeleteAttribute( "AF388013-8DA9-4CD9-B707-082A401D464E" ); // Detail Page Guid
            DeleteAttribute( "964D33F4-27D0-4715-86BE-D30CEB895044" ); // Url
            DeleteBlock( "BD9B2F32-AB18-4761-80C9-FDA4DBEEA9EC" ); // Communication
            DeleteBlock( "190A276D-81B5-4EF6-B1EB-1ABC68B4D770" ); // Channels
            DeleteBlock( "FA8EA948-150A-4668-BDEF-E3669FAC695E" ); // Transports
            DeleteBlock( "4578A82D-0C8A-4316-9047-7FEF2C13190B" ); // Communication List
            DeleteBlock( "A4F04794-9C7E-4B98-BC74-0D4470B8C84A" ); // Redirect
            DeleteBlockType( "D9834641-7F39-4CFA-8CB2-E64068127565" ); // Core - Communication
            DeleteBlockType( "56ABBD0F-8F62-4094-88B3-161E71F21419" ); // Core - Communication List
            DeletePage( "2A22D08D-73A8-4AAF-AC7E-220E8B2E7857" ); // New Communication
            DeletePage( "6FF35C53-F89F-4601-8543-2E2328C623F8" ); // Communication Channels
            DeletePage( "29CC8A0B-6476-4200-8B93-DC9BA8767D59" ); // Communication Transports
            DeletePage( "CADB44F2-2453-4DB5-AB11-DADA5162AB79" ); // Communication List
            DeletePage( "8F3BD682-E2D5-4B7D-A7A8-519AC6B1514A" ); // New Communication

            AddColumn( "dbo.Communication", "IsTemporary", c => c.Boolean( nullable: false ) );
            DropIndex("dbo.Communication", new[] { "ChannelEntityTypeId" });
            DropIndex("dbo.Communication", new[] { "ReviewerPersonId" });
            DropForeignKey("dbo.Communication", "ChannelEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.Communication", "ReviewerPersonId", "dbo.Person");
            DropColumn("dbo.CommunicationRecipient", "StatusNote");
            DropColumn("dbo.CommunicationRecipient", "Status");
            DropColumn("dbo.Communication", "ChannelDataJson");
            DropColumn("dbo.Communication", "ChannelEntityTypeId");
            DropColumn("dbo.Communication", "ReviewerNote");
            DropColumn("dbo.Communication", "ReviewedDateTime");
            DropColumn("dbo.Communication", "ReviewerPersonId");
            DropColumn("dbo.Communication", "Status");
            DropColumn("dbo.Communication", "FutureSendDateTime");
        }
    }
}
