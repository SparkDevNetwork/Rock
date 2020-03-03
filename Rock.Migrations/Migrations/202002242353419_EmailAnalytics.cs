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
    public partial class EmailAnalytics : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( true, "7F79E512-B9DB-4780-9887-AD6D63A39050", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Email Analytics", "", "DF014200-72A3-48A0-A953-E594E5410E36", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "DF014200-72A3-48A0-A953-E594E5410E36", "EmailAnalytics", "DB116A39-BCE2-40D1-9604-FE955472DD35" );// for Page:Email Analytics
            RockMigrationHelper.UpdateBlockType( "Email Analytics", "Shows a graph of email statistics optionally limited to a specific communication or communication list.", "~/Blocks/Communication/EmailAnalytics.ascx", "Communication", "7B506760-93FA-4FBF-9FB5-0D9C3E36DCCD" );

            // Add Block to Page: Email Analytics Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "DF014200-72A3-48A0-A953-E594E5410E36".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "7B506760-93FA-4FBF-9FB5-0D9C3E36DCCD".AsGuid(), "Email Analytics", "Main", @"", @"", 0, "DC951B4F-0F07-47C3-A279-D1AFA1C50549" );
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'DC951B4F-0F07-47C3-A279-D1AFA1C50549'" );  // Page: Email Analytics,  Zone: Main,  Block: Email Analytics
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
