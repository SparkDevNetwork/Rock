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
using System.IO;
using System.Linq;
using System.ServiceModel.Channels;
using System.Web.Http;

using Rock.Mobile;
using Rock.Mobile.Common;
using Rock.Mobile.Common.Enums;
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
        /// <returns></returns>
        [Route( "api/mobile/GetLaunchPacket" )]
        [HttpGet]
        [Authenticate]
        public object GetLaunchPacket()
        {
            var baseUrl = GlobalAttributesCache.Value( "PublicApplicationRoot" );
            var site = MobileHelper.GetCurrentApplicationSite();
            var additionalSettings = site?.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();
            var person = GetPerson();
            var deviceData = Request.GetHeader( "X-Rock-DeviceData" ).FromJsonOrNull<DeviceData>();

            if ( additionalSettings == null || !additionalSettings.LastDeploymentDate.HasValue )
            {
                return NotFound();
            }

            var launchPacket = new LaunchPackage
            {
                LatestVersionId = ( int ) ( additionalSettings.LastDeploymentDate.Value.ToJavascriptMilliseconds() / 1000 ),
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

            return launchPacket;
        }

        /// <summary>
        /// Posts the interactions that have been queued up by the mobile client.
        /// </summary>
        /// <param name="sessions">The sessions.</param>
        /// <returns></returns>
        [Route( "api/mobile/Interactions" )]
        [HttpPost]
        [Authenticate]
        public IHttpActionResult PostInteractions( [FromBody] List<MobileInteractionSession> sessions )
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
                            int? interactionComponentId = null;

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
                                var interactionChannelId = interactionChannelService.Queryable()
                                    .Where( a =>
                                        a.ChannelTypeMediumValueId == channelMediumTypeValue.Id &&
                                        a.ChannelEntityId == site.Id )
                                    .Select( a => ( int? ) a.Id )
                                    .FirstOrDefault();

                                //
                                // If not found, create one.
                                //
                                if ( !interactionChannelId.HasValue )
                                {
                                    var interactionChannel = new InteractionChannel
                                    {
                                        Name = site.Name,
                                        ChannelTypeMediumValueId = channelMediumTypeValue.Id,
                                        ChannelEntityId = site.Id,
                                        ComponentEntityTypeId = pageEntityTypeId
                                    };

                                    interactionChannelService.Add( interactionChannel );
                                    rockContext.SaveChanges();

                                    interactionChannelId = interactionChannel.Id;
                                }

                                //
                                // Get an existing or create a new component.
                                //
                                var interactionComponent = interactionComponentService.GetComponentByEntityId( interactionChannelId.Value, page.Id, page.InternalName);
                                rockContext.SaveChanges();

                                interactionComponentId = interactionComponent.Id;
                            }
                            else if ( mobileInteraction.ChannelId.HasValue )
                            {
                                var interactionChannelId = mobileInteraction.ChannelId;

                                if ( mobileInteraction.ComponentId.HasValue )
                                {
                                    interactionComponentId = mobileInteraction.ComponentId.Value;
                                }
                                else if ( mobileInteraction.ComponentName.IsNotNullOrWhiteSpace() )
                                {
                                    //
                                    // Get an existing or create a new component.
                                    //
                                    var interactionComponent = interactionComponentService.GetComponentByComponentName( interactionChannelId.Value, mobileInteraction.ComponentName );
                                    rockContext.SaveChanges();

                                    interactionComponentId = interactionComponent.Id;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                continue;
                            }

                            //
                            // Add the interaction
                            //
                            if ( interactionComponentId.HasValue )
                            {
                                var interaction = interactionService.CreateInteraction( interactionComponentId.Value,
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
                                interactionService.Add( interaction );
                                rockContext.SaveChanges();
                            }
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
        /// <returns>A MobilePerson object if the login was successful.</returns>
        [Route( "api/mobile/Login" )]
        [HttpPost]
        public IHttpActionResult Login( [FromBody] LoginParameters loginParameters )
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
            var userLoginService = new UserLoginService( new Rock.Data.RockContext() );
            var userLogin = userLoginService.GetByUserName( loginParameters.Username );
            var mobilePerson = MobileHelper.GetMobilePerson( userLogin.Person, site );

            mobilePerson.AuthToken = MobileHelper.GetAuthenticationToken( loginParameters.Username );

            return Ok( mobilePerson );
        }
    }
}
