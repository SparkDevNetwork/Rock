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
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web.Http;

using Rock.Common.Mobile;
using Rock.Common.Mobile.Enums;
using Rock.Mobile;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Provides API interfaces for mobile applications to use when communicating with Rock.
    /// </summary>
    /// <seealso cref="Rock.Rest.ApiControllerBase" />
    [Rock.SystemGuid.RestControllerGuid( "EE29C4BA-5B17-48BB-8309-29BBB29464D0")]
    public class MobileController : ApiControllerBase 
    {
        /// <summary>
        /// Gets the communication interaction channel identifier.
        /// </summary>
        /// <value>
        /// The communication interaction channel identifier.
        /// </value>
        private static int CommunicationInteractionChannelId
        {
            get
            {
                if ( _communicationInteractionChannelId == 0 )
                {
                    _communicationInteractionChannelId = InteractionChannelCache.GetId( SystemGuid.InteractionChannel.COMMUNICATION.AsGuid() ).Value;
                }

                return _communicationInteractionChannelId;
            }
        }
        private static int _communicationInteractionChannelId;

        /// <summary>
        /// The communication interaction channel unique identifier
        /// </summary>
        private static readonly Guid _communicationInteractionChannelGuid = SystemGuid.InteractionChannel.COMMUNICATION.AsGuid();

        #region API Methods

        /// <summary>
        /// Gets the launch packet.
        /// </summary>
        /// <param name="deviceIdentifier">The unique device identifier for this device.</param>
        /// <param name="notificationsEnabled">Determines if notifications are fully enabled on the device.</param>
        /// <returns>An action result.</returns>
        [System.Web.Http.Route( "api/mobile/GetLaunchPacket" )]
        [HttpGet]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "BA9E7BA3-FCC1-4B1D-9FA1-A9946076B361" )]
        public IHttpActionResult GetLaunchPacket( string deviceIdentifier = null, bool? notificationsEnabled = null )
        {
            var site = MobileHelper.GetCurrentApplicationSite();
            var additionalSettings = site?.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();
            var rockContext = new Rock.Data.RockContext();
            var person = GetPerson( rockContext );
            var deviceData = Request.GetHeader( "X-Rock-DeviceData" ).FromJsonOrNull<DeviceData>();

            if ( additionalSettings == null || !additionalSettings.LastDeploymentDate.HasValue )
            {
                return NotFound();
            }

            // Ensure the user login is still active, otherwise log them out.
            var principal = ControllerContext.Request.GetUserPrincipal();
            if ( person != null && !principal.Identity.Name.StartsWith( "rckipid=" ) )
            {
                var userLogin = new UserLoginService( rockContext ).GetByUserName( principal.Identity.Name );

                if ( userLogin?.IsConfirmed != true || userLogin?.IsLockedOut == true )
                {
                    person = null;
                }
            }

            var launchPacket = new LaunchPacket
            {
                RockVersion = Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber(),
                LatestVersionId = additionalSettings.LastDeploymentVersionId ?? ( int ) ( additionalSettings.LastDeploymentDate.Value.ToJavascriptMilliseconds() / 1000 ),
                IsSiteAdministrator = site.IsAuthorized( Rock.Security.Authorization.EDIT, person )
            };

            if ( deviceData.DeviceType == DeviceType.Phone )
            {
                launchPacket.LatestVersionSettingsUrl = additionalSettings.PhoneUpdatePackageUrl;
            }
            else if ( deviceData.DeviceType == DeviceType.Tablet )
            {
                launchPacket.LatestVersionSettingsUrl = additionalSettings.TabletUpdatePackageUrl;
            }
            else
            {
                return NotFound();
            }

            if ( person != null )
            {
                //var principal = ControllerContext.Request.GetUserPrincipal();

                launchPacket.CurrentPerson = MobileHelper.GetMobilePerson( person, site );
                launchPacket.CurrentPerson.AuthToken = MobileHelper.GetAuthenticationToken( principal.Identity.Name );

                UserLoginService.UpdateLastLogin( principal.Identity.Name );
            }

            //
            // Get or create the personal device.
            //
            if ( deviceIdentifier.IsNotNullOrWhiteSpace() )
            {
                var mobileDeviceTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSONAL_DEVICE_TYPE_MOBILE ).Id;
                var personalDeviceService = new PersonalDeviceService( rockContext );
                var personalDevice = personalDeviceService.Queryable()
                    .Where( a => a.DeviceUniqueIdentifier == deviceIdentifier && a.PersonalDeviceTypeValueId == mobileDeviceTypeValueId && a.SiteId == site.Id )
                    .FirstOrDefault();

                if ( personalDevice == null )
                {
                    personalDevice = new PersonalDevice
                    {
                        DeviceUniqueIdentifier = deviceIdentifier,
                        PersonalDeviceTypeValueId = mobileDeviceTypeValueId,
                        SiteId = site.Id,
                        PlatformValueId = deviceData.DevicePlatform.GetDevicePlatformValueId(),
                        PersonAliasId = person?.PrimaryAliasId,
                        NotificationsEnabled = true,
                        Manufacturer = deviceData.Manufacturer,
                        Model = deviceData.Model,
                        Name = deviceData.Name,
                        LastSeenDateTime = RockDateTime.Now
                    };

                    personalDeviceService.Add( personalDevice );
                    rockContext.SaveChanges();
                }
                else
                {
                    // A change is determined as one of the following:
                    // 1) A change in Name, Manufacturer, Model, or NotificationsEnabled.
                    // 2) Device not being active.
                    // 3) Not seen in 24 hours.
                    // 4) Signed in with a different person.
                    var hasDeviceChanged = !personalDevice.IsActive
                        || personalDevice.Name != deviceData.Name
                        || personalDevice.Manufacturer != deviceData.Manufacturer
                        || personalDevice.Model != deviceData.Model
                        || personalDevice.NotificationsEnabled != ( notificationsEnabled ?? true )
                        || !personalDevice.LastSeenDateTime.HasValue
                        || personalDevice.LastSeenDateTime.Value.AddDays( 1 ) < RockDateTime.Now
                        || ( person != null && personalDevice.PersonAliasId != person.PrimaryAliasId );

                    if ( hasDeviceChanged )
                    {
                        personalDevice.IsActive = true;
                        personalDevice.Manufacturer = deviceData.Manufacturer;
                        personalDevice.Model = deviceData.Model;
                        personalDevice.Name = deviceData.Name;
                        personalDevice.LastSeenDateTime = RockDateTime.Now;

                        if ( notificationsEnabled.HasValue )
                        {
                            personalDevice.NotificationsEnabled = notificationsEnabled.Value;
                        }

                        // Update the person tied to the device, but never blank it out. 
                        if ( person != null && personalDevice.PersonAliasId != person.PrimaryAliasId )
                        {
                            personalDevice.PersonAliasId = person.PrimaryAliasId;
                        }

                        rockContext.SaveChanges();
                    }
                }

                launchPacket.PersonalDeviceGuid = personalDevice.Guid;
            }

            return Ok( launchPacket );
        }

        /// <summary>
        /// Updates the push notification registration token for a personal device.
        /// </summary>
        /// <param name="personalDeviceGuid">The personal device unique identifier.</param>
        /// <param name="registration">The registration token used to send push notifications.</param>
        /// <param name="notificationsEnabled">Determines if notifications are fully enabled on the device.</param>
        /// <returns>A status code that indicates if the request was successful.</returns>
        [System.Web.Http.Route( "api/mobile/UpdateDeviceRegistrationByGuid/{personalDeviceGuid}" )]
        [HttpPut]
        [Rock.SystemGuid.RestActionGuid( "B35111EB-9EBF-45CC-8BC1-54C01A271841" )]
        public IHttpActionResult UpdateDeviceRegistrationByGuid( Guid personalDeviceGuid, string registration, bool? notificationsEnabled = null )
        {
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                var service = new PersonalDeviceService( rockContext );

                // MAC address
                var personalDevice = service.Get( personalDeviceGuid );
                if ( personalDevice == null )
                {
                    return NotFound();
                }

                personalDevice.DeviceRegistrationId = registration;

                if ( notificationsEnabled.HasValue )
                {
                    personalDevice.NotificationsEnabled = notificationsEnabled.Value;
                }

                rockContext.SaveChanges();

                return Ok();
            }
        }

        /// <summary>
        /// Posts the interactions that have been queued up by the mobile client.
        /// </summary>
        /// <param name="sessions">The sessions.</param>
        /// <param name="personalDeviceGuid">The unique identifier of the device sending the interaction data.</param>
        /// <returns>An HTTP status code indicating the result.</returns>
        [System.Web.Http.Route( "api/mobile/Interactions" )]
        [HttpPost]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "2AB24DF6-7181-41DC-8968-8192D816AE1C" )]
        public IHttpActionResult PostInteractions( [FromBody] List<MobileInteractionSession> sessions, Guid? personalDeviceGuid = null )
        {
            var person = GetPerson();
            var ipAddress = System.Web.HttpContext.Current?.Request?.UserHostAddress;

            using ( var rockContext = new Data.RockContext() )
            {
                var interactionChannelService = new InteractionChannelService( rockContext );
                var interactionComponentService = new InteractionComponentService( rockContext );
                var interactionSessionService = new InteractionSessionService( rockContext );
                var interactionService = new InteractionService( rockContext );
                var userLoginService = new UserLoginService( rockContext );
                var channelMediumTypeValue = DefinedValueCache.Get( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE );
                var pageEntityTypeId = EntityTypeCache.Get( typeof( Model.Page ) ).Id;

                //
                // Check to see if we have a site and the API key is valid.
                //
                if ( MobileHelper.GetCurrentApplicationSite() == null )
                {
                    return StatusCode( System.Net.HttpStatusCode.Forbidden );
                }

                //
                // Get the personal device identifier if they provided it's unique identifier.
                //
                int? personalDeviceId = null;
                if ( personalDeviceGuid.HasValue )
                {
                    personalDeviceId = new PersonalDeviceService( rockContext ).GetId( personalDeviceGuid.Value );
                }

                //
                // Create a quick way to cache data since we have to loop twice.
                //
                var interactionComponentLookup = new Dictionary<string, int>();

                //
                // Helper method to get a cache key for looking up the component Id.
                //
                string GetComponentCacheKey( MobileInteraction mi )
                {
                    return $"{mi.AppId}:{mi.PageGuid}:{mi.ChannelGuid}:{mi.ChannelId}:{mi.ComponentId}:{mi.ComponentName}";
                }

                //
                // Interactions Components will now try to load from cache which
                // causes problems if we are inside a transaction. So first loop through
                // everything and make sure all our components and channels exist.
                //
                var prePassInteractions = sessions.SelectMany( a => a.Interactions )
                    .DistinctBy( a => GetComponentCacheKey( a ) );

                //
                // It's safe to do this pre-pass outside the transaction since we are just creating
                // the channels and components (if necessary), which is going to have to be done at
                // at some point no matter what.
                //
                foreach ( var mobileInteraction in prePassInteractions )
                {
                    //
                    // Lookup the interaction channel, and create it if it doesn't exist
                    //
                    if ( mobileInteraction.AppId.HasValue && mobileInteraction.PageGuid.HasValue )
                    {
                        var site = SiteCache.Get( mobileInteraction.AppId.Value );
                        var page = PageCache.Get( mobileInteraction.PageGuid.Value );

                        if ( site == null || page == null )
                        {
                            continue;
                        }

                        //
                        // Try to find an existing interaction channel.
                        //
                        var interactionChannelId = InteractionChannelCache.GetChannelIdByTypeIdAndEntityId( channelMediumTypeValue.Id, site.Id, site.Name, pageEntityTypeId, null );

                        //
                        // Get an existing or create a new component.
                        //
                        var interactionComponentId = InteractionComponentCache.GetComponentIdByChannelIdAndEntityId( interactionChannelId, page.Id, page.InternalName );

                        interactionComponentLookup.AddOrReplace( GetComponentCacheKey( mobileInteraction ), interactionComponentId );
                    }
                    else if ( mobileInteraction.ChannelId.HasValue || mobileInteraction.ChannelGuid.HasValue )
                    {
                        int? interactionChannelId = null;

                        if ( mobileInteraction.ChannelId.HasValue )
                        {
                            interactionChannelId = mobileInteraction.ChannelId.Value;
                        }
                        else if ( mobileInteraction.ChannelGuid.HasValue )
                        {
                            interactionChannelId = InteractionChannelCache.Get( mobileInteraction.ChannelGuid.Value )?.Id;
                        }

                        if ( interactionChannelId.HasValue )
                        {
                            if ( mobileInteraction.ComponentId.HasValue )
                            {
                                // Use the provided component identifier.
                                interactionComponentLookup.AddOrReplace( GetComponentCacheKey( mobileInteraction ), mobileInteraction.ComponentId.Value );
                            }
                            else if ( mobileInteraction.ComponentName.IsNotNullOrWhiteSpace() )
                            {
                                int interactionComponentId;

                                // Get or create a new component with the details we have.
                                if ( mobileInteraction.ComponentEntityId.HasValue )
                                {
                                    interactionComponentId = InteractionComponentCache.GetComponentIdByChannelIdAndEntityId( interactionChannelId.Value, mobileInteraction.ComponentEntityId, mobileInteraction.ComponentName );
                                }
                                else
                                {
                                    var interactionComponent = interactionComponentService.GetComponentByComponentName( interactionChannelId.Value, mobileInteraction.ComponentName );

                                    rockContext.SaveChanges();

                                    interactionComponentId = interactionComponent.Id;
                                }

                                interactionComponentLookup.AddOrReplace( GetComponentCacheKey( mobileInteraction ), interactionComponentId );
                            }
                        }
                    }
                }

                //
                // Now wrap the actual interaction creation inside a transaction. We should
                // probably move this so it uses the InteractionTransaction class for better
                // performance. This is so we can inform the client that either everything
                // saved or that nothing saved. No partial saves here.
                //
                rockContext.WrapTransaction( () =>
                {
                    foreach ( var mobileSession in sessions )
                    {
                        // Skip any invalid Guids since the session record is
                        // created with a direct SQL insert which bypasses the
                        // normal validation logic.
                        if ( mobileSession.Guid == Guid.Empty )
                        {
                            continue;
                        }

                        var interactionGuids = mobileSession.Interactions.Select( i => i.Guid ).ToList();
                        var existingInteractionGuids = interactionService.Queryable()
                            .Where( i => interactionGuids.Contains( i.Guid ) )
                            .Select( i => i.Guid )
                            .ToList();

                        //
                        // Loop through all interactions that don't already exist and add each one.
                        //
                        foreach ( var mobileInteraction in mobileSession.Interactions.Where( i => !existingInteractionGuids.Contains( i.Guid ) ) )
                        {
                            string cacheKey = GetComponentCacheKey( mobileInteraction );

                            if ( !interactionComponentLookup.ContainsKey( cacheKey ) )
                            {
                                // Shouldn't happen, but just in case.
                                continue;
                            }

                            var interactionComponentId = interactionComponentLookup[cacheKey];

                            //
                            // Add the interaction
                            //
                            var interaction = interactionService.CreateInteraction( interactionComponentId,
                                mobileInteraction.EntityId,
                                mobileInteraction.Operation,
                                mobileInteraction.Summary,
                                mobileInteraction.Data,
                                person?.PrimaryAliasId,
                                RockDateTime.ConvertLocalDateTimeToRockDateTime( mobileInteraction.DateTime.LocalDateTime ),
                                mobileSession.Application,
                                mobileSession.OperatingSystem,
                                mobileSession.ClientType,
                                null,
                                ipAddress,
                                mobileSession.Guid );

                            interaction.Guid = mobileInteraction.Guid;
                            interaction.PersonalDeviceId = personalDeviceId;
                            interaction.RelatedEntityTypeId = mobileInteraction.RelatedEntityTypeId;
                            interaction.RelatedEntityId = mobileInteraction.RelatedEntityId;
                            interaction.ChannelCustom1 = mobileInteraction.ChannelCustom1;
                            interaction.ChannelCustom2 = mobileInteraction.ChannelCustom2;
                            interaction.ChannelCustomIndexed1 = mobileInteraction.ChannelCustomIndexed1;

                            interactionService.Add( interaction );

                            // Attempt to process this as a communication interaction.
                            ProcessCommunicationInteraction( mobileSession, mobileInteraction, rockContext );
                        }
                    }

                    rockContext.SaveChanges();
                } );
            }

            return Ok();
        }

        /// <summary>
        /// Performs a login from a mobile application.
        /// </summary>
        /// <param name="loginParameters">The login parameters to use during authentication.</param>
        /// <param name="personalDeviceGuid">The personal device unique identifier being used to log the person in.</param>
        /// <returns>A MobilePerson object if the login was successful.</returns>
        [System.Web.Http.Route( "api/mobile/Login" )]
        [HttpPost]
        [Rock.SystemGuid.RestActionGuid( "D21CDF04-9190-4D86-B2F9-0C17EDC52FCC" )]
        public IHttpActionResult Login( [FromBody] LoginParameters loginParameters, Guid? personalDeviceGuid = null )
        {
            var site = MobileHelper.GetCurrentApplicationSite();

            if ( site == null )
            {
                return StatusCode( System.Net.HttpStatusCode.Unauthorized );
            }

            //
            // Use the existing AuthController.IsLoginValid method for actual authorization check. Throws exception if not authorized.
            //
            if ( !AuthController.IsLoginValid( loginParameters, out var errorMessage, out var userName ) )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( System.Net.HttpStatusCode.Unauthorized, errorMessage );
                throw new HttpResponseException( errorResponse );
            }

            //
            // Find the user and translate to a mobile person.
            //
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                var userLoginService = new UserLoginService( rockContext );
                var userLogin = userLoginService.GetByUserName( loginParameters.Username );

                if ( personalDeviceGuid.HasValue )
                {
                    var personalDevice = new PersonalDeviceService( rockContext ).Get( personalDeviceGuid.Value );

                    if ( personalDevice != null && personalDevice.PersonAliasId != userLogin.Person.PrimaryAliasId )
                    {
                        personalDevice.PersonAliasId = userLogin.Person.PrimaryAliasId;
                    }
                }

                userLogin.LastLoginDateTime = RockDateTime.Now;

                rockContext.SaveChanges();

                var mobilePerson = MobileHelper.GetMobilePerson( userLogin.Person, site );
                mobilePerson.AuthToken = MobileHelper.GetAuthenticationToken( loginParameters.Username );

                return Ok( mobilePerson );
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks if the interaction is for a communication and if so do
        /// additional steps to mark the communication as opened..
        /// </summary>
        /// <param name="session">The interaction session.</param>
        /// <param name="interaction">The interaction data.</param>
        /// <param name="rockContext">The rock context.</param>
        private void ProcessCommunicationInteraction( MobileInteractionSession session, MobileInteraction interaction, Rock.Data.RockContext rockContext )
        {
            // The interaction must be for the communication channel.
            if ( interaction.ChannelGuid != _communicationInteractionChannelGuid && interaction.ChannelId != CommunicationInteractionChannelId )
            {
                return;
            }

            // We need the communication recipient identifier and the communication identifier.
            if ( !interaction.EntityId.HasValue || !interaction.ComponentEntityId.HasValue )
            {
                return;
            }

            // Only process "Opened" operations for now.
            if ( !interaction.Operation.Equals( "OPENED", StringComparison.OrdinalIgnoreCase ) )
            {
                return;
            }

            // Because this is a mostly open API, don't trust just the
            // recipient identifier. Do a query that makes sure both the
            // communication identifier and the recipient identifier match
            // for a bit of extra security.
            var communicationRecipient = new CommunicationRecipientService( rockContext ).Queryable()
                .Where( a => a.Id == interaction.EntityId && a.CommunicationId == interaction.ComponentEntityId )
                .FirstOrDefault();

            if ( communicationRecipient == null )
            {
                return;
            }

            communicationRecipient.Status = CommunicationRecipientStatus.Opened;
            communicationRecipient.OpenedDateTime = RockDateTime.ConvertLocalDateTimeToRockDateTime( interaction.DateTime.LocalDateTime );
            communicationRecipient.OpenedClient = string.Format(
                "{0} {1} ({2})",
                session.OperatingSystem ?? "unknown",
                session.Application ?? "unknown",
                session.ClientType ?? "unknown" );
        }

        #endregion
    }
}