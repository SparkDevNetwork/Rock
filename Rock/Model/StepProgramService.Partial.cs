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

            var rockContext = Context as RockContext;
            var stepProgramCompletionService = new StepProgramCompletionService( rockContext );

            // Start with all the completed steps to try to narrow the set
            var completedStepQuery = stepProgramCompletionService.Queryable()
                .AsNoTracking()
                .Where( s =>
                    s.StepProgramId == stepProgram.Id );

            var personQuery = completedStepQuery
                .Select( g => new PersonStepProgramViewModel
                {
                    PersonId = g.PersonAlias.PersonId,
                    StartedDateTime = g.StartDateTime,
                    CompletedDateTime = g.EndDateTime
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
