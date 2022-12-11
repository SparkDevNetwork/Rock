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

namespace Rock.RealTime
{
    /// <summary>
    /// Extension methods for <see cref="ITopic"/>
    /// </summary>
    public static class ITopicExtensions
    {
        /// <summary>
        /// Gets a state tracking object for a connection. This object is
        /// unique to each connection. Meaning a single person with two
        /// connections will have two different state objects. The state object
        /// is valid until the client disconnects.
        /// </summary>
        /// <remarks>
        /// The returned object is not thread safe unless you make it so. For
        /// example, your state object should not use <see cref="System.Collections.Generic.List{T}"/>.
        /// Instead use a <see cref="System.Collections.Concurrent.ConcurrentBag{T}"/>
        /// or similar implementation that is thread safe.
        /// </remarks>
        /// <param name="topic">The topic to use when accessing the client state.</param>
        /// <param name="connectionIdentifier">The connection identifier that the state object should be attached to.</param>
        /// <typeparam name="TState">The type of state object.</typeparam>
        /// <returns>An instance of the <typeparamref name="TState"/> object.</returns>
        public static TState GetConnectionState<TState>( this ITopic topic, string connectionIdentifier )
            where TState : class, new()
        {
            if ( topic is ITopicContextInternal topicInternal )
            {
                return topicInternal.Engine.GetConnectionState<TState>( connectionIdentifier );
            }
            else
            {
                return new TState();
            }
        }
    }
}
