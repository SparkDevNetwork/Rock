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
    /// Removes dynamically registered OAuth clients whose redirect URIs are not https or localhost, and limits the rest
    /// to the MCP scope so their tokens can't be used on the rest of the REST API.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 999, "17.8" )]
    public class LimitDynamicAuthClients : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RemoveDynamicAuthClientsWithInvalidRedirectUrisUp();
            LimitDynamicAuthClientsToMcpScopeUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// Deletes dynamically registered clients with any redirect URI that MCP doesn't allow. MCP clients register
        /// again when their client is no longer found.
        /// </summary>
        private void RemoveDynamicAuthClientsWithInvalidRedirectUrisUp()
        {
            Sql( @"
DELETE FROM [AuthClient]
WHERE [ClientId] LIKE 'dcr.%'
    AND (
        ISNULL([RedirectUri], '') = ''
        OR EXISTS (
            SELECT 1
            FROM STRING_SPLIT([RedirectUri], ',') AS [RedirectUris]
            WHERE [RedirectUris].[value] NOT LIKE 'https://%'
                AND [RedirectUris].[value] NOT LIKE 'http://127.%'
                AND [RedirectUris].[value] NOT LIKE 'http://localhost[:/]%'
                AND [RedirectUris].[value] <> 'http://localhost'
                AND [RedirectUris].[value] NOT LIKE 'http://[[]::1]%'
        )
    );" );
        }

        /// <summary>
        /// Turns off general REST API access for dynamically registered clients and gives those with no scopes the
        /// MCP scope.
        /// </summary>
        private void LimitDynamicAuthClientsToMcpScopeUp()
        {
            Sql( @"
UPDATE [AuthClient]
SET [AllowUserApiAccess] = 0
WHERE [ClientId] LIKE 'dcr.%';

UPDATE [AuthClient]
SET [AllowedScopes] = '[""mcp:invoke""]'
WHERE [ClientId] LIKE 'dcr.%'
    AND ([AllowedScopes] IS NULL OR [AllowedScopes] = '[]');" );
        }
    }
}
