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

using System.Threading.Tasks;

using Rock.Bus.Consumer;
using Rock.Bus.Queue;

namespace Rock.Tasks
{
    /// <summary>
    /// Bus Started Task for the <see cref="BusStartedTaskMessage" />
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="BusStartedTaskMessage"/>
    /// </remarks>
    public abstract class BusStartedTaskAsync<TMessage> : RockConsumerAsync<StartTaskQueue, TMessage>
        where TMessage : BusStartedTaskMessage
    {
        /// <summary>
        /// Consumes the specified context.
        /// </summary>
        /// <param name="message">The message.</param>
        public override Task ConsumeAsync( TMessage message )
        {
            return ExecuteAsync( message );
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public abstract Task ExecuteAsync( TMessage message );
    }
}
