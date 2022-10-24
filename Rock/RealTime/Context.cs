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

using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

using Rock.Utility;

namespace Rock.RealTime
{
    /// <summary>
    /// Default implementation of <see cref="IContext"/>.
    /// </summary>
    internal class Context : IContext, IContextInternal
    {
        #region Fields

        /// <summary>
        /// The engine that created this connection context.
        /// </summary>
        private readonly Engine _engine;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public string ConnectionId { get; set; }

        /// <inheritdoc/>
        public int? CurrentPersonId { get; set; }

        /// <inheritdoc/>
        public int? VisitorAliasId { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="userPrincipal">The principal that identifies the current in person for the request.</param>
        /// <param name="engine">The engine that created this connection context.</param>
        internal Context( string connectionId, IPrincipal userPrincipal, Engine engine )
        {
            _engine = engine;
            ConnectionId = connectionId;

            if ( userPrincipal is ClaimsPrincipal claimsPrincipal )
            {
                CurrentPersonId = claimsPrincipal.Claims
                    .FirstOrDefault( c => c.Type == "rock:person" )
                    ?.Value
                    .AsIntegerOrNull();

                var visitorAliasIdKey = claimsPrincipal.Claims
                    .FirstOrDefault( c => c.Type == "rock:visitor" )
                    ?.Value;

                if ( visitorAliasIdKey.IsNotNullOrWhiteSpace() )
                {
                    VisitorAliasId = IdHasher.Instance.GetId( visitorAliasIdKey );
                }
            }
        }

        #endregion

        #region IContextInternal

        /// <inheritdoc/>
        Engine IContextInternal.Engine => _engine;

        #endregion
    }
}
