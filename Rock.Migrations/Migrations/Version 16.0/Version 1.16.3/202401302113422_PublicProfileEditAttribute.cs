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
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class PublicProfileEditAttribute : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add the new BlockType Attribute ( DisplayMode )
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                "841D1670-8BFD-4913-8409-FB47EB7A2AB9", // BlockTypeGuid
                "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", // Attribute.FieldTypeGuid
                "Display Mode", // Attribute.Name
                "DisplayMode", // Attribute.Key
                "Display Mode", // Attribute.AbbreviatedName
                "Specifies the Display Mode. To prevent people from editing their profile or family records choose 'View Only'.", // Attribute.Description
                5, // Attribute.Order
                "VIEWEDIT", // Attribute.DefaultValue
                "4D577FA2-8F7D-48F0-B511-AF87290E3321" // Attribute.Guid
            );

            // Map the old values to the new values.
            var oldToNewValuesMap = new Dictionary<string, string> {
                { "True", "VIEW" },
                { "False", "VIEWEDIT" }
            };

            RockMigrationHelper.MigrateBlockTypeAttributeKnownValues(
                "841D1670-8BFD-4913-8409-FB47EB7A2AB9",
                "13E4D341-CEEF-4B7E-BB3F-6FF5B3466817",
                "4D577FA2-8F7D-48F0-B511-AF87290E3321",
                oldToNewValuesMap
            );

            // Remove the old attribute ( ViewOnly ).
            RockMigrationHelper.DeleteAttribute( "13E4D341-CEEF-4B7E-BB3F-6FF5B3466817" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
