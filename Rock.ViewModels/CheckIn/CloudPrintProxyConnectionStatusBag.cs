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
using System;

namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// The status of a single proxy connection to a server.
    /// </summary>
    public class CloudPrintProxyConnectionStatusBag
    {
        /// <summary>
        /// The friendly name of the remote proxy service.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The IP address the remote proxy connected from.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// The number of labels sent to the proxy for printing since it
        /// connected to the server.
        /// </summary>
        public int LabelCount { get; set; }

        /// <summary>
        /// The date and time the remote proxy service connected to the server.
        /// </summary>
        public DateTimeOffset ConnectedDateTime { get; set; }
    }
}
