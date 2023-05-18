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
        static TestHelper()
        {
            Trace.Listeners.Add( new TextWriterTraceListener( Console.Out ) );
        }


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
        public static void StartTimer( string name )
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
                Debug.Print( $"** START: {name}" );
            }
            stopwatch.Start();
        }

        /// <summary>
        /// Stops the named timer.
        /// </summary>
        /// <param name="name"></param>
        public static void StopTimer( string name )
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
        public static void EndTimer( string name )
        {
            if ( !_stopwatches.ContainsKey( name ) )
            {
                return;
            }

            var stopwatch = _stopwatches[name];
            stopwatch.Stop();
            _stopwatches.Remove( name );

            Debug.Print( $"**   END: {name} ({stopwatch.ElapsedMilliseconds}ms)" );
        }

        #endregion
    }
}