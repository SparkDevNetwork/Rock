﻿// <copyright>
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

using System;
using System.Collections.Generic;

using Amazon.Runtime.Internal.Transform;

using Rock.Model;


namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 215, "1.16.4" )]
    public class MigrationRollupsForV17_0_18 : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            MakeMapIdForGoogleMapsShortCodeOptionalUp();
            UpdateDetailsPageBlockAttributeKeyForFundraisingListWebformBLock();
            ChopBlocksUp();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            MapMapIDForGoogleMapsShortCodeOptionalDown();
        }

        #region PA: Register block attributes for chop job in v1.17.0.31

        private void ChopBlocksUp()
        {
            RegisterBlockAttributesForChop();
            ChopBlockTypesv17();
        }

        /// <summary>
        /// PA: Update the Attirbute Key of Block Attribute DetailsPage of Fundraising List Webforms Block.
        /// </summary>
        private void UpdateDetailsPageBlockAttributeKeyForFundraisingListWebformBLock()
        {
            Sql( "UPDATE [Attribute] SET [Key] = 'DetailPage' WHERE [Guid] = 'F17BD62D-8134-47A5-BDBC-F7F6CD07974E' AND [Key] = 'DetailsPage'" );
        }

        private void RegisterBlockAttributesForChop()
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
        private void ChopBlockTypesv17()
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
    }
}
