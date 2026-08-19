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

using Rock.AI.Agent.Classes.Skills.ContentChannelSkill;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class ContentChannelSkill
{
    #region Tool(s)

    [Description( "Retrieves all configured content channel types in Rock." )]
    [AgentToolGuid( "2a28de77-675b-4a7a-b38e-6a22d148b2b0" )]
    public AgentToolResult LookupContentChannelTypes()
    {
        var contentChannelTypeResults = ContentChannelTypeCache.All( AgentRequestContext.RockContext )
            .Where( cct => cct.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) )
            .Select( cct => new ContentChannelTypeResult
            {
                Id = cct.Id,
                Guid = cct.Guid,
                Name = cct.Name,
            } )
            .OrderBy( kn => kn.Name )
            .ToList();

        var result = Success( contentChannelTypeResults );

        if ( contentChannelTypeResults.Count > 50 )
        {
            result = result.WithoutHistoryContent();
        }

        return result;
    }

    #endregion
}
