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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Communication.Chat;
using Rock.Communication.Chat.DTO;
using Rock.Communication.Chat.Sync;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action.Communications
{
    /// <summary>
    /// Sends a chat message via a direct message to the recipient.
    /// </summary>
    [ActionCategory( "Communications" )]
    [Description( "Sends a chat message via a direct message to the recipient." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Chat Direct Message Send" )]

    #region Attributes

    [WorkflowAttribute( "Recipient",
        Description = "The individual who should receive the message.",
        Key = AttributeKey.Recipient,
        IsRequired = true,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" },
        Order = 0 )]

    [WorkflowAttribute( "Sender",
        Description = "The individual the chat message should appear to be from.",
        Key = AttributeKey.Sender,
        IsRequired = true,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" },
        Order = 1 )]

    [WorkflowTextOrAttribute( "Message", "Attribute Value",
        Description = "The message to send. Mentions should be in the format of @{personId} (e.g. @123).",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.Message,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" } )]

    [WorkflowAttribute( "Attachment",
        Description = @"Optional attachment to include with the message. Note: the attribute type you specify will determine the attachment type in the chat message. For example, if you use an attribute type of ""Image,"" the attachment will be sent as an image.",
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.Attachment,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.FileFieldType", "Rock.Field.Types.ImageFieldType", "Rock.Field.Types.AudioFileFieldType", "Rock.Field.Types.VideoFileFieldType" } )]

    #endregion Attributes

    public class ChatDirectMessageSend : ActionComponent
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string Recipient = "Recipient";
            public const string Sender = "Sender";
            public const string Message = "Message";
            public const string Attachment = "Attachment";
        }

        #endregion Attribute Keys

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns><see langword="true"/> if successful; <see langword="false"/> otherwise.</returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var mergeFields = GetMergeFields( action );

            var recipientPerson = GetPersonFromAttributeValue( action, AttributeKey.Recipient, true, rockContext );
            if ( recipientPerson == null )
            {
                errorMessages.Add( "A valid recipient person was not provided." );
                return false;
            }

            var senderPerson = GetPersonFromAttributeValue( action, AttributeKey.Sender, true, rockContext );
            if ( senderPerson == null )
            {
                errorMessages.Add( "A valid sender person was not provided." );
                return false;
            }

            var messageText = GetAttributeValueFromWorkflowTextOrAttribute( action, AttributeKey.Message )
                .ResolveMergeFields( mergeFields );

            if ( messageText.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( "A message was not provided." );
                return false;
            }

            List<RockChatMessageAttachment> attachments = null;
            var attachmentAttributeGuid = GetActionAttributeValue( action, AttributeKey.Attachment ).AsGuidOrNull();
            if ( attachmentAttributeGuid.HasValue )
            {
                // The attachment type will be determined from the attribute's field type.
                var attachmentAttribute = AttributeCache.Get( attachmentAttributeGuid.Value );
                var chatAttachmentType = ChatHelper.GetChatAttachmentType( attachmentAttribute?.FieldType );
                if ( chatAttachmentType.HasValue )
                {
                    var attachmentBinaryFile = GetEntityFromAttributeValue<BinaryFile>(
                        action,
                        AttributeKey.Attachment,
                        true,
                        rockContext
                    );

                    if ( attachmentBinaryFile != null )
                    {
                        attachments = new List<RockChatMessageAttachment>
                        {
                            new RockChatMessageAttachment
                            {
                                Type = chatAttachmentType.Value,
                                Url = attachmentBinaryFile.Url
                            }
                        };
                    }
                }
            }

            var sendTaskErrorMessages = new List<string>();

            var sendTask = Task.Run( async () =>
            {
                using ( var chatHelper = new ChatHelper( rockContext ) )
                {
                    var sendChatMessageResult = await chatHelper.SendChatDirectMessageAsync(
                        new SendChatDirectMessageCommand
                        {
                            RecipientPersonIds = new List<int> { recipientPerson.Id },
                            SenderPersonId = senderPerson.Id,
                            MessageText = messageText,
                            Attachments = attachments
                        }
                    );

                    if ( !sendChatMessageResult.WasMessageSent )
                    {
                        if ( sendChatMessageResult.Exception?.Message.IsNotNullOrWhiteSpace() == true )
                        {
                            sendTaskErrorMessages.Add( sendChatMessageResult.Exception.Message );
                        }
                        else
                        {
                            sendTaskErrorMessages.Add( "An unknown error occurred while sending the chat message." );
                        }
                    }
                }
            } );

            // This is not ideal, but is a dependable way to wait for the
            // asynchronous task to complete within this synchronous context.
            while ( !sendTask.IsCompleted )
            {
                Thread.Sleep( 50 );
            }

            // When completed, the task will be in one of the three final states:
            // RanToCompletion, Faulted, or Canceled.
            if ( sendTask.IsFaulted )
            {
                errorMessages.Add( $"An error occurred while sending the chat message: {sendTask.Exception?.GetBaseException().Message}" );

                return false;
            }

            errorMessages.AddRange( sendTaskErrorMessages );

            return !errorMessages.Any();
        }
    }
}
