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
using System.Linq;

using Rock.Attribute;
using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Doors;
using Rock.Communication.Chat.Platform.Session;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.ViewModels.Blocks.Communication.Chat.ChatShell;
using Rock.ViewModels.Controls;
using Rock.Web.Cache;

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
            var person = GetCurrentPerson();

            return new ChatShellInitializationBox
            {
                Session = ChatShellSession.Open( person, BuildSessionContext( person, RockContext ), RockContext ),
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
            // Whether the person may start a direct message is not in the token, so the data
            // view is left out: every open person asks for a token every few minutes, and a data
            // view that fails must not stop chat for everyone.
            var context = new ChatSessionContext { Configuration = ChatPlatformConfigurationService.Read() };

            return ActionOk( ChatShellSession.MintToken( GetCurrentPerson(), context, RockContext ) );
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
            var person = GetCurrentPerson();
            var date = birthDate ?? new DatePartsPickerValueBag();

            return ActionOk( ChatBirthdateDoor.Save( person?.Id, date.Year, date.Month, date.Day, BuildSessionContext( person, RockContext ), RockContext ) );
        }

        #endregion Block Actions

        #region Methods

        /// <summary>
        /// Reads the church's settings and whether this person is in the church's Direct Message
        /// Access data view. Only this person is looked for, so the data view is never read whole
        /// for one open.
        /// </summary>
        /// <param name="person">The person opening chat, or null.</param>
        /// <param name="rockContext">The context the data view is read in.</param>
        /// <returns>The session context the gates read.</returns>
        private static ChatSessionContext BuildSessionContext( Person person, RockContext rockContext )
        {
            var configuration = ChatPlatformConfigurationService.Read();
            var context = new ChatSessionContext { Configuration = configuration };

            if ( person == null || !configuration.DirectMessageAccessDataViewGuid.HasValue )
            {
                return context;
            }

            // A data view that no longer exists admits nobody, so a deleted data view never
            // opens direct messages to everyone.
            context.DirectMessageAccessPersonIds = new HashSet<int>();

            var dataView = DataViewCache.Get( configuration.DirectMessageAccessDataViewGuid.Value );
            if ( dataView == null )
            {
                return context;
            }

            var isInDataView = dataView.GetQuery( new GetQueryableOptions { DbContext = rockContext } )
                .Any( entity => entity.Id == person.Id );

            if ( isInDataView )
            {
                context.DirectMessageAccessPersonIds.Add( person.Id );
            }

            return context;
        }

        #endregion Methods
    }
}
