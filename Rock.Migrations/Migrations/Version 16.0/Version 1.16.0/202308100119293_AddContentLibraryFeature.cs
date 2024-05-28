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
    /// <summary>
    ///
    /// </summary>
    public partial class AddContentLibraryFeature : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.ContentChannelItem", "IsContentLibraryOwner", c => c.Boolean());
            AddColumn("dbo.ContentChannelItem", "ContentLibrarySourceIdentifier", c => c.Guid());
            AddColumn("dbo.ContentChannelItem", "ContentLibraryLicenseTypeValueId", c => c.Int());
            AddColumn("dbo.ContentChannelItem", "ContentLibraryContentTopicId", c => c.Int());
            AddColumn("dbo.ContentChannelItem", "ContentLibraryUploadedByPersonAliasId", c => c.Int());
            AddColumn("dbo.ContentChannelItem", "ContentLibraryUploadedDateTime", c => c.DateTime());
            AddColumn("dbo.ContentChannelItem", "ExperienceLevel", c => c.Int());
            AddColumn("dbo.ContentChannel", "ContentLibraryConfigurationJson", c => c.String());
            CreateIndex("dbo.ContentChannelItem", "ContentLibraryUploadedByPersonAliasId");
            AddForeignKey("dbo.ContentChannelItem", "ContentLibraryUploadedByPersonAliasId", "dbo.PersonAlias", "Id");

            // Add Page
            //  Internal Name: Library Viewer
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, SystemGuid.Page.CMS_CONFIGURATION, "C2467799-BB45-4251-8EE6-F0BF27201535", "Library Viewer", string.Empty, SystemGuid.Page.LIBRARY_VIEWER, "fa fa-book" );

            // Set 'Display in Nav When' to [Never=2]
            Sql( $"UPDATE [Page] SET [DisplayInNavWhen] = 2 WHERE [Guid] = '{SystemGuid.Page.LIBRARY_VIEWER}'" );

#pragma warning disable CS0618 // Type or member is obsolete
            // Add Page Route
            //   Page:Library Viewer
            //   Route:admin/cms/library-viewer
            RockMigrationHelper.AddPageRoute( SystemGuid.Page.LIBRARY_VIEWER, "admin/cms/content-library", SystemGuid.PageRoute.LIBRARY_VIEWER );
#pragma warning restore CS0618 // Type or member is obsolete

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.LibraryViewer
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.LibraryViewer", "Library Viewer", "Rock.Blocks.Cms.LibraryViewer, Rock.Blocks, Version=1.16.0.8, Culture=neutral, PublicKeyToken=null", false, false, "C368D439-37CC-4304-AC18-873DEC76289C" );

            // Add/Update Obsidian Block Type
            //   Name:Library Viewer
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.LibraryViewer
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Library Viewer", "Displays items from the Spark Development Network Content Library.", "Rock.Blocks.Cms.LibraryViewer", "CMS", "B147D578-B5CF-4265-9D92-B7BC43BF1CBC" );

            // Add Block
            //  Block Name: Library Viewer
            //  Page Name: Library Viewer
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.LIBRARY_VIEWER.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "B147D578-B5CF-4265-9D92-B7BC43BF1CBC".AsGuid(), "Library Viewer", "Main", @"", @"", 0, "95486157-9132-41AF-90E6-1A99BDD70FB7" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block
            //  Name: Library Viewer, from Page: Library Viewer, Site: Rock RMS
            //  from Page: Library Viewer, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "95486157-9132-41AF-90E6-1A99BDD70FB7" );

            // Delete BlockType 
            //   Name: Library Viewer
            //   Category: CMS
            //   Path: -
            //   EntityType: Library Viewer
            RockMigrationHelper.DeleteBlockType( "B147D578-B5CF-4265-9D92-B7BC43BF1CBC" );

            // Delete Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.LibraryViewer
            RockMigrationHelper.DeleteEntityType( "C368D439-37CC-4304-AC18-873DEC76289C" );

            // Delete Page Route
            //   Page:Library Viewer
            //   Route:admin/cms/content-library
            RockMigrationHelper.DeletePageRoute( SystemGuid.PageRoute.LIBRARY_VIEWER );

            // Delete Page 
            //  Internal Name: Library Viewer
            //  Site: Rock RMS
            //  Layout: Full Worksurface
            RockMigrationHelper.DeletePage( SystemGuid.Page.LIBRARY_VIEWER );

            DropForeignKey( "dbo.ContentChannelItem", "ContentLibraryUploadedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.ContentChannelItem", new[] { "ContentLibraryUploadedByPersonAliasId" });
            DropColumn("dbo.ContentChannel", "ContentLibraryConfigurationJson");
            DropColumn("dbo.ContentChannelItem", "ExperienceLevel");
            DropColumn("dbo.ContentChannelItem", "ContentLibraryUploadedDateTime");
            DropColumn("dbo.ContentChannelItem", "ContentLibraryUploadedByPersonAliasId");
            DropColumn("dbo.ContentChannelItem", "ContentLibraryContentTopicId");
            DropColumn("dbo.ContentChannelItem", "ContentLibraryLicenseTypeValueId");
            DropColumn("dbo.ContentChannelItem", "ContentLibrarySourceIdentifier");
            DropColumn("dbo.ContentChannelItem", "IsContentLibraryOwner");
        }
    }
}
