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
using System.ComponentModel;
using System.Web;

using Rock.Model;
using Rock.Net;

namespace Rock.Personalization
{
    /// <summary>
    /// Class BrowserRequestFilter.
    /// Implements the <see cref="Rock.Personalization.PersonalizationRequestFilter" />
    /// </summary>
    /// <seealso cref="Rock.Personalization.PersonalizationRequestFilter" />
    public class BrowserRequestFilter : PersonalizationRequestFilter
    {
        /// <summary>
        /// The ua parser
        /// </summary>
        private readonly static UAParser.Parser uaParser = UAParser.Parser.GetDefault();

        #region Configuration

        /// <summary>
        /// Gets or sets the browser family. Configured UI should only present browsers from <seealso cref="BrowserFamilyEnum">supported browsers</seealso>.
        /// </summary>
        /// <value>The browser family.</value>
        public BrowserFamilyEnum BrowserFamily { get; set; }

        /// <summary>
        /// Gets or sets the type of the version comparison.
        /// </summary>
        /// <value>The type of the version comparison.</value>
        public ComparisonType VersionComparisonType { get; set; }

        /// <summary>
        /// The major version
        /// </summary>
        public int MajorVersion { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>The unique identifier.</value>
        public Guid Guid { get; set; }

        #endregion Configuration

        /// <inheritdoc/>
        public override bool IsMatch( HttpRequest httpRequest )
        {
            return IsMatch( uaParser.ParseUserAgent( httpRequest.UserAgent ) );
        }

        /// <inheritdoc/>
        internal override bool IsMatch( RockRequestContext request )
        {
            if ( request.ClientInformation.Browser == null )
            {
                return false;
            }

            return IsMatch( request.ClientInformation.Browser.UA );
        }

        /// <summary>
        /// Determines whether the specified HTTP request meets the criteria of this filter.
        /// </summary>
        /// <param name="ua">The user agent object.</param>
        /// <returns><c>true</c> if the specified user agent is a match; otherwise, <c>false</c>.</returns>
        private bool IsMatch( UAParser.UserAgent ua )
        {
            var detectedFamily = ua.Family;

            var filteredBrowserFamily = this.BrowserFamily.ConvertToString();

            if ( !detectedFamily.Equals( filteredBrowserFamily, StringComparison.OrdinalIgnoreCase ) )
            {
                // If the detected family doesn't match the BrowserFamily for this filter,
                // return false since both the BrowserFamily AND MajorVersion condition must be met.
                return false;
            }

            var majorVersion = ua.Major;
            if ( majorVersion.IsNullOrWhiteSpace() )
            {
                return false;
            }

            return majorVersion.CompareTo( MajorVersion.ToString(), VersionComparisonType );
        }

        /// <summary>
        /// The supported browser family options that the Configuration UI for this filter should show,
        /// </summary>
        public enum BrowserFamilyEnum
        {
            /// <summary>
            /// Other Browser
            /// </summary>
            [Description( "Other" )]
            Other = 0,

            /// <summary>
            /// Chrome Browser
            /// </summary>
            [Description( "Chrome" )]
            Chrome = 1,

            /// <summary>
            /// Chrome Mobile Browser
            /// </summary>
            [Description( "Chrome Mobile" )]
            ChromeMobile = 2,

            /// <summary>
            /// Firefox Browser
            /// </summary>
            [Description( "Firefox" )]
            Firefox = 3,

            /// <summary>
            /// Firefox Mobile Browser
            /// </summary>
            [Description( "Firefox Mobile" )]
            FirefoxMobile = 4,

            /// <summary>
            /// Safari Browser
            /// </summary>
            [Description( "Safari" )]
            Safari = 5,

            /// <summary>
            /// Opera Browser
            /// </summary>
            [Description( "Opera" )]
            Opera = 6,

            /// <summary>
            /// Opera Mini Browser
            /// </summary>
            [Description( "Opera Mini" )]
            OperaMini = 7,

            /// <summary>
            /// Edge Browser
            /// </summary>
            [Description( "Edge" )]
            Edge = 8,

            /// <summary>
            /// Internet Explorer Browser
            /// </summary>
            [Description( "IE" )]
            IE = 9
        }
    }
}
