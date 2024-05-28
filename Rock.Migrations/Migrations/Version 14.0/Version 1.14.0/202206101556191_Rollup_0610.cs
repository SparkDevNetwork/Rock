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

namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_0610 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddMissingMediaInteractionsJob();
            AddBulkUpdateConnectionActivityTypeAndPageUp();
            PersistedDataViewListUp();
            FixGroupViewLavaTemplateDQ();
            AdjustLoginVerbUp();
            ElectronicSignatureLegacyDocumentWarning();
            AddAttributeToGroupScheduleToolboxV2BlockType();
            UpdateSampleDataUrlUp();
            DaysUntilBirthdayColumn();
            AddIpAddressLocationServicesPageBlocksUp();
            AddFieldTripReleaseSignatureDocument();
            RemoveMelissaDataAndServiceObjectsUp();
            AllowCheckoutAtKiosk();
            DisplayBirthdateOnAdults();
            BadgeMigrations();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddBulkUpdateConnectionActivityTypeAndPageDown();
            AdjustLoginVerbDown();
            UpdateSampleDataUrlDown();
            AddIpAddressLocationServicesPageBlocksDown();
            RemoveMelissaDataAndServiceObjectsDown();
            AddOrUpdateStepFlowPageAndRouteDown();
        }

        /// <summary>
        /// KA: Migration to Add Job To Add Missing Media Element Interactions
        /// </summary>
        private void AddMissingMediaInteractionsJob()
        {
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV14DataMigrationsAddMissingMediaElementInteractions'
                    AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_140_ADD_MISSING_MEDIA_ELEMENT_INTERACTIONS}'
            )
            BEGIN
                INSERT INTO [ServiceJob] (
                      [IsSystem]
                    , [IsActive]
                    , [Name]
                    , [Description]
                    , [Class]
                    , [CronExpression]
                    , [NotificationStatus]
                    , [Guid]
                ) VALUES (
                      1
                    , 1
                    , 'Rock Update Helper v14.0 - Add missing Media Element interactions.'
                    , 'This job will update the interation length of media element interactions. After all the operations are done, this job will delete itself.'
                    , 'Rock.Jobs.PostV14DataMigrationsAddMissingMediaElementInteractions'
                    , '0 0 21 1/1 * ? *'
                    , 1
                    , '{SystemGuid.ServiceJob.DATA_MIGRATIONS_140_ADD_MISSING_MEDIA_ELEMENT_INTERACTIONS}'
                );
            END" );
        }

        /// <summary>
        /// KA: Migration for adding BulkUpdate ConnectionActivityType And BulkUpdate Page (Up)
        /// </summary>
        private void AddBulkUpdateConnectionActivityTypeAndPageUp()
        {
            // Add Page - Internal Name: Connection Requests Bulk Update - Site: Rock RMS
            RockMigrationHelper.AddPage( true, SystemGuid.Page.CONNECTIONS, SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Connection Requests Bulk Update", "", SystemGuid.Page.CONNECTION_REQUESTS_BULK_UPDATE, "fa fa-truck" );
            // Add/Update BlockType - Name: Connection Requests Bulk Update - Category: Connection - Path: ~/Blocks/Communication/CommunicationJobPreview.ascx - EntityType: -
            RockMigrationHelper.UpdateBlockType( "Connection Requests Bulk Update", "Used for updating information about several Connection Requests at once. The QueryString must have both the EntitySetId as well as the ConnectionTypeId, and all the connection requests must be for the same opportunity.", "~/Blocks/Connection/BulkUpdateRequests.ascx", "Connection", "175158F8-F10E-476F-809E-A76825E0AC5D" );
            // Add/Update BlockTypeAttribute - Name: Previous Page - Category: Connection
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "175158F8-F10E-476F-809E-A76825E0AC5D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "Previous Page", "", 1, Rock.SystemGuid.Page.CONNECTIONS_BOARD, "80783AF9-3C03-4DC7-BDFF-9940E6338DB8" );
            // Add Block - Block Name: Connection Requests Bulk Update - Page Name: Connection Requests Bulk Update Layout: - Site: Rock RMS
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.CONNECTION_REQUESTS_BULK_UPDATE.AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), "175158F8-F10E-476F-809E-A76825E0AC5D".AsGuid(), "Connection Requests Bulk Update", "Main", @"", @"", 0, SystemGuid.Block.CONNECTION_REQUESTS_BULK_UPDATE );

            Sql( @"
                IF NOT EXISTS (SELECT [Id] FROM [ConnectionActivityType] WHERE [Guid] = '10104830-5AFC-491F-9885-747521A2AA75' )
                BEGIN
                    INSERT INTO [dbo].[ConnectionActivityType] ( [Name], [IsActive], [Guid])
                    VALUES ( N'Bulk Update', 1, N'10104830-5AFC-491F-9885-747521A2AA75')
                END" );
        }

        /// <summary>
        /// KA: Migration for adding BulkUpdate ConnectionActivityType And BulkUpdate Page (Down)
        /// </summary>
        private void AddBulkUpdateConnectionActivityTypeAndPageDown()
        {
            // Delete Attribute for BlockType - BlockType: Connection Requests Bulk Update - Category: Connection - Attribute: Previous Page
            RockMigrationHelper.DeleteAttribute( "80783AF9-3C03-4DC7-BDFF-9940E6338DB8" );
            // Delete BlockType - Name: Connection Requests Bulk Update - Category: Connection - Path: ~/Blocks/Communication/CommunicationJobPreview.ascx - EntityType: -
            RockMigrationHelper.DeleteBlockType( "175158F8-F10E-476F-809E-A76825E0AC5D" );
            // Remove Block - Name: Connection Requests Bulk Update, from Page: Connection Requests Bulk Update, Site: Rock RMS - from Page: Connection Requests Bulk Update, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( SystemGuid.Block.CONNECTION_REQUESTS_BULK_UPDATE );
            // Delete Page Internal Name: Connection Requests Bulk Update Site: Rock RMS Layout: Full Width
            RockMigrationHelper.DeletePage( SystemGuid.Page.SYSTEM_COMMUNICATION_PREVIEW );
        }

        /// <summary>
        /// KA: Adds or updates (if already exists) Persisted Data View List with a well-known Guid
        /// </summary>
        private void PersistedDataViewListUp()
        {
            RockMigrationHelper.UpdateBlockType(
                "Persisted Data View List",
                "Shows a list of Data Views that have persistence enabled.",
                "~/Blocks/Reporting/PersistedDataViewList.ascx",
                "Reporting",
                "3C4FAFAE-41D1-4FF2-B6DC-FF99CD4DABBE" );
        }

        /// <summary>
        /// DV-NA: Fix GroupViewLavaTemplate broken Lava double single quotes. Fix is also in Rollup_04271.
        /// </summary>
        private void FixGroupViewLavaTemplateDQ()
        {
            // Replaces the double-single-quote ''warning'' with just a single quote 'warning' in the GroupType's GroupViewLavaTemplate
            Sql( @"
                UPDATE [GroupType]
                SET [GroupViewLavaTemplate] = REPLACE(GroupViewLavaTemplate,'''''warning''''','''warning''')
                WHERE [GroupViewLavaTemplate] like '%warningLevel = ''''warning''''%'" );
        }

        /// <summary>
        /// DV: Adjust Login to the correct Log In verb form.
        /// </summary>
        private void AdjustLoginVerbUp()
        {
            Sql( @" 
                UPDATE [Page]
                SET [InternalName] = 'Log In'
                    , [PageTitle] = 'Log In'
                    , [BrowserTitle] = 'Log In'
                WHERE [Guid] IN ('03cb988a-138c-448b-a43d-8891844eeb18','d025e14c-f385-43fb-a735-abd24adc1480','31f51dbb-ac84-4724-9219-b46fadab9cb2') AND
                    [InternalName] = 'Login' AND
                    [PageTitle] = 'Login' AND
                    [BrowserTitle] = 'Login'" );
        }

        /// <summary>
        /// DV: Adjust Log In to the incorrect Login noun/adjective form.
        /// </summary>
        private void AdjustLoginVerbDown()
        {
            Sql( @" 
                UPDATE [Page]
                SET [InternalName] = 'Login'
                    , [PageTitle] = 'Login'
                    , [BrowserTitle] = 'Login'
                WHERE [Guid] IN ('03cb988a-138c-448b-a43d-8891844eeb18','d025e14c-f385-43fb-a735-abd24adc1480','31f51dbb-ac84-4724-9219-b46fadab9cb2') AND
                    [InternalName] = 'Log In' AND
                    [PageTitle] = 'Log In' AND
                    [BrowserTitle] = 'Log In'" );
        }

        /// <summary>
        /// MP: Electronic Signature Legacy Document Warning
        /// </summary>
        private void ElectronicSignatureLegacyDocumentWarning()
        {
            Sql( @"
                DECLARE @signNowEntityTypeId INT = (SELECT TOP 1 Id FROM EntityType WHERE [Name] LIKE 'Rock.SignNow.SignNow')
                DECLARE @providerIsActive BIT = (
                    SELECT count(*)
                    FROM AttributeValue
                    WHERE AttributeId IN (SELECT Id FROM Attribute WHERE EntityTypeId = @signNowEntityTypeId AND [Key] = 'Active' ) AND ValueAsBoolean = 1)

                DECLARE @usedbySignatureDocumentTemplate BIT = (SELECT count(*) FROM SignatureDocumentTemplate WHERE ProviderEntityTypeId = @signNowEntityTypeId )
                DECLARE @stillUsed BIT = @providerIsActive | @usedbySignatureDocumentTemplate
                DECLARE @warningHtml NVARCHAR(max) = '<div class=""alert alert-warning"">Support for signature document providers will be fully removed in the next full release.</div>';

                IF (@stillUsed = 0)
                BEGIN
                    -- Delete the External Signature Document Providers Block/Page
                    DELETE
                    FROM [Block]
                    WHERE [Guid] = '8690831c-d48a-48a7-bba7-5bc496e493f2'

                    DELETE
                    FROM [Page]
                    WHERE [Guid] = 'FAA6A2F2-4CFD-4B97-A0C2-8F4F9CE841F3'
                END
                ELSE
                BEGIN
                    -- Put a warning on the External Signature Document Providers Block/Page
                    UPDATE [Block]
                    SET PreHtml = @warningHtml
                    WHERE [Guid] = '8690831c-d48a-48a7-bba7-5bc496e493f2'
                END

                IF (@stillUsed = 1)
                BEGIN
                    -- If still using a Legacy Provider, set the 'ShowLegacyExternalProviders' block option to 'True' on the Signature Document Templates block
                    DECLARE @signatureTemplateBlockTypeId INT = (SELECT Id FROM BlockType WHERE [Guid] = '9f26a1da-74ae-4cb7-babc-6ae81a581a06')
                    DECLARE @signatureTemplateBlockId INT = (SELECT TOP 1 Id FROM [Block] WHERE [Guid] = 'a112ef63-77c5-4570-a52e-5b68bd27ea84')
                    DECLARE @showLegacyExternalProviderAttributeId INT = (
                            SELECT TOP 1 Id
                            FROM [Attribute]
                            WHERE [EntityTypeQualifierColumn] = 'BlockTypeId'
                                AND [EntityTypeQualifierValue] = @signatureTemplateBlockTypeId
                                AND [Key] = 'ShowLegacyExternalProviders')

                    DECLARE @showLegacyExternalProviderAttributeValueId INT = (
                            SELECT Id
                            FROM AttributeValue
                            WHERE EntityId = @signatureTemplateBlockId
                                AND AttributeId = @showLegacyExternalProviderAttributeId)

                    IF (@showLegacyExternalProviderAttributeValueId IS NOT NULL)
                    BEGIN
                        UPDATE [AttributeValue]
                        SET [Value] = 'True'
                        WHERE [Id] = @showLegacyExternalProviderAttributeValueId
                    END
                    ELSE
                    BEGIN
                        IF (@showLegacyExternalProviderAttributeId IS NOT NULL)
                        BEGIN
                            INSERT INTO [AttributeValue] (
                                  IsSystem
                                , AttributeId
                                , EntityId
                                , [Value]
                                , Guid)
                            VALUES (
                                  1
                                , @showLegacyExternalProviderAttributeId
                                , @signatureTemplateBlockId
                                , 'True'
                                , NEWID())
                        END
                    END
                END" );
        }

        /// <summary>
        /// GJ: Fix Group Scheduling V2 Lava
        /// </summary>
        private void AddAttributeToGroupScheduleToolboxV2BlockType()
        {
            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Sign Up Instructions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                "18A6DCE3-376C-4A62-B1DD-5E5177C11595",
                "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5",
                "Sign Up Instructions",
                "SignupInstructions",
                "Sign Up Instructions",
                @"Instructions here will show up on Sign Up tab. <span class='tip tip-lava'></span>",
                1,
                @"<div class=""alert alert-info"">
    {% if IsSchedulesAvailable %}
        {% if CurrentPerson.Id == Person.Id %}
            Sign up to attend a group and location on the given date.
        {% else %}
            Sign up {{ Person.FullName }} to attend a group and location on a given date.
        {% endif %}
    {% else %}
        No sign-ups available.
    {% endif %}
</div>",
                "03FA1A9E-98D0-4D24-AA02-83F7E0D76838" );
        }

        /// <summary>
        /// ED: Update Sample Data (Up)
        /// </summary>
        private void UpdateSampleDataUrlUp()
        {
            RockMigrationHelper.AddBlockAttributeValue( "34CA1FA0-F8F1-449F-9788-B5E6315DC058", "5E26439E-4E98-45B1-B19B-D5B2F3405963", @"http://storage.rockrms.com/sampledata/sampledata_1_14_0.xml" );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                "A42E0031-B2B9-403A-845B-9C968D7716A6",
                "9C204CD0-1233-41C5-818A-C5DA439445AA",
                "XML Document URL",
                "XMLDocumentURL",
                "XML Document URL",
                @"The URL for the input sample data XML document. You can also use a local Windows file path (e.g. C:\Rock\Documentation\sampledata_1_14_0.xml) if you want to test locally with your own fake data.  The file format is loosely defined on the <a target='blank' href='https://github.com/SparkDevNetwork/Rock/wiki/z.-Rock-Solid-Demo-Church-Specification-(sample-data)'>Rock Solid Demo Church Specification</a> wiki.",
                1,
                @"http://storage.rockrms.com/sampledata/sampledata_1_14_0.xml",
                "5E26439E-4E98-45B1-B19B-D5B2F3405963" );
        }

        /// <summary>
        /// ED: Update Sample Data (Down)
        /// </summary>
        private void UpdateSampleDataUrlDown()
        {
            RockMigrationHelper.AddBlockAttributeValue( "34CA1FA0-F8F1-449F-9788-B5E6315DC058", "5E26439E-4E98-45B1-B19B-D5B2F3405963", @"http://storage.rockrms.com/sampledata/sampledata_1_13_0.xml" );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                "A42E0031-B2B9-403A-845B-9C968D7716A6",
                "9C204CD0-1233-41C5-818A-C5DA439445AA",
                "XML Document URL",
                "XMLDocumentURL",
                "XML Document URL",
                @"The URL for the input sample data XML document. You can also use a local Windows file path (e.g. C:\Rock\Documentation\sampledata_1_13_0.xml) if you want to test locally with your own fake data.  The file format is loosely defined on the <a target='blank' href='https://github.com/SparkDevNetwork/Rock/wiki/z.-Rock-Solid-Demo-Church-Specification-(sample-data)'>Rock Solid Demo Church Specification</a> wiki.",
                1,
                @"http://storage.rockrms.com/sampledata/sampledata_1_13_0.xml",
                "5E26439E-4E98-45B1-B19B-D5B2F3405963" );
        }

        /// <summary>
        /// SK: Update Logic for DaysUntilBirthday Computed Column
        /// </summary>
        private void DaysUntilBirthdayColumn()
        {
            Sql( @"ALTER TABLE [dbo].[person] DROP COLUMN [daysuntilbirthday]" );
            Sql( @"ALTER TABLE [dbo].[Person] ADD [DaysUntilBirthday] AS (
                CASE
                    WHEN [BirthMonth]=(2) AND [BirthDay]=(29) AND datepart(month,sysdatetime())<(3)
                    THEN datediff(day,sysdatetime(),dateadd(day,-1,CONVERT([varchar](4),datepart(year,sysdatetime()))+'-03-01'))
                    WHEN [BirthMonth]=(2) AND [BirthDay]=(29) AND isdate(CONVERT([varchar](4),datepart(year,dateadd(year,(1),sysdatetime())))+'-02-29')=(0)
                    THEN datediff(day,sysdatetime(),CONVERT([date],CONVERT([varchar](4),datepart(year,dateadd(year,(1),sysdatetime())))+'-02-28'))
                ELSE
                    CASE
                        WHEN [BirthMonth]=(2) AND [BirthDay]<(30)
                        OR isdate((((CONVERT([varchar](4),datepart(year,sysdatetime()))+'-')+RIGHT('00'+CONVERT([varchar](2),[BirthMonth]),(2)))+'-')+RIGHT('00'+CONVERT([varchar](2),[BirthDay]),(2)))=(1)
                        THEN
                            CASE
                                WHEN (datepart(month,sysdatetime())*(100)+datepart(day,sysdatetime()))>([BirthMonth]*(100)+[BirthDay])
                                THEN datediff(day,sysdatetime(),CONVERT([date],(((CONVERT([varchar](4),datepart(year,dateadd(year,(1),sysdatetime())))+'-')+RIGHT('00'+CONVERT([varchar](2),[birthmonth]),(2)))+'-')+RIGHT('00'+CONVERT([varchar](2),[birthday]),(2))))
                                ELSE datediff(day,sysdatetime(),CONVERT([date],(((CONVERT([varchar](4),datepart(year,sysdatetime()))+'-')+RIGHT('00'+CONVERT([varchar](2),[birthmonth]),(2)))+'-')+RIGHT('00'+CONVERT([varchar](2),[birthday]),(2))))
                            END
                    END
                END)" );
        }

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        private void AddIpAddressLocationServicesPageBlocksUp()
        {
            // Add Page
            // Internal Name: IP Address Location Service
            // Site: Rock RMS
            RockMigrationHelper.AddPage( true, "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "IP Address Location Service", "", "9B6AE429-EFBE-47BF-B4D3-E4FB85D37B49", "fa fa-globe-americas" );

#pragma warning disable CS0618 // Type or member is obsolete
            // Add Page Route
            //  Page:IP Address Location Service
            // Route:admin/system/ip-location-services
            RockMigrationHelper.AddPageRoute( "9B6AE429-EFBE-47BF-B4D3-E4FB85D37B49", "admin/system/ip-location-services", "3D69707E-5F37-4F15-AFE8-903861DE8D90" );
#pragma warning restore CS0618 // Type or member is obsolete
            
            // Add Block
            // Block Name: Components
            // Page Name: IP Address Location Service
            // Layout: -
            // Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "9B6AE429-EFBE-47BF-B4D3-E4FB85D37B49".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "21F5F466-59BC-40B2-8D73-7314D936C3CB".AsGuid(), "Components", "Main", @"", @"", 0, "54805637-AB36-4F22-9F58-D49019D6335D" );
            
            // Add Block Attribute Value
            // Block: Components
            // BlockType: Components
            // Category: Core
            // Block Location: Page=IP Address Location Service, Site=Rock RMS
            // Attribute: Component Container
            // /*   Attribute Value: Rock.IpAddress.IpAddressLookupContainer, Rock */
            RockMigrationHelper.AddBlockAttributeValue( "54805637-AB36-4F22-9F58-D49019D6335D", "259AF14D-0214-4BE4-A7BF-40423EA07C99", @"Rock.IpAddress.IpAddressLookupContainer, Rock" );
            
            // Add Block Attribute Value
            // Block: Components
            // BlockType: Components
            // Category: Core
            // Block Location: Page=IP Address Location Service, Site=Rock RMS
            // Attribute: Support Ordering
            // /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "54805637-AB36-4F22-9F58-D49019D6335D", "A4889D7B-87AA-419D-846C-3E618E79D875", @"True" );
            
            // Add Block Attribute Value
            // Block: Components
            // BlockType: Components
            // Category: Core
            // Block Location: Page=IP Address Location Service, Site=Rock RMS
            // Attribute: Support Security
            // /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "54805637-AB36-4F22-9F58-D49019D6335D", "A8F1D1B8-0709-497C-9DCB-44826F26AE7A", @"True" );
            
            // Add Block Attribute Value
            // Block: Components
            // BlockType: Components
            // Category: Core
            // Block Location: Page=IP Address Location Service, Site=Rock RMS
            // Attribute: core.EnableDefaultWorkflowLauncher
            // /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "54805637-AB36-4F22-9F58-D49019D6335D", "C29E9E43-B246-4CBB-9A8A-274C8C377FDF", @"True" );
            
            // Add Block Attribute Value
            // Block: Components
            // BlockType: Components
            // Category: Core
            // Block Location: Page=IP Address Location Service, Site=Rock RMS
            // Attribute: core.CustomGridEnableStickyHeaders
            // /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "54805637-AB36-4F22-9F58-D49019D6335D", "9912A0F4-83FB-4447-BFFE-BCE713E1B885", @"False" );
            
            Sql( @"
                UPDATE [Site]
                SET [EnablePageViewGeoTracking] = 1
                WHERE [Guid] = 'F3F82256-2D66-432B-9D67-3552CD2F4C2B'" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        private void AddIpAddressLocationServicesPageBlocksDown()
        {
            // Remove Block
            // Name: Components, from Page: IP Address Location Service, Site: Rock RMS
            // from Page: IP Address Location Service, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("54805637-AB36-4F22-9F58-D49019D6335D");
            
            // Delete Page
            // Internal Name: IP Address Location Service
            // Site: Rock RMS
            // Layout: Full Width
            RockMigrationHelper.DeletePage("9B6AE429-EFBE-47BF-B4D3-E4FB85D37B49");  
            
            // Delete Page
            // Internal Name: Registration Instance - Wait List
            // Site: Rock RMS
            // Layout: Full Width
            RockMigrationHelper.DeletePage("E17883C2-6442-4AE5-B561-2C783F7F89C9");  
        }

        /// <summary>
        /// ED: Add Field Trip Release Signature Document
        /// </summary>
        private void AddFieldTripReleaseSignatureDocument()
        {
            Sql( @"
                DECLARE @Name nvarchar(100) = 'Field Trip Release (Event Registration)'
                DECLARE @Description nvarchar(max) = 'An example of a sample ''Field Trip Release'' form.'
                DECLARE @ProviderEntityTypeId int = null
                DECLARE @ProviderTemplateKey nvarchar(100) = ''
                DECLARE @BinaryFileTypeId int = (SELECT [Id] FROM [BinaryFileType] WHERE [Guid] = '40871411-4E2D-45C2-9E21-D9FCBA5FC340' ) -- Rock.SystemGuid.BinaryFiletype.DIGITALLY_SIGNED_DOCUMENTS)
                DECLARE @CreatedDateTime datetime = GETDATE()
                DECLARE @ModifiedDateTime datetime = GETDATE()
                DECLARE @Guid uniqueidentifier = 'ECAE06C7-17E1-44F9-AF7F-B691741C26C5'
                DECLARE @InviteSystemEmailId int = null
                DECLARE @InviteSystemCommunicationId int = null
                DECLARE @IsActive bit = 1
                DECLARE @DocumentTerm nvarchar(100) = 'Release'
                DECLARE @SignatureType int = 1 --Drawn = 0, Typed = 1
                DECLARE @CompletionSystemCommunicationId int = (SELECT [Id] FROM [SystemCommunication] WHERE [Guid] = '224A0E80-069B-463C-8187-E13682F8A550') -- Rock.SystemGuid.SystemCommunication.SYSTEM_ELECTRONIC_SIGNATURE_RECEIPT 
                DECLARE @LavaTemplate nvarchar(max) = '<html>
                    <head>
                        <link rel=""stylesheet"" href=""https://cdn.simplecss.org/simple.min.css""> 
                    </head>
                    <body>
                        <img src=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}{{ ''Global'' | Attribute:''EmailHeaderLogo'' }}""> 
        
                        <p>I, the undersigned, am the parent/guardian of {{ Registrant.FirstName }}.</p>
        
                        <p>I give my permission for my son/daughter to participate in the field trip.</p>
        
                        <p>I understand that although the students will be supervised by (faculty and staff), I do
                        assume the risk in my student''s participation in the event.</p>
        
                        <p>I acknowledge that I will not seek to have {{ ''Global'' | Attribute:''OrganizationName'' }} held liable in the event that
                        any accident, injury, loss of property or any other circumstance or incident occurs during or
                        as a result of my son''s/daughter''s participation in the field trip. This release of liability
                        includes accident, injury, loss, or damages to the student, as well as, to other individuals or
                        property which may result from the student''s participation in the event. I hereby release
                        and agree to hold harmless {{ ''Global'' | Attribute:''OrganizationName'' }}, its officials, agents and employees, from
                        any claims arising out of my son''s/daughter''s participation in the event(s).</p>
        
                        <p>I have read and understand and accept all of the statements recited above and accept full
                        responsibility as described.</p>
                    </body>
                </html>'

                -- If this already exists then don't insert or update
                IF EXISTS(SELECT [Id] FROM [SignatureDocumentTemplate] WHERE [Guid] = @Guid)
                BEGIN RETURN END

                INSERT INTO [SignatureDocumentTemplate] (
                      [Name]
                    , [Description]
                    , [ProviderEntityTypeId]
                    , [ProviderTemplateKey]
                    , [BinaryFileTypeId]
                    , [CreatedDateTime]
                    , [ModifiedDateTime]
                    , [Guid]
                    , [InviteSystemEmailId]
                    , [InviteSystemCommunicationId]
                    , [LavaTemplate]
                    , [IsActive]
                    , [DocumentTerm]
                    , [SignatureType]
                    , [CompletionSystemCommunicationId])
                VALUES (
                      @Name
                    , @Description
                    , @ProviderEntityTypeId
                    , @ProviderTemplateKey
                    , @BinaryFileTypeId
                    , @CreatedDateTime
                    , @ModifiedDateTime
                    , @Guid
                    , @InviteSystemEmailId
                    , @InviteSystemCommunicationId
                    , @LavaTemplate
                    , @IsActive
                    , @DocumentTerm
                    , @SignatureType
                    , @CompletionSystemCommunicationId)" );
        }

        /// <summary>
        /// CR: Remove references to MelissaData and ServiceObjects Location Services (Up)
        /// </summary>
        private void RemoveMelissaDataAndServiceObjectsUp()
        {
            string melissaDataGuid = "25590809-dfea-42b4-b556-67e311ac331a";
            string serviceObjectsGuid = "fdbe662a-51f8-436b-acec-890ee4cde124";

            // Remove Melissa Data attributes and entity type.
            RockMigrationHelper.DeleteAttributesByEntityType( melissaDataGuid );
            RockMigrationHelper.DeleteEntityType( melissaDataGuid );

            // Remove Service Objects attributes and entity type.
            RockMigrationHelper.DeleteAttributesByEntityType( serviceObjectsGuid );
            RockMigrationHelper.DeleteEntityType( serviceObjectsGuid );
        }

        /// <summary>
        /// CR: Remove references to MelissaData and ServiceObjects Location Services (Down)
        /// </summary>
        private void RemoveMelissaDataAndServiceObjectsDown()
        {
            var melissaDataGuid = "25590809-dfea-42b4-b556-67e311ac331a";
            var serviceObjectsGuid = "fdbe662a-51f8-436b-acec-890ee4cde124";

            RockMigrationHelper.UpdateEntityType( "Rock.Address.MelissaData", "Melissa Data", "Rock.Address.MelissaData, Rock, Version=1.11.3.1, Culture=neutral, PublicKeyToken=null", false, true, melissaDataGuid );
            RockMigrationHelper.UpdateEntityType( "Rock.Address.ServiceObjects", "Service Objects", "Rock.Address.ServiceObjects, Rock, Version=1.11.3.1, Culture=neutral, PublicKeyToken=null", false, true, serviceObjectsGuid );
        }

        /// <summary>
        /// SK: Allow Checkout At Kiosk as well as Manager Attribute for Check Type
        /// </summary>
        private void AllowCheckoutAtKiosk()
        {
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", Rock.SystemGuid.FieldType.BOOLEAN, "GroupTypePurposeValueId", "", "Allow Checkout At Kiosk", "", 0, "False", "F24D5B77-BCE3-4B55-9737-7F9C21F6854F", "core_checkin_AllowCheckout_Kiosk" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", Rock.SystemGuid.FieldType.BOOLEAN, "GroupTypePurposeValueId", "", "Allow Checkout in Manager", "", 0, "False", "0D93E822-281F-4676-A1C6-9A7BAB04315B", "core_checkin_AllowCheckout_Manager" );
            Sql( @"
                DECLARE @GroupTypeEntityTypeId int = ( SELECT TOP 1[Id] FROM[EntityType] WHERE[Name] = 'Rock.Model.GroupType' )
                DECLARE @CheckInTemplatePurposeId int = ( SELECT TOP 1[Id] FROM[DefinedValue] WHERE[Guid] = '4A406CB0-495B-4795-B788-52BDFDE00B01' )
                IF @GroupTypeEntityTypeId IS NOT NULL AND @CheckInTemplatePurposeId IS NOT NULL
                BEGIN
                    UPDATE[Attribute] SET[EntityTypeQualifierValue] = CAST( @CheckInTemplatePurposeId AS varchar )
                    WHERE[EntityTypeId] = @GroupTypeEntityTypeId
                    AND[EntityTypeQualifierColumn] = 'GroupTypePurposeValueId'
                    AND[Key] LIKE 'core_checkin_%'

                END
            " );

            Sql( @"
                DECLARE @GroupTypeEntityTypeId int = ( SELECT TOP 1[Id] FROM[EntityType] WHERE[Name] = 'Rock.Model.GroupType' )
                DECLARE @AllowCheckOutAttributeId int = ( SELECT TOP 1[Id] FROM [Attribute] WHERE [EntityTypeId]=@GroupTypeEntityTypeId AND [Key]='core_checkin_AllowCheckout' )
                DECLARE @AllowCheckOutKioskAttributeId int = ( SELECT TOP 1[Id] FROM [Attribute] WHERE [EntityTypeId]=@GroupTypeEntityTypeId AND [Key]='core_checkin_AllowCheckout_Kiosk' )
                DECLARE @AllowCheckOutManagerAttributeId int = ( SELECT TOP 1[Id] FROM [Attribute] WHERE [EntityTypeId]=@GroupTypeEntityTypeId AND [Key]='core_checkin_AllowCheckout_Manager' )
                IF @AllowCheckOutAttributeId IS NOT NULL 
                BEGIN
                    INSERT INTO AttributeValue ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
                    SELECT [IsSystem], @AllowCheckOutKioskAttributeId, [EntityId], [Value], NEWID()
                    FROM [AttributeValue]
                    WHERE (AttributeId = @AllowCheckOutAttributeId)

                    INSERT INTO AttributeValue ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
                    SELECT [IsSystem], @AllowCheckOutManagerAttributeId, [EntityId], [Value], NEWID()
                    FROM [AttributeValue]
                    WHERE (AttributeId = @AllowCheckOutAttributeId)
                END
            " );
        }

        /// <summary>
        /// SK: Add DisplayBirthdateOnAdults attribute for Check In Type
        /// </summary>
        private void DisplayBirthdateOnAdults()
        {
            Sql( $@"
                DECLARE @GroupTypeEntityId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.GroupType') -- Group Type Entity ID
                DECLARE @FieldTypeId int = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '7525c4cb-ee6b-41d4-9b64-a08048d5a5c0') -- Single Select
                DECLARE @EntityTypeQualifierValue int = 142

                IF NOT EXISTS (
                    SELECT [Id]
                    FROM [Attribute]
                    WHERE [EntityTypeId] = @GroupTypeEntityId
                        AND [EntityTypeQualifierColumn] = 'GroupTypePurposeValueId'
                        AND [EntityTypeQualifierValue] = @EntityTypeQualifierValue
                        AND [Key] = 'core_checkin_registration_DisplayBirthdateOnAdults' )
                BEGIN
                    INSERT INTO [Attribute] (
                            [IsSystem]
                        , [FieldTypeId]
                        , [EntityTypeId]
                        , [EntityTypeQualifierColumn]
                        , [EntityTypeQualifierValue]
                        , [Key]
                        , [Name]
                        , [Description]
                        , [Order]
                        , [IsGridColumn]
                        , [DefaultValue]
                        , [IsMultiValue]
                        , [IsRequired]
                        , [Guid])
                    VALUES (
                            1
                        , @FieldTypeId
                        , @GroupTypeEntityId
                        , 'GroupTypePurposeValueId'
                        , @EntityTypeQualifierValue
                        , 'core_checkin_registration_DisplayBirthdateOnAdults'
                        , 'Display Birthdate On Adults'
                        , 'Determines whether the Check-In Registration Birthdate should be displayed for adults.'
                        , 0
                        , 0
                        , 'Hide'
                        , 0
                        , 0
                        , '3BE4B544-C219-44E4-B934-96A28D9B2EA7')
                END" );

            // Insert the attribute qualifiers.
            RockMigrationHelper.AddAttributeQualifier( "3BE4B544-C219-44E4-B934-96A28D9B2EA7", "fieldtype", "ddl", "E50E2DB6-292E-4BBF-AA4F-71D23DE23BB9" );
            RockMigrationHelper.AddAttributeQualifier( "3BE4B544-C219-44E4-B934-96A28D9B2EA7", "values", "Hide,Optional,Required", "1D50F4C8-FB68-41F9-9C5E-DDA612A5A2FA" );
        }

        /// <summary>
        /// CR: Add StepFlow page, route, and block (dependent on v14 block) (Down)
        /// </summary>
        private void AddOrUpdateStepFlowPageAndRouteDown()
        {
            // Remove Block - Name: Step Flow, from Page: Step Flow, Site: Rock RMS - from Page: Step Flow, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "A40684E9-10DA-4CF8-815B-EBDE53624419" );
            
            // Delete Page Internal Name: Step Flow Site: Rock RMS Layout: Full Width
            RockMigrationHelper.DeletePage( SystemGuid.Page.STEP_FLOW );
        }

        /// <summary>
        /// GJ: Badge Migrations
        /// </summary>
        private void BadgeMigrations()
        {
            Sql( @"
-- Warning: Note the subtle ending character difference between these next few Guids
DECLARE @BaptismBadgeId int =          ( SELECT TOP 1 [Id] FROM [Badge] WHERE [Guid] = '66972bff-42cd-49ab-9a7a-e1b9deca4ebe' )
DECLARE @ConnectionStatusBadgeId int = ( SELECT TOP 1 [Id] FROM [Badge] WHERE [Guid] = '66972bff-42cd-49ab-9a7a-e1b9deca4ebf' )
DECLARE @RecordStatusBadgeId int =     ( SELECT TOP 1 [Id] FROM [Badge] WHERE [Guid] = '66972bff-42cd-49ab-9a7a-e1b9deca4eca' )
DECLARE @EraBadgeId int =              ( SELECT TOP 1 [Id] FROM [Badge] WHERE [Guid] = '7fc986b9-ca1e-cbb7-4e63-c79eac34f39d' )
DECLARE @GroupRequirementsBadgeId int =( SELECT TOP 1 [Id] FROM [Badge] WHERE [Guid] = '132f9c2a-0af4-4ad9-87ef-7730b284e10e' )

-- This is a well known attribute: Lava Badge Component's 'DisplayText' key
DECLARE @LavaBadgeDisplayTextAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '01C9BA59-D8D4-4137-90A6-B3C06C70BBC3' )

-- Now we'll update these five badges as long as they have a matching CHECKSUM (meaning they have not be edited/modified by the organization).

-- Baptism Badge
UPDATE [AttributeValue] 
SET 
   [Value]=N'{% assign baptismDate = Person | Attribute:''BaptismDate'' %}
{% if baptismDate != '''' -%}
    <div class=""rockbadge rockbadge-icon rockbadge-baptism"" data-toggle=""tooltip"" data-original-title=""{{ Person.NickName }} was baptized on {{ baptismDate }}."">
<i class=""badge-icon fa fa-tint""></i>
    </div>
{% else -%}
    <div class=""rockbadge rockbadge-icon rockbadge-baptism rockbadge-disabled"" data-toggle=""tooltip"" data-original-title=""No baptism date entered for {{ Person.NickName }}."">
        <i class=""badge-icon fa fa-tint""></i>
    </div>
{% endif -%}' 
WHERE AttributeId = @LavaBadgeDisplayTextAttributeId AND EntityId = @BaptismBadgeId AND CHECKSUM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE([Value], ' ', '*^'), '^*', ''), '*^', ' '), CHAR(10), ''), CHAR(13), ''), CHAR(9), '')) = -572717666

-- Connection Status Badge
UPDATE [AttributeValue]
SET 
   [Value]=N'{% if Person.ConnectionStatusValue.Value != empty -%}
<div class=""rockbadge rockbadge-label"">
        <span class=""label label-success"">{{ Person.ConnectionStatusValue.Value }}</span> 
</div>
{% endif -%}' 
WHERE AttributeId = @LavaBadgeDisplayTextAttributeId AND EntityId = @ConnectionStatusBadgeId AND CHECKSUM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE([Value], ' ', '*^'), '^*', ''), '*^', ' '), CHAR(10), ''), CHAR(13), ''), CHAR(9), '')) = 1740721257

-- Record Status Badge
UPDATE [AttributeValue] 
SET
   [Value]=N'{% if Person.RecordStatusValue.Value != empty and Person.RecordStatusValue.Value == ""Inactive"" -%}
    <div class=""rockbadge rockbadge-label"">
        <span class=""label label-danger"" title=""{{ Person.RecordStatusReasonValue.Value }}"" data-toggle=""tooltip"">{{ Person.RecordStatusValue.Value }}</span>
    </div>
{% elseif Person.RecordStatusValue.Value != empty and Person.RecordStatusValue.Value == ""Pending"" -%}
    <div class=""rockbadge rockbadge-label"">
        <span class=""label label-warning"" title=""{{ Person.RecordStatusReasonValue.Value }}"" data-toggle=""tooltip"">{{ Person.RecordStatusValue.Value }}</span>
    </div>
{% endif -%}' 
WHERE AttributeId = @LavaBadgeDisplayTextAttributeId AND EntityId = @RecordStatusBadgeId AND CHECKSUM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE([Value], ' ', '*^'), '^*', ''), '*^', ' '), CHAR(10), ''), CHAR(13), ''), CHAR(9), '')) = -1338005962

-- ERA Badge
UPDATE [AttributeValue]
SET
   [Value]=N'{% assign isEra = Person | Attribute:''core_CurrentlyAnEra'' %}
{% if isEra == ''Yes'' %}

<div class=""rockbadge rockbadge-standard rockbadge-era"" data-toggle=""tooltip"" data-original-title=""{{ Person.NickName}} became an eRA on {{ Person | Attribute:''core_EraStartDate''}}"">
    <span class=""metric-value"">eRA</span>
</div>

{% else %}
    {% assign eraEndDate = Person | Attribute:''core_EraEndDate'' %}
    
    {% if eraEndDate != '''' %}
        {% assign todayDate = ''Now'' | Date:''M/d/yyyy'' %}
        {% assign daysSinceEnd = eraEndDate | DateDiff:todayDate,''d'' %}
        
        {% if daysSinceEnd <= 30 %}
            <div class=""rockbadge rockbadge-standard rockbadge-era era-loss"" data-toggle=""tooltip"" data-original-title=""{{ Person.NickName}} lost eRA status {{ daysSinceEnd }} days ago"">
                <span class=""metric-value"">eRA</span>
            </div>
        {% endif %}
    {% endif %}
{% endif %}' 
WHERE AttributeId = @LavaBadgeDisplayTextAttributeId AND EntityId = @EraBadgeId AND CHECKSUM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE([Value], ' ', '*^'), '^*', ''), '*^', ' '), CHAR(10), ''), CHAR(13), ''), CHAR(9), '')) = 1934384809

-- Group Requirement Badge
UPDATE [AttributeValue] 
SET 
   [Value]=N'{% assign groupHasRequirements = Entity.GroupType.GroupRequirements | Size | AsBoolean %}
{% assign typeHasRequirements = Entity.GroupRequirements | Size | AsBoolean %}

{% if groupHasRequirements or typeHasRequirements -%}
    <div class=""rockbadge rockbadge-icon"" data-toggle=""tooltip"" data-original-title=""Group has requirements."" style=""color:var(--brand-success);"">
        <i class=""badge-icon fa fa-tasks""></i>
    </div>
{% endif -%}' 
WHERE AttributeId = @LavaBadgeDisplayTextAttributeId AND EntityId = @GroupRequirementsBadgeId AND CHECKSUM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE([Value], ' ', '*^'), '^*', ''), '*^', ' '), CHAR(10), ''), CHAR(13), ''), CHAR(9), '')) = -1807913642
" );
        }
    }
}
