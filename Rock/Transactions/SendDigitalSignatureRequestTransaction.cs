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

namespace Rock.Transactions
{
    /// <summary>
    /// Writes entity audits 
    /// </summary>
    public class SendDigitalSignatureRequestTransaction : ITransaction
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

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                var documentService = new SignatureDocumentService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var appliesPerson = personAliasService.GetPerson( AppliesToPersonAliasId );
                var assignedPerson = personAliasService.GetPerson( AssignedToPersonAliasId );

                if ( !documentService.Queryable().Any( d =>
                        d.AppliesToPersonAliasId.HasValue && 
                        d.AppliesToPersonAliasId.Value == AppliesToPersonAliasId &&
                        d.Status == SignatureDocumentStatus.Signed ) )
                {
                    var documentTypeService = new SignatureDocumentTemplateService( rockContext );
                    var SignatureDocumentTemplate = documentTypeService.Get( SignatureDocumentTemplateId );

                    var errorMessages = new List<string>();
                    if ( documentTypeService.SendDocument( SignatureDocumentTemplate, appliesPerson, assignedPerson, DocumentName, Email, out errorMessages ) )
                    {
                        rockContext.SaveChanges();
                    }
                }
            }
        }
    }
}