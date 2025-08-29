using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DocumentFormat.OpenXml.Wordprocessing;

using Microsoft.SemanticKernel;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Utilities.CommunicationSkill.Mediums
{
    internal class EmailMedium : IAgentCommunicationMedium
    {
        public Model.Communication BuildCommunication( DraftRequest request, List<Rock.Model.Person> recipients, DraftResult content )
        {
            var emailMediumEntityTypeId = EntityTypeCache.Get<Rock.Communication.Medium.Email>().Id;

            var comm = new Rock.Model.Communication();
            comm.Status = CommunicationStatus.Transient;
            comm.CommunicationType = CommunicationType.Email;
            comm.SenderPersonAliasId = request.CurrentPerson.PrimaryAliasId;
            comm.FromEmail = request.CurrentPerson.Email;
            comm.Subject = content.Subject;
            comm.Message = content.Body;

            var commRecipients = new List<CommunicationRecipient>();
            foreach( var recipient in recipients )
            {
                commRecipients.Add( new CommunicationRecipient
                {
                    PersonAliasId = recipient.PrimaryAliasId,
                    MediumEntityTypeId = emailMediumEntityTypeId
                } );
            }
            comm.Recipients = commRecipients;

            return comm;
        }

        public async Task<DraftResult> DraftAsync( Kernel kernel, DraftRequest request )
        {
            var prompt = DraftPromptBuilder.BuildEmailDraftPrompt( request );

            var fnResult = await kernel.InvokePromptAsync( prompt ).ConfigureAwait( false );
            var json = fnResult.GetValue<string>();

            var dto = json.FromJsonOrNull<EmailDraftDto>();
            if ( dto == null || dto.Subject.IsNullOrWhiteSpace() || dto.Body.IsNullOrWhiteSpace() )
            {
                throw new InvalidOperationException( "Draft JSON invalid. Expect: { \"subject\", \"body\" }" );
            }

            return new DraftResult {
                Body = dto.Body,
                Subject = dto.Subject,
                Type = AgentCommunicationType.Email,
                VerificationText = GetVerificationText( request.CurrentPerson, request.Recipients )
            };
        }

        public string GetVerificationText( Rock.Model.Person currentPerson, List<Rock.Model.Person> recipients )
        {
            var verificationText = new StringBuilder();

            foreach ( var recipient in recipients )
            {
                var recipientAddr = string.IsNullOrWhiteSpace( recipient.Email ) ? "" : " (" + recipient.Email + ")";
                
                verificationText.AppendLine( "Recipient: " + recipient.FullName + recipientAddr );
            }

            verificationText.AppendLine();
            verificationText.AppendLine( "From: " + currentPerson.FullName + " (" + currentPerson.Email + ")" );
            verificationText.AppendLine();
            verificationText.AppendLine( "Subject: [subject]" );
            verificationText.AppendLine();
            verificationText.AppendLine( "Body:" );
            verificationText.AppendLine( "[body]" );

            return verificationText.ToString();
        }

        public void UpdateCommunication( DraftRequest request, List<Rock.Model.Person> recipients, Model.Communication comm, DraftResult content )
        {
            var emailMediumEntityTypeId = EntityTypeCache.Get<Rock.Communication.Medium.Email>().Id;

            comm.Status = CommunicationStatus.Transient;
            comm.CommunicationType = CommunicationType.Email;
            comm.SenderPersonAliasId = request.CurrentPerson.PrimaryAliasId;
            comm.FromEmail = request.CurrentPerson.Email;
            comm.Subject = content.Subject;
            comm.Message = content.Body;
            var commRecipients = new List<CommunicationRecipient>();
            foreach ( var recipient in recipients )
            {
                commRecipients.Add( new CommunicationRecipient
                {
                    PersonAliasId = recipient.PrimaryAliasId,
                    MediumEntityTypeId = emailMediumEntityTypeId
                } );
            }
            comm.Recipients = commRecipients;
        }

        /// <summary>
        /// Validates a collection of recipients for this medium.
        /// Returns a list of error messages. If empty, all recipients are valid.
        /// </summary>
        public List<string> ValidateRecipients( List<Rock.Model.Person> recipients )
        {
            var errors = new List<string>();

            if ( recipients == null || recipients.Count == 0 )
            {
                errors.Add( "No recipients were provided." );
                return errors;
            }

            foreach ( var recipient in recipients )
            {
                if ( recipient == null )
                {
                    errors.Add( "A null recipient was encountered." );
                    continue;
                }

                if ( recipient.Email.IsNullOrWhiteSpace() )
                {
                    errors.Add( $"Recipient {recipient.IdKey} does not have a valid email address." );
                }
            }

            return errors;
        }

    }
}
