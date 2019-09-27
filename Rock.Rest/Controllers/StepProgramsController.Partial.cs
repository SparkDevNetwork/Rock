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
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Step Programs REST API
    /// </summary>
    public partial class StepProgramsController
    {
        /// <summary>
        /// Gets data for the steps badge
        /// </summary>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/StepPrograms/BadgeData/{stepProgramGuid}/{personId}" )]
        public List<PersonStepType> GetStepsBadgeData( Guid stepProgramGuid, int personId )
        {
            var stepProgramQuery = Service.Queryable().AsNoTracking().Where( sp => sp.Guid == stepProgramGuid && sp.IsActive );
            var stepTypesQuery = stepProgramQuery.SelectMany( sp => sp.StepTypes.Where( st => st.IsActive ) ).OrderBy( st => st.Order ).ThenBy( st => st.Name );

            var personStepTypes = stepTypesQuery.Select( st => new PersonStepType
            {
                StepTypeId = st.Id,
                StepTypeName = st.Name,
                HighlightColor = st.HighlightColor,
                IconCssClass = st.IconCssClass,
                ShowCountOnBadge = st.ShowCountOnBadge,
                CompletionCount = st.Steps.Count( s => s.StepStatus != null && s.StepStatus.IsCompleteStatus && s.PersonAlias.PersonId == personId ),
                Statuses = st.Steps
                    .Where( s => s.StepStatus != null && s.PersonAlias.PersonId == personId )
                    .OrderBy( s => s.CompletedDateTime ?? s.EndDateTime ?? s.StartDateTime ?? s.CreatedDateTime ?? DateTime.MinValue )
                    .Select( s => s.StepStatus.Name )
            } );

            return personStepTypes.ToList();
        }

        #region Output Data Classes

        /// <summary>
        /// A person's data for a step type
        /// </summary>
        public class PersonStepType
        {
            /// <summary>
            /// Gets or sets the step type identifier.
            /// </summary>
            /// <value>
            /// The step type identifier.
            /// </value>
            public int StepTypeId { get; set; }

            /// <summary>
            /// Gets or sets the name of the step type. This property is required.
            /// </summary>
            public string StepTypeName { get; set; }

            /// <summary>
            /// Gets or sets the icon CSS class.
            /// </summary>
            public string IconCssClass { get; set; }

            /// <summary>
            /// Gets or sets a flag indicating if the number of occurrences should be shown on the badge.
            /// </summary>
            public bool ShowCountOnBadge { get; set; } = true;

            /// <summary>
            /// Gets or sets the highlight color for badges and cards.
            /// </summary>
            public string HighlightColor { get; set; }

            /// <summary>
            /// Gets or sets the completion count.
            /// </summary>
            /// <value>
            /// The completion count.
            /// </value>
            public int CompletionCount { get; set; }

            /// <summary>
            /// Gets or sets the statuses.
            /// </summary>
            /// <value>
            /// The statuses.
            /// </value>
            public IEnumerable<string> Statuses { get; set; }
        }

        #endregion Output Data Classes
    }
}