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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the GetPlayerOptions API action of the MediaPlayer control.
    /// </summary>
    public class MediaPlayerGetPlayerOptionsOptionsBag
    {
        /// <summary>
        /// Get the child accounts for the account of this GUID. Empty Guid gets the root level accounts
        /// </summary>
        public MediaPlayerOptionsBag PlayerOptions { get; set; }

        /// <summary>
        /// Whether or not to include inactive accounts
        /// </summary>
        public Guid MediaElementGuid { get; set; }

        /// <summary>
        /// Whether or not to display the public name (vs the normal name)
        /// </summary>
        public int AutoResumeInDays { get; set; }

        /// <summary>
        /// Whether or not to load the full tree instead of just this level
        /// </summary>
        public int CombinePlayStatisticsInDays { get; set; }
    }
}
