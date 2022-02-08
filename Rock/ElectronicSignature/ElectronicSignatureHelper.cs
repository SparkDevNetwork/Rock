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
        /// Sample signature data URL
        /// </summary>
        public const string SampleSignatureDataURL = @"data:image/png;base64,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";

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
        /// Gets the signature information HTML. This would be the HTML of the drawn or typed signature data
        /// </summary>
        /// <param name="signatureInformationHtmlArgs">The signature information HTML arguments.</param>
        /// <returns>System.String.</returns>
        public static string GetSignatureInformationHtml( GetSignatureInformationHtmlArgs signatureInformationHtmlArgs )
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

            var signatureInfoName = signatureInformationHtmlArgs.SignedName;
            if ( signatureInfoName.IsNullOrWhiteSpace() )
            {
                signatureInfoName = signatureInformationHtmlArgs.SignedByPerson?.FullName;
            }


            var signatureCss = @"
<style>
    .signature-container {
        background-color: #f5f5f5;
        border: #000000 solid 1px;
        padding: 10px;
        width: 600px;
        page-break-inside: avoid;
    }

    .signature-row {
        display: flex;
    }

    .signature-data {
        flex: auto;
    }

    .signature-image {
        width: 100%;
    }

    .signature-details {
        flex:auto;
        white-space: nowrap;
    }
    
    .signature-ref {
        text-align: right;
        font-family: 'Courier New', Courier, monospace;
        font-size: 11px
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
            <div class='signature-fullname'>Name: {signatureInfoName}</div>
            <div class='signature-datetime'>Signed: {signatureInformationHtmlArgs.SignedDateTime.ToShortDateString()}</div>
            <div class='signature-ip-address'>IP: {signatureInformationHtmlArgs.SignedClientIp}</div>
        </div>
    </header>
    <div>
        <input type='checkbox' class='signature-checkbox' checked='true' /> <span class='signature-agreement'>I agree to
            the statements above and understand this is a legal representation of my signature.</span>
    </div>

    <p class='signature-ref'>ref: {signatureInformationHtmlArgs.SignatureVerificationHash}</p>
</div>
";

            return signatureInformationHtml;
        }

        /// <summary>
        /// Sends the signature completion communication.
        /// </summary>
        /// <param name="signatureDocumentId">The signature document identifier.</param>
        /// <param name="errorMessages">The error messages.</param>
        public static bool SendSignatureCompletionCommunication( int signatureDocumentId, out List<string> errorMessages )
        {
            return SendSignatureCompletionCommunication( signatureDocumentId, ( Dictionary<string, object> ) null, out errorMessages );
        }

        /// <summary>
        /// Sends the signature completion document communication.
        /// </summary>
        /// <param name="signatureDocumentId">The signature document identifier.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool SendSignatureCompletionCommunication( int signatureDocumentId, Dictionary<string, object> mergeFields, out List<string> errorMessages )
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
                errorMessages.Add( "Signature Document doesn't have a CompletionSystemCommunication." );
                return false;
            }

            if ( mergeFields == null )
            {
                mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            }

            mergeFields.AddOrIgnore( "SignatureDocument", signatureDocument );

            if ( signatureDocument.EntityTypeId.HasValue && signatureDocument.EntityId.HasValue )
            {
                var entityTypeType = EntityTypeCache.Get( signatureDocument.EntityTypeId.Value )?.GetEntityType();
                var entity = Reflection.GetIEntityForEntityType( entityTypeType, signatureDocument.EntityId.Value );
                mergeFields.AddOrIgnore( "Entity", entity );
            }

            var signedByPerson = signatureDocument.SignedByPersonAlias?.Person;
            var signedByEmail = signatureDocument.SignedByEmail;
            var pdfFile = signatureDocument.BinaryFile;

            var emailMessage = new RockEmailMessage( completionSystemCommunication );
            RockEmailMessageRecipient rockEmailMessageRecipient;
            if ( signedByPerson.Email.Equals( signedByEmail, StringComparison.OrdinalIgnoreCase ) )
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
