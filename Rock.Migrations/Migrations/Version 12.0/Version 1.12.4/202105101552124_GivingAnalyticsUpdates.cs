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
<<<<<<< HEAD
    public partial class GivingAnalyticsUpdates : Rock.Migrations.RockMigration
=======
<<<<<<< HEAD:Rock.Migrations/Migrations/Version 12.0/Version 1.12.4/202105251443228_SparklineChart.cs
    public partial class SparklineChart : Rock.Migrations.RockMigration
=======
    public partial class GivingAnalyticsUpdates : Rock.Migrations.RockMigration
>>>>>>> 0bfe3f8165cc65e4d6e6751dddad9e64a5939b3d:Rock.Migrations/Migrations/Version 12.0/Version 1.12.4/202105101552124_GivingAnalyticsUpdates.cs
>>>>>>> 0bfe3f8165cc65e4d6e6751dddad9e64a5939b3d
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
<<<<<<< HEAD
            AddColumn("dbo.FinancialTransactionAlertType", "MaximumDaysSinceLastGift", c => c.Int());
=======
<<<<<<< HEAD:Rock.Migrations/Migrations/Version 12.0/Version 1.12.4/202105251443228_SparklineChart.cs
            Sql( MigrationSQL._202105251443228_SparklineChart_SparklineUpdate );
=======
            AddColumn("dbo.FinancialTransactionAlertType", "MaximumDaysSinceLastGift", c => c.Int());
>>>>>>> 0bfe3f8165cc65e4d6e6751dddad9e64a5939b3d:Rock.Migrations/Migrations/Version 12.0/Version 1.12.4/202105101552124_GivingAnalyticsUpdates.cs
>>>>>>> 0bfe3f8165cc65e4d6e6751dddad9e64a5939b3d
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
<<<<<<< HEAD
            DropColumn("dbo.FinancialTransactionAlertType", "MaximumDaysSinceLastGift");
=======
<<<<<<< HEAD:Rock.Migrations/Migrations/Version 12.0/Version 1.12.4/202105251443228_SparklineChart.cs
=======
            DropColumn("dbo.FinancialTransactionAlertType", "MaximumDaysSinceLastGift");
>>>>>>> 0bfe3f8165cc65e4d6e6751dddad9e64a5939b3d:Rock.Migrations/Migrations/Version 12.0/Version 1.12.4/202105101552124_GivingAnalyticsUpdates.cs
>>>>>>> 0bfe3f8165cc65e4d6e6751dddad9e64a5939b3d
        }
    }
}
