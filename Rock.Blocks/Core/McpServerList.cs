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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Mcp;
using Rock.Attribute;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Enums.Security;
using Rock.Model;
using Rock.ViewModels.Blocks.Core.McpServerList;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of MCP Servers.
    /// </summary>
    [DisplayName( "MCP Server List" )]
    [Category( "Core" )]
    [Description( "Displays a list of MCP Servers." )]
    [IconCssClass( "ti ti-robot" )]
    [SupportedSiteTypes( SiteType.Web )]

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Primary )]
    [Rock.SystemGuid.EntityTypeGuid( "F0B14291-8035-4986-A4D8-DC1AE08E4F7B" )]
    [Rock.SystemGuid.BlockTypeGuid( "54B23A63-87C0-4955-B915-C91F23C36D48" )]
    public class McpServerList : RockBlockType
    {
        #region Methods

        public override object GetObsidianBlockInitialization()
        {
            var box = new InitializationBox
            {
                Items = GetMcpServers()
            };

            return box;
        }

        private List<McpServerListItemBag> GetMcpServers()
        {
            var mcpAiAgents = AIAgentCache.All()
                .Where( a => a.AgentType == AgentType.Mcp )
                .OrderBy( a => a.Id )
                .Select( a => new
                {
                    AiAgent = a,
                    McpAgentSettings = a.GetAdditionalSettings<McpAgentSettings>()
                } )
                .Select( x => new
                {
                    x.McpAgentSettings.Slug,
                    x.AiAgent.Name,
                    x.AiAgent.Description,
                    x.AiAgent.AudienceType
                } )
                .ToList();

            if ( !mcpAiAgents.Any() )
            {
                // Exit early so we don't create an API Key.
                return new List<McpServerListItemBag>();
            }

            var publicApplicationRoot = GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ).RemoveTrailingForwardslash();

            // Create an API Key on block load instead of waiting for the individual to click the Copy URL button.
            // Doing so here will place the sensitive API Keys in the page's HTML.
            // If done in a block action, the API Key would be included in the API response which could be logged and
            // would be more easily accessible to users inspecting network requests,
            // but including it in the page's HTML means it is less likely to be accidentally exposed in logs and is not included in API responses.
            var apiKey = GetOrCreateMcpApiKeyForCurrentPerson();

            return mcpAiAgents
                .Select( aa => new McpServerListItemBag
                {
                    AudienceType = aa.AudienceType,
                    Name = aa.Name,
                    Description = aa.Description,
                    PartialUrl = $"{publicApplicationRoot}/api/v2/mcp/{aa.Slug}...",
                    FullUrl = $"{publicApplicationRoot}/api/v2/mcp/{aa.Slug}?apikey={apiKey}",
                } )
                .ToList();
        }

        private string GetOrCreateMcpApiKeyForCurrentPerson()
        {
            var currentPerson = GetCurrentPerson();

            // Get this person's single MCP API Key, if it exists.
            // If multiple exist for some reason, just grab the first one.
            // This API Key will be included in the generated MCP Server URL
            // so the AI agent can use it to authenticate API requests from the client back to the server.
            var apiKey = currentPerson
                .Users
                .Where( ul => ul.ApiKeyPurpose == ApiKeyPurpose.Mcp && ul.ApiKey.IsNotNullOrWhiteSpace() )
                .Select( ul => ul.ApiKey )
                .FirstOrDefault();

            if ( apiKey.IsNullOrWhiteSpace() )
            {
                // Generate a new UserLogin API Key record since it hasn't been created yet.
                apiKey = Rock.Utility.KeyHelper.GenerateKey( ( RockContext rockContext, string key ) =>
                {
                    // Only compare ApiKey here so the value is unique across all UserLogin records, regardless of Person or Purpose.
                    // The ApiKey can be used by itself to authenticate Rest API requests.
                    // It would be an issue if multiple people had UserLogin records with the same ApiKey,
                    // because API requests that included that ApiKey could potentially authenticate as any of those people,
                    // and it would be unpredictable which one it would authenticate as.
                    return new UserLoginService( rockContext ).Queryable().Any( a => a.ApiKey == key );
                } );

                // The ApiKey UserLogin will be saved with the Database authentication Entity Type
                // to follow the pattern of how API Keys are created for other rest client authentication types.
                var entityType = new EntityTypeService( RockContext )
                    .Get( "Rock.Security.Authentication.Database" );

                var userLoginService = new UserLoginService( RockContext );
                userLoginService.Add( new UserLogin
                {
                    UserName = Guid.NewGuid().ToString(),
                    IsConfirmed = true,
                    PersonId = currentPerson.Id,
                    EntityTypeId = entityType.Id,
                    ApiKey = apiKey,
                    ApiKeyPurpose = ApiKeyPurpose.Mcp
                } );
                RockContext.SaveChanges();

                // Just in case we hit a race condition and another API Key was created for this user and purpose between when we checked and when we tried to create,
                // delete the one we just created and use the existing one instead.
                var existingApiKey = userLoginService.Queryable()
                    .Where( ul => ul.PersonId == currentPerson.Id && ul.ApiKeyPurpose == ApiKeyPurpose.Mcp && ul.ApiKey != apiKey )
                    .ToList()
                    .Where( ul => ul.ApiKey.IsNotNullOrWhiteSpace() )
                    .OrderBy( ul => ul.CreatedDateTime )
                    .Select( ul => ul.ApiKey )
                    .FirstOrDefault();

                if ( existingApiKey.IsNotNullOrWhiteSpace() )
                {
                    var apiKeysToDelete = userLoginService.Queryable()
                        .Where( ul =>
                            ul.PersonId == currentPerson.Id
                            && ul.ApiKeyPurpose == ApiKeyPurpose.Mcp
                            && ul.ApiKey == apiKey )
                        .ToList();
                    userLoginService.DeleteRange( apiKeysToDelete );
                    RockContext.SaveChanges();

                    // Get the existing API Key that was created by another request.
                    apiKey = existingApiKey;
                }
            }

            return apiKey;
        }
        
        #endregion
    }
}
