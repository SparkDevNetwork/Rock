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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Moves the default security rules seeded on the v2 Check-in and Controls REST controllers behind any rule an
    /// administrator added to the same controller and action.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 348, "17.13" )]
    public class RepairV2ControllerSeededAuthOrder : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
                11/16/26 - JPH

                The 17.0 migration seeded these rules ahead of rules an administrator had already set on the
                controllers, so the seeded grant won over the administrator's restriction. Only rules older than the
                seeded rows (a lower Id) count, since rules added later through the Security dialog already land
                after them. A controller whose seeded rows were re-ordered by hand (ModifiedDateTime is set) is left
                alone. Older rules keep their place; the seeded rows, and any rule added after them, move below the
                older rules in the same order. Nothing is deleted, and a rerun matches nothing because the repaired
                seeded rows have a ModifiedDateTime.

                Reason: Keep administrator security rules ahead of the seeded defaults on the v2 controllers.
            */
            Sql( @"
DECLARE @RestControllerEntityTypeId INT = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.RestController' );

DECLARE @Seeded TABLE ( [Guid] UNIQUEIDENTIFIER PRIMARY KEY );

/* Check-in controller. */
INSERT INTO @Seeded ( [Guid] ) VALUES
    ( '7ed56e29-b07c-46d7-b7ad-1d2b54d6fd8a' ),
    ( '98b917ed-0d9a-4d0e-bd5d-11b95e96c90f' ),
    ( '06787034-6808-4b29-983f-8ba30b3773c2' ),
    ( '183659f2-ac00-420d-8813-6e3c59c59a9c' ),
    ( '5fccefae-cb5a-48a2-ab50-5f0901df47ca' ),
    ( 'b0be618d-fb3f-4890-8af5-734b27e7bc5d' ),
    ( '5880ef39-f135-4760-a69f-9568edd8f2f1' ),
    ( '735acdc1-1922-484c-baf7-3e8b53abfdcf' );

/* Controls controller. */
INSERT INTO @Seeded ( [Guid] ) VALUES
    ( '63b1a279-5637-4133-89c1-5b51d2a28155' ),
    ( '0b7c8e65-1094-4032-a49d-d42209a62210' ),
    ( '951f95b7-81bc-4cd0-95b1-8e0438a59c96' ),
    ( '8ed5b612-4a31-4167-8bf5-79b666a34aef' ),
    ( '6d303c10-ef8f-4901-8b85-334f4ccec44d' ),
    ( '313bccf7-fbba-411a-96bc-52814703f612' ),
    ( '72ec0496-1066-407e-8d96-40490af6ddfb' ),
    ( '539f0406-fcad-4f1c-b9b8-c9e24f3ee06a' );

/* Each controller and action whose seeded rows were never re-ordered by hand, with its oldest seeded row. */
WITH [SeededGroups] AS (
    SELECT a.[EntityId]
        , a.[Action]
        , MIN( a.[Id] ) AS [FirstSeededId]
    FROM [Auth] AS a
    INNER JOIN @Seeded AS s ON s.[Guid] = a.[Guid]
    WHERE a.[EntityTypeId] = @RestControllerEntityTypeId
    GROUP BY a.[EntityId], a.[Action]
    HAVING COUNT( a.[ModifiedDateTime] ) = 0
)
/* The seeded row that Rock reads first in each of those groups (lowest Order, then Id). */
, [FirstSeededRows] AS (
    SELECT a.[EntityId]
        , a.[Action]
        , a.[Id]
        , a.[Order]
        , ROW_NUMBER() OVER ( PARTITION BY a.[EntityId], a.[Action] ORDER BY a.[Order], a.[Id] ) AS [RowNumber]
    FROM [Auth] AS a
    INNER JOIN @Seeded AS s ON s.[Guid] = a.[Guid]
    WHERE a.[EntityTypeId] = @RestControllerEntityTypeId
)
/* Rules that existed before the seeded rows were added. */
, [OlderRules] AS (
    SELECT o.[EntityId]
        , o.[Action]
        , o.[Id]
        , o.[Order]
    FROM [Auth] AS o
    INNER JOIN [SeededGroups] AS g ON g.[EntityId] = o.[EntityId] AND g.[Action] = o.[Action]
    WHERE o.[EntityTypeId] = @RestControllerEntityTypeId
        AND o.[Id] < g.[FirstSeededId]
)
/* Groups where an older rule sorts after the first seeded row, with the highest Order of the older rules. */
, [GroupsToRepair] AS (
    SELECT g.[EntityId]
        , g.[Action]
        , g.[FirstSeededId]
        , ( SELECT MAX( o.[Order] )
            FROM [OlderRules] AS o
            WHERE o.[EntityId] = g.[EntityId]
                AND o.[Action] = g.[Action] ) AS [MaxOlderOrder]
    FROM [SeededGroups] AS g
    INNER JOIN [FirstSeededRows] AS f ON f.[EntityId] = g.[EntityId] AND f.[Action] = g.[Action] AND f.[RowNumber] = 1
    WHERE EXISTS (
        SELECT 1
        FROM [OlderRules] AS o
        WHERE o.[EntityId] = g.[EntityId]
            AND o.[Action] = g.[Action]
            AND ( o.[Order] > f.[Order] OR ( o.[Order] = f.[Order] AND o.[Id] > f.[Id] ) ) )
)
/* The seeded rows and any rule added after them, numbered in their current order, placed after the older rules. */
, [RulesToMove] AS (
    SELECT a.[Id]
        , g.[MaxOlderOrder] + ROW_NUMBER() OVER ( PARTITION BY a.[EntityId], a.[Action] ORDER BY a.[Order], a.[Id] ) AS [NewOrder]
    FROM [Auth] AS a
    INNER JOIN [GroupsToRepair] AS g ON g.[EntityId] = a.[EntityId] AND g.[Action] = a.[Action]
    WHERE a.[EntityTypeId] = @RestControllerEntityTypeId
        AND a.[Id] >= g.[FirstSeededId]
)
UPDATE a
SET a.[Order] = m.[NewOrder]
    , a.[ModifiedDateTime] = GETDATE()
FROM [Auth] AS a
INNER JOIN [RulesToMove] AS m ON m.[Id] = a.[Id];" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
