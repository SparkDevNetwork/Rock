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

using Microsoft.Extensions.DependencyInjection;

using Rock.Attribute;
using Rock.Configuration;
using Rock.Utility.HtmlSanitization;

namespace Rock
{
    /// <summary>
    /// String extensions for allowlist HTML sanitization.
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Removes every element, attribute, URL and CSS declaration that
        /// <paramref name="options"/> does not allow, using the
        /// <see cref="IHtmlAllowListSanitizer"/> registered with <see cref="RockApp"/>.
        /// </summary>
        /// <param name="html">The HTML to sanitize.</param>
        /// <param name="options">The allow rules, such as <see cref="HtmlSanitizeOptions.Simple"/> or <see cref="HtmlSanitizeOptions.Rich"/>.</param>
        /// <returns>The sanitized HTML, or <paramref name="html"/> itself when it is <c>null</c> or empty.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">No <see cref="IHtmlAllowListSanitizer"/> is registered.</exception>
        [RockInternal( "17.12" )]
        public static string SanitizeHtml( this string html, HtmlSanitizeOptions options )
        {
            return RockApp.Current.GetRequiredService<IHtmlAllowListSanitizer>().Sanitize( html, options );
        }
    }
}
