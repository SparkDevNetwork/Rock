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

using System.ComponentModel;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Engagement.OutreachOnboarding.cs;
using Rock.Common.Mobile.ViewModel;
using Rock.Enums.Core;
using Rock.Mobile;
using Rock.Model;

namespace Rock.Blocks.Types.Mobile.Engagement
{
    /// <summary>
    /// On boarding for Outreach.
    /// </summary>
    [DisplayName( "Outreach Onboarding" )]
    [Category( "Engagement" )]
    [IconCssClass( "ti ti-plane-departure" )]
    [Description( "On boarding for Outreach" )]
    [SupportedSiteTypes( SiteType.Mobile )]

    #region Block Attributes

    [LinkedPage(
        "Add Contact Page",
        Description = "Page to link to when user taps on the plus button.",
        IsRequired = true,
        Key = AttributeKey.AddContact,
        Order = 0 )]

    [MobileNavigationActionField( "After Finish Action",
        Description = "The navigation action to perform when the delete button is pressed.",
        IsRequired = false,
        DefaultValue = MobileNavigationActionFieldAttribute.PopSinglePageValue,
        Key = AttributeKey.AfterFinishAction,
        Order = 1 )]

    [TextField( "Toolbox Name",
        Description = "The name that you want to call this tool.",
        IsRequired = false,
        DefaultValue = "Beacon",
        Key = AttributeKey.ToolboxName,
        Order = 2 )]

    #endregion

    [SystemGuid.EntityTypeGuid( SystemGuid.EntityType.MOBILE_OUTREACH_OUTREACH_ONBOARDING_BLOCK_TYPE )]
    [SystemGuid.BlockTypeGuid( SystemGuid.BlockType.MOBILE_OUTREACH_OUTREACH_ONBOARDING )]
    public class OutreachOnboarding : RockBlockType
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string AddContact = "AddContact";
            public const string AfterFinishAction = "AfterFinishAction";
            public const string ToolboxName = "ToolboxName";
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Finishes the onboarding.
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult FinishOnboarding( OutreachOnboardingOption option )
        {
            if ( option == null || option.CurrentPersonIdKey == null )
            {
                return ActionBadRequest( "Invalid options." );
            }
            var personService = new PersonService( RockContext );
            var person = personService.Get( option.CurrentPersonIdKey );
            if ( person == null )
            {
                return ActionBadRequest( "Could not find the current person." );
            }

            person.OutreachTouchpointSchedule = ( DaysOfWeekFlags ) option.DayOfWeekFlags;
            person.OutreachEnableDailyNotification = option.DailyNotificationsEnabled;
            person.OutreachNotificationTimeOfDay = option.DailyNotificationsEnabled ? option.NotificationTime?.ToNative() : null; // Clear out time if daily notifications are not enabled
            person.OutreachEnableSpecialEventsNotification = option.SpecialEventNotificationsEnabled;
            person.OutreachTouchpointGenerationEnabled = true;

            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc />
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Engagement.OutreachOnboarding.Configuration
            {
                AddContactPageGuid = GetAttributeValue( AttributeKey.AddContact ).AsGuidOrNull(),
                ToolboxName = GetAttributeValue( AttributeKey.ToolboxName ),
                AfterFinishAction = GetAttributeValue( AttributeKey.AfterFinishAction ).FromJsonOrNull<MobileNavigationActionViewModel>() ?? new MobileNavigationActionViewModel()
            };
        }

        #endregion
    }
}
