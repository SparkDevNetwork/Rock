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
    public partial class CommunicationTemplateCssInliningEnabled : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.CommunicationTemplate", "CssInliningEnabled", c => c.Boolean(nullable: false));

            // Update the Wizard communication templates (DEFAULT and BLANK so far) templates to have CssInliningEnabled = true
            Sql( $"UPDATE [CommunicationTemplate] set [CssInliningEnabled] = 1 WHERE [Guid] in ('A3C7F623-7F6F-4C48-B66F-CBEE2DF30B6A', '88B7DF18-9C30-4BAC-8CA2-5AD253D57E4D')" );

            // DT: Update 'Unknown' Currency type to be a system value
            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE, "Unknown", "The currency type is unknown. For example, it might have been imported from a system that doesn't indicate currency type.", "56C9AE9C-B5EB-46D5-9650-2EF86B14F856", true );

            // DT: Update Person Duplicate Finder to take into account the Suffix
            Sql( MigrationSQL._201711141624083_CommunicationTemplateCssInliningEnabled_spCrm_PersonDuplicateFinder );

            // MP: Fix AnalyticsDimFinancialAccount IsTaxable
            Sql( MigrationSQL._201711141624083_CommunicationTemplateCssInliningEnabled_AnalyticsDimFinancialAccount );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.CommunicationTemplate", "CssInliningEnabled");
        }
    }
}
