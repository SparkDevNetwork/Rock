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
    public partial class PhoneFullNumber : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.PhoneNumber", "FullNumber", c => c.String(nullable: false, maxLength: 23));
            Sql( "UPDATE [PhoneNumber] SET [FullNumber] = CONCAT([CountryCode], [Number]) where [FullNumber] is null OR [FullNumber] != CONCAT([CountryCode], [Number])" );
            Sql( "ALTER TABLE [PhoneNumber] ALTER COLUMN [FullNumber] nvarchar(23) NOT NULL" );
            CreateIndex( "dbo.PhoneNumber", "FullNumber" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.PhoneNumber", new[] { "FullNumber" });
            DropColumn("dbo.PhoneNumber", "FullNumber");
        }
    }
}
