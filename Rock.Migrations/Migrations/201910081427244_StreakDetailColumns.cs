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
    public partial class StreakDetailColumns : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
                UPDATE [Block] SET [Zone] = 'SectionE' WHERE [Guid] = '366B8DDB-EBD1-455B-A426-4C6A35CE0842';
                UPDATE [Block] SET [Zone] = 'SectionF' WHERE [Guid] = '71680B7B-0922-478C-B0F3-61A75D17F8CC';" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
                UPDATE [Block] SET [Zone] = 'Main' WHERE [Guid] = '366B8DDB-EBD1-455B-A426-4C6A35CE0842';
                UPDATE [Block] SET [Zone] = 'Main' WHERE [Guid] = '71680B7B-0922-478C-B0F3-61A75D17F8CC';" );
        }
    }
}
