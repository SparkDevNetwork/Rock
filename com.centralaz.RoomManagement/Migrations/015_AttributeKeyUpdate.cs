// <copyright>
// Copyright by the Central Christian Church
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
using Rock.Plugin;

namespace com.centralaz.RoomManagement.Migrations
{
    [MigrationNumber( 15, "1.6.0" )]
    public class AttributeKeyUpdate : Migration
    {
        public override void Up()
        {
            Sql( @"
                UPDATE [a]
                SET [a].[Key] = [innerTable].[NewKey]
                FROM [Attribute] [a] 
                INNER JOIN
                (
	                SELECT 'Q'+CAST([a].[Order] AS NVARCHAR(20))+'_ResourceId'+CAST([q].[ResourceId] AS NVARCHAR(20)) AS 'NewKey',
	                [a].[Id] AS 'AttributeId'
	                FROM [Attribute] [a]
	                JOIN [_com_centralaz_RoomManagement_Question] [q] ON [a].[Id] = [q].[AttributeId]
	                WHERE [q].[ResourceId] IS NOT NULL
	                AND [a].[Key] NOT LIKE '%_ResourceId%'
                ) AS [innerTable]
                ON [innerTable].[AttributeId] = [a].[Id]

                UPDATE [a]
                SET [a].[Key] = [innerTable].[NewKey]
                FROM [Attribute] [a] 
                INNER JOIN
                (
	                SELECT 'Q'+CAST([a].[Order] AS NVARCHAR(20))+'_LocationId'+CAST([q].[LocationId] AS NVARCHAR(20)) AS 'NewKey',
	                [a].[Id] AS 'AttributeId'
	                FROM [Attribute] [a]
	                JOIN [_com_centralaz_RoomManagement_Question] [q] ON [a].[Id] = [q].[AttributeId]
	                WHERE [q].[LocationId] IS NOT NULL
	                AND [a].[Key] NOT LIKE '%_LocationId%'
                ) AS [innerTable]
                ON [innerTable].[AttributeId] = [a].[Id]
                " );
        }
        public override void Down()
        {

        }
    }
}
