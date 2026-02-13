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
    public partial class UpdateConnectionWorkflowAndOpportunity : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateConnectionWorkflowUp();
            UpdateConnectionOpportunityUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateConnectionOpportunityDown();
            UpdateConnectionWorkflowDown();
        }

        #region Up Methods

        private void UpdateConnectionWorkflowUp()
        {
            AddColumn( "dbo.ConnectionWorkflow", "AppliesToAgeClassification", c => c.Int( nullable: false ) );
            AddColumn( "dbo.ConnectionWorkflow", "IncludeDataViewId", c => c.Int() );
            AddColumn( "dbo.ConnectionWorkflow", "ExcludeDataViewId", c => c.Int() );
            AddForeignKey( "dbo.ConnectionWorkflow", "ExcludeDataViewId", "dbo.DataView", "Id" );
            AddForeignKey( "dbo.ConnectionWorkflow", "IncludeDataViewId", "dbo.DataView", "Id" );
        }

        private void UpdateConnectionWorkflowDown()
        {
            DropForeignKey( "dbo.ConnectionWorkflow", "IncludeDataViewId", "dbo.DataView" );
            DropForeignKey( "dbo.ConnectionWorkflow", "ExcludeDataViewId", "dbo.DataView" );
            DropColumn( "dbo.ConnectionWorkflow", "ExcludeDataViewId" );
            DropColumn( "dbo.ConnectionWorkflow", "IncludeDataViewId" );
            DropColumn( "dbo.ConnectionWorkflow", "AppliesToAgeClassification" );
        }

        #endregion Up Methods

        #region Down Methods

        private void UpdateConnectionOpportunityUp()
        {
            AddColumn( "dbo.ConnectionOpportunity", "AdditionalSettingsJson", c => c.String() );
        }

        private void UpdateConnectionOpportunityDown()
        {
            DropColumn( "dbo.ConnectionOpportunity", "AdditionalSettingsJson" );
        }

        #endregion Down Methods
    }
}