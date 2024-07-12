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
using System.Threading;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Labels;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Utility.ExtensionMethods;
using Rock.ViewModels.Blocks.CheckIn.CheckInKiosk;
using Rock.ViewModels.CheckIn;
using Rock.ViewModels.CheckIn.Labels;
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

        /// <summary>
        /// Gets the printer device that the kiosk is configured to use.
        /// </summary>
        /// <param name="kioskId">The encrypted identifier of the kiosk.</param>
        /// <returns>An instance of <see cref="DeviceCache"/> that represents the printer device or <c>null</c></returns>
        private DeviceCache GetKioskPrinter( string kioskId )
        {
            var kiosk = DeviceCache.GetByIdKey( kioskId, RockContext );

            if ( kiosk == null )
            {
                return null;
            }

            if ( !kiosk.PrinterDeviceId.HasValue )
            {
                return null;
            }

            return DeviceCache.Get( kiosk.PrinterDeviceId.Value, RockContext );
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
            promotionItems = promotionItems
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
                .GroupJoin( groupLocationQry, g => g.Id, gl => gl.GroupId, ( g, gl ) => new
                {
                    Group = g,
                    GroupLocation = gl
                } )
                .SelectMany( a => a.GroupLocation.DefaultIfEmpty(), ( g, gl ) => new
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

        /// <summary>
        /// Gets the attendance list for use when displaying the list of attendance
        /// records that can be reprinted.
        /// </summary>
        /// <param name="pinCode">The PIN code to authorize the request.</param>
        /// <param name="kioskId">The encrypted identifier of the kiosk.</param>
        /// <param name="searchValue">An optional search value to limit the results.</param>
        /// <returns>A list of attendance records.</returns>
        [BlockAction]
        public BlockActionResult GetReprintAttendanceList( string pinCode, string kioskId, string searchValue )
        {
            if ( !TryAuthenticatePin( pinCode, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            var kiosk = DeviceCache.GetByIdKey( kioskId, RockContext );

            if ( kiosk == null )
            {
                return ActionBadRequest( "Kiosk was not found." );
            }

            var campusId = kiosk.GetCampusId();
            var now = RockDateTime.Now;

            if ( campusId.HasValue )
            {
                var campus = CampusCache.Get( campusId.Value );

                if ( campus != null )
                {
                    now = campus.CurrentDateTime;
                }
            }

            var attendanceQry = CheckInDirector.GetCurrentAttendanceQuery( now, RockContext );

            if ( campusId.HasValue )
            {
                attendanceQry = attendanceQry.Where( a => a.CampusId.HasValue && a.CampusId.Value == campusId.Value );
            }

            var items = attendanceQry
                .Select( a => new
                {
                    a.Id,
                    a.StartDateTime,
                    a.PersonAlias.Person.NickName,
                    a.PersonAlias.Person.LastName,
                    a.AttendanceCode.Code,
                    GroupId = a.Occurrence.GroupId.Value,
                    LocationId = a.Occurrence.LocationId.Value,
                    ScheduleId = a.Occurrence.ScheduleId.Value
                } )
                .ToList();

            var groupIds = items.Select( a => a.GroupId ).Distinct().ToList();
            var locationIds = items.Select( a => a.LocationId ).Distinct().ToList();
            var scheduleIds = items.Select( a => a.ScheduleId ).Distinct().ToList();

            // Get all the groups, locations and schedules from cache in bulk.
            // This improves performance because it is very likely that otherwise
            // we would be requesting the same cached object repeatedly.
            var groups = GroupCache.GetMany( groupIds, RockContext )
                .Select( g => new
                {
                    g.Id,
                    Bag = new CheckInItemBag
                    {
                        Id = g.IdKey,
                        Name = g.Name
                    }
                } )
                .ToList();
            var locations = NamedLocationCache.GetMany( locationIds, RockContext )
                .Select( l => new
                {
                    l.Id,
                    Bag = new CheckInItemBag
                    {
                        Id = l.IdKey,
                        Name = l.Name
                    }
                } )
                .ToList();
            var schedules = NamedScheduleCache.GetMany( scheduleIds, RockContext )
                .Select( s => new
                {
                    s.Id,
                    Bag = new CheckInItemBag
                    {
                        Id = s.IdKey,
                        Name = s.Name
                    }
                } )
                .ToList();

            var data = items
                .Select( a =>
                {
                    var group = groups.FirstOrDefault( g => g.Id == a.GroupId );
                    var location = locations.FirstOrDefault( l => l.Id == a.LocationId );
                    var schedule = schedules.FirstOrDefault( s => s.Id == a.ScheduleId );

                    if ( group == null || location == null || schedule == null )
                    {
                        return null;
                    }

                    return new ReprintAttendanceBag
                    {
                        Id = IdHasher.Instance.GetHash( a.Id ),
                        StartDateTime = a.StartDateTime.ToRockDateTimeOffset(),
                        NickName = a.NickName,
                        LastName = a.LastName,
                        SecurityCode = a.Code,
                        Group = group.Bag,
                        Location = location.Bag,
                        Schedule = schedule.Bag
                    };
                } )
                .Where( a => a != null )
                .ToList();

            return ActionOk( data );
        }

        /// <summary>
        /// Request to print labels for a single attendance record that already
        /// exists in the database. This is used to re-print lost labels.
        /// </summary>
        /// <param name="pinCode">The PIN code to authorize the request.</param>
        /// <param name="kioskId">The encrypted identifier of the kiosk.</param>
        /// <param name="attendanceId">The encrypted identifier of the attendance to print labels for.</param>
        /// <returns>A 200-OK response that indicates if any errors occurred during printing.</returns>
        [BlockAction]
        public async Task<BlockActionResult> PrintLabels( string pinCode, string kioskId, string attendanceId )
        {
            if ( !TryAuthenticatePin( pinCode, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            var printer = GetKioskPrinter( kioskId );

            if ( printer == null )
            {
                return ActionBadRequest( "This kiosk does not have a printer defined." );
            }

            var attendanceIdNumber = IdHasher.Instance.GetId( attendanceId );

            if ( !attendanceIdNumber.HasValue )
            {
                return ActionBadRequest( "Invalid attendance." );
            }

            // If the attendance record has legacy labels then use those for
            // re-printing instead of the new label format.
            if ( RockContext.Set<AttendanceData>().Any( a => a.Id == attendanceIdNumber.Value ) )
            {
                var attendance = new AttendanceService( RockContext ).Get( attendanceIdNumber.Value );
                var attendanceIds = new List<int> { attendance.Id };
                var possibleLabels = ZebraPrint.GetLabelTypesForPerson( attendance.PersonAlias.PersonId, attendanceIds );
                var fileGuids = possibleLabels.Select( pl => pl.FileGuid ).ToList();

                var (legacyMessages, legacyClientLabels) = ZebraPrint.ReprintZebraLabels( fileGuids, attendance.PersonAlias.PersonId, attendanceIds, null );

                var clientLabels = legacyClientLabels
                    .Select( label => new LegacyClientLabelBag
                    {
                        LabelFile = RequestContext.RootUrlPath + label.LabelFile,
                        LabelKey = label.LabelKey,
                        MergeFields = label.MergeFields,
                        PrinterAddress = label.PrinterAddress
                    } )
                    .ToList();

                return ActionOk( new
                {
                    ErrorMessages = legacyMessages,
                    LegacyLabels = clientLabels
                } );
            }

            // Use the new label format for re-printing.
            var director = new CheckInDirector( RockContext );
            var labels = director.LabelProvider.RenderLabels( new List<int> { attendanceIdNumber.Value }, null, false );

            var errorMessages = labels.Where( l => l.Error.IsNotNullOrWhiteSpace() )
                .Select( l => l.Error )
                .ToList();

            if ( !labels.Any() )
            {
                errorMessages.Add( "No labels to print." );
            }

            labels = labels.Where( l => l.Error.IsNullOrWhiteSpace() ).ToList();
            foreach ( var label in labels )
            {
                label.PrintTo = printer;
                label.PrintFrom = PrintFrom.Server;
            }

            // Print the labels with a 5 second timeout.
            var cts = new CancellationTokenSource( 5_000 );
            var printProvider = new LabelPrintProvider();

            try
            {
                var printerErrors = await printProvider.PrintLabelsAsync( labels, cts.Token );

                errorMessages.AddRange( printerErrors );
            }
            catch ( TaskCanceledException ) when ( cts.IsCancellationRequested )
            {
                errorMessages.Add( "Timeout waiting for labels to print." );
            }

            return ActionOk( new
            {
                ErrorMessages = errorMessages
            } );
        }

        #endregion
    }
}
