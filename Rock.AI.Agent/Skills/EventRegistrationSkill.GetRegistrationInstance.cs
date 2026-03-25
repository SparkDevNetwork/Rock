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

        [Description( "Retrieves the details of a event registration instance." )]
        [AgentPurpose( "Retrieves the details of a event registration instance" )]
        [AgentToolGuid( "fd59ad44-cc8a-4e38-93bc-80f0de0d4758" )]
        public IAgentToolResult GetRegistrationInstance( string registrationInstanceIdKey )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );

            var registrationInstance = helper.GetRequiredEntity<RegistrationInstance>( registrationInstanceIdKey, checkSecurity: true );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            var result = new RegistrationInstanceResult
            {
                Id = registrationInstance.Id,
                Name = registrationInstance.Name,
                RegistrationTemplate = new RegistrationTemplateResult
                {
                    Id = registrationInstance.RegistrationTemplate.Id,
                    Name = registrationInstance.RegistrationTemplate.Name,
                },
                StartDateTime = registrationInstance.StartDateTime,
                EndDateTime = registrationInstance.EndDateTime,
                ContactEmail = registrationInstance.ContactEmail,
                ContactPerson = PersonResult.NameOnly( registrationInstance.ContactPersonAlias ),
                ContactPhoneNumber = registrationInstance.ContactPhone,
                MaximumAttendees = registrationInstance.MaxAttendees,
                PaymentAccount = registrationInstance.Account != null
                    ? new FinancialAccountResult
                    {
                        Id = registrationInstance.Account.Id,
                        Name = registrationInstance.Account.Name,
                    }
                    : null,
            };

            return Success( result );
        }

        #endregion
    }
}
