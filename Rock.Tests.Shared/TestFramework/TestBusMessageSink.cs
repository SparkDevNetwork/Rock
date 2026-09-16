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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Rock.Configuration;

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// Captures messages that would normally be published, sent, or requested on
    /// the message bus so a unit test can inspect them without starting a real
    /// bus. Registered on the scoped <see cref="RockApp"/> by
    /// <see cref="TestHelper.CreateScopedRockApp()"/>. When this sink is present
    /// the bus skips its real transport, which also prevents the "bus was not
    /// ready" exception logging that would otherwise reach into the shared mocked
    /// <c>RockContext</c>.
    /// </summary>
    /// <remarks>
    /// Messages are published from background threads, so the backing store is a
    /// thread-safe collection.
    /// </remarks>
    internal class TestBusMessageSink : IBusMessageSink
    {
        /// <summary>
        /// The messages that have been captured, in the order they arrived.
        /// </summary>
        private readonly ConcurrentQueue<object> _messages = new ConcurrentQueue<object>();

        /// <summary>
        /// Gets a snapshot of the messages that have been captured so far.
        /// </summary>
        public IReadOnlyList<object> Messages => _messages.ToList();

        /// <inheritdoc/>
        public void AddMessage( object message )
        {
            _messages.Enqueue( message );
        }
    }
}
