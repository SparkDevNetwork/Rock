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
using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Text;

namespace Rock.Web.v2
{
    /// <summary>
    /// Custom HTML Formatter to handle Rock:Zone elements and render any
    /// child elements as plain, unescaped HTML.
    /// </summary>
    internal class LavaPageHtmlFormatter : HtmlMarkupFormatter
    {
        /// <inheritdoc/>
        public override string OpenTag( IElement element, bool selfClosing )
        {
            if ( element.NodeName == "ROCK:ZONE" )
            {
                var temp = StringBuilderPool.Obtain();

                temp.Append( "<div id=\"zone-" );
                temp.Append( element.GetAttribute( "name" ).ToLower() );
                temp.Append( "\" class=\"zone-instance\"><div class=\"zone-content\">" );

                return temp.ToPool();
            }

            return base.OpenTag( element, selfClosing );
        }

        /// <inheritdoc/>
        public override string Text( ICharacterData text )
        {
            if ( text.Parent?.NodeName == "ROCK:ZONE" )
            {
                return text.Data;
            }

            return base.Text( text );
        }

        /// <inheritdoc/>
        public override string CloseTag( IElement element, bool selfClosing )
        {
            if ( element.NodeName == "ROCK:ZONE" )
            {
                return "</div></div>";
            }

            return base.CloseTag( element, selfClosing );
        }
    }
}
