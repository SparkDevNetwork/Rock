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
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class PersonSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets the attributes that can be set on a person.
    /// </summary>
    [Description( "Gets the attributes that can be set on a person, along with any value format instructions. Attributes the current person can view but not edit are marked read only." )]
    [AgentPurpose( "Determines which attributeValues UpdatePerson accepts, and how each value should be formatted." )]
    [AgentToolPrerequisite( "Call SearchPerson to determine the personIdKey." )]
    [AgentToolGuid( "B61FA792-A62C-4FF9-8E6C-CBB4FF8288FF" )]
    public AgentToolResult GetPersonAvailableAttributes( string personIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var person = new PersonService( AgentRequestContext.RockContext ).Get( personIdKey );

        if ( person == null )
        {
            return Error( "No person could be found with the provided personIdKey." );
        }

        return Success( helper.GetAvailableAttributes( person ) );
    }

    #endregion
}
