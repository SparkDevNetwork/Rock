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
#pragma warning disable IDE1006 // Naming Styles
    public partial class spCrm_PersonDuplicateFinder : Rock.Migrations.RockMigration
#pragma warning restore IDE1006 // Naming Styles
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( MigrationSQL._202012022334116_spCrm_PersonDuplicateFinder_spCrm_PersonDuplicateFinder );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //Sql( MigrationSQL._201804272301456_MergeFromV7_4_spCrm_PersonDuplicateFinder );
        }
    }
}
