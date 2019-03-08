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

namespace Rock.Jobs
{
    /// <summary>
    /// Do not retry to run the job after this type of exception
    /// </summary>
    [Serializable]
    public class NoRetryAggregateException : AggregateException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoRetryAggregateException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the exception. The caller of this constructor is required to ensure that this string has been localized for the current system culture.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception.</param>
        public NoRetryAggregateException( string message, Exception innerException ) : base( message, innerException )
        {
        }
    }
}