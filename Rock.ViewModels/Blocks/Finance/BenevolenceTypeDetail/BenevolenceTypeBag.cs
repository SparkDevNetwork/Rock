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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Finance.BenevolenceTypeDetail
{
    public class BenevolenceTypeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the Rock.Model.BenevolenceType.Description value on the Rock.Model.BenevolenceType. This property is required.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.BenevolenceType.IsActive value on the Rock.Model.BenevolenceType. This property is required.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.BenevolenceType.Name value on the Rock.Model.BenevolenceType. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.BenevolenceType.RequestLavaTemplate value on the Rock.Model.BenevolenceType. This property is required.
        /// </summary>
        public string RequestLavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show financial results].
        /// </summary>
        public bool ShowFinancialResults { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of documents.
        /// </summary>
        /// <value>
        /// The maximum number of documents.
        /// </value>
        public int? MaximumNumberOfDocuments { get; set; }

        /// <summary>
        /// Gets or sets the workflows.
        /// </summary>
        /// <value>
        /// The workflows.
        /// </value>
        public List<BenevolenceWorkflowBag> Workflows { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has administrative rights with this entity.
        /// </summary>
        /// <value>
        ///   <c>true</c> if user can adminstrate; otherwise, <c>false</c>.
        /// </value>
        public bool CanAdminstrate { get; set; }
    }
}
