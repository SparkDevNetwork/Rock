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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rock.Tests.Shared
{
    /// <summary>
    /// Provides logging services for test projects.
    /// </summary>
    public static class LogHelper
    {
        private const string _DateTimeFormat = "HH:mm:ss.fff";

        private static TestContext _TestContext = null;
        private static Dictionary<Guid, string> _tasks = new Dictionary<Guid, string>();

        static LogHelper()
        {
            Trace.Listeners.Add( new System.Diagnostics.TextWriterTraceListener( Console.Out ) );
        }

        public static void SetTestContext( TestContext testContext )
        {
            _TestContext = testContext;
        }

        public static Guid StartTask( string testName )
        {
            var guid = Guid.NewGuid();
            _tasks.Add( guid, testName );

            Log( $"<START> Task: { testName }" );
            return guid;
        }

        public static void StopTask( Guid taskGuid )
        {
            if ( _tasks.ContainsKey( taskGuid ) )
            {
                // Indent to align with START tag.
                Log( $" <STOP> Task: { _tasks[taskGuid] }" );
            }
        }

        public static void Log( string message )
        {
            var msg = $@"[{ DateTime.Now.ToString( _DateTimeFormat ) }] {message}";

            Trace.WriteLine( msg );
            _TestContext?.WriteLine( "<INFO> " + msg );
        }

        public static void LogWarning( string message )
        {
            var msg = $"[{ DateTime.Now.ToString( _DateTimeFormat ) }] {message}";

            Trace.TraceWarning( msg );
            _TestContext?.WriteLine( "<WARN> " + msg );
        }

        public static void LogError( string message )
        {
            var msg = $"[{ DateTime.Now.ToString( _DateTimeFormat ) }] {message}";

            Trace.TraceError( msg );
            _TestContext?.WriteLine( "<ERR*> " + msg );
        }

        public static void LogError( Exception ex, string message = null )
        {
            var msg = $"[{ DateTime.Now.ToString( _DateTimeFormat ) }] {message}";
            msg += "\n" + ex.ToString();

            Trace.TraceError( msg );
            _TestContext?.WriteLine( "<ERR*> " + msg );
        }
    }
}