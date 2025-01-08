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
using System.Collections.Generic;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Security
{
    /// <summary>
    /// A generic ISecured entity that restricts access by default.
    /// </summary>
    public class GlobalRestrictedDefault : ISecured
    {
        /// <inheritdoc/>
        public int TypeId => EntityTypeCache.Get( this.GetType() ).Id;

        /// <inheritdoc/>
        public string TypeName => this.GetType().FullName;

        /// <inheritdoc/>
        public int Id => 0;

        /// <inheritdoc/>
        public ISecured ParentAuthority => null;

        /// <inheritdoc/>
        public virtual Security.ISecured ParentAuthorityPre => null;

        /// <inheritdoc/>
        public virtual Dictionary<string, string> SupportedActions
        {
            get
            {
                return new Dictionary<string, string>
                {
                    { Authorization.VIEW, "The roles and/or users that have access to view." },
                    { Authorization.EDIT, "The roles and/or users that have access to edit." },
                    { Authorization.ADMINISTRATE, "The roles and/or users that have access to administrate." }
                };
            }
        }

        /// <inheritdoc/>
        public bool IsAuthorized( string action, Rock.Model.Person person )
        {
            return Authorization.Authorized( this, action, person );
        }

        /// <inheritdoc/>
        public bool IsAllowedByDefault( string action )
        {
            // RestrictedDefault is the ultimate base Parent Authority of items
            // that need to have restricted access by default. So if we get here
            // then always deny.
            return false;
        }

        /// <inheritdoc/>
        public bool IsPrivate( string action, Person person )
        {
            return Authorization.IsPrivate( this, action, person );
        }

        /// <inheritdoc/>
        public void MakePrivate( string action, Person person, RockContext rockContext = null )
        {
            Authorization.MakePrivate( this, action, person, rockContext );
        }

        /// <inheritdoc/>
        public void MakeUnPrivate( string action, Person person, RockContext rockContext = null )
        {
            Authorization.MakeUnPrivate( this, action, person, rockContext );
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Empty;
        }
    }
}
