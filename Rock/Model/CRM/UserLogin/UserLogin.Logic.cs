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
using System.Runtime.Serialization;

namespace Rock.Model
{
    public partial class UserLogin
    {
        /// <summary>
        /// Gets an encrypted confirmation code for the UserLogin.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the encrypted confirmation code.
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual string ConfirmationCode
        {
            get
            {
                string identifier = string.Format( "ROCK|{0}|{1}|{2}", this.EncryptedKey.ToString(), this.UserName, RockDateTime.Now.Ticks );
                string encryptedCode;
                if ( Rock.Security.Encryption.TryEncryptString( identifier, out encryptedCode ) )
                {
                    return encryptedCode;
                }
                else
                {
                    return null;
                }
            }
            private set { }
        }

        /// <summary>
        /// Gets a URL encoded  and encrypted confirmation code.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a URL encoded and encrypted confirmation code.
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual string ConfirmationCodeEncoded
        {
            get
            {
                return System.Net.WebUtility.UrlEncode( ConfirmationCode );
            }
            private set { }
        }

        /// <summary>
        /// Returns a boolean flag indicating if the provided action is allowed by default
        /// </summary>
        /// <param name="action">A <see cref="System.String"/> representing the action.</param>
        /// <returns>A <see cref="System.Boolean"/> flag indicating if the provided action is allowed by default.</returns>
        public override bool IsAllowedByDefault( string action )
        {
            return false;
        }
    }
}
