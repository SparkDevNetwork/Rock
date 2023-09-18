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

namespace Rock.ViewModels.Blocks.Engagement.StreakDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class StreakDetailOptionsBag
    {
        /// <summary>
        /// The current streak details text that needs to be displayed on the frontend
        /// </summary>
        public string CurrentStreak { get; set; }

        /// <summary>
        /// The longest streak details text that needs to be displayed on the frontend
        /// </summary>
        public string LongestStreak { get; set; }

        /// <summary>
        /// The HTML of the Streak Chart that needs to be displayed on the frontend
        /// </summary>
        public string ChartHTML { get; set; }

        /// <summary>
        /// The person HTML to be displayed on the front end 
        /// </summary>
        public string personHTML { get; set; }
    }
}
