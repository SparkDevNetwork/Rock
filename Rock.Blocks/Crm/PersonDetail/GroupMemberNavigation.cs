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
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.PersonDetail.GroupMemberNavigation;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Allows you to switch between the members of the family the person belongs to.
    /// </summary>

    [DisplayName( "Family Navigation" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Allows you to switch between the members of the family the person belongs to." )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [InitialBlockHeight( -1 )]

    #region Block Attributes

    [GroupTypeField( "Group Type",
        Description = "",
        Key = AttributeKey.GroupType,
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY,
        Order = 0 )]

    [BooleanField( "Show Only Primary Group Members",
        Description = "",
        Key = AttributeKey.ShowOnlyPrimaryGroupMembers,
        IsRequired = true,
        DefaultBooleanValue = false,
        Order = 1 )]

    #endregion

    [Rock.Web.UI.ContextAware( typeof( Person ) )]
    [Rock.SystemGuid.EntityTypeGuid( "1232B013-502C-49B7-AB5C-22EA3251AD82" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "545855F5-1089-4646-9A7C-3EBFF3BEC5F4" )]
    [Rock.SystemGuid.BlockTypeGuid( "35D091FA-8311-42D1-83F7-3E67B9EE9675" )]
    public class GroupMemberNavigation : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string GroupType = "GroupType";
            public const string ShowOnlyPrimaryGroupMembers = "ShowOnlyPrimaryGroupMembers";
        }

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// Query string parameters appended to every avatar URL so the photos
        /// render in the muted icon style used by the profile header.
        /// </summary>
        private const string AvatarStyleParameters = "&Style=icon&BackgroundColor=E4E4E7&ForegroundColor=A1A1AA";

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<GroupMemberNavigationBag, GroupMemberNavigationOptionsBag>
            {
                Bag = new GroupMemberNavigationBag
                {
                    GroupMembers = new List<GroupMemberNavigationItemBag>()
                }
            };

            var person = GetPerson();

            if ( person == null )
            {
                return box;
            }

            box.Bag.PersonFullName = person.FullName;
            box.Bag.PersonPhotoUrl = $"{Person.GetPersonPhotoUrl( person, 400 )}{AvatarStyleParameters}";

            var subpageRoute = GetSubpageRoute( person );

            box.Bag.GroupMembers = GetGroupMembers( person )
                .Select( gm => new GroupMemberNavigationItemBag
                {
                    FullName = gm.Person.FullName,
                    PhotoUrl = $"{Person.GetPersonPhotoUrl( gm.Person.Initials, gm.Person.PhotoId, gm.Person.Age, gm.Person.Gender, gm.Person.RecordTypeValueId, gm.Person.AgeClassification, 400 )}{AvatarStyleParameters}",
                    PersonProfileUrl = RequestContext.ResolveRockUrl( $"~/Person/{gm.Person.IdKey}{subpageRoute}" )
                } )
                .ToList();

            return box;
        }

        /// <summary>
        /// Gets the person whose profile is being viewed, either from the page
        /// context or from the person page parameter.
        /// </summary>
        /// <returns>The resolved <see cref="Person"/> or <c>null</c> if one could not be determined.</returns>
        private Person GetPerson()
        {
            var person = RequestContext.GetContextEntity<Person>();

            if ( person != null )
            {
                return person;
            }

            var personKey = PageParameter( PageParameterKey.PersonId );

            if ( personKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new PersonService( RockContext ).Get( personKey, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Gets the other members of the person's group(s) of the configured
        /// group type, sorted for display (adults first, then children, oldest
        /// to youngest within each bucket).
        /// </summary>
        /// <param name="person">The person whose group members should be listed.</param>
        /// <returns>The sorted list of group members, excluding the person themself.</returns>
        private List<GroupMember> GetGroupMembers( Person person )
        {
            var groupTypeId = GroupTypeCache.GetId( GetAttributeValue( AttributeKey.GroupType ).AsGuid() );

            if ( !groupTypeId.HasValue )
            {
                return new List<GroupMember>();
            }

            var showOnlyPrimaryGroupMembers = GetAttributeValue( AttributeKey.ShowOnlyPrimaryGroupMembers ).AsBoolean();

            return new GroupMemberService( RockContext )
                .GetSortedGroupMemberListForPerson( person.Id, groupTypeId.Value, showOnlyPrimaryGroupMembers )
                .ToList();
        }

        /// <summary>
        /// Gets the portion of the current URL path that follows the person
        /// profile route (for example "/Groups" when viewing
        /// "/Person/123/Groups"), so member links keep the viewer on the same
        /// subpage. Returns an empty string when the current page is not on a
        /// person profile route.
        /// </summary>
        /// <param name="person">The person whose profile is being viewed.</param>
        /// <returns>The subpage route suffix, or an empty string.</returns>
        private string GetSubpageRoute( Person person )
        {
            var absolutePath = RequestContext.RequestUri?.AbsolutePath;

            if ( absolutePath.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            // The person key in the current URL may be an Id, IdKey or Guid, so
            // read it from the page context parameter rather than assuming a
            // particular format.
            var currentPersonKey = person.Id.ToString();

            if ( PageCache.PageContexts.TryGetValue( typeof( Person ).FullName, out var contextParameterName ) )
            {
                var parameterValue = PageParameter( contextParameterName );

                if ( parameterValue.IsNotNullOrWhiteSpace() )
                {
                    currentPersonKey = parameterValue;
                }
            }

            var personRootPath = RequestContext.ResolveRockUrl( $"~/Person/{currentPersonKey}" );
            var subpageRoute = absolutePath.ReplaceCaseInsensitive( personRootPath, string.Empty );

            return subpageRoute == absolutePath ? string.Empty : subpageRoute;
        }

        #endregion Methods
    }
}
