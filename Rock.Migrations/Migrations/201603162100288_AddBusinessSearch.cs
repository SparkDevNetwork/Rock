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
    public partial class AddBusinessSearch : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add new Business Name search component
            RockMigrationHelper.UpdateEntityType( "Rock.Search.Person.BusinessName", "Business Name", "Rock.Search.Person.BusinessName, Rock, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null", false, true, "944ACDD0-A4AC-4E5A-8689-E2D8EF773BC2" );

            // Add Attributes for Business Name search component
            RockMigrationHelper.AddEntityAttribute( "Rock.Search.Person.BusinessName", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "", "Should Service be used?", 0, "False", "35578D31-13DE-4952-9707-67DBE00D2E37" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Search.Person.BusinessName", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "", "", "Order", "", "The order that this service should be used (priority)", 0, "", "699D01D2-57B4-499B-BB98-B773EF343A30" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Search.Person.BusinessName", "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Result URL", "", "The url to redirect user to after they have entered search text.  (use '{0}' for the search text)", 0, "", "1ECEF3CD-0B36-4669-AFA7-72D6A79C1536" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Search.Person.BusinessName", "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Search Label", "", "The text to display in the search type dropdown", 0, "Search", "70321846-B028-40AB-A41F-84AD8A88933C" );

            // Add Attribute Values
            RockMigrationHelper.AddAttributeValue( "70321846-B028-40AB-A41F-84AD8A88933C", 0, @"Business", "70321846-B028-40AB-A41F-84AD8A88933C" ); // Search Label
            RockMigrationHelper.AddAttributeValue( "1ECEF3CD-0B36-4669-AFA7-72D6A79C1536", 0, @"Business/Search/name/?SearchTerm={0}", "1ECEF3CD-0B36-4669-AFA7-72D6A79C1536" ); // Result URL
            RockMigrationHelper.AddAttributeValue( "699D01D2-57B4-499B-BB98-B773EF343A30", 0, @"5", "699D01D2-57B4-499B-BB98-B773EF343A30" ); // Order
            RockMigrationHelper.AddAttributeValue( "35578D31-13DE-4952-9707-67DBE00D2E37", 0, @"True", "35578D31-13DE-4952-9707-67DBE00D2E37" ); // Active

            // Add a route to the Business List page for the search results
            RockMigrationHelper.AddPageRoute( "F4DF4899-2D44-4997-BA9B-9D2C64958A20", "Business/Search/{SearchType}", "A105F0F9-3EAA-4EA8-94B0-394370DFB810" );

            // Secure the Rock.Search.Person.BusinessName component (408 TODO change to Guid)
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Search.Person.BusinessName", 1, "View", true, "6246A7EF-B7A3-4C8C-B1E4-3FF114B84559", 0, "052AF751-BF8B-49AE-BA4A-72C88119BECA" ); // EntityType:Rock.Search.Person.BusinessName Group: 6246A7EF-B7A3-4C8C-B1E4-3FF114B84559 ( RSR - Finance Administration ), 
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Search.Person.BusinessName", 2, "View", true, "2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9", 0, "42F0D039-65F3-4262-AB18-7EFDE30FDB92" ); // EntityType:Rock.Search.Person.BusinessName Group: 2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9 ( RSR - Finance Worker ), 
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Search.Person.BusinessName", 3, "View", false, "", 1, "1FF24243-FF2E-4FF5-B671-D54AAC4423AF" ); // EntityType:Rock.Search.Person.BusinessName Group: <all users>
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Search.Person.BusinessName", 0, "View", true, "628C51A8-4613-43ED-A18D-4A6FB999273E", 0, "D5476011-1B24-4A05-A61D-665DBD1B8379" ); // EntityType:Rock.Search.Person.BusinessName Group: 628C51A8-4613-43ED-A18D-4A6FB999273E ( RSR - Rock Administration ), 

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteSecurityAuth( "052AF751-BF8B-49AE-BA4A-72C88119BECA" ); // EntityType:Rock.Search.Person.BusinessName Group: 6246A7EF-B7A3-4C8C-B1E4-3FF114B84559 ( RSR - Finance Administration ), 
            RockMigrationHelper.DeleteSecurityAuth( "42F0D039-65F3-4262-AB18-7EFDE30FDB92" ); // EntityType:Rock.Search.Person.BusinessName Group: 2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9 ( RSR - Finance Worker ), 
            RockMigrationHelper.DeleteSecurityAuth( "1FF24243-FF2E-4FF5-B671-D54AAC4423AF" ); // EntityType:Rock.Search.Person.BusinessName Group: <all users>
            RockMigrationHelper.DeleteSecurityAuth( "D5476011-1B24-4A05-A61D-665DBD1B8379" ); // EntityType:Rock.Search.Person.BusinessName Group: 628C51A8-4613-43ED-A18D-4A6FB999273E ( RSR - Rock Administration ), 

            RockMigrationHelper.DeleteAttribute( "70321846-B028-40AB-A41F-84AD8A88933C" );    // Rock.Search.Person.BusinessName: Search Label
            RockMigrationHelper.DeleteAttribute( "1ECEF3CD-0B36-4669-AFA7-72D6A79C1536" );    // Rock.Search.Person.BusinessName: Result URL
            RockMigrationHelper.DeleteAttribute( "699D01D2-57B4-499B-BB98-B773EF343A30" );    // Rock.Search.Person.BusinessName: Order
            RockMigrationHelper.DeleteAttribute( "35578D31-13DE-4952-9707-67DBE00D2E37" );    // Rock.Search.Person.BusinessName: Active

            RockMigrationHelper.DeleteEntityType( "944ACDD0-A4AC-4E5A-8689-E2D8EF773BC2" );
        }
    }
}
