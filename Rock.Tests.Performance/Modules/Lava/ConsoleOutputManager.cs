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
using System.Diagnostics;

namespace Rock.Tests.Performance.Modules.Lava
{
    public enum OutputMessageTypeSpecifier
    {
        Critical = 0,
        Warning = 1,
        Information = 2,
        Detail = 3,
        Heading1 = 4,
        Heading2 = 5,
    }

    /// <summary>
    /// Provides functions to manage output for a console application.
    /// </summary>
    public static class ConsoleOutputManager
    {
        /// <summary>
        /// Write a message line to the enabled output devices.
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        public static void WriteLine( OutputMessageTypeSpecifier messageType, string message )
        {
            Write( messageType, message + "\n" );
        }

        /// <summary>
        /// Write a message to the selected output devices.
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        /// <param name="writeToConsole"></param>
        /// <param name="writeToTrace"></param>
        public static void Write( OutputMessageTypeSpecifier messageType, string message, bool writeToConsole = true, bool writeToTrace = true )
        {
            if ( writeToConsole )
            {
                SetConsoleColor( messageType );

                Console.Write( message );
            }

            if ( writeToTrace )
            {
                Trace.Write( message, messageType.ToString() );
            }
        }

        /// <summary>
        /// Write a message to the console only.
        /// </summary>
        /// <param name="message"></param>
        public static void WriteConsole( OutputMessageTypeSpecifier messageType, string message )
        {
            Write( OutputMessageTypeSpecifier.Information, message, writeToConsole: true, writeToTrace: false );
        }

        /// <summary>
        /// Write an information message.
        /// </summary>
        /// <param name="message"></param>
        public static void WriteInfo( string message )
        {
            WriteLine( OutputMessageTypeSpecifier.Information, message );
        }

        /// <summary>
        /// Write a warning message.
        /// </summary>
        /// <param name="message"></param>
        public static void WriteWarning( string message )
        {
            WriteLine( OutputMessageTypeSpecifier.Warning, message );
        }

        /// <summary>
        /// Write a message formatted as a Heading Level 1.
        /// </summary>  
        /// <param name="message"></param>
        public static void WriteHeading1( string message )
        {
            WriteLine( OutputMessageTypeSpecifier.Heading1, message );
        }

        /// <summary>
        /// Write a message formatted as a Heading Level 2.
        /// </summary>  
        /// <param name="message"></param>

        public static void WriteHeading2( string message )
        {
            WriteLine( OutputMessageTypeSpecifier.Heading2, message );
        }

        /// <summary>
        /// Write a message that contains additional non-essential detail.
        /// </summary>  
        /// <param name="message"></param>
        public static void WriteDetail( string message )
        {
            WriteLine( OutputMessageTypeSpecifier.Detail, message );
        }

        private static void SetConsoleColor( OutputMessageTypeSpecifier messageType )
        {
            Console.ResetColor();

            if ( messageType == OutputMessageTypeSpecifier.Critical )
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if ( messageType == OutputMessageTypeSpecifier.Heading1 )
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else if ( messageType == OutputMessageTypeSpecifier.Heading2 )
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
            else if ( messageType == OutputMessageTypeSpecifier.Information )
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else if ( messageType == OutputMessageTypeSpecifier.Detail )
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Blue;
            }
        }
    }
}