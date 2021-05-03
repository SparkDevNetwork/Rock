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
    public partial class AddColumnToDataViewAndReport : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.DataView", "PersistedLastRunDuration", c => c.Int());
            AddColumn("dbo.DataView", "LastRunDateTime", c => c.DateTime());
            AddColumn("dbo.DataView", "RunCount", c => c.Int());
            AddColumn("dbo.DataView", "TimeToRunMS", c => c.Double());
            AddColumn("dbo.Report", "LastRunDateTime", c => c.DateTime());
            AddColumn("dbo.Report", "RunCount", c => c.Int());
            AddColumn("dbo.Report", "TimeToRunMS", c => c.Int());
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Report", "TimeToRunMS");
            DropColumn("dbo.Report", "RunCount");
            DropColumn("dbo.Report", "LastRunDateTime");
            DropColumn("dbo.DataView", "TimeToRunMS");
            DropColumn("dbo.DataView", "RunCount");
            DropColumn("dbo.DataView", "LastRunDateTime");
            DropColumn("dbo.DataView", "PersistedLastRunDuration");
        }
    }
}
