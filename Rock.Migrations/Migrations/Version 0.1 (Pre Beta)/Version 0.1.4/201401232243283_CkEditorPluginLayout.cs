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
    public partial class CkEditorPluginLayout : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddLayout( "C2D29296-6A87-47A9-A753-EE4E9159C4C4", "Blank", "Blank", string.Empty, "2E169330-D7D7-4ECA-B417-72C64BE150F0" );
            Sql( @"
update [Page] set [LayoutId] = (select [Id] from [Layout] where [Guid] = '2E169330-D7D7-4ECA-B417-72C64BE150F0'), [IncludeAdminFooter] = 0 where [Guid] = '4A4995CA-24F6-4D33-B861-A24274F53AA6';
update [Page] set [LayoutId] = (select [Id] from [Layout] where [Guid] = '2E169330-D7D7-4ECA-B417-72C64BE150F0'), [IncludeAdminFooter] = 0 where [Guid] = '1FC09F0D-72F2-44E6-9D16-2884F9AF33DD';
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
update [Page] set [LayoutId] = (select [Id] from [Layout] where [Guid] = '7CFA101B-2D20-4523-9EC5-3F30502797A5'), [IncludeAdminFooter] = 1 where [Guid] = '4A4995CA-24F6-4D33-B861-A24274F53AA6';
update [Page] set [LayoutId] = (select [Id] from [Layout] where [Guid] = '7CFA101B-2D20-4523-9EC5-3F30502797A5'), [IncludeAdminFooter] = 1 where [Guid] = '1FC09F0D-72F2-44E6-9D16-2884F9AF33DD';
" );
            DeleteLayout("2E169330-D7D7-4ECA-B417-72C64BE150F0");
        }
    }
}
