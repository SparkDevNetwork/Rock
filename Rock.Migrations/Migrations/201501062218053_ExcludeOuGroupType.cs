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
    public partial class ExcludeOuGroupType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // exclude ou group type from group viewer
            RockMigrationHelper.AddBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Group Types Exclude", "GroupTypesExclude", "", "Select group types to exclude from this block.", 3, "", "D8EEB91B-745E-4D63-911B-728D8F1B0B6E" );
            RockMigrationHelper.AddBlockAttributeValue( "95612FCE-C40B-4CBB-AE26-800B52BE5FCD", "D8EEB91B-745E-4D63-911B-728D8F1B0B6E", "aab2e9f4-e828-4fee-8467-73dc9dab784c", true );

            // Rename the Defined Type 'Liquid Templates' -> 'Lava Templates
            Sql( @"  UPDATE [DefinedType] SET [Name] = 'Lava Templates' WHERE [Guid] = 'C3D44004-6951-44D9-8560-8567D705A48B'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
