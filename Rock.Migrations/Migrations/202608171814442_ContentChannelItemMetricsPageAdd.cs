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
    /// Adds the Content Item Metrics page (a child of Content Channel Detail), its route, and places
    /// the Content Channel Item Metrics block on it. Registers the block's entity type and block type
    /// first so the block placement resolves during this migration.
    /// </summary>
    public partial class ContentChannelItemMetricsPageAdd : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.ContentChannelItemMetrics
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.ContentChannelItemMetrics", "Content Channel Item Metrics", "Rock.Blocks.Cms.ContentChannelItemMetrics, Rock.Blocks, Version=20.0.6.0, Culture=neutral, PublicKeyToken=null", false, false, "6885E548-DE26-4967-A191-F18BE7313D9F" );

            // Add/Update Obsidian Block Type
            //   Name:Content Channel Item Metrics
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.ContentChannelItemMetrics
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Content Channel Item Metrics", "Displays metrics for a content channel item.", "Rock.Blocks.Cms.ContentChannelItemMetrics", "CMS", "447960A5-276E-4D5A-9AF0-133F90AA43C0" );

            // Add Page
            //  Internal Name: Content Item Metrics
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, Rock.SystemGuid.Page.CONTENT_CHANNEL_DETAIL, Rock.SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Content Item Metrics", "", Rock.SystemGuid.Page.CONTENT_ITEM_METRICS, "" );

            // Add Page Route
            //   Page:Content Item Metrics
            //   Route:admin/cms/content-channels/items/{ContentChannelItemId}/metrics
            RockMigrationHelper.AddOrUpdatePageRoute( Rock.SystemGuid.Page.CONTENT_ITEM_METRICS, "admin/cms/content-channels/items/{ContentChannelItemId}/metrics", "F22F1564-BFD7-40BB-A38D-7A45836E14C3" );

            // Add Block
            //  Block Name: Content Channel Item Metrics
            //  Page Name: Content Item Metrics
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.CONTENT_ITEM_METRICS.AsGuid(), null, Rock.SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), "447960A5-276E-4D5A-9AF0-133F90AA43C0".AsGuid(), "Content Channel Item Metrics", "Main", @"", @"", 0, "A5A37671-5F2D-4D71-9422-FDB713228C55" );

            // Attribute for BlockType
            //   BlockType: Content Channel Item List
            //   Category: CMS
            //   Attribute: Content Channel Item Metrics Page
            // Pin the code-scanned MetricsPage attribute to a known Guid so the block attribute value below
            // can reference it (the [LinkedPage] attribute carries no Guid, so it is machine-generated otherwise).
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B995BE3F-A9EB-4A18-AE24-E93A8796AEDE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Content Channel Item Metrics Page", "MetricsPage", "Content Channel Item Metrics Page", @"The page that links to the metrics for the content channel item.", 1, @"", "3FF8F0A7-AAC1-48CB-B743-E55FF1B976AD" );

            // Point the core Content Channel Item List block (on the Content Channel Detail page) at the new
            // metrics page, so the Metrics column appears there without enabling it on every other CCIL instance.
            RockMigrationHelper.AddBlockAttributeValue( "98B5B613-0DDE-4C74-8B45-F634E7C2B36C", "3FF8F0A7-AAC1-48CB-B743-E55FF1B976AD", Rock.SystemGuid.Page.CONTENT_ITEM_METRICS );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Clear the seeded MetricsPage value on the core Content Channel Item List block. The attribute
            // itself is code-owned (defined by the block's [LinkedPage]), so it is left for the scan to manage.
            RockMigrationHelper.AddBlockAttributeValue( "98B5B613-0DDE-4C74-8B45-F634E7C2B36C", "3FF8F0A7-AAC1-48CB-B743-E55FF1B976AD", "" );

            // Remove Block
            //  Name: Content Channel Item Metrics, from Page: Content Item Metrics, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "A5A37671-5F2D-4D71-9422-FDB713228C55" );

            // Remove Page Route
            //   Route:admin/cms/content-channels/items/{ContentChannelItemId}/metrics
            RockMigrationHelper.DeletePageRoute( "F22F1564-BFD7-40BB-A38D-7A45836E14C3" );

            // Delete Page
            //  Internal Name: Content Item Metrics
            //  Site: Rock RMS
            RockMigrationHelper.DeletePage( Rock.SystemGuid.Page.CONTENT_ITEM_METRICS );

            // Delete BlockType
            //   Name: Content Channel Item Metrics
            //   Category: CMS
            //   EntityType: Rock.Blocks.Cms.ContentChannelItemMetrics
            RockMigrationHelper.DeleteBlockType( "447960A5-276E-4D5A-9AF0-133F90AA43C0" );
        }
    }
}
