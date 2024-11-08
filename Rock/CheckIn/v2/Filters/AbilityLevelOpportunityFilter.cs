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

using Rock.Enums.CheckIn;

namespace Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// Performs filtering of check-in opportunities based on the person's
    /// ability level.
    /// </summary>
    /// <remarks>
    /// This only performs filtering if we will not be asking for an ability
    /// level. It will then filter based on the attendee's current ability
    /// level.
    /// </remarks>
    internal class AbilityLevelOpportunityFilter : OpportunityFilter
    {
        /// <inheritdoc/>
        public override bool IsGroupValid( GroupOpportunity group )
        {
            var doNotAskAbilityLevel =
                TemplateConfiguration.AbilityLevelDetermination == AbilityLevelDeterminationMode.DoNotAsk
                || ( TemplateConfiguration.AbilityLevelDetermination == AbilityLevelDeterminationMode.DoNotAskIfThereIsNoAbilityLevel && Person.Person.AbilityLevel == null )
                || ( TemplateConfiguration.AbilityLevelDetermination == AbilityLevelDeterminationMode.DoNotAskIfThereIsAnAbilityLevel && Person.Person.AbilityLevel != null );

            // If we are asking for ability level later, then we don't need to filter.
            if ( !doNotAskAbilityLevel )
            {
                return true;
            }

            // If the group doesn't have an ability level requirement then
            // we won't filter it.
            if ( group.AbilityLevelId.IsNullOrWhiteSpace() )
            {
                return true;
            }

            // If group has an ability level but person does not have an ability
            // level then it will never match.
            if ( Person.Person.AbilityLevel == null )
            {
                return false;
            }

            return group.AbilityLevelId == Person.Person.AbilityLevel.Id;
        }
    }
}
