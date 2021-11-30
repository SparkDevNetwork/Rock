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

using AngleSharp.Dom;
using AngleSharp.Html;

namespace Rock.Utility
{
    class RockHtmlMarkupFormatter : HtmlMarkupFormatter
    {
        /// <inheritdoc />
        public override String OpenTag( IElement element, Boolean selfClosing )
        {
            bool allowSelfClose = IsValidSelfClose( element ) && selfClosing;
            return base.OpenTag( element, allowSelfClose );
        }

        /// <inheritdoc />
        public override String CloseTag( IElement element, Boolean selfClosing )
        {
            bool allowSelfClose = IsValidSelfClose( element ) && selfClosing;
            return base.CloseTag( element, allowSelfClose );
        }

        /// <summary>
        /// Returns false if the element should not implement a self closing tag.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        private bool IsValidSelfClose( IElement element )
        {
            List<string> voidElementNames = new List<string>
            {
                "area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "param", "source", "track", "wbr"
            };

            if ( voidElementNames.Contains( element.LocalName ) )
            {
                return true;
            }

            return false;
        }

    }
}
