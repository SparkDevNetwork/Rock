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
    public partial class MigrationRollupv5a : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // add page route for packages
            RockMigrationHelper.AddPageRoute( "D6DC6AFE-70D9-43CF-9D76-EAEE2317FB14", "Package/{PackageId}", "65D53A2A-ED63-938B-4350-B1997B12426E" );

            // adjust controller security to allow finacial workers to use the scanning app without any other role
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.RestController", 0, "View", true, SystemGuid.Group.GROUP_FINANCE_USERS, 0, "81C76C47-3CB0-B5B6-4A8D-3A8B62AE8A23" );

            // fix type on 404
            Sql( @"UPDATE[HtmlContent] set[Content] = Replace([Content], 'adminstrator', 'administrator') where[Content] like '%see your adminstrator if you still need assistance%'" );

            // update the dashboard lava
            RockMigrationHelper.DeleteBlockAttributeValue( "415575C3-70AC-4A7A-8936-B98464C5557F", "D2149BA3-7AE8-4FE8-AF7C-4EF40DBEB4B2" );
            RockMigrationHelper.AddBlockAttributeValue( "415575C3-70AC-4A7A-8936-B98464C5557F", "D2149BA3-7AE8-4FE8-AF7C-4EF40DBEB4B2", @"{% include '/Assets/Lava/MyWorkflowsSortable.lava' %}" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // delete route for packages
            Sql( @"DELETE FROM [PageRoute] WHERE [Guid] = '65D53A2A-ED63-938B-4350-B1997B12426E'" );

            // remove security change for financial workers
            RockMigrationHelper.DeleteSecurityAuth( "81C76C47-3CB0-B5B6-4A8D-3A8B62AE8A23" );
        }
    }
}
