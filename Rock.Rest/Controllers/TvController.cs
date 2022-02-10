using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel.Channels;
using System.Text;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using Rock;
using Rock.Common.Tv;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Tv;
using Rock.Web.Cache;
using Rock.VersionInfo;
using Rock.Tv.Classes;

namespace Rock.Rest.Controllers
{

    /// <summary>
    /// Provides API interfaces for TV applications to use when communicating with Rock.
    /// </summary>
    /// <seealso cref="Rock.Rest.ApiControllerBase" />
    public class TvController : ApiControllerBase
    {
        // Used for creating random strings
        private static Random random = new Random();

        #region Constants
        private const int _codeReusePeriodInMinutes = 120;
        #endregion

        /// <summary>
        /// Get's the launch packet for the application
        /// </summary>
        /// <seealso cref="Rock.Rest.ApiControllerBase" />
        [HttpGet]
        [System.Web.Http.Route( "api/v2/tv/apple/GetLaunchPacket" )]
        public IHttpActionResult GetLaunchPacket()
        {
            // Read site Id from the request header
            var siteId = this.Request.GetHeader( "X-Rock-App-Id" ).AsIntegerOrNull();

            // Get device data from the request header
            // Get device data
            var deviceData = JsonConvert.DeserializeObject<DeviceData>( this.Request.GetHeader( "X-Rock-DeviceData" ) );

            if ( deviceData.IsNull() )
            {
                StatusCode( HttpStatusCode.InternalServerError );
            }

            if ( !siteId.HasValue )
            {
                return NotFound();
            }

            var site = SiteCache.Get( siteId.Value );

            // If the site was not found then return 404
            if ( site.IsNull() )
            {
                return NotFound();
            }

            // Return the launch packet
            try
            {
                var rockContext = new RockContext();
                var person = GetPerson( rockContext );

                var launchPacket = new AppleLaunchPacket();
                launchPacket.EnablePageViews = site.EnablePageViews;

                if ( person != null )
                {
                    var principal = ControllerContext.Request.GetUserPrincipal();

                    launchPacket.CurrentPerson = TvHelper.GetTvPerson( person );
                    launchPacket.CurrentPerson.AuthToken = TvHelper.GetAuthenticationTokenFromUsername( principal.Identity.Name );

                    UserLoginService.UpdateLastLogin( principal.Identity.Name );
                }

                // Get or create the personal device.
                var tvDeviceTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSONAL_DEVICE_TYPE_TV ).Id;
                var personalDeviceService = new PersonalDeviceService( rockContext );
                var personalDevice = personalDeviceService.Queryable()
                    .Where( a => a.DeviceUniqueIdentifier == deviceData.DeviceIdentifier && a.PersonalDeviceTypeValueId == tvDeviceTypeValueId && a.SiteId == site.Id )
                    .FirstOrDefault();

                if ( personalDevice == null )
                {
                    personalDevice = new PersonalDevice
                    {
                        DeviceUniqueIdentifier = deviceData.DeviceIdentifier,
                        PersonalDeviceTypeValueId = tvDeviceTypeValueId,
                        SiteId = site.Id,
                        PersonAliasId = person?.PrimaryAliasId,
                        NotificationsEnabled = true,
                        Manufacturer = deviceData.Manufacturer,
                        Model = deviceData.Model,
                        Name = deviceData.Name,
                        IsActive = true
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

                    // Update the person tied to the device, but never blank it out. 
                    if ( person.IsNotNull() && personalDevice.PersonAliasId != person.PrimaryAliasId )
                    {
                        personalDevice.PersonAliasId = person.PrimaryAliasId;
                    }

                    rockContext.SaveChanges();
                }

                launchPacket.PersonalDeviceGuid = personalDevice.Guid;
                launchPacket.HomepageGuid = site.DefaultPage.Guid;

                launchPacket.RockVersion = VersionInfo.VersionInfo.GetRockProductVersionNumber();

                return Ok( launchPacket );
            }
            catch ( Exception )
            {
                // Ooops...
                return StatusCode( HttpStatusCode.InternalServerError );
            }
        }

