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
    public partial class AddGroupLocationOrder : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Adds the Order property to the GroupLocation model/db.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.GroupLocation", "Order", c => c.Int( nullable: false, defaultValue: 0 ) );
        }

        /// <summary>
        /// Removes the Order property from the GroupLocation model/db.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.GroupLocation", "Order");
        }
    }
}
