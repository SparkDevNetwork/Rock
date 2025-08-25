using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.SemanticKernel;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Utilities.CommunicationSkill.Mediums
{
    internal class EmailMedium : IAgentCommunicationMedium
    {
        public Model.Communication BuildCommunication( DraftRequest request, Person recipient, DraftResult content )
        {
            var emailMediumEntityTypeId = EntityTypeCache.Get<Rock.Communication.Medium.Email>().Id;

            var comm = new Rock.Model.Communication();
            comm.Status = CommunicationStatus.Transient;
            comm.CommunicationType = CommunicationType.Email;
            comm.SenderPersonAliasId = request.CurrentPerson.PrimaryAliasId;
            comm.FromEmail = request.CurrentPerson.Email;
            comm.Subject = content.Subject;
            comm.Message = content.Body;
            comm.Recipients = new List<CommunicationRecipient>
            {
                new CommunicationRecipient
                {
                    PersonAliasId = recipient.PrimaryAliasId,
                    MediumEntityTypeId = emailMediumEntityTypeId
                }
            };

            return comm;
        }

        public async Task<DraftResult> DraftAsync( Kernel kernel, DraftRequest request, Person recipient )
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
            };
        }

        public void UpdateCommunication( DraftRequest request, Person recipient, Model.Communication comm, DraftResult content )
        {
            var emailMediumEntityTypeId = EntityTypeCache.Get<Rock.Communication.Medium.Email>().Id;

            comm.Status = CommunicationStatus.Transient;
            comm.CommunicationType = CommunicationType.Email;
            comm.SenderPersonAliasId = request.CurrentPerson.PrimaryAliasId;
            comm.FromEmail = request.CurrentPerson.Email;
            comm.Subject = content.Subject;
            comm.Message = content.Body;
            comm.Recipients = new List<CommunicationRecipient>
            {
                new CommunicationRecipient
                {
                    PersonAliasId = recipient.PrimaryAliasId,
                    MediumEntityTypeId = emailMediumEntityTypeId
                }
            };
        }

        public List<string> ValidateRecipient( Person recipient )
        {
            var errors = new List<string>();

            if ( recipient.Email.IsNullOrWhiteSpace() )
            {
                errors.Add( "Recipient " + recipient.IdKey + " does not have a valid email address." );
            }

            return errors;
        }
    }
}
