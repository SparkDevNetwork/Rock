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
    public partial class VerifySecurityBlockPage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Page: Inspect Security, Parent: Security, Layout: FullWidth, Site: Internal Website
            RockMigrationHelper.AddPage( "91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Inspect Security", string.Empty, "B8CACE4E-1B10-46F4-B147-31F32B442915", "fa fa-search" );

            // Add Block Type: Verify Security
            RockMigrationHelper.AddBlockType( "Verify Security", "Verify the security of an entity and how it applies to a specified user.", "~/Blocks/Security/VerifySecurity.ascx", "Security", "65F18F6C-AD97-42A7-958D-20359E804965" );

            // Add Block: Verify Security, to Page: Inspect Security, Site: Internal Website
            RockMigrationHelper.AddBlock( "B8CACE4E-1B10-46F4-B147-31F32B442915", string.Empty, "65F18F6C-AD97-42A7-958D-20359E804965", "Verify Security", "Main", string.Empty, string.Empty, 0, "E7A8210C-C29E-4EA3-AA9D-3006B6B676DF" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Inspect Security, from Page: Verify Security, Site: Internal Website
            RockMigrationHelper.DeleteBlock( "E7A8210C-C29E-4EA3-AA9D-3006B6B676DF" );

            // Remove Block Type: Inspect Security
            RockMigrationHelper.DeleteBlockType( "65F18F6C-AD97-42A7-958D-20359E804965" );

            // Remove Page: Inspect Security, Layout: FullWidth, Site: Internal Website
            RockMigrationHelper.DeletePage( "B8CACE4E-1B10-46F4-B147-31F32B442915" );
        }
    }
}
