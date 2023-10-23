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
using System.Collections.Generic;
using System.Diagnostics;

namespace Rock.Tests.Shared
{
    public static class TestHelper
    {
        public const string DefaultTaskName = "Main Test Action";

        static TestHelper()
        {
            // Add the console as the default trace output.
            Trace.Listeners.Add( new TextWriterTraceListener( Console.Out ) );
        }

        /// <summary>
        /// Write a message to the current trace output.
        /// </summary>
        /// <param name="message"></param>
        public static void Log( string message )
        {
            var timestamp = DateTime.Now.ToString( "HH:mm:ss.fff" );
            Trace.WriteLine( $"[{timestamp}] {message}" );
        }

        #region Stopwatch

        private static Dictionary<string, Stopwatch> _stopwatches = new Dictionary<string, Stopwatch>( StringComparer.OrdinalIgnoreCase );

        /// <summary>
        /// Starts or restarts a named timer.
        /// </summary>
        /// <param name="name"></param>
        public static Stopwatch ExecuteWithTimer( string message, Action testMethod )
        {
            var stopwatch = StartTimer( message );
            try
            {
                testMethod();
            }
            catch( Exception ex )
            {
                Log( $"** ERROR:\n{ex.Message}" );
            }
            finally
            {
                EndTimer( message );
            }

            return stopwatch;
        }

        /// <summary>
        /// Starts or restarts a named timer.
        /// </summary>
        /// <param name="name"></param>
        public static Stopwatch StartTimer( string name = DefaultTaskName )
        {
            Stopwatch stopwatch;
            if ( _stopwatches.ContainsKey( name ) )
            {
                stopwatch = _stopwatches[name];
            }
            else
            {
                stopwatch = new Stopwatch();
                _stopwatches[name] = stopwatch;
                Log( $"** START: {name}" );
            }
            stopwatch.Start();

            return stopwatch;
        }

        /// <summary>
        /// Pauses the named timer.
        /// </summary>
        /// <param name="name"></param>
        public static void PauseTimer( string name = DefaultTaskName )
        {
            if ( !_stopwatches.ContainsKey( name ) )
            {
                return;
            }

            var stopwatch = _stopwatches[name];
            stopwatch.Stop();
        }

        /// <summary>
        /// Finalizes the named timer and prints the elapsed time to debug output.
        /// </summary>
        /// <param name="name"></param>
        public static Stopwatch EndTimer( string name = DefaultTaskName )
        {
            if ( !_stopwatches.ContainsKey( name ) )
            {
                return null;
            }

            var stopwatch = _stopwatches[name];
            stopwatch.Stop();
            _stopwatches.Remove( name );

            Log( $"**   END: {name} ({stopwatch.ElapsedMilliseconds}ms)" );

            return stopwatch;
        }

        #endregion
    }
}