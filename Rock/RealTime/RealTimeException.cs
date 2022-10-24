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

namespace Rock.RealTime
{
    /// <summary>
    /// Special exception used by the RealTime system. When thrown, the message
    /// will be sent back to the client as a native exception. Other types of
    /// exceptions are replaced by a generic error message on the client.
    /// </summary>
    public class RealTimeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public RealTimeException( string message )
            : base( message )
        {
        }
    }
}
