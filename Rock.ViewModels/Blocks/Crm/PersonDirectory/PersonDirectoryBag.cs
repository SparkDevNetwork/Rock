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

namespace Rock.ViewModels.Blocks.Crm.PersonDirectory
{
    /// <summary>
    /// Holds the initialization data for the Person Directory block.
    /// </summary>
    public class PersonDirectoryBag
    {
        /// <summary>
        /// Gets or sets the initial directory results. This is empty unless the block
        /// is configured to show all people by default.
        /// </summary>
        public PersonDirectoryResultsBag Results { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person is currently
        /// opted out of the directory.
        /// </summary>
        public bool IsCurrentPersonOptedOut { get; set; }
    }
}
