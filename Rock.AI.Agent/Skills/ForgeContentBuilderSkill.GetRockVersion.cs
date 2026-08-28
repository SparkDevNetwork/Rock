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
    [AgentUsage( "Call this before looking up any control, filter, or API in the Rock knowledge base, and pass the returned version to that lookup. Control APIs change between releases, so an unscoped lookup can describe props this instance does not have." )]
    [AgentUsage( "This is the version of the Rock instance you are connected to. It is not the newest Rock release, and it is not the version any documentation defaults to." )]
    [AgentToolGuid( "8A51C3E9-674D-4B02-93F8-2E6B9D40A715" )]
    public AgentToolResult GetRockVersion()
    {
        /*
            8/27/2026 - CLAUDE

            The Composition Rules pointer rides this result because this is the one
            tool every authoring session calls first, and tool results always land
            in the client's context. Seeded instructions do not reach third-party
            MCP clients, so a client that never saw them still gets steered to the
            article before it authors any UI.

            Reason: Deliver the composition-rules pointer on a channel that survives instruction drift.
        */

        // No authorization gate: the version is already visible to anonymous
        // visitors in page markup and asset fingerprints, so this exposes nothing
        // that is not public, and every control lookup depends on it.
        return Success( new RockVersionResult
        {
            Version = Rock.VersionInfo.VersionInfo.GetRockSemanticVersionNumber(),
            FullVersion = Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber()
        } )
            .WithInstructions( "Before authoring any component UI, read the Composition Rules article: call the Community Knowledge Base skill's GetArticle tool with articleKey 'coding-guide/conventions-and-guardrails/composition-rules'. It governs which control to use, in which mode, composed how. When the design matches one of the guide's recipes, also read that recipe and follow its Composition table; a recipe never overrides the rules." );
    }

    #endregion
}
