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
    public partial class UpdateDurationFieldNames : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.DataView", "PersistedLastRunDurationMilliseconds", c => c.Int());
            AddColumn("dbo.DataView", "TimeToRunDurationMilliseconds", c => c.Double());
            AddColumn("dbo.Page", "MedianPageLoadTimeDurationSeconds", c => c.Double());
            AddColumn("dbo.Report", "TimeToRunDurationMilliseconds", c => c.Int());

            Sql( @"
UPDATE [DataView]
SET PersistedLastRunDurationMilliseconds = PersistedLastRunDuration
WHERE PersistedLastRunDuration IS NOT NULL

UPDATE [DataView]
SET TimeToRunDurationMilliseconds = TimeToRunMS
WHERE TimeToRunMS IS NOT NULL

UPDATE [Page]
SET MedianPageLoadTimeDurationSeconds = MedianPageLoadTime
WHERE MedianPageLoadTime IS NOT NULL

UPDATE [Report]
SET TimeToRunDurationMilliseconds = TimeToRunMS
WHERE TimeToRunMS IS NOT NULL
" );

            DropColumn("dbo.DataView", "PersistedLastRunDuration");
            DropColumn("dbo.DataView", "TimeToRunMS");
            DropColumn("dbo.Page", "MedianPageLoadTime");
            DropColumn("dbo.Report", "TimeToRunMS");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Report", "TimeToRunMS", c => c.Int());
            AddColumn("dbo.Page", "MedianPageLoadTime", c => c.Double());
            AddColumn("dbo.DataView", "TimeToRunMS", c => c.Double());
            AddColumn("dbo.DataView", "PersistedLastRunDuration", c => c.Int());
            DropColumn("dbo.Report", "TimeToRunDurationMilliseconds");
            DropColumn("dbo.Page", "MedianPageLoadTimeDurationSeconds");
            DropColumn("dbo.DataView", "TimeToRunDurationMilliseconds");
            DropColumn("dbo.DataView", "PersistedLastRunDurationMilliseconds");
        }
    }
}