        /// <summary>
        /// Gets the application JavaScript for the Apple TV App. Note this needs the application id in the
        /// querystring as this is not being called by the default HttpClient in the shell;
        /// </summary>
        /// <param name="applicationId">The application is (site id).</param>
        /// <returns></returns>
        [HttpGet]
        [System.Web.Http.Route( "api/v2/tv/apple/GetApplicationJavaScript/{applicationId}" )]
        public HttpResponseMessage GetApplicationScript( int applicationId )
        {
            var response = new HttpResponseMessage();

            // Get the site and validate the the request is valid (api key is valid)
            var site = SiteCache.Get( applicationId );

            // If the site was not found then return 404
            if ( site.IsNull() )
            {
                response.StatusCode = System.Net.HttpStatusCode.NotFound;
                return response;
            }

            // Return the script content
            try
            {
                var applicationSettings = JsonConvert.DeserializeObject<AppleTvApplicationSettings>( site.AdditionalSettings );
                response.Content = new StringContent( applicationSettings.ApplicationScript, Encoding.UTF8, "application/javascript" );
            }
            catch ( Exception )
            {
                // Ooops...
                response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                return response;
            }

            return response;
        }

        /// <summary>
        /// Gets the TVML for the provided page.
        /// </summary>
        /// <param name="pageGuid">The page unique identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [System.Web.Http.Route( "api/v2/tv/apple/GetTvmlForPage/{pageGuid}" )]
        public HttpResponseMessage GetTvmlForPage( Guid pageGuid )
        {
            var response = new HttpResponseMessage();

            var currentPerson = GetPerson();

            var page = PageCache.Get( pageGuid );

            // If page is null return 404
            if ( page.IsNull() )
            {
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            // Check security
            if ( !page.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
            {
                response.StatusCode = HttpStatusCode.Unauthorized;
                return response;
            }

            // Get requested cache control from client (client trumps server. We'll use this to set the reponse header to help inform any CDNs
            var cacheRequest = this.Request.GetHeader( "X-Rock-Tv-RequestedCacheControl" );
            if ( cacheRequest.IsNotNullOrWhiteSpace() )
            {
                var cacheParts = cacheRequest.Split( ':' );

                switch ( cacheParts[0] )
                {
                    case "public":
                        {
                            var maxAgeInSeconds = cacheParts[1].IsNotNull() ? cacheParts[1].AsInteger() : 777;
                            response.Headers.CacheControl = new CacheControlHeaderValue()
                            {
                                Public = true,
                                MaxAge = new TimeSpan( 0, 0, maxAgeInSeconds )
                            };
                            break;
                        }
                    case "private":
                        {
                            response.Headers.CacheControl = new CacheControlHeaderValue()
                            {
                                Private = true
                            };
                            break;
                        }
                }
            }
            else
            {
                // Use cache from database
                if ( page.CacheControlHeaderSettings.IsNotNullOrWhiteSpace() )
                {
                    switch ( page.CacheControlHeader.RockCacheablityType )
                    {
                        case Rock.Utility.RockCacheablityType.Public:
                            {
                                response.Headers.CacheControl = new CacheControlHeaderValue()
                                {
                                    Public = true,
                                    MaxAge = new TimeSpan( 0, 0, page.CacheControlHeader.MaxAge.ToSeconds() ),
                                    SharedMaxAge = new TimeSpan( 0, 0, page.CacheControlHeader.SharedMaxAge.ToSeconds() )
                                };
                                break;
                            }
                        case Rock.Utility.RockCacheablityType.Private:
                            {
                                response.Headers.CacheControl = new CacheControlHeaderValue()
                                {
                                    Private = true
                                };
                                break;
                            }
                    }
                }
            }

            // Get content and return it
            try
            {
                var site = SiteCache.Get( page.SiteId );
                var applicationSettings = JsonConvert.DeserializeObject<AppleTvApplicationSettings>( site.AdditionalSettings );

                var mergeFields = RockRequestContext.GetCommonMergeFields();
                mergeFields.Add( "SiteStyles", applicationSettings.ApplicationStyles );
                mergeFields.Add( "CurrentPage", page );
                mergeFields.Add( "CurrentPersonCanEdit", page.IsAuthorized( Rock.Security.Authorization.EDIT, currentPerson ) );
                mergeFields.Add( "CurrentPersonCanAdministrate", page.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, currentPerson ) );
                //mergeFields.Add( "PageParameter", this.Request.GetQueryNameValuePairs() );
                mergeFields.Add( "TvShellVersion", RockRequestContext.GetHeader( "X-Rock-TvShellVersion" ) );
                mergeFields.Add( "TvAppTheme", RockRequestContext.GetHeader( "X-Rock-AppTheme" ) );

                // Get device data
                var deviceData = this.Request.GetHeader( "X-Rock-DeviceData" );

                if ( deviceData.IsNotNullOrWhiteSpace() )
                {
                    mergeFields.Add( "DeviceData", JsonConvert.DeserializeObject<DeviceData>( deviceData ) );
                }

                // Get the page response content from the AdditionalSettings property
                var pageResponse = JsonConvert.DeserializeObject<ApplePageResponse>( page.AdditionalSettings );

                // Run Lava across the content
                pageResponse.Content = pageResponse.Content.ResolveMergeFields( mergeFields, currentPerson, "All" );

                response.Content = new StringContent( pageResponse.ToJson(), System.Text.Encoding.UTF8, "application/json" );
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                // Ooops...
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }
        }

        /// <summary>
        /// Posts the interactions.
        /// </summary>
        /// <param name="sessions">The sessions.</param>
        /// <param name="personalDeviceGuid">The personal device unique identifier.</param>
        /// <returns></returns>
        [System.Web.Http.Route( "api/v2/tv/SaveInteractions/{personalDeviceGuid}" )]
        [HttpPost]
        [Authenticate]
        public IHttpActionResult PostInteractions( [FromBody] List<TvInteractionSession> sessions, Guid? personalDeviceGuid = null )
        {
            var person = GetPerson();
            var ipAddress = System.Web.HttpContext.Current?.Request?.UserHostAddress;

            using ( var rockContext = new RockContext() )
            {
                var interactionChannelService = new InteractionChannelService( rockContext );
                var interactionComponentService = new InteractionComponentService( rockContext );
                var interactionSessionService = new InteractionSessionService( rockContext );
                var interactionService = new InteractionService( rockContext );
                var userLoginService = new UserLoginService( rockContext );
                var channelMediumTypeValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE );
                var pageEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Page ) ).Id;

