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
using System.Net;

using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Personalization
{
    /// <summary>
    /// SegmentIdKeys for the current person/visitor session stored in the <seealso cref="Rock.Personalization.RequestCookieKey.ROCK_SEGMENT_FILTERS"/> cookie
    /// </summary>
    public class SegmentFilterCookieData
    {
        /// <summary>
        /// Gets or sets the IdKey of the Person Alias.
        /// </summary>
        /// <value>The person alias identifier.</value>
        public string PersonAliasIdKey { get; set; }

        /// <summary>
        /// Gets or sets the last time the data in this cookie was updated (in RockDateTime )
        /// </summary>
        /// <value>The last update date time</value>
        public DateTime LastUpdateDateTime { get; set; }

        /// <summary>
        /// Determines whether the <paramref name="otherPersonAliasId"/> is the same PersonAliasId that is embedded in <see cref="PersonAliasIdKey"/>.
        /// </summary>
        /// <param name="otherPersonAliasId">The other person alias identifier.</param>
        /// <returns><c>true</c> if [is same person alias] [the specified other person alias identifier]; otherwise, <c>false</c>.</returns>
        public bool IsSamePersonAlias( int otherPersonAliasId )
        {
            var thisPersonAliasId = IdHasher.Instance.GetId( PersonAliasIdKey ?? string.Empty );
            return thisPersonAliasId.HasValue && thisPersonAliasId.Value == otherPersonAliasId;
        }

        /// <summary>
        /// Determines whether the data in the cookie is stale.
        /// </summary>
        /// <param name="currentDateTime">The current date time.</param>
        /// <returns><c>true</c> if the data is stale; otherwise, <c>false</c>.</returns>
        public bool IsStale( DateTime currentDateTime )
        {
            var activeSegments = PersonalizationSegmentCache.GetActiveSegments();
            if ( !activeSegments.Any() )
            {
                return false;
            }

            var lastModifiedDateTime = activeSegments.Max( a => a.ModifiedDateTime );
            if ( LastUpdateDateTime < lastModifiedDateTime )
            {
                return true;
            }

            var personalizationCookieDuration = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.PERSONALIZATION_SEGMENT_COOKIE_AFFINITY_DURATION_MINUTES ).AsIntegerOrNull() ?? 5;
            TimeSpan maxCacheLifetime =  TimeSpan.FromMinutes( personalizationCookieDuration );
            if ( currentDateTime - LastUpdateDateTime > maxCacheLifetime )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Rock.Data.IEntity.IdKey" >IdKeys</see> of <see cref="Rock.Model.PersonalizationSegment">PersonalizationSegment</see> for
        /// the <see cref="Rock.Web.UI.RockPage.CurrentPerson" /> or <see cref="Rock.Web.UI.RockPage.CurrentVisitor" /> 
        /// </summary>
        /// <value>The segment identifier keys.</value>
        public string[] SegmentIdKeys { get; set; }

        /// <summary>
        /// Gets the segment ids converted from the stored <see cref="SegmentIdKeys"/>
        /// </summary>
        /// <returns>System.Int32[].</returns>
        public int[] GetSegmentIds()
        {
            return SegmentIdKeys.Select( s => IdHasher.Instance.GetId( s ) ).Where( a => a.HasValue ).Select( s => s.Value ).ToArray();
        }

        /// <summary>
        /// The purpose used to encrypt and authenticate the cookie value.
        /// </summary>
        private const string CookieEncryptionPurpose = "Rock.Personalization.SegmentFilterCookieData";

        /// <summary>
        /// Converts this data into the encrypted value stored in the
        /// <see cref="Rock.Personalization.RequestCookieKey.ROCK_SEGMENT_FILTERS"/> cookie.
        /// The value is authenticated so it can't be modified by the client,
        /// and URL encoded so it survives readers that URL decode cookie values.
        /// </summary>
        /// <returns>The encrypted and URL encoded cookie value.</returns>
        internal string ToCookieValue()
        {
            var encryptedValue = Rock.Security.Encryption.EncryptStringForPurpose( this.ToJson(), CookieEncryptionPurpose );

            return WebUtility.UrlEncode( encryptedValue );
        }

        /// <summary>
        /// Reads the data from a value created by <see cref="ToCookieValue"/>.
        /// A blank, malformed, modified or old plain JSON value returns <c>null</c>
        /// so the caller rebuilds the data from the database.
        /// </summary>
        /// <param name="cookieValue">The cookie value.</param>
        /// <param name="isUrlDecoded"><c>true</c> if the cookie value was already URL decoded when the request was read.</param>
        /// <returns>The <see cref="SegmentFilterCookieData"/>; otherwise <c>null</c>.</returns>
        internal static SegmentFilterCookieData FromCookieValue( string cookieValue, bool isUrlDecoded )
        {
            if ( cookieValue.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var encryptedValue = isUrlDecoded ? cookieValue : WebUtility.UrlDecode( cookieValue );

            return Rock.Security.Encryption.DecryptStringForPurpose( encryptedValue, CookieEncryptionPurpose )?.FromJsonOrNull<SegmentFilterCookieData>();
        }
    }
}
