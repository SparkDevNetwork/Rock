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

using System.Threading.Tasks;

namespace Rock.RealTime.Topics
{
    /// <summary>
    /// Topic interface describing the messages that can be sent to clients on
    /// <see cref="CloudPrintTopic"/>.
    /// </summary>
    internal interface ICheckIn
    {
        /// <summary>
        /// Informs all kiosks that they should refresh their configuration
        /// because it might have changed.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents when the message has been queued up.</returns>
        Task RefreshKioskConfiguration();
    }
}
