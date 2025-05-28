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

namespace Rock.Enums.Communication
{
    /// <summary>
    /// The push open action used by communications.
    /// </summary>
    public enum PushOpenAction
    {
        /// <summary>
        /// No action should be taken.
        /// </summary>
        NoAction = 0,

        /// <summary>
        /// Show details of the push notification.
        /// </summary>
        ShowDetails = 1,

        /// <summary>
        /// Link to a mobile page.
        /// </summary>
        LinkToMobilePage = 2,

        /// <summary>
        /// Link to a URL.
        /// </summary>
        LinkToUrl = 3
    }
}
