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

using Serilog.Core;
using Serilog.Events;

namespace Rock.Logging
{
    /// <summary>
    /// Wrapper around the serilog sink so that we can reconfigure the
    /// logging system on the fly.
    /// </summary>
    internal class SeriSinkWrapper : ILogEventSink
    {
        private readonly object _lockSync = new object();

        private Serilog.ILogger _logger;

        /// <inheritdoc/>
        public void Emit( LogEvent logEvent )
        {
            lock (_lockSync )
            {
                _logger?.Write( logEvent );
            }
        }

        public void SetLogger( Serilog.ILogger logger )
        {
            lock ( _lockSync )
            {
                if ( _logger is IDisposable disposable )
                {
                    disposable.Dispose();
                }

                _logger = logger;
            }
        }
    }
}
