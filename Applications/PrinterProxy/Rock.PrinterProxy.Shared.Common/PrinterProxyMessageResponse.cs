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
    /// A special message that encapsulates a response to another message. This
    /// can be used to create a two-way message where the message is sent and
    /// then a response is awaited.
    /// </summary>
    internal class PrinterProxyMessageResponse : PrinterProxyMessage
    {
        /// <summary>
        /// The result of the operation that this message is ins response to.
        /// It is valid for this value to be <see langword="null"/>.
        /// </summary>
#if NET
        public object? Result { get; set; }
#else
        public object Result { get; set; }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="PrinterProxyMessageResponse"/> class.
        /// </summary>
        public PrinterProxyMessageResponse()
        {
            Type = PrinterProxyMessageType.Response;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrinterProxyMessage"/> class.
        /// </summary>
        /// <param name="fromMessage">The original message that this message will be configured to respond to.</param>
        public PrinterProxyMessageResponse( PrinterProxyMessage fromMessage )
            : this()
        {
            Id = fromMessage.Id;
        }
    }
}
