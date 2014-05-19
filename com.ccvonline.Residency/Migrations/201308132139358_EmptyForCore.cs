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
namespace com.ccvonline.Residency.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class EmptyForCore : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
/* Skipped Operations for tables that are not part of ResidencyContext: Review these comments to verify the proper things were skipped */
/* To disable skipping, edit your Migrations\Configuration.cs so that CodeGenerator = new RockCSharpMigrationCodeGenerator<ResidencyContext>(false); */

// Up()...
// AddColumnOperation for TableName GroupType, column LocationSelectionMode.
// DropColumnOperation for TableName GroupType, column LocationType.

// Down()...
// AddColumnOperation for TableName GroupType, column LocationType.
// DropColumnOperation for TableName GroupType, column LocationSelectionMode.
