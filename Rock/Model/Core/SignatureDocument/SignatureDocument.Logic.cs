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
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    public partial class SignatureDocument
    {
        /// <summary>
        /// Calculates the signature verification hash.
        /// </summary>
        /// <returns>System.String.</returns>
        public string CalculateSignatureVerificationHash()
        {
            /* 02/11/2022 MDP

            The SignatureVerificationHash can be used to verify that nothing about the signed document or signature has
            changed since it was originally signed.

            To do that we'll have use xxHash on the signature related data that we store in SignatureDocument (see fields that we use below).
            This hash will be stored in the SignedDocument record and also in the Signed Document PDF.

            NOTE: CalculateSignatureVerificationHash must be deterministic. So don't change the implementation without approval.

            */

            string signedDateTimeData;
            if ( SignedDateTime.HasValue )
            {
                // to make sure DateTime has same precision when coming back out of the database, make it DateTimeKind.Unspecified and rounded to closest millisecond.
                var signedDateTime = this.SignedDateTime.Value;
                var consistentDateTime = new DateTime( signedDateTime.Year, signedDateTime.Month, signedDateTime.Day, signedDateTime.Hour, signedDateTime.Minute, signedDateTime.Second, signedDateTime.Millisecond, DateTimeKind.Unspecified );

                signedDateTimeData = consistentDateTime.ToISO8601DateString();
            }
            else
            {
                signedDateTimeData = string.Empty;
            }

            // to make to we get a deterministic hash, concat the data (vs using JSON, etc)
            var concatString = $@"{this.SignedDocumentText}|
{this.SignedClientIp}|
{this.SignedClientUserAgent}|
{signedDateTimeData}|
{this.SignedByPersonAliasId}|
{this.SignatureData}|
{this.SignedName}";

            // Hash in a way that'll will
            string hashed;
            using ( var crypt = new SHA1Managed() )
            {
                var hash = crypt.ComputeHash( Encoding.UTF8.GetBytes( concatString ) );
                var hashBase64 = Convert.ToBase64String( hash );

                // replace base64's special chars /+= https://en.wikipedia.org/wiki/Base64 with x
                hashed = hashBase64.Replace( '/', 'x' ).Replace( '+', 'x' ).Replace( '=', 'x' );
            }

            const string revisionPrefix = "A";

            // prepend with a revision just in case we have a future change to the implementation
            var verificationHash = $"{revisionPrefix}{hashed}";

            return verificationHash;
        }

        private static UAParser.Parser uaParser = UAParser.Parser.GetDefault();

        /// <summary>
        /// Gets the formatted user agent.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetFormattedUserAgent()
        {
            var userAgent = this.SignedClientUserAgent ?? string.Empty;
            var deviceOs = uaParser.ParseOS( userAgent ).ToString();
            var deviceApplication = uaParser.ParseUserAgent( userAgent ).ToString();
            var deviceClientType = InteractionDeviceType.GetClientType( userAgent );

            return $@"{deviceApplication}
{deviceOs}
{deviceClientType}";
        }

        /// <summary>
        /// Returns true of this document was generated using a legacy document provider.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool UsesLegacyDocumentProvider()
        {
            bool isLegacyProvider = this.SignatureDocumentTemplate?.ProviderEntityTypeId != null;
            return isLegacyProvider;
        }

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
                this.SignatureDataEncrypted = Rock.Security.Encryption.EncryptString( value );
            }
        }
    }
}
