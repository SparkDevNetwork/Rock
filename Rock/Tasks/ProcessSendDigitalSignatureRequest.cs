﻿// <copyright>
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
using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Model;

namespace Rock.Tasks
{
    /// <summary>
    /// Sends a digital signature request
    /// </summary>
    public sealed class ProcessSendDigitalSignatureRequest : BusStartedTask<ProcessSendDigitalSignatureRequest.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            using ( var rockContext = new RockContext() )
            {
                var documentService = new SignatureDocumentService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var appliesPerson = personAliasService.GetPerson( message.AppliesToPersonAliasId );
                var assignedPerson = personAliasService.GetPerson( message.AssignedToPersonAliasId );

                if ( !documentService.Queryable().Any( d =>
                        d.SignatureDocumentTemplateId == message.SignatureDocumentTemplateId &&
                        d.AppliesToPersonAliasId.HasValue &&
                        d.AppliesToPersonAliasId.Value == message.AppliesToPersonAliasId &&
                        d.Status == SignatureDocumentStatus.Signed ) )
                {
                    var documentTypeService = new SignatureDocumentTemplateService( rockContext );
                    var signatureDocumentTemplate = documentTypeService.Get( message.SignatureDocumentTemplateId );

                    var errorMessages = new List<string>();
                    if ( documentTypeService.SendLegacyProviderDocument( signatureDocumentTemplate, appliesPerson, assignedPerson, message.DocumentName, message.Email, out errorMessages ) )
                    {
                        rockContext.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the signature document type identifier.
            /// </summary>
            /// <value>
            /// The signature document type identifier.
            /// </value>
            public int SignatureDocumentTemplateId { get; set; }

            /// <summary>
            /// Gets or sets the applies to person alias identifier.
            /// </summary>
            /// <value>
            /// The applies to person alias identifier.
            /// </value>
            public int AppliesToPersonAliasId { get; set; }

            /// <summary>
            /// Gets or sets the assigned to person alias identifier.
            /// </summary>
            /// <value>
            /// The assigned to person alias identifier.
            /// </value>
            public int AssignedToPersonAliasId { get; set; }

            /// <summary>
            /// Gets or sets the name of the document.
            /// </summary>
            /// <value>
            /// The name of the document.
            /// </value>
            public string DocumentName { get; set; }

            /// <summary>
            /// Gets or sets the alternate email.
            /// </summary>
            /// <value>
            /// The alternate email.
            /// </value>
            public string Email { get; set; }
        }
    }
}