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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

using Rock.Security;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;

using Newtonsoft.Json.Linq;
using System.Web;
using Rock.Communication;

namespace Rock.SignNow
{
    /// <summary>
    /// SignNow Digital Signature Provider
    /// </summary>
    [Description( "SignNow Digital Signature Provider" )]
    [Export( typeof( DigitalSignatureComponent ) )]
    [ExportMetadata( "ComponentName", "SignNow" )]

    [TextField( "Username", "Your SignNow Username", true, "", "", 0 )]
    [TextField( "Password", "Your SignNow Password", true, "", "", 1, null, true )]
    [TextField( "API Client Id", "The SignNow API Client Id", true, "", "", 2 )]
    [TextField( "API Client Secret", "The SignNow API Client Secret", true, "", "", 3, null, true )]
    [BooleanField( "Use API Sandbox", "Use the SignNow API Sandbox (vs. Production Environment)", false, "", 4 )]
    [TextField( "Webhook Url", "The URL of the webhook that SignNow should post to when a document is updated (signed).", true, "", "", 5 )]

    public class SignNow : DigitalSignatureComponent
    {
        /// <summary>
        /// Method that is called before attribute values are updated. Components can
        /// override this to perform any needed initialization of attribute
        /// values.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="rootUrl"></param>
        public override void InitializeAttributeValues( HttpRequest request, string rootUrl )
        {
            if ( string.IsNullOrWhiteSpace( GetAttributeValue( "WebhookUrl" ) ) )
            {
                Uri uri = new Uri( request.Url.ToString() );
                string webhookUrl = "https://" + uri.GetComponents( UriComponents.Host, UriFormat.UriEscaped ) + rootUrl + "WebHooks/SignNow.ashx";
                SetAttributeValue( "WebhookUrl", webhookUrl );
                this.SaveAttributeValues();
            }
        }

