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

using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.SignUp.SignUpProjectDetail
{
    /// <summary>
    /// A group requirement type option for the group requirement modal. Carries the type's due
    /// date type so the modal can display the matching due date field without another request.
    /// </summary>
    public class SignUpProjectRequirementTypeBag : ListItemBag
    {
        /// <summary>
        /// Gets or sets the due date type of the requirement type.
        /// </summary>
        public DueDateType DueDateType { get; set; }
    }
}
