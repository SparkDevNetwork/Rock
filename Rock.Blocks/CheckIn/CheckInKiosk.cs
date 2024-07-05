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

using Rock.Attribute;
using Rock.CheckIn.v2;
using Rock.Model;
using Rock.Utility.ExtensionMethods;
using Rock.ViewModels.Blocks.CheckIn.CheckInKiosk;
using Rock.ViewModels.CheckIn;
using Rock.Web.Cache;

namespace Rock.Blocks.CheckIn
{
    /// <summary>
    /// The standard Rock block for performing check-in at a kiosk.
    /// </summary>

    [DisplayName( "Check-in Kiosk" )]
    [Category( "Check-in" )]
    [Description( "The standard Rock block for performing check-in at a kiosk." )]
    [IconCssClass( "fa fa-clipboard-check" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Setup Page",
        Key = AttributeKey.SetupPage,
        Description = "The page to use when kiosk setup is required.",
        IsRequired = true,
        Order = 0 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "b208cafe-2194-4308-aa52-a920c516805a" )]
    [Rock.SystemGuid.BlockTypeGuid( "a27fd0aa-67ee-44c3-9e5f-3289c6a210f3" )]
    public class CheckInKiosk : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string SetupPage = "SetupPage";
        }

        private static class PageParameterKey
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            RequestContext.Response.AddCssLink( RequestContext.ResolveRockUrl( "~/Styles/Blocks/Checkin/CheckInKiosk.css" ), true );

            return new
            {
                CurrentTheme = PageCache.Layout?.Site?.Theme?.ToLower(),
                SetupPageRoute = this.GetLinkedPageUrl( AttributeKey.SetupPage )
            };
        }

        /// <summary>
        /// Gets the automatic configuration for the specified kiosk.
        /// </summary>
        /// <param name="director">The check-in director for this operation.</param>
        /// <param name="kiosk">The kiosk whose configuration should be retrieved.</param>
        /// <returns>An instance of <see cref="KioskConfigurationBag"/> if the kiosk was valid; otherwise <c>null</c>.</returns>
        private KioskConfigurationBag GetKioskConfiguration( CheckInDirector director, SavedKioskConfigurationBag savedConfiguration )
        {
            var kiosk = DeviceCache.GetByIdKey( savedConfiguration.KioskId, RockContext );
            var templateGroupType = GroupTypeCache.GetByIdKey( savedConfiguration.TemplateId, RockContext );

            if ( kiosk == null || templateGroupType == null )
            {
                return null;
            }

            var areas = director.GetKioskAreas( kiosk );
            var template = director.GetConfigurationTemplateBag( templateGroupType );

            if ( template == null )
            {
                return null;
            }

            return new KioskConfigurationBag
            {
                Kiosk = CheckInKioskSetup.GetKioskBag( kiosk ),
                Areas = areas.Select( a => new CheckInItemBag
                {
                    Id = a.IdKey,
                    Name = a.Name
                } ).ToList(),
                Template = template
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the kiosk configuration to use given the saved configuration
        /// options.
        /// </summary>
        /// <param name="savedConfiguration">The options the kiosk was configured with.</param>
        /// <returns>The full configuration data for the kiosk.</returns>
        [BlockAction]
        public BlockActionResult GetKioskConfiguration( SavedKioskConfigurationBag savedConfiguration )
        {
            var director = new CheckInDirector( RockContext );
            var configuration = GetKioskConfiguration( director, savedConfiguration );

            if ( configuration == null )
            {
                return ActionBadRequest( "Configuration is not valid." );
            }

            return ActionOk( configuration );
        }

        /// <summary>
        /// Gets the promotion list defined for the template and kiosk.
        /// </summary>
        /// <param name="templateId">The check-in template identifier.</param>
        /// <param name="kioskId">The kiosk identifier.</param>
        /// <returns>A list of <see cref="PromotionBag"/> objects.</returns>
        [BlockAction]
        public BlockActionResult GetPromotionList( string templateId, string kioskId )
        {
            var kiosk = DeviceCache.GetByIdKey( kioskId, RockContext );
            var contentChannel = new ContentChannelService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Include( cc => cc.Items )
                .Where( cc => cc.Id == 8 )
                .FirstOrDefault();

            if ( kiosk == null || contentChannel == null )
            {
                return ActionOk( new List<PromotionBag>() );
            }

            contentChannel.Items.LoadAttributes( RockContext );

            var now = RockDateTime.Now;
            var campusId = kiosk.GetCampusId();
            var campusGuid = campusId.HasValue
                ? CampusCache.Get( campusId.Value )?.Guid ?? Guid.Empty
                : Guid.Empty;

            // Filter items by date.
            var promotionItems = contentChannel.Items
                .Where( item => item.StartDateTime <= now
                    && ( !item.ExpireDateTime.HasValue || item.ExpireDateTime >= now ) );

            // Filter items by approval.
            if ( contentChannel.RequiresApproval )
            {
                promotionItems = promotionItems.Where( item => item.Status == ContentChannelItemStatus.Approved );
            }

            // Filter items by kiosk campus.
            promotionItems=  promotionItems
                .Where( item => item.GetAttributeValue( "Campuses" ).IsNullOrWhiteSpace()
                    || item.GetAttributeValue( "Campuses" ).SplitDelimitedValues().AsGuidList().Contains( campusGuid ) );

            // Order the items.
            promotionItems = contentChannel.ItemsManuallyOrdered
                ? contentChannel.Items.OrderBy( item => item.Order )
                : contentChannel.Items.OrderBy( item => item.StartDateTime );

            var promotions = promotionItems
                .Select( item => new PromotionBag
                {
                    Url = $"/GetImage.ashx?guid={item.GetAttributeValue( "Image" )}",
                    Duration = item.GetAttributeValue( "DisplayDuration" ).AsIntegerOrNull() ?? 15
                } )
                .ToList();

            return ActionOk( promotions );
        }

        #endregion
    }
}
