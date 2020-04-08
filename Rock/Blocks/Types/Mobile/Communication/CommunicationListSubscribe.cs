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
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Communication List Subscribe" )]
    [Category( "Mobile > Communication" )]
    [Description( "Allows the user to subscribe or unsubscribe from specific communication lists." )]
    [IconCssClass( "fa fa-at" )]

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

    #endregion

    public class CommunicationListSubscribe : RockMobileBlockType
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

        /// <summary>
        /// Gets the required mobile application binary interface version required to render this block.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version required to render this block.
        /// </value>
        public override int RequiredMobileAbiVersion => 1;

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        public override string MobileBlockType => "Rock.Mobile.Blocks.Communication.CommunicationListSubscribe";

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
            return new Rock.Common.Mobile.Blocks.Content.Configuration
            {
                DynamicContent = true
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
                descriptionLabel = $"<Label StyleClass=\"list-description\"><![CDATA[{subscription.CommunicationList.Description.EncodeXml()}]]></Label>";
            }

            return $@"
<StackLayout Orientation=""Horizontal""
             Spacing=""20""
             StyleClass=""communication-list"">
    <StackLayout Spacing=""0""
                 HorizontalOptions=""FillAndExpand"">
        <Label Text=""{subscription.DisplayName.EncodeXml( true )}""
               StyleClass=""list-name"" />
        {descriptionLabel}
    </StackLayout>
    <Rock:CheckBox x:Name=""cbSubscribed_{subscription.CommunicationList.Id}""
                   IsChecked=""{subscription.IsSubscribed}""
                   EditStyle=""Switch""
                   VerticalOptions=""Center""
                   Command=""{{Binding Callback}}"">
        <Rock:CheckBox.CommandParameter>
            <Rock:CallbackParameters Name="":UpdateSubscription""
                                     Passive=""True"">
                <Rock:Parameter Name=""CommunicationListGuid""
                                Value=""{subscription.CommunicationList.Guid}"" />
                <Rock:Parameter Name=""Subscribed""
                                Value=""{{Binding IsChecked, Source={{x:Reference cbSubscribed_{subscription.CommunicationList.Id}}}}}"" />
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
            return "<BoxView Color=\"#ccc\" HeightRequest=\"1\" />";
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

            return viewableCommunicationLists
                .Select( a =>
                {
                    var publicName = a.GetAttributeValue( "PublicName" );

                    return new Subscription
                    {
                        DisplayName = publicName.IsNotNullOrWhiteSpace() ? publicName : a.Name,
                        CommunicationList = a,
                        Member = communicationListsMember.GetValueOrDefault( a.Id, null )
                    };
                } )
                .OrderBy( a => a.DisplayName )
                .ToList();
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
            /// Gets or sets the communication list.
            /// </summary>
            /// <value>
            /// The communication list.
            /// </value>
            public Group CommunicationList { get; set; }

            /// <summary>
            /// Gets or sets the current member record.
            /// </summary>
            /// <value>
            /// The current member record.
            /// </value>
            public GroupMember Member { get; set; }

            /// <summary>
            /// Gets a value indicating whether the person is subscribed.
            /// </summary>
            /// <value>
            ///   <c>true</c> if per person is subscribed; otherwise, <c>false</c>.
            /// </value>
            public bool IsSubscribed => Member != null && Member.GroupMemberStatus == GroupMemberStatus.Active;
        }

        #endregion
    }
}
