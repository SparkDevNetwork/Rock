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
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;

using Ical.Net.CalendarComponents;

using Rock.Cms;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cached representation of a <see cref="PageShortLink"/> object.
    /// </summary>
    [Serializable]
    [DataContract]
    public class PageShortLinkCache : ModelCache<PageShortLinkCache, PageShortLink>, IHasReadOnlyAdditionalSettings
    {
        #region Fields

        private UtmSettings _utmSettings;

        private List<PageShortLinkScheduleCache> _linkSchedules;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public override TimeSpan? Lifespan => IsScheduled ? base.Lifespan : new TimeSpan( 0, 10, 0 );

        /// <inheritdoc cref="PageShortLink.SiteId"/>
        [DataMember]
        public int SiteId { get; private set; }

        /// <inheritdoc cref="PageShortLink.Token"/>
        [DataMember]
        public string Token { get; private set; }

        /// <inheritdoc cref="PageShortLink.Url"/>
        [DataMember]
        public string Url { get; private set; }

        /// <inheritdoc/>
        [DataMember]
        public string AdditionalSettingsJson { get; private set; }

        /// <inheritdoc cref="PageShortLink.IsScheduled"/>
        [DataMember]
        public bool IsScheduled { get; private set; }

        /// <inheritdoc cref="PageShortLink.CategoryId"/>
        [DataMember]
        public int? CategoryId { get; private set; }

        /// <summary>
        /// The <see cref="SiteCache"/> that this instance references.
        /// </summary>
        public SiteCache Site => SiteCache.Get( SiteId );

        /// <inheritdoc cref="PageShortLink.Category"/>
        public CategoryCache Category => CategoryId.HasValue ? CategoryCache.Get( CategoryId.Value ) : null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( !( entity is PageShortLink link ) )
            {
                return;
            }

            SiteId = link.SiteId;
            Token = link.Token;
            Url = link.Url;
            AdditionalSettingsJson = link.AdditionalSettingsJson;
            IsScheduled = link.IsScheduled;
            CategoryId = link.CategoryId;

            _utmSettings = this.GetAdditionalSettings<UtmSettings>();
            _linkSchedules = this.GetAdditionalSettings<PageShortLinkScheduleData>()
                .Schedules
                ?.Select( ls => new PageShortLinkScheduleCache( ls ) )
                .ToList()
                ?? new List<PageShortLinkScheduleCache>();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Token;
        }

        /// <summary>
        /// Gets the currently active URL without modification.
        /// </summary>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <returns>A string that represents the currently active URL.</returns>
        public string GetCurrentUrl( RockContext rockContext = null )
        {
            return GetCurrentUrlData( rockContext ).Url;
        }

        /// <summary>
        /// Gets the currently active URL and appends any UTM values to
        /// the query string.
        /// </summary>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <returns>A string that represents the current URL and UTM values.</returns>
        public string GetCurrentUrlWithUtm( RockContext rockContext = null )
        {
            return GetCurrentUrlData( rockContext ).UrlWithUtm;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the currently active URL and appends any UTM values to
        /// the query string along with the active purpose key, if any.
        /// </summary>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <returns>A tuple that contains the Url and the purpose key.</returns>
        internal (string Url, string UrlWithUtm, string PurposeKey) GetCurrentUrlData( RockContext rockContext = null )
        {
            foreach ( var linkSchedule in _linkSchedules )
            {
                if ( IsLinkScheduleActive( linkSchedule, rockContext ) )
                {
                    return (linkSchedule.Url, GetUrlWithUtm( linkSchedule.Url, linkSchedule.UtmSettings ), linkSchedule.PurposeKey);
                }
            }

            return (Url, GetUrlWithUtm( Url, _utmSettings ), string.Empty);
        }

        /// <summary>
        /// Gets the URL with the UTM values applied to it.
        /// </summary>
        /// <param name="url">The original URL to be updated.</param>
        /// <param name="utmSettings">The UTM settings to apply.</param>
        /// <returns>A new string that represents the URL with UTM data.</returns>
        internal static string GetUrlWithUtm( string url, UtmSettings utmSettings )
        {
            if ( url.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            if ( utmSettings == null )
            {
                return url;
            }

            ParseUrl( url.RemoveCrLf().Trim(),
                out var protocolDomainAndPath,
                out var queryParameters,
                out var fragment );

            // Add UTM Parameters as lowercase values with URL Encoding.
            var hasUtmValues = false;

            hasUtmValues |= AddUtmValueToQueryString( queryParameters, "utm_source", SystemGuid.DefinedType.UTM_SOURCE.AsGuid(), utmSettings.UtmSourceValueId );
            hasUtmValues |= AddUtmValueToQueryString( queryParameters, "utm_medium", SystemGuid.DefinedType.UTM_MEDIUM.AsGuid(), utmSettings.UtmMediumValueId );
            hasUtmValues |= AddUtmValueToQueryString( queryParameters, "utm_campaign", SystemGuid.DefinedType.UTM_CAMPAIGN.AsGuid(), utmSettings.UtmCampaignValueId );

            if ( utmSettings.UtmTerm.IsNotNullOrWhiteSpace() )
            {
                queryParameters.Add( "utm_term", utmSettings.UtmTerm.Trim().ToLower() );
                hasUtmValues = true;
            }

            if ( utmSettings.UtmContent.IsNotNullOrWhiteSpace() )
            {
                queryParameters.Add( "utm_content", utmSettings.UtmContent.Trim().ToLower() );
                hasUtmValues = true;
            }

            // If no UTM values, return the base URL.
            if ( !hasUtmValues )
            {
                return url;
            }

            // Construct a new URL that includes the UTM query string parameters.
            // The query parameters are stored in a HttpValueCollection, so the ToString() implementation returns a URL-encoded query string.
            var urlWithUtm = protocolDomainAndPath + "?" + queryParameters.ToQueryStringEscaped();

            if ( fragment.IsNotNullOrWhiteSpace() )
            {
                urlWithUtm += fragment;
            }

            return urlWithUtm;
        }

        /// <summary>
        /// Extract a set of query parameters from a Url.
        /// If the input Url is not well-formed, this function attempts to parse for a query string
        /// delimited by the first "?" character.
        /// </summary>
        /// <param name="url">The input Url.</param>
        /// <param name="protocolDomainAndPath"></param>
        /// <param name="queryParameters">The query parameters as a HttpValueCollection:NameValueCollection of Name/Value pairs.</param>
        /// <param name="fragment">The fragment portion of the Url, including the leading '#' character if not empty.</param>
        /// <returns></returns>
        private static void ParseUrl( string url, out string protocolDomainAndPath, out NameValueCollection queryParameters, out string fragment )
        {
            if ( url.IsNullOrWhiteSpace() )
            {
                protocolDomainAndPath = string.Empty;
                queryParameters = new NameValueCollection();
                fragment = string.Empty;
                return;
            }

            // Create a builder for the Uri and attempt to parse the existing Url.
            UriBuilder utmUri = null;
            try
            {
                utmUri = new UriBuilder( url );
            }
            catch
            {
                // The Url is not well-formed.
            }

            if ( utmUri != null )
            {
                fragment = utmUri.Fragment;
                // This is supported in .NET Core.
                queryParameters = System.Web.HttpUtility.ParseQueryString( utmUri.Query );

                utmUri.Query = string.Empty;
                utmUri.Fragment = string.Empty;

                protocolDomainAndPath = utmUri.Uri.ToString();
            }
            else
            {
                // The Url is not well-formed, so parse the query string and fragment segments, and leave the remainder unchanged.
                // The parsing process assumes that although the Url has some incorrect parts, it is otherwise properly encoded.

                // Remove the fragment portion of the Url if it exists.
                var fragmentStartIndex = url.IndexOf( "#" );

                if ( fragmentStartIndex >= 0 )
                {
                    fragment = url.Substring( fragmentStartIndex );
                    url = url.Substring( 0, fragmentStartIndex );
                }
                else
                {
                    fragment = string.Empty;
                }

                // Parse the query string and base path.
                string queryString;
                var queryStartIndex = url.IndexOf( "?" ) + 1;

                if ( queryStartIndex > 0 )
                {
                    protocolDomainAndPath = url.Substring( 0, queryStartIndex - 1 );
                    queryString = url.Substring( queryStartIndex );
                }
                else
                {
                    protocolDomainAndPath = url;
                    queryString = string.Empty;
                }

                // This is supported in .NET Core.
                queryParameters = System.Web.HttpUtility.ParseQueryString( queryString );
            }
        }

        /// <summary>
        /// Adds a UTM value from a <see cref="DefinedValue"/> into the query string.
        /// </summary>
        /// <param name="queryString">The query string to be updated.</param>
        /// <param name="parameterName">The name of the parameter to add.</param>
        /// <param name="definedTypeGuid">The unique identifier of the <see cref="DefinedType"/> to get the value from.</param>
        /// <param name="utmDefinedValueId">The identifier of the <see cref="DefinedValue"/> in <paramref name="definedTypeGuid"/> to get the value from.</param>
        /// <returns><c>true</c> if the query string was modified; otherwise <c>false</c>.</returns>
        private static bool AddUtmValueToQueryString( NameValueCollection queryString, string parameterName, Guid definedTypeGuid, int? utmDefinedValueId )
        {
            if ( !utmDefinedValueId.HasValue )
            {
                return false;
            }

            var utmValue = DefinedTypeCache.Get( definedTypeGuid )
                ?.DefinedValues.FirstOrDefault( v => v.Id == utmDefinedValueId )
                ?.Value
                .Trim()
                .ToLower();

            if ( !utmValue.IsNullOrWhiteSpace() )
            {
                queryString.Set( parameterName, utmValue );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if the link schedule is currently active or not.
        /// </summary>
        /// <param name="linkSchedule">The schedule to check to see if it is currently active.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <returns><c>true</c> if the link schedule is active; otherwise <c>false</c>.</returns>
        private static bool IsLinkScheduleActive( PageShortLinkScheduleCache linkSchedule, RockContext rockContext )
        {
            if ( linkSchedule == null )
            {
                return false;
            }

            if ( linkSchedule.ScheduleId.HasValue )
            {
                var schedule = NamedScheduleCache.Get( linkSchedule.ScheduleId.Value, rockContext );

                return schedule?.WasScheduleActive( RockDateTime.Now ) ?? false;
            }
            else
            {
                if ( linkSchedule.CustomCalendar == null )
                {
                    return false;
                }

                return Schedule.WasScheduleActive( RockDateTime.Now, linkSchedule.CustomCalendar, null, linkSchedule.CustomCalendarContent );
            }
        }

        #endregion

        #region Static Methods

        #endregion

        #region Support Classes

        /// <summary>
        /// A cached representation of <see cref="PageShortLinkSchedule"/>. The
        /// primary purpose of this is to provide a cached version of the
        /// <see cref="CalendarEvent"/> object as creating that is the most
        /// expensive operation in the process of checking if the schedule is
        /// active or not.
        /// </summary>
        private class PageShortLinkScheduleCache : PageShortLinkSchedule
        {
            private readonly Lazy<CalendarEvent> _customCalendar;

            public CalendarEvent CustomCalendar => _customCalendar.Value;

            public PageShortLinkScheduleCache( PageShortLinkSchedule linkSchedule )
            {
                CustomCalendarContent = linkSchedule.CustomCalendarContent;
                PurposeKey = linkSchedule.PurposeKey;
                ScheduleId = linkSchedule.ScheduleId;
                Url = linkSchedule.Url;
                UtmSettings = linkSchedule.UtmSettings;

                _customCalendar = new Lazy<CalendarEvent>( () =>
                {
                    if ( ScheduleId.HasValue )
                    {
                        return null;
                    }

                    return InetCalendarHelper.CreateCalendarEvent( CustomCalendarContent );
                } );
            }
        }

        #endregion
    }
}
