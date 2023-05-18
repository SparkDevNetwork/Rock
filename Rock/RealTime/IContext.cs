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

namespace Rock.RealTime
{
    /// <summary>
    /// Provides the context about an incoming request to a topic handler.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "1.14.1" )]
    public interface IContext
    {
        /// <summary>
        /// Gets the unique connection identifier for the current request.
        /// </summary>
        /// <value>The unique connection identifier for the current request.</value>
        string ConnectionId { get; }

        /// <summary>
        /// Gets the identifier of the logged in person.
        /// </summary>
        /// <value>The identifier of the logged in person.</value>
        int? CurrentPersonId { get; }

        /// <summary>
        /// Gets the visitor person alias identifier. If this has a value then
        /// the person is being tracked as a vistor to the site. This value
        /// will be the same across all connections from the same browser.
        /// </summary>
        /// <value>The visitor person alias identifier.</value>
        int? VisitorAliasId { get; }
    }
}
