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
    public partial class AddPhotoRequestBlocks : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            #region Photo Request Blocks

            // Add new PhotoOptOut block type
            RockMigrationHelper.UpdateBlockType( "Photo Opt-Out", "Allows a person to opt-out of future photo requests.", "~/Blocks/Crm/PhotoRequest/PhotoOptOut.ascx", "CRM > PhotoRequest", "14293AEB-B0F5-434B-844A-66592AE3A416" );

            // Add new VerifyPhoto block type 160DABF9-3549-447C-9E76-6CFCCCA481C0
            RockMigrationHelper.UpdateBlockType( "Verify Photo", "Allows uploaded photos to be verified.", "~/Blocks/Crm/PhotoRequest/VerifyPhoto.ascx", "CRM > PhotoRequest", "160DABF9-3549-447C-9E76-6CFCCCA481C0" );
            // Attrib for BlockType: VerifyPhoto:Photo Size
            RockMigrationHelper.AddBlockTypeAttribute( "160DABF9-3549-447C-9E76-6CFCCCA481C0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Photo Size", "PhotoSize", "", "The size of the preview photo. Default is 65.", 0, @"True", "722F16A0-1F04-4D52-A850-A39055629617" );

            // Replace the GroupMemberList block on the Verify Photo's page with the new, special VerifyPhoto block
            // - Remove Block: Group Member List, from Page: Verify Photos, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "995E30ED-CB90-4A3F-B626-2C565BDB3FDE" );
            // Add Block to Page: Verify Photos, Site: Rock RMS
            RockMigrationHelper.AddBlock( "07E4BA19-614A-42D0-9D75-DFB31374844D", "", "160DABF9-3549-447C-9E76-6CFCCCA481C0", "Verify Photo Uploads", "Main", "", "", 0, "37BBE63E-5CB5-4F4D-8657-BBB13CA52919" );

            // new Photo Request Opt Out page
            RockMigrationHelper.AddPage( "EBAA5140-4B8F-44B8-B1E8-C73B654E4B22", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Photo Opt-Out", "Page where people can opt-out of future photo requests.", "04141667-1A08-4E15-8BB7-E3E312233E11", "" );
            // Add Block to Page: PhotoOptOut, Site: Rock RMS
            RockMigrationHelper.AddBlock( "04141667-1A08-4E15-8BB7-E3E312233E11", "", "14293AEB-B0F5-434B-844A-66592AE3A416", "Photo Opt-Out", "Main", "", "", 0, "44C793B7-8E2B-4D70-BFA1-5FE21FE4390C" );

            // Add routes to new pages
            RockMigrationHelper.AddPageRoute( "8559A9F1-C6A4-4945-B393-74F6706A8FA2", "PhotoRequest/Upload/{Person}" );
            RockMigrationHelper.AddPageRoute( "04141667-1A08-4E15-8BB7-E3E312233E11", "PhotoRequest/OptOut/{Person}" );

            // Add new Photo Request Template communications template which uses the two new routes
            Sql( @"
            -- Add new Photo Request Template communications template
            DECLARE @EmailChannelId int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '5A653EBE-6803-44B4-85D2-FB7B8146D55D')

            DELETE [CommunicationTemplate] WHERE [Guid] = 'B9A0489C-A823-4C5C-A9F9-14A206EC3B88'

            INSERT [dbo].[CommunicationTemplate]
                ([Name]
                ,[Description]
                ,[Subject]
                ,[ChannelEntityTypeId]
                ,[ChannelDataJson]
                ,[CreatedDateTime]
                ,[ModifiedDateTime]
                ,[Guid] )
            VALUES
                (N'Photo Request Template'
                ,N'Includes the standard text and links for requesting a photo from a person.'
                ,N'Photo Request from {{ GlobalAttribute.OrganizationName }}'
                ,@EmailChannelId
                ,N'{ ""HtmlMessage"": ""{{ GlobalAttribute.EmailHeader }}\n<p>{{ Person.NickName }},</p>\n\n<p>We believe ministry is all about people and we find our ministry is much more personal when we have a recent photo of you in our membership system. Please take a minute to send us your photo using the button below - we&#39;d really appreciate it.</p>\n\n<p><a href=\""{{ GlobalAttribute.PublicApplicationRoot }}PhotoRequest/Upload/{{ Person.UrlEncodedKey }}\"">Upload Photo </a></p>\n\n<p>Your picture will remain confidential and will only be visible to staff and volunteers in a leadership position within {{ GlobalAttribute.OrganizationName }}.</p>\n\n<p><small>If you prefer never to receive these kinds of photo requests from us, please click this <a href=\""{{ GlobalAttribute.PublicApplicationRoot }}PhotoRequest/OptOut/{{ Person.UrlEncodedKey }}\"" style=\""line-height: 1.6em;\"">Opt Out</a>&nbsp;link.</small></p>\n\n<p>Thank you for helping us take our ministry to the next level,</p>\n\n<p>{{ Communication.ChannelData.FromName }}<br />\nEmail:&nbsp;<a href=\""mailto:{{ Communication.ChannelData.FromAddress }}\"" style=\""color: rgb(43, 166, 203); text-decoration: none;\"">{{ Communication.ChannelData.FromAddress }}</a></p>\n{{ GlobalAttribute.EmailFooter }}"" }'
                ,CAST(0x0000A37C01572215 AS DateTime)
                ,CAST(0x0000A38500F00EE8 AS DateTime)
                ,N'B9A0489C-A823-4C5C-A9F9-14A206EC3B88' )
" );

            // Add new Action called "Request Photo" on person bio block
//            Sql( @"
//            -- Add new Action called 'Request Photo' on person bio block
//            DECLARE @BlockId int = ( SELECT [Id] FROM [Block] WHERE [Guid] = 'B5C1FDB6-0224-43E4-8E26-6B2EAF86253A' )
//            DECLARE @AttributeId int = ( SELECT [Id] FROM [Attribute] WHERE [Guid] = '35F69669-48DE-4182-B828-4EC9C1C31B08' )
//
//            IF NOT EXISTS (SELECT [ID] FROM  [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId AND [Value] LIKE '%templateGuid=B9A0489C-A823-4C5C-A9F9-14A206EC3B88%' )
//            BEGIN
//                UPDATE [AttributeValue] SET [Value] = [Value] + '
//	                <li>
//		                <a href=''~/Communication/?templateGuid=B9A0489C-A823-4C5C-A9F9-14A206EC3B88&PersonId={0}'' tabindex=''0''>
//			                <i class=''fa fa-camera''></i>
//			                Request Photo
//		                </a>
//	                </li>'
//                WHERE [AttributeId] = @AttributeId
//                AND [EntityId] = @BlockId
//            END
//" );

            #endregion

            #region Add Test Gateway

            // Add the test gateway component and set attributes to make it active
            RockMigrationHelper.UpdateEntityType( "Rock.Financial.TestGateway", "C22B0247-7C9F-411B-A1F5-0051FCBAC199", false, true );
            RockMigrationHelper.AddEntityAttribute("Rock.Financial.TestGateway", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "", "Should Service be used?", 0, "False", "5DBC1A27-6387-4248-B4A7-0292A4702556");
            RockMigrationHelper.AddAttributeValue( "5DBC1A27-6387-4248-B4A7-0292A4702556", 0, "True", "4D0025EA-3A05-420D-8D37-2FAFD031C852" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Financial.TestGateway", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "", "", "Order", "", "The order that this service should be used (priority)", 0, "", "693E05D8-0DE9-42E4-91E0-84ADAAFFCDEE" );
            RockMigrationHelper.AddAttributeValue( "693E05D8-0DE9-42E4-91E0-84ADAAFFCDEE", 0, "", "36E7A7C6-E985-48BB-8E66-A8393B9EA428" );

            // Attrib for BlockType: Scheduled Transaction Edit:Credit Card Gateway
            RockMigrationHelper.UpdateBlockTypeAttribute( "5171C4E5-7698-453E-9CC8-088D362296DE", "A7486B0E-4CA2-4E00-A987-5544C7DABA76", "Credit Card Gateway", "CCGateway", "", "The payment gateway to use for Credit Card transactions", 0, @"C22B0247-7C9F-411B-A1F5-0051FCBAC199", "8177CE07-AA1B-43F4-ABCE-23E63DA8EBC8" );
            RockMigrationHelper.AddBlockAttributeValue( "CC4AC47D-1EA8-406F-94D5-50D19DC6B87A", "8177CE07-AA1B-43F4-ABCE-23E63DA8EBC8", "C22B0247-7C9F-411B-A1F5-0051FCBAC199" );
            RockMigrationHelper.AddBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "8177CE07-AA1B-43F4-ABCE-23E63DA8EBC8", "C22B0247-7C9F-411B-A1F5-0051FCBAC199" );
            RockMigrationHelper.AddBlockAttributeValue( "75F15397-3B82-4879-B069-DABD3619FAA3", "8177CE07-AA1B-43F4-ABCE-23E63DA8EBC8", "C22B0247-7C9F-411B-A1F5-0051FCBAC199" );

            // Attrib for BlockType: Scheduled Transaction Edit:ACH Gateway
            RockMigrationHelper.UpdateBlockTypeAttribute( "5171C4E5-7698-453E-9CC8-088D362296DE", "A7486B0E-4CA2-4E00-A987-5544C7DABA76", "ACH Gateway", "ACHGateway", "", "The payment gateway to use for ACH (bank account) transactions", 1, @"C22B0247-7C9F-411B-A1F5-0051FCBAC199", "FC9DF232-D7B1-4CA9-B348-D139276783BB" );
            RockMigrationHelper.AddBlockAttributeValue( "CC4AC47D-1EA8-406F-94D5-50D19DC6B87A", "FC9DF232-D7B1-4CA9-B348-D139276783BB", "C22B0247-7C9F-411B-A1F5-0051FCBAC199" );
            RockMigrationHelper.AddBlockAttributeValue( "6BF2F96A-51D6-4A84-BDA4-4EE6FDC2B515", "FC9DF232-D7B1-4CA9-B348-D139276783BB", "C22B0247-7C9F-411B-A1F5-0051FCBAC199" );
            RockMigrationHelper.AddBlockAttributeValue( "75F15397-3B82-4879-B069-DABD3619FAA3", "FC9DF232-D7B1-4CA9-B348-D139276783BB", "C22B0247-7C9F-411B-A1F5-0051FCBAC199" );

            // Attrib for BlockType: Transaction Entry:Credit Card Gateway
            RockMigrationHelper.UpdateBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "A7486B0E-4CA2-4E00-A987-5544C7DABA76", "Credit Card Gateway", "CCGateway", "", "The payment gateway to use for Credit Card transactions", 0, @"C22B0247-7C9F-411B-A1F5-0051FCBAC199", "3D478949-1F85-4E81-A403-22BBA96B8F69" );
            RockMigrationHelper.AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "3D478949-1F85-4E81-A403-22BBA96B8F69", "C22B0247-7C9F-411B-A1F5-0051FCBAC199" );
            RockMigrationHelper.AddBlockAttributeValue( "8ADB1C1F-299B-461A-8469-0FF4E2C98216", "3D478949-1F85-4E81-A403-22BBA96B8F69", "C22B0247-7C9F-411B-A1F5-0051FCBAC199" );

            // Attrib for BlockType: Transaction Entry:ACH Gateway
            RockMigrationHelper.UpdateBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "A7486B0E-4CA2-4E00-A987-5544C7DABA76", "ACH Gateway", "ACHGateway", "", "The payment gateway to use for ACH (bank account) transactions", 1, @"C22B0247-7C9F-411B-A1F5-0051FCBAC199", "D6429E78-E8F0-4EF2-9D18-DFDDE4ECC6A7" );
            RockMigrationHelper.AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "D6429E78-E8F0-4EF2-9D18-DFDDE4ECC6A7", "C22B0247-7C9F-411B-A1F5-0051FCBAC199" );
            RockMigrationHelper.AddBlockAttributeValue( "8ADB1C1F-299B-461A-8469-0FF4E2C98216", "D6429E78-E8F0-4EF2-9D18-DFDDE4ECC6A7", "C22B0247-7C9F-411B-A1F5-0051FCBAC199" );

            #endregion

            #region Financial Source Defined Values

            // Update the values for transaction source
            Sql( @"
    DECLARE @MailerId int = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '0149EB64-00C4-4C69-B1A6-2FD0EDFC6ACB' )
    DECLARE @WebsiteId int = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '7D705CE7-7B11-4342-A58E-53617C5B4E69' )
    UPDATE [FinancialTransaction] SET [SourceTypeValueId] = @WebsiteId WHERE [SourceTypeValueId] = @MailerId
