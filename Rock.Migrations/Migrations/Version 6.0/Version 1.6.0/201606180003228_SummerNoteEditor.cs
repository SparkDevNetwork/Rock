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
    public partial class SummerNoteEditor : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.RenameBlockType( "~/Blocks/Utility/CkEditorFileBrowser.ascx", "~/Blocks/Utility/HtmlEditorFileBrowser.ascx", null, "HtmlEditor FileBrowser", "Block to be used as part of the RockFileBrowser HtmlEditor Plugin" );
            RockMigrationHelper.RenameBlockType( "~/Blocks/Utility/CkEditorMergeFieldPicker.ascx", "~/Blocks/Utility/HtmlEditorMergeFieldPicker.ascx", null, "HtmlEditor MergeField", "Block to be used as part of the RockMergeField HtmlEditor Plugin" );

            RockMigrationHelper.AddPageRoute( "4A4995CA-24F6-4D33-B861-A24274F53AA6", "htmleditorplugins/RockFileBrowser" );
            RockMigrationHelper.AddPageRoute( "1FC09F0D-72F2-44E6-9D16-2884F9AF33DD", "htmleditorplugins/RockMergeField" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
