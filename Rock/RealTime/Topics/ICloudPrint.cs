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

using System.Collections.Generic;
using System.Threading.Tasks;

using Rock.ViewModels.CheckIn;

namespace Rock.RealTime.Topics
{
    /// <summary>
    /// Topic interface describing the messages that can be sent to clients on
    /// <see cref="CloudPrintTopic"/>.
    /// </summary>
    internal interface ICloudPrint
    {
        /// <summary>
        /// Notifies clients of new status details for all proxies that this
        /// server has knowledge of.
        /// </summary>
        /// <remarks>
        /// This is called automatically when the Cloud Print Monitor block
        /// is on screen and requesting updates.
        /// </remarks>
        /// <param name="statuses">A list of objects that represent the status of each proxy.</param>
        /// <returns>A task the represents the operation.</returns>
        Task ProxyStatus( List<CloudPrintProxyStatusBag> statuses );
    }
}
