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

using Rock.Attribute;

namespace Rock.Utility.HtmlSanitization
{
    /// <summary>
    /// Removes every element, attribute, URL and CSS declaration from an HTML
    /// fragment that the given options do not allow. Resolve it from
    /// <see cref="Rock.Configuration.RockApp"/>, or call the
    /// <c>SanitizeHtml( html, options )</c> string extension method.
    /// </summary>
    [RockInternal( "17.12" )]
    public interface IHtmlAllowListSanitizer
    {
        /// <summary>
        /// Sanitizes an HTML fragment meant for the body of a page. A full
        /// HTML page comes back as its sanitized body content only. The
        /// options are made read-only.
        /// </summary>
        /// <param name="html">The HTML to sanitize.</param>
        /// <param name="options">The allow rules to apply.</param>
        /// <returns>The sanitized HTML, or <paramref name="html"/> itself when it is <c>null</c> or empty.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is <c>null</c>.</exception>
        string Sanitize( string html, HtmlSanitizeOptions options );
    }
}
