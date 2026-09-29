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

namespace Rock.ViewModels.Blocks.Engagement.PersonProgramStepList
{
    /// <summary>
    /// One prerequisite step type of a step type and whether the person has completed it.
    /// </summary>
    public class PersonProgramPrerequisiteBag
    {
        /// <summary>
        /// Gets or sets the prerequisite step type name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person has a completed
        /// step of the prerequisite type.
        /// </summary>
        public bool IsComplete { get; set; }
    }
}
