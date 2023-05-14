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

namespace Rock.Lava
{
    /// <summary>
    /// An exception that occurs during Lava processing.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class LavaException : Exception
    {
        private string _message;
        private string _details;

        private const string _defaultErrorMessage = "A Lava processing error has occurred.";

        #region Constructors

        public LavaException()
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaException"/> class.
        /// </summary>
        public LavaException( string message )
            : base( message )
        {
            _message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaException"/> class.
        /// </summary>
        public LavaException( string message, params object[] args )
            : base( message )
        {
            if ( args != null && args.Length > 0 )
            {
                message = string.Format( message, args );
            }

            _message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaException"/> class.
        /// </summary>
        public LavaException( string message, IEnumerable<string> details )
            : base( message )
        {
            _message = message;

            _details = string.Join( "\n", details );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaException"/> class.
        /// </summary>
        public LavaException( string message, Exception ex )
            : base( message, ex )
        {
            _message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaException"/> class.
        /// </summary>
        public LavaException( Exception ex )
            : base( _defaultErrorMessage, ex )
        {
        }

        #endregion

        /// <summary>
        /// Get a user-friendly message that describes the exception.
        /// </summary>
        /// <returns></returns>
        public virtual string GetUserMessage()
        {
            // By default, the user message provides the same level of detail as the diagnostic message.
            return GetDiagnosticMessage();
        }

        /// <summary>
        /// Gets a detailed message that provides diagnostic information for the exception.
        /// </summary>
        /// <returns></returns>
        public virtual string GetDiagnosticMessage()
        {
            if ( string.IsNullOrWhiteSpace( _message ) && string.IsNullOrWhiteSpace( _details ) )
            {
                if ( this.InnerException is LavaException le )
                {
                    return le.GetDiagnosticMessage();
                }

                return _message ?? _defaultErrorMessage;
            }
            else
            {
                var message = ( _message == null ) ? _defaultErrorMessage : _message.Trim();

                if ( string.IsNullOrWhiteSpace( _details ) )
                {
                    return message;
                }
                else if ( message.EndsWith( "." ) || message.EndsWith( ":" ) )
                {
                    return $"{message} {_details}";
                }
                else
                {
                    return $"{message}: {_details}";
                }
            }
        }

        /// <summary>
        /// Gets the default message that describes the current exception.
        /// </summary>
        public override string Message
        {
            get
            {
                // Return the diagnostic message by default, because this method is called by internal error reporting mechanisms.
                return GetDiagnosticMessage();
            }
        }
    }
}