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

using Rock.ViewModels.Event.InteractiveExperiences;

namespace Rock.ViewModels.Blocks.Event.InteractiveExperiences.LiveExperience
{
    /// <summary>
    /// Class LiveExperienceInitializationBox.
    /// Implements the <see cref="Rock.ViewModels.Blocks.BlockBox" />
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Blocks.BlockBox" />
    public class LiveExperienceInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the experience token used to authenticate to the RealTime topic.
        /// </summary>
        /// <value>The experience token used to authenticate to the RealTime topic.</value>
        public string ExperienceToken { get; set; }

        /// <summary>
        /// Gets or sets the style information for this experience.
        /// </summary>
        /// <value>The style information for this experience.</value>
        public ExperienceStyleBag Style { get; set; }

        /// <summary>
        /// Gets or sets the keep alive interval in seconds.
        /// </summary>
        /// <value>The keep alive interval in seconds.</value>
        public int KeepAliveInterval { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this experience is inactive.
        /// </summary>
        /// <value><c>true</c> if this experience is inactive; otherwise, <c>false</c>.</value>
        public bool IsExperienceInactive { get; set; }

        /// <summary>
        /// Gets or sets the content to display when the experience has ended.
        /// </summary>
        /// <value>The content to display when the experience has ended.</value>
        public string ExperienceEndedContent { get; set; }
    }
}
