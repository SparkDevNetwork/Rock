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

using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.SignUp.SignUpProjectDetail
{
    /// <summary>
    /// The options returned by the <c>GetGroupRequirementOptions</c> block action when the group
    /// requirement modal is opened.
    /// </summary>
    public class SignUpProjectRequirementOptionsBag
    {
        /// <summary>
        /// Gets or sets the group requirement types that can be selected.
        /// </summary>
        public List<SignUpProjectRequirementTypeBag> GroupRequirementTypes { get; set; }

        /// <summary>
        /// Gets or sets the date and date time group attributes that can hold a requirement's
        /// due date. The value of each item is the attribute unique identifier.
        /// </summary>
        public List<ListItemBag> GroupAttributes { get; set; }
    }
}
