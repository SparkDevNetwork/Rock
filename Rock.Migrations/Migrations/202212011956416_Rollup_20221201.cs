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
    public partial class Rollup_20221201 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddAppleDeviceModels();
            UpdateAppleModelsOnPersonalDevices();
            Update_spAnalytics_ETL_Family();
            AddInteractiveExperiencePagesUp();
            ChartShortcodeNumberFormattingFix();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddInteractiveExperiencePagesDown();
        }

        /// <summary>
        /// ED: Add Apple Device Models
        /// </summary>
        private void AddAppleDeviceModels()
        {
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,3-A", "iPad Pro 11 inch 4th Gen", "529BDBD1-85FB-4F38-82D7-747B6C2BCD65", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,3-B", "iPad Pro 11 inch 4th Gen", "5837DBC4-EE9B-4B5A-9900-E8F8678609AC", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,4-A", "iPad Pro 11 inch 4th Gen", "04A2EE64-EA46-4F61-8497-4AC0EEE5F6A1", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,4-B", "iPad Pro 11 inch 4th Gen", "B2F49E10-5211-414B-8258-4B1DA0D7BB1A", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,5-A", "iPad Pro 12.9 inch 6th Gen", "7D7D90BE-1456-47F9-BAC9-B22B7FBE8A7C", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,5-B", "iPad Pro 12.9 inch 6th Gen", "285FBF08-A40C-4EF3-8225-24C37E3ED8EA", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,6-A", "iPad Pro 12.9 inch 6th Gen", "F902AB78-90EB-46E8-839D-8082F537AF22", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,6-B", "iPad Pro 12.9 inch 6th Gen", "8417E05B-C79A-4CCE-B781-F38908E509C9", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone14,7", "iPhone 14", "4E261C44-87EE-4439-AA70-DFD29462F0AE", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone14,8", "iPhone 14 Plus", "F91B11C0-6D97-4C9F-AE4A-A1C21FC99F95", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone15,2", "iPhone 14 Pro", "B8E425E3-413B-44E7-80B2-CF39878D31F1", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone15,3", "iPhone 14 Pro Max", "821C355F-B020-4F6E-A932-701DA88C66D8", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPod9,1", "7th Gen iPod", "BA270342-8857-4903-994A-5D2BD2BD107A", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,10", "Apple Watch SE 40mm case (GPS)", "156448B2-3533-44AA-A66D-85A07715E252", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,11", "Apple Watch SE 44mm case (GPS)", "66FBE4DC-C22C-4317-A6CF-7E6488B19AD6", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,12", "Apple Watch SE 40mm case (GPS+Cellular)", "55C68622-C275-4AC4-B532-EF83EE8DE3AA", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,13", "Apple Watch SE 44mm case (GPS+Cellular)", "AF02E15C-2A1E-4F16-8096-BAA9FF6F79E9", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,14", "Apple Watch Series 8 41mm case (GPS)", "383C9A7E-C839-4352-8A3B-10DC1FB432F5", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,15", "Apple Watch Series 8 45mm case (GPS)", "71BE1A30-1603-424B-85BE-A914A85CBD2F", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,16", "Apple Watch Series 8 41mm case (GPS+Cellular)", "F515FFC7-0D57-4300-A8C2-A0E1C4AB9721", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,17", "Apple Watch Series 8 45mm case (GPS+Cellular)", "540640F1-971C-48E8-AFE8-BF465E4CF13F", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,18", "Apple Watch Ultra", "DE66D28A-A9AE-4984-8F91-351A258261D3", true );
        }

        /// <summary>
        /// ED: Update Apple Models on PersonalDevices
        /// </summary>
        private void UpdateAppleModelsOnPersonalDevices()
        {
            Sql( @"--Make sure to run this AFTER the newer models are added to defined types.
                DECLARE @AppleDeviceDefinedTypeId INT = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = 'DAE31F78-7AB9-4ACE-9EE1-C1E6A734562C')

                UPDATE [PersonalDevice] SET [Model] = dv.[Description]
                FROM [PersonalDevice] pd
                JOIN [DefinedValue] dv ON pd.[Model] = dv.[Value]
                WHERE pd.[Manufacturer] = 'Apple'
	                AND pd.[Model] like '%,%'
	                AND dv.[DefinedTypeId] = @AppleDeviceDefinedTypeId" );
        }

        /// <summary>
        /// SK: Updated spAnalytics ETL Family Stored Procedure to update AnalyticsSourceFamilyHistorical if a family CampusId changes from NULL
        /// </summary>
        private void Update_spAnalytics_ETL_Family()
        {
            Sql( MigrationSQL._202212011956416_Rollup_20221201_Update_spAnalytics_ETL_Family_11182022 );
        }

        /// <summary>
        /// DH: Add Pages and Blocks for Interactive Experiences
        /// </summary>
        private void AddInteractiveExperiencePagesUp()
        {
            // Add Page
            //  Internal Name: Interactive Experiences
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "F7105BFE-B28C-41B6-9CE6-F1018D77DD8F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Interactive Experiences", "", "0C1C9100-C2B0-44DA-840D-DDD50B970FE2", "" );
            Sql( "UPDATE [Page] SET [DisplayInNavWhen] = 2 WHERE [Guid] = '0C1C9100-C2B0-44DA-840D-DDD50B970FE2'" );

            // Add Page
            //  Internal Name: Experience Administration
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "0C1C9100-C2B0-44DA-840D-DDD50B970FE2", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Experience Administration", "", "4698F11D-4702-44D6-B58D-DF4A6F79564B", "fa fa-ticket-alt" );

            // Add Page
            //  Internal Name: Experience Manager
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "0C1C9100-C2B0-44DA-840D-DDD50B970FE2", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Experience Manager", "", "1DA3B534-FB71-483B-BD64-9BFB92F59123", "fa fa-chalkboard-teacher" );
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '1DA3B534-FB71-483B-BD64-9BFB92F59123'" );

            // Add Page
            //  Internal Name: Experience Questions
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "0C1C9100-C2B0-44DA-840D-DDD50B970FE2", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Experience Questions", "", "9B7AC621-EF83-4DA8-8132-3BCE25A597E1", "fa fa-question-circle" );
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '9B7AC621-EF83-4DA8-8132-3BCE25A597E1'" );

            // Add Page
            //  Internal Name: Experience Manager
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "1DA3B534-FB71-483B-BD64-9BFB92F59123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Experience Manager", "", "ACE5644C-96F4-41E7-BA12-4A425DB15A6E", "" );

            // Add Page
            //  Internal Name: Interactive Experience Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "4698F11D-4702-44D6-B58D-DF4A6F79564B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Interactive Experience Detail", "", "EA064109-00E9-456C-9792-19DEF14B1140", "" );

            // Add Page
            //  Internal Name: Experience Manager Occurrences
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "4698F11D-4702-44D6-B58D-DF4A6F79564B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Experience Manager Occurrences", "", "F5C03021-66C3-49AF-9FB5-07B8A64885D9", "" );

            // Add Page
            //  Internal Name: Live Experience Preview
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "ACE5644C-96F4-41E7-BA12-4A425DB15A6E", "2E169330-D7D7-4ECA-B417-72C64BE150F0", "Live Experience Preview", "", "523DBC29-BF75-4692-9916-167ED5B385F2", "" );

            // Add Page
            //  Internal Name: Experience Questions
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "9B7AC621-EF83-4DA8-8132-3BCE25A597E1", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Experience Questions", "", "4C1E2C5A-7924-4C19-A00A-D45A1F8E06D5", "" );

            // Add Page
            //  Internal Name: Experience Visualizer
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "0C1C9100-C2B0-44DA-840D-DDD50B970FE2", "2E169330-D7D7-4ECA-B417-72C64BE150F0", "Experience Visualizer", "", "73EDF655-1AB8-4FF8-AB83-B9E9107744CA", "fa fa-chart-area" );
            Sql( "UPDATE [Page] SET [DisplayInNavWhen] = 2 WHERE [Guid] = '73EDF655-1AB8-4FF8-AB83-B9E9107744CA'" );

            // Add/Update BlockType
            //   Name: Interactive Experience List
            //   Category: Event > Interactive Experiences
            //   Path: ~/Blocks/Event/InteractiveExperiences/InteractiveExperienceList.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType( "Interactive Experience List", "List Interactive Experiences", "~/Blocks/Event/InteractiveExperiences/InteractiveExperienceList.ascx", "Event > Interactive Experiences", "BD89FE49-4DD2-4313-AFF8-ABAA97B3235D" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Events.LiveExperience
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Events.LiveExperience", "Live Experience", "Rock.Blocks.Types.Mobile.Events.LiveExperience, Rock, Version=1.14.1.1, Culture=neutral, PublicKeyToken=null", false, false, "FC408E41-872D-4B71-A08C-513D7500E980" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Events.LiveExperienceOccurrences
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Events.LiveExperienceOccurrences", "Live Experience Occurrences", "Rock.Blocks.Types.Mobile.Events.LiveExperienceOccurrences, Rock, Version=1.14.1.1, Culture=neutral, PublicKeyToken=null", false, false, "AF20692A-9AE1-4FAF-A506-D408B14652D1" );

            // Add/Update Mobile Block Type
            //   Name:Live Experience
            //   Category:Mobile > Events
            //   EntityType:Rock.Blocks.Types.Mobile.Events.LiveExperience
            RockMigrationHelper.UpdateMobileBlockType( "Live Experience", "Block that is used to connect to a Live Experience from within your mobile application.", "Rock.Blocks.Types.Mobile.Events.LiveExperience", "Mobile > Events", "969EB376-281C-41D8-B7E9-A183DEA751DB" );

            // Add/Update Mobile Block Type
            //   Name:Live Experience Occurrences
            //   Category:Mobile > Events
            //   EntityType:Rock.Blocks.Types.Mobile.Events.LiveExperienceOccurrences
            RockMigrationHelper.UpdateMobileBlockType( "Live Experience Occurrences", "Displays a lava formatted list of experience occurrences that are currently happening.", "Rock.Blocks.Types.Mobile.Events.LiveExperienceOccurrences", "Mobile > Events", "C45BA1C6-CE7F-4C37-82BF-A86D28BB28FE" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Event.InteractiveExperiences.ExperienceManager
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Event.InteractiveExperiences.ExperienceManager", "Experience Manager", "Rock.Blocks.Event.InteractiveExperiences.ExperienceManager, Rock.Blocks, Version=1.14.1.1, Culture=neutral, PublicKeyToken=null", false, false, "5D2594D9-2695-41BE-880C-966FF25BCF11" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Event.InteractiveExperiences.ExperienceManagerOccurrences
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Event.InteractiveExperiences.ExperienceManagerOccurrences", "Experience Manager Occurrences", "Rock.Blocks.Event.InteractiveExperiences.ExperienceManagerOccurrences, Rock.Blocks, Version=1.14.1.1, Culture=neutral, PublicKeyToken=null", false, false, "08C31C15-7328-4759-B530-49C9D342CDB7" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Event.InteractiveExperiences.ExperienceVisualizer
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Event.InteractiveExperiences.ExperienceVisualizer", "Experience Visualizer", "Rock.Blocks.Event.InteractiveExperiences.ExperienceVisualizer, Rock.Blocks, Version=1.14.1.1, Culture=neutral, PublicKeyToken=null", false, false, "932A5219-0822-4D88-B8A7-E0F5C301348A" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Event.InteractiveExperiences.InteractiveExperienceDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Event.InteractiveExperiences.InteractiveExperienceDetail", "Interactive Experience Detail", "Rock.Blocks.Event.InteractiveExperiences.InteractiveExperienceDetail, Rock.Blocks, Version=1.14.1.1, Culture=neutral, PublicKeyToken=null", false, false, "E9E76F40-3E00-40E1-BD9D-3156E9208557" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Event.InteractiveExperiences.LiveExperience
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Event.InteractiveExperiences.LiveExperience", "Live Experience", "Rock.Blocks.Event.InteractiveExperiences.LiveExperience, Rock.Blocks, Version=1.14.1.1, Culture=neutral, PublicKeyToken=null", false, false, "9A853836-5155-4CE5-817F-49BFCBE7502C" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Event.InteractiveExperiences.LiveExperienceOccurrences
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Event.InteractiveExperiences.LiveExperienceOccurrences", "Live Experience Occurrences", "Rock.Blocks.Event.InteractiveExperiences.LiveExperienceOccurrences, Rock.Blocks, Version=1.14.1.1, Culture=neutral, PublicKeyToken=null", false, false, "B7E9AC68-35DB-4D18-A5DB-A250832E8AD9" );

            // Add/Update Obsidian Block Type
            //   Name:Experience Manager
            //   Category:Event > Interactive Experiences
            //   EntityType:Rock.Blocks.Event.InteractiveExperiences.ExperienceManager
            RockMigrationHelper.UpdateMobileBlockType( "Experience Manager", "Manages an active interactive experience.", "Rock.Blocks.Event.InteractiveExperiences.ExperienceManager", "Event > Interactive Experiences", "7AF57181-DD9A-446A-B321-ABAD900DF9BC" );

            // Add/Update Obsidian Block Type
            //   Name:Experience Manager Occurrences
            //   Category:Event > Interactive Experiences
            //   EntityType:Rock.Blocks.Event.InteractiveExperiences.ExperienceManagerOccurrences
            RockMigrationHelper.UpdateMobileBlockType( "Experience Manager Occurrences", "Displays a list of interactive experience occurrences for the individual to pick from.", "Rock.Blocks.Event.InteractiveExperiences.ExperienceManagerOccurrences", "Event > Interactive Experiences", "B8BE65EC-04CC-4423-944E-B6B30F6EB38C" );

            // Add/Update Obsidian Block Type
            //   Name:Experience Visualizer
            //   Category:Event > Interactive Experiences
            //   EntityType:Rock.Blocks.Event.InteractiveExperiences.ExperienceVisualizer
            RockMigrationHelper.UpdateMobileBlockType( "Experience Visualizer", "Displays the visuals of an experience.", "Rock.Blocks.Event.InteractiveExperiences.ExperienceVisualizer", "Event > Interactive Experiences", "B98ABF9B-9345-48C6-A15D-4DD5AC73F0BC" );

            // Add/Update Obsidian Block Type
            //   Name:Interactive Experience Detail
            //   Category:Event > Interactive Experiences
            //   EntityType:Rock.Blocks.Event.InteractiveExperiences.InteractiveExperienceDetail
            RockMigrationHelper.UpdateMobileBlockType( "Interactive Experience Detail", "Displays the details of a particular interactive experience.", "Rock.Blocks.Event.InteractiveExperiences.InteractiveExperienceDetail", "Event > Interactive Experiences", "DC997692-3BB4-470C-A2EE-83CB87D246B1" );

            // Add/Update Obsidian Block Type
            //   Name:Live Experience
            //   Category:Event > Interactive Experiences
            //   EntityType:Rock.Blocks.Event.InteractiveExperiences.LiveExperience
            RockMigrationHelper.UpdateMobileBlockType( "Live Experience", "Displays a live interactive experience", "Rock.Blocks.Event.InteractiveExperiences.LiveExperience", "Event > Interactive Experiences", "BA26F4FC-F6DB-462E-9697-BD6A0504A0A8" );

            // Add/Update Obsidian Block Type
            //   Name:Live Experience Occurrences
            //   Category:Event > Interactive Experiences
            //   EntityType:Rock.Blocks.Event.InteractiveExperiences.LiveExperienceOccurrences
            RockMigrationHelper.UpdateMobileBlockType( "Live Experience Occurrences", "Displays a list of interactive experience occurrences for the individual to pick from.", "Rock.Blocks.Event.InteractiveExperiences.LiveExperienceOccurrences", "Event > Interactive Experiences", "8B384269-3D54-4C84-B230-2061BE4866F9" );

            // Add Block
            //  Block Name: Page Menu
            //  Page Name: Interactive Experiences
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0C1C9100-C2B0-44DA-840D-DDD50B970FE2".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Page Menu", "Main", @"", @"", 0, "F49005D1-4B1E-4BDC-B849-B075292F2802" );

            // Add Block
            //  Block Name: Interactive Experience List
            //  Page Name: Experience Administration
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "4698F11D-4702-44D6-B58D-DF4A6F79564B".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "BD89FE49-4DD2-4313-AFF8-ABAA97B3235D".AsGuid(), "Interactive Experience List", "Main", @"", @"", 0, "3EB67362-CB49-4A77-ACF7-668855DF6D01" );

            // Add Block
            //  Block Name: Experience Manager
            //  Page Name: Experience Manager
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "ACE5644C-96F4-41E7-BA12-4A425DB15A6E".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "7AF57181-DD9A-446A-B321-ABAD900DF9BC".AsGuid(), "Experience Manager", "Main", @"", @"", 0, "15B14273-3BEE-467B-8DCC-73C245136E17" );

            // Add Block
            //  Block Name: Experience Manager Occurrences
            //  Page Name: Experience Questions
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "9B7AC621-EF83-4DA8-8132-3BCE25A597E1".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "B8BE65EC-04CC-4423-944E-B6B30F6EB38C".AsGuid(), "Experience Manager Occurrences", "Main", @"", @"", 0, "33B1F9F4-F0AF-4E55-9B46-6F96497AA58F" );

            // Add Block
            //  Block Name: Interactive Experience Detail
            //  Page Name: Interactive Experience Detail
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "EA064109-00E9-456C-9792-19DEF14B1140".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "DC997692-3BB4-470C-A2EE-83CB87D246B1".AsGuid(), "Interactive Experience Detail", "Main", @"", @"", 0, "29437F08-FF8F-46E8-AFB2-F0FFE5C4122E" );

            // Add Block
            //  Block Name: Experience Manager Occurrences
            //  Page Name: Experience Manager Occurrences
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "F5C03021-66C3-49AF-9FB5-07B8A64885D9".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "B8BE65EC-04CC-4423-944E-B6B30F6EB38C".AsGuid(), "Experience Manager Occurrences", "Main", @"", @"", 0, "671CBC25-B9A7-416A-B5C9-9938F56CA57E" );

            // Add Block
            //  Block Name: Live Experience
            //  Page Name: Live Experience Preview
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "523DBC29-BF75-4692-9916-167ED5B385F2".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "BA26F4FC-F6DB-462E-9697-BD6A0504A0A8".AsGuid(), "Live Experience", "Main", @"", @"", 0, "CB0C94AD-0007-4A93-BF21-1B176C58DF48" );

            // Add Block
            //  Block Name: Experience Questions
            //  Page Name: Experience Questions
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "4C1E2C5A-7924-4C19-A00A-D45A1F8E06D5".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "7AF57181-DD9A-446A-B321-ABAD900DF9BC".AsGuid(), "Experience Questions", "Main", @"", @"", 0, "D40112EF-8E05-4476-95FF-690B640C10DC" );

            // Add Block
            //  Block Name: Experience Manager Occurrences
            //  Page Name: Experience Manager
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "1DA3B534-FB71-483B-BD64-9BFB92F59123".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "B8BE65EC-04CC-4423-944E-B6B30F6EB38C".AsGuid(), "Experience Manager Occurrences", "Main", @"", @"", 0, "6736974E-BF70-4582-ADC0-E439A4991D49" );

            // Add Block
            //  Block Name: Experience Visualizer
            //  Page Name: Experience Visualizer
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "73EDF655-1AB8-4FF8-AB83-B9E9107744CA".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "B98ABF9B-9345-48C6-A15D-4DD5AC73F0BC".AsGuid(), "Experience Visualizer", "Main", @"", @"", 0, "42952868-35A0-478C-8792-E42CB4A26C4A" );

            // Attribute for BlockType
            //   BlockType: Experience Manager
            //   Category: Event > Interactive Experiences
            //   Attribute: Live Experience Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7AF57181-DD9A-446A-B321-ABAD900DF9BC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Live Experience Page", "LiveExperiencePage", "Live Experience Page", @"The page that will provide the live experience preview.", 0, @"", "543BEC53-2A4C-4806-8CFD-2181EBB3B1CD" );

            // Attribute for BlockType
            //   BlockType: Experience Manager
            //   Category: Event > Interactive Experiences
            //   Attribute: Participant Count Update Interval
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7AF57181-DD9A-446A-B321-ABAD900DF9BC", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Participant Count Update Interval", "ParticipantCountUpdateInterval", "Participant Count Update Interval", @"The number of seconds between updates to the participant count. Setting this value too low can cause extra load on the server.", 1, @"30", "E2B7175A-136B-4D12-87E6-B17D570620B9" );

            // Attribute for BlockType
            //   BlockType: Experience Manager
            //   Category: Event > Interactive Experiences
            //   Attribute: Tabs to Display
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7AF57181-DD9A-446A-B321-ABAD900DF9BC", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Tabs to Display", "TabsToDisplay", "Tabs to Display", @"The tabs to be made visible to people managing the experience.", 2, @"Live Event,Moderation,Live Questions", "25828C97-6F86-4A36-8EFB-EC88FA850ED3" );

            // Attribute for BlockType
            //   BlockType: Experience Manager Occurrences
            //   Category: Event > Interactive Experiences
            //   Attribute: Experience Manager Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B8BE65EC-04CC-4423-944E-B6B30F6EB38C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Experience Manager Page", "ExperienceManagerPage", "Experience Manager Page", @"", 0, @"", "D26373FB-C24B-4C73-B401-D6E215D1CD89" );

            // Attribute for BlockType
            //   BlockType: Experience Visualizer
            //   Category: Event > Interactive Experiences
            //   Attribute: Keep Alive Interval
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B98ABF9B-9345-48C6-A15D-4DD5AC73F0BC", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Keep Alive Interval", "KeepAliveInterval", "Keep Alive Interval", @"How often in seconds the browser will inform the server it is still here.", 0, @"30", "F11E007F-9526-48BF-959B-6B9FA3E226F5" );

            // Attribute for BlockType
            //   BlockType: Experience Visualizer
            //   Category: Event > Interactive Experiences
            //   Attribute: Interactive Experience
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B98ABF9B-9345-48C6-A15D-4DD5AC73F0BC", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Interactive Experience", "InteractiveExperience", "Interactive Experience", @"The interactive experience to use when determining the active occurrence.", 1, @"", "1D4B7246-56C8-4EDE-9228-1E3D052BECA5" );

            // Attribute for BlockType
            //   BlockType: Experience Visualizer
            //   Category: Event > Interactive Experiences
            //   Attribute: Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B98ABF9B-9345-48C6-A15D-4DD5AC73F0BC", "1B71FEF4-201F-4D53-8C60-2DF21F1985ED", "Campus", "Campus", "Campus", @"The campus to use when determining which experience occurrence to display. If no campus is selected then only occurrences with no campus will be considered.", 2, @"", "8F9CA1EC-BDCF-4EC5-AA8F-B5257B457A2B" );

            // Attribute for BlockType
            //   BlockType: Live Experience
            //   Category: Event > Interactive Experiences
            //   Attribute: Keep Alive Interval
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BA26F4FC-F6DB-462E-9697-BD6A0504A0A8", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Keep Alive Interval", "KeepAliveInterval", "Keep Alive Interval", @"How often in seconds the browser will inform the server it is still here.", 0, @"30", "E7DE4A2B-1CAC-4B80-929C-D70BA7135022" );

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Event > Interactive Experiences
            //   Attribute: Destination Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8B384269-3D54-4C84-B230-2061BE4866F9", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Destination Page", "DestinationPage", "Destination Page", @"The page to link to when selecting an occurrence.", 0, @"", "BC0D6891-9012-481D-B420-DED23680DE03" );

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Event > Interactive Experiences
            //   Attribute: Login Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8B384269-3D54-4C84-B230-2061BE4866F9", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Login Page", "LoginPage", "Login Page", @"The page to use when showing the login page. If not set then the default site login page will be used instead.", 1, @"", "D57E0B0A-0466-4FD2-874C-FC56CD962104" );

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Event > Interactive Experiences
            //   Attribute: Show All
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8B384269-3D54-4C84-B230-2061BE4866F9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show All", "ShowAll", "Show All", @"When enabled, normal filtering is not performed and all active occurrences will be shown. Intended for use on admin pages.", 2, @"False", "EE7A5A06-B00E-4DC2-9A4B-BA38DE7B058A" );

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Event > Interactive Experiences
            //   Attribute: Always Request Location
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8B384269-3D54-4C84-B230-2061BE4866F9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Always Request Location", "AlwaysRequestLocation", "Always Request Location", @"When enabled, the device location will always be requested. Otherwise it will only be used if it has already been requested in the past.", 3, @"False", "8E01BD5C-80FC-4753-9BFC-216CB51D0DB6" );

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Event > Interactive Experiences
            //   Attribute: Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8B384269-3D54-4C84-B230-2061BE4866F9", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Template", "Template", "Template", @"The lava template to use when rendering the occurrences on the page.", 4, @"{% if Occurrences == empty %}
    <div class=""alert alert-info"">
        There are not any live experiences in progress.
    </div>
{% endif %}

