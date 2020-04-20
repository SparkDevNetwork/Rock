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
    public partial class GroupInactiveReason : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddNewColumnsUp();
            AddDefinedTypeUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddNewColumnsDown();
            AddDefinedtypeDown();
        }

        /// <summary>
        /// Adds the new columns up.
        /// </summary>
        private void AddNewColumnsUp()
        {
            AddColumn("dbo.Group", "InactiveReasonValueId", c => c.Int());
            AddColumn("dbo.Group", "InactiveReasonNote", c => c.String());
            AddColumn("dbo.GroupType", "EnableInactiveReason", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupType", "RequiresInactiveReason", c => c.Boolean(nullable: false));
            CreateIndex("dbo.Group", "InactiveReasonValueId");
            AddForeignKey("dbo.Group", "InactiveReasonValueId", "dbo.DefinedValue", "Id");
        }

        /// <summary>
        /// Adds the new columns down.
        /// </summary>
        private void AddNewColumnsDown()
        {
            DropForeignKey("dbo.Group", "InactiveReasonValueId", "dbo.DefinedValue");
            DropIndex("dbo.Group", new[] { "InactiveReasonValueId" });
            DropColumn("dbo.GroupType", "RequiresInactiveReason");
            DropColumn("dbo.GroupType", "EnableInactiveReason");
            DropColumn("dbo.Group", "InactiveReasonNote");
            DropColumn("dbo.Group", "InactiveReasonValueId");
        }

        /// <summary>
        /// Adds the Inactive Group Reasons defined type.
        /// </summary>
        private void AddDefinedTypeUp()
        {
            RockMigrationHelper.AddDefinedType("Group","Inactive Group Reasons","List of reasons why a group might be inactivated.","EB5D9839-F770-4E22-8B56-0B09397307D9",@"");
            RockMigrationHelper.AddDefinedTypeAttribute("EB5D9839-F770-4E22-8B56-0B09397307D9","F725B854-A15E-46AE-9D4C-0608D4154F1E","Group Type Filter","core_InactiveReasonsGroupTypeFilter","",1038,"","F3BE6600-F60A-45AB-B57A-1CC9231C873D");
            RockMigrationHelper.AddAttributeQualifier("F3BE6600-F60A-45AB-B57A-1CC9231C873D","repeatColumns","","F2FEA6D1-E614-4780-814C-E0F87D87C938");
        }

        /// <summary>
        /// Removes the Inactive Group Reasons defined type
        /// </summary>
        private void AddDefinedtypeDown()
        {
            RockMigrationHelper.DeleteAttribute("F3BE6600-F60A-45AB-B57A-1CC9231C873D"); // core_InactiveReasonsGroupTypeFilter
            RockMigrationHelper.DeleteDefinedType("EB5D9839-F770-4E22-8B56-0B09397307D9"); // Inactive Group Reasons
        }
    }
}
