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

namespace Rock.Core.Automation
{
    /// <summary>
    /// The base class that all automation event executors must inherit from.
    /// This provides the basic functionality for executing one or more
    /// automation requests.
    /// </summary>
    internal abstract class AutomationEventExecutor : IDisposable
    {
        /// <summary>
        /// <c>true</c> if the instance has already been disposed.
        /// </summary>
        private bool disposedValue;

        /// <summary>
        /// Executes a single <see cref="AutomationRequest"/>. When a trigger
        /// fires, it will create a new instance of <see cref="AutomationRequest"/>
        /// that will be passed to all of the event executors attached to that
        /// trigger.
        /// </summary>
        /// <param name="request"></param>
        public abstract void Execute( AutomationRequest request );

        /// <summary>
        /// Disposes the resources used by this instance. This is called when
        /// the executor is no longer needed and is being shut down. Care must
        /// be taken if you override this method as it is still possible to
        /// receive an <see cref="Execute(AutomationRequest)"/> call after it
        /// is disposed due to timing issues across threads.
        /// </summary>
        /// <param name="disposing"><c>true</c> if this is being called from the Dispose method; <c>false</c> if being called from destructor.</param>
        protected virtual void Dispose( bool disposing )
        {
            if ( !disposedValue )
            {
                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method.
            Dispose( disposing: true );
            GC.SuppressFinalize( this );
        }
    }
}
