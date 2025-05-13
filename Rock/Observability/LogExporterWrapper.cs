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
using System.Runtime.CompilerServices;

using OpenTelemetry;
using OpenTelemetry.Logs;

namespace Rock.Observability
{
    /// <summary>
    /// Exporter that wraps the real exporter so that we can switch it out
    /// on the fly without restarting Rock.
    /// </summary>
    internal class LogExporterWrapper : BaseExporter<LogRecord>
    {
        /// <summary>
        /// Determines if the wrapped exporter has been initialized yet.
        /// </summary>
        private bool _initialized = false;

        /// <summary>
        /// The initialize lock so that we only initialize the wrapped
        /// exporter on a single thread.
        /// </summary>
        private readonly object _initializeLock = new object();

        /// <summary>
        /// The wrapped exporter that is currently in use.
        /// </summary>
        private BaseExporter<LogRecord> _exporter;

        /// <summary>
        /// Gets or sets the exporter.
        /// </summary>
        /// <value>The exporter.</value>
        public BaseExporter<LogRecord> Exporter
        {
            get => _exporter;
            set
            {
                _exporter = value;
                _initialized = _exporter == null;

                InitializeExporter();
            }
        }

        /// <inheritdoc/>
        public override ExportResult Export( in Batch<LogRecord> batch )
        {
            if ( !_initialized )
            {
                InitializeExporter();
            }

            // Use a local variable to ensure another thread doesn't change it.
            var exporter = Exporter;

            if ( exporter != null )
            {
                return exporter.Export( batch );
            }

            return ExportResult.Success;
        }

        /// <summary>
        /// Initializes the wrapped exporter.
        /// </summary>
        private void InitializeExporter()
        {
            lock ( _initializeLock )
            {
                if ( _initialized || ParentProvider == null )
                {
                    return;
                }

                // Use a local variable to ensure another thread doesn't change it.
                var exporter = Exporter;

                if ( exporter == null )
                {
                    _initialized = true;
                    return;
                }

                // There is a unit test to ensure this method still exists.
                var parentProviderProperty = GetParentProviderProperty( exporter );

                parentProviderProperty?.SetValue( exporter, ParentProvider );

                _initialized = true;
            }
        }

        /// <summary>
        /// Gets the parent provider property information from reflection.
        /// </summary>
        /// <param name="exporter">The exporter.</param>
        /// <returns>The property or <c>null</c> if it could not be found.</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static System.Reflection.PropertyInfo GetParentProviderProperty( BaseExporter<LogRecord> exporter )
        {
            return exporter.GetType().GetProperty( nameof( exporter.ParentProvider ) );
        }
    }
}
