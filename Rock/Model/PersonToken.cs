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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Web;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "PersonToken" )]
    [DataContract]
    public partial class PersonToken : Entity<PersonToken>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the unique token. Use PersonToken
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [MaxLength( 32 )]
        [HideFromReporting]
        [Index( IsUnique = true )]
        public string Token { get; private set; }

        // NOTE: Intentionally do not include [DataMember] on Token so that is isn't accessible to REST api or Lava

        /// <summary>
        /// Gets or sets the expire date time.
        /// </summary>
        /// <value>
        /// The expire date time.
        /// </value>
        [DataMember]
        public DateTime? ExpireDateTime { get; set; }

        /// <summary>
        /// Gets or sets the times used.
        /// </summary>
        /// <value>
        /// The times used.
        /// </value>
        [DataMember]
        public int TimesUsed { get; set; }

        /// <summary>
        /// Gets or sets the usage limit.
        /// </summary>
        /// <value>
        /// The usage limit.
        /// </value>
        [DataMember]
        public int? UsageLimit { get; set; }

        /// <summary>
        /// Gets or sets the last used date time.
        /// </summary>
        /// <value>
        /// The last used date time.
        /// </value>
        [DataMember]
        public DateTime? LastUsedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the page identifier.
        /// </summary>
        /// <value>
        /// The page identifier.
        /// </value>
        [DataMember]
        public int? PageId { get; set; }

        #endregion Entity Properties

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the page.
        /// </summary>
        /// <value>
        /// The page.
        /// </value>
        public virtual Page Page { get; set; }

        #endregion Virtual Properties

        #region Public Methods

        /// <summary>
        /// Creates and stores a new PersonToken for a person using the default ExpireDateTime and UsageLimit.
        /// Returns the encrypted URLEncoded Token which can be used as a rckipid.
        /// </summary>
        /// <param name="personAlias">The person alias.</param>
        /// <returns></returns>
        public static string CreateNew( PersonAlias personAlias )
        {
            return CreateNew( personAlias, null, null, null );
        }

        /// <summary>
        /// Creates and stores a new PersonToken for a person limited to a specific PageId
        /// Returns the encrypted URLEncoded Token which can be used as a rckipid
        /// </summary>
        /// <param name="personAlias">The person alias.</param>
        /// <param name="pageId">The page identifier.</param>
        /// <returns></returns>
        public static string CreateNew( PersonAlias personAlias, int pageId )
        {
            return CreateNew( personAlias, null, null, pageId );
        }

        /// <summary>
        /// Creates and stores a new PersonToken for a person using the specified ExpireDateTime, UsageLimit, and Page
        /// Returns the encrypted URLEncoded Token which can be used as a rckipid
        /// </summary>
        /// <param name="personAlias">The person alias.</param>
        /// <param name="expireDateTime">The expire date time.</param>
        /// <param name="usageLimit">The usage limit.</param>
        /// <param name="pageId">The page identifier.</param>
        /// <returns></returns>
        public static string CreateNew( PersonAlias personAlias, DateTime? expireDateTime, int? usageLimit, int? pageId )
        {
            using ( var rockContext = new RockContext() )
            {
                var token = Rock.Security.Encryption.GenerateUniqueToken();

                PersonToken personToken = new PersonToken();
                personToken.PersonAliasId = personAlias.Id;
                personToken.Token = token;
                if ( expireDateTime != null )
                {
                    personToken.ExpireDateTime = expireDateTime;
                }
                else
                {
                    int? tokenExpireMinutes = GlobalAttributesCache.Get().GetValue( "core.PersonTokenExpireMinutes" ).AsIntegerOrNull();
                    if ( tokenExpireMinutes.HasValue )
                    {
                        personToken.ExpireDateTime = RockDateTime.Now.AddMinutes( tokenExpireMinutes.Value );
                    }
                    else
                    {
                        personToken.ExpireDateTime = null;
                    }
                }

                personToken.TimesUsed = 0;
                personToken.UsageLimit = usageLimit ?? GlobalAttributesCache.Get().GetValue( "core.PersonTokenUsageLimit" ).AsIntegerOrNull();

                personToken.PageId = pageId;

                var personTokenService = new PersonTokenService( rockContext );
                personTokenService.Add( personToken );
                rockContext.SaveChanges( true );

                var encryptedToken = Rock.Security.Encryption.EncryptString( token );

                // do a Replace('%', '!') after we UrlEncode it (to make it more safely embeddable in HTML and cross browser compatible)
                return System.Web.HttpUtility.UrlEncode( encryptedToken ).Replace( '%', '!' );
            }
        }

        /// <summary>
        /// The rckipid reg ex
        /// </summary>
        private static Regex rckipidRegEx = new Regex( @"rckipid=([^&]*)", RegexOptions.Compiled );

        /// <summary>
        /// Obfuscates any instances of a rckipid parameter within the specified url so that doesn't get displayed or stored 
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static string ObfuscateRockMagicToken( string url )
        {
            // obfuscate rock magic token
            return ObfuscateRockMagicToken( url, null );
        }

        /// <summary>
        /// Obfuscates the rock magic token.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public static string ObfuscateRockMagicToken( string url, System.Web.UI.Page page )
        {
            if ( string.IsNullOrWhiteSpace( url ) )
            {
                return url;
            }

            var match = rckipidRegEx.Match( url );
            if ( match.Success )
            {
                return rckipidRegEx.Replace( url, "rckipid=XXXXXXXXXXXXXXXXXXXXXXXXXXXX" );
            }

            var routeData = page?.RouteData;
            if ( routeData == null )
            {
                Uri uri;

                // if this is a valid full url, lookup the route so we can obfuscate any {rckipid} keys in it 
                if ( Uri.TryCreate( url, UriKind.Absolute, out uri ) )
                {
                    routeData = Rock.Web.UI.RouteUtils.GetRouteDataByUri( uri, HttpContext.Current?.Request?.ApplicationPath );
                }
            }

            if ( routeData != null && routeData.Values.ContainsKey( "rckipid" ) )
            {
                return url.Replace( ( string ) routeData.Values["rckipid"], "XXXXXXXXXXXXXXXXXXXXXXXXXXXX" );
            }

            return url;
        }

        /// <summary>
        /// Removes any instances of a rckipid parameter within the specified url so that isn't included.
        /// This is also handy if you are redirecting to the login page and don't want to include the magic token.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static string RemoveRockMagicToken( string url )
        {
            // remove rock magic token
            return rckipidRegEx.Replace( url, "" );
        }

        #endregion Public Methods
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class PersonTokenConfiguration : EntityTypeConfiguration<PersonToken>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonTokenConfiguration"/> class.
        /// </summary>
        public PersonTokenConfiguration()
        {
            this.HasRequired( r => r.PersonAlias ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( true );
            this.HasOptional( r => r.Page ).WithMany().HasForeignKey( r => r.PageId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}
