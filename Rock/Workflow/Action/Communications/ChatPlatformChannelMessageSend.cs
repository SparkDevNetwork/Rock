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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Attribute;
using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Doors;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action.Communications
{
    /// <summary>
    /// Posts a message into a chat channel, under a person's name or as a system line.
    /// </summary>
    /// <remarks>
    /// A post under a person's name follows that person's rules in the channel, and they are
    /// enrolled in chat first if they have never opened it. Without a sender the post is a system
    /// line. The action waits for the message to land and fails with the chat platform's reason
    /// when it does not, such as a group chat has not seen yet.
    /// </remarks>
    [ActionCategory( "Communications" )]
    [Description( "Posts a message into a chat channel, under a person's name or as a system line." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Chat Platform Channel Message Send" )]

    #region Attributes

    [WorkflowAttribute( "Channel",
        Description = "The group whose chat channel the message is posted in.",
        Key = AttributeKey.Channel,
        IsRequired = true,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.GroupFieldType" },
        Order = 0 )]

    [WorkflowAttribute( "Sender",
        Description = "The person the message is from. Leave blank to post a system line.",
        Key = AttributeKey.Sender,
        IsRequired = false,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" },
        Order = 1 )]

    [WorkflowTextOrAttribute( "Message", "Attribute Value",
        Description = "The message to send. <span class='tip tip-lava'></span>",
        IsRequired = true,
        Order = 2,
        Key = AttributeKey.Message,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" } )]

    #endregion Attributes

    [Rock.SystemGuid.EntityTypeGuid( "776E2094-DC16-4F93-94BE-572FF12260D8" )]
    public class ChatPlatformChannelMessageSend : ActionComponent
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string Channel = "Channel";
            public const string Sender = "Sender";
            public const string Message = "Message";
        }

        #endregion Attribute Keys

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns><see langword="true"/> if the message was posted; <see langword="false"/> otherwise.</returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var groupGuid = GetAttributeValue( action, AttributeKey.Channel, true ).AsGuidOrNull();
            if ( !groupGuid.HasValue )
            {
                errorMessages.Add( "A valid channel group was not provided." );
                return false;
            }

            // A sender is optional, so a blank one is a system line rather than an error.
            var sender = GetPersonFromAttributeValue( action, AttributeKey.Sender, true, rockContext );

            var message = GetAttributeValueFromWorkflowTextOrAttribute( action, AttributeKey.Message )
                .ResolveMergeFields( GetMergeFields( action ) );

            if ( message.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( "A message was not provided." );
                return false;
            }

            var outcome = ChatDoorHelper.SendWorkflowChannelMessage( groupGuid.Value, sender?.Id, message, ChatPlatformConfigurationService.Read() );
            if ( outcome.Code != ChatDoorHelper.OkCode )
            {
                errorMessages.Add( string.Format( "The chat message was not posted: {0}: {1}", outcome.Code, outcome.Message ) );
                return false;
            }

            return true;
        }
    }
}
