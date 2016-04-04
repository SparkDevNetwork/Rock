﻿// <copyright>
// Copyright by the Spark Development Network
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
    public partial class Metaphone : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.Metaphone",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 50),
                        Metaphone1 = c.String(maxLength: 4),
                        Metaphone2 = c.String(maxLength: 4),
                    })
                .PrimaryKey(t => t.Name);

            CreateIndex( "dbo.Metaphone", "Metaphone1", false );
            CreateIndex( "dbo.Metaphone", "Metaphone2", false );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropTable("dbo.Metaphone");
        }
    }
}
