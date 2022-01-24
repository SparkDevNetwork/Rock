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

using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Rock.Model
{
    public partial class Auth
    {
        /// <summary>
        /// Gets or sets the Audit Log.
        /// </summary>
        /// <value>
        /// The Audit Log.
        /// </value>
        [NotMapped]
        private AuthAuditLog AuthAuditLog { get; set; }

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat( "{0} ", this.AllowOrDeny == "A" ? "Allow" : "Deny" );

            if ( SpecialRole != Model.SpecialRole.None )
            {
                sb.AppendFormat( "{0} ", SpecialRole.ToStringSafe().SplitCase() );
            }
            else if ( PersonAlias != null )
            {
                sb.AppendFormat( "{0} ", PersonAlias.ToStringSafe() );
            }
            else if ( Group != null )
            {
                sb.AppendFormat( "{0} ", Group.ToStringSafe() );
            }

            sb.AppendFormat( "{0} Access", Action );

            return sb.ToString();
        }

        /// <summary>
        /// Returns the default authorization for a specific action.
        /// </summary>
        /// <param name="action">A <see cref="System.String"/> representing the name of the action.</param>
        /// <returns>A <see cref="System.Boolean"/> that is <c>true</c> if the specified action is allowed by default; otherwise <c>false</c>.</returns>
        public override bool IsAllowedByDefault( string action )
        {
            return false;
        }

        #endregion Public Methods
    }
}
