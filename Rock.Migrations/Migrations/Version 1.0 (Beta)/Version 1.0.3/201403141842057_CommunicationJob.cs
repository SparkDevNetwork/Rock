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
    public partial class CommunicationJob : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
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
        ,'Send Communications'
        ,'Job to send any future communications or communications not sent immediately by Rock.'
        ,'Rock.Jobs.SendCommunications'
        ,'0 0/10 * 1/1 * ? *'
        ,3
        ,'F1FB3E17-93A6-42E5-BE1A-8FA3AAD2B3AB')
");

            AddEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.SendCommunications", "Delay Period", "",
                "The number of minutes to wait before sending any new communication (If the communication block's 'Send When Approved' option is turned on, then a delay should be used here to prevent a send overlap).",
                0, "30", "321805C2-7737-4025-8CCA-90D6CB875CB4" );

            AddEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.SendCommunications", "Expiration Period", "",
                "The number of days after a communication was created or scheduled to be sent when it should no longer be sent.",
                1, "3", "6431DB53-93E0-468F-8555-5E054BDC5829" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
