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

namespace Rock.ViewModels.Blocks.Group.GroupSimpleRegister
{
    /// <summary>
    /// The information a person enters to register for a group via the Group Simple Register block.
    /// Used both as the initial (optionally prefilled) form values and as the input for the Register action.
    /// </summary>
    public class GroupSimpleRegisterBag
    {
        /// <summary>
        /// Gets or sets the person's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the person's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the person's email address.
        /// </summary>
        public string Email { get; set; }
    }
}
