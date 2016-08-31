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
    public partial class AccountNumberSecureNonUnique : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // drop the unique index
            DropIndex( "FinancialPersonBankAccount", new string[] { "AccountNumberSecured" } );

            // create the index, but as a non-unique
            CreateIndex( "FinancialPersonBankAccount", new string[] { "AccountNumberSecured" }, false );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // drop the index
            DropIndex( "FinancialPersonBankAccount", new string[] { "AccountNumberSecured" } );

            CreateIndex( "FinancialPersonBankAccount", new string[] { "AccountNumberSecured" }, true );
        }
    }
}
