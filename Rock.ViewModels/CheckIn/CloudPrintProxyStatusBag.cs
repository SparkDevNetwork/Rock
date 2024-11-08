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

namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// The status of a single proxy as defined in the server database.
    /// </summary>
    public class CloudPrintProxyStatusBag
    {
        /// <summary>
        /// The encrypted identifier of this proxy.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of the proxy device in the database.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The name of the server node sending this status update.
        /// </summary>
        public string ServerNode { get; set; }

        /// <summary>
        /// A list of current connections that the server has for this proxy.
        /// </summary>
        public List<CloudPrintProxyConnectionStatusBag> Connections { get; set; }
    }
}
