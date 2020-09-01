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

using System;
using System.IdentityModel.Tokens;
using Microsoft.Owin;
using Owin;
using Rock.Oidc.Authorization;
using Rock.Oidc.Configuration;
using Rock.Web.Cache;

namespace Rock.Oidc
{
    public static class Startup
    {
        /// <summary>
        /// Method that will be run at Rock Owin startup
        /// </summary>
        /// <param name="app"></param>
        public static void OnStartup( IAppBuilder app )
        {
            /*
	            8/21/2020 - MSB
	            In the future we could modify this to get the settings from a system configuration option to
                allow the clients to change them, but for how they are hard coded.
		
                Reason: Future Development
            */

            var rockOidcSettings = RockOidcSettings.GetDefaultSettings();
            app.UseOAuthValidation();

            app.UseOpenIdConnectServer( options =>
            {
                options.Provider = new AuthorizationProvider();

                options.Issuer = new Uri( GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ).EnsureTrailingForwardslash() );

                /*
	                8/21/2020 - MSB
	                In the future we could modify this to get the settings from a system configuration option to
                    allow the clients to change them, but for how they are hard coded.
		
                    Reason: Future Development
                */
                options.AuthorizationEndpointPath = new PathString( Paths.AuthorizePath );
                options.LogoutEndpointPath = new PathString( Paths.LogoutPath );
                options.TokenEndpointPath = new PathString( Paths.TokenPath );
                options.UserinfoEndpointPath = new PathString( Paths.UserInfo );

                options.AccessTokenLifetime = TimeSpan.FromSeconds( rockOidcSettings.AccessTokenLifetime );
                options.IdentityTokenLifetime = TimeSpan.FromSeconds( rockOidcSettings.IdentityTokenLifetime );
                options.RefreshTokenLifetime = TimeSpan.FromSeconds( rockOidcSettings.RefreshTokenLifetime );

                options.ApplicationCanDisplayErrors = System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment;
                options.AllowInsecureHttp = System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment;

                var rockSigningCredentials = new RockOidcSigningCredentials( rockOidcSettings );

                foreach ( var key in rockSigningCredentials.SigningKeys )
                {
                    options.SigningCredentials.AddKey( new RsaSecurityKey( key ) );
                }
            } );
        }
    }
}