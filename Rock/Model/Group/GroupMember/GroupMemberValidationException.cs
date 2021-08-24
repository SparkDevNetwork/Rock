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

namespace Rock.Model
{
    /// <summary>
    /// Exception to throw if GroupMember validation rules are invalid (and can't be checked using .IsValid)
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class GroupMemberValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMemberValidationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public GroupMemberValidationException( string message ) : base( message )
        {
        }
    }
}
