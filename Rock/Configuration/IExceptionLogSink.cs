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

namespace Rock.Configuration
{
    /// <summary>
    /// <para>
    /// A seam that captures exceptions which would normally be written to the
    /// <c>ExceptionLog</c> table by <see cref="Rock.Model.ExceptionLogService"/>.
    /// </para>
    /// <para>
    /// This is intended solely for the unit test framework, which registers an
    /// implementation on its scoped <see cref="RockApp"/> so that background
    /// exception logging can be observed without a database and, more
    /// importantly, without reaching into the shared mocked <c>RockContext</c>.
    /// No implementation is registered in production, where exceptions continue
    /// to be written to the database as usual.
    /// </para>
    /// </summary>
    internal interface IExceptionLogSink
    {
        /// <summary>
        /// Captures an exception that would otherwise have been written to the
        /// <c>ExceptionLog</c> table.
        /// </summary>
        /// <param name="exception">The exception that was being logged.</param>
        void AddException( Exception exception );
    }
}
