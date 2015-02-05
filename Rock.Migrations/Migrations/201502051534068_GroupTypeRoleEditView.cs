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
    public partial class GroupTypeRoleEditView : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.GroupTypeRole", "CanView", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupTypeRole", "CanEdit", c => c.Boolean(nullable: false));

            Sql( @"
    UPDATE [GroupTypeRole] SET [CanView] = 1 
    WHERE [IsLeader] = 1
    OR [Guid] IN ( 
	    '8438D6C5-DB92-4C99-947B-60E9100F223D',	-- Organization Unit Leader
	    '17E516FC-76A4-4BF4-9B6F-0F859B13F563',	-- Organization Unit Member
	    'F6CECB48-52C1-4D25-9411-F1465755EB70', -- Serving Team Leader
	    '8F63AB81-A2F7-4D69-82E9-158FDD92DB3D', -- Serving Team Member
	    '6D798EFA-0110-41D5-BCE4-30ACEFE4317E', -- Small Group Leader
	    'F0806058-7E5D-4CA9-9C04-3BDF92739462'  -- Small Group Member
    )

    UPDATE [GroupTypeRole] SET [CanEdit] = 1 
    WHERE [IsLeader] = 1
    OR [Guid] IN ( 
	    '8438D6C5-DB92-4C99-947B-60E9100F223D',	-- Organization Unit Leader
	    'F6CECB48-52C1-4D25-9411-F1465755EB70', -- Serving Team Leader
	    '6D798EFA-0110-41D5-BCE4-30ACEFE4317E'  -- Small Group Leader
    )
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.GroupTypeRole", "CanEdit");
            DropColumn("dbo.GroupTypeRole", "CanView");
        }
    }
}
