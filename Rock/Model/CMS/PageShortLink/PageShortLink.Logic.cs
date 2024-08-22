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
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Text;

using Rock.Cms;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class PageShortLink : ICacheable
    {
        private static readonly Random _random = new Random( Guid.NewGuid().GetHashCode() );
        private static readonly char[] alphaCharacters = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        /// <summary>
        /// Gets a random token.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string GetRandomToken( int length )
        {
            StringBuilder sb = new StringBuilder();
            int poolSize = alphaCharacters.Length;
            for ( int i = 0; i < length; i++ )
            {
                sb.Append( alphaCharacters[_random.Next( poolSize )] );
            }

            return sb.ToString();
        }

        #region Virtual Properties

        /// <summary>
        /// Gets the URL, including query parameters for UTM values.
        /// </summary>
        /// <value>
        /// The URL, including query string parameters for any specified UTM fields.
        /// </value>
        /// <remarks>
        /// The Url produced by this is not guaranteed to be well-formed.
        /// </remarks>
        public virtual string UrlWithUtm
        {
            get
            {
                if ( Url.IsNullOrWhiteSpace() )
                {
                    return string.Empty;
                }

                var utmSettings = this.GetAdditionalSettings<UtmSettings>();

                return PageShortLinkCache.GetUrlWithUtm( Url, utmSettings );
            }
        }

        #endregion Virtual Properties

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return PageShortLinkCache.Get( Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            PageShortLinkCache.FlushItem( this.Id );
        }

        #endregion

        /// <summary>
        /// Determines if this instance has any scheduled URL data.
        /// </summary>
        /// <returns><c>true</c> if this instance has schedule data; otherwise <c>false</c>.</returns>
        private bool HasSchedules()
        {
            var data = GetScheduleData();

            return data.Schedules != null && data.Schedules.Count > 0;
        }

        /// <summary>
        /// Gets the schedule data for this instance. If no schedule data has
        /// been configured then an empty instance of <see cref="PageShortLinkScheduleData"/>
        /// will be returned.
        /// </summary>
        /// <returns>An instance of <see cref="PageShortLinkScheduleData"/> that describes all the schedule details for this instance.</returns>
        internal PageShortLinkScheduleData GetScheduleData()
        {
            return this.GetAdditionalSettings<PageShortLinkScheduleData>();
        }

        /// <summary>
        /// Updates this instance to use the specified schedule data. If <c>null</c>
        /// is passed then the instance will be returned to non-schedule mode. In
        /// that case you must also set the <see cref="Url"/> property to a valid
        /// value.
        /// </summary>
        /// <param name="data">The schedule data or <c>null</c> to return the instance to non-scheduled.</param>
        internal void SetScheduleData( PageShortLinkScheduleData data )
        {
            this.SetAdditionalSettings( data );
        }
    }
}
