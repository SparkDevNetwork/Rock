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
    public partial class PageTitleFields : Rock.Migrations.RockMigration2
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameColumn( "dbo.Page", "Name", "InternalName" );
            RenameColumn( "dbo.Page", "Title", "PageTitle" );

            AddColumn("dbo.Page", "BrowserTitle", c => c.String(maxLength: 100));
            AddColumn("dbo.Page", "KeyWords", c => c.String());
            AddColumn("dbo.Page", "HeaderContent", c => c.String());

            Sql( @"
    UPDATE [Page] SET [BrowserTitle] = [PageTitle]
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RenameColumn( "dbo.Page", "InternalName", "Name" );
            RenameColumn( "dbo.Page", "PageTitle", "Title" );

            DropColumn("dbo.Page", "HeaderContent");
            DropColumn("dbo.Page", "KeyWords");
            DropColumn("dbo.Page", "BrowserTitle");
        }
    }
}