                // Check to see if we have a site and the API key is valid.
                if ( TvHelper.GetCurrentApplicationSite() == null )
                {
                    return StatusCode( System.Net.HttpStatusCode.Forbidden );
                }

                // Get the personal device identifier if they provided it's unique identifier.
                int? personalDeviceId = null;
                if ( personalDeviceGuid.HasValue )
                {
                    personalDeviceId = new PersonalDeviceService( rockContext ).GetId( personalDeviceGuid.Value );
                }

                // Create a quick way to cache data since we have to loop twice.
                var interactionComponentLookup = new Dictionary<string, int>();

                //
                // Helper method to get a cache key for looking up the component Id.
                //
                string GetComponentCacheKey( TvInteraction mi )
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
                    foreach ( var tvSession in sessions )
                    {
                        var interactionGuids = tvSession.Interactions.Select( i => i.Guid ).ToList();
                        var existingInteractionGuids = interactionService.Queryable()
                            .Where( i => interactionGuids.Contains( i.Guid ) )
                            .Select( i => i.Guid )
                            .ToList();

                        //
                        // Loop through all interactions that don't already exist and add each one.
                        //
                        foreach ( var tvInteraction in tvSession.Interactions.Where( i => !existingInteractionGuids.Contains( i.Guid ) ) )
                        {
                            string cacheKey = GetComponentCacheKey( tvInteraction );

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
                                tvInteraction.EntityId,
                                tvInteraction.Operation,
                                tvInteraction.Summary,
                                tvInteraction.Data,
                                person?.PrimaryAliasId,
                                RockDateTime.ConvertLocalDateTimeToRockDateTime( tvInteraction.DateTime.LocalDateTime ),
                                tvSession.Application,
                                tvSession.OperatingSystem,
                                tvSession.ClientType,
                                null,
                                ipAddress,
                                tvSession.Guid );

                            interaction.Guid = tvInteraction.Guid;
                            interaction.PersonalDeviceId = personalDeviceId;
                            interaction.RelatedEntityTypeId = tvInteraction.RelatedEntityTypeId;
                            interaction.RelatedEntityId = tvInteraction.RelatedEntityId;
                            interaction.ChannelCustom1 = tvInteraction.ChannelCustom1;
                            interaction.ChannelCustom2 = tvInteraction.ChannelCustom2;
                            interaction.ChannelCustomIndexed1 = tvInteraction.ChannelCustomIndexed1;
                            interaction.InteractionTimeToServe = tvInteraction.InteractionTimeToServe;

                            interactionService.Add( interaction );
                        }
                    }

