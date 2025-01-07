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
using System.Collections.Generic;

namespace Rock.CloudPrint.Shared
{
    /// <summary>
    /// A generic message that will be sent to or received from the proxy.
    /// </summary>
    internal class CloudPrintMessage
    {
        /// <summary>
        /// This is used when deserializing to quickly know what CLR type the
        /// message should be deserialized into.
        /// </summary>
        internal static IReadOnlyDictionary<CloudPrintMessageType, Type> MessageLookup = new Dictionary<CloudPrintMessageType, Type>
        {
            [CloudPrintMessageType.Ping] = typeof( CloudPrintMessagePing ),
            [CloudPrintMessageType.Response] = typeof( CloudPrintMessageResponse ),
            [CloudPrintMessageType.Print] = typeof( CloudPrintMessagePrint ),
        };

        /// <summary>
        /// The type of message that will be sent or was received. This is
        /// primarily used during serialization so we can include the message
        /// type code in the data stream.
        /// </summary>
        public CloudPrintMessageType Type { get; set; }

        /// <summary>
        /// The unique identifier of this message.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudPrintMessage"/> class.
        /// </summary>
        public CloudPrintMessage()
        {
        }
    }
}
