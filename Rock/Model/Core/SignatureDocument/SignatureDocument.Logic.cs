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

using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    public partial class SignatureDocument
    {
        /// <summary>
        /// The data that was collected during a drawn signature type.
        /// This is an img data url. Example:
        /// <code>data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAngAAABkCAYAAAAVH...</code>
        /// This is stored as <seealso cref="SignatureDataEncrypted"/>.
        /// </summary>
        /// <value>The signature data.</value>
        [NotMapped]
        [HideFromReporting]
        [LavaHidden]
        public virtual string SignatureData
        {
            /*
            1/25/2022 MDP

            SignatureData is then base64 DataURL of the drawn signature. For example, the 'src' value of

            <img src='data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAngAAABkCAYAAAAVH...' />

            So, 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAngAAABkCAYAAAAVH...' is what SignatureData would be.

            Note that this is PII Data (https://en.wikipedia.org/wiki/Personal_data), so it needs to be stored
            in the database encrypted in the SignatureDataEncrypted field.
            */

            get
            {
                return Rock.Security.Encryption.DecryptString( this.SignatureDataEncrypted );
            }

            set
            {
                this.SignatureDataEncrypted = Rock.Security.Encryption.EncryptString( this.SignatureData );
            }
        }
    }
}
