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

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowPerformance
{
    /// <summary>
    /// The conversion goal settings for the Communication Flow Performance block.
    /// </summary>
    public class ConversionGoalSettingsBag
    {
        /// <summary>
        /// Gets or sets the conversion goal settings when the conversion goal type is "Completed Form".
        /// </summary>
        public CompletedFormSettingsBag CompletedFormSettings { get; set; }

        /// <summary>
        /// Gets or sets the conversion goal settings when the conversion goal type is "Joined Group Of Type".
        /// </summary>
        public JoinedGroupTypeSettingsBag JoinedGroupTypeSettings { get; set; }

        /// <summary>
        /// Gets or sets the conversion goal settings when the conversion goal type is "Joined Specific Group".
        /// </summary>
        public JoinedGroupSettingsBag JoinedGroupSettings { get; set; }

        /// <summary>
        /// Gets or sets the conversion goal settings when the conversion goal type is "Registered".
        /// </summary>
        public RegisteredSettingsBag RegisteredSettings { get; set; }

        /// <summary>
        /// Gets or sets the conversion goal settings when the conversion goal type is "Completed Step".
        /// </summary>
        public TookStepSettingsBag TookStepSettings { get; set; }

        /// <summary>
        /// Gets or sets the conversion goal settings when the conversion goal type is "Entered Data View".
        /// </summary>
        public EnteredDataViewSettingsBag EnteredDataViewSettings { get; set; }
    }
}
