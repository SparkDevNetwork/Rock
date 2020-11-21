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
using System;
using System.Data.Entity;
using System.Linq;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.StepType"/> entity objects.
    /// </summary>
    public partial class StepTypeService
    {
        /// <summary>
        /// Returns a collection of Step Types that can be selected as prerequisites of the specified Step Type.
        /// A Step Type cannot be a prequisite of itself, or of any Step Type that has it as a prerequisite.
        /// </summary>
        /// <param name="stepTypeId">The unique identifier of the Step Type for which prerequisites are required.</param>
        /// <returns></returns>
        public IQueryable<StepType> GetEligiblePrerequisiteStepTypes( int stepTypeId )
        {
            // Get Step Types in the same Step Program.
            var stepProgramQuery = this.Queryable()
                .Where( x => x.Id == stepTypeId )
                .Select( x => x.StepProgramId );

            // Get Step Types of which the specified Step Type is not already a prerequisite.
            var availablePrerequisites = this.Queryable()
                .Where( x => stepProgramQuery.Contains( x.StepProgramId )
                             && x.IsActive
                             && x.Id != stepTypeId
                             && !x.StepTypePrerequisites.Any( p => p.PrerequisiteStepTypeId == stepTypeId ) );

            return availablePrerequisites;
        }
    }
}
