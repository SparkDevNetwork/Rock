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

namespace Rock.ViewModels.Blocks.Event.InteractiveExperiences.ExperienceManagerOccurrences
{
    /// <summary>
    /// Class ExperienceManagerOccurrencesInitializationBox.
    /// Implements the <see cref="Rock.ViewModels.Blocks.BlockBox" />
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Blocks.BlockBox" />
    public class ExperienceManagerOccurrencesInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the name of the experience.
        /// </summary>
        /// <value>The name of the experience.</value>
        public string ExperienceName { get; set; }

        /// <summary>
        /// Gets or sets the occurrences that are available to be chosen.
        /// </summary>
        /// <value>The occurrences that are available to be chosen.</value>
        public List<ListItemBag> Occurrences { get; set; }
    }
}
