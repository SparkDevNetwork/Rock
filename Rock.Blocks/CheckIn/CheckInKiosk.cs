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
using Rock.Security;
using Rock.Utility;
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
        Description = "The page to use when kiosk setup is required.",
        Key = AttributeKey.SetupPage,
        IsRequired = true,
        Order = 0 )]

    [BooleanField( "Show Counts By Location",
        Description = "When displaying attendance counts on the admin login screen this will group the counts by location first instead of area first.",
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowCountsByLocation,
        Order = 1 )]

    [ContentChannelField( "Promotions Content Channel",
        Description = "The content channel to use for displaying promotions on the kiosk welcome screen.",
        Key = AttributeKey.PromotionsContentChannel,
        IsRequired = false,
        Order = 2 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "b208cafe-2194-4308-aa52-a920c516805a" )]
    [Rock.SystemGuid.BlockTypeGuid( "a27fd0aa-67ee-44c3-9e5f-3289c6a210f3" )]
    public class CheckInKiosk : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string SetupPage = "SetupPage";
            public const string ShowCountsByLocation = "ShowCountsByLocation";
            public const string PromotionsContentChannel = "PromotionsContentChannel";
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
                SetupPageRoute = this.GetLinkedPageUrl( AttributeKey.SetupPage ),
                ShowCountsByLocation = GetAttributeValue( AttributeKey.ShowCountsByLocation ).AsBoolean()
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
                Areas = areas.Where( a => savedConfiguration.AreaIds.Contains( a.IdKey ) )
                    .Select( a => new CheckInItemBag
                {
                    Id = a.IdKey,
                    Name = a.Name
                } ).ToList(),
                Template = template
            };
        }

        /// <summary>
        /// Tries to authenticate the PIN code provided.
        /// </summary>
        /// <param name="pinCode">The PIN code to be authenticated.</param>
        /// <param name="errorMessage">On return contains any error message that should be displayed.</param>
        /// <returns><c>true</c> if the PIN code was valid and trusted; otherwise <c>false</c>.</returns>
        private bool TryAuthenticatePin( string pinCode, out string errorMessage )
        {
            var pinAuth = AuthenticationContainer.GetComponent( typeof( Rock.Security.Authentication.PINAuthentication ).FullName );

            // Make sure PIN authentication is enabled.
            if ( pinAuth == null || !pinAuth.IsActive )
            {
                errorMessage = "Sorry, we couldn't find an account matching that PIN.";
                return false;
            }

            var userLoginService = new UserLoginService( RockContext );
            var userLogin = userLoginService.GetByUserName( pinCode );

            // Make sure this is a PIN auth user login.
            if ( userLogin == null || !userLogin.EntityTypeId.HasValue || userLogin.EntityTypeId.Value != pinAuth.TypeId )
            {
                errorMessage = "Sorry, we couldn't find an account matching that PIN.";
                return false;
            }

            // This should always return true, but just in case something changes
            // in the future.
            if ( !pinAuth.Authenticate( userLogin, null ) )
            {
                errorMessage = "Sorry, we couldn't find an account matching that PIN.";
                return false;
            }

            if ( !( userLogin.IsConfirmed ?? true ) )
            {
                errorMessage = "Sorry, account needs to be confirmed.";
                return false;
            }
            else if ( userLogin.IsLockedOut ?? false )
            {
                errorMessage = "Sorry, account is locked-out.";
                return false;
            }
            else
            {
                errorMessage = string.Empty;
                return true;
            }
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
            var promotionContentChannelGuid = GetAttributeValue( AttributeKey.PromotionsContentChannel ).AsGuidOrNull();

            if ( !promotionContentChannelGuid.HasValue )
            {
                return ActionOk( new List<PromotionBag>() );
            }

            var kiosk = DeviceCache.GetByIdKey( kioskId, RockContext );
            var contentChannel = new ContentChannelService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Include( cc => cc.Items )
                .Where( cc => cc.Guid == promotionContentChannelGuid.Value )
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

        /// <summary>
        /// Gets the current attendance counts for this kiosk. This will return
        /// all attendance records that match the kiosk's configured locations
        /// and the specified area list.
        /// </summary>
        /// <param name="kioskId">The encrypted kiosk identifier.</param>
        /// <param name="areaIds">The list of encrypted area identifiers.</param>
        /// <returns>An object that contains all the information to display the counts.</returns>
        [BlockAction]
        public BlockActionResult GetCurrentAttendance( string kioskId, List<string> areaIds )
        {
            var kiosk = DeviceCache.GetByIdKey( kioskId, RockContext );

            if ( kiosk == null )
            {
                return ActionBadRequest( "Kiosk not found." );
            }

            var kioskLocations = kiosk.GetAllLocations().ToList();
            var kioskLocationIds = kioskLocations.Select( l => l.Id ).ToList();

            var attendance = CheckInDirector.GetCurrentAttendance( RockDateTime.Now, kioskLocationIds, RockContext )
                .Select( a => new
                {
                    Id = a.AttendanceId,
                    AreaId = a.GroupTypeId,
                    GroupId = a.GroupId,
                    LocationId = a.LocationId,
                    Status = a.Status
                } )
                .ToList();

            var areaIdNumbers = areaIds.Select( id => IdHasher.Instance.GetId( id ) ).Where( id => id.HasValue ).Select( id => id.Value ).ToList();
            var groupLocationQry = new GroupLocationService( RockContext ).Queryable()
                .Select( gl => new
                {
                    gl.LocationId,
                    gl.GroupId
                } );
            var groups = new GroupService( RockContext )
                .Queryable()
                .Where( g => areaIdNumbers.Contains( g.GroupTypeId ) )
                .GroupJoin( groupLocationQry, g => g.Id, gl => gl.GroupId, (g, gl) => new
                {
                    Group = g,
                    GroupLocation = gl
                })
                .SelectMany( a => a.GroupLocation.DefaultIfEmpty(), (g, gl) => new
                {
                    g.Group.Id,
                    g.Group.Name,
                    g.Group.GroupTypeId,
                    gl.LocationId
                } )
                .ToList()
                .GroupBy( a => new { a.Id, a.Name, a.GroupTypeId } )
                .Select( g => new GroupOpportunityBag
                {
                    Id = IdHasher.Instance.GetHash( g.Key.Id ),
                    Name = g.Key.Name,
                    AreaId = IdHasher.Instance.GetHash( g.Key.GroupTypeId ),
                    LocationIds = g.Select( l => IdHasher.Instance.GetHash( l.LocationId ) ).ToList()
                } )
                .ToList();

            var locations = kioskLocations
                .Select( l => new LocationStatusItemBag
                {
                    Id = l.IdKey,
                    Name = l.Name,
                    IsOpen = l.IsActive
                } )
                .ToList();

            foreach ( var att in attendance )
            {
                //if ( groups.FirstOrDefault( a => a.Id == att.GroupId ) == null )
                //{
                //    var group = GroupCache.GetByIdKey( att.GroupId, RockContext );

                //    if ( group != null )
                //    {
                //        groups.Add( new CheckInItemBag
                //        {
                //            Id = group.IdKey,
                //            Name = group.Name
                //        } );
                //    }
                //}

                //if ( locations.FirstOrDefault( a => a.Id == att.LocationId ) == null )
                //{
                //    var location = NamedLocationCache.GetByIdKey( att.LocationId, RockContext );

                //    if ( location != null )
                //    {
                //        locations.Add( new CheckInItemBag
                //        {
                //            Id = location.IdKey,
                //            Name = location.Name
                //        } );
                //    }
                //}
            }

            return ActionOk( new
            {
                Attendance = attendance,
                Groups = groups,
                Locations = locations
            } );
        }

        /// <summary>
        /// Verifies that the PIN code is valid and can be used. This is used
        /// by the UI to perform the initial login step to the admins creen.
        /// </summary>
        /// <param name="pinCode">The PIN code to validate.</param>
        /// <returns>A 200-OK status if the PIN code was valid.</returns>
        [BlockAction]
        public BlockActionResult ValidatePinCode( string pinCode )
        {
            if ( !TryAuthenticatePin( pinCode, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk();
        }

        /// <summary>
        /// Sets the open/closed status of a location.
        /// </summary>
        /// <param name="pinCode">The PIN code to authenticate the person as.</param>
        /// <param name="locationId">The encrypted identifier of the location.</param>
        /// <param name="isOpen"><c>true</c> if the location should be opened; otherwise <c>false</c>.</param>
        /// <returns>A 200-OK status if the location's status was updated.</returns>
        [BlockAction]
        public BlockActionResult SetLocationStatus( string pinCode, string locationId, bool isOpen )
        {
            if ( !TryAuthenticatePin( pinCode, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            var location = new LocationService( RockContext ).Get( locationId, false );

            if ( location == null )
            {
                return ActionBadRequest( "Location not found." );
            }

            location.IsActive = isOpen;
            RockContext.SaveChanges();

            // Clear the old v1 cache to match functionality.
            Rock.CheckIn.KioskDevice.Clear();

            return ActionOk();
        }

        #endregion
    }
}
