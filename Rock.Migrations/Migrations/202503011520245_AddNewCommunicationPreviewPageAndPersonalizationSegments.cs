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

    using Rock.Migrations.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class AddNewCommunicationPreviewPageAndPersonalizationSegments : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            #region Update Communication Schema And Data

            AddColumn("dbo.Communication", "PersonalizationSegments", c => c.String( nullable: true ));

            RockMigrationHelper.UpdateCategory( SystemGuid.EntityType.PERSONALIZATION_SEGMENT, "Communications", string.Empty, string.Empty, SystemGuid.Category.PERSONALIZATION_SEGMENT_COMMUNICATIONS );

            // Add Blank (Preview) template.
            Sql( @"
DECLARE @Guid UNIQUEIDENTIFIER = '6280214C-404E-4F4E-BC33-7A5D4CDF8DBC';
DECLARE @CategoryId INT = (SELECT [Id] FROM [dbo].[Category] WHERE [Guid] = 'A7F79054-5539-4910-A13F-AA5884B8C01D');

IF NOT EXISTS (SELECT [Id] FROM [dbo].[CommunicationTemplate] WHERE [Guid] = @Guid)
BEGIN
    INSERT INTO [dbo].[CommunicationTemplate]
               ([Name]
               ,[Description]
               ,[Subject]
               ,[Guid]
               ,[IsSystem]
               ,[Message]
               ,[IsActive]
               ,[CategoryId]
               ,[CssInliningEnabled]
               ,[IsStarter])
         VALUES
               ('Blank (Preview)'
               ,'The default email template for sending emails using Rock''s Communications Wizard.'
               ,''
               ,@Guid
               ,1
               ,'<!DOCTYPE html>
    <html>
    <head>
        <title>A Responsive Email Template</title>
    
        <meta charset=""utf-8"">
        <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
        <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" >
        <style>
            /* ========================================================
               CLIENT-SPECIFIC STYLES
               - Handles quirks across various email clients
            ======================================================== */
    
            /* Prevents mobile devices from adjusting text size */
            body, table, td, a {
                -webkit-text-size-adjust: 100%;
                -ms-text-size-adjust: 100%;
            }
    
            /* Removes extra spacing between tables in Outlook */
            table, td {
                mso-table-lspace: 0pt;
                mso-table-rspace: 0pt;
            }
    
            /* Improves image scaling in older versions of Outlook (Word engine) */
            img {
                -ms-interpolation-mode: bicubic;
            }
    
            /* ========================================================
               GENERAL RESET STYLES
               - Normalize elements across clients
            ======================================================== */
    
            img {
                border: 0;
                height: auto;
                line-height: 100%;
                outline: none;
                text-decoration: none;
            }
    
            table {
                border-collapse: collapse !important;
            }
    
            body {
                height: 100% !important;
                margin: 0 !important;
                padding: 0 !important;
                width: 100% !important;
            }
    
            /* ========================================================
               iOS BLUE LINKS
               - Resets auto-linked phone numbers, emails, etc.
            ======================================================== */
    
            a[x-apple-data-detectors] {
                color: inherit !important;
                text-decoration: none !important;
                font-size: inherit !important;
                font-family: inherit !important;
                font-weight: inherit !important;
                line-height: inherit !important;
            }
    
            /* ========================================================
               RESPONSIVE MOBILE STYLES
               - Adjusts layout for screens <= 525px wide
            ======================================================== */
    
            @media screen and (max-width: 525px) {
                /* Makes tables fluid */
                .wrapper {
                    width: 100% !important;
                    max-width: 100% !important;
                }
    
                /* Centers logo images */
                .logo img {
                    margin: 0 auto !important;
                }
    
                /* Hides elements on mobile */
                .mobile-hide {
                    display: none !important;
                }
    
                /* Ensures images scale fluidly */
                .img-max {
                    max-width: 100% !important;
                    width: 100% !important;
                    height: auto !important;
                }
    
                /* Makes tables take full width */
                .responsive-table {
                    width: 100% !important;
                }
    
                /* Padding utility classes for mobile adjustments */
                .padding {
                    padding: 10px 5% 15px 5% !important;
                }
    
                .padding-meta {
                    padding: 30px 5% 0px 5% !important;
                    text-align: center;
                }
    
                .padding-copy {
                    padding: 10px 5% 10px 5% !important;
                    text-align: center;
                }
    
                .no-padding {
                    padding: 0 !important;
                }
    
                .section-padding {
                    padding: 50px 15px 50px 15px !important;
                }
    
                /* Styles buttons for better touch targets */
                .mobile-button-container {
                    margin: 0 auto;
                    width: 100% !important;
                }
    
                .mobile-button {
                    padding: 15px !important;
                    border: 0 !important;
                    font-size: 16px !important;
                    display: block !important;
                }
            }
    
            /* ========================================================
               ANDROID GMAIL FIX
               - Removes margin added by some Android email clients
            ======================================================== */
            div[style*=""margin: 16px 0;""] {
                margin: 0 !important;
            }
        </style>
        <!-- ========================================================
             OUTLOOK (MSO) CONDITIONAL STYLES
             - Targets Outlook 2007+ (mso 12 and later)
             - Used for padding adjustments or MSO-specific tweaks
        ========================================================= -->
        <!--[if gte mso 12]>
        <style>
            .mso-right {
                padding-left: 20px;
            }
        </style>
        <![endif]-->
        <style class=""ignore"">
            @media screen and (max-width: 600px) {
                .email-wrapper {
                    min-height: 100vh;
                }
            }
        </style>
    </head>

    <body>
        <style class=""rock-styles"">
            .email-row-content>tbody>tr>td,
            .email-row-content>tr>td {
                padding: 24px;
            }

            body,
            .email-wrapper>tbody>tr>td,
            .email-wrapper>tr>td {
                font-family: Arial;
                font-size: 16px;
                color: rgb(54, 65, 83);
                line-height: 1.5;
            }

            .component-title h1 {
                font-family: Arial;
                font-size: 38px;
                font-weight: bold;
                color: rgb(3, 7, 18);
                line-height: 1.2;
                margin: 0px;
            }

            .component-title h2 {
                font-family: Arial;
                font-size: 30px;
                font-weight: bold;
                color: rgb(3, 7, 18);
                line-height: 1.2;
                margin: 0px;
            }

            .component-title h3 {
                font-family: Arial;
                font-size: 24px;
                font-weight: bold;
                color: rgb(3, 7, 18);
                line-height: 1.2;
                margin: 0px;
            }

            .component-text {
                font-family: Arial;
                font-size: 16px;
                color: rgb(54, 65, 83);
                line-height: 1.2;
                margin: 0px;
            }

            .component-button .button-link {
                background-color: rgb(57, 118, 246);
                font-family: Arial;
                font-weight: bold;
                text-decoration: none;
                border-bottom-width: 0px;
                color: rgb(255, 255, 255);
                line-height: 1.2;
                padding: 15px;
                text-align: center;
                letter-spacing: normal;
            }

            .component-button .button-content,
            .component-button .button-link {
                border-radius: 4px;
            }

            .component-divider hr {
                border-style: solid none none;
                border-width: 1px 0px 0px;
                border-color: rgb(139, 139, 167) transparent transparent;
                width: 100%;
                margin: 12px 0px;
            }

            html,
            body {
                margin: 0px;
                padding: 0px;
                height: 100%;
            }

            .email-wrapper {
                width: 100%;
                height: 100%;
                background-color: rgb(231, 231, 231);
            }

            .email-row-content {
                max-width: 600px;
                width: 100%;
                background-color: rgb(255, 255, 255);
            }
        </style>
        <div id=""preheader-text"" 
            style=""display: none; font-size: 1px; color: #ffffff; line-height: 1px; font-family: Helvetica, Arial, sans-serif; max-height: 0px; max-width: 0px; opacity: 0; overflow: hidden; mso-hide: all; height: 0px; visibility: hidden;""> </div>
        <table class=""email-wrapper"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""presentation"" bgcolor=""#e7e7e7"" style=""min-width: 100%; height: 100%;"">
            <tbody>
                <tr>
                    <td align=""center"" valign=""top"" style=""height: 100%;"">
                        <div class=""structure-dropzone"">
                            <div class=""component component-row"" data-state=""component"">
                                <table class=""email-row"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""presentation"">
                                    <tbody>
                                        <tr>
                                            <td align=""center"">
                                                <table class=""email-row-content"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""600"" role=""presentation"" bgcolor=""#ffffff"">
                                                    <tbody>
                                                        <tr>
                                                            <td>
                                                                <table class=""email-content"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""presentation"">
                                                                    <tbody>
                                                                        <tr>
                                                                            <td>
                                                                                <div class=""dropzone""></div>
                                                                            </td>
                                                                        </tr>
                                                                    </tbody>
                                                                </table>
                                                            </td>
                                                        </tr>
                                                    </tbody>
                                                </table>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </td>
                </tr>
            </tbody>
        </table>
    </body>
    </html>'
               ,1
               ,@CategoryId
               ,1
               ,1)
END" );

            // Remove IsStarter from Blank template.
            Sql( $"UPDATE [CommunicationTemplate] SET [IsStarter] = 0 WHERE [Guid] = '{SystemGuid.Communication.COMMUNICATION_TEMPLATE_BLANK}'" );


            #endregion Update Communication Schema And Data
            
            #region Update Communication Recipients Stored Procedure

            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spCommunicationRecipientDetails] (dropping it first).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spCommunicationRecipientDetails]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spCommunicationRecipientDetails];" );

            Sql( RockMigrationSQL._202503011520245_AddNewCommunicationPreviewPageAndPersonalizationSegments_spCommunicationRecipientDetails );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );

            #endregion Update Communication Recipients Stored Procedure

            #region Create Communication Preview Page

            var newCommunicationPreviewPageGuid = "9F7AE226-CC95-4E6A-B333-C0294A2024BC";

            // Create the New Communication (Preview) page from the New Communication page.
            Sql( $@"
DECLARE @SourcePageGuid UNIQUEIDENTIFIER = '{SystemGuid.Page.NEW_COMMUNICATION}';
DECLARE @NewPageGuid UNIQUEIDENTIFIER = '{newCommunicationPreviewPageGuid}';
DECLARE @NewPageName VARCHAR(100) = N'New Communication (Preview)';

DECLARE @SourcePageId INT
DECLARE @SourcePageParentPageId INT
DECLARE @SourcePageOrder INT
SELECT  @SourcePageId = [Id], 
        @SourcePageParentPageId = [ParentPageId], 
        @SourcePageOrder = [Order] 
  FROM [dbo].[Page] 
 WHERE [Guid] = @SourcePageGuid

DECLARE @PageEntityTypeId INT = (SELECT [Id] FROM [dbo].[EntityType] WHERE [Guid] = '{SystemGuid.EntityType.PAGE}');
DECLARE @NewPageId INT = (SELECT [Id] FROM [dbo].[Page] WHERE [Guid] = @NewPageGuid);

/* Create a new page from the source page. */
IF @NewPageId IS NULL OR @NewPageId = 0
BEGIN
UPDATE P
   SET [Order] = [Order] + 1
  FROM [dbo].[Page] P
 WHERE [ParentPageId] = @SourcePageParentPageId
   AND [Order] = @SourcePageOrder + 1;

INSERT INTO [dbo].[Page]
           ([Guid]
           ,[InternalName]
           ,[BrowserTitle]
           ,[PageTitle]
           ,[ParentPageId]
           ,[IsSystem]
           ,[LayoutId]
           ,[RequiresEncryption]
           ,[EnableViewState]
           ,[PageDisplayTitle]
           ,[PageDisplayBreadCrumb]
           ,[PageDisplayIcon]
           ,[PageDisplayDescription]
           ,[DisplayInNavWhen]
           ,[MenuDisplayDescription]
           ,[MenuDisplayIcon]
           ,[MenuDisplayChildPages]
           ,[BreadCrumbDisplayName]
           ,[BreadCrumbDisplayIcon]
           ,[Order]
           ,[OutputCacheDuration]
           ,[Description]
           ,[IconCssClass]
           ,[IncludeAdminFooter]
           ,[KeyWords]
           ,[HeaderContent]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[AllowIndexing]
           ,[BodyCssClass]
           ,[IconBinaryFileId]
           ,[AdditionalSettings]
           ,[MedianPageLoadTimeDurationSeconds]
           ,[CacheControlHeaderSettings]
           ,[RateLimitRequestPerPeriod]
           ,[RateLimitPeriodDurationSeconds]
           ,[AdditionalSettingsJson]
           ,[BotGuardianLevel]
           ,[SiteId])
     SELECT TOP 1
            @NewPageGuid
           ,@NewPageName
           ,@NewPageName
           ,@NewPageName
           ,[ParentPageId]
           ,[IsSystem]
           ,[LayoutId]
           ,[RequiresEncryption]
           ,[EnableViewState]
           ,[PageDisplayTitle]
           ,[PageDisplayBreadCrumb]
           ,[PageDisplayIcon]
           ,[PageDisplayDescription]
           ,[DisplayInNavWhen]
           ,[MenuDisplayDescription]
           ,[MenuDisplayIcon]
           ,[MenuDisplayChildPages]
           ,[BreadCrumbDisplayName]
           ,[BreadCrumbDisplayIcon]
           ,[Order] + 1
           ,[OutputCacheDuration]
           ,[Description]
           ,[IconCssClass]
           ,[IncludeAdminFooter]
           ,[KeyWords]
           ,[HeaderContent]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[ForeignGuid]
           ,[ForeignId]
           ,[AllowIndexing]
           ,[BodyCssClass]
           ,[IconBinaryFileId]
           ,[AdditionalSettings]
           ,[MedianPageLoadTimeDurationSeconds]
           ,[CacheControlHeaderSettings]
           ,[RateLimitRequestPerPeriod]
           ,[RateLimitPeriodDurationSeconds]
           ,[AdditionalSettingsJson]
           ,[BotGuardianLevel]
           ,[SiteId]
    FROM [dbo].[Page]
    WHERE [Guid] = @SourcePageGuid;

    SELECT @NewPageId = SCOPE_IDENTITY();
END

/* Copy page security from the source page to the new page. */
IF NOT EXISTS (SELECT [Id] FROM [dbo].[Auth] WHERE [EntityTypeId] = @PageEntityTypeId AND [EntityId] = @NewPageId)
INSERT INTO [dbo].[Auth]
           ([EntityTypeId]
           ,[EntityId]
           ,[Order]
           ,[Action]
           ,[AllowOrDeny]
           ,[SpecialRole]
           ,[GroupId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[PersonAliasId]
           ,[ForeignGuid]
           ,[ForeignId])
     SELECT
           [EntityTypeId]
           ,@NewPageId
           ,[Order]
           ,[Action]
           ,[AllowOrDeny]
           ,[SpecialRole]
           ,[GroupId]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId]
           ,[ForeignKey]
           ,[PersonAliasId]
           ,[ForeignGuid]
           ,[ForeignId]
    FROM [dbo].[Auth]
    WHERE [EntityTypeId] = @PageEntityTypeId AND [EntityId] = @SourcePageId" );

            // Create page routes.
            this.RockMigrationHelper.AddOrUpdatePageRoute( newCommunicationPreviewPageGuid, "Communication/Preview/{CommunicationId}", "07E4E970-8C2B-46B3-900C-F12E7EF00E14" );
            this.RockMigrationHelper.AddOrUpdatePageRoute( newCommunicationPreviewPageGuid, "Communication/Preview", "0AC9BD59-3786-4D81-89A3-77FC06A618AE" );
            this.RockMigrationHelper.AddOrUpdatePageRoute( newCommunicationPreviewPageGuid, "Communications/Preview/{CommunicationId}", "45409F5C-C031-49BE-A50D-D14E7E378BA3" );
            this.RockMigrationHelper.AddOrUpdatePageRoute( newCommunicationPreviewPageGuid, "Communications/Preview/new", "CEE3AF81-6CAA-4B6B-8CC8-0B5FC55B1B2E" );

            // Create Communication Entry Wizard v2 block to new page.

            // Add Block 
            //  Block Name: Communication Entry Wizard
            //  Page Name: New Communication (Preview)
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, newCommunicationPreviewPageGuid.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27".AsGuid(), "Communication Entry Wizard", "Main", @"", @"", 0, "EF53AAFB-EEA4-4354-BA1A-D430D465A913" );
            // Add Block 
            //  Block Name: Communication Detail
            //  Page Name: New Communication (Preview)
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "9F7AE226-CC95-4E6A-B333-C0294A2024BC".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"CEDC742C-0AB3-487D-ABC2-77A0A443AEBF".AsGuid(), "Communication Detail","Main",@"",@"",1,"8B93FA03-6EE8-4DFC-B0F4-E0145699AF55"); 

            // update block order for pages with new blocks if the page,zone has multiple blocks

            // Update Order for Page: New Communication (Preview),  Zone: Main,  Block: Communication Detail
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '8B93FA03-6EE8-4DFC-B0F4-E0145699AF55'"  );

            // Update Order for Page: New Communication (Preview),  Zone: Main,  Block: Communication Entry Wizard
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'EF53AAFB-EEA4-4354-BA1A-D430D465A913'"  );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Image Binary File Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "09EC7F0D-3505-4090-B010-ABA68CB9B904", "Image Binary File Type", "ImageBinaryFileType", "Image Binary File Type", @"The FileType to use for images that are added to the email using the image component", 1, @"60B896C3-F00C-411C-A31C-2D5D4CCBB65F", "5254D74F-E9C4-4825-93A9-85803006F4F1" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Attachment Binary File Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "09EC7F0D-3505-4090-B010-ABA68CB9B904", "Attachment Binary File Type", "AttachmentBinaryFileType", "Attachment Binary File Type", @"The FileType to use for files that are attached to an SMS or email communication", 2, @"10FD7FE8-7187-45CC-A1E7-D9F71BD90E6C", "8279CF6E-E60F-4C0F-BB05-7975AAC5B3E8" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Maximum Recipients
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Maximum Recipients", "MaximumRecipients", "Maximum Recipients", @"The maximum number of recipients allowed before communication will need to be approved.", 5, @"300", "576EF0EA-63E4-4C41-B3F2-E9DAD76129A8" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Send When Approved
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Send When Approved", "SendWhenApproved", "Send When Approved", @"Should communication be sent once it's approved (vs. just being queued for scheduled job to send)?", 6, @"True", "0D931D61-28DA-4CA4-9610-97A131598340" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Max SMS Image Width
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max SMS Image Width", "MaxSMSImageWidth", "Max SMS Image Width", @"The maximum width (in pixels) of an image attached to a mobile communication. If its width is over the max, Rock will automatically resize image to the max width.", 7, @"600", "DDD9C8F3-1024-4561-9014-71B59560E66D" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Show Duplicate Prevention Option
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Duplicate Prevention Option", "ShowDuplicatePreventionOption", "Show Duplicate Prevention Option", @"Set this to true to show an option to prevent communications from being sent to people with the same email/SMS addresses. Typically, in Rock you’d want to send two emails as each will be personalized to the individual.", 10, @"False", "4F43506E-5246-40FE-B02E-78C8F5873EB7" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Default As Bulk
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default As Bulk", "DefaultAsBulk", "Default As Bulk", @"Should new entries be flagged as bulk communication by default?", 11, @"False", "456EB249-56B2-400F-A45B-B2A2F236CA9D" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Enable Person Parameter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Person Parameter", "EnablePersonParameter", "Enable Person Parameter", @"When enabled, allows passing a 'Person' or 'PersonId' querystring parameter with a person Id to the block to create a communication for that person.", 12, @"True", "34FE0E4A-DB17-4B7B-97AB-5B501887A4A9" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Disable Adding Individuals to Recipient Lists
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Adding Individuals to Recipient Lists", "DisableAddingIndividualsToRecipientLists", "Disable Adding Individuals to Recipient Lists", @"When set to 'Yes' the person picker will be hidden so that additional individuals cannot be added to the recipient list.", 13, @"False", "C814862A-C1A5-448D-9C72-4EFE2E0607C3" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Personalization Segment Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Personalization Segment Category", "PersonalizationSegmentCategory", "Personalization Segment Category", @"Choose a category of Personalization Segments to be displayed.", 14, @"F80DADC1-AF83-4953-BDA6-6A9F046EA8E6", "72BEE329-609E-4A1E-838D-0676CB1C4335" );

            // Add Block Attribute Value
            //   Block: Communication Entry Wizard
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Block Location: Page=Communication Entry Wizard V2, Site=Rock RMS
            //   Attribute: Personalization Segment Category
            /*   Attribute Value: f80dadc1-af83-4953-bda6-6a9f046ea8e6 */
            RockMigrationHelper.AddBlockAttributeValue( "EF53AAFB-EEA4-4354-BA1A-D430D465A913", "72BEE329-609E-4A1E-838D-0676CB1C4335", @"f80dadc1-af83-4953-bda6-6a9f046ea8e6" );

            // Add Block Attribute Value
            //   Block: Communication Entry Wizard
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Block Location: Page=Communication Entry Wizard V2, Site=Rock RMS
            //   Attribute: Image Binary File Type
            /*   Attribute Value: 60b896c3-f00c-411c-a31c-2d5d4ccbb65f */
            RockMigrationHelper.AddBlockAttributeValue( "EF53AAFB-EEA4-4354-BA1A-D430D465A913", "5254D74F-E9C4-4825-93A9-85803006F4F1", @"60b896c3-f00c-411c-a31c-2d5d4ccbb65f" );

            // Add Block Attribute Value
            //   Block: Communication Entry Wizard
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Block Location: Page=Communication Entry Wizard V2, Site=Rock RMS
            //   Attribute: Attachment Binary File Type
            /*   Attribute Value: 10fd7fe8-7187-45cc-a1e7-d9f71bd90e6c */
            RockMigrationHelper.AddBlockAttributeValue( "EF53AAFB-EEA4-4354-BA1A-D430D465A913", "8279CF6E-E60F-4C0F-BB05-7975AAC5B3E8", @"10fd7fe8-7187-45cc-a1e7-d9f71bd90e6c" );

            // Add Block Attribute Value
            //   Block: Communication Entry Wizard
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Block Location: Page=Communication Entry Wizard V2, Site=Rock RMS
            //   Attribute: Maximum Recipients
            /*   Attribute Value: 300 */
            RockMigrationHelper.AddBlockAttributeValue( "EF53AAFB-EEA4-4354-BA1A-D430D465A913", "576EF0EA-63E4-4C41-B3F2-E9DAD76129A8", @"300" );

            // Add Block Attribute Value
            //   Block: Communication Entry Wizard
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Block Location: Page=Communication Entry Wizard V2, Site=Rock RMS
            //   Attribute: Send When Approved
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "EF53AAFB-EEA4-4354-BA1A-D430D465A913", "0D931D61-28DA-4CA4-9610-97A131598340", @"True" );

            // Add Block Attribute Value
            //   Block: Communication Entry Wizard
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Block Location: Page=Communication Entry Wizard V2, Site=Rock RMS
            //   Attribute: Max SMS Image Width
            /*   Attribute Value: 600 */
            RockMigrationHelper.AddBlockAttributeValue( "EF53AAFB-EEA4-4354-BA1A-D430D465A913", "DDD9C8F3-1024-4561-9014-71B59560E66D", @"600" );

            // Add Block Attribute Value
            //   Block: Communication Entry Wizard
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Block Location: Page=Communication Entry Wizard V2, Site=Rock RMS
            //   Attribute: Show Duplicate Prevention Option
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "EF53AAFB-EEA4-4354-BA1A-D430D465A913", "4F43506E-5246-40FE-B02E-78C8F5873EB7", @"False" );

            // Add Block Attribute Value
            //   Block: Communication Entry Wizard
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Block Location: Page=Communication Entry Wizard V2, Site=Rock RMS
            //   Attribute: Default As Bulk
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "EF53AAFB-EEA4-4354-BA1A-D430D465A913", "456EB249-56B2-400F-A45B-B2A2F236CA9D", @"False" );

            // Add Block Attribute Value
            //   Block: Communication Entry Wizard
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Block Location: Page=Communication Entry Wizard V2, Site=Rock RMS
            //   Attribute: Enable Person Parameter
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "EF53AAFB-EEA4-4354-BA1A-D430D465A913", "34FE0E4A-DB17-4B7B-97AB-5B501887A4A9", @"True" );

            // Add Block Attribute Value
            //   Block: Communication Entry Wizard
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Block Location: Page=Communication Entry Wizard V2, Site=Rock RMS
            //   Attribute: Disable Adding Individuals to Recipient Lists
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "EF53AAFB-EEA4-4354-BA1A-D430D465A913", "C814862A-C1A5-448D-9C72-4EFE2E0607C3", @"False" );

            // Add the Communication Detail block to the page too.

            #endregion Create Communication Preview Page
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            #region Delete Communication Preview Page

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Personalization Segment Category
            RockMigrationHelper.DeleteAttribute( "72BEE329-609E-4A1E-838D-0676CB1C4335" );

            // Remove Block
            //  Name: Communication Entry Wizard, from Page: New Communication (Preview), Site: Rock RMS
            //  from Page: New Communication (Preview), Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "EF53AAFB-EEA4-4354-BA1A-D430D465A913" );

            // Remove Block
            //  Name: Communication Detail, from Page: New Communication (Preview), Site: Rock RMS
            //  from Page: New Communication (Preview), Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "8B93FA03-6EE8-4DFC-B0F4-E0145699AF55" );

            // Delete Page 
            //  Internal Name: New Communication (Preview)
            //  Site: Rock RMS
            //  Layout: Full Worksurface
            RockMigrationHelper.DeletePage( "9F7AE226-CC95-4E6A-B333-C0294A2024BC" );

            #endregion Delete Communication Preview Page

            #region Drop Communication Recipients Stored Procedure

            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spCommunicationRecipientDetails]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spCommunicationRecipientDetails];" );

            #endregion Drop Communication Recipients Stored Procedure

            #region Revert Communication Schema And Data
           
            // Add IsStarter to Blank template.
            Sql( $"UPDATE [CommunicationTemplate] SET [IsStarter] = 1 WHERE [Guid] = '{SystemGuid.Communication.COMMUNICATION_TEMPLATE_BLANK}'" );

            RockMigrationHelper.DeleteCategory( SystemGuid.Category.PERSONALIZATION_SEGMENT_COMMUNICATIONS );

            DropColumn( "dbo.Communication", "PersonalizationSegments" );
            
            #endregion Revert Communication Schema And Data
        }
    }
}
