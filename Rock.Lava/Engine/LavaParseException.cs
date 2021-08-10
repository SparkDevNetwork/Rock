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

        public override string Message
        {
            get
            {
                string msg = _message;

                if ( string.IsNullOrWhiteSpace( _message ) && this.InnerException != null )
                {
                    msg = this.InnerException.Message;
                }

                msg = "Lava Template Parse failed" + ( string.IsNullOrWhiteSpace( msg ) ? "." : $": { msg }" );

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
}