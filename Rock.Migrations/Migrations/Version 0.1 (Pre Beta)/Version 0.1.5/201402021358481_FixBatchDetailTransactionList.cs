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
    public partial class FixBatchDetailTransactionList : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddBlockAttributeValue( "1133795B-3325-4D81-B603-F442F0AC892B", "29A6C37A-EFB3-41CC-A522-9CEFAAEEA910", "BDD09C8E-2C52-4D08-9062-BE7D52D190C2" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlockAttributeValue( "1133795B-3325-4D81-B603-F442F0AC892B", "29A6C37A-EFB3-41CC-A522-9CEFAAEEA910" );
        }
    }
}
