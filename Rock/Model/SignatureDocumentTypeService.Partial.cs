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
using System.Linq;

using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data Access class for <see cref="Rock.Model.SignatureDocumentType"/> entity objects.
    /// </summary>
    public partial class SignatureDocumentTypeService
    {

        /// <summary>
        /// Sends the document.
        /// </summary>
        /// <param name="signatureDocumentType">Type of the signature document.</param>
        /// <param name="appliesToPerson">The person.</param>
        /// <param name="assignedToPerson">The assigned to person.</param>
        /// <param name="documentName">Name of the document.</param>
        /// <param name="alternateEmail">The alternate email.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public bool SendDocument( SignatureDocumentType signatureDocumentType, Person appliesToPerson, Person assignedToPerson, string documentName, string alternateEmail, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            if ( signatureDocumentType == null )
            {
                errorMessages.Add( "Invalid Document Type." );
            }

            if ( appliesToPerson == null )
            {
                errorMessages.Add( "Invalid Applies To Person." );
            }

            if ( assignedToPerson == null )
            {
                errorMessages.Add( "Invalid Assigned To Person." );
            }

            if ( !errorMessages.Any() )
            {
                var provider = DigitalSignatureContainer.GetComponent( signatureDocumentType.ProviderEntityType.Name );
                if ( provider == null || !provider.IsActive )
                {
                    errorMessages.Add( "Digital Signature provider was not found or is not active." );
                }
                else
                { 
                    string email = string.IsNullOrWhiteSpace( alternateEmail ) ? assignedToPerson.Email : alternateEmail;
                    if ( string.IsNullOrWhiteSpace( email ) )
                    {
                        errorMessages.Add( string.Format( "There is no email address for {0}.", assignedToPerson.FullName ) );
                    }
                    else
                    {
                        var sendErrors = new List<string>();

                        var rockContext = this.Context as RockContext;
                        var documentService = new SignatureDocumentService( rockContext );
                        var document = documentService.Queryable()
                            .Where( d =>
                                d.SignatureDocumentTypeId == signatureDocumentType.Id &&
                                d.AppliesToPersonAlias.PersonId == appliesToPerson.Id &&
                                d.AssignedToPersonAlias.PersonId == assignedToPerson.Id &&
                                d.Status != SignatureDocumentStatus.Signed )
                            .OrderByDescending( d => d.CreatedDateTime )
                            .FirstOrDefault();
                        if ( document == null )
                        {
                            string documentKey = provider.SendDocument( signatureDocumentType, email, documentName, out sendErrors );
                            if ( documentKey != null )
                            {
                                document = new SignatureDocument();
                                document.SignatureDocumentTypeId = signatureDocumentType.Id;
                                document.Name = documentName;
                                document.DocumentKey = documentKey;
                                document.AppliesToPersonAliasId = appliesToPerson.PrimaryAliasId;
                                document.AssignedToPersonAliasId = assignedToPerson.PrimaryAliasId;
                                documentService.Add( document );
                            }
                        }
                        else
                        {
                            provider.ResendDocument( document, email, out sendErrors );
                        }

                        if ( !sendErrors.Any() )
                        {
                            document.RequestDate = RockDateTime.Now;
                            if ( document.Status != SignatureDocumentStatus.Sent )
                            {
                                document.LastStatusDate = document.RequestDate;
                            }
                            document.Status = SignatureDocumentStatus.Sent;

                            return true;
                        }
                        else
                        {
                            errorMessages.AddRange( sendErrors );
                        }
                    }
                }
            }

            return false;
        }
    }
}
