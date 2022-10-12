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
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data Access class for <see cref="Rock.Model.SignatureDocument"/> entity objects.
    /// </summary>
    public partial class SignatureDocumentService 
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.SignatureDocument">SignatureDocuments</see> that belong to a specified <see cref="Rock.Model.SignatureDocumentTemplate"/> retrieved by the SignatureDocumentTemplate's SignatureDocumentTemplateId.
        /// </summary>
        /// <param name="definedTypeId">A <see cref="System.Int32"/> representing the SignatureDocumentTemplateId of the <see cref="Rock.Model.SignatureDocumentTemplate"/> to retrieve <see cref="Rock.Model.SignatureDocument">SignatureDocuments</see> for.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.SignatureDocument">SignatureDocuments</see> that belong to the specified <see cref="Rock.Model.SignatureDocumentTemplate"/>. The <see cref="Rock.Model.SignatureDocument">SignatureDocuments</see> will 
        /// be ordered by the <see cref="SignatureDocument">SignatureDocument's</see> Order property.</returns>
        public IOrderedQueryable<SignatureDocument> GetBySignatureDocumentTemplateId( int definedTypeId )
        {
            return Queryable()
                .Where( t => t.SignatureDocumentTemplateId == definedTypeId )
                .OrderBy( t => t.Name );
        }

        /// <summary>
        /// Gets the by document key.
        /// </summary>
        /// <param name="documentKey">The document key.</param>
        /// <returns></returns>
        public SignatureDocument GetByDocumentKey( string documentKey )
        {
            return this.Queryable().Where( d => d.DocumentKey == documentKey ).FirstOrDefault();
        }

        /// <summary>
        /// Calculates the signature verification hash from the data contained
        /// in the document.
        /// </summary>
        /// <returns>System.String.</returns>
        public static string CalculateSignatureVerificationHash( SignatureDocument document )
        {
            /* 02/11/2022 MDP

            The SignatureVerificationHash can be used to verify that nothing about the signed document or signature has
            changed since it was originally signed.

            To do that we'll have use xxHash on the signature related data that we store in SignatureDocument (see fields that we use below).
            This hash will be stored in the SignedDocument record and also in the Signed Document PDF.

            NOTE: CalculateSignatureVerificationHash must be deterministic. So don't change the implementation without approval.

            */

            string signedDateTimeData;
            if ( document.SignedDateTime.HasValue )
            {
                // to make sure DateTime has same precision when coming back out of the database, make it DateTimeKind.Unspecified and rounded to closest millisecond.
                var signedDateTime = document.SignedDateTime.Value;
                var consistentDateTime = new DateTime( signedDateTime.Year, signedDateTime.Month, signedDateTime.Day, signedDateTime.Hour, signedDateTime.Minute, signedDateTime.Second, signedDateTime.Millisecond, DateTimeKind.Unspecified );

                signedDateTimeData = consistentDateTime.ToISO8601DateString();
            }
            else
            {
                signedDateTimeData = string.Empty;
            }

            // to make to we get a deterministic hash, concat the data (vs using JSON, etc)
            var concatString = $@"{document.SignedDocumentText}|
{document.SignedClientIp}|
{document.SignedClientUserAgent}|
{signedDateTimeData}|
{document.SignedByPersonAliasId}|
{document.SignatureData}|
{document.SignedName}";

            // Hash in a way that'll will
            string hashed;
            using ( var crypt = new SHA1Managed() )
            {
                var hash = crypt.ComputeHash( Encoding.UTF8.GetBytes( concatString ) );
                var hashBase64 = Convert.ToBase64String( hash );

                // Replace base64's special chars /+= https://en.wikipedia.org/wiki/Base64 with "x".
                // This is an intentional modification to the base64 data to make it look
                // more friendly. We understand it is a non-reversable change - meaning we can't
                // decode the base64 data back to the hashed binary data.
                hashed = hashBase64.Replace( '/', 'x' ).Replace( '+', 'x' ).Replace( '=', 'x' );
            }

            const string revisionPrefix = "A";

            // prepend with a revision just in case we have a future change to the implementation
            var verificationHash = $"{revisionPrefix}{hashed}";

            return verificationHash;
        }
    }
}
