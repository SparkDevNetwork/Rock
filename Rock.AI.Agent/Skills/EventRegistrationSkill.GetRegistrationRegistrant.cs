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
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.EventRegistrationSkill;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class EventRegistrationSkill
    {
        #region Tool(s)

        [Description( "Retrieves the details of a event registration registrant." )]
        [AgentPurpose( "Retrieves the details of a event registration registrant." )]
        [AgentToolGuid( "ecca3bad-e440-4743-a20f-4013350e3520" )]
        public IAgentToolResult GetRegistrationRegistrant( string registrationRegistrantIdKey )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );

            var registrationRegistrant = helper.GetRequiredEntity<RegistrationRegistrant>( registrationRegistrantIdKey, checkSecurity: true );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            registrationRegistrant.LoadAttributes( AgentRequestContext.RockContext );

            var result = new RegistrationRegistrantResult
            {
                Id = registrationRegistrant.Id,
                Person = PersonResult.NameOnly( registrationRegistrant.PersonAlias ),
                RegistrationInstance = new RegistrationInstanceResult
                {
                    Id = registrationRegistrant.Registration.RegistrationInstance.Id,
                    Name = registrationRegistrant.Registration.RegistrationInstance.Name,
                },
                RegisteredDateTime = registrationRegistrant.CreatedDateTime,
                BaseRegistrationCost = registrationRegistrant.Cost,
                IsOnWaitList = registrationRegistrant.OnWaitList,
                RegisteredBy = registrationRegistrant.Registration.PersonAlias != null
                    ? PersonResult.NameOnly( registrationRegistrant.Registration.PersonAlias )
                    : new PersonResult
                    {
                        FirstName = registrationRegistrant.Registration.FirstName,
                        LastName = registrationRegistrant.Registration.LastName,
                    },
                AttributeValues = registrationRegistrant.GetAttributeValueResults( AgentRequestContext ).ToList(),
            };

            return Success( result );
        }

        #endregion
    }
}
