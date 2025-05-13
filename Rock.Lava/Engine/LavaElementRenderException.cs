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
    /// Represents an error encountered while attempting to render an element of a Lava template.
    /// </summary>
    /// <remarks>
    /// Exceptions of this type may be supressed or displayed in final output according to the framework configuration.
    /// </remarks>
    public class LavaElementRenderException : LavaException
    {
        private string _elementName;
        private string _message;
        private Dictionary<string, string> _parameters = new Dictionary<string, string>();

        #region Constructors

        /// <summary>
        /// Create a new instance of the LavaElementRenderException
        /// </summary>
        /// <param name="elementName">The name of the element.</param>
        /// <param name="message">The error message.</param>
        /// <returns></returns>
        public LavaElementRenderException( string elementName, string message )
            : base( message )
        {
            _elementName = elementName;
            _message = message;
        }

        /// <summary>
        /// Create a new instance of the LavaElementRenderException
        /// </summary>
        /// <param name="elementName">The name of the element.</param>
        /// <param name="ex">The inner exception.</param>
        /// <returns></returns>
        public LavaElementRenderException( string elementName, Exception ex ) :
            base( ex )
        {
            _elementName = elementName;
        }

        #endregion

        #region Factory Methods

        /// <summary>
        /// Create a new instance of the LavaElementRenderException
        /// </summary>
        /// <param name="elementName">The name of the element.</param>
        /// <param name="message">The error message.</param>
        /// <returns></returns>
        public static LavaElementRenderException New( string elementName, string message )
        {
            return new LavaElementRenderException( elementName, message );
        }

        #endregion

        /// <inheritdoc />
        public override string GetUserMessage()
        {
            if ( !string.IsNullOrWhiteSpace( _message ) )
            {
                return $"Lava Error: {_message}";
            }

            if ( this.InnerException is LavaException le )
            {
                // Try to get a user-friendly error message from the wrapped exception.
                return le.GetUserMessage();
            }
            else if ( this.InnerException != null )
            {
                return $"Lava Error: {this.InnerException.Message}";
            }

            return "Lava Template Render failed.";
        }

        /// <inheritdoc />
        public override string GetDiagnosticMessage()
        {
            var parameterString = $"Element=\"{_elementName}\"";

            foreach ( var p in _parameters )
            {
                parameterString += $", {p.Key}=\"{p.Value}\"";
            }

            if ( string.IsNullOrWhiteSpace( _message ) )
            {
                return $"Lava Element Render failed. [{parameterString}]";
            }
            else
            {
                return $"Lava Template Render failed: {_message} [{parameterString}]";
            }
        }

        internal void AddParameter( string parameterName, string parameterValue )
        {
            _parameters.AddOrReplace( parameterName, parameterValue );
        }

        internal void SetMessage( string message )
        {
            _message = message;
        }
    }
}