" );
            RockMigrationHelper.DeleteDefinedValue( "0149EB64-00C4-4C69-B1A6-2FD0EDFC6ACB" );  // Delete 'Mailer'
            RockMigrationHelper.UpdateDefinedValue_pre20140819( "4F02B41E-AB7D-4345-8A97-3904DDD89B01", "Website", "Transactions that originated from the website", "7D705CE7-7B11-4342-A58E-53617C5B4E69", true );
            RockMigrationHelper.UpdateDefinedValue_pre20140819( "4F02B41E-AB7D-4345-8A97-3904DDD89B01", "Kiosk", "Transactions that originated from a kiosk", "260EEA80-821A-4F79-973F-49DF79C955F7", false );
            RockMigrationHelper.UpdateDefinedValue_pre20140819( "4F02B41E-AB7D-4345-8A97-3904DDD89B01", "Mobile Application", "Transactions that originated from a mobile application", "8ADCEC72-63FC-4F08-A4CC-72BCE470172C", false );
            RockMigrationHelper.UpdateDefinedValue_pre20140819( "4F02B41E-AB7D-4345-8A97-3904DDD89B01", "On-Site Collection", "Transactions that were collected on-site", "BE7ECF50-52BC-4774-808D-574BA842DB98", false );

            // Set default source to website on Financial Entry block
            RockMigrationHelper.AddBlockAttributeValue( "8ADB1C1F-299B-461A-8469-0FF4E2C98216", "5C54E6E7-1C21-4959-98EA-FB1C2D0A0D61", @"7D705CE7-7B11-4342-A58E-53617C5B4E69" );
            RockMigrationHelper.AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "5C54E6E7-1C21-4959-98EA-FB1C2D0A0D61", @"7D705CE7-7B11-4342-A58E-53617C5B4E69" );

            #endregion

            #region Deceased DataView Filter

            Sql( @"
    -- Add a filter to base dataview to exclude deceased people
    DECLARE @ParentDataViewFilterId INT = ( SELECT TOP 1 [DataViewFilterId] FROM [DataView] WHERE [Guid] = '0DA5F82F-CFFE-45AF-B725-49B3899A1F72' )
    DECLARE @PropertyFilterEntityTypeId INT = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323' )
    INSERT INTO [DataViewFilter] ( [ExpressionType], [ParentId], [EntityTypeId], [Selection], [Guid] )
    VALUES ( 0, @ParentDataViewFilterId, @PropertyFilterEntityTypeId ,N'[
  ""IsDeceased"",
  ""False""
]'
    ,'53B219C5-A25D-42C3-9345-DFDFF6331CF1' )
