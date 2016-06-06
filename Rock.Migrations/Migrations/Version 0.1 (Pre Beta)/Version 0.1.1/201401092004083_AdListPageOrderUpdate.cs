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
    public partial class AdListPageOrderUpdate : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
                Sql(@"UPDATE [PAGE] SET [Order] = 4 WHERE [Guid] = '1A3437C8-D4CB-4329-A366-8D6A4CBF79BF'");
                Sql(@"UPDATE [PAGE] SET [Order] = 5 WHERE [Guid] = 'CADB44F2-2453-4DB5-AB11-DADA5162AB79'");
                Sql(@"UPDATE [PAGE] SET [Order] = 6 WHERE [Guid] = '2A22D08D-73A8-4AAF-AC7E-220E8B2E7857'");
                Sql(@"UPDATE [PAGE] SET [Order] = 3 WHERE [Guid] = '78D470E9-221B-4EBD-9FF6-995B45FB9CD5'");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
