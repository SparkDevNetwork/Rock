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
    public partial class CodeGenerated_20261118 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType
            //   BlockType: File Editor
            //   Category: CMS
            //   Attribute: Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0F1DADBC-6B12-4BAA-A828-FD1AA86AA387", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Root Folder", "RootFolder", "Root Folder", @"The folder that limits which files can be passed in the URL. Files outside it cannot be opened or saved. The file set in Relative File Path is always allowed. Must start with ~/. Uses ~/Content when blank.", 1, @"~/Content", "8E9B8BE9-18B7-40A4-901E-4FCED9322F18" );

            // Attribute for BlockType
            //   BlockType: SMS Conversation
            //   Category: Mobile > Communication
            //   Attribute: Allow Unrestricted Uploads
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4EF4250E-2D22-426C-ADAC-571C1301D18E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Unrestricted Uploads", "AllowUnrestrictedUploads", "Allow Unrestricted Uploads", @"If true, anyone with access to send messages can upload images or files. Otherwise, it'll use the permissions set for Communication Attachment binary file type.", 3, @"True", "E5FA7341-CA96-40F4-B7DF-59D858406B81" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: SMS Conversation
            //   Category: Mobile > Communication
            //   Attribute: Allow Unrestricted Uploads
            RockMigrationHelper.DeleteAttribute("E5FA7341-CA96-40F4-B7DF-59D858406B81");

            // Attribute for BlockType
            //   BlockType: File Editor
            //   Category: CMS
            //   Attribute: Root Folder
            RockMigrationHelper.DeleteAttribute("8E9B8BE9-18B7-40A4-901E-4FCED9322F18");
        }
    }
}
