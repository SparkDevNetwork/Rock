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
using Rock.Data;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Connection Request View Model (cards)
    /// </summary>
    public class ConnectionRequestViewModel
    {
        #region Properties

        /// <summary>
        /// Connection Request Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the placement group identifier.
        /// </summary>
        public int? PlacementGroupId { get; set; }

        /// <summary>
        /// Gets or sets the placement group role identifier.
        /// </summary>
        public int? PlacementGroupRoleId { get; set; }

        /// <summary>
        /// Gets or sets the placement group member status.
        /// </summary>
        public GroupMemberStatus? PlacementGroupMemberStatus { get; set; }

        /// <summary>
        /// Gets or sets the name of the placement group role.
        /// </summary>
        /// <value>
        /// The name of the placement group role.
        /// </value>
        public string PlacementGroupRoleName { get; set; }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Requester Person Id
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the person email.
        /// </summary>
        public string PersonEmail { get; set; }

        /// <summary>
        /// Gets or sets the name of the person.
        /// </summary>
        /// <value>
        /// The name of the person.
        /// </value>
        public string PersonNickName { get; set; }

        /// <summary>
        /// Person Last Name
        /// </summary>
        public string PersonLastName { get; set; }

        /// <summary>
        /// Person Photo Id
        /// </summary>
        public int? PersonPhotoId { get; set; }

        /// <summary>
        /// Gets or sets the person phones.
        /// </summary>
        public List<PhoneViewModel> PersonPhones { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        public Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        public int? CampusId { get; set; }

        /// <summary>
        /// Campus Name
        /// </summary>
        public string CampusName { get; set; }

        /// <summary>
        /// Campus Code
        /// </summary>
        public string CampusCode { get; set; }

        /// <summary>
        /// Gets or sets the connector photo identifier.
        /// </summary>
        public int? ConnectorPhotoId { get; set; }

        /// <summary>
        /// Gets or sets the name of the connector person.
        /// </summary>
        public string ConnectorPersonNickName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the connector person.
        /// </summary>
        public string ConnectorPersonLastName { get; set; }

        /// <summary>
        /// Connector Person Id
        /// </summary>
        public int? ConnectorPersonId { get; set; }

        /// <summary>
        /// Connector person alias id
        /// </summary>
        public int? ConnectorPersonAliasId { get; set; }

        /// <summary>
        /// Connection Status Id
        /// </summary>
        public int StatusId { get; set; }

        /// <summary>
        /// Gets or sets the connection opportunity identifier.
        /// </summary>
        public int ConnectionOpportunityId { get; set; }

        /// <summary>
        /// Gets or sets the connection type identifier.
        /// </summary>
        public int ConnectionTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the status.
        /// </summary>
        public string StatusName { get; set; }

        /// <summary>
        /// Gets or sets the color of the status highlight.
        /// </summary>
        public string StatusHighlightColor
        {
            get
            {
                return _statusHighlightColor.IsNullOrWhiteSpace() ? ConnectionStatus.DefaultHighlightColor : _statusHighlightColor;
            }

            set
            {
                _statusHighlightColor = value;
            }
        }

        private string _statusHighlightColor = null;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is status critical.
        /// </summary>
        public bool IsStatusCritical { get; set; }

        /// <summary>
        /// Activity count
        /// </summary>
        public int ActivityCount { get; set; }

        /// <summary>
        /// Last activity date
        /// </summary>
        public DateTime? LastActivityDate { get; set; }

        /// <summary>
        /// Date Opened
        /// </summary>
        public DateTime? DateOpened { get; set; }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the last activity.
        /// </summary>
        public string LastActivityTypeName { get; set; }

        /// <summary>
        /// Gets or sets the last activity type identifier.
        /// </summary>
        public int? LastActivityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the state of the connection.
        /// </summary>
        public ConnectionState ConnectionState { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is assigned to you.
        /// </summary>
        public bool IsAssignedToYou { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is critical.
        /// </summary>
        public bool IsCritical { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is idle.
        /// </summary>
        public bool IsIdle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is unassigned.
        /// </summary>
        public bool IsUnassigned { get; set; }

        /// <summary>
        /// Gets or sets the followup date.
        /// </summary>
        public DateTime? FollowupDate { get; set; }

        /// <summary>
        /// Gets or sets the status icons HTML.
        /// </summary>
        public string StatusIconsHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can connect.
        /// </summary>
        public bool CanConnect { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [current person can edit].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [current person can edit]; otherwise, <c>false</c>.
        /// </value>
        public bool CanCurrentUserEdit { get; set; }

        /// <summary>
        /// Gets or sets the attributes of this instance
        /// </summary>
        public string RequestAttributes { get; set; }

        #endregion Properties

        #region Computed

        /// <summary>
        /// The state label
        /// </summary>
        public string StateLabel
        {
            get
            {
                var css = string.Empty;

                switch ( ConnectionState )
                {
                    case ConnectionState.Active:
                        css = "success";
                        break;
                    case ConnectionState.Inactive:
                        css = "danger";
                        break;
                    case ConnectionState.FutureFollowUp:
                        css = ( FollowupDate.HasValue && FollowupDate.Value > RockDateTime.Today ) ? "info" : "danger";
                        break;
                    case ConnectionState.Connected:
                        css = "success";
                        break;
                }

                var text = ConnectionState.ConvertToString();

                if ( ConnectionState == ConnectionState.FutureFollowUp && FollowupDate.HasValue )
                {
                    text += string.Format( " ({0})", FollowupDate.Value.ToShortDateString() );
                }

                return string.Format( "<span class='label label-{0}'>{1}</span>", css, text );
            }
        }

        /// <summary>
        /// Gets or sets the status label.
        /// </summary>
        public string StatusLabelClass
        {
            get
            {
                return IsStatusCritical ? "warning" : "info";
            }
        }

        /// <summary>
        /// Activity Count Text
        /// </summary>
        public string ActivityCountText
        {
            get
            {
                if ( ActivityCount == 1 )
                {
                    return "1 Activity";
                }

                return string.Format( "{0} Activities", ActivityCount );
            }
        }

        /// <summary>
        /// Gets the last activity text.
        /// </summary>
        public string LastActivityText
        {
            get
            {
                if ( !LastActivityTypeName.IsNullOrWhiteSpace() && LastActivityDate.HasValue )
                {
                    return string.Format(
                        "{0} (<span class='small'>{1}</small>)",
                        LastActivityTypeName,
                        LastActivityDate.ToRelativeDateString() );
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Connector Person Fullname
        /// </summary>
        public string ConnectorPersonFullname
        {
            get
            {
                return string.Format( "{0} {1}", ConnectorPersonNickName, ConnectorPersonLastName );
            }
        }

        /// <summary>
        /// Person Fullname
        /// </summary>
        public string PersonFullname
        {
            get
            {
                return string.Format( "{0} {1}", PersonNickName, PersonLastName );
            }
        }

        /// <summary>
        /// Person Photo Html
        /// </summary>
        public string PersonPhotoUrl
        {
            get
            {
                if ( PersonPhotoId.HasValue )
                {
                    return FileUrlHelper.GetImageUrl( PersonPhotoId.Value );
                }
                else
                {
                    Person person = new PersonService( new RockContext() ).Get( PersonId );
                    return Person.GetPersonPhotoUrl( person.Initials, person.PhotoId, person.Age, person.Gender, person.RecordTypeValueId, person.AgeClassification );
                }
            }
        }

        /// <summary>
        /// Gets the connector photo URL.
        /// </summary>
        public string ConnectorPhotoUrl
        {
            get
            {
                if ( ConnectorPhotoId.HasValue )
                {
                    return FileUrlHelper.GetImageUrl( ConnectorPhotoId.Value );
                }
                else
                {
                    if ( ConnectorPersonId.HasValue )
                    {
                        Person person = new PersonService( new RockContext() ).Get( ConnectorPersonId.Value );
                        return Person.GetPersonPhotoUrl( person.Initials, person.PhotoId, person.Age, person.Gender, person.RecordTypeValueId, person.AgeClassification );
                    }
                    else
                    {
                        return "/Assets/Images/person-no-photo-unknown.svg";
                    }
                }
            }
        }

        /// <summary>
        /// Has Campus
        /// </summary>
        public string CampusHtml
        {
            get
            {
                if ( CampusCode.IsNullOrWhiteSpace() )
                {
                    return string.Empty;
                }

                return string.Format(
                    @"<span class=""badge badge-info font-weight-normal"" title=""{0}"">{1}</span>",
                    CampusName,
                    CampusCode );
            }
        }

        /// <summary>
        /// Days Since Opening
        /// </summary>
        public int? DaysSinceOpening
        {
            get
            {
                if ( !DateOpened.HasValue )
                {
                    return null;
                }

                return ( RockDateTime.Today - DateOpened.Value.Date ).Days;
            }
        }

        /// <summary>
        /// Days Since Opening Short Text
        /// </summary>
        public string DaysSinceOpeningShortText
        {
            get
            {
                if ( !DaysSinceOpening.HasValue )
                {
                    return "No Opening";
                }

                return string.Format( "{0}d", DaysSinceOpening.Value );
            }
        }

        /// <summary>
        /// Gets the days or weeks since opening text.
        /// </summary>
        public string DaysOrWeeksSinceOpeningText
        {
            get
            {
                if ( !DaysSinceOpening.HasValue )
                {
                    return "No Opening";
                }

                if ( DaysSinceOpening.Value == 1 )
                {
                    return "1 day";
                }

                if ( DaysSinceOpening.Value < 14 )
                {
                    return string.Format( "{0} days", DaysSinceOpening.Value );
                }

                return string.Format( "{0} weeks", DaysSinceOpening.Value / 7 );
            }
        }

        /// <summary>
        /// Days Since Opening Long Text
        /// </summary>
        public string DaysSinceOpeningLongText
        {
            get
            {
                if ( !DaysSinceOpening.HasValue )
                {
                    return "No Opening";
                }

                if ( DaysSinceOpening.Value == 1 )
                {
                    return string.Format( "Opened 1 Day Ago ({0})", DateOpened.Value.ToShortDateString() );
                }

                return string.Format( "Opened {0} Days Ago ({1})", DaysSinceOpening.Value, DateOpened.Value.ToShortDateString() );
            }
        }

        /// <summary>
        /// Days Since Last Activity
        /// </summary>
        public int? DaysSinceLastActivity
        {
            get
            {
                if ( !LastActivityDate.HasValue )
                {
                    return null;
                }

                return ( RockDateTime.Now - LastActivityDate.Value ).Days;
            }
        }

        /// <summary>
        /// Days Since Last Activity Short Text
        /// </summary>
        public string DaysSinceLastActivityShortText
        {
            get
            {
                if ( !DaysSinceLastActivity.HasValue )
                {
                    return "No Activity";
                }

                return string.Format( "{0}d", DaysSinceLastActivity.Value );
            }
        }

        /// <summary>
        /// Days Since Last Activity Long Text
        /// </summary>
        public string DaysSinceLastActivityLongText
        {
            get
            {
                if ( !DaysSinceLastActivity.HasValue )
                {
                    return "No Activity";
                }

                if ( DaysSinceLastActivity.Value == 1 )
                {
                    return "1 Day Since Last Activity";
                }

                return string.Format( "{0} Days Since Last Activity", DaysSinceLastActivity.Value );
            }
        }

        /// <summary>
        /// Gets the group name with role and status.
        /// </summary>
        /// <value>
        /// The group name with role and status.
        /// </value>
        public string GroupNameWithRoleAndStatus
        {
            get
            {
                if (!string.IsNullOrWhiteSpace( PlacementGroupRoleName ) || PlacementGroupMemberStatus != null )
                {
                    return string.Format("{0} ({1} {2})", GroupName, PlacementGroupMemberStatus, PlacementGroupRoleName );
                }

                return GroupName;
            }
        }

        #endregion Computed

        /// <summary>
        /// Phone View Model
        /// </summary>
        public sealed class PhoneViewModel : RockDynamic
        {
            /// <summary>
            /// Gets or sets the type of the phone.
            /// </summary>
            public string PhoneType { get; set; }

            /// <summary>
            /// Gets or sets the formatted phone number.
            /// </summary>
            public string FormattedPhoneNumber { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is messaging enabled.
            /// </summary>
            public bool IsMessagingEnabled { get; set; }

            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            public override string ToString()
            {
                return string.Format( "{0}: {1}", PhoneType, FormattedPhoneNumber );
            }
        }
    }
}