" );

            #endregion

            #region Misc Changes

            // set security for View My Giving page (VIEW: auth users = true, all users = false)
            RockMigrationHelper.AddSecurityAuthForPage( "621e03c5-6586-450b-a340-cc7d9727b69a", 0, "View", true, null, 2, "f328528c-e874-415a-9319-70783dcd6841" );
            RockMigrationHelper.AddSecurityAuthForPage( "621e03c5-6586-450b-a340-cc7d9727b69a", 0, "View", false, null, 1, "0a451fc6-72ef-4daf-a251-beada05b360a" );

            // set security for Manage Giving Profile page (VIEW: auth users = true, all users = false)
            RockMigrationHelper.AddSecurityAuthForPage( "fffdce23-7b67-4b0d-8da0-e44d883708cc", 0, "View", true, null, 2, "6189ac92-8a2c-4019-b44e-3433370f5eea" );
            RockMigrationHelper.AddSecurityAuthForPage( "fffdce23-7b67-4b0d-8da0-e44d883708cc", 0, "View", false, null, 1, "de61aff2-e2a8-4922-a665-4e866685f5d0" );

            // set security for Edit Giving Profile page (VIEW: auth users = true, all users = false)
            RockMigrationHelper.AddSecurityAuthForPage( "2072f4bc-53b4-4481-bc15-38f14425c6c9", 0, "View", true, null, 2, "e9865cc7-4aa5-4564-98b5-02e447700de0" );
            RockMigrationHelper.AddSecurityAuthForPage( "2072f4bc-53b4-4481-bc15-38f14425c6c9", 0, "View", false, null, 1, "de3f1cce-d9f0-4df8-b4c3-75055f9bf526" );

            Sql( @"
    --make content blocks on intranet pages non system    
    UPDATE [Block]
    SET [IsSystem] = 0
    WHERE [Guid] IN ( 'b8224c72-4168-40f0-96be-38f2afd525f5', '718c516f-0a1d-4dbc-a939-1d9777208fec')

    -- rename the inactive report to 'self inactivated individuals' this is closer to what it really is
    UPDATE [Report] 
    SET [Name] = 'Self Inactivated Individuals', [IsSystem] = 0
    WHERE [Guid] = '87d3e118-ada8-4424-b63b-9482a7d9e609'

    -- set display setting so pages show in nav with new permissions
    UPDATE [Page]
    SET [DisplayInNavWhen] = 1
    WHERE [Guid] IN ('fffdce23-7b67-4b0d-8da0-e44d883708cc','621e03c5-6586-450b-a340-cc7d9727b69a')
" );
            
            #endregion
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            #region Deceased DataView Filter

            Sql( @"
    -- Remove deceased filter
    DELETE [DataViewFilter] WHERE [Guid] = '53B219C5-A25D-42C3-9345-DFDFF6331CF1'
" );

            #endregion

            #region Photo Request Block

            RockMigrationHelper.DeleteBlock( "37BBE63E-5CB5-4F4D-8657-BBB13CA52919" );
            RockMigrationHelper.DeleteBlock( "44C793B7-8E2B-4D70-BFA1-5FE21FE4390C" );
            RockMigrationHelper.DeletePage( "04141667-1A08-4E15-8BB7-E3E312233E11" );
            RockMigrationHelper.DeleteAttribute( "722F16A0-1F04-4D52-A850-A39055629617" );
            RockMigrationHelper.DeleteBlockType( "14293AEB-B0F5-434B-844A-66592AE3A416" );
            RockMigrationHelper.DeleteBlockType( "160DABF9-3549-447C-9E76-6CFCCCA481C0" );

            #endregion

        }
    }
}
