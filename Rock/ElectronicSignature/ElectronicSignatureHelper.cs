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
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.ElectronicSignature
{
    /// <summary>
    /// Class ElectronicSignatureHelper.
    /// </summary>
    public static class ElectronicSignatureHelper
    {
        /// <summary>
        /// Sample Ted Decker signature data URL
        /// </summary>
        public const string SampleSignatureDataURL = SampleDrawnSignatures.SampleSignatureTedDeckerDataURL;

        /// <summary>
        /// Gets the signature document HTML (prior to signing)
        /// </summary>
        /// <param name="lavaTemplate">The lava template.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <returns>System.String.</returns>
        public static string GetSignatureDocumentHtml( string lavaTemplate, Dictionary<string, object> mergeFields )
        {
            return lavaTemplate.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Gets the signed document HTML.
        /// </summary>
        /// <param name="signatureDocumentHtml">The signature document HTML.</param>
        /// <param name="signatureInformation">The signature information.</param>
        /// <returns>System.String.</returns>
        public static string GetSignedDocumentHtml( string signatureDocumentHtml, string signatureInformation )
        {
            return signatureDocumentHtml + signatureInformation;
        }

        /// <summary>
        /// Gets the signature information HTML. This would be the HTML of the drawn or typed signature data.
        /// </summary>
        /// <param name="signatureInformationHtmlArgs">The signature information HTML arguments.</param>
        /// <returns>System.String.</returns>
        public static string GetSignatureInformationHtml( GetSignatureInformationHtmlOptions signatureInformationHtmlArgs )
        {
            string signatureHtml;

            if ( signatureInformationHtmlArgs.SignatureType == SignatureType.Drawn )
            {
                signatureHtml = $@"<img src='{signatureInformationHtmlArgs.DrawnSignatureDataUrl}' class='signature-image' />";
            }
            else
            {
                signatureHtml = $@"<span class='signature-typed'> {signatureInformationHtmlArgs.SignedName} <span>";
            }

            // If SignedByPerson is not null, use the SignedByPerson's name, regardless of Typed or Drawn
            // otherwise, use the Name they typed
            var signatureInfoName = signatureInformationHtmlArgs.SignedByPerson?.FullName;
            if ( signatureInfoName.IsNullOrWhiteSpace() )
            {
                signatureInfoName = signatureInformationHtmlArgs.SignedName;
            }

            // NOTE that the Signature Document will be rendered as a PDF without any external styles, so we'll put the styles here.
            var signatureCss = @"
<style>
    .signature-container {
        background-color: #f5f5f5;
        border: #000000 solid 1px;
        padding: 10px;
        page-break-inside: avoid;
    }

    .signature-container td,
    .signature-container table {
        border: none;
    }

    .signature-row {
        display: flex;
    }

    .signature-data {
        flex-grow: 1
    }

    .signature-details {
        font-size: 14px;
    }

    .signature-image {
        width: 400px;
    }

    .signature-ref {
        text-align: right;
        font-family: 'Courier New', Courier, monospace;
        font-size: 11px
    }
    
    .signature-check svg {
        width: 20px;
        margin-right: 12px;
        margin-top: 8px;
    }
</style>
";

            var signatureInformationHtml = $@"
{signatureCss}

<div class='signature-container'>
    <header class='signature-row'>
        <div class='col signature-data'>
            {signatureHtml}
        </div>
        <div class='col signature-details'>
            <div class='signature-fullname'><strong>Name:</strong> {signatureInfoName}</div>
            <div class='signature-datetime'><strong>Signed:</strong> {signatureInformationHtmlArgs.SignedDateTime.ToShortDateString()}</div>
            <div class='signature-ip-address'><strong>IP:</strong> {signatureInformationHtmlArgs.SignedClientIp}</div>
        </div>
    </header>
    <div>
        <table>
            <tr>
                <td class='signature-check'><svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 448 512'><path d='M400 32H48C21.49 32 0 53.49 0 80v352c0 26.51 21.49 48 48 48h352c26.51 0 48-21.49 48-48V80c0-26.51-21.49-48-48-48zm0 400H48V80h352v352zm-35.864-241.724L191.547 361.48c-4.705 4.667-12.303 4.637-16.97-.068l-90.781-91.516c-4.667-4.705-4.637-12.303.069-16.971l22.719-22.536c4.705-4.667 12.303-4.637 16.97.069l59.792 60.277 141.352-140.216c4.705-4.667 12.303-4.637 16.97.068l22.536 22.718c4.667 4.706 4.637 12.304-.068 16.971z'/></svg></td>
                <td class='signature-agreement'>
                    I agree to the statements above and understand this is a legal representation of my signature.
                </td>
            </tr>
        </table>
    </div>

    <p class='signature-ref'>Reference Code: {signatureInformationHtmlArgs.SignatureVerificationHash}</p>
</div>
";

            return signatureInformationHtml;
        }

        /// <summary>
        /// Sends the signature completion communication if the SignatureDocument's <see cref="SignatureDocumentTemplate.CompletionSystemCommunication"/> is assigned.
        /// Note that if this is called and the there is no <see cref="SignatureDocumentTemplate.CompletionSystemCommunication"/>, this method will return true, even though
        /// there was no email sent.
        /// </summary>
        /// <param name="signatureDocumentId">The signature document identifier.</param>
        /// <param name="errorMessages">The error messages.</param>
        public static bool SendSignatureCompletionCommunication( int signatureDocumentId, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            var rockContext = new RockContext();
            var signatureDocument = new SignatureDocumentService( rockContext ).Queryable()
                .Where( a => a.Id == signatureDocumentId )
                .Include( s => s.SignatureDocumentTemplate.CompletionSystemCommunication )
                .Include( s => s.SignedByPersonAlias.Person )
                .Include( s => s.BinaryFile )
                .FirstOrDefault();

            var completionSystemCommunication = signatureDocument.SignatureDocumentTemplate?.CompletionSystemCommunication;

            if ( completionSystemCommunication == null )
            {
                /* MP 02/08/2022

                If no completionSystemCommunication is configured, and this method is called,
                return true even though an email doesn't end up getting sent.
                We'll only return false if there are errors sending the email.

                */
                return true;
            }

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            mergeFields.Add( "SignatureDocument", signatureDocument );

            if ( signatureDocument.EntityTypeId.HasValue && signatureDocument.EntityId.HasValue )
            {
                var entityTypeType = EntityTypeCache.Get( signatureDocument.EntityTypeId.Value )?.GetEntityType();
                var entity = Reflection.GetIEntityForEntityType( entityTypeType, signatureDocument.EntityId.Value );
                mergeFields.Add( "Entity", entity );
            }

            var signedByPerson = signatureDocument.SignedByPersonAlias?.Person;
            var signedByEmail = signatureDocument.SignedByEmail;
            var pdfFile = signatureDocument.BinaryFile;

            var emailMessage = new RockEmailMessage( completionSystemCommunication );
            RockEmailMessageRecipient rockEmailMessageRecipient;
            if ( signedByPerson != null && signedByPerson.Email.Equals( signedByEmail, StringComparison.OrdinalIgnoreCase ) )
            {
                // if they specified the same email they already have, send it as a normal email message
                rockEmailMessageRecipient = new RockEmailMessageRecipient( signedByPerson, mergeFields );
            }
            else
            {
                // if they selected a different email address, don't change their email address. Just send to the specified email address.
                rockEmailMessageRecipient = RockEmailMessageRecipient.CreateAnonymous( signedByEmail, mergeFields );
            }

            emailMessage.Attachments.Add( pdfFile );

            emailMessage.AddRecipient( rockEmailMessageRecipient );

            // errors will be logged by send
            var successfullySent = emailMessage.Send( out errorMessages );
            if ( successfullySent )
            {
                signatureDocument.CompletionEmailSentDateTime = RockDateTime.Now;
                rockContext.SaveChanges();
            }

            return successfullySent;
        }
    }
}
