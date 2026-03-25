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
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class FinanceSkill
{
    #region Tool(s)

    [Description( "Retrieves all configured benevolence types in Rock." )]
    [AgentPurpose( "Retrieves all configured benevolence types in Rock." )]
    [AgentToolGuid( "a7a059f6-08a2-4032-a91d-a787d1857752" )]
    public IAgentToolResult LookupBenevolenceTypes()
    {
        var benevolenceTypeResults = GetConfiguredBenevolenceTypes()
            .Select( bt => new KeyNameResult( bt.Id, bt.Name ) )
            .OrderBy( kn => kn.Name )
            .ToList();

        var result = Success( benevolenceTypeResults );

        if ( benevolenceTypeResults.Count > 50 )
        {
            result = result.WithoutHistoryContent();
        }

        return result;
    }

    #endregion
}
