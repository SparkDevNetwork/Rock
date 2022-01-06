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

namespace Rock.Model
{
    public partial class PersonAlias
    {
        #region Methods

        /// <summary>
        /// Gets the previous encrypted key for the <see cref="Rock.Model.Person"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the previous encrypted key for the <see cref="Rock.Model.Person"/>.
        /// </value>
        [NotMapped]
        public virtual string AliasEncryptedKey
        {
            get
            {
                if ( this.AliasPersonId.HasValue && this.AliasPersonGuid.HasValue )
                {
                    string identifier = this.AliasPersonId.ToString() + ">" + this.AliasPersonGuid.ToString();
                    return Rock.Security.Encryption.EncryptString( identifier );
                }

                return null;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( !string.IsNullOrWhiteSpace( Name ) )
            {
                return Name;
            }

            if ( this.Person != null )
            {
                return this.Person.ToString();
            }

            return string.Format( "{0}->{1}", this.AliasPersonId, this.PersonId );
        }

        #endregion
    }
}
