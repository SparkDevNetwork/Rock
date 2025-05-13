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

using Microsoft.Extensions.Logging;

using Rock.Logging;
using Rock.Model;

namespace Rock.Bus.Faults
{
    /// <summary>
    /// This class is responsible for handling errors thrown by mass transit.
    /// </summary>
    /// <seealso cref="MassTransit.IReceiveObserver" />
    [RockLoggingCategory]
    public sealed class ReceiveFaultObserver : IReceiveObserver
    {
        /// <summary>
        /// The logger for this instance.
        /// </summary>
        private ILogger _logger;

        /// <summary>
        /// Gets the logger for this instance.
        /// </summary>
        /// <value>The logger for this instance.</value>
        private ILogger Logger
        {
            get
            {
                if ( _logger == null )
                {
                    _logger = RockLogger.LoggerFactory.CreateLogger( GetType().FullName );
                }

                return _logger;
            }
        }

        /// <summary>
        /// Called when a message being consumed produced a fault
        /// </summary>
        /// <typeparam name="T">The message type</typeparam>
        /// <param name="context">The message consume context</param>
        /// <param name="duration">The consumer duration</param>
        /// <param name="consumerType">The consumer type</param>
        /// <param name="exception">The exception from the consumer</param>
        /// <returns></returns>
        public Task ConsumeFault<T>( ConsumeContext<T> context, TimeSpan duration, string consumerType, Exception exception ) where T : class
        {
            var errorMessage = $"A ConsumeFault occurred in the {consumerType}. Original Message: {context.Message}. Exception: {exception}";
            ExceptionLogService.LogException( new BusException( errorMessage, exception ) );

            Logger.LogError( "A ConsumeFault occurred in the @consumerType. Original Message: @originalMessage Exception: @exception", consumerType, context.Message, exception );
            return RockMessageBus.GetCompletedTask();
        }

        /// <summary>
        /// Called when a message has been consumed by a consumer
        /// </summary>
        /// <typeparam name="T">The message type</typeparam>
        /// <param name="context">The message consume context</param>
        /// <param name="duration">The consumer duration</param>
        /// <param name="consumerType">The consumer type</param>
        /// <returns></returns>
        public Task PostConsume<T>( ConsumeContext<T> context, TimeSpan duration, string consumerType ) where T : class
        {
            return RockMessageBus.GetCompletedTask();
        }

        /// <summary>
        /// Called when the message has been received and acknowledged on the transport
        /// </summary>
        /// <param name="context">The receive context of the message</param>
        /// <returns></returns>
        public Task PostReceive( ReceiveContext context )
        {
            return RockMessageBus.GetCompletedTask();
        }

        /// <summary>
        /// Called when a message has been delivered by the transport is about to be received by the endpoint
        /// </summary>
        /// <param name="context">The receive context of the message</param>
        /// <returns></returns>
        public Task PreReceive( ReceiveContext context )
        {
            return RockMessageBus.GetCompletedTask();
        }

        /// <summary>
        /// Called when the transport receive faults
        /// </summary>
        /// <param name="context">The receive context of the message</param>
        /// <param name="exception">The exception that was thrown</param>
        /// <returns></returns>
        public Task ReceiveFault( ReceiveContext context, Exception exception )
        {
            var errorMessage = $"A ReceiveFault occurred. Context: {context}. Exception {exception}";
            ExceptionLogService.LogException( new BusException( errorMessage, exception ) );

            Logger.LogError( "A ReceiveFault occurred Context: @context Exception: @exception", context, exception );
            return RockMessageBus.GetCompletedTask();
        }
    }
}