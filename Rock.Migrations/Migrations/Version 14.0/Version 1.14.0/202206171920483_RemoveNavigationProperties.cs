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
    public partial class RemoveNavigationProperties : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
             SK: 06/18/2022
                We removed the navigation properties due to which EF trying to remove the reference.
                But we want to kept the reference intact that is the reason we have commented out the below lines.
             */
            //DropForeignKey("dbo.GroupMember", "GroupTypeId", "dbo.GroupType");
            //DropForeignKey("dbo.RegistrationRegistrant", "RegistrationTemplateId", "dbo.RegistrationTemplate");
            //DropForeignKey("dbo.ConnectionRequest", "ConnectionTypeId", "dbo.ConnectionType");
            //DropIndex("dbo.RegistrationRegistrant", new[] { "RegistrationTemplateId" });
            //DropIndex("dbo.GroupMember", new[] { "GroupTypeId" });
            //DropIndex("dbo.ConnectionRequest", new[] { "ConnectionTypeId" });
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //CreateIndex("dbo.ConnectionRequest", "ConnectionTypeId");
            //CreateIndex("dbo.GroupMember", "GroupTypeId");
            //CreateIndex("dbo.RegistrationRegistrant", "RegistrationTemplateId");
            //AddForeignKey("dbo.ConnectionRequest", "ConnectionTypeId", "dbo.ConnectionType", "Id");
            //AddForeignKey("dbo.RegistrationRegistrant", "RegistrationTemplateId", "dbo.RegistrationTemplate", "Id");
            //AddForeignKey("dbo.GroupMember", "GroupTypeId", "dbo.GroupType", "Id");
        }
    }
}
