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
using Rock.Security;

namespace Rock.Model
{
    public partial class MetricCategory
    {
        /// <inheritdoc/>
        public virtual bool IsAuthorized( string action, Person person )
        {
            return Authorization.Authorized( this, action, person );
        }

        /// <inheritdoc/>
        public virtual bool IsAllowedByDefault( string action )
        {
            return action == Authorization.VIEW;
        }

        /// <inheritdoc/>
        public virtual ISecured ParentAuthority
        {
            get
            {
                // Attempt to mirror what would happen in Model<T>.
                if ( Metric != null )
                {
                    return Metric;
                }
                else if ( this.Id == 0 )
                {
                    return new GlobalDefault();
                }
                else
                {
                    return new MetricCategory();
                }
            }
        }

        /// <inheritdoc/>
        public virtual bool IsPrivate( string action, Person person )
        {
            return Authorization.IsPrivate( this, action, person );
        }

        /// <inheritdoc/>
        public virtual void MakePrivate( string action, Person person, RockContext rockContext = null )
        {
            Authorization.MakePrivate( this, action, person, rockContext );
        }

        /// <inheritdoc/>
        public virtual void MakeUnPrivate( string action, Person person, RockContext rockContext = null )
        {
            Authorization.MakeUnPrivate( this, action, person, rockContext );
        }
    }
}
