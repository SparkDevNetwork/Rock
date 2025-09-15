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
using System.Text;
using System.Threading.Tasks;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Utilities.CommunicationSkill.Mediums
{
    internal class SmsMedium : IAgentCommunicationMedium
    {

        #region Fields

        private readonly SystemPhoneNumberCache _fromNumber = null;

        #endregion

        #region Constructors

        public SmsMedium( int fromSystemPhoneNumberId )
        {
            _fromNumber = SystemPhoneNumberCache.Get( fromSystemPhoneNumberId );

            if ( _fromNumber == null )
            {
                throw new InvalidOperationException( $"The System Phone Number with Id '{fromSystemPhoneNumberId}' was not found." );
            }
            if ( _fromNumber.Number.IsNullOrWhiteSpace() )
            {
                throw new InvalidOperationException( $"The System Phone Number with Id '{fromSystemPhoneNumberId}' does not have a valid phone number." );
            }
            if( !_fromNumber.IsSmsEnabled )
            {
                throw new InvalidOperationException( $"The System Phone Number with Id '{fromSystemPhoneNumberId}' is not enabled for SMS." );
            }
        }

        #endregion

        #region IAgentCommunicationMedium

        /// <inheritdoc />
        public async Task<DraftResult> DraftAsync( IChatAgent agent, DraftRequest request )
        {
            var prompt = DraftPromptBuilder.BuildSmsDraftPrompt( request, _fromNumber.Number );

            var promptResult = await agent.InvokePromptAsync( prompt, null );

            var dto = promptResult.ResponseText.FromJsonOrNull<DraftDto>();
            if ( dto == null || dto.Subject.IsNullOrWhiteSpace() || dto.Body.IsNullOrWhiteSpace() )
            {
                throw new InvalidOperationException( "Draft JSON invalid. Expect: { \"subject\", \"body\" }" );
            }

            return new DraftResult
            {
                Body = dto.Body,
                Type = AgentCommunicationType.Sms,
                VerificationText = GetVerificationText( request.CurrentPerson, request.Recipients )
            };
        }

        /// <inheritdoc />
        public Model.Communication BuildCommunication( DraftRequest request, List<Rock.Model.Person> recipients, DraftResult content )
        {
            return CreateOrUpdateCommunication( request, recipients, content );
        }

        /// <inheritdoc />
        public Model.Communication UpdateCommunication( DraftRequest request, List<Rock.Model.Person> recipients, Model.Communication comm, DraftResult content )
        {

            return CreateOrUpdateCommunication( request, recipients, content, comm );
        }

        /// <inheritdoc />
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

                var smsNumber = recipient.PhoneNumbers.GetFirstSmsNumber();
                if ( smsNumber == null )
                {
                    errors.Add( $"The recipient '{recipient.FullName}' does not have an SMS enabled phone number." );
                    continue;
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

            var smsMediumEntityTypeId = EntityTypeCache.Get<Rock.Communication.Medium.Sms>().Id;

            comm.Status = CommunicationStatus.Transient;
            comm.CommunicationType = CommunicationType.SMS;
            comm.SenderPersonAliasId = request.CurrentPerson.PrimaryAliasId;
            comm.SmsFromSystemPhoneNumberId = _fromNumber.Id;
            comm.SMSMessage = content.Body;

            var commRecipients = new List<CommunicationRecipient>();
            foreach ( var recipient in recipients )
            {
                commRecipients.Add( new CommunicationRecipient
                {
                    PersonAliasId = recipient.PrimaryAliasId,
                    MediumEntityTypeId = smsMediumEntityTypeId
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
                var recipientAddr = string.IsNullOrWhiteSpace( recipient.Email ) ? "" : " (" + recipient.Email + ")";

                verificationText.AppendLine( "Recipient: " + recipient.FullName + recipientAddr );
            }

            verificationText.AppendLine();
            verificationText.AppendLine( "From: " + _fromNumber.Name + " (" + _fromNumber.Number + ")" );
            verificationText.AppendLine();

            // Body + Subject are returned in the actual payload, so just use placeholders here.  
            verificationText.AppendLine( "Body:" );
            verificationText.AppendLine( "[body]" );

            return verificationText.ToString();
        }

        #endregion
    }
}
