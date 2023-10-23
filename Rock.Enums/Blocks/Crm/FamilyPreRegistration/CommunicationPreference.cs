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

namespace Rock.Enums.Blocks.Crm.FamilyPreRegistration
{
    /// <summary>
    /// The communication preference options for the Family Pre-Registration block.
    /// </summary>
    public enum CommunicationPreference
    {
        /// <summary>
        /// No communication preference.
        /// </summary>
        None = 0,

        /// <summary>
        /// Email communication preference.
        /// </summary>
        Email = 1,

        /// <summary>
        /// SMS communication preference.
        /// </summary>
        SMS = 2,

        /// <summary>
        /// Push notification communication preference.
        /// </summary>
        PushNotification = 3
    }
}
