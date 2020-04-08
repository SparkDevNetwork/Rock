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
    public partial class PersonGivingLeaderIdUpdate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreatePersonGivingLeaderIdPersistedIndexed();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
IF EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'IX_GivingLeaderId'
			AND object_id = OBJECT_ID('Person')
		)
BEGIN
	DROP INDEX IX_GivingLeaderId ON [Person]
END

ALTER TABLE Person
DROP COLUMN GivingLeaderId
" );

            Sql(@"

ALTER TABLE Person ADD [GivingLeaderId] AS ([dbo].[ufnFinancial_GetGivingLeader]([Id], [GivingGroupId]))
" );
        }

        /// <summary>
        /// Re-creates the person GivingLeaderID column as a computed-persisted column and adds an index on it
        /// </summary>
        private void CreatePersonGivingLeaderIdPersistedIndexed()
        {
            Sql( @"IF EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'IX_GivingLeaderId'
			AND object_id = OBJECT_ID('Person')
		)
BEGIN
	DROP INDEX IX_GivingLeaderId ON [Person]
END

ALTER TABLE Person
DROP COLUMN GivingLeaderId" );


            Sql( @"
ALTER TABLE Person ADD GivingLeaderId INT NULL

CREATE INDEX IX_GivingLeaderId ON [Person] ([GivingLeaderId])
" );

            Sql( @"UPDATE x
SET x.GivingLeaderId = x.CalculatedGivingLeaderId
FROM (
	SELECT p.Id
		,p.NickName
		,p.LastName
		,p.GivingLeaderId
		,isnull(pf.CalculatedGivingLeaderId, p.Id) CalculatedGivingLeaderId
	FROM Person p
	OUTER APPLY (
		SELECT TOP 1 p2.[Id] CalculatedGivingLeaderId
		FROM [GroupMember] gm
		INNER JOIN [GroupTypeRole] r ON r.[Id] = gm.[GroupRoleId]
		INNER JOIN [Person] p2 ON p2.[Id] = gm.[PersonId]
		WHERE gm.[GroupId] = p.GivingGroupId
			AND p2.[IsDeceased] = 0
			AND p2.[GivingGroupId] = p.GivingGroupId
		ORDER BY r.[Order]
			,p2.[Gender]
			,p2.[BirthYear]
			,p2.[BirthMonth]
			,p2.[BirthDay]
		) pf
	WHERE (
			p.GivingLeaderId IS NULL
			OR (p.GivingLeaderId != pf.CalculatedGivingLeaderId)
			)
	) x" );
        }
    }
}
