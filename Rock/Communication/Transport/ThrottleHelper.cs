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

using System;
using System.Threading;
using System.Threading.Tasks;
using Rock.Model;

namespace Rock.Communication.Transport
{
    internal static class ThrottleHelper
    {
        /// <summary>
        /// Throttles the execute based on the specified semaphore.
        /// </summary>
        /// <param name="throttledMethod">The throttled method.</param>
        /// <param name="mutex">The mutex.</param>
        public static async Task ThrottledExecute( Func<Task> throttledMethod, SemaphoreSlim mutex )
        {
            try
            {
                await throttledMethod().ConfigureAwait( false );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }
            finally
            {
                mutex.Release();
            }
        }

        /// <summary>
        /// Throttles the execute based on the specified semaphore.
        /// </summary>
        /// <param name="throttledMethod">The throttled method.</param>
        /// <param name="mutex">The mutex.</param>
        /// <returns></returns>
        public static async Task<SendMessageResult> ThrottledExecute( Func<Task<SendMessageResult>> throttledMethod, SemaphoreSlim mutex )
        {
            try
            {
                return await throttledMethod().ConfigureAwait( false );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }
            finally
            {
                mutex.Release();
            }

            return default;
        }
    }
}
