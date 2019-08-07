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

using Rock.UniversalSearch.Crawler.RobotsTxt.Enums;
using Rock.UniversalSearch.Crawler.RobotsTxt.Helpers;

namespace Rock.UniversalSearch.Crawler.RobotsTxt
{
    internal class Line
    {
        public LineType Type { get; private set; }
        public string Raw { get; private set; }
        public string Field { get; private set; }
        public string Value { get; private set; }

        public Line( string line )
        {
            if ( String.IsNullOrWhiteSpace( line ) )
            {
                throw new ArgumentException( "Can't create a new instance of Line class with an empty line.", "line" );
            }

            Raw = line;
            line = line.Trim();

            if ( line.StartsWith( "#" ) )
            {
                // whole line is comment
                Type = LineType.Comment;
                return;
            }

            // if line contains comments, get rid of them
            if ( line.IndexOf( '#' ) > 0 )
            {
                line = line.Remove( line.IndexOf( '#' ) );
            }

            string field = getField( line );
            if ( String.IsNullOrWhiteSpace( field ) )
            {
                // If could not find the first ':' char or if there wasn't a field declaration before ':'
                Type = LineType.Unknown;
                return;
            }

            Field = field.Trim();
            Type = EnumHelper.GetLineType( field.Trim().ToLowerInvariant() );
            Value = line.Substring( field.Length + 1 ).Trim(); //we remove <field>:
        }

        static string getField( string line )
        {
            var index = line.IndexOf( ':' );
            if ( index == -1 )
            {
                return String.Empty;
            }

            return line.Substring( 0, index );
        }
    }
}
