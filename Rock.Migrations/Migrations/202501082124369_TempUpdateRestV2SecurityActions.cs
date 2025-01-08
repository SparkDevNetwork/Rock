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
    /// <summary>
    /// Convert existing security actions to the new v2 API action terms.
    /// </summary>
    public partial class TempUpdateRestV2SecurityActions : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
UPDATE [A]
    SET [A].[Action] = CASE
        WHEN [A].[Action] = 'View' THEN 'ExecuteRead'
        WHEN [A].[Action] = 'Edit' THEN 'ExecuteWrite'
        ELSE [A].[Action]
    END
FROM [Auth] AS [A]
INNER JOIN [EntityType] AS [ET] On [ET].[Id] = [A].[EntityTypeId]
LEFT OUTER JOIN [RestController] AS [RC] ON [RC].[Id] = [A].[EntityId]
WHERE [ET].[Name] = 'Rock.Model.RestController'
  AND [RC].[ClassName] LIKE 'Rock.Rest.v2.%'

UPDATE [A]
    SET [A].[Action] = CASE
        WHEN [A].[Action] = 'FullSearch' THEN 'ExecuteUnrestrictedRead'
        WHEN [A].[Action] = 'Edit' THEN 'ExecuteRead'
        ELSE [A].[Action]
    END
FROM [Auth] AS [A]
INNER JOIN [EntityType] AS [ET] On [ET].[Id] = [A].[EntityTypeId]
LEFT OUTER JOIN [RestAction] AS [RA] ON [RA].[Id] = [A].[EntityId]
WHERE [ET].[Name] = 'Rock.Model.RestAction'
  AND [RA].[Guid] = 'dca338b6-9749-427e-8238-1686c9587d16'

UPDATE [A]
    SET [A].[Action] = CASE
        WHEN [A].[Action] = 'View' THEN 'ExecuteRead'
        WHEN [A].[Action] = 'Edit' THEN 'ExecuteWrite'
        ELSE [A].[Action]
    END
FROM [Auth] AS [A]
INNER JOIN [EntityType] AS [ET] On [ET].[Id] = [A].[EntityTypeId]
LEFT OUTER JOIN [RestAction] AS [RA] ON [RA].[Id] = [A].[EntityId]
LEFT OUTER JOIN [RestController] AS [RC] ON [RC].[Id] = [RA].[ControllerId]
WHERE [ET].[Name] = 'Rock.Model.RestAction'
  AND [RC].[ClassName] LIKE 'Rock.Rest.v2.%'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
UPDATE [A]
    SET [A].[Action] = CASE
        WHEN [A].[Action] = 'ExecuteRead' THEN 'View'
        WHEN [A].[Action] = 'ExecuteWrite' THEN 'Edit'
        ELSE [A].[Action]
    END
FROM [Auth] AS [A]
INNER JOIN [EntityType] AS [ET] On [ET].[Id] = [A].[EntityTypeId]
LEFT OUTER JOIN [RestController] AS [RC] ON [RC].[Id] = [A].[EntityId]
WHERE [ET].[Name] = 'Rock.Model.RestController'
  AND [RC].[ClassName] LIKE 'Rock.Rest.v2.%'

UPDATE [A]
    SET [A].[Action] = CASE
        WHEN [A].[Action] = 'ExecuteUnrestrictedRead' THEN 'FullSearch'
        WHEN [A].[Action] = 'ExecuteRead' THEN 'Edit'
        ELSE [A].[Action]
    END
FROM [Auth] AS [A]
INNER JOIN [EntityType] AS [ET] On [ET].[Id] = [A].[EntityTypeId]
LEFT OUTER JOIN [RestAction] AS [RA] ON [RA].[Id] = [A].[EntityId]
WHERE [ET].[Name] = 'Rock.Model.RestAction'
  AND [RA].[Guid] = 'dca338b6-9749-427e-8238-1686c9587d16'

UPDATE [A]
    SET [A].[Action] = CASE
        WHEN [A].[Action] = 'View' THEN 'ExecuteRead'
        WHEN [A].[Action] = 'Edit' THEN 'ExecuteWrite'
        ELSE [A].[Action]
    END
FROM [Auth] AS [A]
INNER JOIN [EntityType] AS [ET] On [ET].[Id] = [A].[EntityTypeId]
LEFT OUTER JOIN [RestAction] AS [RA] ON [RA].[Id] = [A].[EntityId]
LEFT OUTER JOIN [RestController] AS [RC] ON [RC].[Id] = [RA].[ControllerId]
WHERE [ET].[Name] = 'Rock.Model.RestAction'
  AND [RC].[ClassName] LIKE 'Rock.Rest.v2.%'" );
        }
    }
}
