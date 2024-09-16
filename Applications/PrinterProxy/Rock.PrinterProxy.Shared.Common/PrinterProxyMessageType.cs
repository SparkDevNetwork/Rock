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
namespace Rock.PrinterProxy.Shared
{
    /// <summary>
    /// The type of message that is being sent or received.
    /// </summary>
    internal enum PrinterProxyMessageType
    {
        /// <summary>
        /// No message, this is treated as a "no operation" message and will be
        /// ignored if it is ever received.
        /// </summary>
        None,

        /// <summary>
        /// A message to check if the other end of the connection is still there.
        /// The response should be of type <see cref="PrinterProxyResponsePing"/>.
        /// </summary>
        Ping,

        /// <summary>
        /// A special message that contains a response to a previous message. The
        /// <see cref="PrinterProxyMessage.Id"/> value should match the original
        /// message.
        /// </summary>
        Response,

        /// <summary>
        /// A request to a raw chunk of data to a printer.
        /// </summary>
        Print
    }
}
