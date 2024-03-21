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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Communication;
using Rock.Model;
using Rock.Reporting;
using Rock.ViewModels.Blocks.Mobile.Communication.SmsConversationList;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Communication
{
    /// <summary>
    /// Displays a list of SMS conversations that the individual can interact with.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "SMS Conversation List" )]
    [Category( "Mobile > Communication" )]
    [Description( "Displays a list of SMS conversations that the individual can interact with." )]
    [IconCssClass( "fa fa-sms" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [SystemPhoneNumberField( "Allowed SMS Numbers",
        Description = "Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included). ",
        IsRequired = false,
        AllowMultiple = true,
        Key = AttributeKey.AllowedSMSNumbers,
        Order = 0 )]

    [BooleanField( "Show only personal SMS number",
        Description = "Only SMS Numbers tied to the current individual will be shown. Those with ADMIN rights will see all SMS Numbers.",
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowOnlyPersonalSmsNumber,
        Order = 1 )]

    [BooleanField( "Hide personal SMS numbers",
        Description = "When enabled, only SMS Numbers that are not 'Assigned to a person' will be shown.",
        DefaultBooleanValue = false,
        Key = AttributeKey.HidePersonalSmsNumbers,
        Order = 2 )]

    [IntegerField( "Show Conversations From Months Ago",
        Description = "Limits the conversations shown in the left pane to those of X months ago or newer.",
        DefaultIntegerValue = 6,
        Key = AttributeKey.ShowConversationsFromMonthsAgo,
        Order = 3 )]

    [IntegerField( "Max Conversations",
        Description = "Limits the number of conversations shown in the left pane.",
        DefaultIntegerValue = 100,
        Key = AttributeKey.MaxConversations,
        Order = 4
         )]

    [IntegerField( "Database Timeout",
        Description = "The number of seconds to wait before reporting a database timeout.",
        IsRequired = false,
        DefaultIntegerValue = 180,
        Key = AttributeKey.DatabaseTimeoutSeconds,
        Order = 5 )]

    [LinkedPage( "Conversation Page",
        Description = "The page that the person will be pushed to when selecting a conversation.",
        IsRequired = false,
        Key = AttributeKey.ConversationPage,
        Order = 6 )]

    [IntegerField( "Person Search Stopped Typing Behavior Threshold",
        Description = "Changes the amount of time (in milliseconds) that a user must stop typing for the search command to execute. Set to 0 to disable entirely.",
        IsRequired = true,
        DefaultIntegerValue = 200,
        Key = AttributeKey.StoppedTypingBehaviorThreshold,
        Order = 7 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "77701BE2-1335-45F3-93B3-F06466CA391F" )]
    [Rock.SystemGuid.BlockTypeGuid( "E16DC868-101F-4944-BE6C-29D858D9821D" )]
    public class SmsConversationList : RockBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the <see cref="SmsConversationList"/> block.
        /// </summary>
        private static class AttributeKey
        {
            public const string AllowedSMSNumbers = "AllowedSMSNumbers";
            public const string ShowOnlyPersonalSmsNumber = "ShowOnlyPersonalSmsNumber";
            public const string HidePersonalSmsNumbers = "HidePersonalSmsNumbers";
            public const string ShowConversationsFromMonthsAgo = "ShowConversationsFromMonthsAgo";
            public const string MaxConversations = "MaxConversations";
            public const string DatabaseTimeoutSeconds = "DatabaseTimeoutSeconds";
            public const string ConversationPage = "ConversationPage";
            public const string StoppedTypingBehaviorThreshold = "StoppedTypingBehaviorThreshold";
        }

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 5 );

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            return new
            {
                ConversationPageGuid = GetAttributeValue( AttributeKey.ConversationPage ).AsGuidOrNull(),
                StoppedTypingBehaviorThreshold = GetAttributeValue( AttributeKey.StoppedTypingBehaviorThreshold ).AsIntegerOrNull()
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the phone numbers that the current person is allowed to access.
        /// </summary>
        /// <returns>A collection of objects representing the SMS phone numbers.</returns>
        private IEnumerable<PhoneNumberBag> LoadPhoneNumbers()
        {
            // First load up all of the available numbers
            var smsNumbers = SystemPhoneNumberCache.All( false )
                .OrderBy( spn => spn.Order )
                .ThenBy( spn => spn.Name )
                .ThenBy( spn => spn.Id )
                .Where( spn => spn.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson ) && spn.IsSmsEnabled );

            var selectedNumberGuids = GetAttributeValue( AttributeKey.AllowedSMSNumbers ).SplitDelimitedValues( true ).AsGuidList();
            if ( selectedNumberGuids.Any() )
            {
                smsNumbers = smsNumbers.Where( v => selectedNumberGuids.Contains( v.Guid ) );
            }

            // filter personal numbers (any that have a response recipient) if the hide personal option is enabled
            if ( GetAttributeValue( AttributeKey.HidePersonalSmsNumbers ).AsBoolean() )
            {
                smsNumbers = smsNumbers.Where( spn => !spn.AssignedToPersonAliasId.HasValue );
            }

            // Show only numbers 'tied to the current' individual...unless they have 'Admin rights'.
            if ( GetAttributeValue( AttributeKey.ShowOnlyPersonalSmsNumber ).AsBoolean() && !BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                smsNumbers = smsNumbers.Where( spn => RequestContext.CurrentPerson.Aliases.Any( a => a.Id == spn.AssignedToPersonAliasId ) );
            }

            return smsNumbers
                .Select( n => new PhoneNumberBag
                {
                    Guid = n.Guid,
                    ContactKey = n.Guid.ToString(),
                    PhoneNumber = n.Number,
                    Description = n.Name
                } )
                .ToList();
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets all phone numbers available to the currently logged in person.
        /// </summary>
        /// <returns>A collection of phone number objects or an HTTP error.</returns>
        [BlockAction]
        public BlockActionResult GetPhoneNumbers()
        {
            return ActionOk( LoadPhoneNumbers() );
        }

        /// <summary>
        /// Gets all conversations available to the currently logged in person.
        /// </summary>
        /// <param name="phoneNumberGuid">The unique identifier of the phone number to retrieve conversations for.</param>
        /// <returns>A collection of conversation objects or an HTTP error.</returns>
        [BlockAction]
        public BlockActionResult GetConversations( Guid phoneNumberGuid )
        {
            var phoneNumber = SystemPhoneNumberCache.Get( phoneNumberGuid );

            if ( phoneNumber == null )
            {
                return ActionBadRequest( "Invalid Rock phone number specified." );
            }

            if ( !phoneNumber.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to view conversations for this phone number." );
            }

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.CommandTimeout = GetAttributeValue( AttributeKey.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180;

                    var months = GetAttributeValue( AttributeKey.ShowConversationsFromMonthsAgo ).AsInteger();
                    var startDateTime = RockDateTime.Now.AddMonths( -months );
                    var maxConversations = GetAttributeValue( AttributeKey.MaxConversations ).AsIntegerOrNull() ?? 1000;
                    int? personId = null;

                    var communicationResponseService = new CommunicationResponseService( rockContext );
                    var responseListItems = communicationResponseService.GetCommunicationAndResponseRecipients( phoneNumber.Id, startDateTime, maxConversations, CommunicationMessageFilter.ShowAllReplies, personId );

                    var conversations = responseListItems.ToMessageBags();

                    return ActionOk( conversations );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );

                if ( ReportingHelper.FindSqlTimeoutException( ex ) != null )
                {
                    return ActionInternalServerError( "Unable to load SMS responses in a timely manner. You can try again or adjust the timeout setting of this block." );
                }
                else
                {
                    return ActionInternalServerError( "An error occurred when loading SMS responses." );
                }
            }
        }

        #endregion
    }
}