        /// <summary>
        /// Method that is called when attribute values are updated. Components can
        /// override this to perform any needed setup based on current attribute
        /// values.
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public override bool ValidateAttributeValues( out string errorMessage )
        {
            // Get the access token
            errorMessage = string.Empty;
            string accessToken = GetAccessToken( true, out errorMessage );
            if ( string.IsNullOrWhiteSpace( accessToken ) )
            {
                return false;
            }

            // Delete any existing document.update webhook
            JObject listWebhookRes = CudaSign.Webhook.List( accessToken );
            var errors = ParseErrors( listWebhookRes );
            if ( errors.Any() )
            {
                errorMessage = errors.AsDelimited( "; " );
                return false;
            }
            JArray subscriptions = listWebhookRes.Value<JArray>( "subscriptions" );
            if ( subscriptions != null )
            {
                foreach ( JObject subscription in subscriptions )
                {
                    if ( subscription.Value<string>( "event" ) == "document.update" )
                    {
                        string subscriptionId = subscription.Value<string>( "id" );
                        if ( !string.IsNullOrWhiteSpace( subscriptionId ) )
                        {
                            JObject deleteWebHookRes = CudaSign.Webhook.Delete( accessToken, subscriptionId );
                            errors = ParseErrors( deleteWebHookRes );
                            if ( errors.Any() )
                            {
                                errorMessage = errors.AsDelimited( "; " );
                                return false;
                            }
                        }
                    }
                }
            }

            // Re-add the webhook
            JObject createWebhookRes = CudaSign.Webhook.Create( accessToken, "document.update", GetAttributeValue( "WebhookUrl" ) );
            string webhookId = createWebhookRes.Value<string>( "id" );
            if ( string.IsNullOrWhiteSpace( webhookId ) )
            {
                errors = ParseErrors( createWebhookRes );
                if ( errors.Any() )
                {
                    errorMessage = errors.AsDelimited( "; " );
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the templates.
        /// </summary>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Dictionary<string, string> GetTemplates( out List<string> errors )
        {
            var templates = new Dictionary<string, string>();

            errors = new List<string>();

            // Get the access token
            string errorMessage = string.Empty;
            string accessToken = GetAccessToken( false, out errorMessage );
            if ( string.IsNullOrWhiteSpace( accessToken ) )
            {
                errors.Add( errorMessage );
                return null;
            }

            JObject folderListRes = CudaSign.Folder.List( accessToken );
            errors = ParseErrors( folderListRes );
            if ( errors.Any() )
            {
                errorMessage = errors.AsDelimited( "; " );
                return null;
            }

            JArray folders = folderListRes.Value<JArray>( "folders" );
            if ( folders != null )
            {
                foreach ( JObject folder in folders )
                {
                    if ( folder.Value<string>( "name" ) == "Templates" )
                    {
                        string folderId = folder.Value<string>( "id" );
                        if ( !string.IsNullOrWhiteSpace( folderId ) )
                        {
                            JObject documentListRes = CudaSign.Folder.Get( accessToken, folderId );
                            errors = ParseErrors( documentListRes );
                            if ( errors.Any() )
                            {
                                errorMessage = errors.AsDelimited( "; " );
                                return null;
                            }

                            JArray documents = documentListRes.Value<JArray>( "documents" );
                            if ( documents != null )
                            {
                                foreach ( JObject document in documents )
                                {
                                    templates.AddOrIgnore(
                                        document.Value<string>( "id" ),
                                        document.Value<string>( "document_name" ) );
                                }
                            }
                        }
                    }
                }
            }

            return templates;
        }

        /// <summary>
        /// Sends the document.
        /// </summary>
        /// <param name="documentTemplate">Type of the document.</param>
        /// <param name="appliesTo">The applies to.</param>
        /// <param name="assignedTo">The recipient.</param>
        /// <param name="documentName">Name of the document.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="sendInvite">if set to <c>true</c> [send invite].</param>
        /// <returns></returns>
        public override string CreateDocument( SignatureDocumentTemplate documentTemplate, Person appliesTo, Person assignedTo, string documentName, out List<string> errors, bool sendInvite )
        {
            errors = new List<string>();

            // Get the access token
            string errorMessage = string.Empty;
            string accessToken = GetAccessToken( false, out errorMessage );
            if ( string.IsNullOrWhiteSpace( accessToken ) )
            {
                errors.Add( errorMessage );
                return null;
            }

            // Create a docuemnt from the template
            JObject copyTemplateRes = CudaSign.Template.Copy( accessToken, documentTemplate.ProviderTemplateKey, documentName );
            string documentId = copyTemplateRes.Value<string>( "id" );
            if ( string.IsNullOrWhiteSpace( documentId ) )
            {
                errors = ParseErrors( copyTemplateRes );
                return null;
            }

            if ( sendInvite && assignedTo != null && !string.IsNullOrEmpty( assignedTo.Email ) )
            {
                string orgAbbrev = GlobalAttributesCache.Value( "OrganizationAbbreviation" );
                if ( string.IsNullOrWhiteSpace( orgAbbrev) )
                {
                    orgAbbrev = GlobalAttributesCache.Value( "OrganizationName" );
                }

                string subject = string.Format( "Digital Signature Request from {0}", orgAbbrev );
                string message = string.Format( "{0} has requested a digital signature for a '{1}' document for {2}.",
                    GlobalAttributesCache.Value( "OrganizationName" ), documentTemplate.Name, appliesTo != null ? appliesTo.FullName : assignedTo.FullName );

                // Get the document to determine the roles (if any) are needed
                JObject getDocumentRes = CudaSign.Document.Get( accessToken, documentId );
                errors = ParseErrors( getDocumentRes );
                if ( errors.Any() )
                {
                    errorMessage = errors.AsDelimited( "; " );
                    return null;
                }

                dynamic inviteObj = null;
                JArray roles = getDocumentRes.Value<JArray>( "roles" );
                if ( roles != null && roles.Count > 0 )
                {
                    var to = new List<dynamic>();
                    foreach ( JObject role in roles )
                    {
                        to.Add( new
                        {
                            email = assignedTo.Email,
                            role_id = string.Empty,
                            role = role.Value<string>( "name" ),
                            order = role.Value<int>( "signing_order" ),
                            expiration_days = 15,
                            reminder = 5
                        } );
                    }

                    inviteObj = new
                    {
                        to = to.ToArray(),
                        from = GetAttributeValue( "Username" ),
                        subject = subject,
                        message = message
                    };
                }
                else
                {
                    inviteObj = new
                    {
                        to = assignedTo.Email,
                        from = GetAttributeValue( "Username" ),
                        subject = subject,
                        message = message
                    };
                }

                // Send the invite
                JObject inviteRes = CudaSign.Document.Invite( accessToken, documentId, inviteObj );
                errors = ParseErrors( inviteRes );
                if ( errors.Any() )
                {
                    return null;
                }
            }
            return documentId;
        }

        /// <summary>
        /// Gets the invite link.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="recipient">The recipient.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public override string GetInviteLink( SignatureDocument document, Person recipient, out List<string> errors )
        {
            errors = new List<string>();

            if ( document == null )
            {
                errors.Add( "Invalid Document!" );
                return null;
            }

            // Get the access token
            string errorMessage = string.Empty;
            string accessToken = GetAccessToken( false, out errorMessage );
            if ( string.IsNullOrWhiteSpace( accessToken ) )
            {
                errors.Add( errorMessage );
                return null;
            }

            JObject inviteLinkRes = CudaSign.Link.Create( accessToken, document.DocumentKey );
            string inviteLink = inviteLinkRes.Value<string>( "url_no_signup" );
            if ( string.IsNullOrWhiteSpace( inviteLink ) )
            {
                errors = ParseErrors( inviteLinkRes );
                return null;
            }

            return inviteLink;
        }

        /// <summary>
        /// Resends the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public override bool ResendDocument( SignatureDocument document, out List<string> errors )
        {
            errors = new List<string>();

            if ( document == null )
            {
                errors.Add( "Invalid Document!" );
            }

            if ( document.AssignedToPersonAlias == null || document.AssignedToPersonAlias.Person == null ||
                string.IsNullOrWhiteSpace( document.AssignedToPersonAlias.Person.Email ) )
            {
                errors.Add( "Invalid Assigned To Person or Email!" );
            }
            
            if ( errors.Any() )
            {
                return false;
            }

            // Get the access token
            string errorMessage = string.Empty;
            string accessToken = GetAccessToken( false, out errorMessage );
            if ( string.IsNullOrWhiteSpace( accessToken ) )
            {
                errors.Add( errorMessage );
                return false;
            }

            // Get the document to determine the roles (if any) are needed
            JObject getDocumentRes = CudaSign.Document.Get( accessToken, document.DocumentKey );
            errors = ParseErrors( getDocumentRes );
            if ( errors.Any() )
            {
                errorMessage = errors.AsDelimited( "; " );
                return false;
            }

            // Cancel existing invite
            JObject CancelRes = CudaSign.Document.CancelInvite( accessToken, document.DocumentKey );
            errors = ParseErrors( CancelRes );
            if ( errors.Any() )
            {
                return false;
            }

            string orgAbbrev = GlobalAttributesCache.Value( "OrganizationAbbreviation" );
            if ( string.IsNullOrWhiteSpace( orgAbbrev ) )
            {
                orgAbbrev = GlobalAttributesCache.Value( "OrganizationName" );
            }

            string subject = string.Format( "Digital Signature Request from {0}", orgAbbrev );
            string message = string.Format( "{0} has requested a digital signature for a '{1}' document for {2}.",
                GlobalAttributesCache.Value( "OrganizationName" ), document.SignatureDocumentTemplate.Name, 
                document.AppliesToPersonAlias != null ? document.AppliesToPersonAlias.Person.FullName : document.AssignedToPersonAlias.Person.FullName );


            dynamic inviteObj = null;
            JArray roles = getDocumentRes.Value<JArray>( "roles" );
            if ( roles != null && roles.Count > 0 )
            {
                var to = new List<dynamic>();
                foreach ( JObject role in roles )
                {
                    to.Add( new
                    {
                        email = document.AssignedToPersonAlias.Person.Email,
                        role_id = string.Empty,
                        role = role.Value<string>( "name" ),
                        order = role.Value<int>( "signing_order" ),
                        expiration_days = 15,
                        reminder = 5
                    } );
                }

                inviteObj = new
                {
                    to = to.ToArray(),
                    from = GetAttributeValue( "Username" ),
                    subject = subject,
                    message = message
                };
            }
            else
            {
                inviteObj = new
                {
                    to = document.AssignedToPersonAlias.Person.Email,
                    from = GetAttributeValue( "Username" ),
                    subject = subject,
                    message = message
                };
            }

            // Send the invite
            JObject inviteRes = CudaSign.Document.Invite( accessToken, document.DocumentKey, inviteObj );
            errors = ParseErrors( inviteRes );
            if ( errors.Any() )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Cancels the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public override bool CancelDocument( SignatureDocument document, out List<string> errors )
        {
            errors = new List<string>();

            if ( document == null )
            {
                errors.Add( "Invalid Document!" );
                return false;
            }

            // Get the access token
            string errorMessage = string.Empty;
            string accessToken = GetAccessToken( false, out errorMessage );
            if ( string.IsNullOrWhiteSpace( accessToken ) )
            {
                errors.Add( errorMessage );
                return false;
            }

            JObject CancelRes = CudaSign.Document.CancelInvite( accessToken, document.DocumentKey );
            errors = ParseErrors( CancelRes );
            if ( errors.Any() )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public override string GetDocument( SignatureDocument document, string folderPath, out List<string> errors )
        {
            errors = new List<string>();

            if ( document == null )
            {
                errors.Add( "Invalid Document!" );
                return null;
            }

            // Get the access token
            string errorMessage = string.Empty;
            string accessToken = GetAccessToken( false, out errorMessage );
            if ( string.IsNullOrWhiteSpace( accessToken ) )
            {
                errors.Add( errorMessage );
                return null;
            }

            var folder = new DirectoryInfo( folderPath );
            if ( !folder.Exists )
            {
                folder.Create();
            }

            int i = 1;
            string fileName = document.Name;
            while ( File.Exists( Path.Combine( folder.FullName, fileName ) + ".pdf" ) )
            {
                fileName = string.Format( "{0}_{1}", document.Name, i++ );
            }

            string filePath = Path.Combine( folder.FullName, fileName );
            JObject downloadRes = CudaSign.Document.Download( accessToken, document.DocumentKey, filePath, fileName );
            errors = ParseErrors( downloadRes );
            if ( !errors.Any() )
            {
                return downloadRes.Value<string>( "file" );
            }

            return null;
        }

        /// <summary>
        /// Determines whether [is document signed] [the specified document].
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public override bool IsDocumentSigned( SignatureDocument document, out List<string> errors )
        {
            errors = new List<string>();

            if ( document == null )
            {
                errors.Add( "Invalid Document!" );
                return false;
            }

            // Get the access token
            string errorMessage = string.Empty;
            string accessToken = GetAccessToken( false, out errorMessage );
            if ( string.IsNullOrWhiteSpace( accessToken ) )
            {
                errors.Add( errorMessage );
                return false;
            }

            // Get the document to determine the roles (if any) are needed
            JObject getDocumentRes = CudaSign.Document.Get( accessToken, document.DocumentKey );
            errors = ParseErrors( getDocumentRes );
            if ( errors.Any() )
            {
                errorMessage = errors.AsDelimited( "; " );
                return false;
            }
            JArray signatures = getDocumentRes.Value<JArray>( "signatures" );
            if ( signatures != null && signatures.Count > 0 )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <param name="recreate">if set to <c>true</c> [recreate].</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public string GetAccessToken( bool recreate, out string errorMessage )
        {
            errorMessage = string.Empty;

            string accessToken = GlobalAttributesCache.Value( "SignNowAccessToken" );
            if ( !recreate && !string.IsNullOrWhiteSpace( accessToken ) )
            {
                try
                {
                    JObject tokenVerification = CudaSign.OAuth2.Verify( accessToken );
                    var errors = ParseErrors( tokenVerification );
                    if ( !errors.Any() )
                    {
                        return accessToken;
                    }
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex, null );
                }
            }

            // Get new Access Token
            CudaSign.Config.init( GetAttributeValue( "APIClientId" ), GetAttributeValue( "APIClientSecret" ), !GetAttributeValue( "UseAPISandbox" ).AsBoolean() );
            JObject OAuthRes = CudaSign.OAuth2.RequestToken( GetAttributeValue( "Username" ), GetAttributeValue( "Password" ) );
            errorMessage = ParseErrors( OAuthRes ).AsDelimited( "; " );
            if ( string.IsNullOrWhiteSpace( errorMessage ) )
            {
                accessToken = OAuthRes.Value<string>( "access_token" );
                GlobalAttributesCache globalCache = GlobalAttributesCache.Read();
                globalCache.SetValue( "SignNowAccessToken", accessToken, true );
            }

            return accessToken;
        }

        public List<string> ParseErrors( JObject jObject )
        {
            var msgs = new List<string>();

            if ( jObject == null )
            {
                msgs.Add( "API Call returned a null result!" );
            }
            else
            { 
                JArray errors = jObject.Value<JArray>( "errors" );
                if ( errors != null )
                {
                    foreach ( JObject error in errors )
                    {
                        msgs.Add( error.Value<string>( "message" ) );
                    }
                }

                string errorMsg = jObject.Value<string>( "error" );
                if ( errorMsg != null )
                {
                    msgs.Add( errorMsg );
                }
            }


            return msgs;

        }


    }
}