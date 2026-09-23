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
    /// Adds the Page Builder layout, block, the internal page that hosts it, and the sample page it edits.
    /// </summary>
    public partial class PageBuilderPoc : Rock.Migrations.RockMigration
    {
        private const string BlankLayoutGuid = "2E169330-D7D7-4ECA-B417-72C64BE150F0";
        private const string PageBuilderLayoutGuid = "680D62E2-8018-4187-838F-7593CFB3F7CA";
        private const string PageBuilderPageGuid = "2540C899-5B4E-452B-865A-1E43A8BEAC2B";
        private const string PageBuilderPageRouteGuid = "B59DEF98-8C20-47EB-8E93-96300909ABE2";
        private const string PageBuilderSamplePageGuid = "83FC4394-ED09-461D-B865-68C1F5AFB34E";
        private const string PageBuilderEntityTypeGuid = "5968C4FC-2508-4457-B1D6-E3AC6711C9F9";
        private const string PageBuilderBlockTypeGuid = "89B56E5A-5A7D-44E6-85A0-49F101F814BE";
        private const string PageBuilderBlockGuid = "55C62D64-DCF5-4943-B0F2-74D9CC3CFEF2";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPageBuilderLayout_Up();
            AddPageBuilderPages_Up();
            AddPageBuilderBlock_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddPageBuilderBlock_Down();
            AddPageBuilderPages_Down();
            AddPageBuilderLayout_Down();
        }

        #region Private Methods

        /// <summary>
        /// Adds the internal site layout whose zone opts in to the Page Builder.
        /// </summary>
        private void AddPageBuilderLayout_Up()
        {
            RockMigrationHelper.AddLayout( SystemGuid.Site.SITE_ROCK_INTERNAL, "PageBuilder", "Page Builder", "", PageBuilderLayoutGuid );
        }

        /// <summary>
        /// Removes the Page Builder layout.
        /// </summary>
        private void AddPageBuilderLayout_Down()
        {
            RockMigrationHelper.DeleteLayout( PageBuilderLayoutGuid );
        }

        /// <summary>
        /// Adds the Page Builder page under CMS Configuration and the sample page it edits.
        /// </summary>
        private void AddPageBuilderPages_Up()
        {
            // The builder is a full screen worksurface, so its page uses the Blank layout to drop the site navigation.
            RockMigrationHelper.AddPage( true, SystemGuid.Page.CMS_CONFIGURATION, BlankLayoutGuid, "Page Builder", "", PageBuilderPageGuid, "ti ti-layout" );
            RockMigrationHelper.AddOrUpdatePageRoute( PageBuilderPageGuid, "admin/cms/page-builder", PageBuilderPageRouteGuid );

            RockMigrationHelper.AddPage( true, PageBuilderPageGuid, PageBuilderLayoutGuid, "Page Builder Sample", "", PageBuilderSamplePageGuid );

            // The sample page is only reached through the builder's iframe, so keep it out of navigation.
            Sql( $"UPDATE [Page] SET [DisplayInNavWhen] = {( int ) Rock.Model.DisplayInNavWhen.Never} WHERE [Guid] = '{PageBuilderSamplePageGuid}';" );
        }

        /// <summary>
        /// Removes the Page Builder sample page, route, and page.
        /// </summary>
        private void AddPageBuilderPages_Down()
        {
            RockMigrationHelper.DeletePage( PageBuilderSamplePageGuid );
            RockMigrationHelper.DeletePageRoute( PageBuilderPageRouteGuid );
            RockMigrationHelper.DeletePage( PageBuilderPageGuid );
        }

        /// <summary>
        /// Registers the Page Builder block type and places it on the Page Builder page.
        /// </summary>
        private void AddPageBuilderBlock_Up()
        {
            // Startup registers block entity types after migrations run, and AddOrUpdateEntityBlockType
            // does nothing without one, so the entity type is registered here first.
            RockMigrationHelper.UpdateEntityType(
                "Rock.Blocks.Cms.PageBuilder",
                "Page Builder",
                "Rock.Blocks.Cms.PageBuilder, Rock.Blocks, Version=21.0.1.0, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                PageBuilderEntityTypeGuid );

            RockMigrationHelper.AddOrUpdateEntityBlockType(
                "Page Builder",
                "Composes a page by dragging modules into its builder-enabled zones.",
                "Rock.Blocks.Cms.PageBuilder",
                "CMS",
                PageBuilderBlockTypeGuid );

            RockMigrationHelper.AddBlock( true, PageBuilderPageGuid, "", PageBuilderBlockTypeGuid, "Page Builder", "Main", "", "", 0, PageBuilderBlockGuid );
        }

        /// <summary>
        /// Removes the Page Builder block and block type.
        /// </summary>
        private void AddPageBuilderBlock_Down()
        {
            RockMigrationHelper.DeleteBlock( PageBuilderBlockGuid );
            RockMigrationHelper.DeleteBlockType( PageBuilderBlockTypeGuid );
        }

        #endregion
    }
}
