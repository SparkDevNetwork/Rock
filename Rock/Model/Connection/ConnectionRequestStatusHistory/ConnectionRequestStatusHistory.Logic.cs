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

using Rock.Security;

namespace Rock.Model
{
    public partial class ConnectionRequestStatusHistory
    {
        #region ISecured

        /// <inheritdoc/>
        public override ISecured ParentAuthority => ConnectionRequest ?? ConnectionStatus ?? base.ParentAuthority;

        /// <inheritdoc/>
        public override bool IsAuthorized( string action, Person person )
        {
            /*
                11/23/2026 - MSE

                A status history row belongs to its connection request, so it must be
                secured like the request. ParentAuthority alone walks the request's
                opportunity and type but never calls ConnectionRequest.IsAuthorized, which
                also grants the assigned connector access when the type has request
                security turned on. A new row posted through REST v2 is still checked
                against its request, because CrudEndpointHelper.Create makes it a proxy
                and ConnectionRequest lazy-loads from ConnectionRequestId. Only a row with
                no request, or one that is not a proxy attached to a context, falls back
                to the old check through ParentAuthority.

                Reason: Do not show or change history rows of requests the person cannot access.
            */
            return ConnectionRequest != null ? ConnectionRequest.IsAuthorized( action, person ) : base.IsAuthorized( action, person );
        }

        #endregion
    }
}
