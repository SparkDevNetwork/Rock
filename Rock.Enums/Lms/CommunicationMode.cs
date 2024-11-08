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
namespace Rock.Enums.Lms
{
    /// <summary>
    /// Determines the method of communicating an announcement.
    /// </summary>
    public enum CommunicationMode
    {
        /// <summary>
        /// The announcement isn't sent via any communication mode.
        /// </summary>
        None = 0,

        /// <summary>
        /// The announcement is sent by Email.
        /// </summary>
        Email = 1,

        /// <summary>
        /// The announcement is sent by SMS.
        /// </summary>
        SMS = 2
    }
}
