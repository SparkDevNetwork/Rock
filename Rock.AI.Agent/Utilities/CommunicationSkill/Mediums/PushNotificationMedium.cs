using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.SemanticKernel;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Utilities.CommunicationSkill.Mediums
{
    internal class PushNotificationMedium : IAgentCommunicationMedium
    {

        private readonly RockContext _rockContext;

        #region Constructors

        public PushNotificationMedium( RockContext rockContext )
        {
            _rockContext = rockContext;
        }

        #endregion
        #region IAgentCommunicationMedium

        /// <inheritdoc />
        public Model.Communication BuildCommunication( DraftRequest request, List<Person> recipients, DraftResult content )
        {
            return CreateOrUpdateCommunication( request, recipients, content );
        }

        /// <inheritdoc />
        public async Task<DraftResult> DraftAsync( Kernel kernel, DraftRequest request )
        {
            var prompt = DraftPromptBuilder.BuildPushDraftPrompt( request );

            var fnResult = await kernel.InvokePromptAsync( prompt );
            var json = fnResult.GetValue<string>();

            var dto = json.FromJsonOrNull<DraftDto>();
            if ( dto == null || dto.Body.IsNullOrWhiteSpace() )
            {
                throw new InvalidOperationException( "Draft JSON invalid. Expect: { \"body\" }" );
            }

            return new DraftResult
            {
                Body = dto.Body,
                Subject = dto.Subject,
                Type = AgentCommunicationType.Push,
                VerificationText = GetVerificationText( request.CurrentPerson, request.Recipients )
            };
        }

        /// <inheritdoc />
        public Model.Communication UpdateCommunication( DraftRequest request, List<Person> recipients, Model.Communication communication, DraftResult content )
        {
            return CreateOrUpdateCommunication( request, recipients, content, communication );
        }

        /// <inheritdoc />
        public List<string> ValidateRecipients( List<Person> recipient )
        {
            // Verify that the person has a device registered for push notifications.
            var errors = new List<string>();
            if ( recipient == null || recipient.Count == 0 )
            {
                errors.Add( "No recipients were provided." );
                return errors;
            }

            foreach ( var person in recipient )
            {
                List<string> devices = new PersonalDeviceService( _rockContext ).Queryable()
                    .Where( a => a.PersonAliasId.HasValue && a.PersonAliasId == person.PrimaryAliasId && a.IsActive && a.NotificationsEnabled )
                    .Select( a => a.DeviceRegistrationId )
                    .ToList();

                if( !devices.Any() )
                {
                    errors.Add( $"Recipient {person.IdKey} does not have any active devices registered for push notifications." );
                }
            }

            return errors;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates or updates the communication entity from the draft content.
        /// </summary>
        /// <param name="request">The request associated with this communication.</param>
        /// <param name="recipients">The recipients associated with this communication.</param>
        /// <param name="content">The content of this communication.</param>
        /// <param name="existingCommunication">The existing communication to update; creates a new one if null.</param>
        /// <returns></returns>
        private Rock.Model.Communication CreateOrUpdateCommunication( DraftRequest request, List<Rock.Model.Person> recipients, DraftResult content, Rock.Model.Communication existingCommunication = null )
        {
            var comm = existingCommunication;

            if ( comm == null )
            {
                comm = new Rock.Model.Communication();
            }

            var pushMediumEntityTypeId = EntityTypeCache.Get<Rock.Communication.Medium.PushNotification>().Id;

            comm.Status = CommunicationStatus.Transient;
            comm.CommunicationType = CommunicationType.PushNotification;
            comm.SenderPersonAliasId = request.CurrentPerson.PrimaryAliasId;
            comm.PushTitle = content.Subject;
            comm.PushMessage = content.Body;
            comm.PushOpenAction = Utility.PushOpenAction.ShowDetails;

            var commRecipients = new List<CommunicationRecipient>();
            foreach ( var recipient in recipients )
            {
                commRecipients.Add( new CommunicationRecipient
                {
                    PersonAliasId = recipient.PrimaryAliasId,
                    MediumEntityTypeId = pushMediumEntityTypeId
                } );
            }
            comm.Recipients = commRecipients;

            return comm;
        }

        /// <summary>
        /// Returns a text representation of the email for verification purposes.
        /// </summary>
        /// <param name="currentPerson"></param>
        /// <param name="recipients"></param>
        /// <returns></returns>
        public string GetVerificationText( Rock.Model.Person currentPerson, List<Rock.Model.Person> recipients )
        {
            var verificationText = new StringBuilder();

            foreach ( var recipient in recipients )
            {
                verificationText.AppendLine( "Recipient: " + recipient.FullName );
            }

            verificationText.AppendLine();
            verificationText.AppendLine( "From: " + currentPerson.FullName );
            verificationText.AppendLine();

            // Body + Subject are returned in the actual payload, so just use placeholders here.  
            verificationText.AppendLine( "Title: [subject]" );
            verificationText.AppendLine();
            verificationText.AppendLine( "Message:" );
            verificationText.AppendLine( "[body]" );

            return verificationText.ToString();
        }


        #endregion
    }
}
