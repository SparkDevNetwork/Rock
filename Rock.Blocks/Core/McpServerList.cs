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

    [BooleanField( "Append API Key to URL",
        Description = "When enabled, the individual's API key is appended to the MCP URL. Use this if the MCP server requires authentication via URL parameter rather than using OAuth. Note that API keys grant access based on the permissions of the individual they belong to — treat them as sensitive credentials and avoid sharing or exposing MCP URLs that contain them.",
        DefaultBooleanValue = false,
        Order = 0,
        Key = AttributeKey.AppendApiKeyToUrl )]

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Primary )]
    [Rock.SystemGuid.EntityTypeGuid( "F0B14291-8035-4986-A4D8-DC1AE08E4F7B" )]
    [Rock.SystemGuid.BlockTypeGuid( "54B23A63-87C0-4955-B915-C91F23C36D48" )]
    public class McpServerList : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string AppendApiKeyToUrl = "AppendApiKeyToUrl";
        }

        #endregion

        #region Methods

        public override object GetObsidianBlockInitialization()
        {
            var appendApiKeyToUrl = GetAttributeValue( AttributeKey.AppendApiKeyToUrl ).AsBoolean();

            var box = new InitializationBox
            {
                Items = GetMcpServers( appendApiKeyToUrl ),
                IsApiKeyAppendedToUrl = appendApiKeyToUrl
            };

            return box;
        }

        private List<McpServerListItemBag> GetMcpServers( bool appendApiKeyToUrl )
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

            // Only create/fetch an API Key when the block is configured to append it to the URL.
            // When appending, place the API Key in the page's HTML rather than returning it from a block action.
            // A block action would include the API Key in an API response which could be logged and
            // would be more easily accessible to users inspecting network requests,
            // but including it in the page's HTML means it is less likely to be accidentally exposed in logs and is not included in API responses.
            var apiKey = appendApiKeyToUrl
                ? Types.Mobile.Cms.VoiceAgent.GetOrCreateMcpApiKeyForCurrentPerson( GetCurrentPerson(), RockContext )
                : null;

            return mcpAiAgents
                .Select( aa => new McpServerListItemBag
                {
                    AudienceType = aa.AudienceType,
                    Name = aa.Name,
                    Description = aa.Description,
                    PartialUrl = appendApiKeyToUrl
                        ? $"{publicApplicationRoot}/api/v2/mcp/{aa.Slug}..."
                        : $"{publicApplicationRoot}/api/v2/mcp/{aa.Slug}",
                    FullUrl = appendApiKeyToUrl
                        ? $"{publicApplicationRoot}/api/v2/mcp/{aa.Slug}?apikey={apiKey}"
                        : $"{publicApplicationRoot}/api/v2/mcp/{aa.Slug}",
                } )
                .ToList();
        }

        #endregion
    }
}
