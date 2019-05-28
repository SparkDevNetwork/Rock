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
    public partial class AddGlobalAttrib_EnableRouteDomainMatching : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddGlobalAttribute( "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Enable Route Domain Matching", "When enabled, the domain in the request must match the page's site domain; otherwise the site's 404 page will be used. If set to No, a route outside the page's site domain can match (if one inside the site was not found).", 0, @"True", "0B7DD63E-AD00-445E-8E9D-047956FEAFB3", "core_EnableRouteDomainMatching" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "0B7DD63E-AD00-445E-8E9D-047956FEAFB3" );    // Global Attribute : Enable Route Domain Matching
        }
    }
}
