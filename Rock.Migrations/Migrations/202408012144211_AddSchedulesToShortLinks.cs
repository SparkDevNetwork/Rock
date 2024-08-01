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
    public partial class AddSchedulesToShortLinks : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddSchedulesToShortLinksUp();
            AddSourceValueIdToAttendanceUp();
            FixIncorrectCmsNamespaceUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            FixIncorrectCmsNamespaceDown();
            AddSourceValueIdToAttendanceDown();
            AddSchedulesToShortLinksDown();
        }

        private void AddSchedulesToShortLinksUp()
        {
            AddColumn( "dbo.PageShortLink", "IsScheduled", c => c.Boolean( nullable: false, defaultValue: false ) );
            AddColumn( "dbo.PageShortLink", "CategoryId", c => c.Int() );
            CreateIndex( "dbo.PageShortLink", "CategoryId" );
            AddForeignKey( "dbo.PageShortLink", "CategoryId", "dbo.Category", "Id" );
        }

        private void AddSchedulesToShortLinksDown()
        {
            DropForeignKey( "dbo.PageShortLink", "CategoryId", "dbo.Category" );
            DropIndex( "dbo.PageShortLink", new[] { "CategoryId" } );
            DropColumn( "dbo.PageShortLink", "CategoryId" );
            DropColumn( "dbo.PageShortLink", "IsScheduled" );
        }

        private void AddSourceValueIdToAttendanceUp()
        {
            AddColumn( "dbo.Attendance", "SourceValueId", c => c.Int() );
            CreateIndex( "dbo.Attendance", "SourceValueId" );
            AddForeignKey( "dbo.Attendance", "SourceValueId", "dbo.DefinedValue", "Id" );

            RockMigrationHelper.AddDefinedType( "Check-in",
                "Attendance Source",
                "List of possible sources for an attendance record.",
                SystemGuid.DefinedType.ATTENDANCE_SOURCE );

            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.ATTENDANCE_SOURCE,
                "Legacy Kiosk",
                "The attendance record was created by a legacy Kiosk.",
                SystemGuid.DefinedValue.ATTENDANCE_SOURCE_LEGACY_KIOSK );

            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.ATTENDANCE_SOURCE,
                "Kiosk",
                "The attendance record was created by a Kiosk.",
                SystemGuid.DefinedValue.ATTENDANCE_SOURCE_KIOSK );

            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.ATTENDANCE_SOURCE,
                "Mobile",
                "The attendance record was created by a personal mobile device.",
                SystemGuid.DefinedValue.ATTENDANCE_SOURCE_MOBILE );
        }

        private void AddSourceValueIdToAttendanceDown()
        {
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.ATTENDANCE_SOURCE_MOBILE );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.ATTENDANCE_SOURCE_KIOSK );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.ATTENDANCE_SOURCE_LEGACY_KIOSK );
            RockMigrationHelper.DeleteDefinedType( SystemGuid.DefinedType.ATTENDANCE_SOURCE );

            DropForeignKey( "dbo.Attendance", "SourceValueId", "dbo.DefinedValue" );
            DropIndex( "dbo.Attendance", new[] { "SourceValueId" } );
            DropColumn( "dbo.Attendance", "SourceValueId" );
        }

        private void FixIncorrectCmsNamespaceUp()
        {
            RockMigrationHelper.RenameEntityType( "AD614123-C7CA-40EE-B5D5-64D0D1C91378",
                "Rock.Blocks.Cms.PageShortLinkDetail",
                "Page Short Link Detail",
                "Rock.Blocks.Cms.PageShortLinkDetail, Rock.Blocks, Version=1.16.6.6, Culture=neutral, PublicKeyToken=null",
                false,
                false );
        }

        private void FixIncorrectCmsNamespaceDown()
        {
            RockMigrationHelper.RenameEntityType( "AD614123-C7CA-40EE-B5D5-64D0D1C91378",
                "Rock.Blocks.CMS.PageShortLinkDetail",
                "Page Short Link Detail",
                "Rock.Blocks.CMS.PageShortLinkDetail, Rock.Blocks, Version=1.16.6.6, Culture=neutral, PublicKeyToken=null",
                false,
                false );
        }
    }
}
