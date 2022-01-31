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

using System.ComponentModel.DataAnnotations;

namespace Rock.Model
{
    /// <summary>
    /// Type of communication
    /// </summary>
    public enum CommunicationType
    {
        /// <summary>
        /// RecipientPreference
        /// </summary>
        [Display( Name = "No Preference" )]
        RecipientPreference = 0,

        /// <summary>
        /// Email
        /// </summary>
        Email = 1,

        /// <summary>
        /// SMS
        /// </summary>
        SMS = 2,

        /// <summary>
        /// Push notification
        /// </summary>
        PushNotification = 3
    }
}
