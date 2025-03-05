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
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Text;

    using Rock.Migrations.Migrations;
    using Rock.Model;
    using Rock.Plugin.HotFixes;
    using Rock.Security;
    using Rock.SystemGuid;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20241218 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            #region MigrationRollupsForV17_0_0 UP

            var cmsConfigurationPageGuid = "B4A24AB7-9369-4055-883F-4F4892C39AE3";
            var fullWidthLayoutGuid = "D65F783D-87A9-4CC9-8110-E83466A0EADB";
            var pagelistBlockGuid = "BEDFF750-3EB8-4EE7-A8B4-23863FB0315D";

            // Add the Website Configurations Section
            RockMigrationHelper.AddPage( true, cmsConfigurationPageGuid, fullWidthLayoutGuid, "Website Configuration", "Configuration features, designed to simplify the process of managing and fine-tuning your website.", "CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89" );

            // Move the Pages to the Website Configuration Section
            RockMigrationHelper.MovePage( "7596D389-4EAB-4535-8BEE-229737F46F44", "CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89" ); // Sites Page
            RockMigrationHelper.MovePage( "EC7A06CD-AAB5-4455-962E-B4043EA2440E", "CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89" ); // Pages Page
            RockMigrationHelper.MovePage( "6F074DAA-BDCC-44C5-BB89-B899C1AAC6C1", "CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89" ); // File Manager Page
            RockMigrationHelper.MovePage( "4A833BE3-7D5E-4C38-AF60-5706260015EA", "CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89" ); // Routes Page
            RockMigrationHelper.MovePage( "5FBE9019-862A-41C6-ACDC-287D7934757D", "CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89" ); // Block Types Page
            RockMigrationHelper.MovePage( "BC2AFAEF-712C-4173-895E-81347F6B0B1C", "CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89" ); // Themes Page
            RockMigrationHelper.MovePage( "39F928A5-1374-4380-B807-EADF145F18A1", "CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89" ); // HTTP Modules Page

            // Add the Content Channels Section
            RockMigrationHelper.AddPage( true, cmsConfigurationPageGuid, fullWidthLayoutGuid, "Content Channels", "In this section, you'll find tools to effectively configure and manage all aspects of your content channels.", "889D7F7F-EB0F-40CD-9E80-E58A00EE69F7" );

            // Move the Pages to the Content Channels Section
            RockMigrationHelper.MovePage( "8ADCE4B2-8E95-4FA3-89C4-06A883E8145E", "889D7F7F-EB0F-40CD-9E80-E58A00EE69F7" ); // Content Channels Page
            RockMigrationHelper.MovePage( "37E3D602-5D7D-4818-BCAA-C67EBB301E55", "889D7F7F-EB0F-40CD-9E80-E58A00EE69F7" ); // Content Channel Types Page
            RockMigrationHelper.MovePage( "40875E7E-B912-43FF-892B-6161C21F130B", "889D7F7F-EB0F-40CD-9E80-E58A00EE69F7" ); // Content Collections Page
            RockMigrationHelper.MovePage( "F1ED10C2-A17D-4310-9F86-76E11A4A7ED2", "889D7F7F-EB0F-40CD-9E80-E58A00EE69F7" ); // Content Component Templates Page
            RockMigrationHelper.MovePage( "BBDE39C3-01C9-4C9E-9506-C2205508BC77", "889D7F7F-EB0F-40CD-9E80-E58A00EE69F7" ); // Content Channel Item Attribute Categories Page
            RockMigrationHelper.MovePage( "0F1B45B8-032D-4306-834D-670FA3933589", "889D7F7F-EB0F-40CD-9E80-E58A00EE69F7" ); // Content Channel Categories Page

            // Add the Personalization Section
            RockMigrationHelper.AddPage( true, cmsConfigurationPageGuid, fullWidthLayoutGuid, "Personalization", "Personalization features designed to tailor content and interactions to each individual's unique preferences and needs.", "B892DF6D-4789-4AC3-9E6C-2BFE0D9E30E4" );

            // Move the Pages to the Personalization Section
            RockMigrationHelper.MovePage( "905F6132-AE1C-4C85-9752-18D22E604C3A", "B892DF6D-4789-4AC3-9E6C-2BFE0D9E30E4" ); // Personalization Segments Page
            RockMigrationHelper.MovePage( "511FC29A-EAF2-4AC0-B9B3-8613739A9ACF", "B892DF6D-4789-4AC3-9E6C-2BFE0D9E30E4" ); // Request Filters Page

            // Add the Content Platform Section
            RockMigrationHelper.AddPage( true, cmsConfigurationPageGuid, fullWidthLayoutGuid, "Content Platform", "Tools designed for creating, managing, and seamlessly distributing digital content.", "04FE297E-D45E-44EC-B521-181423F05A1C" );

            // Move the Pages to the Content Platform Section
            RockMigrationHelper.MovePage( "6CFF2C81-6303-4477-A7EC-156DDBF8BE64", "04FE297E-D45E-44EC-B521-181423F05A1C" ); // Lava Shortcodes Page
            RockMigrationHelper.MovePage( "07CB7BB5-1465-4E75-8DD4-28FA6EA48222", "04FE297E-D45E-44EC-B521-181423F05A1C" ); // Media Accounts Page
            RockMigrationHelper.MovePage( "37C20B91-737B-42D1-907D-9868104DBA7B", "04FE297E-D45E-44EC-B521-181423F05A1C" ); // Persisted Datasets Page
            RockMigrationHelper.MovePage( "8C0114FF-31CF-443E-9278-3F9E6087140C", "04FE297E-D45E-44EC-B521-181423F05A1C" ); // Short Links Page
            RockMigrationHelper.MovePage( "C206A96E-6926-4EB9-A30F-E5FCE559D180", "04FE297E-D45E-44EC-B521-181423F05A1C" ); // Shared Links Page
            RockMigrationHelper.MovePage( "4B8691C7-537F-4B6E-9ED1-E3BA3FA0051E", "04FE297E-D45E-44EC-B521-181423F05A1C" ); // Cache Manager Page
            RockMigrationHelper.MovePage( "D2B919E2-3725-438F-8A86-AC87F81A72EB", "04FE297E-D45E-44EC-B521-181423F05A1C" ); // Asset Manager Page
            RockMigrationHelper.MovePage( "706C0584-285F-4014-BA61-EC42C8F6F76B", "04FE297E-D45E-44EC-B521-181423F05A1C" ); // Control Gallery Page
            RockMigrationHelper.MovePage( "BB2AF2B3-6D06-48C6-9895-EDF2BA254533", "04FE297E-D45E-44EC-B521-181423F05A1C" ); // Font Awesome Settings Page

            // Add the Digital Media Applications Section
            RockMigrationHelper.AddPage( true, cmsConfigurationPageGuid, fullWidthLayoutGuid, "Digital Media Applications", "Suite of tools and platforms designed to enhance digital content creation, distribution, and engagement.", "82726ACD-3480-4514-A920-FE920A71C046" );

            // Move the Pages to the Digital Media Applications Section
            RockMigrationHelper.MovePage( "784259EC-46B7-4DE3-AC37-E8BFDB0B90A6", "82726ACD-3480-4514-A920-FE920A71C046" ); // Mobile Applications Page
            RockMigrationHelper.MovePage( "C8B81EBE-E98F-43EF-9E39-0491685145E2", "82726ACD-3480-4514-A920-FE920A71C046" ); // Apple TV Apps Page

            // Update the CMS Page List Attributes
            RockMigrationHelper.AddBlockAttributeValue( pagelistBlockGuid, "1322186A-862A-4CF1-B349-28ECB67229BA", "{% include '~~/Assets/Lava/PageListAsBlocksSections.lava' %}" );
            RockMigrationHelper.AddBlockAttributeValue( pagelistBlockGuid, "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"2" );

            #endregion

            #region MigrationRollupsForV17_0_1

            AddAdaptiveMessageRelatedPagesUp();
            RemoveLavaEngineFrameworkGlobalAttribute_Up();
            RemoveLavaEngineWarningBlockInstance();
            AddRawHtmlLavaStrucuturedContentToolUp();
            SortCMSPagesUp();

            #endregion

            #region MigrationRollupsForV17_0_2

            UpdateIndexesOnInteraction();
            ChopLegacyCommunicationRecipientList();

            #endregion

            #region MigratoinRollupForV17_0_3

            RemoveLegacyPreferences();
            AddUpdatePersistedDatasetServiceUp();

            #endregion

            #region MigrationRollupForV17_0_4

            UpdateFavIcon();

            #endregion

            #region MigrationRollupsForV17_0_5

            Sql( $@"
DECLARE @ServiceJobEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.ServiceJob' )
-- Get the Attribute Id For Service Job by the key
DECLARE @AttributeId int
SET @AttributeId = (
    SELECT [Id]
    FROM [Attribute]
    WHERE [EntityTypeId] = @ServiceJobEntityTypeId
        AND [EntityTypeQualifierColumn] = 'Class'
        AND [EntityTypeQualifierValue] = 'Rock.Jobs.PostUpdateDataMigrationsReplaceWebFormsBlocksWithObsidianBlocks'
        AND [Key] = 'BlockAttributeKeysToIgnore' );

DECLARE @ChopCommunicationRecipientListBlockServiceJobId int = (SELECT [Id] FROM [ServiceJob] WHERE [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_170_REMOVE_COMMUNICATION_RECIPIENT_LIST_BLOCK}');
IF NOT EXISTS ( SELECT * FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @ChopCommunicationRecipientListBlockServiceJobId ) AND @ChopCommunicationRecipientListBlockServiceJobId IS NOT NULL
BEGIN
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
VALUES (1, @AttributeId, @ChopCommunicationRecipientListBlockServiceJobId, N'EBEA5996-5695-4A42-A21C-29E11E711BE8^ContextEntityType,DetailPage,core.CustomGridColumnsConfig,core.CustomActionsConfigs,core.EnableDefaultWorkflowLauncher,core.CustomGridEnableStickyHeaders', NEWID());
END" );

            #endregion

            #region MigrationRollupsForV17_0_6

            FixRouteForAdaptiveMessageUp();

            #endregion

            #region MigrationRollupsForV17_0_7

            CreateAdministrativeSettingsPage();
            MoveSettingsPages();
            TithingOverviewMetricsUp();
            TithingOverviewPageAndBlockUp();

            #endregion

            #region AddMaxMindGeolocation

            RenameObservabilityHttpModuleUp();
            RefactorPopulateInteractionSessionDataJobUp();
            RemoveIpAddressServiceLocationPageUp();

            #endregion

            #region MigrationRollupsForV17_0_8

            AddWarningMessagesForActiveObsoletedBlock();
            ChopDISCBlock();
            UpdateCmsLayout();

            #endregion

            #region MigrationRollupsForV17_0_9

            NewWeeklyMetricsAndMeasurementClassificationDefinedValuesUp();
            AddLocationHistoryCategoryUp();
            UpdateTithingMetricsUp();
            AddLmsDataUp();

            #endregion

            #region MigrationRollupsForV17_0_10

            ReorganizeAdminSettingsPages();
            AddRouteNameForTithingOverviewUp();
            UpdateGivingMetricSqlUp();
            PeerNetworkRelationshipTypeForFollowingUp();
            LMSUp();
            ChopShortenedLinkBlockUp();

            #endregion

            #region MigrationRollupsForV17_0_11

            UpdateProgramCompletionsJobUp();

            #endregion

            #region MigrationRollupsForV17_0_12

            AIProvidersUp();
            UpdateERAMetricScheduleUp();
            UpdateGivingAnalyticsAndFamilyAnalyticsAttendanceProceduresUp();
            LMSUpdatesUp();
            PrayerAutomationUp();

            #endregion

            #region MigrationRollupsForV17_0_13

            UpdateDigitalToolsPageSettingsUp();

            #endregion

            #region MigrationRollupsForV17_0_14

            UpdateHistoryTableIndexUp();
            PrayerAutomationCompletionsUp();
            StandardizePageShortlinkNamesUp();
            ChopBlocksUp();

            #endregion

            #region MigrationRollupsForV17_0_15

            MigrationRollupsForV17_0_15_PrayerAutomationCompletionsUp();
            UpdateGivingHouseholdsMetricsUp();
            AddExceptionLogFilterGlobalAttributeUp();
            AddModalLayoutUp();
            MigrationRollupsForV17_0_15_ChopBlocksUp();

            #endregion

            #region MigrationRollupsForV17_0_16

            UpdateStarkListAndStarkDetailBlockNameUp();
            AddObsidianControlGalleryToCMSUp();
            //MigrationRollupsForV17_0_16_SwapBlocksUp();
            MigrationRollupsForV17_0_16_ChopBlocksUp();

            #endregion

            #region LMSPageChanges

            // Removes legacy values that are no longer used or have been repurposed.
            // This needs to run before the RockMigrationHelper method calls
            // to prevent potential conflicts with Attribute Keys.
            RemoveLegacyValues();

            // Add Page 
            //  Internal Name: Program Summary Pages
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "84DBEC51-EE0B-41C2-94B3-F361C4B98879", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Program Summary Pages", "Container page for the Sub Menu of the LMS Program Summary.", "7ECE252B-6844-474C-AEFD-307E1DDA3A83", "" );

            // Add Page 
            //  Internal Name: Program Detail Pages
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "84DBEC51-EE0B-41C2-94B3-F361C4B98879", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Program Detail Pages", "Container page for the Sub Menu of the LMS Program Detail.", "6BDF3243-72BA-4C08-80BD-B76E40667A33", "" );

            // Add Page 
            //  Internal Name: Current Classes
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "7ECE252B-6844-474C-AEFD-307E1DDA3A83", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Current Classes", "", "56459D93-32DF-4151-8F6D-003B9AFF0F94", "" );

            // Add Page 
            //  Internal Name: Completions
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "7ECE252B-6844-474C-AEFD-307E1DDA3A83", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Completions", "LMS Program Completions", "395BE5DD-E524-4B75-A4CA-5A0548645647", "" );

            // Add Page 
            //  Internal Name: Courses
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "6BDF3243-72BA-4C08-80BD-B76E40667A33", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Courses", "", "0E5103B8-EF4A-46C9-8F76-313A259B0A3C", "" );

            // Add Page 
            //  Internal Name: Semesters
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "6BDF3243-72BA-4C08-80BD-B76E40667A33", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Semesters", "", "0D89CFE6-BA23-4AC0-AF95-1016BAEF734C", "" );

            // Add Page Route
            //   Page:Current Classes
            //   Route:learning/{LearningProgramId}/summary/classes
            RockMigrationHelper.AddOrUpdatePageRoute( "56459D93-32DF-4151-8F6D-003B9AFF0F94", "learning/{LearningProgramId}/classes", "56A1387A-DDDE-46D9-A23D-B19D6A3BFC50" );

            // Add Page Route
            //   Page:Completions
            //   Route:learning/{LearningProgramId}/summary/completions
            RockMigrationHelper.AddOrUpdatePageRoute( "395BE5DD-E524-4B75-A4CA-5A0548645647", "learning/{LearningProgramId}/completions", "1AA1F901-A07C-4F64-A8EF-70A4160C0F22" );

            // Add Page Route
            //   Page:Courses
            //   Route:learning/{LearningProgramId}/summary/courses
            RockMigrationHelper.AddOrUpdatePageRoute( "0E5103B8-EF4A-46C9-8F76-313A259B0A3C", "learning/{LearningProgramId}/courses", "5208B1E5-BE28-44D0-9DE2-D2B1A26471AE" );

            // Add Page Route
            //   Page:Semesters
            //   Route:learning/{LearningProgramId}/summary/semesters
            RockMigrationHelper.AddOrUpdatePageRoute( "0D89CFE6-BA23-4AC0-AF95-1016BAEF734C", "learning/{LearningProgramId}/semesters", "E0F8F5D7-99C7-4FC3-9205-D9100E1F1027" );

            // Add Block 
            //  Block Name: Current Classes
            //  Page Name: Current Classes
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "56459D93-32DF-4151-8F6D-003B9AFF0F94".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "340F6CC1-8C38-4579-9383-A6168680194A".AsGuid(), "Current Classes", "Main", @"", @"", 2, "41645AA0-1899-4B08-9833-B61B23BB7294" );

            // Add Block 
            //  Block Name: Learning Program Detail
            //  Page Name: Current Classes
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "56459D93-32DF-4151-8F6D-003B9AFF0F94".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "796C87E7-678F-4A38-8C04-A401A4F7AC21".AsGuid(), "Learning Program Detail", "Main", @"", @"", 0, "CBD3BE2F-1BF1-4ADF-99A5-774D4C4E4770" );

            // Add Block 
            //  Block Name: Page Menu
            //  Page Name: Current Classes
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "56459D93-32DF-4151-8F6D-003B9AFF0F94".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Page Menu", "Main", @"", @"", 1, "89AE19CD-E801-4685-9F4B-FE2D9AC00BD6" );

            // Add Block 
            //  Block Name: Page Menu
            //  Page Name: Completions
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "395BE5DD-E524-4B75-A4CA-5A0548645647".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Page Menu", "Main", @"", @"", 1, "CB9CC864-1958-4A2A-8424-42A028134006" );

            // Add Block 
            //  Block Name: Learning Program Completion List
            //  Page Name: Completions
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "395BE5DD-E524-4B75-A4CA-5A0548645647".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CE703EB6-028F-47FC-A09A-AD8F72C12CBC".AsGuid(), "Learning Program Completion List", "Main", @"", @"", 2, "319F2F80-A12C-48E8-B5A1-434C1BCF0AD2" );

            // Add Block 
            //  Block Name: Learning Program Detail
            //  Page Name: Completions
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "395BE5DD-E524-4B75-A4CA-5A0548645647".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "796C87E7-678F-4A38-8C04-A401A4F7AC21".AsGuid(), "Learning Program Detail", "Main", @"", @"", 0, "28F72594-912E-4C73-9E89-DF3C4D93863B" );

            // Add Block 
            //  Block Name: Learning Program Detail
            //  Page Name: Courses
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0E5103B8-EF4A-46C9-8F76-313A259B0A3C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "796C87E7-678F-4A38-8C04-A401A4F7AC21".AsGuid(), "Learning Program Detail", "Main", @"", @"", 0, "AB20591D-C843-4099-966D-D54101793288" );

            // Add Block 
            //  Block Name: Learning Course List
            //  Page Name: Courses
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0E5103B8-EF4A-46C9-8F76-313A259B0A3C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D0AFDF98-4AFC-4E4F-A6E2-07CA4E7358E8".AsGuid(), "Learning Course List", "Main", @"", @"", 2, "53830D4A-32F3-4543-8DF2-8E46046BBF4E" );

            // Add Block 
            //  Block Name: Page Menu
            //  Page Name: Courses
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0E5103B8-EF4A-46C9-8F76-313A259B0A3C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Page Menu", "Main", @"", @"", 1, "B3C55400-76E9-42D9-9ECA-842FBFC7C123" );

            // Add Block 
            //  Block Name: Page Menu
            //  Page Name: Semesters
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0D89CFE6-BA23-4AC0-AF95-1016BAEF734C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Page Menu", "Main", @"", @"", 1, "F8273D25-960A-4081-BCBF-2E45433C398C" );

            // Add Block 
            //  Block Name: Learning Program Detail
            //  Page Name: Semesters
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0D89CFE6-BA23-4AC0-AF95-1016BAEF734C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "796C87E7-678F-4A38-8C04-A401A4F7AC21".AsGuid(), "Learning Program Detail", "Main", @"", @"", 0, "539CFC03-C265-4D3F-BE11-B592E3969969" );

            // Add Block 
            //  Block Name: Learning Semester List
            //  Page Name: Semesters
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0D89CFE6-BA23-4AC0-AF95-1016BAEF734C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "C89C7F15-FB8A-43D4-9AFB-5E40E397F246".AsGuid(), "Learning Semester List", "Main", @"", @"", 2, "E8076C21-D1A3-4D6E-A4AB-0352462DDA74" );

            // update block order for pages with new blocks if the page,zone has multiple blocks

            // Update Order for Page: Completions,  Zone: Main,  Block: Learning Program Completion List
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '319F2F80-A12C-48E8-B5A1-434C1BCF0AD2'" );

            // Update Order for Page: Completions,  Zone: Main,  Block: Learning Program Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '28F72594-912E-4C73-9E89-DF3C4D93863B'" );

            // Update Order for Page: Completions,  Zone: Main,  Block: Page Menu
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'CB9CC864-1958-4A2A-8424-42A028134006'" );

            // Update Order for Page: Courses,  Zone: Main,  Block: Learning Course List
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '53830D4A-32F3-4543-8DF2-8E46046BBF4E'" );

            // Update Order for Page: Courses,  Zone: Main,  Block: Learning Program Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'AB20591D-C843-4099-966D-D54101793288'" );

            // Update Order for Page: Courses,  Zone: Main,  Block: Page Menu
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'B3C55400-76E9-42D9-9ECA-842FBFC7C123'" );

            // Update Order for Page: Current Classes,  Zone: Main,  Block: Current Classes
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '41645AA0-1899-4B08-9833-B61B23BB7294'" );

            // Update Order for Page: Current Classes,  Zone: Main,  Block: Learning Program Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'CBD3BE2F-1BF1-4ADF-99A5-774D4C4E4770'" );

            // Update Order for Page: Current Classes,  Zone: Main,  Block: Page Menu
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '89AE19CD-E801-4685-9F4B-FE2D9AC00BD6'" );

            // Update Order for Page: Semesters,  Zone: Main,  Block: Learning Program Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '539CFC03-C265-4D3F-BE11-B592E3969969'" );

            // Update Order for Page: Semesters,  Zone: Main,  Block: Learning Semester List
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = 'E8076C21-D1A3-4D6E-A4AB-0352462DDA74'" );

            // Update Order for Page: Semesters,  Zone: Main,  Block: Page Menu
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'F8273D25-960A-4081-BCBF-2E45433C398C'" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "796C87E7-678F-4A38-8C04-A401A4F7AC21", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the Detail view for the learning program (if Display Mode is 'Summary').", 4, @"", "06B0D94D-7A16-4E4E-A53A-743EE89804D3" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Display Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "796C87E7-678F-4A38-8C04-A401A4F7AC21", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Mode", "DisplayMode", "Display Mode", @"Select 'Summary' to show the summary page with (optional) KPIs and the gear icon that navigates to the traditional 'Detail' view.", 3, @"Summary", "18731EB1-888B-4AB2-B1C1-759493B2E639" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Attribute Display Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "796C87E7-678F-4A38-8C04-A401A4F7AC21", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Attribute Display Mode", "AttributeDisplayMode", "Attribute Display Mode", @"Select 'Is Grid Column' to show only attributes that are 'Show on Grid'. Select 'All' to show all attributes.", 3, @"Is Grid Column", "85AD9ED7-6AC2-4392-B448-7FD377D3DE9F" );

            // Add Block Attribute Value
            //   Block: Learning Program List
            //   BlockType: Learning Program List
            //   Category: LMS
            //   Block Location: Page=Learning, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "B7D8BC2F-CEDD-4E22-8D46-69A759DCB59C", "9CB472E3-755B-4702-ADC7-06FF6FF6B598", @"False" );

            // Value: 56459d93-32df-4151-8f6d-003b9aff0f94,56a1387a-ddde-46d9-a23d-b19d6a3bfc50

            // Add Block Attribute Value
            //   Block: Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Course, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0", "A0B96A3E-E7CB-45A7-898F-C798CD8FFF9D", @"False" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Attribute Display Mode
            /*   Attribute Value: Is Grid Column */
            RockMigrationHelper.AddBlockAttributeValue( "CBD3BE2F-1BF1-4ADF-99A5-774D4C4E4770", "85AD9ED7-6AC2-4392-B448-7FD377D3DE9F", @"Is Grid Column" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Show KPIs
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "CBD3BE2F-1BF1-4ADF-99A5-774D4C4E4770", "6AE52A7E-EFA3-4685-B331-A2D3058438D3", @"True" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 0e5103b8-ef4a-46c9-8f76-313a259b0a3c,5208b1e5-be28-44d0-9de2-d2b1a26471ae */
            RockMigrationHelper.AddBlockAttributeValue( "CBD3BE2F-1BF1-4ADF-99A5-774D4C4E4770", "06B0D94D-7A16-4E4E-A53A-743EE89804D3", @"0e5103b8-ef4a-46c9-8f76-313a259b0a3c,5208b1e5-be28-44d0-9de2-d2b1a26471ae" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Display Mode
            /*   Attribute Value: Summary */
            RockMigrationHelper.AddBlockAttributeValue( "CBD3BE2F-1BF1-4ADF-99A5-774D4C4E4770", "18731EB1-888B-4AB2-B1C1-759493B2E639", @"Summary" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Is Secondary Block
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "89AE19CD-E801-4685-9F4B-FE2D9AC00BD6", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Include Current Parameters
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "89AE19CD-E801-4685-9F4B-FE2D9AC00BD6", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"True" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Template
            /*   Attribute Value: {% include '~~/Assets/Lava/PageListAsTabs.lava' %} */
            RockMigrationHelper.AddBlockAttributeValue( "89AE19CD-E801-4685-9F4B-FE2D9AC00BD6", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsTabs.lava' %}" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Root Page
            /*   Attribute Value: 7ece252b-6844-474c-aefd-307e1dda3a83 */
            RockMigrationHelper.AddBlockAttributeValue( "89AE19CD-E801-4685-9F4B-FE2D9AC00BD6", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"7ece252b-6844-474c-aefd-307e1dda3a83" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Number of Levels
            /*   Attribute Value: 1 */
            RockMigrationHelper.AddBlockAttributeValue( "89AE19CD-E801-4685-9F4B-FE2D9AC00BD6", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Include Current QueryString
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "89AE19CD-E801-4685-9F4B-FE2D9AC00BD6", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Show Location Column
            /*   Attribute Value: No */
            RockMigrationHelper.AddBlockAttributeValue( "41645AA0-1899-4B08-9833-B61B23BB7294", "4FBE92ED-3CF1-4BD4-A78E-B50C0080FA0B", @"No" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Show Schedule Column
            /*   Attribute Value: No */
            RockMigrationHelper.AddBlockAttributeValue( "41645AA0-1899-4B08-9833-B61B23BB7294", "BECBD2AE-5D1E-4065-8CAA-FE7972CC5742", @"No" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Show Semester Column
            /*   Attribute Value: No */
            RockMigrationHelper.AddBlockAttributeValue( "41645AA0-1899-4B08-9833-B61B23BB7294", "28087DBE-A334-47BF-9CA5-76AFF5EE1A6F", @"No" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 23d5076e-c062-4987-9985-b3a4792bf3ce,5fce29a7-4530-4cce-9891-c95242923efe */
            RockMigrationHelper.AddBlockAttributeValue( "41645AA0-1899-4B08-9833-B61B23BB7294", "0C995F98-F483-4814-B3A1-6FACCD2D686F", @"23d5076e-c062-4987-9985-b3a4792bf3ce,5fce29a7-4530-4cce-9891-c95242923efe" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "41645AA0-1899-4B08-9833-B61B23BB7294", "4778450C-5B2A-40AD-9FC0-B3ED8AAE2F5C", @"True" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "41645AA0-1899-4B08-9833-B61B23BB7294", "A0B96A3E-E7CB-45A7-898F-C798CD8FFF9D", @"False" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Attribute Display Mode
            /*   Attribute Value: Is Grid Column */
            RockMigrationHelper.AddBlockAttributeValue( "28F72594-912E-4C73-9E89-DF3C4D93863B", "85AD9ED7-6AC2-4392-B448-7FD377D3DE9F", @"Is Grid Column" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 0e5103b8-ef4a-46c9-8f76-313a259b0a3c,5208b1e5-be28-44d0-9de2-d2b1a26471ae */
            RockMigrationHelper.AddBlockAttributeValue( "28F72594-912E-4C73-9E89-DF3C4D93863B", "06B0D94D-7A16-4E4E-A53A-743EE89804D3", @"0e5103b8-ef4a-46c9-8f76-313a259b0a3c,5208b1e5-be28-44d0-9de2-d2b1a26471ae" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Show KPIs
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "28F72594-912E-4C73-9E89-DF3C4D93863B", "6AE52A7E-EFA3-4685-B331-A2D3058438D3", @"True" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Display Mode
            /*   Attribute Value: Summary */
            RockMigrationHelper.AddBlockAttributeValue( "28F72594-912E-4C73-9E89-DF3C4D93863B", "18731EB1-888B-4AB2-B1C1-759493B2E639", @"Summary" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Is Secondary Block
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "CB9CC864-1958-4A2A-8424-42A028134006", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Include Current QueryString
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "CB9CC864-1958-4A2A-8424-42A028134006", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Template
            /*   Attribute Value: {% include '~~/Assets/Lava/PageListAsTabs.lava' %} */
            RockMigrationHelper.AddBlockAttributeValue( "CB9CC864-1958-4A2A-8424-42A028134006", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsTabs.lava' %}" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Number of Levels
            /*   Attribute Value: 1 */
            RockMigrationHelper.AddBlockAttributeValue( "CB9CC864-1958-4A2A-8424-42A028134006", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Root Page
            /*   Attribute Value: 7ece252b-6844-474c-aefd-307e1dda3a83 */
            RockMigrationHelper.AddBlockAttributeValue( "CB9CC864-1958-4A2A-8424-42A028134006", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"7ece252b-6844-474c-aefd-307e1dda3a83" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Include Current Parameters
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "CB9CC864-1958-4A2A-8424-42A028134006", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"True" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Display Mode
            /*   Attribute Value: Detail */
            RockMigrationHelper.AddBlockAttributeValue( "AB20591D-C843-4099-966D-D54101793288", "18731EB1-888B-4AB2-B1C1-759493B2E639", @"Detail" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Show KPIs
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "AB20591D-C843-4099-966D-D54101793288", "6AE52A7E-EFA3-4685-B331-A2D3058438D3", @"False" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Attribute Display Mode
            /*   Attribute Value: Is Grid Column */
            RockMigrationHelper.AddBlockAttributeValue( "AB20591D-C843-4099-966D-D54101793288", "85AD9ED7-6AC2-4392-B448-7FD377D3DE9F", @"Is Grid Column" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Is Secondary Block
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "B3C55400-76E9-42D9-9ECA-842FBFC7C123", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Include Current Parameters
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "B3C55400-76E9-42D9-9ECA-842FBFC7C123", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"True" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Root Page
            /*   Attribute Value: 6bdf3243-72ba-4c08-80bd-b76e40667a33 */
            RockMigrationHelper.AddBlockAttributeValue( "B3C55400-76E9-42D9-9ECA-842FBFC7C123", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"6bdf3243-72ba-4c08-80bd-b76e40667a33" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Number of Levels
            /*   Attribute Value: 1 */
            RockMigrationHelper.AddBlockAttributeValue( "B3C55400-76E9-42D9-9ECA-842FBFC7C123", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Template
            /*   Attribute Value: {% include '~~/Assets/Lava/PageListAsTabs.lava' %} */
            RockMigrationHelper.AddBlockAttributeValue( "B3C55400-76E9-42D9-9ECA-842FBFC7C123", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsTabs.lava' %}" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Include Current QueryString
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "B3C55400-76E9-42D9-9ECA-842FBFC7C123", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Add Block Attribute Value
            //   Block: Learning Course List
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: a57d990e-6f34-45cf-abaa-08c40e8d4844,c77ebcb8-f174-4f2d-8113-48d9b0d489ea */
            RockMigrationHelper.AddBlockAttributeValue( "53830D4A-32F3-4543-8DF2-8E46046BBF4E", "67E7F552-C0F2-4852-B817-E216795D1E30", @"a57d990e-6f34-45cf-abaa-08c40e8d4844,c77ebcb8-f174-4f2d-8113-48d9b0d489ea" );

            // Add Block Attribute Value
            //   Block: Learning Course List
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "53830D4A-32F3-4543-8DF2-8E46046BBF4E", "223232AD-21C4-4024-ADE2-B9180B165728", @"True" );

            // Add Block Attribute Value
            //   Block: Learning Course List
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "53830D4A-32F3-4543-8DF2-8E46046BBF4E", "184900A4-881E-4EA1-B047-D865A4B948AF", @"False" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Attribute Display Mode
            /*   Attribute Value: Is Grid Column */
            RockMigrationHelper.AddBlockAttributeValue( "539CFC03-C265-4D3F-BE11-B592E3969969", "85AD9ED7-6AC2-4392-B448-7FD377D3DE9F", @"Is Grid Column" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Display Mode
            /*   Attribute Value: Detail */
            RockMigrationHelper.AddBlockAttributeValue( "539CFC03-C265-4D3F-BE11-B592E3969969", "18731EB1-888B-4AB2-B1C1-759493B2E639", @"Detail" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Show KPIs
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "539CFC03-C265-4D3F-BE11-B592E3969969", "6AE52A7E-EFA3-4685-B331-A2D3058438D3", @"False" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Is Secondary Block
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "F8273D25-960A-4081-BCBF-2E45433C398C", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Include Current QueryString
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "F8273D25-960A-4081-BCBF-2E45433C398C", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Root Page
            /*   Attribute Value: 6bdf3243-72ba-4c08-80bd-b76e40667a33 */
            RockMigrationHelper.AddBlockAttributeValue( "F8273D25-960A-4081-BCBF-2E45433C398C", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"6bdf3243-72ba-4c08-80bd-b76e40667a33" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Number of Levels
            /*   Attribute Value: 1 */
            RockMigrationHelper.AddBlockAttributeValue( "F8273D25-960A-4081-BCBF-2E45433C398C", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Include Current Parameters
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "F8273D25-960A-4081-BCBF-2E45433C398C", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"True" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Template
            /*   Attribute Value: {% include '~~/Assets/Lava/PageListAsTabs.lava' %} */
            RockMigrationHelper.AddBlockAttributeValue( "F8273D25-960A-4081-BCBF-2E45433C398C", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsTabs.lava' %}" );

            // Add Block Attribute Value
            //   Block: Learning Semester List
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "E8076C21-D1A3-4D6E-A4AB-0352462DDA74", "29621F9F-24AE-4698-B719-D5D3FA1A6C91", @"False" );

            // Add Block Attribute Value
            //   Block: Learning Semester List
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 36ffa805-b283-443e-990d-87040339d16f,d796b863-964f-4a10-a880-9d398376851a */
            RockMigrationHelper.AddBlockAttributeValue( "E8076C21-D1A3-4D6E-A4AB-0352462DDA74", "EEA24DC4-4CFC-4B34-9348-917839DDBBA2", @"36ffa805-b283-443e-990d-87040339d16f,d796b863-964f-4a10-a880-9d398376851a" );

            // Add Block Attribute Value
            //   Block: Learning Semester List
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "E8076C21-D1A3-4D6E-A4AB-0352462DDA74", "57106CC3-5D67-40BA-809B-2EA9826478D8", @"True" );

            AttributeValuesUp();

            #endregion

            #region MigrationRollupsForV17_0_17

            UpdateGoogleMapsLavaShortcodeUp();
            UpdateMetricsDateComponentUp();
            UpdateChartShortCodeLava();
            MigrationRollupsForV17_0_17_ChopBlocksUp();

            #endregion

            #region PrayerCategoryPageBlockSettings

            PrayerCategoryPageBlockSettings_PrayerAutomationCompletionsUp();
            CategoryDetailAttributes();

            #endregion

            #region CategoryTreeViewBlockSettings

            UpdateCategoryTreeViewBlockSettings();

            #endregion

            #region UpdateGradingScaleDescriptions

            UpdateGradingScaleDescriptionsUp();
            UpdateCourseAndClassDetailPageMenuMargin();

            #endregion

            #region MigrationRollupsForV17_0_18

            MakeMapIdForGoogleMapsShortCodeOptionalUp();
            UpdateDetailsPageBlockAttributeKeyForFundraisingListWebformBLock();
            MigrationRollupsForV17_0_18_ChopBlocksUp();

            #endregion

            #region LMSRoutes

            UpdateRoutesAndBinaryFileType();

            #endregion

            #region DefaultGradeScaleHighlightColors

            RemovePublicLavaTemplateAttributeValues();
            SetGradingColors();
            SetFacilitatorPortalPage();

            // Update the layout for the Public Class Workspace to use the Full Width
            // layout so that breadcrumbs are shown.
            var publicClassWorkspacePageGuid = "61BE63C7-6611-4235-A6F2-B22456620F35";
            RockMigrationHelper.UpdatePageLayout( publicClassWorkspacePageGuid, SystemGuid.Layout.FULL_WIDTH );

            #endregion

            #region MigrationRollupsForV17_0_19

            UpdateFinishLavaTemplateUp();
            MigrationRollupsForV17_0_19_UpdateGoogleMapsLavaShortcodeUp();
            UnhideVolunteerGenerosityUp();
            UpdateFollowIconLavaShortcodeUp();
            MigrationRollupsForV17_0_19_ChopBlocksUp();

            #endregion

            #region UndoObsidianChop

            UndoObsidianChop_SwapBlockUp();

            #endregion

            #region CompletionGradingSystem

            Sql( @"
DECLARE @now DATETIMEOFFSET = SYSDATETIMEOFFSET();
DECLARE @completeGradingSystemGuid NVARCHAR(40) = '48A4EFCD-6B59-481F-9E09-0C06193A77FC';
DECLARE @completeGradingSystemId INT = (SELECT [Id] FROM [dbo].[LearningGradingSystem] WHERE [Guid] = @completeGradingSystemGuid);

IF @completeGradingSystemId IS NULL
BEGIN
	INSERT [dbo].[LearningGradingSystem] (
		  [Name]
		, [Description]
		, [IsActive]
		, [CreatedDateTime]
		, [ModifiedDateTime]
		, [Guid]
	)
	VALUES ('Completion', 'The Completion grading system focuses solely on whether an activity has been completed. Any completion is considered passing.', 1, @now, @now, @completeGradingSystemGuid)
	SET @completeGradingSystemId = SCOPE_IDENTITY();
END

DECLARE @completeGradingSystemScaleGuid NVARCHAR(40) = '9D17F412-25A8-4638-9CD7-5343017620D6';

IF NOT EXISTS (SELECT 1 FROM [dbo].[LearningGradingSystemScale] WHERE [Guid] = @completeGradingSystemScaleGuid)
BEGIN
	INSERT [dbo].[LearningGradingSystemScale] (
		  [Name]
		, [Description]
		, [ThresholdPercentage]
		, [IsPassing]
		, [Order]
		, [LearningGradingSystemId]
        , [HighlightColor] 
		, [CreatedDateTime]
		, [ModifiedDateTime]
		, [Guid]
	)
	VALUES ('', 'Has been completed.', 0, 1, 1, @completeGradingSystemId, '#34D87D', @now, @now, @completeGradingSystemScaleGuid)
END
" );

            #endregion

            #region PeerNetworkPageAndBlocks

            AddPeerNetworkPageAndBlocksUp();

            #endregion

            #region LMSSecurity

            AddLMSRoleAccess();

            #endregion

            #region ClassAttendancePage

            AddLMSEntitySecurity();
            AddAttendancePages();

            #endregion

            #region MigrationRollupsForV16_8_0

            UpdateVolGenBuildScript_CampusName_Up();
            FixFinancialStatementTemplateUp();
            FixGoogleMapsLavaShortcodeUp();

            #endregion

            #region ImprovePeerNetworkCalculations

            ImprovePeerNetworkCalculationsUp();

            #endregion

            #region MigrationRollupsForV16_8_1

            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_168_UPDATE_INDEXES}'
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
                    , 'Rock Update Helper v16.8 - Update Indexes.'
                    , 'This job updates indexes for general database performance.'
                    , 'Rock.Jobs.PostV168UpdateIndexes'
                    , '0 0 21 1/1 * ? *'
                    , 1
                    , '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_168_UPDATE_INDEXES}'
                );
            END" );

            #endregion

            #region AddEnableDefaultAddressStateSelectionSetting

            RockMigrationHelper.UpdateSystemSetting( SystemKey.SystemSetting.ENABLE_DEFAULT_ADDRESS_STATE_SELECTION, true.ToString() );

            #endregion

            #region SetConnectionTypeLinkUrl

            Sql( $@"
                UPDATE  [EntityType]
                SET     [LinkUrlLavaTemplate] = '~/people/connections/types/{{{{ Entity.Id }}}}'
                WHERE   [Guid] = '{Rock.SystemGuid.EntityType.CONNECTION_TYPE}'"
);

            #endregion

            #region PeerNetworkFitNFinish

            PeerNetworkFitNFinishUp();

            #endregion

            #region NA: Add Kids and Students Classifications and Metrics

            AddNewMeasurementClassificationDefinedValuesAndMetrics();

            #endregion

            #region KA: Update MergeField and AddressFormat Defined Value Attrribute Description

            // Check-In Label => MergeField Attribute
            Sql( @"
UPDATE [Attribute] 
SET [Description] = 'The .Fluid syntax for accessing values from the check-in state object'
WHERE [Guid] = '51eb8583-55ea-4431-8b66-b5bd0f83d81e'
" );

            // Country => AddressFormat Attribute
            Sql( @"
UPDATE [Attribute] 
SET [Description] = 'The Fluid syntax to use for formatting addresses'
WHERE [Guid] = 'b6ef4138-c488-4043-a628-d35f91503843'
" );

            #endregion

            #region KH: Update Account Entry Block Attributes

            UpdateAccountEntryBlockAttribute();

            #endregion

            #region SK: Google Map Short Code Update to remove all the JavaScript warning console

            UpdateGoogleMapShortcode_Up();

            #endregion

            #region DH: Update Saved Configuration Defined Type to Saved Kiosk Template

            SavedKioskTemplateDefinedTypeUp();

            #endregion

            #region SK: Update Adaptive Message List Page

            UpdateAdaptiveMessageListPageUp();

            #endregion

            #region JC: Learning Hub Image

            UpdateLearningHubDefaultImage();
            SetDefaultRoleTypeForLMS();
            UpdateParentEntityTypeForAIAutomationAttibutes();

            #endregion
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            #region KA: Update MergeField and AddressFormat Defined Value Attrribute Description

            // Check-In Label => MergeField Attribute
            Sql( @"
UPDATE [Attribute] 
SET [Description] = 'The .Liquid syntax for accessing values from the check-in state object'
WHERE [Guid] = '51eb8583-55ea-4431-8b66-b5bd0f83d81e'
" );

            // Country => AddressFormat Attribute
            Sql( @"
UPDATE [Attribute] 
SET [Description] = 'The Liquid syntax to use for formatting addresses'
WHERE [Guid] = 'b6ef4138-c488-4043-a628-d35f91503843'
" );

            #endregion

            #region DH: Update Saved Configuration Defined Type to Saved Kiosk Template

            SavedKioskTemplateDefinedTypeDown();

            #endregion
        }

        #region EF Migration With Plugin Migration Dependencies
        private void UpdateLearningHubDefaultImage()
        {
            var fileData = BitConverter.ToString( RockMigrationSQL.lms_header_min ).Replace( "-", "" );

            Sql( $@"
-- Add the Banner Image for the External Learn Page.
DECLARE @binaryFileGuid NVARCHAR(40) = '605FD4B7-2DCA-4782-8826-95AAC6C6BAB6';
DECLARE @now DATETIMEOFFSET = SYSDATETIMEOFFSET();

UPDATE [BinaryFile] SET
        [FileName] = 'lms-header-min.jpg',
        [Description] = 'The default image for the Learning Hub.',
        [MimeType] = 'image/png',
        [FileSize] = 193735,
        [Width] = 1140,
        [Height] = 300,
        [ModifiedDateTime] = @now,
        [ContentLastModified] = @now
WHERE [Guid] = @binaryFileGuid

DECLARE @binaryFileId INT = (SELECT [Id] FROM [dbo].[BinaryFile] WHERE [Guid] = @binaryFileGuid);
DECLARE @binaryFileDataGuid NVARCHAR(40) = '85FAE5A9-C28D-41FB-B5AA-5B5BB0499B3C';

DELETE d
from BinaryFile f
join BinaryFileData d on d.Id = f.Id
where f.[Guid] = @binaryFileGuid

INSERT [BinaryFileData] ( [Id], [Guid], [Content], [CreatedDateTime], [ModifiedDateTime] )
SELECT @binaryFileId [Id],  @binaryFileDataGuid [Guid], 0x{fileData} [Content], @now [CreatedDateTime], @now [ModifiedDateTime]
WHERE @binaryFileId IS NOT NULL
" );
        }

        private void SetDefaultRoleTypeForLMS()
        {
            Sql( @"
DECLARE @studentRoleId INT = (
    SELECT Id 
    FROM GroupTypeRole 
    WHERE [Guid] = 'FA3ACAC2-0377-484C-B888-974CA3BF2FF2'
);

UPDATE [dbo].[GroupType] SET
	[DefaultGroupRoleId] = @studentRoleId
WHERE [Guid] = '4BBC41E2-0A37-4289-B7A7-756B9FE8F775'

" );
        }

        private void UpdateParentEntityTypeForAIAutomationAttibutes()
        {
            Sql( @"
DECLARE @categoryEntityTypeId NVARCHAR(400) = (
	SELECT Id
	FROM [dbo].[EntityType]
	WHERE [Guid] = '1D68154E-EC76-44C8-9813-7736B27AECF9'
);

UPDATE [Category] SET
	EntityTypeQualifierColumn = 'EntityTypeId',
	EntityTypeQualifierValue = @categoryEntityTypeId
WHERE [Guid] = '571B1191-7F6A-4C9F-8953-1C5B14274F3F'
" );
        }

        #endregion

        #region MigrationRollupsForV17_0_1

        /// <summary>
        /// SK: 2) Add Adaptive Message related Pages
        /// </summary>
        private void AddAdaptiveMessageRelatedPagesUp()
        {
            RockMigrationHelper.AddPage( true, "B892DF6D-4789-4AC3-9E6C-2BFE0D9E30E4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Adaptive Message Attributes", "", "222ED9E3-06C0-438F-B520-C899B8835650", "fa fa-list-ul" );
            RockMigrationHelper.AddPage( true, "B892DF6D-4789-4AC3-9E6C-2BFE0D9E30E4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Adaptive Messages", "", "73112D38-E051-4452-AEF9-E473EEDD0BCB", "fa fa-comment" );
            RockMigrationHelper.AddPage( true, "73112D38-E051-4452-AEF9-E473EEDD0BCB", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Adaptive Message Detail", "", "BEC30E90-0434-43C4-B839-09E11775E497", "" );
            RockMigrationHelper.AddPage( true, "BEC30E90-0434-43C4-B839-09E11775E497", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Adaptive Message Adaptation Detail", "", "FE12A90C-C20F-4F23-A1B1-528E0C5FDA83", "" );

            // Failed when converted from Plugin Migration to EF Migration. Fixed with AddOrUpdatePageRoute Method.
            //RockMigrationHelper.AddPageRoute( "222ED9E3-06C0-438F-B520-C899B8835650", "admin/cms/adaptive-message/attributes", "E612018C-FD4B-4F6F-9BCD-3B76B58CC8AB" );

            RockMigrationHelper.AddOrUpdatePageRoute( "222ED9E3-06C0-438F-B520-C899B8835650", "admin/cms/adaptive-message/attributes", "E612018C-FD4B-4F6F-9BCD-3B76B58CC8AB" );

            RockMigrationHelper.AddBlock( true, "222ED9E3-06C0-438F-B520-C899B8835650".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "791DB49B-58A4-44E1-AEF5-ABFF2F37E197".AsGuid(), "Attributes", "Main", @"", @"", 0, "F1233621-77CC-4CBE-AE21-9221B2EC4034" );
            RockMigrationHelper.AddBlock( true, "73112D38-E051-4452-AEF9-E473EEDD0BCB".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CBA57502-8C9A-4414-B0D4-DB0D57EF89BD".AsGuid(), "Adaptive Message List", "Main", @"", @"", 0, "2DBFA85E-BA20-4FF2-8372-80688C8B9CD1" );
            RockMigrationHelper.AddBlock( true, "BEC30E90-0434-43C4-B839-09E11775E497".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "A81FE4E0-DF9F-4978-83A7-EB5459F37938".AsGuid(), "Adaptive Message Detail", "Main", @"", @"", 0, "8535DA65-D941-4ECE-9FAD-AA887361DF0E" );
            RockMigrationHelper.AddBlock( true, "FE12A90C-C20F-4F23-A1B1-528E0C5FDA83".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "113C4223-19B9-46F2-AAE8-AC646BC5A3C7".AsGuid(), "Adaptive Message Adaptation Detail", "Main", @"", @"", 0, "39A91338-89BC-4149-B642-C39732247A9E" );
            RockMigrationHelper.AddBlockAttributeValue( "F1233621-77CC-4CBE-AE21-9221B2EC4034", "9434F17F-F28C-4CEF-B65A-1A42CB7A17DC", @"39753cce-184a-4f14-ae80-08241de8fc2e" );
            RockMigrationHelper.AddBlockAttributeValue( "F1233621-77CC-4CBE-AE21-9221B2EC4034", "B921CB38-9D1F-4717-B4F9-D370BB8B3219", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "F1233621-77CC-4CBE-AE21-9221B2EC4034", "9AE7890A-1ABD-409C-9F3F-26D8497EC8EA", @"0" );
            RockMigrationHelper.AddBlockAttributeValue( "F1233621-77CC-4CBE-AE21-9221B2EC4034", "F09CAD11-1232-4F12-8875-BC22BA2A7693", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "2DBFA85E-BA20-4FF2-8372-80688C8B9CD1", "0AC7BE6D-22FB-4A4F-9188-C1FAB8AC1EC1", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "2DBFA85E-BA20-4FF2-8372-80688C8B9CD1", "F9E1AD87-CA8A-49DF-84AC-8F18895FCB87", @"bec30e90-0434-43c4-b839-09e11775e497" );
            RockMigrationHelper.AddBlockAttributeValue( "2DBFA85E-BA20-4FF2-8372-80688C8B9CD1", "A46D73FB-6BEF-492B-97B5-4A0351CC8286", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "8535DA65-D941-4ECE-9FAD-AA887361DF0E", "E4F286BC-9338-4E17-ABFF-4578793B7A54", @"fe12a90c-c20f-4f23-a1b1-528e0c5fda83" );
        }

        /// <summary>
        /// DL: Data Migration to Remove Lava Engine Global Attribute
        /// </summary>
        private void RemoveLavaEngineFrameworkGlobalAttribute_Up()
        {
            Sql( @"
            DELETE FROM [AttributeValue] WHERE [AttributeId] IN ( SELECT [Id] FROM [Attribute] WHERE [key] = 'core_LavaEngine_LiquidFramework' );
        " );

            Sql( @"
            DELETE FROM [AttributeQualifier] WHERE [AttributeId] IN ( SELECT [Id] FROM [Attribute] WHERE [key] = 'core_LavaEngine_LiquidFramework' );
        " );

            Sql( @"
            DELETE FROM [Attribute] WHERE [key] = 'core_LavaEngine_LiquidFramework';
        " );
        }

        /// <summary>
        /// KA: Remove Lava Engine Warning Block Instance
        /// </summary>
        private void RemoveLavaEngineWarningBlockInstance()
        {
            RockMigrationHelper.DeleteSecurityAuthForBlock( "CAEC8719-BEDF-4BE3-9847-55DE93624974" );
            RockMigrationHelper.DeleteBlock( "CAEC8719-BEDF-4BE3-9847-55DE93624974" );
        }

        /// <summary>
        /// DH: Add Raw structured editor tool.
        /// </summary>
        private void AddRawHtmlLavaStrucuturedContentToolUp()
        {
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.STRUCTURED_CONTENT_EDITOR_TOOLS,
                "Default",
                @"{
    header: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Header,
        inlineToolbar: ['link'
        ],
        config: {
            placeholder: 'Header'
        },
        shortcut: 'CMD+SHIFT+H'
    },
    image: {
        class: Rock.UI.StructuredContentEditor.EditorTools.RockImage,
        inlineToolbar: ['link'
        ],
    },
    list: {
        class: Rock.UI.StructuredContentEditor.EditorTools.NestedList,
        inlineToolbar: true,
        shortcut: 'CMD+SHIFT+L'
    },
    checklist: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Checklist,
        inlineToolbar: true,
    },
    quote: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Quote,
        inlineToolbar: true,
        config: {
            quotePlaceholder: 'Enter a quote',
            captionPlaceholder: 'Quote\'s author',
        },
        shortcut: 'CMD+SHIFT+O'
    },
    warning: Rock.UI.StructuredContentEditor.EditorTools.Warning,
    marker: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Marker,
        shortcut: 'CMD+SHIFT+M'
    },
    code: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Code,
        shortcut: 'CMD+SHIFT+C'
    },
    raw: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Raw
    },
    delimiter: Rock.UI.StructuredContentEditor.EditorTools.Delimiter,
    inlineCode: {
        class: Rock.UI.StructuredContentEditor.EditorTools.InlineCode,
        shortcut: 'CMD+SHIFT+C'
    },
    embed: Rock.UI.StructuredContentEditor.EditorTools.Embed,
    table: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Table,
        config: {
            defaultHeadings: true
        },
        inlineToolbar: true,
        shortcut: 'CMD+ALT+T'
    }
}",
                SystemGuid.DefinedValue.STRUCTURE_CONTENT_EDITOR_DEFAULT );
        }

        /// <summary>
        /// PA: Sort CMS Config Pages
        /// </summary>
        private void SortCMSPagesUp()
        {
            // Remove page display options for Section Pages in CMS Config
            var cmsSectionPageGuids = new string[] {
                "\'CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89\'", // Website Configurations Section Page
                "\'889D7F7F-EB0F-40CD-9E80-E58A00EE69F7\'", // Content Channels Section Page
                "\'B892DF6D-4789-4AC3-9E6C-2BFE0D9E30E4\'", // Personalization Section Page
                "\'04FE297E-D45E-44EC-B521-181423F05A1C\'", // Content Platform Section Page
                "\'82726ACD-3480-4514-A920-FE920A71C046\'" // Digital Media Applications Section Page
            }.JoinStrings( ", " );

            Sql( $@"UPDATE [dbo].[Page]
                    SET [PageDisplayBreadCrumb] = 0,
                        [PageDisplayDescription] = 0,
                        [PageDisplayIcon] = 0,
                        [PageDisplayTitle] = 0,
                        [BreadCrumbDisplayName] = 0
                WHERE [Guid] IN ( 
                    {cmsSectionPageGuids}
                )" );

            // Set the order of the pages within the Sections
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 1 WHERE [Guid] = '7596D389-4EAB-4535-8BEE-229737F46F44'" ); // Sites Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 2 WHERE [Guid] = 'EC7A06CD-AAB5-4455-962E-B4043EA2440E'" ); // Pages Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 3 WHERE [Guid] = '6F074DAA-BDCC-44C5-BB89-B899C1AAC6C1'" ); // File Manager Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 4 WHERE [Guid] = '4A833BE3-7D5E-4C38-AF60-5706260015EA'" ); // Routes Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 5 WHERE [Guid] = '5FBE9019-862A-41C6-ACDC-287D7934757D'" ); // Block Types Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 6 WHERE [Guid] = 'BC2AFAEF-712C-4173-895E-81347F6B0B1C'" ); // Themes Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 7 WHERE [Guid] = '39F928A5-1374-4380-B807-EADF145F18A1'" ); // HTTP Modules Page

            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 1 WHERE [Guid] = '8ADCE4B2-8E95-4FA3-89C4-06A883E8145E'" ); // Content Channels Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 2 WHERE [Guid] = '37E3D602-5D7D-4818-BCAA-C67EBB301E55'" ); // Content Channel Types Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 3 WHERE [Guid] = '40875E7E-B912-43FF-892B-6161C21F130B'" ); // Content Collections Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 4 WHERE [Guid] = 'F1ED10C2-A17D-4310-9F86-76E11A4A7ED2'" ); // Content Component Templates Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 5 WHERE [Guid] = 'BBDE39C3-01C9-4C9E-9506-C2205508BC77'" ); // Content Channel Item Attribute Categories Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 6 WHERE [Guid] = '0F1B45B8-032D-4306-834D-670FA3933589'" ); // Content Channel Categories Page

            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 1 WHERE [Guid] = '905F6132-AE1C-4C85-9752-18D22E604C3A'" ); // Personalization Segments Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 2 WHERE [Guid] = '511FC29A-EAF2-4AC0-B9B3-8613739A9ACF'" ); // Request Filters Page

            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 1 WHERE [Guid] = '6CFF2C81-6303-4477-A7EC-156DDBF8BE64'" ); // Lava Shortcodes Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 2 WHERE [Guid] = '07CB7BB5-1465-4E75-8DD4-28FA6EA48222'" ); // Media Accounts Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 3 WHERE [Guid] = '37C20B91-737B-42D1-907D-9868104DBA7B'" ); // Persisted Datasets Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 4 WHERE [Guid] = '8C0114FF-31CF-443E-9278-3F9E6087140C'" ); // Short Links Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 5 WHERE [Guid] = 'C206A96E-6926-4EB9-A30F-E5FCE559D180'" ); // Shared Links Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 6 WHERE [Guid] = '4B8691C7-537F-4B6E-9ED1-E3BA3FA0051E'" ); // Cache Manager Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 7 WHERE [Guid] = 'D2B919E2-3725-438F-8A86-AC87F81A72EB'" ); // Asset Manager Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 8 WHERE [Guid] = '706C0584-285F-4014-BA61-EC42C8F6F76B'" ); // Control Gallery Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 9 WHERE [Guid] = 'BB2AF2B3-6D06-48C6-9895-EDF2BA254533'" ); // Font Awesome Settings Page

            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 1 WHERE [Guid] = '784259EC-46B7-4DE3-AC37-E8BFDB0B90A6'" ); // Mobile Applications Page
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 2 WHERE [Guid] = 'C8B81EBE-E98F-43EF-9E39-0491685145E2'" ); // Apple TV Apps Page

        }

        #endregion

        #region MigrationRollupsForV17_0_2

        /// <summary>
        /// PA: Updated indexes on the Interaction table.
        /// </summary>
        private void UpdateIndexesOnInteraction()
        {
            // note: the cronExpression was chosen at random. It is provided as it is mandatory in the Service Job. Feel free to change it if needed.
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v17.0 - Interaction Index Post Migration Job",
                description: "This job adds the IX_InteractionSessionId_CreatedDateTime index on the Interaction Table.",
                jobType: "Rock.Jobs.PostV17InteractionIndexPostMigration", cronExpression: "0 0 21 1/1 * ? *",
                guid: "9984C806-FAEE-4005-973B-9FBE21948972" );
        }

        /// <summary>
        /// PA: Chop Legacy Communication Recipient List Webforms Block
        /// </summary>
        private void ChopLegacyCommunicationRecipientList()
        {

#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
#pragma warning restore CS0618 // Type or member is obsolete
                "Chop CommunicationRecipientList",
                blockTypeReplacements: new Dictionary<string, string> {
                    { "EBEA5996-5695-4A42-A21C-29E11E711BE8", "3F294916-A02D-48D5-8FE4-E8D7B98F61F7" }
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_REMOVE_COMMUNICATION_RECIPIENT_LIST_BLOCK );
        }

        #endregion

        #region MigratoinRollupForV17_0_3

        /// <summary>
        /// DH: Add Run-Once job for Rock.Jobs.PostUpdateJobs.PostV17RemoveLegacyPreferencesPostMigration
        /// </summary>
        private void RemoveLegacyPreferences()
        {
            // note: the cronExpression was chosen at random. It is provided as it is mandatory in the Service Job. Feel free to change it if needed.
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v17.0 - Remove Legacy Preferences Post Migration Job.",
                description: "This job removes the legacy user preferences from the Attribute and AttributeValue tables.",
                jobType: "Rock.Jobs.PostUpdateJobs.PostV17RemoveLegacyPreferencesPostMigration",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_170_REMOVE_LEGACY_PREFERENCES );
        }

        /// <summary>
        /// JDR: Add the UpdatePersistedDataset job to ServiceJobs out of the box
        /// If the job is already there, update the cron expression to run every minute
        /// </summary>
        private void AddUpdatePersistedDatasetServiceUp()
        {
            Sql( $@"
          IF NOT EXISTS (
              SELECT 1
              FROM [ServiceJob]
              WHERE [Guid] = '{SystemGuid.ServiceJob.UPDATE_PERSISTED_DATASETS}'
          )
          BEGIN
              INSERT INTO [ServiceJob] (
                  [IsSystem],
                  [IsActive],
                  [Name],
                  [Description],
                  [Class],
                  [CronExpression],
                  [NotificationStatus],
                  [Guid]
              ) VALUES (
                  1, -- IsSystem
                  1, -- IsActive
                  'Update Persisted Datasets', -- Name
                  'This job will update persisted datasets.', -- Description
                  'Rock.Jobs.UpdatePersistedDatasets', -- Class
                  '0 * * * * ?', -- CronExpression to run every minute
                  1, -- NotificationStatus
                  '{SystemGuid.ServiceJob.UPDATE_PERSISTED_DATASETS}' -- Guid
              );
          END
          ELSE
          BEGIN
              -- Update the cron expression if the job already exists
              UPDATE [ServiceJob]
              SET [CronExpression] = '0 * * * * ?' -- CronExpression to run every minute
              WHERE [Guid] = '{SystemGuid.ServiceJob.UPDATE_PERSISTED_DATASETS}';
          END" );

        }

        #endregion

        #region MigrationRollupForV17_0_4

        /// <summary>
        /// JC: Replace the binary content of the default icon file.
        /// The old file has a circular shape
        /// while the new file has a square shape with rounded corners.
        /// </summary>
        private void UpdateFavIcon()
        {
            Sql( @"
--Replace the original circular FavIcon binary file content with the new square with rounded corners icon
DECLARE @oldIcon VARBINARY( MAX ) = 0x89504E470D0A1A0A0000000D49484452000000C0000000C0080600000052DC6C07000000017352474200AECE1CE90000000467414D410000B18F0BFC61050000001974455874536F6674776172650041646F626520496D616765526561647971C9653C0000147B49444154785EEDDD6BD01C55990770B7586F1FD6FDB4A5E896B5A501E4B6885C2468291731145A968B5F30BA8A6ED62D0B763752BB24908BE1B6262001431010638004595012944A2A68022810562808E112D935140410A34B32F3DEAF33EDF3EFB7676AE69DFFBCEFF44C779FA74F3FA7EA57541EE672FA9CA7939933DDCF79CBC18BE798E41C2AE68A2F8A4BC4F56283D82A7E23F68AD7C4C1C8B808A2FFD662F8FF781C1E8FE7E1F9781DBC1E5E17AF8FF761EF6F62A24133ABF78979E2628104DD25460492392B78BF6704DE1FFD407FD02FD65FD3060D9A26878813C5B7C4BDE275C112520BF40FFD447FD16FF49F1D97113468E6FCAD58207E2ACA82255A5EA0FF380E1C0F8E8B1D6F61D160411D25568867054B245FE0F8709C385E360E85428305F241B144F89EF4EDE0B871FC1807363EDEA341CFBD5D60356587A80A9618458371C078CC17181F366E5EA2414F7D40AC1607044B023305E38371C278B171F40A0D7AE664718F98146CC20D87F1C2B861FCD8B87A81063D7196F8B560936BE2C1387E5AB071CE351ACCB9D3C423824DA4E90DC615E3CBC63D976830A73E22B60B36712659F8C27C8260F3902B349833B82E669DA8083659261D186F8C7BAEAF4BA2C19C789BC01AF680601364B281F1C73C603ED83CA9468339F031B147B009316E603E302F6CBED4A241C5DE25D60AFBB8A313E605F3837962F3A70E0D2A75A6C0B5F26CE08D2E98A74F09368FAAD0A032F869FE1AA1FE6FFDD28A1382FE5BFE3118DCF8AFA1FE1F7C25285D76127D6C0160BE306FAA2FADA0414570C5226E366103AC4279D599C1E823EB83CAC1D78376AD527A23187DF4F6A0EF9AB3E86B780EF3A7F6CA531A540217AC0D0A36A8CE952EFF6830B6EBE74150AD4469DE4193C78EEDBA3F285F3197BEA6C78604E693CDB35334E8D85B05EE816503A9C2C0AD5F0D2AFD7F8AB23A7EAB0E1E0806D6FD137D6DCF615E31BF6CDE9DA04187DE2D7E25D8E0A930B8F1DF826072224AE51E5A653218BC73217D0FCFE1BA22CC339BFFCCD1A023478B97051B3415FA6F9E2FC93F1E6570024D5E0B5F9AD97B79EE1581F9667990291A74E00C8192206CB054C0975D7C7449BA55874AE16BB3F7F45C4960699BE543666830635F1563820D920A58DEACBCF94A94B2C937BC76E9B293E97B7B0EF590CE172C2F32418319FA77A1FBB6C44B8E0C26F6EE8C5235BD36B1F7F1F0BD681FFC86F95F28587EA48E0633820BA8D880A832F6D4E62845D36F634FFF2C38B8E830DA8F02582A589EA48A0633F01DC1064195E1ADD744A9995DC37BB2BE14C44AC1F225353498321C243B7855B0441954AB515A66D8E43D07EFBA88F6A920323D09683045F8678E1DB42AFD6BBF1054C787A38CCCBEE1BDD107D6B782582658FE248E0653822F3AEC605529AF3C3D95E5CEB80D7D28AF3A83F6B12032F9624C8329F892505F84AAB4FCF860F20F2F4629E8BEA12FE813EB6B01205F90372C9F12438309C38F5CAAD7F9438B8FC864B9336E439FD037DA67FF216F903F2CAF124183093A56E4A2BAF2E8E3774629A7AFA16FACCF0581FC411EB1FCEA190D26E46FC43EC10E4A95E12DABA254D3DB86B7ACA47D2F08DC61F61EC1F2AC273498005CF2AAFAAACE9A81DBFE25BC32537D933EA2AFEC180A025791267E29350D26E03AC10E4295FE35E73A5DEE8CDBD0D7BE35FF408FA520703F01CBB7AED1608FCE13ACF3AA94AFFA785029FF214AADFC34F4197D67C7541028E1CEF2AE2B34D88323056E7F631D57A3B4EC3855CB9D715BB83CBAF4587A6C0580FC4AEC1E631AECD23B84FE9D56161F118CBFB03D4AA5FC361C438197479167EF142C0F63A1C12EAD11ACB3AA8CEEDC10A550FE1B2A4DB0632C881B04CBC35868B00BA81DAFFE97DEA14DCBA3D4F1A7E198D8B11600F2ADE73D0B6830A6BF12EA2BB60DACFF463E963BE3362C8FAEFB3A3DE60240DE21FF585E76840663BA51B0CEA9D1B7FA334175A43FCA18FF1A8E0DC7C88EBD00907F2C2F3B428331A01AB0EA9285E5AB3E96CBE5CEB80DC788625D6C0C3C87FCEBBA2A350D7608351F5597282F2D393698D8F74C9422FE371C2B8E998D85E790875DD520A5C10E2D16AC333A2C3ADC8BE5CEB86DECD96D45BDAF18F9C8F2744634D8015C98D42F5847541879E8E628258AD770EC6C4C3C877C8C7DC11C0D7660BD609D506168D3B228158ADB06EF5944C7C673B70996AF6DD1E02C4E146ABFF8A2D460A2E50BF3DA8A5976117989FC64794BD1E02CD46E45DAB7FA1CAF973BE3368C4501CB2E620B5796B7140DCE009B24B337750EA505D32C5F18A7E1CBF7F8730F447F72DB0A5A76B1E3CDBC697006B82981BDA153A525C7A859EEAC2D456A5A829D78F9C9A2955DC48EF62C7F5BD0601BF3047B33B7161D169614D4D0A65FAB8F9D60B4FC0857C0B28BC85796C74D68B00D95B7388E3C785334C56E5BBBCB11345D8631FCC0752DFDF3183EADB03C6E4283C42982BD89532821E8A47CE1F436CBFDBA033F5A103EC6792B5ED945E42DCBE73A1A24EE16EC0D9CD1B4DCD9C925C96A2EC5C6F26871CA2E226F593ED7D1E03473C484606FE0445ABBB574D346776EA47D64461FBB237A96DB365576B110CBA3C85BE42FCBEB100D4EA3AAC283B6E5CE58B7252ABA1D73F28F7B8B527611F9CBF23A44830D709FEF9B82BD70F632DAADA5933655B7F3C3BC9F33D074437E5876D1FFE551E46FDB2B4569B0014A50B017CD9EE2E5CEB834956419FD9FBB681F3DD3B6940A0D367850B017CC9C8BDD5A580B6BF7DF702EED631C780D2D45B90AB02B0DF298E5F78C2700BE3CA8B8D13D2FCB9D71A929CB2863EB79D945E431FD32DC1268B05CB017CB94EBDD5A1ADBF0D6AB691F7BA1EA5F36BF9747E9AE332D81062F0AF6429951B5DC996289722DA5D92BE5FD3E975D7C4EB4E4794B20729C602F92194DBBB54CAD967C88F63311EA56B7BC5D1E6D29A9D8F48706970BF602D92860424C9DF0FF1BBDABDB36BE6787AF65175788A65C6FFA4303A7353EB57C24087F315D793AED631A8AF291CF21E47553AE37FD21F27EC19E9C89A27F290CBFF44F8C46BD70DB3C2DBBF85E51CFF7C6C4AFF9A6604F4C5DB82C58AD44C3EFB0E1AAC91F2FA47DCC82AA65DFF5DFA07DCCB105A29EEF8D895FB359B027A64AD772A7FB1F8686B75D1BF5C66DF3B0ECE23DA29EEF8D890F8788CC77759CBA34607F34E46EDBD8D3F7D13E664EDBA51F579ECAFB993FC86FE4393D018E17EC49A9D1B7DCA9E8E230AC86BDF49BA8776E9B67651791E7F404B840B027A4039707EFD9110DB1DB16564F587122EFA743EA2EFFF6E3BEE20B053D01EE12EC09A950B5DCA9F80691707974B81CF5D66DF3A4EC22F29C9E00AF0AF684C40D6DFE7634A48E1B6E11BC793EEDA326AA6E01BD7729ED638E20CF5B4E00AC8FB207274ED3559079BA497CF0EE8BA38E3B6E7E945D0C7F0F683C01CE16EC81899A2A1332108DA4DB36B2E346DA47CDB454BD0E9747BF3B8FF6312790EF4D27C07F0AF6C0C468BA132AB785A2B03CBA7B4B74146E5BCECB2E22DF9B4E803B047B6022C25281AFEE8E86CE6DC392DEC14B8FA2FDCC03556517F7EDD2B574DCB90DA2E904D82DD8037BA7A81A822FC562C3E5D1D2EFA3A372DB72FAAF6978615CE309302CD8037B36F2D02DD150B96DE1E7D66BCFA67DCC234D651747B6AFA57D540CF95E3F01B0B50C7B50CF545544F370C388811F9E6F2B6ADD7B6FED0448A5F667B876AD6172A479B076DD96AABF64BE7F1EEDA352A7D64E80C4EBFFA8FAE7B9009BC68D3EB23E3A5AB72D676517E7D74E804B1B823D0BBFA01D78351A12B76DFCF95FF872FDCACC342D34FCFFCB79B9AFF8D2DA099058FD4F5D4B74C5DA381AC73AF9C66FA3A377DB725276F1FADA0990CC6F00F89166D7FDD110B86DE135EC57CCE5FDF498AAB28B4FFC84F651910DB513605B43B06BAA7EA6F7EB2EA658FAAEFF9CDD5DD7996DB513E089866077161D1EF47DEFF32A94FFEB13BC8F59917FFAB11A822B5E477EB92618DDB9212C429B252D371985CBA31B2EE0E3E4DE13B513606F43D074A9EFEAB3C2E4AB0EF745B36F0D4D71D9C5976A27C0FE86A089095F3E471FBD5DCD6F1E1A5B75E04D8D6517F7D54E80CC6F84F705FED6D772CBA2F6A6B0EC62B976024C36044D8750354ECB8A4B5EDAF88B0F6B2ABB583F01D8FF3433D054CD226F6DE4E15BE9983A3061274097F065D75A97AD5A55731FB69D005DE8BBEEB3F685B7C736F9FB3D1A2E51A9FF0B60DF0162D0F26B77DE1B2EE566E39BA1FA77005B05EA5069E9DF07D5F191680AADF5D2C23BC9C81867A87E02D8EF001D0A4BBA584BA4E1D26936C619AAFF0EF05243D0CC60F881D5D1F4594BA239FE71ACFE4B70EFD7021504AE70B4965CC3B55B6C9C33F264ED0448E46AD022D052CCD797E6F88B70FD6AD0546B02F964ECC97BA3A9B39644737C0F71FD7E80C4EE08F31DCA295A4BAE95BFF3493ACE19A9DF1196E83DC13E1BDC70613475D67A6DD5A1B2EB1FC3EAF704275E15C257A51527D8AFC009B5B1DD5BE91867A85E15E2D486A099C5F80BBF8CA6D05A2F6D60FD3FD3F1CD50BD2E506A95E17CD47FD379D1145AEBB64DEEFF9D86CBA2EB95E120B5DAA03ED252A23CAF6D60DDD7E9B86608F9FE178D2700AAE5B2071AA274D94941A5FF4FD1745A8BD3B0371C1BD38CB55487B6DF0262EABFE5CB613D4C6B9DB7F1FF7B544BC1AC8DA2E904487D87181FA122326EF0B0367B1B7B765B50BAF4683A8E0EB4EC1093C91E613E1A79F0A6688AADB1561D1B0A867E7EA5B61AAD2D7B8465B64BA4775012F2E99F45D36DADD670B9F3C8AF6E0DCA979FC2C7CDAD965D2221B37D827D535A728C9AA2C06813AF3C259FB71F8BE785EDE18F535D7BFABEB04CFBF0FD574D5DE5B9E8703A560ABC26C29C9F7E0264BA53BC6FC2B2F04A6A0421991527A06B6D778ABF40B027980EF5AD3EC73606D1EF42414F80E3057B828921DC1A4AC9F2E8D0A665B48F05F711414F804384DD209F00249E8AE6E9E6803D407E23CFE90900F709F6441393AEFD12CEA17D2CA09F887ABE37267ECD37057BA2896BD161B641B83E0B443DDF1B13BFE6FD823DD17441DF9E69C7D07E1648B8FE5FD398F88DECC2B80495AF3C554D15E9B01895AE5F64B3145E00D7A8E90F0D2E17EC054C976CDF64152E134DB9DEF487061F16EC054C0F7007948ADB29B16FD75D17D13E7AEE68D194EB4D7F98E645C15EC4F46068D3F2280B1DB7E22D8F3E275AF2BC25D060B9602F647A845D2335345CAC565E7526EDA3879689963C6F0934384C54057B31D38BC547D8F268B690C773444B9EB704A67948B017343D2A2D3B4ECD164B137B776AB94B2B2DC86396DFB39E00562F2845A88C6CCBA399401EB3FC9EF50478873820D88B9A04F4AF3937DC485A431BDE7A0DED63CEBD2990C72CBF673D01C0EA86A62CDC74C39647D382FC65791DA2C169F0E5C1F6104BD9F096955116BA6DF8D7A87FED17681F7308794BBFFCD6D02071B7606F6012847A391A9A47CBA3C85B96CF7534489C22D81B98242D3E225C91D1D0B04285CDC0693FF30379CBF2B98E06DBF8B5606F6212A46907FA9C2F8F3E22581E37A1C136E609F6462661E595A7851F4334B4D1C77F4CFB9803C85796C74D68700638ABD89B9984E18BA82D8F76ED51C1F2B7050DCEE034C1DED0A460F0CE85E1D2A4F356AD844BB5AC8F4A214F59FEB6A0C159EC10EC4D4D0AF0B7AF8696A3E5D10705CB5B8A066771A2A808F6E62605634F6D8ED2D06DAB94F7BBDED87A36B8E80DF9C9F296A2C10EDC2658074C1A2E39D296473B83BC64F9DA160D76005B2A0D08D60993026CCEA7A6ECE29E1D1AB6379A0EF988BC64F9DA160D7668B1601D3129C1AFB37A964755ECF2D208F9C8F2744634D8A1B70BBB6D3263FD37CFD7537671F3B7691F1D401E221F599ECE880663C07293DD35963135BBD25426352C8F22FF4E172C3F67458331FD50B08E99148D6CBF31CA42B7AD3A3210967C617DCC08F28FE565476830A6BF16D8708075CEA445D1AE34B8ABCDD1F228F20EF9C7F2B22334D8854F0BFB2894352C8F6A29BBF8DAEEB00C24ED673A906F1D5DEF33131AECD20D8275D4A448DDAE34D92D8FAE152C0F63A1C12EBD53584D5107FABE3B4F4FD9C5877F40FB98B0E705F28DE5612C34D883A3C490609D3629D2B52BCD72DAC78420BF5A4A1C768B067B64A5541C19FAE99228051DB7CA649A6517BF2C58DE758506136095241CD1B52B4DE2CBA3D70B966F5DA3C104BC55D82D942E60579AE77F11A5A1DB5639F06A9265177133D6DB04CBB7AED16042DE2DECF70107F4ED4AD3F3F228F2E850C1F2AC273498A06385ED3AE940F98AB97ACA2EEEBABF97B28BC81FE411CBAF9ED160C2CE1063821D9C499107BBD2206F903F2CAF12418329F892B05F8A1D18F8D182705546431BFCEFFFA07D6C03F982BC61F994181A4CC9B7043B5093B29CEE4A837C617994281A4C1176E960076B523674EF52F918728B73C35B5675F27D80EEE692061A4CD92AC10EDA18B85AB0BC49050D66C04E02C3202F58BEA4860633B254B04130C594D9C79E463498A185C256878A0DF39FC9175E86063376BE18176C708CDF30EF5F132C2F3241830E9C294A820D92F1137EE1C5BCB37CC80C0D3A826BBC5F116CB08C5FF689C4AEE9EF050D3A840BE8EC2A52BFE1AA4ECC339BFFCCD1A063B894FA7B820D9EC937CC2BE697CDBB1334A804EE2CB3DB2BFD80796CBB59B54B34A808EE31DE25D8A09A7C7846601ED9FC3A4783CAA0E6E3B5C27E2FC817CC17E6ADAB9A9D59A141A53E25EC0EB37C785D60BED83CAA42838ABD4BDC286C871A9D302FDF179827367FEAD0600E7C5CEC116C128C1BBF159817365F6AD1604EE0B3E512613BD5B885F1C73CA8FEACDF0E0DE60CAA05AC13F6B1285B186F8C7B2AD51AB242833975B2C016996CB24CB230CE1F156C1E728506730EBB8560A7703671A63718D7AE7763D188063D81DAF1B8EE844DA489E731D1732D7E8D68D033F868748F98146C720D87F1C2B879F151A71D1AF4D407048AF61E106CC2CD148C0FC609E3C5C6D12B34E8392CD7A1E0D243C22EAF988271C078605C72B99CD92D1A2C900F0ADC8CFD9C6089E13B1C378E1FE3C0C6C77B345850B8627185F07D9B271C1F8E53ED159A59A24133E7EFC405E23E91F7EAD6E83F8E03C783E362C75B5834689A1C224E1217894D02573AB244D302FD433FD15FF4FB2F053B2E2368D0CCEA7DE26C71B1D828768B11C112322D783FBC2FDE1FFD407FD02FD65FD3060D9AAEE1BA98B9E28BE212813DAD3688ADE209F19278431C8CD4AE5FC27F6B31FC7F3C0E8FC7F3F07CBC0E5E0FAF8BD7CFF5F5379AD0A031C530E72D7F068CF79FD77AF9B6C60000000049454E44AE426082
DECLARE @newIcon VARBINARY( MAX ) = 0x89504E470D0A1A0A0000000D49484452000000C0000000C0080600000052DC6C070000000467414D410000B18F0BFC6105000000097048597300000B1300000B1301009A9C1800000AFF49444154785EED9DCF6E135718C5F3083C82253C94256A1FA0F401CA0B14099674956C62D35590DA4AEC02DDB1A274895421D15D5964018B56AA940D7487BCA08004498610201002EE7C9E6B489C6BFB8E7DFF9C7BEF39D20F29B18927CE773CF7FBE6CCCC826F95CBC5A9B25B2C969DF66AC5ADB27BA2276C758B3EC987C1DFBD53AC57B5B036A805A9896EFBB42A9374542EB58ED5BF5CB156FDE2E5E81B41C808E5C0143F1C3F57765B2D554671498A7EE362715E15BDEE9724C488A1195469614B0A7FB37BE252B5E1FCA427B6E9496DC1EE1558F8C4133DA83D82342E5B6C62897F7ACF4336CDF572A7B8A2D93042BC51F508ABAA24FDA9EC9E6C6D758A75DD061112809EB7DEA05EF270AD4FE070BF2492D1A6E6850981C15983BCD9F96249F78284A0B1D9692FA9B2B5237EF293D8B0B627506B7EED8B1082CCDC3DC160DAC3193F89958EE48AE6980EB1F849F448E274A9754C95B4B978908BA442E383655CF793D468D40F70E9639F72E5CBFE8BCBDF7C42BED63D8F38A3A7CA7BB238F2B4C7CB6B67FBBB77AFF7F71FFFDBD749BE2F8F6F5F39A3FDFFC42E65A7BDA2CA5C2F4E7DEC2085FFFEE1DFAACCCDB4573DFFC5E5D3DA9F472C2153A1490DB1CAF4EBFF3331E2EDBD5F5549CFA637777ED1FE5C6287897B017EFACF8EACE9C72D759AEADD3FBF6B5F835860DC5E806BFFF9B055FC43D104EED0C6247802FBECEC56CB16177A7DFB27EDEB91B95953655FAB6E7EB54F24537055FC43BDBCF69DF675C97C1C5A0671F9331BDB57CFA83275A70F5BFF713AE4806AC5B3A8CA9FCB9F5990A294E2F4A1FDC70F78E0CC3E9F9741D5173CC5B101528CBE8A7F2836C596E914E5A0F899FB69CEDE833BAA2CFD8AC708ECF26CB93825CB9F45DD83448FEBA6779A5EDDEC68B78B3467D00754FFACEA1E244779FDC7CFAA0CC3E9E39B6D36C59690AB52B30136C4C7C4C754D27FB029B640A77D4B1A605EE06A0A3E273EA6DA7BF897765B49237ACCFF4C21C4C4C7546FEF5DD76E3331460CA07D8028424D7C4CC5B8C47CD00013083DF13115E312B343038C0161E2632A59A27132341B348006A4898FA91897980D1A6004C4898FA91897680E0D7000E4898FA9189768060D708079CFE745D1CE8D0BDADF8F1C850650C432F13111E312E6D00015AF6E7655E9A42359CAB1299E4EF606904FCA8FBB2F55D9A425C625A693B501629EF8988A7189C9646D00DB97324115E312E3C9D600A94C7C4CB57DE55BEDFB903B591A20A5898FA91897D0939D01766E7CAF4A223F312E7194AC0C90F2C4C7548C4B1C261B03E430F13115E3129FC9C600B94C7C4CC5B8444D1606C86DE26322C6256A9237408E131F53312E91B801D0263E1FB61E5586BCAABEC250EE7189640D8038F19133CD64DBD04C90735C224903204E7C5EDFFEF1D036CA381249B9C625923440D33B34BAD69BEA137F741BCB4B5FF5F79F604DA6728C4B246700B4A677D21ABBDE533D52CF0CAF1CE312491900ADF8A5B8A715945CD30749B9C525923100DAA54C4C8A7F88ACBF9194535C2209032036BD4DAFD6B60B76B02E97B844F40640BC9489AEE935E17DD52F202987B844F40640BB78EDEEDDD967EA684D710E7189A80D80D6F44A03A9DBCE26482F8374002FF5B844B40640BB786D93A6771A721F3024A51C9788D20068139F8FBBF6970A8C4BF8213A03204E7C5C358B8C4BB8272A03A434F13141E212484DB128B5B84454064869E2638AECF1D09AE2942643D11820C5898F298C4BB8230A03A43CF13185710937C01B2087898F298C4BD807DA00394D7C4C413B8720F63B54C21A20B7898F29F58702E312B6803500DAA54C7C4C7C4C615CC21E9006403CB145B79D21615CC20E700640BB5D5188898F298C4BCC0F9401A4D09076ED21273EA6A01D1C8C2D2E016380BAB9C36A7A472F6582085A5C429AE298E212300640BB782DC2C4C714B43D674C71090803A04D7C6459A1DB4E641897988DE006409CF8C43AD2635CA239410D8078F1DA5876DDE3605CA219C10C80B66E15C90126DDB6C606E312E6043100273E6EA9DF5FC6254C0862004E7CDCC3B88419DE0DC0898F3F1897988E570370E2E31FB4B8045A53ECCD009CF884032D2E217B26DD7686C08B0138F1090BE312E3716E004E7C3040FB1042894B3837400CB72BCA05C6258EE2D400684D6FCA131F53D0E212A1CF217066004E7C70418B4B843C87C09901647DE713D9BD4F02A9F85F5CFEA6DAA6B383B3DF5C23D33779AD83C8F7D08612F237D2BD57AE71DE03901A293C39B11E6D2080A25071091AC031F247451B04A02A445C820670885CD2116DA9812EDF71091AC01168438098E4332E41033800ED62BE31CA575C8206B08CACF9A9F9E5AB29A6012CC3298F3DF9E80768008BC8DC9DB22BD7C70768008BF0D3DFBE5CEF0568004BC8812ECA8D5CF60234802538F971279759211AC0127BF7B1CEBA4A496F1D5E608B06B004E30EEEE4B20FA0012CC106D89DE4BDD5BDE736A0012C4103B8130D10015C02B913974011208D1AE546EFEEFFA97DCF6D40035882635077E2183402E4DA3B941BF1405824BCAFD6AA945DED3F79A07DAF6D41035844825B945DB93E2F8006B00CF702F62497B2D1BDC736A1012C23EB559E076C473C212652D0AEBE16A37C5DBF95067004DAD5D76292CFEBB7D2000E613FD04C1F77B7FB3B372E68DF4B57D0000E41BB2E3FB2E4C2C53ED6FCA3D0008E61533C5EF2E120CB9D9037CBA0013CB0F31BD6EDA186776811739A30BCC0B02DE4EE3C28172BA6013C8176B33ADF9720448506F0C83BB0C468E89B5320400378449AE2FD275837090F79730A046800CFC89A1A6D328472C7C610D000019026104972CAA11853B7ADA9430304022D2E8170C7C610D00001418B4B4893AEDBCE94A10102831697F079730A046880C020C625E460956E5B53840600401A50A4B844A83B3686800600012D2E2193A11C9A621A0008C625FC43038081169748BD29A601C0408C4BF8BA636308680040D0E212C3F8B46E5B638706008571093FD000C020C62574DB1933340038687189D4CE21A00122002D2E91D23904344004302EE10E1A2012189770030D10118C4BD88706880CB8B884C3DB17F980068810C625EC41034408E312F6A001228571093BD0001123A34824C51897A00122072D2E11DB3904344002302E313B344022302E311B344022302E311B344042A0C52562688A6980C490793C92D02FB95819E0444FF7008917B4B804F025177B3440A2302E6140A7585F28BBED5BDA0749D420C6257CDF02753AEDB5CA00C5AAFE41123B684D31DA390465A7BD2A0658D43D48D200312E81D2144BED2F94CBC529DD83241D1897D0532EB7BF5E10555F94A30F92B4605C62844E510E8A5F547D63EDC8134872302E7190AA011E8A7D401E48038A169708750E41D9397E4E957F658095D631DD93487AC82517199728FA4FBBAD962AFF5AD537B90CCA04C6250E2C7F86DA582ECEEB9F4C5224E7B8C4A1E5CF506A19C4695046641A97E8A9923FAAB2D35ED1FC079228399E43B059D5B82AF7A3E25E203F328B4BF48E34BFA3E25E203FE45357F60436B021577109EDDA5F2746A449828C5FFB8FEA79B77D5AF3030889964FB91F536D32264D12A16A7C5755599B4B35C4EBA33F8C90C8305FFA8CEA69F764ABFA019C0A9158993EF59926F60324561AAFFBC769E3226312242E364C479EA6AA1A8925DD0B1182C6A69CEAE842DC131074AC7FF28FAAEE0978A08CC0515A5BF34F533D1DA20908089D627DEE69CF2CE2C132121A39C8D55B6A1D5325E95F5C129140F4BC2D794C549F51462310E7941333FD2125BD816C1C8D401C3028FCA0CB9D26AAF7086D9E684FE6A4BD2639FE680A7F54B257386006E68AC834AA1AA98ABE5B2C465BF493248D8BFC72D2BD2B5348E2944BA6FC90BF7945FB96D482D4C4B3E5E2942A134F5A58F81F9905C6FAE952D5C20000000049454E44AE426082

-- Only include the original 2 files that were created with this content.
-- This was necessary to prevent Azure from timing out during the update.
SELECT Id, Content
INTO #limitedFiles
FROM BinaryFileData
WHERE Id IN (57, 58)

-- Perform the update against the BinaryFileData table based on the Content match.
UPDATE bfd SET
	Content = @newIcon
FROM #limitedFiles l
JOIN BinaryFileData bfd on bfd.Id = l.Id
WHERE l.Content = @oldIcon

-- Drop the temp table we used for limiting our results.
DROP TABLE #limitedFiles
    " );
        }
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        private void RevertFavIcon()
        {
            Sql( @"
--Replace the original circular FavIcon binary file content with the new square with rounded corners icon
DECLARE @oldIcon VARBINARY( MAX ) = 0x89504E470D0A1A0A0000000D49484452000000C0000000C0080600000052DC6C07000000017352474200AECE1CE90000000467414D410000B18F0BFC61050000001974455874536F6674776172650041646F626520496D616765526561647971C9653C0000147B49444154785EEDDD6BD01C55990770B7586F1FD6FDB4A5E896B5A501E4B6885C2468291731145A968B5F30BA8A6ED62D0B763752BB24908BE1B6262001431010638004595012944A2A68022810562808E112D935140410A34B32F3DEAF33EDF3EFB7676AE69DFFBCEFF44C779FA74F3FA7EA57541EE672FA9CA7939933DDCF79CBC18BE798E41C2AE68A2F8A4BC4F56283D82A7E23F68AD7C4C1C8B808A2FFD662F8FF781C1E8FE7E1F9781DBC1E5E17AF8FF761EF6F62A24133ABF78979E2628104DD25460492392B78BF6704DE1FFD407FD02FD65FD3060D9A26878813C5B7C4BDE275C112520BF40FFD447FD16FF49F1D97113468E6FCAD58207E2ACA82255A5EA0FF380E1C0F8E8B1D6F61D160411D25568867054B245FE0F8709C385E360E85428305F241B144F89EF4EDE0B871FC1807363EDEA341CFBD5D60356587A80A9618458371C078CC17181F366E5EA2414F7D40AC1607044B023305E38371C278B171F40A0D7AE664718F98146CC20D87F1C2B861FCD8B87A81063D7196F8B560936BE2C1387E5AB071CE351ACCB9D3C423824DA4E90DC615E3CBC63D976830A73E22B60B36712659F8C27C8260F3902B349833B82E669DA8083659261D186F8C7BAEAF4BA2C19C789BC01AF680601364B281F1C73C603ED83CA9468339F031B147B009316E603E302F6CBED4A241C5DE25D60AFBB8A313E605F3837962F3A70E0D2A75A6C0B5F26CE08D2E98A74F09368FAAD0A032F869FE1AA1FE6FFDD28A1382FE5BFE3118DCF8AFA1FE1F7C25285D76127D6C0160BE306FAA2FADA0414570C5226E366103AC4279D599C1E823EB83CAC1D78376AD527A23187DF4F6A0EF9AB3E86B780EF3A7F6CA531A540217AC0D0A36A8CE952EFF6830B6EBE74150AD4469DE4193C78EEDBA3F285F3197BEA6C78604E693CDB35334E8D85B05EE816503A9C2C0AD5F0D2AFD7F8AB23A7EAB0E1E0806D6FD137D6DCF615E31BF6CDE9DA04187DE2D7E25D8E0A930B8F1DF826072224AE51E5A653218BC73217D0FCFE1BA22CC339BFFCCD1A023478B97051B3415FA6F9E2FC93F1E6570024D5E0B5F9AD97B79EE1581F9667990291A74E00C8192206CB054C0975D7C7449BA55874AE16BB3F7F45C4960699BE543666830635F1563820D920A58DEACBCF94A94B2C937BC76E9B293E97B7B0EF590CE172C2F32418319FA77A1FBB6C44B8E0C26F6EE8C5235BD36B1F7F1F0BD681FFC86F95F28587EA48E0633820BA8D880A832F6D4E62845D36F634FFF2C38B8E830DA8F02582A589EA48A0633F01DC1064195E1ADD744A9995DC37BB2BE14C44AC1F225353498321C243B7855B0441954AB515A66D8E43D07EFBA88F6A920323D09683045F8678E1DB42AFD6BBF1054C787A38CCCBEE1BDD107D6B782582658FE248E0653822F3AEC605529AF3C3D95E5CEB80D7D28AF3A83F6B12032F9624C8329F892505F84AAB4FCF860F20F2F4629E8BEA12FE813EB6B01205F90372C9F12438309C38F5CAAD7F9438B8FC864B9336E439FD037DA67FF216F903F2CAF124183093A56E4A2BAF2E8E3774629A7AFA16FACCF0581FC411EB1FCEA190D26E46FC43EC10E4A95E12DABA254D3DB86B7ACA47D2F08DC61F61EC1F2AC273498005CF2AAFAAACE9A81DBFE25BC32537D933EA2AFEC180A025791267E29350D26E03AC10E4295FE35E73A5DEE8CDBD0D7BE35FF408FA520703F01CBB7AED1608FCE13ACF3AA94AFFA785029FF214AADFC34F4197D67C7541028E1CEF2AE2B34D88323056E7F631D57A3B4EC3855CB9D715BB83CBAF4587A6C0580FC4AEC1E631AECD23B84FE9D56161F118CBFB03D4AA5FC361C438197479167EF142C0F63A1C12EAD11ACB3AA8CEEDC10A550FE1B2A4DB0632C881B04CBC35868B00BA81DAFFE97DEA14DCBA3D4F1A7E198D8B11600F2ADE73D0B6830A6BF12EA2BB60DACFF463E963BE3362C8FAEFB3A3DE60240DE21FF585E76840663BA51B0CEA9D1B7FA334175A43FCA18FF1A8E0DC7C88EBD00907F2C2F3B428331A01AB0EA9285E5AB3E96CBE5CEB80DC788625D6C0C3C87FCEBBA2A350D7608351F5597282F2D393698D8F74C9422FE371C2B8E998D85E790875DD520A5C10E2D16AC333A2C3ADC8BE5CEB86DECD96D45BDAF18F9C8F2744634D8015C98D42F5847541879E8E628258AD770EC6C4C3C877C8C7DC11C0D7660BD609D506168D3B228158ADB06EF5944C7C673B70996AF6DD1E02C4E146ABFF8A2D460A2E50BF3DA8A5976117989FC64794BD1E02CD46E45DAB7FA1CAF973BE3368C4501CB2E620B5796B7140DCE009B24B337750EA505D32C5F18A7E1CBF7F8730F447F72DB0A5A76B1E3CDBC697006B82981BDA153A525C7A859EEAC2D456A5A829D78F9C9A2955DC48EF62C7F5BD0601BF3047B33B7161D169614D4D0A65FAB8F9D60B4FC0857C0B28BC85796C74D68B00D95B7388E3C785334C56E5BBBCB11345D8631FCC0752DFDF3183EADB03C6E4283C42982BD89532821E8A47CE1F436CBFDBA033F5A103EC6792B5ED945E42DCBE73A1A24EE16EC0D9CD1B4DCD9C925C96A2EC5C6F26871CA2E226F593ED7D1E03473C484606FE0445ABBB574D346776EA47D64461FBB237A96DB365576B110CBA3C85BE42FCBEB100D4EA3AAC283B6E5CE58B7252ABA1D73F28F7B8B527611F9CBF23A44830D709FEF9B82BD70F632DAADA5933655B7F3C3BC9F33D074437E5876D1FFE551E46FDB2B4569B0014A50B017CD9EE2E5CEB834956419FD9FBB681F3DD3B6940A0D367850B017CC9C8BDD5A580B6BF7DF702EED631C780D2D45B90AB02B0DF298E5F78C2700BE3CA8B8D13D2FCB9D71A929CB2863EB79D945E431FD32DC1268B05CB017CB94EBDD5A1ADBF0D6AB691F7BA1EA5F36BF9747E9AE332D81062F0AF6429951B5DC996289722DA5D92BE5FD3E975D7C4EB4E4794B20729C602F92194DBBB54CAD967C88F63311EA56B7BC5D1E6D29A9D8F48706970BF602D92860424C9DF0FF1BBDABDB36BE6787AF65175788A65C6FFA4303A7353EB57C24087F315D793AED631A8AF291CF21E47553AE37FD21F27EC19E9C89A27F290CBFF44F8C46BD70DB3C2DBBF85E51CFF7C6C4AFF9A6604F4C5DB82C58AD44C3EFB0E1AAC91F2FA47DCC82AA65DFF5DFA07DCCB105A29EEF8D895FB359B027A64AD772A7FB1F8686B75D1BF5C66DF3B0ECE23DA29EEF8D890F8788CC77759CBA34607F34E46EDBD8D3F7D13E664EDBA51F579ECAFB993FC86FE4393D018E17EC49A9D1B7DCA9E8E230AC86BDF49BA8776E9B67651791E7F404B840B027A4039707EFD9110DB1DB16564F587122EFA743EA2EFFF6E3BEE20B053D01EE12EC09A950B5DCA9F80691707974B81CF5D66DF3A4EC22F29C9E00AF0AF684C40D6DFE7634A48E1B6E11BC793EEDA326AA6E01BD7729ED638E20CF5B4E00AC8FB207274ED3559079BA497CF0EE8BA38E3B6E7E945D0C7F0F683C01CE16EC81899A2A1332108DA4DB36B2E346DA47CDB454BD0E9747BF3B8FF6312790EF4D27C07F0AF6C0C468BA132AB785A2B03CBA7B4B74146E5BCECB2E22DF9B4E803B047B6022C25281AFEE8E86CE6DC392DEC14B8FA2FDCC03556517F7EDD2B574DCB90DA2E904D82DD8037BA7A81A822FC562C3E5D1D2EFA3A372DB72FAAF6978615CE309302CD8037B36F2D02DD150B96DE1E7D66BCFA67DCC234D651747B6AFA57D540CF95E3F01B0B50C7B50CF545544F370C388811F9E6F2B6ADD7B6FED0448A5F667B876AD6172A479B076DD96AABF64BE7F1EEDA352A7D64E80C4EBFFA8FAE7B9009BC68D3EB23E3A5AB72D676517E7D74E804B1B823D0BBFA01D78351A12B76DFCF95FF872FDCACC342D34FCFFCB79B9AFF8D2DA099058FD4F5D4B74C5DA381AC73AF9C66FA3A377DB725276F1FADA0990CC6F00F89166D7FDD110B86DE135EC57CCE5FDF498AAB28B4FFC84F651910DB513605B43B06BAA7EA6F7EB2EA658FAAEFF9CDD5DD7996DB513E089866077161D1EF47DEFF32A94FFEB13BC8F59917FFAB11A822B5E477EB92618DDB9212C429B252D371985CBA31B2EE0E3E4DE13B513606F43D074A9EFEAB3C2E4AB0EF745B36F0D4D71D9C5976A27C0FE86A089095F3E471FBD5DCD6F1E1A5B75E04D8D6517F7D54E80CC6F84F705FED6D772CBA2F6A6B0EC62B976024C36044D8750354ECB8A4B5EDAF88B0F6B2ABB583F01D8FF3433D054CD226F6DE4E15BE9983A3061274097F065D75A97AD5A55731FB69D005DE8BBEEB3F685B7C736F9FB3D1A2E51A9FF0B60DF0162D0F26B77DE1B2EE566E39BA1FA77005B05EA5069E9DF07D5F191680AADF5D2C23BC9C81867A87E02D8EF001D0A4BBA584BA4E1D26936C619AAFF0EF05243D0CC60F881D5D1F4594BA239FE71ACFE4B70EFD7021504AE70B4965CC3B55B6C9C33F264ED0448E46AD022D052CCD797E6F88B70FD6AD0546B02F964ECC97BA3A9B39644737C0F71FD7E80C4EE08F31DCA295A4BAE95BFF3493ACE19A9DF1196E83DC13E1BDC70613475D67A6DD5A1B2EB1FC3EAF704275E15C257A51527D8AFC009B5B1DD5BE91867A85E15E2D486A099C5F80BBF8CA6D05A2F6D60FD3FD3F1CD50BD2E506A95E17CD47FD379D1145AEBB64DEEFF9D86CBA2EB95E120B5DAA03ED252A23CAF6D60DDD7E9B86608F9FE178D2700AAE5B2071AA274D94941A5FF4FD1745A8BD3B0371C1BD38CB55487B6DF0262EABFE5CB613D4C6B9DB7F1FF7B544BC1AC8DA2E904487D87181FA122326EF0B0367B1B7B765B50BAF4683A8E0EB4EC1093C91E613E1A79F0A6688AADB1561D1B0A867E7EA5B61AAD2D7B8465B64BA4775012F2E99F45D36DADD670B9F3C8AF6E0DCA979FC2C7CDAD965D2221B37D827D535A728C9AA2C06813AF3C259FB71F8BE785EDE18F535D7BFABEB04CFBF0FD574D5DE5B9E8703A560ABC26C29C9F7E0264BA53BC6FC2B2F04A6A0421991527A06B6D778ABF40B027980EF5AD3EC73606D1EF42414F80E3057B828921DC1A4AC9F2E8D0A665B48F05F711414F804384DD209F00249E8AE6E9E6803D407E23CFE90900F709F6441393AEFD12CEA17D2CA09F887ABE37267ECD37057BA2896BD161B641B83E0B443DDF1B13BFE6FD823DD17441DF9E69C7D07E1648B8FE5FD398F88DECC2B80495AF3C554D15E9B01895AE5F64B3145E00D7A8E90F0D2E17EC054C976CDF64152E134DB9DEF487061F16EC054C0F7007948ADB29B16FD75D17D13E7AEE68D194EB4D7F98E645C15EC4F46068D3F2280B1DB7E22D8F3E275AF2BC25D060B9602F647A845D2335345CAC565E7526EDA3879689963C6F0934384C54057B31D38BC547D8F268B690C773444B9EB704A67948B017343D2A2D3B4ECD164B137B776AB94B2B2DC86396DFB39E00562F2845A88C6CCBA399401EB3FC9EF50478873820D88B9A04F4AF3937DC485A431BDE7A0DED63CEBD2990C72CBF673D01C0EA86A62CDC74C39647D382FC65791DA2C169F0E5C1F6104BD9F096955116BA6DF8D7A87FED17681F7308794BBFFCD6D02071B7606F6012847A391A9A47CBA3C85B96CF7534489C22D81B98242D3E225C91D1D0B04285CDC0693FF30379CBF2B98E06DBF8B5606F6212A46907FA9C2F8F3E22581E37A1C136E609F6462661E595A7851F4334B4D1C77F4CFB9803C85796C74D68700638ABD89B9984E18BA82D8F76ED51C1F2B7050DCEE034C1DED0A460F0CE85E1D2A4F356AD844BB5AC8F4A214F59FEB6A0C159EC10EC4D4D0AF0B7AF8696A3E5D10705CB5B8A066771A2A808F6E62605634F6D8ED2D06DAB94F7BBDED87A36B8E80DF9C9F296A2C10EDC2658074C1A2E39D296473B83BC64F9DA160D76005B2A0D08D60993026CCEA7A6ECE29E1D1AB6379A0EF988BC64F9DA160D7668B1601D3129C1AFB37A964755ECF2D208F9C8F2744634D8A1B70BBB6D3263FD37CFD7537671F3B7691F1D401E221F599ECE880663C07293DD35963135BBD25426352C8F22FF4E172C3F67458331FD50B08E99148D6CBF31CA42B7AD3A3210967C617DCC08F28FE565476830A6BF16D8708075CEA445D1AE34B8ABCDD1F228F20EF9C7F2B22334D8854F0BFB2894352C8F6A29BBF8DAEEB00C24ED673A906F1D5DEF33131AECD20D8275D4A448DDAE34D92D8FAE152C0F63A1C12EBD53584D5107FABE3B4F4FD9C5877F40FB98B0E705F28DE5612C34D883A3C490609D3629D2B52BCD72DAC78420BF5A4A1C768B067B64A5541C19FAE99228051DB7CA649A6517BF2C58DE758506136095241CD1B52B4DE2CBA3D70B966F5DA3C104BC55D82D942E60579AE77F11A5A1DB5639F06A9265177133D6DB04CBB7AED16042DE2DECF70107F4ED4AD3F3F228F2E850C1F2AC273498A06385ED3AE940F98AB97ACA2EEEBABF97B28BC81FE411CBAF9ED160C2CE1063821D9C499107BBD2206F903F2CAF12418329F892B05F8A1D18F8D182705546431BFCEFFFA07D6C03F982BC61F994181A4CC9B7043B5093B29CEE4A837C617994281A4C1176E960076B523674EF52F918728B73C35B5675F27D80EEE692061A4CD92AC10EDA18B85AB0BC49050D66C04E02C3202F58BEA4860633B254B04130C594D9C79E463498A185C256878A0DF39FC9175E86063376BE18176C708CDF30EF5F132C2F3241830E9C294A820D92F1137EE1C5BCB37CC80C0D3A826BBC5F116CB08C5FF689C4AEE9EF050D3A840BE8EC2A52BFE1AA4ECC339BFFCCD1A063B894FA7B820D9EC937CC2BE697CDBB1334A804EE2CB3DB2BFD80796CBB59B54B34A808EE31DE25D8A09A7C7846601ED9FC3A4783CAA0E6E3B5C27E2FC817CC17E6ADAB9A9D59A141A53E25EC0EB37C785D60BED83CAA42838ABD4BDC286C871A9D302FDF179827367FEAD0600E7C5CEC116C128C1BBF159817365F6AD1604EE0B3E512613BD5B885F1C73CA8FEACDF0E0DE60CAA05AC13F6B1285B186F8C7B2AD51AB242833975B2C016996CB24CB230CE1F156C1E728506730EBB8560A7703671A63718D7AE7763D188063D81DAF1B8EE844DA489E731D1732D7E8D68D033F868748F98146C720D87F1C2B879F151A71D1AF4D407048AF61E106CC2CD148C0FC609E3C5C6D12B34E8392CD7A1E0D243C22EAF988271C078605C72B99CD92D1A2C900F0ADC8CFD9C6089E13B1C378E1FE3C0C6C77B345850B8627185F07D9B271C1F8E53ED159A59A24133E7EFC405E23E91F7EAD6E83F8E03C783E362C75B5834689A1C224E1217894D02573AB244D302FD433FD15FF4FB2F053B2E2368D0CCEA7DE26C71B1D828768B11C112322D783FBC2FDE1FFD407FD02FD65FD3060D9AAEE1BA98B9E28BE212813DAD3688ADE209F19278431C8CD4AE5FC27F6B31FC7F3C0E8FC7F3F07CBC0E5E0FAF8BD7CFF5F5379AD0A031C530E72D7F068CF79FD77AF9B6C60000000049454E44AE426082
DECLARE @newIcon VARBINARY( MAX ) = 0x89504E470D0A1A0A0000000D49484452000000C0000000C0080600000052DC6C070000000467414D410000B18F0BFC6105000000097048597300000B1300000B1301009A9C1800000AFF49444154785EED9DCF6E135718C5F3083C82253C94256A1FA0F401CA0B14099674956C62D35590DA4AEC02DDB1A274895421D15D5964018B56AA940D7487BCA08004498610201002EE7C9E6B489C6BFB8E7DFF9C7BEF39D20F29B18927CE773CF7FBE6CCCC826F95CBC5A9B25B2C969DF66AC5ADB27BA2276C758B3EC987C1DFBD53AC57B5B036A805A9896EFBB42A9374542EB58ED5BF5CB156FDE2E5E81B41C808E5C0143F1C3F57765B2D554671498A7EE362715E15BDEE9724C488A1195469614B0A7FB37BE252B5E1FCA427B6E9496DC1EE1558F8C4133DA83D82342E5B6C62897F7ACF4336CDF572A7B8A2D93042BC51F508ABAA24FDA9EC9E6C6D758A75DD061112809EB7DEA05EF270AD4FE070BF2492D1A6E6850981C15983BCD9F96249F78284A0B1D9692FA9B2B5237EF293D8B0B627506B7EED8B1082CCDC3DC160DAC3193F89958EE48AE6980EB1F849F448E274A9754C95B4B978908BA442E383655CF793D468D40F70E9639F72E5CBFE8BCBDF7C42BED63D8F38A3A7CA7BB238F2B4C7CB6B67FBBB77AFF7F71FFFDBD749BE2F8F6F5F39A3FDFFC42E65A7BDA2CA5C2F4E7DEC2085FFFEE1DFAACCCDB4573DFFC5E5D3DA9F472C2153A1490DB1CAF4EBFF3331E2EDBD5F5549CFA637777ED1FE5C6287897B017EFACF8EACE9C72D759AEADD3FBF6B5F835860DC5E806BFFF9B055FC43D104EED0C6247802FBECEC56CB16177A7DFB27EDEB91B95953655FAB6E7EB54F24537055FC43BDBCF69DF675C97C1C5A0671F9331BDB57CFA83275A70F5BFF713AE4806AC5B3A8CA9FCB9F5990A294E2F4A1FDC70F78E0CC3E9F9741D5173CC5B101528CBE8A7F2836C596E914E5A0F899FB69CEDE833BAA2CFD8AC708ECF26CB93825CB9F45DD83448FEBA6779A5EDDEC68B78B3467D00754FFACEA1E244779FDC7CFAA0CC3E9E39B6D36C59690AB52B30136C4C7C4C754D27FB029B640A77D4B1A605EE06A0A3E273EA6DA7BF897765B49237ACCFF4C21C4C4C7546FEF5DD76E3331460CA07D8028424D7C4CC5B8C47CD00013083DF13115E312B343038C0161E2632A59A27132341B348006A4898FA91897980D1A6004C4898FA91897680E0D7000E4898FA9189768060D708079CFE745D1CE8D0BDADF8F1C850650C432F13111E312E6D00015AF6E7655E9A42359CAB1299E4EF606904FCA8FBB2F55D9A425C625A693B501629EF8988A7189C9646D00DB97324115E312E3C9D600A94C7C4CB57DE55BEDFB903B591A20A5898FA91897D0939D01766E7CAF4A223F312E7194AC0C90F2C4C7548C4B1C261B03E430F13115E3129FC9C600B94C7C4CC5B8444D1606C86DE26322C6256A9237408E131F53312E91B801D0263E1FB61E5586BCAABEC250EE7189640D8038F19133CD64DBD04C90735C224903204E7C5EDFFEF1D036CA381249B9C625923440D33B34BAD69BEA137F741BCB4B5FF5F79F604DA6728C4B246700B4A677D21ABBDE533D52CF0CAF1CE312491900ADF8A5B8A715945CD30749B9C525923100DAA54C4C8A7F88ACBF9194535C2209032036BD4DAFD6B60B76B02E97B844F40640BC9489AEE935E17DD52F202987B844F40640BB78EDEEDDD967EA684D710E7189A80D80D6F44A03A9DBCE26482F8374002FF5B844B40640BB786D93A6771A721F3024A51C9788D20068139F8FBBF6970A8C4BF8213A03204E7C5C358B8C4BB8272A03A434F13141E212484DB128B5B84454064869E2638AECF1D09AE2942643D11820C5898F298C4BB8230A03A43CF13185710937C01B2087898F298C4BD807DA00394D7C4C413B8720F63B54C21A20B7898F29F58702E312B6803500DAA54C7C4C7C4C615CC21E9006403CB145B79D21615CC20E700640BB5D5188898F298C4BCC0F9401A4D09076ED21273EA6A01D1C8C2D2E016380BAB9C36A7A472F6582085A5C429AE298E212300640BB782DC2C4C714B43D674C71090803A04D7C6459A1DB4E641897988DE006409CF8C43AD2635CA239410D8078F1DA5876DDE3605CA219C10C80B66E15C90126DDB6C606E312E6043100273E6EA9DF5FC6254C0862004E7CDCC3B88419DE0DC0898F3F1897988E570370E2E31FB4B8045A53ECCD009CF884032D2E217B26DD7686C08B0138F1090BE312E3716E004E7C3040FB1042894B3837400CB72BCA05C6258EE2D400684D6FCA131F53D0E212A1CF217066004E7C70418B4B843C87C09901647DE713D9BD4F02A9F85F5CFEA6DAA6B383B3DF5C23D33779AD83C8F7D08612F237D2BD57AE71DE03901A293C39B11E6D2080A25071091AC031F247451B04A02A445C820670885CD2116DA9812EDF71091AC01168438098E4332E41033800ED62BE31CA575C8206B08CACF9A9F9E5AB29A6012CC3298F3DF9E80768008BC8DC9DB22BD7C70768008BF0D3DFBE5CEF0568004BC8812ECA8D5CF60234802538F971279759211AC0127BF7B1CEBA4A496F1D5E608B06B004E30EEEE4B20FA0012CC106D89DE4BDD5BDE736A0012C4103B8130D10015C02B913974011208D1AE546EFEEFFA97DCF6D40035882635077E2183402E4DA3B941BF1405824BCAFD6AA945DED3F79A07DAF6D41035844825B945DB93E2F8006B00CF702F62497B2D1BDC736A1012C23EB559E076C473C212652D0AEBE16A37C5DBF95067004DAD5D76292CFEBB7D2000E613FD04C1F77B7FB3B372E68DF4B57D0000E41BB2E3FB2E4C2C53ED6FCA3D0008E61533C5EF2E120CB9D9037CBA0013CB0F31BD6EDA186776811739A30BCC0B02DE4EE3C28172BA6013C8176B33ADF9720448506F0C83BB0C468E89B5320400378449AE2FD275837090F79730A046800CFC89A1A6D328472C7C610D000019026104972CAA11853B7ADA9430304022D2E8170C7C610D00001418B4B4893AEDBCE94A10102831697F079730A046880C020C625E460956E5B53840600401A50A4B844A83B3686800600012D2E2193A11C9A621A0008C625FC43038081169748BD29A601C0408C4BF8BA636308680040D0E212C3F8B46E5B638706008571093FD000C020C62574DB1933340038687189D4CE21A00122002D2E91D23904344004302EE10E1A2012189770030D10118C4BD88706880CB8B884C3DB17F980068810C625EC41034408E312F6A001228571093BD0001123A34824C51897A00122072D2E11DB3904344002302E313B344022302E311B344022302E311B344042A0C52562688A6980C490793C92D02FB95819E0444FF7008917B4B804F025177B3440A2302E6140A7585F28BBED5BDA0749D420C6257CDF02753AEDB5CA00C5AAFE41123B684D31DA390465A7BD2A0658D43D48D200312E81D2144BED2F94CBC529DD83241D1897D0532EB7BF5E10555F94A30F92B4605C62844E510E8A5F547D63EDC8134872302E7190AA011E8A7D401E48038A169708750E41D9397E4E957F658095D631DD93487AC82517199728FA4FBBAD962AFF5AD537B90CCA04C6250E2C7F86DA582ECEEB9F4C5224E7B8C4A1E5CF506A19C4695046641A97E8A9923FAAB2D35ED1FC079228399E43B059D5B82AF7A3E25E203F328B4BF48E34BFA3E25E203FE45357F60436B021577109EDDA5F2746A449828C5FFB8FEA79B77D5AF3030889964FB91F536D32264D12A16A7C5755599B4B35C4EBA33F8C90C8305FFA8CEA69F764ABFA019C0A9158993EF59926F60324561AAFFBC769E3226312242E364C479EA6AA1A8925DD0B1182C6A69CEAE842DC131074AC7FF28FAAEE0978A08CC0515A5BF34F533D1DA20908089D627DEE69CF2CE2C132121A39C8D55B6A1D5325E95F5C129140F4BC2D794C549F51462310E7941333FD2125BD816C1C8D401C3028FCA0CB9D26AAF7086D9E684FE6A4BD2639FE680A7F54B257386006E68AC834AA1AA98ABE5B2C465BF493248D8BFC72D2BD2B5348E2944BA6FC90BF7945FB96D482D4C4B3E5E2942A134F5A58F81F9905C6FAE952D5C20000000049454E44AE426082

-- Only include the original 2 files that were created with this content.
-- This was necessary to prevent Azure from timing out during the update.
SELECT Id, Content
INTO #limitedFiles
FROM BinaryFileData
WHERE Id IN (57, 58)

-- Perform the update against the BinaryFileData table based on the Content match.
UPDATE bfd SET
	Content = @oldIcon
FROM #limitedFiles l
JOIN BinaryFileData bfd on bfd.Id = l.Id
WHERE l.Content = @newIcon

-- Drop the temp table we used for limiting our results.
DROP TABLE #limitedFiles
    " );
        }

        #endregion

        #region MigrationRollupsForV17_0_6

        /// <summary>
        /// SK: Fix Routes in Adaptive Message
        /// </summary>
        private void FixRouteForAdaptiveMessageUp()
        {
            RockMigrationHelper.UpdatePageRoute( "E612018C-FD4B-4F6F-9BCD-3B76B58CC8AB", "222ED9E3-06C0-438F-B520-C899B8835650", "admin/cms/adaptive-messages/attributes" );

#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.AddPageRoute( "73112D38-E051-4452-AEF9-E473EEDD0BCB", "admin/cms/adaptive-messages", "3B35F17E-B2DE-4512-8873-06A82F572ABD" );
#pragma warning restore CS0618 // Type or member is obsolete

            Sql( @"UPDATE 
                [Page]
            SET [DisplayInNavWhen] = 2
            WHERE [Guid]='222ED9E3-06C0-438F-B520-C899B8835650'" );
        }

        #endregion

        #region MigrationRollupsForV17_0_7

        // KH: Create Administrative Settings Page/Page Search Block and move various Admin Tool Settings Pages under the new Administrative Settings Page
        private const string AdministrativeSettingsPageGuid = "A7E36E7A-EFBD-4912-B46E-BB61A74B86FF";

        //KA: D: V17 Migration to add Tithing overview metrics
        const string TITHING_OVERVIEW_CATEGORY = "914E7A39-EA2D-469B-95B5-B6518DBE5F52";
        const string TITHING_OVERVIEW_SCHEDULE_GUID = "5E51EA9E-8475-4955-875E-45F44270A462";
        const string TITHING_OVERVIEW_BY_CAMPUS_METRIC_GUID = "F4951A42-9F71-4CB1-A46E-2A7ED84CD923";
        const string TITHING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID = "2B798177-E8F4-46DB-A1D7-308D63CA519A";
        const string GIVING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID = "B5BFAB51-9B46-4E7E-992E-B0119E4D25EC";

        //KA: Create data-migration to add new page with new Tithing Overview block
        const string TITHING_OVERVIEW_PAGE = "72BA5DD9-8685-4182-833D-22BB1E0F9A36";
        const string TITHING_OVERVIEW_BLOCK_ENTITY_TYPE = "1E44B061-7767-487D-A98F-16912E8C7DE7";
        const string TITHING_OVERVIEW_BLOCK_TYPE = "DB756565-8A35-42E2-BC79-8D11F57E4004";
        const string TITHING_OVERVIEW_BLOCK = "E6956ECC-08DC-4EF0-9F9A-67F8BD2F5F91";

        private void CreateAdministrativeSettingsPage()
        {
            // Add Page 
            //  Internal Name: Administrative Settings
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "550A898C-EDEA-48B5-9C58-B20EC13AF13B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Administrative Settings", "", AdministrativeSettingsPageGuid, "" );

            // Add Page Route
            //   Page:Administrative Settings
            //   Route:admin/settings
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.AddPageRoute( AdministrativeSettingsPageGuid, "admin/settings", "A000D38F-D19C-4F99-B498-227E3509A5C7" );
#pragma warning restore CS0618 // Type or member is obsolete

            // Update page order
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 0 WHERE [Guid] = '{AdministrativeSettingsPageGuid}'" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.PageSearch
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.PageSearch", "Page Search", "Rock.Blocks.Cms.PageSearch, Rock.Blocks, Version=1.17.0.18, Culture=neutral, PublicKeyToken=null", false, false, "85BA51A4-41CF-4F60-9EAE-1D8B1E73C736" );

            // Add/Update Obsidian Block Type
            //   Name:Rock.Blocks.Cms.PageSearch
            //   Category:
            //   EntityType:Rock.Blocks.Cms.PageSearch
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Page Search", "Displays a search page to find child pages", "Rock.Blocks.Cms.PageSearch", "CMS", "A279A88E-D4E0-4867-A108-2AA743B3CFD0" );

            // Add Block 
            //  Block Name: Page Search
            //  Page Name: Administrative Settings
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, AdministrativeSettingsPageGuid.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "A279A88E-D4E0-4867-A108-2AA743B3CFD0".AsGuid(), "Page Search", "Main", @"", @"", 0, "2B8A3D46-8E5F-44E2-AC84-2554DCA502EC" );
        }

        private void MoveSettingsPages()
        {
            // General
            var generalSettingsPageGuid = "0B213645-FA4E-44A5-8E4C-B2D8EF054985";

            Sql( $@"UPDATE [Page]
SET InternalName = 'General',
PageTitle = 'General',
BrowserTitle = 'General'
WHERE [Page].[Guid] = '{generalSettingsPageGuid}'" );

            RockMigrationHelper.MovePage( generalSettingsPageGuid, AdministrativeSettingsPageGuid );

            // Security
            RockMigrationHelper.MovePage( "91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F", AdministrativeSettingsPageGuid );

            // Communications
            RockMigrationHelper.MovePage( "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", AdministrativeSettingsPageGuid );

            // CMS
            // Move all the pages back to the CMS Configuration Page
            var cmsConfigurationPageGuid = "B4A24AB7-9369-4055-883F-4F4892C39AE3";

            Sql( $@"UPDATE [Page]
SET InternalName = 'CMS',
PageTitle = 'CMS',
BrowserTitle = 'CMS'
WHERE [Guid] = '{cmsConfigurationPageGuid}'" );

            RockMigrationHelper.MovePage( cmsConfigurationPageGuid, AdministrativeSettingsPageGuid );

            Sql( $@"
DECLARE @newParentPageId INT = (SELECT [Id] FROM [Page] WHERE [Guid] = '{cmsConfigurationPageGuid}')

UPDATE [dbo].[Page] SET [ParentPageId] = @newParentPageId WHERE [ParentPageId] IN (
   SELECT [Id] FROM [dbo].[Page] WHERE [Guid] IN (
   'CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89'  -- Website Configuration Section 
   ,'889D7F7F-EB0F-40CD-9E80-E58A00EE69F7' -- Content Channels Section
   ,'B892DF6D-4789-4AC3-9E6C-2BFE0D9E30E4' -- Personalization Section
   ,'04FE297E-D45E-44EC-B521-181423F05A1C' -- Content Platform Section
   ,'82726ACD-3480-4514-A920-FE920A71C046' -- Digital Media Applications Section
   )
)" );

            // Check In
            RockMigrationHelper.MovePage( "66C5DD58-094C-4FF9-9AFB-44801FCFCC2D", AdministrativeSettingsPageGuid );

            // Power Tools
            RockMigrationHelper.MovePage( "7F1F4130-CB98-473B-9DE1-7A886D2283ED", AdministrativeSettingsPageGuid );

            // System Settings
            var systemSettingsPageGuid = "C831428A-6ACD-4D49-9B2D-046D399E3123";

            Sql( $@"UPDATE [dbo].[Page]
SET [InternalName] = 'System',
[PageTitle] = 'System',
[BrowserTitle] = 'System'
WHERE [Page].[Guid] = '{systemSettingsPageGuid}'" );

            RockMigrationHelper.MovePage( systemSettingsPageGuid, AdministrativeSettingsPageGuid );
        }

        /// <summary>
        /// KA: Migration to add Tithing overview metrics
        /// </summary>
        private void TithingOverviewMetricsUp()
        {
            AddSchedule();

            RockMigrationHelper.UpdateCategory( SystemGuid.EntityType.METRICCATEGORY, "Tithing Overview", "icon-fw fa fa-chart-bar", "A few metrics to show high-level tithing statistics.", TITHING_OVERVIEW_CATEGORY );

            AddTithingOverviewByCampusMetrics();
            AddTithingHouseholdsByCampusMetrics();
            AddGivingHouseholdsOverviewByCampusMetrics();
        }

        /// <summary>
        /// KA: Migration to add Tithing overview metrics
        /// </summary>
        private void TithingOverviewMetricsDown()
        {
            DeleteMetric( TITHING_OVERVIEW_BY_CAMPUS_METRIC_GUID );
            DeleteMetric( TITHING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID );
            DeleteMetric( GIVING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID );

            RockMigrationHelper.DeleteCategory( TITHING_OVERVIEW_CATEGORY );

            Sql( $"DELETE FROM Schedule WHERE [Guid] = '{TITHING_OVERVIEW_SCHEDULE_GUID}'" );
        }

        private void AddSchedule()
        {
            Sql( $@"
DECLARE @ScheduleId int = ( 
    SELECT [Id] 
    FROM [dbo].[Schedule] 
    WHERE Guid = '{TITHING_OVERVIEW_SCHEDULE_GUID}'),

@MetricsCategoryId int = ( 
    SELECT [Id] 
    FROM [dbo].[Category] 
    WHERE Guid = '{SystemGuid.Category.SCHEDULE_METRICS}'
)
IF @ScheduleId IS NULL
BEGIN
	INSERT [dbo].[Schedule] ([Name], [Description], [iCalendarContent], [CategoryId], [Guid], [IsActive])
	VALUES (N'Tithing Overview', NULL, N'BEGIN:VCALENDAR
PRODID:-//github.com/SparkDevNetwork/Rock//NONSGML Rock//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:20240409T040200
DTSTAMP:20240409T040100
DTSTART:20240409T040000
RRULE:FREQ=WEEKLY;BYDAY=TU
SEQUENCE:0
UID:407d07c9-36a8-430e-bbea-8b5062e9b5a6
END:VEVENT
END:VCALENDAR',
@MetricsCategoryId, N'{TITHING_OVERVIEW_SCHEDULE_GUID}', 1)
END" );
        }

        private void AddTithingOverviewByCampusMetrics()
        {
            const string sql = @"DECLARE @StartDate int = FORMAT( DATEADD( d, -365, GETDATE()), 'yyyyMMdd' )
DECLARE @EndDate int = FORMAT( GETDATE(), 'yyyyMMdd' )
-- Only Include Person Type Records
DECLARE @PersonRecordTypeId INT = ( SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = '36CF10D6-C695-413D-8E7C-4546EFEF385E' )

;WITH CTE AS (
    SELECT
    [GivingLeaderId]
    , [PrimaryCampusId]
    , [PrimaryFamilyId]
    , SUM(ftd.[Amount]) AS [GivingAmount]
    , (SELECT TOP 1 
            LEFT([PostalCode], 5)
        FROM [dbo].[Location] l
        INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMappedLocation] = 1
      ) AS [PostalCode]
    , (SELECT [FamiliesMedianIncome] * .1 FROM [dbo].[AnalyticsSourcePostalCode] WHERE [PostalCode] = (SELECT TOP 1 LEFT([PostalCode], 5) FROM [dbo].[Location] l INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMappedLocation] = 1) ) AS [FamiliesMedianTithe]
	FROM
		[Person] p
		INNER JOIN [dbo].[PersonAlias] pa ON pa.[PersonId] = p.[Id]
		INNER JOIN [dbo].[FinancialTransaction] ft ON ft.[AuthorizedPersonAliasId] = pa.[Id]
		INNER JOIN [dbo].[FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
		INNER JOIN [dbo].[FinancialAccount] fa ON fa.[Id] = ftd.[AccountId] 
		INNER JOIN [dbo].[AnalyticsSourceDate] asd ON asd.[DateKey] = ft.[TransactionDateKey]
	WHERE
		fa.[IsTaxDeductible] = 1
		AND asd.[DateKey] > = @StartDate AND asd.[DateKey] <= @EndDate
-- Only include person type records.
AND p.[RecordTypeValueId] = @PersonRecordTypeId
	GROUP BY [GivingLeaderId], [PrimaryCampusId], [PrimaryFamilyId]
)
SELECT 
    CAST(COUNT(CASE WHEN [GivingAmount] > [FamiliesMedianTithe] THEN 1 END) AS FLOAT) / COUNT(*) * 100 AS PercentageAboveMedianTithe,
    [PrimaryCampusId],
    COUNT(*) AS TotalFamilies,
    COUNT(CASE WHEN [GivingAmount] > [FamiliesMedianTithe] THEN 1 END) AS FamiliesAboveMedianTithe
FROM 
    CTE
-- Only include families that have a postal code and/or we have a [FamiliesMedianIncome] value
WHERE ( [PostalCode] IS NOT NULL AND [PostalCode] != '') and [FamiliesMedianTithe] is NOT NULL
GROUP BY [PrimaryCampusId];";
            AddSqlSourcedMetric( TITHING_OVERVIEW_BY_CAMPUS_METRIC_GUID, "Tithing Overview By Campus (percent)", TITHING_OVERVIEW_CATEGORY, sql, "This a breakdown of the percentage of families above the median tithe for each campus. Only families with a recognized postal code are included in this metric value." );
        }

        private void AddTithingHouseholdsByCampusMetrics()
        {
            const string sql = @"DECLARE @StartDate int = FORMAT( DATEADD( d, -365, GETDATE()), 'yyyyMMdd' )
DECLARE @EndDate int = FORMAT( GETDATE(), 'yyyyMMdd' )
DECLARE @PersonRecordTypeId INT = ( SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = '36CF10D6-C695-413D-8E7C-4546EFEF385E' )

;WITH CTE AS (
    SELECT
    [GivingLeaderId]
    , [PrimaryCampusId]
    , [PrimaryFamilyId]
    , SUM(ftd.[Amount]) AS [GivingAmount]
    , (SELECT TOP 1 
            LEFT([PostalCode], 5)
        FROM [dbo].[Location] l
        INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMailingLocation] = 1
      ) AS [PostalCode]
    , (SELECT [FamiliesMedianIncome] * .1 FROM [dbo].[AnalyticsSourcePostalCode] WHERE [PostalCode] = (SELECT TOP 1 LEFT([PostalCode], 5) FROM [dbo].[Location] l INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMailingLocation] = 1) ) AS [FamiliesMedianTithe]
	FROM
		[Person] p
		INNER JOIN [dbo].[PersonAlias] pa ON pa.[PersonId] = p.[Id]
		INNER JOIN [dbo].[FinancialTransaction] ft ON ft.[AuthorizedPersonAliasId] = pa.[Id]
		INNER JOIN [dbo].[FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
		INNER JOIN [dbo].[FinancialAccount] fa ON fa.[Id] = ftd.[AccountId] 
		INNER JOIN [dbo].[AnalyticsSourceDate] asd ON asd.[DateKey] = ft.[TransactionDateKey]
	WHERE
		fa.[IsTaxDeductible] = 1
		AND asd.[DateKey] > = @StartDate AND asd.[DateKey] <= @EndDate
AND p.[RecordTypeValueId] = @PersonRecordTypeId
	GROUP BY [GivingLeaderId], [PrimaryCampusId], [PrimaryFamilyId]
)
SELECT
    SUM(CASE WHEN [GivingAmount] >= [FamiliesMedianTithe] THEN 1 ELSE 0 END) AS [TotalTithingHouseholds]
    , PrimaryCampusId AS [CampusId]
FROM CTE
WHERE 
   ( PostalCode IS NOT NULL AND PostalCode != '') and FamiliesMedianTithe is NOT NULL
GROUP BY PrimaryCampusId;";
            AddSqlSourcedMetric( TITHING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID, "Tithing Households Per Campus", TITHING_OVERVIEW_CATEGORY, sql, "This is the percent of households that are at/above the tithe. The tithe value is one tenth of the median family income for a family's location/postal code as determined by the AnalyticsSourcePostalCode table. Only families with a recognized postal code are included in this metric value." );
        }

        private void AddGivingHouseholdsOverviewByCampusMetrics()
        {
            const string sql = @"DECLARE @StartDate int = FORMAT( DATEADD( d, -365, GETDATE()), 'yyyyMMdd' )
DECLARE @EndDate int = FORMAT( GETDATE(), 'yyyyMMdd' )
DECLARE @PersonRecordTypeId INT = ( SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = '36CF10D6-C695-413D-8E7C-4546EFEF385E' )

;WITH CTE AS (
    SELECT
    [GivingLeaderId]
    , [PrimaryCampusId]
    , [PrimaryFamilyId]
    , SUM(ftd.[Amount]) AS [GivingAmount]
    , (SELECT TOP 1 
            LEFT([PostalCode], 5)
        FROM [dbo].[Location] l
        INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMailingLocation] = 1
      ) AS [PostalCode]
    , (SELECT [FamiliesMedianIncome] * .1 FROM [dbo].[AnalyticsSourcePostalCode] WHERE [PostalCode] = (SELECT TOP 1 LEFT([PostalCode], 5) FROM [dbo].[Location] l INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMailingLocation] = 1) ) AS [FamiliesMedianTithe]
	FROM
		[Person] p
		INNER JOIN [dbo].[PersonAlias] pa ON pa.[PersonId] = p.[Id]
		INNER JOIN [dbo].[FinancialTransaction] ft ON ft.[AuthorizedPersonAliasId] = pa.[Id]
		INNER JOIN [dbo].[FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
		INNER JOIN [dbo].[FinancialAccount] fa ON fa.[Id] = ftd.[AccountId] 
		INNER JOIN [dbo].[AnalyticsSourceDate] asd ON asd.[DateKey] = ft.[TransactionDateKey]
	WHERE
		fa.[IsTaxDeductible] = 1
		AND asd.[DateKey] > = @StartDate AND asd.[DateKey] <= @EndDate
AND p.[RecordTypeValueId] = @PersonRecordTypeId
	GROUP BY [GivingLeaderId], [PrimaryCampusId], [PrimaryFamilyId]
)
SELECT
    SUM(CASE WHEN [GivingAmount] >= 0 THEN 1 ELSE 0 END) AS [TotalGivingHouseholds]
    , PrimaryCampusId AS [CampusId]
FROM CTE
WHERE ( PostalCode IS NOT NULL AND PostalCode != '') and FamiliesMedianTithe is NOT NULL
GROUP BY PrimaryCampusId;";
            AddSqlSourcedMetric( GIVING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID, "Giving Households Per Campus", TITHING_OVERVIEW_CATEGORY, sql, "This is the percent of households per campus that are giving. Only families with a recognized postal code are included in this metric value." );
        }

        private void AddSqlSourcedMetric( string guid, string title, string categoryGuid, string sourceSql, string description = null )
        {
            var formattedTitle = title?.Replace( "'", "''" ) ?? throw new ArgumentNullException( nameof( title ) );
            var createMetricAndMetricCategorySql = $@"DECLARE @MetricId [int] = (SELECT [Id] FROM [dbo].[Metric] WHERE ([Guid] = '{guid}'))
    , @SourceValueTypeId [int] = (SELECT [Id] FROM [dbo].[DefinedValue] WHERE ([Guid] = '{SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_SQL}'))
    , @MetricCategoryId [int] = (SELECT [Id] FROM [dbo].[Category] WHERE ([Guid] = '{categoryGuid}'))
    , @Description [varchar] (max) = {( string.IsNullOrWhiteSpace( description ) ? "NULL" : $"'{description.Replace( "'", "''" )}'" )};

IF (@MetricId IS NULL AND @SourceValueTypeId IS NOT NULL AND @MetricCategoryId IS NOT NULL)
BEGIN
    DECLARE @Now [datetime] = GETDATE();
    INSERT INTO [dbo].[Metric]
    (
        [IsSystem]
        , [Title]
        , [Description]
        , [IsCumulative]
        , [SourceValueTypeId]
        , [SourceSql]
        , [ScheduleId]
        , [CreatedDateTime]
        , [ModifiedDateTime]
        , [Guid]
        , [NumericDataType]
        , [EnableAnalytics]
    )
    VALUES
    (
        0
        , '{formattedTitle}'
        , @Description
        , 0
        , @SourceValueTypeId
        , '{sourceSql.Replace( "'", "''" )}'
        , (SELECT [Id] FROM Schedule WHERE Guid = '{TITHING_OVERVIEW_SCHEDULE_GUID}')
        , @Now
        , @Now
        , '{guid}'
        , 1
        , 0
    );
    SET @MetricId = SCOPE_IDENTITY();
    INSERT INTO [dbo].[MetricCategory]
    (
        [MetricId]
        , [CategoryId]
        , [Order]
        , [Guid]
    )
    VALUES
    (
        @MetricId
        , @MetricCategoryId
        , 0
        , NEWID()
    );
    INSERT INTO [dbo].[MetricPartition]
    (
        [MetricId]
        , [Label]
        , [EntityTypeId]
        , [IsRequired]
        , [Order]
        , [CreatedDateTime]
        , [ModifiedDateTime]
        , [Guid]
    )
    VALUES
    (
        @MetricId
        , 'Campus'
        , (SELECT Id FROM [dbo].[EntityType] WHERE GUID = '{SystemGuid.EntityType.CAMPUS}')
        , 1
        , 0
        , @Now
        , @Now
        , NEWID()
    );
END
";

            Sql( createMetricAndMetricCategorySql );
        }

        private void DeleteMetric( string guid )
        {
            Sql( $@"DECLARE @MetricId [int] = (SELECT [Id] FROM [Metric] WHERE ([Guid] = '{guid}'));
IF (@MetricId IS NOT NULL)
BEGIN
    DELETE FROM [dbo].[MetricPartition] WHERE ([MetricId] = @MetricId);
    DELETE FROM [dbo].[MetricCategory] WHERE ([MetricId] = @MetricId);
    DELETE FROM [dbo].[Metric] WHERE ([Id] = @MetricId);
END" );
        }

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        private void TithingOverviewPageAndBlockUp()
        {
            // Add the Tithing Overview Page
            RockMigrationHelper.AddPage( true, "8D5917F1-4E0E-4F18-8815-62EFBF808995", SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Tithing Overview", "Shows high-level statistics of the tithing overview.", TITHING_OVERVIEW_PAGE, "UpdateAgeBracketValues" );

            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Blocks.Reporting.TithingOverview", TITHING_OVERVIEW_BLOCK_ENTITY_TYPE, true, true );

            // Add/Update the Tithing Overview block type
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Tithing Overview", "Shows high-level statistics of the tithing overview.", "Rock.Blocks.Reporting.TithingOverview", "Reporting", TITHING_OVERVIEW_BLOCK_TYPE );

            // Add Tithing Overview block to the Tithing Overview Page
            RockMigrationHelper.AddBlock( true, TITHING_OVERVIEW_PAGE, null, TITHING_OVERVIEW_BLOCK_TYPE, "Tithing Overview", "Main", "", "", 0, TITHING_OVERVIEW_BLOCK );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        private void TithingOverviewPageAndBlockDown()
        {
            // Delete the Tithing Overview Block
            RockMigrationHelper.DeleteBlock( TITHING_OVERVIEW_BLOCK );

            // Delete the Tithing Overview Page
            RockMigrationHelper.DeletePage( TITHING_OVERVIEW_PAGE );
        }

        #endregion

        #region AddMaxMindGeolocation

        /// <summary>
        /// Rename Observability HTTP module up.
        /// </summary>
        private void RenameObservabilityHttpModuleUp()
        {
            Sql( $@"
UPDATE [EntityType]
SET [Name] = 'Rock.Web.HttpModules.RockGateway'
    , [AssemblyName] = 'Rock' -- This will get overwritten with the correct assembly name on Rock startup.
    , [FriendlyName] = 'Rock Gateway'
WHERE [Guid] = '{SystemGuid.EntityType.HTTP_MODULE_ROCK_GATEWAY}';" );
        }

        /// <summary>
        /// Refactor PopulateInteractionSessionData job up.
        /// </summary>
        private void RefactorPopulateInteractionSessionDataJobUp()
        {
            // Attribute: Rock.Jobs.PopulateInteractionSessionData: IP Address Geocoding Component
            RockMigrationHelper.DeleteAttribute( "B58B9B93-779D-46DE-8308-E8BCAE7DC352" );

            // Attribute: Rock.Jobs.PopulateInteractionSessionData: Lookback Maximum (days)
            RockMigrationHelper.DeleteAttribute( "BF23E452-603F-41F9-A7BF-FB68E8296686" );

            // Attribute: Rock.Jobs.PopulateInteractionSessionData: Max Records To Process Per Run
            RockMigrationHelper.DeleteAttribute( "C1096F04-0ECF-4519-9ABF-0CBB58BFFEA8" );

            Sql( @"
UPDATE [ServiceJob]
SET [Description] = 'This job will update Interaction counts and Durations for InteractionSession records.'
WHERE [Guid] = 'C0A57A17-1FE9-4C52-96B9-2F6EA1433D00';" );
        }

        /// <summary>
        /// Remove "IP Address Service Location" page up.
        /// </summary>
        private void RemoveIpAddressServiceLocationPageUp()
        {
            // Remove Block
            // Name: Components, from Page: IP Address Location Service, Site: Rock RMS
            // from Page: IP Address Location Service, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "54805637-AB36-4F22-9F58-D49019D6335D" );

            try
            {
                // Delete Page
                // Internal Name: IP Address Location Service
                // Site: Rock RMS
                // Layout: Full Width
                RockMigrationHelper.DeletePage( "9B6AE429-EFBE-47BF-B4D3-E4FB85D37B49" );
            }
            catch
            {
                // Don't fail the whole migration if we're unable to delete this page.
            }
        }

        /// <summary>
        /// Remove "IP Address Service Location" page down.
        /// </summary>
        private void RemoveIpAddressServiceLocationPageDown()
        {
            // Add Page
            // Internal Name: IP Address Location Service
            // Site: Rock RMS
            RockMigrationHelper.AddPage( true, "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "IP Address Location Service", "", "9B6AE429-EFBE-47BF-B4D3-E4FB85D37B49", "fa fa-globe-americas" );

            // Add Page Route
            //  Page:IP Address Location Service
            // Route:admin/system/ip-location-services
            RockMigrationHelper.AddOrUpdatePageRoute( "9B6AE429-EFBE-47BF-B4D3-E4FB85D37B49", "admin/system/ip-location-services", "3D69707E-5F37-4F15-AFE8-903861DE8D90" );

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
        }

        /// <summary>
        /// Refactor PopulateInteractionSessionData job down.
        /// </summary>
        private void RefactorPopulateInteractionSessionDataJobDown()
        {
            // Attribute: Rock.Jobs.PopulateInteractionSessionData: IP Address Geocoding Component
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", "A7486B0E-4CA2-4E00-A987-5544C7DABA76", "Class", "Rock.Jobs.PopulateInteractionSessionData", "IP Address Geocoding Component", "IP Address Geocoding Component", @"The service that will perform the IP GeoCoding lookup for any new IPs that have not been GeoCoded. Not required to be set here because the job will use the first active component if one is not configured here.", 0, @"", "B58B9B93-779D-46DE-8308-E8BCAE7DC352", "IPAddressGeoCodingComponent" );

            // Attribute: Rock.Jobs.PopulateInteractionSessionData: Lookback Maximum (days)
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.PopulateInteractionSessionData", "Lookback Maximum (days)", "Lookback Maximum (days)", @"The number of days into the past the job should look for unmatched addresses in the InteractionSession table. (default 30 days)", 1, @"30", "BF23E452-603F-41F9-A7BF-FB68E8296686", "LookbackMaximumInDays" );

            // Attribute: Rock.Jobs.PopulateInteractionSessionData: Max Records To Process Per Run
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.PopulateInteractionSessionData", "Max Records To Process Per Run", "Max Records To Process Per Run", @"The number of unique IP addresses to process on each run of this job.", 2, @"50000", "C1096F04-0ECF-4519-9ABF-0CBB58BFFEA8", "HowManyRecords" );

            Sql( @"
UPDATE [ServiceJob]
SET [Description] = 'This job will create new InteractionSessionLocation records and / or link existing InteractionSession records to the corresponding InteractionSessionLocation record.'
WHERE [Guid] = 'C0A57A17-1FE9-4C52-96B9-2F6EA1433D00';" );
        }

        /// <summary>
        /// Rename Observability HTTP module down.
        /// </summary>
        private void RenameObservabilityHttpModuleDown()
        {
            Sql( $@"
UPDATE [EntityType]
SET [Name] = 'Rock.Web.HttpModules.Observability'
    , [AssemblyName] = 'Rock' -- This will get overwritten with the correct assembly name on Rock startup.
    , [FriendlyName] = 'Observability HTTP Module'
WHERE [Guid] = '{SystemGuid.EntityType.HTTP_MODULE_ROCK_GATEWAY}';" );
        }

        #endregion

        #region MigrationRollupsForV17_0_8

        /// <summary>
        /// PA: Add warning message in the Pre-HTML of obsoleted blocks and an entry to the Admin Checklist to remove the obsoleted blocks if they are present.
        /// </summary>
        private void AddWarningMessagesForActiveObsoletedBlock()
        {
            var sqlObsoletePersonProfile = $@"
DECLARE @ObsoletePersonBlockId INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '48BBB7A7-1E1D-461E-9B64-E9CAD815E9E1');
UPDATE [Block]
SET [PreHtml] = CONCAT([PreHtml], '<div class=""alert alert-danger"">This block will be removed in v17.0. Please replace it with either the Person Profile or the Person Recent Attendances blocks.</div>')
WHERE [BlockTypeId] = @ObsoletePersonBlockId
IF EXISTS(SELECT * FROM [Block] WHERE [BlockTypeId] = @ObsoletePersonBlockId)
BEGIN
-- Insert the entry to the Admin CheckList to delete the occurrences of the obsolete Person block from Rock
INSERT INTO [dbo].[DefinedValue] (
		[IsSystem]
    , [DefinedTypeId]
    , [Order]
    , [Value]
    , [Description]
    , [Guid]
    , [CreatedDateTime]
    , [ModifiedDateTime]
    , [IsActive])
VALUES (
		1
    , @AdminChecklistDefinedTypeId
    , @ORDER+1
    , 'Obsoleted Person Profile block will be removed in v17.0. Please replace it with the Person Profile or the Person Recent Attendances blocks.'
    , '<div class=""alert alert-danger"">Rock still has some instances of the obsoleted Person Profile (Obsolete) block. This block will be removed in v17.0. Please replace the instances with either the Person Profile or the Person Recent Attendances blocks.</div>'
    , '80343B7C-7731-473F-9947-15A3D74ABB02'
    , GETDATE()
    , GETDATE()
    , 1)
END";

            var sqlObsoleteLocation = $@"
DECLARE @ObsoleteLocationBlockId INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '00FC1DEA-FE34-41E3-BC0A-2EE9138091EC');
UPDATE [Block]
SET [PreHtml] = CONCAT([PreHtml], '<div class=""alert alert-danger"">This block will be removed in v17.0. Use Roster, Live Metrics, or Room Settings blocks instead.</div>')
WHERE [BlockTypeId] = @ObsoleteLocationBlockId
IF EXISTS(SELECT * FROM [Block] WHERE [BlockTypeId] = @ObsoleteLocationBlockId)
BEGIN
-- Insert the entry to delete the occurrences of the obsolete Location from Rock
INSERT INTO [dbo].[DefinedValue] (
		[IsSystem]
    , [DefinedTypeId]
    , [Order]
    , [Value]
    , [Description]
    , [Guid]
    , [CreatedDateTime]
    , [ModifiedDateTime]
    , [IsActive])
VALUES (
		1
    , @AdminChecklistDefinedTypeId
    , @ORDER+2
    , 'Obsoleted Locations block will be removed in v17.0. Please replace it with Roster or Live Metrics or Room Settings Blocks.'
    , '<div class=""alert alert-danger"">Rock still has some instances of the obsoleted Locations block. This block will be removed in v17.0. Please replace the instances with the Roster or the Live Metrics or the Room Settings blocks.</div>'
    , '84D3F58A-B427-4348-98B9-D1121376FB30'
    , GETDATE()
    , GETDATE()
    , 1)
END";

            var sqlObsoleteContributionStatementLava = $@"
DECLARE @ObsoleteContributionStatementLavaBlockId INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = 'AF986B72-ADD9-4E05-971F-1DE4EBED8667');
UPDATE [Block]
SET [PreHtml] = CONCAT([PreHtml], '<div class=""alert alert-danger"">This block will be removed in v17.0. Use the Contribution Statement Generator block instead.</div>')
WHERE [BlockTypeId] = @ObsoleteContributionStatementLavaBlockId
IF EXISTS(SELECT * FROM [Block] WHERE [BlockTypeId] = @ObsoleteContributionStatementLavaBlockId)
BEGIN
-- Insert the entry to delete the occurrences of the obsolete Contribution Statement Lava block from Rock
INSERT INTO [dbo].[DefinedValue] (
		[IsSystem]
    , [DefinedTypeId]
    , [Order]
    , [Value]
    , [Description]
    , [Guid]
    , [CreatedDateTime]
    , [ModifiedDateTime]
    , [IsActive])
VALUES (
		1
    , @AdminChecklistDefinedTypeId
    , @ORDER+3
    , 'Obsoleted Contribution Statement Generator block will be removed in v17.0. Please replace it with Contribution Statement Generator block.'
    , '<div class=""alert alert-danger"">Rock still has some instances of the obsoleted Contribution Statement Lava block. This block will be removed in v17.0. Please replace the instances with Contribution Statement Generator blocks.</div>'
    , '85A4AE6F-8B52-484B-B594-C9A15EEDEBE1'
    , GETDATE()
    , GETDATE()
    , 1)
END";

            var sqlObsoleteSystemEmailListBlock = $@"
DECLARE @ObsoleteSystemEmailList INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '2645A264-D5E5-43E8-8FE2-D351F3D5435B');
UPDATE [Block]
SET [PreHtml] = CONCAT([PreHtml], '<div class=""alert alert-danger"">This block will be removed in v17.0. Use System Communication List instead.</div>')
WHERE [BlockTypeId] = @ObsoleteSystemEmailList
IF EXISTS(SELECT * FROM [Block] WHERE [BlockTypeId] = @ObsoleteSystemEmailList AND [Guid] <> '68F10E30-BD74-49F5-B63F-DA671E31DA90')
BEGIN
-- Insert the entry to delete the occurrences of the obsolete Person block from Rock
INSERT INTO [dbo].[DefinedValue] (
		[IsSystem]
    , [DefinedTypeId]
    , [Order]
    , [Value]
    , [Description]
    , [Guid]
    , [CreatedDateTime]
    , [ModifiedDateTime]
    , [IsActive])
VALUES (
		1
    , @AdminChecklistDefinedTypeId
    , @ORDER+4
    , 'Obsoleted System Email List block will be removed in v17.0. Please replace it System Communication List block.'
    , '<div class=""alert alert-danger"">Rock still has some instances of the obsoleted System Email List block. This block will be removed in v17.0. Please replace the instances with the System Communication List block.</div>'
    , '6D0A5968-4BF3-43B0-866C-11D9779165F6'
    , GETDATE()
    , GETDATE()
    , 1)
END";

            var sqlObsoleteSystemEmailDetail = $@"
DECLARE @ObsoleteSystemEmailDetail INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '82B00455-B8CF-4673-ACF5-641B961DF59F');
UPDATE [Block]
SET [PreHtml] = CONCAT([PreHtml], '<div class=""alert alert-danger"">This block will be removed in v17.0. Use System Communication Detail block instead.</div>')
WHERE [BlockTypeId] = @ObsoleteSystemEmailDetail
IF EXISTS(SELECT * FROM [Block] WHERE [BlockTypeId] = @ObsoleteSystemEmailDetail AND [Guid] <> '707A99EB-C24A-46BB-9230-8607E674246C')
BEGIN
INSERT INTO [dbo].[DefinedValue] (
		[IsSystem]
    , [DefinedTypeId]
    , [Order]
    , [Value]
    , [Description]
    , [Guid]
    , [CreatedDateTime]
    , [ModifiedDateTime]
    , [IsActive])
VALUES (
		1
    , @AdminChecklistDefinedTypeId
    , @ORDER+5
    , 'Obsoleted System Email Detail block will be removed in v17.0. Please replace it with the System Communication Detail block.'
    , '<div class=""alert alert-danger"">Rock still has some instances of the obsoleted System Email Detail Block. This block will be removed in v17.0. Please replaces the instances with the System Communication Detail block.</div>'
    , 'EF040A12-DEAD-43D5-84D2-8FEE0C9CF365'
    , GETDATE()
    , GETDATE()
    , 1)
END";

            Sql( $@"
DECLARE @AdminChecklistDefinedTypeId INT = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '4BF34677-37E9-4E71-BD03-252B66C9373D');
DECLARE @Order INT
SELECT @Order = ISNULL(MAX([order]) + 1, 0) FROM [DefinedValue] WHERE [DefinedTypeId] = @AdminChecklistDefinedTypeId
-- Add entry for obsoleted Person Profile block
{sqlObsoletePersonProfile}
-- Add entry for obsoleted Location block
{sqlObsoleteLocation}
-- Add entry for obsoleted Contribution Statement Lava block
{sqlObsoleteContributionStatementLava}
-- Add entry for obsoleted System Email List block
{sqlObsoleteSystemEmailListBlock}
-- Add entry for obsoleted System Email Detail block
{sqlObsoleteSystemEmailDetail}
" );
        }

        /// <summary>
        /// PA: Chop Legacy DISC Results Block with DISC block
        /// </summary>
        private void ChopDISCBlock()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop DISC block",
                blockTypeReplacements:
            new Dictionary<string, string> {
            { "0549519D-4048-4B28-89CC-94493B29BBD4", Rock.SystemGuid.BlockType.DISC }
                },
                migrationStrategy:
            "Chop",
                jobGuid:
            SystemGuid.ServiceJob.DATA_MIGRATIONS_170_REMOVE_DISC_BLOCK,
                blockAttributeKeysToIgnore: null );
        }

        /// <summary>
        /// KH: Fix CMS Lava Layout
        /// </summary>
        private void UpdateCmsLayout()
        {
            RockMigrationHelper.AddBlockAttributeValue( "BEDFF750-3EB8-4EE7-A8B4-23863FB0315D", "1322186A-862A-4CF1-B349-28ECB67229BA", "{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" );

            RockMigrationHelper.DeletePage( "CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89" ); // Website Configuration Section
            RockMigrationHelper.DeletePage( "889D7F7F-EB0F-40CD-9E80-E58A00EE69F7" ); // Content Channels Section
            RockMigrationHelper.DeletePage( "B892DF6D-4789-4AC3-9E6C-2BFE0D9E30E4" ); // Personalization Section
            RockMigrationHelper.DeletePage( "04FE297E-D45E-44EC-B521-181423F05A1C" ); // Content Platform Section
            RockMigrationHelper.DeletePage( "82726ACD-3480-4514-A920-FE920A71C046" ); // Digital Media Applications Section
        }

        #endregion

        #region MigrationRollupsForV17_0_9

        #region KA: Migration to add New Weekly Metrics and MeasurementClassification Defined Values

        private const string AllowMultipleValues = "218CC5B7-AF55-49DF-8959-93459485BA3B";
        private const string WeeklyMetricsCategory = "64B29ADE-144D-4E84-96CC-A79398589733";

        // Defined Value Guids
        private const string TotalWeekendAttendanceValue = "B24ACB41-8B75-41DC-9B47-F289D8C9F04F";
        private const string VolunteerAttendanceValue = "6A2621BF-E600-428A-94C2-CCB79645FA27";
        private const string PrayerRequestsValue = "0428144F-F28D-4DFB-9C16-568751AE2B8E";
        private const string PrayersValue = "03B8B301-58B9-4DE3-87FB-C77A588EE258";
        private const string ActiveFamiliesValue = "26F7B2F5-4D3F-4D55-AA2E-23FAC4D40F7B";
        private const string BaptismsValue = "C1E5B1E5-3DB1-460E-9761-CF07CE105632";
        private const string GivingValue = "152F3B1C-8797-4ACA-9948-C1F9A000EA8B";
        private const string eRAWeeklyWinsValue = "3DB1FB2D-2A55-4F24-9FF6-6D7021574132";
        private const string eRAWeeklyLossesValue = "30489505-50BE-4A1B-8AD2-2D19853F820A";

        private const string ActiveFamiliesMetricPartitionGuid = "2B962606-6162-4C86-A46A-760EC9FF3486";

        private const string WeeklyScheduleGuid = "C31DF106-D7C8-4B64-81E7-5C4AB20DBA7B";
        private const string MonthlyScheduleGuid = "599A64CF-41CD-476C-AC0A-52BA9E71D354";

        // Metric Guids 
        private const string TotalWeekendAttendanceMetric = "89553EEE-91F3-4169-9D7C-04A17471E035";
        private const string VolunteerAttendanceMetric = "4F965AE3-D455-4346-988F-2A2B5E236C0C";
        private const string PrayerRequestsMetric = "2B5ECA35-47D8-4690-A8AD-72488485F2B4";
        private const string PrayersMetric = "685B7912-CB17-473B-90C1-2804F221931C";
        private const string ActiveFamiliesMetric = "491061B7-1834-44DA-8EA1-BB73B2D52AD3";
        private const string BaptismsMetric = "8B63D9D5-A82D-49D4-9AED-2EDBCF60FDEE";
        private const string GivingMetric = "43338E8A-622A-4195-B153-285E570B229D";
        private const string eRAWeeklyWinsMetric = "D05D685A-9A88-4375-A563-70BB44FBD237";
        private const string eRAWeeklyLossesMetric = "16A3FF64-31F0-4CFF-B5F4-83EEB69E0C25";

        private void NewWeeklyMetricsAndMeasurementClassificationDefinedValuesUp()
        {
            AddDefinedTypeAndAttribute();
            AddDefinedValues();
            AddMetricSchedules();
            AddMetrics();
        }

        private void AddMetricSchedules()
        {
            // Add Weekly Schedule.
            AddSchedule( WeeklyScheduleGuid, "Weekly Metric Schedule", @"
BEGIN:VCALENDAR
PRODID:-//github.com/SparkDevNetwork/Rock//NONSGML Rock//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:20240610T040100
DTSTAMP:20240610T144847
DTSTART:20240610T040000
RRULE:FREQ=WEEKLY;BYDAY=MO
SEQUENCE:0
UID:65398ce3-3a71-4261-a52d-ad28e49840c3
END:VEVENT
END:VCALENDAR" );

            // Add Monthly Schedule.
            AddSchedule( MonthlyScheduleGuid, "Monthly Metric Schedule", @"BEGIN:VCALENDAR
PRODID:-//github.com/SparkDevNetwork/Rock//NONSGML Rock//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:20240601T040100
DTSTAMP:20240610T144835
DTSTART:20240601T040000
RRULE:FREQ=MONTHLY;BYMONTHDAY=1
SEQUENCE:0
UID:ce40496e-b079-44b9-abb7-dc94d8a631f3
END:VEVENT
END:VCALENDAR" );
        }

        private void AddSchedule( string guid, string name, string schedule )
        {
            Sql( $@"
DECLARE @ScheduleId int = ( 
    SELECT [Id] 
    FROM dbo.[Schedule] 
    WHERE Guid = '{guid}'),

@MetricsCategoryId int = ( 
    SELECT [Id] 
    FROM dbo.[Category] 
    WHERE Guid = '5A794741-5444-43F0-90D7-48E47276D426'
)
IF @ScheduleId IS NULL
BEGIN
	INSERT [dbo].[Schedule] ([Name], [Description], [iCalendarContent], [CategoryId], [Guid], [IsActive], [IsPublic])
	VALUES ('{name}', NULL, '{schedule}',@MetricsCategoryId, '{guid}', 1, 0)
END" );
        }

        private void AddDefinedTypeAndAttribute()
        {
            // Add Measurement Classification Defined Type.
            RockMigrationHelper.AddDefinedType( "Metric",
                "Measurement Classification",
                "The values for this defined type will clarify what the metrics are measuring, enabling the system to utilize these metrics for analytics. The description of the defined values should outline any expected configuration for the metric, such as partitions, schedules, etc.",
                SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "Choose the purpose of this metric based on what you''re measuring." );

            // Add Allow Multiple Metrics attribute to the Classification Defined Type.
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                SystemGuid.FieldType.BOOLEAN,
                "Allow Multiple Metrics",
                "AllowMultipleMetrics",
                "This setting determines whether multiple metrics can share the same classification.",
                0,
                "False",
                AllowMultipleValues );
        }

        private void AddDefinedValues()
        {
            // Add Total Weekend Attendance Defined Value.
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "Total Weekend Attendance",
                "This metric measures the total weekend attendance for the organization, partitioned by Campus > Schedule.",
                TotalWeekendAttendanceValue );

            // Add Volunteer Attendance Defined Value.
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "Volunteer Attendance",
                "This metric measures the number of volunteers that served for the given week. This metric should be partitioned by Campus > Schedule.",
                VolunteerAttendanceValue );

            // Add Prayer Requests Defined Value.
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "Prayer Requests",
                "This metric measures the number of active prayer requests for the given week. This metric should be partitioned by Campus.",
                PrayerRequestsValue );

            // Add Prayers Defined Value.
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "Prayers",
                "This metrics measures the number of prayers for the given week. This metric should be partitioned by Campus.",
                PrayersValue );

            // Add Active Families Defined Value.
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "Active Families",
                "This metric represents the number of active families in the given week. This metric should be partitioned by Campus.",
                ActiveFamiliesValue );

            // Add Baptisms Defined Value.
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "Baptisms",
                "The metric that represents the number of baptisms in a given month. This metric should be partitioned by Campus.",
                BaptismsValue );

            // Add Giving Defined Value.
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "Giving",
                "This metric represents weekly giving to the tithe. It's up to each organization to define the financial accounts that make up this metric. This metric should be partitioned by Campus of the financial account.",
                GivingValue );

            // Add eRA Weekly Wins Defined Value.
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "eRA Weekly Wins",
                "The metric that tracks eRA wins by week. This metric should be partitioned by Campus.",
                eRAWeeklyWinsValue );

            // Add eRA Weekly Losses Defined Value.
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "eRA Weekly Losses",
                "The metric that tracks eRA losses by week. This metric should be partitioned by Campus.",
                eRAWeeklyLossesValue );
        }

        private void AddMetrics()
        {
            // Update Active Families Metric
            Sql( $@"
UPDATE [dbo].[Metric]
SET [SourceSql] = 'DECLARE @FamilyGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = ''790e3215-3b10-442b-af69-616c0dcb998e'')
SELECT COUNT( DISTINCT(g.[Id])) as ActiveFamilies, p.[PrimaryCampusId]
FROM [Person] p
    INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id]
    INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
WHERE ([RecordTypeValueId] = 1) AND ([RecordStatusValueId] = 3)
GROUP BY p.[PrimaryCampusId]',
[ScheduleId] = (SELECT [Id] FROM Schedule WHERE Guid = '{WeeklyScheduleGuid}'),
[MeasurementClassificationValueId] = (SELECT [Id] FROM dbo.[DefinedValue] WHERE [Guid] = '{ActiveFamiliesValue}')
WHERE [Guid] = '{ActiveFamiliesMetric}'
" );

            // Add Campus Partition to Active Families Metric
            Sql( $@"
DECLARE @ActiveFamiliesMetricId int = (SELECT TOP 1 [Id] FROM dbo.[Metric] WHERE [Guid] = '{ActiveFamiliesMetric}')
DECLARE @CampusEntityTypeId int = (SELECT TOP 1 [Id] FROM dbo.[EntityType] WHERE [Guid] = '{SystemGuid.EntityType.CAMPUS}')
IF NOT EXISTS (SELECT [Id] FROM dbo.[MetricPartition] WHERE [Guid] = 'F879279D-3484-4F58-A16D-F64BDB277358')
BEGIN
	INSERT INTO dbo.[MetricPartition]
		([MetricId], [Label],[EntityTypeId],[IsRequired],[Guid],[Order])
	VALUES 
		(@ActiveFamiliesMetricId, 'Campus', @CampusEntityTypeId, 1, 'F879279D-3484-4F58-A16D-F64BDB277358',0)
END
ELSE
BEGIN
 UPDATE dbo.[MetricPartition]
    SET [Label] = 'Campus',
     [EntityTypeId] = @CampusEntityTypeId
     WHERE [Guid] = 'F879279D-3484-4F58-A16D-F64BDB277358'
END
" );

            // Add Total Weekend Attendance Metric
            AddMetric( TotalWeekendAttendanceMetric,
                "Total Weekend Attendance",
                WeeklyMetricsCategory,
                @"
DECLARE @STARTDATE DATETIME = DATEADD(DAY, -7, GETDATE())
DECLARE @ENDDATE DATETIME = GETDATE() 

SELECT COUNT(1) as AttendanceCount, a.[CampusId], oa.[ScheduleId]
FROM [Attendance] a
INNER JOIN [AttendanceOccurrence] oa ON oa.Id = a.[OccurrenceId]
INNER JOIN [Group] g ON g.Id = oa.[GroupId]
INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
WHERE
   gt.[AttendanceCountsAsWeekendService] = 1
   AND a.[DidAttend] = 1 
   AND a.[StartDateTime] BETWEEN @STARTDATE AND @ENDDATE
GROUP BY ALL a.[CampusId], oa.[ScheduleId]
",
                new List<PartitionDetails>() { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ), new PartitionDetails( "Schedule", SystemGuid.EntityType.SCHEDULE ) },
                WeeklyScheduleGuid,
                "This metric represents attendance records (total for the week) for any group(s) per campus of GroupTypes that have the Weekend Service field checked.",
                TotalWeekendAttendanceValue );

            // Add Volunteer Attendance Metric
            AddMetric( VolunteerAttendanceMetric,
                "Volunteer Attendance",
                WeeklyMetricsCategory,
                @"
DECLARE @STARTDATE DATETIME = DATEADD(DAY, -7, GETDATE())
DECLARE @ENDDATE DATETIME = GETDATE()
DECLARE @ServiceAreaDefinedValueId INT = (SELECT Id FROM dbo.[DefinedValue] WHERE [Guid] = '36A554CE-7815-41B9-A435-93F3D52A2828')

SELECT COUNT(1) as AttendanceCount, a.[CampusId], oa.[ScheduleId]
FROM [Attendance] a
INNER JOIN [AttendanceOccurrence] oa ON oa.Id = a.[OccurrenceId]
INNER JOIN [Group] g ON g.Id = oa.[GroupId]
INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
WHERE
   gt.[GroupTypePurposeValueId] = @ServiceAreaDefinedValueId
   AND a.[DidAttend] = 1 
   AND a.[StartDateTime] BETWEEN @STARTDATE AND @ENDDATE
GROUP BY ALL a.[CampusId], oa.[ScheduleId]
",
                new List<PartitionDetails>() { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ), new PartitionDetails( "Schedule", SystemGuid.EntityType.SCHEDULE ) },
                WeeklyScheduleGuid,
                "This metric represents attendance records (total for the week) for any group(s) per campus of GroupTypes that have a Purpose of Serving Area.",
                VolunteerAttendanceValue );

            // Add Prayer Requests Metric
            AddMetric( PrayerRequestsMetric,
                "Prayer Requests",
                WeeklyMetricsCategory,
                @"
DECLARE @STARTDATE DATETIME = DATEADD(DAY, -7, GETDATE())
DECLARE @ENDDATE DATETIME = GETDATE()

SELECT COUNT(1) as PrayerRequests, p.[PrimaryCampusId]
FROM dbo.[PrayerRequest] pr
INNER JOIN [PersonAlias] pa ON pa.[Id] = pr.[RequestedByPersonAliasId]
INNER JOIN dbo.[Person] p ON p.[Id] = pa.[PersonId]
WHERE
   pr.[IsActive] = 1
   AND pr.[CreatedDateTime] BETWEEN @STARTDATE AND @ENDDATE
GROUP BY ALL p.[PrimaryCampusId]
",
                new List<PartitionDetails>() { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) },
                WeeklyScheduleGuid,
                "This metric represents the number of PrayerRequests created during the given week per campus.",
                PrayerRequestsValue );

            // Add Prayers Metric
            AddMetric( PrayersMetric,
                "Prayers",
                WeeklyMetricsCategory,
                @"
DECLARE @STARTDATE DATETIME = DATEADD(DAY, -7, GETDATE())
DECLARE @ENDDATE DATETIME = GETDATE() 

SELECT  COUNT(*) as Prayers, p.[PrimaryCampusId]
FROM dbo.[Interaction] i 
INNER JOIN [InteractionComponent] ic ON ic.[Id] = i.[InteractionComponentId]
INNER JOIN [InteractionChannel] ichan ON ichan.[Id] = ic.[InteractionChannelId]
INNER JOIN [PrayerRequest] pr ON pr.[Id] = ic.[EntityId]
INNER JOIN [PersonAlias] pa ON pa.[Id] = i.[PersonAliasId]
INNER JOIN [Person] p ON p.[Id] = pa.[PersonId]
WHERE 
   ichan.[Guid] = '3D49FB99-94D1-4F63-B1A2-30D4FEDE11E9'
   AND i.[Operation] = 'Prayed'
   AND i.[InteractionDateTime] BETWEEN @STARTDATE AND @ENDDATE
GROUP BY ALL p.[PrimaryCampusId]
",
                new List<PartitionDetails>() { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) },
                WeeklyScheduleGuid,
                "This metric uses prayer request Interaction records to determine the number of prayers offered (see Prayer Session page) during the given week per campus.",
                PrayersValue );

            // Add Baptisms Metric
            AddMetric( BaptismsMetric,
                "Baptisms",
                WeeklyMetricsCategory,
                $@"
DECLARE @STARTDATE DATETIME= DATEADD(mm, DATEDIFF(mm, 0, GETDATE()), 0)
DECLARE @ENDDATE DATETIME = DATEADD(DAY, -1, DATEADD(mm, 1, @STARTDATE));
DECLARE @BaptismDateAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = 'D42763FA-28E9-4A55-A25A-48998D7D7FEF')

SELECT COUNT(*) as Baptisms, p.PrimaryCampusId FROM Person p
JOIN dbo.[AttributeValue] av
ON p.[Id] = av.[EntityId]
WHERE av.[AttributeId] = @BaptismDateAttributeId
AND av.[ValueAsDateTime] >= @STARTDATE
AND av.[ValueAsDateTime] < @ENDDATE
GROUP BY ALL p.[PrimaryCampusId]
",
                new List<PartitionDetails>() { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) },
                MonthlyScheduleGuid,
                "This metric uses the system/core \"BaptismDate\" attribute and values to get a total number of people who were baptized within the given month per campus.",
                BaptismsValue );

            // Add Giving Metric
            AddMetric( GivingMetric,
                "Giving",
                WeeklyMetricsCategory,
                @"
-- =====================================================================================================
-- Description: This metric represents weekly giving to the tithe and should be partitioned by Campus.
-- =====================================================================================================
-- You can edit this to match the financial accounts that are considered part of the 'tithe', but please
-- do not change the remainder of this script:
 
DECLARE @Accounts VARCHAR(100) = '1';   -- Comma separated accounts to extract giving information from, their child accounts will be included.
 
-------------------------------------------------------------------------------------------------------
DECLARE @STARTDATE int = FORMAT( DATEADD(DAY, -7, GETDATE()), 'yyyyMMdd' )
DECLARE @ENDDATE int = FORMAT( GETDATE(), 'yyyyMMdd' )
DECLARE @PersonRecordTypeId INT = ( SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = '36CF10D6-C695-413D-8E7C-4546EFEF385E' )
DECLARE @AccountsWithChildren TABLE (Id INT);
-- Recursively get accounts and their children.
WITH AccountHierarchy AS (
    SELECT [Id]
    FROM dbo.[FinancialAccount] fa
    WHERE [Id] IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@Accounts, ','))
    UNION ALL
    SELECT e.[Id]
    FROM dbo.[FinancialAccount] e
    INNER JOIN AccountHierarchy ah ON e.[ParentAccountId] = ah.[Id]
)
INSERT INTO @AccountsWithChildren SELECT * FROM AccountHierarchy;

;WITH CTE AS (
    SELECT
	fa.[Name] AS AccountName
    , fa.[CampusId] AS AccountCampusId
    , [PrimaryFamilyId]
    , SUM(ftd.[Amount]) AS [GivingAmount]
	FROM
		[Person] p
		INNER JOIN [dbo].[PersonAlias] pa ON pa.[PersonId] = p.[Id]
		INNER JOIN [dbo].[FinancialTransaction] ft ON ft.[AuthorizedPersonAliasId] = pa.[Id]
		INNER JOIN [dbo].[FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
		INNER JOIN [dbo].[FinancialAccount] fa ON fa.[Id] = ftd.[AccountId] 
		INNER JOIN [dbo].[AnalyticsSourceDate] asd ON asd.[DateKey] = ft.[TransactionDateKey]
	WHERE
		fa.[IsTaxDeductible] = 1
		AND asd.[DateKey] > = @STARTDATE AND asd.[DateKey] <= @ENDDATE
		AND fa.[Id] IN (SELECT * FROM @AccountsWithChildren)
AND p.[RecordTypeValueId] = @PersonRecordTypeId
	GROUP BY fa.[CampusId], fa.[Name], [PrimaryFamilyId]
)
SELECT
    [GivingAmount] AS [GivingAmount]
    , [AccountCampusId] AS [CampusId]
	, [AccountName]
FROM CTE
GROUP BY [AccountCampusId], [AccountName], [GivingAmount];
",
                new List<PartitionDetails>() { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) },
                WeeklyScheduleGuid,
                "This metric represents weekly giving to the tithe per campus of the financial account.",
                GivingValue );

            // Add Weekly eRA Wins Metric
            AddMetric( eRAWeeklyWinsMetric,
                "Weekly eRA Wins",
                WeeklyMetricsCategory,
                @"
DECLARE @StartDate DATETIME = DATEADD(DAY, -7, GETDATE())
DECLARE @EndDate DATETIME = GETDATE()
DECLARE @EraStartDateAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = 'A106610C-A7A1-469E-4097-9DE6400FDFC2')
DECLARE @IsEraAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = 'CE5739C5-2156-E2AB-48E5-1337C38B935E')

SELECT COUNT(*) as eraWins, p.[PrimaryCampusId]
FROM dbo.[Person] p
JOIN dbo.[AttributeValue] av ON p.Id = av.[EntityId]
WHERE av.[AttributeId] = @EraStartDateAttributeId AND av.[ValueAsDateTime] BETWEEN @StartDate AND @EndDate
AND EXISTS (
    SELECT 1
    FROM dbo.[AttributeValue] av2
    WHERE av2.[EntityId] = p.Id
    AND av2.[AttributeId] = @IsEraAttributeId
    AND av2.ValueAsBoolean = 1
)
GROUP BY ALL p.[PrimaryCampusId];
",
                new List<PartitionDetails>() { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) },
                WeeklyScheduleGuid,
                "This metric tracks the number of individuals who attained eRA status within the current week per campus.",
                eRAWeeklyWinsValue );

            // Add Weekly eRA Losses Metric
            AddMetric( eRAWeeklyLossesMetric,
                "Weekly eRA Losses",
                WeeklyMetricsCategory,
                @"
DECLARE @StartDate DATETIME = DATEADD(DAY, -7, GETDATE())
DECLARE @EndDate DATETIME = GETDATE()
DECLARE @EraStartDateAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = '4711D67E-7526-9582-4A8E-1CD7BBE1B3A2')
DECLARE @IsEraAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = 'CE5739C5-2156-E2AB-48E5-1337C38B935E')

SELECT COUNT(*) as eraLosses, p.[PrimaryCampusId]
FROM dbo.[Person] p
JOIN dbo.[AttributeValue] av ON p.Id = av.[EntityId]
WHERE av.[AttributeId] = @EraStartDateAttributeId AND av.[ValueAsDateTime] BETWEEN @StartDate AND @EndDate
AND EXISTS (
    SELECT 1
    FROM dbo.[AttributeValue] av2
    WHERE av2.[EntityId] = p.Id
    AND av2.[AttributeId] = @IsEraAttributeId
    AND av2.ValueAsBoolean = 0
)
GROUP BY ALL p.[PrimaryCampusId];
",
                new List<PartitionDetails>() { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) },
                WeeklyScheduleGuid,
                "This metric monitors the number of individuals who exited eRA status within the current week per campus.",
                eRAWeeklyLossesValue );
        }

        private void AddMetric( string guid, string title, string categoryGuid, string sourceSql, List<PartitionDetails> partitions, string scheduleGuid, string description, string measurementClassificationValueGuid )
        {
            var formattedTitle = title?.Replace( "'", "''" ) ?? throw new ArgumentNullException( nameof( title ) );
            var createMetricAndMetricCategorySql = $@"DECLARE @MetricId [int] = (SELECT [Id] FROM dbo.[Metric] WHERE [Guid] = '{guid}')
    , @SourceValueTypeId [int] = (SELECT [Id] FROM dbo.[DefinedValue] WHERE [Guid] = '{SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_SQL}')
    , @MetricCategoryId [int] = (SELECT [Id] FROM dbo.[Category] WHERE [Guid] = '{categoryGuid}')
    , @Description [varchar] (max) = {( string.IsNullOrWhiteSpace( description ) ? "NULL" : $"'{description.Replace( "'", "''" )}'" )}
    , @MeasurementClassificationId [int] = (SELECT [Id] FROM dbo.[DefinedValue] WHERE [Guid] = '{measurementClassificationValueGuid}');

IF (@MetricId IS NULL AND @SourceValueTypeId IS NOT NULL AND @MetricCategoryId IS NOT NULL)
BEGIN
    DECLARE @Now [datetime] = GETDATE();
    INSERT INTO dbo.[Metric]
    (
        [IsSystem]
        , [Title]
        , [Description]
        , [IsCumulative]
        , [SourceValueTypeId]
        , [SourceSql]
        , [ScheduleId]
        , [CreatedDateTime]
        , [ModifiedDateTime]
        , [Guid]
        , [NumericDataType]
        , [EnableAnalytics]
        , [MeasurementClassificationValueId]
    )
    VALUES
    (
        0
        , '{formattedTitle}'
        , @Description
        , 0
        , @SourceValueTypeId
        , '{sourceSql.Replace( "'", "''" )}'
        , (SELECT [Id] FROM Schedule WHERE Guid = '{scheduleGuid}')
        , @Now
        , @Now
        , '{guid}'
        , 1
        , 0
        , @MeasurementClassificationId
    );
    SET @MetricId = SCOPE_IDENTITY();
    INSERT INTO dbo.[MetricCategory]
    (
        [MetricId]
        , [CategoryId]
        , [Order]
        , [Guid]
    )
    VALUES
    (
        @MetricId
        , @MetricCategoryId
        , 0
        , NEWID()
    );";
            var sqlBuilder = new StringBuilder( createMetricAndMetricCategorySql );

            if ( partitions == null || partitions.Count == 0 )
            {
                sqlBuilder.Append( @"INSERT INTO dbo.[MetricPartition]
    (
        [MetricId]
        , [IsRequired]
        , [Order]
        , [Guid]
    )
    VALUES
    (
        @MetricId
        , 1
        , 0
        , NEWID()
    );" );
            }
            else
            {
                foreach ( var partitionDetail in partitions )
                {
                    var createMetricPartitionSql = $@"INSERT INTO dbo.[MetricPartition]
    (
        [MetricId]
        , [Label]
        , [EntityTypeId]
        , [IsRequired]
        , [Order]
        , [CreatedDateTime]
        , [ModifiedDateTime]
        , [Guid]
    )
    VALUES
    (
        @MetricId
        , '{partitionDetail.Label}'
        , (SELECT Id FROM dbo.[EntityType] WHERE [GUID] = '{partitionDetail.EntityTypeGuid}')
        , 1
        , {partitions.IndexOf( partitionDetail )}
        , @Now
        , @Now
        , NEWID()
    );";
                    sqlBuilder.Append( createMetricPartitionSql );
                }
            }

            sqlBuilder.AppendLine( "END" );

            Sql( sqlBuilder.ToString() );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        private void NewWeeklyMetricsAndMeasurementClassificationDefinedValuesDown()
        {
            DeleteMetrics();
            DeleteMetricSchedules();
            DeleteDefinedValues();
            DeleteDefinedTypeAndAttribute();
        }

        private void DeleteMetricSchedules()
        {
            Sql( $"DELETE FROM dbo.[Schedule] WHERE [Guid] = '{WeeklyScheduleGuid}'" );
            Sql( $"DELETE FROM dbo.[Schedule] WHERE [Guid] = '{MonthlyScheduleGuid}'" );
        }

        private void DeleteDefinedTypeAndAttribute()
        {
            RockMigrationHelper.DeleteDefinedType( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION );
            RockMigrationHelper.DeleteAttribute( AllowMultipleValues );
        }

        private void DeleteDefinedValues()
        {
            RockMigrationHelper.DeleteDefinedValue( TotalWeekendAttendanceValue );
            RockMigrationHelper.DeleteDefinedValue( VolunteerAttendanceValue );
            RockMigrationHelper.DeleteDefinedValue( PrayerRequestsValue );
            RockMigrationHelper.DeleteDefinedValue( PrayersValue );
            RockMigrationHelper.DeleteDefinedValue( ActiveFamiliesValue );
            RockMigrationHelper.DeleteDefinedValue( BaptismsValue );
            RockMigrationHelper.DeleteDefinedValue( GivingValue );
            RockMigrationHelper.DeleteDefinedValue( eRAWeeklyWinsValue );
            RockMigrationHelper.DeleteDefinedValue( eRAWeeklyLossesValue );
        }

        private void DeleteMetrics()
        {
            Sql( $"DELETE FROM dbo.[MetricPartition] WHERE [Guid] = '{ActiveFamiliesMetricPartitionGuid}'" );

            Sql( @"
UPDATE [dbo].[Metric]
SET [SourceSql] = 'DECLARE @FamilyGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = ''790e3215-3b10-442b-af69-616c0dcb998e'')
SELECT COUNT( DISTINCT(g.[Id])) 
FROM [Person] p
    INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id]
    INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
WHERE ([RecordTypeValueId] = 1) AND ([RecordStatusValueId] = 3)',
[ScheduleId] = (SELECT Id FROM Schedule WHERE [Guid] = '717d75f1-644f-45a4-b25e-64652a270ad9'),
[MeasurementClassificationValueId] = NULL
WHERE [Guid] = '491061B7-1834-44DA-8EA1-BB73B2D52AD3'
" );

            Sql( $"DELETE FROM dbo.[Metric] WHERE [Guid] = '{TotalWeekendAttendanceMetric}'" );
            Sql( $"DELETE FROM dbo.[Metric] WHERE [Guid] = '{VolunteerAttendanceMetric}'" );
            Sql( $"DELETE FROM dbo.[Metric] WHERE [Guid] = '{PrayerRequestsMetric}'" );
            Sql( $"DELETE FROM dbo.[Metric] WHERE [Guid] = '{PrayersMetric}'" );
            Sql( $"DELETE FROM dbo.[Metric] WHERE [Guid] = '{BaptismsMetric}'" );
            Sql( $"DELETE FROM dbo.[Metric] WHERE [Guid] = '{GivingMetric}'" );
            Sql( $"DELETE FROM dbo.[Metric] WHERE [Guid] = '{eRAWeeklyWinsMetric}'" );
            Sql( $"DELETE FROM dbo.[Metric] WHERE [Guid] = '{eRAWeeklyLossesMetric}'" );
        }

        private sealed class PartitionDetails
        {
            public PartitionDetails( string label, string entityTypeGuid )
            {
                Label = label;
                EntityTypeGuid = entityTypeGuid;
            }

            /// <summary>
            /// Gets or sets the label.
            /// </summary>
            /// <value>
            /// The label.
            /// </value>
            public string Label { get; set; }

            /// <summary>
            /// Gets or sets the entity type unique identifier.
            /// </summary>
            /// <value>
            /// The entity type unique identifier.
            /// </value>
            public string EntityTypeGuid { get; set; }
        }

        #endregion

        #region KA: Add Location History Category Migration

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        private void AddLocationHistoryCategoryUp()
        {
            // Add Location History Category.
            RockMigrationHelper.UpdateCategory( "546D5F43-1184-47C9-8265-2D7BF4E1BCA5", "Location", "", "", Rock.SystemGuid.Category.HISTORY_LOCATION );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        private void AddLocationHistoryCategoryDown()
        {
            RockMigrationHelper.DeleteCategory( Rock.SystemGuid.Category.HISTORY_LOCATION );
        }

        #endregion

        #region KA: D: v17 Migration to add color attribute to Campus

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        private void UpdateTithingMetricsUp()
        {
            // Update Tithing overview page icon and hide description.
            Sql( $@"
UPDATE [dbo].[PAGE]
SET [PageDisplayDescription] = 0,
[IconCssClass] = 'fa fa-chart-bar'
WHERE [Guid] = '{TITHING_OVERVIEW_PAGE}' 
" );

            // Add Campus Color Attribute.
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.Campus",
                SystemGuid.FieldType.COLOR,
                string.Empty,
                string.Empty,
                "Color",
                "This color will be shown in certain places where multiple campuses are being visualized.",
                0,
                string.Empty,
                "B63C8C1A-58DC-4DDF-AA5D-C71F1E2D74B6",
                "core_CampusColor" );

            // Update Schedule, Set [IsPublic] to false and update execution time from 4am to 5am.
            Sql( $@"
UPDATE [dbo].[Schedule]
SET [IsPublic] = 0,
[iCalendarContent] = 'BEGIN:VCALENDAR
PRODID:-//github.com/SparkDevNetwork/Rock//NONSGML Rock//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:20240409T050200
DTSTAMP:20240529T183514
DTSTART:20240409T050000
RRULE:FREQ=WEEKLY;BYDAY=TU
SEQUENCE:0
UID:1b23207e-e482-4de1-af49-7e445d084739
END:VEVENT
END:VCALENDAR'
WHERE [Guid] = '{TITHING_OVERVIEW_SCHEDULE_GUID}'
" );

            // Update Tithing Overview by Campus description and SourceSql.
            Sql( $@"
UPDATE [dbo].[Metric]
SET [Description] = 'This a breakdown of the percentage of households at/above the median tithe for each campus. Only households with a recognized postal code are included in this metric value.',
[SourceSql] = 'DECLARE @StartDate int = FORMAT( DATEADD( d, -365, GETDATE()), ''yyyyMMdd'' )
DECLARE @EndDate int = FORMAT( GETDATE(), ''yyyyMMdd'' )
-- Only Include Person Type Records
DECLARE @PersonRecordTypeId INT = ( SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E'' )

;WITH CTE AS (
    SELECT
    [PrimaryCampusId]
    , [PrimaryFamilyId]
    , SUM(ftd.[Amount]) AS [GivingAmount]
    , (SELECT TOP 1 
            LEFT([PostalCode], 5)
        FROM [dbo].[Location] l
        INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMappedLocation] = 1
      ) AS [PostalCode]
    , (SELECT [FamiliesMedianIncome] * .1 FROM [dbo].[AnalyticsSourcePostalCode] WHERE [PostalCode] = (SELECT TOP 1 LEFT([PostalCode], 5) FROM [dbo].[Location] l INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMappedLocation] = 1) ) AS [FamiliesMedianTithe]
	FROM
		[Person] p
		INNER JOIN [dbo].[PersonAlias] pa ON pa.[PersonId] = p.[Id]
		INNER JOIN [dbo].[FinancialTransaction] ft ON ft.[AuthorizedPersonAliasId] = pa.[Id]
		INNER JOIN [dbo].[FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
		INNER JOIN [dbo].[FinancialAccount] fa ON fa.[Id] = ftd.[AccountId] 
		INNER JOIN [dbo].[AnalyticsSourceDate] asd ON asd.[DateKey] = ft.[TransactionDateKey]
	WHERE
		fa.[IsTaxDeductible] = 1
		AND asd.[DateKey] > = @StartDate AND asd.[DateKey] <= @EndDate
-- Only include person type records.
AND p.[RecordTypeValueId] = @PersonRecordTypeId
	GROUP BY [PrimaryCampusId], [PrimaryFamilyId]
)
SELECT 
    CAST(COUNT(CASE WHEN [GivingAmount] > [FamiliesMedianTithe] THEN 1 END) AS FLOAT) / COUNT(*) * 100 AS PercentageAboveMedianTithe,
    [PrimaryCampusId],
    COUNT(*) AS TotalFamilies,
    COUNT(CASE WHEN [GivingAmount] > [FamiliesMedianTithe] THEN 1 END) AS FamiliesAboveMedianTithe
FROM 
    CTE
-- Only include families that have a postal code and/or we have a [FamiliesMedianIncome] value
WHERE ( [PostalCode] IS NOT NULL AND [PostalCode] != '''') and [FamiliesMedianTithe] is NOT NULL
GROUP BY [PrimaryCampusId];'
WHERE [Guid] = '{TITHING_OVERVIEW_BY_CAMPUS_METRIC_GUID}'
" );

            // Update Tithing households by Campus description.
            Sql( $@"
UPDATE [dbo].[Metric]
SET [Description] = 'This is the percent of households that are at/above the tithe. The tithe value is one tenth of the median family income for a household''s location/postal code as determined by the AnalyticsSourcePostalCode table. Only households with a recognized postal code are included in this metric value'
WHERE [Guid] = '{TITHING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID}'
" );

            // Update Giving households by Campus description.
            Sql( $@"
UPDATE [dbo].[Metric]
SET [Description] = 'This is the percent of households per campus that are giving. Only households with a recognized postal code are included in this metric value.'
WHERE [Guid] = '{GIVING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID}'
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        private void UpdateTithingMetricsDown()
        {
            // Delete Campus Color Attribute
            RockMigrationHelper.DeleteAttribute( "B63C8C1A-58DC-4DDF-AA5D-C71F1E2D74B6" );
        }

        #endregion

        #region LMS Add Data

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        private void AddLmsDataUp()
        {
            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Model.LearningProgram", Rock.SystemGuid.EntityType.LEARNING_PROGRAM, true, false );
            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Model.LearningActivityCompletion", Rock.SystemGuid.EntityType.LEARNING_CLASS_ACTIVITY_COMPLETION, true, false );
            LmsEntityTypesPagesBlocksUp();
            AddSeedData();
            AddOrUpdateSendLearningActivityNotificationsJob();

            // Hide the Learning Navigation from the menu by default.
            Sql( $"UPDATE [Page] SET [DisplayInNavWhen] = {( int ) DisplayInNavWhen.Never} WHERE [Guid] = '84DBEC51-EE0B-41C2-94B3-F361C4B98879';" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        private void AddLmsDataDown()
        {
            DeleteSendLearningActivityNotificationsJob();
            LmsEntityTypesPagesBlocksDown();

            RemoveSeedData();
            RockMigrationHelper.DeleteEntityType( Rock.SystemGuid.EntityType.LEARNING_CLASS_ACTIVITY_COMPLETION );
        }

        /// <summary>
        /// Adds the LMS seed data to the database.
        /// </summary>
        private void AddSeedData()
        {
            Sql( @"
DECLARE @now DATETIMEOFFSET = SYSDATETIMEOFFSET();

/* Grading Systems */
-- Create Learning Grading Systems using well-known Identity (and GUID) values.
SET IDENTITY_INSERT [dbo].[LearningGradingSystem] ON;

INSERT [dbo].[LearningGradingSystem] (
	[Id]
	, [Name]
	, [Description]
	, [IsActive]
	, [CreatedDateTime]
	, [ModifiedDateTime]
	, [Guid]
)
SELECT Id, [Name], [Description], 1 IsActive, @now Created, @now Modified, [Guid]
FROM (
	SELECT 1 Id, 'Pass/Fail' [Name], 'The Pass/Fail grading system evaluates students simply as ""Pass"" if they meet the course requirements, or ""Fail"" if they do not.' [Description], '99D9914B-7CCB-4E32-BD2D-541ACD7A1B22' [Guid]
	UNION SELECT 2 Id, 'Letter Grade' [Name], 'The Letter Grade system assigns grades ranging from A (excellent) to F (failing), reflecting a student''s performance in a course. ' [Description], '1EBBA9F7-E3DF-4930-9677-639D0915CAA3' [Guid]
) seed
WHERE NOT EXISTS (
	SELECT 1
	FROM [dbo].[LearningGradingSystem] ex
	WHERE ex.Id = seed.Id
)

SET IDENTITY_INSERT [dbo].[LearningGradingSystem] OFF;


/* Grading System Scales */
-- Create Learning Grading System Scales using well-known Identity values.
SET IDENTITY_INSERT [dbo].[LearningGradingSystemScale] ON;

INSERT [dbo].[LearningGradingSystemScale] (
	[Id]
	, [Name]
	, [Description]
	, [ThresholdPercentage]
	, [IsPassing]
	, [Order]
	, [LearningGradingSystemId]
	, [CreatedDateTime]
	, [ModifiedDateTime]
	, [Guid]
)
SELECT Id, [Name], [Description], [Threshold], [IsPassing], [Order], [GradingSystemId], @now Created, @now Modified, [Guid]
FROM (
	SELECT 1 Id, 'Pass' [Name], 'Passes.' [Description], 70 [Threshold], 1 IsPassing, 1 [Order], 1 [GradingSystemId], 'C07A3227-7188-4D61-AC02-FF6AB8380AAD' [Guid]
	UNION SELECT 2 Id, 'Fail' [Name], 'Doesn''t Pass.' [Description], 0 [Threshold], 0 IsPassing, 2 [Order], 1 [GradingSystemId], 'BD209F2D-22E0-41A9-B425-ED42D515E13B' [Guid]
	UNION SELECT 3 Id, 'A' [Name], 'Passes with an ""A"".' [Description], 93 [Threshold], 1 IsPassing, 3 [Order], 2 [GradingSystemId], 'F96BDDD2-EA0F-4C35-90BB-0B7D9FAABD26' [Guid]
	UNION SELECT 4 Id, 'B' [Name], 'Passes with a ""B"".' [Description], 83 [Threshold], 1 IsPassing, 4 [Order], 2 [GradingSystemId], 'E8128844-04B0-4772-AB59-55F17645AB7A' [Guid]
	UNION SELECT 5 Id, 'C' [Name], 'Passes with a ""C"".' [Description], 73 [Threshold], 1 IsPassing, 5 [Order], 2 [GradingSystemId], 'A99DC539-D363-416F-BDA8-00163D186919' [Guid]
	UNION SELECT 6 Id, 'D' [Name], 'Fails with a ""D"".' [Description], 63 [Threshold], 0 IsPassing, 6 [Order], 2 [GradingSystemId], '6E6A61C3-3305-491D-80C6-1C3723468460' [Guid]
	UNION SELECT 7 Id, 'F' [Name], 'Fais with an ""F"".' [Description], 0 [Threshold], 0 IsPassing, 7 [Order], 2 [GradingSystemId], '2F7885F5-4DFB-4057-92D7-2684B4542BF7' [Guid]
) seed
WHERE NOT EXISTS (
	SELECT 1
	FROM [dbo].[LearningGradingSystemScale] ex
	WHERE ex.Id = seed.Id
)

SET IDENTITY_INSERT [dbo].[LearningGradingSystemScale] OFF;

/* Group Type */
-- Create an LMS Group Type using a well-known Guid.
DECLARE @lmsGroupTypeGuid uniqueidentifier = '4BBC41E2-0A37-4289-B7A7-756B9FE8F775';
DECLARE @mustBelongToCheckIn INT = 2;
DECLARE @attendancePrintToDefault INT = 0;
DECLARE @groupLocationPicker INT = 2;

INSERT [GroupType] ( [IsSystem], [Name], [GroupTerm], [GroupMemberTerm], [AllowMultipleLocations], [ShowInGroupList], [ShowInNavigation], [TakesAttendance], [AttendanceRule], [AttendancePrintTo], [Order], [LocationSelectionMode], [Guid] )
SELECT [IsSystem], [Name], [GroupTerm], [GroupMemberTerm], [AllowMultipleLocations], [ShowInGroupList], [ShowInNavigation], [TakesAttendance], [AttendanceRule], [AttendancePrintTo], [Order], [LocationSelectionMode], [Guid]
FROM (
	SELECT 1 [IsSystem], 'LMS Class' [Name], 'Class' [GroupTerm], 'Participant' [GroupMemberTerm], 1 [AllowMultipleLocations], 1 [ShowInGroupList], 0 [ShowInNavigation], 1 [TakesAttendance], @mustBelongToCheckIn [AttendanceRule], @attendancePrintToDefault [AttendancePrintTo], 1 [Order], @groupLocationPicker [LocationSelectionMode], @lmsGroupTypeGuid [Guid]
) seed
WHERE NOT EXISTS (
	SELECT 1
	FROM GroupType
	WHERE [Guid] = seed.[Guid]
)

/* Group Type Roles */
-- Get the LMS Group Type based on the well-known Guid used above.
DECLARE @lmsGroupType INT = (SELECT TOP 1 Id FROM GroupType WHERE [Guid] = @lmsGroupTypeGuid);

INSERT [GroupTypeRole] ( [IsSystem], [GroupTypeId], [Name], [Order], [IsLeader], [Guid], [Description] )
SELECT [IsSystem], [GroupTypeId], [Name], [Order], [IsLeader], [Guid], [Description]
FROM (
	SELECT 1 [IsSystem], @lmsGroupType [GroupTypeId], 'Facilitator' [Name], 2 [Order], 1 [IsLeader], '80F802CE-2F59-4AB1-ABD8-CFD7A009A00A' [Guid], 'Indicates the person is a facilitator/teacher/administrator of the class' [Description]
	UNION SELECT 1 [IsSystem], @lmsGroupType [GroupTypeId], 'Student' [Name], 1 [Order], 0 [IsLeader], 'FA3ACAC2-0377-484C-B888-974CA3BF2FF2' [Guid], 'Indicates the person is enrolled as a student in the class' [Description]
) seed
WHERE NOT EXISTS (
	SELECT 1
	FROM GroupTypeRole ex
	WHERE ex.[Guid] = seed.[Guid]
)

/* Category */
DECLARE @entityTypeId INT = (SELECT TOP 1 Id FROM [dbo].[EntityType] WHERE [Name] = 'Rock.Model.LearningActivityCompletion');
DECLARE @categoryGuid NVARCHAR(200) = '6d0d5e3a-944c-4de9-a436-8b9bf37b4879';

INSERT [Category] ( [IsSystem], [EntityTypeId], [Name], [IconCssClass], [Description], [Order], [Guid] )
SELECT [IsSystem], [EntityTypeId], [Name], [IconCssClass], [Description], [Order], [Guid]
FROM (
	SELECT 1 [IsSystem], @entityTypeId [EntityTypeId], 'Learning Management' [Name], 'fa fa-university' [IconCssClass], 'System Category for Learning Management' [Description], 0 [Order], @categoryGuid [Guid]
) [seed]
WHERE NOT EXISTS (
	SELECT 1
	FROM [Category] [ex]
	WHERE [ex].[Guid] = [seed].[Guid]
)

/* System Communication */
DECLARE @categoryId INT = (SELECT TOP 1 [Id] FROM [dbo].[Category] WHERE [Guid] = @categoryGuid);
DECLARE @activityCommunicationGuid UNIQUEIDENTIFIER = 'd40a9c32-f179-4e5e-9b0d-ce208c5d1870';

DECLARE @activityNotificatonCommunicationSubject NVARCHAR(200) = 'New {%if ActivityCount == 1 %}Activity{% else %}Activities{%endif%} Available'
DECLARE @activityNotificatonCommunicationBody NVARCHAR(MAX) = '
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd""><html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
    <meta name=""viewport"" content=""width=device-width"">
    <style type=""text/css"">
        .grid {
            display: grid;
            grid-template-columns: 33%;
        }

        .grid.header > div {
            margin-bottom: 4px;
            grid-row: 1;
            font-weight: bold;
        }

        .column-1 {
            grid-column: 1;
        }
        
        .column-2 {
            grid-column: 2;
        }
        
        .column-3 {
            grid-column: 3;
        }
    </style>
</head>
<body>
    <h2 style=""display: flex; justify-content: center;"">
        <span>
        {%if ActivityCount == 1 %}
            You have a new activity available!
        {% else %}
            You have new activities available!
        {%endif%}
        </span>
    </h2>
    <div>
        {% for course in Courses %}
            <h3 style=""display: flex; justify-content: center;"">
                <span>
                {{course.ProgramName}}: {{course.CourseName}} - {{course.CourseCode}}
                </span>
            </h3>
            <div class=""grid header"">
                <div>
                    Activity
                </div>

                <div>
                    Available as of
                </div>

                <div>
                    Due
                </div>
            </div>
            <div class=""grid"">
                {% for activity in course.Activities %}
                    <div class=""column-1"">
                        {{activity.ActivityName}}
                    </div>

                    <div class=""column-2"">
                        {% if activity.AvailableDate == null %}
                            Always
                        {% else %}
                            <dd>{{ activity.AvailableDate | HumanizeDateTime }}</dd>
                        {% endif %}
                    </div>

                    <div class=""column-3"">
                        {% if activity.DueDate == null %}
                            Optional
                        {% else %}
                            {{ activity.DueDate | HumanizeDateTime }}
                        {% endif %}
                    </div>				
                {% endfor %}
            </div>
        {% endfor %}
    </div>
</body>
';

INSERT [SystemCommunication] ( [IsSystem], [IsActive], [Title], [Subject], [Body], [Guid], [CategoryId] )
SELECT [IsSystem], [IsActive], [Title], [Subject], [Body], [Guid], [CategoryId]
FROM (
	SELECT 1 [IsSystem], 1 [IsActive], 'Learning Activity Available' [Title], @activityNotificatonCommunicationSubject [Subject], @activityNotificatonCommunicationBody [Body], @activityCommunicationGuid [Guid],  @categoryId [CategoryId]
) [seed]
WHERE NOT EXISTS (
	SELECT 1
	FROM [SystemCommunication] [ex]
	WHERE [ex].[Guid] = [seed].[Guid]
)

/* Binary File Type */
DECLARE @storageEntityTypeId INT = (
	SELECT TOP 1 [e].[Id]
	FROM [dbo].[EntityType] [e]
	WHERE [e].[Name] = 'Rock.Storage.Provider.Database'
);

INSERT [BinaryFileType] ( [IsSystem], [Name], [CacheToServerFileSystem], [Description], [IconCssClass], [StorageEntityTypeId], [CacheControlHeaderSettings], [Guid], [RequiresViewSecurity] )
SELECT [IsSystem], [Name], [CacheToServerFileSystem], [Description], [IconCssClass], [StorageEntityTypeId], [CacheControlHeaderSettings], [Guid], [RequiresViewSecurity]
FROM (
	SELECT 
		1 [IsSystem], 
		'Learning Management' [Name], 
		1 [CacheToServerFileSystem], 
		'File related to the Learning Management System (LMS).' [Description], 
		'fa fa-university' [IconCssClass], 
		@storageEntityTypeId [StorageEntityTypeId],
		'{
			""RockCacheablityType"": 3,
			""MaxAge"": null,
			""MaxSharedAge"": null
		}' [CacheControlHeaderSettings], 
		'4f55987b-5279-4d10-8c38-f320046b4bbb' [Guid],
        1 [RequiresViewSecurity]
) [seed]
WHERE NOT EXISTS (
	SELECT 1
	FROM [BinaryFileType] [ex]
	WHERE [ex].[Guid] = [seed].[Guid]
)
" );
        }

        /// <summary>
        /// Removes the LMS Seed data from the database.
        /// </summary>
        private void RemoveSeedData()
        {
            var activityCommunicationGuid = "d40a9c32-f179-4e5e-9b0d-ce208c5d1870";
            var lmsBinaryFileTypeGuid = "4f55987b-5279-4d10-8c38-f320046b4bbb";
            var activityCompletionCategoryGuid = "6d0d5e3a-944c-4de9-a436-8b9bf37b4879";
            var lmsGroupTypeGuid = "4BBC41E2-0A37-4289-B7A7-756B9FE8F775";

            Sql( $"DELETE [BinaryFileType] WHERE [Guid] = '{lmsBinaryFileTypeGuid}';" );
            Sql( $"DELETE [SystemCommunication] WHERE [Guid] = '{activityCommunicationGuid}'" );
            Sql( $"DELETE [Category] WHERE [Guid] = '{activityCompletionCategoryGuid}'" );
            Sql( $"DELETE gm FROM [GroupMember] gm JOIN [GroupType] gt ON gm.[GroupTypeId] = gt.[Id] WHERE gt.[Guid] = '{lmsGroupTypeGuid}'" );
            Sql( $"DELETE gtr FROM [GroupTypeRole] gtr JOIN [GroupType] gt ON gtr.[GroupTypeId] = gt.[Id] WHERE gt.[Guid] = '{lmsGroupTypeGuid}'" );
            Sql( $"DELETE g FROM [Group] g JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId] WHERE gt.[Guid] = '{lmsGroupTypeGuid}'" );
            Sql( $"DELETE [GroupType] WHERE [Guid] = '{lmsGroupTypeGuid}'" );
        }

        /// <summary>
        ///  Deletes the SendLearningNotifications Job based on Guid.
        /// </summary>
        private void DeleteSendLearningActivityNotificationsJob()
        {
            Sql( $"DELETE [ServiceJob] WHERE [Guid] = '{SystemGuid.ServiceJob.SEND_LEARNING_ACTIVITY_NOTIFICATIONS}'" );
        }

        /// <summary>
        /// Adds or Updates the SendLearningNotifications Job.
        /// </summary>
        private void AddOrUpdateSendLearningActivityNotificationsJob()
        {
            var cronSchedule = "0 0 7 1/1 * ? *"; // 7am daily.
            var jobClass = "Rock.Jobs.SendLearningNotifications";
            var name = "Send Learning Notifications";
            var description = "A job that sends notifications to students for newly available activities and class announcements.";

            Sql( $@"
            IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Guid] = '{SystemGuid.ServiceJob.SEND_LEARNING_ACTIVITY_NOTIFICATIONS}' )
            BEGIN
                INSERT INTO [ServiceJob] (
                    [IsSystem],
                    [IsActive],
                    [Name],
                    [Description],
                    [Class],
                    [CronExpression],
                    [NotificationStatus],
                    [Guid] )
                VALUES (
                    0,
                    1,
                    '{name}',
                    '{description}',
                    '{jobClass}',
                    '{cronSchedule}',
                    1,
                    '{SystemGuid.ServiceJob.SEND_LEARNING_ACTIVITY_NOTIFICATIONS}' );
            END
            ELSE
            BEGIN
	            UPDATE	[ServiceJob]
	            SET
		              [IsSystem] = 1
		            , [IsActive] = 1
		            , [Name] = '{name}'
		            , [Description] = '{description}'
		            , [Class] = '{jobClass}'
		            , [CronExpression] = '{cronSchedule}'
		            , [NotificationStatus] = 1
	            WHERE
		              [Guid] = '{SystemGuid.ServiceJob.SEND_LEARNING_ACTIVITY_NOTIFICATIONS}';
            END" );
        }

        /// <summary>
        /// Results of CodeGen_PagesBlocksAttributesMigration.sql for LMS (Up).
        /// </summary>
        private void LmsEntityTypesPagesBlocksUp()
        {
            // Add Page 
            //  Internal Name: Learning
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "48242949-944A-4651-B6CC-60194EDE08A0", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Learning", "", "84DBEC51-EE0B-41C2-94B3-F361C4B98879", "" );

            // Add Page 
            //  Internal Name: Program
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "84DBEC51-EE0B-41C2-94B3-F361C4B98879", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Program", "", "7888CAF4-AF5D-44BA-AB9E-80138361F69D", "" );

            // Add Page 
            //  Internal Name: Courses
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "7888CAF4-AF5D-44BA-AB9E-80138361F69D", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Courses", "", "870318D8-5381-4B3C-BE4A-04E57125B590", "" );

            // Add Page 
            //  Internal Name: Completions
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "7888CAF4-AF5D-44BA-AB9E-80138361F69D", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Completions", "", "532BC5A9-40B3-42AF-9AD3-740FC0B3EB41", "" );

            // Add Page 
            //  Internal Name: Semesters
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "7888CAF4-AF5D-44BA-AB9E-80138361F69D", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Semesters", "", "F7073393-D3A7-4C2E-8001-A73F9E169D60", "" );

            // Add Page 
            //  Internal Name: Grading Systems
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Grading Systems", "", "F76C9FC6-CDE0-4122-8D05-7862D683A3CA", "fa fa-graduation-cap" );

            // Add Page 
            //  Internal Name: Grading System
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "F76C9FC6-CDE0-4122-8D05-7862D683A3CA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Grading System", "", "6163CCFD-CB15-452E-99F2-229A5E5B22F0", "" );

            // Add Page 
            //  Internal Name: Course
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "870318D8-5381-4B3C-BE4A-04E57125B590", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Course", "", "A57D990E-6F34-45CF-ABAA-08C40E8D4844", "" );

            // Add Page 
            //  Internal Name: Grading Scale
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "6163CCFD-CB15-452E-99F2-229A5E5B22F0", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Grading Scale", "", "AE85B3FC-C951-497F-8C43-9D0A1E467A50", "" );

            // Add Page 
            //  Internal Name: Class
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "A57D990E-6F34-45CF-ABAA-08C40E8D4844", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Class", "", "23D5076E-C062-4987-9985-B3A4792BF3CE", "" );

            // Add Page 
            //  Internal Name: Activity
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "23D5076E-C062-4987-9985-B3A4792BF3CE", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Activity", "", "D72DCBC4-C57F-4028-B503-1954925EDC7D", "" );

            // Add Page 
            //  Internal Name: Participant
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "23D5076E-C062-4987-9985-B3A4792BF3CE", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Participant", "", "72C75C91-18F8-48D0-B0CF-1FBD82EB50FC", "" );

            // Add Page 
            //  Internal Name: Semester
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "F7073393-D3A7-4C2E-8001-A73F9E169D60", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Semester", "", "36FFA805-B283-443E-990D-87040339D16F", "" );

            // Add Page 
            //  Internal Name: Completion
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "D72DCBC4-C57F-4028-B503-1954925EDC7D", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Completion", "", "E0F2E4F1-ED10-49F6-B053-AC6807994204", "" );

            // Add Page 
            //  Internal Name: Content Page
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "23D5076E-C062-4987-9985-B3A4792BF3CE", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Content Page", "", "E6A89360-B7B4-48A8-B799-39A27EAB6F36", "" );

            // Add Page 
            //  Internal Name: Announcement
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "23D5076E-C062-4987-9985-B3A4792BF3CE", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Announcement", "", "7C8134A4-524A-4C3D-BA4C-875FEE672850", "" );

            // Add Page Route
            //   Page:Learning
            //   Route:learning
            RockMigrationHelper.AddOrUpdatePageRoute( "84DBEC51-EE0B-41C2-94B3-F361C4B98879", "learning", "A3A52449-4B51-45CE-B91D-3AEEB42C1577" );

            // Add Page Route
            //   Page:Program
            //   Route:learning/{LearningProgramId}
            RockMigrationHelper.AddOrUpdatePageRoute( "7888CAF4-AF5D-44BA-AB9E-80138361F69D", "learning/{LearningProgramId}", "84F3567D-6FA8-4B78-8224-51B6BF995558" );

            // Add Page Route
            //   Page:Courses
            //   Route:learning/{LearningProgramId}/courses
            RockMigrationHelper.AddOrUpdatePageRoute( "870318D8-5381-4B3C-BE4A-04E57125B590", "learning/{LearningProgramId}/courses", "D82F6906-AEF2-440A-B2A5-3DA95419CE14" );

            // Add Page Route
            //   Page:Completions
            //   Route:learning/{LearningProgramId}/completion
            RockMigrationHelper.AddOrUpdatePageRoute( "532BC5A9-40B3-42AF-9AD3-740FC0B3EB41", "learning/{LearningProgramId}/completion", "218FB50F-A231-4029-887D-0F921918ECB1" );

            // Add Page Route
            //   Page:Semesters
            //   Route:learning/{LearningProgramId}/semester
            RockMigrationHelper.AddOrUpdatePageRoute( "F7073393-D3A7-4C2E-8001-A73F9E169D60", "learning/{LearningProgramId}/semester", "E95CC988-2D64-4299-9BA2-E6928B191A33" );

            // Add Page Route
            //   Page:Grading Systems
            //   Route:admin/system/grading-system
            RockMigrationHelper.AddOrUpdatePageRoute( "F76C9FC6-CDE0-4122-8D05-7862D683A3CA", "admin/system/grading-system", "E895CCC1-119A-48B3-80E5-DA7B04BDAB51" );

            // Add Page Route
            //   Page:Grading System
            //   Route:admin/system/grading-system/{LearningGradingSystemId}
            RockMigrationHelper.AddOrUpdatePageRoute( "6163CCFD-CB15-452E-99F2-229A5E5B22F0", "admin/system/grading-system/{LearningGradingSystemId}", "B6D9D035-FB6B-45D4-B1EA-66E1E1B5C846" );

            // Add Page Route
            //   Page:Course
            //   Route:learning/{LearningProgramId}/courses/{LearningCourseId}
            RockMigrationHelper.AddOrUpdatePageRoute( "A57D990E-6F34-45CF-ABAA-08C40E8D4844", "learning/{LearningProgramId}/courses/{LearningCourseId}", "C77EBCB8-F174-4F2D-8113-48D9B0D489EA" );

            // Add Page Route
            //   Page:Grading Scale
            //   Route:admin/system/grading-system/{LearningGradingSystemId}/scale/{LearningGradingSystemScaleId}
            RockMigrationHelper.AddOrUpdatePageRoute( "AE85B3FC-C951-497F-8C43-9D0A1E467A50", "admin/system/grading-system/{LearningGradingSystemId}/scale/{LearningGradingSystemScaleId}", "D4C428AD-300D-4A07-AEC7-94E9518A4A7D" );

            // Add Page Route
            //   Page:Class
            //   Route:learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}
            RockMigrationHelper.AddOrUpdatePageRoute( "23D5076E-C062-4987-9985-B3A4792BF3CE", "learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}", "5FCE29A7-4530-4CCE-9891-C95242923EFE" );

            // Add Page Route
            //   Page:Activity
            //   Route:learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/activities/{LearningActivityId}
            RockMigrationHelper.AddOrUpdatePageRoute( "D72DCBC4-C57F-4028-B503-1954925EDC7D", "learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/activities/{LearningActivityId}", "E2581432-C9D8-4324-97E2-BCFE6BBD0F57" );

            // Add Page Route
            //   Page:Participant
            //   Route:learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/participants/{LearningParticipantId}
            RockMigrationHelper.AddOrUpdatePageRoute( "72C75C91-18F8-48D0-B0CF-1FBD82EB50FC", "learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/participants/{LearningParticipantId}", "827AF9A8-BF1A-4008-B4C3-3D07076ACB84" );

            // Add Page Route
            //   Page:Semester
            //   Route:learning/{LearningProgramId}/semester/{LearningSemesterId}
            RockMigrationHelper.AddOrUpdatePageRoute( "36FFA805-B283-443E-990D-87040339D16F", "learning/{LearningProgramId}/semester/{LearningSemesterId}", "D796B863-964F-4A10-A880-9D398376851A" );

            // Add Page Route
            //   Page:Completion
            //   Route:learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/activities/{LearningActivityId}/completions/{LearningActivityCompletionId}
            RockMigrationHelper.AddOrUpdatePageRoute( "E0F2E4F1-ED10-49F6-B053-AC6807994204", "learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/activities/{LearningActivityId}/completions/{LearningActivityCompletionId}", "8C40AE8D-60C6-49DE-B7DE-BE46D8A64AA6" );

            // Add Page Route
            //   Page:Content Page
            //   Route:learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/content-pages/{LearningClassContentPageId}
            RockMigrationHelper.AddOrUpdatePageRoute( "E6A89360-B7B4-48A8-B799-39A27EAB6F36", "learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/content-pages/{LearningClassContentPageId}", "4F35DC5D-FBB5-4B10-91B4-BC1C6A7009E8" );

            // Add Page Route
            //   Page:Announcement
            //   Route:learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/announcement/{LearningClassAnnouncementId}
            RockMigrationHelper.AddOrUpdatePageRoute( "7C8134A4-524A-4C3D-BA4C-875FEE672850", "learning/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/announcement/{LearningClassAnnouncementId}", "864A8615-AC20-4B3A-9D5F-087E92859AD1" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningClassList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningClassList", "Learning Class List", "Rock.Blocks.Lms.LearningClassList, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "AB72D147-D4CA-4FF5-AB49-696319CB9844" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningCourseDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningCourseDetail", "Learning Course Detail", "Rock.Blocks.Lms.LearningCourseDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "CB48C60A-E518-42E8-AA52-6A549A1A4152" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningCourseList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningCourseList", "Learning Course List", "Rock.Blocks.Lms.LearningCourseList, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "E882D582-BC31-4B68-945B-D0D44A2CE5BC" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningParticipantDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningParticipantDetail", "Learning Participant Detail", "Rock.Blocks.Lms.LearningParticipantDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "7F3752EF-7A4A-4F96-BD5C-E6609F0BFAC6" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningProgramCompletionList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningProgramCompletionList", "Learning Program Completion List", "Rock.Blocks.Lms.LearningProgramCompletionList, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "A4E78CB9-B53C-4188-BB9A-4435C051916D" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningProgramDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningProgramDetail", "Learning Program Detail", "Rock.Blocks.Lms.LearningProgramDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "41351A30-3B4F-44DA-B413-49D7C997FBB5" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningProgramList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningProgramList", "Learning Program List", "Rock.Blocks.Lms.LearningProgramList, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "ADEA7D88-0C99-4B0C-9784-6E97074C2742" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningSemesterList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningSemesterList", "Learning Semester List", "Rock.Blocks.Lms.LearningSemesterList, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "928978C0-9695-454D-9E17-33F12F278F78" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningGradingSystemList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningGradingSystemList", "Learning Grading System List", "Rock.Blocks.Lms.LearningGradingSystemList, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "10D43EAD-972C-4D6D-B5CB-A2D463247369" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningGradingSystemDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningGradingSystemDetail", "Learning Grading System Detail", "Rock.Blocks.Lms.LearningGradingSystemDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "C174C0EF-9085-4ED6-B21D-019E2AC04B12" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningGradingSystemScaleList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningGradingSystemScaleList", "Learning Grading System Scale List", "Rock.Blocks.Lms.LearningGradingSystemScaleList, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "FB5A07B0-CA85-460E-8700-2E57AE5194C8" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningGradingSystemScaleDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningGradingSystemScaleDetail", "Learning Grading System Scale Detail", "Rock.Blocks.Lms.LearningGradingSystemScaleDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "B14CB1A6-B60B-45B0-8F7D-457A869A25F2" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningActivityCompletionList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningActivityCompletionList", "Learning Activity Completion List", "Rock.Blocks.Lms.LearningActivityCompletionList, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "152FEA00-5721-4CB2-897F-1B6829F4B7C4" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningSemesterDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningSemesterDetail", "Learning Semester Detail", "Rock.Blocks.Lms.LearningSemesterDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "78BCF0D7-B5AC-4429-8055-B436652083A7" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningActivityDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningActivityDetail", "Learning Activity Detail", "Rock.Blocks.Lms.LearningActivityDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "FE13BFEF-6266-4667-B51F-01AF8E6C5B89" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningClassDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningClassDetail", "Learning Class Detail", "Rock.Blocks.Lms.LearningClassDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "08B8DA88-BE2E-4237-883D-B9A2DB5F6260" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningActivityCompletionDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningActivityCompletionDetail", "Learning Activity Completion Detail", "Rock.Blocks.Lms.LearningActivityCompletionDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "19474EB0-EEDA-4FCB-B1EA-A35E23E6F691" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.PublicLearningProgramList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.PublicLearningProgramList", "Public Learning Program List", "Rock.Blocks.Lms.PublicLearningProgramList, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "59D82730-E4A7-4AAF-BB1E-BEC4B7AA8624" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.PublicLearningCourseList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.PublicLearningCourseList", "Public Learning Course List", "Rock.Blocks.Lms.PublicLearningCourseList, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "4356FEBE-5EFD-421A-BFC4-05942B6BD910" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.PublicLearningCourseDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.PublicLearningCourseDetail", "Public Learning Course Detail", "Rock.Blocks.Lms.PublicLearningCourseDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "C5D5A151-038E-4295-A03C-63196883F68E" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.PublicLearningClassWorkspace
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.PublicLearningClassWorkspace", "Public Learning Class Workspace", "Rock.Blocks.Lms.PublicLearningClassWorkspace, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "1BF70976-85AC-43D3-B98A-0B87A2FFD9B6" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningClassAnnouncementDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningClassAnnouncementDetail", "Learning Class Announcement Detail", "Rock.Blocks.Lms.LearningClassAnnouncementDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "08429949-4774-41F7-8840-2D8DEFFF14AB" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningClassContentPageDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningClassContentPageDetail", "Learning Class Content Page Detail", "Rock.Blocks.Lms.LearningClassContentPageDetail, Rock.Blocks, Version=1.17.0.21, Culture=neutral, PublicKeyToken=null", false, false, "0220D970-1865-4B79-9E07-3CE9452E2B82" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Activity Completion Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningActivityCompletionDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Activity Completion Detail", "Displays the details of a particular learning activity completion.", "Rock.Blocks.Lms.LearningActivityCompletionDetail", "LMS", "4569F28D-1EFB-4B95-A506-0D9043C24775" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Activity Completion List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningActivityCompletionList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Activity Completion List", "Displays a list of learning activity completions.", "Rock.Blocks.Lms.LearningActivityCompletionList", "LMS", "EF1A5CDD-6769-4FFC-B826-55C194B01897" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Activity Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningActivityDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Activity Detail", "Displays the details of a particular learning activity.", "Rock.Blocks.Lms.LearningActivityDetail", "LMS", "4B18BF0D-D91B-4934-AC2D-A7188B15B893" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Activity List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningActivityList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Activity List", "Displays a list of learning activities.", "Rock.Blocks.Lms.LearningActivityList", "LMS", "5CEB6EC7-69F5-43B6-A74F-144A57F9B465" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Class Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningClassDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Class Detail", "Displays the details of a particular learning class.", "Rock.Blocks.Lms.LearningClassDetail", "LMS", "D5369F8D-11AA-482B-AE08-2B3C519D8D87" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Class List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningClassList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Class List", "Displays a list of learning classes.", "Rock.Blocks.Lms.LearningClassList", "LMS", "340F6CC1-8C38-4579-9383-A6168680194A" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Course Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningCourseDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Course Detail", "Displays the details of a particular learning course.", "Rock.Blocks.Lms.LearningCourseDetail", "LMS", "94C4CB0B-5617-4F46-B902-6E6DD4A447B8" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Course List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningCourseList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Course List", "Displays a list of learning courses.", "Rock.Blocks.Lms.LearningCourseList", "LMS", "D0AFDF98-4AFC-4E4F-A6E2-07CA4E7358E8" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Grading System Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningGradingSystemDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Grading System Detail", "Displays the details of a particular learning grading system.", "Rock.Blocks.Lms.LearningGradingSystemDetail", "LMS", "A4182806-95B0-49AE-97B5-246A834156E3" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Grading System List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningGradingSystemList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Grading System List", "Displays a list of learning grading systems.", "Rock.Blocks.Lms.LearningGradingSystemList", "LMS", "F003DAAC-B9FE-4007-B218-983084E1126B" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Grading System Scale Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningGradingSystemScaleDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Grading System Scale Detail", "Displays the details of a particular learning grading system scale.", "Rock.Blocks.Lms.LearningGradingSystemScaleDetail", "LMS", "34D7A280-8D4D-4FE5-BB45-810732F76341" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Grading System Scale List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningGradingSystemScaleList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Grading System Scale List", "Displays a list of learning grading system scales.", "Rock.Blocks.Lms.LearningGradingSystemScaleList", "LMS", "27390ED3-57B2-42EF-A212-F8B29851F9BA" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Participant Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningParticipantDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Participant Detail", "Displays the details of a particular learning participant.", "Rock.Blocks.Lms.LearningParticipantDetail", "LMS", "F1179439-31A1-4897-AB2E-B991D60455AA" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Program Completion List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningProgramCompletionList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Program Completion List", "Displays a list of learning program completions.", "Rock.Blocks.Lms.LearningProgramCompletionList", "LMS", "CE703EB6-028F-47FC-A09A-AD8F72C12CBC" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Program Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningProgramDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Program Detail", "Displays the details of a particular learning program.", "Rock.Blocks.Lms.LearningProgramDetail", "LMS", "796C87E7-678F-4A38-8C04-A401A4F7AC21" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Program List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningProgramList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Program List", "Displays a list of learning programs.", "Rock.Blocks.Lms.LearningProgramList", "LMS", "7B1DB013-A552-455F-A1D0-7B17488D0D5C" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Semester Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningSemesterDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Semester Detail", "Displays the details of a particular learning semester.", "Rock.Blocks.Lms.LearningSemesterDetail", "LMS", "97B2E57F-3A03-490D-834F-CD3640C7FF1E" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Semester List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningSemesterList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Semester List", "Displays a list of learning semesters.", "Rock.Blocks.Lms.LearningSemesterList", "LMS", "C89C7F15-FB8A-43D4-9AFB-5E40E397F246" );

            // Add/Update Obsidian Block Type
            //   Name:Public Learning Class Workspace
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.PublicLearningClassWorkspace
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Public Learning Class Workspace", "The main block for interacting with enrolled classes.", "Rock.Blocks.Lms.PublicLearningClassWorkspace", "LMS", "55F2E89B-DE57-4E24-AC6C-576956FB97C5" );

            // Add/Update Obsidian Block Type
            //   Name:Public Learning Course Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.PublicLearningCourseDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Public Learning Course Detail", "Displays the details of a particular public learning course.", "Rock.Blocks.Lms.PublicLearningCourseDetail", "LMS", "B0DCE130-0C91-4AA0-8161-57E8FA523392" );

            // Add/Update Obsidian Block Type
            //   Name:Public Learning Course List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.PublicLearningCourseList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Public Learning Course List", "Displays a list of public learning courses.", "Rock.Blocks.Lms.PublicLearningCourseList", "LMS", "5D6BA94F-342A-4EC1-B024-FC5046FFE14D" );

            // Add/Update Obsidian Block Type
            //   Name:Public Learning Program List
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.PublicLearningProgramList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Public Learning Program List", "Displays a list of public learning programs.", "Rock.Blocks.Lms.PublicLearningProgramList", "LMS", "DA1460D8-E895-4B23-8A8E-10EBBED3990F" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Class Announcement Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningClassAnnouncementDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Class Announcement Detail", "Displays the details of a particular learning class announcement.", "Rock.Blocks.Lms.LearningClassAnnouncementDetail", "LMS", "53C12A53-773E-4398-8627-DD44C1421675" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Class Content Page Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningClassContentPageDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Class Content Page Detail", "Displays the details of a particular learning class content page.", "Rock.Blocks.Lms.LearningClassContentPageDetail", "LMS", "92E0DB96-0FFF-41F2-982D-F750AB23EF5B" );

            // Add Block 
            //  Block Name: Learning Program List
            //  Page Name: Learning
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "84DBEC51-EE0B-41C2-94B3-F361C4B98879".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "7B1DB013-A552-455F-A1D0-7B17488D0D5C".AsGuid(), "Learning Program List", "Main", @"", @"", 1, "B7D8BC2F-CEDD-4E22-8D46-69A759DCB59C" );

            // Add Block 
            //  Block Name: Current Classes
            //  Page Name: Program
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "7888CAF4-AF5D-44BA-AB9E-80138361F69D".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "340F6CC1-8C38-4579-9383-A6168680194A".AsGuid(), "Current Classes", "Main", @"", @"", 2, "53A87507-D270-4340-A93F-AD3AB023E0B1" );

            // Add Block 
            //  Block Name: Program
            //  Page Name: Program
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "7888CAF4-AF5D-44BA-AB9E-80138361F69D".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "796C87E7-678F-4A38-8C04-A401A4F7AC21".AsGuid(), "Program", "Main", @"", @"", 1, "D4ED6CD7-C28E-4ABB-90B3-39499BE41E1B" );

            // Add Block 
            //  Block Name: Learning Course List
            //  Page Name: Courses
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "870318D8-5381-4B3C-BE4A-04E57125B590".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D0AFDF98-4AFC-4E4F-A6E2-07CA4E7358E8".AsGuid(), "Learning Course List", "Main", @"", @"", 0, "60979468-1F45-467B-A600-B1A230BF6CB9" );

            // Add Block 
            //  Block Name: Program Completions
            //  Page Name: Completions
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "532BC5A9-40B3-42AF-9AD3-740FC0B3EB41".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CE703EB6-028F-47FC-A09A-AD8F72C12CBC".AsGuid(), "Program Completions", "Main", @"", @"", 0, "ADCFF541-8908-4F78-9B96-2C3E37D5C4AB" );

            // Add Block 
            //  Block Name: Semesters
            //  Page Name: Semesters
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "F7073393-D3A7-4C2E-8001-A73F9E169D60".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "C89C7F15-FB8A-43D4-9AFB-5E40E397F246".AsGuid(), "Semesters", "Main", @"", @"", 0, "E58406FD-E17B-4480-95EB-ADCFD956CA17" );

            // Add Block 
            //  Block Name: Learning Grading System List
            //  Page Name: Grading Systems
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "F76C9FC6-CDE0-4122-8D05-7862D683A3CA".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "F003DAAC-B9FE-4007-B218-983084E1126B".AsGuid(), "Learning Grading System List", "Main", @"", @"", 0, "AF6C3E7F-C7EB-40ED-BCE2-038FABC6A868" );

            // Add Block 
            //  Block Name: Learning Grading System Detail
            //  Page Name: Grading System
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6163CCFD-CB15-452E-99F2-229A5E5B22F0".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "A4182806-95B0-49AE-97B5-246A834156E3".AsGuid(), "Learning Grading System Detail", "Main", @"", @"", 0, "88233998-6298-40F8-B433-931486D30B2D" );

            // Add Block 
            //  Block Name: Grading Scales
            //  Page Name: Grading System
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6163CCFD-CB15-452E-99F2-229A5E5B22F0".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "27390ED3-57B2-42EF-A212-F8B29851F9BA".AsGuid(), "Grading Scales", "Main", @"", @"", 1, "FD56EBFE-8BC5-42B7-8847-941B58AD14B6" );

            // Add Block 
            //  Block Name: Classes
            //  Page Name: Course
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "A57D990E-6F34-45CF-ABAA-08C40E8D4844".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "340F6CC1-8C38-4579-9383-A6168680194A".AsGuid(), "Classes", "Main", @"", @"", 1, "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0" );

            // Add Block 
            //  Block Name: Learning Course Detail
            //  Page Name: Course
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "A57D990E-6F34-45CF-ABAA-08C40E8D4844".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "94C4CB0B-5617-4F46-B902-6E6DD4A447B8".AsGuid(), "Learning Course Detail", "Main", @"", @"", 0, "D85084D3-E298-4307-9AA2-C1570C4A3FA4" );

            // Add Block 
            //  Block Name: Grading Scale
            //  Page Name: Grading Scale
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "AE85B3FC-C951-497F-8C43-9D0A1E467A50".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "34D7A280-8D4D-4FE5-BB45-810732F76341".AsGuid(), "Grading Scale", "Main", @"", @"", 0, "5FBFF087-3366-4620-8C5C-6A12F8BC6BD2" );

            // Add Block 
            //  Block Name: Class
            //  Page Name: Class
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "23D5076E-C062-4987-9985-B3A4792BF3CE".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D5369F8D-11AA-482B-AE08-2B3C519D8D87".AsGuid(), "Class", "Main", @"", @"", 0, "C67D2164-33E5-46C0-94EF-DF387EF8477D" );

            // Add Block 
            //  Block Name: Learning Activity Detail
            //  Page Name: Activity
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "D72DCBC4-C57F-4028-B503-1954925EDC7D".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "4B18BF0D-D91B-4934-AC2D-A7188B15B893".AsGuid(), "Learning Activity Detail", "Main", @"", @"", 0, "8BDFF6BF-E043-4EB5-BDD9-C1813AE83295" );

            // Add Block 
            //  Block Name: Activity List
            //  Page Name: Activity
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "D72DCBC4-C57F-4028-B503-1954925EDC7D".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "EF1A5CDD-6769-4FFC-B826-55C194B01897".AsGuid(), "Activity List", "Main", @"", @"", 1, "7CE84856-A789-4D74-AB6C-DA05A5C82479" );

            // Add Block 
            //  Block Name: Participant
            //  Page Name: Participant
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "72C75C91-18F8-48D0-B0CF-1FBD82EB50FC".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "F1179439-31A1-4897-AB2E-B991D60455AA".AsGuid(), "Participant", "Main", @"", @"", 0, "8C05CAC8-AB57-46F2-8E0C-21293BE8F464" );

            // Add Block 
            //  Block Name: Semester
            //  Page Name: Semester
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "36FFA805-B283-443E-990D-87040339D16F".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "97B2E57F-3A03-490D-834F-CD3640C7FF1E".AsGuid(), "Semester", "Main", @"", @"", 0, "2DF1C8E3-B65E-47F0-8B82-86AA5A3DA21A" );

            // Add Block 
            //  Block Name: Activity Completion Detail
            //  Page Name: Completion
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "E0F2E4F1-ED10-49F6-B053-AC6807994204".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "4569F28D-1EFB-4B95-A506-0D9043C24775".AsGuid(), "Activity Completion Detail", "Main", @"", @"", 0, "43D3D01A-101E-494E-AF4D-BBEE9595DE0A" );

            // Add Block 
            //  Block Name: Class Content Page
            //  Page Name: Class Content Page
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "E6A89360-B7B4-48A8-B799-39A27EAB6F36".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "92E0DB96-0FFF-41F2-982D-F750AB23EF5B".AsGuid(), "Class Content Page", "Main", @"", @"", 0, "9788FDD8-A587-48CA-BE0B-D5A937CCEE70" );

            // Add Block 
            //  Block Name: Class Announcement Detail
            //  Page Name: Announcement
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "7C8134A4-524A-4C3D-BA4C-875FEE672850".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "53C12A53-773E-4398-8627-DD44C1421675".AsGuid(), "Class Announcement Detail", "Main", @"", @"", 0, "3EAEE2BB-E223-4C47-9C9D-230A412D6D2A" );

            // update block order for pages with new blocks if the page,zone has multiple blocks

            // Update Order for Page: Activity,  Zone: Main,  Block: Activity List
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '7CE84856-A789-4D74-AB6C-DA05A5C82479'" );

            // Update Order for Page: Activity,  Zone: Main,  Block: Learning Activity Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '8BDFF6BF-E043-4EB5-BDD9-C1813AE83295'" );

            // Update Order for Page: Course,  Zone: Main,  Block: Classes
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '0AE21CCB-BE0C-4565-8EC9-33A61C503DC0'" );

            // Update Order for Page: Course,  Zone: Main,  Block: Learning Course Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'D85084D3-E298-4307-9AA2-C1570C4A3FA4'" );

            // Update Order for Page: Grading System,  Zone: Main,  Block: Grading Scales
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'FD56EBFE-8BC5-42B7-8847-941B58AD14B6'" );

            // Update Order for Page: Grading System,  Zone: Main,  Block: Learning Grading System Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '88233998-6298-40F8-B433-931486D30B2D'" );

            // Update Order for Page: Learning,  Zone: Main,  Block: Learning Program List
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'B7D8BC2F-CEDD-4E22-8D46-69A759DCB59C'" );

            // Update Order for Page: Program,  Zone: Main,  Block: Current Classes
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '53A87507-D270-4340-A93F-AD3AB023E0B1'" );

            // Update Order for Page: Program,  Zone: Main,  Block: Program
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'D4ED6CD7-C28E-4ABB-90B3-39499BE41E1B'" );

            // Attribute for BlockType
            //   BlockType: Learning Activity List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5CEB6EC7-69F5-43B6-A74F-144A57F9B465", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "B4130B6F-E3DE-4161-A28D-B7F6F160CB38" );

            // Attribute for BlockType
            //   BlockType: Learning Activity List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5CEB6EC7-69F5-43B6-A74F-144A57F9B465", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "8783E0C3-6B14-4252-A912-A13C4D1E89B0" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: Show Location Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "340F6CC1-8C38-4579-9383-A6168680194A", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Show Location Column", "ShowLocationColumn", "Show Location Column", @"Select 'Show' to show the 'Location'.", 1, @"No", "4FBE92ED-3CF1-4BD4-A78E-B50C0080FA0B" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: Show Schedule Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "340F6CC1-8C38-4579-9383-A6168680194A", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Show Schedule Column", "ShowScheduleColumn", "Show Schedule Column", @"Select 'Show' to show the 'Schedule' column.", 2, @"No", "BECBD2AE-5D1E-4065-8CAA-FE7972CC5742" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: Show Semester Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "340F6CC1-8C38-4579-9383-A6168680194A", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Show Semester Column", "ShowSemesterColumn", "Show Semester Column", @"Select 'Show' to show the 'Semester' column when the configuration is 'Academic Calendar'.", 3, @"No", "28087DBE-A334-47BF-9CA5-76AFF5EE1A6F" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: Display Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "340F6CC1-8C38-4579-9383-A6168680194A", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Mode", "DisplayMode", "Display Mode", @"Select 'Show only Acadmemic Calendar Mode' to show the block only when the configuration mode is 'Academic Calendar'.", 4, @"AcademicCalendarOnly", "DA69DB3C-139E-4DC6-BBA5-DC03C0ACBD6A" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "340F6CC1-8C38-4579-9383-A6168680194A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the learning class details.", 5, @"", "0C995F98-F483-4814-B3A1-6FACCD2D686F" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "340F6CC1-8C38-4579-9383-A6168680194A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "FFEF4113-FBD9-49B6-BCBD-4844CFF5FA3B" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "340F6CC1-8C38-4579-9383-A6168680194A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "4778450C-5B2A-40AD-9FC0-B3ED8AAE2F5C" );

            // Attribute for BlockType
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Attribute: Activity Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "94C4CB0B-5617-4F46-B902-6E6DD4A447B8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Activity Detail Page", "ActivityDetailPage", "Activity Detail Page", @"The page that will be navigated to when clicking an activity row.", 1, @"", "30C34DC5-08B4-4826-9EA1-5008B0864805" );

            // Attribute for BlockType
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Attribute: Participant Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "94C4CB0B-5617-4F46-B902-6E6DD4A447B8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Participant Detail Page", "ParticipantDetailPage", "Participant Detail Page", @"The page that will be navigated to when clicking a student or facilitator row.", 2, @"72C75C91-18F8-48D0-B0CF-1FBD82EB50FC", "5F4ADFD7-1CA0-4A3F-8ADA-1B0744119CB0" );

            // Attribute for BlockType
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D0AFDF98-4AFC-4E4F-A6E2-07CA4E7358E8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the learning course details.", 0, @"", "67E7F552-C0F2-4852-B817-E216795D1E30" );

            // Attribute for BlockType
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D0AFDF98-4AFC-4E4F-A6E2-07CA4E7358E8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "16763698-7847-43BF-B2F2-68FBB4408FF8" );

            // Attribute for BlockType
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D0AFDF98-4AFC-4E4F-A6E2-07CA4E7358E8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "223232AD-21C4-4024-ADE2-B9180B165728" );

            // Attribute for BlockType
            //   BlockType: Learning Program Completion List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CE703EB6-028F-47FC-A09A-AD8F72C12CBC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the learning program completion details.", 0, @"", "206A8316-1203-4661-A9E7-A4032C930075" );

            // Attribute for BlockType
            //   BlockType: Learning Program Completion List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CE703EB6-028F-47FC-A09A-AD8F72C12CBC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "C16D7666-6155-4D64-B515-BEB96C934E4C" );

            // Attribute for BlockType
            //   BlockType: Learning Program Completion List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CE703EB6-028F-47FC-A09A-AD8F72C12CBC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "C7138043-593C-4807-A0F1-E31B1A6F297F" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "796C87E7-678F-4A38-8C04-A401A4F7AC21", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Category", "Category", "Category", @"Optional category for the Program.", 1, @"", "BEABF6B9-F85E-44DD-9B70-D49209AD84A8" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Display Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "796C87E7-678F-4A38-8C04-A401A4F7AC21", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Mode", "DisplayMode", "Display Mode", @"Select 'Summary' to show only attributes that are 'Show on Grid'. Select 'Full' to show all attributes.", 2, @"Summary", "18731EB1-888B-4AB2-B1C1-759493B2E639" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Courses Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "796C87E7-678F-4A38-8C04-A401A4F7AC21", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Courses Page", "CoursesPage", "Courses Page", @"The page that will show the courses for the learning program.", 4, @"", "8C648BE3-357F-42D1-9402-500A290F9FD5" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Completions Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "796C87E7-678F-4A38-8C04-A401A4F7AC21", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Completions Page", "CompletionsPage", "Completions Page", @"The page that will show the course completions for the learning program.", 5, @"", "94676A26-01A8-4D53-B382-70DDF17AEEDA" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Semesters Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "796C87E7-678F-4A38-8C04-A401A4F7AC21", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Semesters Page", "SemestersPage", "Semesters Page", @"The page that will show the semesters for the learning program.", 6, @"", "80BFAC80-B689-4B66-898E-69DA756DE093" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Show KPIs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "796C87E7-678F-4A38-8C04-A401A4F7AC21", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show KPIs", "ShowKPIs", "Show KPIs", @"Determines if the KPIs are visible.", 0, @"Yes", "6AE52A7E-EFA3-4685-B331-A2D3058438D3" );

            // Attribute for BlockType
            //   BlockType: Learning Program List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7B1DB013-A552-455F-A1D0-7B17488D0D5C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the learning program details.", 0, @"", "CE5E2633-0E73-4A30-AB96-DE6A0BB40AD6" );

            // Attribute for BlockType
            //   BlockType: Learning Program List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7B1DB013-A552-455F-A1D0-7B17488D0D5C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "B870E6CC-2952-400C-ADAA-B51BF3147E6D" );

            // Attribute for BlockType
            //   BlockType: Learning Program List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7B1DB013-A552-455F-A1D0-7B17488D0D5C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F676DCD1-F5DA-440A-80C9-29930A0B7E21" );

            // Attribute for BlockType
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C89C7F15-FB8A-43D4-9AFB-5E40E397F246", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the learning semester details.", 0, @"", "EEA24DC4-4CFC-4B34-9348-917839DDBBA2" );

            // Attribute for BlockType
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C89C7F15-FB8A-43D4-9AFB-5E40E397F246", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "FCA17D14-C775-4651-A41F-C5EF673B446F" );

            // Attribute for BlockType
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C89C7F15-FB8A-43D4-9AFB-5E40E397F246", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "57106CC3-5D67-40BA-809B-2EA9826478D8" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F003DAAC-B9FE-4007-B218-983084E1126B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the learning grading system details.", 0, @"", "877C67A0-91A2-45C2-94AC-D2FB4951A0C8" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F003DAAC-B9FE-4007-B218-983084E1126B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "56A23C5C-8BE5-419A-9945-9044CBB514E9" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F003DAAC-B9FE-4007-B218-983084E1126B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F07A92F7-2B8B-4ADA-AE64-C8E3D7734A65" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System Scale List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "27390ED3-57B2-42EF-A212-F8B29851F9BA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the learning grading system scale details.", 0, @"", "B930784E-C501-482C-8007-C9C3380BFDF4" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System Scale List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "27390ED3-57B2-42EF-A212-F8B29851F9BA", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "9EF3D35C-C7C0-4AF2-8F1C-4682FDB79FDB" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System Scale List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "27390ED3-57B2-42EF-A212-F8B29851F9BA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "AAB4897B-5FED-4093-BAC0-C2CA043CC285" );

            // Attribute for BlockType
            //   BlockType: Learning Activity Completion List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EF1A5CDD-6769-4FFC-B826-55C194B01897", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the learning activity completion details.", 0, @"", "7733ED7C-A8C4-4501-8BD1-6B66F54DF13B" );

            // Attribute for BlockType
            //   BlockType: Learning Activity Completion List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EF1A5CDD-6769-4FFC-B826-55C194B01897", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "BABBE946-AB0E-405B-9787-5A09D9D055DF" );

            // Attribute for BlockType
            //   BlockType: Learning Activity Completion List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EF1A5CDD-6769-4FFC-B826-55C194B01897", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "2DB21AA3-F122-4D3A-97A5-3BD3F0020D13" );

            // Attribute for BlockType
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Attribute: Attendance Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D5369F8D-11AA-482B-AE08-2B3C519D8D87", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Attendance Page", "AttendancePage", "Attendance Page", @"The page that to be used for taking attendance for the class.", 2, @"", "B417E2A7-BBA1-453F-9933-3BE439CD2063" );

            // Attribute for BlockType
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Attribute: Activity Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D5369F8D-11AA-482B-AE08-2B3C519D8D87", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Activity Detail Page", "ActivityDetailPage", "Activity Detail Page", @"The page that will be navigated to when clicking an activity row.", 1, @"", "44565F75-B4CA-4438-A9B6-BEB943813559" );

            // Attribute for BlockType
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Attribute: Participant Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D5369F8D-11AA-482B-AE08-2B3C519D8D87", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Participant Detail Page", "ParticipantDetailPage", "Participant Detail Page", @"The page that to be used for taking attendance for the class.", 3, @"", "1B237D13-A86A-42CC-8E91-96CBCCAC6866" );

            // Attribute for BlockType
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Attribute: Content Page Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D5369F8D-11AA-482B-AE08-2B3C519D8D87", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Content Page Detail Page", "ContentPageDetailPage", "Content Page Detail Page", @"The page that will be navigated to when clicking a content page row.", 4, @"", "AAF9675D-BAF0-4F25-8C63-4F864F4369C0" );

            // Attribute for BlockType
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Attribute: Announcement Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D5369F8D-11AA-482B-AE08-2B3C519D8D87", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Announcement Detail Page", "AnnouncementDetailPage", "Announcement Detail Page", @"The page that will be navigated to when clicking a content page row.", 5, @"", "B570F05B-95DE-4F22-95FF-DDDCCE943E29" );

            // Attribute for BlockType
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Attribute: Participant Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "94C4CB0B-5617-4F46-B902-6E6DD4A447B8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Participant Detail Page", "ParticipantDetailPage", "Participant Detail Page", @"The page that will be navigated to when clicking a student or facilitator row.", 2, @"", "75CF60D8-19F7-45DB-BB6C-CEF859F2327D" );

            // Attribute for BlockType
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Attribute: Content Page Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "94C4CB0B-5617-4F46-B902-6E6DD4A447B8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Content Page Detail Page", "ContentPageDetailPage", "Content Page Detail Page", @"The page that will be navigated to when clicking a content page row.", 3, @"", "8FFDFA67-E20E-4BEC-91D3-7DDF7975291C" );

            // Attribute for BlockType
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Attribute: Announcement Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "94C4CB0B-5617-4F46-B902-6E6DD4A447B8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Announcement Detail Page", "AnnouncementDetailPage", "Announcement Detail Page", @"The page that will be navigated to when clicking a content page row.", 4, @"", "45F3C2D4-8C84-4CE2-BAB3-ACF93DC8E940" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA1460D8-E895-4B23-8A8E-10EBBED3990F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the courses for the program.", 0, @"", "0317E0AD-9FE9-409B-9A14-C3D30D303B23" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Program List Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA1460D8-E895-4B23-8A8E-10EBBED3990F", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Program List Template", "ProgramListTemplate", "Program List Template", @"The lava template showing the program list. Merge fields include: Programs, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
//- Styles
{% stylesheet %}
    .page-container {
        display: flex;
        flex-direction: column;
    }
    
    .page-header-section {
        height: 280px; 
        align-items: center; 
        border-radius: 12px; 
        background-image: url('/GetImage.ashx?guid=4812baaf-a173-472c-a9a7-8ceb83c06f53'); 
        background-size: cover;
    }
    
    .programs-list-header-section {
        margin-top: 100px;   
    }
    
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        bottom: -80%;
        -webkit-transform: translateY(-30%);
        transform: translateY(-30%);
        background-color: white; 
        border-radius: 12px; 
        width: 80%; 
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        margin-bottom: 12px;
        padding-bottom: 12px;
    }
    
    .program-item-container {
        background-color: white; 
        border-radius: 12px;
        margin: 8px;
        display: flex;
        flex-direction: column;
        justify-content: space-between;
    }
{% endstylesheet %}
<div class=""page-container"" >
	<div class=""page-header-section"">
		<div class=""header-block text-center"">
			<div class=""h1 text-bold"">
				Growth Through Learning
			</div>
			<div class=""page-sub-header"">
				We believe that spiritual growth is deeply intertwined with continuous learning. 
				""Growth Through Learning"" is our commitment to nurture your faith journey, providing resources and opportunies to deepen your understanding of God's Word and his purpose for your life.
			</div>
		</div>
	</div>
	
	<div class=""programs-list-header-section center-block text-center mb-4"">
		<span class=""program-list-header h5"">
			Programs Available
		</span>

		<div class=""program-list-sub-header text-muted"">
			The following types of classes are available.
		</div>
	</div>
	
	<div class=""program-list-container d-flex flex-fill"">
		{% for program in Programs %}
		<div class=""program-item-container"">
		
			{% if program.ImageFileGuid and program.ImageFileGuid != '' %}
			<div class=""program-image program-item-header"" >
				<img style=""border-radius: 12px 12px 0 0;"" src=""/GetImage.ashx?guid={{program.ImageFileGuid}}&maxwidth=300&maxheight=150&mode=crop"" />
			</div>
			{% endif %}
			
			<div class=""program-item-middle p-3"">
			
				<h4 class=""program-name"">
					{{ program.Entity.PublicName }}
				</h4>
				<div class=""program-category"">
					<span class=""badge badge-info"">{{ program.Category }}</span>
				</div>
				<div class=""program-summary text-muted"">
					{{ program.Entity.Summary }} 
				</div>
			</div>
			
			<div class=""program-item-footer d-flex justify-content-between mt-4 p-3"">
				<a class=""btn btn-default"" href=""{{ program.CoursesLink }}"">Learn More</a>
				
				{% if program.CompletionStatus == 'Completed' %}
					<span class=""badge badge-success p-2"" style=""line-height: normal;"">Completed</span>
				{% elseif program.CompletionStatus == 'Pending' %}
					<span class=""badge badge-info p-2"" style=""line-height: normal;"">Enrolled</span>
				{% endif %}
			</div>
		</div>
		{% endfor %}
	</div>
</div>", "63F97CF2-774C-480C-9933-A2BAA664DCE2" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5D6BA94F-342A-4EC1-B024-FC5046FFE14D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the course details.", 0, @"", "5A8A251D-56B3-4E3B-8BF9-1D333C016B74" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Course List Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5D6BA94F-342A-4EC1-B024-FC5046FFE14D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Course List Template", "CourseListTemplate", "Course List Template", @"The lava template showing the courses list. Merge fields include: Program, Courses, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
//- Variables
{% assign imageFileNameLength = Program.ImageBinaryFile.Guid | Size %}

//- Styles
{% stylesheet %}
    .page-container {
        display: flex;
        flex-direction: column;
        margin-bottom: 12px;
    }
    
    .page-header-section {
        {% if imageFileNameLength > 0 %}
            height: 280px; 
        {% endif %}
        align-items: center; 
        border-radius: 12px; 
        background-image: url('/GetImage.ashx?guid={{Program.ImageBinaryFile.Guid}}'); 
        background-size: cover;
    }
    
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        {% if imageFileNameLength > 0 %}
            bottom: -85%;
            -webkit-transform: translateY(-30%);
            transform: translateY(-30%);
        {% endif %}
        background-color: white; 
        border-radius: 12px; 
        width: 80%; 
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        padding-bottom: 12px;
        margin-bottom: 12px;
    }
    
    .course-item-container {
        max-width: 300px;
        background-color: white; 
        border-radius: 12px;
        margin: 8px;
        display: flex;
        flex-direction: column;
        justify-content: space-between;
    }
{% endstylesheet %}
<div class=""page-container"">
	<div class=""page-header-section mb-5"">
		<div class=""header-block text-center"">
			<h2>
				{{ Program.Name }}
			</h2>
			<div class=""page-sub-header"">
				{{ Program.Summary }}
			</div>
		</div>
	</div>
	
	<div class=""course-list-header-section center-block text-center mb-4"">
		<span class=""course-list-header h5"">
			Courses
		</span>
	</div>
	
	<div class=""course-list-container d-flex flex-fill"">
		{% for course in Courses %}
		<div class=""course-item-container"">
			<div class=""course-item-middle p-3"">
			
				<h4 class=""course-name"">
					{{ course.Entity.PublicName }}
				</h4>
				<div class=""course-category d-flex justify-content-between mb-2"">
				    {% if course.Category and course.Category <> '' %}
					    <span class=""badge badge-info"">{{ course.Category }}</span>
					{% else %}
					    <span> </span>
				    {% endif %}
				    {% if course.Entity.Credits %}
				        <span class=""badge"" style=""background-color: #ddedf2; color: #546a71;"">Credits: {{ course.Entity.Credits }}</span>
				    {% endif %}
				</div>
				<div class=""course-summary text-muted"">
					{{ course.Entity.Summary }} 
				</div>
			</div>
		
			<div class=""course-item-footer d-flex flex-column mt-4 p-3"">
                <div class=""course-next-session text-muted mb-3"">
                    <div class=""text-bold"">Next Session Starts</div>
                    <ul><li>{{ course.NextSemester.StartDate | Date:'MMMM dd, yyyy' }}</li></ul>
                </div>
                
                <div class=""d-flex justify-content-between"">
    				<a class=""btn btn-default"" href=""{{ course.CourseDetailsLink }}"">Learn More</a>
    				
    				{% if course.LearningCompletionStatus == 'Pass' %}
    					<span class=""badge badge-success p-2"" style=""line-height: normal;"">Completed</span>
    				{% elseif course.LearningCompletionStatus == 'Incomplete' %}
    					<span class=""badge badge-info p-2"" style=""line-height: normal;"">Enrolled</span>
    				{% elseif course.UnmetPrerequisites != empty %}
                        <a class=""text-muted"" href=""{{ course.PrerequisiteEnrollmentLink }}"">Prerequisites Not Met</a>
                    {% else %}
                        <a class=""text-bold ml-3"" href=""{{ course.CourseEnrollmentLink }}"">Enroll</a>
    				{% endif %}
			    </div>
			</div>
		</div>
		{% endfor %}
	</div>
</div>", "BB215F63-EF8A-4196-A4F1-A4A822B4BD60" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Enrollment Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5D6BA94F-342A-4EC1-B024-FC5046FFE14D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Enrollment Page", "CourseEnrollmentPage", "Detail Page", @"The page that will enroll the student in the course.", 0, @"", "FF294CFA-6D54-4CBC-B8EF-D45893677D58" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Class Workspace Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0DCE130-0C91-4AA0-8161-57E8FA523392", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Class Workspace Page", "DetailPage", "Class Workspace Page", @"The page that will show the class workspace.", 0, @"", "8787C9F3-1CF9-4790-B65E-90303F446536" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Enrollment Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0DCE130-0C91-4AA0-8161-57E8FA523392", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Enrollment Page", "CourseEnrollmentPage", "Enrollment Page", @"The page that will enroll the student in the course.", 0, @"", "96644CEF-4FC7-4986-B591-D6675AA38C2C" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Course Detail Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0DCE130-0C91-4AA0-8161-57E8FA523392", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Course Detail Template", "CourseListTemplate", "Course Detail Template", @"The lava template showing the course detail. Merge fields include: Course, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
//- Variable Assignments
{% assign requirementTypes = Course.CourseRequirements | Distinct:'RequirementType' %}
{% assign prerequisitesText = Course.CourseRequirements | Where:'RequirementType','Prerequisite' | Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
{% assign facilitatorCount = Course.Facilitators | Size %}
{% assign facilitators = Course.Facilitators | Join:', ' | ReplaceLast:',',' and' | Default:'TBD' %}
{% assign imageFileNameLength = Course.ImageFileGuid | Size %}

//- Styles
{% stylesheet %}
    .page-container {
        display: flex;
        flex-direction: column;
        margin-bottom: 12px;
    }
    
    .page-header-section {
        {% if imageFileNameLength > 0 %}
            height: 280px;
            background-image: url('/GetImage.ashx?guid={{Course.ImageFileGuid}}'); 
            background-size: cover;
        {% endif %}
        align-items: center; 
        border-radius: 12px; 
    }
    
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        {% if imageFileNameLength > 0 %}
            bottom: -85%;
            -webkit-transform: translateY(-30%);
            transform: translateY(-30%);
        {% endif %}
        background-color: white; 
        border-radius: 12px; 
        width: 80%; 
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        padding-bottom: 12px;
        margin-bottom: 12px;
    }
    
    .page-main-content {
        margin-top: 30px;   
    }
    
    .course-detail-container {
        background-color: white; 
        border-radius: 12px;
        padding: 12px;
        display: flex;
        flex-direction: column;
    }
    
    .course-status-sidebar-container {
        padding: 12px; 
        margin-left: 12px;
        background-color: white; 
        border-radius: 12px;
        width: 300px;
    }
{% endstylesheet %}
<div class=""page-container"">
	<div class=""page-header-section mb-5"">
		<div class=""header-block text-center"">
			<h2>
				{{ Course.Entity.PublicName }}
			</h2>
			<div class=""page-sub-header"">
				{{ Course.Entity.Summary }}
			</div>
		</div>
	</div>
	
	<div class=""page-main-content d-flex"">
		<div class=""course-detail-container text-muted"">
			<div class=""description-header h4"">Course Description</div>
			
			<div class=""course-item-pair-container course-code"">
				<span class=""text-bold"">Course Code: </span>
				<span>{{Course.Entity.CourseCode}}</span>
			</div>
			
			<div class=""course-item-pair-container credits"">
				<span class=""text-bold"">Credits: </span>
				<span>{{Course.Entity.Credits}}</span>
			</div>
			
			<div class=""course-item-pair-container prerequisites"">
				<span class=""text-bold"">Prerequisites: </span>
				
				<span>
					{{ prerequisitesText }}
				</span>
			</div>
			
			<div class=""course-item-pair-container description"">
				<span>{{Course.DescriptionAsHtml}}</span>
			</div>
		</div>
		
		
		<div class=""course-side-panel d-flex flex-column"">
			<div class=""course-status-sidebar-container"">
				
			{% case Course.LearningCompletionStatus %}
			{% when 'Incomplete' %} 
				<div class=""sidebar-header text-bold"">Currently Enrolled</div>
				<div class=""sidebar-value text-muted"">You are currently enrolled in this course.</div>
					
				<div class=""side-bar-action mt-3"">
					<a class=""btn btn-info"" href=""{{ Course.ClassWorkspaceLink }}"">View Course</a>
				</div>
				
                {% for requirementType in requirementTypes %}
                	{% assign requirementsText = Course.CourseRequirements | Where:'RequirementType',requirementType | Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
                				<div class=""sidebar-header text-bold"">{{ requirementType | Pluralize }}</div>
                				<div class=""sidebar-value text-muted"">{{ requirementsText }}</div>
                {% endfor %}
			{% when 'Passed' %} 
				<div class=""sidebar-header text-bold"">History</div>
				<div class=""sidebar-value text-muted"">You completed this class on {{ Course.MostRecentParticipation.LearningCompletionDateTime | Date: 'MMMM dd, yyyy' }}</div>
				
				<div class=""side-bar-action mt-3"">
					<a href=""{{ Course.ClassWorkspaceLink }}"">View Class Work</a>
				</div>
				
                {% for requirementType in requirementTypes %}
                	{% assign requirementsText = Course.CourseRequirements | Where:'RequirementType',requirementType | Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
                				<div class=""sidebar-header text-bold"">{{ requirementType | Pluralize }}</div>
                				<div class=""sidebar-value text-muted"">{{ requirementsText }}</div>
                {% endfor %}
			{% else %} 
                {% for requirementType in requirementTypes %}
                	{% assign requirementsText = Course.CourseRequirements | Where:'RequirementType',requirementType | Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
                				<div class=""sidebar-header text-bold"">{{ requirementType | Pluralize }}</div>
                				<div class=""sidebar-value text-muted"">{{ requirementsText }}</div>
                {% endfor %}
				
				<div class=""sidebar-upcoming-schedule h4"">Upcoming Schedule</div>
				
				<div class=""side-bar-header-value-pair text-muted"">
					<div class=""sidebar-header text-bold"">Next Session Semester: </div>
					<div class=""sidebar-value"">{{ Course.NextSemester.Name }}</div>
				</div>
				
				<div class=""side-bar-header-value-pair text-muted"">
					<div class=""sidebar-header text-bold"">{{ 'Instructor' | PluralizeForQuantity:facilitatorCount }}:</div>
					<div class=""sidebar-value"">{{ facilitators }}</div>
				</div>
				<div class=""side-bar-action mt-3"">
					<a class=""btn btn-info"" href=""{{ Course.CourseEnrollmentLink }}"">Enroll</a>
				</div>
			{% endcase %}
			</div>
		</div>
	</div>
</div>", "0935CE10-BD61-4C85-9D6C-0269DEC2EEAC" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Attribute: Facilitator Portal Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "55F2E89B-DE57-4E24-AC6C-576956FB97C5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Facilitator Portal Page", "FacilitatorPortalPage", "Facilitator Portal Page", @"The page that will be navigated to when clicking facilitator portal link.", 1, @"", "72DFE773-DA2F-45A8-976A-6C19FD0AFE28" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Attribute: Header Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "55F2E89B-DE57-4E24-AC6C-576956FB97C5", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Header Template", "HeaderTemplate", "Header Template", @"The lava template showing the program list. Merge fields include: Course, Activities, Facilitators and other Common Merge Fields. <span class='tip tip-lava'></span>", 2, @"
{% assign imageFileNameLength = Course.ImageFileGuid | Size %}

//- Styles
{% stylesheet %}
    .header-container {
        display: flex;
        flex-direction: column;
        margin-bottom: 12px;
    }
    
    .page-header-section {
        {% if imageFileNameLength > 0 %}
            height: 280px;
            background-image: url('/GetImage.ashx?guid={{Course.ImageFileGuid}}'); 
            background-size: cover;
        {% endif %}
        align-items: center; 
        border-radius: 12px; 
    }
    
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        {% if imageFileNameLength > 0 %}
            bottom: -85%;
            -webkit-transform: translateY(-30%);
            transform: translateY(-30%);
        {% endif %}
        background-color: white; 
        border-radius: 12px; 
        width: 80%; 
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        padding-bottom: 12px;
        margin-bottom: 12px;
    }
{% endstylesheet %}
<div class=""header-container"">
	<div class=""page-header-section mb-5"">
		<div class=""header-block text-center"">
			<h2>
				{{ Course.PublicName }}
			</h2>
			<div class=""page-sub-header"">
				{{ Course.Summary }}
			</div>
		</div>
	</div>
</div>
", "F7F1BE33-AF44-4F73-9C55-0F8FF97E1B69" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Attribute: The Number of Notifications to Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "55F2E89B-DE57-4E24-AC6C-576956FB97C5", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "The Number of Notifications to Show", "NumberOfNotificationsToShow", "The Number of Notifications to Show", @"The number of notifications to show on the class overview page", 3, @"3", "3B8378C9-80DD-4A50-9197-CA9E1EC88507" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Attribute: Show Grades
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "55F2E89B-DE57-4E24-AC6C-576956FB97C5", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Show Grades", "ShowGrades", "Show Grades", @"Select 'Show' to show grades on the class overview page; 'Hide' to not show any grades.", 4, @"Show", "6FE66C3C-E37B-440D-942C-88C008E844F5" );

            // Add Block Attribute Value
            //   Block: Learning Program List
            //   BlockType: Learning Program List
            //   Category: LMS
            //   Block Location: Page=Learning, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 7888caf4-af5d-44ba-ab9e-80138361f69d */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "B7D8BC2F-CEDD-4E22-8D46-69A759DCB59C", "CE5E2633-0E73-4A30-AB96-DE6A0BB40AD6", @"7888caf4-af5d-44ba-ab9e-80138361f69d" );

            // Add Block Attribute Value
            //   Block: Learning Program List
            //   BlockType: Learning Program List
            //   Category: LMS
            //   Block Location: Page=Learning, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "B7D8BC2F-CEDD-4E22-8D46-69A759DCB59C", "71BC04BF-E3D4-41C6-96EA-25B69CE7C466", @"False" );

            // Add Block Attribute Value
            //   Block: Learning Program List
            //   BlockType: Learning Program List
            //   Category: LMS
            //   Block Location: Page=Learning, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "B7D8BC2F-CEDD-4E22-8D46-69A759DCB59C", "F676DCD1-F5DA-440A-80C9-29930A0B7E21", @"True" );

            // Add Block Attribute Value
            //   Block: Program
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Display Mode
            /*   Attribute Value: Full */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D4ED6CD7-C28E-4ABB-90B3-39499BE41E1B", "18731EB1-888B-4AB2-B1C1-759493B2E639", @"Full" );

            // Add Block Attribute Value
            //   Block: Program
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Courses Page
            /*   Attribute Value: 870318d8-5381-4b3c-be4a-04e57125b590 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D4ED6CD7-C28E-4ABB-90B3-39499BE41E1B", "8C648BE3-357F-42D1-9402-500A290F9FD5", @"870318d8-5381-4b3c-be4a-04e57125b590" );

            // Add Block Attribute Value
            //   Block: Program
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Completions Page
            /*   Attribute Value: 532bc5a9-40b3-42af-9ad3-740fc0b3eb41 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D4ED6CD7-C28E-4ABB-90B3-39499BE41E1B", "94676A26-01A8-4D53-B382-70DDF17AEEDA", @"532bc5a9-40b3-42af-9ad3-740fc0b3eb41" );

            // Add Block Attribute Value
            //   Block: Program
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Semesters Page
            /*   Attribute Value: f7073393-d3a7-4c2e-8001-a73f9e169d60 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D4ED6CD7-C28E-4ABB-90B3-39499BE41E1B", "80BFAC80-B689-4B66-898E-69DA756DE093", @"f7073393-d3a7-4c2e-8001-a73f9e169d60" );

            // Add Block Attribute Value
            //   Block: Program
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Show KPIs
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D4ED6CD7-C28E-4ABB-90B3-39499BE41E1B", "6AE52A7E-EFA3-4685-B331-A2D3058438D3", @"False" );

            // Add Block Attribute Value
            //   Block: Learning Grading System List
            //   BlockType: Learning Grading System List
            //   Category: LMS
            //   Block Location: Page=Grading Systems, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 6163ccfd-cb15-452e-99f2-229a5e5b22f0 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "AF6C3E7F-C7EB-40ED-BCE2-038FABC6A868", "877C67A0-91A2-45C2-94AC-D2FB4951A0C8", @"6163ccfd-cb15-452e-99f2-229a5e5b22f0" );

            // Add Block Attribute Value
            //   Block: Learning Grading System List
            //   BlockType: Learning Grading System List
            //   Category: LMS
            //   Block Location: Page=Grading Systems, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "AF6C3E7F-C7EB-40ED-BCE2-038FABC6A868", "B80C9816-EA60-4EB2-9920-DCE896A6FCC8", @"False" );

            // Add Block Attribute Value
            //   Block: Learning Grading System List
            //   BlockType: Learning Grading System List
            //   Category: LMS
            //   Block Location: Page=Grading Systems, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "AF6C3E7F-C7EB-40ED-BCE2-038FABC6A868", "F07A92F7-2B8B-4ADA-AE64-C8E3D7734A65", @"True" );

            // Add Block Attribute Value
            //   Block: Learning Course List
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: a57d990e-6f34-45cf-abaa-08c40e8d4844,85f70ecd-9425-4ba5-8a60-c2f6891b1265 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "60979468-1F45-467B-A600-B1A230BF6CB9", "67E7F552-C0F2-4852-B817-E216795D1E30", @"a57d990e-6f34-45cf-abaa-08c40e8d4844,85f70ecd-9425-4ba5-8a60-c2f6891b1265" );

            // Add Block Attribute Value
            //   Block: Learning Course List
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "60979468-1F45-467B-A600-B1A230BF6CB9", "148C0E92-ACA0-45DE-84EB-C8CB76D57747", @"False" );

            // Add Block Attribute Value
            //   Block: Learning Course List
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "60979468-1F45-467B-A600-B1A230BF6CB9", "223232AD-21C4-4024-ADE2-B9180B165728", @"True" );

            // Add Block Attribute Value
            //   Block: Learning Course Detail
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Block Location: Page=Course, Site=Rock RMS
            //   Attribute: Activity Detail Page
            /*   Attribute Value: d72dcbc4-c57f-4028-b503-1954925edc7d */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D85084D3-E298-4307-9AA2-C1570C4A3FA4", "30C34DC5-08B4-4826-9EA1-5008B0864805", @"d72dcbc4-c57f-4028-b503-1954925edc7d" );

            // Add Block Attribute Value
            //   Block: Learning Course Detail
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Block Location: Page=Course, Site=Rock RMS
            //   Attribute: Participant Detail Page
            /*   Attribute Value: 72c75c91-18f8-48d0-b0cf-1fbd82eb50fc,827af9a8-bf1a-4008-b4c3-3d07076acb84 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D85084D3-E298-4307-9AA2-C1570C4A3FA4", "75CF60D8-19F7-45DB-BB6C-CEF859F2327D", @"72c75c91-18f8-48d0-b0cf-1fbd82eb50fc,827af9a8-bf1a-4008-b4c3-3d07076acb84" );

            // Add Block Attribute Value
            //   Block: Grading Scales
            //   BlockType: Learning Grading System Scale List
            //   Category: LMS
            //   Block Location: Page=Grading System, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: ae85b3fc-c951-497f-8c43-9d0a1e467a50,4a7b8b2e-9820-4932-b849-35624a27e47b */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "FD56EBFE-8BC5-42B7-8847-941B58AD14B6", "B930784E-C501-482C-8007-C9C3380BFDF4", @"ae85b3fc-c951-497f-8c43-9d0a1e467a50,4a7b8b2e-9820-4932-b849-35624a27e47b" );

            // Add Block Attribute Value
            //   Block: Grading Scales
            //   BlockType: Learning Grading System Scale List
            //   Category: LMS
            //   Block Location: Page=Grading System, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "FD56EBFE-8BC5-42B7-8847-941B58AD14B6", "27B7F5D0-39FF-4913-8525-E9F571F8C1BB", @"False" );

            // Add Block Attribute Value
            //   Block: Grading Scales
            //   BlockType: Learning Grading System Scale List
            //   Category: LMS
            //   Block Location: Page=Grading System, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "FD56EBFE-8BC5-42B7-8847-941B58AD14B6", "AAB4897B-5FED-4093-BAC0-C2CA043CC285", @"True" );

            // Add Block Attribute Value
            //   Block: Activity List
            //   BlockType: Learning Activity Completion List
            //   Category: LMS
            //   Block Location: Page=Activity, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: e0f2e4f1-ed10-49f6-b053-ac6807994204,8c40ae8d-60c6-49de-b7de-be46d8a64aa6 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "7CE84856-A789-4D74-AB6C-DA05A5C82479", "7733ED7C-A8C4-4501-8BD1-6B66F54DF13B", @"e0f2e4f1-ed10-49f6-b053-ac6807994204,8c40ae8d-60c6-49de-b7de-be46d8a64aa6" );

            // Add Block Attribute Value
            //   Block: Activity List
            //   BlockType: Learning Activity Completion List
            //   Category: LMS
            //   Block Location: Page=Activity, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "7CE84856-A789-4D74-AB6C-DA05A5C82479", "F353856F-95FA-4708-9B2C-8E99C09B4399", @"False" );

            // Add Block Attribute Value
            //   Block: Activity List
            //   BlockType: Learning Activity Completion List
            //   Category: LMS
            //   Block Location: Page=Activity, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "7CE84856-A789-4D74-AB6C-DA05A5C82479", "2DB21AA3-F122-4D3A-97A5-3BD3F0020D13", @"True" );

            // Add Block Attribute Value
            //   Block: Semesters
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 36ffa805-b283-443e-990d-87040339d16f,bfbdea91-9519-4946-9836-073efe02551f */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "E58406FD-E17B-4480-95EB-ADCFD956CA17", "EEA24DC4-4CFC-4B34-9348-917839DDBBA2", @"36ffa805-b283-443e-990d-87040339d16f,bfbdea91-9519-4946-9836-073efe02551f" );

            // Add Block Attribute Value
            //   Block: Semesters
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "E58406FD-E17B-4480-95EB-ADCFD956CA17", "A0627D65-2057-42C0-8730-65C87F05F86A", @"False" );

            // Add Block Attribute Value
            //   Block: Semesters
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "E58406FD-E17B-4480-95EB-ADCFD956CA17", "57106CC3-5D67-40BA-809B-2EA9826478D8", @"True" );

            // Add Block Attribute Value
            //   Block: Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Course, Site=Rock RMS
            //   Attribute: Show Schedule Column
            /*   Attribute Value: Yes */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0", "BECBD2AE-5D1E-4065-8CAA-FE7972CC5742", @"Yes" );

            // Add Block Attribute Value
            //   Block: Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Course, Site=Rock RMS
            //   Attribute: Show Semester Column
            /*   Attribute Value: Yes */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0", "28087DBE-A334-47BF-9CA5-76AFF5EE1A6F", @"Yes" );

            // Add Block Attribute Value
            //   Block: Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Course, Site=Rock RMS
            //   Attribute: Display Mode
            /*   Attribute Value: Always */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0", "DA69DB3C-139E-4DC6-BBA5-DC03C0ACBD6A", @"Always" );

            // Add Block Attribute Value
            //   Block: Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Course, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 23d5076e-c062-4987-9985-b3a4792bf3ce,5fce29a7-4530-4cce-9891-c95242923efe */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0", "0C995F98-F483-4814-B3A1-6FACCD2D686F", @"23d5076e-c062-4987-9985-b3a4792bf3ce,5fce29a7-4530-4cce-9891-c95242923efe" );

            // Add Block Attribute Value
            //   Block: Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Course, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0", "8D30B478-C616-40A3-A159-7320D22390B6", @"False" );

            // Add Block Attribute Value
            //   Block: Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Course, Site=Rock RMS
            //   Attribute: Show Location Column
            /*   Attribute Value: Yes */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0", "4FBE92ED-3CF1-4BD4-A78E-B50C0080FA0B", @"Yes" );

            // Add Block Attribute Value
            //   Block: Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Course, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0", "4778450C-5B2A-40AD-9FC0-B3ED8AAE2F5C", @"True" );

            // Add Block Attribute Value
            //   Block: Class
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Block Location: Page=Class, Site=Rock RMS
            //   Attribute: Activity Detail Page
            /*   Attribute Value: d72dcbc4-c57f-4028-b503-1954925edc7d */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "C67D2164-33E5-46C0-94EF-DF387EF8477D", "44565F75-B4CA-4438-A9B6-BEB943813559", @"d72dcbc4-c57f-4028-b503-1954925edc7d" );

            // Add Block Attribute Value
            //   Block: Class
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Block Location: Page=Class, Site=Rock RMS
            //   Attribute: Participant Detail Page
            /*   Attribute Value: 72c75c91-18f8-48d0-b0cf-1fbd82eb50fc,827af9a8-bf1a-4008-b4c3-3d07076acb84 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "C67D2164-33E5-46C0-94EF-DF387EF8477D", "1B237D13-A86A-42CC-8E91-96CBCCAC6866", @"72c75c91-18f8-48d0-b0cf-1fbd82eb50fc,827af9a8-bf1a-4008-b4c3-3d07076acb84" );

            // Add Block Attribute Value
            //   Block: Class
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Block Location: Page=Class, Site=Rock RMS
            //   Attribute: Attendance Page
            /*   Attribute Value: 78b79290-3234-4d8c-96d3-1901901ba1dd */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "C67D2164-33E5-46C0-94EF-DF387EF8477D", "B417E2A7-BBA1-453F-9933-3BE439CD2063", @"78b79290-3234-4d8c-96d3-1901901ba1dd" );

            // Add Block Attribute Value
            //   Block: Class
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Block Location: Page=Class, Site=Rock RMS
            //   Attribute: Content Page Detail Page
            /*   Attribute Value: e6a89360-b7b4-48a8-b799-39a27eab6f36 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "C67D2164-33E5-46C0-94EF-DF387EF8477D", "AAF9675D-BAF0-4F25-8C63-4F864F4369C0", @"e6a89360-b7b4-48a8-b799-39a27eab6f36" );

            // Add Block Attribute Value
            //   Block: Class
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Block Location: Page=Class, Site=Rock RMS
            //   Attribute: Announcement Detail Page
            /*   Attribute Value: 7c8134a4-524a-4c3d-ba4c-875fee672850 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "C67D2164-33E5-46C0-94EF-DF387EF8477D", "B570F05B-95DE-4F22-95FF-DDDCCE943E29", @"7c8134a4-524a-4c3d-ba4c-875fee672850" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "53A87507-D270-4340-A93F-AD3AB023E0B1", "8D30B478-C616-40A3-A159-7320D22390B6", @"False" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "53A87507-D270-4340-A93F-AD3AB023E0B1", "4778450C-5B2A-40AD-9FC0-B3ED8AAE2F5C", @"True" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 23d5076e-c062-4987-9985-b3a4792bf3ce,5fce29a7-4530-4cce-9891-c95242923efe */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "53A87507-D270-4340-A93F-AD3AB023E0B1", "0C995F98-F483-4814-B3A1-6FACCD2D686F", @"23d5076e-c062-4987-9985-b3a4792bf3ce,5fce29a7-4530-4cce-9891-c95242923efe" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Show Semester Column
            /*   Attribute Value: Yes */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "53A87507-D270-4340-A93F-AD3AB023E0B1", "28087DBE-A334-47BF-9CA5-76AFF5EE1A6F", @"Yes" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Display Mode
            /*   Attribute Value: Always */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "53A87507-D270-4340-A93F-AD3AB023E0B1", "DA69DB3C-139E-4DC6-BBA5-DC03C0ACBD6A", @"Always" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Show Location Column
            /*   Attribute Value: No */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "53A87507-D270-4340-A93F-AD3AB023E0B1", "4FBE92ED-3CF1-4BD4-A78E-B50C0080FA0B", @"No" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Show Schedule Column
            /*   Attribute Value: No */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "53A87507-D270-4340-A93F-AD3AB023E0B1", "BECBD2AE-5D1E-4065-8CAA-FE7972CC5742", @"No" );
        }

        /// <summary>
        /// Results of CodeGen_PagesBlocksAttributesMigration.sql for LMS (Down).
        /// </summary>
        private void LmsEntityTypesPagesBlocksDown()
        {
            // Attribute for BlockType
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Attribute: Show Grades
            RockMigrationHelper.DeleteAttribute( "6FE66C3C-E37B-440D-942C-88C008E844F5" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Attribute: The Number of Notifications to Show
            RockMigrationHelper.DeleteAttribute( "3B8378C9-80DD-4A50-9197-CA9E1EC88507" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Attribute: Header Template
            RockMigrationHelper.DeleteAttribute( "F7F1BE33-AF44-4F73-9C55-0F8FF97E1B69" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Course Detail Template
            RockMigrationHelper.DeleteAttribute( "0935CE10-BD61-4C85-9D6C-0269DEC2EEAC" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Enrollment Page
            RockMigrationHelper.DeleteAttribute( "96644CEF-4FC7-4986-B591-D6675AA38C2C" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Class Workspace Page
            RockMigrationHelper.DeleteAttribute( "8787C9F3-1CF9-4790-B65E-90303F446536" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Enrollment Page
            RockMigrationHelper.DeleteAttribute( "FF294CFA-6D54-4CBC-B8EF-D45893677D58" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Course List Template
            RockMigrationHelper.DeleteAttribute( "BB215F63-EF8A-4196-A4F1-A4A822B4BD60" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "5A8A251D-56B3-4E3B-8BF9-1D333C016B74" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Program List Template
            RockMigrationHelper.DeleteAttribute( "63F97CF2-774C-480C-9933-A2BAA664DCE2" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "0317E0AD-9FE9-409B-9A14-C3D30D303B23" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Show KPIs
            RockMigrationHelper.DeleteAttribute( "6AE52A7E-EFA3-4685-B331-A2D3058438D3" );

            // Attribute for BlockType
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Attribute: Participant Detail Page
            RockMigrationHelper.DeleteAttribute( "C247F6BF-C7D1-428E-B42F-65D1F8610CED" );

            // Attribute for BlockType
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Attribute: Activity Detail Page
            RockMigrationHelper.DeleteAttribute( "44565F75-B4CA-4438-A9B6-BEB943813559" );

            // Attribute for BlockType
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Attribute: Attendance Page
            RockMigrationHelper.DeleteAttribute( "B417E2A7-BBA1-453F-9933-3BE439CD2063" );

            // Attribute for BlockType
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Attribute: Announcement Detail Page
            RockMigrationHelper.DeleteAttribute( "B570F05B-95DE-4F22-95FF-DDDCCE943E29" );

            // Attribute for BlockType
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Attribute: Content Page Detail Page
            RockMigrationHelper.DeleteAttribute( "AAF9675D-BAF0-4F25-8C63-4F864F4369C0" );

            // Attribute for BlockType
            //   BlockType: Learning Activity Completion List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "2DB21AA3-F122-4D3A-97A5-3BD3F0020D13" );

            // Attribute for BlockType
            //   BlockType: Learning Activity Completion List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "BABBE946-AB0E-405B-9787-5A09D9D055DF" );

            // Attribute for BlockType
            //   BlockType: Learning Activity Completion List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "7733ED7C-A8C4-4501-8BD1-6B66F54DF13B" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System Scale List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "AAB4897B-5FED-4093-BAC0-C2CA043CC285" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System Scale List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "9EF3D35C-C7C0-4AF2-8F1C-4682FDB79FDB" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System Scale List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "B930784E-C501-482C-8007-C9C3380BFDF4" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "F07A92F7-2B8B-4ADA-AE64-C8E3D7734A65" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "56A23C5C-8BE5-419A-9945-9044CBB514E9" );

            // Attribute for BlockType
            //   BlockType: Learning Grading System List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "877C67A0-91A2-45C2-94AC-D2FB4951A0C8" );

            // Attribute for BlockType
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "57106CC3-5D67-40BA-809B-2EA9826478D8" );

            // Attribute for BlockType
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "FCA17D14-C775-4651-A41F-C5EF673B446F" );

            // Attribute for BlockType
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "EEA24DC4-4CFC-4B34-9348-917839DDBBA2" );

            // Attribute for BlockType
            //   BlockType: Learning Program List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "F676DCD1-F5DA-440A-80C9-29930A0B7E21" );

            // Attribute for BlockType
            //   BlockType: Learning Program List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "B870E6CC-2952-400C-ADAA-B51BF3147E6D" );

            // Attribute for BlockType
            //   BlockType: Learning Program List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "CE5E2633-0E73-4A30-AB96-DE6A0BB40AD6" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Semesters Page
            RockMigrationHelper.DeleteAttribute( "80BFAC80-B689-4B66-898E-69DA756DE093" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Completions Page
            RockMigrationHelper.DeleteAttribute( "94676A26-01A8-4D53-B382-70DDF17AEEDA" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Courses Page
            RockMigrationHelper.DeleteAttribute( "8C648BE3-357F-42D1-9402-500A290F9FD5" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Display Mode
            RockMigrationHelper.DeleteAttribute( "18731EB1-888B-4AB2-B1C1-759493B2E639" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Category
            RockMigrationHelper.DeleteAttribute( "BEABF6B9-F85E-44DD-9B70-D49209AD84A8" );

            // Attribute for BlockType
            //   BlockType: Learning Program Completion List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "C7138043-593C-4807-A0F1-E31B1A6F297F" );

            // Attribute for BlockType
            //   BlockType: Learning Program Completion List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "C16D7666-6155-4D64-B515-BEB96C934E4C" );

            // Attribute for BlockType
            //   BlockType: Learning Program Completion List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "206A8316-1203-4661-A9E7-A4032C930075" );

            // Attribute for BlockType
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "223232AD-21C4-4024-ADE2-B9180B165728" );

            // Attribute for BlockType
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "16763698-7847-43BF-B2F2-68FBB4408FF8" );

            // Attribute for BlockType
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "67E7F552-C0F2-4852-B817-E216795D1E30" );

            // Attribute for BlockType
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Attribute: Announcement Detail Page
            RockMigrationHelper.DeleteAttribute( "45F3C2D4-8C84-4CE2-BAB3-ACF93DC8E940" );

            // Attribute for BlockType
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Attribute: Content Page Detail Page
            RockMigrationHelper.DeleteAttribute( "8FFDFA67-E20E-4BEC-91D3-7DDF7975291C" );

            // Attribute for BlockType
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Attribute: Participant Detail Page
            RockMigrationHelper.DeleteAttribute( "75CF60D8-19F7-45DB-BB6C-CEF859F2327D" );

            // Attribute for BlockType
            //   BlockType: Learning Course Detail
            //   Category: LMS
            //   Attribute: Activity Detail Page
            RockMigrationHelper.DeleteAttribute( "30C34DC5-08B4-4826-9EA1-5008B0864805" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "4778450C-5B2A-40AD-9FC0-B3ED8AAE2F5C" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "FFEF4113-FBD9-49B6-BCBD-4844CFF5FA3B" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "0C995F98-F483-4814-B3A1-6FACCD2D686F" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: Display Mode
            RockMigrationHelper.DeleteAttribute( "DA69DB3C-139E-4DC6-BBA5-DC03C0ACBD6A" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: Show Semester Column
            RockMigrationHelper.DeleteAttribute( "28087DBE-A334-47BF-9CA5-76AFF5EE1A6F" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: Show Schedule Column
            RockMigrationHelper.DeleteAttribute( "BECBD2AE-5D1E-4065-8CAA-FE7972CC5742" );

            // Attribute for BlockType
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Attribute: Show Location Column
            RockMigrationHelper.DeleteAttribute( "4FBE92ED-3CF1-4BD4-A78E-B50C0080FA0B" );

            // Attribute for BlockType
            //   BlockType: Learning Activity List
            //   Category: LMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "8783E0C3-6B14-4252-A912-A13C4D1E89B0" );

            // Attribute for BlockType
            //   BlockType: Learning Activity List
            //   Category: LMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "B4130B6F-E3DE-4161-A28D-B7F6F160CB38" );

            // Remove Block
            //  Name: Current Classes, from Page: Program, Site: Rock RMS
            //  from Page: Program, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "53A87507-D270-4340-A93F-AD3AB023E0B1" );

            // Remove Block
            //  Name: Activity Completion Detail, from Page: Completion, Site: Rock RMS
            //  from Page: Completion, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "43D3D01A-101E-494E-AF4D-BBEE9595DE0A" );

            // Remove Block
            //  Name: Class, from Page: Class, Site: Rock RMS
            //  from Page: Class, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "C67D2164-33E5-46C0-94EF-DF387EF8477D" );

            // Remove Block
            //  Name: Classes, from Page: Course, Site: Rock RMS
            //  from Page: Course, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0" );

            // Remove Block
            //  Name: Learning Activity Detail, from Page: Activity, Site: Rock RMS
            //  from Page: Activity, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "8BDFF6BF-E043-4EB5-BDD9-C1813AE83295" );

            // Remove Block
            //  Name: Program Completions, from Page: Completions, Site: Rock RMS
            //  from Page: Completions, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "ADCFF541-8908-4F78-9B96-2C3E37D5C4AB" );

            // Remove Block
            //  Name: Semester, from Page: Semester, Site: Rock RMS
            //  from Page: Semester, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "2DF1C8E3-B65E-47F0-8B82-86AA5A3DA21A" );

            // Remove Block
            //  Name: Participant, from Page: Participant, Site: Rock RMS
            //  from Page: Participant, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "8C05CAC8-AB57-46F2-8E0C-21293BE8F464" );

            // Remove Block
            //  Name: Semesters, from Page: Semesters, Site: Rock RMS
            //  from Page: Semesters, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "E58406FD-E17B-4480-95EB-ADCFD956CA17" );

            // Remove Block
            //  Name: Activity List, from Page: Activity, Site: Rock RMS
            //  from Page: Activity, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "7CE84856-A789-4D74-AB6C-DA05A5C82479" );

            // Remove Block
            //  Name: Grading Scale, from Page: Grading Scale, Site: Rock RMS
            //  from Page: Grading Scale, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "5FBFF087-3366-4620-8C5C-6A12F8BC6BD2" );

            // Remove Block
            //  Name: Grading Scales, from Page: Grading System, Site: Rock RMS
            //  from Page: Grading System, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "FD56EBFE-8BC5-42B7-8847-941B58AD14B6" );

            // Remove Block
            //  Name: Learning Grading System Detail, from Page: Grading System, Site: Rock RMS
            //  from Page: Grading System, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "88233998-6298-40F8-B433-931486D30B2D" );

            // Remove Block
            //  Name: Learning Course Detail, from Page: Course, Site: Rock RMS
            //  from Page: Course, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "D85084D3-E298-4307-9AA2-C1570C4A3FA4" );

            // Remove Block
            //  Name: Learning Course List, from Page: Courses, Site: Rock RMS
            //  from Page: Courses, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "60979468-1F45-467B-A600-B1A230BF6CB9" );

            // Remove Block
            //  Name: Learning Grading System List, from Page: Grading Systems, Site: Rock RMS
            //  from Page: Grading Systems, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "AF6C3E7F-C7EB-40ED-BCE2-038FABC6A868" );

            // Remove Block
            //  Name: Program, from Page: Program, Site: Rock RMS
            //  from Page: Program, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "D4ED6CD7-C28E-4ABB-90B3-39499BE41E1B" );

            // Remove Block
            //  Name: Learning Program List, from Page: Learning, Site: Rock RMS
            //  from Page: Learning, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B7D8BC2F-CEDD-4E22-8D46-69A759DCB59C" );

            // Remove Block
            //  Name: Class Announcement Detail, from Page: Announcement, Site: Rock RMS
            //  from Page: Announcement, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "3EAEE2BB-E223-4C47-9C9D-230A412D6D2A" );

            // Remove Block
            //  Name: Content Page, from Page: Content Page, Site: Rock RMS
            //  from Page: Content Page, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "9788FDD8-A587-48CA-BE0B-D5A937CCEE70" );

            // Delete BlockType 
            //   Name: Public Learning Class Workspace
            //   Category: LMS
            //   Path: -
            //   EntityType: Public Learning Class Workspace
            RockMigrationHelper.DeleteBlockType( "55F2E89B-DE57-4E24-AC6C-576956FB97C5" );

            // Delete BlockType 
            //   Name: Public Learning Course Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Public Learning Course Detail
            RockMigrationHelper.DeleteBlockType( "B0DCE130-0C91-4AA0-8161-57E8FA523392" );

            // Delete BlockType 
            //   Name: Public Learning Course List
            //   Category: LMS
            //   Path: -
            //   EntityType: Public Learning Course List
            RockMigrationHelper.DeleteBlockType( "5D6BA94F-342A-4EC1-B024-FC5046FFE14D" );

            // Delete BlockType 
            //   Name: Public Learning Program List
            //   Category: LMS
            //   Path: -
            //   EntityType: Public Learning Program List
            RockMigrationHelper.DeleteBlockType( "DA1460D8-E895-4B23-8A8E-10EBBED3990F" );

            // Delete BlockType 
            //   Name: Learning Activity Completion Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Activity Completion Detail
            RockMigrationHelper.DeleteBlockType( "4569F28D-1EFB-4B95-A506-0D9043C24775" );

            // Delete BlockType 
            //   Name: Learning Class Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Class Detail
            RockMigrationHelper.DeleteBlockType( "D5369F8D-11AA-482B-AE08-2B3C519D8D87" );

            // Delete BlockType 
            //   Name: Learning Activity Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Activity Detail
            RockMigrationHelper.DeleteBlockType( "4B18BF0D-D91B-4934-AC2D-A7188B15B893" );

            // Delete BlockType 
            //   Name: Learning Semester Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Semester Detail
            RockMigrationHelper.DeleteBlockType( "97B2E57F-3A03-490D-834F-CD3640C7FF1E" );

            // Delete BlockType 
            //   Name: Learning Activity Completion List
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Activity Completion List
            RockMigrationHelper.DeleteBlockType( "EF1A5CDD-6769-4FFC-B826-55C194B01897" );

            // Delete BlockType 
            //   Name: Learning Grading System Scale Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Grading System Scale Detail
            RockMigrationHelper.DeleteBlockType( "34D7A280-8D4D-4FE5-BB45-810732F76341" );

            // Delete BlockType 
            //   Name: Learning Grading System Scale List
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Grading System Scale List
            RockMigrationHelper.DeleteBlockType( "27390ED3-57B2-42EF-A212-F8B29851F9BA" );

            // Delete BlockType 
            //   Name: Learning Grading System Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Grading System Detail
            RockMigrationHelper.DeleteBlockType( "A4182806-95B0-49AE-97B5-246A834156E3" );

            // Delete BlockType 
            //   Name: Learning Grading System List
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Grading System List
            RockMigrationHelper.DeleteBlockType( "F003DAAC-B9FE-4007-B218-983084E1126B" );

            // Delete BlockType 
            //   Name: Learning Semester List
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Semester List
            RockMigrationHelper.DeleteBlockType( "C89C7F15-FB8A-43D4-9AFB-5E40E397F246" );

            // Delete BlockType 
            //   Name: Learning Program List
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Program List
            RockMigrationHelper.DeleteBlockType( "7B1DB013-A552-455F-A1D0-7B17488D0D5C" );

            // Delete BlockType 
            //   Name: Learning Program Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Program Detail
            RockMigrationHelper.DeleteBlockType( "796C87E7-678F-4A38-8C04-A401A4F7AC21" );

            // Delete BlockType 
            //   Name: Learning Program Completion List
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Program Completion List
            RockMigrationHelper.DeleteBlockType( "CE703EB6-028F-47FC-A09A-AD8F72C12CBC" );

            // Delete BlockType 
            //   Name: Learning Participant Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Participant Detail
            RockMigrationHelper.DeleteBlockType( "F1179439-31A1-4897-AB2E-B991D60455AA" );

            // Delete BlockType 
            //   Name: Learning Course List
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Course List
            RockMigrationHelper.DeleteBlockType( "D0AFDF98-4AFC-4E4F-A6E2-07CA4E7358E8" );

            // Delete BlockType 
            //   Name: Learning Course Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Course Detail
            RockMigrationHelper.DeleteBlockType( "94C4CB0B-5617-4F46-B902-6E6DD4A447B8" );

            // Delete BlockType 
            //   Name: Learning Class List
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Class List
            RockMigrationHelper.DeleteBlockType( "340F6CC1-8C38-4579-9383-A6168680194A" );

            // Delete BlockType 
            //   Name: Learning Activity List
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Activity List
            RockMigrationHelper.DeleteBlockType( "5CEB6EC7-69F5-43B6-A74F-144A57F9B465" );

            // Delete BlockType 
            //   Name: Learning Class Content Page Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Class Content Page Detail
            RockMigrationHelper.DeleteBlockType( "92E0DB96-0FFF-41F2-982D-F750AB23EF5B" );

            // Delete BlockType 
            //   Name: Learning Class Announcement Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Class Announcement Detail
            RockMigrationHelper.DeleteBlockType( "53C12A53-773E-4398-8627-DD44C1421675" );

            // Delete Page 
            //  Internal Name: Completion
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "E0F2E4F1-ED10-49F6-B053-AC6807994204" );

            // Delete Page 
            //  Internal Name: Semester
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "36FFA805-B283-443E-990D-87040339D16F" );

            // Delete Page 
            //  Internal Name: Participant
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "72C75C91-18F8-48D0-B0CF-1FBD82EB50FC" );

            // Delete Page 
            //  Internal Name: Activity
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "D72DCBC4-C57F-4028-B503-1954925EDC7D" );

            // Delete Page 
            //  Internal Name: Class
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "23D5076E-C062-4987-9985-B3A4792BF3CE" );

            // Delete Page 
            //  Internal Name: Grading Scale
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "AE85B3FC-C951-497F-8C43-9D0A1E467A50" );

            // Delete Page 
            //  Internal Name: Course
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "A57D990E-6F34-45CF-ABAA-08C40E8D4844" );

            // Delete Page 
            //  Internal Name: Grading System
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "6163CCFD-CB15-452E-99F2-229A5E5B22F0" );

            // Delete Page 
            //  Internal Name: Grading Systems
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "F76C9FC6-CDE0-4122-8D05-7862D683A3CA" );

            // Delete Page 
            //  Internal Name: Semesters
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "F7073393-D3A7-4C2E-8001-A73F9E169D60" );

            // Delete Page 
            //  Internal Name: Completions
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "532BC5A9-40B3-42AF-9AD3-740FC0B3EB41" );

            // Delete Page 
            //  Internal Name: Courses
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "870318D8-5381-4B3C-BE4A-04E57125B590" );

            // Delete Page 
            //  Internal Name: Program
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "7888CAF4-AF5D-44BA-AB9E-80138361F69D" );

            // Delete Page 
            //  Internal Name: Learning
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "84DBEC51-EE0B-41C2-94B3-F361C4B98879" );

            // Delete Page 
            //  Internal Name: Privacy
            //  Site: External Website
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "964A4B4B-BF7F-4520-8620-05A6DEEAB5C8" );

            // Delete Page 
            //  Internal Name: Terms
            //  Site: External Website
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "DF471E9C-EEFC-4493-B6C0-C8D94BC248EB" );
        }

        #endregion

        #endregion

        #region MigrationRollupsForV17_0_10

        #region PA: Chop Shortened Links Block

        private void ChopShortenedLinkBlockUp()
        {
            RockMigrationHelper.UpdatePageLayout( "A9188D7A-80D9-4865-9C77-9F90E992B65C", "2E169330-D7D7-4ECA-B417-72C64BE150F0" );

            // Failed when converted from Plugin Migration to EF Migration. Fixed with entityTypeExists check.
            var entityTypeExists = Convert.ToBoolean( SqlScalar( "SELECT CAST (1 AS BIT) FROM [dbo].[EntityType] WHERE [Guid] = '026C6A93-5295-43E9-B67D-C3708ACB25B9'" ) );

            if ( entityTypeExists == false )
            {
                // Add/Update Obsidian Block Entity Type
                //   EntityType:Rock.Blocks.Cms.ShortLink
                RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.ShortLink", "Short Link", "Rock.Blocks.Cms.ShortLink, Rock.Blocks, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null", false, false, "026C6A93-5295-43E9-B67D-C3708ACB25B9" );
            }

            // Add/Update Obsidian Block Type              
            //   Name:Shortened Links          
            //   Category:CMS              
            //   EntityType:RockWeb.Blocks.Administration.ShortLink              
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Shortened Links", "Displays a dialog for adding a short link to the current page.", "Rock.Blocks.Cms.ShortLink", "Administration", "C85551E8-A363-4AA6-9BFD-E6A1C9CEDE80" );

            // Attribute for BlockType
            //   BlockType: Shortened Links
            //   Category: Administration
            //   Attribute: Minimum Token Length
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C85551E8-A363-4AA6-9BFD-E6A1C9CEDE80", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Minimum Token Length", "MinimumTokenLength", "Minimum Token Length", @"The minimum number of characters for the token.", 0, @"7", "D15CEAF4-8354-42DE-9C3B-50D4F31E32FB" );

            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Shortened Link",
                blockTypeReplacements: new Dictionary<string, string> {
        { "86FB6B0E-E426-4581-96C0-A7654D6A5C7D", "C85551E8-A363-4AA6-9BFD-E6A1C9CEDE80" }, // Shortened Link
                },
                migrationStrategy: "Chop",
                jobGuid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_170_CHOP_SHORTENED_LINKS_BLOCK, blockAttributeKeysToIgnore: null );
        }

        #endregion

        #region JC: LMS Plugin Migration 2

        private void LMSUp()
        {
            AddHistoryCategories();
            SetBreadCrumbForDetailPagesWithBreadCrumbBlocks( false );
            SetShowInGroupList( false );
            AddOrUpdateBlockAndPageAttributes();

            // Ensure the public facing learning pages don't show in navigation.
            var learningUniversityRootPageGuid = "B32639B3-604F-441E-A6E4-2613C0A6BE2B";
            Sql( $"UPDATE [Page] SET [DisplayInNavWhen] = {( int ) DisplayInNavWhen.Never} WHERE [Guid] = '{learningUniversityRootPageGuid}';" );

            var learningProgramEntityTypeGuid = "AFD89885-6923-4FA1-B6FE-A1DF8D821BBC";
            var allChurchCategoryGuid = "824B5DD9-47A7-4CE4-A461-F4FDEC8343F3";
            var internalStaffCategoryGuid = "87E1BEC7-171F-4E7E-8EC7-4D0102DCDE70";
            var volunteeringCategoryGuid = "A94C5563-647D-4509-B213-890B6D2A8530";
            RockMigrationHelper.UpdateCategory( learningProgramEntityTypeGuid, "All Church", "", "All Church", allChurchCategoryGuid, 0 );
            RockMigrationHelper.UpdateCategory( learningProgramEntityTypeGuid, "Internal Staff", "", "Internal Staff", internalStaffCategoryGuid, 1 );
            RockMigrationHelper.UpdateCategory( learningProgramEntityTypeGuid, "Volunteering", "", "Volunteering", volunteeringCategoryGuid, 2 );
        }

        private void LMSDown()
        {
            RemoveHistoryCategories();
            SetBreadCrumbForDetailPagesWithBreadCrumbBlocks( true );
            SetShowInGroupList( true );
            DeleteBlockAndPageAttributes();
        }

        /// <summary>
        /// The Up methods from the SQL code-generated CodeGen_PagesBlocksAttributesMigration.sql file.
        /// </summary>
        private void AddOrUpdateBlockAndPageAttributes()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.LearningProgramCompletionDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.LearningProgramCompletionDetail", "Learning Program Completion Detail", "Rock.Blocks.Lms.LearningProgramCompletionDetail, Rock.Blocks, Version=1.17.0.22, Culture=neutral, PublicKeyToken=null", false, false, "F9164B10-C913-4CD4-A612-27AD25D62ACA" );

            // Add/Update Obsidian Block Type
            //   Name:Learning Program Completion Detail
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.LearningProgramCompletionDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Learning Program Completion Detail", "Displays the details of a particular learning program completion.", "Rock.Blocks.Lms.LearningProgramCompletionDetail", "LMS", "E0C38A42-2ACE-4258-8D11-BD971C41EADB" );

            // Add Block 
            //  Block Name: Program Completion Detail
            //  Page Name: Completions
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "532BC5A9-40B3-42AF-9AD3-740FC0B3EB41".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "E0C38A42-2ACE-4258-8D11-BD971C41EADB".AsGuid(), "Program Completion Detail", "Main", @"", @"", 0, "F7F137EE-C66D-4BF9-B117-DC8899C6603A" );

            // Update the path for learning completion details.
            Sql( "UPDATE [dbo].[PageRoute] SET [Route] = 'learning/{LearningProgramId}/completions/{LearningProgramCompletionId}' WHERE [Guid] = '218FB50F-A231-4029-887D-0F921918ECB1'" );

            // Should default to True (show KPIs on the Learning Program Detail page).
            // Add Block Attribute Value
            //   Block: Program
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Program, Site=Rock RMS
            //   Attribute: Show KPIs
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( false, "D4ED6CD7-C28E-4ABB-90B3-39499BE41E1B", "6AE52A7E-EFA3-4685-B331-A2D3058438D3", @"True" );

            // This is no longer necessary.
            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Semester Detail Page
            RockMigrationHelper.DeleteAttribute( "5A60ECD5-9BCA-4DC5-8077-12E00D475C3B" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Completion Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "796C87E7-678F-4A38-8C04-A401A4F7AC21", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Completion Detail Page", "CompletionDetailPage", "Completion Detail Page", @"The page that will show the program completion detail.", 5, @"", "9FEE276F-9CC9-4FD2-99FF-270B721C6300" );

            // Add Page 
            //  Internal Name: Learning University
            //  Site: External Website
            RockMigrationHelper.AddPage( true, "85F25819-E948-4960-9DDF-00F54D32444E", "093ACC5F-F7B6-4EB1-B9B7-9F3F5FB85F13", "Learning University", "", "B32639B3-604F-441E-A6E4-2613C0A6BE2B", "" );

            // Add Page 
            //  Internal Name: Courses
            //  Site: External Website
            RockMigrationHelper.AddPage( true, "B32639B3-604F-441E-A6E4-2613C0A6BE2B", "093ACC5F-F7B6-4EB1-B9B7-9F3F5FB85F13", "Courses", "", "5B9DEA29-8C8F-4EDD-9FCF-739061B654D3", "" );

            // Add Page 
            //  Internal Name: Course
            //  Site: External Website
            RockMigrationHelper.AddPage( true, "5B9DEA29-8C8F-4EDD-9FCF-739061B654D3", "093ACC5F-F7B6-4EB1-B9B7-9F3F5FB85F13", "Course", "", "FCCDB330-E1EA-4DC2-971E-3900F1EC2826", "" );

            // Add Page 
            //  Internal Name: Class Workspace
            //  Site: External Website
            RockMigrationHelper.AddPage( true, "FCCDB330-E1EA-4DC2-971E-3900F1EC2826", "093ACC5F-F7B6-4EB1-B9B7-9F3F5FB85F13", "Class Workspace", "", "61BE63C7-6611-4235-A6F2-B22456620F35", "" );

            // Add Page Route
            //   Page:Learning University
            //   Route:learn
            RockMigrationHelper.AddOrUpdatePageRoute( "B32639B3-604F-441E-A6E4-2613C0A6BE2B", "learn", "5F29A36F-8B41-492B-89A5-76DF70284F0D" );

            // Add Page Route
            //   Page:Courses
            //   Route:learn/{LearningProgramId}
            RockMigrationHelper.AddOrUpdatePageRoute( "5B9DEA29-8C8F-4EDD-9FCF-739061B654D3", "learn/{LearningProgramId}", "FA31BDF7-875A-4AAA-BD27-734FF10AF61A" );

            // Add Page Route
            //   Page:Course
            //   Route:learn/{LearningProgramId}/courses/{LearningCourseId}
            RockMigrationHelper.AddOrUpdatePageRoute( "FCCDB330-E1EA-4DC2-971E-3900F1EC2826", "learn/{LearningProgramId}/courses/{LearningCourseId}", "6AC3D62C-488B-44C8-98AF-7D23B7B701DD" );

            // Add Page Route
            //   Page:Class Workspace
            //   Route:learn/{LearningProgramId}/courses/{LearningCourseId}/{LearningClassId}
            RockMigrationHelper.AddOrUpdatePageRoute( "61BE63C7-6611-4235-A6F2-B22456620F35", "learn/{LearningProgramId}/courses/{LearningCourseId}/{LearningClassId}", "E2EF9FAC-3E9B-4EC8-A21F-D01178416247" );

            // Add Block 
            //  Block Name: Program List
            //  Page Name: Learning University
            //  Layout: -
            //  Site: External Website
            RockMigrationHelper.AddBlock( true, "B32639B3-604F-441E-A6E4-2613C0A6BE2B".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "DA1460D8-E895-4B23-8A8E-10EBBED3990F".AsGuid(), "Program List", "Feature", @"", @"", 0, "B15CC3F1-766B-4469-8F95-E31011A3279F" );

            // Add Block 
            //  Block Name: Course List
            //  Page Name: Courses
            //  Layout: -
            //  Site: External Website
            RockMigrationHelper.AddBlock( true, "5B9DEA29-8C8F-4EDD-9FCF-739061B654D3".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "5D6BA94F-342A-4EC1-B024-FC5046FFE14D".AsGuid(), "Course List", "Feature", @"", @"", 0, "7CE9134A-FF7B-4663-8B79-0CA6EED53A1A" );

            // Add Block 
            //  Block Name: Course Detail
            //  Page Name: Course
            //  Layout: -
            //  Site: External Website
            RockMigrationHelper.AddBlock( true, "FCCDB330-E1EA-4DC2-971E-3900F1EC2826".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "B0DCE130-0C91-4AA0-8161-57E8FA523392".AsGuid(), "Course Detail", "Feature", @"", @"", 0, "E921788F-38EA-48F2-B80A-9B7181AB70A5" );

            // Add Block 
            //  Block Name: Class Workspace
            //  Page Name: Class Workspace
            //  Layout: -
            //  Site: External Website
            RockMigrationHelper.AddBlock( true, "61BE63C7-6611-4235-A6F2-B22456620F35".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "55F2E89B-DE57-4E24-AC6C-576956FB97C5".AsGuid(), "Class Workspace", "Feature", @"", @"", 0, "D46C2787-60BA-4776-BE6E-7F785A984922" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Show Completion Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0DCE130-0C91-4AA0-8161-57E8FA523392", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Show Completion Status", "ShowCompletionStatus", "Show Completion Status", @"Determines if the individual's completion status should be shown.", 3, @"Show", "50A1C1C9-42F9-43A1-B115-B762D64F5E84" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Next Session Date Range
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0DCE130-0C91-4AA0-8161-57E8FA523392", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Next Session Date Range", "NextSessionDateRange", "Next Session Date Range", @"Filter to limit the display of upcming sessions.", 4, @",", "E5232B07-6F05-4A14-B58B-DC1978D4D878" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5D6BA94F-342A-4EC1-B024-FC5046FFE14D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "Lava Template", @"The lava template to use to render the page. Merge fields include: Program, Courses, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
//- Variables
{% assign imageFileNameLength = Program.ImageBinaryFile.Guid | Size %}

//- Styles
{% stylesheet %}
    .page-container {
        display: flex;
        flex-direction: column;
        margin-bottom: 12px;
    }
    
    .page-header-section {
        {% if imageFileNameLength > 0 %}
            height: 280px; 
        {% endif %}
        align-items: center; 
        border-radius: 12px; 
        background-image: url('/GetImage.ashx?guid={{Program.ImageBinaryFile.Guid}}'); 
        background-size: cover;
    }
    
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        {% if imageFileNameLength > 0 %}
            bottom: -85%;
            -webkit-transform: translateY(-30%);
            transform: translateY(-30%);
        {% endif %}
        background-color: white; 
        border-radius: 12px; 
        width: 80%; 
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        padding-bottom: 12px;
        margin-bottom: 12px;
    }
    
    .course-item-container {
        max-width: 300px;
        background-color: white; 
        border-radius: 12px;
        margin: 8px;
        display: flex;
        flex-direction: column;
        justify-content: space-between;
    }
{% endstylesheet %}
<div class=""page-container"">
	<div class=""page-header-section mb-5"">
		<div class=""header-block text-center"">
			<h2>
				{{ Program.Name }}
			</h2>
			<div class=""page-sub-header"">
				{{ Program.Summary }}
			</div>
		</div>
	</div>
	
	<div class=""course-list-header-section center-block text-center mb-4"">
		<span class=""course-list-header h5"">
			Courses
		</span>
	</div>
	
	<div class=""course-list-container d-flex flex-fill"">
		{% for course in Courses %}
		<div class=""course-item-container"">
			<div class=""course-item-middle p-3"">
			
				<h4 class=""course-name"">
					{{ course.Entity.PublicName }}
				</h4>
				<div class=""course-category d-flex justify-content-between mb-2"">
				    {% if course.Category and course.Category <> '' %}
					    <span class=""badge badge-info"">{{ course.Category }}</span>
					{% else %}
					    <span> </span>
				    {% endif %}
				    {% if course.Entity.Credits %}
				        <span class=""badge"" style=""background-color: #ddedf2; color: #546a71;"">Credits: {{ course.Entity.Credits }}</span>
				    {% endif %}
				</div>
				<div class=""course-summary text-muted"">
					{{ course.Entity.Summary }} 
				</div>
			</div>
		
			<div class=""course-item-footer d-flex flex-column mt-4 p-3"">
                <div class=""course-next-session text-muted mb-3"">
                    <div class=""text-bold"">Next Session Starts</div>
                    <ul><li>{{ course.NextSemester.StartDate | Date:'MMMM dd, yyyy' }}</li></ul>
                </div>
                
                <div class=""d-flex justify-content-between"">
    				<a class=""btn btn-default"" href=""{{ course.CourseDetailsLink }}"">Learn More</a>

                    {% if ShowCompletionStatus %}
    				    {% if course.LearningCompletionStatus == 'Pass' %}
    					    <span class=""badge badge-success p-2"" style=""line-height: normal;"">Completed</span>
    				    {% elseif course.LearningCompletionStatus == 'Incomplete' %}
    					    <span class=""badge badge-info p-2"" style=""line-height: normal;"">Enrolled</span>
    				    {% elseif course.UnmetPrerequisites != empty %}
                            <a class=""text-muted"" href=""{{ course.PrerequisiteEnrollmentLink }}"">Prerequisites Not Met</a>
                        {% else %}
                            <a class=""text-bold ml-3"" href=""{{ course.CourseEnrollmentLink }}"">Enroll</a>
    				    {% endif %}
                    {% endif %}
			    </div>
			</div>
		</div>
		{% endfor %}
	</div>
</div>", "6ADA636C-F93A-4347-A2E6-3D9C1BDA51C0" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Show Completion Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5D6BA94F-342A-4EC1-B024-FC5046FFE14D", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Show Completion Status", "ShowCompletionStatus", "Show Completion Status", @"Determines if the individual's completion status should be shown.", 3, @"Show", "C276C064-85DF-47AF-886B-672A0942496F" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Next Session Date Range
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5D6BA94F-342A-4EC1-B024-FC5046FFE14D", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Next Session Date Range", "NextSessionDateRange", "Next Session Date Range", @"Filter to limit the display of upcming sessions.", 4, @",", "224BC26D-D7F6-4674-822A-547424E67B77" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA1460D8-E895-4B23-8A8E-10EBBED3990F", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "Lava Template", @"The lava template to use to render the page. Merge fields include: Programs, ShowCompletionStatus, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
//- Styles
{% stylesheet %}
    .page-container {
        display: flex;
        flex-direction: column;
    }
    
    .page-header-section {
        height: 280px; 
        align-items: center; 
        border-radius: 12px; 
        background-image: url('/GetImage.ashx?guid=4812baaf-a173-472c-a9a7-8ceb83c06f53'); 
        background-size: cover;
    }
    
    .programs-list-header-section {
        margin-top: 100px;   
    }
    
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        bottom: -80%;
        -webkit-transform: translateY(-30%);
        transform: translateY(-30%);
        background-color: white; 
        border-radius: 12px; 
        width: 80%; 
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        margin-bottom: 12px;
        padding-bottom: 12px;
    }
    
    .program-item-container {
        background-color: white; 
        border-radius: 12px;
        margin: 8px;
        display: flex;
        flex-direction: column;
        justify-content: space-between;
    }
{% endstylesheet %}
<div class=""page-container"" >
	<div class=""page-header-section"">
		<div class=""header-block text-center"">
			<div class=""h1 text-bold"">
				Growth Through Learning
			</div>
			<div class=""page-sub-header"">
				We believe that spiritual growth is deeply intertwined with continuous learning. 
				""Growth Through Learning"" is our commitment to nurture your faith journey, providing resources and opportunies to deepen your understanding of God's Word and his purpose for your life.
			</div>
		</div>
	</div>
	
	<div class=""programs-list-header-section center-block text-center mb-4"">
		<span class=""program-list-header h5"">
			Programs Available
		</span>

		<div class=""program-list-sub-header text-muted"">
			The following types of classes are available.
		</div>
	</div>
	
	<div class=""program-list-container d-flex flex-fill"">
		{% for program in Programs %}
		<div class=""program-item-container"">
		
			{% if program.ImageFileGuid and program.ImageFileGuid != '' %}
			<div class=""program-image program-item-header"" >
				<img style=""border-radius: 12px 12px 0 0;"" src=""/GetImage.ashx?guid={{program.ImageFileGuid}}&maxwidth=300&maxheight=150&mode=crop"" />
			</div>
			{% endif %}
			
			<div class=""program-item-middle p-3"">
			
				<h4 class=""program-name"">
					{{ program.Entity.PublicName }}
				</h4>
				<div class=""program-category"">
					<span class=""badge badge-info"">{{ program.Category }}</span>
				</div>
				<div class=""program-summary text-muted"">
					{{ program.Entity.Summary }} 
				</div>
			</div>
			
			<div class=""program-item-footer d-flex justify-content-between mt-4 p-3"">
				<a class=""btn btn-default"" href=""{{ program.CoursesLink }}"">Learn More</a>

                {% if ShowCompletionStatus == True %} 
				    {% if program.CompletionStatus == 'Completed' %}
					    <span class=""badge badge-success p-2"" style=""line-height: normal;"">Completed</span>
				    {% elseif program.CompletionStatus == 'Pending' %}
					    <span class=""badge badge-info p-2"" style=""line-height: normal;"">Enrolled</span>
				    {% endif %}
                {% endif %}
			</div>
		</div>
		{% endfor %}
	</div>
</div>", "D7B2012B-3D59-4DE3-8328-53B7EB9F7C1C" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Program Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA1460D8-E895-4B23-8A8E-10EBBED3990F", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Program Categories", "ProgramCategories", "Program Categories", @"The categories to filter the programs by.", 3, @"", "7F8B8367-2A29-49E1-B2B4-96560AE68510" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Show Completion Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA1460D8-E895-4B23-8A8E-10EBBED3990F", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Show Completion Status", "ShowCompletionStatus", "Show Completion Status", @"Determines if the individual's completion status should be shown.", 4, @"Show", "D7B8A019-B32F-4D1E-BECE-40EB254AF5A7" );

            // Add Block Attribute Value
            //   Block: Class Workspace
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Block Location: Page=Class Workspace, Site=External Website
            //   Attribute: Facilitator Portal Page
            /*   Attribute Value: 72c75c91-18f8-48d0-b0cf-1fbd82eb50fc,827af9a8-bf1a-4008-b4c3-3d07076acb84 */
            RockMigrationHelper.AddBlockAttributeValue( "D46C2787-60BA-4776-BE6E-7F785A984922", "72DFE773-DA2F-45A8-976A-6C19FD0AFE28", @"72c75c91-18f8-48d0-b0cf-1fbd82eb50fc,827af9a8-bf1a-4008-b4c3-3d07076acb84" );

            // Add Block Attribute Value
            //   Block: Class Workspace
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Block Location: Page=Class Workspace, Site=External Website
            //   Attribute: Lava Header Template
            /*   Attribute Value: ... */
            RockMigrationHelper.AddBlockAttributeValue( "D46C2787-60BA-4776-BE6E-7F785A984922", "F7F1BE33-AF44-4F73-9C55-0F8FF97E1B69", @"{% assign imageFileNameLength = Course.ImageFileGuid | Size %}

//- Styles
{% stylesheet %}
    .header-container {
        display: flex;
        flex-direction: column;
        margin-bottom: 12px;
    }
    
    .page-header-section {
        {% if imageFileNameLength > 0 %}
            height: 280px;
            background-image: url('/GetImage.ashx?guid={{Course.ImageFileGuid}}'); 
            background-size: cover;
        {% endif %}
        align-items: center; 
        border-radius: 12px; 
    }
    
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        {% if imageFileNameLength > 0 %}
            bottom: -85%;
            -webkit-transform: translateY(-30%);
            transform: translateY(-30%);
        {% endif %}
        background-color: white; 
        border-radius: 12px; 
        width: 80%; 
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        padding-bottom: 12px;
        margin-bottom: 12px;
    }
{% endstylesheet %}
<div class=""header-container"">
	<div class=""page-header-section mb-5"">
		<div class=""header-block text-center"">
			<h2>
				{{ Course.PublicName }}
			</h2>
			<div class=""page-sub-header"">
				{{ Course.Summary }}
			</div>
		</div>
	</div>
</div>
" );

            // Add Block Attribute Value
            //   Block: Class Workspace
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Block Location: Page=Class Workspace, Site=External Website
            //   Attribute: The Number of Notifications to Show
            /*   Attribute Value: 3 */
            RockMigrationHelper.AddBlockAttributeValue( "D46C2787-60BA-4776-BE6E-7F785A984922", "3B8378C9-80DD-4A50-9197-CA9E1EC88507", @"3" );

            // Add Block Attribute Value
            //   Block: Class Workspace
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Block Location: Page=Class Workspace, Site=External Website
            //   Attribute: Show Grades
            /*   Attribute Value: Show */
            RockMigrationHelper.AddBlockAttributeValue( "D46C2787-60BA-4776-BE6E-7F785A984922", "6FE66C3C-E37B-440D-942C-88C008E844F5", @"Show" );

            // Add Block Attribute Value
            //   Block: Course Detail
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Block Location: Page=Course, Site=External Website
            //   Attribute: Workspace Page
            /*   Attribute Value: 61be63c7-6611-4235-a6f2-b22456620f35,e2ef9fac-3e9b-4ec8-a21f-d01178416247 */
            RockMigrationHelper.AddBlockAttributeValue( "E921788F-38EA-48F2-B80A-9B7181AB70A5", "8787C9F3-1CF9-4790-B65E-90303F446536", @"61be63c7-6611-4235-a6f2-b22456620f35,e2ef9fac-3e9b-4ec8-a21f-d01178416247" );

            // Add Block Attribute Value
            //   Block: Course Detail
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Block Location: Page=Course, Site=External Website
            //   Attribute: Enrollment Page
            /*   Attribute Value: 61be63c7-6611-4235-a6f2-b22456620f35,e2ef9fac-3e9b-4ec8-a21f-d01178416247 */
            RockMigrationHelper.AddBlockAttributeValue( "E921788F-38EA-48F2-B80A-9B7181AB70A5", "96644CEF-4FC7-4986-B591-D6675AA38C2C", @"61be63c7-6611-4235-a6f2-b22456620f35,e2ef9fac-3e9b-4ec8-a21f-d01178416247" );

            // Add Block Attribute Value
            //   Block: Course Detail
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Block Location: Page=Course, Site=External Website
            //   Attribute: Lava Template
            /*   Attribute Value: ... */
            RockMigrationHelper.AddBlockAttributeValue( "E921788F-38EA-48F2-B80A-9B7181AB70A5", "0935CE10-BD61-4C85-9D6C-0269DEC2EEAC", @"//- Variable Assignments
{% assign requirementTypes = Course.CourseRequirements | Distinct:'RequirementType' %}
{% assign prerequisitesText = Course.CourseRequirements | Where:'RequirementType','Prerequisite' | Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
{% assign facilitatorCount = Course.Facilitators | Size %}
{% assign facilitators = Course.Facilitators | Join:', ' | ReplaceLast:',',' and' | Default:'TBD' %}
{% assign imageFileNameLength = Course.ImageFileGuid | Size %}

//- Styles
{% stylesheet %}
    .page-container {
        display: flex;
        flex-direction: column;
        margin-bottom: 12px;
    }
    
    .page-header-section {
        {% if imageFileNameLength > 0 %}
            height: 280px;
            background-image: url('/GetImage.ashx?guid={{Course.ImageFileGuid}}'); 
            background-size: cover;
        {% endif %}
        align-items: center; 
        border-radius: 12px; 
    }
    
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        {% if imageFileNameLength > 0 %}
            bottom: -85%;
            -webkit-transform: translateY(-30%);
            transform: translateY(-30%);
        {% endif %}
        background-color: white; 
        border-radius: 12px; 
        width: 80%; 
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        padding-bottom: 12px;
        margin-bottom: 12px;
    }
    
    .page-main-content {
        margin-top: 30px;   
    }
    
    .course-detail-container {
        background-color: white; 
        border-radius: 12px;
        padding: 12px;
        display: flex;
        flex-direction: column;
    }
    
    .course-status-sidebar-container {
        padding: 12px; 
        margin-left: 12px;
        background-color: white; 
        border-radius: 12px;
        width: 300px;
    }
{% endstylesheet %}
<div class=""page-container"">
	<div class=""page-header-section mb-5"">
		<div class=""header-block text-center"">
			<h2>
				{{ Course.Entity.PublicName }}
			</h2>
			<div class=""page-sub-header"">
				{{ Course.Entity.Summary }}
			</div>
		</div>
	</div>
	
	<div class=""page-main-content d-flex"">
		<div class=""course-detail-container text-muted"">
			<div class=""description-header h4"">Course Description</div>
			
			<div class=""course-item-pair-container course-code"">
				<span class=""text-bold"">Course Code: </span>
				<span>{{Course.Entity.CourseCode}}</span>
			</div>
			
			<div class=""course-item-pair-container credits"">
				<span class=""text-bold"">Credits: </span>
				<span>{{Course.Entity.Credits}}</span>
			</div>
			
			<div class=""course-item-pair-container prerequisites"">
				<span class=""text-bold"">Prerequisites: </span>
				
				<span>
					{{ prerequisitesText }}
				</span>
			</div>
			
			<div class=""course-item-pair-container description"">
				<span>{{Course.DescriptionAsHtml}}</span>
			</div>
		</div>
		
		
		<div class=""course-side-panel d-flex flex-column"">
			<div class=""course-status-sidebar-container"">
				
			{% case Course.LearningCompletionStatus %}
			{% when 'Incomplete' %} 
				<div class=""sidebar-header text-bold"">Currently Enrolled</div>
				<div class=""sidebar-value text-muted"">You are currently enrolled in this course.</div>
					
				<div class=""side-bar-action mt-3"">
					<a class=""btn btn-info"" href=""{{ Course.ClassWorkspaceLink }}"">View Course</a>
				</div>
				
                {% for requirementType in requirementTypes %}
                	{% assign requirementsText = Course.CourseRequirements | Where:'RequirementType',requirementType | Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
                				<div class=""sidebar-header text-bold"">{{ requirementType | Pluralize }}</div>
                				<div class=""sidebar-value text-muted"">{{ requirementsText }}</div>
                {% endfor %}
			{% when 'Passed' %} 
				<div class=""sidebar-header text-bold"">History</div>
				<div class=""sidebar-value text-muted"">You completed this class on {{ Course.MostRecentParticipation.LearningCompletionDateTime | Date: 'MMMM dd, yyyy' }}</div>
				
				<div class=""side-bar-action mt-3"">
					<a href=""{{ Course.ClassWorkspaceLink }}"">View Class Work</a>
				</div>
				
                {% for requirementType in requirementTypes %}
                	{% assign requirementsText = Course.CourseRequirements | Where:'RequirementType',requirementType | Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
                				<div class=""sidebar-header text-bold"">{{ requirementType | Pluralize }}</div>
                				<div class=""sidebar-value text-muted"">{{ requirementsText }}</div>
                {% endfor %}
			{% else %} 
                {% for requirementType in requirementTypes %}
                	{% assign requirementsText = Course.CourseRequirements | Where:'RequirementType',requirementType | Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
                				<div class=""sidebar-header text-bold"">{{ requirementType | Pluralize }}</div>
                				<div class=""sidebar-value text-muted"">{{ requirementsText }}</div>
                {% endfor %}
				
				<div class=""sidebar-upcoming-schedule h4"">Upcoming Schedule</div>
				
				<div class=""side-bar-header-value-pair text-muted"">
					<div class=""sidebar-header text-bold"">Next Session Semester: </div>
					<div class=""sidebar-value"">{{ Course.NextSemester.Name }}</div>
				</div>
				
				<div class=""side-bar-header-value-pair text-muted"">
					<div class=""sidebar-header text-bold"">{{ 'Instructor' | PluralizeForQuantity:facilitatorCount }}:</div>
					<div class=""sidebar-value"">{{ facilitators }}</div>
				</div>
				<div class=""side-bar-action mt-3"">
					<a class=""btn btn-info"" href=""{{ Course.CourseEnrollmentLink }}"">Enroll</a>
				</div>
			{% endcase %}
			</div>
		</div>
	</div>
</div>" );

            // Add Block Attribute Value
            //   Block: Course List
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=External Website
            //   Attribute: Detail Page
            /*   Attribute Value: fccdb330-e1ea-4dc2-971e-3900f1ec2826,6ac3d62c-488b-44c8-98af-7d23b7b701dd */
            RockMigrationHelper.AddBlockAttributeValue( "7CE9134A-FF7B-4663-8B79-0CA6EED53A1A", "5A8A251D-56B3-4E3B-8BF9-1D333C016B74", @"fccdb330-e1ea-4dc2-971e-3900f1ec2826,6ac3d62c-488b-44c8-98af-7d23b7b701dd" );

            // Add Block Attribute Value
            //   Block: Course List
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=External Website
            //   Attribute: Enrollment Page
            /*   Attribute Value: 61be63c7-6611-4235-a6f2-b22456620f35,e2ef9fac-3e9b-4ec8-a21f-d01178416247 */
            RockMigrationHelper.AddBlockAttributeValue( "7CE9134A-FF7B-4663-8B79-0CA6EED53A1A", "FF294CFA-6D54-4CBC-B8EF-D45893677D58", @"61be63c7-6611-4235-a6f2-b22456620f35,e2ef9fac-3e9b-4ec8-a21f-d01178416247" );

            // Add Block Attribute Value
            //   Block: Course List
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=External Website
            //   Attribute: Lava Template
            /*   Attribute Value: ... */
            RockMigrationHelper.AddBlockAttributeValue( "7CE9134A-FF7B-4663-8B79-0CA6EED53A1A", "6ADA636C-F93A-4347-A2E6-3D9C1BDA51C0", @"//- Variables
{% assign imageFileNameLength = Program.ImageBinaryFile.Guid | Size %}

//- Styles
{% stylesheet %}
    .page-container {
        display: flex;
        flex-direction: column;
        margin-bottom: 12px;
    }
    
    .page-header-section {
        {% if imageFileNameLength > 0 %}
            height: 280px; 
        {% endif %}
        align-items: center; 
        border-radius: 12px; 
        background-image: url('/GetImage.ashx?guid={{Program.ImageBinaryFile.Guid}}'); 
        background-size: cover;
    }
    
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        {% if imageFileNameLength > 0 %}
            bottom: -85%;
            -webkit-transform: translateY(-30%);
            transform: translateY(-30%);
        {% endif %}
        background-color: white; 
        border-radius: 12px; 
        width: 80%; 
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        padding-bottom: 12px;
        margin-bottom: 12px;
    }
    
    .course-item-container {
        max-width: 300px;
        background-color: white; 
        border-radius: 12px;
        margin: 8px;
        display: flex;
        flex-direction: column;
        justify-content: space-between;
    }
{% endstylesheet %}
<div class=""page-container"">
	<div class=""page-header-section mb-5"">
		<div class=""header-block text-center"">
			<h2>
				{{ Program.Name }}
			</h2>
			<div class=""page-sub-header"">
				{{ Program.Summary }}
			</div>
		</div>
	</div>
	
	<div class=""course-list-header-section center-block text-center mb-4"">
		<span class=""course-list-header h5"">
			Courses
		</span>
	</div>
	
	<div class=""course-list-container d-flex flex-fill"">
		{% for course in Courses %}
		<div class=""course-item-container"">
			<div class=""course-item-middle p-3"">
			
				<h4 class=""course-name"">
					{{ course.Entity.PublicName }}
				</h4>
				<div class=""course-category d-flex justify-content-between mb-2"">
				    {% if course.Category and course.Category <> '' %}
					    <span class=""badge badge-info"">{{ course.Category }}</span>
					{% else %}
					    <span> </span>
				    {% endif %}
				    {% if course.Entity.Credits %}
				        <span class=""badge"" style=""background-color: #ddedf2; color: #546a71;"">Credits: {{ course.Entity.Credits }}</span>
				    {% endif %}
				</div>
				<div class=""course-summary text-muted"">
					{{ course.Entity.Summary }} 
				</div>
			</div>
		
			<div class=""course-item-footer d-flex flex-column mt-4 p-3"">
                <div class=""course-next-session text-muted mb-3"">
                    <div class=""text-bold"">Next Session Starts</div>
                    <ul><li>{{ course.NextSemester.StartDate | Date:'MMMM dd, yyyy' }}</li></ul>
                </div>
                
                <div class=""d-flex justify-content-between"">
    				<a class=""btn btn-default"" href=""{{ course.CourseDetailsLink }}"">Learn More</a>

                    {% if ShowCompletionStatus %}
    				    {% if course.LearningCompletionStatus == 'Pass' %}
    					    <span class=""badge badge-success p-2"" style=""line-height: normal;"">Completed</span>
    				    {% elseif course.LearningCompletionStatus == 'Incomplete' %}
    					    <span class=""badge badge-info p-2"" style=""line-height: normal;"">Enrolled</span>
    				    {% elseif course.UnmetPrerequisites != empty %}
                            <a class=""text-muted"" href=""{{ course.PrerequisiteEnrollmentLink }}"">Prerequisites Not Met</a>
                        {% else %}
                            <a class=""text-bold ml-3"" href=""{{ course.CourseEnrollmentLink }}"">Enroll</a>
    				    {% endif %}
                    {% endif %}
			    </div>
			</div>
		</div>
		{% endfor %}
	</div>
</div>" );

            // Add Block Attribute Value
            //   Block: Course List
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=External Website
            //   Attribute: Show Completion Status
            /*   Attribute Value: Show */
            RockMigrationHelper.AddBlockAttributeValue( "7CE9134A-FF7B-4663-8B79-0CA6EED53A1A", "C276C064-85DF-47AF-886B-672A0942496F", @"Show" );

            // Add Block Attribute Value
            //   Block: Course List
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=External Website
            //   Attribute: Next Session Date Range
            /*   Attribute Value: DateRange|||2024-07-18T00:00:00.0000000|2025-07-18T00:00:00.0000000 */
            RockMigrationHelper.AddBlockAttributeValue( "7CE9134A-FF7B-4663-8B79-0CA6EED53A1A", "224BC26D-D7F6-4674-822A-547424E67B77", @"DateRange|||2024-07-18T00:00:00.0000000|2025-07-18T00:00:00.0000000" );

            // Add Block Attribute Value
            //   Block: Program List
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Block Location: Page=Learning University, Site=External Website
            //   Attribute: Lava Template
            /*   Attribute Value: ... */
            RockMigrationHelper.AddBlockAttributeValue( "B15CC3F1-766B-4469-8F95-E31011A3279F", "D7B2012B-3D59-4DE3-8328-53B7EB9F7C1C", @"//- Styles
{% stylesheet %}
    .page-container {
        display: flex;
        flex-direction: column;
    }
    
    .page-header-section {
        height: 280px; 
        align-items: center; 
        border-radius: 12px; 
        background-image: url('/GetImage.ashx?guid=4812baaf-a173-472c-a9a7-8ceb83c06f53'); 
        background-size: cover;
    }
    
    .programs-list-header-section {
        margin-top: 100px;   
    }
    
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        bottom: -80%;
        -webkit-transform: translateY(-30%);
        transform: translateY(-30%);
        background-color: white; 
        border-radius: 12px; 
        width: 80%; 
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        margin-bottom: 12px;
        padding-bottom: 12px;
    }
    
    .program-item-container {
        background-color: white; 
        border-radius: 12px;
        margin: 8px;
        display: flex;
        flex-direction: column;
        justify-content: space-between;
    }
{% endstylesheet %}
<div class=""page-container"" >
	<div class=""page-header-section"">
		<div class=""header-block text-center"">
			<div class=""h1 text-bold"">
				Growth Through Learning
			</div>
			<div class=""page-sub-header"">
				We believe that spiritual growth is deeply intertwined with continuous learning. 
				""Growth Through Learning"" is our commitment to nurture your faith journey, providing resources and opportunies to deepen your understanding of God's Word and his purpose for your life.
			</div>
		</div>
	</div>
	
	<div class=""programs-list-header-section center-block text-center mb-4"">
		<span class=""program-list-header h5"">
			Programs Available
		</span>

		<div class=""program-list-sub-header text-muted"">
			The following types of classes are available.
		</div>
	</div>
	
	<div class=""program-list-container d-flex flex-fill"">
		{% for program in Programs %}
		<div class=""program-item-container"">
		
			{% if program.ImageFileGuid and program.ImageFileGuid != '' %}
			<div class=""program-image program-item-header"" >
				<img style=""border-radius: 12px 12px 0 0;"" src=""/GetImage.ashx?guid={{program.ImageFileGuid}}&maxwidth=300&maxheight=150&mode=crop"" />
			</div>
			{% endif %}
			
			<div class=""program-item-middle p-3"">
			
				<h4 class=""program-name"">
					{{ program.Entity.PublicName }}
				</h4>
				<div class=""program-category"">
					<span class=""badge badge-info"">{{ program.Category }}</span>
				</div>
				<div class=""program-summary text-muted"">
					{{ program.Entity.Summary }} 
				</div>
			</div>
			
			<div class=""program-item-footer d-flex justify-content-between mt-4 p-3"">
				<a class=""btn btn-default"" href=""{{ program.CoursesLink }}"">Learn More</a>
				
				{% if ShowCompletionStatus %}
    				{% if program.CompletionStatus == 'Completed' %}
    					<span class=""badge badge-success p-2"" style=""line-height: normal;"">Completed</span>
    				{% elseif program.CompletionStatus == 'Pending' %}
    					<span class=""badge badge-info p-2"" style=""line-height: normal;"">Enrolled</span>
    				{% endif %}
				{% endif %}
			</div>
		</div>
		{% endfor %}
	</div>
</div>" );

            // Add Block Attribute Value
            //   Block: Program List
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Block Location: Page=Learning University, Site=External Website
            //   Attribute: Show Completion Status
            /*   Attribute Value: Show */
            RockMigrationHelper.AddBlockAttributeValue( "B15CC3F1-766B-4469-8F95-E31011A3279F", "D7B8A019-B32F-4D1E-BECE-40EB254AF5A7", @"Show" );

            // Add Block Attribute Value
            //   Block: Program List
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Block Location: Page=Learning University, Site=External Website
            //   Attribute: Detail Page
            /*   Attribute Value: 5b9dea29-8c8f-4edd-9fcf-739061b654d3,fa31bdf7-875a-4aaa-bd27-734ff10af61a */
            RockMigrationHelper.AddBlockAttributeValue( "B15CC3F1-766B-4469-8F95-E31011A3279F", "0317E0AD-9FE9-409B-9A14-C3D30D303B23", @"5b9dea29-8c8f-4edd-9fcf-739061b654d3,fa31bdf7-875a-4aaa-bd27-734ff10af61a" );
        }

        /// <summary>
        /// The Down methods from the SQL code-generated CodeGen_PagesBlocksAttributesMigration.sql file.
        /// </summary>
        private void DeleteBlockAndPageAttributes()
        {
            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Semester Detail Page
            RockMigrationHelper.DeleteAttribute( "5A60ECD5-9BCA-4DC5-8077-12E00D475C3B" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Completion Detail Page
            RockMigrationHelper.DeleteAttribute( "9FEE276F-9CC9-4FD2-99FF-270B721C6300" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Next Session Date Range
            RockMigrationHelper.DeleteAttribute( "E5232B07-6F05-4A14-B58B-DC1978D4D878" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Show Completion Status
            RockMigrationHelper.DeleteAttribute( "50A1C1C9-42F9-43A1-B115-B762D64F5E84" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Next Session Date Range
            RockMigrationHelper.DeleteAttribute( "224BC26D-D7F6-4674-822A-547424E67B77" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Show Completion Status
            RockMigrationHelper.DeleteAttribute( "C276C064-85DF-47AF-886B-672A0942496F" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Lava Template
            RockMigrationHelper.DeleteAttribute( "6ADA636C-F93A-4347-A2E6-3D9C1BDA51C0" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Show Completion Status
            RockMigrationHelper.DeleteAttribute( "D7B8A019-B32F-4D1E-BECE-40EB254AF5A7" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Program Categories
            RockMigrationHelper.DeleteAttribute( "7F8B8367-2A29-49E1-B2B4-96560AE68510" );

            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Lava Template
            RockMigrationHelper.DeleteAttribute( "D7B2012B-3D59-4DE3-8328-53B7EB9F7C1C" );

            // Remove Block
            //  Name: Program List, from Page: Learning University, Site: External Website
            //  from Page: Learning University, Site: External Website
            RockMigrationHelper.DeleteBlock( "B15CC3F1-766B-4469-8F95-E31011A3279F" );

            // Remove Block
            //  Name: Course List, from Page: Courses, Site: External Website
            //  from Page: Courses, Site: External Website
            RockMigrationHelper.DeleteBlock( "7CE9134A-FF7B-4663-8B79-0CA6EED53A1A" );

            // Remove Block
            //  Name: Public Learning Course Detail, from Page: Course, Site: External Website
            //  from Page: Course, Site: External Website
            RockMigrationHelper.DeleteBlock( "E921788F-38EA-48F2-B80A-9B7181AB70A5" );

            // Remove Block
            //  Name: Class Workspace, from Page: Class Workspace, Site: External Website
            //  from Page: Class Workspace, Site: External Website
            RockMigrationHelper.DeleteBlock( "D46C2787-60BA-4776-BE6E-7F785A984922" );

            // Remove Block
            //  Name: Program Completion Detail, from Page: Completions, Site: Rock RMS
            //  from Page: Completions, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F7F137EE-C66D-4BF9-B117-DC8899C6603A" );

            // Delete Page 
            //  Internal Name: Class Workspace
            //  Site: External Website
            //  Layout: Homepage
            RockMigrationHelper.DeletePage( "61BE63C7-6611-4235-A6F2-B22456620F35" );

            // Delete Page 
            //  Internal Name: Course
            //  Site: External Website
            //  Layout: Homepage
            RockMigrationHelper.DeletePage( "FCCDB330-E1EA-4DC2-971E-3900F1EC2826" );

            // Delete Page 
            //  Internal Name: Courses
            //  Site: External Website
            //  Layout: Homepage
            RockMigrationHelper.DeletePage( "5B9DEA29-8C8F-4EDD-9FCF-739061B654D3" );

            // Delete Page 
            //  Internal Name: Learning University
            //  Site: External Website
            //  Layout: Homepage
            RockMigrationHelper.DeletePage( "B32639B3-604F-441E-A6E4-2613C0A6BE2B" );

            // Delete BlockType 
            //   Name: Learning Program Completion Detail
            //   Category: LMS
            //   Path: -
            //   EntityType: Learning Program Completion Detail
            RockMigrationHelper.DeleteBlockType( "E0C38A42-2ACE-4258-8D11-BD971C41EADB" );
        }

        /// <summary>
        /// Sets the ShowInGroupList value for the LMS Group Type.
        /// </summary>
        /// <param name="show"><c>true</c> to show in Group List, otherwise <c>false</c></param>
        private void SetShowInGroupList( bool show )
        {
            var sqlBitValue = show ? "1" : "0";
            Sql( $@"
DECLARE @lmsGroupTypeGuid uniqueidentifier = '4BBC41E2-0A37-4289-B7A7-756B9FE8F775';
UPDATE [dbo].[GroupType] SET [ShowInGroupList] = {sqlBitValue} WHERE [Guid] = @lmsGroupTypeGuid
" );
        }

        /// <summary>
        /// Sets the BreadCrumbDisplayName bit value for LMS Detail Pages.
        /// </summary>
        /// <param name="showBreadCrumbDisplayName"><c>true</c> to show in Group List, otherwise <c>false</c></param>
        private void SetBreadCrumbForDetailPagesWithBreadCrumbBlocks( bool showBreadCrumbDisplayName )
        {
            var sqlBitValue = showBreadCrumbDisplayName ? "1" : "0";
            Sql( $@"
DECLARE @programDetailGuid NVARCHAR(40) = '7888CAF4-AF5D-44BA-AB9E-80138361F69D',
		@courseDetailGuid NVARCHAR(40) = 'A57D990E-6F34-45CF-ABAA-08C40E8D4844',
		@classDetailGuid NVARCHAR(40) = '23D5076E-C062-4987-9985-B3A4792BF3CE',
		@activityDetailGuid NVARCHAR(40) = 'D72DCBC4-C57F-4028-B503-1954925EDC7D',
		@completionDetailGuid NVARCHAR(40) = 'E0F2E4F1-ED10-49F6-B053-AC6807994204',
		@participantDetailGuid NVARCHAR(40) = '72C75C91-18F8-48D0-B0CF-1FBD82EB50FC',
		@contentPageDetailGuid NVARCHAR(40) = 'E6A89360-B7B4-48A8-B799-39A27EAB6F36',
		@announcementDetailGuid NVARCHAR(40) = '7C8134A4-524A-4C3D-BA4C-875FEE672850',
		@semesterDetailGuid NVARCHAR(40) = '36FFA805-B283-443E-990D-87040339D16F';

UPDATE p SET
	BreadCrumbDisplayName = {sqlBitValue}
FROM [dbo].[Page] p
WHERE p.[Guid] IN (
	@programDetailGuid, 
	@courseDetailGuid, 
	@classDetailGuid, 
	@activityDetailGuid,
	@completionDetailGuid, 
	@participantDetailGuid,
	@contentPageDetailGuid,
	@announcementDetailGuid,
	@semesterDetailGuid
)" );
        }

        /// <summary>
        /// Adds a parent and child history category for <see cref="LearningClassActivityCompletion"/> changes.
        /// </summary>
        private void AddHistoryCategories()
        {
            Sql( @"
DECLARE @historyEntityTypeId INT = (SELECT TOP 1 Id FROM [dbo].[EntityType] WHERE [Guid] = '546D5F43-1184-47C9-8265-2D7BF4E1BCA5');
DECLARE @parentCategoryGuid NVARCHAR(200) = 'FE5E132D-1A34-4ED5-AE0D-0FDE26D88D25';

-- Add the parent category.
INSERT [Category] ( [IsSystem], [EntityTypeId], [Name], [Description], [Order], [Guid] )
SELECT [IsSystem], [EntityTypeId], [Name], [Description], [Order], [Guid]
FROM (
	SELECT 1 [IsSystem], @historyEntityTypeId [EntityTypeId], 'Learning' [Name], 'Learning History' [Description], 0 [Order], @parentCategoryGuid [Guid]
) [seed]
WHERE NOT EXISTS (
	SELECT 1
	FROM [dbo].[Category] [ex]
	WHERE [ex].[Guid] = [seed].[Guid]
)

DECLARE @learningParentCategoryId INT = (SELECT TOP 1 [Id] FROM [dbo].[Category] WHERE [Guid] = @parentCategoryGuid);
DECLARE @childCategoryGuid NVARCHAR(200) = '40A49BFF-5AE5-487B-B4AA-95DE435209FE';
-- Add the child category.
INSERT [Category] ( [IsSystem], [ParentCategoryId], [EntityTypeId], [Name], [IconCssClass], [Description], [Order], [Guid] )
SELECT [IsSystem], [ParentCategoryId], [EntityTypeId], [Name], [IconCssClass], [Description], [Order], [Guid]
FROM (
	SELECT 1 [IsSystem], @learningParentCategoryId [ParentCategoryId], @historyEntityTypeId [EntityTypeId], 'Activity Completion' [Name], 'fa fa-university' [IconCssClass], 'Learning History' [Description], 0 [Order], @childCategoryGuid [Guid]
) [seed]
WHERE NOT EXISTS (
	SELECT 1
	FROM [dbo].[Category] [ex]
	WHERE [ex].[Guid] = [seed].[Guid]
)" );
        }

        /// <summary>
        /// Removes the parent and child history categories for <see cref="LearningClassActivityCompletion"/> changes.
        /// </summary>
        private void RemoveHistoryCategories()
        {
            Sql( @"
-- Remove the Learning History Categories.
DECLARE @parentCategoryGuid NVARCHAR(200) = 'FE5E132D-1A34-4ED5-AE0D-0FDE26D88D25';
DECLARE @childCategoryGuid NVARCHAR(200) = '40A49BFF-5AE5-487B-B4AA-95DE435209FE';
DELETE Category WHERE [Guid] IN (@parentCategoryGuid, @childCategoryGuid)
" );
        }

        #endregion

        #region JE: Peer Network Relationship Type for Following

        private void PeerNetworkRelationshipTypeForFollowingUp()
        {
            // Failed when converted from Plugin Migration to EF Migration. Fixed with wrapping the SQL in an IF NOT EXISTS.
            //            Sql( @"
            //DECLARE @RelationshipTypeDefindedTypeId int = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = 'f2e8e639-f16d-489d-aafb-be0133531e41');
            //INSERT INTO [DefinedValue]
            //( [DefinedTypeId], [Value], [Description], [Guid], [IsSystem], [Order] )
            //VALUES
            //( @RelationshipTypeDefindedTypeId, 'Following Connections','Connects individuals who are following each other.', '84E0360E-0828-E5A5-4BCC-F3113BE338A1', 1, 0 )
            //" );

            Sql( @"IF NOT EXISTS( SELECT [Guid] FROM [DefinedValue] WHERE [Guid] = '84E0360E-0828-E5A5-4BCC-F3113BE338A1' )
BEGIN
	DECLARE @RelationshipTypeDefindedTypeId int = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = 'f2e8e639-f16d-489d-aafb-be0133531e41');
	INSERT INTO [DefinedValue]
	( [DefinedTypeId], [Value], [Description], [Guid], [IsSystem], [Order] )
	VALUES
	( @RelationshipTypeDefindedTypeId, 'Following Connections','Connects individuals who are following each other.', '84E0360E-0828-E5A5-4BCC-F3113BE338A1', 1, 0 )
END" );
        }

        #endregion

        #region KA: Migration to Update Giving Metric SQL

        void UpdateGivingMetricSqlUp()
        {
            Sql( @"
UPDATE dbo.[Metric]
SET [SourceSql] = REPLACE(
	REPLACE([SourceSql], 'DECLARE @Accounts VARCHAR(100) = ''1''' , 'DECLARE @Accounts VARCHAR(100) = ''1,2,3'''),
'SELECT
    [GivingAmount] AS [GivingAmount]
    , [AccountCampusId] AS [CampusId]
	, [AccountName]
FROM CTE
GROUP BY [AccountCampusId], [AccountName], [GivingAmount];',
'SELECT
    SUM([GivingAmount]) AS [GivingAmount]
    , [AccountCampusId] AS [CampusId]
FROM CTE
GROUP BY [AccountCampusId];')
WHERE [Guid] = '43338E8A-622A-4195-B153-285E570B229D'
" );
        }

        #endregion

        #region KA: D: v17 Data Migration to add route name for Tithing Overview

        private void AddRouteNameForTithingOverviewUp()
        {
            // Add Page Route.
            RockMigrationHelper.AddOrUpdatePageRoute( "72BA5DD9-8685-4182-833D-22BB1E0F9A36", "finance/tithing-overview", "8DC007D6-1C14-4D8D-B04B-886F36406806" );

            // Update Tithing Metrics SQL to not include PostalCodes with negative FamiliesMedianTithe.
            Sql( @"
UPDATE dbo.[Metric]
SET SourceSql = REPLACE([SourceSql]
,'FamiliesMedianTithe is NOT NULL'
,'[FamiliesMedianTithe] is NOT NULL and [FamiliesMedianTithe] > 0')
WHERE [Guid] IN ('f4951a42-9f71-4cb1-a46e-2a7ed84cd923','2b798177-e8f4-46db-a1d7-308d63ca519a', 'b5bfab51-9b46-4e7e-992e-b0119e4d25ec')
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        private void AddRouteNameForTithingOverviewDown()
        {
            RockMigrationHelper.DeletePageRoute( "8DC007D6-1C14-4D8D-B04B-886F36406806" );
        }

        #endregion

        #region KH: v17 ONLY - Reorganize Admin Settings Pages (data migration rollup)

        private const string AdministrationPageGuid = "550A898C-EDEA-48B5-9C58-B20EC13AF13B";
        private const string SQLPageGuid = "03C49950-9C4C-4668-9C65-9A0DF43D1B33";
        private const string DigitalToolsPageGuid = "A6D78C4F-958F-4196-B8FF-527A10F5F047";
        private const string ContentChannelsPageGuid = "8ADCE4B2-8E95-4FA3-89C4-06A883E8145E";
        private const string WebsitesPageGuid = "7596D389-4EAB-4535-8BEE-229737F46F44";
        private const string MobileApplicationsPageGuid = "784259EC-46B7-4DE3-AC37-E8BFDB0B90A6";
        private const string TVAppsPageGuid = "C8B81EBE-E98F-43EF-9E39-0491685145E2";
        private const string MediaAccountsPageGuid = "07CB7BB5-1465-4E75-8DD4-28FA6EA48222";
        private const string ShortLinksPageGuid = "8C0114FF-31CF-443E-9278-3F9E6087140C";
        private const string AdaptiveMessagePageGuid = "73112D38-E051-4452-AEF9-E473EEDD0BCB";

        private void ReorganizeAdminSettingsPages()
        {
            AddAndSetupDigitalToolsPage();
            UpdateAdminToolsPages();
            MoveAdminToolsPages();
            AddAdminToolsPageRoutes();
        }

        private void AddAndSetupDigitalToolsPage()
        {
            // Add Page 
            //  Internal Name: Digital Tools
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "84E12152-E456-478E-AF68-BA640E9CE65B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Digital Tools", "", "A6D78C4F-958F-4196-B8FF-527A10F5F047", "" );

            Sql( $@"
                DECLARE @EntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '{Rock.SystemGuid.EntityType.PAGE}')
                DECLARE @CMSSourcePageId INT = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B4A24AB7-9369-4055-883F-4F4892C39AE3')
                DECLARE @DigitalToolsPageId INT = (SELECT [Id] FROM [Page] WHERE [Guid] = 'A6D78C4F-958F-4196-B8FF-527A10F5F047')

                INSERT INTO [Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid], [PersonAliasId])
                SELECT 
	                  a1.[EntityTypeId]
	                , @DigitalToolsPageId
	                , a1.[Order]
	                , a1.[Action]
	                , a1.[AllowOrDeny]
	                , a1.[SpecialRole]
	                , a1.[GroupId]
	                , NEWID()
                    , a1.[PersonAliasId]
                FROM [Auth] a1
                WHERE a1.[EntityTypeId] = @EntityTypeId
	                AND a1.[EntityId] = @CMSSourcePageId
	                AND NOT EXISTS (
		                SELECT 1
		                FROM [Auth] a2
		                WHERE a2.[EntityTypeId] = a1.[EntityTypeId]
			                AND a2.[EntityId] = @DigitalToolsPageId
			                AND a2.[Order] = a1.[Order]
			                AND a2.[Action] = a1.[Action]
			                AND a2.[AllowOrDeny] = a1.[AllowOrDeny]
			                AND a2.[SpecialRole] = a1.[SpecialRole]
                            AND (a2.[GroupId] = a1.[GroupId] OR (a2.GroupId IS NULL AND a1.GroupId IS NULL))
                            AND (a2.[PersonAliasId] = a1.[PersonAliasId] OR (a2.[PersonAliasId] IS NULL AND a1.[PersonAliasId] IS NULL)))" );
        }

        private void UpdateAdminToolsPages()
        {
            Sql( $@"UPDATE [dbo].[Page]
SET [InternalName] = 'Administration',
[PageTitle] = 'Administration',
[BrowserTitle] = 'Administration'
WHERE [Page].[Guid] = '{AdministrationPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [InternalName] = 'Settings',
[PageTitle] = 'Settings',
[BrowserTitle] = 'Settings'
WHERE [Page].[Guid] = 'A7E36E7A-EFBD-4912-B46E-BB61A74B86FF'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [InternalName] = 'SQL Editor',
[PageTitle] = 'SQL Editor',
[BrowserTitle] = 'SQL Editor'
WHERE [Page].[Guid] = '{SQLPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [Order] = 1
WHERE [Guid] = '{ContentChannelsPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [InternalName] = 'Websites',
[PageTitle] = 'Websites',
[BrowserTitle] = 'Websites',
[Order] = 2
WHERE [Page].[Guid] = '{WebsitesPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [Order] = 3
WHERE [Guid] = '{MobileApplicationsPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [InternalName] = 'TV Apps',
[PageTitle] = 'TV Apps',
[BrowserTitle] = 'TV Apps',
[Order] = 4
WHERE [Page].[Guid] = '{TVAppsPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [Order] = 5
WHERE [Guid] = '{MediaAccountsPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [Order] = 6
WHERE [Guid] = '{ShortLinksPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [InternalName] = 'Adaptive Messages',
[PageTitle] = 'Adaptive Messages',
[BrowserTitle] = 'Adaptive Messages',
[Order] = 7
WHERE [Guid] = '{AdaptiveMessagePageGuid}'" );
        }

        private void MoveAdminToolsPages()
        {
            RockMigrationHelper.MovePage( SQLPageGuid, AdministrationPageGuid ); // SQL Editor
            RockMigrationHelper.MovePage( ContentChannelsPageGuid, DigitalToolsPageGuid ); // Content Channels
            RockMigrationHelper.MovePage( WebsitesPageGuid, DigitalToolsPageGuid ); // Websites
            RockMigrationHelper.MovePage( MobileApplicationsPageGuid, DigitalToolsPageGuid ); // Mobile Applications
            RockMigrationHelper.MovePage( TVAppsPageGuid, DigitalToolsPageGuid ); // TV Apps
            RockMigrationHelper.MovePage( MediaAccountsPageGuid, DigitalToolsPageGuid ); // Media Accounts
            RockMigrationHelper.MovePage( ShortLinksPageGuid, DigitalToolsPageGuid ); // Short Links
            RockMigrationHelper.MovePage( AdaptiveMessagePageGuid, DigitalToolsPageGuid ); // Adaptive Message
        }

        private void AddAdminToolsPageRoutes()
        {
            // First we're going to replace/hijack the old route name with the new route name for that particular existing route (926F6D75-1753-0ECD-868D-C7DE06C400DB)
            RockMigrationHelper.AddOrUpdatePageRoute( SQLPageGuid, "admin/sql", "926F6D75-1753-0ECD-868D-C7DE06C400DB" ); // SQL Editor New Route
            // Now we're going to add the old route name back in with a new route record (with new GUID 91A2528B-F4A6-47F0-82CA-A4A64895C8B0)
            RockMigrationHelper.AddOrUpdatePageRoute( SQLPageGuid, "admin/power-tools/sql", "91A2528B-F4A6-47F0-82CA-A4A64895C8B0" ); // SQL Editor Old Route
            // Continue following the pattern described in the comments above for the rest of the page routes.
            RockMigrationHelper.AddOrUpdatePageRoute( ContentChannelsPageGuid, "admin/content-channels", "EFAA0125-1F7F-A293-1EC2-CFA1EF5E1D34" ); // Content Channels New Route
            RockMigrationHelper.AddOrUpdatePageRoute( ContentChannelsPageGuid, "admin/cms/content-channels", "EC7A7EEE-D3C0-4A6E-B778-B4D0CD064795" ); // Content Channels Old Route
            RockMigrationHelper.AddOrUpdatePageRoute( WebsitesPageGuid, "admin/websites", "2EF88F58-0726-4B08-B40F-81D984642F00" ); // Websites New Route
            RockMigrationHelper.AddOrUpdatePageRoute( MobileApplicationsPageGuid, "admin/mobile-applications", "077F1E30-4803-79E6-0658-6A906C1143C5" ); // Mobile Applications New Route
            RockMigrationHelper.AddOrUpdatePageRoute( MobileApplicationsPageGuid, "admin/cms/mobile-applications", "473BF3DB-AEFA-4BE6-B083-FE708EF43349" ); // Mobile Applications Old Route

            Sql( $@"UPDATE [PageRoute]
SET [PageRoute].[Route] = 'admin/tv-apps'
WHERE [PageRoute].[Id] = (
SELECT TOP 1 [PageRoute].[Id]
FROM [PageRoute]
WHERE [PageRoute].[PageId] = (
SELECT [Page].[Id]
FROM [Page]
WHERE [Page].[Guid] = '{TVAppsPageGuid}')
AND [PageRoute].[Route] = 'admin/cms/appletv-applications'
AND [PageRoute].[Guid] != '4FED41DF-C9F8-411C-B281-EB6A8B794399'
ORDER BY [PageRoute].[Id] ASC)" );// TV Apps New Route

            RockMigrationHelper.AddOrUpdatePageRoute( TVAppsPageGuid, "admin/cms/appletv-applications", "4FED41DF-C9F8-411C-B281-EB6A8B794399" ); // TV Apps Old Route
            RockMigrationHelper.AddOrUpdatePageRoute( MediaAccountsPageGuid, "admin/media-accounts", "B12F5786-88A4-6D48-01EA-6E32514C220E" ); // Media Accounts New Route
            RockMigrationHelper.AddOrUpdatePageRoute( MediaAccountsPageGuid, "admin/cms/media-accounts", "9C8B1779-3A9D-4463-8928-BFF97EA355A4" ); // Media Accounts Old Route
            RockMigrationHelper.AddOrUpdatePageRoute( ShortLinksPageGuid, "admin/short-links", "87A329D4-6358-3BB2-0F05-51C814773DC7" ); // Short Links New Route
            RockMigrationHelper.AddOrUpdatePageRoute( ShortLinksPageGuid, "admin/cms/short-links", "671B7309-9725-4D02-937F-D9FE0BE5BD1D" ); // Short Links Old Route
            RockMigrationHelper.AddOrUpdatePageRoute( AdaptiveMessagePageGuid, "admin/adaptive-messages", "3B35F17E-B2DE-4512-8873-06A82F572ABD" ); // Adaptive Message New Route
            RockMigrationHelper.AddOrUpdatePageRoute( AdaptiveMessagePageGuid, "admin/cms/adaptive-messages", "620D5715-AB0F-4CA4-9956-EABF5CDFEFBF" ); // Adaptive Message Old Route
        }

        #endregion

        #endregion

        #region MigrationRollupsForV17_0_11

        #region LMS Update Program Completions Job

        private void UpdateProgramCompletionsJobUp()
        {
            AddOrUpdateUpdateProgramCompletionsJob();
            IconCssClassesUp();
            AddShowOnlyEnrolledCoursesBlockAttribute();
            UpdateSendLearningNotificationSystemCommunicationBody();
        }

        private void UpdateProgramCompletionsJobDown()
        {
            DeleteUpdateProgramCompletionsJob();
            IconCssClassesDown();
            RemoveShowOnlyEnrolledCoursesBlockAttribute();
        }

        /// <summary>
        ///  Deletes the UpdateProgramCompletions Job based on Guid and Class.
        /// </summary>
        private void DeleteUpdateProgramCompletionsJob()
        {
            var jobClass = "Rock.Jobs.UpdateProgramCompletions";
            Sql( $"DELETE [ServiceJob] WHERE [Class] = '{jobClass}' AND [Guid] = '{SystemGuid.ServiceJob.UPDATE_PROGRAM_COMPLETIONS}'" );
        }

        /// <summary>
        /// Adds or Updates the UpdateProgramCompletions Job.
        /// </summary>
        private void AddOrUpdateUpdateProgramCompletionsJob()
        {
            var cronSchedule = "0 0 5 1/1 * ? *"; // 5am daily.
            var jobClass = "Rock.Jobs.UpdateProgramCompletions";
            var name = "Update Program Completions";
            var description = "A job that updates learning program completion records for programs that track completion status.";

            Sql( $@"
            IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = '{jobClass}' AND [Guid] = '{SystemGuid.ServiceJob.UPDATE_PROGRAM_COMPLETIONS}' )
            BEGIN
                INSERT INTO [ServiceJob] (
                    [IsSystem],
                    [IsActive],
                    [Name],
                    [Description],
                    [Class],
                    [CronExpression],
                    [NotificationStatus],
                    [Guid] )
                VALUES (
                    0,
                    1,
                    '{name}',
                    '{description}',
                    '{jobClass}',
                    '{cronSchedule}',
                    1,
                    '{SystemGuid.ServiceJob.UPDATE_PROGRAM_COMPLETIONS}' );
            END
            ELSE
            BEGIN
	            UPDATE	[ServiceJob]
	            SET
		              [IsSystem] = 1
		            , [IsActive] = 1
		            , [Name] = '{name}'
		            , [Description] = '{description}'
		            , [Class] = '{jobClass}'
		            , [CronExpression] = '{cronSchedule}'
		            , [NotificationStatus] = 1
	            WHERE
		              [Guid] = '{SystemGuid.ServiceJob.UPDATE_PROGRAM_COMPLETIONS}';
            END" );
        }

        private void IconCssClassesUp()
        {
            var learningProgramEntityTypeGuid = "AFD89885-6923-4FA1-B6FE-A1DF8D821BBC";
            var allChurchCategoryGuid = "824B5DD9-47A7-4CE4-A461-F4FDEC8343F3";
            var internalStaffCategoryGuid = "87E1BEC7-171F-4E7E-8EC7-4D0102DCDE70";
            var volunteeringCategoryGuid = "A94C5563-647D-4509-B213-890B6D2A8530";
            RockMigrationHelper.UpdateCategory( learningProgramEntityTypeGuid, "All Church", "fa fa-church", "All Church", allChurchCategoryGuid, 0 );
            RockMigrationHelper.UpdateCategory( learningProgramEntityTypeGuid, "Internal Staff", "fa fa-id-badge", "Internal Staff", internalStaffCategoryGuid, 1 );
            RockMigrationHelper.UpdateCategory( learningProgramEntityTypeGuid, "Volunteering", "fa fa-people-carry", "Volunteering", volunteeringCategoryGuid, 2 );
        }

        private void IconCssClassesDown()
        {
            var learningProgramEntityTypeGuid = "AFD89885-6923-4FA1-B6FE-A1DF8D821BBC";
            var allChurchCategoryGuid = "824B5DD9-47A7-4CE4-A461-F4FDEC8343F3";
            var internalStaffCategoryGuid = "87E1BEC7-171F-4E7E-8EC7-4D0102DCDE70";
            var volunteeringCategoryGuid = "A94C5563-647D-4509-B213-890B6D2A8530";
            RockMigrationHelper.UpdateCategory( learningProgramEntityTypeGuid, "All Church", "", "All Church", allChurchCategoryGuid, 0 );
            RockMigrationHelper.UpdateCategory( learningProgramEntityTypeGuid, "Internal Staff", "", "Internal Staff", internalStaffCategoryGuid, 1 );
            RockMigrationHelper.UpdateCategory( learningProgramEntityTypeGuid, "Volunteering", "", "Volunteering", volunteeringCategoryGuid, 2 );
        }

        private void AddShowOnlyEnrolledCoursesBlockAttribute()
        {
            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Show Only Enrolled Courses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5D6BA94F-342A-4EC1-B024-FC5046FFE14D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Only Enrolled Courses", "Show Only Enrolled Courses", "Show Only Enrolled Courses", @"Filter to only those courses that the viewer is enrolled in.", 5, @"False", "7F5A60B0-1687-4527-8F4B-C1EE0917B0EC" );
        }

        private void RemoveShowOnlyEnrolledCoursesBlockAttribute()
        {
            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Show Only Enrolled Courses
            RockMigrationHelper.DeleteAttribute( "7F5A60B0-1687-4527-8F4B-C1EE0917B0EC" );
        }

        private void UpdateSendLearningNotificationSystemCommunicationBody()
        {
            Sql( @"
UPDATE s SET
	Body = '
{% assign currentDate = ''Now'' | Date:''MMMM d, yyyy'' %}
{{ ''Global'' | Attribute:''EmailHeader'' }}
<h1 style=""margin:0;"">
	Your Activities
</h1>

<p>
    Below are your newly available activities as of {{ currentDate }}.
</p>

{% for course in Courses %}
	<h2> {{course.ProgramName}}: {{course.CourseName}} - {{course.CourseCode}} </h2>
	
	<table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""table-layout: fixed;"">
		<tr>
			<th valign=""top"" style=""vertical-align:top;"" width=""50%"">
					Activity
			</th>
			<th valign=""top"" style=""vertical-align:top;"" width=""25%"">
					Available As Of
			</th>
			<th valign=""top"" style=""vertical-align:top;"" width=""25%"">
					Due
			</th>
		</tr>
		{% for activity in course.Activities %}
			<tr>
				<td>
					{{activity.ActivityName}}
				</td>
				<td>	
					{% if activity.AvailableDate == null %}
						Always
					{% else %}
						{{ activity.AvailableDate | HumanizeDateTime }}
					{% endif %}
				</td>
				<td>
					{% if activity.DueDate == null %}
						Optional
					{% else %}
						{{ activity.DueDate | HumanizeDateTime }}
					{% endif %}
				</td>		
			</tr>			
		{% endfor %}
	</table>
{% endfor %}

{{ ''Global'' | Attribute:''EmailFooter'' }}'
from systemcommunication s
WHERE [Guid] = 'D40A9C32-F179-4E5E-9B0D-CE208C5D1870'
" );
        }

        #endregion

        #endregion

        #region MigrationRollupsForV17_0_12

        #region JC: AI Providers

        private void AIProvidersUp()
        {
            AddAIProviderBlocksAndPages_Up();
            AddAIProviderDefaultInstance_Up();
        }

        private void AddAIProviderDefaultInstance_Up()
        {
            const string aiProviderEntityTypeGuid = "945A994F-F15E-43AC-B503-A54BDE70F77F";
            const string aiProviderComponentEntityTypeGuid = "8D3F25B1-4891-31AA-4FA6-365F5C808563";

            // Add/Update Entity Type: AIProvider.
            RockMigrationHelper.UpdateEntityType( "Rock.Model.AIProvider",
                "AI Provider",
                "Rock.Model.AIProvider, Rock, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null",
                isEntity: true,
                isSecured: false,
                aiProviderEntityTypeGuid );

            // Add/Update Entity Type: OpenAIProvider.
            RockMigrationHelper.UpdateEntityType( "Rock.AI.OpenAI.Provider.OpenAIProvider",
                "Open AI Provider",
                "Rock.AI.Provider.AIProviderComponent, Rock, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null",
                isEntity: true,
                isSecured: false,
                aiProviderComponentEntityTypeGuid );

            // Add AI Provider: OpenAI Provider.
            Sql( $@"
/*
    Create a new OpenAI AIProvider instance.
*/
IF NOT EXISTS (
    SELECT 1
    FROM [AIProvider]
    WHERE [Guid] = '2AA26B14-94CB-4A30-9E97-C7250BA464BB'
)
BEGIN
    DECLARE @componentEntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '8D3F25B1-4891-31AA-4FA6-365F5C808563');
    INSERT INTO [AIProvider] (
        [IsSystem]
        ,[IsActive]
        ,[Order]
        ,[Name]
        ,[Description]
        ,[ProviderComponentEntityTypeId]
        ,[Guid]
    ) VALUES (
        1
        ,1
        ,0
        ,'Open AI'
        ,'Provider to use the OpenAI API for use in Rock.'
        ,@componentEntityTypeId
        ,'2AA26B14-94CB-4A30-9E97-C7250BA464BB'
    );
END

/*
    Migrate OpenAI Component Settings to OpenAI Instance.
*/
-- Get Entity Type for AIProvider Component.
DECLARE @openAiProviderComponentEntityTypeId INT = (
        SELECT TOP 1 [Id]
        FROM [EntityType]
        WHERE [Guid] = '8d3f25b1-4891-31aa-4fa6-365f5c808563'
        )
-- Get Entity Type for AIProvider instances.
DECLARE @aiProviderEntityTypeId INT = (
        SELECT TOP 1 [Id]
        FROM [EntityType]
        WHERE [Guid] = '945A994F-F15E-43AC-B503-A54BDE70F77F'
        )

-- Get EntityId for OpenAIProvider instance.
DECLARE @openAiProviderId INT = (
        SELECT TOP 1 [Id]
        FROM [AIProvider]
        WHERE [Guid] = '2AA26B14-94CB-4A30-9E97-C7250BA464BB'
        )

-- Create the same value for the Attribute of the new OpenAIProvider instance.
DECLARE @textFieldTypeId INT = (
        SELECT TOP 1 [Id]
        FROM [FieldType]
        WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA'
        );
DECLARE @booleanFieldTypeId INT = (
        SELECT TOP 1 [Id]
        FROM [FieldType]
        WHERE [Guid] = '1EDAFDED-DFE6-4334-B019-6EECBA89E05A'
        );
DECLARE @counter INT = 0;
DECLARE @attributeKey NVARCHAR(1000);
DECLARE @attributeName NVARCHAR(1000);
DECLARE @attributeValue NVARCHAR(MAX);
DECLARE @attributeFieldTypeId INT;
DECLARE @providerWasMigrated BIT = 0;

WHILE ( @counter < 3 )
BEGIN
    SET @counter = @counter + 1;

    IF ( @counter = 1 )
    BEGIN
        SET @attributeFieldTypeId = @textFieldTypeId;
        SET @attributeKey = 'SecretKey';
        SET @attributeName = 'Secret Key';
    END
    ELSE IF ( @counter = 2 )
    BEGIN
        SET @attributeFieldTypeId = @textFieldTypeId;
        SET @attributeKey = 'Organization'
        SET @attributeName = 'Organization'
    END
    ELSE IF ( @counter = 3 )
    BEGIN
        SET @attributeFieldTypeId = @booleanFieldTypeId;
        SET @attributeKey = 'Active'
        SET @attributeName = 'Active'
    END

    -- Get the Attribute for the existing OpenAIProvider Component.
    DECLARE @componentAttributeId INT = (
            SELECT TOP 1 [Id]
            FROM [Attribute]
            WHERE [EntityTypeId] = @openAiProviderComponentEntityTypeId
                AND [Key] = @attributeKey
            )
    DECLARE @componentAttributeValueId INT = (
            SELECT TOP 1 [Id]
            FROM [AttributeValue]
            WHERE [AttributeId] = @componentAttributeId
            );

    IF (@componentAttributeId IS NOT NULL AND @componentAttributeValueId IS NOT NULL)
    BEGIN
		-- Flag that we migrated at least 1 attribute.
		SET @providerWasMigrated = 1;

        -- Get the current value of this Attribute for the existing OpenAIProvider component.
        DECLARE @value NVARCHAR(MAX) = (
                SELECT TOP 1 [Value]
                FROM [AttributeValue]
                WHERE [AttributeId] = @componentAttributeId
                    AND [EntityId] = 0
                )

        -- Get the Attribute for the new OpenAIProvider instance.
        DECLARE @instanceAttributeId INT = (
                SELECT TOP 1 [Id]
                FROM [Attribute]
                WHERE [EntityTypeId] = @aiProviderEntityTypeId
                    AND [EntityTypeQualifierColumn] = 'ProviderComponentEntityTypeId'
                    AND [EntityTypeQualifierValue] = @openAiProviderComponentEntityTypeId
                    AND [Key] = @attributeKey
                )

        IF (@instanceAttributeId IS NULL)
        BEGIN
            DECLARE @newAttributeGuid UNIQUEIDENTIFIER = NEWID();

            INSERT INTO [Attribute] (
                [Guid],
                [Key],
                [Name],
                [EntityTypeId],
                [EntityTypeQualifierColumn],
                [EntityTypeQualifierValue],
                [IsSystem],
                [FieldTypeId],
                [Order],
                [IsGridColumn],
                [IsMultiValue],
                [IsRequired]
                )
            VALUES (
                @newAttributeGuid,
                @attributeKey,
                @attributeName,
                @aiProviderEntityTypeId,
                'ProviderComponentEntityTypeId',
                @openAiProviderComponentEntityTypeId,
                0,
                @attributeFieldTypeId,
                0,
                0,
                0,
                1
                )

            SET @instanceAttributeId = (
                    SELECT [Id]
                    FROM [Attribute]
                    WHERE [Guid] = @newAttributeGuid
                    );
        END

        SET @attributeValue = ( SELECT TOP 1 [Value] FROM [AttributeValue] WHERE [Id] = @componentAttributeValueId );

        DECLARE @instanceAttributeValueId int = ( SELECT TOP 1 [Id] FROM [AttributeValue] WHERE [AttributeId] = @instanceAttributeId );
        IF ( @instanceAttributeValueId IS NULL)
            BEGIN
                INSERT INTO [AttributeValue]
                ( IsSystem, AttributeId, EntityId, [Value], [Guid] )
                VALUES( 0, @instanceAttributeId, @openAiProviderId, @attributeValue, NEWID() )
            END
        ELSE 
            BEGIN
                UPDATE [AttributeValue]
                SET [Value] = @attributeValue
                WHERE [Id] = @instanceAttributeValueId
            END
    END
END

IF @providerWasMigrated = 1
	BEGIN
			
		-- Get the Attribute for the new OpenAIProvider instance.
		DECLARE @defaultModelAttributeId INT = ( SELECT TOP 1 [Id]
			FROM [Attribute]
			WHERE [EntityTypeId] = @aiProviderEntityTypeId
				AND [EntityTypeQualifierColumn] = 'ProviderComponentEntityTypeId'
				AND [EntityTypeQualifierValue] = @openAiProviderComponentEntityTypeId
				AND [Key] = 'DefaultModel'
			)
                
		IF ( @defaultModelAttributeId IS NULL )
		BEGIN
			INSERT INTO [Attribute] (
				[Guid],
				[Key],
				[Name],
				[EntityTypeId],
				[EntityTypeQualifierColumn],
				[EntityTypeQualifierValue],
				[IsSystem],
				[FieldTypeId],
				[Order],
				[IsGridColumn],
				[IsMultiValue],
				[IsRequired]
				)
			VALUES (
				NEWID(),
				'DefaultModel',
				'Default Model',
				@aiProviderEntityTypeId,
				'ProviderComponentEntityTypeId',
				@openAiProviderComponentEntityTypeId,
				0,
				@textFieldTypeId,
				0,
				0,
				0,
				1
				)

			SET @defaultModelAttributeId = SCOPE_IDENTITY();
		END
		
		INSERT INTO [AttributeValue]
			( IsSystem, AttributeId, EntityId, [Value], [Guid] )
			SELECT 0, @defaultModelAttributeId, @openAiProviderId, 'gpt-4o-mini', NEWID()
			WHERE NOT EXISTS (
				SELECT *
				FROM [AttributeValue] ex
				WHERE ex.AttributeId = @defaultModelAttributeId
					AND ex.EntityId = @openAiProviderId
			)
END
" );
        }

        private void AddAIProviderBlocksAndPages_Up()
        {
            // Add Page 
            //  Internal Name: AI Provider Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "54E421B1-B89C-4C3B-BECA-16349D750691", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "AI Provider Detail", "", "64D722C2-F9F5-4DF5-947E-33862B93EECA", "" );

            // Add/Update BlockType 
            //   Name: AI Provider List
            //   Category: Core
            //   Path: ~/Blocks/AI/AIProviderList.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType( "AI Provider List", "Block for viewing the list of AI providers.", "~/Blocks/AI/AIProviderList.ascx", "Core", "B3F280BD-13F4-4195-A68A-AC4A64F574A5" );

            // Add/Update BlockType 
            //   Name: AI Provider Detail
            //   Category: Core
            //   Path: ~/Blocks/AI/AiProviderDetail.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType( "AI Provider Detail", "Displays the details of an AI Provider.", "~/Blocks/AI/AiProviderDetail.ascx", "Core", "88820905-1B5A-4B82-8E56-F9A0736A0E98" );

            // Attribute for  BlockType
            //   BlockType: AI Provider List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B3F280BD-13F4-4195-A68A-AC4A64F574A5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"64D722C2-F9F5-4DF5-947E-33862B93EECA", "881771AA-00A3-4E7E-8B9D-F4E4EE434836" );

            // Add Block 
            //  Block Name: AI Provider List
            //  Page Name: AI Providers
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "54E421B1-B89C-4C3B-BECA-16349D750691".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "B3F280BD-13F4-4195-A68A-AC4A64F574A5".AsGuid(), "AI Provider List", "Main", @"", @"", 1, "EE972857-A6A9-435F-BCB3-159FFE72D892" );

            // Add Block Attribute Value
            //   Block: AI Provider List
            //   BlockType: AI Provider List
            //   Category: Core
            //   Block Location: Page=AI Providers, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 64d722c2-f9f5-4df5-947e-33862b93eeca */
            RockMigrationHelper.AddBlockAttributeValue( "EE972857-A6A9-435F-BCB3-159FFE72D892", "881771AA-00A3-4E7E-8B9D-F4E4EE434836", @"64d722c2-f9f5-4df5-947e-33862b93eeca" );

            // Add Block 
            //  Block Name: AI Provider Detail
            //  Page Name: AI Provider Detail
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "64D722C2-F9F5-4DF5-947E-33862B93EECA".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "88820905-1B5A-4B82-8E56-F9A0736A0E98".AsGuid(), "AI Provider Detail", "Main", @"", @"", 0, "C681F836-8FB1-490E-AB78-2E4273E5E98B" );

            // Remove the existing Component List block for AI components.
            RockMigrationHelper.DeleteBlock( "F62F88D5-9E56-4DA0-8990-D2B6BBA0D28E" );
        }

        #endregion

        #region KA: Migration to update Prayer Request metric SourceSql

        private const string UniqueScheduleGuid = "091E1F41-104A-4856-B7CD-F6506DD59AF7";
        private const string Schedule = @"
BEGIN:VCALENDAR
PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 4.0//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:20240724T090001
DTSTAMP:20240724T101307
DTSTART:20240724T210000
RRULE:FREQ=WEEKLY;BYDAY=FR
SEQUENCE:0
UID:ae4e25e4-bb92-4626-9729-513f4635745f
END:VEVENT
END:VCALENDAR";

        private void UpdateERAMetricScheduleUp()
        {
            // Create new unique weekly schedule at 9pm.
            Sql( $@"
DECLARE @ScheduleId int = ( 
    SELECT [Id] 
    FROM dbo.[Schedule] 
    WHERE Guid = '{UniqueScheduleGuid}'),

@MetricsCategoryId int = ( 
    SELECT [Id] 
    FROM dbo.[Category] 
    WHERE Guid = '5A794741-5444-43F0-90D7-48E47276D426'
)
IF @ScheduleId IS NULL
BEGIN
	INSERT [dbo].[Schedule] ([Name], [Description], [iCalendarContent], [CategoryId], [Guid], [IsActive], [IsPublic])
	VALUES ('', NULL, '{Schedule}',@MetricsCategoryId, '{UniqueScheduleGuid}', 1, 0)
END" );

            // Update eRA metrics to use 9pm schedule so it runs after FamilyAnalytics job which runs at 8pm.
            Sql( $@"
DECLARE @ScheduleId int = ( 
    SELECT [Id] 
    FROM dbo.[Schedule] 
    WHERE Guid = '{UniqueScheduleGuid}')

IF @ScheduleId IS NOT NULL
UPDATE dbo.[Metric]
SET [ScheduleId] = @ScheduleId
WHERE [Guid] IN ('D05D685A-9A88-4375-A563-70BB44FBD237','16A3FF64-31F0-4CFF-B5F4-83EEB69E0C25')
" );

            // Update Prayers metric to use prayer request campus instead of requester's campusId.
            Sql( @"
UPDATE [dbo].[Metric]
SET [SourceSql] = 'DECLARE @STARTDATE DATETIME = DATEADD(DAY, -7, GETDATE())
DECLARE @ENDDATE DATETIME = GETDATE()
SELECT COUNT(1) as PrayerRequests, pr.[CampusId]
FROM dbo.[PrayerRequest] pr
WHERE
   pr.[IsActive] = 1
   AND pr.[CreatedDateTime] BETWEEN @STARTDATE AND @ENDDATE
GROUP BY ALL pr.[CampusId]'
WHERE [Guid] = '2B5ECA35-47D8-4690-A8AD-72488485F2B4'
" );
        }

        #endregion

        #region KA : Migration to Update GivingAnalytics and FamilyAnalytics Attendance Procedures

        private void UpdateGivingAnalyticsAndFamilyAnalyticsAttendanceProceduresUp()
        {
            Sql( @"
ALTER PROCEDURE [dbo].[spCrm_FamilyAnalyticsAttendance]

AS
BEGIN
	
	-- configuration of the duration in weeks
	DECLARE @EntryAttendanceDurationWeeks int = 16
		
	DECLARE @cACTIVE_RECORD_STATUS_VALUE_GUID uniqueidentifier = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
	DECLARE @cPERSON_RECORD_TYPE_VALUE_GUID uniqueidentifier = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
	DECLARE @cATTRIBUTE_IS_ERA_GUID uniqueidentifier = 'CE5739C5-2156-E2AB-48E5-1337C38B935E'
	DECLARE @cFAMILY_GROUPTYPE_GUID uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'
	DECLARE @cADULT_ROLE_GUID uniqueidentifier = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42'
	DECLARE @cCHILD_ROLE_GUID uniqueidentifier = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9'

	DECLARE @cATTRIBUTE_FIRST_ATTENDED uniqueidentifier  = 'AB12B3B0-55B8-D6A5-4C1F-DB9CCB2C4342'
	DECLARE @cATTRIBUTE_LAST_ATTENDED uniqueidentifier  = '5F4C6462-018E-D19C-4AB0-9843CB21C57E'
	DECLARE @cATTRIBUTE_TIMES_ATTENDED_IN_DURATION uniqueidentifier  = '45A1E978-DC5B-CFA1-4AF4-EA098A24C914'
	DECLARE @Now DATETIME = dbo.RockGetDate()

	-- --------- END CONFIGURATION --------------

	DECLARE @ActiveRecordStatusValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cACTIVE_RECORD_STATUS_VALUE_GUID)
	DECLARE @PersonRecordTypeValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cPERSON_RECORD_TYPE_VALUE_GUID)
	DECLARE @IsEraAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_IS_ERA_GUID)
	DECLARE @FamilyGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = @cFAMILY_GROUPTYPE_GUID)
	DECLARE @AdultRoleId int = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = @cADULT_ROLE_GUID)
	DECLARE @ChildRoleId int = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = @cCHILD_ROLE_GUID)

	-- calculate dates for query
	DECLARE @SundayDateStart datetime = [dbo].[ufnUtility_GetPreviousSundayDate]()
	DECLARE @SundayEntryAttendanceDuration datetime = DATEADD(DAY,  (7 * @EntryAttendanceDurationWeeks * -1), @SundayDateStart)
	

	-- first checkin
	DECLARE @FirstAttendedAttributeId int = (SELECT TOP 1 [Id] FROM dbo.[Attribute] WHERE [Guid] = @cATTRIBUTE_FIRST_ATTENDED);

	WITH
	  cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	  AS
	  (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	  )
	SELECT [PersonId], [FirstAttendedDate], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
	INTO #firstAttended
	FROM 
		(SELECT 
			i.[PersonId]
			, CASE WHEN [FamilyRole] = 'Adult' THEN 
					(SELECT 
						MIN(A.[StartDateTime] )
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](i.[PersonId])))
				ELSE
					(SELECT 
						MIN(a.[StartDateTime] )
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] = i.[PersonId])
			  END AS [FirstAttendedDate]
		FROM cteIndividual i ) AS a
	LEFT JOIN [AttributeValue] av ON av.[EntityId] = a.[PersonId]
	AND av.[AttributeId] = @FirstAttendedAttributeId
	WHERE a.[FirstAttendedDate] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[FirstAttendedDate],
		av.[ValueAsDateTime] = f.[FirstAttendedDate],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #firstAttended f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[FirstAttendedDate]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsDateTime]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @FirstAttendedAttributeId
		, f.[FirstAttendedDate]
		, 0
		, newid()
		, @Now
		, f.[FirstAttendedDate]
		, 1
	FROM #firstAttended f
	WHERE f.[AttributeValueId] IS NULL

	-- Remove the temp table.
	DROP TABLE #firstAttended

	-- last checkin
	DECLARE @LastAttendedAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_LAST_ATTENDED);

	WITH
	  cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	  AS
	  (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	  )
	SELECT [PersonId], [LastAttendedDate], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
	INTO #lastAttended
	FROM 
		(SELECT 
			i.[PersonId]
			, CASE WHEN [FamilyRole] = 'Adult' THEN 
					(SELECT 
						MAX(a.StartDateTime )
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](i.[PersonId])))
				ELSE
					(SELECT 
						MAX(a.StartDateTime )
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] = i.[PersonId])
			  END AS [LastAttendedDate]
		FROM cteIndividual i ) AS a
	LEFT JOIN dbo.[AttributeValue] av ON av.[EntityId] = a.[PersonId] 
	AND av.[AttributeId] = @LastAttendedAttributeId
	WHERE a.[LastAttendedDate] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[LastAttendedDate],
		av.[ValueAsDateTime] = f.[LastAttendedDate],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #lastAttended f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[LastAttendedDate]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsDateTime]
        , [IsPersistedValueDirty])
	SELECT f.PersonId
		, @LastAttendedAttributeId
		, f.[LastAttendedDate]
		, 0
		, newid()
		, @Now
		, f.[LastAttendedDate]
		, 1
	FROM #lastAttended f
	WHERE f.[AttributeValueId] IS NULL

	-- Remove the temp table.
	DROP TABLE #lastAttended

	-- times checkedin
	DECLARE @TimesAttendedAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_TIMES_ATTENDED_IN_DURATION)
	DELETE FROM [AttributeValue] WHERE [AttributeId] = @TimesAttendedAttributeId;

	WITH
	  cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	  AS
	  (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	  )
	SELECT [PersonId], [CheckinCount], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
	INTO #checkInCount
	FROM 
		(SELECT 
			i.[PersonId]
			, CASE WHEN [FamilyRole] = 'Adult' THEN 
					(SELECT 
						COUNT(*)
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
						AND CAST( a.[StartDateTime] AS DATE ) <= @SundayDateStart AND a.[StartDateTime] >= @SundayEntryAttendanceDuration
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](i.[PersonId])))
				ELSE
					(SELECT 
						COUNT(*)
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
						AND CAST( a.[StartDateTime] AS DATE ) <= @SundayDateStart AND a.[StartDateTime] >= @SundayEntryAttendanceDuration
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] = i.[PersonId])
			  END AS [CheckinCount]
		FROM cteIndividual i ) AS a
	LEFT JOIN dbo.[AttributeValue] av ON av.[EntityId] = a.[PersonId]
	AND av.[AttributeId] = @TimesAttendedAttributeId
	WHERE a.[CheckinCount] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[CheckinCount],
		av.[ValueAsNumeric] = f.[CheckinCount],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #checkInCount f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[CheckinCount]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsNumeric]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @TimesAttendedAttributeId
		, f.[CheckinCount]
		, 0
		, newid()
		, @Now
		, f.[CheckinCount]
		, 1
	FROM #checkInCount f
	WHERE f.[AttributeValueId] IS NULL

	-- Remove the temp table.
	DROP TABLE #checkInCount
	
END
" );

            Sql( @"
ALTER PROCEDURE [dbo].[spCrm_FamilyAnalyticsGiving]
	
AS
BEGIN
	
	-- configuration of the duration in weeks
	DECLARE @GivingDurationLongWeeks int = 52
	DECLARE @GivingDurationShortWeeks int = 6
	
	DECLARE @cACTIVE_RECORD_STATUS_VALUE_GUID uniqueidentifier = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
	DECLARE @cPERSON_RECORD_TYPE_VALUE_GUID uniqueidentifier = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
	DECLARE @cFAMILY_GROUPTYPE_GUID uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'
	DECLARE @cADULT_ROLE_GUID uniqueidentifier = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42'	
	DECLARE @cCONTRIBUTION_TYPE_VALUE_GUID uniqueidentifier = '2D607262-52D6-4724-910D-5C6E8FB89ACC'
	DECLARE @cATTRIBUTE_FIRST_GAVE uniqueidentifier  = 'EE5EC76A-D4B9-56B5-4B48-29627D945F10'
	DECLARE @cATTRIBUTE_LAST_GAVE uniqueidentifier  = '02F64263-E290-399E-4487-FC236F4DE81F'
	DECLARE @cATTRIBUTE_GIFT_COUNT_SHORT uniqueidentifier  = 'AC11EF53-AE55-79A0-4CAD-43721750E988'
	DECLARE @cATTRIBUTE_GIFT_COUNT_LONG uniqueidentifier  = '57700E8F-ED11-D787-415A-04DDF411BB10'
	-- --------- END CONFIGURATION --------------
	DECLARE @ActiveRecordStatusValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cACTIVE_RECORD_STATUS_VALUE_GUID)
	DECLARE @PersonRecordTypeValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cPERSON_RECORD_TYPE_VALUE_GUID)
	DECLARE @FamilyGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = @cFAMILY_GROUPTYPE_GUID)
	DECLARE @AdultRoleId int = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = @cADULT_ROLE_GUID)
	DECLARE @ContributionTypeId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cCONTRIBUTION_TYPE_VALUE_GUID)
	
	-- calculate dates for queries
	DECLARE @SundayDateStart datetime = [dbo].[ufnUtility_GetPreviousSundayDate]()
	DECLARE @SundayGivingDurationLong datetime = DATEADD(DAY,  (7 * @GivingDurationLongWeeks * -1), @SundayDateStart)
	DECLARE @SundayGivingDurationShort datetime = DATEADD(DAY,  (7 * @GivingDurationShortWeeks * -1), @SundayDateStart);
	DECLARE @Now DATETIME = dbo.RockGetDate()

	-- first gift (people w/Giving Group)
	DECLARE @FirstGaveAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_FIRST_GAVE);
	WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NOT NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
		SELECT [PersonId], [FirstContributionDate], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #firstGiftWithGroup
		FROM 
			(SELECT 
	    		[PersonId]
	    		, (SELECT MIN(ft.TransactionDateTime)
	    					FROM [FinancialTransaction] ft
	    						INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
	    						INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
	    						INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
	    						INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
	    					WHERE 
	    						gp.[GivingGroupId] = i.[GivingGroupId]
	    						AND ft.TransactionTypeValueId = @ContributionTypeId) AS [FirstContributionDate]
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @FirstGaveAttributeId
	    WHERE g.[FirstContributionDate] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[FirstContributionDate],
		av.[ValueAsDateTime] = f.[FirstContributionDate],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #firstGiftWithGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[FirstContributionDate]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsDateTime]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @FirstGaveAttributeId
		, f.[FirstContributionDate]
		, 0
		, newid()
		, @Now
		, f.[FirstContributionDate]
		, 1
	FROM #firstGiftWithGroup f
	WHERE f.[AttributeValueId] IS NULL

	-- Remove the temp table.
	DROP TABLE #firstGiftWithGroup

	-- first gift (people WITHOUT Giving Group)
	;WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )

		SELECT [PersonId], [FirstContributionDate], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #firstGiftWithoutGroup
		FROM 
	    	(SELECT 
	    		[PersonId]
	    		, (SELECT MIN(ft.TransactionDateTime)
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[Id] = i.[PersonId] -- match by person id
							AND ft.TransactionTypeValueId = @ContributionTypeId) AS [FirstContributionDate]
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @FirstGaveAttributeId
	    WHERE g.[FirstContributionDate] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[FirstContributionDate],
		av.[ValueAsDateTime] = f.[FirstContributionDate],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #firstGiftWithoutGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[FirstContributionDate]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsDateTime]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @FirstGaveAttributeId
		, f.[FirstContributionDate]
		, 0
		, newid()
		, @Now
		, f.[FirstContributionDate]
		, 1
	FROM #firstGiftWithoutGroup f
	WHERE f.[AttributeValueId] IS NULL

	-- Remove the temp table.
	DROP TABLE #firstGiftWithoutGroup
	
	-- last gift (people w/Giving Group)
	DECLARE @LastGaveAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_LAST_GAVE);
	WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NOT NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )

		SELECT [PersonId], [LastContributionDate], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #lastGiftWithGroup
		FROM 
	    	(SELECT 
	    		[PersonId]
	    		, (SELECT MAX(ft.TransactionDateTime)
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[GivingGroupId] = i.[GivingGroupId]
							AND ft.TransactionTypeValueId = @ContributionTypeId) AS [LastContributionDate]
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @LastGaveAttributeId
	    WHERE g.[LastContributionDate] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[LastContributionDate],
		av.[ValueAsDateTime] = f.[LastContributionDate],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #lastGiftWithGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[LastContributionDate]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsDateTime]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @LastGaveAttributeId
		, f.[LastContributionDate]
		, 0
		, newid()
		, @Now
		, f.[LastContributionDate]
		, 1
	FROM #lastGiftWithGroup f
	WHERE f.[AttributeValueId] IS NULL

	-- Remove the temp table.
	DROP TABLE #lastGiftWithGroup

	-- last gift (people WITHOUT Giving Group)
	;WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] -- match by person id
				AND p.[GivingGroupId] IS NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )

		SELECT [PersonId], [LastContributionDate], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #lastGiftWithoutGroup
		FROM 
	    	(SELECT 
	    		[PersonId]
	    		, (SELECT MAX(ft.TransactionDateTime)
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[Id] = i.[PersonId]
							AND ft.TransactionTypeValueId = @ContributionTypeId) AS [LastContributionDate]
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @LastGaveAttributeId
	    WHERE g.[LastContributionDate] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[LastContributionDate],
		av.[ValueAsDateTime] = f.[LastContributionDate],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #lastGiftWithoutGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[LastContributionDate]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsDateTime]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @LastGaveAttributeId
		, f.[LastContributionDate]
		, 0
		, newid()
		, @Now
		, f.[LastContributionDate]
		, 1
	FROM #lastGiftWithoutGroup f
	WHERE f.[AttributeValueId] IS NULL

	-- Remove the temp table.
	DROP TABLE #lastGiftWithoutGroup

	-- number of gifts short duration (people w/Giving Group)
	DECLARE @GiftCountShortAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_GIFT_COUNT_SHORT);
	WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NOT NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )

		SELECT [PersonId], [GiftCountDurationShort], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #giftCountShortWithGroup
		FROM 
	    	(SELECT 
	    		[PersonId]
	    		, (SELECT COUNT(DISTINCT(ft.[Id])) 
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[GivingGroupId] = i.[GivingGroupId]
							AND ft.TransactionTypeValueId = @ContributionTypeId
							AND ft.TransactionDateTime >= @SundayGivingDurationShort
							AND ft.SundayDate <= @SundayDateStart) AS [GiftCountDurationShort]
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @GiftCountShortAttributeId
	    WHERE g.[GiftCountDurationShort] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[GiftCountDurationShort],
		av.[ValueAsNumeric] = f.[GiftCountDurationShort],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #giftCountShortWithGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[GiftCountDurationShort]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsNumeric]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @GiftCountShortAttributeId
		, f.[GiftCountDurationShort]
		, 0
		, newid()
		, @Now
		, f.[GiftCountDurationShort]
		, 1
	FROM #giftCountShortWithGroup f
	WHERE f.[AttributeValueId] IS NULL

    -- Remove the temp table.
	DROP TABLE #giftCountShortWithGroup

	-- number of gifts short duration (people WITHOUT Giving Group)
	;WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )

		SELECT [PersonId], [GiftCountDurationShort], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #giftCountShortWithoutGroup
		FROM 
	    	(SELECT 
	    		[PersonId]
	    		, (SELECT COUNT(DISTINCT(ft.[Id])) 
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[Id] = i.[PersonId] -- match by person id
							AND ft.TransactionTypeValueId = @ContributionTypeId
							AND ft.TransactionDateTime >= @SundayGivingDurationShort
							AND ft.SundayDate <= @SundayDateStart) AS [GiftCountDurationShort]
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @GiftCountShortAttributeId
	    WHERE g.[GiftCountDurationShort] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[GiftCountDurationShort],
		av.[ValueAsNumeric] = f.[GiftCountDurationShort],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #giftCountShortWithoutGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[GiftCountDurationShort]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsNumeric]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @GiftCountShortAttributeId
		, f.[GiftCountDurationShort]
		, 0
		, newid()
		, @Now
		, f.[GiftCountDurationShort]
		, 1
	FROM #giftCountShortWithoutGroup f
	WHERE f.[AttributeValueId] IS NULL

    -- Remove the temp table.
	DROP TABLE #giftCountShortWithoutGroup

	-- number of gifts long duration (people w/Giving Group)
	DECLARE @GiftCountLongAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_GIFT_COUNT_LONG);
	WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NOT NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )

		SELECT [PersonId], [GiftCountDurationLong], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #giftCountLongWithGroup
		FROM 
	    	(SELECT 
	    		[PersonId]
	    		, (SELECT COUNT(DISTINCT(ft.[Id])) 
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[GivingGroupId] = i.[GivingGroupId]
							AND ft.TransactionTypeValueId = @ContributionTypeId
							AND ft.TransactionDateTime >= @SundayGivingDurationLong
							AND ft.SundayDate <= @SundayDateStart) AS [GiftCountDurationLong]
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @GiftCountLongAttributeId
	    WHERE g.[GiftCountDurationLong] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[GiftCountDurationLong],
		av.[ValueAsNumeric] = f.[GiftCountDurationLong],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #giftCountLongWithGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[GiftCountDurationLong]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsNumeric]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @GiftCountLongAttributeId
		, f.[GiftCountDurationLong]
		, 0
		, newid()
		, @Now
		, f.[GiftCountDurationLong]
		, 1
	FROM #giftCountLongWithGroup f
	WHERE f.[AttributeValueId] IS NULL

    -- Remove the temp table.
	DROP TABLE #giftCountLongWithGroup
	
	-- number of gifts long duration (people WITHOUT Giving Group)
	;WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )

		SELECT [PersonId], [GiftCountDurationLong], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #giftCountLongWithoutGroup
		FROM 
	    	(SELECT 
	    		[PersonId]
	    		, (SELECT COUNT(DISTINCT(ft.[Id])) 
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[Id] = i.[PersonId] -- match by person id
							AND ft.TransactionTypeValueId = @ContributionTypeId
							AND ft.TransactionDateTime >= @SundayGivingDurationLong
							AND ft.SundayDate <= @SundayDateStart) AS [GiftCountDurationLong]
	    		, 0 AS [IsSystem]
	    		, newid() AS [Guid]
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @GiftCountLongAttributeId
	    WHERE g.[GiftCountDurationLong] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[GiftCountDurationLong],
		av.[ValueAsNumeric] = f.[GiftCountDurationLong],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #giftCountLongWithoutGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[GiftCountDurationLong]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsNumeric]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @GiftCountLongAttributeId
		, f.[GiftCountDurationLong]
		, 0
		, newid()
		, @Now
		, f.[GiftCountDurationLong]
		, 1
	FROM #giftCountLongWithoutGroup f
	WHERE f.[AttributeValueId] IS NULL

    -- Remove the temp table.
	DROP TABLE #giftCountLongWithoutGroup
END
" );
        }

        private void UpdateGivingAnalyticsAndFamilyAnalyticsAttendanceProceduresDown()
        {
            Sql( @"
ALTER PROCEDURE [dbo].[spCrm_FamilyAnalyticsAttendance]

AS
BEGIN
	
	-- configuration of the duration in weeks
	DECLARE @EntryAttendanceDurationWeeks int = 16
		
	DECLARE @cACTIVE_RECORD_STATUS_VALUE_GUID uniqueidentifier = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
	DECLARE @cPERSON_RECORD_TYPE_VALUE_GUID uniqueidentifier = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
	DECLARE @cATTRIBUTE_IS_ERA_GUID uniqueidentifier = 'CE5739C5-2156-E2AB-48E5-1337C38B935E'
	DECLARE @cFAMILY_GROUPTYPE_GUID uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'
	DECLARE @cADULT_ROLE_GUID uniqueidentifier = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42'
	DECLARE @cCHILD_ROLE_GUID uniqueidentifier = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9'

	DECLARE @cATTRIBUTE_FIRST_ATTENDED uniqueidentifier  = 'AB12B3B0-55B8-D6A5-4C1F-DB9CCB2C4342'
	DECLARE @cATTRIBUTE_LAST_ATTENDED uniqueidentifier  = '5F4C6462-018E-D19C-4AB0-9843CB21C57E'
	DECLARE @cATTRIBUTE_TIMES_ATTENDED_IN_DURATION uniqueidentifier  = '45A1E978-DC5B-CFA1-4AF4-EA098A24C914'

	-- --------- END CONFIGURATION --------------

	DECLARE @ActiveRecordStatusValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cACTIVE_RECORD_STATUS_VALUE_GUID)
	DECLARE @PersonRecordTypeValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cPERSON_RECORD_TYPE_VALUE_GUID)
	DECLARE @IsEraAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_IS_ERA_GUID)
	DECLARE @FamilyGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = @cFAMILY_GROUPTYPE_GUID)
	DECLARE @AdultRoleId int = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = @cADULT_ROLE_GUID)
	DECLARE @ChildRoleId int = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = @cCHILD_ROLE_GUID)

	-- calculate dates for query
	DECLARE @SundayDateStart datetime = [dbo].[ufnUtility_GetPreviousSundayDate]()
	DECLARE @SundayEntryAttendanceDuration datetime = DATEADD(DAY,  (7 * @EntryAttendanceDurationWeeks * -1), @SundayDateStart)
	


	-- first checkin
	DECLARE @FirstAttendedAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_FIRST_ATTENDED)
	DELETE FROM [AttributeValue] WHERE [AttributeId] = @FirstAttendedAttributeId;

	WITH
	  cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	  AS
	  (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	  )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsDateTime], [IsPersistedValueDirty])
	SELECT [PersonId], [AttributeId], [FirstAttendedDate], [IsSystem], [Guid], [CreateDate], [FirstAttendedDate], 1  FROM 
		(SELECT 
			i.[PersonId]
			, @FirstAttendedAttributeId AS [AttributeId]
			, CASE WHEN [FamilyRole] = 'Adult' THEN 
					(SELECT 
						MIN(A.[StartDateTime] )
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](i.[PersonId])))
				ELSE
					(SELECT 
						MIN(a.StartDateTime )
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] = i.[PersonId])
			  END AS [FirstAttendedDate]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i ) AS a
	WHERE a.[FirstAttendedDate] IS NOT NULL

	-- last checkin
	DECLARE @LastAttendedAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_LAST_ATTENDED)
	DELETE FROM [AttributeValue] WHERE [AttributeId] = @LastAttendedAttributeId;

	WITH
	  cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	  AS
	  (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	  )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsDateTime], [IsPersistedValueDirty])
	SELECT [PersonId], [AttributeId], [LastAttendedDate], [IsSystem], [Guid], [CreateDate], [LastAttendedDate], 1  FROM  
		(SELECT 
			i.[PersonId]
			, @LastAttendedAttributeId AS [AttributeId]
			, CASE WHEN [FamilyRole] = 'Adult' THEN 
					(SELECT 
						MAX(a.StartDateTime )
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](i.[PersonId])))
				ELSE
					(SELECT 
						MAX(a.StartDateTime )
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] = i.[PersonId])
			  END AS [LastAttendedDate]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i ) AS a
	WHERE a.[LastAttendedDate] IS NOT NULL

	-- times checkedin
	DECLARE @TimesAttendedAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_TIMES_ATTENDED_IN_DURATION)
	DELETE FROM [AttributeValue] WHERE [AttributeId] = @TimesAttendedAttributeId;

	WITH
	  cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	  AS
	  (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	  )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsNumeric], [IsPersistedValueDirty])
	SELECT [PersonId], [AttributeId], [CheckinCount], [IsSystem], [Guid], [CreateDate], [CheckinCount], 1
    FROM 
		(SELECT 
			i.[PersonId]
			, @TimesAttendedAttributeId AS [AttributeId]
			, CASE WHEN [FamilyRole] = 'Adult' THEN 
					(SELECT 
						COUNT(*)
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
						AND CAST( a.[StartDateTime] AS DATE ) <= @SundayDateStart AND a.[StartDateTime] >= @SundayEntryAttendanceDuration
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](i.[PersonId])))
				ELSE
					(SELECT 
						COUNT(*)
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
						AND CAST( a.[StartDateTime] AS DATE ) <= @SundayDateStart AND a.[StartDateTime] >= @SundayEntryAttendanceDuration
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] = i.[PersonId])
			  END AS [CheckinCount]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i ) AS a
	WHERE a.[CheckinCount] IS NOT NULL

	
END
" );

            Sql( @"
ALTER PROCEDURE [dbo].[spCrm_FamilyAnalyticsGiving]
	
AS
BEGIN
	
	-- configuration of the duration in weeks
	DECLARE @GivingDurationLongWeeks int = 52
	DECLARE @GivingDurationShortWeeks int = 6
	
	DECLARE @cACTIVE_RECORD_STATUS_VALUE_GUID uniqueidentifier = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
	DECLARE @cPERSON_RECORD_TYPE_VALUE_GUID uniqueidentifier = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
	DECLARE @cFAMILY_GROUPTYPE_GUID uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'
	DECLARE @cADULT_ROLE_GUID uniqueidentifier = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42'	
	DECLARE @cCONTRIBUTION_TYPE_VALUE_GUID uniqueidentifier = '2D607262-52D6-4724-910D-5C6E8FB89ACC'
	DECLARE @cATTRIBUTE_FIRST_GAVE uniqueidentifier  = 'EE5EC76A-D4B9-56B5-4B48-29627D945F10'
	DECLARE @cATTRIBUTE_LAST_GAVE uniqueidentifier  = '02F64263-E290-399E-4487-FC236F4DE81F'
	DECLARE @cATTRIBUTE_GIFT_COUNT_SHORT uniqueidentifier  = 'AC11EF53-AE55-79A0-4CAD-43721750E988'
	DECLARE @cATTRIBUTE_GIFT_COUNT_LONG uniqueidentifier  = '57700E8F-ED11-D787-415A-04DDF411BB10'
	-- --------- END CONFIGURATION --------------
	DECLARE @ActiveRecordStatusValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cACTIVE_RECORD_STATUS_VALUE_GUID)
	DECLARE @PersonRecordTypeValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cPERSON_RECORD_TYPE_VALUE_GUID)
	DECLARE @FamilyGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = @cFAMILY_GROUPTYPE_GUID)
	DECLARE @AdultRoleId int = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = @cADULT_ROLE_GUID)
	DECLARE @ContributionTypeId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cCONTRIBUTION_TYPE_VALUE_GUID)
	
	-- calculate dates for queries
	DECLARE @SundayDateStart datetime = [dbo].[ufnUtility_GetPreviousSundayDate]()
	DECLARE @SundayGivingDurationLong datetime = DATEADD(DAY,  (7 * @GivingDurationLongWeeks * -1), @SundayDateStart)
	DECLARE @SundayGivingDurationShort datetime = DATEADD(DAY,  (7 * @GivingDurationShortWeeks * -1), @SundayDateStart);

	-- first gift (people w/Giving Group)
	DECLARE @FirstGaveAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_FIRST_GAVE)
	DELETE FROM [AttributeValue] WHERE [AttributeId] = @FirstGaveAttributeId;
	WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NOT NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsDateTime], [IsPersistedValueDirty] )
	SELECT [PersonId], [AttributeId], [FirstContributionDate], [IsSystem], [Guid], [CreateDate], [FirstContributionDate], 1  FROM 
		(SELECT 
			[PersonId]
			, @FirstGaveAttributeId AS [AttributeId]
			, (SELECT MIN(ft.TransactionDateTime)
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[GivingGroupId] = i.[GivingGroupId]
							AND ft.TransactionTypeValueId = @ContributionTypeId) AS [FirstContributionDate]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i
		WHERE [FamilyRole] = 'Adult') AS g
	WHERE g.[FirstContributionDate] IS NOT NULL
	-- first gift (people WITHOUT Giving Group)
	;WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsDateTime], [IsPersistedValueDirty])
	SELECT [PersonId], [AttributeId], [FirstContributionDate], [IsSystem], [Guid], [CreateDate], [FirstContributionDate], 1 FROM 
		(SELECT 
			[PersonId]
			, @FirstGaveAttributeId AS [AttributeId]
			, (SELECT MIN(ft.TransactionDateTime)
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[Id] = i.[PersonId] -- match by person id
							AND ft.TransactionTypeValueId = @ContributionTypeId) AS [FirstContributionDate]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i
		WHERE [FamilyRole] = 'Adult') AS g
	WHERE g.[FirstContributionDate] IS NOT NULL
	
	-- last gift (people w/Giving Group)
	DECLARE @LastGaveAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_LAST_GAVE)
	DELETE FROM [AttributeValue] WHERE [AttributeId] = @LastGaveAttributeId;
	WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NOT NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsDateTime], [IsPersistedValueDirty])
	SELECT [PersonId], [AttributeId], [LastContributionDate], [IsSystem], [Guid], [CreateDate], [LastContributionDate], 1 FROM 
		(SELECT 
			[PersonId]
			, @LastGaveAttributeId AS [AttributeId]
			, (SELECT MAX(ft.TransactionDateTime)
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[GivingGroupId] = i.[GivingGroupId]
							AND ft.TransactionTypeValueId = @ContributionTypeId) AS [LastContributionDate]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i
		WHERE [FamilyRole] = 'Adult') AS g
	WHERE g.[LastContributionDate] IS NOT NULL
	-- last gift (people WITHOUT Giving Group)
	;WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] -- match by person id
				AND p.[GivingGroupId] IS NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsDateTime], [IsPersistedValueDirty] )
	SELECT [PersonId], [AttributeId], [LastContributionDate], [IsSystem], [Guid], [CreateDate], [LastContributionDate], 1 FROM 
		(SELECT 
			[PersonId]
			, @LastGaveAttributeId AS [AttributeId]
			, (SELECT MAX(ft.TransactionDateTime)
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[Id] = i.[PersonId]
							AND ft.TransactionTypeValueId = @ContributionTypeId) AS [LastContributionDate]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i
		WHERE [FamilyRole] = 'Adult') AS g
	WHERE g.[LastContributionDate] IS NOT NULL
	-- number of gifts short duration (people w/Giving Group)
	DECLARE @GiftCountShortAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_GIFT_COUNT_SHORT)
	DELETE FROM [AttributeValue] WHERE [AttributeId] = @GiftCountShortAttributeId;
	WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NOT NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsNumeric], [IsPersistedValueDirty] )
	SELECT [PersonId], [AttributeId], [GiftCountDurationShort], [IsSystem], [Guid], [CreateDate], [GiftCountDurationShort], 1
    FROM 
		(SELECT 
			[PersonId]
			, @GiftCountShortAttributeId AS [AttributeId]
			, (SELECT COUNT(DISTINCT(ft.[Id])) 
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[GivingGroupId] = i.[GivingGroupId]
							AND ft.TransactionTypeValueId = @ContributionTypeId
							AND ft.TransactionDateTime >= @SundayGivingDurationShort
							AND ft.SundayDate <= @SundayDateStart) AS [GiftCountDurationShort]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i
		WHERE [FamilyRole] = 'Adult') AS g
	WHERE g.[GiftCountDurationShort] IS NOT NULL
	-- number of gifts short duration (people WITHOUT Giving Group)
	;WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsNumeric], [IsPersistedValueDirty])
	SELECT [PersonId], [AttributeId], [GiftCountDurationShort], [IsSystem], [Guid], [CreateDate], [GiftCountDurationShort], 1
    FROM 
		(SELECT 
			[PersonId]
			, @GiftCountShortAttributeId AS [AttributeId]
			, (SELECT COUNT(DISTINCT(ft.[Id])) 
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[Id] = i.[PersonId] -- match by person id
							AND ft.TransactionTypeValueId = @ContributionTypeId
							AND ft.TransactionDateTime >= @SundayGivingDurationShort
							AND ft.SundayDate <= @SundayDateStart) AS [GiftCountDurationShort]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i
		WHERE [FamilyRole] = 'Adult') AS g
	WHERE g.[GiftCountDurationShort] IS NOT NULL
	-- number of gifts long duration (people w/Giving Group)
	DECLARE @GiftCountLongAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_GIFT_COUNT_LONG)
	DELETE FROM [AttributeValue] WHERE [AttributeId] = @GiftCountLongAttributeId;
	WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NOT NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsNumeric], [IsPersistedValueDirty])
	SELECT [PersonId], [AttributeId], [GiftCountDurationLong], [IsSystem], [Guid], [CreateDate], [GiftCountDurationLong], 1
    FROM 
		(SELECT 
			[PersonId]
			, @GiftCountLongAttributeId AS [AttributeId]
			, (SELECT COUNT(DISTINCT(ft.[Id])) 
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[GivingGroupId] = i.[GivingGroupId]
							AND ft.TransactionTypeValueId = @ContributionTypeId
							AND ft.TransactionDateTime >= @SundayGivingDurationLong
							AND ft.SundayDate <= @SundayDateStart) AS [GiftCountDurationLong]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i
		WHERE [FamilyRole] = 'Adult') AS g
	WHERE g.[GiftCountDurationLong] IS NOT NULL
	
	-- number of gifts long duration (people WITHOUT Giving Group)
	;WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsNumeric], [IsPersistedValueDirty])
	SELECT [PersonId], [AttributeId], [GiftCountDurationLong], [IsSystem], [Guid], [CreateDate], [GiftCountDurationLong], 1
    FROM 
		(SELECT 
			[PersonId]
			, @GiftCountLongAttributeId AS [AttributeId]
			, (SELECT COUNT(DISTINCT(ft.[Id])) 
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[Id] = i.[PersonId] -- match by person id
							AND ft.TransactionTypeValueId = @ContributionTypeId
							AND ft.TransactionDateTime >= @SundayGivingDurationLong
							AND ft.SundayDate <= @SundayDateStart) AS [GiftCountDurationLong]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i
		WHERE [FamilyRole] = 'Adult') AS g
	WHERE g.[GiftCountDurationLong] IS NOT NULL
	
END
" );
        }

        #endregion

        #region JC: LMS

        private void LMSUpdatesUp()
        {
            LMSProgramListUp();
            ActivitiesAvailableSystemCommunicationUp();
        }

        private void LMSProgramListUp()
        {
            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA1460D8-E895-4B23-8A8E-10EBBED3990F", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "Lava Template", @"The lava template to use to render the page. Merge fields include: Programs, ShowCompletionStatus, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
//- Styles
{% stylesheet %}
    .page-container {
        display: flex;
        flex-direction: column;
    }
    
    .page-header-section {
        align-items: center; 
        border-radius: 12px;
        background-size: cover;
    }
 
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        background-color: white;
        border-radius: 12px;
        border: 1px solid #D9EDF2;
        width: 80%;
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        margin-bottom: 12px;
        padding-bottom: 12px;
    }
    
    .program-item-container {
        background-color: white; 
        border-radius: 12px;
        margin: 8px;
        display: flex;
        flex-direction: column;
        justify-content: space-between;
    }
{% endstylesheet %}

<div class=""page-container"" >
	<div class=""page-header-section"">
		<div class=""header-block text-center"">
			<div class=""h1 text-bold"">
				Growth Through Learning
			</div>
			<div class=""page-sub-header"">
				We believe that spiritual growth is deeply intertwined with continuous learning. 
				""Growth Through Learning"" is our commitment to nurture your faith journey, providing resources and opportunies to deepen your understanding of God's Word and his purpose for your life.
			</div>
		</div>
	</div>
	
	{% if Programs == empty %}
    	<div class=""programs-list-header-section center-block text-center mt-4 mb-4"">
    		<div class=""program-list-sub-header text-muted"">
 			There are currently no programs available.
		</div>
    	</div>
    {% else %}
    	<div class=""programs-list-header-section center-block text-center mb-4"">
    		<span class=""program-list-header h5"">
    			Programs Available
    		</span>
    
    		<div class=""program-list-sub-header text-muted"">
    			The following types of classes are available.
    		</div>
    	</div>
	{% endif %}
	<div class=""program-list-container d-flex flex-fill"">
		{% for program in Programs %}
		<div class=""program-item-container"">
		
			{% if program.ImageFileGuid and program.ImageFileGuid != '' %}
			<div class=""program-image program-item-header"" >
				<img style=""border-radius: 12px 12px 0 0;"" src=""/GetImage.ashx?guid={{program.ImageFileGuid}}&maxwidth=300&maxheight=150&mode=crop"" />
			</div>
			{% endif %}
			
			<div class=""program-item-middle p-3"">
			
				<h4 class=""program-name"">
					{{ program.Entity.PublicName }}
				</h4>
				<div class=""program-category"">
					<span class=""badge badge-info"">{{ program.Category }}</span>
				</div>
				<div class=""program-summary text-muted"">
					{{ program.Entity.Summary }} 
				</div>
			</div>
			
			<div class=""program-item-footer d-flex justify-content-between mt-4 p-3"">
				<a class=""btn btn-default"" href=""{{ program.CoursesLink }}"">Learn More</a>
				
				{% if ShowCompletionStatus %}
    				{% if program.CompletionStatus == 'Completed' %}
    					<span class=""badge badge-success p-2"" style=""line-height: normal;"">Completed</span>
    				{% elseif program.CompletionStatus == 'Pending' %}
    					<span class=""badge badge-info p-2"" style=""line-height: normal;"">Enrolled</span>
    				{% endif %}
				{% endif %}
			</div>
		</div>
		{% endfor %}
	</div>
</div>
", "D7B2012B-3D59-4DE3-8328-53B7EB9F7C1C" );
        }

        /// <summary>
        /// Updates the SystemCommunication for Newly Available Learning Activities.
        /// </summary>
        private void ActivitiesAvailableSystemCommunicationUp()
        {
            Sql( @"
UPDATE s SET
	Subject = 'New {%if ActivityCount > 1 %}Activities{%else%}Activity{%endif%} Available',
	Body = '
{% assign currentDate = ''Now'' | Date:''MMMM d, yyyy'' %}
{{ ''Global'' | Attribute:''EmailHeader'' }}
<h1 style=""margin:0;"">
	Your Activities
</h1>

<p>
    Below are your newly available activities as of {{ currentDate }}.
</p>

{% for course in Courses %}
	<h2> {{course.ProgramName}}: {{course.CourseName}} - {{course.CourseCode}} </h2>
	
	{% for activity in course.Activities %}
		<p class=""mb-4"">
			<strong>Activity:</strong>
			{{activity.ActivityName}}
				{% if activity.AvailableDate == null %}
					(always available)
				{% else %}
					(available {{ activity.AvailableDate | Date: ''MMM dd'' }})
				{% endif %}
			<br />
			<strong>Due:</strong>
			{% if activity.DueDate == null %}
				Optional
			{% else %}
				{{ activity.DueDate | HumanizeDateTime }}
			{% endif %}
		</p>	
	{% endfor %}
{% endfor %}

{{ ''Global'' | Attribute:''EmailFooter'' }}'
from systemcommunication s
WHERE [Guid] = 'D40A9C32-F179-4E5E-9B0D-CE208C5D1870'
" );
        }

        #endregion

        #region JC: Prayers

        private void PrayerAutomationUp()
        {
            RockMigrationHelper.UpdateCategory( SystemGuid.EntityType.CATEGORY, "AI Automations", "fa fa-brain", "Configurations for AI Automation", SystemGuid.Category.AI_AUTOMATION );

            var categoryName = "Global";
            RockMigrationHelper.AddDefinedType( categoryName, "Sentiment Emotions", "The sentiment of the related text as determined by an AI automation.", SystemGuid.DefinedType.SENTIMENT_EMOTIONS );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.SENTIMENT_EMOTIONS, "Anger", "The most identifiable sentiment of the available options is anger." );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.SENTIMENT_EMOTIONS, "Anticipation", "The most identifiable sentiment of the available options is anticipation." );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.SENTIMENT_EMOTIONS, "Disgust", "The most identifiable sentiment of the available options is disgust." );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.SENTIMENT_EMOTIONS, "Fear", "The most identifiable sentiment of the available options is fear." );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.SENTIMENT_EMOTIONS, "Joy", "The most identifiable sentiment of the available options is joy." );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.SENTIMENT_EMOTIONS, "Neutral", "The most identifiable sentiment of the available options is neutral." );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.SENTIMENT_EMOTIONS, "Sadness", "The most identifiable sentiment of the available options is sadness." );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.SENTIMENT_EMOTIONS, "Worry", "The most identifiable sentiment of the available options is worry." );

            RockMigrationHelper.UpdateSystemSetting( "core_PrayerRequestAICompletions", PrayerRequestAICompletionTemplate().ToJson() );

            RockMigrationHelper.UpdateFieldType( "AI Provider", "Field type to select an AI Provider.", "Rock", "Rock.Field.Types.AIProviderFieldType", SystemGuid.FieldType.AI_PROVIDER );

            AIAutomationAttributesUp();
            PrayerCategoriesPageUp();
        }

        private void PrayerCategoriesPageUp()
        {
            var categoryBlockTypeGuid = "E0EF4ED1-A621-43A1-A75D-C6319F7F10D2";
            var categoryTreeViewInstanceGuid = "42E90A50-D8EC-4370-B970-83E48518BC26";
            var categoryDetailInstanceGuid = "4B617C53-556E-4A1C-882E-D86BBC7B2CBC";
            var categoryTreeViewBlockTypeGuid = "ADE003C7-649B-466A-872B-B8AC952E7841";

            RockMigrationHelper.UpdatePageLayout( SystemGuid.Page.PRAYER_CATEGORIES, SystemGuid.Layout.LEFT_SIDEBAR_INTERNAL_SITE );
            RockMigrationHelper.DeleteBlock( categoryBlockTypeGuid );
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.PRAYER_CATEGORIES, null, categoryTreeViewBlockTypeGuid, "Category Tree View", "Sidebar1", "", "", 0, categoryTreeViewInstanceGuid );
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.PRAYER_CATEGORIES, null, SystemGuid.BlockType.OBSIDIAN_CATEGORY_DETAIL, "Category Detail", "Main", "", "", 0, categoryDetailInstanceGuid );

            Sql( $@"
-- Page and Block Type Guids.
DECLARE @prayerCategoriesPageGuid NVARCHAR(40) = '{SystemGuid.Page.PRAYER_CATEGORIES}';
DECLARE @categoryDetailBlockTypeGuid NVARCHAR(40) = '{SystemGuid.BlockType.OBSIDIAN_CATEGORY_DETAIL}';
DECLARE @categoryTreeViewBlockTypeGuid NVARCHAR(40) = '{categoryTreeViewBlockTypeGuid}';


-- Entity Type Id and Guid.
DECLARE @prayerRequestEntityTypeGuid NVARCHAR(40) = '{SystemGuid.EntityType.PRAYER_REQUEST}';
DECLARE @prayerRequestEntityTypeId NVARCHAR(40) = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = @prayerRequestEntityTypeGuid);

-- Attribute Guids
DECLARE @pageParameterKeyTreeViewAttributeGuid NVARCHAR(40) = 'AA057D3E-00CC-42BD-9998-600873356EDB';
DECLARE @entityTypeTreeViewAttributeGuid NVARCHAR(40) = '06D414F0-AA20-4D3C-B297-1530CCD64395';
DECLARE @detailPageTreeViewAttributeGuid NVARCHAR(40) = 'AEE521D8-124D-4BB3-8A80-5F368E5CEC15';
DECLARE @entityTypeCategoryDetailAttributeGuid NVARCHAR(40) = '3C6E056B-5087-4E02-B9FD-853B658E3C85';

-- Block Ids based on BlockType.Guid and Page.Guid
DECLARE @categoryTreeViewBlockId INT = (
	SELECT TOP 1 b.[Id]
	FROM [Page] p
	JOIN [Block] b ON b.[PageId] = p.[Id]
	JOIN [BlockType] bt ON bt.[Id] = b.[BlockTypeId]
	WHERE p.[Guid] = @prayerCategoriesPageGuid
		AND bt.[Guid] = @categoryTreeViewBlockTypeGuid
);
DECLARE @categoryDetailBlockId INT = (
	SELECT TOP 1 b.[Id]
	from [Page] p
	JOIN [Block] b ON b.[PageId] = p.[Id]
	JOIN [BlockType] bt ON bt.[Id] = b.BlockTypeId
	WHERE p.[Guid] = @prayerCategoriesPageGuid
		AND bt.[Guid] = @categoryDetailBlockTypeGuid
);

-- Add AttributeValues for the Category Tree View block instance.
INSERT AttributeValue (
	[IsSystem],
	[AttributeId],
	[EntityId],
	[Value],
	[Guid],
	[IsPersistedValueDirty]
)
SELECT 0, a.[AttributeId], @categoryTreeViewBlockId, a.[Value], NEWID(), 1
FROM (
	SELECT 
		a.[Id] [AttributeId], 
		CASE a.[Guid] 
			WHEN @pageParameterKeyTreeViewAttributeGuid THEN 'CategoryId'
			WHEN @entityTypeTreeViewAttributeGuid THEN @prayerRequestEntityTypeGuid
			WHEN @detailPageTreeViewAttributeGuid THEN @prayerCategoriesPageGuid -- Use the same page Guid.
		END [Value]
	FROM [Attribute] a
	WHERE a.[Guid] IN (
		@pageParameterKeyTreeViewAttributeGuid,
		@entityTypeTreeViewAttributeGuid,
		@detailPageTreeViewAttributeGuid
	)
) a
WHERE NOT EXISTS (
	SELECT *
	FROM [AttributeValue] ex
	WHERE ex.[AttributeId] = a.[AttributeId]
		AND ex.[EntityId] = @categoryTreeViewBlockId
)

-- Add AttributeValues for the Category Detail block instance.
INSERT [AttributeValue] (
	[IsSystem],
	[AttributeId],
	[EntityId],
	[Value],
	[Guid],
	[IsPersistedValueDirty]
)
SELECT 0, a.[Id], @categoryDetailBlockId, @prayerRequestEntityTypeId, NEWID(), 1
FROM [Attribute] a
WHERE a.[Guid] = @entityTypeCategoryDetailAttributeGuid
	AND NOT EXISTS (
	    SELECT *
	    FROM [AttributeValue] ex
	    WHERE ex.[AttributeId] = a.Id
		    AND ex.[EntityId] = @categoryDetailBlockId
)" );
        }

        private void AIAutomationAttributesUp()
        {
            Sql( $@"
DECLARE @categoryEntityTypeId INT = (SELECT Id FROM EntityType WHERE [Guid] = '{SystemGuid.EntityType.CATEGORY}');
DECLARE @prayerRequestEntityTypeId INT = (SELECT Id FROM EntityType WHERE [Guid] = '{SystemGuid.EntityType.PRAYER_REQUEST}');
DECLARE @nextOrder INT = (SELECT MAX([Order]) FROM Attribute);

DECLARE @aiProviderAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_AI_PROVIDER}';
DECLARE @aiTextEnhancementAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_TEXT_ENHANCEMENT}';
DECLARE @aiNameRemovalAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_REMOVE_NAMES}';
DECLARE @aiSentimentAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_CLASSIFY_SENTIMENT}';
DECLARE @aiAutoCategorizeAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_AUTO_CATEGORIZE}';
DECLARE @aiAIModerationAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_ENABLE_AI_MODERATION}';
DECLARE @aiModerationWorkflowAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_MODERATION_ALERT_WORKFLOW_TYPE}';
DECLARE @aiPublicAppropriatenessAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_CHECK_PUBLIC_APPROPRIATENESS}';
DECLARE @aiChildCategoriesInheritAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_CHILD_CATEGORIES_INHERIT_CONFIGURATION}';

DECLARE @aiProviderPickerFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = '{SystemGuid.FieldType.AI_PROVIDER}');
DECLARE @booleanFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = '{SystemGuid.FieldType.BOOLEAN}');
DECLARE @singleSelectFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = '{SystemGuid.FieldType.SINGLE_SELECT}');
DECLARE @workflowTypeFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = '{SystemGuid.FieldType.WORKFLOW_TYPE}');
DECLARE @definedValuePickerFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = '{SystemGuid.FieldType.DEFINED_VALUE}');

INSERT [Attribute] ( [IsSystem], [FieldTypeId], [Key], [Name], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Description], [Guid] )
SELECT [IsSystem], [FieldTypeId], [Key], [Name], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Description], [Guid]
FROM (
		  SELECT 0 [IsSystem], @aiProviderPickerFieldTypeId [FieldTypeId], 'AIProvider' [Key], 'AI Provider' [Name], @nextOrder [Order], 1 [IsGridColumn], 0 [IsMultiValue], 0 [IsRequired], @categoryEntityTypeId [EntityTypeId], 'EntityTypeId' [EntityTypeQualifierColumn], @prayerRequestEntityTypeId [EntityTypeQualifierValue], 'Determines what AI service to use for the processing. If no value is provided the first active provider configured will be used.' [Description], NULL [DefaultValue], @aiProviderAttributeGuid [Guid]
	UNION SELECT 0 [IsSystem], @singleSelectFieldTypeId [FieldTypeId], 'PrayerRequestTextEnhancement' [Key], 'Prayer Request Text Enhancement' [Name], @nextOrder + 1 [Order], 1 [IsGridColumn], 0 [IsMultiValue], 0 [IsRequired], @categoryEntityTypeId [EntityTypeId], 'EntityTypeId' [EntityTypeQualifierColumn], @prayerRequestEntityTypeId [EntityTypeQualifierValue], 'Determines if the AI should attempt to polish the text of the request.' [Description], '' [DefaultValue], @aiTextEnhancementAttributeGuid [Guid]
	UNION SELECT 0 [IsSystem], @singleSelectFieldTypeId [FieldTypeId], 'RemoveNames' [Key], 'Remove Names' [Name], @nextOrder + 2 [Order], 1 [IsGridColumn], 0 [IsMultiValue], 0 [IsRequired], @categoryEntityTypeId [EntityTypeId], 'EntityTypeId' [EntityTypeQualifierColumn], @prayerRequestEntityTypeId [EntityTypeQualifierValue], 'Determines if the AI should remove names from the request text.' [Description], '' [DefaultValue], @aiNameRemovalAttributeGuid [Guid]
	UNION SELECT 0 [IsSystem], @booleanFieldTypeId [FieldTypeId], 'ClassifySentiment' [Key], 'Classify Sentiment' [Name], @nextOrder + 3 [Order], 1 [IsGridColumn], 0 [IsMultiValue], 0 [IsRequired], @categoryEntityTypeId [EntityTypeId], 'EntityTypeId' [EntityTypeQualifierColumn], @prayerRequestEntityTypeId [EntityTypeQualifierValue], 'Determines if the AI should determine the type of emotion found in the request. The list of sentiments are configured in the Sentiment Emotions defined type.' [Description], 'False' [DefaultValue], @aiSentimentAttributeGuid [Guid]
	UNION SELECT 0 [IsSystem], @booleanFieldTypeId [FieldTypeId], 'AutoCategorize' [Key], 'Auto Categorize' [Name], @nextOrder + 4 [Order], 1 [IsGridColumn], 0 [IsMultiValue], 0 [IsRequired], @categoryEntityTypeId [EntityTypeId], 'EntityTypeId' [EntityTypeQualifierColumn], @prayerRequestEntityTypeId [EntityTypeQualifierValue], 'Determines if the AI should pick the child category that best matches the text of the request.' [Description], 'False' [DefaultValue], @aiAutoCategorizeAttributeGuid [Guid]
	UNION SELECT 0 [IsSystem], @booleanFieldTypeId [FieldTypeId], 'EnableAIModeration' [Key], 'Enable AI Moderation' [Name], @nextOrder + 5 [Order], 1 [IsGridColumn], 0 [IsMultiValue], 0 [IsRequired], @categoryEntityTypeId [EntityTypeId], 'EntityTypeId' [EntityTypeQualifierColumn], @prayerRequestEntityTypeId [EntityTypeQualifierValue], 'Determines if the AI should moderate the request. This will determine if any of the moderation categories (e.g. Self-Harm, Violence, Sexual etc.) are present.' [Description], 'False' [DefaultValue], @aiAIModerationAttributeGuid [Guid]
	UNION SELECT 0 [IsSystem], @workflowTypeFieldTypeId [FieldTypeId], 'ModerationAlertWorkflowType' [Key], 'Moderation Alert Workflow Type' [Name], @nextOrder + 6 [Order], 1 [IsGridColumn], 0 [IsMultiValue], 0 [IsRequired], @categoryEntityTypeId [EntityTypeId], 'EntityTypeId' [EntityTypeQualifierColumn], @prayerRequestEntityTypeId [EntityTypeQualifierValue], 'The workflow to launch if any of the moderation categories are found.' [Description], '' [DefaultValue], @aiModerationWorkflowAttributeGuid [Guid]
	UNION SELECT 0 [IsSystem], @booleanFieldTypeId [FieldTypeId], 'CheckforPublicAppropriateness' [Key], 'Check for Public Appropriateness' [Name], @nextOrder + 7 [Order], 1 [IsGridColumn], 0 [IsMultiValue], 0 [IsRequired], @categoryEntityTypeId [EntityTypeId], 'EntityTypeId' [EntityTypeQualifierColumn], @prayerRequestEntityTypeId [EntityTypeQualifierValue], 'Determines if the AI should review the text for public appropriateness.' [Description], 'False' [DefaultValue], @aiPublicAppropriatenessAttributeGuid [Guid]
	UNION SELECT 0 [IsSystem], @booleanFieldTypeId [FieldTypeId], 'ChildCategoriesInherit' [Key], 'Child Categories Inherit Configuration' [Name], @nextOrder + 8 [Order], 1 [IsGridColumn], 0 [IsMultiValue], 0 [IsRequired], @categoryEntityTypeId [EntityTypeId], 'EntityTypeId' [EntityTypeQualifierColumn], @prayerRequestEntityTypeId [EntityTypeQualifierValue], 'Specifies whether this configuration should apply to requests in categories under this parent.' [Description], 'False' [DefaultValue], @aiChildCategoriesInheritAttributeGuid [Guid]
) [seed]
WHERE NOT EXISTS (
	SELECT 1
	FROM [Attribute] [ex]
	WHERE [ex].[Guid] = [seed].[Guid]
);

-- AttributeQualifer configuration for SingleSelect value lists.
DECLARE @valuesQualifierKey NVARCHAR(100) = 'values';
DECLARE @textEnhancementValuesList NVARCHAR(MAX) = '1^Minor Formatting and Spelling,2^Enhance Readability';
DECLARE @nameRemovalValuesList NVARCHAR(MAX) = '1^Last Names Only,2^First and Last Names';

-- AttributeQualifier configuration for SingleSelect as a radio button list.
DECLARE @radioButtonControlTypeQualifierKey NVARCHAR(100) = 'fieldtype';
DECLARE @radioButtonControlTypeId NVARCHAR(MAX) = 'rb';

-- AttributeQualifier configuration for Booleans as a checkbox.
DECLARE @booleanControlTypeQualifierKey NVARCHAR(100) = 'BooleanControlType';
DECLARE @checkboxControlTypeId NVARCHAR(MAX) = '1';

-- Add value lists for single selects.
INSERT [AttributeQualifier] ( [IsSystem], [AttributeId], [Key], [Value], [Guid] )
SELECT 
	0 [IsSystem],
	a.Id [AttributeId], 
	@valuesQualifierKey [Key], 
	-- Use appropriate value list for the attribute being inserted.
	IIF(
		a.[Guid] = @aiTextEnhancementAttributeGuid, 
		@textEnhancementValuesList, 
		@nameRemovalValuesList
	) [Value], 
	NEWID() [Guid]
FROM [Attribute] a
WHERE a.[Guid] IN (
	-- Single-Select field types.
	@aiTextEnhancementAttributeGuid,
	@aiNameRemovalAttributeGuid
)	
	AND NOT EXISTS (
		SELECT *
		FROM [AttributeQualifier] aq
		WHERE aq.AttributeId = a.Id
			AND aq.[Key] = @valuesQualifierKey
	)

-- Add control type qualifiers for both single selects and boolean field types.
INSERT [AttributeQualifier] ( [IsSystem], [AttributeId], [Key], [Value], [Guid] )
SELECT 
	0 [IsSystem],
	a.Id [AttributeId], 
	-- Use the fieldtype key for single-selects and the BooleanControlType for booleans.
	IIF(
		a.[Guid] IN (@aiTextEnhancementAttributeGuid, @aiNameRemovalAttributeGuid), 
		@radioButtonControlTypeQualifierKey, 
		@booleanControlTypeQualifierKey
	) [Key], 
	-- Use the 'rb' value for single-selects/radio buttons and '1' for booleans.
	IIF(
		a.[Guid] IN (@aiTextEnhancementAttributeGuid, @aiNameRemovalAttributeGuid), 
		@radioButtonControlTypeId, 
		@checkboxControlTypeId
	) [Value], 
	NEWID() [Guid]
FROM [Attribute] a
WHERE a.[Guid] IN (
	-- Single-Select field types.
	@aiTextEnhancementAttributeGuid,
	@aiNameRemovalAttributeGuid,

	-- Boolean field types.
	@aiSentimentAttributeGuid,
	@aiAutoCategorizeAttributeGuid,
	@aiAIModerationAttributeGuid,
	@aiPublicAppropriatenessAttributeGuid,
	@aiChildCategoriesInheritAttributeGuid
)	
	AND NOT EXISTS (
		SELECT *
		FROM [AttributeQualifier] aq
		WHERE aq.AttributeId = a.Id
			AND aq.[Key] = IIF(
				a.[Guid] IN (@aiTextEnhancementAttributeGuid, @aiNameRemovalAttributeGuid), 
				@radioButtonControlTypeQualifierKey, 
				@booleanControlTypeQualifierKey
			)
	)

DECLARE @aiAutomationsCategoryId INT = (SELECT Id FROM Category WHERE [Guid] = '{SystemGuid.Category.AI_AUTOMATION}' );

INSERT [AttributeCategory] (AttributeId, CategoryId)
SELECT a.Id, @aiAutomationsCategoryId
FROM [Attribute] a
WHERE a.[Guid] IN (
	@aiProviderAttributeGuid,
	@aiTextEnhancementAttributeGuid,
	@aiNameRemovalAttributeGuid,
	@aiSentimentAttributeGuid,
	@aiAutoCategorizeAttributeGuid,
	@aiAIModerationAttributeGuid,
	@aiModerationWorkflowAttributeGuid,
	@aiPublicAppropriatenessAttributeGuid,
	@aiChildCategoriesInheritAttributeGuid
)
	AND NOT EXISTS (
		SELECT *
		FROM [AttributeCategory] ex
		WHERE ex.AttributeId = a.Id
			AND ex.CategoryId = @aiAutomationsCategoryId
	)" );
        }

        private PrayerRequest.PrayerRequestAICompletions PrayerRequestAICompletionTemplate()
        {
            return new PrayerRequest.PrayerRequestAICompletions
            {
                PrayerRequestFormatterTemplate = @"
{%- comment -%}
    This is the lava template for the AI automation that occurs in the PrayerRequest PostSave SaveHook.
    Available Lava Fields:

    PrayerRequest - The PrayerRequest entity object.
    ParentCategoryId - The integer identifier of the parent category (for the PrayerRequest).
    HasTextTransformations - True if the AI automation is configured to perform any text changes.
    AutoCategorize - True if the AI automation is configured to auto-categorize the prayer request.
    ClassifySentiment - True if the AI automation is configured to classify the sentiment of the prayer request.
    CheckAppropriateness - True if the AI automation is configured to check if prayer request is appropriate for public viewing.
    EnableFixFormattingAndSpelling - True if the AI Automation AttributeValue for TextEnhancement is equal to MinorFormattingAndSpelling; otherwise false.
    EnableEnhancedReadability - True if the AI Automation AttributeValue for TextEnhancement is equal to EnhanceReadability; otherwise false.    
    EnableRemovalLastNames - True if the AI Automation AttributeValue for NameRemoval is equal to LastNamesOnly; otherwise false.
    EnableRemovalFirstAndLastNames - True if the AI Automation AttributeValue for NameRemoval is equal to FirstAndLastNames; otherwise false.
    Categories - The child categories of the currently selected Prayer Request category or children of ""All Church"" if no category is selected.
    SentimentEmotions - The Sentiment Emotions DefinedType with DefinedValues from Cache (well-known Guid: C9751C20-DA81-4521-81DE-0099D6F598BA).
{%- endcomment -%}
{%- if EnableRemovalLastNames == true and EnableRemovalFirstAndLastNames == false %}
Remove surname names, but leave first names from the text below.
{%- endif -%}
{%- if EnableRemovalFirstAndLastNames == true %}
Remove names, both first and last, from the text below.
{%- endif -%}
{%- if EnableFixFormattingAndSpelling == ture and EnableEnhancedReadability == false %}
Fix any formatting and spelling mistakes, but do not change the text.
{% endif -%}
{%- if EnableEnhancedReadability == true %}
Make the request more readable and polished.
{% endif -%}

```Prayer Request```
{{ PrayerRequest.Text }}
```Prayer Request```
",
                PrayerRequestAnalyzerTemplate = @"
{%- comment -%}
    This is the lava template for the AI automation that occurs in the PrayerRequest PostSave SaveHook.
    Available Lava Fields:

    PrayerRequest - The PrayerRequest entity object.
    ParentCategoryId - The integer identifier of the parent category (for the PrayerRequest).
    AutoCategorize - True if the AI automation is configured to auto-categorize the prayer request.
    ClassifySentiment - True if the AI automation is configured to classify the sentiment of the prayer request.
    CheckAppropriateness - True if the AI automation is configured to check if prayer request is appropriate for public viewing.
    Categories - The child categories of the currently selected Prayer Request category or children of ""All Church"" if no category is selected.
    SentimentEmotions - The Sentiment Emotions DefinedType with DefinedValues from Cache (well-known Guid: C9751C20-DA81-4521-81DE-0099D6F598BA).
{%- endcomment -%}

{%- if AutoCategorize == true %}

Determine the category for the text delimited by ```Prayer Request```. Include the Id of the category that most closely matches from the JSON list below.



    {%- assign categoriesJson = '[' -%}

    {%- for category in Categories -%}

        {%- capture categoriesJsonRow -%}
            {
                ""Id"": {{ category.Id }},
                ""CategoryName"": {{category.Name | ToJSON }}
            }{% unless forloop.last %},{% endunless %}
        {%- endcapture -%}

        {%- assign categoriesJson = categoriesJson | Append:categoriesJsonRow -%}
    {%- endfor -%}

    {%- assign categoriesJson = categoriesJson | Append: ']' %}

```Categories```
{{ categoriesJson | FromJSON | ToJSON }}
```Categories```
{% endif -%}

{%- if ClassifySentiment == true %}

Determine the person's sentiment for the text. Include the Id of the sentiment that most closely matches from the JSON list below.

    {%- assign sentimentsJson = '[' -%}

    {%- for definedValue in SentimentEmotions.DefinedValues -%}

        {%- capture sentimentsJsonRow -%}
            {
                ""Id"": {{ definedValue.Id }},
                ""Sentiment"": {{ definedValue.Value | ToJSON }}
            }{% unless forloop.last %},{% endunless %}
        {%- endcapture -%}

        {%- assign sentimentsJson = sentimentsJson | Append:sentimentsJsonRow -%}
    {%- endfor -%}

    {%- assign sentimentsJson = sentimentsJson | Append: ']' %}

```Sentiments```
{{ sentimentsJson | FromJSON | ToJSON }}
```Sentiments```
{% endif -%}

{%- if CheckAppropriateness == true -%}
    Determine if the text is appropriate for public viewing being sensitive to privacy and legal concerns.
{%- endif %}

```Prayer Request```
{{ PrayerRequest.Text }}
```Prayer Request```

Respond with ONLY a JSON object in the following format:
{
    {% if ClassifySentiment == true %}""sentimentId"": <The Id of the Sentiment from the list delimited by ```Sentiments``` that most closely matches the modified Prayer Request text>,{% endif %}
    {% if AutoCategorize == true %}""categoryId"": <The Id of the Category from the list delimited by ```Categories``` that most closely matches the modified Prayer Request text>,{% endif %}
    {% if CheckAppropriateness == true %}""isAppropriateForPublic"": <boolean value indicating whether the modified text is appropriate for public viewing>{% endif %}
}
"
            };
        }

        private void PrayerAutomationsDown()
        {
            PrayerCategoriesPageDown();
            AIAutomationAttributesDown();
        }

        private void PrayerCategoriesPageDown()
        {
            var categoryBlockTypeGuid = "620FC4A2-6587-409F-8972-22065919D9AC";
            var categoryBlockGuid = "E0EF4ED1-A621-43A1-A75D-C6319F7F10D2";
            var treeViewInstanceGuid = "42E90A50-D8EC-4370-B970-83E48518BC26";
            var categoryDetailInstanceGuid = "4B617C53-556E-4A1C-882E-D86BBC7B2CBC";
            RockMigrationHelper.UpdatePageLayout( SystemGuid.Page.PRAYER_CATEGORIES, SystemGuid.Layout.FULL_WIDTH );
            RockMigrationHelper.DeleteBlock( treeViewInstanceGuid );
            RockMigrationHelper.DeleteBlock( categoryDetailInstanceGuid );
            RockMigrationHelper.AddBlock( SystemGuid.Page.PRAYER_CATEGORIES, SystemGuid.Layout.FULL_WIDTH, categoryBlockTypeGuid, "Categories", "Main", "", "", 0, categoryBlockGuid );

        }

        private void AIAutomationAttributesDown()
        {
            Sql( $@"
DECLARE @aiProviderAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_AI_PROVIDER}';
DECLARE @aiTextEnhancementAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_TEXT_ENHANCEMENT}';
DECLARE @aiNameRemovalAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_REMOVE_NAMES}';
DECLARE @aiSentimentAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_CLASSIFY_SENTIMENT}';
DECLARE @aiAutoCategorizeAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_AUTO_CATEGORIZE}';
DECLARE @aiAIModerationAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_ENABLE_AI_MODERATION}';
DECLARE @aiModerationWorkflowAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_MODERATION_ALERT_WORKFLOW_TYPE}';
DECLARE @aiPublicAppropriatenessAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_CHECK_PUBLIC_APPROPRIATENESS}';
DECLARE @aiChildCategoriesInheritAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_CHILD_CATEGORIES_INHERIT_CONFIGURATION}';

DELETE a
FROM Attribute a
WHERE [Guid] IN (
	@aiProviderAttributeGuid,
	@aiTextEnhancementAttributeGuid,
	@aiNameRemovalAttributeGuid,
	@aiSentimentAttributeGuid,
	@aiAutoCategorizeAttributeGuid,
	@aiAIModerationAttributeGuid,
	@aiModerationWorkflowAttributeGuid,
	@aiPublicAppropriatenessAttributeGuid,
	@aiChildCategoriesInheritAttributeGuid
)" );
        }

        #endregion

        #endregion

        #region MigrationRollupsForV17_0_13

        #region KH: Update Digital Tools Page Settings

        private void UpdateDigitalToolsPageSettingsUp()
        {
            Sql( $@"
UPDATE [dbo].[Page]
SET [BreadCrumbDisplayName] = 0
WHERE [Page].[Guid] = 'A6D78C4F-958F-4196-B8FF-527A10F5F047'
" );
        }

        private void UpdateDigitalToolsPageSettingsDown()
        {
            Sql( $@"
UPDATE [dbo].[Page]
SET [BreadCrumbDisplayName] = 1
WHERE [Page].[Guid] = 'A6D78C4F-958F-4196-B8FF-527A10F5F047'
" );
        }

        #endregion

        #endregion

        #region MigrationRollupsForV17_0_14

        #region JC: Update IX_EntityTypeId_EntityId Index

        private void UpdateHistoryTableIndexUp()
        {
            // This job can be run at any time and should improve performance of the HistoryLog web forms block.
            // By default it will run at 2 am unless manually run sooner by an administrator.
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v17.0 - History Index Update - Add Includes",
                description: "This job updates the IX_EntityTypeId_EntityId index on the dbo.History table to add includes for: " +
                "[RelatedEntityTypeId], [RelatedEntityId], [CategoryId], [CreatedByPersonAliasId], [CreatedDateTime].",
                jobType: "Rock.Jobs.PostUpdateJobs.PostV17UpdateHistoryTableEntityTypeIdIndexPostMigration",
                cronExpression: "0 0 2 ? * * *",
                guid: Rock.SystemGuid.ServiceJob.POST_170_UPDATE_HISTORY_ENTITYTYPEID_INDEX );
        }

        #endregion

        #region JC: Prayer Automation Completions

        private void PrayerAutomationCompletionsUp()
        {
            RockMigrationHelper.UpdateSystemSetting( "core_PrayerRequestAICompletions", MigrationRollupsForV17_0_14_PrayerRequestAICompletionTemplate().ToJson() );
        }

        private PrayerRequest.PrayerRequestAICompletions MigrationRollupsForV17_0_14_PrayerRequestAICompletionTemplate()
        {
            return new PrayerRequest.PrayerRequestAICompletions
            {
                PrayerRequestFormatterTemplate = @"
{%- comment -%}
This is the lava template for the Text formatting AI automation that occurs in the PrayerRequest PostSave SaveHook.
Available Lava Fields:

PrayerRequest - The PrayerRequest entity object.
EnableFixFormattingAndSpelling - True if the AI Automation AttributeValue for TextEnhancement is equal to MinorFormattingAndSpelling; otherwise false.
EnableEnhancedReadability - True if the AI Automation AttributeValue for TextEnhancement is equal to EnhanceReadability; otherwise false.    
EnableRemovalLastNames - True if the AI Automation AttributeValue for NameRemoval is equal to LastNamesOnly; otherwise false.
EnableRemovalFirstAndLastNames - True if the AI Automation AttributeValue for NameRemoval is equal to FirstAndLastNames; otherwise false.
{%- endcomment -%}

Refer to the Prayer Request below, delimited by ```Prayer Request```. Return only the modified text without any additional comments.

{%- if EnableRemovalLastNames == true and EnableRemovalFirstAndLastNames == false %}
Remove surname and family names, but leave first names in their original form from the text below.
{%- endif -%}
{%- if EnableRemovalFirstAndLastNames == true %}
Remove names, both first and last, from the text below. If the text uses a pronoun or possessive pronoun continue to use that; otherwise use generic words like: ""an individual"", ""some individuals"", ""a family"" etc.
{%- endif -%}
{%- if EnableFixFormattingAndSpelling == true and EnableEnhancedReadability == false %}
Fix any formatting and spelling mistakes, but do not change the text.
{% endif -%}
{%- if EnableEnhancedReadability == true %}
Make the request more readable and polished. Do not change words if they significantly alter the perceived meaning.
If the request is not in English and a translation is included - leave the translation in it's original form.
{% endif -%}

```Prayer Request```
{{ PrayerRequest.Text }}
```Prayer Request```
",
                PrayerRequestAnalyzerTemplate = @"
%- comment -%}
This is the lava template for the AI analysis automation that occurs in the PrayerRequest PostSave SaveHook.
Available Lava Fields:
PrayerRequest - The PrayerRequest entity object.
ParentCategoryId - The integer identifier of the parent category (for the PrayerRequest).
AutoCategorize - True if the AI automation is configured to auto-categorize the prayer request.
ClassifySentiment - True if the AI automation is configured to classify the sentiment of the prayer request.
CheckAppropriateness - True if the AI automation is configured to check if prayer request is appropriate for public viewing.
Categories - The child categories of the currently selected Prayer Request category or children of ""All Church"" if no category is selected.
SentimentEmotions - The Sentiment Emotions DefinedType with DefinedValues from Cache (well-known Guid: C9751C20-DA81-4521-81DE-0099D6F598BA).
{%- endcomment -%}
{%- if AutoCategorize == true and Categories != empty %}
Choose the Id of the category that most closely matches the main theme of the prayer request.
{%- assign categoriesJson = '[' -%}
{%- for category in Categories -%}
    {%- capture categoriesJsonRow -%}
        {
            ""Id"": {{ category.Id }},
            ""CategoryName"": {{category.Name | ToJSON }}
        }{% unless forloop.last %},{% endunless %}
    {%- endcapture -%}
    {%- assign categoriesJson = categoriesJson | Append:categoriesJsonRow -%}
{%- endfor -%}
{%- assign categoriesJson = categoriesJson | Append: ']' %}
```Categories```
{{ categoriesJson | FromJSON | ToJSON }}
```Categories```
{% endif -%}

{%- if ClassifySentiment == true %}
Choose the Id of the sentiment that most closely matches the prayer request text.
{%- assign sentimentsJson = '[' -%}
{%- for definedValue in SentimentEmotions.DefinedValues -%}
    {%- capture sentimentsJsonRow -%}
        {
            ""Id"": {{ definedValue.Id }},
            ""Sentiment"": {{ definedValue.Value | ToJSON }}
        }{% unless forloop.last %},{% endunless %}
    {%- endcapture -%}
    {%- assign sentimentsJson = sentimentsJson | Append:sentimentsJsonRow -%}
{%- endfor -%}
{%- assign sentimentsJson = sentimentsJson | Append: ']' %}
```Sentiments```
{{ sentimentsJson | FromJSON | ToJSON }}
```Sentiments```
{% endif -%}
{%- if CheckAppropriateness == true -%}
Determine if the prayer request text is appropriate for public viewing being sensitive to privacy and legal concerns.
First names alone are ok, but pay attention to other details which might make it easy to uniquely identify an individual within a community.
{%- endif %}

```Prayer Request```
{{PrayerRequest.Text}}
```Prayer Request```
Respond with ONLY a VALID JSON object in the format below. Do not use backticks ```.
{
""sentimentId"": <The Id of the Sentiment from the list delimited by ```Sentiments``` that most closely matches the main theme of the prayer request text>,
""categoryId"": <The Id of the Category from the list delimited by ```Categories``` that most closely matches the main theme of the prayer request text>,
""isAppropriateForPublic"": <boolean value indicating whether the prayer request text is appropriate for public viewing>
}
"
            };
        }

        #endregion

        #region PA: Standardize names of Page Shortlink Blocks

        private void StandardizePageShortlinkNamesUp()
        {
            RockMigrationHelper.RenameEntityType( "026C6A93-5295-43E9-B67D-C3708ACB25B9",
                "Rock.Blocks.Cms.PageShortLinkDialog",
                "Short Link Detail",
                "Rock.Blocks.Cms.PageShortLinkDialog, Rock.Blocks, Version=1.16.6.6, Culture=neutral, PublicKeyToken=null",
                false,
                false );

            // Add/Update Obsidian Block Type
            //   Name:Shortened Links (dialog)
            //   Category:Administration
            //   EntityType:Rock.Blocks.Cms.ShortLink
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Page Short Link (dialog)", "Displays a dialog for adding a short link to the current page.", "Rock.Blocks.Cms.PageShortLinkDialog", "Administration", "C85551E8-A363-4AA6-9BFD-E6A1C9CEDE80" );
        }

        #endregion

        #region PA: Chop blocks for v1.17.0.27

        // PA: Register block attributes for chop job in v1.17.0.27
        private void RegisterBlockAttributesForChop()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.BenevolenceTypeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.BenevolenceTypeDetail", "Benevolence Type Detail", "Rock.Blocks.Finance.BenevolenceTypeDetail, Rock.Blocks, Version=1.17.0.26, Culture=neutral, PublicKeyToken=null", false, false, "B39BA58D-83DD-46E0-BA47-787C4EB4EB69" );

            // Add/Update Obsidian Block Type
            //   Name:Benevolence Type Detail
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.BenevolenceTypeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Benevolence Type Detail", "Block to display the benevolence type detail.", "Rock.Blocks.Finance.BenevolenceTypeDetail", "Finance", "03397615-EF2B-4D33-BD62-A79186F56ACE" );

            // Attribute for BlockType
            //   BlockType: Benevolence Type Detail
            //   Category: Finance
            //   Attribute: Benevolence Type Attributes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "03397615-EF2B-4D33-BD62-A79186F56ACE", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Benevolence Type Attributes", "BenevolenceTypeAttributes", "Benevolence Type Attributes", @"The attributes that should be displayed / edited for benevolence types.", 1, @"", "2ACAFAC1-CF36-4BD4-A7E0-42FD771092E3" );
        }

        // PA: Chop blocks for v1.17.0.27
        private void ChopBlockTypesv17()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types - 1.17.0.27",
                blockTypeReplacements: new Dictionary<string, string> {
{ "052B84EA-0C34-4A07-AC4C-1FBCEC87C223", "E18AB976-6665-48A5-B418-8FAC8F374135" }, // Suggestion Detail
{ "14293AEB-B0F5-434B-844A-66592AE3A416", "7E2DFB55-F1AB-4452-A5DF-6CE65FBFDDAD" }, // Photo Opt-Out
{ "0DE16268-BD5B-4CFC-A7C6-F1E07F47527A", "F61C0FDF-E8A0-457A-B8AF-42CAC8A18718" }, // Benevolence Type List
{ "C96479B6-E309-4B1A-B024-1F1276122A13", "03397615-EF2B-4D33-BD62-A79186F56ACE" }, // Benevolence Type Detail
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_CHOP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: null );
        }

        private void ChopBlocksUp()
        {
            RegisterBlockAttributesForChop();

            // Commenting out as the job has been updated in later migrations.
            // ChopBlockTypesv17();
        }

        #endregion

        #endregion

        #region MigrationRollupsForV17_0_15

        #region JC: Prayer Automation Completions

        private void MigrationRollupsForV17_0_15_PrayerAutomationCompletionsUp()
        {
            RockMigrationHelper.UpdateSystemSetting( "core_PrayerRequestAICompletions", MigrationRollupsForV17_0_15_PrayerRequestAICompletionTemplate().ToJson() );
        }

        private PrayerRequest.PrayerRequestAICompletions MigrationRollupsForV17_0_15_PrayerRequestAICompletionTemplate()
        {
            return new PrayerRequest.PrayerRequestAICompletions
            {
                PrayerRequestFormatterTemplate = @"
{%- comment -%}
This is the lava template for the Text formatting AI automation that occurs in the PrayerRequest PostSave SaveHook.
Available Lava Fields:

PrayerRequest - The PrayerRequest entity object.
EnableFixFormattingAndSpelling - True if the AI Automation AttributeValue for TextEnhancement is equal to MinorFormattingAndSpelling; otherwise false.
EnableEnhancedReadability - True if the AI Automation AttributeValue for TextEnhancement is equal to EnhanceReadability; otherwise false.    
EnableRemovalLastNames - True if the AI Automation AttributeValue for NameRemoval is equal to LastNamesOnly; otherwise false.
EnableRemovalFirstAndLastNames - True if the AI Automation AttributeValue for NameRemoval is equal to FirstAndLastNames; otherwise false.
{%- endcomment -%}

Refer to the Prayer Request below, delimited by ```Prayer Request```. Return only the modified text without any additional comments.

{%- if EnableRemovalLastNames == true and EnableRemovalFirstAndLastNames == false %}
Remove surname and family names, but leave first names in their original form from the text below.
{%- endif -%}
{%- if EnableRemovalFirstAndLastNames == true %}
Remove names, both first and last, from the text below. If the text uses a pronoun or possessive pronoun continue to use that; otherwise use generic words like: ""an individual"", ""some individuals"", ""a family"" etc.
{%- endif -%}
{%- if EnableFixFormattingAndSpelling == true and EnableEnhancedReadability == false %}
Fix any formatting and spelling mistakes, but do not change the text.
{% endif -%}
{%- if EnableEnhancedReadability == true %}
Make the request more readable and polished. Do not change words if they significantly alter the perceived meaning.
If the request is not in English and a translation is included - leave the translation in it's original form.
{% endif -%}

```Prayer Request```
{{ PrayerRequest.Text }}
```Prayer Request```
",
                PrayerRequestAnalyzerTemplate = @"
{%- comment -%}
This is the lava template for the AI analysis automation that occurs in the PrayerRequest PostSave SaveHook.
Available Lava Fields:
PrayerRequest - The PrayerRequest entity object.
ParentCategoryId - The integer identifier of the parent category (for the PrayerRequest).
AutoCategorize - True if the AI automation is configured to auto-categorize the prayer request.
ClassifySentiment - True if the AI automation is configured to classify the sentiment of the prayer request.
CheckAppropriateness - True if the AI automation is configured to check if prayer request is appropriate for public viewing.
Categories - The child categories of the currently selected Prayer Request category or children of ""All Church"" if no category is selected.
SentimentEmotions - The Sentiment Emotions DefinedType with DefinedValues from Cache (well-known Guid: C9751C20-DA81-4521-81DE-0099D6F598BA).
{%- endcomment -%}
{%- if AutoCategorize == true and Categories != empty %}
Choose the Id of the category that most closely matches the main theme of the prayer request.
{%- assign categoriesJson = '[' -%}
{%- for category in Categories -%}
    {%- capture categoriesJsonRow -%}
        {
            ""Id"": {{ category.Id }},
            ""CategoryName"": {{category.Name | ToJSON }}
        }{% unless forloop.last %},{% endunless %}
    {%- endcapture -%}
    {%- assign categoriesJson = categoriesJson | Append:categoriesJsonRow -%}
{%- endfor -%}
{%- assign categoriesJson = categoriesJson | Append: ']' %}
```Categories```
{{ categoriesJson | FromJSON | ToJSON }}
```Categories```
{% endif -%}

{%- if ClassifySentiment == true %}
Choose the Id of the sentiment that most closely matches the prayer request text.
{%- assign sentimentsJson = '[' -%}
{%- for definedValue in SentimentEmotions.DefinedValues -%}
    {%- capture sentimentsJsonRow -%}
        {
            ""Id"": {{ definedValue.Id }},
            ""Sentiment"": {{ definedValue.Value | ToJSON }}
        }{% unless forloop.last %},{% endunless %}
    {%- endcapture -%}
    {%- assign sentimentsJson = sentimentsJson | Append:sentimentsJsonRow -%}
{%- endfor -%}
{%- assign sentimentsJson = sentimentsJson | Append: ']' %}
```Sentiments```
{{ sentimentsJson | FromJSON | ToJSON }}
```Sentiments```
{% endif -%}
{%- if CheckAppropriateness == true -%}
Determine if the prayer request text is appropriate for public viewing being sensitive to privacy and legal concerns.
First names alone are ok, but pay attention to other details which might make it easy to uniquely identify an individual within a community.
{%- endif %}

```Prayer Request```
{{PrayerRequest.Text}}
```Prayer Request```
Respond with ONLY a VALID JSON object in the format below. Do not use backticks ```.
{
""sentimentId"": <The Id of the Sentiment from the list delimited by ```Sentiments``` that most closely matches the main theme of the prayer request text>,
""categoryId"": <The Id of the Category from the list delimited by ```Categories``` that most closely matches the main theme of the prayer request text>,
""isAppropriateForPublic"": <boolean value indicating whether the prayer request text is appropriate for public viewing>
}
"
            };
        }

        #endregion

        #region: KA Update Giving Households Metric

        private void UpdateGivingHouseholdsMetricsUp()
        {
            Sql( @"
UPDATE [dbo].[Metric]
SET [SourceSql] = 'DECLARE @StartDate int = FORMAT( DATEADD( d, -365, GETDATE()), ''yyyyMMdd'' )
DECLARE @EndDate int = FORMAT( GETDATE(), ''yyyyMMdd'' )
DECLARE @PersonRecordTypeId INT = ( SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E'' )

;WITH CTE AS (
    SELECT
	[PrimaryCampusId]
    , [PrimaryFamilyId]
    , SUM(ftd.[Amount]) AS [GivingAmount]
    , (SELECT TOP 1 
            LEFT([PostalCode], 5)
        FROM [dbo].[Location] l
        INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMappedLocation] = 1
      ) AS [PostalCode]
    , (SELECT [FamiliesMedianIncome] * .1 FROM [dbo].[AnalyticsSourcePostalCode] WHERE [PostalCode] = (SELECT TOP 1 LEFT([PostalCode], 5) FROM [dbo].[Location] l INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMappedLocation] = 1) ) AS [FamiliesMedianTithe]
	FROM
		[Person] p
		INNER JOIN [dbo].[PersonAlias] pa ON pa.[PersonId] = p.[Id]
		INNER JOIN [dbo].[FinancialTransaction] ft ON ft.[AuthorizedPersonAliasId] = pa.[Id]
		INNER JOIN [dbo].[FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
		INNER JOIN [dbo].[FinancialAccount] fa ON fa.[Id] = ftd.[AccountId] 
		INNER JOIN [dbo].[AnalyticsSourceDate] asd ON asd.[DateKey] = ft.[TransactionDateKey]
	WHERE
		fa.[IsTaxDeductible] = 1
		AND asd.[DateKey] > = @StartDate AND asd.[DateKey] <= @EndDate
AND p.[RecordTypeValueId] = @PersonRecordTypeId
	GROUP BY [PrimaryCampusId], [PrimaryFamilyId]
)
SELECT
    SUM(CASE WHEN [GivingAmount] >= 0 THEN 1 ELSE 0 END) AS [TotalGivingHouseholds]
    , PrimaryCampusId AS [CampusId]
FROM CTE
WHERE (PostalCode IS NOT NULL AND PostalCode != '''') and ([FamiliesMedianTithe] is NOT NULL and [FamiliesMedianTithe] > 0)
GROUP BY PrimaryCampusId;'
WHERE [Guid] = 'B5BFAB51-9B46-4E7E-992E-B0119E4D25EC'
" );

            Sql( @"
UPDATE [dbo].[Metric]
SET [SourceSql] = 'DECLARE @StartDate int = FORMAT( DATEADD( d, -365, GETDATE()), ''yyyyMMdd'' )
DECLARE @EndDate int = FORMAT( GETDATE(), ''yyyyMMdd'' )
DECLARE @PersonRecordTypeId INT = ( SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E'' )

;WITH CTE AS (
    SELECT
    [PrimaryCampusId]
    , [PrimaryFamilyId]
    , SUM(ftd.[Amount]) AS [GivingAmount]
    , (SELECT TOP 1 
            LEFT([PostalCode], 5)
        FROM [dbo].[Location] l
        INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMappedLocation] = 1
      ) AS [PostalCode]
    , (SELECT [FamiliesMedianIncome] * .1 FROM [dbo].[AnalyticsSourcePostalCode] WHERE [PostalCode] = (SELECT TOP 1 LEFT([PostalCode], 5) FROM [dbo].[Location] l INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMappedLocation] = 1) ) AS [FamiliesMedianTithe]
	FROM
		[Person] p
		INNER JOIN [dbo].[PersonAlias] pa ON pa.[PersonId] = p.[Id]
		INNER JOIN [dbo].[FinancialTransaction] ft ON ft.[AuthorizedPersonAliasId] = pa.[Id]
		INNER JOIN [dbo].[FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
		INNER JOIN [dbo].[FinancialAccount] fa ON fa.[Id] = ftd.[AccountId] 
		INNER JOIN [dbo].[AnalyticsSourceDate] asd ON asd.[DateKey] = ft.[TransactionDateKey]
	WHERE
		fa.[IsTaxDeductible] = 1
		AND asd.[DateKey] > = @StartDate AND asd.[DateKey] <= @EndDate
AND p.[RecordTypeValueId] = @PersonRecordTypeId
	GROUP BY [PrimaryCampusId], [PrimaryFamilyId]
)
SELECT
    SUM(CASE WHEN [GivingAmount] >= [FamiliesMedianTithe] THEN 1 ELSE 0 END) AS [TotalTithingHouseholds]
    , PrimaryCampusId AS [CampusId]
FROM CTE
WHERE (PostalCode IS NOT NULL AND PostalCode != '''') and ([FamiliesMedianTithe] is NOT NULL and [FamiliesMedianTithe] > 0)
GROUP BY PrimaryCampusId;'
WHERE [Guid] = '2B798177-E8F4-46DB-A1D7-308D63CA519A'
" );

            Sql( @"
UPDATE [dbo].[Metric]
SET [SourceSql] = 'DECLARE @StartDate int = FORMAT( DATEADD( d, -365, GETDATE()), ''yyyyMMdd'' )
DECLARE @EndDate int = FORMAT( GETDATE(), ''yyyyMMdd'' )
-- Only Include Person Type Records
DECLARE @PersonRecordTypeId INT = ( SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E'' )

;WITH CTE AS (
    SELECT
    [PrimaryCampusId]
    , [PrimaryFamilyId]
    , SUM(ftd.[Amount]) AS [GivingAmount]
    , (SELECT TOP 1 
            LEFT([PostalCode], 5)
        FROM [dbo].[Location] l
        INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMappedLocation] = 1
      ) AS [PostalCode]
    , (SELECT [FamiliesMedianIncome] * .1 FROM [dbo].[AnalyticsSourcePostalCode] WHERE [PostalCode] = (SELECT TOP 1 LEFT([PostalCode], 5) FROM [dbo].[Location] l INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMappedLocation] = 1) ) AS [FamiliesMedianTithe]
	FROM
		[Person] p
		INNER JOIN [dbo].[PersonAlias] pa ON pa.[PersonId] = p.[Id]
		INNER JOIN [dbo].[FinancialTransaction] ft ON ft.[AuthorizedPersonAliasId] = pa.[Id]
		INNER JOIN [dbo].[FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
		INNER JOIN [dbo].[FinancialAccount] fa ON fa.[Id] = ftd.[AccountId] 
		INNER JOIN [dbo].[AnalyticsSourceDate] asd ON asd.[DateKey] = ft.[TransactionDateKey]
	WHERE
		fa.[IsTaxDeductible] = 1
		AND asd.[DateKey] > = @StartDate AND asd.[DateKey] <= @EndDate
-- Only include person type records.
AND p.[RecordTypeValueId] = @PersonRecordTypeId
	GROUP BY [PrimaryCampusId], [PrimaryFamilyId]
)
SELECT 
    CAST(COUNT(CASE WHEN [GivingAmount] > [FamiliesMedianTithe] THEN 1 END) AS FLOAT) / COUNT(*) * 100 AS PercentageAboveMedianTithe,
    [PrimaryCampusId],
    COUNT(*) AS TotalFamilies,
    COUNT(CASE WHEN [GivingAmount] > [FamiliesMedianTithe] THEN 1 END) AS FamiliesAboveMedianTithe
FROM 
    CTE
-- Only include families that have a postal code and/or we have a [FamiliesMedianIncome] value
WHERE (PostalCode IS NOT NULL AND PostalCode != '''') and ([FamiliesMedianTithe] is NOT NULL and [FamiliesMedianTithe] > 0)
GROUP BY [PrimaryCampusId];'
WHERE [Guid] = 'F4951A42-9F71-4CB1-A46E-2A7ED84CD923'
" );
        }

        #endregion

        #region PA: Added Exception Log Filter Global Attribute

        private void AddExceptionLogFilterGlobalAttributeUp()
        {
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.KEY_VALUE_LIST, "", string.Empty, "Exception Log Filter", "Before logging an unhandled exception, Rock can evaluate the current client's HTTP Server variables and ignore/skip any that are from clients that have server variable values containing the values configured here. (Example: key: HTTP_USER_AGENT value: Googlebot)", 0, "", Guid.NewGuid().ToString(),
                Rock.SystemKey.GlobalAttributeKey.EXCEPTION_LOG_FILTER, false );
        }

        #endregion

        #region PA: Added Modal Layout and move Shortlink Block Obsidian to it

        private void AddModalLayoutUp()
        {
            RockMigrationHelper.AddLayout( SystemGuid.Site.SITE_ROCK_INTERNAL, "Modal", "Modal", "", SystemGuid.Layout.MODAL );

            RockMigrationHelper.UpdatePageLayout( "A9188D7A-80D9-4865-9C77-9F90E992B65C", SystemGuid.Layout.MODAL );
        }

        #endregion

        #region PA: Register block attributes for chop job in v1.17.0.28

        private void MigrationRollupsForV17_0_15_ChopBlocksUp()
        {
            MigrationRollupsForV17_0_15_RegisterBlockAttributesForChop();

            // Commenting out as the job has been updated in later migrations.
            // MigrationRollupsForV17_0_15_ChopBlockTypesv17();
        }

        private void MigrationRollupsForV17_0_15_RegisterBlockAttributesForChop()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.LocationList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.LocationList", "Location List", "Rock.Blocks.Core.LocationList, Rock.Blocks, Version=1.17.0.27, Culture=neutral, PublicKeyToken=null", false, false, "62622112-8375-44CF-957B-2B8FB4922C2B" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Group.GroupRequirementTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Group.GroupRequirementTypeList", "Group Requirement Type List", "Rock.Blocks.Group.GroupRequirementTypeList, Rock.Blocks, Version=1.17.0.27, Culture=neutral, PublicKeyToken=null", false, false, "C1D4FEC2-F868-4FE7-899F-62CCFBEB29C6" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Group.GroupRequirementTypeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Group.GroupRequirementTypeDetail", "Group Requirement Type Detail", "Rock.Blocks.Group.GroupRequirementTypeDetail, Rock.Blocks, Version=1.17.0.27, Culture=neutral, PublicKeyToken=null", false, false, "8A95BCF0-63CB-4CD6-99C9-E812D9AFAE99" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Crm.SignalTypeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.SignalTypeDetail", "Signal Type Detail", "Rock.Blocks.Crm.SignalTypeDetail, Rock.Blocks, Version=1.17.0.27, Culture=neutral, PublicKeyToken=null", false, false, "E7B94691-BB91-4995-B2A0-3C3724224250" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.FollowingSuggestionTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.FollowingSuggestionTypeList", "Following Suggestion Type List", "Rock.Blocks.Core.FollowingSuggestionTypeList, Rock.Blocks, Version=1.17.0.27, Culture=neutral, PublicKeyToken=null", false, false, "BD0594CB-7A1B-40D2-A3C7-D27CB7481511" );


            // Add/Update Obsidian Block Type
            //   Name:Location List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.LocationList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Location List", "Displays a list of locations.", "Rock.Blocks.Core.LocationList", "Core", "8A5AF4F4-32A2-426F-8363-57AC4F02A6F6" );

            // Add/Update Obsidian Block Type
            //   Name:Group Requirement Type List
            //   Category:Group
            //   EntityType:Rock.Blocks.Group.GroupRequirementTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Group Requirement Type List", "List of Group Requirement Types.", "Rock.Blocks.Group.GroupRequirementTypeList", "Group", "DA7834C6-C5C6-470B-B1C8-9AFA492151F8" );

            // Add/Update Obsidian Block Type
            //   Name:Group Requirement Type Detail
            //   Category:Group
            //   EntityType:Rock.Blocks.Group.GroupRequirementTypeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Group Requirement Type Detail", "Displays the details of the given group requirement type for editing.", "Rock.Blocks.Group.GroupRequirementTypeDetail", "Group", "C17B6D03-FDF3-4DD7-B9A9-3D6159A838F5" );

            // Add/Update Obsidian Block Type
            //   Name:Person Signal Type Detail
            //   Category:CRM
            //   EntityType:Rock.Blocks.Crm.SignalTypeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Person Signal Type Detail", "Shows the details of a particular person signal type.", "Rock.Blocks.Crm.SignalTypeDetail", "CRM", "8B65EE51-4075-4FC0-B1A9-F56C7153AA77" );

            // Add/Update Obsidian Block Type
            //   Name:Suggestion List
            //   Category:Follow
            //   EntityType:Rock.Blocks.Core.FollowingSuggestionTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Suggestion List", "Block for viewing list of following events.", "Rock.Blocks.Core.FollowingSuggestionTypeList", "Follow", "0C01496C-B4FD-4335-A11F-9B3D14D3C0E1" );


            // Attribute for BlockType
            //   BlockType: Location List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A5AF4F4-32A2-426F-8363-57AC4F02A6F6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the location details.", 0, @"", "EF5E30A6-8855-471F-8B82-25353D65C56A" );

            // Attribute for BlockType
            //   BlockType: Location List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A5AF4F4-32A2-426F-8363-57AC4F02A6F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "528C7391-5B8B-4BBA-8A5E-CAC5D3153FE0" );

            // Attribute for BlockType
            //   BlockType: Location List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A5AF4F4-32A2-426F-8363-57AC4F02A6F6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "EC607613-E22D-4DB0-B5C5-C9107D9F4A37" );

            // Attribute for BlockType
            //   BlockType: Group Requirement Type List
            //   Category: Group
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA7834C6-C5C6-470B-B1C8-9AFA492151F8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the group requirement type details.", 0, @"", "70790688-B494-4D58-92FC-2727BFA48A51" );

            // Attribute for BlockType
            //   BlockType: Group Requirement Type List
            //   Category: Group
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA7834C6-C5C6-470B-B1C8-9AFA492151F8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "6FECF863-2F24-4E6E-9D09-246AC035A752" );

            // Attribute for BlockType
            //   BlockType: Group Requirement Type List
            //   Category: Group
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA7834C6-C5C6-470B-B1C8-9AFA492151F8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "1A9B8368-70D4-4E26-A803-555C293AC335" );
            // Attribute for BlockType
            //   BlockType: Suggestion List
            //   Category: Follow
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0C01496C-B4FD-4335-A11F-9B3D14D3C0E1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the following suggestion type details.", 0, @"", "E137AADA-0039-4A32-BCA6-093505BC521E" );

            // Attribute for BlockType
            //   BlockType: Suggestion List
            //   Category: Follow
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0C01496C-B4FD-4335-A11F-9B3D14D3C0E1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "6D5576D1-7465-4AD2-AC21-BB1983B380E8" );

            // Attribute for BlockType
            //   BlockType: Suggestion List
            //   Category: Follow
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0C01496C-B4FD-4335-A11F-9B3D14D3C0E1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "C01EEADC-F78F-4442-A1C6-E2AC53D2B049" );
        }

        // PA: Chop blocks for v1.17.0.28
        private void MigrationRollupsForV17_0_15_ChopBlockTypesv17()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types - 1.17.0.28",
                blockTypeReplacements: new Dictionary<string, string> {
{ "08189564-1245-48F8-86CC-560F4DD48733", "D0203B97-5856-437E-8700-8846309F8EED" }, // Location Detail
{ "5144ED5B-89A9-4D77-B0E5-695070BE0C8E", "8A5AF4F4-32A2-426F-8363-57AC4F02A6F6" }, // Location List
{ "92E4BFE8-DF80-49D7-819D-417E579E282D", "C0CFDAB7-BB29-499E-BD0A-468B0856C037" }, // Registration List Lava
{ "1270E3F7-5ACB-4044-94CD-E2B4368FF391", "DA7834C6-C5C6-470B-B1C8-9AFA492151F8" }, // Group Requirement Type List
{ "68FC983E-05F0-4067-83AC-97DD226F5071", "C17B6D03-FDF3-4DD7-B9A9-3D6159A838F5" }, // Group Requirement Type Detail
{ "E9AB79D9-429F-410D-B4A8-327829FC7C63", "8B65EE51-4075-4FC0-B1A9-F56C7153AA77" }, // Person Signal Type Detail
{ "23818F47-D81E-4B6E-B89B-045B1FAD4C2B", "0C01496C-B4FD-4335-A11F-9B3D14D3C0E1" }, // Suggestion List

                    // blocks chopped in v1.17.0.27
{ "052B84EA-0C34-4A07-AC4C-1FBCEC87C223", "E18AB976-6665-48A5-B418-8FAC8F374135" }, // Suggestion Detail
{ "14293AEB-B0F5-434B-844A-66592AE3A416", "7E2DFB55-F1AB-4452-A5DF-6CE65FBFDDAD" }, // Photo Opt-Out
{ "0DE16268-BD5B-4CFC-A7C6-F1E07F47527A", "F61C0FDF-E8A0-457A-B8AF-42CAC8A18718" }, // Benevolence Type List
{ "C96479B6-E309-4B1A-B024-1F1276122A13", "03397615-EF2B-4D33-BD62-A79186F56ACE" }, // Benevolence Type Detail
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_CHOP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: new Dictionary<string, string> {
                { "92E4BFE8-DF80-49D7-819D-417E579E282D", "LimitToOwed,MaxResults" }
            } );
        }

        #endregion

        #region PA: Swap blocks for v1.17.0.28

        private void SwapBlocksUp()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                    "Swap Block Types - 1.17.0.28",
                    blockTypeReplacements: new Dictionary<string, string> {
    { "37D43C21-1A4D-4B13-9555-EF0B7304EB8A", "511D8E2E-4AF3-48D8-88EF-2AB311CD47E0" }, // Group Scheduler
                    },
                    migrationStrategy: "Swap",
                    jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_SWAP_OBSIDIAN_BLOCKS,
                    blockAttributeKeysToIgnore: new Dictionary<string, string>{
    { "37D43C21-1A4D-4B13-9555-EF0B7304EB8A",  "FutureWeeksToShow" }
                } );
        }

        #endregion

        #endregion

        #region MigrationRollupsForV17_0_16

        #region KA: Migration to append 'Legacy' to StarkDetail and StarkList block name

        private void UpdateStarkListAndStarkDetailBlockNameUp()
        {
            Sql( @"
UPDATE BlockType
SET [Name] = 'Stark Detail (Legacy)'
WHERE GUID = 'D6B14847-B652-49E2-9D4B-658D502F0AEC'
" );

            Sql( @"
UPDATE BlockType
SET [Name] = 'Stark List (Legacy)'
WHERE GUID = 'E333D1CC-CB55-4E73-8568-41DAD296971C'
" );
        }

        private void UpdateStarkListAndStarkDetailBlockNameDown()
        {
            Sql( @"
UPDATE BlockType
SET [Name] = 'Stark Detail'
WHERE GUID = 'D6B14847-B652-49E2-9D4B-658D502F0AEC'
" );

            Sql( @"
UPDATE BlockType
SET [Name] = 'Stark List'
WHERE GUID = 'E333D1CC-CB55-4E73-8568-41DAD296971C'
" );
        }

        #endregion

        #region KA: Migration to add Obsidian control gallery page to CMS

        private void AddObsidianControlGalleryToCMSUp()
        {
            // Add Page to the CMS Page
            RockMigrationHelper.AddPage( true, "B4A24AB7-9369-4055-883F-4F4892C39AE3", "C2467799-BB45-4251-8EE6-F0BF27201535", "Obsidian Control Gallery", "", guid: "0CD9A871-2F7D-4713-8FBB-0D8EDDE02886", iconCssClass: "fa fa-magic", insertAfterPageGuid: "706C0584-285F-4014-BA61-EC42C8F6F76B" );

            // Add custom route
            RockMigrationHelper.AddOrUpdatePageRoute( "0CD9A871-2F7D-4713-8FBB-0D8EDDE02886", "admin/cms/control-gallery-obsidian", "B50D4D65-A7BA-40E3-AD87-00F5CEC6A874" );

            // Add Obsidian Control Gallery Block to page
            RockMigrationHelper.AddBlock( true, "0CD9A871-2F7D-4713-8FBB-0D8EDDE02886", "", "6FAB07FF-D4C6-412B-B13F-7B881ECBFAD0", "Obsidian Control Gallery", "Main", "", "", 0, "6150D7FB-08FC-4A4E-BCBF-359F779A5A93" );
        }

        private void AddObsidianControlGalleryToCMSDown()
        {
            RockMigrationHelper.DeleteBlock( "6150D7FB-08FC-4A4E-BCBF-359F779A5A93" );
            RockMigrationHelper.DeletePageRoute( "B50D4D65-A7BA-40E3-AD87-00F5CEC6A874" );
            RockMigrationHelper.DeletePage( "0CD9A871-2F7D-4713-8FBB-0D8EDDE02886" );
        }

        #endregion

        #region PA: Swap blocks for v1.17.0.29

        private void MigrationRollupsForV17_0_16_SwapBlocksUp()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Swap Block Types - 1.17.0.29",
                blockTypeReplacements: new Dictionary<string, string> {
{ "37D43C21-1A4D-4B13-9555-EF0B7304EB8A", "511D8E2E-4AF3-48D8-88EF-2AB311CD47E0" }, // Group Scheduler
                },
                migrationStrategy: "Swap",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_SWAP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: new Dictionary<string, string>{
{ "37D43C21-1A4D-4B13-9555-EF0B7304EB8A",  "FutureWeeksToShow" }
            } );
        }

        #endregion

        #region PA: Register block attributes for chop job in v1.17.0.29

        private void MigrationRollupsForV17_0_16_ChopBlocksUp()
        {
            MigrationRollupsForV17_0_16_RegisterBlockAttributesForChop();
            //MigrationRollupsForV17_0_16_ChopBlockTypesv17();
        }

        private void MigrationRollupsForV17_0_16_RegisterBlockAttributesForChop()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.NoteTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.NoteTypeList", "Note Type List", "Rock.Blocks.Core.NoteTypeList, Rock.Blocks, Version=1.17.0.28, Culture=neutral, PublicKeyToken=null", false, false, "CA07CFE0-AC86-4AD5-A4E2-03A90B0281F5" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.AttributeMatrixTemplateDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.AttributeMatrixTemplateDetail", "Attribute Matrix Template Detail", "Rock.Blocks.Core.AttributeMatrixTemplateDetail, Rock.Blocks, Version=1.17.0.28, Culture=neutral, PublicKeyToken=null", false, false, "86759D9B-281C-4C1B-95E6-D4305731C03B" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.PersonalLinkList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.PersonalLinkList", "Personal Link List", "Rock.Blocks.Cms.PersonalLinkList, Rock.Blocks, Version=1.17.0.28, Culture=neutral, PublicKeyToken=null", false, false, "06F055E8-D396-4AD6-B542-342EE5907D74" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Bus.ConsumerList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Bus.ConsumerList", "Consumer List", "Rock.Blocks.Bus.ConsumerList, Rock.Blocks, Version=1.17.0.28, Culture=neutral, PublicKeyToken=null", false, false, "444BD66E-A715-4367-A3A6-5C0BBD6E93B4" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.FinancialStatementTemplateDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialStatementTemplateDetail", "Financial Statement Template Detail", "Rock.Blocks.Finance.FinancialStatementTemplateDetail, Rock.Blocks, Version=1.17.0.28, Culture=neutral, PublicKeyToken=null", false, false, "FEEA3B29-3FCE-4216-AB28-E1F69C67A574" );



            // Add/Update Obsidian Block Type
            //   Name:Note Type List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.NoteTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Note Type List", "Displays a list of note types.", "Rock.Blocks.Core.NoteTypeList", "Core", "23E3CA31-6A1F-43CB-AC06-374BD9CB9FA5" );

            // Add/Update Obsidian Block Type
            //   Name:Attribute Matrix Template Detail
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.AttributeMatrixTemplateDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Attribute Matrix Template Detail", "Displays the details of a particular attribute matrix template.", "Rock.Blocks.Core.AttributeMatrixTemplateDetail", "Core", "96C5DF9E-6F5C-4E55-92F1-61FE16A18563" );

            // Add/Update Obsidian Block Type
            //   Name:Personal Link List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.PersonalLinkList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Personal Link List", "Displays a list of personal links.", "Rock.Blocks.Cms.PersonalLinkList", "CMS", "6C9E7EBF-8F27-48EF-94C4-900AC3A2C167" );

            // Add/Update Obsidian Block Type
            //   Name:Consumer List
            //   Category:Bus
            //   EntityType:Rock.Blocks.Bus.ConsumerList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Consumer List", "Displays a list of consumers.", "Rock.Blocks.Bus.ConsumerList", "Bus", "63F5509A-3D71-4F0F-A074-FA5869856038" );

            // Add/Update Obsidian Block Type
            //   Name:Financial Statement Template Detail
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.FinancialStatementTemplateDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Financial Statement Template Detail", "Displays the details of the statement template.", "Rock.Blocks.Finance.FinancialStatementTemplateDetail", "Finance", "3D13455F-7E5C-46F7-975A-4A5CE12BD330" );


            // Attribute for BlockType
            //   BlockType: Note Type List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23E3CA31-6A1F-43CB-AC06-374BD9CB9FA5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the note type details.", 0, @"", "EDE4E14E-C251-4B42-BF04-9D4B017950FB" );

            // Attribute for BlockType
            //   BlockType: Note Type List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23E3CA31-6A1F-43CB-AC06-374BD9CB9FA5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "7BF8AE56-BE37-4F6B-BC33-35E78DCE9C1F" );

            // Attribute for BlockType
            //   BlockType: Note Type List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23E3CA31-6A1F-43CB-AC06-374BD9CB9FA5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "E8A3D7B0-09BF-4C66-A439-89B16D105A0A" );

            // Attribute for BlockType
            //   BlockType: Personal Link List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C9E7EBF-8F27-48EF-94C4-900AC3A2C167", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "F8B7B83F-2380-4198-A08D-6603AEBABB8D" );

            // Attribute for BlockType
            //   BlockType: Personal Link List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C9E7EBF-8F27-48EF-94C4-900AC3A2C167", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F3E689B0-6401-45E3-AECB-4657C752268E" );

        }

        // PA: Chop blocks for v1.17.0.29
        private void MigrationRollupsForV17_0_16_ChopBlockTypesv17()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types - 1.17.0.29",
                blockTypeReplacements: new Dictionary<string, string> {
{ "616D1A98-067D-43B8-B7F5-41FB12FB894E", "53A34D60-31B8-4D22-BC42-E3B669ED152B" }, // Auth Client List
{ "312EAD0E-4068-4211-8410-2EB45B7D8BAB", "8246EF8B-27E9-449E-9CAB-1C267B31DBC2" }, // Auth Client Detail
{ "7EFD5D30-2FF0-4C75-86A2-984A8F45D8A5", "63F5509A-3D71-4F0F-A074-FA5869856038" }, // Consumer List
{ "D4262E61-9CB2-4FF0-A7CA-90BAD1141BF5", "96C5DF9E-6F5C-4E55-92F1-61FE16A18563" }, // Attribute Matrix Template Detail
{ "BEC5B592-9E9E-4C55-BD0D-2B8065A1802E", "23E3CA31-6A1F-43CB-AC06-374BD9CB9FA5" }, // Note Type List
{ "5DA1D088-2142-4645-AF9C-EF52DA5B4EEA", "9E901A5A-82C2-4788-9623-3720FFC4DAEC" }, // Note Type Detail
{ "3C9D442B-D066-43FA-9380-98C60936992E", "662AF7BB-5B61-43C6-BDA6-A6E7AAB8FC00" }, // Media Folder Detail
{ "ACAF8CEB-18CD-4BAE-BF6A-12C08CF6D61F", "6035AC10-07A5-4EDD-A1E9-10862FC41494" }, // Persisted Dataset Detail
{ "E7546752-C3DC-4B96-88D9-A431F2D1C989", "6C9E7EBF-8F27-48EF-94C4-900AC3A2C167" }, // Personal Link List
{ "78DB9184-97CF-4FC4-BD71-8F3ABE4100BA", "3D13455F-7E5C-46F7-975A-4A5CE12BD330" }, // Financial Statement Template Detail
{ "49F3D87E-BD8D-43D4-8217-340F3DFF4562", "CDAB601D-1369-44CB-A146-4E80C7D66BCD" }, // Apple TV App Detail
{ "5D58BF6A-3914-420C-9013-53CE8A15E390", "A8062FE5-5BCD-48AC-8C37-2124462656A7" }, // Workflow Trigger Detail
{ "005E5980-E2D2-4958-ACB6-BECBC6D1F5C4", "F140B415-9BB3-4492-844E-5A529517A484" }, // Tag Report


                    // blocks chopped in v1.17.0.28
{ "08189564-1245-48F8-86CC-560F4DD48733", "D0203B97-5856-437E-8700-8846309F8EED" }, // Location Detail
{ "5144ED5B-89A9-4D77-B0E5-695070BE0C8E", "8A5AF4F4-32A2-426F-8363-57AC4F02A6F6" }, // Location List
{ "92E4BFE8-DF80-49D7-819D-417E579E282D", "C0CFDAB7-BB29-499E-BD0A-468B0856C037" }, // Registration List Lava
{ "1270E3F7-5ACB-4044-94CD-E2B4368FF391", "DA7834C6-C5C6-470B-B1C8-9AFA492151F8" }, // Group Requirement Type List
{ "68FC983E-05F0-4067-83AC-97DD226F5071", "C17B6D03-FDF3-4DD7-B9A9-3D6159A838F5" }, // Group Requirement Type Detail
{ "E9AB79D9-429F-410D-B4A8-327829FC7C63", "8B65EE51-4075-4FC0-B1A9-F56C7153AA77" }, // Person Signal Type Detail
{ "23818F47-D81E-4B6E-B89B-045B1FAD4C2B", "0C01496C-B4FD-4335-A11F-9B3D14D3C0E1" }, // Suggestion List
                    // blocks chopped in v1.17.0.27
{ "052B84EA-0C34-4A07-AC4C-1FBCEC87C223", "E18AB976-6665-48A5-B418-8FAC8F374135" }, // Suggestion Detail
{ "14293AEB-B0F5-434B-844A-66592AE3A416", "7E2DFB55-F1AB-4452-A5DF-6CE65FBFDDAD" }, // Photo Opt-Out
{ "0DE16268-BD5B-4CFC-A7C6-F1E07F47527A", "F61C0FDF-E8A0-457A-B8AF-42CAC8A18718" }, // Benevolence Type List
{ "C96479B6-E309-4B1A-B024-1F1276122A13", "03397615-EF2B-4D33-BD62-A79186F56ACE" }, // Benevolence Type Detail
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_CHOP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: new Dictionary<string, string> {
                { "92E4BFE8-DF80-49D7-819D-417E579E282D", "LimitToOwed,MaxResults" }, // Registration List Lava
                { "BEC5B592-9E9E-4C55-BD0D-2B8065A1802E", "EntityType" }, // Note Type List
            } );
        }

        #endregion

        #endregion

        #region LMSPageChanges

        private void RemoveLegacyValues()
        {
            Sql( $@"
-- Remove the Legacy DisplayMode Attribute before adding the new one.
DECLARE @programDetailBlockTypeId NVARCHAR(200) = (
	select [Id] 
	from BlockType 
	where [Guid] = '796c87e7-678f-4a38-8c04-a401a4f7ac21'
);

DECLARE @legacyDisplayModeAttributeId INT = (
	select a.Id
	from Attribute a
	where [Key] = 'DisplayMode'
		AND EntityTypeQualifierColumn = 'BlockTypeId'
		AND EntityTypeQualifierValue = @programDetailBlockTypeId
		AND Description = 'Select ''Summary'' to show only attributes that are ''Show on Grid''. Select ''Full'' to show all attributes.'
);

DELETE AttributeValue WHERE AttributeId = @legacyDisplayModeAttributeId
DELETE Attribute WHERE Id = @legacyDisplayModeAttributeId
" );
        }

        private void AttributeValuesUp()
        {
            Sql( $@"
-- Well-known Guids.
DECLARE 
	-- New summary and detail page roots (for sub-menu).
	@summaryPageRootGuid NVARCHAR(40) = '7ECE252B-6844-474C-AEFD-307E1DDA3A83',
	@detailPageRootGuid NVARCHAR(40) = '6BDF3243-72BA-4C08-80BD-B76E40667A33',

	-- The new Completions page guid for removing breadcrumbs.
	@programCompletionsPageGuid NVARCHAR(40) = '395BE5DD-E524-4B75-A4CA-5A0548645647',

	-- The new Courses list page guid for identifying the new home of the course detail page.
	@coursesPageGuid NVARCHAR(40) = '0E5103B8-EF4A-46C9-8F76-313A259B0A3C',
	
	-- The new Semesters list page guid for identifying the new home of the Semester Detail
	@semestersPageGuid NVARCHAR(40) = '0D89CFE6-BA23-4AC0-AF95-1016BAEF734C',

	-- Program List page and block type for identifying the AttributeValue to update
	-- for the grid click detail page.
	@programListPageGuid NVARCHAR(40) = '84DBEC51-EE0B-41C2-94B3-F361C4B98879',
	@programListBlockTypeGuid NVARCHAR(40) = '7b1db013-a552-455f-a1d0-7b17488d0d5c',

	-- Classes page and route to use for updating program list block attribute (detail page)
	@currentClassesPageGuid NVARCHAR(40) = '56459D93-32DF-4151-8F6D-003B9AFF0F94',
	@summaryClassesRouteGuid NVARCHAR(40) = '56A1387A-DDDE-46D9-A23D-B19D6A3BFC50',

	-- Course detail page to be moved
	@courseDetailPageGuid NVARCHAR(40) = 'A57D990E-6F34-45CF-ABAA-08C40E8D4844',

	-- Legacy pages to be removed.
	@legacyProgramDetailPageGuid NVARCHAR(40) = '7888caf4-af5d-44ba-ab9e-80138361f69d',
	@legacyCoursesPageGuid NVARCHAR(40) = '870318D8-5381-4B3C-BE4A-04E57125B590',
	@legacySemestersPageGuid NVARCHAR(40) = 'F7073393-D3A7-4C2E-8001-A73F9E169D60',
	@legacyProgramCompletionsPageGuid NVARCHAR(40) = '532BC5A9-40B3-42AF-9AD3-740FC0B3EB41',
	
	-- Detail necessary for adding Content Page and Announcement attribute values
	-- to Course Detail and Class Detail blocks.
	@classDetailPageGuid NVARCHAR(40) = '23D5076E-C062-4987-9985-B3A4792BF3CE',
	@blockEntityTypeId NVARCHAR(40) = (SELECT [Id] FROM [dbo].[EntityType] WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65'),
	@classDetailBlockGuid NVARCHAR(40) = 'C67D2164-33E5-46C0-94EF-DF387EF8477D',
	@courseDetailBlockGuid NVARCHAR(40) = 'D85084D3-E298-4307-9AA2-C1570C4A3FA4',

	@contentPageGuid NVARCHAR(40) = 'E6A89360-B7B4-48A8-B799-39A27EAB6F36',
	@announcementPageGuid NVARCHAR(40) = '7C8134A4-524A-4C3D-BA4C-875FEE672850',

	@contentPageRouteGuid NVARCHAR(40) = '4F35DC5D-FBB5-4B10-91B4-BC1C6A7009E8',
	@announcementRouteGuid NVARCHAR(40) = '864A8615-AC20-4B3A-9D5F-087E92859AD1',

	@contentDetailPageAttributeValue NVARCHAR(100),
	@announcementPageAttributeValue NVARCHAR(100);

-- Include the route when setting the Program List block's DetailPage attribute value.
DECLARE @summaryClassesPageAndRouteAttributeValue NVARCHAR(MAX) =
    CONCAT(@currentClassesPageGuid, ',', @summaryClassesRouteGuid);

-- Get the Block.Id of the Program List Block for the AttributeValue.EntityId.
DECLARE @programListBlockId INT = (
	SELECT TOP 1 b.[Id]
	FROM [dbo].[Block] b
	JOIN [dbo].[Page] p ON p.[Id] = b.[PageId]
	JOIN [dbo].[BlockType] bt on bt.[Id] = b.[BlockTypeId]
	WHERE bt.[Guid] = @programListBlockTypeGuid
		AND p.[Guid] = @programListPageGuid
);

-- Only update the DetailPage Attribute if the value
-- exactly matches the originally configured value.
UPDATE av SET
	[Value] = @summaryClassesPageAndRouteAttributeValue
FROM Attribute a
JOIN AttributeValue av ON av.AttributeId = a.Id 
WHERE av.EntityId = @programListBlockId
	AND a.[Key] = 'DetailPage'
	AND av.[Value] = @legacyProgramDetailPageGuid

-- We've modified the layout and blocks and this detail page is no longer used.
-- Get the Id so we can update child pages to point to the new parent page 
-- before deleting the parent.
DECLARE @legacyProgramDetailPageId INT = (
	SELECT TOP 1 [Id]
	FROM [Page]
	WHERE [Guid] = @legacyProgramDetailPageGuid
);

-- The new layout has a page with a submenu - 
-- this is the page where the Courses sub-menu is selected.
DECLARE @programDetailCoursesPageId INT = (
	SELECT TOP 1 [Id]
	FROM [Page]
	WHERE [Guid] = @coursesPageGuid
);

-- Update the ParentPage for the children of the legacy program detail page.
UPDATE p SET
	ParentPageId = @programDetailCoursesPageId
FROM [Page] p
WHERE [ParentPageId] = @legacyProgramDetailPageId

-- We've modified the layout and blocks and this detail page is no longer used.
-- Get the Id so we can update child pages to point to the new parent page 
-- before deleting the parent.
DECLARE @legacySemestersPageId INT = (
	SELECT TOP 1 [Id]
	FROM [Page]
	WHERE [Guid] = @legacySemestersPageGuid
);

-- The new layout has a page with a submenu - 
-- this is the page where the Courses sub-menu is selected.
DECLARE @semestersPageId INT = (
	SELECT TOP 1 [Id]
	FROM [Page]
	WHERE [Guid] = @semestersPageGuid
);

-- Update the ParentPage for the legacy semester detail page.
UPDATE p SET
	ParentPageId = @semestersPageId
FROM [Page] p
WHERE [ParentPageId] = @legacySemestersPageId

DECLARE @coursesPageId INT = (
	SELECT TOP 1 [Id]
	FROM [Page]
	WHERE [Guid] = @coursesPageGuid
)

DECLARE @coursePageId INT = (
	SELECT TOP 1 [Id]
	FROM [Page]
	WHERE [Guid] = @courseDetailPageGuid
);

-- Re-home the Course Page to the new parent page under 
-- Program Detail Pages > Courses.
UPDATE p SET
	ParentPageId = @coursesPageId
FROM [Page] p
WHERE [Guid] = @courseDetailPageGuid

-- Delete the legacy pages.
DELETE [Page] WHERE [Guid] IN ( 
	@legacyCoursesPageGuid, 
	@legacyProgramCompletionsPageGuid, 
	@legacyProgramDetailPageGuid, 
	@legacySemestersPageGuid
);

-- Don't show breadcrumbs for sub-menu root pages 
-- or the pages available from the sub-menu.
UPDATE p SET
	p.BreadCrumbDisplayName = 0
FROM [Page] p
WHERE p.BreadCrumbDisplayName = 1
	AND p.[Guid] IN (
		@summaryPageRootGuid, 
		@detailPageRootGuid,
		@currentClassesPageGuid,
		@semestersPageGuid,
		@coursesPageGuid,
		@programCompletionsPageGuid
	)

-- Set the AttributeValues for the Content Detail and Announcement Detail block settings.
SELECT 
	@contentDetailPageAttributeValue = CONCAT(@contentPageGuid, ',', @contentPageRouteGuid),
	@announcementPageAttributeValue = CONCAT(@announcementPageGuid, ',', @announcementRouteGuid);

-- Add the block settings to both blocks (if they don't already exist).
 INSERT INTO [AttributeValue] (
     [IsSystem],
	 [AttributeId],
	 [EntityId],
     [Value],
     [Guid])
SELECT 0, 
	a.Id,
	b.Id,
	CASE 
		WHEN a.[Key] = 'ContentDetailPage' THEN @contentDetailPageAttributeValue
		WHEN a.[Key] = 'AnnouncementDetailPage' THEN @announcementPageAttributeValue
	END [AttributeValue],
	NEWID()
FROM [dbo].[Attribute] a
JOIN [dbo].[Block] b ON b.[Guid] IN (@classDetailBlockGuid, @courseDetailBlockGuid)
WHERE a.[Key] IN ('ContentDetailPage', 'AnnouncementDetailPage')
	AND NOT EXISTS (
		SELECT *
		FROM [dbo].[AttributeValue] av
		WHERE av.AttributeId = a.Id
			AND av.EntityId = b.Id
	)
" );
        }

        #endregion

        #region MigrationRollupsForV17_0_17

        #region KA: Migration to update Google Maps Lavashortcode

        private void UpdateGoogleMapsLavaShortcodeUp()
        {
            // Update Shortcode
            var markup = @"
{% capture singleQuote %}'{% endcapture %}
{% capture escapedQuote %}\'{% endcapture %}
{% assign apiKey = 'Global' | Attribute:'GoogleApiKey' %}
{% assign url = 'key=' | Append:apiKey %}
{% assign id = uniqueid | Replace:'-','' %}
{% assign mapId = 'Global' | Attribute:'core_GoogleMapId' %}

{% if apiKey == """" %}
    <div class=""alert alert-warning"">
        There is no Google API key defined. Please add your key under: 'Admin Tools > General Settings > Global Attributes > Google API Key'.
    </div>
{% endif %}

{% assign markerCount = markers | Size -%}

{% assign mapCenter = center | Trim %}
{% if mapCenter == """" and markerCount > 0 -%}
    {% assign centerPoint = markers | First %}
    {% assign mapCenter = centerPoint.location %}
{% endif %}

{% assign mapZoom = zoom | Trim %}
{% if mapZoom == """" %}
    {% if markerCount == 1 -%}
        {% assign mapZoom = '11' %}
    {% else %}
        {% assign mapZoom = '10' %}
    {% endif %}
{% endif %}

{% javascript id:'googlemapsapi' url:'{{ ""https://maps.googleapis.com/maps/api/js?libraries=marker&key="" | Append:apiKey }}' %}{% endjavascript %}

{% case markeranimation %}
{% when 'drop' %}
    {% assign markeranimation = 'drop' %}
{% when 'bounce' %}
    {% assign markeranimation = 'bounce' %}
{% else %}
    {% assign markeranimation = null %}
{% endcase %}

{% stylesheet %}

.{{ id }} {
    width: {{ width }};
}

#map-container-{{ id }} {
    position: relative;
}

#{{ id }} {
    height: {{ height }};
    overflow: hidden;
    padding-bottom: 22.25%;
    padding-top: 30px;
    position: relative;
}

@keyframes drop {
  0% {
    transform: translateY(-200px) scaleY(0.9);
    opacity: 0;
  }
  5% {
    opacity: 0.7;
  }
  50% {
    transform: translateY(0px) scaleY(1);
    opacity: 1;
  }
  65% {
    transform: translateY(-17px) scaleY(0.9);
    opacity: 1;
  }
  75% {
    transform: translateY(-22px) scaleY(0.9);
    opacity: 1;
  }
  100% {
    transform: translateY(0px) scaleY(1);
    opacity: 1;
  }
}

.drop {
  animation: drop 0.3s linear forwards .5s;
}

@keyframes bounce {
  0%, 20%, 50%, 80%, 100% {
    transform: translateY(0);
  }
  40% {
    transform: translateY(-30px);
  }
  60% {
    transform: translateY(-15px);
  }
}

.bounce {
  animation: bounce 2s infinite;
}

{% endstylesheet %}

<div class=""map-container {{ id }}"">
    <div id=""map-container-{{ id }}""></div>
    <div id=""{{ id }}""></div>
</div>	

<script>
    // create javascript array of marker info
    var markers{{ id }} = [
        {% for marker in markers -%}
            {% assign title = '' -%}
            {% assign content = '' -%}
            {% assign icon = '' -%}
            {% assign location = marker.location | Split:',' -%}
            {% if marker.title and marker.title != '' -%}
                {% assign title = marker.title | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.content != '' -%}
                {% assign content = marker.content | StripNewlines | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.icon and marker.icon != '' -%}
                {% assign icon = marker.icon -%}
            {% endif -%}
            [{{ location[0] }}, {{ location[1] }},'{{ title }}','{{ content }}','{{ icon }}'],
        {% endfor -%}
    ];

    //Set Map
    function initialize{{ id }}() {
        var bounds = new google.maps.LatLngBounds();
        var centerLatLng = new google.maps.LatLng( {{ mapCenter }} );
        if ( isNaN( centerLatLng.lat() ) || isNaN( centerLatLng.lng() ) ) {
            centerLatLng = null;
        };

        var mapOptions = {
            zoom: {{ mapZoom }},
            scrollwheel: {{ scrollwheel }},
            draggable: {{ draggable }},
            center: centerLatLng,
            mapTypeId: '{{ maptype }}',
            zoomControl: {{ showzoom }},
            mapTypeControl: {{ showmaptype }},
            gestureHandling: '{{ gesturehandling }}',
            streetViewControl: {{ showstreetview }},
            fullscreenControl: {{ showfullscreen }}
            {% if style and style.content != """" %}
                ,styles: {{ style.content | StripNewlines | Trim }}
            {% endif %},
	        mapId: '{{ mapId }}'
        }

        var map = new google.maps.Map(document.getElementById('{{ id }}'), mapOptions);
        var infoWindow = new google.maps.InfoWindow(), marker, i;
        // place each marker on the map  
        for( i = 0; i < markers{{ id }}.length; i++ ) {
            var position = new google.maps.LatLng(markers{{ id }}[i][0], markers{{ id }}[i][1]);
            bounds.extend(position);
	        var glyph = null;
            if (markers{{ id }}[i][4] != ''){
            	glyph = markers{{ id }}[i][4];
            }
            var pin = new google.maps.marker.PinElement({
                background: '#FE7569',
                borderColor: '#000',
                scale: 1,
                glyph: glyph
            });
            marker = new google.maps.marker.AdvancedMarkerElement({
                position: position,
                map: map,
                title: markers{{ id }}[i][2],
                content: pin.element
            });

	        const content = marker.content;

    	    {% if markeranimation -%}
            // Drop animation should be onetime so remove class once animation ends.
		        {% if markeranimation == 'drop' -%}
                    content.style.opacity = ""0"";
		            content.addEventListener('animationend', (event) => {
                        content.classList.remove('{{ markeranimation }}');
                        content.style.opacity = ""1"";
                    });
                {% endif -%}
                content.classList.add('{{ markeranimation }}');
            {% endif -%}

            // Add info window to marker
            google.maps.event.addListener(marker, 'click', (function(marker, i) {
                return function() {
                    if (markers{{ id }}[i][3] != ''){
                        infoWindow.setContent(markers{{ id }}[i][3]);
                        infoWindow.open(map, marker);
                    }
                }
            })(marker, i));
        }

        // Center the map to fit all markers on the screen
        {% if zoom == """" and center == """" and markerCount > 1 -%}
            map.fitBounds(bounds);
        {% endif -%}

        // Resize Function
        google.maps.event.addDomListener(window, ""resize"", function() {
            var center = map.getCenter();
            if ( center ) {
                google.maps.event.trigger(map, ""resize"");
                map.setCenter(center);
            }
        });
    }

    google.maps.event.addDomListener(window, 'load', initialize{{ id }});

</script>
";

            var sql = @"
-- Update Shortcode: Google Maps
UPDATE [dbo].[LavaShortcode] SET [Markup]=N'$markup'
WHERE ([Guid]='FE298210-1307-49DF-B28B-3735A414CCA0')
";

            markup = markup.Replace( "'", "''" ).Trim();
            sql = sql.Replace( "$markup", markup );
            Sql( sql );

            // Add MapId attribute
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.TEXT,
                null,
                null,
                "Google Maps Id",
                "The map identifier that's associated with a specific map style or feature on your google console, when you reference a map ID, its associated map style is displayed in your map.",
                0,
                "DEFAULT_MAP_ID",
                "9CE0FE85-CE25-4DBA-92B7-70D480E23BA8",
                "core_GoogleMapId" );
        }

        private void UpdateGoogleMapsLavaShortcodeDown()
        {
            var markup = @"
{% capture singleQuote %}'{% endcapture %}
{% capture escapedQuote %}\'{% endcapture %}
{% assign apiKey = 'Global' | Attribute:'GoogleApiKey' %}
{% assign url = 'key=' | Append:apiKey %}
{% assign id = uniqueid | Replace:'-','' %}

{% if apiKey == """" %}
    <div class=""alert alert-warning"">
        There is no Google API key defined. Please add your key under: 'Admin Tools > General Settings > Global Attributes > Google API Key'.
    </div>
{% endif %}

{% assign markerCount = markers | Size -%}

{% assign mapCenter = center | Trim %}
{% if mapCenter == """" and markerCount > 0 -%}
    {% assign centerPoint = markers | First %}
    {% assign mapCenter = centerPoint.location %}
{% endif %}

{% assign mapZoom = zoom | Trim %}
{% if mapZoom == """" %}
    {% if markerCount == 1 -%}
        {% assign mapZoom = '11' %}
    {% else %}
        {% assign mapZoom = '10' %}
    {% endif %}
{% endif %}

{% javascript id:'googlemapsapi' url:'{{ ""https://maps.googleapis.com/maps/api/js?key="" | Append:apiKey }}' %}{% endjavascript %}

{% case markeranimation %}
{% when 'drop' %}
    {% assign markeranimation = 'google.maps.Animation.DROP' %}
{% when 'bounce' %}
    {% assign markeranimation = 'google.maps.Animation.BOUNCE' %}
{% else %}
    {% assign markeranimation = 'null' %}
{% endcase %}

{% stylesheet %}

.{{ id }} {
    width: {{ width }};
}

#map-container-{{ id }} {
    position: relative;
}

#{{ id }} {
    height: {{ height }};
    overflow: hidden;
    padding-bottom: 22.25%;
    padding-top: 30px;
    position: relative;
}

{% endstylesheet %}

<div class=""map-container {{ id }}"">
    <div id=""map-container-{{ id }}""></div>
    <div id=""{{ id }}""></div>
</div>	

<script>
    // create javascript array of marker info
    var markers{{ id }} = [
        {% for marker in markers -%}
            {% assign title = '' -%}
            {% assign content = '' -%}
            {% assign icon = '' -%}
            {% assign location = marker.location | Split:',' -%}
            {% if marker.title and marker.title != '' -%}
                {% assign title = marker.title | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.content != '' -%}
                {% assign content = marker.content | StripNewlines | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.icon and marker.icon != '' -%}
                {% assign icon = marker.icon -%}
            {% endif -%}
            [{{ location[0] }}, {{ location[1] }},'{{ title }}','{{ content }}','{{ icon }}'],
        {% endfor -%}
    ];

    //Set Map
    function initialize{{ id }}() {
        var bounds = new google.maps.LatLngBounds();
        var centerLatLng = new google.maps.LatLng( {{ mapCenter }} );
        if ( isNaN( centerLatLng.lat() ) || isNaN( centerLatLng.lng() ) ) {
            centerLatLng = null;
        };

        var mapOptions = {
            zoom: {{ mapZoom }},
            scrollwheel: {{ scrollwheel }},
            draggable: {{ draggable }},
            center: centerLatLng,
            mapTypeId: '{{ maptype }}',
            zoomControl: {{ showzoom }},
            mapTypeControl: {{ showmaptype }},
            gestureHandling: '{{ gesturehandling }}',
            streetViewControl: {{ showstreetview }},
            fullscreenControl: {{ showfullscreen }}
            {% if style and style.content != """" %}
                ,styles: {{ style.content | StripNewlines | Trim }}
            {% endif %}
        }

        var map = new google.maps.Map(document.getElementById('{{ id }}'), mapOptions);
        var infoWindow = new google.maps.InfoWindow(), marker, i;

        // place each marker on the map  
        for( i = 0; i < markers{{ id }}.length; i++ ) {
            var position = new google.maps.LatLng(markers{{ id }}[i][0], markers{{ id }}[i][1]);
            bounds.extend(position);
            marker = new google.maps.Marker({
                position: position,
                map: map,
                animation: {{ markeranimation }},
                title: markers{{ id }}[i][2],
                icon: markers{{ id }}[i][4]
            });

            // Add info window to marker
            google.maps.event.addListener(marker, 'click', (function(marker, i) {
                return function() {
                    if (markers{{ id }}[i][3] != ''){
                        infoWindow.setContent(markers{{ id }}[i][3]);
                        infoWindow.open(map, marker);
                    }
                }
            })(marker, i));
        }

        // Center the map to fit all markers on the screen
        {% if zoom == """" and center == """" and markerCount > 1 -%}
            map.fitBounds(bounds);
        {% endif -%}

        // Resize Function
        google.maps.event.addDomListener(window, ""resize"", function() {
            var center = map.getCenter();
            if ( center ) {
                google.maps.event.trigger(map, ""resize"");
                map.setCenter(center);
            }
        });
    }

    google.maps.event.addDomListener(window, 'load', initialize{{ id }});

</script>
";

            var sql = @"
-- Update Shortcode: Google Maps
UPDATE [dbo].[LavaShortcode] SET [Markup]=N'$markup'
WHERE ([Guid]='FE298210-1307-49DF-B28B-3735A414CCA0')
";
            markup = markup.Replace( "'", "''" ).Trim();
            sql = sql.Replace( "$markup", markup );
            Sql( sql );

            RockMigrationHelper.DeleteAttribute( "9CE0FE85-CE25-4DBA-92B7-70D480E23BA8" );
        }

        #endregion

        #region KA: Migration to Update Metrics to use date component of DateTime for comparisons

        private void UpdateMetricsDateComponentUp()
        {
            // TOTAL WEEKEND ATTENDANCE
            Sql( @"
UPDATE [Metric]
SET [SourceSql] = 'DECLARE @STARTDATE DATETIME = CONVERT(date, DATEADD(DAY, -7, dbo.RockGetDate()))
DECLARE @ENDDATE DATETIME = CONVERT(date, dbo.RockGetDate())

SELECT COUNT(1) as AttendanceCount, a.[CampusId], oa.[ScheduleId]
FROM [Attendance] a
	INNER JOIN [AttendanceOccurrence] oa ON oa.Id = a.[OccurrenceId]
	INNER JOIN [Group] g ON g.Id = oa.[GroupId]
	INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
WHERE
	gt.[AttendanceCountsAsWeekendService] = 1
	AND a.[DidAttend] = 1
	AND CONVERT(date, a.[StartDateTime]) BETWEEN @STARTDATE AND @ENDDATE
GROUP BY ALL a.[CampusId], oa.[ScheduleId]'
WHERE [Guid] = '89553EEE-91F3-4169-9D7C-04A17471E035'" );

            // VOLUNTEER ATTENDANCE
            Sql( @"
UPDATE [Metric]
SET [SourceSql] = 'DECLARE @STARTDATE DATETIME = CONVERT(date, DATEADD(DAY, -7, dbo.RockGetDate()))
DECLARE @ENDDATE DATETIME = CONVERT(date, dbo.RockGetDate())
DECLARE @ServiceAreaDefinedValueId INT = (SELECT Id FROM dbo.[DefinedValue] WHERE [Guid] = ''36A554CE-7815-41B9-A435-93F3D52A2828'')

SELECT COUNT(1) as AttendanceCount, a.[CampusId], oa.[ScheduleId]
FROM [Attendance] a
INNER JOIN [AttendanceOccurrence] oa ON oa.Id = a.[OccurrenceId]
INNER JOIN [Group] g ON g.Id = oa.[GroupId]
INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
WHERE
   gt.[GroupTypePurposeValueId] = @ServiceAreaDefinedValueId
   AND a.[DidAttend] = 1 
   AND CONVERT(date, a.[StartDateTime]) BETWEEN @STARTDATE AND @ENDDATE
GROUP BY ALL a.[CampusId], oa.[ScheduleId]'
WHERE [Guid] = '4F965AE3-D455-4346-988F-2A2B5E236C0C'" );

            // PRAYER REQUESTS
            Sql( @"
UPDATE [Metric]
SET [SourceSql] = 'DECLARE @STARTDATE DATETIME = CONVERT(date, DATEADD(DAY, -7, dbo.RockGetDate()))
DECLARE @ENDDATE DATETIME = CONVERT(date, dbo.RockGetDate())

SELECT COUNT(1) as PrayerRequests, pr.[CampusId]
FROM dbo.[PrayerRequest] pr
WHERE
   pr.[IsActive] = 1
   AND CONVERT(date, pr.[CreatedDateTime]) BETWEEN @STARTDATE AND @ENDDATE
GROUP BY ALL pr.[CampusId]'
WHERE [Guid] = '2B5ECA35-47D8-4690-A8AD-72488485F2B4'" );

            // PRAYERS
            Sql( @"
UPDATE [Metric]
SET [SourceSql] = 'DECLARE @STARTDATE DATETIME = CONVERT(date, DATEADD(DAY, -7, dbo.RockGetDate()))
DECLARE @ENDDATE DATETIME = CONVERT(date, dbo.RockGetDate())

SELECT  COUNT(*) as Prayers, p.[PrimaryCampusId]
FROM dbo.[Interaction] i 
INNER JOIN [InteractionComponent] ic ON ic.[Id] = i.[InteractionComponentId]
INNER JOIN [InteractionChannel] ichan ON ichan.[Id] = ic.[InteractionChannelId]
INNER JOIN [PrayerRequest] pr ON pr.[Id] = ic.[EntityId]
INNER JOIN [PersonAlias] pa ON pa.[Id] = i.[PersonAliasId]
INNER JOIN [Person] p ON p.[Id] = pa.[PersonId]
WHERE 
   ichan.[Guid] = ''3D49FB99-94D1-4F63-B1A2-30D4FEDE11E9''
   AND i.[Operation] = ''Prayed''
   AND CONVERT(date, i.[InteractionDateTime]) BETWEEN @STARTDATE AND @ENDDATE
GROUP BY ALL p.[PrimaryCampusId]'
WHERE [Guid] = '685B7912-CB17-473B-90C1-2804F221931C'" );

            // BAPTISMS
            Sql( @"
UPDATE [Metric]
SET [SourceSql] = 'DECLARE @STARTDATE DATETIME= DATEADD(mm, DATEDIFF(mm, 0, dbo.RockGetDate()), 0)
DECLARE @ENDDATE DATETIME = DATEADD(DAY, -1, DATEADD(mm, 1, @STARTDATE));
DECLARE @BaptismDateAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = ''D42763FA-28E9-4A55-A25A-48998D7D7FEF'')

SELECT COUNT(*) as Baptisms, p.PrimaryCampusId FROM Person p
JOIN dbo.[AttributeValue] av
ON p.[Id] = av.[EntityId]
WHERE av.[AttributeId] = @BaptismDateAttributeId
AND CONVERT(date, av.[ValueAsDateTime]) >= @STARTDATE
AND CONVERT(date, av.[ValueAsDateTime]) < @ENDDATE
GROUP BY ALL p.[PrimaryCampusId]'
WHERE [Guid] = '8B63D9D5-A82D-49D4-9AED-2EDBCF60FDEE'" );

            // GIVING
            Sql( @"
UPDATE [Metric]
SET [SourceSql] = '-- =====================================================================================================
-- Description: This metric represents weekly giving to the tithe and should be partitioned by Campus.
-- =====================================================================================================
-- You can edit this to match the TOP LEVEL financial accounts that are considered part of the ''tithe'', but please
-- do not change the remainder of this script:

-- Let them be fetched dynamically like this: 
DECLARE @Accounts VARCHAR(512); 
DECLARE @Date DATETIME = CONVERT(date, dbo.RockGetDate());

SELECT @Accounts = COALESCE(@Accounts + '','', '''') + CAST([Id]  as NVARCHAR(50)) 
FROM [FinancialAccount]
WHERE [IsTaxDeductible] = 1
   AND [ParentAccountId] IS NULL
   AND [IsActive] = 1
   AND (
        ([StartDate] IS NOT NULL AND [EndDate] IS NOT NULL AND @Date BETWEEN CONVERT(date, [StartDate]) AND CONVERT(date, [EndDate]))
		OR ([EndDate] IS NOT NULL AND @Date < CONVERT(date, [EndDate]))
        OR ([StartDate] IS NOT NULL AND @Date > CONVERT(date, [StartDate]))
        OR ([StartDate] IS NULL AND [EndDate] IS NULL)
	)

-- OR 
-- You can manually set them like this as a comma separated list of accounts.
-- (NOTE: Their child accounts will be included):
-- DECLARE @Accounts VARCHAR(100) = ''1,2,3'';
 
-------------------------------------------------------------------------------------------------------
DECLARE @STARTDATE int = FORMAT( DATEADD(DAY, -7, dbo.RockGetDate()), ''yyyyMMdd'' )
DECLARE @ENDDATE int = FORMAT( dbo.RockGetDate(), ''yyyyMMdd'' )
DECLARE @PersonRecordTypeId INT = ( SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E'' )
DECLARE @AccountsWithChildren TABLE (Id INT);
-- Recursively get accounts and their children.
WITH AccountHierarchy AS (
    SELECT [Id]
    FROM dbo.[FinancialAccount] fa
    WHERE [Id] IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@Accounts, '',''))
    UNION ALL
    SELECT e.[Id]
    FROM dbo.[FinancialAccount] e
    INNER JOIN AccountHierarchy ah ON e.[ParentAccountId] = ah.[Id]
)
INSERT INTO @AccountsWithChildren SELECT * FROM AccountHierarchy;

;WITH CTE AS (
    SELECT
	fa.[Name] AS AccountName
    , fa.[CampusId] AS AccountCampusId
    , [PrimaryFamilyId]
    , SUM(ftd.[Amount]) AS [GivingAmount]
	FROM
		[Person] p
		INNER JOIN [dbo].[PersonAlias] pa ON pa.[PersonId] = p.[Id]
		INNER JOIN [dbo].[FinancialTransaction] ft ON ft.[AuthorizedPersonAliasId] = pa.[Id]
		INNER JOIN [dbo].[FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
		INNER JOIN [dbo].[FinancialAccount] fa ON fa.[Id] = ftd.[AccountId] 
		INNER JOIN [dbo].[AnalyticsSourceDate] asd ON asd.[DateKey] = ft.[TransactionDateKey]
	WHERE
		fa.[IsTaxDeductible] = 1
		AND asd.[DateKey] > = @STARTDATE AND asd.[DateKey] <= @ENDDATE
		AND fa.[Id] IN (SELECT * FROM @AccountsWithChildren)
AND p.[RecordTypeValueId] = @PersonRecordTypeId
	GROUP BY fa.[CampusId], fa.[Name], [PrimaryFamilyId]
)
SELECT
    SUM([GivingAmount]) AS [GivingAmount]
    , [AccountCampusId] AS [CampusId]
FROM CTE
GROUP BY [AccountCampusId];'
WHERE [Guid] = '43338e8a-622a-4195-b153-285e570b229d'" );

            // eRA WEEKLY LOSSES
            Sql( @"
UPDATE [Metric]
SET [SourceSql] = 'DECLARE @STARTDATE DATETIME = CONVERT(date, DATEADD(DAY, -7, dbo.RockGetDate()))
DECLARE @ENDDATE DATETIME = CONVERT(date, dbo.RockGetDate())
DECLARE @EraEndDateAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = ''4711D67E-7526-9582-4A8E-1CD7BBE1B3A2'')
DECLARE @IsEraAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = ''CE5739C5-2156-E2AB-48E5-1337C38B935E'')

SELECT COUNT(*) as eraLosses, p.[PrimaryCampusId]
FROM dbo.[Person] p
JOIN dbo.[AttributeValue] av ON p.Id = av.[EntityId]
WHERE av.[AttributeId] = @EraEndDateAttributeId
AND CONVERT(date, av.[ValueAsDateTime]) BETWEEN @StartDate AND @EndDate
AND EXISTS (
    SELECT 1
    FROM dbo.[AttributeValue] av2
    WHERE av2.[EntityId] = p.Id
    AND av2.[AttributeId] = @IsEraAttributeId
    AND av2.ValueAsBoolean = 0
)
GROUP BY ALL p.[PrimaryCampusId]'
WHERE [Guid] = '16A3FF64-31F0-4CFF-B5F4-83EEB69E0C25'" );

            // eRA WEEKLY WINS
            Sql( @"
UPDATE [Metric]
SET [SourceSql] = 'DECLARE @STARTDATE DATETIME = CONVERT(date, DATEADD(DAY, -7, dbo.RockGetDate()))
DECLARE @ENDDATE DATETIME = CONVERT(date, dbo.RockGetDate())
DECLARE @EraStartDateAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = ''A106610C-A7A1-469E-4097-9DE6400FDFC2'')
DECLARE @IsEraAttributeId INT = (SELECT Id FROM dbo.[Attribute] WHERE [Guid] = ''CE5739C5-2156-E2AB-48E5-1337C38B935E'')

SELECT COUNT(*) as eraWins, p.[PrimaryCampusId]
FROM dbo.[Person] p
JOIN dbo.[AttributeValue] av ON p.Id = av.[EntityId]
WHERE av.[AttributeId] = @EraStartDateAttributeId
AND CONVERT(date, av.[ValueAsDateTime]) BETWEEN @StartDate AND @EndDate
AND EXISTS (
    SELECT 1
    FROM dbo.[AttributeValue] av2
    WHERE av2.[EntityId] = p.Id
    AND av2.[AttributeId] = @IsEraAttributeId
    AND av2.ValueAsBoolean = 1
)
GROUP BY ALL p.[PrimaryCampusId];'
WHERE [Guid] = 'D05D685A-9A88-4375-A563-70BB44FBD237'" );
        }

        #endregion

        #region PA: Migration to update the Chart Lava Short Code

        // PA: Update the Lava for Chart Short Code to include ItemClickUrl setting to the dataitem configuration item.
        private void UpdateChartShortCodeLava()
        {
            Sql( @"UPDATE [LavaShortcode] 
SET [Documentation]=N'<p>
    Adding dynamic charts to a page can be difficult, even for an experienced Javascript developer. The 
    chart shortcode allows anyone to create charts with just a few lines of Lava. There are two modes for 
    creating a chart. The first ‘simple’ mode creates a chart with a single series. This option will suffice 
    for most of your charting needs. The second ‘series’ option allows you to create charts with multiple 
    series. Let’s look at each option separately starting with the simple option.
</p>
<h4>Simple Mode</h4>
<p>
    Let’s start by jumping to an example. We’ll then talk about the various configuration options, deal?
</p>
<pre>{[ chart type:''bar'' ]}
    [[ dataitem label:''Small Groups'' value:''45'' ]] [[ enddataitem ]]
    [[ dataitem label:''Serving Groups'' value:''38'' ]] [[ enddataitem ]]
    [[ dataitem label:''General Groups'' value:''34'' ]] [[ enddataitem ]]
    [[ dataitem label:''Fundraising Groups'' value:''12'' ]] [[ enddataitem ]]
{[ endchart ]}</pre>
    
<p>    
    As you can see this sample provides a nice-looking bar chart. The shortcode defines the chart type (several other 
    options are available). The [[ dataitem ]] configuration item defines settings for each bar/point on the chart. Each 
    has the following settings:
</p>
<ul>
    <li><strong>label</strong> – The label for the data item.</li>
    <li><strong>value</strong> – The data point for the item.</li><li><b>itemclickurl&nbsp;</b>(Optional)<b>&nbsp;</b>– The url of the page to redirect to when the item is clicked. Generally, the relative url of the target page is provided in this setting.</li>
</ul>
<p>
    The chart itself has quite a few settings for you to consider. These include:
</p>
<ul>
    <li><strong>type</strong> (bar) – The type of chart to display. The valid options include: bar, stackedbar, horizontalBar, line, radar, pie, doughnut, polarArea (think radar meets pie).</li>
    <li><strong>bordercolor</strong> (#059BFF) – The color of the border of the data item.</li>
    <li><strong>borderdash</strong> – This setting defines how the lines on the chart should be displayed. No value makes them display as solid lines. You can make interesting dot/dash patterns by providing an array of numbers representing lines and spaces. For instance, the setting of ''[5, 5]'' would say draw a line of length 5px and then a space of 5px and repeat. You can provide as many numbers as you like to make more complex patterns (but isn''t that getting a little too fancy?)</li>
    <li><strong>borderwidth</strong> (0) – The pixel width of the border.</li>
    <li><strong>legendposition</strong> (bottom) – This determines where the legend should be displayed.</li>
    <li><strong>legendshow</strong> (false) – Setting determines if the legend should be shown.</li>
    <li><strong>chartheight</strong> (400px) – The height of the chart must be set in pixels.</li>
    <li><strong>chartwidth</strong> (100%) – The width of the chart (can set as either a percentage or pixel size).</li>
    <li><strong>tooltipshow</strong> (true) – Determines if tooltips should be displayed when rolling over data items.</li>
    <li><strong>tooltipbackgroundcolor</strong> (#000) – The background color of the tooltip.</li>
    <li><strong>tooltipfontcolor</strong> (#fff) – The font color of the tooltip.</li>
    <li><strong>fontcolor</strong> (#777) – The font color to use on the chart.</li>
    <li><strong>fontfamily</strong> (sans-serif) – The font to use for the chart.</li>
    <li><strong>pointradius</strong> (3) – Some charts, like the line chart, have dots (points) for the values. This determines how big the points should be.</li>
    <li><strong>pointcolor</strong> (#059BFF) – The color of the points on the chart.</li>
    <li><strong>pointbordercolor</strong> (#059BFF) – The color of the border on the points.</li>
    <li><strong>pointborderwidth</strong> (0) – The width, in pixels, of the border on points.</li>
    <li><strong>pointhovercolor</strong> (rgba(5,155,255,.6)) – The hover color of points on the chart.</li>
    <li><strong>pointhoverbordercolor</strong> (rgba(5,155,255,.6)) – The hover color of the border on points.</li>
    <li><strong>pointhoverradius</strong> (3) – The size of the point when hovering.</li>
    <li><strong>curvedlines</strong> (true) – This determines if the lines should be straight between two points or beautifully curved. Based on this description you should be able to determine the default.</li>
    <li><strong>filllinearea</strong> (false) – This setting determines if the area under a line should be filled in (basically creating an area chart).</li>
    <li><strong>fillcolor</strong> (rgba(5,155,255,.6)) – The fill color for data items. You can also provide a fill color for each item independently on the [[ dataitem ]] configuration. </li>
    <li><strong>label</strong> – The label to show for the single axis (not often needed in a single axis chart, but hey it''s there.)</li>
    <li><strong>xaxisshow</strong> (true) – Show or hide the x-axis labels. Valid values are ''true'' and ''false''.</li>
    <li><strong>yaxisshow</strong> (true) – Show or hide the y-axis labels. Valid values are ''true'' and ''false''.</li>
    <li><strong>xaxistype</strong> (linear) – The x-axis type. This is primarily used for time based charts. Valid values are ''linear'', ''time'' and ''linearhorizontal0to100''. The linearhorizontal0to100 option makes the horizontal axis scale from 0 to 100.</li>
    <li><strong>yaxismin</strong> (undefined) – The minimum number value of the y-axis. If no value is provided the min value is automatically calculated. To set a chart to always start from zero, rather than using a computed minimum, set the value to 0</li>
		<li><strong>yaxismax</strong> (undefined) – The maximum number value of the y-axis. If no value is provided the max value is automatically calculated.</li>
    <li><strong>yaxisstepsize</strong> (undefined) – If set, the y-axis scale ticks are displayed by a multiple of the defined value. So a yaxisstepsize of 10 means one tick on 10, 20, 30, 40 etc. If no value is provided the step size is automatically computed.</li>
    <li><strong>valueformat</strong> (number) – Format numbers on tooltips and chart axis labels. Valid options include: number (formats to the browser''s locale, in the US adds a thousands comma), currency (adds a comma and currency symbol from the global attribute), percentage (formats number to a percentage in the browser''s locale, expects whole numbers [100 = 100%]), none (no formatting applied).</li>
		<li><strong>X Axis Advanced Options</strong> - Used with the horizontalBar chart type.
		<ul>
		    <li><strong>xaxismin</strong> (undefined) – The minimum number value of the x-axis. If no value is provided the min value is automatically calculated. To set a chart to always start from zero, rather than using a computed minimum, set the value to 0</li>
			<li><strong>xaxismax</strong> (undefined) – The maximum number value of the x-axis. If no value is provided the max value is automatically calculated.</li>
			<li><strong>xaxisstepsize</strong> (undefined) – If set, the x-axis scale ticks are displayed by a multiple of the defined value. So a yaxisstepsize of 10 means one tick on 10, 20, 30, 40 etc. If no value is provided the step size is automatically computed.</li>
		</ul>
		</li>
		
</ul>
<h5>Time Based Charts</h5>
<p>
    If the x-axis of your chart is date/time based you’ll want to set the ''xaxistype'' to ''time'' and provide
    the date in the label field.
</p>
<pre>{[ chart type:''line'' xaxistype:''time'' ]}
    [[ dataitem label:''1/1/2017'' value:''24'']] [[ enddataitem ]]
    [[ dataitem label:''2/1/2017'' value:''38'' ]] [[ enddataitem ]]
    [[ dataitem label:''3/1/2017'' value:''42''  ]] [[ enddataitem ]]
    [[ dataitem label:''5/1/2017'' value:''23'' ]] [[ enddataitem ]]
{[ endchart ]}</pre>
<p>
    That should be more than enough settings to get you started on the journey to chart success. But… what about 
    multiple series? Glad you asked…
</p>
<h4>Multiple Series</h4>
<p>
    It’s simple to add multiple series to your charts using the [[ dataset ]] configuration option. Each series is defined 
    by a [[ dataset ]] configuration block. Let’s again start with an example.
</p>
<pre>{[ chart type:''bar'' labels:''2015,2016,2017'' ]}
    [[ dataset label:''Small Groups'' data:''12, 15, 34'' fillcolor:''#059BFF'' ]] [[ enddataset ]]
    [[ dataset label:''Serving Teams'' data:''10, 22, 41'' fillcolor:''#FF3D67'' ]] [[ enddataset ]]
    [[ dataset label:''General Groups'' data:''5, 12, 21'' fillcolor:''#4BC0C0'' ]] [[ enddataset ]]
    [[ dataset label:''Fundraising Groups'' data:''3, 17, 32'' fillcolor:''#FFCD56'' ]] [[ enddataset ]]
{[ endchart ]}</pre>
<p><img src=""https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/chart-series.jpg"" class=""img-responsive mx-auto"" width=""747"" height=""408"" loading=""lazy""></p>
<p>
    If there is a trick to using series it’s understanding the organization of the data. In our example each [[ dataset ]] 
    is type of group. The data property of the dataset determine the number of groups for each year. The configuration of dataset 
    was created to help you write Lava to dynamically create your charts.
</p>
<p>
    Each of the dataset items have the following configuration options:
</p>
<ul>
    <li><strong>label</strong> – This is the descriptor of the dataset used for the legend.</li>
    <li><strong>fillcolor</strong> (rgba(5,155,255,.6)) – The fill color for the data set. You should change this to help differentiate the series.</li>
    <li><strong>filllinearea</strong> (false) – This setting determines if the area under a line should be filled in (basically creating an area chart).</li>
    <li><strong>bordercolor</strong> (#059BFF) – The color of the border of the data item.</li>
    <li><strong>borderwidth</strong> (0) – The pixel width of the border.</li>
    <li><strong>pointradius</strong> (3) – Some charts, like the line chart, have dots (points) for the values. This determines how big the points should be. </li>
    <li><strong>pointcolor</strong> (#059BFF) – The color of the points on the chart.</li>
    <li><strong>pointbordercolor</strong> (#059BFF) – The color of the border on the points.</li>
    <li><strong>pointborderwidth</strong> (0) – The width, in pixels, of the border on points.</li>
    <li><strong>pointhovercolor</strong> (rgba(5,155,255,.6)) – The hover color of points on the chart.</li>
    <li><strong>pointhoverbordercolor</strong> (rgba(5,155,255,.6)) – The hover color of the border on points.</li>
    <li><strong>pointhoverradius</strong> (3) – The size of the point when hovering.</li>
</ul>
<h5>Time Based Multi-Series Charts</h5>
<p>
    Like their single series brothers, multi-series charts can be line based to by setting
    the xseriestype = ''line'' and providing the dates in the ''label'' setting.
</p>
<pre>{[ chart type:''line'' labels:''1/1/2017,2/1/2017,6/1/2017'' xaxistype:''time'' ]}
    [[ dataset label:''Small Groups'' data:''12, 15, 34'' fillcolor:''#059BFF'' ]] [[ enddataset ]]
    [[ dataset label:''Serving Teams'' data:''10, 22, 41'' fillcolor:''#FF3D67'' ]] [[ enddataset ]]
    [[ dataset label:''General Groups'' data:''5, 12, 21'' fillcolor:''#4BC0C0'' ]] [[ enddataset ]]
    [[ dataset label:''Fundraising Groups'' data:''3, 17, 32'' fillcolor:''#FFCD56'' ]] [[ enddataset ]]
{[ endchart ]}</pre>
<h4>Gauge Charts</h4>
<p>Gauge charts allow you to create speedometer style charts.</p>
<p><img src=""https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/gauge-chart.png"" class=""img-responsive mx-auto"" width=""500"" height=""268"" loading=""lazy""></p>
<pre>{[ chart type:''tsgauge'' gaugelimits:''0,90,130'' backgroundcolor:''#16c98d,#d4442e'' ]}
  [[ dataitem value:''23'' fillcolor:''#484848'' ]][[ enddataitem ]]
{[ endchart ]}</pre>
<p>
  Each chart has the following configuration options:
</p>
<ul>
  <li><strong>type</strong> – Set to <code>tsgauge</code>.</li>
  <li><strong>gaugelimits</strong> - Gauge limits are comma separated numbers representing each ""band"" of the chart. The first value is the minimum value of the chart, and the last value represents the maximum value.To create additional bands, add additional comma separated numbers between the minimum and maximum value. Note that each comma represents a colored band, and requires an additional background color (detailed below).</li>
  <li><strong>backgroundcolor</strong> – A comma separated list of the color of each ""band"" shown on the chart. A gauge chart will require at least one background color, and additional colors for each additional ""band"".</li>
  <li>
    <strong>dataitem</strong> – A gauge chart has one dataitem that represents the achieved value within the chart.
    <ul>
      <li><strong>value</strong> – The position of the arrow indicator on the chart.</li>
      <li><strong>fillcolor</strong> – The color of the arrow indicator.</li>
			<li><strong>label</strong> (Optional) – Replace the center number, which is the value by default, with the provided label. </li>
    </ul>
  </li>
</ul>',
[Markup]=N'{% javascript url:''~/Scripts/moment.min.js'' id:''moment''%}{% endjavascript %}
{% javascript url:''~/Scripts/Chartjs/Chart.min.js'' id:''chartjs''%}{% endjavascript %}
{% assign tooltipvalueformat = valueformat %}
{% assign xvalueformat = ''none'' %}
{%- if type == ''gauge'' or type == ''tsgauge'' -%}
    {%- assign type = ''tsgauge'' -%}
    {% javascript url:''~/Scripts/Chartjs/Gauge.js'' id:''gaugejs''%}{% endjavascript %}
{%- elseif type == ''stackedbar'' -%}
    {%- assign type = ''bar'' -%}
    {%- assign xaxistype = ''stacked'' -%}
{%- elseif type == ''horizontalBar'' %}
    {% assign xvalueformat = valueformat %}
    {% assign valueformat = ''none'' %}
{% endif %}
{% assign id = uniqueid %}
{% assign curvedlines = curvedlines | AsBoolean %}
{%- if type == ''tsgauge'' -%}
  {% assign backgroundColor = backgroundcolor | Split:'','' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
  {% assign gaugeLimits = gaugelimits | Split:'','' | Join:'','' | Prepend:''['' | Append:'']'' %}
  {%- assign tooltipshow = false -%}
  {%- capture seriesData -%}
    {
        backgroundColor: {{ backgroundColor }},
        borderWidth: {{ borderwidth }},
        gaugeData: {
            value: {{dataitems[0].value}},
            {% if dataitems[0].label != '''' %}
                label: ''{{dataitems[0].label}}'',
            {% endif %}
            valueColor: ""{{dataitems[0].fillcolor | Default:''#000000''}}""
        },
        gaugeLimits: {{ gaugeLimits }}
    }
  {%- endcapture -%}
{% else %}
  {% assign dataitemCount = dataitems | Size -%}
  {% if dataitemCount > 0 -%}
      {% assign fillColors = dataitems | Map:''fillcolor'' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
      {% assign borderColors = dataitems | Map:''bordercolor'' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
      {% assign tooltips = dataitems | Map:''tooltip'' | Join:''"", ""'' | Prepend:''""'' | Append:''""'' %}
      {% assign itemclickurls = dataitems | Select:''itemclickurl'' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
      {% assign firstDataItem = dataitems | First  %}
      {% capture seriesData -%}
      {
          fill: {{ filllinearea }},
          backgroundColor: {% if firstDataItem.fillcolor %}{{ fillColors }}{% else %}''{{ fillcolor }}''{% endif %},
          borderColor: {% if firstDataItem.bordercolor %}{{ borderColors }}{% else %}''{{ bordercolor }}''{% endif %},
          borderWidth: {{ borderwidth }},
          pointRadius: {{ pointradius }},
          pointBackgroundColor: ''{{ pointcolor }}'',
          pointBorderColor: ''{{ pointbordercolor }}'',
          pointBorderWidth: {{ pointborderwidth }},
          pointHoverBackgroundColor: ''{{ pointhovercolor }}'',
          pointHoverBorderColor: ''{{ pointhoverbordercolor }}'',
          pointHoverRadius: ''{{ pointhoverradius }}'',
          {% if borderdash != '''' -%} borderDash: {{ borderdash }},{% endif -%}
          {% if curvedlines == false -%} lineTension: 0,{% endif -%}
          data: {{ dataitems | Map:''value'' | Join:'','' | Prepend:''['' | Append:'']'' }},
          {% if firstDataItem.tooltip %}
          tooltips: [{{ tooltips }}],
          {% endif %}
      }
      {% endcapture -%}
      {% assign labels = dataitems | Map:''label'' | Join:''"", ""'' | Prepend:''""'' | Append:''""'' -%}
  {% else -%}
      {% if labels == '''' -%}
          <div class=""alert alert-warning"">
              When using datasets you must provide labels on the shortcode to define each unit of measure.
              {% raw %}{[ chart labels:''Red, Green, Blue'' ... ]}{% endraw %}
          </div>
      {% else %}
          {% assign labelItems = labels | Split:'','' -%}
          {% assign labels = ''""'' -%}
          {% for labelItem in labelItems -%}
              {% assign labelItem = labelItem | Trim %}
              {% assign labels = labels | Append:labelItem | Append:''"",""'' %}
          {% endfor -%}
          {% assign labels = labels | ReplaceLast:''"",""'',''""'' %}
      {% endif -%}
      {% assign seriesData = '''' -%}
      {% for dataset in datasets -%}
          {% if dataset.label -%} {% assign datasetLabel = dataset.label %} {% else -%} {% assign datasetLabel = '' '' %} {% endif -%}
          {% if dataset.fillcolor -%} {% assign datasetFillColor = dataset.fillcolor %} {% else -%} {% assign datasetFillColor = fillcolor %} {% endif -%}
          {% if dataset.filllinearea -%} {% assign datasetFillLineArea = dataset.filllinearea %} {% else -%} {% assign datasetFillLineArea = filllinearea %} {% endif -%}
          {% if dataset.bordercolor -%} {% assign datasetBorderColor = dataset.bordercolor %} {% else -%} {% assign datasetBorderColor = bordercolor %} {% endif -%}
          {% if dataset.borderwidth -%} {% assign datasetBorderWidth = dataset.borderwidth %} {% else -%} {% assign datasetBorderWidth = borderwidth %} {% endif -%}
          {% if dataset.pointradius -%} {% assign datasetPointRadius = dataset.pointradius %} {% else -%} {% assign datasetPointRadius = pointradius %} {% endif -%}
          {% if dataset.pointcolor -%} {% assign datasetPointColor = dataset.pointcolor %} {% else -%} {% assign datasetPointColor = pointcolor %} {% endif -%}
          {% if dataset.pointbordercolor -%} {% assign datasetPointBorderColor = dataset.pointbordercolor %} {% else -%} {% assign datasetPointBorderColor = pointbordercolor %} {% endif -%}
          {% if dataset.pointborderwidth -%} {% assign datasetPointBorderWidth = dataset.pointborderwidth %} {% else -%} {% assign datasetPointBorderWidth = pointborderwidth %} {% endif -%}
          {% if dataset.pointhovercolor -%} {% assign datasetPointHoverColor = dataset.pointhovercolor %} {% else -%} {% assign datasetPointHoverColor = pointhovercolor %} {% endif -%}
          {% if dataset.pointhoverbordercolor -%} {% assign datasetPointHoverBorderColor = dataset.pointhoverbordercolor %} {% else -%} {% assign datasetPointHoverBorderColor = pointhoverbordercolor %} {% endif -%}
          {% if dataset.pointhoverradius -%} {% assign datasetPointHoverRadius = dataset.pointhoverradius %} {% else -%} {% assign datasetPointHoverRadius = pointhoverradius %} {% endif -%}
          {%- capture itemData -%}
              {
                  label: ''{{ datasetLabel }}'',
                  fill: {{ datasetFillLineArea }},
                  backgroundColor: ''{{ datasetFillColor }}'',
                  borderColor: ''{{ datasetBorderColor }}'',
                  borderWidth: {{ datasetBorderWidth }},
                  pointRadius: {{ datasetPointRadius }},
                  pointBackgroundColor: ''{{ datasetPointColor }}'',
                  pointBorderColor: ''{{ datasetPointBorderColor }}'',
                  pointBorderWidth: {{ datasetPointBorderWidth }},
                  pointHoverBackgroundColor: ''{{ datasetPointHoverColor }}'',
                  pointHoverBorderColor: ''{{ datasetPointHoverBorderColor }}'',
                  pointHoverRadius: ''{{ datasetPointHoverRadius }}'',
                  {%- if dataset.borderdash and dataset.borderdash != '''' -%} borderDash: {{ dataset.borderdash }},{%- endif -%}
                  {%- if dataset.curvedlines and dataset.curvedlines == ''false'' -%} lineTension: 0,{%- endif -%}
                  data: [{{ dataset.data }}]
              },
          {% endcapture -%}
          {% assign seriesData = seriesData | Append:itemData -%}
      {% endfor -%}
      {% assign seriesData = seriesData | ReplaceLast:'','', '''' -%}
  {% endif -%}
{%- endif -%}
<div class=""chart-container"" style=""position: relative; height:{{ chartheight }}; width:{{ chartwidth }}"">
    <canvas id=""chart-{{ id }}""></canvas>
</div>
<script>
    var options = {
    maintainAspectRatio: false,
    onClick: function(event, array) { 
        if (array.length > 0) {
            var index = array[0]._index;
            var redirectUrl = data.itemclickurl[index];
            // enable redirection only if a vaild itemclickurl is provided.
            if(data && data.itemclickurl && data.itemclickurl[index]) {
                window.location.href = data.itemclickurl[index];
            }
        }
    },
    hover: {
        onHover: function(event, array) {
            var target = event.target || event.srcElement;
            if (array.length > 0) {
                var index = array[0]._index;
                var redirectUrl = data.itemclickurl[index];
                // enable redirection only if a vaild itemclickurl is provided.
                if(data && data.itemclickurl && data.itemclickurl[index]) {
                    target.style.cursor = ''pointer'';
                    return;
                }
            }
            target.style.cursor = ''default'';
        }
    },scales: {
         yAxes: [{
            ticks: {
                beginAtZero: true
            }
        }]
    },
    {%- if type != ''tsgauge'' -%}
        legend: {
            position: ''{{ legendposition }}'',
            display: {{ legendshow }}
        },
        tooltips: {
            enabled: {{ tooltipshow }}
            {% if tooltipshow %}
            , backgroundColor: ''{{ tooltipbackgroundcolor }}''
            , bodyFontColor: ''{{ tooltipfontcolor }}''
            , titleFontColor: ''{{ tooltipfontcolor }}''
                {% if tooltipvalueformat != '''' and tooltipvalueformat != ''none'' %}
                , callbacks: {
                    label: function(tooltipItem, data) {
                        {% if type == ''pie'' %}
                            {% case tooltipvalueformat %}
                                {% when ''currency'' %}
                                    return data.labels[tooltipItem.index] + "": "" + Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                                {% when ''percentage'' %}
                                    return data.labels[tooltipItem.index] + "": "" + Intl.NumberFormat(undefined, { style: ''percent'', maximumFractionDigits: 2 }).format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]/100);
                                {% else %}
                                    return data.labels[tooltipItem.index] + "": "" + Intl.NumberFormat().format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                            {% endcase %}
                        {% else %}
                            {% case tooltipvalueformat %}
                                {% when ''currency'' %}
                                    return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                                {% when ''percentage'' %}
                                    return Intl.NumberFormat(undefined, { style: ''percent'', maximumFractionDigits: 2 }).format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]/100);
                                {% when ''number'' %}
                                    return Intl.NumberFormat().format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                                {% else %}
                                    return Intl.NumberFormat().format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                            {% endcase %}
                        {% endif %}
                    }
                }
                {% endif %}
            {% endif %}
        }
        {%- else -%}
        events: [],
        showMarkers: false
        {%- endif -%}
        {% if xaxistype == ''time'' %}
            ,scales: {
                xAxes: [{
                    type: ""time"",
                    display: {{ xaxisshow }},
                    scaleLabel: {
                        display: true,
                        labelString: ''Date''
                    }
                }],
                yAxes: [{
                    display: {{ yaxisshow }},
                    {% if label != null and label != '''' %}
                    scaleLabel: {
                        display: true,
                        labelString: ''{{ label }}''
                    },
                    {% endif %}
                    ticks: {
                        {% if valueformat != '''' and valueformat != ''none'' %}
                        callback: function(label, index, labels) {
                            {% case valueformat %}
                                {% when ''currency'' %}
                                    return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'', minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(label);
                                {% when ''percentage'' %}
                                    return Intl.NumberFormat(undefined, { style: ''percent'', maximumFractionDigits: 2 }).format(label/100);
                                {% else %}
                                    return Intl.NumberFormat().format(label);
                            {% endcase %}
                        },
                        {% endif %}
                    {% if yaxismin != '''' %}min: {{ yaxismin }}{%- endif %}
                    {% if yaxismax != '''' %},max: {{ yaxismax }}{%- endif %}
                    {% if yaxisstepsize != '''' %}, stepSize: {{ yaxisstepsize }}{% endif %}
                    }
                }]
            }
        {% elseif xaxistype == ''linearhorizontal0to100'' %}
            ,scales: {
                xAxes: [{
                    display: {{ xaxisshow }},
                    ticks: {
                        min: 0,
                        max: 100
                    }
                }],
                yAxes: [{
                    display: {{ yaxisshow }},
                    gridLines: {
                        display: false
                    }
                }]
            }
        {% elseif xaxistype == ''stacked'' %}
        {% if yaxislabels != '''' %}
            {%- assign yaxislabels = yaxislabels | Split:'','' -%}
            {%- assign yaxislabelcount = yaxislabels | Size -%}
        {% else %}
            {%- assign yaxislabelcount = 0 -%}
        {% endif %}
        ,scales: {
            xAxes: [{
                display: {{ xaxisshow }},
                stacked: true,
                {%- if xaxismin != '''' or xaxismax != '''' or xaxisstepsize != '''' -%}
                    , ticks: {
                    {% if xaxismin != '''' %}min: {{ xaxismin }},{% endif %}
                    {% if xaxismax != '''' %}max: {{ xaxismax }},{% endif %}
                    {% if xaxisstepsize != '''' %}stepSize: {{ xaxisstepsize }}, {% endif %}
                    }
                {% endif %}
            }],
            yAxes: [{
                display: {{ yaxisshow }},
                stacked: true
                {%- if yaxislabelcount > 0 or yaxismin != '''' or yaxismax != '''' or yaxisstepsize != '''' -%}
                , ticks: {
                {% if yaxismin != '''' %}min: {{ yaxismin }}{% endif %}
                {% if yaxismax != '''' %},max: {{ yaxismax }}{% endif %}
                {% if yaxisstepsize != '''' %}, stepSize: {{ yaxisstepsize }}{% endif %}
                {% if yaxislabelcount > 0 %}
                    ,
                    callback: function(label, index, labels) {
                        switch (label) {
                            {%- for yaxislabel in yaxislabels -%}
                                {%- assign axislabel = yaxislabel | Split:''^'' -%}
                                case {{ axislabel[0] }}: return ''{{axislabel[1]}}'';
                            {%- endfor -%}
                        }
                    }
                {% endif %}
                },
        {% endif %}
            }]
    }
        {%- elseif type != ''pie'' and type != ''tsgauge'' -%}
            ,scales: {
                xAxes: [{
                    display: {{ xaxisshow }},
                    {%- if xaxismin != '''' or xaxismax != '''' or xaxisstepsize != '''' -%}
                        ticks: {
                        {% if xaxismin != '''' %}min: {{ xaxismin }},{% endif %}
                        {% if xaxismax != '''' %}max: {{ xaxismax }},{% endif %}
                        {% if xaxisstepsize != '''' %}stepSize: {{ xaxisstepsize }}, {% endif %}
                        {% if xvalueformat != '''' and xvalueformat != ''none'' %}
                            callback: function(label, index, labels) {
                                {% case xvalueformat %}
                                    {% when ''currency'' %}
                                        if (label % 1 === 0) {
                                            return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'', minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(label);
                                        } else {
                                            return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(label);
                                        }
                                    {% when ''percentage'' %}
                                        return Intl.NumberFormat(undefined, { style: ''percent'', maximumFractionDigits: 2 }).format(label/100);
                                    {% else %}
                                        return Intl.NumberFormat().format(label);
                                {% endcase %}
                            },
                        {% endif %}
                        }
                    {% endif %}
                }],
                yAxes: [{
                    display: {{ yaxisshow }},
                    {% if label != null and label != '''' %}
                    scaleLabel: {
                        display: true,
                        labelString: ''{{ label }}''
                    },
                    {% endif %}
                    ticks: {
                        {% if valueformat != '''' and valueformat != ''none'' %}
                        callback: function(label, index, labels) {
                            {% case valueformat %}
                                {% when ''currency'' %}
                                    if (label % 1 === 0) {
                                        return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'', minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(label);
                                    } else {
                                        return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(label);
                                    }
                                {% when ''percentage'' %}
                                    return Intl.NumberFormat(undefined, { style: ''percent'', maximumFractionDigits: 2 }).format(label/100);
                                {% else %}
                                    return Intl.NumberFormat().format(label);
                            {% endcase %}
                        },
                        {% endif %}
                        {% if yaxismin != '''' %}min: {{ yaxismin }}{%- endif %}
                        {% if yaxismax != '''' %},max: {{ yaxismax }}{%- endif %}
                        {% if yaxisstepsize != '''' %}, stepSize: {{ yaxisstepsize }}{% endif %}
                        },
                }]
            }
        {% endif %}
    };
    {%- if type == ''tsgauge'' -%}
        var data = {
            datasets: [{{ seriesData }}]
        };
    {%- else -%}
        var data = {
            labels: [{{ labels }}],
            datasets: [{{ seriesData }}],
            borderWidth: {{ borderwidth }},
            {% if itemclickurls %}
                itemclickurl: {{ itemclickurls }},
            {% endif %}
        };
    {% endif %}
    Chart.defaults.global.defaultFontColor = ''{{ fontcolor }}'';
    Chart.defaults.global.defaultFontFamily = ""{{ fontfamily }}"";
    var ctx = document.getElementById(''chart-{{ id }}'').getContext(''2d'');
    var chart = new Chart(ctx, {
        type: ''{{ type }}'',
        data: data,
        options: options
    });
</script>
'
WHERE [GUID] = '43819A34-4819-4507-8FEA-2E406B5474EA'" );
        }

        #endregion

        #region PA: Register block attributes for chop job in v1.17.0.30

        private void MigrationRollupsForV17_0_17_ChopBlocksUp()
        {
            MigrationRollupsForV17_0_17_RegisterBlockAttributesForChop();
            //MigrationRollupsForV17_0_17_ChopBlockTypesv17();
        }

        private void MigrationRollupsForV17_0_17_RegisterBlockAttributesForChop()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Mobile.MobileLayoutDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Mobile.MobileLayoutDetail", "Mobile Layout Detail", "Rock.Blocks.Mobile.MobileLayoutDetail, Rock.Blocks, Version=1.17.0.29, Culture=neutral, PublicKeyToken=null", false, false, "E83C989B-5ECB-4DE4-B5BF-11AF7FC2CCA3" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.EventList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.EventList", "Event List", "Rock.Blocks.Core.EventList, Rock.Blocks, Version=1.17.0.29, Culture=neutral, PublicKeyToken=null", false, false, "3097CE11-E5D7-4708-A576-EF327BE8F6E4" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.PersonalLinkSectionList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.PersonalLinkSectionList", "Personal Link Section List", "Rock.Blocks.Cms.PersonalLinkSectionList, Rock.Blocks, Version=1.17.0.29, Culture=neutral, PublicKeyToken=null", false, false, "55429A67-E6C6-42FE-813B-3EA67A575EB0" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Crm.PhotoUpload
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.PhotoUpload", "Photo Upload", "Rock.Blocks.Crm.PhotoUpload, Rock.Blocks, Version=1.17.0.29, Culture=neutral, PublicKeyToken=null", false, false, "E9B8A70B-BB59-4044-900F-44150DA73300" );


            // Add/Update Mobile Block Type
            //   Name:Mobile Layout Detail
            //   Category:Mobile
            //   EntityType:Rock.Blocks.Mobile.MobileLayoutDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Mobile Layout Detail", "Edits and configures the settings of a mobile layout.", "Rock.Blocks.Mobile.MobileLayoutDetail", "Mobile", "C64F92CC-38A6-4562-8EAE-D4F30B4AF017" );

            // Add/Update Obsidian Block Type
            //   Name:Event List
            //   Category:Follow
            //   EntityType:Rock.Blocks.Core.EventList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Event List", "Block for viewing list of following events.", "Rock.Blocks.Core.EventList", "Follow", "04AB8A15-1D0A-4F53-84FE-7B0DE611EB02" );

            // Add/Update Obsidian Block Type
            //   Name:Personal Link Section List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.PersonalLinkSectionList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Personal Link Section List", "Lists personal link section in the system.", "Rock.Blocks.Cms.PersonalLinkSectionList", "CMS", "904DB731-4A40-494C-B52C-95CF0F54C21F" );

            // Add/Update Obsidian Block Type
            //   Name:Photo Upload
            //   Category:CRM
            //   EntityType:Rock.Blocks.Crm.PhotoUpload
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Photo Upload", "Allows a photo to be uploaded for the given person (logged in person) and optionally their family members.", "Rock.Blocks.Crm.PhotoUpload", "CRM", "C523CABA-A32C-46A3-A8B4-8F962CDC6A78" );


            // Attribute for BlockType
            //   BlockType: Event List
            //   Category: Follow
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "04AB8A15-1D0A-4F53-84FE-7B0DE611EB02", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the following event type details.", 0, @"", "495CBB8D-5AD0-48E6-8801-70EBF1807A1F" );

            // Attribute for BlockType
            //   BlockType: Event List
            //   Category: Follow
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "04AB8A15-1D0A-4F53-84FE-7B0DE611EB02", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "86D5B788-D0EE-4838-8B6C-BB3D973512A1" );

            // Attribute for BlockType
            //   BlockType: Event List
            //   Category: Follow
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "04AB8A15-1D0A-4F53-84FE-7B0DE611EB02", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "BF2A85E5-6D7F-4581-84EE-B3E8681D42EE" );

            // Attribute for BlockType
            //   BlockType: Personal Link Section List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "904DB731-4A40-494C-B52C-95CF0F54C21F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the personal link section details.", 0, @"", "224969C0-CC89-41CB-94C1-F569A97DC293" );

            // Attribute for BlockType
            //   BlockType: Personal Link Section List
            //   Category: CMS
            //   Attribute: Shared Sections
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "904DB731-4A40-494C-B52C-95CF0F54C21F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Shared Sections", "SharedSection", "Shared Sections", @"When enabled, only shared sections will be displayed.", 1, @"False", "FE13BFB1-1ABA-44CC-825D-DDC24753154C" );

            // Attribute for BlockType
            //   BlockType: Personal Link Section List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "904DB731-4A40-494C-B52C-95CF0F54C21F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "61B4DD25-1D91-444D-929B-EEC86073E506" );

            // Attribute for BlockType
            //   BlockType: Personal Link Section List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "904DB731-4A40-494C-B52C-95CF0F54C21F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "2E1D6AC7-A801-4642-8D7C-EC8CA0983458" );

            // Attribute for BlockType
            //   BlockType: Photo Upload
            //   Category: CRM
            //   Attribute: Include Family Members
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C523CABA-A32C-46A3-A8B4-8F962CDC6A78", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Family Members", "IncludeFamilyMembers", "Include Family Members", @"If checked, other family members will also be displayed allowing their photos to be uploaded.", 0, @"True", "57EBA90F-09FD-4A9F-A6A6-4B5C6F236061" );

            // Attribute for BlockType
            //   BlockType: Photo Upload
            //   Category: CRM
            //   Attribute: Allow Staff
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C523CABA-A32C-46A3-A8B4-8F962CDC6A78", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Staff", "AllowStaff", "Allow Staff", @"If checked, staff members will also be allowed to upload new photos for themselves.", 1, @"False", "5401B675-F6D0-4AAA-B369-601E12F74D0B" );

        }

        // PA: Chop blocks for v1.17.0.30
        private void MigrationRollupsForV17_0_17_ChopBlockTypesv17()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types - 1.17.0.30",
                blockTypeReplacements: new Dictionary<string, string> {
{ "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "5AA30F53-1B7D-4CA9-89B6-C10592968870" }, // Prayer Request Entry
{ "74B6C64A-9617-4745-9928-ABAC7948A95D", "C64F92CC-38A6-4562-8EAE-D4F30B4AF017" }, // Mobile Layout Detail
{ "092BFC5F-A291-4472-B737-0C69EA33D08A", "3852E96A-9270-4C0E-A0D0-3CD9601F183E" }, // Lava Shortcode Detail
{ "0C3CEBAD-3CCB-493B-9CBA-9D0D33852050", "04AB8A15-1D0A-4F53-84FE-7B0DE611EB02" }, // Event List
{ "0BFD74A8-1888-4407-9102-D3FCEABF3095", "904DB731-4A40-494C-B52C-95CF0F54C21F" }, // Personal Link Section List
{ "160DABF9-3549-447C-9E76-6CFCCCA481C0", "1228F248-6AA1-4871-AF9E-195CF0FDA724" }, // Verify Photo
{ "CE06640D-C1BA-4ACE-AF03-8D733FD3247C", "DBFA9E41-FA62-4869-8A44-D03B561433B2" }, // User Login List
{ "7764E323-7460-4CB7-8024-056136C99603", "C523CABA-A32C-46A3-A8B4-8F962CDC6A78" }, // Photo Upload


                    // blocks chopped in v1.17.0.29
{ "616D1A98-067D-43B8-B7F5-41FB12FB894E", "53A34D60-31B8-4D22-BC42-E3B669ED152B" }, // Auth Client List
{ "312EAD0E-4068-4211-8410-2EB45B7D8BAB", "8246EF8B-27E9-449E-9CAB-1C267B31DBC2" }, // Auth Client Detail
{ "7EFD5D30-2FF0-4C75-86A2-984A8F45D8A5", "63F5509A-3D71-4F0F-A074-FA5869856038" }, // Consumer List
{ "D4262E61-9CB2-4FF0-A7CA-90BAD1141BF5", "96C5DF9E-6F5C-4E55-92F1-61FE16A18563" }, // Attribute Matrix Template Detail
{ "BEC5B592-9E9E-4C55-BD0D-2B8065A1802E", "23E3CA31-6A1F-43CB-AC06-374BD9CB9FA5" }, // Note Type List
{ "5DA1D088-2142-4645-AF9C-EF52DA5B4EEA", "9E901A5A-82C2-4788-9623-3720FFC4DAEC" }, // Note Type Detail
{ "3C9D442B-D066-43FA-9380-98C60936992E", "662AF7BB-5B61-43C6-BDA6-A6E7AAB8FC00" }, // Media Folder Detail
{ "ACAF8CEB-18CD-4BAE-BF6A-12C08CF6D61F", "6035AC10-07A5-4EDD-A1E9-10862FC41494" }, // Persisted Dataset Detail
{ "E7546752-C3DC-4B96-88D9-A431F2D1C989", "6C9E7EBF-8F27-48EF-94C4-900AC3A2C167" }, // Personal Link List
{ "78DB9184-97CF-4FC4-BD71-8F3ABE4100BA", "3D13455F-7E5C-46F7-975A-4A5CE12BD330" }, // Financial Statement Template Detail
{ "49F3D87E-BD8D-43D4-8217-340F3DFF4562", "CDAB601D-1369-44CB-A146-4E80C7D66BCD" }, // Apple TV App Detail
{ "5D58BF6A-3914-420C-9013-53CE8A15E390", "A8062FE5-5BCD-48AC-8C37-2124462656A7" }, // Workflow Trigger Detail
{ "005E5980-E2D2-4958-ACB6-BECBC6D1F5C4", "F140B415-9BB3-4492-844E-5A529517A484" }, // Tag Report
                    // blocks chopped in v1.17.0.28
{ "08189564-1245-48F8-86CC-560F4DD48733", "D0203B97-5856-437E-8700-8846309F8EED" }, // Location Detail
{ "5144ED5B-89A9-4D77-B0E5-695070BE0C8E", "8A5AF4F4-32A2-426F-8363-57AC4F02A6F6" }, // Location List
{ "92E4BFE8-DF80-49D7-819D-417E579E282D", "C0CFDAB7-BB29-499E-BD0A-468B0856C037" }, // Registration List Lava
{ "1270E3F7-5ACB-4044-94CD-E2B4368FF391", "DA7834C6-C5C6-470B-B1C8-9AFA492151F8" }, // Group Requirement Type List
{ "68FC983E-05F0-4067-83AC-97DD226F5071", "C17B6D03-FDF3-4DD7-B9A9-3D6159A838F5" }, // Group Requirement Type Detail
{ "E9AB79D9-429F-410D-B4A8-327829FC7C63", "8B65EE51-4075-4FC0-B1A9-F56C7153AA77" }, // Person Signal Type Detail
{ "23818F47-D81E-4B6E-B89B-045B1FAD4C2B", "0C01496C-B4FD-4335-A11F-9B3D14D3C0E1" }, // Suggestion List
                    // blocks chopped in v1.17.0.27
{ "052B84EA-0C34-4A07-AC4C-1FBCEC87C223", "E18AB976-6665-48A5-B418-8FAC8F374135" }, // Suggestion Detail
{ "14293AEB-B0F5-434B-844A-66592AE3A416", "7E2DFB55-F1AB-4452-A5DF-6CE65FBFDDAD" }, // Photo Opt-Out
{ "0DE16268-BD5B-4CFC-A7C6-F1E07F47527A", "F61C0FDF-E8A0-457A-B8AF-42CAC8A18718" }, // Benevolence Type List
{ "C96479B6-E309-4B1A-B024-1F1276122A13", "03397615-EF2B-4D33-BD62-A79186F56ACE" }, // Benevolence Type Detail
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_CHOP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: new Dictionary<string, string> {
                { "92E4BFE8-DF80-49D7-819D-417E579E282D", "LimitToOwed,MaxResults" }, // Registration List Lava
                { "BEC5B592-9E9E-4C55-BD0D-2B8065A1802E", "EntityType" }, // Note Type List
            } );
        }

        #endregion

        #endregion

        #region PrayerCategoryPageBlockSettings

        #region JC: Prayer Automation Completions

        private void PrayerCategoryPageBlockSettings_PrayerAutomationCompletionsUp()
        {
            RockMigrationHelper.UpdateSystemSetting( "core_PrayerRequestAICompletions", PrayerCategoryPageBlockSettings_PrayerRequestAICompletionTemplate().ToJson() );
        }

        /// <summary>
        /// Updates the PrayerRequest AI Completion Templates to include instructions
        /// to not edit any text when no text enhancement is requested, but name removal is.
        /// </summary>
        /// <returns>The prompts to use for Prayer Request AI completions.</returns>
        private PrayerRequest.PrayerRequestAICompletions PrayerCategoryPageBlockSettings_PrayerRequestAICompletionTemplate()
        {
            return new PrayerRequest.PrayerRequestAICompletions
            {
                PrayerRequestFormatterTemplate = @"
{%- comment -%}
This is the lava template for the Text formatting AI automation that occurs in the PrayerRequest PostSave SaveHook.
Available Lava Fields:

PrayerRequest - The PrayerRequest entity object.
EnableFixFormattingAndSpelling - True if the AI Automation AttributeValue for TextEnhancement is equal to MinorFormattingAndSpelling; otherwise false.
EnableEnhancedReadability - True if the AI Automation AttributeValue for TextEnhancement is equal to EnhanceReadability; otherwise false.    
EnableRemovalLastNames - True if the AI Automation AttributeValue for NameRemoval is equal to LastNamesOnly; otherwise false.
EnableRemovalFirstAndLastNames - True if the AI Automation AttributeValue for NameRemoval is equal to FirstAndLastNames; otherwise false.
{%- endcomment -%}

Refer to the Prayer Request below, delimited by ```Prayer Request```. Return only the modified text without any additional comments.

{%- if EnableRemovalLastNames == true and EnableRemovalFirstAndLastNames == false %}
Remove surname and family names, but leave first names in their original form from the text below.
{%- endif -%}
{%- if EnableRemovalFirstAndLastNames == true %}
Remove names, both first and last, from the text below. If the text uses a pronoun or possessive pronoun continue to use that; otherwise use generic words like: ""an individual"", ""some individuals"", ""a family"" etc.
{%- endif -%}
{%- if EnableFixFormattingAndSpelling == true and EnableEnhancedReadability == false %}
Fix any formatting and spelling mistakes, but do not change the text.
{% endif -%}
{%- if EnableEnhancedReadability == true %}
Make the request more readable and polished. Do not change words if they significantly alter the perceived meaning.
If the request is not in English and a translation is included - leave the translation in it's original form.
{% endif -%}
{%- if EnableFixFormattingAndSpelling == false and EnableEnhancedReadability == false %}
Do not modify any other text such as spelling mistakes or grammatical errors.
{% endif -%}

```Prayer Request```
{{ PrayerRequest.Text }}
```Prayer Request```
",
                PrayerRequestAnalyzerTemplate = @"
{%- comment -%}
This is the lava template for the AI analysis automation that occurs in the PrayerRequest PostSave SaveHook.
Available Lava Fields:
PrayerRequest - The PrayerRequest entity object.
ParentCategoryId - The integer identifier of the parent category (for the PrayerRequest).
AutoCategorize - True if the AI automation is configured to auto-categorize the prayer request.
ClassifySentiment - True if the AI automation is configured to classify the sentiment of the prayer request.
CheckAppropriateness - True if the AI automation is configured to check if prayer request is appropriate for public viewing.
Categories - The child categories of the currently selected Prayer Request category or children of ""All Church"" if no category is selected.
SentimentEmotions - The Sentiment Emotions DefinedType with DefinedValues from Cache (well-known Guid: C9751C20-DA81-4521-81DE-0099D6F598BA).
{%- endcomment -%}
{%- if AutoCategorize == true and Categories != empty %}
Choose the Id of the category that most closely matches the main theme of the prayer request.
{%- assign categoriesJson = '[' -%}
{%- for category in Categories -%}
    {%- capture categoriesJsonRow -%}
        {
            ""Id"": {{ category.Id }},
            ""CategoryName"": {{category.Name | ToJSON }}
        }{% unless forloop.last %},{% endunless %}
    {%- endcapture -%}
    {%- assign categoriesJson = categoriesJson | Append:categoriesJsonRow -%}
{%- endfor -%}
{%- assign categoriesJson = categoriesJson | Append: ']' %}
```Categories```
{{ categoriesJson | FromJSON | ToJSON }}
```Categories```
{% endif -%}

{%- if ClassifySentiment == true %}
Choose the Id of the sentiment that most closely matches the prayer request text.
{%- assign sentimentsJson = '[' -%}
{%- for definedValue in SentimentEmotions.DefinedValues -%}
    {%- capture sentimentsJsonRow -%}
        {
            ""Id"": {{ definedValue.Id }},
            ""Sentiment"": {{ definedValue.Value | ToJSON }}
        }{% unless forloop.last %},{% endunless %}
    {%- endcapture -%}
    {%- assign sentimentsJson = sentimentsJson | Append:sentimentsJsonRow -%}
{%- endfor -%}
{%- assign sentimentsJson = sentimentsJson | Append: ']' %}
```Sentiments```
{{ sentimentsJson | FromJSON | ToJSON }}
```Sentiments```
{% endif -%}
{%- if CheckAppropriateness == true -%}
Determine if the prayer request text is appropriate for public viewing being sensitive to privacy and legal concerns.
First names alone are ok, but pay attention to other details which might make it easy to uniquely identify an individual within a community.
{%- endif %}

```Prayer Request```
{{PrayerRequest.Text}}
```Prayer Request```
Respond with ONLY a VALID JSON object in the format below. Do not use backticks ```.
{
""sentimentId"": <The Id of the Sentiment from the list delimited by ```Sentiments``` that most closely matches the main theme of the prayer request text>,
""categoryId"": <The Id of the Category from the list delimited by ```Categories``` that most closely matches the main theme of the prayer request text>,
""isAppropriateForPublic"": <boolean value indicating whether the prayer request text is appropriate for public viewing>
}
"
            };
        }

        /// <summary>
        /// Ensures the Category Detail and Category Tree View blocks have
        /// the proper block settings on the Prayer Request Categories Page.
        /// </summary>
        private void CategoryDetailAttributes()
        {
            Sql( $@"
DECLARE @prayerRequestEntityTypeGuid NVARCHAR(40) = '{SystemGuid.EntityType.PRAYER_REQUEST}'
DECLARE @blockEntityTypeId INT = (SELECT [Id] FROM [dbo].[EntityType] WHERE [Guid] = '{SystemGuid.EntityType.BLOCK}');
DECLARE @prayerCategoriesPageGuid NVARCHAR(40) = '{SystemGuid.Page.PRAYER_CATEGORIES}';
DECLARE @categoryDetailBlockTypeGuid NVARCHAR(40) = '{SystemGuid.BlockType.OBSIDIAN_CATEGORY_DETAIL}';

-- Get the AttributeId of the EntityType block setting for the Obsidian CategoryDetail block.
DECLARE @entityTypeAttributedId INT = (
	SELECT TOP 1 a.[Id]
	FROM [dbo].[block] b
	JOIN [dbo].[BlockType] bt ON bt.Id = b.BlockTypeId
	JOIN [dbo].[Page] p ON p.Id = b.PageId
	JOIN [dbo].[Attribute] a ON a.EntityTypeId = @blockEntityTypeId
		AND a.[key] = 'EntityType'
		AND a.EntityTypeQualifierColumn = 'BlockTypeId'
		AND a.EntityTypeQualifierValue = CONVERT(VARCHAR(50), bt.Id)
	WHERE bt.[Guid] = @categoryDetailBlockTypeGuid
);

-- Get the Id of the Category Detail block on the Prayer Categories page.
DECLARE @categoryDetailBlockId INT = (
	SELECT TOP 1 b.[Id]
	from [Page] p
	JOIN [Block] b ON b.[PageId] = p.[Id]
	JOIN [BlockType] bt ON bt.[Id] = b.BlockTypeId
	WHERE p.[Guid] = @prayerCategoriesPageGuid
		AND bt.[Guid] = @categoryDetailBlockTypeGuid
);

-- Update the Value of the EntityType attribute to use the GUID if it's numeric.
UPDATE av SET
	[Value] = @prayerRequestEntityTypeGuid
FROM [dbo].[AttributeValue] av
WHERE av.AttributeId = @entityTypeAttributedId
	AND ISNUMERIC(av.[Value]) = 1

DECLARE @now DATETIMEOFFSET = SYSDATETIMEOFFSET();

-- Add AttributeValues for the Category Detail block instance.
INSERT [AttributeValue] (
	[IsSystem],
	[AttributeId],
	[EntityId],
	[Value],
	[Guid],
	[IsPersistedValueDirty],
	[CreatedDateTime],
	[ModifiedDateTime]
)
SELECT 0, @entityTypeAttributedId, @categoryDetailBlockId, @prayerRequestEntityTypeGuid, NEWID(), 1, @now, @now
WHERE NOT EXISTS (
	    SELECT *
	    FROM [dbo].[AttributeValue] ex
	    WHERE ex.[AttributeId] = @entityTypeAttributedId
		    AND ex.[EntityId] = @categoryDetailBlockId
)
" );
        }

        #endregion

        #endregion

        #region CategoryTreeViewBlockSettings

        private const string ALL_CHURCH_CATEGORY_GUID = "5A94E584-35F0-4214-91F1-D72531CC6325";
        private const string CATEGORY_TREEVIEW_BLOCK_TYPE_GUID = "ADE003C7-649B-466A-872B-B8AC952E7841";
        private const string CATEGORY_TREEVIEW_INSTANCE_GUID = "42E90A50-D8EC-4370-B970-83E48518BC26";
        private const string ENTITY_TYPE_FRIENDLY_NAME_ATTRIBUTE_GUID = "07213E2C-C239-47CA-A781-F7A908756DC2";
        private const string GENERAL_CATEGORY_GUID = "4B2D88F5-6E45-4B4B-8776-11118C8E8269";
        private const string PAGE_PARAMETER_KEY_ATTRIBUTE_GUID = "AA057D3E-00CC-42BD-9998-600873356EDB";
        private const string SHOW_ONLY_CATEGORIES_ATTRIBUTE_GUID = "845AC4E4-ACD1-40CC-96F6-8D22136C30CC";

        private void RevertCategoryTreeViewBlockSettings()
        {
            Sql( $@"
DECLARE @categoryTreeViewBlockId INT = (SELECT [Id] FROM [Block] WHERE [Guid] = '{CATEGORY_TREEVIEW_INSTANCE_GUID}');
DECLARE @SHOW_ONLY_CATEGORIES_ATTRIBUTE_GUID NVARCHAR(40) = '{SHOW_ONLY_CATEGORIES_ATTRIBUTE_GUID}';
DECLARE @showOnlyCategoriesAttributeId INT = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = @SHOW_ONLY_CATEGORIES_ATTRIBUTE_GUID);
DECLARE @pageParameterKeyAttributeId INT = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = '{PAGE_PARAMETER_KEY_ATTRIBUTE_GUID}');

-- Change the value of the Page Parameter Key attribute back to 'CategoryId'
-- where it was changed to 'PrayerRequestId'.
UPDATE av SET
	[Value] = 'CategoryId'
FROM [dbo].[AttributeValue] av
WHERE av.AttributeId = @pageParameterKeyAttributeId
	AND EntityId = @categoryTreeViewBlockId
	AND av.[Value] = 'PrayerRequestId'

DELETE [dbo].[AttributeValue] WHERE [AttributeId] = @showOnlyCategoriesAttributeId
DELETE [dbo].[Attribute] WHERE [Id] = @showOnlyCategoriesAttributeId

/* Reset the 'General' and 'All Church' categories back to system. */
UPDATE [dbo].[Category] SET
    IsSystem = 1
WHERE [Guid] IN ('{GENERAL_CATEGORY_GUID}', '{ALL_CHURCH_CATEGORY_GUID}')
" );
        }

        private void UpdateCategoryTreeViewBlockSettings()
        {
            Sql( $@"
DECLARE @blockEntityTypeId INT = (SELECT [Id] FROM [dbo].[EntityType] WHERE [Guid] = '{SystemGuid.EntityType.BLOCK}');
DECLARE @booleanFieldTypeId INT = (SELECT [Id] FROM FieldType WHERE [Guid] = '{SystemGuid.FieldType.BOOLEAN}');
DECLARE @categoryTreeViewBlockTypeId INT = (SELECT Id FROM [BlockType] WHERE [Guid] = '{CATEGORY_TREEVIEW_BLOCK_TYPE_GUID}');
DECLARE @categoryTreeViewBlockId INT = (SELECT [Id] FROM [Block] WHERE [Guid] = '{CATEGORY_TREEVIEW_INSTANCE_GUID}');

DECLARE @SHOW_ONLY_CATEGORIES_ATTRIBUTE_GUID NVARCHAR(40) = '{SHOW_ONLY_CATEGORIES_ATTRIBUTE_GUID}';
DECLARE @showOnlyCategoriesAttributeId INT = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = @SHOW_ONLY_CATEGORIES_ATTRIBUTE_GUID);
DECLARE @pageParameterKeyAttributeId INT = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = '{PAGE_PARAMETER_KEY_ATTRIBUTE_GUID}');
DECLARE @entityTypeFriendlyNameAttributeId INT = (SELECT TOP 1 Id FROM Attribute WHERE [Guid] = '{ENTITY_TYPE_FRIENDLY_NAME_ATTRIBUTE_GUID}');
DECLARE @now DATETIMEOFFSET = SYSDATETIMEOFFSET();

/* Change the 'General' and 'All Church' categories to non-system. */
UPDATE [dbo].[Category] SET
    IsSystem = 0
WHERE [Guid] IN ('{GENERAL_CATEGORY_GUID}', '{ALL_CHURCH_CATEGORY_GUID}')

/* If the Show Only Categories attribute doesn't yet exist - create it. */
IF @showOnlyCategoriesAttributeId IS NULL
BEGIN
	INSERT [Attribute] ( [IsSystem], [FieldTypeId], [Key], [Name], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Description], [Guid] )
	SELECT 
		1 [IsSystem], 
		@booleanFieldTypeId [FieldTypeId], 
		'ShowOnlyCategories' [Key], 
		'Show Only Categories' [Name], 
		12 [Order], 
		0 [IsGridColumn], 
		0 [IsMultiValue], 
		0 [IsRequired], 
		@blockEntityTypeId [EntityTypeId], 
		'BlockTypeId' [EntityTypeQualifierColumn],
		CONVERT(NVARCHAR(40), @categoryTreeViewBlockTypeId) [EntityTypeQualifierValue], 
		'Set to true to show only the categories (rather than the categorized entities) for the configured entity type.' [Description], 
		@SHOW_ONLY_CATEGORIES_ATTRIBUTE_GUID[Guid]

    -- Update the Id of the variable to the newly inserted identity.
	SET @showOnlyCategoriesAttributeId = SCOPE_IDENTITY();
END

/* Add a value to the 'Show Only Categories' AttributeValue for the CategoryTreeView on the Prayer Categories page. */
INSERT [AttributeValue] (
	[IsSystem],
	[AttributeId],
	[EntityId],
	[Value],
	[ValueAsBoolean],
	[Guid],
	[IsPersistedValueDirty],
	[CreatedDateTime],
	[ModifiedDateTime]
)
SELECT 0, @showOnlyCategoriesAttributeId, @categoryTreeViewBlockId, 'True', 1, NEWID(), 1, @now, @now
WHERE NOT EXISTS (
	    SELECT *
	    FROM [dbo].[AttributeValue] ex
	    WHERE ex.[AttributeId] = @showOnlyCategoriesAttributeId
		    AND ex.[EntityId] = @categoryTreeViewBlockId
)
AND ISNULL(@showOnlyCategoriesAttributeId, 0) > 0

/*
    Change the value of the Page Parameter Key attribute to anything except CategoryId.
    The Page Parameter Key is for identifying the selected entity who's category should be shown
    in this case it would be a Prayer Request so use PrayerRequestId even though we don't need it.
    If set to 'CategoryId' it conflicts with the behavior of the Category Tree View control.
*/
UPDATE av SET
	[Value] = 'PrayerRequestId'
FROM [dbo].[AttributeValue] av
WHERE av.AttributeId = @pageParameterKeyAttributeId
	AND EntityId = @categoryTreeViewBlockId
	AND av.[Value] = 'CategoryId'

/* Update the title of the Category Tree View to 'Prayer Request Cateogries'. */
DECLARE @categoryTreeViewTitle NVARCHAR(200) = 'Prayer Request Categories';

IF NOT EXISTS (
    SELECT *
    FROM AttributeValue
    WHERE EntityId = @categoryTreeViewBlockId
        AND AttributeId = @entityTypeFriendlyNameAttributeId
)
BEGIN
	INSERT [AttributeValue] (
		[IsSystem],
		[AttributeId],
		[EntityId],
		[Value],
		[Guid],
		[IsPersistedValueDirty],
		[CreatedDateTime],
		[ModifiedDateTime]
	)
	SELECT 
		0 [IsSystem],
		@entityTypeFriendlyNameAttributeId [AttributeId],
		@categoryTreeViewBlockId [EntityId],
		@categoryTreeViewTitle [Value],
		NEWID() [Guid],
		1 [IsPersistedValueDirty],
		@now [CreatedDateTime],
		@now [ModifiedDateTime]
END
ELSE
BEGIN
	UPDATE av SET
		[Value] = @categoryTreeViewTitle,
		ModifiedDateTime = @now
	FROM [dbo].[AttributeValue] av
	WHERE av.AttributeId = @entityTypeFriendlyNameAttributeId
		AND av.EntityId = @categoryTreeViewBlockId
        AND av.[Value] = ''
END
" );
        }

        #endregion

        #region UpdateGradingScaleDescriptions

        /// <summary>
        /// Updates the description for LearningGradingSystemScales.
        /// </summary>
        private void UpdateGradingScaleDescriptionsUp()
        {
            Sql( @"
-- Update descriptions for Grading Scales.
-- Pass/Fail should have more descriptive values
-- while A - F will be removed.
UPDATE s SET
	[Description] = [updates].[NewDescription]
from (
	SELECT 'Pass' [Name], 'A ""Pass"" grade indicates that the individual has met all the basic requirements and demonstrated sufficient understanding of the material.' NewDescription, 'C07A3227-7188-4D61-AC02-FF6AB8380AAD' [Guid]
	UNION SELECT 'Fail' [Name], 'A ""Fail"" grade indicates that the individual did not meet the minimum requirements or demonstrate an adequate understanding of the material.' NewDescription, 'BD209F2D-22E0-41A9-B425-ED42D515E13B' [Guid]
	UNION SELECT 'A' [Name], '' NewDescription, 'F96BDDD2-EA0F-4C35-90BB-0B7D9FAABD26' [Guid]
	UNION SELECT 'B' [Name], '' NewDescription, 'E8128844-04B0-4772-AB59-55F17645AB7A' [Guid]
	UNION SELECT 'C' [Name], '' NewDescription, 'A99DC539-D363-416F-BDA8-00163D186919' [Guid]
	UNION SELECT 'D' [Name], '' NewDescription, '6E6A61C3-3305-491D-80C6-1C3723468460' [Guid]
	UNION SELECT 'F' [Name], '' NewDescription, '2F7885F5-4DFB-4057-92D7-2684B4542BF7' [Guid]
) [updates]
JOIN [dbo].[LearningGradingSystemScale] s ON s.[Guid] = [updates].[Guid]
" );
        }

        private void UpdateCourseAndClassDetailPageMenuMargin()
        {
            Sql( $@"
DECLARE 
	@blockEntityTypeId INT = (SELECT Id FROM [dbo].[EntityType] WHERE [Guid] = '{SystemGuid.EntityType.BLOCK}'),

    -- The Attribute Key for the Page Menu 'Template' block setting.
    @templateAttributeKey NVARCHAR(2000) = 'Template',

	-- The 4 pages that use the PageMenu block which should be updated.
	@programCompletionsPageGuid NVARCHAR(40) = '395BE5DD-E524-4B75-A4CA-5A0548645647',
	@coursesPageGuid NVARCHAR(40) = '0E5103B8-EF4A-46C9-8F76-313A259B0A3C',
	@semestersPageGuid NVARCHAR(40) = '0D89CFE6-BA23-4AC0-AF95-1016BAEF734C',
	@currentClassesPageGuid NVARCHAR(40) = '56459D93-32DF-4151-8F6D-003B9AFF0F94';

UPDATE av SET
	[Value] = '<div class=""mb-3"">
    {{% include ''~~/Assets/Lava/PageListAsTabs.lava'' %}}
</div>'
from  [dbo].[Block] b
JOIN [dbo].[Page] p ON p.Id = b.PageId
JOIN [dbo].[BlockType] bt ON bt.[Id] = b.[BlockTypeId]
JOIN [dbo].[AttributeValue] av ON av.EntityId = b.Id
JOIN [dbo].[Attribute] a ON a.Id = av.AttributeId
WHERE a.EntityTypeId = @blockEntityTypeId
	AND a.EntityTypeQualifierColumn = 'BlockTypeId'
	AND a.EntityTypeQualifierValue = CONVERT(NVARCHAR(MAX), bt.Id)
	AND a.[Key] = @templateAttributeKey
	AND p.[Guid] IN (
		@programCompletionsPageGuid,
		@coursesPageGuid,
		@semestersPageGuid,
		@currentClassesPageGuid
	)
	AND av.[Value] = '{{% include ''~~/Assets/Lava/PageListAsTabs.lava'' %}}'
" );
        }

        #endregion

        #region MigrationRollupsForV17_0_18

        #region PA: Register block attributes for chop job in v1.17.0.31

        private void MigrationRollupsForV17_0_18_ChopBlocksUp()
        {
            MigrationRollupsForV17_0_18_RegisterBlockAttributesForChop();
            //MigrationRollupsForV17_0_18_ChopBlockTypesv17();
        }

        /// <summary>
        /// PA: Update the Attirbute Key of Block Attribute DetailsPage of Fundraising List Webforms Block.
        /// </summary>
        private void UpdateDetailsPageBlockAttributeKeyForFundraisingListWebformBLock()
        {
            Sql( "UPDATE [Attribute] SET [Key] = 'DetailPage' WHERE [Guid] = 'F17BD62D-8134-47A5-BDBC-F7F6CD07974E' AND [Key] = 'DetailsPage'" );
        }

        private void MigrationRollupsForV17_0_18_RegisterBlockAttributesForChop()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.ContentChannelTypeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.ContentChannelTypeDetail", "Content Channel Type Detail", "Rock.Blocks.Cms.ContentChannelTypeDetail, Rock.Blocks, Version=1.17.0.30, Culture=neutral, PublicKeyToken=null", false, false, "1E0CAF78-D33A-45B8-91DB-7A435158F98A" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.DefinedTypeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.DefinedTypeDetail", "Defined Type Detail", "Rock.Blocks.Core.DefinedTypeDetail, Rock.Blocks, Version=1.17.0.30, Culture=neutral, PublicKeyToken=null", false, false, "BCD79456-EBD5-4A2F-94E5-C7387B0EA4B7" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.DeviceDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.DeviceDetail", "Device Detail", "Rock.Blocks.Core.DeviceDetail, Rock.Blocks, Version=1.17.0.30, Culture=neutral, PublicKeyToken=null", false, false, "69638956-3539-44A6-9B66-520133ED6489" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.DocumentTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.DocumentTypeList", "Document Type List", "Rock.Blocks.Core.DocumentTypeList, Rock.Blocks, Version=1.17.0.30, Culture=neutral, PublicKeyToken=null", false, false, "C4442D5F-4F50-45AA-B82E-CF3DF95D9E8C" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.NoteWatchDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.NoteWatchDetail", "Note Watch Detail", "Rock.Blocks.Core.NoteWatchDetail, Rock.Blocks, Version=1.17.0.30, Culture=neutral, PublicKeyToken=null", false, false, "02EE1267-4407-48F5-B28E-428DE8297648" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.NoteWatchList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.NoteWatchList", "Note Watch List", "Rock.Blocks.Core.NoteWatchList, Rock.Blocks, Version=1.17.0.30, Culture=neutral, PublicKeyToken=null", false, false, "8FDB4340-BDDE-4797-B173-EA456A825B2A" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.RestActionList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.RestActionList", "Rest Action List", "Rock.Blocks.Core.RestActionList, Rock.Blocks, Version=1.17.0.30, Culture=neutral, PublicKeyToken=null", false, false, "C8EE0E9B-7F66-488C-B3A6-357EBC62B174" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.ScheduleList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.ScheduleList", "Schedule List", "Rock.Blocks.Core.ScheduleList, Rock.Blocks, Version=1.17.0.30, Culture=neutral, PublicKeyToken=null", false, false, "259B6074-EEFA-4638-A7ED-C2169F450BEE" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.FinancialAccountDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialAccountDetail", "Financial Account Detail", "Rock.Blocks.Finance.FinancialAccountDetail, Rock.Blocks, Version=1.17.0.30, Culture=neutral, PublicKeyToken=null", false, false, "76D45D23-1291-4829-A1FD-D3680DCC7DB1" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.FinancialAccountList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialAccountList", "Financial Account List", "Rock.Blocks.Finance.FinancialAccountList, Rock.Blocks, Version=1.17.0.30, Culture=neutral, PublicKeyToken=null", false, false, "20CBCD56-E896-41DE-AD82-0E3862D502B3" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Group.GroupArchivedList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Group.GroupArchivedList", "Group Archived List", "Rock.Blocks.Group.GroupArchivedList, Rock.Blocks, Version=1.17.0.30, Culture=neutral, PublicKeyToken=null", false, false, "B67A0C89-1550-4960-8AAF-BAA713BE3277" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Reporting.InteractionDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Reporting.InteractionDetail", "Interaction Detail", "Rock.Blocks.Reporting.InteractionDetail, Rock.Blocks, Version=1.17.0.30, Culture=neutral, PublicKeyToken=null", false, false, "A2A1C452-6916-4C91-AB96-DF744512032A" );


            // Add/Update Obsidian Block Type
            //   Name:Content Channel Type Detail
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.ContentChannelTypeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Content Channel Type Detail", "Displays the details for a content channel type.", "Rock.Blocks.Cms.ContentChannelTypeDetail", "CMS", "2AD9E6BC-F764-4374-A714-53E365D77A36" );

            // Add/Update Obsidian Block Type
            //   Name:Defined Type Detail
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.DefinedTypeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Defined Type Detail", "Displays the details of a particular defined type.", "Rock.Blocks.Core.DefinedTypeDetail", "Core", "73FD23B4-FA3A-49EA-B271-FFB228C6A49E" );

            // Add/Update Obsidian Block Type
            //   Name:Device Detail
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.DeviceDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Device Detail", "Displays the details of the given device.", "Rock.Blocks.Core.DeviceDetail", "Core", "E3B5DB5C-280F-461C-A6E3-64462C9B329D" );

            // Add/Update Obsidian Block Type
            //   Name:Document Type List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.DocumentTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Document Type List", "Displays a list of document types.", "Rock.Blocks.Core.DocumentTypeList", "Core", "5F3151BF-577D-485B-9EE3-90F3F86F5739" );

            // Add/Update Obsidian Block Type
            //   Name:Note Watch Detail
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.NoteWatchDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Note Watch Detail", "Displays the details of a note watch.", "Rock.Blocks.Core.NoteWatchDetail", "Core", "B1F65833-CECA-4054-BCC3-2DE5692741ED" );

            // Add/Update Obsidian Block Type
            //   Name:Note Watch List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.NoteWatchList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Note Watch List", "Displays a list of note watches.", "Rock.Blocks.Core.NoteWatchList", "Core", "ED4CD6AE-ED86-4607-A252-F15971E4F2E3" );

            // Add/Update Obsidian Block Type
            //   Name:Rest Action List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.RestActionList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Rest Action List", "Displays a list of rest actions.", "Rock.Blocks.Core.RestActionList", "Core", "2EAFA987-79C6-4477-A181-63392AA24D20" );

            // Add/Update Obsidian Block Type
            //   Name:Schedule List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.ScheduleList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Schedule List", "Lists all the schedules.", "Rock.Blocks.Core.ScheduleList", "Core", "B6A17E77-E53D-4C96-BCB2-643123B8160C" );

            // Add/Update Obsidian Block Type
            //   Name:Account Detail
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.FinancialAccountDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Account Detail", "Displays the details of the given financial account.", "Rock.Blocks.Finance.FinancialAccountDetail", "Finance", "C0C464C0-2C72-449F-B46F-8E31C1DAF29B" );

            // Add/Update Obsidian Block Type
            //   Name:Account List
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.FinancialAccountList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Account List", "Displays a list of financial accounts.", "Rock.Blocks.Finance.FinancialAccountList", "Finance", "57BABD60-2A45-43AC-8ED3-B09AF79C54AB" );

            // Add/Update Obsidian Block Type
            //   Name:Group Archived List
            //   Category:Utility
            //   EntityType:Rock.Blocks.Group.GroupArchivedList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Group Archived List", "Lists Groups that have been archived.", "Rock.Blocks.Group.GroupArchivedList", "Utility", "972AD143-8294-4462-B2A7-1B36EA127374" );

            // Add/Update Obsidian Block Type
            //   Name:Interaction Detail
            //   Category:Reporting
            //   EntityType:Rock.Blocks.Reporting.InteractionDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Interaction Detail", "Presents the details of a interaction using Lava", "Rock.Blocks.Reporting.InteractionDetail", "Reporting", "011AEDE7-B036-4F4A-BF3E-4C284DC45DE8" );



            // Attribute for BlockType
            //   BlockType: Content Channel Type List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29227FC7-8F24-44B1-A0FB-E6A8694F1C3B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the content channel type details.", 0, @"", "1FE6EC0B-F714-45BD-AD00-A5E1F1DAF27E" );

            // Attribute for BlockType
            //   BlockType: Content Channel Type List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29227FC7-8F24-44B1-A0FB-E6A8694F1C3B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "569A371C-30DA-4588-9368-4A5F72CC8335" );

            // Attribute for BlockType
            //   BlockType: Content Channel Type List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29227FC7-8F24-44B1-A0FB-E6A8694F1C3B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "AA12725C-136A-4535-A61B-EAEE7163B009" );

            // Attribute for BlockType
            //   BlockType: Defined Type Detail
            //   Category: Core
            //   Attribute: Defined Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "73FD23B4-FA3A-49EA-B271-FFB228C6A49E", "BC48720C-3610-4BCF-AE66-D255A17F1CDF", "Defined Type", "DefinedType", "Defined Type", @"If a Defined Type is set, only details for it will be displayed (regardless of the querystring parameters).", 0, @"", "AED9D410-387D-4E28-A77E-3C0B6DA6C728" );

            // Attribute for BlockType
            //   BlockType: Device Detail
            //   Category: Core
            //   Attribute: Map Style
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E3B5DB5C-280F-461C-A6E3-64462C9B329D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Map Style", "MapStyle", "Map Style", @"The map theme that should be used for styling the GeoPicker map.", 0, @"FDC5D6BA-A818-4A06-96B1-9EF31B4087AC", "2083B166-F6B0-4E06-A4C0-A540A82AA5F3" );


            // Attribute for BlockType
            //   BlockType: Document Type List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F3151BF-577D-485B-9EE3-90F3F86F5739", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the document type details.", 0, @"", "62A960C3-16B7-494B-A10B-83E1DDDB1CAF" );

            // Attribute for BlockType
            //   BlockType: Document Type List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F3151BF-577D-485B-9EE3-90F3F86F5739", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "D9EF034F-2371-42A8-B0B7-AFF5F395A9EA" );

            // Attribute for BlockType
            //   BlockType: Document Type List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F3151BF-577D-485B-9EE3-90F3F86F5739", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "0A5F776F-FEE2-46A7-B25A-D0D130586199" );

            // Attribute for BlockType
            //   BlockType: Note Watch Detail
            //   Category: Core
            //   Attribute: Watched Note Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "361F15FC-4C08-4A26-B482-CC260E708F7C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Watched Note Lava Template", "WatchedNoteLavaTemplate", "Watched Note Lava Template", @"The Lava template to use to show the watched note type. <span class='tip tip-lava'></span>", 0, @"", "0FCE76C2-6312-46D9-B951-69B76343412E" );

            // Attribute for BlockType
            //   BlockType: Note Watch List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "ED4CD6AE-ED86-4607-A252-F15971E4F2E3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the note watch details.", 0, @"", "C2EC1DEB-3342-400E-BCBB-A4F7D0B06DEA" );

            // Attribute for BlockType
            //   BlockType: Note Watch List
            //   Category: Core
            //   Attribute: Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "ED4CD6AE-ED86-4607-A252-F15971E4F2E3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "EntityType", "Entity Type", @"Set an Entity Type to limit this block to Note Types and Entities for a specific entity type.", 0, @"", "D1DF8893-4502-4D45-8A8C-E5065B492BCD" );

            // Attribute for BlockType
            //   BlockType: Note Watch List
            //   Category: Core
            //   Attribute: Note Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "ED4CD6AE-ED86-4607-A252-F15971E4F2E3", "E3FF88AC-13F6-4DF8-8371-FC0D7FD9A571", "Note Type", "NoteType", "Note Type", @"Set Note Type to limit this block to a specific note type", 1, @"", "C310016B-EAE6-4F4C-A910-0AA06EAAB8B8" );

            // Attribute for BlockType
            //   BlockType: Note Watch List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "ED4CD6AE-ED86-4607-A252-F15971E4F2E3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "B7A303F4-CEC1-40F8-BE2A-223867F0D642" );

            // Attribute for BlockType
            //   BlockType: Note Watch List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "ED4CD6AE-ED86-4607-A252-F15971E4F2E3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F7E14700-B89D-4298-B5EF-6AAD44298490" );

            // Attribute for BlockType
            //   BlockType: Rest Action List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2EAFA987-79C6-4477-A181-63392AA24D20", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the rest action details.", 0, @"", "316C277E-08FC-4908-A50E-B592B07A7C1D" );

            // Attribute for BlockType
            //   BlockType: Rest Action List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2EAFA987-79C6-4477-A181-63392AA24D20", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "81F4B430-398C-462B-8AB7-06DEE3F9DFBC" );

            // Attribute for BlockType
            //   BlockType: Rest Action List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2EAFA987-79C6-4477-A181-63392AA24D20", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "994D01F2-5F0E-40FB-8DEB-A850133B93D6" );

            // Attribute for BlockType
            //   BlockType: Schedule List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B6A17E77-E53D-4C96-BCB2-643123B8160C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "76D22794-4E77-48C5-B9A6-8EF0702A0282" );

            // Attribute for BlockType
            //   BlockType: Schedule List
            //   Category: Core
            //   Attribute: Filter Category From Query String
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B6A17E77-E53D-4C96-BCB2-643123B8160C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Filter Category From Query String", "FilterCategoryFromQueryString", "Filter Category From Query String", @"", 1, @"False", "2260FA88-8502-47D7-A573-0BF1B3A9A1D2" );

            // Attribute for BlockType
            //   BlockType: Schedule List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B6A17E77-E53D-4C96-BCB2-643123B8160C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "41C80642-2841-4844-B62A-E400725BA5DF" );

            // Attribute for BlockType
            //   BlockType: Schedule List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B6A17E77-E53D-4C96-BCB2-643123B8160C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "18C9D352-F7D1-4BA5-8AEA-9408A1FE520F" );

            // Attribute for BlockType
            //   BlockType: Account List
            //   Category: Finance
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "57BABD60-2A45-43AC-8ED3-B09AF79C54AB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the financial account details.", 0, @"", "DFC76B4B-C988-4FFA-94EC-BF47E3398613" );

            // Attribute for BlockType
            //   BlockType: Account List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "57BABD60-2A45-43AC-8ED3-B09AF79C54AB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "43D29DF9-D603-457E-897E-B7AFC93BF856" );

            // Attribute for BlockType
            //   BlockType: Account List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "57BABD60-2A45-43AC-8ED3-B09AF79C54AB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "2FFDF5DC-4165-425B-A600-7FE2B91367D0" );

            // Attribute for BlockType
            //   BlockType: Group Archived List
            //   Category: Utility
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "972AD143-8294-4462-B2A7-1B36EA127374", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "B159731D-BF74-4DD2-87F7-84D2354E661C" );

            // Attribute for BlockType
            //   BlockType: Group Archived List
            //   Category: Utility
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "972AD143-8294-4462-B2A7-1B36EA127374", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "095B940C-C4FB-4170-BAE7-C22072D74F5A" );

            // Attribute for BlockType
            //   BlockType: Interaction Detail
            //   Category: Reporting
            //   Attribute: Default Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "011AEDE7-B036-4F4A-BF3E-4C284DC45DE8", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Default Template", "DefaultTemplate", "Default Template", @"The Lava template to use as default.", 2, @"<div class='panel panel-block'>
        <div class='panel-heading'>
	        <h1 class='panel-title'>
                <i class='fa fa-user'></i>
                Interaction Detail
            </h1>
        </div>
        <div class='panel-body'>
            <div class='row'>
                <div class='col-md-6'>
                    <dl>
                        <dt>Channel</dt><dd>{{ InteractionChannel.Name }}<dd/>
                        <dt>Date / Time</dt><dd>{{ Interaction.InteractionDateTime }}<dd/>
                        <dt>Operation</dt><dd>{{ Interaction.Operation }}<dd/>
                        {% if InteractionEntityName != '' %}
                            <dt>Entity Name</dt><dd>{{ InteractionEntityName }}<dd/>
                        {% endif %}
                    </dl>
                </div>
                <div class='col-md-6'>
                    <dl>
                        <dt> Component</dt><dd>{{ InteractionComponent.Name }}<dd/>
                        {% if Interaction.PersonAlias.Person.FullName != '' %}
                            <dt>Person</dt><dd>{{ Interaction.PersonAlias.Person.FullName }}<dd/>
                        {% endif %}
                        {% if Interaction.InteractionSummary and Interaction.InteractionSummary != '' %}
                            <dt>Interaction Summary</dt><dd>{{ Interaction.InteractionSummary }}<dd/>
                        {% endif %}
                        {% if Interaction.InteractionData and Interaction.InteractionData != '' %}
                            <dt>Interaction Data</dt><dd>{{ Interaction.InteractionData }}<dd/>
                        {% endif %}
                    </dl>
                </div>
            </div>
        </div>
    </div>", "72C90A4A-ACB7-4B61-9040-369666A52C33" );

        }

        // PA: Chop blocks for v1.17.0.31
        private void MigrationRollupsForV17_0_18_ChopBlockTypesv17()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types - 1.17.0.31",
                blockTypeReplacements: new Dictionary<string, string> {
{ "41CD9629-9327-40D4-846A-1BB8135D130C", "dbcfb477-0553-4bae-bac9-2aec38e1da37" }, // Registration Instance - Fee List
{ "0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4", "5ecca4fb-f8fb-49db-96b7-082bb4e4c170" }, // Assessment List
{ "E8718774-3DCA-4AFF-9F4C-FBA50A00BB57", "ed4cd6ae-ed86-4607-a252-f15971e4f2e3" }, // Note Watch List
{ "361F15FC-4C08-4A26-B482-CC260E708F7C", "b1f65833-ceca-4054-bcc3-2de5692741ed" }, // Note Watch Detail
{ "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "f431f950-f007-493e-81c8-16559fe4c0f0" }, // Defined Value List
{ "08C35F15-9AF7-468F-9D50-CDFD3D21220C", "73fd23b4-fa3a-49ea-b271-ffb228c6a49e" }, // Defined Type Detail
{ "7BF616C1-CE1D-4EF0-B56F-B9810B811192", "a6d8bfd9-0c3d-4f1e-ae0d-325a9c70b4c8" }, // REST Controller List
{ "20AD75DD-0DF3-49E9-9DB1-8537C12B1664", "2eafa987-79c6-4477-a181-63392aa24d20" }, // Rest Action List
{ "87A5A2E4-E6BA-4F3C-A6F2-ED046A04062E", "57babd60-2a45-43ac-8ed3-b09af79c54ab" }, // Account List
{ "DCD63280-B661-48AA-8DEB-F5ED63C7AB77", "c0c464c0-2c72-449f-b46f-8e31c1daf29b" }, // Account Detail (Finance)
{ "E30354A1-A1B8-4BE5-ADCE-43EEDDEF6C65", "507F5108-FB55-48F0-A66E-CC3D5185D35D" }, // Campus Detail
{ "B3E4584A-D3C3-4F68-9B7C-D1641B9B08CF", "b150e767-e964-460c-9ed1-b293474c5f5d" }, // Tag Detail
{ "AD5B3A8A-2111-4FC4-A026-51EEB4929CBA", "972ad143-8294-4462-b2a7-1b36ea127374" }, // Group Archived List
{ "C1B934D1-2139-471E-B2B8-B22FF4499B2F", "b6a17e77-e53d-4c96-bcb2-643123b8160c" }, // Schedule List
{ "C679A2C6-8126-4EF5-8C28-269A51EC4407", "5f3151bf-577d-485b-9ee3-90f3f86f5739" }, // Document Type List
{ "85E9AA73-7C96-4731-8DD6-AA604C35E536", "fd3eb724-1afa-4507-8850-c3aee170c83b" }, // Document Type Detail
{ "4280625A-C69A-4B47-A4D3-89B61F43C967", "d9510038-0547-45f3-9eca-c2ca85e64416" }, // Web Farm Settings
{ "B6AD2D98-0DF3-4DFB-AE2B-A8CF6E21E5C0", "011aede7-b036-4f4a-bf3e-4c284dc45de8" }, // Interaction Detail
{ "4AAE3DB5-C9F8-4985-B6DC-9037B2F91100", "054a8469-a838-4708-b18f-9f2819346298" }, // Fundraising Donation List
{ "8CD3C212-B9EE-4258-904C-91BA3570EE11", "e3b5db5c-280f-461c-a6e3-64462c9b329d" }, // Device Detail
{ "678ED4B6-D76F-4D43-B069-659E352C9BD8", "e07607c6-5428-4ccf-a826-060f48cacd32" }, // Attendance List
{ "451E9690-D851-4641-8BA0-317B65819918", "2ad9e6bc-f764-4374-a714-53e365d77a36" }, // Content Channel Type Detail
{ "E664BB02-D501-40B0-AAD6-D8FA0E63438B", "699ed6d1-e23a-4757-a0a2-83c5406b658a" }, // Fundraising List

                    // blocks chopped in v1.17.0.30
{ "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "5AA30F53-1B7D-4CA9-89B6-C10592968870" }, // Prayer Request Entry
{ "74B6C64A-9617-4745-9928-ABAC7948A95D", "C64F92CC-38A6-4562-8EAE-D4F30B4AF017" }, // Mobile Layout Detail
{ "092BFC5F-A291-4472-B737-0C69EA33D08A", "3852E96A-9270-4C0E-A0D0-3CD9601F183E" }, // Lava Shortcode Detail
{ "0C3CEBAD-3CCB-493B-9CBA-9D0D33852050", "04AB8A15-1D0A-4F53-84FE-7B0DE611EB02" }, // Event List
{ "0BFD74A8-1888-4407-9102-D3FCEABF3095", "904DB731-4A40-494C-B52C-95CF0F54C21F" }, // Personal Link Section List
{ "160DABF9-3549-447C-9E76-6CFCCCA481C0", "1228F248-6AA1-4871-AF9E-195CF0FDA724" }, // Verify Photo
{ "CE06640D-C1BA-4ACE-AF03-8D733FD3247C", "DBFA9E41-FA62-4869-8A44-D03B561433B2" }, // User Login List
{ "7764E323-7460-4CB7-8024-056136C99603", "C523CABA-A32C-46A3-A8B4-8F962CDC6A78" }, // Photo Upload
                    // blocks chopped in v1.17.0.29
{ "616D1A98-067D-43B8-B7F5-41FB12FB894E", "53A34D60-31B8-4D22-BC42-E3B669ED152B" }, // Auth Client List
{ "312EAD0E-4068-4211-8410-2EB45B7D8BAB", "8246EF8B-27E9-449E-9CAB-1C267B31DBC2" }, // Auth Client Detail
{ "7EFD5D30-2FF0-4C75-86A2-984A8F45D8A5", "63F5509A-3D71-4F0F-A074-FA5869856038" }, // Consumer List
{ "D4262E61-9CB2-4FF0-A7CA-90BAD1141BF5", "96C5DF9E-6F5C-4E55-92F1-61FE16A18563" }, // Attribute Matrix Template Detail
{ "BEC5B592-9E9E-4C55-BD0D-2B8065A1802E", "23E3CA31-6A1F-43CB-AC06-374BD9CB9FA5" }, // Note Type List
{ "5DA1D088-2142-4645-AF9C-EF52DA5B4EEA", "9E901A5A-82C2-4788-9623-3720FFC4DAEC" }, // Note Type Detail
{ "3C9D442B-D066-43FA-9380-98C60936992E", "662AF7BB-5B61-43C6-BDA6-A6E7AAB8FC00" }, // Media Folder Detail
{ "ACAF8CEB-18CD-4BAE-BF6A-12C08CF6D61F", "6035AC10-07A5-4EDD-A1E9-10862FC41494" }, // Persisted Dataset Detail
{ "E7546752-C3DC-4B96-88D9-A431F2D1C989", "6C9E7EBF-8F27-48EF-94C4-900AC3A2C167" }, // Personal Link List
{ "78DB9184-97CF-4FC4-BD71-8F3ABE4100BA", "3D13455F-7E5C-46F7-975A-4A5CE12BD330" }, // Financial Statement Template Detail
{ "49F3D87E-BD8D-43D4-8217-340F3DFF4562", "CDAB601D-1369-44CB-A146-4E80C7D66BCD" }, // Apple TV App Detail
{ "5D58BF6A-3914-420C-9013-53CE8A15E390", "A8062FE5-5BCD-48AC-8C37-2124462656A7" }, // Workflow Trigger Detail
{ "005E5980-E2D2-4958-ACB6-BECBC6D1F5C4", "F140B415-9BB3-4492-844E-5A529517A484" }, // Tag Report
                    // blocks chopped in v1.17.0.28
{ "08189564-1245-48F8-86CC-560F4DD48733", "D0203B97-5856-437E-8700-8846309F8EED" }, // Location Detail
{ "5144ED5B-89A9-4D77-B0E5-695070BE0C8E", "8A5AF4F4-32A2-426F-8363-57AC4F02A6F6" }, // Location List
{ "92E4BFE8-DF80-49D7-819D-417E579E282D", "C0CFDAB7-BB29-499E-BD0A-468B0856C037" }, // Registration List Lava
{ "1270E3F7-5ACB-4044-94CD-E2B4368FF391", "DA7834C6-C5C6-470B-B1C8-9AFA492151F8" }, // Group Requirement Type List
{ "68FC983E-05F0-4067-83AC-97DD226F5071", "C17B6D03-FDF3-4DD7-B9A9-3D6159A838F5" }, // Group Requirement Type Detail
{ "E9AB79D9-429F-410D-B4A8-327829FC7C63", "8B65EE51-4075-4FC0-B1A9-F56C7153AA77" }, // Person Signal Type Detail
{ "23818F47-D81E-4B6E-B89B-045B1FAD4C2B", "0C01496C-B4FD-4335-A11F-9B3D14D3C0E1" }, // Suggestion List
                    // blocks chopped in v1.17.0.27
{ "052B84EA-0C34-4A07-AC4C-1FBCEC87C223", "E18AB976-6665-48A5-B418-8FAC8F374135" }, // Suggestion Detail
{ "14293AEB-B0F5-434B-844A-66592AE3A416", "7E2DFB55-F1AB-4452-A5DF-6CE65FBFDDAD" }, // Photo Opt-Out
{ "0DE16268-BD5B-4CFC-A7C6-F1E07F47527A", "F61C0FDF-E8A0-457A-B8AF-42CAC8A18718" }, // Benevolence Type List
{ "C96479B6-E309-4B1A-B024-1F1276122A13", "03397615-EF2B-4D33-BD62-A79186F56ACE" }, // Benevolence Type Detail
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_CHOP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: new Dictionary<string, string> {
                { "92E4BFE8-DF80-49D7-819D-417E579E282D", "EnableDebug,LimitToOwed,MaxResults" }, // Registration List Lava
                { "BEC5B592-9E9E-4C55-BD0D-2B8065A1802E", "EntityType" }, // Note Type List
                { "361F15FC-4C08-4A26-B482-CC260E708F7C", "NoteType,EntityType" }, // Note Watch Detail
                { "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "EnableDebug" }, // Prayer Request Entry
                { "C96479B6-E309-4B1A-B024-1F1276122A13", "MaximumNumberOfDocuments" } // Benevolence Type Detail
            } );
        }

        #endregion

        #region KA: Migration to Update  Google Maps Lavashortcode

        /// <summary>
        /// PA: Update Google Maps Short Code to Work without the Google Map Id
        /// </summary>
        private void MakeMapIdForGoogleMapsShortCodeOptionalUp()
        {
            // Update Shortcode
            var markup = @"
{% capture singleQuote %}'{% endcapture %}
{% capture escapedQuote %}\'{% endcapture %}
{% assign apiKey = 'Global' | Attribute:'GoogleApiKey' %}
{% assign url = 'key=' | Append:apiKey %}
{% assign id = uniqueid | Replace:'-','' %}
{% assign mapId = 'Global' | Attribute:'core_GoogleMapId' %}

{% if apiKey == """" %}
    <div class=""alert alert-warning"">
        There is no Google API key defined. Please add your key under: 'Admin Tools > General Settings > Global Attributes > Google API Key'.
    </div>
{% endif %}

{% assign markerCount = markers | Size -%}

{% assign mapCenter = center | Trim %}
{% if mapCenter == """" and markerCount > 0 -%}
    {% assign centerPoint = markers | First %}
    {% assign mapCenter = centerPoint.location %}
{% endif %}

{% assign mapZoom = zoom | Trim %}
{% if mapZoom == """" %}
    {% if markerCount == 1 -%}
        {% assign mapZoom = '11' %}
    {% else %}
        {% assign mapZoom = '10' %}
    {% endif %}
{% endif %}

{% if mapId == 'DEFAULT_MAP_ID' %}
    {% assign googleMapsUrl ='https://maps.googleapis.com/maps/api/js?key='%}
{% else %}
    {% assign googleMapsUrl ='https://maps.googleapis.com/maps/api/js?libraries=marker&key='%}
{% endif %}

{% javascript id:'googlemapsapi' url:'{{ googleMapsUrl  | Append:apiKey }}' %}{% endjavascript %}

{% if mapId == 'DEFAULT_MAP_ID' %}
    {% case markeranimation %}
    {% when 'drop' %}
        {% assign markeranimation = 'google.maps.Animation.DROP' %}
    {% when 'bounce' %}
        {% assign markeranimation = 'google.maps.Animation.BOUNCE' %}
    {% else %}
        {% assign markeranimation = 'null' %}
    {% endcase %}
{% else %}
    {% case markeranimation %}
    {% when 'drop' %}
        {% assign markeranimation = 'drop' %}
    {% when 'bounce' %}
        {% assign markeranimation = 'bounce' %}
    {% else %}
        {% assign markeranimation = null %}
    {% endcase %}
{% endif %}

{% stylesheet %}

.{{ id }} {
    width: {{ width }};
}

#map-container-{{ id }} {
    position: relative;
}

#{{ id }} {
    height: {{ height }};
    overflow: hidden;
    padding-bottom: 22.25%;
    padding-top: 30px;
    position: relative;
}

@keyframes drop {
  0% {
    transform: translateY(-200px) scaleY(0.9);
    opacity: 0;
  }
  5% {
    opacity: 0.7;
  }
  50% {
    transform: translateY(0px) scaleY(1);
    opacity: 1;
  }
  65% {
    transform: translateY(-17px) scaleY(0.9);
    opacity: 1;
  }
  75% {
    transform: translateY(-22px) scaleY(0.9);
    opacity: 1;
  }
  100% {
    transform: translateY(0px) scaleY(1);
    opacity: 1;
  }
}

.drop {
  animation: drop 0.3s linear forwards .5s;
}

@keyframes bounce {
  0%, 20%, 50%, 80%, 100% {
    transform: translateY(0);
  }
  40% {
    transform: translateY(-30px);
  }
  60% {
    transform: translateY(-15px);
  }
}

.bounce {
  animation: bounce 2s infinite;
}

{% endstylesheet %}

<div class=""map-container {{ id }}"">
    <div id=""map-container-{{ id }}""></div>
    <div id=""{{ id }}""></div>
</div>	

<script>
    // create javascript array of marker info
    var markers{{ id }} = [
        {% for marker in markers -%}
            {% assign title = '' -%}
            {% assign content = '' -%}
            {% assign icon = '' -%}
            {% assign location = marker.location | Split:',' -%}
            {% if marker.title and marker.title != '' -%}
                {% assign title = marker.title | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.content != '' -%}
                {% assign content = marker.content | StripNewlines | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.icon and marker.icon != '' -%}
                {% assign icon = marker.icon -%}
            {% endif -%}
            [{{ location[0] }}, {{ location[1] }},'{{ title }}','{{ content }}','{{ icon }}'],
        {% endfor -%}
    ];

    //Set Map
    function initialize{{ id }}() {
        var bounds = new google.maps.LatLngBounds();
        var centerLatLng = new google.maps.LatLng( {{ mapCenter }} );
        if ( isNaN( centerLatLng.lat() ) || isNaN( centerLatLng.lng() ) ) {
            centerLatLng = null;
        };

        var mapOptions = {
            zoom: {{ mapZoom }},
            scrollwheel: {{ scrollwheel }},
            draggable: {{ draggable }},
            center: centerLatLng,
            mapTypeId: '{{ maptype }}',
            zoomControl: {{ showzoom }},
            mapTypeControl: {{ showmaptype }},
            gestureHandling: '{{ gesturehandling }}',
            streetViewControl: {{ showstreetview }},
            fullscreenControl: {{ showfullscreen }}
            {% if style and style.content != """" %}
                ,styles: {{ style.content | StripNewlines | Trim }}
            {% endif %}
            {% if mapId != 'DEFAULT_MAP_ID' %}
	            ,mapId: '{{ mapId }}'
            {% endif %}
        }

        var map = new google.maps.Map(document.getElementById('{{ id }}'), mapOptions);
        var infoWindow = new google.maps.InfoWindow(), marker, i;
        // place each marker on the map  
        for( i = 0; i < markers{{ id }}.length; i++ ) {
            var position = new google.maps.LatLng(markers{{ id }}[i][0], markers{{ id }}[i][1]);
            bounds.extend(position);

            {% if mapId == 'DEFAULT_MAP_ID' %}
                marker = new google.maps.Marker({
                     position: position,
                     map: map,
                     animation: {{ markeranimation }},
                     title: markers{{ id }}[i][2],
                     icon: markers{{ id }}[i][4]
                 });

            {% else %}
                if (markers{{ id }}[i][4] != ''){
                    const glyph = document.createElement('img');
                	glyph.src = markers{{ id }}[i][4];
                    
                    marker = new google.maps.marker.AdvancedMarkerElement({
                        position: position,
                        map: map,
                        title: markers{{ id }}[i][2],
                        content: glyph
                    });
                }
                else {
                    var pin = new google.maps.marker.PinElement({
                        background: '#FE7569',
                        borderColor: '#000',
                        scale: 1,
                        glyph: null
                    });

                    marker = new google.maps.marker.AdvancedMarkerElement({
                        position: position,
                        map: map,
                        title: markers{{ id }}[i][2],
                        content: pin.element
                    });
                }

	            const content = marker.content;

    	        {% if markeranimation -%}
                // Drop animation should be onetime so remove class once animation ends.
		            {% if markeranimation == 'drop' -%}
                        content.style.opacity = ""0"";
		                content.addEventListener('animationend', (event) => {
                            content.classList.remove('{{ markeranimation }}');
                            content.style.opacity = ""1"";
                        });
                    {% endif -%}
                    content.classList.add('{{ markeranimation }}');
                {% endif -%}
            {% endif %}

            // Add info window to marker
            google.maps.event.addListener(marker, 'click', (function(marker, i) {
                return function() {
                    if (markers{{ id }}[i][3] != ''){
                        infoWindow.setContent(markers{{ id }}[i][3]);
                        infoWindow.open(map, marker);
                    }
                }
            })(marker, i));
        }

        // Center the map to fit all markers on the screen
        {% if zoom == """" and center == """" and markerCount > 1 -%}
            map.fitBounds(bounds);
        {% endif -%}

        // Resize Function
        google.maps.event.addDomListener(window, ""resize"", function() {
            var center = map.getCenter();
            if ( center ) {
                google.maps.event.trigger(map, ""resize"");
                map.setCenter(center);
            }
        });
    }

    google.maps.event.addDomListener(window, 'load', initialize{{ id }});

</script>
";

            var sql = @"
-- Update Shortcode: Google Maps
UPDATE [dbo].[LavaShortcode] SET [Markup]=N'$markup'
WHERE ([Guid]='FE298210-1307-49DF-B28B-3735A414CCA0')
";

            markup = markup.Replace( "'", "''" ).Trim();
            sql = sql.Replace( "$markup", markup );
            Sql( sql );

            // Add MapId attribute
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.TEXT,
                null,
                null,
                "Google Maps Id",
                "The map identifier that's associated with a specific map style or feature on your google console, when you reference a map ID, its associated map style is displayed in your map.",
                0,
                "DEFAULT_MAP_ID",
                "9CE0FE85-CE25-4DBA-92B7-70D480E23BA8",
                "core_GoogleMapId" );
        }

        private void MapMapIDForGoogleMapsShortCodeOptionalDown()
        {
            // Update Shortcode
            var markup = @"
{% capture singleQuote %}'{% endcapture %}
{% capture escapedQuote %}\'{% endcapture %}
{% assign apiKey = 'Global' | Attribute:'GoogleApiKey' %}
{% assign url = 'key=' | Append:apiKey %}
{% assign id = uniqueid | Replace:'-','' %}
{% assign mapId = 'Global' | Attribute:'core_GoogleMapId' %}

{% if apiKey == """" %}
    <div class=""alert alert-warning"">
        There is no Google API key defined. Please add your key under: 'Admin Tools > General Settings > Global Attributes > Google API Key'.
    </div>
{% endif %}

{% assign markerCount = markers | Size -%}

{% assign mapCenter = center | Trim %}
{% if mapCenter == """" and markerCount > 0 -%}
    {% assign centerPoint = markers | First %}
    {% assign mapCenter = centerPoint.location %}
{% endif %}

{% assign mapZoom = zoom | Trim %}
{% if mapZoom == """" %}
    {% if markerCount == 1 -%}
        {% assign mapZoom = '11' %}
    {% else %}
        {% assign mapZoom = '10' %}
    {% endif %}
{% endif %}

{% javascript id:'googlemapsapi' url:'{{ ""https://maps.googleapis.com/maps/api/js?libraries=marker&key="" | Append:apiKey }}' %}{% endjavascript %}

{% case markeranimation %}
{% when 'drop' %}
    {% assign markeranimation = 'drop' %}
{% when 'bounce' %}
    {% assign markeranimation = 'bounce' %}
{% else %}
    {% assign markeranimation = null %}
{% endcase %}

{% stylesheet %}

.{{ id }} {
    width: {{ width }};
}

#map-container-{{ id }} {
    position: relative;
}

#{{ id }} {
    height: {{ height }};
    overflow: hidden;
    padding-bottom: 22.25%;
    padding-top: 30px;
    position: relative;
}

@keyframes drop {
  0% {
    transform: translateY(-200px) scaleY(0.9);
    opacity: 0;
  }
  5% {
    opacity: 0.7;
  }
  50% {
    transform: translateY(0px) scaleY(1);
    opacity: 1;
  }
  65% {
    transform: translateY(-17px) scaleY(0.9);
    opacity: 1;
  }
  75% {
    transform: translateY(-22px) scaleY(0.9);
    opacity: 1;
  }
  100% {
    transform: translateY(0px) scaleY(1);
    opacity: 1;
  }
}

.drop {
  animation: drop 0.3s linear forwards .5s;
}

@keyframes bounce {
  0%, 20%, 50%, 80%, 100% {
    transform: translateY(0);
  }
  40% {
    transform: translateY(-30px);
  }
  60% {
    transform: translateY(-15px);
  }
}

.bounce {
  animation: bounce 2s infinite;
}

{% endstylesheet %}

<div class=""map-container {{ id }}"">
    <div id=""map-container-{{ id }}""></div>
    <div id=""{{ id }}""></div>
</div>	

<script>
    // create javascript array of marker info
    var markers{{ id }} = [
        {% for marker in markers -%}
            {% assign title = '' -%}
            {% assign content = '' -%}
            {% assign icon = '' -%}
            {% assign location = marker.location | Split:',' -%}
            {% if marker.title and marker.title != '' -%}
                {% assign title = marker.title | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.content != '' -%}
                {% assign content = marker.content | StripNewlines | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.icon and marker.icon != '' -%}
                {% assign icon = marker.icon -%}
            {% endif -%}
            [{{ location[0] }}, {{ location[1] }},'{{ title }}','{{ content }}','{{ icon }}'],
        {% endfor -%}
    ];

    //Set Map
    function initialize{{ id }}() {
        var bounds = new google.maps.LatLngBounds();
        var centerLatLng = new google.maps.LatLng( {{ mapCenter }} );
        if ( isNaN( centerLatLng.lat() ) || isNaN( centerLatLng.lng() ) ) {
            centerLatLng = null;
        };

        var mapOptions = {
            zoom: {{ mapZoom }},
            scrollwheel: {{ scrollwheel }},
            draggable: {{ draggable }},
            center: centerLatLng,
            mapTypeId: '{{ maptype }}',
            zoomControl: {{ showzoom }},
            mapTypeControl: {{ showmaptype }},
            gestureHandling: '{{ gesturehandling }}',
            streetViewControl: {{ showstreetview }},
            fullscreenControl: {{ showfullscreen }}
            {% if style and style.content != """" %}
                ,styles: {{ style.content | StripNewlines | Trim }}
            {% endif %},
	        mapId: '{{ mapId }}'
        }

        var map = new google.maps.Map(document.getElementById('{{ id }}'), mapOptions);
        var infoWindow = new google.maps.InfoWindow(), marker, i;
        // place each marker on the map  
        for( i = 0; i < markers{{ id }}.length; i++ ) {
            var position = new google.maps.LatLng(markers{{ id }}[i][0], markers{{ id }}[i][1]);
            bounds.extend(position);
	        var glyph = null;
            if (markers{{ id }}[i][4] != ''){
            	glyph = markers{{ id }}[i][4];
            }
            var pin = new google.maps.marker.PinElement({
                background: '#FE7569',
                borderColor: '#000',
                scale: 1,
                glyph: glyph
            });
            marker = new google.maps.marker.AdvancedMarkerElement({
                position: position,
                map: map,
                title: markers{{ id }}[i][2],
                content: pin.element
            });

	        const content = marker.content;

    	    {% if markeranimation -%}
            // Drop animation should be onetime so remove class once animation ends.
		        {% if markeranimation == 'drop' -%}
                    content.style.opacity = ""0"";
		            content.addEventListener('animationend', (event) => {
                        content.classList.remove('{{ markeranimation }}');
                        content.style.opacity = ""1"";
                    });
                {% endif -%}
                content.classList.add('{{ markeranimation }}');
            {% endif -%}

            // Add info window to marker
            google.maps.event.addListener(marker, 'click', (function(marker, i) {
                return function() {
                    if (markers{{ id }}[i][3] != ''){
                        infoWindow.setContent(markers{{ id }}[i][3]);
                        infoWindow.open(map, marker);
                    }
                }
            })(marker, i));
        }

        // Center the map to fit all markers on the screen
        {% if zoom == """" and center == """" and markerCount > 1 -%}
            map.fitBounds(bounds);
        {% endif -%}

        // Resize Function
        google.maps.event.addDomListener(window, ""resize"", function() {
            var center = map.getCenter();
            if ( center ) {
                google.maps.event.trigger(map, ""resize"");
                map.setCenter(center);
            }
        });
    }

    google.maps.event.addDomListener(window, 'load', initialize{{ id }});

</script>
";

            var sql = @"
-- Update Shortcode: Google Maps
UPDATE [dbo].[LavaShortcode] SET [Markup]=N'$markup'
WHERE ([Guid]='FE298210-1307-49DF-B28B-3735A414CCA0')
";

            markup = markup.Replace( "'", "''" ).Trim();
            sql = sql.Replace( "$markup", markup );
            Sql( sql );

            // Add MapId attribute
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.TEXT,
                null,
                null,
                "Google Maps Id",
                "The map identifier that's associated with a specific map style or feature on your google console, when you reference a map ID, its associated map style is displayed in your map.",
                0,
                "DEFAULT_MAP_ID",
                "9CE0FE85-CE25-4DBA-92B7-70D480E23BA8",
                "core_GoogleMapId" );
        }

        #endregion

        #endregion

        #region LMSRoutes

        private void UpdateRoutesAndBinaryFileType()
        {
            // Page and Route Guids
            var publicLearnPageGuid = "B32639B3-604F-441E-A6E4-2613C0A6BE2B";
            var publicCourseDetailPageGuid = "FCCDB330-E1EA-4DC2-971E-3900F1EC2826";
            var publicCourseListPageGuid = "5B9DEA29-8C8F-4EDD-9FCF-739061B654D3";
            var publicCourseWorkspacePageGuid = "61BE63C7-6611-4235-A6F2-B22456620F35";
            var enrollPageGuid = "4CDD7BB9-A23E-42E0-BC0B-24D0E2360993";
            var enrollPageRouteGuid = "25FC2399-4885-460B-9608-7005DB17819E";
            var courseListRouteGuid = "FA31BDF7-875A-4AAA-BD27-734FF10AF61A";
            var courseDetailRouteGuid = "6AC3D62C-488B-44C8-98AF-7D23B7B701DD";
            var courseWorkspaceRouteGuid = "E2EF9FAC-3E9B-4EC8-A21F-D01178416247";
            var programListPageGuid = "84DBEC51-EE0B-41C2-94B3-F361C4B98879";

            var enrollmentBlockTypeGuid = "E80F9006-3C00-4F36-839E-7A0883F9E229";
            var enrollmentBlockGuid = "13557A75-5374-4965-B2F6-14DC04764057";
            var pageReferenceFieldTypeGuid = "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108";

            // Attribute Guids
            var courseListPageAttributeGuid = "D80D46CC-716C-48F2-A1F0-8382A01A6957";
            var courseDetailPageAttributeGuid = "F39FC142-A196-407B-BE0D-066CD3C18525";
            var classWorkspacePageAttributeGuid = "AB0300A9-4C91-493B-BFB0-7A63514DA1E9";

            // Update the page layout for public pages to use 'Full Width' instead of 'Home Page'.
            // Except the Workspace which should not include the breadcrumbs.
            RockMigrationHelper.UpdatePageLayout( publicLearnPageGuid, SystemGuid.Layout.FULL_WIDTH );
            RockMigrationHelper.UpdatePageLayout( publicCourseListPageGuid, SystemGuid.Layout.FULL_WIDTH );
            RockMigrationHelper.UpdatePageLayout( publicCourseDetailPageGuid, SystemGuid.Layout.FULL_WIDTH );

            // Update the Public Course Detail page title to "Course Description"
            Sql( $@"
UPDATE [dbo].[Page] SET
    [InternalName] = 'Course Description',
    [PageTitle] = 'Course Description',
    [BrowserTitle] = 'Course Description'
WHERE [Guid] = '{publicCourseDetailPageGuid}'
" );

            // Unhide both the public and internal root LMS pages.
            Sql( $@"
UPDATE [Page] SET
    [DisplayInNavWhen] = {( int ) DisplayInNavWhen.WhenAllowed}
WHERE [Guid] IN ('{publicLearnPageGuid}', '{programListPageGuid}');
" );
            // Remove default breadcrumbs for the public pages
            // so that breadcrumbs continue to work even after a server restart
            // (because those blocks are implementing IBreadCrumbBlock).
            Sql( $@"
UPDATE [dbo].[Page] SET
    [BreadCrumbDisplayName] = 0
WHERE [Guid] IN ('{publicCourseListPageGuid}', '{publicCourseDetailPageGuid}')
" );

            // Add the enrollment page, route, block and initial attributes/values.
            RockMigrationHelper.AddPage( true, publicCourseDetailPageGuid, SystemGuid.Layout.FULL_WIDTH, "Enroll", "", enrollPageGuid, "", "" );

            RockMigrationHelper.AddOrUpdatePageRoute( enrollPageGuid, "learn/{LearningProgramId}/courses/{LearningCourseId}/enroll/{LearningClassId}", enrollPageRouteGuid );

            RockMigrationHelper.AddSecurityAuthForPage(
                enrollPageGuid,
                0,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUnAuthenticatedUsers,
                Guid.NewGuid().ToString() );

            // Add the linked page attributes for the enroll block.
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.PublicLearningClassEnrollment
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.PublicLearningClassEnrollment", "Public Learning Class Enrollment", "Rock.Blocks.Lms.PublicLearningClassEnrollment, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "4F9F2B15-14EF-47AB-858B-641858674AC7" );

            // Add/Update Obsidian Block Type
            //   Name:Public Learning Class Enrollment
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.PublicLearningClassEnrollment
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Public Learning Class Enrollment", "Allows the current person to enroll in a learning class.", "Rock.Blocks.Lms.PublicLearningClassEnrollment", "LMS", enrollmentBlockTypeGuid );

            // Add Block 
            //  Block Name: Public Learning Class Enrollment
            //  Page Name: Enroll
            //  Layout: -
            //  Site: External Website
            RockMigrationHelper.AddBlock( true, enrollPageGuid.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), enrollmentBlockTypeGuid.AsGuid(), "Enroll", "Feature", @"", @"", 1, enrollmentBlockGuid );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Course List Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( enrollmentBlockTypeGuid, pageReferenceFieldTypeGuid, "Course List Page", "CourseListPage", "Course List Page", @"Page to use for links back to the course list.", 1, @"", courseListPageAttributeGuid );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Course Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( enrollmentBlockTypeGuid, pageReferenceFieldTypeGuid, "Course Detail Page", "CourseDetailPage", "Course Detail Page", @"Page to use for links back to the course detail.", 1, @"", courseDetailPageAttributeGuid );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Course Workspace Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( enrollmentBlockTypeGuid, pageReferenceFieldTypeGuid, "Class Workspace Page", "ClassWorkspacePage", "Class Workspace Page", @"Page to use for links to the class workspace", 1, @"", classWorkspacePageAttributeGuid );

            // Add Block Attribute Value
            //   Block: Public Learning Class Enrollment
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Block Location: Page=Enroll, Site=External Site
            //   Attribute: Course List Page
            /*   Attribute Value: PAGE_GUID,ROUTE_GUID */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, enrollmentBlockGuid, courseListPageAttributeGuid, $@"{publicCourseListPageGuid},{courseListRouteGuid}" );

            // Add Block Attribute Value
            //   Block: Public Learning Class Enrollment
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Block Location: Page=Enroll, Site=External Site
            //   Attribute: Course Detail Page
            /*   Attribute Value: PAGE_GUID,ROUTE_GUID */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, enrollmentBlockGuid, courseDetailPageAttributeGuid, $@"{publicCourseDetailPageGuid},{courseDetailRouteGuid}" );

            // Add Block Attribute Value
            //   Block: Public Learning Class Enrollment
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Block Location: Page=Enroll, Site=External Site
            //   Attribute: Class Workspace Page
            /*   Attribute Value: PAGE_GUID,ROUTE_GUID */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, enrollmentBlockGuid, classWorkspacePageAttributeGuid, $@"{publicCourseWorkspacePageGuid},{courseWorkspaceRouteGuid}" );

            // Add Block Attribute Value
            //   Block: Course Detail
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Block Location: Page=Course, Site=External Website
            //   Attribute: Enrollment Page
            /*   Attribute Value: 61be63c7-6611-4235-a6f2-b22456620f35,e2ef9fac-3e9b-4ec8-a21f-d01178416247 */
            RockMigrationHelper.AddBlockAttributeValue( "E921788F-38EA-48F2-B80A-9B7181AB70A5", "96644CEF-4FC7-4986-B591-D6675AA38C2C", $@"{enrollPageGuid},{enrollPageRouteGuid}" );

            var programCoursesPageMenuBlockGuid = "B3C55400-76E9-42D9-9ECA-842FBFC7C123";
            var coursePageMenuTemplate = @"
{% comment %}
   Only show the Page Menu when the program is not an On-Demand ConfigurationMode.
{% endcomment %}

{% assign programId = PageParameter[""LearningProgramId""] | FromIdHash %}

{% if programId > 0 %}
    {% sql %}
        SELECT 1
        FROM [LearningProgram] 
        WHERE [Id] = '{{ programId }}'
            AND [ConfigurationMode] <> 1 -- On-Demand
    {% endsql %}
    
    {% for item in results %}
        <div class=""mb-3"">
            {% include '~~/Assets/Lava/PageListAsTabs.lava' %}
        </div>
    {% endfor %}
{% endif %}
";

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Template
            /*   Attribute Value: [see coursePageMenuTemplate variable] */
            RockMigrationHelper.AddBlockAttributeValue( programCoursesPageMenuBlockGuid, "1322186A-862A-4CF1-B349-28ECB67229BA", coursePageMenuTemplate );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: EnabledLavaCommands
            /*   Attribute Value: 'Sql '*/
            RockMigrationHelper.AddBlockAttributeValue( programCoursesPageMenuBlockGuid, "EF10B2F9-93E5-426F-8D43-8C020224670F", "Sql" );

            Sql( $@"
-- Update LMS internal page routes to use people/learn/ instead of learning/
UPDATE pr SET
	[Route] = 'people/learn' + RIGHT(pr.[Route], LEN(pr.[Route]) - LEN('learning'))
FROM [dbo].[PageRoute] pr
WHERE pr.[Route] NOT LIKE 'people/%'
	AND pr.[Guid] IN (
    'A3A52449-4B51-45CE-B91D-3AEEB42C1577', -- Learn
    'C77EBCB8-F174-4F2D-8113-48D9B0D489EA', -- Course
    '5FCE29A7-4530-4CCE-9891-C95242923EFE', -- Class
    'E2581432-C9D8-4324-97E2-BCFE6BBD0F57', -- Activity
    '827AF9A8-BF1A-4008-B4C3-3D07076ACB84', -- Participant
    'D796B863-964F-4A10-A880-9D398376851A', -- Semester
    '8C40AE8D-60C6-49DE-B7DE-BE46D8A64AA6', -- Completion
    '4F35DC5D-FBB5-4B10-91B4-BC1C6A7009E8', -- Content Page
    '864A8615-AC20-4B3A-9D5F-087E92859AD1', -- Announcement
    '56A1387A-DDDE-46D9-A23D-B19D6A3BFC50', -- Current Classes
    '1AA1F901-A07C-4F64-A8EF-70A4160C0F22', -- Completions
    '5208B1E5-BE28-44D0-9DE2-D2B1A26471AE', -- Courses
    'E0F8F5D7-99C7-4FC3-9205-D9100E1F1027' -- Semesters
)

-- Update the PageTitle for the main page on both internal and external sites.
UPDATE p SET
	InternalName = 'Learn',
	PageTitle = 'Learn',
	BrowserTitle = 'Learn'
FROM [Page] p
WHERE [Guid] IN (
	'84DBEC51-EE0B-41C2-94B3-F361C4B98879', -- Learning
	'B32639B3-604F-441E-A6E4-2613C0A6BE2B' -- Learning University (External Site)
)
	AND PageTitle IN ('Learning', 'Learning University')

-- Update the AllowAnonymous flag for Learning Management files so anyone can upload.
UPDATE bft SET
	[AllowAnonymous] = 1
FROM [dbo].[BinaryFileType] bft
WHERE bft.[Guid] = '4F55987B-5279-4D10-8C38-F320046B4BBB' -- Learning Management
" );

            var fileData = BitConverter.ToString( HotFixMigrationResource._216_open_bible_on_notebook_jpg ).Replace( "-", "" );

            Sql( $@"
-- Add the Banner Image for the External Learn Page.
DECLARE @binaryFileGuid NVARCHAR(40) = '605FD4B7-2DCA-4782-8826-95AAC6C6BAB6';
DECLARE @unsecuredFileTypeId INT = (SELECT [Id] FROM [dbo].[BinaryFileType] WHERE [Guid] = 'C1142570-8CD6-4A20-83B1-ACB47C1CD377');
DECLARE @databaseStorageEntityTypeId INT = (SELECT [Id] FROM [dbo].[EntityType] WHERE [Guid] = '0AA42802-04FD-4AEC-B011-FEB127FC85CD');
DECLARE @now DATETIMEOFFSET = SYSDATETIMEOFFSET();

INSERT [BinaryFile] ( [IsTemporary], [IsSystem], [BinaryFileTypeId], [FileName], [Description], [MimeType], [StorageEntityTypeId], [Guid], [StorageEntitySettings], [Path], [CreatedDateTime], [ModifiedDateTime], [ContentLastModified] )
SELECT [IsTemporary], [IsSystem], [BinaryFileTypeId], [FileName], [Description], [MimeType], [StorageEntityTypeId], [Guid], [StorageEntitySettings], [Path], [CreatedDateTime], [ModifiedDateTime], [ContentLastModified]
FROM (
    SELECT 
        0 [IsTemporary], 
        1 [IsSystem], 
        @unsecuredFileTypeId [BinaryFileTypeId], 
        'open_bible_on_notebook.jpg' [FileName], 
        'An open bible on a notebook. This is the default image for the Learning Hub.' [Description], 
        'image/jpeg' [MimeType], 
        @databaseStorageEntityTypeId [StorageEntityTypeId],
        @binaryFileGuid [Guid],
        '{{}}' [StorageEntitySettings],
        CONCAT('/GetImage.ashx?guid=', @binaryFileGuid) [Path],
        729642 [FileSize], 
        2400 [Width], 
        1600 [Height],
        @now [CreatedDateTime], 
        @now [ModifiedDateTime],
        @now [ContentLastModified]
) [seed]
WHERE NOT EXISTS (
    SELECT 1
    FROM [BinaryFile] [ex]
    WHERE [ex].[Guid] = [seed].[Guid]
)

DECLARE @binaryFileId INT = (SELECT [Id] FROM [dbo].[BinaryFile] WHERE [Guid] = @binaryFileGuid);
DECLARE @binaryFileDataGuid NVARCHAR(40) = '85FAE5A9-C28D-41FB-B5AA-5B5BB0499B3C';

DELETE d
from BinaryFile f
join BinaryFileData d on d.Id = f.Id
where f.[Guid] = @binaryFileGuid

INSERT [BinaryFileData] ( [Id], [Guid], [Content], [CreatedDateTime], [ModifiedDateTime] )
SELECT @binaryFileId [Id],  @binaryFileDataGuid [Guid], 0x{fileData} [Content], @now [CreatedDateTime], @now [ModifiedDateTime]
WHERE @binaryFileId IS NOT NULL
" );
        }

        #endregion

        #region DefaultGradeScaleHighlightColors

        private void SetFacilitatorPortalPage()
        {
            // Attribute for BlockType
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Attribute: Facilitator Portal Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "55F2E89B-DE57-4E24-AC6C-576956FB97C5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Facilitator Portal Page", "FacilitatorPortalPage", "Facilitator Portal Page", @"The page that will be navigated to when clicking facilitator portal link.", 1, @"", "72DFE773-DA2F-45A8-976A-6C19FD0AFE28" );
            var facilitatorPortalPageAttributeGuid = "72DFE773-DA2F-45A8-976A-6C19FD0AFE28";
            var classWorkspaceBlockGuid = "D46C2787-60BA-4776-BE6E-7F785A984922";
            var internalClassDetailPageGuid = "23D5076E-C062-4987-9985-B3A4792BF3CE";
            var interalClassDetailPageRouteGuid = "5FCE29A7-4530-4CCE-9891-C95242923EFE";

            // Add Block Attribute Value
            //   Block: Public Learning Class Workspace
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Block Location: Page=Workspace, Site=External Site
            //   Attribute: Facilitator Portal Page
            /*   Attribute Value: PAGE_GUID,ROUTE_GUID */
            //   Skip If Already Exists: false
            RockMigrationHelper.AddBlockAttributeValue( false, classWorkspaceBlockGuid, facilitatorPortalPageAttributeGuid, $@"{internalClassDetailPageGuid},{interalClassDetailPageRouteGuid}" );
        }

        /// <summary>
        /// Removes the lava templates that were persisted by the code-gen scripts in <see cref="MigrationRollupsForV17_0_10"/>.
        /// </summary>
        private void RemovePublicLavaTemplateAttributeValues()
        {
            // Add Block Attribute Value
            //   Block: Program List
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Block Location: Page=Learning University, Site=External Website
            //   Attribute: Lava Template
            /*   Attribute Value: ... */
            RockMigrationHelper.DeleteBlockAttributeValue( "B15CC3F1-766B-4469-8F95-E31011A3279F", "D7B2012B-3D59-4DE3-8328-53B7EB9F7C1C" );

            // Add Block Attribute Value
            //   Block: Course List
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=External Website
            //   Attribute: Lava Template
            /*   Attribute Value: ... */
            RockMigrationHelper.DeleteBlockAttributeValue( "7CE9134A-FF7B-4663-8B79-0CA6EED53A1A", "6ADA636C-F93A-4347-A2E6-3D9C1BDA51C0" );

            // Add Block Attribute Value
            //   Block: Course Detail
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Block Location: Page=Course, Site=External Website
            //   Attribute: Lava Template
            RockMigrationHelper.DeleteBlockAttributeValue( "E921788F-38EA-48F2-B80A-9B7181AB70A5", "0935CE10-BD61-4C85-9D6C-0269DEC2EEAC" );

            // Add Block Attribute Value
            //   Block: Class Workspace
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Block Location: Page=Class Workspace, Site=External Website
            //   Attribute: Lava Header Template
            /*   Attribute Value: ... */
            RockMigrationHelper.DeleteBlockAttributeValue( "D46C2787-60BA-4776-BE6E-7F785A984922", "F7F1BE33-AF44-4F73-9C55-0F8FF97E1B69" );

        }

        /// <summary>
        /// Sets the HighlightColors for the default grading system scales.
        /// </summary>
        private void SetGradingColors()
        {
            Sql( $@"
-- Pass/fail respectively.
UPDATE [dbo].[LearningGradingSystemScale] set HighlightColor = '#34D87D' where [Guid] = 'C07A3227-7188-4D61-AC02-FF6AB8380AAD'
UPDATE [dbo].[LearningGradingSystemScale] set HighlightColor = '#FF624F' where [Guid] = 'BD209F2D-22E0-41A9-B425-ED42D515E13B'

-- A, B, C, D, F respectively.
UPDATE [dbo].[LearningGradingSystemScale] set HighlightColor = '#34D87D' where [Guid] = 'F96BDDD2-EA0F-4C35-90BB-0B7D9FAABD26'
UPDATE [dbo].[LearningGradingSystemScale] set HighlightColor = '#CCE744' where [Guid] = 'E8128844-04B0-4772-AB59-55F17645AB7A'
UPDATE [dbo].[LearningGradingSystemScale] set HighlightColor = '#F7E64A' where [Guid] = 'A99DC539-D363-416F-BDA8-00163D186919'
UPDATE [dbo].[LearningGradingSystemScale] set HighlightColor = '#FAAE48' where [Guid] = '6E6A61C3-3305-491D-80C6-1C3723468460'
UPDATE [dbo].[LearningGradingSystemScale] set HighlightColor = '#FF624F' where [Guid] = '2F7885F5-4DFB-4057-92D7-2684B4542BF7'
" );
        }

        #endregion

        #region MigrationRollupsForV17_0_19

        #region KH: Update the Finish Lava Template Block Setting Value for Transactions

        private void UpdateFinishLavaTemplateUp()
        {
            Sql( @"UPDATE [AttributeValue]
SET [Value] = CASE 
    WHEN [Value] LIKE '%starting on {{ Transaction.NextPaymentDate | Date:''sd'' }}%'
    THEN REPLACE(
        [Value], 
        'starting on {{ Transaction.NextPaymentDate | Date:''sd'' }}', 
        '//- Updated to include EndDate
{% if Transaction.EndDate %}starting on {{ Transaction.NextPaymentDate | Date:''sd'' }} and ending on {{ Transaction.EndDate | Date:''sd'' }}{% else %}starting on {{ Transaction.NextPaymentDate | Date:''sd'' }}{% endif %}'
    )
    ELSE [Value]
END
WHERE [AttributeId] IN (
    SELECT [Id]
    FROM [Attribute]
    WHERE [Guid] IN ('9F8D74CB-6E0D-47ED-B522-F6A3E3289326', 
                     '44DDFBF9-F63E-46E3-84A3-A9FC72D9F146', 
                     '6BEE06A9-969E-4704-9DC7-6B881D7280E3')
) AND [Value] NOT LIKE '%//- Updated to include EndDate%'" );
        }

        private void UpdateFinishLavaTemplateDown()
        {
            Sql( @"UPDATE [AttributeValue]
SET [Value] = CASE 
    WHEN [Value] LIKE '%//- Updated to include EndDate
{% if Transaction.EndDate %}starting on {{ Transaction.NextPaymentDate | Date:''sd'' }} and ending on {{ Transaction.EndDate | Date:''sd'' }}{% else %}starting on {{ Transaction.NextPaymentDate | Date:''sd'' }}{% endif %}%'
    THEN REPLACE(
        [Value], 
        '//- Updated to include EndDate
{% if Transaction.EndDate %}starting on {{ Transaction.NextPaymentDate | Date:''sd'' }} and ending on {{ Transaction.EndDate | Date:''sd'' }}{% else %}starting on {{ Transaction.NextPaymentDate | Date:''sd'' }}{% endif %}', 
        'starting on {{ Transaction.NextPaymentDate | Date:''sd'' }}'
    )
    ELSE [Value]
END
WHERE [AttributeId] IN (
    SELECT [Id]
    FROM [Attribute]
    WHERE [Guid] IN ('9F8D74CB-6E0D-47ED-B522-F6A3E3289326', 
                     '44DDFBF9-F63E-46E3-84A3-A9FC72D9F146', 
                     '6BEE06A9-969E-4704-9DC7-6B881D7280E3')
) AND [Value] LIKE '%//- Updated to include EndDate%'" );
        }

        #endregion

        #region KH: Migration to Update Google Maps Lavashortcode

        private void MigrationRollupsForV17_0_19_UpdateGoogleMapsLavaShortcodeUp()
        {
            // Update Shortcode
            var markup = @"
{% capture singleQuote %}'{% endcapture %}
{% capture escapedQuote %}\'{% endcapture %}
{% assign apiKey = 'Global' | Attribute:'GoogleApiKey' %}
{% assign url = 'key=' | Append:apiKey %}
{% assign id = uniqueid | Replace:'-','' %}
{% assign mapId = mapid | Trim %}
{% if mapId == """" %}
    {% assign mapId = 'Global' | Attribute:'core_GoogleMapId' %}
{% endif %}
{% if apiKey == """" %}
    <div class=""alert alert-warning"">
        There is no Google API key defined. Please add your key under: 'Admin Tools > General Settings > Global Attributes > Google API Key'.
    </div>
{% endif %}
{% assign markerCount = markers | Size -%}
{% assign mapCenter = center | Trim %}
{% if mapCenter == """" and markerCount > 0 -%}
    {% assign centerPoint = markers | First %}
    {% assign mapCenter = centerPoint.location %}
{% endif %}
{% assign mapZoom = zoom | Trim %}
{% if mapZoom == """" %}
    {% if markerCount == 1 -%}
        {% assign mapZoom = '11' %}
    {% else %}
        {% assign mapZoom = '10' %}
    {% endif %}
{% endif %}
{% if mapId == 'DEFAULT_MAP_ID' %}
    {% assign googleMapsUrl ='https://maps.googleapis.com/maps/api/js?key='%}
{% else %}
    {% assign googleMapsUrl ='https://maps.googleapis.com/maps/api/js?libraries=marker&key='%}
{% endif %}
{% javascript id:'googlemapsapi' url:'{{ googleMapsUrl  | Append:apiKey }}' %}{% endjavascript %}
{% if mapId == 'DEFAULT_MAP_ID' %}
    {% case markeranimation %}
    {% when 'drop' %}
        {% assign markeranimation = 'google.maps.Animation.DROP' %}
    {% when 'bounce' %}
        {% assign markeranimation = 'google.maps.Animation.BOUNCE' %}
    {% else %}
        {% assign markeranimation = 'null' %}
    {% endcase %}
{% else %}
    {% case markeranimation %}
    {% when 'drop' %}
        {% assign markeranimation = 'drop' %}
    {% when 'bounce' %}
        {% assign markeranimation = 'bounce' %}
    {% else %}
        {% assign markeranimation = null %}
    {% endcase %}
{% endif %}
{% stylesheet %}
.{{ id }} {
    width: {{ width }};
}
#map-container-{{ id }} {
    position: relative;
}
#{{ id }} {
    height: {{ height }};
    overflow: hidden;
    padding-bottom: 22.25%;
    padding-top: 30px;
    position: relative;
}
@keyframes drop {
  0% {
    transform: translateY(-200px) scaleY(0.9);
    opacity: 0;
  }
  5% {
    opacity: 0.7;
  }
  50% {
    transform: translateY(0px) scaleY(1);
    opacity: 1;
  }
  65% {
    transform: translateY(-17px) scaleY(0.9);
    opacity: 1;
  }
  75% {
    transform: translateY(-22px) scaleY(0.9);
    opacity: 1;
  }
  100% {
    transform: translateY(0px) scaleY(1);
    opacity: 1;
  }
}
.drop {
  animation: drop 0.3s linear forwards .5s;
}
@keyframes bounce {
  0%, 20%, 50%, 80%, 100% {
    transform: translateY(0);
  }
  40% {
    transform: translateY(-30px);
  }
  60% {
    transform: translateY(-15px);
  }
}
.bounce {
  animation: bounce 2s infinite;
}
{% endstylesheet %}
<div class=""map-container {{ id }}"">
    <div id=""map-container-{{ id }}""></div>
    <div id=""{{ id }}""></div>
</div>	
<script>
    // create javascript array of marker info
    var markers{{ id }} = [
        {% for marker in markers -%}
            {% assign title = '' -%}
            {% assign content = '' -%}
            {% assign icon = '' -%}
            {% assign location = marker.location | Split:',' -%}
            {% if marker.title and marker.title != '' -%}
                {% assign title = marker.title | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.content != '' -%}
                {% assign content = marker.content | StripNewlines | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.icon and marker.icon != '' -%}
                {% assign icon = marker.icon -%}
            {% endif -%}
            [{{ location[0] }}, {{ location[1] }},'{{ title }}','{{ content }}','{{ icon }}'],
        {% endfor -%}
    ];
    //Set Map
    function initialize{{ id }}() {
        var bounds = new google.maps.LatLngBounds();
        var centerLatLng = new google.maps.LatLng( {{ mapCenter }} );
        if ( isNaN( centerLatLng.lat() ) || isNaN( centerLatLng.lng() ) ) {
            centerLatLng = null;
        };
        var mapOptions = {
            zoom: {{ mapZoom }},
            scrollwheel: {{ scrollwheel }},
            draggable: {{ draggable }},
            center: centerLatLng,
            mapTypeId: '{{ maptype }}',
            zoomControl: {{ showzoom }},
            mapTypeControl: {{ showmaptype }},
            gestureHandling: '{{ gesturehandling }}',
            streetViewControl: {{ showstreetview }},
            fullscreenControl: {{ showfullscreen }}
            {% if style and style.content != """" %}
                ,styles: {{ style.content | StripNewlines | Trim }}
            {% endif %}
            {% if mapId != 'DEFAULT_MAP_ID' %}
	            ,mapId: '{{ mapId }}'
            {% endif %}
        }
        var map = new google.maps.Map(document.getElementById('{{ id }}'), mapOptions);
        var infoWindow = new google.maps.InfoWindow(), marker, i;
        // place each marker on the map  
        for( i = 0; i < markers{{ id }}.length; i++ ) {
            var position = new google.maps.LatLng(markers{{ id }}[i][0], markers{{ id }}[i][1]);
            bounds.extend(position);
            {% if mapId == 'DEFAULT_MAP_ID' %}
                marker = new google.maps.Marker({
                     position: position,
                     map: map,
                     animation: {{ markeranimation }},
                     title: markers{{ id }}[i][2],
                     icon: markers{{ id }}[i][4]
                 });
            {% else %}
                if (markers{{ id }}[i][4] != ''){
                    const glyph = document.createElement('img');
                	glyph.src = markers{{ id }}[i][4];
                    
                    marker = new google.maps.marker.AdvancedMarkerElement({
                        position: position,
                        map: map,
                        title: markers{{ id }}[i][2],
                        content: glyph
                    });
                }
                else {
                    var pin = new google.maps.marker.PinElement({
                        background: '#FE7569',
                        borderColor: '#000',
                        scale: 1,
                        glyph: null
                    });
                    marker = new google.maps.marker.AdvancedMarkerElement({
                        position: position,
                        map: map,
                        title: markers{{ id }}[i][2],
                        content: pin.element
                    });
                }
	            const content = marker.content;
    	        {% if markeranimation -%}
                // Drop animation should be onetime so remove class once animation ends.
		            {% if markeranimation == 'drop' -%}
                        content.style.opacity = ""0"";
		                content.addEventListener('animationend', (event) => {
                            content.classList.remove('{{ markeranimation }}');
                            content.style.opacity = ""1"";
                        });
                    {% endif -%}
                    content.classList.add('{{ markeranimation }}');
                {% endif -%}
            {% endif %}
            // Add info window to marker
            google.maps.event.addListener(marker, 'click', (function(marker, i) {
                return function() {
                    if (markers{{ id }}[i][3] != ''){
                        infoWindow.setContent(markers{{ id }}[i][3]);
                        infoWindow.open(map, marker);
                    }
                }
            })(marker, i));
        }
        // Center the map to fit all markers on the screen
        {% if zoom == """" and center == """" and markerCount > 1 -%}
            map.fitBounds(bounds);
        {% endif -%}
        // Resize Function
        google.maps.event.addDomListener(window, ""resize"", function() {
            var center = map.getCenter();
            if ( center ) {
                google.maps.event.trigger(map, ""resize"");
                map.setCenter(center);
            }
        });
    }
    google.maps.event.addDomListener(window, 'load', initialize{{ id }});
</script>
";

            var sql = @"
-- Update Shortcode: Google Maps
UPDATE [dbo].[LavaShortcode] SET [Markup]=N'$markup'
WHERE ([Guid]='FE298210-1307-49DF-B28B-3735A414CCA0')
";

            markup = markup.Replace( "'", "''" ).Trim();
            sql = sql.Replace( "$markup", markup );
            Sql( sql );
        }

        private void MigrationRollupsForV17_0_19_UpdateGoogleMapsLavaShortcodeDown()
        {
            var markup = @"
{% capture singleQuote %}'{% endcapture %}
{% capture escapedQuote %}\'{% endcapture %}
{% assign apiKey = 'Global' | Attribute:'GoogleApiKey' %}
{% assign url = 'key=' | Append:apiKey %}
{% assign id = uniqueid | Replace:'-','' %}
{% assign mapId = 'Global' | Attribute:'core_GoogleMapId' %}
{% if apiKey == """" %}
    <div class=""alert alert-warning"">
        There is no Google API key defined. Please add your key under: 'Admin Tools > General Settings > Global Attributes > Google API Key'.
    </div>
{% endif %}
{% assign markerCount = markers | Size -%}
{% assign mapCenter = center | Trim %}
{% if mapCenter == """" and markerCount > 0 -%}
    {% assign centerPoint = markers | First %}
    {% assign mapCenter = centerPoint.location %}
{% endif %}
{% assign mapZoom = zoom | Trim %}
{% if mapZoom == """" %}
    {% if markerCount == 1 -%}
        {% assign mapZoom = '11' %}
    {% else %}
        {% assign mapZoom = '10' %}
    {% endif %}
{% endif %}
{% if mapId == 'DEFAULT_MAP_ID' %}
    {% assign googleMapsUrl ='https://maps.googleapis.com/maps/api/js?key='%}
{% else %}
    {% assign googleMapsUrl ='https://maps.googleapis.com/maps/api/js?libraries=marker&key='%}
{% endif %}
{% javascript id:'googlemapsapi' url:'{{ googleMapsUrl  | Append:apiKey }}' %}{% endjavascript %}
{% if mapId == 'DEFAULT_MAP_ID' %}
    {% case markeranimation %}
    {% when 'drop' %}
        {% assign markeranimation = 'google.maps.Animation.DROP' %}
    {% when 'bounce' %}
        {% assign markeranimation = 'google.maps.Animation.BOUNCE' %}
    {% else %}
        {% assign markeranimation = 'null' %}
    {% endcase %}
{% else %}
    {% case markeranimation %}
    {% when 'drop' %}
        {% assign markeranimation = 'drop' %}
    {% when 'bounce' %}
        {% assign markeranimation = 'bounce' %}
    {% else %}
        {% assign markeranimation = null %}
    {% endcase %}
{% endif %}
{% stylesheet %}
.{{ id }} {
    width: {{ width }};
}
#map-container-{{ id }} {
    position: relative;
}
#{{ id }} {
    height: {{ height }};
    overflow: hidden;
    padding-bottom: 22.25%;
    padding-top: 30px;
    position: relative;
}
@keyframes drop {
  0% {
    transform: translateY(-200px) scaleY(0.9);
    opacity: 0;
  }
  5% {
    opacity: 0.7;
  }
  50% {
    transform: translateY(0px) scaleY(1);
    opacity: 1;
  }
  65% {
    transform: translateY(-17px) scaleY(0.9);
    opacity: 1;
  }
  75% {
    transform: translateY(-22px) scaleY(0.9);
    opacity: 1;
  }
  100% {
    transform: translateY(0px) scaleY(1);
    opacity: 1;
  }
}
.drop {
  animation: drop 0.3s linear forwards .5s;
}
@keyframes bounce {
  0%, 20%, 50%, 80%, 100% {
    transform: translateY(0);
  }
  40% {
    transform: translateY(-30px);
  }
  60% {
    transform: translateY(-15px);
  }
}
.bounce {
  animation: bounce 2s infinite;
}
{% endstylesheet %}
<div class=""map-container {{ id }}"">
    <div id=""map-container-{{ id }}""></div>
    <div id=""{{ id }}""></div>
</div>	
<script>
    // create javascript array of marker info
    var markers{{ id }} = [
        {% for marker in markers -%}
            {% assign title = '' -%}
            {% assign content = '' -%}
            {% assign icon = '' -%}
            {% assign location = marker.location | Split:',' -%}
            {% if marker.title and marker.title != '' -%}
                {% assign title = marker.title | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.content != '' -%}
                {% assign content = marker.content | StripNewlines | HtmlDecode | Replace:singleQuote,escapedQuote -%}
            {% endif -%}
            {% if marker.icon and marker.icon != '' -%}
                {% assign icon = marker.icon -%}
            {% endif -%}
            [{{ location[0] }}, {{ location[1] }},'{{ title }}','{{ content }}','{{ icon }}'],
        {% endfor -%}
    ];
    //Set Map
    function initialize{{ id }}() {
        var bounds = new google.maps.LatLngBounds();
        var centerLatLng = new google.maps.LatLng( {{ mapCenter }} );
        if ( isNaN( centerLatLng.lat() ) || isNaN( centerLatLng.lng() ) ) {
            centerLatLng = null;
        };
        var mapOptions = {
            zoom: {{ mapZoom }},
            scrollwheel: {{ scrollwheel }},
            draggable: {{ draggable }},
            center: centerLatLng,
            mapTypeId: '{{ maptype }}',
            zoomControl: {{ showzoom }},
            mapTypeControl: {{ showmaptype }},
            gestureHandling: '{{ gesturehandling }}',
            streetViewControl: {{ showstreetview }},
            fullscreenControl: {{ showfullscreen }}
            {% if style and style.content != """" %}
                ,styles: {{ style.content | StripNewlines | Trim }}
            {% endif %}
            {% if mapId != 'DEFAULT_MAP_ID' %}
	            ,mapId: '{{ mapId }}'
            {% endif %}
        }
        var map = new google.maps.Map(document.getElementById('{{ id }}'), mapOptions);
        var infoWindow = new google.maps.InfoWindow(), marker, i;
        // place each marker on the map  
        for( i = 0; i < markers{{ id }}.length; i++ ) {
            var position = new google.maps.LatLng(markers{{ id }}[i][0], markers{{ id }}[i][1]);
            bounds.extend(position);
            {% if mapId == 'DEFAULT_MAP_ID' %}
                marker = new google.maps.Marker({
                     position: position,
                     map: map,
                     animation: {{ markeranimation }},
                     title: markers{{ id }}[i][2],
                     icon: markers{{ id }}[i][4]
                 });
            {% else %}
                if (markers{{ id }}[i][4] != ''){
                    const glyph = document.createElement('img');
                	glyph.src = markers{{ id }}[i][4];
                    
                    marker = new google.maps.marker.AdvancedMarkerElement({
                        position: position,
                        map: map,
                        title: markers{{ id }}[i][2],
                        content: glyph
                    });
                }
                else {
                    var pin = new google.maps.marker.PinElement({
                        background: '#FE7569',
                        borderColor: '#000',
                        scale: 1,
                        glyph: null
                    });
                    marker = new google.maps.marker.AdvancedMarkerElement({
                        position: position,
                        map: map,
                        title: markers{{ id }}[i][2],
                        content: pin.element
                    });
                }
	            const content = marker.content;
    	        {% if markeranimation -%}
                // Drop animation should be onetime so remove class once animation ends.
		            {% if markeranimation == 'drop' -%}
                        content.style.opacity = ""0"";
		                content.addEventListener('animationend', (event) => {
                            content.classList.remove('{{ markeranimation }}');
                            content.style.opacity = ""1"";
                        });
                    {% endif -%}
                    content.classList.add('{{ markeranimation }}');
                {% endif -%}
            {% endif %}
            // Add info window to marker
            google.maps.event.addListener(marker, 'click', (function(marker, i) {
                return function() {
                    if (markers{{ id }}[i][3] != ''){
                        infoWindow.setContent(markers{{ id }}[i][3]);
                        infoWindow.open(map, marker);
                    }
                }
            })(marker, i));
        }
        // Center the map to fit all markers on the screen
        {% if zoom == """" and center == """" and markerCount > 1 -%}
            map.fitBounds(bounds);
        {% endif -%}
        // Resize Function
        google.maps.event.addDomListener(window, ""resize"", function() {
            var center = map.getCenter();
            if ( center ) {
                google.maps.event.trigger(map, ""resize"");
                map.setCenter(center);
            }
        });
    }
    google.maps.event.addDomListener(window, 'load', initialize{{ id }});
</script>
";

            var sql = @"
-- Update Shortcode: Google Maps
UPDATE [dbo].[LavaShortcode] SET [Markup]=N'$markup'
WHERE ([Guid]='FE298210-1307-49DF-B28B-3735A414CCA0')
";

            markup = markup.Replace( "'", "''" ).Trim();
            sql = sql.Replace( "$markup", markup );
            Sql( sql );
        }

        #endregion

        #region JDR: Add to v17 Migration to unhide new page

        private void UnhideVolunteerGenerosityUp()
        {
            Sql( @"UPDATE [Page] SET [DisplayInNavWhen] = 1 WHERE [Guid] = '16DD0891-E3D4-4FF3-9857-0869A6CCBA39'" );
        }

        #endregion

        #region KH: Update Follow Icon Lava Shortcode Up

        private void UpdateFollowIconLavaShortcodeUp()
        {
            var markup = @"
{% if CurrentPerson %}
    {% assign entitytypeguid = entitytypeguid | Trim %}
    {% assign entityguid = entityguid | Trim %}
    {% assign entitytypeid = entitytypeid | Trim %}
    {% assign entityid = entityid | Trim %}
    {% if entitytypeguid != '' and entityguid != '' %}
    {% assign entitytype = entitytypeguid %}
    {% assign entity = entityguid %}
    {% else %}
    {% assign entitytype = entitytypeid %}
    {% assign entity = entityid %}
    {% endif %}
    {% assign purposekey = purposekey | Trim %}
    {% assign suppresswarnings = suppresswarnings | AsBoolean %}
    {% assign isfollowed = isfollowed | AsBoolean %}
    
    {% if entitytype != '' and entity != '' %}
        <div class=""followicon js-followicon {% if isfollowed %}isfollowed{% endif %}"" data-entitytype=""{{ entitytype }}"" data-entity=""{{ entity }}"" {% if purposekey != '' %}data-purpose-key=""{{ purposekey }}""{% endif %} data-followed=""{{ isfollowed }}"">
            {{ blockContent }}
        </div>
    
        {% javascript id:'followicon' disableanonymousfunction:'true'%}
            $(document).ready(function() {
                // Use event delegation to bind the click event
                $(document).on('click', '.js-followicon', function(e) {
                    e.preventDefault();

                    var icon = $(this);
                    var entityType = icon.data('entitytype');
                    var entity = icon.data('entity');
                    var purpose = icon.data('purpose-key');
        
                    if (purpose) {
                        purpose = '?purposeKey=' + purpose;
                    } else {
                        purpose = '';
                    }
        
                    icon.toggleClass('isfollowed');
        
                    var actionType = icon.hasClass('isfollowed') ? 'POST' : 'DELETE';
        
                    $.ajax({
                        url: '/api/Followings/' + entityType + '/' + entity + purpose,
                        type: actionType,
                        statusCode: {
                            201: function() {
                                icon.attr('data-followed', 'true');
                            },
                            204: function() {
                                icon.attr('data-followed', 'false');
                            },
                            500: function() {
                                {% unless suppresswarnings %}
                                alert('Error: Check your Rock security settings and try again.');
                                {% endunless %}
                            }
                        },
                        error: function() {
                            icon.toggleClass('isfollowed');
                        }
                    });
                });
            });
        {% endjavascript %}
    {% else %}
        <!-- Follow Icon Shortcode is missing entitytype and/or entity. Note: Guids or Ids must be provided  -->
    {% endif %}
{% endif %}
";

            var sql = @"
-- Update Shortcode: Follow Icon
UPDATE [dbo].[LavaShortcode] SET [Markup]=N'$markup'
WHERE ([Guid]='1E6785C0-7D92-49A7-9E15-68E113399152')
";

            markup = markup.Replace( "'", "''" ).Trim();
            sql = sql.Replace( "$markup", markup );
            Sql( sql );
        }

        private void UpdateFollowIconLavaShortcodeDown()
        {
            var markup = @"
{% if CurrentPerson %}
    {% assign entitytypeguid = entitytypeguid | Trim %}
    {% assign entityguid = entityguid | Trim %}
    {% assign entitytypeid = entitytypeid | Trim %}
    {% assign entityid = entityid | Trim %}
    {% if entitytypeguid != '' and entityguid != '' %}
    {% assign entitytype = entitytypeguid %}
    {% assign entity = entityguid %}
    {% else %}
    {% assign entitytype = entitytypeid %}
    {% assign entity = entityid %}
    {% endif %}
    {% assign purposekey = purposekey | Trim %}
    {% assign suppresswarnings = suppresswarnings | AsBoolean %}
    {% assign isfollowed = isfollowed | AsBoolean %}
    
    {% if entitytype != '' and entity != '' %}
        <div class=""followicon js-followicon {% if isfollowed %}isfollowed{% endif %}"" data-entitytype=""{{ entitytype }}"" data-entity=""{{ entity }}"" {% if purposekey != '' %}data-purpose-key=""{{ purposekey }}""{% endif %} data-followed=""{{ isfollowed }}"">
            {{ blockContent }}
        </div>
    
        {% javascript id:'followicon' disableanonymousfunction:'true'%}
            $( document ).ready(function() {
                $('.js-followicon').click(function(e) {
                    e.preventDefault();
                    var icon = $(this);
                    var entityType = icon.data('entitytype');
                    var entity = icon.data('entity');
                    var purpose = icon.data('purpose-key');
                    if (purpose != undefined && purpose != '') {
                        purpose = '?purposeKey=' + purpose;
                    } else {
                        purpose = '';
                    }
                    icon.toggleClass('isfollowed');
                    if ( icon.hasClass('isfollowed') ) {
                        var actionType = 'POST';
                    } else {
                        var actionType = 'DELETE';
                    }
                    $.ajax({
                        url: '/api/Followings/' + entityType + '/' + entity + purpose,
                        type: actionType,
                        statusCode: {
                            201: function() {
                                icon.attr('data-followed', 'true');
                            },
                            204: function() {
                                icon.attr('data-followed', 'false');
                            },
                            500: function() {
                                {% unless suppresswarnings %}
                                alert('Error: Check your Rock security settings and try again.');
                                {% endunless %}
                            }
                        },
                        error: function() {
                            icon.toggleClass('isfollowing');
                        }
                    });
                });
            });
        {% endjavascript %}
    {% else %}
        <!-- Follow Icon Shortcode is missing entitytype and/or entity. Note: Guids or Ids must be provided  -->
    {% endif %}
{% endif %}
";

            var sql = @"
-- Update Shortcode: Follow Icon
UPDATE [dbo].[LavaShortcode] SET [Markup]=N'$markup'
WHERE ([Guid]='1E6785C0-7D92-49A7-9E15-68E113399152')
";

            markup = markup.Replace( "'", "''" ).Trim();
            sql = sql.Replace( "$markup", markup );
            Sql( sql );
        }

        #endregion

        #region JC: Register block attributes for chop job in v1.17.0.32

        private void MigrationRollupsForV17_0_19_ChopBlocksUp()
        {
            MigrationRollupsForV17_0_19_RegisterBlockAttributesForChop();
            //MigrationRollupsForV17_0_19_ChopBlockTypesv17();
        }

        private void MigrationRollupsForV17_0_19_RegisterBlockAttributesForChop()
        {
            // FinancialPledgeList's ShowAccountColumn key was pluralized by mistake.
            // Since this hasn't been released yet - delete the incorrectly keyed attribute
            // before recreating it below.
            RockMigrationHelper.DeleteAttribute( "d685afae-3f10-4c4c-a19e-f483075774f0" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Administration.SystemConfiguration
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Administration.SystemConfiguration", "System Configuration", "Rock.Blocks.Administration.SystemConfiguration, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "7ECDCE1B-D63F-42AA-88B6-7C5585E1F33A" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.CheckIn.AttendanceHistoryList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.CheckIn.AttendanceHistoryList", "Attendance History List", "Rock.Blocks.CheckIn.AttendanceHistoryList, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "8B678DC2-25E0-4589-BC3E-765BE9729BC8" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.CheckIn.Config.CheckinTypeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.CheckIn.Config.CheckinTypeDetail", "Checkin Type Detail", "Rock.Blocks.CheckIn.Config.CheckinTypeDetail, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "7D1DEC32-3A94-45B4-B567-48D9478041B9" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.SystemCommunicationPreview
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.SystemCommunicationPreview", "System Communication Preview", "Rock.Blocks.Communication.SystemCommunicationPreview, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "D61A57A2-C067-435F-99F6-7B6BB9534058" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.PersonSignalList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.PersonSignalList", "Person Signal List", "Rock.Blocks.Core.PersonSignalList, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "DB2E3CE3-94BD-4D12-8ADD-598BF938E8E1" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Crm.AssessmentTypeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.AssessmentTypeDetail", "Assessment Type Detail", "Rock.Blocks.Crm.AssessmentTypeDetail, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "83D4C6CA-A605-44D3-8BEA-99B3E881BAA0" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.FinancialGatewayDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialGatewayDetail", "Financial Gateway Detail", "Rock.Blocks.Finance.FinancialGatewayDetail, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "68CC9376-8123-4749-ACA0-1E7ED8459704" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.FinancialPledgeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialPledgeList", "Financial Pledge List", "Rock.Blocks.Finance.FinancialPledgeList, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "8B1663EB-B5CB-4C78-B0C6-ED14E173E4C0" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Security.RestKeyDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Security.RestKeyDetail", "Rest Key Detail", "Rock.Blocks.Security.RestKeyDetail, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "AED330CA-40A4-407A-B2DC-A0C1310FDC39" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Security.SecurityChangeAuditList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Security.SecurityChangeAuditList", "Security Change Audit List", "Rock.Blocks.Security.SecurityChangeAuditList, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "5A2E4F3C-9915-4B67-8FFE-87056D2E68DF" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Tv.AppleTvPageDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Tv.AppleTvPageDetail", "Apple Tv Page Detail", "Rock.Blocks.Tv.AppleTvPageDetail, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "D8419B3C-EDA1-46FC-9810-B1D81FB37CB3" );

            // Add/Update Obsidian Block Type
            //   Name:Apple TV Page Detail
            //   Category:TV > TV Apps
            //   EntityType:Rock.Blocks.Tv.AppleTvPageDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Apple TV Page Detail", "Displays the details of an Apple TV page.", "Rock.Blocks.Tv.AppleTvPageDetail", "TV > TV Apps", "ADBF3377-A491-4016-9375-346496A25FB4" );

            // Add/Update Obsidian Block Type
            //   Name:Assessment Type Detail
            //   Category:CRM
            //   EntityType:Rock.Blocks.Crm.AssessmentTypeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Assessment Type Detail", "Displays the details of a particular assessment type.", "Rock.Blocks.Crm.AssessmentTypeDetail", "CRM", "3B8B5AE5-4139-44A6-8EAA-99D48E51134E" );

            // Add/Update Obsidian Block Type
            //   Name:Attendance History
            //   Category:Check-in
            //   EntityType:Rock.Blocks.CheckIn.AttendanceHistoryList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Attendance History", "Block for displaying the attendance history of a person or a group.", "Rock.Blocks.CheckIn.AttendanceHistoryList", "Check-in", "68D2ABBC-3C43-4450-973F-071D1715C0C9" );

            // Add/Update Obsidian Block Type
            //   Name:Check-in Type Detail
            //   Category:Check-in > Configuration
            //   EntityType:Rock.Blocks.CheckIn.Config.CheckinTypeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Check-in Type Detail", "Displays the details of a particular Check-in Type.", "Rock.Blocks.CheckIn.Config.CheckinTypeDetail", "Check-in > Configuration", "7EA2E093-2F33-4213-A33E-9E9A7A760181" );

            // Add/Update Obsidian Block Type
            //   Name:Financial Pledge List
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.FinancialPledgeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Financial Pledge List", "Displays a list of financial pledges.", "Rock.Blocks.Finance.FinancialPledgeList", "Finance", "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B" );

            // Add/Update Obsidian Block Type
            //   Name:Gateway Detail
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.FinancialGatewayDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Gateway Detail", "Displays the details of the given financial gateway.", "Rock.Blocks.Finance.FinancialGatewayDetail", "Finance", "C12C615C-384D-478E-892D-0F353E2EF180" );

            // Add/Update Obsidian Block Type
            //   Name:Person Signal List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.PersonSignalList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Person Signal List", "Displays a list of person signals.", "Rock.Blocks.Core.PersonSignalList", "Core", "653052A0-CA1C-41B8-8340-4B13149C6E66" );

            // Add/Update Obsidian Block Type
            //   Name:Rest Key Detail
            //   Category:Security
            //   EntityType:Rock.Blocks.Security.RestKeyDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Rest Key Detail", "Displays the details of a particular user login.", "Rock.Blocks.Security.RestKeyDetail", "Security", "28A34F1C-80F4-496F-A598-180974ADEE61" );

            // Add/Update Obsidian Block Type
            //   Name:Security Change Audit List
            //   Category:Security
            //   EntityType:Rock.Blocks.Security.SecurityChangeAuditList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Security Change Audit List", "Block for Security Change Audit List.", "Rock.Blocks.Security.SecurityChangeAuditList", "Security", "CFE6F48B-ED85-4FA8-B068-EFE116B32284" );

            // Add/Update Obsidian Block Type
            //   Name:System Communication Preview
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.SystemCommunicationPreview
            RockMigrationHelper.AddOrUpdateEntityBlockType( "System Communication Preview", "Create a preview and send a test message for the given system communication using the selected date and target person.", "Rock.Blocks.Communication.SystemCommunicationPreview", "Communication", "C28368CA-5218-4B59-8BD8-75BD78AA9BE9" );

            // Add/Update Obsidian Block Type
            //   Name:System Configuration
            //   Category:Administration
            //   EntityType:Rock.Blocks.Administration.SystemConfiguration
            RockMigrationHelper.AddOrUpdateEntityBlockType( "System Configuration", "Used for making configuration changes to configurable items in the web.config.", "Rock.Blocks.Administration.SystemConfiguration", "Administration", "3855B15B-C903-446A-AE5B-891AB52851CB" );

            // Attribute for BlockType
            //   BlockType: Attendance History
            //   Category: Check-in
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "68D2ABBC-3C43-4450-973F-071D1715C0C9", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "71EFE820-B132-4AAB-A702-61486E6B2FD8" );

            // Attribute for BlockType
            //   BlockType: Attendance History
            //   Category: Check-in
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "68D2ABBC-3C43-4450-973F-071D1715C0C9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "E8C27B15-4F00-4516-A624-FBB5C26DF28F" );

            // Attribute for BlockType
            //   BlockType: Attendance History
            //   Category: Check-in
            //   Attribute: Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "68D2ABBC-3C43-4450-973F-071D1715C0C9", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "Entity Type", @"The type of entity that will provide context for this block", 0, @"", "7C624BAB-A392-43C8-96C8-59B62E171EF4" );

            // Attribute for BlockType
            //   BlockType: Attendance History
            //   Category: Check-in
            //   Attribute: Filter Attendance By Default
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "68D2ABBC-3C43-4450-973F-071D1715C0C9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Filter Attendance By Default", "FilterAttendanceByDefault", "Filter Attendance By Default", @"Sets the default display of Attended to Did Attend instead of [All]", 0, @"False", "EA5AF2D0-E197-4523-8ED5-D100F1C8E245" );

            // Attribute for BlockType
            //   BlockType: Check-in Type Detail
            //   Category: Check-in > Configuration
            //   Attribute: Schedule Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7EA2E093-2F33-4213-A33E-9E9A7A760181", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Schedule Page", "SchedulePage", "Schedule Page", @"Page used to manage schedules for the check-in type.", 0, @"", "1F11C34E-09D8-4FF5-A188-D84BF333DA03" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "Accounts", "Accounts", @"Limit the results to pledges that match the selected accounts.", 5, @"", "237658C7-0DED-4BE1-8026-613E155B23B5" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "711B6CCF-A999-4582-B1A9-770BD9BAF963" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "200AFB5A-655F-4206-858B-59376CB96856" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "A7AE994B-4A18-48D9-9FBF-04CE9C00426A" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "Entity Type", @"The type of entity that will provide context for this block", 0, @"", "9BA6DDD6-E511-4CEB-8E65-3201FDE2F715" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Hide Amount
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Amount", "HideAmount", "Hide Amount", @"Allows the amount column to be hidden.", 6, @"False", "F9C562AE-EDC6-4B96-876D-933DBA58E675" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Limit Pledges To Current Person
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit Pledges To Current Person", "LimitPledgesToCurrentPerson", "Limit Pledges To Current Person", @"Limit the results to pledges for the current person.", 4, @"False", "F2E7D073-ED8C-485B-8597-8F62203134F1" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Account Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Account Column", "ShowAccountColumn", "Show Account Column", @"Allows the account column to be hidden.", 1, @"True", "D685AFAE-3F10-4C4C-A19E-F483075774F0" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Account Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Account Filter", "ShowAccountFilter", "Show Account Filter", @"Allows account filter to be hidden.", 1, @"True", "CEEE570C-013F-47DB-99F4-D3D00C5200DC" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Account Summary
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Account Summary", "ShowAccountSummary", "Show Account Summary", @"Should the account summary be displayed at the bottom of the list?", 5, @"False", "8F62CD6A-B740-47E2-8B3F-83A3CF4E06B2" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Date Range Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Date Range Filter", "ShowDateRangeFilter", "Show Date Range Filter", @"Allows date range filter to be hidden.", 2, @"True", "884EE556-66A1-4F65-B037-2CD6D0964315" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Group Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Column", "ShowGroupColumn", "Show Group Column", @"Allows the group column to be hidden.", 3, @"False", "189D6D31-8A92-43C9-A42D-DE44C663F1F9" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Last Modified Date Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Last Modified Date Column", "ShowLastModifiedDateColumn", "Show Last Modified Date Column", @"Allows the Last Modified Date column to be hidden.", 2, @"True", "E6FBE09B-437C-4281-89B4-2C323283BA64" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Last Modified Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Last Modified Filter", "ShowLastModifiedFilter", "Show Last Modified Filter", @"Allows last modified filter to be hidden.", 3, @"True", "6CE0FF15-C6E7-4667-ADA5-5F6D3AA71D90" );

            // Attribute for BlockType
            //   BlockType: Financial Pledge List
            //   Category: Finance
            //   Attribute: Show Person Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Person Filter", "ShowPersonFilter", "Show Person Filter", @"Allows person filter to be hidden.", 0, @"True", "A7F66BEA-9B90-40B4-9E86-03836EF9BF74" );

            // Attribute for BlockType
            //   BlockType: Person Signal List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "653052A0-CA1C-41B8-8340-4B13149C6E66", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "00E16ED2-BC73-41C4-BA16-471725A23547" );

            // Attribute for BlockType
            //   BlockType: Person Signal List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "653052A0-CA1C-41B8-8340-4B13149C6E66", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F6CBDC1B-B5E6-4611-9A3B-F8229E3C27EA" );

            // Attribute for BlockType
            //   BlockType: Security Change Audit List
            //   Category: Security
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CFE6F48B-ED85-4FA8-B068-EFE116B32284", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "88B7ED40-401C-4BCB-90FA-94EEE4BBC6C4" );

            // Attribute for BlockType
            //   BlockType: Security Change Audit List
            //   Category: Security
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CFE6F48B-ED85-4FA8-B068-EFE116B32284", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "56D7689D-3BDF-435B-9605-2F61BFCA07B1" );

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C28368CA-5218-4B59-8BD8-75BD78AA9BE9", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled.", 5, @"", "9BA51754-B5A4-4853-A927-2215D6DB91B3" );

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: Lava Template Append
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C28368CA-5218-4B59-8BD8-75BD78AA9BE9", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template Append", "LavaTemplateAppend", "Lava Template Append", @"This Lava will be appended to the system communication template to help setup any data that the template needs. This data would typically be passed to the template by a job or other means.", 6, @"", "2D757741-0B23-4583-9504-1648EB0B394A" );

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: Number of Future Weeks to Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C28368CA-5218-4B59-8BD8-75BD78AA9BE9", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Future Weeks to Show", "FutureWeeksToShow", "Number of Future Weeks to Show", @"How many weeks ahead to show in the drop down.", 4, @"1", "BF904834-0030-4C50-A578-311E9942A596" );

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: Number of Previous Weeks to Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C28368CA-5218-4B59-8BD8-75BD78AA9BE9", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Previous Weeks to Show", "PreviousWeeksToShow", "Number of Previous Weeks to Show", @"How many previous weeks to show in the drop down.", 3, @"6", "448BC527-3AFB-498E-8297-22A10F4CC77D" );

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: Send Day of the Week
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C28368CA-5218-4B59-8BD8-75BD78AA9BE9", "08943FF9-F2A8-4DB4-A72A-31938B200C8C", "Send Day of the Week", "SendDaysOfTheWeek", "Send Day of the Week", @"Used to determine which dates to list in the Message Date drop down. <i><strong>Note:</strong> If no day is selected the Message Date drop down will not be shown and the ‘SendDateTime’ Lava variable will be set to the current day.</i>", 1, @"", "AD747D02-F838-4C8C-8B14-2D4D14E8C1BE" );

            // Attribute for BlockType
            //   BlockType: System Communication Preview
            //   Category: Communication
            //   Attribute: System Communication
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C28368CA-5218-4B59-8BD8-75BD78AA9BE9", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "System Communication", "SystemCommunication", "System Communication", @"The system communication to use when previewing the message. When set as a block setting, it will not allow overriding by the query string.", 0, @"", "85D89D8E-53E0-42E6-AF53-B75DF0914421" );
        }

        // JC: Chop blocks for v1.17.0.32
        private void MigrationRollupsForV17_0_19_ChopBlockTypesv17()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types - 1.17.0.32",
                blockTypeReplacements: new Dictionary<string, string> {
                    // blocks chopped in v1.17.0.32
{ "21FFA70E-18B3-4148-8FC4-F941100B49B8", "68D2ABBC-3C43-4450-973F-071D1715C0C9" }, // Attendance History ( Check-in )
{ "23CA8858-6D02-48A8-92C4-CE415DAB41B6", "ADBF3377-A491-4016-9375-346496A25FB4" }, // Apple TV Page Detail ( TV > TV Apps )
// { "6CB1416A-3B25-41FD-8E60-1B94F4A64AE6", "7EA2E093-2F33-4213-A33E-9E9A7A760181" }, // Check-in Type Detail ( Check-in > Configuration )
{ "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B" }, // Financial Pledge List ( Finance )
{ "813CFCCF-30BF-4A2F-BB55-F240A3B7809F", "653052A0-CA1C-41B8-8340-4B13149C6E66" }, // Person Signal List ( Core )
{ "95366DA1-D878-4A9A-A26F-83160DBE784F", "C28368CA-5218-4B59-8BD8-75BD78AA9BE9" }, // System Communication Preview ( Communication )
{ "9F577C39-19FB-4C33-804B-35023284B856", "CFE6F48B-ED85-4FA8-B068-EFE116B32284" }, // Security Change Audit List ( Security )
{ "A2C41730-BF79-4F8C-8368-2C4D5F76129D", "28A34F1C-80F4-496F-A598-180974ADEE61" }, // Rest Key Detail( Security )
{ "A81AB554-B438-4C7F-9C45-1A9AE2F889C5", "3B8B5AE5-4139-44A6-8EAA-99D48E51134E" }, // Assessment Type Detail ( CRM )
{ "B4D8CBCA-00F6-4D81-B8B6-170373D28128", "C12C615C-384D-478E-892D-0F353E2EF180" }, // Gateway Detail ( Finance )
{ "E2D423B8-10F0-49E2-B2A6-D62892379429", "3855B15B-C903-446A-AE5B-891AB52851CB" }, // System Configuration ( Administration )
                    // blocks chopped in v1.17.0.31
{ "41CD9629-9327-40D4-846A-1BB8135D130C", "dbcfb477-0553-4bae-bac9-2aec38e1da37" }, // Registration Instance - Fee List
{ "0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4", "5ecca4fb-f8fb-49db-96b7-082bb4e4c170" }, // Assessment List
{ "E8718774-3DCA-4AFF-9F4C-FBA50A00BB57", "ed4cd6ae-ed86-4607-a252-f15971e4f2e3" }, // Note Watch List
{ "361F15FC-4C08-4A26-B482-CC260E708F7C", "b1f65833-ceca-4054-bcc3-2de5692741ed" }, // Note Watch Detail
// { "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "f431f950-f007-493e-81c8-16559fe4c0f0" }, // Defined Value List
// { "08C35F15-9AF7-468F-9D50-CDFD3D21220C", "73fd23b4-fa3a-49ea-b271-ffb228c6a49e" }, // Defined Type Detail
{ "7BF616C1-CE1D-4EF0-B56F-B9810B811192", "a6d8bfd9-0c3d-4f1e-ae0d-325a9c70b4c8" }, // REST Controller List
{ "20AD75DD-0DF3-49E9-9DB1-8537C12B1664", "2eafa987-79c6-4477-a181-63392aa24d20" }, // Rest Action List
{ "87A5A2E4-E6BA-4F3C-A6F2-ED046A04062E", "57babd60-2a45-43ac-8ed3-b09af79c54ab" }, // Account List
{ "DCD63280-B661-48AA-8DEB-F5ED63C7AB77", "c0c464c0-2c72-449f-b46f-8e31c1daf29b" }, // Account Detail (Finance)
{ "E30354A1-A1B8-4BE5-ADCE-43EEDDEF6C65", "507F5108-FB55-48F0-A66E-CC3D5185D35D" }, // Campus Detail
{ "B3E4584A-D3C3-4F68-9B7C-D1641B9B08CF", "b150e767-e964-460c-9ed1-b293474c5f5d" }, // Tag Detail
{ "AD5B3A8A-2111-4FC4-A026-51EEB4929CBA", "972ad143-8294-4462-b2a7-1b36ea127374" }, // Group Archived List
{ "C1B934D1-2139-471E-B2B8-B22FF4499B2F", "b6a17e77-e53d-4c96-bcb2-643123b8160c" }, // Schedule List
{ "C679A2C6-8126-4EF5-8C28-269A51EC4407", "5f3151bf-577d-485b-9ee3-90f3f86f5739" }, // Document Type List
{ "85E9AA73-7C96-4731-8DD6-AA604C35E536", "fd3eb724-1afa-4507-8850-c3aee170c83b" }, // Document Type Detail
{ "4280625A-C69A-4B47-A4D3-89B61F43C967", "d9510038-0547-45f3-9eca-c2ca85e64416" }, // Web Farm Settings
{ "B6AD2D98-0DF3-4DFB-AE2B-A8CF6E21E5C0", "011aede7-b036-4f4a-bf3e-4c284dc45de8" }, // Interaction Detail
{ "4AAE3DB5-C9F8-4985-B6DC-9037B2F91100", "054a8469-a838-4708-b18f-9f2819346298" }, // Fundraising Donation List
{ "8CD3C212-B9EE-4258-904C-91BA3570EE11", "e3b5db5c-280f-461c-a6e3-64462c9b329d" }, // Device Detail
{ "678ED4B6-D76F-4D43-B069-659E352C9BD8", "e07607c6-5428-4ccf-a826-060f48cacd32" }, // Attendance List
{ "451E9690-D851-4641-8BA0-317B65819918", "2ad9e6bc-f764-4374-a714-53e365d77a36" }, // Content Channel Type Detail
{ "E664BB02-D501-40B0-AAD6-D8FA0E63438B", "699ed6d1-e23a-4757-a0a2-83c5406b658a" }, // Fundraising List
                    // blocks chopped in v1.17.0.30
{ "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "5AA30F53-1B7D-4CA9-89B6-C10592968870" }, // Prayer Request Entry
{ "74B6C64A-9617-4745-9928-ABAC7948A95D", "C64F92CC-38A6-4562-8EAE-D4F30B4AF017" }, // Mobile Layout Detail
{ "092BFC5F-A291-4472-B737-0C69EA33D08A", "3852E96A-9270-4C0E-A0D0-3CD9601F183E" }, // Lava Shortcode Detail
{ "0C3CEBAD-3CCB-493B-9CBA-9D0D33852050", "04AB8A15-1D0A-4F53-84FE-7B0DE611EB02" }, // Event List
{ "0BFD74A8-1888-4407-9102-D3FCEABF3095", "904DB731-4A40-494C-B52C-95CF0F54C21F" }, // Personal Link Section List
{ "160DABF9-3549-447C-9E76-6CFCCCA481C0", "1228F248-6AA1-4871-AF9E-195CF0FDA724" }, // Verify Photo
{ "CE06640D-C1BA-4ACE-AF03-8D733FD3247C", "DBFA9E41-FA62-4869-8A44-D03B561433B2" }, // User Login List
{ "7764E323-7460-4CB7-8024-056136C99603", "C523CABA-A32C-46A3-A8B4-8F962CDC6A78" }, // Photo Upload
                    // blocks chopped in v1.17.0.29
{ "616D1A98-067D-43B8-B7F5-41FB12FB894E", "53A34D60-31B8-4D22-BC42-E3B669ED152B" }, // Auth Client List
{ "312EAD0E-4068-4211-8410-2EB45B7D8BAB", "8246EF8B-27E9-449E-9CAB-1C267B31DBC2" }, // Auth Client Detail
{ "7EFD5D30-2FF0-4C75-86A2-984A8F45D8A5", "63F5509A-3D71-4F0F-A074-FA5869856038" }, // Consumer List
{ "D4262E61-9CB2-4FF0-A7CA-90BAD1141BF5", "96C5DF9E-6F5C-4E55-92F1-61FE16A18563" }, // Attribute Matrix Template Detail
{ "BEC5B592-9E9E-4C55-BD0D-2B8065A1802E", "23E3CA31-6A1F-43CB-AC06-374BD9CB9FA5" }, // Note Type List
{ "5DA1D088-2142-4645-AF9C-EF52DA5B4EEA", "9E901A5A-82C2-4788-9623-3720FFC4DAEC" }, // Note Type Detail
{ "3C9D442B-D066-43FA-9380-98C60936992E", "662AF7BB-5B61-43C6-BDA6-A6E7AAB8FC00" }, // Media Folder Detail
{ "ACAF8CEB-18CD-4BAE-BF6A-12C08CF6D61F", "6035AC10-07A5-4EDD-A1E9-10862FC41494" }, // Persisted Dataset Detail
{ "E7546752-C3DC-4B96-88D9-A431F2D1C989", "6C9E7EBF-8F27-48EF-94C4-900AC3A2C167" }, // Personal Link List
{ "78DB9184-97CF-4FC4-BD71-8F3ABE4100BA", "3D13455F-7E5C-46F7-975A-4A5CE12BD330" }, // Financial Statement Template Detail
{ "49F3D87E-BD8D-43D4-8217-340F3DFF4562", "CDAB601D-1369-44CB-A146-4E80C7D66BCD" }, // Apple TV App Detail
{ "5D58BF6A-3914-420C-9013-53CE8A15E390", "A8062FE5-5BCD-48AC-8C37-2124462656A7" }, // Workflow Trigger Detail
{ "005E5980-E2D2-4958-ACB6-BECBC6D1F5C4", "F140B415-9BB3-4492-844E-5A529517A484" }, // Tag Report
                    // blocks chopped in v1.17.0.28
{ "08189564-1245-48F8-86CC-560F4DD48733", "D0203B97-5856-437E-8700-8846309F8EED" }, // Location Detail
{ "5144ED5B-89A9-4D77-B0E5-695070BE0C8E", "8A5AF4F4-32A2-426F-8363-57AC4F02A6F6" }, // Location List
{ "92E4BFE8-DF80-49D7-819D-417E579E282D", "C0CFDAB7-BB29-499E-BD0A-468B0856C037" }, // Registration List Lava
{ "1270E3F7-5ACB-4044-94CD-E2B4368FF391", "DA7834C6-C5C6-470B-B1C8-9AFA492151F8" }, // Group Requirement Type List
{ "68FC983E-05F0-4067-83AC-97DD226F5071", "C17B6D03-FDF3-4DD7-B9A9-3D6159A838F5" }, // Group Requirement Type Detail
{ "E9AB79D9-429F-410D-B4A8-327829FC7C63", "8B65EE51-4075-4FC0-B1A9-F56C7153AA77" }, // Person Signal Type Detail
{ "23818F47-D81E-4B6E-B89B-045B1FAD4C2B", "0C01496C-B4FD-4335-A11F-9B3D14D3C0E1" }, // Suggestion List
                    // blocks chopped in v1.17.0.27
{ "052B84EA-0C34-4A07-AC4C-1FBCEC87C223", "E18AB976-6665-48A5-B418-8FAC8F374135" }, // Suggestion Detail
{ "14293AEB-B0F5-434B-844A-66592AE3A416", "7E2DFB55-F1AB-4452-A5DF-6CE65FBFDDAD" }, // Photo Opt-Out
{ "0DE16268-BD5B-4CFC-A7C6-F1E07F47527A", "F61C0FDF-E8A0-457A-B8AF-42CAC8A18718" }, // Benevolence Type List
{ "C96479B6-E309-4B1A-B024-1F1276122A13", "03397615-EF2B-4D33-BD62-A79186F56ACE" }, // Benevolence Type Detail
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_CHOP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: new Dictionary<string, string> {
                { "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "ShowAccountFilter,ShowDateRangeFilter,ShowLastModifiedFilter,ShowPersonFilter" }, // Pledge List ( Finance )
                { "92E4BFE8-DF80-49D7-819D-417E579E282D", "EnableDebug,LimitToOwed,MaxResults" }, // Registration List Lava
                { "BEC5B592-9E9E-4C55-BD0D-2B8065A1802E", "EntityType" }, // Note Type List
                { "361F15FC-4C08-4A26-B482-CC260E708F7C", "NoteType,EntityType" }, // Note Watch Detail
                { "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "EnableDebug" }, // Prayer Request Entry
                { "C96479B6-E309-4B1A-B024-1F1276122A13", "MaximumNumberOfDocuments" } // Benevolence Type Detail
            } );
        }

        #endregion

        #endregion

        #region UndoObsidianChop

        private void UndoObsidianChop_SwapBlockUp()
        {
            UndoObsidianChop_RegisterBlockAttributesForSwap();
            UndoObsidianChop_SwapObsidianBlocks();
        }

        private void UndoObsidianChop_RegisterBlockAttributesForSwap()
        {
            RockMigrationHelper.UpdateBlockType( "Defined Type Detail", "Displays the details of the given defined type.", "~/Blocks/Core/DefinedTypeDetail.ascx", "Core", "08C35F15-9AF7-468F-9D50-CDFD3D21220C" );

            RockMigrationHelper.UpdateBlockType( "Defined Value List", "Block for viewing values for a defined type.", "~/Blocks/Core/DefinedValueList.ascx", "Core", "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE" );

            /*
	            11/15/2024 - JC & KH
	            Update the qualifier value to match the new webforms BlockTypeId.
	            This could happen if the church updates regularly and the webforms 
	            block was chopped and then re-added as part of this migration.
	            RockMigrationHelper.AddOrUpdateBlockTypeAttribute method expects
	            the EntityTypeQualifierValue to match the current BlockTypeId.
	            We can't just delete the attribute though because upgrading 
	            from an older (< 16.0) could cause data loss.
 
	            Reason: Premature chop of some blocks prevents use of non-obsidian FieldTypes.
            */
            Sql( @"UPDATE a SET
	EntityTypeQualifierValue = CONVERT(NVARCHAR(400), bt.Id)
FROM (
	SELECT '0305EF98-C791-4626-9996-F189B9BB674C' WebformsAttributeGuid, '08C35F15-9AF7-468F-9D50-CDFD3D21220C' WebformsBlockTypeGuid
	UNION SELECT '9280D61F-C4F3-4A3E-A9BB-BCD67FF78637', '0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE' -- Does this need names (i.e WebformsAttributeGuid, WebformsBlockTypeGuid)
	UNION SELECT '87DAF7ED-AAF5-4D5C-8339-CB30B16CC9FF', '0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE'
	UNION SELECT '0A3F078E-8A2A-4E9D-9763-3758E123E042', '0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE'
	UNION SELECT '80765648-83B0-4B75-A296-851384C41CAB', '0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE'
	UNION SELECT 'DF5BE156-A4B8-4FA5-A730-0579733F42F5', '0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE'
) attributes
JOIN [dbo].[Attribute] a ON a.[Guid] = WebformsAttributeGuid
LEFT JOIN [dbo].[BlockType] bt ON bt.[Guid] = WebformsBlockTypeGuid
WHERE a.EntityTypeQualifierValue <> CONVERT(NVARCHAR(400), bt.Id)" );

            // Attribute for BlockType
            //   BlockType: Defined Type Detail
            //   Category: Core
            //   Attribute: DefinedType
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "08C35F15-9AF7-468F-9D50-CDFD3D21220C", "BC48720C-3610-4BCF-AE66-D255A17F1CDF", "Defined Type", "DefinedType", "Defined Type", @"If a Defined Type is set, only details for it will be displayed (regardless of the querystring parameters).", 0, @"", "0305EF98-C791-4626-9996-F189B9BB674C" );

            // Attribute for BlockType
            //   BlockType: Defined Value List
            //   Category: Core
            //   Attribute: Defined Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "BC48720C-3610-4BCF-AE66-D255A17F1CDF", "Defined Type", "DefinedType", "", @"If a Defined Type is set, only its Defined Values will be displayed (regardless of the querystring parameters).", 0, @"", "9280D61F-C4F3-4A3E-A9BB-BCD67FF78637" );

            // Attribute for BlockType
            //   BlockType: Defined Value List
            //   Category: Core
            //   Attribute: core.CustomGridColumnsConfig
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "87DAF7ED-AAF5-4D5C-8339-CB30B16CC9FF" );

            // Attribute for BlockType
            //   BlockType: Defined Value List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "0A3F078E-8A2A-4E9D-9763-3758E123E042" );

            // Attribute for BlockType
            //   BlockType: Defined Value List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "80765648-83B0-4B75-A296-851384C41CAB" );

            // Attribute for BlockType
            //   BlockType: Defined Value List
            //   Category: Core
            //   Attribute: core.CustomGridEnableStickyHeaders
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", @"", 0, @"False", "DF5BE156-A4B8-4FA5-A730-0579733F42F5" );
        }

        private void UndoObsidianChop_SwapObsidianBlocks()
        {
            // Custom swap to replace Obsidian DefinedTypeDetail with Webforms DefinedTypeDetail.
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Swap Webforms Blocks",
                blockTypeReplacements: new Dictionary<string, string> {
                { "f431f950-f007-493e-81c8-16559fe4c0f0", "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE" }, // DefinedValueList -> DefinedValueList.ascx
                { "73fd23b4-fa3a-49ea-b271-ffb228c6a49e", "08C35F15-9AF7-468F-9D50-CDFD3D21220C" }, // DefinedTypeDetail -> DefinedTypeDetail.ascx
                },
                migrationStrategy: "Swap",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_SWAP_WEBFORMS_BLOCKS,
                blockAttributeKeysToIgnore: null );
        }


        #endregion

        #region PeerNetworkPageAndBlocks

        /// <summary>
        /// JPH: Add Peer Network page and blocks - up.
        /// </summary>
        private void AddPeerNetworkPageAndBlocksUp()
        {
            #region Cleanup Spark Site Feature-Development Blocks And Page

            // Delete the following blocks and page that were added to the Spark site for the development of this
            // feature. This way, we'll ensure the final blocks and page are seeded the same across all Rock instances.

            // Person Profile > SectionA2 > Peer Network block.
            RockMigrationHelper.DeleteBlock( "1CD1D0BF-7377-4244-896F-56AA80A2CAEB" );

            // Peer Network > SectionC1 > Peer List block.
            RockMigrationHelper.DeleteBlock( "64961379-E7EC-456E-9A44-493C5F2DB7D9" );

            // Peer Network > SectionC1 > Peer Map block.
            RockMigrationHelper.DeleteBlock( "9552A5AD-8B5A-470C-9136-58FE49F394CC" );

            // The [HtmlContent] records will cascade-delete.
            // The blocks' orphaned [AttributeValue] records will be deleted by the RockCleanup job.

            // Person Pages > Peer Network page.
            RockMigrationHelper.DeletePage( "0A93F551-1226-473E-A8EF-F1DC6680FA39" );

            // The [PageRoute] record will cascade-delete.

            #endregion Cleanup Spark Site Feature-Development Blocks And Page

            #region Add Person Profile > Peer Network Page

            // Add Page 
            //  Internal Name: Peer Network
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "BF04BB7E-BE3A-4A38-A37C-386B55496303", "6AD84AFC-B3A1-4E30-B53B-C6E57B513839", "Peer Network", "", "AC27E363-108F-4876-8177-2DC9A65815B7", "" );

            // Add Page Route
            //   Page:Peer Network
            //   Route:person/{PersonId}/peer-graph
            RockMigrationHelper.AddOrUpdatePageRoute( "AC27E363-108F-4876-8177-2DC9A65815B7", "person/{PersonId}/peer-graph", "D4A5A34D-4968-4F6A-B516-FC301842B809" );

            // Hide the page from navigation.
            Sql( @"
UPDATE [Page]
SET [DisplayInNavWhen] = 2
WHERE [Guid] = 'AC27E363-108F-4876-8177-2DC9A65815B7';" );

            #endregion Add Person Profile > Peer Network Page

            #region Add Peer Network > Peer Map Block

            // Add Block 
            //  Block Name: Peer Map
            //  Page Name: Peer Network
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "AC27E363-108F-4876-8177-2DC9A65815B7".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "19B61D65-37E3-459F-A44F-DEF0089118A3".AsGuid(), "Peer Map", "SectionC1", @"", @"", 0, "D2D0FF94-1816-4B43-A49D-104CC42A5DC3" );

            // Add/Update HtmlContent for Block: Peer Map
            RockMigrationHelper.UpdateHtmlContentBlock( "D2D0FF94-1816-4B43-A49D-104CC42A5DC3", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' %}

{% sql %}

    SELECT 
        tp.[NickName] + ' ' + tp.[LastName] AS [TargetName]
        , tp.[Id] AS [TargetPersonId]
        , pn.[RelationshipScore]
        , pn.[RelationshipTrend]
        , pn.[RelationshipTypeValueId]
        , pn.[RelatedEntityId]
        , pn.[Caption]
    FROM [PeerNetwork] pn
        INNER JOIN [Person] tp ON tp.[Id] = pn.[TargetPersonId]
    WHERE [SourcePersonId] = {{ personId }};

{% endsql %}

<div class=""card card-profile card-peer-map panel-widget"">

    <div class=""card-header"">
        <span class=""card-title"">Peer Map</span>
    </div>

    <div class=""card-section p-0"">
        <div style=""height: 800px"">
            {[ networkgraph height:'100%' minimumnodesize:'10' highlightcolor:'#bababa' ]}

                {% assign followingConnectionsValueId = '84E0360E-0828-E5A5-4BCC-F3113BE338A1' | GuidToId:'DefinedValue' %}
                {% assign following = results | Where:'RelationshipTypeValueId', followingConnectionsValueId %}

                [[ node id:'F-MASTER' label:'Following' color:'#36cf8c' ]][[ endnode ]]

                {% for followed in following %}
                    [[ node id:'F-{{ followed.TargetPersonId }}' label:'{{ followed.TargetName }}' color:'#88ebc0' size:'10' ]][[ endnode ]]
                    [[ edge source:'F-MASTER' target:'F-{{ followed.TargetPersonId }}' color:'#c4c4c4' ]][[ endedge ]]
                {% endfor %}

                {% assign groupConnectionsValueId = 'CB51DC46-FBDB-43DA-B7F3-60E7C6E70F40' | GuidToId:'DefinedValue' %}
                {% assign groups = results | Where:'RelationshipTypeValueId', groupConnectionsValueId | GroupBy:'RelatedEntityId' %}

                {% for group in groups %}
                    {% assign parts = group | PropertyToKeyValue %}

                    {% assign groupName = parts.Value | First | Property:'Caption' %}
                    [[ node id:'G-{{ parts.Key }}' label:""{{ groupName }}"" color:'#4e9fd9' ]][[ endnode ]]

                    {% for member in parts.Value %}
                        [[ node id:'GM-{{ parts.Key }}-{{ member.TargetPersonId }}' label:'{{ member.TargetName }}' color:'#a6d5f7' ]][[ endnode ]]

                        [[ edge source:'GM-{{ parts.Key }}-{{ member.TargetPersonId }}' target:'G-{{ parts.Key }}' color:'#c4c4c4' ]][[ endedge ]]
                    {% endfor %}

                {% endfor %}

            {[ endnetworkgraph ]}
        </div>
    </div>
</div>", "A311EB92-5BB5-407D-AF6C-74BC9FB9FA64" );

            // Add Block Attribute Value
            //   Block: Peer Map
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Peer Network, Site=Rock RMS
            //   Attribute: Start in Code Editor mode
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "D2D0FF94-1816-4B43-A49D-104CC42A5DC3", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );

            // Add Block Attribute Value
            //   Block: Peer Map
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Peer Network, Site=Rock RMS
            //   Attribute: Image Root Folder
            /*   Attribute Value: ~/Content */
            RockMigrationHelper.AddBlockAttributeValue( "D2D0FF94-1816-4B43-A49D-104CC42A5DC3", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );

            // Add Block Attribute Value
            //   Block: Peer Map
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Peer Network, Site=Rock RMS
            //   Attribute: Is Secondary Block
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "D2D0FF94-1816-4B43-A49D-104CC42A5DC3", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"False" );

            // Add Block Attribute Value
            //   Block: Peer Map
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Peer Network, Site=Rock RMS
            //   Attribute: User Specific Folders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "D2D0FF94-1816-4B43-A49D-104CC42A5DC3", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );

            // Add Block Attribute Value
            //   Block: Peer Map
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Peer Network, Site=Rock RMS
            //   Attribute: Validate Markup
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "D2D0FF94-1816-4B43-A49D-104CC42A5DC3", "6E71FE26-5628-4DDA-BDBC-8E4D47BE72CD", @"True" );

            // Add Block Attribute Value
            //   Block: Peer Map
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Peer Network, Site=Rock RMS
            //   Attribute: Enabled Lava Commands
            /*   Attribute Value: Sql */
            RockMigrationHelper.AddBlockAttributeValue( "D2D0FF94-1816-4B43-A49D-104CC42A5DC3", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"Sql" );

            // Add Block Attribute Value
            //   Block: Peer Map
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Peer Network, Site=Rock RMS
            //   Attribute: Document Root Folder
            /*   Attribute Value: ~/Content */
            RockMigrationHelper.AddBlockAttributeValue( "D2D0FF94-1816-4B43-A49D-104CC42A5DC3", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );

            // Add Block Attribute Value
            //   Block: Peer Map
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Peer Network, Site=Rock RMS
            //   Attribute: Require Approval
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "D2D0FF94-1816-4B43-A49D-104CC42A5DC3", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );

            // Add Block Attribute Value
            //   Block: Peer Map
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Peer Network, Site=Rock RMS
            //   Attribute: Cache Duration
            /*   Attribute Value: 0 */
            RockMigrationHelper.AddBlockAttributeValue( "D2D0FF94-1816-4B43-A49D-104CC42A5DC3", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );

            // Add Block Attribute Value
            //   Block: Peer Map
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Peer Network, Site=Rock RMS
            //   Attribute: Enable Versioning
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "D2D0FF94-1816-4B43-A49D-104CC42A5DC3", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );

            #endregion Add Peer Network > Peer Map Block

            #region Add Peer Network > Peer List Block

            // Add Block 
            //  Block Name: Peer List
            //  Page Name: Peer Network
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "AC27E363-108F-4876-8177-2DC9A65815B7".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "19B61D65-37E3-459F-A44F-DEF0089118A3".AsGuid(), "Peer List", "SectionC1", @"", @"", 1, "46775056-3ADF-43CD-809A-88EE3378C039" );

            // Add/Update HtmlContent for Block: Peer List
            RockMigrationHelper.UpdateHtmlContentBlock( "46775056-3ADF-43CD-809A-88EE3378C039", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' %}

{% sql %}

    SELECT
        tp.[NickName] + ' ' + tp.[LastName] AS [TargetName]
        , tp.[Id] AS [TargetPersonId]
        , CAST( ROUND( SUM(pn.[RelationshipScore]), 0 ) AS INT ) AS [RelationshipScore]
        , MAX(pn.[RelationshipTrend]) AS [RelationshipTrend]
        , SUM( pn.[RelationshipScore] ) - SUM( pn.[RelationshipScoreLastUpdateValue] ) AS [PointDifference]
    FROM [PeerNetwork] pn
        INNER JOIN [Person] tp ON tp.[Id] = pn.[TargetPersonId]
    WHERE [SourcePersonId] = {{ personId }}
    GROUP BY tp.[NickName], tp.[LastName], tp.[Id]
    ORDER BY [RelationshipScore] DESC;

{% endsql %}

<div class=""card card-profile card-peer-network panel-widget"">

    <div class=""card-header"">
        <span class=""card-title"">Full Peer Network</span>
    </div>

    <div class=""card-section"">
        <div class=""row"">
            {% for peer in results %}
                <div class=""col-xs-8"">
                    <a href=""/person/{{ peer.TargetPersonId }}"">
                        {{ peer.TargetName }}
                    </a>
                </div>
                <div class=""col-xs-2"">{{ peer.RelationshipScore }}</div>
                <div class=""col-xs-2"">
                    {% if peer.PointDifference > 0 %}
                        <i class=""fa fa-arrow-up text-success""></i>
                    {% elseif peer.PointDifference < 0 %}
                        <i class=""fa fa-arrow-down text-danger""></i>
                    {% else %}
                        <i class=""fa fa-minus text-muted""></i>
                    {% endif %}
                </div>
            {% endfor %}
        </div>
    </div>
</div>", "0A35B353-E14E-4B9C-8E0C-7E7D0863A67B" );

            // Add Block Attribute Value
            //   Block: Peer List
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Peer Network, Site=Rock RMS
            //   Attribute: Enable Versioning
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "46775056-3ADF-43CD-809A-88EE3378C039", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );

            // Add Block Attribute Value
            //   Block: Peer List
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Peer Network, Site=Rock RMS
            //   Attribute: Cache Duration
            /*   Attribute Value: 0 */
            RockMigrationHelper.AddBlockAttributeValue( "46775056-3ADF-43CD-809A-88EE3378C039", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );

            // Add Block Attribute Value
            //   Block: Peer List
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Peer Network, Site=Rock RMS
            //   Attribute: Require Approval
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "46775056-3ADF-43CD-809A-88EE3378C039", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );

            // Add Block Attribute Value
            //   Block: Peer List
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Peer Network, Site=Rock RMS
            //   Attribute: Start in Code Editor mode
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "46775056-3ADF-43CD-809A-88EE3378C039", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );

            // Add Block Attribute Value
            //   Block: Peer List
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Peer Network, Site=Rock RMS
            //   Attribute: Document Root Folder
            /*   Attribute Value: ~/Content */
            RockMigrationHelper.AddBlockAttributeValue( "46775056-3ADF-43CD-809A-88EE3378C039", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );

            // Add Block Attribute Value
            //   Block: Peer List
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Peer Network, Site=Rock RMS
            //   Attribute: Enabled Lava Commands
            /*   Attribute Value: Sql */
            RockMigrationHelper.AddBlockAttributeValue( "46775056-3ADF-43CD-809A-88EE3378C039", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"Sql" );

            // Add Block Attribute Value
            //   Block: Peer List
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Peer Network, Site=Rock RMS
            //   Attribute: Is Secondary Block
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "46775056-3ADF-43CD-809A-88EE3378C039", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"False" );

            // Add Block Attribute Value
            //   Block: Peer List
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Peer Network, Site=Rock RMS
            //   Attribute: Image Root Folder
            /*   Attribute Value: ~/Content */
            RockMigrationHelper.AddBlockAttributeValue( "46775056-3ADF-43CD-809A-88EE3378C039", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );

            // Add Block Attribute Value
            //   Block: Peer List
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Peer Network, Site=Rock RMS
            //   Attribute: User Specific Folders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "46775056-3ADF-43CD-809A-88EE3378C039", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );

            // Add Block Attribute Value
            //   Block: Peer List
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Peer Network, Site=Rock RMS
            //   Attribute: Validate Markup
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "46775056-3ADF-43CD-809A-88EE3378C039", "6E71FE26-5628-4DDA-BDBC-8E4D47BE72CD", @"True" );

            #endregion Add Peer Network > Peer List Block

            #region Add Person Profile > Peer Network Block

            // Get the order of the existing "Implied Relationship" block we're about to delete (so we can add a new
            // block in its place).
            var blockOrder = SqlScalar( "SELECT [Order] FROM [Block] WHERE [Guid] = '32847AAF-15F5-4F8B-9F84-92D6AE827857';" ) as int?;
            if ( !blockOrder.HasValue )
            {
                // The block didn't exist. Get the max order of blocks in the target page/zone.
                blockOrder = SqlScalar( $@"
DECLARE @PageId INT = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '08DBD8A5-2C35-4146-B4A8-0F7652348B25');
SELECT MAX([Order]) FROM [Block] WHERE [PageId] = @PageId AND [Zone] = 'SectionA2';" ) as int?;

                if ( blockOrder.HasValue )
                {
                    // Increment by one to add the new "Peer Network" block after all existing blocks.
                    blockOrder++;
                }
                else
                {
                    // There are no blocks in this zone; add the "Peer Network" block as the first and only block.
                    blockOrder = 0;
                }
            }

            // Delete the Person Profile > SectionA2 > Implied Relationship block.
            // The block's orphaned [AttributeValue] records will be deleted by the RockCleanup job.
            RockMigrationHelper.DeleteBlock( "32847AAF-15F5-4F8B-9F84-92D6AE827857" );

            // Add Block 
            //  Block Name: Peer Network
            //  Page Name: Person Profile
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "08DBD8A5-2C35-4146-B4A8-0F7652348B25".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "19B61D65-37E3-459F-A44F-DEF0089118A3".AsGuid(), "Peer Network", "SectionA2", @"", @"", blockOrder.Value, "6094C135-10E2-4AF4-A46B-1FC6D073A854" );

            // Add/Update HtmlContent for Block: Peer Network
            RockMigrationHelper.UpdateHtmlContentBlock( "6094C135-10E2-4AF4-A46B-1FC6D073A854", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' %}

{% sql %}

    SELECT
        TOP 20
        tp.[NickName] + ' ' + tp.[LastName] AS [TargetName]
        , tp.[Id] AS [TargetPersonId]
        , CAST( ROUND( SUM(pn.[RelationshipScore]), 0 ) AS INT ) AS [RelationshipScore]
        , MAX(pn.[RelationshipTrend]) AS [RelationshipTrend]
        , SUM( pn.[RelationshipScore] ) - SUM( pn.[RelationshipScoreLastUpdateValue] ) AS [PointDifference]
    FROM [PeerNetwork] pn
        INNER JOIN [Person] tp ON tp.[Id] = pn.[TargetPersonId]
    WHERE [SourcePersonId] = {{ personId }}
    GROUP BY tp.[NickName], tp.[LastName], tp.[Id]
    ORDER BY [RelationshipScore] DESC;

{% endsql %}

<div class=""card card-profile card-peer-network panel-widget"">

    <div class=""card-header"">
        <span class=""card-title"">Peer Network</span>

        <div class=""panel-labels"">
            <a href=""/person/{{ personId }}/peer-graph""><span class=""label label-default"">Peer Graph</span></a>
        </div>
    </div>

    <div class=""card-section"">
        <div class=""row"">
            {% for peer in results %}
                <div class=""col-xs-8"">
                    <a href=""/person/{{ peer.TargetPersonId }}"">
                        {{ peer.TargetName }}
                    </a>
                </div>
                <div class=""col-xs-2"">{{ peer.RelationshipScore }}</div>
                <div class=""col-xs-2"">
                    {% if peer.PointDifference > 0 %}
                        <i class=""fa fa-arrow-up text-success""></i>
                    {% elseif peer.PointDifference < 0 %}
                        <i class=""fa fa-arrow-down text-danger""></i>
                    {% else %}
                        <i class=""fa fa-minus text-muted""></i>
                    {% endif %}
                </div>
            {% endfor %}
        </div>
    </div>
</div>", "879C5623-3A45-4D6F-9759-F9A294D7425B" );

            // Add Block Attribute Value
            //   Block: Peer Network
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Person Profile, Site=Rock RMS
            //   Attribute: Validate Markup
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "6094C135-10E2-4AF4-A46B-1FC6D073A854", "6E71FE26-5628-4DDA-BDBC-8E4D47BE72CD", @"True" );

            // Add Block Attribute Value
            //   Block: Peer Network
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Person Profile, Site=Rock RMS
            //   Attribute: Cache Duration
            /*   Attribute Value: 0 */
            RockMigrationHelper.AddBlockAttributeValue( "6094C135-10E2-4AF4-A46B-1FC6D073A854", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );

            // Add Block Attribute Value
            //   Block: Peer Network
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Person Profile, Site=Rock RMS
            //   Attribute: Enable Versioning
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "6094C135-10E2-4AF4-A46B-1FC6D073A854", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );

            // Add Block Attribute Value
            //   Block: Peer Network
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Person Profile, Site=Rock RMS
            //   Attribute: Require Approval
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "6094C135-10E2-4AF4-A46B-1FC6D073A854", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );

            // Add Block Attribute Value
            //   Block: Peer Network
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Person Profile, Site=Rock RMS
            //   Attribute: Start in Code Editor mode
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "6094C135-10E2-4AF4-A46B-1FC6D073A854", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );

            // Add Block Attribute Value
            //   Block: Peer Network
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Person Profile, Site=Rock RMS
            //   Attribute: Enabled Lava Commands
            /*   Attribute Value: Sql */
            RockMigrationHelper.AddBlockAttributeValue( "6094C135-10E2-4AF4-A46B-1FC6D073A854", "7146AC24-9250-4FC4-9DF2-9803B9A84299", @"Sql" );

            // Add Block Attribute Value
            //   Block: Peer Network
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Person Profile, Site=Rock RMS
            //   Attribute: Document Root Folder
            /*   Attribute Value: ~/Content */
            RockMigrationHelper.AddBlockAttributeValue( "6094C135-10E2-4AF4-A46B-1FC6D073A854", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );

            // Add Block Attribute Value
            //   Block: Peer Network
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Person Profile, Site=Rock RMS
            //   Attribute: Is Secondary Block
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "6094C135-10E2-4AF4-A46B-1FC6D073A854", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"False" );

            // Add Block Attribute Value
            //   Block: Peer Network
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Person Profile, Site=Rock RMS
            //   Attribute: Image Root Folder
            /*   Attribute Value: ~/Content */
            RockMigrationHelper.AddBlockAttributeValue( "6094C135-10E2-4AF4-A46B-1FC6D073A854", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );

            // Add Block Attribute Value
            //   Block: Peer Network
            //   BlockType: HTML Content
            //   Category: CMS
            //   Block Location: Page=Person Profile, Site=Rock RMS
            //   Attribute: User Specific Folders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "6094C135-10E2-4AF4-A46B-1FC6D073A854", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );

            #endregion Add Person Profile > Peer Network Block

            #region Add Peer Network Stored Procedures

            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spPeerNetwork_UpdateFollowing] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spPeerNetwork_UpdateFollowing]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spPeerNetwork_UpdateFollowing];" );

            Sql( HotFixMigrationResource._221_PeerNetworkPageAndBlocks_spPeerNetwork_UpdateFollowing );

            // Add [spPeerNetwork_UpdateGroupConnections] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spPeerNetwork_UpdateGroupConnections]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spPeerNetwork_UpdateGroupConnections];" );

            Sql( HotFixMigrationResource._221_PeerNetworkPageAndBlocks_spPeerNetwork_UpdateGroupConnections );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );

            #endregion Add Peer Network Stored Procedures

            #region Add Peer Network Job

            Sql( $@"
DECLARE @Now DATETIME = (SELECT GETDATE());

IF EXISTS
(
    SELECT [Id]
    FROM [ServiceJob]
    WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.CALCULATE_PEER_NETWORK}'
)
BEGIN
    UPDATE [ServiceJob]
    SET [Name] = 'Calculate Peer Network'
        , [Description] = 'Job that calculates Rock''s peer networks for individuals.'
        , [Class] = 'Rock.Jobs.CalculatePeerNetwork'
        , [CronExpression] = '0 5 2 1 1/1 ? *'
        , [ModifiedDateTime] = @Now
    WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.CALCULATE_PEER_NETWORK}';
END
ELSE
BEGIN
    INSERT INTO [ServiceJob]
    (
        [IsSystem]
        , [IsActive]
        , [Name]
        , [Description]
        , [Class]
        , [CronExpression]
        , [NotificationStatus]
        , [Guid]
        , [CreatedDateTime]
        , [ModifiedDateTime]
        , [HistoryCount]
    )
    VALUES
    (
        0
        , 1
        , 'Calculate Peer Network'
        , 'Job that calculates Rock''s peer networks for individuals.'
        , 'Rock.Jobs.CalculatePeerNetwork'
        , '0 5 2 1 1/1 ? *'
        , 1
        , '{Rock.SystemGuid.ServiceJob.CALCULATE_PEER_NETWORK}'
        , @Now
        , @Now
        , 500
    );
END" );

            #endregion Add Peer Network Job

            #region Add and Update Peer Network Indexes

            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v17.0 - Add and Update Peer Network Indexes",
                description: "This job will add new and update existing indexes to support the Peer Network feature.",
                jobType: "Rock.Jobs.PostV17AddAndUpdatePeerNetworkIndexes",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_170_ADD_AND_UPDATE_PEER_NETWORK_INDEXES );

            #endregion Add and Update Peer Network Indexes

            #region Delete Legacy Peer Network Groups and Group Type

            try
            {
                Sql( @"
DECLARE @GroupTypeId INT = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '8C0E5852-F08F-4327-9AA5-87800A6AB53E');

-- [GroupMember] records tied to these groups will cascade-delete.
DELETE [Group] WHERE [GroupTypeId] = @GroupTypeId;" );

                RockMigrationHelper.DeleteGroupType( "8C0E5852-F08F-4327-9AA5-87800A6AB53E" );
            }
            catch ( Exception ex )
            {
                // This could be risky, as there might be unforeseen foreign key relationships to the records we're
                // trying to delete here. If there's an exception, log it and move on. It's not detrimental for these
                // records to remain in the database. But let's at least try to hide this group type from display in
                // lists and navigation, and ensure it doesn't take part in the new Peer Network functionality.
                Sql( @"
DECLARE @GroupTypeId INT = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '8C0E5852-F08F-4327-9AA5-87800A6AB53E');

UPDATE [GroupType]
SET [ShowInGroupList] = 0
    , [ShowInNavigation] = 0
    , [ModifiedDateTime] = GETDATE()
    , [IsPeerNetworkEnabled] = 0
    , [RelationshipStrength] = 0
    , [RelationshipGrowthEnabled] = 0
    , [LeaderToLeaderRelationshipMultiplier] = 0
    , [LeaderToNonLeaderRelationshipMultiplier] = 0
    , [NonLeaderToLeaderRelationshipMultiplier] = 0
    , [NonLeaderToNonLeaderRelationshipMultiplier] = 0
WHERE [Id] = @GroupTypeId;

UPDATE [Group]
SET [IsActive] = 0
    , [ModifiedDateTime] = GETDATE()
    , [RelationshipStrengthOverride] = NULL
    , [RelationshipGrowthEnabledOverride] = NULL
    , [LeaderToLeaderRelationshipMultiplierOverride] = NULL
    , [LeaderToNonLeaderRelationshipMultiplierOverride] = NULL
    , [NonLeaderToLeaderRelationshipMultiplierOverride] = NULL
    , [NonLeaderToNonLeaderRelationshipMultiplierOverride] = NULL
WHERE [GroupTypeId] = @GroupTypeId;" );

                var exception = new Exception( "Unable to delete legacy Peer Network Groups and Group Type.", ex );
                ExceptionLogService.LogException( exception );
            }

            #endregion Delete Legacy Peer Network Groups and Group Type
        }

        /// <summary>
        /// JPH: Add Peer Network page and blocks - down.
        /// </summary>
        private void AddPeerNetworkPageAndBlocksDown()
        {
            #region Delete Peer Network Job

            Sql( $@"
DELETE FROM [ServiceJob]
WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.CALCULATE_PEER_NETWORK}';" );

            #endregion Delete Peer Network Job

            #region Delete Peer Network Stored Procedures

            // Delete [spPeerNetwork_UpdateGroupConnections].
            Sql( "DROP PROCEDURE [dbo].[spPeerNetwork_UpdateGroupConnections];" );

            // Delete [spPeerNetwork_UpdateFollowing].
            Sql( "DROP PROCEDURE [dbo].[spPeerNetwork_UpdateFollowing];" );

            #endregion Delete Peer Network Stored Procedures

            #region Delete Person Profile > Peer Network Block

            // Remove Block
            //  Name: Peer Network, from Page: Person Profile, Site: Rock RMS
            //  from Page: Person Profile, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "6094C135-10E2-4AF4-A46B-1FC6D073A854" );

            // The [HtmlContent] record will cascade-delete.
            // The block's orphaned [AttributeValue] records will be deleted by the RockCleanup job.

            // Don't re-add the Person Profile > SectionA2 > Implied Relationship block, as we're not sure they would
            // have still had it + we don't know which settings they might have modified.

            #endregion Delete Person Profile > Peer Network Block

            #region Delete Peer Network > Peer List Block

            // Remove Block
            //  Name: Peer List, from Page: Peer Network, Site: Rock RMS
            //  from Page: Peer Network, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "46775056-3ADF-43CD-809A-88EE3378C039" );

            // The [HtmlContent] record will cascade-delete.
            // The block's orphaned [AttributeValue] records will be deleted by the RockCleanup job.

            #endregion Delete Peer Network > Peer List Block

            #region Delete Peer Network > Peer Map Block

            // Remove Block
            //  Name: Peer Map, from Page: Peer Network, Site: Rock RMS
            //  from Page: Peer Network, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "D2D0FF94-1816-4B43-A49D-104CC42A5DC3" );

            // The [HtmlContent] record will cascade-delete.
            // The block's orphaned [AttributeValue] records will be deleted by the RockCleanup job.

            #endregion Delete Peer Network > Peer Map Block

            #region Delete Person Profile > Peer Network Page

            // Delete Page 
            //  Internal Name: Peer Network
            //  Site: Rock RMS
            //  Layout: Person Profile Detail
            RockMigrationHelper.DeletePage( "AC27E363-108F-4876-8177-2DC9A65815B7" );

            // The [PageRoute] record will cascade-delete.

            #endregion Delete Person Profile > Peer Network Page
        }

        #endregion

        #region LMSSecurity

        private const string RSR_STAFF_GUID = "2C112948-FF4C-46E7-981A-0257681EADF4";
        private const string RSR_STAFF_LIKE_GUID = "300BA2C8-49A3-44BA-A82A-82E3FD8C3745";
        private const string RSR_LMS_ADMINISTRATION_GUID = "5E0E02A9-F16B-437E-A9D9-C3D9D6AFABB0";
        private const string RSR_LMS_WORKER_GUID = "B5481A0E-52E3-4D7B-93B5-A8F5C908DC67";

        private void AddLMSRoleAccess()
        {
            // Page Guids
            var programListPageGuid = "84DBEC51-EE0B-41C2-94B3-F361C4B98879";
            var programClassesPageGuid = "56459D93-32DF-4151-8F6D-003B9AFF0F94";
            var programCompletionsPageGuid = "395BE5DD-E524-4B75-A4CA-5A0548645647";
            var programCompletionDetailPageGuid = "DF896952-E38A-490D-BF85-3601249C3630";
            var classWorkspacePageGuid = "61BE63C7-6611-4235-A6F2-B22456620F35";

            // Route Guids
            var programSummaryClassesRouteGuid = "56A1387A-DDDE-46D9-A23D-B19D6A3BFC50";
            var programCompletionDetailRouteGuid = "1394A3BA-6CB0-4301-9C2E-3F58BA3A8AEB";

            // Block Guids
            var programDetailForCoursesBlockGuid = "AB20591D-C843-4099-966D-D54101793288";
            var programDetailForSemestersBlockGuid = "539CFC03-C265-4D3F-BE11-B592E3969969";
            var programCompletionListBlockGuid = "319F2F80-A12C-48E8-B5A1-434C1BCF0AD2";
            var programCompletionDetailBlockGuid = "C30D2737-97F7-46C9-8DA8-EDA937EA0D15";

            // Block Type Guids
            var programCompletionDetailBlockTypeGuid = "e0c38a42-2ace-4258-8d11-bd971c41eadb";

            var programSummaryClassListPageAndRouteGuids = $"{programClassesPageGuid},{programSummaryClassesRouteGuid}";

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: {programClassesPageGuid},{programSummaryClassesRouteGuid} */
            RockMigrationHelper.AddBlockAttributeValue( programDetailForCoursesBlockGuid, "06B0D94D-7A16-4E4E-A53A-743EE89804D3", programSummaryClassListPageAndRouteGuids );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( programDetailForSemestersBlockGuid, "06B0D94D-7A16-4E4E-A53A-743EE89804D3", programSummaryClassListPageAndRouteGuids );

            // Require authentication for the Class Workspace page.
            RockMigrationHelper.AddSecurityAuthForPage(
                classWorkspacePageGuid,
                0,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUnAuthenticatedUsers,
                Guid.NewGuid().ToString() );

            // Failed when converted from Plugin Migration to EF Migration. Fixed with guid checks.
            var lmsAdminSecurityRoleGroupExists = Convert.ToBoolean( SqlScalar( "SELECT CAST (1 AS BIT) FROM [dbo].[Group] WHERE [Guid] = '5e0e02a9-f16b-437e-a9d9-c3d9d6afabb0'" ) );

            if ( lmsAdminSecurityRoleGroupExists == false )
            {
                RockMigrationHelper.AddSecurityRoleGroup( "RSR - LMS Administration", "Group of individuals who can administrate the various parts of the LMS functionality.", RSR_LMS_ADMINISTRATION_GUID );
            }

            var lmsWorkersSecurityRoleGroupExists = Convert.ToBoolean( SqlScalar( "SELECT CAST (1 AS BIT) FROM [dbo].[Group] WHERE [Guid] = 'B5481A0E-52E3-4D7B-93B5-A8F5C908DC67'" ) );

            if ( lmsWorkersSecurityRoleGroupExists == false )
            {
                RockMigrationHelper.AddSecurityRoleGroup( "RSR - LMS Workers", "Group of individuals who have basic access to the LMS functionality (such as facilitators).", RSR_LMS_WORKER_GUID );
            }

            // Programs Page.
            RockMigrationHelper.AddSecurityAuthForPage( programListPageGuid, 0, Authorization.VIEW, true, RSR_LMS_ADMINISTRATION_GUID, 0, "C8FD52E6-D626-4D07-B1D0-4F73ECEEFA27" );
            RockMigrationHelper.AddSecurityAuthForPage( programListPageGuid, 0, Authorization.EDIT, true, RSR_LMS_ADMINISTRATION_GUID, 0, "31BA820C-FBE6-4781-A7CC-4D60FA43506B" );
            RockMigrationHelper.AddSecurityAuthForPage( programListPageGuid, 0, Authorization.ADMINISTRATE, true, RSR_LMS_ADMINISTRATION_GUID, 0, "5532D681-F77E-47CF-83CA-75E1EF36045C" );
            RockMigrationHelper.AddSecurityAuthForPage( programListPageGuid, 1, Authorization.VIEW, true, RSR_LMS_WORKER_GUID, 0, "98D8FA2F-194D-46E8-A7AA-8FF3413A1621" );
            RockMigrationHelper.AddSecurityAuthForPage( programListPageGuid, 2, Authorization.VIEW, true, RSR_STAFF_LIKE_GUID, 0, "5DF0316A-AEF6-497B-8152-86EAD097D179" );
            RockMigrationHelper.AddSecurityAuthForPage( programListPageGuid, 3, Authorization.VIEW, true, RSR_STAFF_GUID, 0, "F777AE37-D28A-45FF-9419-95CA86E8E67A" );

            // Add the program completion detail page, route.
            RockMigrationHelper.AddPage( true, programCompletionsPageGuid, SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Program Completion Detail", "", programCompletionDetailPageGuid, "", "" );

            RockMigrationHelper.AddOrUpdatePageRoute( programCompletionDetailPageGuid, "people/learn/{LearningProgramId}/completions/{LearningProgramCompletionId}", programCompletionDetailRouteGuid );

            // Add Block 
            //  Block Name: Learning Program Detail
            //  Page Name: Completions
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, programCompletionDetailPageGuid.AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), programCompletionDetailBlockTypeGuid.AsGuid(), "Program Completion Detail", "Main", @"", @"", 0, programCompletionDetailBlockGuid );

            // Add Block Attribute Value
            //   Block: Learning Program Completion List
            //   BlockType: Learning Program Completion List
            //   Category: LMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( programCompletionListBlockGuid, "206A8316-1203-4661-A9E7-A4032C930075", $"{programCompletionDetailPageGuid},{programCompletionDetailRouteGuid}" );
        }

        #endregion

        #region ClassAttendancePage

        private const string RSR_ROCK_ADMINISTRATION_GUID = "628C51A8-4613-43ED-A18D-4A6FB999273E";

        private void AddLMSEntitySecurity()
        {
            // LMS Administration: View, Edit and Administrate.
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( LearningProgram ).FullName,
                0,
                Authorization.VIEW,
                true,
                RSR_LMS_ADMINISTRATION_GUID,
                0,
                "B878C8FE-B197-4616-A2A8-87BD3DDF1641" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( LearningProgram ).FullName,
                0,
                Authorization.EDIT,
                true,
                RSR_LMS_ADMINISTRATION_GUID,
                0,
                "2EDF461A-0D74-4E8A-844D-72997A49E210" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( LearningProgram ).FullName,
                0,
                Authorization.ADMINISTRATE,
                true,
                RSR_LMS_ADMINISTRATION_GUID,
                0,
                "1BE10CD9-8251-4B61-8DFB-1341B9B1C2D7" );

            // Rock Administration
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( LearningProgram ).FullName,
                0,
                Authorization.VIEW,
                true,
                RSR_ROCK_ADMINISTRATION_GUID,
                0,
                "A1F923D8-D3BF-4A72-BBC9-9AEDC3C3BA9C" );

            // LMS Worker: View.
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( LearningProgram ).FullName,
                0,
                Authorization.VIEW,
                true,
                RSR_LMS_WORKER_GUID,
                0,
                "BF394A67-2761-4FD9-AC19-FCDA0354159F" );

            // Staff: View.
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( LearningProgram ).FullName,
                0,
                Authorization.VIEW,
                true,
                RSR_STAFF_GUID,
                0,
                "AED1C50A-7758-40AB-A8B1-AAD7D2004017" );

            // Staff-Like Workers: View.
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( LearningProgram ).FullName,
                0,
                Authorization.VIEW,
                true,
                RSR_STAFF_LIKE_GUID,
                0,
                "281DB12F-B762-41F2-95F0-57C1010A38AF" );

            // Failed when converted from Plugin Migration to EF Migration. Fixed with guid check.
            var lmaAuthExists = Convert.ToBoolean( SqlScalar( "SELECT CAST (1 AS BIT) FROM [dbo].[Group] WHERE [Guid] = '5e0e02a9-f16b-437e-a9d9-c3d9d6afabb0'" ) );

            if ( lmaAuthExists == false )
            {
                RockMigrationHelper.AddSecurityAuthForEntityType(
                    typeof( LearningProgram ).FullName,
                    1,
                    Authorization.VIEW,
                    false,
                    null,
                    ( int ) SpecialRole.AllUsers,
                    "4DF13B7F-6B1A-4102-830B-5F8B27F87CA9" );
            }
        }

        private void AddAttendancePages()
        {
            var classDetailPageGuid = "23D5076E-C062-4987-9985-B3A4792BF3CE";
            var classAttendanceListPageGuid = "C96E184D-62CB-4E6B-A53D-496903240E25";
            var classAttendanceDetailPageGuid = "463F18E2-C575-4EA8-A458-DB768419F3B3";

            var classDetailBlockId = "C67D2164-33E5-46C0-94EF-DF387EF8477D";
            var attendancePageAttributeGuid = "B417E2A7-BBA1-453F-9933-3BE439CD2063";
            var detailPageAttributeGuid = "15299237-7F47-404D-BEFF-460F7818D3D7";

            var classAttendanceListRouteGuid = "D8595643-6703-4B18-B9F6-0D213D86ED47";
            var classAttendanceDetailRouteGuid = "1E296771-03DE-4D63-8851-EDC817791C0D";
            var groupAttendanceDetailBlockTypeGuid = "308DBA32-F656-418E-A019-9D18235027C1";
            var classAttendanceListBlockGuid = "5D01D91F-0AED-4955-96BE-E5309488BC74";
            var classAttendanceDetailBlockGuid = "BDB64C4D-392E-419B-96F2-6556652C958A";

            // Add the Attendance list and detail pages, routes and blocks.
            RockMigrationHelper.AddPage( true, classDetailPageGuid, SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Attendance List", "", classAttendanceListPageGuid, "", "" );
            RockMigrationHelper.AddPage( true, classAttendanceListPageGuid, SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Attendance Detail", "", classAttendanceDetailPageGuid, "", "" );

            RockMigrationHelper.AddOrUpdatePageRoute( classAttendanceListPageGuid, "people/learn/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/attendance", classAttendanceListRouteGuid );
            RockMigrationHelper.AddOrUpdatePageRoute( classAttendanceDetailPageGuid, "people/learn/{LearningProgramId}/courses/{LearningCourseId}/classes/{LearningClassId}/attendance/{GroupId}", classAttendanceDetailRouteGuid );

            // Add Block Attribute Value
            //   Block: Learning Class Detail
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Block Location: Page=Class, Site=Rock RMS
            //   Attribute: Attendance Page
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( classDetailBlockId, attendancePageAttributeGuid, $"{classAttendanceListPageGuid},{classAttendanceListRouteGuid}" );

            // Add Block 
            //  Block Name: Group Attendance Detail
            //  Page Name: Class Attendance
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, classAttendanceListPageGuid.AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), Rock.SystemGuid.BlockType.GROUP_ATTENDANCE_LIST.AsGuid(), "Attendance List", "Main", @"", @"", 0, classAttendanceListBlockGuid );

            // Add Block Attribute Value
            //   Block: Group Attendance List
            //   BlockType: Group Attendance List
            //   Category: LMS
            //   Block Location: Page=Attendance List, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( classAttendanceListBlockGuid, detailPageAttributeGuid, $"{classAttendanceDetailPageGuid},{classAttendanceDetailRouteGuid}" );

            // Add Block 
            //  Block Name: Group Attendance Detail
            //  Page Name: Class Attendance
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, classAttendanceDetailPageGuid.AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), groupAttendanceDetailBlockTypeGuid.AsGuid(), "Attendance Detail", "Main", @"", @"", 0, classAttendanceDetailBlockGuid );
        }

        #endregion

        #region MigrationRollupsForV16_8_0

        #region SMC: Update Volunteer Generosity Build Script (Campus Name)

        ///<summary>
        /// 
        /// </summary>
        private void UpdateVolGenBuildScript_CampusName_Up()
        {
            string newBuildScript = @"//- Retrieve the base URL for linking photos from a global attribute 
{% assign publicApplicationRoot = 'Global' | Attribute:'PublicApplicationRoot' %}

{% sql %}
DECLARE @NumberOfDays INT = 365;
DECLARE @NumberOfMonths INT = 13;
DECLARE @ServingAreaDefinedValueGuid UNIQUEIDENTIFIER = '36a554ce-7815-41b9-a435-93f3d52a2828';
DECLARE @ActiveRecordStatusValueId INT = (SELECT Id FROM DefinedValue WHERE Guid = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E');
DECLARE @ConnectionStatusDefinedTypeId INT = (SELECT Id FROM DefinedType WHERE [Guid] = '2e6540ea-63f0-40fe-be50-f2a84735e600');
DECLARE @StartDateKey INT = (SELECT TOP 1 [DateKey] FROM [AnalyticsSourceDate] WHERE [Date] = CAST(DATEADD(DAY, -@NumberOfDays, GETDATE()) AS DATE));
DECLARE @CurrentMonth DATE = DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0);
DECLARE @StartingDateKeyForGiving INT = (SELECT [DateKey] FROM [AnalyticsSourceDate] WHERE [Date] = DATEADD(MONTH, -@NumberOfMonths, @CurrentMonth));

;WITH CTE_Giving AS (
    SELECT
        p.[GivingId],
        asd.[DateKey],
        SUM(ftd.[Amount]) AS TotalAmount
    FROM
        [Person] p
        INNER JOIN [PersonAlias] pa ON pa.[PersonId] = p.[Id]
        INNER JOIN [FinancialTransaction] ft ON ft.[AuthorizedPersonAliasId] = pa.[Id]
        INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
        INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.[AccountId]
        INNER JOIN [AnalyticsSourceDate] asd ON asd.[DateKey] = ft.[TransactionDateKey]
    WHERE
        fa.[IsTaxDeductible] = 1 AND ft.[TransactionDateKey] >= @StartingDateKeyForGiving
    GROUP BY
        p.[GivingId], asd.[DateKey]
    HAVING
        SUM(ftd.[Amount]) > 0
),
CTE_GivingAggregated AS (
    SELECT
        GD.[GivingId],
        STRING_AGG(CAST(GD.[DateKey] AS VARCHAR(8)), '|') AS DonationDateKeys
    FROM
        CTE_Giving GD
    GROUP BY
        GD.[GivingId]
),
CTE_CampusShortCode AS (
    SELECT
        g.[Id] AS GroupId,
        CASE WHEN c.[ShortCode] IS NOT NULL AND c.[ShortCode] != '' THEN c.[ShortCode] ELSE c.[Name] END AS CampusShortCode,
		c.[Name] AS CampusName
    FROM
        [Group] g
        LEFT JOIN [Campus] c ON c.[Id] = g.[CampusId]
)
SELECT DISTINCT
    p.[Id] AS PersonId,
    CONCAT(CAST(p.[Id] AS NVARCHAR(12)), '-', CAST(g.[Id] AS NVARCHAR(12))) AS PersonGroupKey,
    p.[LastName],
    p.[NickName],
    p.[PhotoId],
    p.[GivingId],
    p.[Gender],
    p.[Age],
    p.[AgeClassification],
    g.[Id] AS GroupId,
    g.[Name] AS GroupName,
    csc.CampusShortCode,
    MAX(ao.[OccurrenceDate]) AS LastAttendanceDate,
    dvcs.[Value] AS ConnectionStatus,
    CAST(CASE WHEN p.[RecordStatusValueId] = @ActiveRecordStatusValueId AND gm.[IsArchived] = 0 THEN 1 ELSE 0 END AS BIT) AS IsActive,
    GR.DonationDateKeys
FROM
    [Person] p
    INNER JOIN [PersonAlias] pa ON pa.[PersonId] = p.[Id]
    INNER JOIN [Attendance] a ON a.[PersonAliasId] = pa.[Id]
    INNER JOIN [AttendanceOccurrence] ao ON ao.[Id] = a.[OccurrenceId]
    INNER JOIN [Group] g ON g.[Id] = ao.[GroupId]
    INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
    INNER JOIN [DefinedValue] dvp ON dvp.[Id] = gt.[GroupTypePurposeValueId] AND dvp.[Guid] = @ServingAreaDefinedValueGuid
    LEFT JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id] AND gm.[GroupId] = g.[Id]
    LEFT JOIN CTE_CampusShortCode csc ON csc.GroupId = g.[Id]
    LEFT JOIN [DefinedValue] dvcs ON dvcs.[Id] = p.[ConnectionStatusValueId] AND dvcs.[DefinedTypeId] = @ConnectionStatusDefinedTypeId
    LEFT JOIN CTE_GivingAggregated GR ON p.[GivingId] = GR.GivingId
WHERE
    ao.[OccurrenceDateKey] >= @StartDateKey AND a.[DidAttend] = 1
GROUP BY
    p.[Id], p.[LastName], p.[NickName], p.[PhotoId], p.[Gender], p.[Age], p.[AgeClassification], p.[GivingId], g.[Id], g.[Name], csc.[CampusShortCode], csc.[CampusName], dvcs.[Value], p.[RecordStatusValueId], gm.[IsArchived], GR.[DonationDateKeys];

{% endsql %}
{
    ""PeopleData"": [
    {% for result in results %}
        {% if forloop.first != true %},{% endif %}
        {
            ""PersonGroupKey"": {{ result.PersonGroupKey | ToJSON }},
            ""PersonId"": {{ result.PersonId }},
            ""LastName"": {{ result.LastName | ToJSON }},
            ""NickName"": {{ result.NickName | ToJSON }},
            ""Gender"": {{ result.Gender | ToJSON }},
            ""Age"": {{ result.Age | ToJSON }},
            ""AgeClassification"": {{ result.AgeClassification | ToJSON }},
            ""PhotoId"": {{ result.PhotoId | ToJSON }},
            ""GivingId"": {{ result.GivingId | ToJSON }},
            ""LastAttendanceDate"": ""{{ result.LastAttendanceDate | Date: 'yyyy-MM-dd' }}"",
            ""GroupId"": {{ result.GroupId }},
            ""GroupName"": {{ result.GroupName | ToJSON }},
            ""CampusShortCode"": {{ result.CampusShortCode | ToJSON }},
            ""ConnectionStatus"": {{ result.ConnectionStatus | ToJSON }},
            ""IsActive"": {{ result.IsActive }},
            ""DonationDateKeys"": {% if result.DonationDateKeys != null %}{{ result.DonationDateKeys | ToJSON}}{% else %}null{% endif %}
        }
    {% endfor %}
    ]
}
";

            Sql( $@"UPDATE [PersistedDataset]
           SET [BuildScript] = '{newBuildScript.Replace( "'", "''" )}'
           WHERE [Guid] = '10539E72-B5D3-48E2-B9C6-DB43AFDAD55F'" );

            Sql( $@"UPDATE [PersistedDataset]
               SET [ResultData] = null
               WHERE [Guid] = '10539E72-B5D3-48E2-B9C6-DB43AFDAD55F'" );
        }

        #endregion

        #region SK: Fix Obsidian Contribution Statement dates

        private void FixFinancialStatementTemplateUp()
        {
            string newValue = "| Date:''sd''";
            string oldValue = "| Date:''M/d/yyyy''";

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "ReportTemplate" );

            Sql( $@"UPDATE [dbo].[FinancialStatementTemplate] 
             SET [ReportTemplate] = REPLACE({targetColumn}, '{oldValue}', '{newValue}')
             WHERE {targetColumn} LIKE '%{oldValue}%'
                     AND [Guid] = '4B93657A-DD5F-4D8A-A13F-1B4E9ADBDAD0'" );
        }

        #endregion

        #region SMC: Fix Google Maps Lava Shortcode

        private void FixGoogleMapsLavaShortcodeUp()
        {
            Sql( @"UPDATE	[LavaShortcode]
SET	  [Markup] = REPLACE([Markup], '
            scrollwheel: {{ scrollwheel }},
            draggable: {{ draggable }},', '')
	, [Parameters] = REPLACE([Parameters], 'scrollwheel^true|draggable^true|gesturehandling^cooperative', 'gesturehandling^cooperative')
WHERE	[Guid] = 'FE298210-1307-49DF-B28B-3735A414CCA0';" );
        }

        #endregion

        #endregion

        #region ImprovePeerNetworkCalculations

        /// <summary>
        /// JPH: Improve Peer Network calculations - up.
        /// </summary>
        private void ImprovePeerNetworkCalculationsUp()
        {
            #region Update Peer Network Stored Procedure

            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spPeerNetwork_UpdateGroupConnections] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spPeerNetwork_UpdateGroupConnections]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spPeerNetwork_UpdateGroupConnections];" );

            Sql( HotFixMigrationResource._225_ImprovePeerNetworkCalculations_spPeerNetwork_UpdateGroupConnections );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );

            #endregion Update Peer Network Stored Procedure

            #region Update Peer Network > Peer Map Block

            RockMigrationHelper.UpdateHtmlContentBlock( "D2D0FF94-1816-4B43-A49D-104CC42A5DC3", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' %}

{% sql %}

    SELECT
        tp.[NickName] + ' ' + tp.[LastName] AS [TargetName]
        , tp.[Id] AS [TargetPersonId]
        , CAST( ROUND( SUM(pn.[RelationshipScore]), 0 ) AS INT ) AS [RelationshipScore]
        , MAX(pn.[RelationshipTrend]) AS [RelationshipTrend]
        , pn.[RelationshipTypeValueId]
        , pn.[RelatedEntityId]
        , pn.[Caption]
    FROM [PeerNetwork] pn
        INNER JOIN [Person] tp ON tp.[Id] = pn.[TargetPersonId]
    WHERE [SourcePersonId] = {{ personId }}
    GROUP BY tp.[NickName]
        , tp.[LastName]
        , tp.[Id]
        , pn.[RelationshipTypeValueId]
        , pn.[RelatedEntityId]
        , pn.[Caption];

{% endsql %}

<div class=""card card-profile card-peer-map panel-widget"">

    <div class=""card-header"">
        <span class=""card-title"">Peer Map</span>
    </div>

    <div class=""card-section p-0"">
        <div style=""height: 800px"">
            {[ networkgraph height:'100%' minimumnodesize:'10' highlightcolor:'#bababa' ]}

                {% assign followingConnectionsValueId = '84E0360E-0828-E5A5-4BCC-F3113BE338A1' | GuidToId:'DefinedValue' %}
                {% assign following = results | Where:'RelationshipTypeValueId', followingConnectionsValueId %}

                [[ node id:'F-MASTER' label:'Following' color:'#36cf8c' ]][[ endnode ]]

                {% for followed in following %}
                    [[ node id:'F-{{ followed.TargetPersonId }}' label:'{{ followed.TargetName }}' color:'#88ebc0' size:'10' ]][[ endnode ]]
                    [[ edge source:'F-MASTER' target:'F-{{ followed.TargetPersonId }}' color:'#c4c4c4' ]][[ endedge ]]
                {% endfor %}

                {% assign groupConnectionsValueId = 'CB51DC46-FBDB-43DA-B7F3-60E7C6E70F40' | GuidToId:'DefinedValue' %}
                {% assign groups = results | Where:'RelationshipTypeValueId', groupConnectionsValueId | GroupBy:'RelatedEntityId' %}

                {% for group in groups %}
                    {% assign parts = group | PropertyToKeyValue %}

                    {% assign groupName = parts.Value | First | Property:'Caption' %}
                    [[ node id:'G-{{ parts.Key }}' label:""{{ groupName }}"" color:'#4e9fd9' ]][[ endnode ]]

                    {% for member in parts.Value %}
                        [[ node id:'GM-{{ parts.Key }}-{{ member.TargetPersonId }}' label:'{{ member.TargetName }}' color:'#a6d5f7' ]][[ endnode ]]

                        [[ edge source:'GM-{{ parts.Key }}-{{ member.TargetPersonId }}' target:'G-{{ parts.Key }}' color:'#c4c4c4' ]][[ endedge ]]
                    {% endfor %}

                {% endfor %}

            {[ endnetworkgraph ]}
        </div>
    </div>
</div>", "A311EB92-5BB5-407D-AF6C-74BC9FB9FA64" );

            #endregion Update Peer Network > Peer Map Block
        }

        /// <summary>
        /// JPH: Improve Peer Network calculations - down.
        /// </summary>
        private void ImprovePeerNetworkCalculationsDown()
        {
            #region Revert Peer Network > Peer Map Block

            RockMigrationHelper.UpdateHtmlContentBlock( "D2D0FF94-1816-4B43-A49D-104CC42A5DC3", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' %}

{% sql %}

    SELECT 
        tp.[NickName] + ' ' + tp.[LastName] AS [TargetName]
        , tp.[Id] AS [TargetPersonId]
        , pn.[RelationshipScore]
        , pn.[RelationshipTrend]
        , pn.[RelationshipTypeValueId]
        , pn.[RelatedEntityId]
        , pn.[Caption]
    FROM [PeerNetwork] pn
        INNER JOIN [Person] tp ON tp.[Id] = pn.[TargetPersonId]
    WHERE [SourcePersonId] = {{ personId }};

{% endsql %}

<div class=""card card-profile card-peer-map panel-widget"">

    <div class=""card-header"">
        <span class=""card-title"">Peer Map</span>
    </div>

    <div class=""card-section p-0"">
        <div style=""height: 800px"">
            {[ networkgraph height:'100%' minimumnodesize:'10' highlightcolor:'#bababa' ]}

                {% assign followingConnectionsValueId = '84E0360E-0828-E5A5-4BCC-F3113BE338A1' | GuidToId:'DefinedValue' %}
                {% assign following = results | Where:'RelationshipTypeValueId', followingConnectionsValueId %}

                [[ node id:'F-MASTER' label:'Following' color:'#36cf8c' ]][[ endnode ]]

                {% for followed in following %}
                    [[ node id:'F-{{ followed.TargetPersonId }}' label:'{{ followed.TargetName }}' color:'#88ebc0' size:'10' ]][[ endnode ]]
                    [[ edge source:'F-MASTER' target:'F-{{ followed.TargetPersonId }}' color:'#c4c4c4' ]][[ endedge ]]
                {% endfor %}

                {% assign groupConnectionsValueId = 'CB51DC46-FBDB-43DA-B7F3-60E7C6E70F40' | GuidToId:'DefinedValue' %}
                {% assign groups = results | Where:'RelationshipTypeValueId', groupConnectionsValueId | GroupBy:'RelatedEntityId' %}

                {% for group in groups %}
                    {% assign parts = group | PropertyToKeyValue %}

                    {% assign groupName = parts.Value | First | Property:'Caption' %}
                    [[ node id:'G-{{ parts.Key }}' label:""{{ groupName }}"" color:'#4e9fd9' ]][[ endnode ]]

                    {% for member in parts.Value %}
                        [[ node id:'GM-{{ parts.Key }}-{{ member.TargetPersonId }}' label:'{{ member.TargetName }}' color:'#a6d5f7' ]][[ endnode ]]

                        [[ edge source:'GM-{{ parts.Key }}-{{ member.TargetPersonId }}' target:'G-{{ parts.Key }}' color:'#c4c4c4' ]][[ endedge ]]
                    {% endfor %}

                {% endfor %}

            {[ endnetworkgraph ]}
        </div>
    </div>
</div>", "A311EB92-5BB5-407D-AF6C-74BC9FB9FA64" );

            #endregion Revert Peer Network > Peer Map Block

            #region Revert Peer Network Stored Procedure

            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Roll back to the pervious version of [spPeerNetwork_UpdateGroupConnections] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spPeerNetwork_UpdateGroupConnections]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spPeerNetwork_UpdateGroupConnections];" );

            Sql( HotFixMigrationResource._221_PeerNetworkPageAndBlocks_spPeerNetwork_UpdateGroupConnections );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );

            #endregion Revert Peer Network Stored Procedure
        }

        #endregion

        #region PeerNetworkFitNFinish

        /// <summary>
        /// JPH: Peer Network Fit-n-Finish - up.
        /// </summary>
        private void PeerNetworkFitNFinishUp()
        {
            #region Update Person Profile > Peer Network Block

            RockMigrationHelper.UpdateHtmlContentBlock( "6094C135-10E2-4AF4-A46B-1FC6D073A854", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' %}

{% sql %}

    SELECT
        TOP 20
        tp.[NickName] + ' ' + tp.[LastName] AS [TargetName]
        , tp.[Id] AS [TargetPersonId]
        , CAST( ROUND( SUM(pn.[RelationshipScore]), 0 ) AS INT ) AS [RelationshipScore]
        , MAX(pn.[RelationshipTrend]) AS [RelationshipTrend]
        , SUM( pn.[RelationshipScore] ) - SUM( pn.[RelationshipScoreLastUpdateValue] ) AS [PointDifference]
    FROM [PeerNetwork] pn
        INNER JOIN [Person] tp ON tp.[Id] = pn.[TargetPersonId]
    WHERE [SourcePersonId] = {{ personId }}
    GROUP BY tp.[NickName]
        , tp.[LastName]
        , tp.[Id]
    ORDER BY [RelationshipScore] DESC
        , tp.[LastName]
        , tp.[NickName];

{% endsql %}

<div class=""card card-profile card-peer-network panel-widget"">

    <div class=""card-header"">
        <span class=""card-title"">Peer Network</span>

        <div class=""panel-labels"">
            <a href=""/person/{{ personId }}/peer-graph""><span class=""label label-default"">Peer Graph</span></a>
        </div>
    </div>

    <div class=""card-section"">
        <div class=""row"">
            {% for peer in results %}
                <div class=""col-xs-8"">
                    <a href=""/person/{{ peer.TargetPersonId }}"">
                        {{ peer.TargetName }}
                    </a>
                </div>
                <div class=""col-xs-2"">{{ peer.RelationshipScore }}</div>
                <div class=""col-xs-2"">
                    {% if peer.PointDifference > 0 %}
                        <i class=""fa fa-arrow-up text-success""></i>
                    {% elseif peer.PointDifference < 0 %}
                        <i class=""fa fa-arrow-down text-danger""></i>
                    {% else %}
                        <i class=""fa fa-minus text-muted""></i>
                    {% endif %}
                </div>
            {% endfor %}
        </div>
    </div>
</div>", "879C5623-3A45-4D6F-9759-F9A294D7425B" );

            #endregion Update Person Profile > Peer Network Block

            #region Update Peer Network > Peer List Block

            RockMigrationHelper.UpdateHtmlContentBlock( "46775056-3ADF-43CD-809A-88EE3378C039", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' %}

{% sql %}

    SELECT
        tp.[NickName] + ' ' + tp.[LastName] AS [TargetName]
        , tp.[Id] AS [TargetPersonId]
        , CAST( ROUND( SUM(pn.[RelationshipScore]), 0 ) AS INT ) AS [RelationshipScore]
        , MAX(pn.[RelationshipTrend]) AS [RelationshipTrend]
        , SUM( pn.[RelationshipScore] ) - SUM( pn.[RelationshipScoreLastUpdateValue] ) AS [PointDifference]
    FROM [PeerNetwork] pn
        INNER JOIN [Person] tp ON tp.[Id] = pn.[TargetPersonId]
    WHERE [SourcePersonId] = {{ personId }}
    GROUP BY tp.[NickName]
        , tp.[LastName]
        , tp.[Id]
    ORDER BY [RelationshipScore] DESC
        , tp.[LastName]
        , tp.[NickName];

{% endsql %}

<div class=""card card-profile card-peer-network panel-widget"">

    <div class=""card-header"">
        <span class=""card-title"">Full Peer Network</span>
    </div>

    <div class=""card-section"">
        <div class=""row"">
            {% for peer in results %}
                <div class=""col-xs-8"">
                    <a href=""/person/{{ peer.TargetPersonId }}"">
                        {{ peer.TargetName }}
                    </a>
                </div>
                <div class=""col-xs-2"">{{ peer.RelationshipScore }}</div>
                <div class=""col-xs-2"">
                    {% if peer.PointDifference > 0 %}
                        <i class=""fa fa-arrow-up text-success""></i>
                    {% elseif peer.PointDifference < 0 %}
                        <i class=""fa fa-arrow-down text-danger""></i>
                    {% else %}
                        <i class=""fa fa-minus text-muted""></i>
                    {% endif %}
                </div>
            {% endfor %}
        </div>
    </div>
</div>", "0A35B353-E14E-4B9C-8E0C-7E7D0863A67B" );

            #endregion Update Peer Network > Peer List Block

            #region Update Peer Network Job

            Sql( $@"
UPDATE [ServiceJob]
SET [IsSystem] = 1
WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.CALCULATE_PEER_NETWORK}';" );

            RockMigrationHelper.AddEntityAttributeIfMissing( "Rock.Model.ServiceJob", SystemGuid.FieldType.BOOLEAN, "Class", "Rock.Jobs.CalculatePeerNetwork", "Calculate Peer Network for Following", "Determines if peer networks should be calculated for followed individuals.", 0, true.ToString(), "E7EF5FAF-9858-4D5B-B15E-FC474D382E3E", "CalculatePeerNetworkForFollowing", false );

            RockMigrationHelper.AddEntityAttributeIfMissing( "Rock.Model.ServiceJob", SystemGuid.FieldType.BOOLEAN, "Class", "Rock.Jobs.CalculatePeerNetwork", "Calculate Peer Network for Groups", "Determines if peer networks should be calculated for individuals in groups.", 1, true.ToString(), "193E2FBC-9E38-4B30-BFB7-4B17A204A004", "CalculatePeerNetworkForGroups", false );

            RockMigrationHelper.AddEntityAttributeIfMissing( "Rock.Model.ServiceJob", SystemGuid.FieldType.INTEGER, "Class", "Rock.Jobs.CalculatePeerNetwork", "Command Timeout", "Maximum amount of time (in seconds) to wait for the sql operations to complete. Leave blank to use the default for this job (3600). Note, some operations could take several minutes, so you might want to set it at 3600 (60 minutes) or higher.", 2, 3600.ToString(), "B5823990-985D-461C-ACD2-D8F6B75DBA11", "CommandTimeout", false );

            #endregion Update Peer Network Job
        }

        /// <summary>
        /// JPH: Peer Network Fit-n-Finish - down.
        /// </summary>
        private void PeerNetworkFitNFinishDown()
        {
            #region Update Person Profile > Peer Network Block

            RockMigrationHelper.UpdateHtmlContentBlock( "6094C135-10E2-4AF4-A46B-1FC6D073A854", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' %}

{% sql %}

    SELECT
        TOP 20
        tp.[NickName] + ' ' + tp.[LastName] AS [TargetName]
        , tp.[Id] AS [TargetPersonId]
        , CAST( ROUND( SUM(pn.[RelationshipScore]), 0 ) AS INT ) AS [RelationshipScore]
        , MAX(pn.[RelationshipTrend]) AS [RelationshipTrend]
        , SUM( pn.[RelationshipScore] ) - SUM( pn.[RelationshipScoreLastUpdateValue] ) AS [PointDifference]
    FROM [PeerNetwork] pn
        INNER JOIN [Person] tp ON tp.[Id] = pn.[TargetPersonId]
    WHERE [SourcePersonId] = {{ personId }}
    GROUP BY tp.[NickName], tp.[LastName], tp.[Id]
    ORDER BY [RelationshipScore] DESC;

{% endsql %}

<div class=""card card-profile card-peer-network panel-widget"">

    <div class=""card-header"">
        <span class=""card-title"">Peer Network</span>

        <div class=""panel-labels"">
            <a href=""/person/{{ personId }}/peer-graph""><span class=""label label-default"">Peer Graph</span></a>
        </div>
    </div>

    <div class=""card-section"">
        <div class=""row"">
            {% for peer in results %}
                <div class=""col-xs-8"">
                    <a href=""/person/{{ peer.TargetPersonId }}"">
                        {{ peer.TargetName }}
                    </a>
                </div>
                <div class=""col-xs-2"">{{ peer.RelationshipScore }}</div>
                <div class=""col-xs-2"">
                    {% if peer.PointDifference > 0 %}
                        <i class=""fa fa-arrow-up text-success""></i>
                    {% elseif peer.PointDifference < 0 %}
                        <i class=""fa fa-arrow-down text-danger""></i>
                    {% else %}
                        <i class=""fa fa-minus text-muted""></i>
                    {% endif %}
                </div>
            {% endfor %}
        </div>
    </div>
</div>", "879C5623-3A45-4D6F-9759-F9A294D7425B" );

            #endregion Update Person Profile > Peer Network Block

            #region Update Peer Network > Peer List Block

            RockMigrationHelper.UpdateHtmlContentBlock( "46775056-3ADF-43CD-809A-88EE3378C039", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' %}

{% sql %}

    SELECT
        tp.[NickName] + ' ' + tp.[LastName] AS [TargetName]
        , tp.[Id] AS [TargetPersonId]
        , CAST( ROUND( SUM(pn.[RelationshipScore]), 0 ) AS INT ) AS [RelationshipScore]
        , MAX(pn.[RelationshipTrend]) AS [RelationshipTrend]
        , SUM( pn.[RelationshipScore] ) - SUM( pn.[RelationshipScoreLastUpdateValue] ) AS [PointDifference]
    FROM [PeerNetwork] pn
        INNER JOIN [Person] tp ON tp.[Id] = pn.[TargetPersonId]
    WHERE [SourcePersonId] = {{ personId }}
    GROUP BY tp.[NickName], tp.[LastName], tp.[Id]
    ORDER BY [RelationshipScore] DESC;

{% endsql %}

<div class=""card card-profile card-peer-network panel-widget"">

    <div class=""card-header"">
        <span class=""card-title"">Full Peer Network</span>
    </div>

    <div class=""card-section"">
        <div class=""row"">
            {% for peer in results %}
                <div class=""col-xs-8"">
                    <a href=""/person/{{ peer.TargetPersonId }}"">
                        {{ peer.TargetName }}
                    </a>
                </div>
                <div class=""col-xs-2"">{{ peer.RelationshipScore }}</div>
                <div class=""col-xs-2"">
                    {% if peer.PointDifference > 0 %}
                        <i class=""fa fa-arrow-up text-success""></i>
                    {% elseif peer.PointDifference < 0 %}
                        <i class=""fa fa-arrow-down text-danger""></i>
                    {% else %}
                        <i class=""fa fa-minus text-muted""></i>
                    {% endif %}
                </div>
            {% endfor %}
        </div>
    </div>
</div>", "0A35B353-E14E-4B9C-8E0C-7E7D0863A67B" );

            #endregion Update Peer Network > Peer List Block

            #region Update Peer Network Job

            Sql( $@"
UPDATE [ServiceJob]
SET [IsSystem] = 0
WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.CALCULATE_PEER_NETWORK}';" );

            RockMigrationHelper.DeleteAttribute( "E7EF5FAF-9858-4D5B-B15E-FC474D382E3E" );
            RockMigrationHelper.DeleteAttribute( "193E2FBC-9E38-4B30-BFB7-4B17A204A004" );
            RockMigrationHelper.DeleteAttribute( "B5823990-985D-461C-ACD2-D8F6B75DBA11" );

            #endregion Update Peer Network Job
        }

        #endregion

        #region NA: Add Kids and Students Classifications and Metrics

        // Defined Value Guids
        private const string DEFINED_VALUE_TOTAL_CHILDRENS_ATTENDANCE_GUID = "9B16A979-48B1-4180-B44F-57FCD38A103A";
        private const string DEFINED_VALUE_TOTAL_STUDENTS_ATTENDANCE_GUID = "8EC797E4-7DCE-4A70-B1E8-9B21192476C3";

        // Metric Guids 
        private const string METRIC_TOTAL_CHILDRENS_ATTENDANCE = "1747F42D-791D-41D5-BFFC-49D6C31B9549";
        private const string METRIC_TOTAL_STUDENTS_ATTENDANCE = "310E5B92-E744-4E69-A832-9E395191A91C";

        // Other Guids - Already included
        //private const string WeeklyScheduleGuid = "C31DF106-D7C8-4B64-81E7-5C4AB20DBA7B";
        //private const string WeeklyMetricsCategory = "64B29ADE-144D-4E84-96CC-A79398589733";

        /// <summary>
        /// This code is largely borrowed from previous rollup migrations 
        /// such as 202_MigrationRollupsForV17_0_9.cs and 211_MigrationRollupsForV17_0_17.cs.
        /// </summary>
        private void AddNewMeasurementClassificationDefinedValuesAndMetrics()
        {
            // Add Total Children's Attendance Defined Value.
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "Total Children's Attendance",
                "This metric measures the total children's (0-6th grade) weekend attendance for the organization, partitioned by Campus > Schedule.",
                DEFINED_VALUE_TOTAL_CHILDRENS_ATTENDANCE_GUID );
            Sql( $"UPDATE [DefinedValue] SET [Order] = 0 WHERE [Guid] = '{DEFINED_VALUE_TOTAL_CHILDRENS_ATTENDANCE_GUID}'" );

            // Add Total Students Attendance Defined Value.
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.MEASUREMENT_CLASSIFICATION,
                "Total Students Attendance",
                "This metric measures the total students (7-12th grade) weekend attendance for the organization, partitioned by Campus > Schedule.",
                DEFINED_VALUE_TOTAL_STUDENTS_ATTENDANCE_GUID );
            Sql( $"UPDATE [DefinedValue] SET [Order] = 0 WHERE [Guid] = '{DEFINED_VALUE_TOTAL_STUDENTS_ATTENDANCE_GUID}'" );

            // Add two new Metrics

            // Add Total Children's Attendance Metric
            AddMetric( METRIC_TOTAL_CHILDRENS_ATTENDANCE,
                "Total Children's Attendance",
                WeeklyMetricsCategory,
                @"
-- Feel free to replace this with your own SQL or updated Check-in Areas below.
DECLARE @STARTDATE DATETIME = CONVERT(date, DATEADD(DAY, -7, dbo.RockGetDate()))
DECLARE @ENDDATE DATETIME = CONVERT(date, dbo.RockGetDate())

SELECT COUNT(1) as AttendanceCount, a.[CampusId], oa.[ScheduleId]
FROM [Attendance] a
INNER JOIN [AttendanceOccurrence] oa ON oa.Id = a.[OccurrenceId]
INNER JOIN [Group] g ON g.Id = oa.[GroupId]
INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
WHERE
    gt.[AttendanceCountsAsWeekendService] = 1
    AND a.[DidAttend] = 1 
    AND CONVERT(date, a.[StartDateTime]) BETWEEN @STARTDATE AND @ENDDATE
    AND gt.[Guid] IN (
          'CADB2D12-7836-44BC-8EEA-3C6AB22FD5E8' -- Nursery/Preschool Area
        , 'E3C8F7D6-5CEB-43BB-802F-66C3E734049E' -- Elementary Area
    )
GROUP BY ALL a.[CampusId], oa.[ScheduleId]
",
                new List<NewMeasurementClassificationDefinedValuesAndMetrics_PartitionDetails>() { new NewMeasurementClassificationDefinedValuesAndMetrics_PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ), new NewMeasurementClassificationDefinedValuesAndMetrics_PartitionDetails( "Schedule", SystemGuid.EntityType.SCHEDULE ) },
                WeeklyScheduleGuid,
                "This metric represents attendance records (total for the week) for any group(s) per campus of GroupTypes that have the Weekend Service field checked and are part of the Children's Check-in Areas (Nursery/Preschool Area and Elementary Area).",
                DEFINED_VALUE_TOTAL_CHILDRENS_ATTENDANCE_GUID );

            // Add Total Students Attendance Metric
            AddMetric( METRIC_TOTAL_STUDENTS_ATTENDANCE,
                "Total Students Attendance",
                WeeklyMetricsCategory,
                @"
-- Feel free to replace this with your own SQL or updated Check-in Areas below.
DECLARE @STARTDATE DATETIME = CONVERT(date, DATEADD(DAY, -7, dbo.RockGetDate()))
DECLARE @ENDDATE DATETIME = CONVERT(date, dbo.RockGetDate())

SELECT COUNT(1) as AttendanceCount, a.[CampusId], oa.[ScheduleId]
FROM [Attendance] a
INNER JOIN [AttendanceOccurrence] oa ON oa.Id = a.[OccurrenceId]
INNER JOIN [Group] g ON g.Id = oa.[GroupId]
INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
WHERE
    gt.[AttendanceCountsAsWeekendService] = 1
    AND a.[DidAttend] = 1 
    AND CONVERT(date, a.[StartDateTime]) BETWEEN @STARTDATE AND @ENDDATE
    AND gt.[Guid] IN (
          '7A17235B-69AD-439B-BAB0-1A0A472DB96F' -- Jr High Area
        , '9A88743B-F336-4404-B877-2A623689195D' -- High School Area
    )
GROUP BY ALL a.[CampusId], oa.[ScheduleId]
",
                new List<NewMeasurementClassificationDefinedValuesAndMetrics_PartitionDetails>() { new NewMeasurementClassificationDefinedValuesAndMetrics_PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ), new NewMeasurementClassificationDefinedValuesAndMetrics_PartitionDetails( "Schedule", SystemGuid.EntityType.SCHEDULE ) },
                WeeklyScheduleGuid,
                "This metric represents attendance records (total for the week) for any group(s) per campus of GroupTypes that have the Weekend Service field checked and are part of the Students Check-in Areas (Jr High Area and High School Area).",
                DEFINED_VALUE_TOTAL_STUDENTS_ATTENDANCE_GUID );
        }

        /// <summary>
        /// This method comes from the original 202_MigrationRollupsForV17_0_9.cs rollup.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="title"></param>
        /// <param name="categoryGuid"></param>
        /// <param name="sourceSql"></param>
        /// <param name="partitions"></param>
        /// <param name="scheduleGuid"></param>
        /// <param name="description"></param>
        /// <param name="measurementClassificationValueGuid"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private void AddMetric( string guid, string title, string categoryGuid, string sourceSql, List<NewMeasurementClassificationDefinedValuesAndMetrics_PartitionDetails> partitions, string scheduleGuid, string description, string measurementClassificationValueGuid )
        {
            var formattedTitle = title?.Replace( "'", "''" ) ?? throw new ArgumentNullException( nameof( title ) );
            var createMetricAndMetricCategorySql = $@"DECLARE @MetricId [int] = (SELECT [Id] FROM dbo.[Metric] WHERE [Guid] = '{guid}')
    , @SourceValueTypeId [int] = (SELECT [Id] FROM dbo.[DefinedValue] WHERE [Guid] = '{SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_SQL}')
    , @MetricCategoryId [int] = (SELECT [Id] FROM dbo.[Category] WHERE [Guid] = '{categoryGuid}')
    , @Description [varchar] (max) = {( string.IsNullOrWhiteSpace( description ) ? "NULL" : $"'{description.Replace( "'", "''" )}'" )}
    , @MeasurementClassificationId [int] = (SELECT [Id] FROM dbo.[DefinedValue] WHERE [Guid] = '{measurementClassificationValueGuid}');

IF (@MetricId IS NULL AND @SourceValueTypeId IS NOT NULL AND @MetricCategoryId IS NOT NULL)
BEGIN
    DECLARE @Now [datetime] = GETDATE();
    INSERT INTO dbo.[Metric]
    (
        [IsSystem]
        , [Title]
        , [Description]
        , [IsCumulative]
        , [SourceValueTypeId]
        , [SourceSql]
        , [ScheduleId]
        , [CreatedDateTime]
        , [ModifiedDateTime]
        , [Guid]
        , [NumericDataType]
        , [EnableAnalytics]
        , [MeasurementClassificationValueId]
    )
    VALUES
    (
        0
        , '{formattedTitle}'
        , @Description
        , 0
        , @SourceValueTypeId
        , '{sourceSql.Replace( "'", "''" )}'
        , (SELECT [Id] FROM Schedule WHERE Guid = '{scheduleGuid}')
        , @Now
        , @Now
        , '{guid}'
        , 1
        , 0
        , @MeasurementClassificationId
    );
    SET @MetricId = SCOPE_IDENTITY();
    INSERT INTO dbo.[MetricCategory]
    (
        [MetricId]
        , [CategoryId]
        , [Order]
        , [Guid]
    )
    VALUES
    (
        @MetricId
        , @MetricCategoryId
        , 0
        , NEWID()
    );";
            var sqlBuilder = new StringBuilder( createMetricAndMetricCategorySql );

            if ( partitions == null || partitions.Count == 0 )
            {
                sqlBuilder.Append( @"INSERT INTO dbo.[MetricPartition]
    (
        [MetricId]
        , [IsRequired]
        , [Order]
        , [Guid]
    )
    VALUES
    (
        @MetricId
        , 1
        , 0
        , NEWID()
    );" );
            }
            else
            {
                foreach ( var partitionDetail in partitions )
                {
                    var createMetricPartitionSql = $@"INSERT INTO dbo.[MetricPartition]
    (
        [MetricId]
        , [Label]
        , [EntityTypeId]
        , [IsRequired]
        , [Order]
        , [CreatedDateTime]
        , [ModifiedDateTime]
        , [Guid]
    )
    VALUES
    (
        @MetricId
        , '{partitionDetail.Label}'
        , (SELECT Id FROM dbo.[EntityType] WHERE [GUID] = '{partitionDetail.EntityTypeGuid}')
        , 1
        , {partitions.IndexOf( partitionDetail )}
        , @Now
        , @Now
        , NEWID()
    );";
                    sqlBuilder.Append( createMetricPartitionSql );
                }
            }

            sqlBuilder.AppendLine( "END" );

            Sql( sqlBuilder.ToString() );
        }


        /// <summary>
        /// This idea comes from the original 202_MigrationRollupsForV17_0_9.cs rollup.
        /// </summary>
        private sealed class NewMeasurementClassificationDefinedValuesAndMetrics_PartitionDetails
        {
            public NewMeasurementClassificationDefinedValuesAndMetrics_PartitionDetails( string label, string entityTypeGuid )
            {
                Label = label;
                EntityTypeGuid = entityTypeGuid;
            }

            /// <summary>
            /// Gets or sets the label.
            /// </summary>
            /// <value>
            /// The label.
            /// </value>
            public string Label { get; set; }

            /// <summary>
            /// Gets or sets the entity type unique identifier.
            /// </summary>
            /// <value>
            /// The entity type unique identifier.
            /// </value>
            public string EntityTypeGuid { get; set; }
        }

        #endregion

        #region KH: Update Account Entry Block Attributes

        private void UpdateSecurityChopJob()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop AccountEntry and Login",
                blockTypeReplacements: new Dictionary<string, string> {
{ "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "E5C34503-DDAD-4881-8463-0E1E20B1675D" }, // Account Entry
{ "7B83D513-1178-429E-93FF-E76430E038E4", "5437C991-536D-4D9C-BE58-CBDB59D1BBB3" }, // Login
                },
                migrationStrategy: "Chop",
                jobGuid: "A65D26C1-229E-4198-B388-E269C3534BC0",
                blockAttributeKeysToIgnore: new Dictionary<string, string> {
                    { "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "ShowGender,AddressRequired,ShowAddress" }, // Account Entry
                    { "7B83D513-1178-429E-93FF-E76430E038E4", "RemoteAuthorizationTypes" }, // Login
            } );
        }

        private void UpdateAccountEntryBlockAttribute()
        {
            // Attribute for Obsidian BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Gender
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Gender", "Gender", "Gender", @"How should Gender be displayed.", 28, @"Optional", "ACC7FB9D-4B0B-4C1A-9347-3217991D94B5" );

            // Attribute for Obsidian BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Address
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Address", "Address", "Address", @"How should Address be displayed.", 15, @"Optional", "E7F73AA5-DCB5-4102-BBC5-C6833E4483E1" );

            Sql( @"
DECLARE @showGenderAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '442A7BF5-50E0-4DD8-9BB2-F36160DEB50B')
DECLARE @genderAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'ACC7FB9D-4B0B-4C1A-9347-3217991D94B5')
DECLARE @showAddressAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'F0650100-74A3-4356-9DCA-E05F74453699')
DECLARE @addressRequiredAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '0112FC95-B856-4C34-B693-CD9E296C93D5')
DECLARE @addressAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E7F73AA5-DCB5-4102-BBC5-C6833E4483E1')
-- Updating Gender Attribute
IF EXISTS (SELECT 1 FROM [Attribute] WHERE [Id] = @showGenderAttributeId)
BEGIN
    -- Delete existing attribute value first (might have been created by Rock system)
    DELETE FROM [AttributeValue]
    WHERE [AttributeId] = @genderAttributeId
    -- Insert new AttributeValues based on the old ones
    INSERT INTO [AttributeValue] (
        [IsSystem], [AttributeId], [EntityId], [Value], [Guid]
    )
    SELECT 
        [IsSystem],
        @genderAttributeId,
        [EntityId],
        CASE 
            WHEN [Value] = 'False' THEN 'Hide'
            ELSE 'Optional'
        END,
        NEWID()
    FROM 
        [AttributeValue] AS [showGenderAttributeValue]
    WHERE 
        [showGenderAttributeValue].[AttributeId] = @showGenderAttributeId
        AND NOT EXISTS (
            SELECT 1 
            FROM [AttributeValue] AS [newAttributeValue]
            WHERE [newAttributeValue].[AttributeId] = @genderAttributeId
            AND [newAttributeValue].[EntityId] = [showGenderAttributeValue].[EntityId]
        )
END
-- Updating Address Attribute
IF EXISTS (SELECT 1 FROM [Attribute] WHERE [Id] = @showAddressAttributeId)
BEGIN
    -- Delete existing attribute value first (might have been created by Rock system)
    DELETE FROM [AttributeValue]
    WHERE [AttributeId] = @addressAttributeId
    -- Insert new AttributeValues based on the old ones
    INSERT INTO [AttributeValue] (
        [IsSystem], [AttributeId], [EntityId], [Value], [Guid]
    )
    SELECT 
        [IsSystem],
        @addressAttributeId,
        [EntityId],
        CASE 
            WHEN [Value] = 'False' THEN 'Hide'
            WHEN EXISTS (
                SELECT 1 
                FROM [AttributeValue] AS [addressRequiredAttributeValue]
                WHERE [addressRequiredAttributeValue].[AttributeId] = @addressRequiredAttributeId
                AND [addressRequiredAttributeValue].[EntityId] = [showAddressAttributeValue].[EntityId]
                AND [addressRequiredAttributeValue].[Value] = 'True'
            ) THEN 'Required'
            ELSE 'Optional'
        END,
        NEWID()
    FROM 
        [AttributeValue] AS [showAddressAttributeValue]
    WHERE 
        [showAddressAttributeValue].[AttributeId] = @showAddressAttributeId
        AND NOT EXISTS (
            SELECT 1 
            FROM [AttributeValue] AS [newAttributeValue]
            WHERE [newAttributeValue].[AttributeId] = @addressAttributeId
            AND [newAttributeValue].[EntityId] = [showAddressAttributeValue].[EntityId]
        )
END" );

            // Update the Security Chop Job to ignore deleted attributes.
            UpdateSecurityChopJob();

            // Delete old Attributes

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Show Gender
            RockMigrationHelper.DeleteAttribute( "442A7BF5-50E0-4DD8-9BB2-F36160DEB50B" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Show Address
            RockMigrationHelper.DeleteAttribute( "F0650100-74A3-4356-9DCA-E05F74453699" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Obsidian > Security
            //   Attribute: Address Required
            RockMigrationHelper.DeleteAttribute( "0112FC95-B856-4C34-B693-CD9E296C93D5" );
        }

        #endregion

        #region SK: Google Map Short Code Update to remove all the JavaScript warning console

        private void UpdateGoogleMapShortcode_Up()
        {
            Sql( @"UPDATE LavaShortcode SET [Markup]=REPLACE([Markup],'https://maps.googleapis.com/maps/api/js?key=','https://maps.googleapis.com/maps/api/js?loading=async&key=') where [Guid]='FE298210-1307-49DF-B28B-3735A414CCA0'
          UPDATE LavaShortcode SET [Markup]=REPLACE([Markup],'https://maps.googleapis.com/maps/api/js?libraries=marker&key=','https://maps.googleapis.com/maps/api/js?libraries=marker&loading=async&key=') where [Guid]='FE298210-1307-49DF-B28B-3735A414CCA0'" );

            var oldMarkup = @"google.maps.event.addDomListener(window, ""resize"", function() {".Replace( "'", "''" );
            string newMarkup = @"google.maps.event.addListener(window, ""resize"", function() {".Replace( "'", "''" );
            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Markup" );
            Sql( $@"UPDATE
                [dbo].[LavaShortcode] 
            SET [Markup] = REPLACE({targetColumn}, '{oldMarkup}', '{newMarkup}')
            WHERE 
                {targetColumn} LIKE '%{oldMarkup}%'
                AND [Guid] = 'FE298210-1307-49DF-B28B-3735A414CCA0'" );

            oldMarkup = @"google.maps.event.addDomListener(window, 'load', initialize{{ id }});".Replace( "'", "''" );
            newMarkup = @"window.addEventListener('load', initialize{{ id }});".Replace( "'", "''" );
            Sql( $@"UPDATE
                [dbo].[LavaShortcode] 
            SET [Markup] = REPLACE({targetColumn}, '{oldMarkup}', '{newMarkup}')
            WHERE 
                {targetColumn} LIKE '%{oldMarkup}%'
                AND [Guid] = 'FE298210-1307-49DF-B28B-3735A414CCA0'" );
        }

        #endregion

        #region DH: Update Saved Configuration Defined Type to Saved Kiosk Template

        private void SavedKioskTemplateDefinedTypeUp()
        {
            Sql( "UPDATE [DefinedType] SET [Name] = 'Saved Check-in Kiosk Templates' WHERE [Guid] = 'F986008C-99BB-4C48-8A6E-38C8A121D75B'" );
        }

        private void SavedKioskTemplateDefinedTypeDown()
        {
            Sql( "UPDATE [DefinedType] SET [Name] = 'Saved Check-in Configuration' WHERE [Guid] = 'F986008C-99BB-4C48-8A6E-38C8A121D75B'" );
        }

        #endregion

        #region SK: Update Adaptive Message List Page

        private void UpdateAdaptiveMessageListPageUp()
        {
            Sql( @"
      DECLARE @LayoutId INT = (SELECT Id FROM [Layout] WHERE [Guid]='0CB60906-6B74-44FD-AB25-026050EF70EB')
      UPDATE 
          [Page]
      SET [LayoutId] = @LayoutId
      WHERE [Guid] = '73112D38-E051-4452-AEF9-E473EEDD0BCB'" );

            RockMigrationHelper.AddBlock( true, "73112D38-E051-4452-AEF9-E473EEDD0BCB".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "ADE003C7-649B-466A-872B-B8AC952E7841".AsGuid(), "Category Tree View", "Sidebar1", @"", @"", 0, "9912C605-6699-4484-B88B-469171F2F693" );
            RockMigrationHelper.AddBlock( true, "73112D38-E051-4452-AEF9-E473EEDD0BCB".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "515DC5C2-4FBD-4EEA-9D8E-A807409DEFDE".AsGuid(), "Category Detail", "Main", @"", @"", 0, "F10D62AB-46D0-4C01-9A34-FAE2BE8CFDB6" );
            RockMigrationHelper.AddBlock( true, "73112D38-E051-4452-AEF9-E473EEDD0BCB".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "A81FE4E0-DF9F-4978-83A7-EB5459F37938".AsGuid(), "Adaptive Message Detail", "Main", @"", @"", 2, "859B5FE9-9068-40EC-B7AD-78598BEDC6AA" );
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '859B5FE9-9068-40EC-B7AD-78598BEDC6AA'" );
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '2DBFA85E-BA20-4FF2-8372-80688C8B9CD1'" );
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'F10D62AB-46D0-4C01-9A34-FAE2BE8CFDB6'" );
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '9912C605-6699-4484-B88B-469171F2F693'" );
            RockMigrationHelper.AddBlockAttributeValue( "2DBFA85E-BA20-4FF2-8372-80688C8B9CD1", "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D", @"09053C7C-9374-4489-8A7B-71F02E3E7D89" );
            RockMigrationHelper.AddBlockAttributeValue( "2DBFA85E-BA20-4FF2-8372-80688C8B9CD1", "831403EB-262E-4BC5-8B5E-F16153493BF5", @"8B53F981-6FF6-4657-9CD5-01E36EB0DF51" );
            RockMigrationHelper.AddBlockAttributeValue( "2DBFA85E-BA20-4FF2-8372-80688C8B9CD1", "F9E1AD87-CA8A-49DF-84AC-8F18895FCB87", @"bec30e90-0434-43c4-b839-09e11775e497" );
            RockMigrationHelper.AddBlockAttributeValue( "2DBFA85E-BA20-4FF2-8372-80688C8B9CD1", "0AC7BE6D-22FB-4A4F-9188-C1FAB8AC1EC1", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "9912C605-6699-4484-B88B-469171F2F693", "845AC4E4-ACD1-40CC-96F6-8D22136C30CC", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "9912C605-6699-4484-B88B-469171F2F693", "D2596ADF-4455-42A4-848F-6DFD816C2867", @"fa fa-list-ol" );
            RockMigrationHelper.AddBlockAttributeValue( "9912C605-6699-4484-B88B-469171F2F693", "C48600CD-2C65-46EF-84E8-975F0DE8C28E", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "9912C605-6699-4484-B88B-469171F2F693", "AA057D3E-00CC-42BD-9998-600873356EDB", @"AdaptiveMessageCategoryId" );
            RockMigrationHelper.AddBlockAttributeValue( "9912C605-6699-4484-B88B-469171F2F693", "06D414F0-AA20-4D3C-B297-1530CCD64395", @"d47bda25-03a3-46ee-a0a6-f8b220e39e4a" );
            RockMigrationHelper.AddBlockAttributeValue( "9912C605-6699-4484-B88B-469171F2F693", "AEE521D8-124D-4BB3-8A80-5F368E5CEC15", @"73112d38-e051-4452-aef9-e473eedd0bcb" );
            RockMigrationHelper.AddBlockAttributeValue( "9912C605-6699-4484-B88B-469171F2F693", "AEE521D8-124D-4BB3-8A80-5F368E5CEC15", @"73112d38-e051-4452-aef9-e473eedd0bcb" );
            RockMigrationHelper.AddBlockAttributeValue( "F10D62AB-46D0-4C01-9A34-FAE2BE8CFDB6", "3C6E056B-5087-4E02-B9FD-853B658E3C85", @"d47bda25-03a3-46ee-a0a6-f8b220e39e4a" );
            RockMigrationHelper.AddBlockAttributeValue( "F10D62AB-46D0-4C01-9A34-FAE2BE8CFDB6", "3C6E056B-5087-4E02-B9FD-853B658E3C85", @"d47bda25-03a3-46ee-a0a6-f8b220e39e4a" );
        }

        #endregion
    }
}
