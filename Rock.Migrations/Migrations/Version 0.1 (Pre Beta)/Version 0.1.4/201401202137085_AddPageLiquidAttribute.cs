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
    public partial class AddPageLiquidAttribute : Rock.Migrations.RockMigration2
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddBlockTypeAttribute( "CACB9D1A-A820-4587-986A-D66A69EE9948", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Secondary Block", "IsSecondaryBlock", "", "Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", 0, "False", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2");
            AddBlockAttributeValue( "34EC5861-84EF-4D1A-8C89-D207B3004FDC", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", "True" );
            AddBlockAttributeValue( "6E8216F2-6A48-4EF8-ABA3-736C2468610D", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", "True" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlockAttributeValue( "6E8216F2-6A48-4EF8-ABA3-736C2468610D", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2" );
            DeleteBlockAttributeValue( "34EC5861-84EF-4D1A-8C89-D207B3004FDC", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2" );
            DeleteAttribute( "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2" );
        }
    }
}
