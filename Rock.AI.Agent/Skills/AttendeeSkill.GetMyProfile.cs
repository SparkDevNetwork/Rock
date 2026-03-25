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
using Rock.AI.Agent.Classes.Entity;
using Rock.Attribute;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class AttendeeSkill
    {
        #region Tool(s)

        [Description( "Get the profile details of the currently logged in person." )]
        [AgentUsage( "Get the profile details of the currently logged in person, including contact information and family members." )]
        [AgentToolGuid( "12d052b5-50d9-46b2-866c-c773a4a145f4" )]
        public IAgentToolResult GetMyProfile()
        {
            if ( AgentRequestContext.CurrentPerson == null )
            {
                return Error( "A user must be logged in to list their profile." );
            }

            var helper = new AgentToolHelper( AgentRequestContext, _logger );
            var currentPerson = new PersonService( AgentRequestContext.RockContext ).Get( AgentRequestContext.CurrentPerson.Id );
            var editablePersonAttributeGuids = ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.ViewablePersonAttributes, string.Empty ).SplitDelimitedValues().AsGuidList();
            var availableAttributes = AttributeCache.GetMany( editablePersonAttributeGuids, AgentRequestContext.RockContext ).ToList();

            void AddAttributeValues( Model.Person person, PersonResult personResult )
            {
                Helper.LoadAttributes( person, AgentRequestContext.RockContext, availableAttributes );
                personResult.AttributeValues = person.GetAttributeValueResults( AgentRequestContext ).ToList();
            }

            var profileResult = PersonSkill.GetPrimaryPersonResult( currentPerson, publicProfile: true );
            var family = currentPerson.GetFamily();

            AddAttributeValues( currentPerson, profileResult );

            PersonSkill.PopulatePhoneNumbers( profileResult, currentPerson );
            PersonSkill.PopulateAddresses( profileResult, family );
            PersonSkill.PopulateSpouse( profileResult, currentPerson, AddAttributeValues );
            PersonSkill.PopulateAdults( profileResult, family, AddAttributeValues );
            PersonSkill.PopulateChildren( profileResult, family, AddAttributeValues );

            // Run security on profile result
            profileResult.Sanitize( AgentRequestContext );

            return Success( profileResult );
        }

        #endregion
    }
}
