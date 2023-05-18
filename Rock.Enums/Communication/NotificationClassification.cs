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

namespace Rock.Model
{
    /// <summary>
    /// The classification of the notification (borrowed from Bootstrap)
    /// </summary>
    [Enums.EnumDomain( "Communication" )]
    public enum NotificationClassification
    {
        /// <summary>
        /// The success
        /// </summary>
        Success,

        /// <summary>
        /// The information
        /// </summary>
        Info,

        /// <summary>
        /// The warning
        /// </summary>
        Warning,

        /// <summary>
        /// The danger
        /// </summary>
        Danger
    }
}
