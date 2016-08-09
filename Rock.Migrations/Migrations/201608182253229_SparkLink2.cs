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
    public partial class SparkLink2 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateBlockType( "Notification List", "Displays notifications from the Spark Link.", "~/Blocks/Administration/NotificationList.ascx", "Utility", "9C0FD17D-677D-4A37-A61F-54C370954E83" );
            // Add Block to Page: Internal Homepage, Site: Rock RMS            
            RockMigrationHelper.AddBlock( "20F97A93-7949-4C2A-8A5E-C756FE8585CA", "", "9C0FD17D-677D-4A37-A61F-54C370954E83", "Notification List", "Main", "", "", 0, "60469A41-5180-446F-9935-0A09D81CD319" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '60469A41-5180-446F-9935-0A09D81CD319'" );  // Page: Internal Homepage,  Zone: Main,  Block: Notification List
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '62B1DBE6-B3D9-4C0B-BD12-1DD8C4F2C6EB'" );  // Page: Internal Homepage,  Zone: Main,  Block: Install Checklist
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '5F0DBB84-BFEF-43ED-9E51-E245DC85B7B5'" );  // Page: Internal Homepage,  Zone: Main,  Block: Homepage Welcome

            Random rnd = new Random();
            int minute = rnd.Next( 0, 61 );

            string insertJob = @"
                INSERT INTO [ServiceJob]
                ([IsSystem]
                ,[IsActive]
                ,[Name]
                ,[Description]
                ,[Class]
                ,[CronExpression]
                ,[NotificationStatus]
                ,[Guid])
             VALUES
                (0
                ,1
                ,'Spark Link'
                ,'Fetches Rock notifications from the Spark Development Network'
                ,'Rock.Jobs.SparkLink'
                ,'0 {0} 0/7 1/1 * ? *'
                ,1
                ,'645b1230-0c53-4fe3-91e2-8601ff00cbb5');";

            Sql( string.Format( insertJob, minute) );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Notification List, from Page: Internal Homepage, Site: Rock RMS       
            RockMigrationHelper.DeleteBlock( "60469A41-5180-446F-9935-0A09D81CD319" );
            RockMigrationHelper.DeleteBlockType( "9C0FD17D-677D-4A37-A61F-54C370954E83" ); // Notification List

            // Delete Job
            Sql( @"DELETE FROM [ServiceJob]
                WHERE [Guid]='645b1230-0c53-4fe3-91e2-8601ff00cbb5'" );
        }
    }
}
