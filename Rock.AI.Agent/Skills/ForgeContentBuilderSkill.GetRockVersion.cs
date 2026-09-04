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
using Rock.AI.Agent.Classes.Skills.ForgeContentBuilderSkill;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class ForgeContentBuilderSkill
{
    #region Tool(s)

    [Description( "Reports the Rock version this instance is running, so control and API lookups can be scoped to the release actually deployed here." )]
    [AgentToolPreamble( "Checking the Rock version." )]
    [AgentUsage( "Call this before looking up any control, filter, or API in the Rock knowledge base. The knowledge base tools scope lookups to the connected Rock version automatically, so use this result to interpret compatibility and coverage rather than trying to pass an unsupported version argument." )]
    [AgentUsage( "This is the version of the Rock instance you are connected to. It is not the newest Rock release, and it is not the version any documentation defaults to." )]
    [AgentToolGuid( "8A51C3E9-674D-4B02-93F8-2E6B9D40A715" )]
    public AgentToolResult GetRockVersion()
    {
        /*
            8/27/2026 - CLAUDE

            The Coding Guide pointer rides this result because this is the one
            tool every authoring session calls first, and tool results always land
            in the client's context. Seeded instructions do not reach third-party
            MCP clients, so a client that never saw them still gets steered to the
            routing article before it authors any UI.

            Reason: Deliver the Coding Guide pointer on a channel that survives instruction drift.
        */

        // No authorization gate: the version is already visible to anonymous
        // visitors in page markup and asset fingerprints, so this exposes nothing
        // that is not public, and every control lookup depends on it.
        return Success( new RockVersionResult
        {
            Version = Rock.VersionInfo.VersionInfo.GetRockSemanticVersionNumber(),
            FullVersion = Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber()
        } )
            .WithInstructions( "Before authoring component UI, call the Community Knowledge Base skill's GetKnowledgeBaseOverview tool and locate the Rock Coding Guide topic. Pass the returned topic key unchanged to GetTopic, open the root article listed by that topic, and follow the guide's own routing for the requested outcome. Retrieve only the material assigned by the selected Playbook. Never construct or guess a topic or article key." );
    }

    #endregion
}
