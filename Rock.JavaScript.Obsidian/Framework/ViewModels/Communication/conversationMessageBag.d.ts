//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
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

import { Guid } from "@Obsidian/Types";
import { ConversationAttachmentBag } from "@Obsidian/ViewModels/Communication/conversationAttachmentBag";

/** Class ConversationMessageBag */
export type ConversationMessageBag = {
    /** Gets or sets the attachments to this message. */
    attachments?: ConversationAttachmentBag[] | null;

    /**
     * Gets or sets the contact key of the recipient. This would contain
     * a phone number, e-mail address, or other transport specific key
     * to allow communication.
     */
    contactKey?: string | null;

    /**
     * Gets or sets the unique identifier of the conversation that
     * this message belongs to.
     */
    conversationKey?: string | null;

    /** Gets or sets the full name of the person being communicated with. */
    fullName?: string | null;

    /** Gets or sets a value indicating whether the recipient is a nameless person. */
    isNamelessPerson: boolean;

    /** Gets or sets a value indicating whether the message was sent from Rock. */
    isOutbound: boolean;

    /**
     * Gets or sets a value indicating whether the most recent
     * message has been read.
     */
    isRead: boolean;

    /** Gets or sets the content of the most recent message. */
    message?: string | null;

    /** Gets or sets the created date time of the most recent message. */
    messageDateTime?: string | null;

    /**
     * Gets or sets the unique identifier for this message. This can
     * be used to determine if the message has already been seen.
     */
    messageKey?: string | null;

    /**
     * Gets or sets the full name of the person that send the message from
     * Rock. This is only valid if Rock.ViewModels.Communication.ConversationMessageBag.IsOutbound is true.
     */
    outboundSenderFullName?: string | null;

    /**
     * Gets or sets the unique identifier of the person being
     * communicated with.
     */
    personGuid: Guid;

    /**
     * Gets or sets the photo URL for the person. Value will be null
     * if no photo is available.
     */
    photoUrl?: string | null;

    /**
     * Gets or sets the contact key of the Rock side of the conversation.
     * For an SMS message this would be the Guid of the Rock phone number.
     */
    rockContactKey?: string | null;
};