{% for occurrence in Occurrences %}
    {% if occurrence.Campus != null %}
        {% capture occurrenceName %}{{ occurrence.InteractiveExperienceSchedule.InteractiveExperience.Name }} at {{ occurrence.Campus.Name }}{% endcapture %}
    {% else %}
        {% capture occurrenceName %}{{ occurrence.InteractiveExperienceSchedule.InteractiveExperience.Name }}{% endcapture %}
    {% endif %}

    <a class=""d-flex rounded overflow-hidden mb-2 align-items-stretch border border-gray-400 bg-white"" href=""{{ occurrence.PageUrl | Escape }}"">
        <div class=""p-2 d-flex align-items-center align-self-stretch bg-info text-white"">
            <span>
                <i class=""fa fa-calendar-alt""></i>
            </span>
        </div>

        <div class=""p-2 d-flex align-items-center align-self-stretch flex-grow-1 text-body"">
            {{ occurrenceName | Escape }}
        </div>

        <div class=""p-2 mr-2 d-flex align-items-center align-self-stretch text-info"">
            <span>
                <i class=""fa fa-arrow-circle-right""></i>
            </span>
        </div>
    </a>
{% endfor %}

{% if LoginRecommended == true %}
    <div class=""alert alert-info"">
        There may be more experiences available to you if you <a href=""{{ LoginUrl | Escape }}"">login</a>.
    </div>
{% endif %}