                    rockContext.SaveChanges();
                } );
            }

            return Ok();
        }

        /// <summary>
        /// Starts the authenication session.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [System.Web.Http.Route( "api/v2/tv/StartAuthenticationSession/{siteId}" )]
        public HttpResponseMessage StartAuthenicationSession( int siteId )
        {
            var response = new HttpResponseMessage();
            var authCode = string.Empty;

            var deviceData = JsonConvert.DeserializeObject<DeviceData>( this.Request.GetHeader( "X-Rock-DeviceData" ) );
            var site = SiteCache.Get( siteId );

            var authGenerationCount = 0;
            var maxAuthGenerationAttempts = 50;

            var rockContext = new RockContext();
            var remoteAuthenticationSessionService = new RemoteAuthenticationSessionService( rockContext );

            // Get client IP
            var clientIp = GetClientIp( Request );

            var expireDateTime = RockDateTime.Now.AddMinutes( _codeReusePeriodInMinutes );

            // Get random code
            while ( authCode == string.Empty && authGenerationCount < maxAuthGenerationAttempts )
            {
                authCode = RandomString( 6 );

                // Remove characters that could be confusing for clarity
                authCode.Replace( '0', '2' );
                authCode.Replace( 'O', 'P' );

                // Check that the code is not already being used in the last 5 days
                var codeReuseWindow = RockDateTime.Now.AddDays( -5 );
                var usedCodes = remoteAuthenticationSessionService.Queryable()
                                    .Where( s => s.Code == authCode
                                        && s.SessionStartDateTime > expireDateTime )
                                    .Any();

                // If matching code was found try again
                if ( usedCodes )
                {
                    authCode = string.Empty;
                }

                // Remove bad sequences
                foreach ( var badSequence in AttendanceCodeService.NoGood )
                {
                    if ( authCode.Contains( badSequence ) )
                    {
                        authCode = string.Empty;
                    }
                }

                authGenerationCount++;
            }

            // Check that we were able to generate an auth code
            if ( authGenerationCount >= maxAuthGenerationAttempts )
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }

            // Create authenication session
            var remoteSession = new RemoteAuthenticationSession();
            remoteAuthenticationSessionService.Add( remoteSession );

            remoteSession.SiteId = siteId;
            remoteSession.ClientIpAddress = clientIp;
            remoteSession.Code = authCode;
            remoteSession.SessionStartDateTime = ( DateTime ) RockDateTime.Now;
            remoteSession.DeviceUniqueIdentifier = deviceData.DeviceIdentifier;

            rockContext.SaveChanges();

            var authReturn = new AuthCodeResponse();
            authReturn.Code = authCode;
            authReturn.ExpireDateTime = expireDateTime;

            var loginPageReference = site.LoginPageReference;
            loginPageReference.Parameters.AddOrReplace( "AuthCode", authCode );

            var qrData = HttpUtility.UrlEncode( $"{GlobalAttributesCache.Value( "PublicApplicationRoot" ).TrimEnd( '/' )}{loginPageReference.BuildUrl()}" );
            authReturn.AuthenticationUrlQrCode = $"{GlobalAttributesCache.Value( "PublicApplicationRoot" )}GetQRCode.ashx?data={qrData}&outputType=png";

            response.Content = new StringContent( authReturn.ToJson(), System.Text.Encoding.UTF8, "application/json" );

            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        /// <summary>
        /// Checks the authenication session.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        [HttpGet]
        [System.Web.Http.Route( "api/v2/tv/CheckAuthenticationSession/{siteId}/{code}" )]
        public HttpResponseMessage CheckAuthenicationSession( int siteId, string code )
        {
            var response = new HttpResponseMessage();
            var authCheckResponse = new AuthCodeCheckResponse();

            var deviceData = JsonConvert.DeserializeObject<DeviceData>( this.Request.GetHeader( "X-Rock-DeviceData" ) );

            var rockContext = new RockContext();
            var remoteAuthenticationSessionService = new RemoteAuthenticationSessionService( rockContext );

            // Get client Ip address
            var clientIp = GetClientIp( Request );

            var expireDateTime = RockDateTime.Now.AddMinutes( _codeReusePeriodInMinutes );

            // Check for validated account
            var validatedSession = remoteAuthenticationSessionService.Queryable()
                                    .Include( s => s.AuthorizedPersonAlias.Person )
                                    .Where( s => s.Code == code
                                        && s.SiteId == siteId
                                        && s.SessionStartDateTime < expireDateTime
                                        && s.AuthorizedPersonAliasId.HasValue
                                        && s.DeviceUniqueIdentifier == deviceData.DeviceIdentifier
                                        && s.ClientIpAddress == clientIp )
                                    .OrderByDescending( s => s.SessionStartDateTime )
                                    .FirstOrDefault();

            if ( validatedSession != null )
            {
                // Mark the auth session as ended
                validatedSession.SessionEndDateTime = RockDateTime.Now;
                rockContext.SaveChanges();

                authCheckResponse.CurrentPerson = TvHelper.GetTvPerson( validatedSession.AuthorizedPersonAlias.Person );
                authCheckResponse.IsAuthenciated = true;

                // Link personal device
                var tvDeviceTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSONAL_DEVICE_TYPE_TV ).Id;
                var personalDeviceService = new PersonalDeviceService( rockContext );
                var personalDevice = personalDeviceService.Queryable()
                    .Where( a => a.DeviceUniqueIdentifier == deviceData.DeviceIdentifier && a.PersonalDeviceTypeValueId == tvDeviceTypeValueId && a.SiteId == siteId )
                    .FirstOrDefault();

                if ( personalDevice != null )
                {
                    personalDevice.PersonAliasId = validatedSession.AuthorizedPersonAliasId;
                }
            }
            else
            {
                authCheckResponse.IsAuthenciated = false;
            }



            rockContext.SaveChanges();

            // Return
            response.Content = new StringContent( authCheckResponse.ToJson(), System.Text.Encoding.UTF8, "application/json" );
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        #region Private Methods

        /// <summary>
        /// Randoms the string.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string RandomString( int length )
        {
            // Removed vowels to prevent bad words and the letter nine to prevent other immature references
            const string chars = "BCDFGHJKLMNPQRSTVWXYZ012345678";
            return new string( Enumerable.Repeat( chars, length )
                .Select( s => s[random.Next( s.Length )] ).ToArray() );
        }

        /// <summary>
        /// Gets the client ip.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        private string GetClientIp( HttpRequestMessage request )
        {
            if ( request.Properties.ContainsKey( "MS_HttpContext" ) )
            {
                return ( ( HttpContextWrapper ) request.Properties["MS_HttpContext"] ).Request.UserHostAddress;
            }
            else if ( request.Properties.ContainsKey( RemoteEndpointMessageProperty.Name ) )
            {
                RemoteEndpointMessageProperty prop;
                prop = ( RemoteEndpointMessageProperty ) this.Request.Properties[RemoteEndpointMessageProperty.Name];
                return prop.Address;
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
