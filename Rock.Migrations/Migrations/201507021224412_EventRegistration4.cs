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
    public partial class EventRegistration4 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Registration", "DiscountCode", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Registration", "DiscountPercentage", c => c.Double(nullable: false));
            AddColumn("dbo.Registration", "DiscountAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));

            CreateIndex( "dbo.FinancialTransactionDetail", new string[] { "EntityTypeId", "EntityId" } );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex( "dbo.FinancialTransactionDetail", new string[] { "EntityTypeId", "EntityId" }  );

            DropColumn("dbo.Registration", "DiscountAmount");
            DropColumn("dbo.Registration", "DiscountPercentage");
            DropColumn("dbo.Registration", "DiscountCode");
        }
    }
}
