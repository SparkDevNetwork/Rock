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

namespace Rock.PrinterProxy.Shared
{
    /// <summary>
    /// A ping message is used to check if the other end of the connection
    /// is still there and also determine what the current latency is.
    /// </summary>
    internal class PrinterProxyMessagePing : PrinterProxyMessage
    {
        /// <summary>
        /// The point in time that this message was originally sent at.
        /// </summary>
        public DateTimeOffset SentAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrinterProxyMessagePing"/> class.
        /// </summary>
        public PrinterProxyMessagePing()
        {
            Type = PrinterProxyMessageType.Ping;
        }
    }
}
