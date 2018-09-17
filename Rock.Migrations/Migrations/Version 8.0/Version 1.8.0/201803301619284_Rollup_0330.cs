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
    public partial class Rollup_0330 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Connection Opportunity Signup:Connection Opportunity
            RockMigrationHelper.UpdateBlockTypeAttribute( "C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9", "B188B729-FE6D-498B-8871-65AB8FD1E11E", "Connection Opportunity", "ConnectionOpportunity", "", @"If a Connection Opportunity is set, only details for it will be displayed (regardless of the querystring parameters).", 7, @"", "07CE421A-56F5-4E59-BE7F-6BC74F98ADA5" );

            // Attrib for BlockType: Account Tree View:Use Public Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "EC6ECB2B-665F-43FC-9FF4-A6B1CD5F2AE6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Public Name", "UsePublicName", "", @"Determines if the public name to be displayed for accounts.", 6, @"False", "8619897E-784B-45AF-BC81-664A482C1DA1" );

            Sql( MigrationSQL._201803301619284_Rollup_0330 );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
