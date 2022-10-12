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
    public partial class AddPropertyForInheritedAttribute : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.RegistrationRegistrant", "RegistrationTemplateId", c => c.Int( nullable: true ) );
            Sql( @"
	            -- Set all RegistrationRegistrant RegistrationTemplateIds to their current respective value
                UPDATE rr 
	            SET 
		            rr.[RegistrationTemplateId] = ri.[RegistrationTemplateId]
	            FROM [RegistrationRegistrant] rr
	            INNER JOIN [Registration] r ON  r.[Id] = rr.[RegistrationId]
	            INNER JOIN [RegistrationInstance] ri ON ri.[Id] = r.[RegistrationInstanceId]
	            WHERE rr.[RegistrationTemplateId] IS NULL" );
            AlterColumn( "dbo.RegistrationRegistrant", "RegistrationTemplateId", c => c.Int( nullable: false ) );


            AddColumn( "dbo.GroupMember", "GroupTypeId", c => c.Int( nullable: true ) );
            Sql( @"
	            -- Set all GroupMember GroupTypeIds to their current respective value
                UPDATE gm 
	            SET 
		            gm.GroupTypeId = g.GroupTypeId
	            FROM [GroupMember] gm
	            INNER JOIN [Group] g ON  g.Id = gm.GroupId
	            WHERE gm.GroupTypeId IS NULL" );
            AlterColumn( "dbo.GroupMember", "GroupTypeId", c => c.Int( nullable: false ) );

            AddColumn( "dbo.ConnectionRequest", "ConnectionTypeId", c => c.Int( nullable: true ) );
            Sql( @"
	            -- Set all Connection Request ConnectionTypeIds to their current respective value
                UPDATE cr
 	            SET 
		            cr.[ConnectionTypeId] = co.[ConnectionTypeId]
	            FROM [ConnectionRequest] cr
	            INNER JOIN [ConnectionOpportunity] co ON  co.[Id] = cr.[ConnectionOpportunityId]
	            WHERE cr.[ConnectionTypeId] IS NULL" );
            AlterColumn( "dbo.ConnectionRequest", "ConnectionTypeId", c => c.Int( nullable: false ) );

            AddForeignKey("dbo.GroupMember", "GroupTypeId", "dbo.GroupType", "Id");
            AddForeignKey("dbo.RegistrationRegistrant", "RegistrationTemplateId", "dbo.RegistrationTemplate", "Id");
            AddForeignKey("dbo.ConnectionRequest", "ConnectionTypeId", "dbo.ConnectionType", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.ConnectionRequest", "ConnectionTypeId", "dbo.ConnectionType");
            DropForeignKey("dbo.RegistrationRegistrant", "RegistrationTemplateId", "dbo.RegistrationTemplate");
            DropForeignKey("dbo.GroupMember", "GroupTypeId", "dbo.GroupType");
            DropColumn("dbo.ConnectionRequest", "ConnectionTypeId");
            DropColumn("dbo.GroupMember", "GroupTypeId");
            DropColumn("dbo.RegistrationRegistrant", "RegistrationTemplateId");
        }
    }
}
