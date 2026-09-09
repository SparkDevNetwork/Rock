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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Rock.Configuration;

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// Captures exceptions that would normally be written to the ExceptionLog
    /// table so that a unit test can inspect them and, more importantly, so the
    /// background exception logging thread does not reach into the shared mocked
    /// <c>RockContext</c>. Registered on the scoped <see cref="RockApp"/> by
    /// <see cref="TestHelper.CreateScopedRockApp()"/>.
    /// </summary>
    /// <remarks>
    /// Exceptions are logged from background threads, so the backing store is a
    /// thread-safe collection.
    /// </remarks>
    internal class TestExceptionLogSink : IExceptionLogSink
    {
        /// <summary>
        /// The exceptions that have been captured, in the order they arrived.
        /// </summary>
        private readonly ConcurrentQueue<Exception> _exceptions = new ConcurrentQueue<Exception>();

        /// <summary>
        /// Gets a snapshot of the exceptions that have been captured so far.
        /// </summary>
        public IReadOnlyList<Exception> Exceptions => _exceptions.ToList();

        /// <inheritdoc/>
        public void AddException( Exception exception )
        {
            _exceptions.Enqueue( exception );
        }
    }
}
