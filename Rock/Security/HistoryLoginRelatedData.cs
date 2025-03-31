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

namespace Rock.Security
{
    /// <summary>
    /// Additional data related to a login history record.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "17.0" )]
    public class HistoryLoginRelatedData
    {
        /// <summary>
        /// Gets or sets the full name of the person who was impersonating the person associated with the login history.
        /// </summary>
        public string ImpersonatedByPersonFullName { get; set; }

        /// <summary>
        /// Gets or sets the optional context of the login attempt.
        /// </summary>
        public string LoginContext { get; set; }
    }
}
