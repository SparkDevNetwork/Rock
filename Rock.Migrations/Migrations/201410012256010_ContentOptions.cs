// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class ContentOptions : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.ContentChannel", "ContentControlType", c => c.Int(nullable: false));
            AddColumn("dbo.ContentChannel", "RootImageDirectory", c => c.String(maxLength: 200));
            AddColumn("dbo.ContentChannelType", "DisablePriority", c => c.Boolean(nullable: false));
            AlterColumn("dbo.ContentChannel", "ChannelUrl", c => c.String(maxLength: 200));
            AlterColumn("dbo.ContentChannel", "ItemUrl", c => c.String(maxLength: 200));
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterColumn("dbo.ContentChannel", "ItemUrl", c => c.String());
            AlterColumn("dbo.ContentChannel", "ChannelUrl", c => c.String());
            DropColumn("dbo.ContentChannelType", "DisablePriority");
            DropColumn("dbo.ContentChannel", "RootImageDirectory");
            DropColumn("dbo.ContentChannel", "ContentControlType");
        }
    }
}
