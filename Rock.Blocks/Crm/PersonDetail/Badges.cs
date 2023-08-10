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
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Crm.PersonDetail.Badges;
using Rock.ViewModels.Crm;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Handles displaying badges for a person.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Badges" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Handles displaying badges for a person." )]
    [IconCssClass( "fa fa-certificate" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [BadgesField(
        "Top Left Badges",
        Description = "The badges that displayed in the top left section of the badge bar.",
        EntityTypeGuid = Rock.SystemGuid.EntityType.PERSON,
        EnhancedSelection = true,
        IsRequired = false,
        Key = AttributeKey.TopLeftBadges,
        Order = 0 )]

    [BadgesField(
        "Top Middle Badges",
        Description = "The badges that displayed in the top middle section of the badge bar.",
        EntityTypeGuid = Rock.SystemGuid.EntityType.PERSON,
        EnhancedSelection = true,
        IsRequired = false,
        Key = AttributeKey.TopMiddleBadges,
        Order = 1 )]

    [BadgesField(
        "Top Right Badges",
        Description = "The badges that displayed in the top right section of the badge bar.",
        EntityTypeGuid = Rock.SystemGuid.EntityType.PERSON,
        EnhancedSelection = true,
        IsRequired = false,
        Key = AttributeKey.TopRightBadges,
        Order = 2 )]

    [BadgesField(
        "Bottom Left Badges",
        Description = "The badges that displayed in the bottom left section of the badge bar.",
        EntityTypeGuid = Rock.SystemGuid.EntityType.PERSON,
        EnhancedSelection = true,
        IsRequired = false,
        Key = AttributeKey.BottomLeftBadges,
        Order = 3 )]

    [BadgesField(
        "Bottom Right Badges",
        Description = "The badges that displayed in the bottom right section of the badge bar.",
        EntityTypeGuid = Rock.SystemGuid.EntityType.PERSON,
        EnhancedSelection = true,
        IsRequired = false,
        Key = AttributeKey.BottomRightBadges,
        Order = 4 )]

    #endregion

    [Rock.Web.UI.ContextAware( typeof( Person ) )]
    [Rock.SystemGuid.EntityTypeGuid( "86e12a6c-2086-4562-b50e-3ea1e8b5b017" )]
    [Rock.SystemGuid.BlockTypeGuid( "2412c653-9369-4772-955e-80ee8fa051e3" )]
    public class Badges : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string TopLeftBadges = "TopLeftBadges";
            public const string TopMiddleBadges = "TopMiddleBadges";
            public const string TopRightBadges = "TopRightBadges";
            public const string BottomLeftBadges = "BottomLeftBadges";
            public const string BottomRightBadges = "BottomRightBadges";
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var person = GetPersonForBadges( rockContext );

                return new BadgesConfigurationBox
                {
                    PersonKey = person?.IdKey,
                    TopLeftBadges = GetBadges( person, GetAttributeValue( AttributeKey.TopLeftBadges ).SplitDelimitedValues().AsGuidList() ),
                    TopMiddleBadges = GetBadges( person, GetAttributeValue( AttributeKey.TopMiddleBadges ).SplitDelimitedValues().AsGuidList() ),
                    TopRightBadges = GetBadges( person, GetAttributeValue( AttributeKey.TopRightBadges ).SplitDelimitedValues().AsGuidList() ),
                    BottomLeftBadges = GetBadges( person, GetAttributeValue( AttributeKey.BottomLeftBadges ).SplitDelimitedValues().AsGuidList() ),
                    BottomRightBadges = GetBadges( person, GetAttributeValue( AttributeKey.BottomRightBadges ).SplitDelimitedValues().AsGuidList() )
                };
            }
        }

        /// <summary>
        /// Gets the person object to use for rendering badges.
        /// </summary>
        /// <returns>A <see cref="Person"/> object to use or <c>null</c> if we were unable to determine one.</returns>
        private Person GetPersonForBadges( RockContext rockContext )
        {
            var person = RequestContext.GetContextEntity<Person>();

            if ( person != null )
            {
                return person;
            }

            var personKey = RequestContext.GetPageParameter( "personId" );

            return new PersonService( rockContext ).Get( personKey, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Gets the list of badge content for the given badge types.
        /// </summary>
        /// <param name="person">The person to use as the entity when rendering the badges.</param>
        /// <param name="badgeTypeGuids">The badge type unique identifiers to be rendered.</param>
        /// <returns>A list of <see cref="RenderedBadgeBag"/> objects that contain the rendered badges.</returns>
        private List<RenderedBadgeBag> GetBadges( Person person, List<Guid> badgeTypeGuids )
        {
            if ( person == null || !badgeTypeGuids.Any() )
            {
                return new List<RenderedBadgeBag>();
            }

            // Get all the badge types to display and filter out any that the
            // user does not have access to.
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

        #endregion
    }
}
