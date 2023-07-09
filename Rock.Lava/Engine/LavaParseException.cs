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
    /// Represents an error encountered while attempting to parse a Lava template.
    /// </summary>
    public class LavaParseException : LavaException
    {
        private string _engineName;
        private string _template;
        private string _message;

        public LavaParseException( string engineName, string template, string message )
            : base()
        {
            _engineName = engineName;
            _template = template;
            _message = message;
        }

        public LavaParseException( string engineName, string template, Exception ex ) :
            base( ex )
        {
            _engineName = engineName;
            _template = template;
        }

        /// <inheritdoc />
        public override string GetUserMessage()
        {
            if ( !string.IsNullOrWhiteSpace( _message ) )
            {
                return $"Lava Error: { _message }";
            }

            if ( this.InnerException is LavaException le )
            {
                // Try to get a user-friendly error message from the wrapped exception.
                return le.GetUserMessage();
            }
            else if ( this.InnerException != null )
            {
                return $"Lava Error: { this.InnerException.Message }";
            }

            return "Lava Template Parse failed.";
        }

        /// <inheritdoc />
        public override string GetDiagnosticMessage()
        {
            var msg = GetUserMessage();

            // Add information about the source template and Lava Engine name.
            if ( !string.IsNullOrWhiteSpace( _template ) )
            {
                msg += $"\n[Template=\"{ _template }\"]";
            }
            if ( !string.IsNullOrWhiteSpace( _engineName ) )
            {
                msg += $"\n[Engine={ _engineName }]";
            }

            return msg;
        }
    }
}