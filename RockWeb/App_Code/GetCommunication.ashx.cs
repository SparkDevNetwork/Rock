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
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace RockWeb
{
    /// <summary>
    /// Renders the HTML from a communication
    /// </summary>
    public class GetCommunication : IHttpHandler
    {

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler" /> instance is reusable; otherwise, false.</returns>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest( HttpContext context )
        {

            int? communicationId = context.Request.QueryString["c"].AsIntegerOrNull();
            if ( communicationId.HasValue )
            {
                var rockContext = new RockContext();
                var communication = new CommunicationService( rockContext ).Get( communicationId.Value );

                if ( communication != null )
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                    mergeFields.Add( "Communication", communication );

                    Person person = null;

                    string encodedKey = context.Request.QueryString["p"];
                    if ( !string.IsNullOrWhiteSpace( encodedKey ) )
                    {
                        // first try and see if we can use the new GetByPersonActionIdentifier() otherwise
                        // fall-back to the old GetByImpersonationToken method.
                        var personService = new PersonService( rockContext );
                        person = personService.GetByPersonActionIdentifier( encodedKey, "Unsubscribe" );
                        if ( person == null )
                        {
                            // TODO: Support for trying via impersonation token should be removed once we get to Rock v11
                            person = personService.GetByImpersonationToken( encodedKey, true, null );
                        }
                    }

                    if ( person == null )
                    {
                        var principal = context.User;
                        if ( principal != null && principal.Identity != null )
                        {
                            var userLoginService = new Rock.Model.UserLoginService( new RockContext() );
                            var userLogin = userLoginService.GetByUserName( principal.Identity.Name );

                            if ( userLogin != null )
                            {
                                var currentPerson = userLogin.Person;
                                // if a person wasn't specified in the URL, then only show it if the current person has EDIT auth to the communication
                                if ( communication.IsAuthorized( Authorization.EDIT, currentPerson ) )
                                {
                                    person = currentPerson;
                                }
                            }
                        }
                    }

                    if ( person != null )
                    {
                        mergeFields.Add( "Person", person );

                        var recipient = new CommunicationRecipientService( rockContext ).Queryable()
                            .Where( r => 
                                r.CommunicationId == communication.Id &&
                                r.PersonAlias != null && 
                                r.PersonAlias.PersonId == person.Id )
                            .FirstOrDefault();

                        if ( recipient != null )
                        {
                            // Add any additional merge fields created through a report
                            foreach ( var mergeField in recipient.AdditionalMergeValues )
                            {
                                if ( !mergeFields.ContainsKey( mergeField.Key ) )
                                {
                                    mergeFields.Add( mergeField.Key, mergeField.Value );
                                }
                            }
                        }

                        context.Response.ContentType = "text/html";
                        context.Response.Write( GetHtmlPreview( communication, mergeFields ) );

                        if ( recipient != null )
                        {
                            // write an 'opened' interaction
                            var interactionService = new InteractionService( rockContext );

                            InteractionComponent interactionComponent = new InteractionComponentService( rockContext )
                                                .GetComponentByEntityId( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid(),
                                                    communication.Id, communication.Subject );
                            rockContext.SaveChanges();

                            var ipAddress = Rock.Web.UI.RockPage.GetClientIpAddress();

                            var userAgent = context.Request.UserAgent ?? "";

                            UAParser.ClientInfo client = UAParser.Parser.GetDefault().Parse( userAgent );
                            var clientOs = client.OS.ToString();
                            var clientBrowser = client.UserAgent.ToString();
                            var clientType = InteractionDeviceType.GetClientType( userAgent );

                            interactionService.AddInteraction( interactionComponent.Id, recipient.Id, "Opened", "", recipient.PersonAliasId, RockDateTime.Now, clientBrowser, clientOs, clientType, userAgent, ipAddress, null );

                            rockContext.SaveChanges();
                            
                        }

                        return;
                    }
                }
            }

            context.Response.ContentType = "text/plain";
            context.Response.Write( "Sorry, the communication you requested does not exist, or you are not authorized to view it." );
            return;

        }

        private string GetHtmlPreview( Communication communication, Dictionary<string, object> mergeFields )
        {
            var sb = new StringBuilder();

            if ( communication.CommunicationType == CommunicationType.Email || communication.CommunicationType == CommunicationType.RecipientPreference )
            {
                string body = communication.Message.ResolveMergeFields( mergeFields, communication.EnabledLavaCommands );
                body = Regex.Replace( body, @"\[\[\s*UnsubscribeOption\s*\]\]", string.Empty );
                sb.Append( body );
            }
            else if ( communication.CommunicationType == CommunicationType.SMS )
            {
                sb.Append( communication.SMSMessage.ResolveMergeFields( mergeFields, communication.EnabledLavaCommands ) );
            }
            else if ( communication.CommunicationType == CommunicationType.PushNotification )
            {
                sb.Append( communication.PushMessage.ResolveMergeFields( mergeFields, communication.EnabledLavaCommands ) );
            }

            // Email Attachments
            var emailAttachments = communication.GetAttachments( CommunicationType.Email );
            if ( emailAttachments.Any() )
            {
                sb.Append( "<br/><br/>" );
                foreach ( var binaryFile in emailAttachments.Select( a => a.BinaryFile ) )
                {
                    sb.AppendFormat( "<a target='_blank' href='{0}'>{1}</a><br/>", binaryFile.Url, binaryFile.FileName );
                }
            }

            return sb.ToString();
        }

    }
}