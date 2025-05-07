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
using Rock.Attribute;

namespace Rock.Model
{
    /// <summary>
    /// Method arguments for <see cref="UserLoginService.UpdateLastLogin(UpdateLastLoginArgs)"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "17.1" )]
    public class UpdateLastLoginArgs
    {
        /// <summary>
        /// Gets or sets the user name of the individual who successfully logged in.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the optional value to override <see cref="HistoryLogin.SourceSiteId"/>.
        /// </summary>
        public int? SourceSiteIdOverride { get; set; }

        /// <summary>
        /// Gets or sets whether to skip writing this successful login to the individual's <see cref="HistoryLogin"/> log.
        /// </summary>
        /// <remarks>
        /// You should skip writing to the individual's history log if:
        /// [1] A <see cref="HistoryLogin"/> record will already be written elsewhere in Rock, and we need to prevent
        /// writing a duplicate record here (e.g. a successful OIDC login attempt will result in writing to the
        /// individual's history log AFTER an access token is issued).
        /// [2] This "login" does not actually represent a true login action taken by the individual, but is instead the
        /// result of an auth cookie preexisting in the browser or a mobile launch packet being issued.
        /// <para>
        /// If you're unsure, pass <see langword="false"/> for this parameter, at the risk of duplicate successful
        /// logins being logged.
        /// </para>
        /// </remarks>
        public bool ShouldSkipWritingHistoryLog { get; set; }
    }
}
