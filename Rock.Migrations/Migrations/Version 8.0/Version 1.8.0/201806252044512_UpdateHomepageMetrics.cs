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
    public partial class UpdateHomepageMetrics : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"UPDATE [dbo].[MetricValue]
SET [YValue] = (SELECT COUNT(*) 
FROM [Person]
WHERE ([RecordTypeValueId] = 1) AND ([RecordStatusValueId] = 3)),
[MetricValueDateTime] = '" + RockDateTime.Now.ToString( "s" ) +  @"'
WHERE [Guid] = '34325795-9016-47e9-a9d9-6283d1a84275'" ); // Active Records

            Sql( @"UPDATE [dbo].[MetricValue]
SET [YValue] = (SELECT COUNT( DISTINCT(g.[Id])) 
FROM [Person] p
    INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id]
    INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '790e3215-3b10-442b-af69-616c0dcb998e')
WHERE ([RecordTypeValueId] = 1) AND ([RecordStatusValueId] = 3)),
[MetricValueDateTime] = '" + RockDateTime.Now.ToString( "s" ) + @"'
WHERE [Guid] = '932479dd-9612-4d07-b9cd-9227976cf5dd'" ); //Active Families

            Sql( @"UPDATE [dbo].[MetricValue]
SET [YValue] = (SELECT COUNT(*) 
FROM [ConnectionRequest]
WHERE [ConnectionState] = 0),
[MetricValueDateTime] = '" + RockDateTime.Now.ToString( "s" ) + @"'
WHERE [Guid] = '90cd5a83-3079-4656-b7ce-bfa21055c980'" ); // Active Connection Requests
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
