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

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// An exception from the check-in system that can be displayed to the
    /// individual making the request. It will be formatted in a user-friendly
    /// way. These should be used to indicate an error that can be likely be
    /// corrected by the individual rather than some system-level error.
    /// </summary>
    internal class CheckInMessageException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInMessageException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CheckInMessageException( string message )
            : base( message )
        {
        }
    }
}
