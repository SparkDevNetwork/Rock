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

using Rock.ViewModels.Utility;
using Rock.Model;

namespace Rock.ViewModels.Blocks.Core.SignatureDocumentDetail
{
    public class SignatureDocumentBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the applies to person alias.
        /// </summary>
        public ListItemBag AppliesToPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the applies to person alias identifier.
        /// </summary>
        public int? AppliesToPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the assigned to person alias.
        /// </summary>
        public ListItemBag AssignedToPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the assigned to person alias identifier.
        /// </summary>
        public int? AssignedToPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the binary file.
        /// </summary>
        public ListItemBag BinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the binary file identifier.
        /// </summary>
        public int? BinaryFileId { get; set; }

        /// <summary>
        /// The date and time the document completion email was sent.
        /// </summary>
        public DateTime? CompletionEmailSentDateTime { get; set; }

        /// <summary>
        /// Gets or sets the document key.
        /// </summary>
        public string DocumentKey { get; set; }

        /// <summary>
        /// The ID of the entity to which the document is related.
        /// </summary>
        public int? EntityId { get; set; }

        /// <summary>
        /// The EntityType that this document is related to (example Rock.Model.Registration)
        /// </summary>
        public ListItemBag EntityType { get; set; }

        /// <summary>
        /// The EntityType that this document is related to (example Rock.Model.Registration)
        /// </summary>
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the invite count.
        /// </summary>
        public int InviteCount { get; set; }

        /// <summary>
        /// Gets or sets the request date.
        /// </summary>
        public DateTime? LastInviteDate { get; set; }

        /// <summary>
        /// Gets or sets the last status date.
        /// </summary>
        public DateTime? LastStatusDate { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The encrypted data that was collected during a drawn signature type.
        /// Use Rock.Model.SignatureDocument.SignatureData to set this from the unencrypted drawn signature.
        /// </summary>
        public string SignatureDataEncrypted { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.SignatureDocumentTemplate that is being executed in this persisted SignatureDocument instance.
        /// </summary>
        public ListItemBag SignatureDocumentTemplate { get; set; }

        /// <summary>
        /// Gets or sets the SignatureDocumentTemplateId of the Rock.Model.SignatureDocumentTemplate that this SignatureDocument instance is executing.
        /// </summary>
        public int SignatureDocumentTemplateId { get; set; }

        /// <summary>
        /// The computed SHA1 hash for the SignedDocumentText, SignedClientIP address, SignedClientUserAgent, SignedDateTime, SignedByPersonAliasId, SignatureData, and SignedName.
        /// This hash can be used to prove the authenticity of the unaltered signature document.
        /// This is only calculated once during the pre-save event when the SignedDateTime was originally null/empty but now has a value.
        /// </summary>
        public string SignatureVerificationHash { get; set; }

        /// <summary>
        /// The email address that was used to send the completion receipt.
        /// </summary>
        public string SignedByEmail { get; set; }

        /// <summary>
        /// Gets or sets the signed by person alias.
        /// </summary>
        public ListItemBag SignedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the signed by person alias identifier.
        /// </summary>
        public int? SignedByPersonAliasId { get; set; }

        /// <summary>
        /// The observed IP address of the client system of the individual who signed the document.
        /// </summary>
        public string SignedClientIp { get; set; }

        /// <summary>
        /// The observed 'user agent' of the client system of the individual who signed the document.
        /// </summary>
        public string SignedClientUserAgent { get; set; }

        /// <summary>
        /// The date and time the document was signed.
        /// </summary>
        public DateTime? SignedDateTime { get; set; }

        /// <summary>
        /// The resulting text/document using the Lava template from the Rock.Model.SignatureDocumentTemplate at the time the document was signed.
        /// Does not include the signature data. It would be what they saw just prior to signing.
        /// </summary>
        public string SignedDocumentText { get; set; }

        /// <summary>
        /// The name of the individual who signed the document.
        /// </summary>
        public string SignedName { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public SignatureDocumentStatus Status { get; set; }
    }
}
