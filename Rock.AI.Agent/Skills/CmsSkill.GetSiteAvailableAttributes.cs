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
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CmsSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets the attributes that can be set on a site.
    /// </summary>
    [Description( "Gets the attributes that can be set when updating a site." )]
    [AgentPurpose( "Determines which attribute values UpdateSite accepts." )]
    [AgentToolPrerequisite( "Call LookupSites to determine the siteIdKey." )]
    [AgentToolGuid( "C73B7403-1A23-41CE-A94A-C37242062BF6" )]
    public AgentToolResult GetSiteAvailableAttributes( string siteIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var site = helper.GetRequiredEntity<Model.Site>( siteIdKey, checkSecurity: true );

        if ( site == null )
        {
            return helper.ErrorResult
                .WithInstructions( "Call the LookupSites function to determine the available sites." );
        }

        return Success( helper.GetAvailableAttributes( site ) );
    }

    #endregion
}
