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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.AssessmentTypeDetail
{
    /// <summary>
    /// Class AssessmentTypeBag.
    /// Implements the <see cref="Rock.ViewModels.Utility.EntityBagBase" />
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class AssessmentTypeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the AssessmentPath of the Rock.Model.AssessmentType  
        /// </summary>
        public string AssessmentPath { get; set; }

        /// <summary>
        /// Gets or sets the AssessmentResultsPath of the Rock.Model.Assessment or the Rock.Model.AssessmentType if no requestor required.
        /// </summary>
        public string AssessmentResultsPath { get; set; }

        /// <summary>
        /// Gets or sets the Description of the Rock.Model.AssessmentType
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the IsActive flag for the Rock.Model.AssessmentType.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this Rock.Model.AssessmentType is a part of the Rock core system/framework. This property is required.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the number of days given for the Rock.Model.AssessmentType. to be retaken.
        /// </summary>
        public int? MinimumDaysToRetake { get; set; }

        /// <summary>
        /// Gets or sets the RequiresRequest flag for the Rock.Model.AssessmentType.
        /// </summary>
        public bool RequiresRequest { get; set; }

        /// <summary>
        /// Gets or sets the Title of the Rock.Model.AssessmentType  
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the number of days the assessment is valid for Rock.Model.AssessmentType.
        /// How long (in days) is this assessment valid before it must be taken again.
        /// </summary>
        public int ValidDuration { get; set; }
    }
}
