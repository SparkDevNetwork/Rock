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
    public partial class AttributeColor : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /* 12-10-2021 MDP 
            The previous version of this migration had set AttributeColor as NOT NULL. But it should have been nullable.
            This fixes that, and the next migration will make sure it get set back nullable just in case they previous version of this migration ran.
            
            */
            AddColumn("dbo.Attribute", "AttributeColor", c => c.String(maxLength: 100));
            
            
            AddColumn("dbo.FinancialScheduledTransaction", "PreviousGatewayScheduleIdsJson", c => c.String());
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.FinancialScheduledTransaction", "PreviousGatewayScheduleIdsJson");
            DropColumn("dbo.Attribute", "AttributeColor");
        }
    }
}
