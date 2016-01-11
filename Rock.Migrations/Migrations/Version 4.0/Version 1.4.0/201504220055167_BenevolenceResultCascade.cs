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
    public partial class BenevolenceResultCascade : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.BenevolenceResult", "BenevolenceRequestId", "dbo.BenevolenceRequest");
            AddForeignKey("dbo.BenevolenceResult", "BenevolenceRequestId", "dbo.BenevolenceRequest", "Id", cascadeDelete: true);

            RockMigrationHelper.AddGroupTypeGroupAttribute( "2C42B2D4-1C5F-4AD5-A9AD-08631B872AC4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Requires Background Check", "Is a background check required to serve on this team?", 0, "", "6DC6E992-4CAF-4C9F-B11D-5918D244BD40" );

            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.AddPersonToGroup", "Add Person to Specified Group", "Rock.Workflow.Action.AddPersonToGroup, Rock, Version=1.3.1.0, Culture=neutral, PublicKeyToken=null", false, true, "DF0167A1-6928-4FBC-893B-5826A28AAC83" );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.AddPersonToGroupWFAttribute", "Add Person to Group using Workflow Attribute", "Rock.Workflow.Action.AddPersonToGroupWFAttribute, Rock, Version=1.3.1.0, Culture=neutral, PublicKeyToken=null", false, true, "BD53F375-78A2-4A54-B1D1-2D805F3FCD44" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.BenevolenceResult", "BenevolenceRequestId", "dbo.BenevolenceRequest");
            AddForeignKey("dbo.BenevolenceResult", "BenevolenceRequestId", "dbo.BenevolenceRequest", "Id");
        }
    }
}
