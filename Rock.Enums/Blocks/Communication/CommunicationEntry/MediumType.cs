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

namespace Rock.Enums.Blocks.Communication.CommunicationEntry
{
    /// <summary>
    /// The Medium Type used for the Communication Entry block.
    /// </summary>
    public enum MediumType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// Email message
        /// </summary>
        Email,

        /// <summary>
        /// SMS message
        /// </summary>
        Sms,

        /// <summary>
        /// Push notification
        /// </summary>
        Push
    }
}