{% if GeoLocationRecommended == true %}
    <div class=""alert alert-info"">
        There may be more experiences available to you if you <a href=""{{ ProvideLocationUrl | Escape }}"">provide your location</a>.
    </div>
{% endif %}", "3D4A29D1-361D-4EE2-BDE2-32DD36C6B918" );

            // Attribute for BlockType
            //   BlockType: Live Experience
            //   Category: Mobile > Events
            //   Attribute: Live Experience Web Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "969EB376-281C-41D8-B7E9-A183DEA751DB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Live Experience Web Page", "LiveExperienceWebPage", "Live Experience Web Page", @"The page to link the live experience to. This page should contain a Live Experience block.", 0, @"", "5598D31F-AAF4-46EC-BB01-BC97E4E8522D" );

            // Attribute for BlockType
            //   BlockType: Live Experience
            //   Category: Mobile > Events
            //   Attribute: Always Request Location
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "969EB376-281C-41D8-B7E9-A183DEA751DB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Always Request Location", "AlwaysRequestLocation", "Always Request Location", @"Location data will always be included if available. If not available and this is enabled then access to the device location will be requested.", 1, @"False", "BF04C4DB-2AC9-4CFC-BE58-DB27DD03F364" );

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Mobile > Events
            //   Attribute: Destination Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C45BA1C6-CE7F-4C37-82BF-A86D28BB28FE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Destination Page", "DestinationPage", "Destination Page", @"The page to link to when selecting an occurrence.", 0, @"", "168D26F4-B7DF-40A1-912C-618CAB56BC1D" );

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Mobile > Events
            //   Attribute: Login Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C45BA1C6-CE7F-4C37-82BF-A86D28BB28FE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Login Page", "LoginPage", "Login Page", @"The page to use when showing the login page. If not set then the default application login page will be used instead.", 1, @"", "D54810AD-37F0-4302-84F2-018C9E41C8BD" );

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Mobile > Events
            //   Attribute: Show All
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C45BA1C6-CE7F-4C37-82BF-A86D28BB28FE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show All", "ShowAll", "Show All", @"When enabled, normal filtering is not performed and all active occurrences will be shown. Intended for use on admin pages.", 2, @"False", "4E313C27-D80D-496F-A257-46EF4C9B8078" );

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Mobile > Events
            //   Attribute: Always Request Location
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C45BA1C6-CE7F-4C37-82BF-A86D28BB28FE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Always Request Location", "AlwaysRequestLocation", "Always Request Location", @"When enabled, the device location will always be requested. Otherwise it will only be used if it has already been requested in the past.", 3, @"False", "0D215195-04C5-40B2-8B17-EBBA28688B9D" );

            // Attribute for BlockType
            //   BlockType: Interactive Experience List
            //   Category: Event > Interactive Experiences
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BD89FE49-4DD2-4313-AFF8-ABAA97B3235D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "36CCFBDD-610D-47CF-8C03-9760135D1A89" );

            // Attribute for BlockType
            //   BlockType: Interactive Experience List
            //   Category: Event > Interactive Experiences
            //   Attribute: Occurrence Chooser Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BD89FE49-4DD2-4313-AFF8-ABAA97B3235D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Occurrence Chooser Page", "OccurrenceChooserPage", "Occurrence Chooser Page", @"", 1, @"", "9CCBDC33-F2A4-4A55-83F5-E2E8209E7105" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Interactive Experiences, Site=Rock RMS
            //   Attribute: Is Secondary Block
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "F49005D1-4B1E-4BDC-B849-B075292F2802", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Interactive Experiences, Site=Rock RMS
            //   Attribute: Include Current Parameters
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "F49005D1-4B1E-4BDC-B849-B075292F2802", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Interactive Experiences, Site=Rock RMS
            //   Attribute: Template
            /*   Attribute Value: {% include '~~/Assets/Lava/PageListAsBlocks.lava' %} */
            RockMigrationHelper.AddBlockAttributeValue( "F49005D1-4B1E-4BDC-B849-B075292F2802", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Interactive Experiences, Site=Rock RMS
            //   Attribute: Number of Levels
            /*   Attribute Value: 1 */
            RockMigrationHelper.AddBlockAttributeValue( "F49005D1-4B1E-4BDC-B849-B075292F2802", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Interactive Experiences, Site=Rock RMS
            //   Attribute: Include Current QueryString
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "F49005D1-4B1E-4BDC-B849-B075292F2802", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Add Block Attribute Value
            //   Block: Interactive Experience List
            //   BlockType: Interactive Experience List
            //   Category: Event > Interactive Experiences
            //   Block Location: Page=Experience Administration, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: ea064109-00e9-456c-9792-19def14b1140 */
            RockMigrationHelper.AddBlockAttributeValue( "3EB67362-CB49-4A77-ACF7-668855DF6D01", "36CCFBDD-610D-47CF-8C03-9760135D1A89", @"ea064109-00e9-456c-9792-19def14b1140" );

            // Add Block Attribute Value
            //   Block: Interactive Experience List
            //   BlockType: Interactive Experience List
            //   Category: Event > Interactive Experiences
            //   Block Location: Page=Experience Administration, Site=Rock RMS
            //   Attribute: Occurrence Chooser Page
            /*   Attribute Value: f5c03021-66c3-49af-9fb5-07b8a64885d9 */
            RockMigrationHelper.AddBlockAttributeValue( "3EB67362-CB49-4A77-ACF7-668855DF6D01", "9CCBDC33-F2A4-4A55-83F5-E2E8209E7105", @"f5c03021-66c3-49af-9fb5-07b8a64885d9" );

            // Add Block Attribute Value
            //   Block: Experience Manager Occurrences
            //   BlockType: Experience Manager Occurrences
            //   Category: Event > Interactive Experiences
            //   Block Location: Page=Experience Manager Occurrences, Site=Rock RMS
            //   Attribute: Experience Manager Page
            /*   Attribute Value: ace5644c-96f4-41e7-ba12-4a425db15a6e */
            RockMigrationHelper.AddBlockAttributeValue( "671CBC25-B9A7-416A-B5C9-9938F56CA57E", "D26373FB-C24B-4C73-B401-D6E215D1CD89", @"ace5644c-96f4-41e7-ba12-4a425db15a6e" );

            // Add Block Attribute Value
            //   Block: Experience Manager
            //   BlockType: Experience Manager
            //   Category: Event > Interactive Experiences
            //   Block Location: Page=Experience Manager, Site=Rock RMS
            //   Attribute: Tabs to Display
            /*   Attribute Value: Live Event,Moderation,Live Questions */
            RockMigrationHelper.AddBlockAttributeValue( "15B14273-3BEE-467B-8DCC-73C245136E17", "25828C97-6F86-4A36-8EFB-EC88FA850ED3", @"Live Event,Moderation,Live Questions" );

            // Add Block Attribute Value
            //   Block: Experience Manager
            //   BlockType: Experience Manager
            //   Category: Event > Interactive Experiences
            //   Block Location: Page=Experience Manager, Site=Rock RMS
            //   Attribute: Live Experience Page
            /*   Attribute Value: 523dbc29-bf75-4692-9916-167ed5b385f2 */
            RockMigrationHelper.AddBlockAttributeValue( "15B14273-3BEE-467B-8DCC-73C245136E17", "543BEC53-2A4C-4806-8CFD-2181EBB3B1CD", @"523dbc29-bf75-4692-9916-167ed5b385f2" );

            // Add Block Attribute Value
            //   Block: Experience Manager
            //   BlockType: Experience Manager
            //   Category: Event > Interactive Experiences
            //   Block Location: Page=Experience Manager, Site=Rock RMS
            //   Attribute: Participant Count Update Interval
            /*   Attribute Value: 30 */
            RockMigrationHelper.AddBlockAttributeValue( "15B14273-3BEE-467B-8DCC-73C245136E17", "E2B7175A-136B-4D12-87E6-B17D570620B9", @"30" );

            // Add Block Attribute Value
            //   Block: Experience Questions
            //   BlockType: Experience Manager
            //   Category: Event > Interactive Experiences
            //   Block Location: Page=Experience Questions, Site=Rock RMS
            //   Attribute: Participant Count Update Interval
            /*   Attribute Value: 30 */
            RockMigrationHelper.AddBlockAttributeValue( "D40112EF-8E05-4476-95FF-690B640C10DC", "E2B7175A-136B-4D12-87E6-B17D570620B9", @"30" );

            // Add Block Attribute Value
            //   Block: Experience Questions
            //   BlockType: Experience Manager
            //   Category: Event > Interactive Experiences
            //   Block Location: Page=Experience Questions, Site=Rock RMS
            //   Attribute: Tabs to Display
            /*   Attribute Value: Live Questions */
            RockMigrationHelper.AddBlockAttributeValue( "D40112EF-8E05-4476-95FF-690B640C10DC", "25828C97-6F86-4A36-8EFB-EC88FA850ED3", @"Live Questions" );

            // Add Block Attribute Value
            //   Block: Experience Manager Occurrences
            //   BlockType: Experience Manager Occurrences
            //   Category: Event > Interactive Experiences
            //   Block Location: Page=Experience Manager, Site=Rock RMS
            //   Attribute: Experience Manager Page
            /*   Attribute Value: ace5644c-96f4-41e7-ba12-4a425db15a6e */
            RockMigrationHelper.AddBlockAttributeValue( "6736974E-BF70-4582-ADC0-E439A4991D49", "D26373FB-C24B-4C73-B401-D6E215D1CD89", @"ace5644c-96f4-41e7-ba12-4a425db15a6e" );

            // Add Block Attribute Value
            //   Block: Experience Manager Occurrences
            //   BlockType: Experience Manager Occurrences
            //   Category: Event > Interactive Experiences
            //   Block Location: Page=Experience Questions, Site=Rock RMS
            //   Attribute: Experience Manager Page
            /*   Attribute Value: 4c1e2c5a-7924-4c19-a00a-d45a1f8e06d5 */
            RockMigrationHelper.AddBlockAttributeValue( "33B1F9F4-F0AF-4E55-9B46-6F96497AA58F", "D26373FB-C24B-4C73-B401-D6E215D1CD89", @"4c1e2c5a-7924-4c19-a00a-d45a1f8e06d5" );
        }

        /// <summary>
        /// DH: Add Pages and Blocks for Interactive Experiences
        /// </summary>
        private void AddInteractiveExperiencePagesDown()
        {
            // Attribute for BlockType
            //   BlockType: Interactive Experience List
            //   Category: Event > Interactive Experiences
            //   Attribute: Occurrence Chooser Page
            RockMigrationHelper.DeleteAttribute( "9CCBDC33-F2A4-4A55-83F5-E2E8209E7105" );

            // Attribute for BlockType
            //   BlockType: Interactive Experience List
            //   Category: Event > Interactive Experiences
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "36CCFBDD-610D-47CF-8C03-9760135D1A89" );

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Mobile > Events
            //   Attribute: Always Request Location
            RockMigrationHelper.DeleteAttribute( "0D215195-04C5-40B2-8B17-EBBA28688B9D" );

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Mobile > Events
            //   Attribute: Show All
            RockMigrationHelper.DeleteAttribute( "4E313C27-D80D-496F-A257-46EF4C9B8078" );

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Mobile > Events
            //   Attribute: Login Page
            RockMigrationHelper.DeleteAttribute( "D54810AD-37F0-4302-84F2-018C9E41C8BD" );

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Mobile > Events
            //   Attribute: Destination Page
            RockMigrationHelper.DeleteAttribute( "168D26F4-B7DF-40A1-912C-618CAB56BC1D" );

            // Attribute for BlockType
            //   BlockType: Live Experience
            //   Category: Mobile > Events
            //   Attribute: Always Request Location
            RockMigrationHelper.DeleteAttribute( "BF04C4DB-2AC9-4CFC-BE58-DB27DD03F364" );

            // Attribute for BlockType
            //   BlockType: Live Experience
            //   Category: Mobile > Events
            //   Attribute: Live Experience Web Page
            RockMigrationHelper.DeleteAttribute( "5598D31F-AAF4-46EC-BB01-BC97E4E8522D" );

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Event > Interactive Experiences
            //   Attribute: Template
            RockMigrationHelper.DeleteAttribute( "3D4A29D1-361D-4EE2-BDE2-32DD36C6B918" );

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Event > Interactive Experiences
            //   Attribute: Always Request Location
            RockMigrationHelper.DeleteAttribute( "8E01BD5C-80FC-4753-9BFC-216CB51D0DB6" );

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Event > Interactive Experiences
            //   Attribute: Show All
            RockMigrationHelper.DeleteAttribute( "EE7A5A06-B00E-4DC2-9A4B-BA38DE7B058A" );

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Event > Interactive Experiences
            //   Attribute: Login Page
            RockMigrationHelper.DeleteAttribute( "D57E0B0A-0466-4FD2-874C-FC56CD962104" );

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Event > Interactive Experiences
            //   Attribute: Destination Page
            RockMigrationHelper.DeleteAttribute( "BC0D6891-9012-481D-B420-DED23680DE03" );

            // Attribute for BlockType
            //   BlockType: Live Experience
            //   Category: Event > Interactive Experiences
            //   Attribute: Keep Alive Interval
            RockMigrationHelper.DeleteAttribute( "E7DE4A2B-1CAC-4B80-929C-D70BA7135022" );

            // Attribute for BlockType
            //   BlockType: Experience Visualizer
            //   Category: Event > Interactive Experiences
            //   Attribute: Campus
            RockMigrationHelper.DeleteAttribute( "8F9CA1EC-BDCF-4EC5-AA8F-B5257B457A2B" );

            // Attribute for BlockType
            //   BlockType: Experience Visualizer
            //   Category: Event > Interactive Experiences
            //   Attribute: Interactive Experience
            RockMigrationHelper.DeleteAttribute( "1D4B7246-56C8-4EDE-9228-1E3D052BECA5" );

            // Attribute for BlockType
            //   BlockType: Experience Visualizer
            //   Category: Event > Interactive Experiences
            //   Attribute: Keep Alive Interval
            RockMigrationHelper.DeleteAttribute( "F11E007F-9526-48BF-959B-6B9FA3E226F5" );

            // Attribute for BlockType
            //   BlockType: Experience Manager Occurrences
            //   Category: Event > Interactive Experiences
            //   Attribute: Experience Manager Page
            RockMigrationHelper.DeleteAttribute( "D26373FB-C24B-4C73-B401-D6E215D1CD89" );

            // Attribute for BlockType
            //   BlockType: Experience Manager
            //   Category: Event > Interactive Experiences
            //   Attribute: Tabs to Display
            RockMigrationHelper.DeleteAttribute( "25828C97-6F86-4A36-8EFB-EC88FA850ED3" );

            // Attribute for BlockType
            //   BlockType: Experience Manager
            //   Category: Event > Interactive Experiences
            //   Attribute: Participant Count Update Interval
            RockMigrationHelper.DeleteAttribute( "E2B7175A-136B-4D12-87E6-B17D570620B9" );

            // Attribute for BlockType
            //   BlockType: Experience Manager
            //   Category: Event > Interactive Experiences
            //   Attribute: Live Experience Page
            RockMigrationHelper.DeleteAttribute( "543BEC53-2A4C-4806-8CFD-2181EBB3B1CD" );

            // Remove Block
            //  Name: Experience Visualizer, from Page: Experience Visualizer, Site: Rock RMS
            //  from Page: Experience Visualizer, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "42952868-35A0-478C-8792-E42CB4A26C4A" );

            // Remove Block
            //  Name: Experience Manager Occurrences, from Page: Experience Questions, Site: Rock RMS
            //  from Page: Experience Questions, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "33B1F9F4-F0AF-4E55-9B46-6F96497AA58F" );

            // Remove Block
            //  Name: Experience Manager Occurrences, from Page: Experience Manager, Site: Rock RMS
            //  from Page: Experience Manager, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "6736974E-BF70-4582-ADC0-E439A4991D49" );

            // Remove Block
            //  Name: Experience Questions, from Page: Experience Questions, Site: Rock RMS
            //  from Page: Experience Questions, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "D40112EF-8E05-4476-95FF-690B640C10DC" );

            // Remove Block
            //  Name: Live Experience, from Page: Live Experience Preview, Site: Rock RMS
            //  from Page: Live Experience Preview, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "CB0C94AD-0007-4A93-BF21-1B176C58DF48" );

            // Remove Block
            //  Name: Experience Manager, from Page: Experience Manager, Site: Rock RMS
            //  from Page: Experience Manager, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "15B14273-3BEE-467B-8DCC-73C245136E17" );

            // Remove Block
            //  Name: Experience Manager Occurrences, from Page: Experience Manager Occurrences, Site: Rock RMS
            //  from Page: Experience Manager Occurrences, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "671CBC25-B9A7-416A-B5C9-9938F56CA57E" );

            // Remove Block
            //  Name: Interactive Experience Detail, from Page: Interactive Experience Detail, Site: Rock RMS
            //  from Page: Interactive Experience Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "29437F08-FF8F-46E8-AFB2-F0FFE5C4122E" );

            // Remove Block
            //  Name: Interactive Experience List, from Page: Experience Administration, Site: Rock RMS
            //  from Page: Experience Administration, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "3EB67362-CB49-4A77-ACF7-668855DF6D01" );

            // Remove Block
            //  Name: Page Menu, from Page: Interactive Experiences, Site: Rock RMS
            //  from Page: Interactive Experiences, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F49005D1-4B1E-4BDC-B849-B075292F2802" );

            // Delete BlockType
            //   Name: Interactive Experience List
            //   Category: Event > Interactive Experiences
            //   Path: ~/Blocks/Event/InteractiveExperiences/InteractiveExperienceList.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType( "BD89FE49-4DD2-4313-AFF8-ABAA97B3235D" );

            // Delete BlockType
            //   Name: Live Experience Occurrences
            //   Category: Mobile > Events
            //   Path: -
            //   EntityType: Live Experience Occurrences
            RockMigrationHelper.DeleteBlockType( "C45BA1C6-CE7F-4C37-82BF-A86D28BB28FE" );

            // Delete BlockType
            //   Name: Live Experience
            //   Category: Mobile > Events
            //   Path: -
            //   EntityType: Live Experience
            RockMigrationHelper.DeleteBlockType( "969EB376-281C-41D8-B7E9-A183DEA751DB" );

            // Delete BlockType
            //   Name: Live Experience Occurrences
            //   Category: Event > Interactive Experiences
            //   Path: -
            //   EntityType: Live Experience Occurrences
            RockMigrationHelper.DeleteBlockType( "8B384269-3D54-4C84-B230-2061BE4866F9" );

            // Delete BlockType
            //   Name: Live Experience
            //   Category: Event > Interactive Experiences
            //   Path: -
            //   EntityType: Live Experience
            RockMigrationHelper.DeleteBlockType( "BA26F4FC-F6DB-462E-9697-BD6A0504A0A8" );

            // Delete BlockType
            //   Name: Interactive Experience Detail
            //   Category: Event > Interactive Experiences
            //   Path: -
            //   EntityType: Interactive Experience Detail
            RockMigrationHelper.DeleteBlockType( "DC997692-3BB4-470C-A2EE-83CB87D246B1" );

            // Delete BlockType
            //   Name: Experience Visualizer
            //   Category: Event > Interactive Experiences
            //   Path: -
            //   EntityType: Experience Visualizer
            RockMigrationHelper.DeleteBlockType( "B98ABF9B-9345-48C6-A15D-4DD5AC73F0BC" );

            // Delete BlockType
            //   Name: Experience Manager Occurrences
            //   Category: Event > Interactive Experiences
            //   Path: -
            //   EntityType: Experience Manager Occurrences
            RockMigrationHelper.DeleteBlockType( "B8BE65EC-04CC-4423-944E-B6B30F6EB38C" );

            // Delete BlockType
            //   Name: Experience Manager
            //   Category: Event > Interactive Experiences
            //   Path: -
            //   EntityType: Experience Manager
            RockMigrationHelper.DeleteBlockType( "7AF57181-DD9A-446A-B321-ABAD900DF9BC" );

            // Delete Page
            //  Internal Name: Experience Visualizer
            //  Site: Rock RMS
            //  Layout: Blank
            RockMigrationHelper.DeletePage( "73EDF655-1AB8-4FF8-AB83-B9E9107744CA" );


            // Delete Page
            //  Internal Name: Experience Manager
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "1DA3B534-FB71-483B-BD64-9BFB92F59123" );


            // Delete Page
            //  Internal Name: Experience Questions
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "4C1E2C5A-7924-4C19-A00A-D45A1F8E06D5" );


            // Delete Page
            //  Internal Name: Live Experience Preview
            //  Site: Rock RMS
            //  Layout: Blank
            RockMigrationHelper.DeletePage( "523DBC29-BF75-4692-9916-167ED5B385F2" );


            // Delete Page
            //  Internal Name: Experience Manager Occurrences
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "F5C03021-66C3-49AF-9FB5-07B8A64885D9" );


            // Delete Page
            //  Internal Name: Interactive Experience Detail
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "EA064109-00E9-456C-9792-19DEF14B1140" );


            // Delete Page
            //  Internal Name: Experience Questions
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "9B7AC621-EF83-4DA8-8132-3BCE25A597E1" );


            // Delete Page
            //  Internal Name: Experience Manager
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "ACE5644C-96F4-41E7-BA12-4A425DB15A6E" );


            // Delete Page
            //  Internal Name: Experience Administration
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "4698F11D-4702-44D6-B58D-DF4A6F79564B" );


            // Delete Page
            //  Internal Name: Interactive Experiences
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "0C1C9100-C2B0-44DA-840D-DDD50B970FE2" );
        }

        /// <summary>
        /// GJ: Chart Shortcode number formatting fix
        /// </summary>
        private void ChartShortcodeNumberFormattingFix()
        {
            Sql( MigrationSQL._202212011956416_Rollup_20221201_chartupdate );
        }
    }
}
