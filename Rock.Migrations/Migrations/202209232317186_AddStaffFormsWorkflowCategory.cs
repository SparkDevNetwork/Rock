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
    public partial class AddStaffFormsWorkflowCategory : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateCategory( Rock.SystemGuid.EntityType.WORKFLOW_TYPE, "Staff Forms", "fa fa-clipboard-list", "A default category for staff built forms from Form Builder.", Rock.SystemGuid.Category.WORKFLOW_TYPE_STAFF_FORMS );
            RockMigrationHelper.AddSecurityAuthForCategory( Rock.SystemGuid.Category.WORKFLOW_TYPE_STAFF_FORMS, 1, "Edit", true, "2C112948-FF4C-46E7-981A-0257681EADF4", 0, "A16D6D37-86A7-4038-8033-14BE2D9503FC" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
