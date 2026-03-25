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
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal partial class SystemUtilitySkill
    {
        #region Tool

        [Description( "Gets minimal information about the person currently logged in and interacting with the agent." )]
        [AgentPurpose( "Gets minimal information about the user/person currently logged in." )]
        [AgentToolGuid( "cb9f23f1-3d21-4451-80c3-4efbd18a7fbc" )]
        public IAgentToolResult GetCurrentPerson()
        {
            var currentPerson = AgentRequestContext.CurrentPerson;

            if ( currentPerson == null )
            {
                return Error( "There is no person currently logged in." );
            }

            var result = PersonResult.Basic( currentPerson );
            var historyResult = PersonResult.NameOnly( currentPerson );

            result.BirthDay = currentPerson.BirthDay;
            result.BirthMonth = currentPerson.BirthMonth;
            result.BirthYear = currentPerson.BirthYear;
            result.Email = currentPerson.Email.IfEmpty( null );

            return Success( result )
                .WithHistoryContent( historyResult );
        }

        #endregion
    }
}
