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
    public partial class UpdateContentChannelItemSlugLength : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AlterColumn("dbo.ContentChannelItem", "ItemGlobalKey", c => c.String(maxLength: 200));
            AlterColumn("dbo.ContentChannelItemSlug", "Slug", c => c.String(maxLength: 200));
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"UPDATE [ContentChannelItemSlug]
SET [Slug] = LEFT( [Slug], 75 )
WHERE LEN( [Slug] ) > 75" );
            Sql( @"UPDATE [ContentChannelItem]
SET [ItemGlobalKey] = LEFT( [ItemGlobalKey], 100 )
WHERE LEN( [ItemGlobalKey] ) > 100" );
            AlterColumn("dbo.ContentChannelItemSlug", "Slug", c => c.String(maxLength: 75));
            AlterColumn("dbo.ContentChannelItem", "ItemGlobalKey", c => c.String(maxLength: 100));
        }
    }
}
