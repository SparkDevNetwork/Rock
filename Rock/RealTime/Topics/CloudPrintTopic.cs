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

namespace Rock.RealTime.Topics
{
    /// <summary>
    /// Describes the messages that can be received from clients for the cloud
    /// print service as well as helpful methods to facilitate communication.
    /// </summary>
    [RealTimeTopic]
    internal sealed class CloudPrintTopic : Topic<ICloudPrint>
    {
        /// <summary>
        /// The channel used for proxy status messages.
        /// </summary>
        public static readonly string ProxyStatusChannel = "proxy-status";
    }
}
