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

using Rock.ViewModels.Event.InteractiveExperiences;

namespace Rock.ViewModels.Blocks.Event.InteractiveExperiences.ExperienceVisualizer
{
    public class VisualizerOccurrenceBag
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
        /// Gets or sets the date and time the occurrence will end.
        /// </summary>
        /// <value>The date and time the occurrence will end.</value>
        public DateTimeOffset OccurrenceEndDateTime { get; set; }
    }
}
