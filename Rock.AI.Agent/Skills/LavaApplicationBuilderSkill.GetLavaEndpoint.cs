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

using System.ComponentModel;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Skills.LavaApplicationBuilderSkill;
using Rock.Cms;
using Rock.Configuration;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class LavaApplicationBuilderSkill
{
    #region Tool(s)

    [Description( "Reads the current template and configuration of a Lava endpoint so it can be iterated on." )]
    [AgentToolPreamble( "Reading the Lava endpoint." )]
    [AgentUsage( "Read the endpoint before changing it, so an AddOrUpdateLavaEndpoint call replaces the template you expect. Endpoints are keyed by slug AND method." )]
    [AgentToolGuid( "E64B9F07-2C58-41DA-A83F-05D9C7B24E61" )]
    public AgentToolResult GetLavaEndpoint(
        [Description( "The slug of the Lava application the endpoint belongs to." )]
        string applicationSlug,

        [Description( "The slug of the endpoint to read." )]
        string endpointSlug,

        [Description( "The HTTP method of the endpoint. Defaults to Post." )]
        string httpMethod = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        var endpoint = GetAuthorizedEndpoint( helper, rockContext, applicationSlug, endpointSlug, httpMethod );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var url = GetEndpointUrl( endpoint.LavaApplication.Slug, endpoint.Slug );

        // The template can be large, so only the endpoint's identity is kept
        // in session history.
        return Success( new LavaEndpointDetailResult
        {
            ApplicationSlug = endpoint.LavaApplication.Slug,
            EndpointSlug = endpoint.Slug,
            Method = endpoint.HttpMethod.ToString(),
            Name = endpoint.Name,
            IsActive = endpoint.IsActive,
            CodeTemplate = endpoint.CodeTemplate,
            EnabledLavaCommands = endpoint.EnabledLavaCommands,
            SecurityMode = endpoint.SecurityMode.ToString(),
            ContentType = endpoint.GetAdditionalSettings<LavaEndpointAdditionalSettings>()?.ContentType,
            Url = url
        } )
            .WithHistoryContent( new LavaEndpointReferenceResult
            {
                ApplicationSlug = endpoint.LavaApplication.Slug,
                EndpointSlug = endpoint.Slug,
                Method = endpoint.HttpMethod.ToString(),
                Url = url
            }, "lava-endpoint" );
    }

    #endregion
}
