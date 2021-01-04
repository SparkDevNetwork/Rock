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

namespace Rock.WebFarm
{
    /// <summary>
    /// Executes an action every designated time span
    /// </summary>
    internal sealed class IntervalAction
    {
        private Func<Task> _func;
        private TimeSpan _timeSpan;
        private bool doKeepGoing = true;

        /// <summary>
        /// Starts a new interval action.
        /// </summary>
        /// <param name="func">The function.</param>
        /// <param name="timeSpan">The time span.</param>
        /// <returns></returns>
        public static IntervalAction Start( Func<Task> func, TimeSpan timeSpan )
        {
            var instance = new IntervalAction( func, timeSpan );
            instance.Start();
            return instance;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntervalAction" /> class.
        /// </summary>
        /// <param name="func">The function.</param>
        /// <param name="timeSpan">The time span.</param>
        public IntervalAction( Func<Task> func, TimeSpan timeSpan )
        {
            _func = func;
            _timeSpan = timeSpan;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Stop()
        {
            doKeepGoing = false;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            Task.Delay( _timeSpan ).ContinueWith( async t =>
            {
                if ( !doKeepGoing )
                {
                    return;
                }

                // Schedule the next interval execution
                Start();

                // Execute the action
                await _func();
            } );
        }
    }
}
