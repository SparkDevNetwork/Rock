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
using System.Threading.Tasks;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Utility;

namespace Rock.RealTime.Topics
{
    [RealTimeTopic]
    internal class TestCommunicationTransportTopic : Topic<ITestCommunicationTransport>
    {
        /// <inheritdoc/>
        public override Task OnConnectedAsync()
        {
            using ( var rockContext = new RockContext() )
            {
                var transport = new EntityTypeService( rockContext ).Get( new Guid( "c50fb8f9-6ada-4c4b-88ee-ed7bc93b1819" ) );

                if ( transport == null )
                {
                    throw new RealTimeException( "Test SMS transport was not found." );
                }

                var currentPerson = new PersonService( rockContext ).Get( Context.CurrentPersonId ?? 0 );

                // Only let them connect to the topic if they have Administrate
                // access to the transport.
                if ( !transport.IsAuthorized( Security.Authorization.ADMINISTRATE, currentPerson ) )
                {
                    throw new RealTimeException( "Not authorized to access test SMS transport." );
                }
            }

            return base.OnConnectedAsync();
        }

        /// <summary>
        /// Called by clients to simulate the reception of a new message
        /// into the transport.
        /// </summary>
        /// <param name="message">The message details.</param>
        public async Task MessageReceived( SmsMessage message )
        {
            if ( !message.PipelineGuid.HasValue )
            {
                string errorMessage;
                var medium = CommunicationServicesHost.GetCommunicationMediumSms();
                if ( medium != null )
                {
                    medium.ProcessResponse( message.ToNumber, message.FromNumber, message.Body, out errorMessage );
                }
                else
                {
                    errorMessage = "SMS Medium not available.";
                }

                if ( errorMessage.IsNotNullOrWhiteSpace() )
                {
                    throw new RealTimeException( errorMessage );
                }
            }
            else
            {
                using ( var rockContext = new RockContext() )
                {
                    var pipelineId = new SmsPipelineService( rockContext ).GetId( message.PipelineGuid.Value );

                    if ( !pipelineId.HasValue )
                    {
                        throw new RealTimeException( "Pipeline was not found." );
                    }

                    var msg = new Rock.Communication.SmsActions.SmsMessage
                    {
                        ToNumber = message.ToNumber,
                        FromNumber = message.FromNumber,
                        Message = message.Body,
                        FromPerson = new PersonService( rockContext ).GetPersonFromMobilePhoneNumber( message.FromNumber, true )
                    };

                    var outcomes = SmsActionService.ProcessIncomingMessage( msg, pipelineId );
                    var smsResponse = SmsActionService.GetResponseFromOutcomes( outcomes );

                    if ( smsResponse != null )
                    {
                        await PostSmsMessage( message.FromNumber, message.ToNumber, smsResponse.Message, smsResponse.Attachments );
                    }
                }
            }
        }

        /// <summary>
        /// Called by the test transport when a message has been sent out of the
        /// transport. Notify all connected clients of the message.
        /// </summary>
        /// <param name="toNumber">To number the message was sent to.</param>
        /// <param name="fromNumber">The Rock phone number the message was sent from.</param>
        /// <param name="message">The body text of the message.</param>
        /// <param name="attachments">The attachments that should be included with the message.</param>
        public static async Task PostSmsMessage( string toNumber, string fromNumber, string message, IEnumerable<BinaryFile> attachments )
        {
            var publicAppRoot = Web.Cache.GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" );

            var smsAttachments = attachments
                ?.Select( bf =>
                {
                    var isImage = bf.MimeType.StartsWith( "image/", StringComparison.OrdinalIgnoreCase );

                    return new SmsAttachment
                    {
                        FileName = bf.FileName,
                        Url = isImage
                            ? FileUrlHelper.GetImageUrl( bf.Id, new GetImageUrlOptions { PublicAppRoot = publicAppRoot } )
                            : FileUrlHelper.GetFileUrl( bf.Id, new GetFileUrlOptions { PublicAppRoot = publicAppRoot } )
                    };
                } )
                .ToList();

            await RealTimeHelper.GetTopicContext<ITestCommunicationTransport>()
                .Clients
                .All
                .SmsMessageSent( new SmsMessage
                {
                    FromNumber = fromNumber ?? string.Empty,
                    ToNumber = toNumber ?? string.Empty,
                    Body = message ?? string.Empty,
                    Attachments = smsAttachments ?? new List<SmsAttachment>()
                } );
        }

        #region Support Classes

        internal class SmsMessage
        {
            public string FromNumber { get; set; }

            public string ToNumber { get; set; }

            public Guid? PipelineGuid { get; set; }

            public string Body { get; set; }

            public List<SmsAttachment> Attachments { get; set; }
        }

        internal class SmsAttachment
        {
            public string FileName { get; set; }

            public string Url { get; set; }
        }

        #endregion
    }
}
