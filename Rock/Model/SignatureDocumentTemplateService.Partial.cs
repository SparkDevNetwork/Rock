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
using Rock.Communication;
using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data Access class for <see cref="Rock.Model.SignatureDocumentTemplate"/> entity objects.
    /// </summary>
    public partial class SignatureDocumentTemplateService
    {

        /// <summary>
        /// Sends the document.
        /// </summary>
        /// <param name="signatureDocumentTemplate">Type of the signature document.</param>
        /// <param name="appliesToPerson">The person.</param>
        /// <param name="assignedToPerson">The assigned to person.</param>
        /// <param name="documentName">Name of the document.</param>
        /// <param name="alternateEmail">The alternate email.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public bool SendDocument( SignatureDocumentTemplate signatureDocumentTemplate, Person appliesToPerson, Person assignedToPerson, string documentName, string alternateEmail, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            if ( signatureDocumentTemplate == null )
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
                var provider = DigitalSignatureContainer.GetComponent( signatureDocumentTemplate.ProviderEntityType.Name );
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
                                d.SignatureDocumentTemplateId == signatureDocumentTemplate.Id &&
                                d.AppliesToPersonAlias.PersonId == appliesToPerson.Id &&
                                d.AssignedToPersonAlias.PersonId == assignedToPerson.Id &&
                                d.Status != SignatureDocumentStatus.Signed )
                            .OrderByDescending( d => d.CreatedDateTime )
                            .FirstOrDefault();
                        if ( document == null )
                        {
                            string documentKey = provider.CreateDocument( signatureDocumentTemplate, assignedToPerson, documentName, out sendErrors );
                            if ( documentKey != null )
                            {
                                document = new SignatureDocument();
                                document.SignatureDocumentTemplate = signatureDocumentTemplate;
                                document.SignatureDocumentTemplateId = signatureDocumentTemplate.Id;
                                document.Name = documentName;
                                document.DocumentKey = documentKey;
                                document.AppliesToPersonAliasId = appliesToPerson.PrimaryAliasId;
                                document.AssignedToPersonAliasId = assignedToPerson.PrimaryAliasId;
                                documentService.Add( document );

                                if ( signatureDocumentTemplate.InviteSystemEmailId.HasValue )
                                {
                                    string inviteLink = provider.GetInviteLink( document, assignedToPerson, out sendErrors );
                                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, assignedToPerson );
                                    mergeFields.Add( "SignatureDocument", document );
                                    mergeFields.Add( "InviteLink", inviteLink );

                                    var recipients = new List<RecipientData>();
                                    recipients.Add( new RecipientData( assignedToPerson.Email, mergeFields ) );

                                    var systemEmail = new SystemEmailService( rockContext ).Get( signatureDocumentTemplate.InviteSystemEmailId.Value );
                                    if ( systemEmail != null )
                                    {
                                        var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "InternalApplicationRoot" );
                                        Email.Send( systemEmail.Guid, recipients, appRoot, string.Empty, false );
                                    }
                                }
                            }
                        }
                        else
                        {
                            provider.ResendDocument( document, email, out sendErrors );
                        }

                        if ( !sendErrors.Any() )
                        {
                            document.LastInviteDate = RockDateTime.Now;
                            document.InviteCount = document.InviteCount + 1;
                            if ( document.Status != SignatureDocumentStatus.Sent )
                            {
                                document.LastStatusDate = document.LastInviteDate;
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

        /// <summary>
        /// Updates the document status.
        /// </summary>
        /// <param name="signatureDocument">The signature document.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public bool UpdateDocumentStatus( SignatureDocument signatureDocument, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            if ( signatureDocument == null )
            {
                errorMessages.Add( "Invalid Document." );
            }

            if ( signatureDocument.SignatureDocumentTemplate == null )
            {
                errorMessages.Add( "Document has an invalid document type." );
            }

            if ( !errorMessages.Any() )
            {
                var provider = DigitalSignatureContainer.GetComponent( signatureDocument.SignatureDocumentTemplate.ProviderEntityType.Name );
                if ( provider == null || !provider.IsActive )
                {
                    errorMessages.Add( "Digital Signature provider was not found or is not active." );
                }
                else
                {
                    provider.UpdateDocumentStatus( signatureDocument, out errorMessages );
                }
            }

            return !errorMessages.Any();
        }

    }
}
