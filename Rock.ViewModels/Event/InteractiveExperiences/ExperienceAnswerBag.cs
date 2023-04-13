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

using Rock.Enums.Event;

namespace Rock.ViewModels.Event.InteractiveExperiences
{
    /// <summary>
    /// Details about an answer to an experience question.
    /// </summary>
    public class ExperienceAnswerBag
    {
        /// <summary>
        /// Gets or sets the identifier of this answer.
        /// </summary>
        /// <value>The identifier of this answer.</value>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the action identifier key.
        /// </summary>
        /// <value>The action identifier key.</value>
        public string ActionIdKey { get; set; }

        /// <summary>
        /// Gets or sets the campus unique identifier.
        /// </summary>
        /// <value>The campus unique identifier.</value>
        public Guid? CampusGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the campus.
        /// </summary>
        /// <value>The name of the campus.</value>
        public string CampusName { get; set; }

        /// <summary>
        /// Gets or sets the name of the submitter.
        /// </summary>
        /// <value>The name of the submitter.</value>
        public string SubmitterName { get; set; }

        /// <summary>
        /// Gets or sets the approval status.
        /// </summary>
        /// <value>The approval status.</value>
        public InteractiveExperienceApprovalStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the response text.
        /// </summary>
        /// <value>The response text.</value>
        public string Response { get; set; }
    }
}
