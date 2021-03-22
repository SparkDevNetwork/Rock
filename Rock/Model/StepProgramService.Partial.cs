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
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Step Program Service
    /// </summary>
    public partial class StepProgramService
    {
        /// <summary>
        /// Gets a query of the people completing the program.
        /// </summary>
        /// <returns></returns>
        public IQueryable<PersonStepProgramViewModel> GetPersonCompletingProgramQuery( int stepProgramId )
        {
            var stepProgram = StepProgramCache.Get( stepProgramId );

            if ( stepProgram == null )
            {
                return null;
            }

            var stepTypeIds = stepProgram.StepTypes.Where( st => st.IsActive ).Select( st => st.Id );
            var rockContext = Context as RockContext;
            var stepService = new StepService( rockContext );

            // Start with all the completed steps to try to narrow the set
            var completedStepQuery = stepService.Queryable()
                .AsNoTracking()
                .Where( s =>
                    stepTypeIds.Contains( s.StepTypeId ) &&
                    s.CompletedDateKey != null );

            // Group the completed steps by person where the person has all the step types completed
            // Then find the date the person started (min start date) and the max(min complete date of each step type)
            var personQuery = completedStepQuery.GroupBy( s => s.PersonAlias.PersonId )
                .Where( g => !stepTypeIds.Except( g.Select( s => s.StepTypeId ) ).Any() )
                .Select( g => new PersonStepProgramViewModel
                {
                    PersonId = g.Key,
                    StartedDateTime = g.Min( s => s.StartDateTime ),
                    CompletedDateTime = stepTypeIds.Select( i => g.Where( s => s.StepTypeId == i ).Min( s => s.CompletedDateTime ) ).Max()
                } );

            return personQuery;
        }

        #region View Models

        /// <summary>
        /// Person Program View Model
        /// </summary>
        public sealed class PersonStepProgramViewModel
        {
            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the started date time.
            /// </summary>
            /// <value>
            /// The started date time.
            /// </value>
            public DateTime? StartedDateTime { get; set; }

            /// <summary>
            /// Gets or sets the completed date time.
            /// </summary>
            /// <value>
            /// The completed date time.
            /// </value>
            public DateTime? CompletedDateTime { get; set; }
        }

        #endregion View Models
    }
}
