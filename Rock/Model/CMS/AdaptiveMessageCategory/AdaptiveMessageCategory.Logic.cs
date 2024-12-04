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

using Rock.Data;

namespace Rock.Model
{
    public partial class AdaptiveMessageCategory
    {  /// <summary>
       /// Return <c>true</c> if the user is authorized to perform the selected action on this object.
       /// </summary>
       /// <param name="action">The action.</param>
       /// <param name="person">The person.</param>
       /// <returns>
       ///   <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
       /// </returns>
        public virtual bool IsAuthorized( string action, Person person )
        {
            if ( this.AdaptiveMessage != null )
            {
                return this.AdaptiveMessage.IsAuthorized( action, person );
            }
            else
            {
                return action == Rock.Security.Authorization.VIEW;
            }
        }

        /// <summary>
        /// If a user or role is not specifically allowed or denied to perform the selected action,
        /// return <c>true</c> if they should be allowed anyway or <c>false</c> if not.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public virtual bool IsAllowedByDefault( string action )
        {
            return this.AdaptiveMessage.IsAllowedByDefault( action );
        }

        /// <summary>
        /// A parent authority.  A user is specifically allowed or denied access to
        /// this object based on the Metric ParentAuthority of the current object.
        /// </summary>
        public virtual Security.ISecured ParentAuthority
        {
            get
            {
                return this.AdaptiveMessage.ParentAuthority;
            }
        }

        /// <summary>
        /// Determines whether the specified action is private (Only the current user has access).
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is private; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsPrivate( string action, Person person )
        {
            return this.AdaptiveMessage.IsPrivate( action, person );
        }

        /// <summary>
        /// Makes the action on the current entity private (Only the current user will have access).
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        public virtual void MakePrivate( string action, Person person, RockContext rockContext = null )
        {
            this.AdaptiveMessage.MakePrivate( action, person, rockContext );
        }

        /// <summary>
        /// If action on the current entity is private, removes security that made it private.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        public virtual void MakeUnPrivate( string action, Person person, RockContext rockContext = null )
        {
            this.AdaptiveMessage.MakeUnPrivate( action, person, rockContext );
        }
    }
}