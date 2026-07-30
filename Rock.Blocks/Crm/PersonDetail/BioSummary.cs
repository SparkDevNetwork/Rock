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
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Utility.Enums;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.PersonDetail.BioSummary;
using Rock.ViewModels.Crm;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Displays the person's photo, name, family drop-down, and badges at
    /// the top of the person profile.
    /// </summary>

    [DisplayName( "Person Bio Summary" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Person name, picture, and badges." )]
    [IconCssClass( "ti ti-user" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [SecurityAction( Authorization.VIEW_PROTECTION_PROFILE, "The roles and/or users that can view the protection profile alert for the selected person." )]

    #region Block Attributes

    [BadgesField( "Badges",
        Description = "The label badges to display in this block.",
        EntityTypeGuid = Rock.SystemGuid.EntityType.PERSON,
        EnhancedSelection = true,
        IsRequired = false,
        Key = AttributeKey.Badges,
        Order = 0 )]

    #endregion Block Attributes

    [InitialBlockHeight( 0 )]
    [Rock.Web.UI.ContextAware( typeof( Person ) )]
    [Rock.SystemGuid.EntityTypeGuid( "17CA6FB3-E714-46CF-9EBF-5BD49A8DDFE8" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "D3946070-E288-4FD7-8B57-B59BD882C89C" )]
    [Rock.SystemGuid.BlockTypeGuid( "7249D05F-0FD1-4F44-88EB-AD46DEB1DAEA" )]
    public class BioSummary : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Badges = "Badges";
        }

        /*
            7/6/26 - MSE

            The WebForms Bio Summary had no group-type settings of its own. It
            read them from the GroupMemberNavigation block via a shared page
            item, which Obsidian blocks cannot use. So we read those two
            attribute values directly from that block, keeping it the single
            source of truth.

            Reason: Follow GroupMemberNavigation's family drop-down config
            without duplicating its settings here.
        */
        private static class GroupMemberNavigationBlock
        {
            public const string BlockTypeGuid = "35D091FA-8311-42D1-83F7-3E67B9EE9675";

            public const string GroupTypeAttributeKey = "GroupType";

            public const string ShowOnlyPrimaryGroupMembersAttributeKey = "ShowOnlyPrimaryGroupMembers";
        }

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The query string appended to avatar URLs so profile photos render
        /// with the standard icon style and neutral colors.
        /// </summary>
        private static readonly string AvatarStyleQueryString = "&Style=icon&BackgroundColor=E4E4E7&ForegroundColor=A1A1AA";

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<BioSummaryBag, BioSummaryOptionsBag>();

            var person = GetPerson();

            if ( person == null )
            {
                RedirectToCurrentPersonIfMerged();

                return box;
            }

            RequestContext.Response.SetBrowserTitle( person.FullName );

            if ( IsNamelessPerson( person ) )
            {
                return box;
            }

            box.Bag = new BioSummaryBag
            {
                PhotoUrl = GetPhotoUrl( person ),
                PersonName = GetPersonName( person ),
                IsBusiness = person.IsBusiness(),
                IsDeceased = person.IsDeceased,
                AccountProtectionProfileText = GetAccountProtectionProfileText( person ),
                FamilyMembers = GetFamilyMembers( person ),
                Badges = GetBadges( person )
            };

            return box;
        }

        /// <summary>
        /// Gets the person being viewed, either from the page context or
        /// from the PersonId page parameter.
        /// </summary>
        /// <returns>The person being viewed or <c>null</c> if one could not be determined.</returns>
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
        /// Permanently redirects to the current person's profile when the
        /// PersonId page parameter refers to a person that was merged into
        /// another record.
        /// </summary>
        private void RedirectToCurrentPersonIfMerged()
        {
            var personAlias = GetMergedPersonAlias();

            if ( personAlias == null )
            {
                return;
            }

            // Emit the identifier in the same form the site accepts so the
            // redirected request is able to resolve it.
            var personKey = PageCache.Layout.Site.DisablePredictableIds
                ? IdHasher.Instance.GetHash( personAlias.PersonId )
                : personAlias.PersonId.ToString();

            var parameters = new Dictionary<string, string>( RequestContext.GetPageParameters(), StringComparer.OrdinalIgnoreCase )
            {
                [PageParameterKey.PersonId] = personKey
            };

            // Prefer a route that carries the PersonId so the corrected URL
            // keeps the friendly form instead of a query string.
            var routeId = PageCache.PageRoutes
                .Where( r => r.Route.IndexOf( $"{{{PageParameterKey.PersonId}}}", StringComparison.OrdinalIgnoreCase ) >= 0 )
                .Select( r => r.Id )
                .FirstOrDefault();

            var url = new PageReference( PageCache.Id, routeId, parameters ).BuildUrl();

            RequestContext.Response.RedirectToUrl( url, permanent: true );
        }

        /// <summary>
        /// Gets the person alias left behind by a person merge when the
        /// PersonId page parameter is an old identifier for the merged person.
        /// </summary>
        /// <returns>The alias pointing at the surviving person, or <c>null</c> if the parameter is not an old alias identifier.</returns>
        private PersonAlias GetMergedPersonAlias()
        {
            /*
                7/6/26 - MSE

                This only runs after the parameter failed to resolve to a live
                person, so decode it the same way GetPerson() would have:
                integer ids only when the site allows predictable ids, IdKeys
                and Guids always. Accepting integers on a site with predictable
                ids disabled caused an infinite redirect loop, because
                GetByAliasId() resolves a live person's primary alias and the
                redirected request would reject the integer all over again.

                Reason: Redirect stale IdKey/Guid links and avoid a redirect loop.
            */
            var personKey = PageParameter( PageParameterKey.PersonId );
            var personAliasService = new PersonAliasService( RockContext );

            var aliasPersonGuid = personKey.AsGuidOrNull();

            if ( aliasPersonGuid.HasValue )
            {
                return personAliasService.GetByAliasGuid( aliasPersonGuid.Value );
            }

            var aliasPersonId = PageCache.Layout.Site.DisablePredictableIds
                ? null
                : personKey.AsIntegerOrNull();

            if ( !aliasPersonId.HasValue )
            {
                aliasPersonId = IdHasher.Instance.GetId( personKey );
            }

            if ( !aliasPersonId.HasValue )
            {
                return null;
            }

            return personAliasService.GetByAliasId( aliasPersonId.Value );
        }

        /// <summary>
        /// Determines whether the person is a nameless record, which has no
        /// meaningful bio information to display.
        /// </summary>
        /// <param name="person">The person being viewed.</param>
        /// <returns><c>true</c> if the person is a nameless record; otherwise <c>false</c>.</returns>
        private bool IsNamelessPerson( Person person )
        {
            var namelessRecordTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() );

            return namelessRecordTypeId.HasValue && person.RecordTypeValueId == namelessRecordTypeId.Value;
        }

        /// <summary>
        /// Gets the URL of the person's profile photo, resolved for the
        /// current application path and including the standard avatar style
        /// parameters used on person profile pages.
        /// </summary>
        /// <param name="person">The person being viewed.</param>
        /// <returns>The photo URL including the avatar styling parameters.</returns>
        private string GetPhotoUrl( Person person )
        {
            var photoUrl = RequestContext.ResolveRockUrl( Person.GetPersonPhotoUrl( person, 400 ) );

            return $"{photoUrl}{AvatarStyleQueryString}";
        }

        /// <summary>
        /// Gets the person's display name. A business record displays its
        /// business name, otherwise the nick name and last name are used with
        /// a formal title prefix when the person's title is marked formal.
        /// </summary>
        /// <param name="person">The person being viewed.</param>
        /// <returns>The display name for the person.</returns>
        private string GetPersonName( Person person )
        {
            if ( person.IsBusiness() )
            {
                return person.LastName;
            }

            var titleText = string.Empty;

            if ( person.TitleValueId.HasValue )
            {
                var titleValue = DefinedValueCache.Get( person.TitleValueId.Value );

                if ( titleValue != null && titleValue.GetAttributeValue( "IsFormal" ).AsBoolean() )
                {
                    titleText = $"{titleValue.Value} ";
                }
            }

            return $"{titleText}{person.NickName} {person.LastName}";
        }

        /// <summary>
        /// Gets the display text for the person's account protection profile,
        /// or <c>null</c> when the alert should not be shown because the
        /// profile is low or the viewer lacks permission.
        /// </summary>
        /// <param name="person">The person being viewed.</param>
        /// <returns>The protection profile display text or <c>null</c>.</returns>
        private string GetAccountProtectionProfileText( Person person )
        {
            var isAlertVisible = person.AccountProtectionProfile > AccountProtectionProfile.Low
                && BlockCache.IsAuthorized( Authorization.VIEW_PROTECTION_PROFILE, RequestContext.CurrentPerson );

            return isAlertVisible ? person.AccountProtectionProfile.ConvertToString( true ) : null;
        }

        /// <summary>
        /// Gets the other members of the person's family, ordered for display
        /// in the family drop-down.
        /// </summary>
        /// <param name="person">The person being viewed.</param>
        /// <returns>A list of <see cref="FamilyMemberBag"/> objects describing the family members.</returns>
        private List<FamilyMemberBag> GetFamilyMembers( Person person )
        {
            var groupMemberNavigationBlockTypeGuid = GroupMemberNavigationBlock.BlockTypeGuid.AsGuid();
            var groupMemberNavigationBlock = PageCache.Blocks
                .FirstOrDefault( b => b.BlockType.Guid == groupMemberNavigationBlockTypeGuid );

            var groupTypeGuid = groupMemberNavigationBlock?.GetAttributeValue( GroupMemberNavigationBlock.GroupTypeAttributeKey ).AsGuidOrNull()
                ?? Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
            var groupTypeId = GroupTypeCache.GetId( groupTypeGuid );

            if ( !groupTypeId.HasValue )
            {
                return new List<FamilyMemberBag>();
            }

            var showOnlyPrimaryGroupMembers = groupMemberNavigationBlock?.GetAttributeValue( GroupMemberNavigationBlock.ShowOnlyPrimaryGroupMembersAttributeKey ).AsBoolean() ?? false;

            return new GroupMemberService( RockContext )
                .GetSortedGroupMemberListForPerson( person.Id, groupTypeId.Value, showOnlyPrimaryGroupMembers )
                .Select( groupMember => new FamilyMemberBag
                {
                    PersonIdKey = groupMember.Person.IdKey,
                    FullName = groupMember.Person.FullName,
                    PhotoUrl = RequestContext.ResolveRockUrl( Person.GetPersonPhotoUrl(
                        groupMember.Person.Initials,
                        groupMember.Person.PhotoId,
                        groupMember.Person.Age,
                        groupMember.Person.Gender,
                        groupMember.Person.RecordTypeValueId,
                        groupMember.Person.AgeClassification,
                        400 ) ) + AvatarStyleQueryString
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the rendered badge content for the badge types configured on
        /// the block, filtered to those the viewer may see.
        /// </summary>
        /// <param name="person">The person to use as the entity when rendering the badges.</param>
        /// <returns>A list of <see cref="RenderedBadgeBag"/> objects that contain the rendered badges.</returns>
        private List<RenderedBadgeBag> GetBadges( Person person )
        {
            var badgeTypeGuids = GetAttributeValue( AttributeKey.Badges ).SplitDelimitedValues().AsGuidList();

            if ( !badgeTypeGuids.Any() )
            {
                return new List<RenderedBadgeBag>();
            }

            // Get all the badge types to display and filter out any that the
            // viewer does not have access to.
            var badges = badgeTypeGuids
                .Select( g => BadgeCache.Get( g ) )
                .Where( b => b != null && b.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .OrderBy( b => b.Order )
                .ToList();

            // Render all the badges and then filter out any that are empty.
            return badges.Select( b => b.RenderBadge( person ) )
                .Where( b => b.Html.IsNotNullOrWhiteSpace() || b.JavaScript.IsNotNullOrWhiteSpace() )
                .ToList();
        }

        #endregion Methods
    }
}
