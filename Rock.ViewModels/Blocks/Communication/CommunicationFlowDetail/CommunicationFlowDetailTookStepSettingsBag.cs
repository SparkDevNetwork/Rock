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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowDetail
{
    /// <summary>
    /// The conversion goal settings for the Communication Flow Detail block when the conversion goal type is "Completed Step".
    /// </summary>
    public class CommunicationFlowDetailTookStepSettingsBag
    {
        /// <summary>
        /// Gets or sets the Step Type that needs to be taken for the conversion goal to be achieved.
        /// </summary>
        public ListItemBag StepType { get; set; }
    }
}
