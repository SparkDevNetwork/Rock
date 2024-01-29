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
using System.Data.Entity;
using System.Linq;
using System.Text;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Content;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Events
{
    /// <summary>
    /// Allows the user to subscribe or unsubscribe from specific communication lists.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Communication List Subscribe" )]
    [Category( "Mobile > Communication" )]
    [Description( "Allows the user to subscribe or unsubscribe from specific communication lists." )]
    [IconCssClass( "fa fa-at" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [GroupCategoryField( "Communication List Categories",
        allowMultiple: true,
        groupTypeGuid: Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST,
        Description = "Select the categories of the communication lists to display, or select none to show all that the user is authorized to view.",
        IsRequired = false,
        Key = AttributeKeys.CommunicationListCategories,
        Order = 0 )]

    [BooleanField( "Show Description",
        Description = "If enabled then the description of the communication list will be shown.",
        IsRequired = false,
        Key = AttributeKeys.ShowDescription,
        Order = 1 )]

    [BooleanField( "Show Medium Preference",
        Description = "If enabled then the medium preference will be shown.",
        IsRequired = false,
        Key = AttributeKeys.ShowMediumPreference,
        Order = 2 )]

    [BooleanField( "Show Push Notification As Medium Preference",
        Description = "If enabled then the push notification medium preference will be shown. Irrelevant if Show Medium Preference is disabled.",
        IsRequired = false,
        Key = AttributeKeys.ShowPushNotificationsAsMediumPreference,
        Order = 3 )]

    [BooleanField( "Filter Groups By Campus Context",
        Description = "When enabled will filter the listed Communication Lists by the campus context of the page. Groups with no campus will always be shown.",
        IsRequired = false,
        Key = AttributeKeys.FilterGroupsByCampusContext,
        Order = 4 )]

    [BooleanField( "Always Include Subscribed Lists",
        Description = "When filtering is enabled this setting will include lists that the person is subscribed to even if they don't match the current campus context. (note this would still filter by the category though, so lists not in the configured category would not show even if subscribed to them)",
        IsRequired = false,
        DefaultBooleanValue = true,
        Key = AttributeKeys.AlwaysIncludeSubscribedLists,
        Order = 5 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_EVENTS_COMMUNICATION_LIST_SUBSCRIBE_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "D0C51784-71ED-46F3-86AB-972148B78BE8" )]
    public class CommunicationListSubscribe : RockBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the <see cref="CommunicationListSubscribe"/> block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The communication list categories key.
            /// </summary>
            public const string CommunicationListCategories = "CommunicationListCategories";

            /// <summary>
            /// The show description key.
            /// </summary>
            public const string ShowDescription = "ShowDescription";

            /// <summary>
            /// An attribute key defining whether or not to show the medium preference.
            /// </summary>
            public const string ShowMediumPreference = "ShowMediumPreference";

            /// <summary>
            /// The show push notifications as medium preference.
            /// </summary>
            public const string ShowPushNotificationsAsMediumPreference = "ShowPushNotificationsAsMediumPreference";

            /// <summary>
            /// The filter groups by campus context attribute key.
            /// </summary>
            public const string FilterGroupsByCampusContext = "FilterGroupsByCampusContext";

            /// <summary>
            /// The always include subscribed lists key.
            /// </summary>
            public const string AlwaysIncludeSubscribedLists = "AlwaysIncludeSubscribedLists";
        }

        /// <summary>
        /// Gets the communication list category guids.
        /// </summary>
        /// <value>
        /// The communication list category guids.
        /// </value>
        protected List<Guid> CommunicationListCategories => GetAttributeValue( AttributeKeys.CommunicationListCategories ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Gets the show description flag.
        /// </summary>
        /// <value>
        /// The show description flag.
        /// </value>
        protected bool ShowDescription => GetAttributeValue( AttributeKeys.ShowDescription ).AsBoolean();

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 1 );

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            //
            // Indicate that we are a dynamic content providing block.
            //
            return new
            {
                DynamicContent = true,
                ShowDescription = this.ShowDescription,
                ShowMediumPreference = GetAttributeValue( AttributeKeys.ShowMediumPreference ).AsBoolean(),
                ShowPushNotificationsAsMediumPreference = GetAttributeValue( AttributeKeys.ShowPushNotificationsAsMediumPreference ).AsBoolean(),
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the xaml for subscription item.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <returns>A string that contains XAML content.</returns>
        protected virtual string GetXamlForSubscription( Subscription subscription )
        {
            string descriptionLabel = string.Empty;

            if ( ShowDescription )
            {
                descriptionLabel = $"<Label StyleClass=\"communicationlist-item-description\"><![CDATA[{subscription.CommunicationListDescription}]]></Label>";
            }

            return $@"
<StackLayout Orientation=""Horizontal""
             Spacing=""20""
             StyleClass=""communicationlist-item"">
    <StackLayout Spacing=""0""
                 HorizontalOptions=""FillAndExpand"">
        <Label Text=""{subscription.DisplayName.EncodeXml( true )}""
               StyleClass=""h3, communicationlist-item-name"" />
        {descriptionLabel}
    </StackLayout>
    <Rock:CheckBox x:Name=""cbSubscribed_{subscription.CommunicationListId}""
                   IsChecked=""{subscription.IsSubscribed}""
                   EditStyle=""Switch""
                   VerticalOptions=""Center""
                   Command=""{{Binding Callback}}"">
        <Rock:CheckBox.CommandParameter>
            <Rock:CallbackParameters Name="":UpdateSubscription""
                                     Passive=""True"">
                <Rock:Parameter Name=""CommunicationListGuid""
                                Value=""{subscription.CommunicationListGuid}"" />
                <Rock:Parameter Name=""Subscribed""
                                Value=""{{Binding IsChecked, Source={{x:Reference cbSubscribed_{subscription.CommunicationListId}}}}}"" />
            </Rock:CallbackParameters>
        </Rock:CheckBox.CommandParameter>
    </Rock:CheckBox>
</StackLayout>";
        }

        /// <summary>
        /// Gets the subscription separator.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetSubscriptionSeparator()
        {
            return "<Rock:Divider StyleClass=\"my-12\" />";
        }

        /// <summary>
        /// Builds the content to be shown.
        /// </summary>
        /// <returns>A string containing the XAML content.</returns>
        protected virtual string BuildContent()
        {
            var subscriptions = GetSubscriptions();

            var subscriptionList = subscriptions
                .Select( a => GetXamlForSubscription( a ) )
                .SelectMany( a => new[] { a, GetSubscriptionSeparator() } )
                .ToList();

            //
            // Add a starting separator.
            //
            subscriptionList.Insert( 0, GetSubscriptionSeparator() );

            var subscriptionListXaml = subscriptionList
                .JoinStrings( string.Empty );

            return $@"<StackLayout Spacing=""0"">{subscriptionListXaml}</StackLayout>";
        }

        /// <summary>
        /// Gets the subscription options for the current person.
        /// </summary>
        /// <returns>A collection of <see cref="Subscription"/> objects.</returns>
        protected virtual IEnumerable<Subscription> GetSubscriptions()
        {
            int communicationListGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST.AsGuid() ).Id;
            int? communicationListGroupTypeDefaultRoleId = GroupTypeCache.Get( communicationListGroupTypeId ).DefaultGroupRoleId;

            if ( RequestContext.CurrentPerson == null )
            {
                return new List<Subscription>();
            }

            var rockContext = new RockContext();

            //
            // Get all the lists this person is already a member of.
            //
            var memberOfList = new GroupMemberService( rockContext )
                .GetByPersonId( RequestContext.CurrentPerson.Id )
                .AsNoTracking()
                .Where( a => a.Group.GroupTypeId == communicationListGroupTypeId )
                .Select( a => a.GroupId )
                .ToList();

            //
            // Get a list of syncs for the communication list groups
            // where the default role is sync'd AND the current person
            // is NOT a member of.
            // This is used to filter out the list of communication lists.
            //
            var commGroupSyncsForDefaultRole = new GroupSyncService( rockContext )
                .Queryable()
                .Where( a => a.Group.GroupTypeId == communicationListGroupTypeId )
                .Where( a => a.GroupTypeRoleId == communicationListGroupTypeDefaultRoleId )
                .Where( a => !memberOfList.Contains( a.GroupId ) )
                .Select( a => a.GroupId )
                .ToList();

            //
            // Get all the communication lists, excluding those from the above
            // query about synced groups.
            //
            var communicationLists = new GroupService( rockContext )
               .Queryable()
               .Where( a => a.GroupTypeId == communicationListGroupTypeId )
               .Where( a => !commGroupSyncsForDefaultRole.Contains( a.Id ) )
               .ToList();

            var categoryGuids = CommunicationListCategories;

            var viewableCommunicationLists = communicationLists
                .Where( a =>
                {
                    a.LoadAttributes( rockContext );

                    if ( !categoryGuids.Any() )
                    {
                        //
                        // If no categories where specified, only show
                        // lists that the person has VIEW auth to.
                        //
                        if ( a.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson ) )
                        {
                            return true;
                        }
                    }
                    else
                    {
                        //
                        // If categories were specified, ensure that this
                        // communication list has a category and is one of
                        // the specified categories.
                        //
                        Guid? categoryGuid = a.GetAttributeValue( "Category" ).AsGuidOrNull();
                        if ( categoryGuid.HasValue && categoryGuids.Contains( categoryGuid.Value ) )
                        {
                            return true;
                        }
                    }

                    return false;
                } )
                .ToList();

            var groupIds = viewableCommunicationLists.Select( a => a.Id ).ToList();
            var personId = RequestContext.CurrentPerson.Id;

            var communicationListsMember = new GroupMemberService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( a => groupIds.Contains( a.GroupId ) && a.PersonId == personId )
                .GroupBy( a => a.GroupId )
                .ToList()
                .ToDictionary( k => k.Key, v => v.FirstOrDefault() );

            var filterByCampus = GetAttributeValue( AttributeKeys.FilterGroupsByCampusContext ).AsBoolean();
            if ( filterByCampus )
            {
                var alwaysIncludeSubscribed = GetAttributeValue( AttributeKeys.AlwaysIncludeSubscribedLists ).AsBoolean();
                var contextCampus = RequestContext.GetContextEntity<Campus>();

                if( contextCampus != null )
                {
                    // We're going to do two steps of filtering here.
                    viewableCommunicationLists = viewableCommunicationLists.Where( x =>
                    // 1. Filter by campus.
                    ( x.Campus?.Id == contextCampus.Id || x.Campus == null )
                    // 2. OR: Include the communication lists we're already subscribed to.
                    || ( alwaysIncludeSubscribed && ContainsActivePersonRecord( x.Members, RequestContext.CurrentPerson?.Id ?? 0 ) ) ).ToList();
                }
            }

            return viewableCommunicationLists
                .Select( a =>
                {
                    var publicName = a.GetAttributeValue( "PublicName" );
                    var member = communicationListsMember.GetValueOrDefault( a.Id, null );
                    var isSubscribed = member != null && member.GroupMemberStatus == GroupMemberStatus.Active;
                    return new Subscription
                    {
                        DisplayName = publicName.IsNotNullOrWhiteSpace() ? publicName : a.Name,
                        CommunicationListGuid = a.Guid,
                        CommunicationListId = a.Id,
                        CommunicationListDescription = a.Description,
                        CommunicationPreference = member?.CommunicationPreference ?? CommunicationType.Email,
                        IsSubscribed = isSubscribed,
                    };
                } )
                .OrderBy( a => a.DisplayName )
                .ToList();
        }

        private bool ContainsActivePersonRecord( ICollection<GroupMember> groupMembers, int personId )
        {
            return ( groupMembers?.Any( m => m.PersonId == personId && m.GroupMemberStatus == GroupMemberStatus.Active ) ) ?? false;
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the initial content for this block.
        /// </summary>
        /// <returns>The content to be displayed.</returns>
        [BlockAction]
        public object GetInitialContent()
        {
            return new CallbackResponse
            {
                Content = BuildContent()
            };
        }

        /// <summary>
        /// Updates the subscription state.
        /// </summary>
        /// <param name="communicationListGuid">The communication list unique identifier.</param>
        /// <param name="subscribed">if set to <c>true</c> the user should be subscribed.</param>
        [BlockAction]
        public void UpdateSubscription( Guid communicationListGuid, bool subscribed )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupMemberService = new GroupMemberService( rockContext );
                var group = new GroupService( rockContext ).Get( communicationListGuid );
                var groupMemberRecordsForPerson = groupMemberService.Queryable()
                    .Where( a => a.GroupId == group.Id && a.PersonId == RequestContext.CurrentPerson.Id )
                    .ToList();

                if ( groupMemberRecordsForPerson.Any() )
                {
                    // normally there would be at most 1 group member record for the person, but just in case, mark them all
                    foreach ( var groupMember in groupMemberRecordsForPerson )
                    {
                        if ( subscribed )
                        {
                            if ( groupMember.GroupMemberStatus == GroupMemberStatus.Inactive )
                            {
                                groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                                if ( groupMember.Note == "Unsubscribed" )
                                {
                                    groupMember.Note = string.Empty;
                                }
                            }
                        }
                        else
                        {
                            if ( groupMember.GroupMemberStatus == GroupMemberStatus.Active )
                            {
                                groupMember.GroupMemberStatus = GroupMemberStatus.Inactive;
                                if ( groupMember.Note.IsNullOrWhiteSpace() )
                                {
                                    groupMember.Note = "Unsubscribed";
                                }
                            }
                        }

                        // TODO: Change this to null.
                        groupMember.CommunicationPreference = CommunicationType.Email;
                    }
                }
                else if ( subscribed )
                {
                    var groupMember = new GroupMember
                    {
                        PersonId = RequestContext.CurrentPerson.Id,
                        GroupId = group.Id
                    };

                    int? defaultGroupRoleId = GroupTypeCache.Get( group.GroupTypeId ).DefaultGroupRoleId;
                    if ( defaultGroupRoleId.HasValue )
                    {
                        groupMember.GroupRoleId = defaultGroupRoleId.Value;
                    }

                    groupMember.GroupMemberStatus = GroupMemberStatus.Active;

                    if ( groupMember.IsValidGroupMember( rockContext ) )
                    {
                        groupMemberService.Add( groupMember );
                        rockContext.SaveChanges();
                    }
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the subscriptions data.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetSubscriptionsData()
        {
            return ActionOk( GetSubscriptions() );
        }

        /// <summary>
        /// Updates the communication preference.
        /// </summary>
        /// <param name="communicationListGuid">The communication list unique identifier.</param>
        /// <param name="communicationType">Type of the communication.</param>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult UpdateCommunicationPreference( Guid communicationListGuid, CommunicationType communicationType )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupMemberService = new GroupMemberService( rockContext );
                var group = new GroupService( rockContext ).Get( communicationListGuid );
                var groupMemberRecordsForPerson = groupMemberService.Queryable()
                    .Where( a => a.GroupId == group.Id && a.PersonId == RequestContext.CurrentPerson.Id )
                    .ToList();

                if ( groupMemberRecordsForPerson.Any() )
                {
                    // normally there would be at most 1 group member record for the person, but just in case, mark them all
                    foreach ( var groupMember in groupMemberRecordsForPerson )
                    {
                        groupMember.CommunicationPreference = communicationType;
                    }
                }

                rockContext.SaveChanges();
                return ActionOk();
            }
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Contains information about a single subscription option.
        /// </summary>
        protected class Subscription
        {
            /// <summary>
            /// Gets or sets the display name of the communication list.
            /// </summary>
            /// <value>
            /// The display name of the communication list.
            /// </value>
            public string DisplayName { get; set; }

            /// <summary>
            /// Gets or sets the description.
            /// </summary>
            /// <value>The description.</value>
            public string CommunicationListDescription { get; set; }

            /// <summary>
            /// Gets or sets the communication list.
            /// </summary>
            /// <value>
            /// The communication list.
            /// </value>
            public int CommunicationListId { get; set; }

            /// <summary>
            /// Gets or sets the communication list unique identifier.
            /// </summary>
            /// <value>The communication list unique identifier.</value>
            public Guid CommunicationListGuid { get; set; }

            /// <summary>
            /// Gets or sets the communication preference.
            /// </summary>
            /// <value>The communication preference.</value>
            public CommunicationType CommunicationPreference { get; set; }

            /// <summary>
            /// Gets a value indicating whether the person is subscribed.
            /// </summary>
            /// <value>
            ///   <c>true</c> if per person is subscribed; otherwise, <c>false</c>.
            /// </value>
            public bool IsSubscribed { get; set; }
        }

        #endregion
    }
}
