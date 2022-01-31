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

namespace Rock.Lava
{
    /// <summary>
    /// An exception raised during Lava processing to force a termination of the current operation.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class LavaInterruptException : LavaException
    {
        private string _message;

        private const string _defaultErrorMessage = "The operation was terminated.";

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaException"/> class.
        /// </summary>
        public LavaInterruptException()
        {
            _message = _defaultErrorMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaException"/> class.
        /// </summary>
        public LavaInterruptException( string message )
            : base( message )
        {
            _message = message;
        }

        #endregion

        /// <summary>
        /// Gets a message that describes the reason for the interrupt.
        /// </summary>
        public override string Message
        {
            get
            {
                return _message;
            }
        }
    }
}