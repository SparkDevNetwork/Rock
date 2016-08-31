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
    public partial class BinaryFileLastModifiedDateTime : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // make sure that ModifiedDateTime has the most recent ModifiedDateTime of either LastModifiedDateTime or ModifiedDateTIme
            Sql( "Update BinaryFile set [ModifiedDateTime] = [LastModifiedDateTime] where [ModifiedDateTime] is null or [ModifiedDateTime] > [LastModifiedDateTime]" );
            DropColumn("dbo.BinaryFile", "LastModifiedDateTime");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.BinaryFile", "LastModifiedDateTime", c => c.DateTime());
        }
    }
}
