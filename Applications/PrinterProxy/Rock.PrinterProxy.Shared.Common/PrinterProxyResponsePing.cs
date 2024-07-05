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
    /// The response object that will be returned by a Ping message.
    /// </summary>
    internal class PrinterProxyResponsePing
    {
        /// <summary>
        /// The value from <see cref="PrinterProxyMessagePing.SentAt"/>
        /// </summary>
        public DateTimeOffset RequestedAt { get; set; }

        /// <summary>
        /// The date and time that the response was sent.
        /// </summary>
        public DateTimeOffset RespondedAt { get; set; }
    }
}
