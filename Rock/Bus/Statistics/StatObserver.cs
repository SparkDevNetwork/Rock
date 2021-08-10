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
using System.Threading.Tasks;
using MassTransit;
using Rock.Bus.Message;
using Rock.Logging;
using Rock.Model;

namespace Rock.Bus.Statistics
{
    /// <summary>
    /// Statistics Observer
    /// </summary>
    public sealed class StatObserver : IConsumeObserver
    {
        /// <summary>
        /// Called after the message has been dispatched to all consumers when one or more exceptions have occurred
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task ConsumeFault<T>( ConsumeContext<T> context, Exception exception ) where T : class
        {
            ExceptionLogService.LogException( exception );
            RockLogger.Log.Error( RockLogDomains.Core, "A Consume Fault occurred Original Message: @originalMessage Exception: @exception", context.Message, exception );
            return RockMessageBus.GetCompletedTask();
        }

        /// <summary>
        /// Called after the message has been dispatched to all consumers - note that in the case of an exception
        /// this method is not called, and the DispatchFaulted method is called instead
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task PostConsume<T>( ConsumeContext<T> context ) where T : class
        {
            // Log for the bus as a whole
            RockMessageBus.StatLog?.LogConsume();

            // Log for the specific queue
            var queue = RockMessage.GetQueue( typeof( T ) );
            queue?.StatLog?.LogConsume();

            return RockMessageBus.GetCompletedTask();
        }

        /// <summary>
        /// Called before a message is dispatched to any consumers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The consume context</param>
        /// <returns></returns>
        public Task PreConsume<T>( ConsumeContext<T> context ) where T : class
        {
            return RockMessageBus.GetCompletedTask();
        }
    }
}
