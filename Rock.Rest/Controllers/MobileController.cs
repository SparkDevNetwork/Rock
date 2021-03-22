﻿// <copyright>
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
using System.Data.Entity;
using System.Linq;
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
    public class MobileController : ApiControllerBase
    {
        /// <summary>
        /// Gets the launch packet.
        /// </summary>
        /// <param name="deviceIdentifier">The unique device identifier for this device.</param>
        /// <returns>An action result.</returns>
        [Route( "api/mobile/GetLaunchPacket" )]
        [HttpGet]
        [Authenticate]
        public IHttpActionResult GetLaunchPacket( string deviceIdentifier = null )
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

            var launchPacket = new LaunchPacket
            {
                LatestVersionId = additionalSettings.LastDeploymentVersionId ?? ( int ) ( additionalSettings.LastDeploymentDate.Value.ToJavascriptMilliseconds() / 1000 ),
                IsSiteAdministrator = site.IsAuthorized( Authorization.EDIT, person )
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
                var principal = ControllerContext.Request.GetUserPrincipal();

                launchPacket.CurrentPerson = MobileHelper.GetMobilePerson( person, site );
                launchPacket.CurrentPerson.AuthToken = MobileHelper.GetAuthenticationToken( principal.Identity.Name );
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
                        Name = deviceData.Name
                    };

                    personalDeviceService.Add( personalDevice );
                    rockContext.SaveChanges();
                }
                else if ( !personalDevice.IsActive || personalDevice.Name != deviceData.Name )
                {
                    personalDevice.IsActive = true;
                    personalDevice.Manufacturer = deviceData.Manufacturer;
                    personalDevice.Model = deviceData.Model;
                    personalDevice.Name = deviceData.Name;

                    rockContext.SaveChanges();
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
        /// <returns></returns>
        [Route( "api/mobile/UpdateDeviceRegistrationByGuid/{personalDeviceGuid}" )]
        [HttpPut]
        public IHttpActionResult UpdateDeviceRegistrationByGuid( Guid personalDeviceGuid, string registration )
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
        [Route( "api/mobile/Interactions" )]
        [HttpPost]
        [Authenticate]
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
                    return $"{mi.AppId}:{mi.PageGuid}:{mi.ChannelId}:{mi.ComponentId}:{mi.ComponentName}";
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
                    else if ( mobileInteraction.ChannelId.HasValue )
                    {
                        var interactionChannelId = mobileInteraction.ChannelId;

                        if ( mobileInteraction.ComponentId.HasValue )
                        {
                            interactionComponentLookup.AddOrReplace( GetComponentCacheKey( mobileInteraction ), mobileInteraction.ComponentId.Value );
                        }
                        else if ( mobileInteraction.ComponentName.IsNotNullOrWhiteSpace() )
                        {
                            //
                            // Get an existing or create a new component.
                            //
                            var interactionComponent = interactionComponentService.GetComponentByComponentName( interactionChannelId.Value, mobileInteraction.ComponentName );
                            rockContext.SaveChanges();

                            interactionComponentLookup.AddOrReplace( GetComponentCacheKey( mobileInteraction ), interactionComponent.Id );
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
                                mobileInteraction.DateTime,
                                mobileSession.Application,
                                mobileSession.OperatingSystem,
                                mobileSession.ClientType,
                                null,
                                ipAddress,
                                mobileSession.Guid );

                            interaction.Guid = mobileInteraction.Guid;
                            interaction.PersonalDeviceId = personalDeviceId;
                            interactionService.Add( interaction );
                            rockContext.SaveChanges();
                        }
                    }
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
        [Route( "api/mobile/Login" )]
        [HttpPost]
        public IHttpActionResult Login( [FromBody] LoginParameters loginParameters, Guid? personalDeviceGuid = null )
        {
            var authController = new AuthController();
            var site = MobileHelper.GetCurrentApplicationSite();

            if ( site == null )
            {
                return StatusCode( System.Net.HttpStatusCode.Unauthorized );
            }

            //
            // Chain to the existing login method for actual authorization check.
            // Throws exception if not authorized.
            //
            authController.Login( loginParameters );

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
                        rockContext.SaveChanges();
                    }
                }

                var mobilePerson = MobileHelper.GetMobilePerson( userLogin.Person, site );
                mobilePerson.AuthToken = MobileHelper.GetAuthenticationToken( loginParameters.Username );

                return Ok( mobilePerson );
            }
        }
    }
}
