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
    /// Represents an error encountered while attempting to render a Lava template.
    /// </summary>
    public class LavaRenderException : LavaException
    {
        private string _engineName;
        private string _template;
        private string _message;

        public LavaRenderException( string engineName, string template, string message )
        {
            _engineName = engineName;
            _template = template;
            _message = message;
        }

        public LavaRenderException( string engineName, string template, Exception ex ) :
            base( ex )
        {
            _engineName = engineName;
            _template = template;
        }

        public override string Message
        {
            get
            {
                if ( string.IsNullOrWhiteSpace( _message ) )
                {
                    return $"Lava Template Render failed. [Engine={ _engineName }, Template=\"{ _template }\"]";
                }
                else
                {
                    return $"Lava Template Render failed: { _message } [Engine={ _engineName }, Template=\"{ _template }\"]";
                }
            }
        }
    }
}