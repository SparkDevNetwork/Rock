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
    public partial class ScheduledTxnSource : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.FinancialScheduledTransaction", "SourceTypeValueId", c => c.Int());
            CreateIndex("dbo.FinancialScheduledTransaction", "SourceTypeValueId");
            AddForeignKey("dbo.FinancialScheduledTransaction", "SourceTypeValueId", "dbo.DefinedValue", "Id");

            // MP: Fix Statement Generator when Individual Selected
            Sql( MigrationSQL._201605180005401_ScheduledTxnSource_GetFamilyTitle );
            Sql( MigrationSQL._201605180005401_ScheduledTxnSource_ContributionStatementQuery );

            // NA: SampleData block setting for new 5.0 feature
            RockMigrationHelper.AddBlockAttributeValue( "34CA1FA0-F8F1-449F-9788-B5E6315DC058", "5E26439E-4E98-45B1-B19B-D5B2F3405963", @"http://storage.rockrms.com/sampledata/sampledata_1_5_0.xml" );

            // JE: Change System Setting for Admin Group Membership in RSR - Rock Administrators Role
            Sql( @"
    UPDATE [GroupMember] SET [IsSystem] = 0 
	WHERE [Guid] = '6F23CACA-6749-4454-85DF-5A55251B644C'
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.FinancialScheduledTransaction", "SourceTypeValueId", "dbo.DefinedValue");
            DropIndex("dbo.FinancialScheduledTransaction", new[] { "SourceTypeValueId" });
            DropColumn("dbo.FinancialScheduledTransaction", "SourceTypeValueId");
        }
    }
}
