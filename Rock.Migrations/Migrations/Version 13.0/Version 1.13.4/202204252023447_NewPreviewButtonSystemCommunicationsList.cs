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
    public partial class NewPreviewButtonSystemCommunicationsList : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Page - Internal Name: System Communication Preview - Site: Rock RMS
            RockMigrationHelper.AddPage( true, "14D8F894-F70F-44F7-9F0C-2545F87256FF", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "System Communication Preview", "", SystemGuid.Page.SYSTEM_COMMUNICATION_PREVIEW, "fa-file-search" );
            // Add Page Route - Page:System Communication Preview - Route:admin/communications/system/preview
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.AddPageRoute( SystemGuid.Page.SYSTEM_COMMUNICATION_PREVIEW, "admin/communications/system/preview", "AAC42941-8B2C-4F20-923D-E74146D2E103" );
#pragma warning restore CS0618 // Type or member is obsolete
            // Add Block - Block Name: System Communication Preview - Page Name: System Communication Preview Layout: - Site: Rock RMS
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.SYSTEM_COMMUNICATION_PREVIEW.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "95366DA1-D878-4A9A-A26F-83160DBE784F".AsGuid(), "System Communication Preview", "Main", @"", @"", 0, "B8E3BF17-B4F7-49ED-87CF-EB3A837B1DB2" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block - Name: System Communication Preview, from Page: System Communication Preview, Site: Rock RMS - from Page: System Communication Preview, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B8E3BF17-B4F7-49ED-87CF-EB3A837B1DB2" );
            // Delete Page Internal Name: System Communication Preview Site: Rock RMS Layout: Full Width
            RockMigrationHelper.DeletePage( SystemGuid.Page.SYSTEM_COMMUNICATION_PREVIEW );
        }
    }
}
