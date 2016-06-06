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
    public partial class HtmlEditorMergeFieldPlugin : Rock.Migrations.RockMigration2
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "E7BD353C-91A6-4C15-A6C8-F44D0B16D16E", "7CFA101B-2D20-4523-9EC5-3F30502797A5", "CKEditor RockFileBrowser Plugin Frame", "", "4A4995CA-24F6-4D33-B861-A24274F53AA6", "" ); // Site:Rock Internal
            AddPage( "E7BD353C-91A6-4C15-A6C8-F44D0B16D16E", "7CFA101B-2D20-4523-9EC5-3F30502797A5", "CKEditor RockMergeField Plugin Frame", "", "1FC09F0D-72F2-44E6-9D16-2884F9AF33DD", "" ); // Site:Rock Internal
            AddBlockType( "CkEditor FileBrowser", "Block to be used as part of the RockFileBrowser CKEditor Plugin", "~/Blocks/Utility/CkEditorFileBrowser.ascx", "17A1687B-A2C7-4160-BF2B-2424DF69E9D5" );
            AddBlockType( "CkEditor MergeField", "Block to be used as part of the RockMergeField CKEditor Plugin", "~/Blocks/Utility/CkEditorMergeFieldPicker.ascx", "90187FFA-5474-40E0-BA0C-9C7E631CC46C" );

            // Add Block to Page: CKEditor RockFileBrowser Plugin Frame, Site: Rock Internal
            AddBlock( "4A4995CA-24F6-4D33-B861-A24274F53AA6", "", "17A1687B-A2C7-4160-BF2B-2424DF69E9D5", "CkEditor FileBrowser", "Main", "", "", 0, "74BD3FE7-E758-45AE-B598-F2A1AE133B7D" );

            // Add Block to Page: CKEditor RockMergeField Plugin Frame, Site: Rock Internal
            AddBlock( "1FC09F0D-72F2-44E6-9D16-2884F9AF33DD", "", "90187FFA-5474-40E0-BA0C-9C7E631CC46C", "CkEditor MergeField", "Main", "", "", 0, "A795CF36-E6AD-4B1A-9076-4435B5FF0267" );

            AddPageRoute( "4A4995CA-24F6-4D33-B861-A24274F53AA6", "ckeditorplugins/RockFileBrowser" );
            AddPageRoute( "1FC09F0D-72F2-44E6-9D16-2884F9AF33DD", "ckeditorplugins/RockMergeField" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: CkEditor MergeField, from Page: CKEditor RockMergeField Plugin Frame, Site: Rock Internal
            DeleteBlock( "A795CF36-E6AD-4B1A-9076-4435B5FF0267" );
            
            // Remove Block: CkEditor FileBrowser, from Page: CKEditor RockFileBrowser Plugin Frame, Site: Rock Internal
            DeleteBlock( "74BD3FE7-E758-45AE-B598-F2A1AE133B7D" );
            
            DeleteBlockType( "90187FFA-5474-40E0-BA0C-9C7E631CC46C" ); // CkEditor MergeField
            DeleteBlockType( "17A1687B-A2C7-4160-BF2B-2424DF69E9D5" ); // CkEditor FileBrowser
            DeletePage( "1FC09F0D-72F2-44E6-9D16-2884F9AF33DD" ); // Page: CKEditor RockMergeField Plugin FrameLayout: Dialog, Site: Rock Internal
            DeletePage( "4A4995CA-24F6-4D33-B861-A24274F53AA6" ); // Page: CKEditor RockFileBrowser Plugin FrameLayout: Dialog, Site: Rock Internal
        }
    }
}
