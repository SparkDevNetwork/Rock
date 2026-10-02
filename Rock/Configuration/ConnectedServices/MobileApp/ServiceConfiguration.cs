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

namespace Rock.Configuration.ConnectedServices.MobileApp
{
    /// <summary>
    /// The church's enrollment in the shared (multitenant) mobile application,
    /// as Rock remembers it between visits to the Connected Services card.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Unlike the Rock Intelligence configuration this is not read from
    ///         the manifest. The administrator owns these values and the church
    ///         directory only echoes them back, so a manifest refresh leaves
    ///         this object alone.
    ///     </para>
    ///     <para>
    ///         Disabling keeps everything except <see cref="IsEnrolled"/>, because
    ///         the church code and its printed QR posters come back to life when
    ///         the church enables again.
    ///     </para>
    /// </remarks>
    internal class ServiceConfiguration
    {
        /// <summary>
        /// Whether the church is currently listed in the shared mobile
        /// application. Drives Enable versus Update and Disable on the card.
        /// </summary>
        public bool IsEnrolled { get; set; }

        /// <summary>
        /// The code the church directory minted for this church on its first
        /// enrollment. Stable across disable and re-enable.
        /// </summary>
        public string ChurchCode { get; set; }

        /// <summary>
        /// The URL to print on the church's QR poster, as returned by the
        /// church directory.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// The name shown for the church in the directory.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The directory color, as <c>#RRGGBB</c>. This is not the app's own
        /// palette, which the control panel holds.
        /// </summary>
        public string BrandColor { get; set; }

        /// <summary>
        /// The directory logo, if one was uploaded. The URL is composed when it
        /// is sent and never stored.
        /// </summary>
        public Guid? LogoBinaryFileGuid { get; set; }
    }
}
