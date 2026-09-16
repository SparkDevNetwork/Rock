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
namespace Rock.Configuration
{
    /// <summary>
    /// <para>
    /// A seam that captures messages which would normally be published, sent,
    /// or requested on the <see cref="Rock.Bus.RockMessageBus"/>.
    /// </para>
    /// <para>
    /// This is intended solely for the unit test framework, which registers an
    /// implementation on its scoped <see cref="RockApp"/>. When present, the bus
    /// captures the message here and skips the real transport, so a unit test
    /// neither has to start a message bus nor triggers the "bus was not ready"
    /// exception logging that would otherwise reach into the shared mocked
    /// <c>RockContext</c>. No implementation is registered in production, where
    /// messages are published to the real transport as usual.
    /// </para>
    /// </summary>
    internal interface IBusMessageSink
    {
        /// <summary>
        /// Captures a message that would otherwise have been published, sent, or
        /// requested on the message bus.
        /// </summary>
        /// <param name="message">The message that was being published.</param>
        void AddMessage( object message );
    }
}
