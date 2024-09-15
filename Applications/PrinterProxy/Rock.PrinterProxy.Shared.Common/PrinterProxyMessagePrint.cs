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
    /// A request to send some data to the specified printer. The data to be
    /// printed is contained in the extra data after the message.
    /// </summary>
    internal class PrinterProxyMessagePrint : PrinterProxyMessage
    {
        /// <summary>
        /// The address of the printer that the data should be sent to. This may
        /// be in <c>host:port</c> notation.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrinterProxyMessagePrint"/> class.
        /// </summary>
        public PrinterProxyMessagePrint()
        {
            Type = PrinterProxyMessageType.Print;
        }
    }
}
