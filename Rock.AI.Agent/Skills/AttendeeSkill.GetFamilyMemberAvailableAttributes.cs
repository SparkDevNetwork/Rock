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
using Rock.Attribute;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal partial class AttendeeSkill
    {
        #region Tool(s)

        [Description( "Gets the available attributes that can be set when updating a family member." )]
        [AgentPurpose( "Provides a list of attribute definitions for family members and any value format instructions." )]
        [AgentToolGuid( "66b757c4-b2e5-419c-8289-fba2213750d4" )]
        public IAgentToolResult GetFamilyMemberAvailableAttributes()
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );
            var editablePersonAttributeGuids = ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.ViewablePersonAttributes, string.Empty ).SplitDelimitedValues().AsGuidList();
            var availableAttributes = AttributeCache.GetMany( editablePersonAttributeGuids, AgentRequestContext.RockContext ).ToList();
            var person = new Model.Person();

            Helper.LoadAttributes( person, AgentRequestContext.RockContext, availableAttributes );

            return Success( helper.GetAvailableAttributes( person ) );
        }


        #endregion
    }
}
