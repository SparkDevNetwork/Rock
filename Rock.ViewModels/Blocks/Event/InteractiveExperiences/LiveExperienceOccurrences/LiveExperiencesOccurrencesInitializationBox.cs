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

namespace Rock.ViewModels.Blocks.Event.InteractiveExperiences.LiveExperienceOccurrences
{
    public class LiveExperienceOccurrencesInitializationBox : BlockBox
    {
        /// <summary>
        /// The key to use when adding the JavaScript callback to the global context.
        /// </summary>
        public string ProvideLocationKey { get; set; }

        /// <summary>
        /// Gets a value that indicates if the device location should always be requested
        /// at initial page load.
        /// </summary>
        public bool AlwaysRequestLocation { get; set; }
    }
}
