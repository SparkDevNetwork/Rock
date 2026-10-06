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
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Doors;
using Rock.Communication.Chat.Platform.Session;
using Rock.ViewModels.Blocks.Communication.Chat.ChatShell;
using Rock.ViewModels.Controls;

namespace Rock.Blocks.Communication.Chat
{
    /// <summary>
    /// Chat for the person signed in: their channels, the open channel's messages, sending and
    /// receiving. The conversation itself is served by the chat platform; this block runs Rock's
    /// gates for the person, enrols them on their first open, and signs the short-lived token
    /// the browser exchanges with the platform, asking the gates again on every token.
    /// </summary>

    [DisplayName( "Chat" )]
    [Category( "Communication > Chat" )]
    [Description( "Chat for the person signed in: their channels, the open channel, sending and receiving." )]
    [IconCssClass( "ti ti-messages" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "31F2556F-3BF1-4D8A-85B5-D6B3DD3646B6" )]
    [Rock.SystemGuid.BlockTypeGuid( "079E31A8-F2EF-4A1B-A0F7-23AA0E28CF90" )]
    public class ChatShell : RockBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            /// <summary>
            /// The channel to open first, as a link from a notification, an email or a workflow
            /// message names it.
            /// </summary>
            public const string ChannelGuid = "ChannelGuid";
        }

        #endregion Keys

        #region RockBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new ChatShellInitializationBox
            {
                Session = ChatSessionHelper.OpenSession( GetCurrentPerson(), RockContext ),
                ChannelGuid = PageParameter( PageParameterKey.ChannelGuid ).AsGuidOrNull()
            };
        }

        #endregion RockBlockType Implementation

        #region Block Actions

        /// <summary>
        /// Signs a church token for the person, after running every gate again, so a person who
        /// has been banned or has become ineligible since chat opened gets no new token.
        /// </summary>
        /// <returns>The token and its expiry, or the gate that refused it.</returns>
        [BlockAction]
        public BlockActionResult MintChurchToken()
        {
            return ActionOk( ChatSessionHelper.MintToken( GetCurrentPerson(), RockContext ) );
        }

        /// <summary>
        /// Records the birthdate chat asked the person for, when Rock holds none, and describes
        /// the session as it now stands so the shell can carry on without reloading.
        /// </summary>
        /// <param name="birthDate">The date the person gave.</param>
        /// <returns>What happened, and the session after it.</returns>
        [BlockAction]
        public BlockActionResult SaveBirthdate( DatePartsPickerValueBag birthDate )
        {
            var date = birthDate ?? new DatePartsPickerValueBag();

            return ActionOk( ChatSessionHelper.SaveBirthdate( GetCurrentPerson(), date.Year, date.Month, date.Day, RockContext ) );
        }

        /// <summary>
        /// Starts a direct message with the people chosen, or reopens the one they already have,
        /// as the first message sent to a draft asks. Only people cross: the conversation is
        /// worked out by Rock, never named by the browser.
        /// </summary>
        /// <param name="personAliasGuids">The other people, one to eight, by any of their aliases.</param>
        /// <returns>The conversation, or why there is none.</returns>
        [BlockAction]
        public async Task<BlockActionResult> StartDirectMessage( List<Guid> personAliasGuids )
        {
            var person = GetCurrentPerson();
            var context = ChatSessionHelper.BuildSessionContext( person, ChatPlatformConfigurationService.Read(), RockContext );

            return ActionOk( await ChatDoorHelper.StartDirectMessageAsync( person, personAliasGuids, context, RockContext ) );
        }

        #endregion Block Actions
    }
}
