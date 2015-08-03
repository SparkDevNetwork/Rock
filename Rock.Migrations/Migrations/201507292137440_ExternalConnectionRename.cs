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
    public partial class ExternalConnectionRename : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //
            // rename blocks
            //

            // External Connection Opportunity Detail -> Connection Opportunity Detail Lava
            Sql( @"UPDATE [BlockType] SET [Path] = '~/Blocks/Connection/ConnectionOpportunityDetailLava.ascx', [Name] = 'Connection Opportunity Detail Lava' WHERE [Guid] = 'B8CA0630-29E7-41B9-B4F1-EB6DE043EBDC'" );

            // External Opportunity Search -> Connection Opportunity Search
            Sql( @"UPDATE [BlockType] SET [Path] = '~/Blocks/Connection/ConnectionOpportunitySearch.ascx', [Name] = 'Connection Opportunity Search' WHERE [Guid] = 'C0D58DEE-D266-4AA8-8750-414A3CC26C07'" );

            // External Connection Opportunity Signup -> Connection Opportunity Signup
            Sql( @"UPDATE [BlockType] SET [Path] = '~/Blocks/Connection/ConnectionOpportunitySignup.ascx', [Name] = 'Connection Opportunity Signup' WHERE [Guid] = 'C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9'" );

            RockMigrationHelper.AddBlockTypeAttribute( "B8CA0630-29E7-41B9-B4F1-EB6DE043EBDC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Set Page Title", "SetPageTitle", "", "Determines if the block should set the page title with the package name.", 0, @"False", "754A1F8A-6AEB-4DE8-BF27-3C4EB5ED24FC" );

            RockMigrationHelper.AddBlockTypeAttribute( "B8CA0630-29E7-41B9-B4F1-EB6DE043EBDC", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "Lava template to use to display the package details.", 2, @"{% include '~~/Assets/Lava/OpportunityDetail.lava' %}", "0A47D905-5029-4F55-8F89-9D45EC93B304" );

            RockMigrationHelper.AddBlockTypeAttribute( "B8CA0630-29E7-41B9-B4F1-EB6DE043EBDC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Display a list of merge fields available for lava.", 3, @"False", "7AA6A7EB-28A3-4A9B-91FF-B4855AD97671" );

            RockMigrationHelper.AddBlockTypeAttribute( "C0D58DEE-D266-4AA8-8750-414A3CC26C07", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Campus Context", "EnableCampusContext", "", "If the page has a campus context it's value will be used as a filter", 0, @"True", "47A74DCA-9D5D-458C-81F1-ABD6D0ACA23E" );

            RockMigrationHelper.AddBlockTypeAttribute( "C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Campus Context", "EnableCampusContext", "", "If the page has a campus context it's value will be used as a filter", 0, @"True", "1DB9D0A9-15DE-43A5-BC36-AC9501865064" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "1DB9D0A9-15DE-43A5-BC36-AC9501865064" );
            RockMigrationHelper.DeleteAttribute( "47A74DCA-9D5D-458C-81F1-ABD6D0ACA23E" );
            RockMigrationHelper.DeleteAttribute( "7AA6A7EB-28A3-4A9B-91FF-B4855AD97671" );
            RockMigrationHelper.DeleteAttribute( "0A47D905-5029-4F55-8F89-9D45EC93B304" );
            RockMigrationHelper.DeleteAttribute( "754A1F8A-6AEB-4DE8-BF27-3C4EB5ED24FC" );
        }
    }
}
