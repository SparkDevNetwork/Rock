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
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

using Newtonsoft.Json;

using Rock.Attribute;
using Rock.Data;
using Rock.Net;
using Rock.Security;

namespace Rock.Model
{
    public partial class HistoryLogin
    {
        #region Properties

        /// <summary>
        /// The settings that should be used when serializing and deserializing related data JSON, silently handling
        /// errors and minimizing whitespace.
        /// </summary>
        private static JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                if ( _jsonSerializerSettings == null )
                {
                    _jsonSerializerSettings = JsonExtensions.GetSerializeSettings( indentOutput: false, ignoreErrors: true, camelCase: false );
                }

                return _jsonSerializerSettings;
            }
        }

        private static JsonSerializerSettings _jsonSerializerSettings;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets the deserialized <see cref="HistoryLoginRelatedData"/> from <see cref="RelatedDataJson"/> or
        /// <see langword="null"/> there is no related data.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "17.0" )]
        public HistoryLoginRelatedData GetRelatedDataOrNull()
        {
            if ( this.RelatedDataJson.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return JsonConvert.DeserializeObject<HistoryLoginRelatedData>( this.RelatedDataJson, JsonSerializerSettings );
        }

        /// <summary>
        /// Serializes the info within the provided <paramref name="relatedData"/> and sets the resulting JSON string on
        /// the <see cref="RelatedDataJson"/> property. If <see langword="null"/> is provided, any existing related data
        /// will be cleared.
        /// </summary>
        /// <param name="relatedData"></param>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "17.0" )]
        public void SetRelatedDataJson( HistoryLoginRelatedData relatedData )
        {
            if ( relatedData == null )
            {
                this.RelatedDataJson = null;
            }

            this.RelatedDataJson = JsonConvert.SerializeObject( relatedData, JsonSerializerSettings );
        }

        /// <summary>
        /// Saves the history login to the database in a background task, after a short delay.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Make sure to set all fields for data that you already have in hand (to avoid unneeded queries), but
        ///         take note that the following fields will be supplemented in the record before saving if possible,
        ///         when missing:
        ///     </para>
        ///     <list type="bullet">
        ///         <item><see cref="UserLoginId"/></item>
        ///         <item><see cref="PersonAliasId"/></item>
        ///         <item><see cref="ClientIpAddress"/></item>
        ///         <item><see cref="ExternalSource"/></item>
        ///         <item><see cref="DestinationUrl"/></item>
        ///     </list>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "17.0" )]
        public void SaveAfterDelay()
        {
            try
            {
                // Attempt to supplement the record with missing request info (client IP address and destination URL).
                if ( HttpContext.Current?.Request != null || RockRequestContextAccessor.Current != null )
                {
                    if ( this.ClientIpAddress.IsNullOrWhiteSpace() )
                    {
                        // Get the IP address from the NextGen request context.
                        var clientIPAddress = RockRequestContextAccessor.Current?.ClientInformation?.IpAddress;

                        // Fall back to the legacy HTTP context.
                        if ( clientIPAddress.IsNullOrWhiteSpace() )
                        {
                            try
                            {
                                clientIPAddress = Rock.Web.UI.RockPage.GetClientIpAddress();
                            }
                            catch
                            {
                                // Intentionally ignore.
                            }
                        }

                        this.ClientIpAddress = clientIPAddress;
                    }

                    if ( this.DestinationUrl.IsNullOrWhiteSpace() )
                    {
                        // Get the absolute URI from the NextGen request context.
                        var absoluteUri = RockRequestContextAccessor.Current?.RequestUri?.AbsoluteUri;

                        // Fall back to the legacy HTTP context.
                        if ( absoluteUri.IsNullOrWhiteSpace() )
                        {
                            absoluteUri = HttpContext.Current?.Request?.UrlProxySafe()?.AbsoluteUri;
                        }

                        var cleanUrl = PersonToken.ObfuscateRockMagicToken( absoluteUri );

                        /*
                            12/16/2024 - JPH

                            The old [UserLogin] code had the following lines, but they seem redundant with the above obfuscation.
                            If we decide we want to retain the `returnurl` value, this is the place to investigate further.

                            Reason: Retain feature parity when moving code.
                         */
                        // Obfuscate the URL specified in the returnurl, just in case it contains any sensitive information (like a rckipid).
                        Regex returnUrlRegex = new Regex( @"returnurl=([^&]*)" );
                        cleanUrl = returnUrlRegex.Replace( cleanUrl, "returnurl=XXXXXXXXXXXXXXXXXXXXXXXXXXXX" );

                        this.DestinationUrl = cleanUrl;
                    }
                }

                // Look up additional data and save the new record in a background task after waiting 1 second, in an
                // attempt to allow all related post save actions to complete. While this might not be necessary, this
                // is the way the user login service used to work (when saving a standard History record to represent
                // the login), so we're carrying this behavior forward.
                Task.Run( async () =>
                {
                    await Task.Delay( 1000 );

                    using ( var rockContext = new RockContext() )
                    {
                        // Attempt to supplement the record with the user login ID.
                        int? personId = null;
                        if ( this.UserName.IsNotNullOrWhiteSpace() && !this.UserLoginId.HasValue )
                        {
                            var userLoginInfo = new UserLoginService( rockContext )
                                .Queryable()
                                .Where( u => u.UserName == this.UserName )
                                .Select( u => new
                                {
                                    u.Id,
                                    u.PersonId
                                } )
                                .FirstOrDefault();

                            this.UserLoginId = userLoginInfo?.Id;
                            personId = userLoginInfo?.PersonId;
                        }

                        // Attempt to supplement the record with the person alias ID.
                        if ( this.UserLoginId.HasValue && !this.PersonAliasId.HasValue )
                        {
                            if ( !personId.HasValue )
                            {
                                personId = new UserLoginService( rockContext )
                                    .Queryable()
                                    .Where( u => u.Id == this.UserLoginId.Value )
                                    .Select( u => u.PersonId )
                                    .FirstOrDefault();
                            }

                            if ( personId.HasValue )
                            {
                                this.PersonAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( personId.Value );
                            }
                        }

                        // Attempt to supplement the record with the external source name.
                        if ( this.AuthClientClientId.IsNotNullOrWhiteSpace() && this.ExternalSource.IsNullOrWhiteSpace() )
                        {
                            this.ExternalSource = new AuthClientService( rockContext )
                                .Queryable()
                                .Where( a => a.ClientId == this.AuthClientClientId )
                                .Select( a => a.Name )
                                .FirstOrDefault();
                        }

                        var historyLoginService = new HistoryLoginService( rockContext );
                        historyLoginService.Add( this );

                        rockContext.SaveChanges();
                    }
                } );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
            }
        }

        #endregion Methods
    }
}
