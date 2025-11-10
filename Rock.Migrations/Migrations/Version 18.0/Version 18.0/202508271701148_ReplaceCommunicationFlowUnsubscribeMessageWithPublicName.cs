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
    public partial class ReplaceCommunicationFlowUnsubscribeMessageWithPublicName : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.CommunicationFlow", "PublicName", c => c.String( maxLength: 500 ) );
            MigrateUnsubscribeMessageToPublicNameUp();
            DropColumn( "dbo.CommunicationFlow", "UnsubscribeMessage" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn( "dbo.CommunicationFlow", "UnsubscribeMessage", c => c.String( maxLength: 500 ) );
            MigrateUnsubscribeMessageToPublicNameDown();
            DropColumn( "dbo.CommunicationFlow", "PublicName" );
        }

        private void MigrateUnsubscribeMessageToPublicNameUp()
        {
            Sql( @"UPDATE [CommunicationFlow]
SET [PublicName] = 
    CASE 
        WHEN [UnsubscribeMessage] LIKE 'Only unsubscribe from the % flow.'
             AND LTRIM(RTRIM(
                    SUBSTRING(
                        [UnsubscribeMessage],
                        LEN('Only unsubscribe from the ') + 1,
                        LEN([UnsubscribeMessage]) 
                          - LEN('Only unsubscribe from the ') 
                          - LEN(' flow.')
                    )
                )) <> [Name]
             AND LTRIM(RTRIM(
                    SUBSTRING(
                        [UnsubscribeMessage],
                        LEN('Only unsubscribe from the ') + 1,
                        LEN([UnsubscribeMessage]) 
                          - LEN('Only unsubscribe from the ') 
                          - LEN(' flow.')
                    )
                )) <> ''
        THEN LTRIM(RTRIM(
            SUBSTRING(
                [UnsubscribeMessage],
                LEN('Only unsubscribe from the ') + 1,
                LEN([UnsubscribeMessage]) 
                  - LEN('Only unsubscribe from the ') 
                  - LEN(' flow.')
            )
        ))
        ELSE NULL
    END" );
        }

        private void MigrateUnsubscribeMessageToPublicNameDown()
        {
            Sql( @"UPDATE [CommunicationFlow]
SET [UnsubscribeMessage] =
    CASE 
        WHEN [PublicName] IS NOT NULL AND LTRIM(RTRIM([PublicName])) <> ''
            THEN 'Only unsubscribe from the ' + [PublicName] + ' flow.'
        WHEN [Name] IS NOT NULL AND LTRIM(RTRIM([Name])) <> ''
            THEN 'Only unsubscribe from the ' + [Name] + ' flow.'
        ELSE NULL
    END" );
        }
    }
}
