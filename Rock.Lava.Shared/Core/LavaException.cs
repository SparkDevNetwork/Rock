﻿// <copyright>
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
            _message = _defaultErrorMessage;
        }

        #endregion

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message
        {
            get
            {
                if ( string.IsNullOrWhiteSpace( _details ) )
                {
                    return _message;
                }
                else
                {
                    var message = _message == null ? string.Empty : _message.Trim();

                    if ( message.EndsWith( "." ) || message.EndsWith( ":" ) )
                    {
                        return $"{message} {_details}";
                    }
                    else
                    {
                        return $"{message}: {_details}";
                    }
                }
            }
        }
    }
}