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
using System.Runtime.Serialization;

namespace Rock.Web.Cache
{
    /// <summary>
    /// This class is used to rate limit calls or block to help prevent them from being spammed.
    /// </summary>
    /// <seealso cref="Rock.Web.Cache.ItemCache{T}" />
    [DataContract]
    public class RateLimiterCache : ItemCache<RateLimiterCache>, IHasLifespan
    {
        /// <summary>
        /// Gets or sets the actions per period.
        /// </summary>
        /// <value>
        /// The actions per period.
        /// </value>
        [DataMember]
        private int ActionsPerPeriod { get; set; }

        /// <summary>
        /// Gets or sets the maximum actions per period.
        /// </summary>
        /// <value>
        /// The maximum actions per period.
        /// </value>
        [DataMember]
        private int MaxActionsPerPeriod { get; set; }

        /// <summary>
        /// Gets or sets the date last action performed.
        /// </summary>
        /// <value>
        /// The date last action performed.
        /// </value>
        [DataMember]
        private DateTime DateLastActionPerformed { get; set; }

        /// <summary>
        /// Gets or sets the minimum time between actions.
        /// </summary>
        /// <value>
        /// The minimum time between actions.
        /// </value>
        [DataMember]
        private TimeSpan MinTimeBetweenActions { get; set; }

        private TimeSpan? _lifespan = null;
        /// <summary>
        /// The amount of time that this item will live in the cache before expiring. If null, then the
        /// <see cref="P:Rock.Web.Cache.ItemCache`1.DefaultLifespan" /> is used.
        /// </summary>
        [DataMember]
        public override TimeSpan? Lifespan { get => _lifespan; }

        private RateLimiterCache( TimeSpan lifespan, int maxActionsInPeriod, TimeSpan? minTimeBetweenActions = null )
        {
            ActionsPerPeriod = 0;
            DateLastActionPerformed = DateTime.MinValue;
            _lifespan = lifespan;

            MaxActionsPerPeriod = maxActionsInPeriod;

            if ( minTimeBetweenActions == null )
            {
                MinTimeBetweenActions = new TimeSpan( 0 );
            }
            else
            {
                MinTimeBetweenActions = minTimeBetweenActions.Value;
            }
        }

        /// <summary>
        /// Determines whether the RateLimiter can process a page, or if it should be blocked due to exceeding the rate limit.
        /// </summary>
        /// <param name="pageId">The page identifier.</param>
        /// <param name="clientIpAddress">The client ip address.</param>
        /// <param name="period">The period.</param>
        /// <param name="maxActionsInPeriod">The maximum actions in period.</param>
        /// <param name="minTimeBetweenActions">The minimum time between actions.</param>
        /// <returns>
        ///   <c>true</c> if the RateLimiter can process the page; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanProcessPage( int pageId, string clientIpAddress, TimeSpan period, int maxActionsInPeriod, TimeSpan? minTimeBetweenActions = null )
        {
            var limiter = GetRateLimiter( $"page-${pageId}", clientIpAddress, period, maxActionsInPeriod, minTimeBetweenActions );

            if ( limiter.CanPerformAction() )
            {
                limiter.UpdateActionsPerPeriod();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Performs the action.
        /// </summary>
        private void UpdateActionsPerPeriod()
        {
            ActionsPerPeriod++;
            DateLastActionPerformed = DateTime.Now;
        }

        /// <summary>
        /// Determines whether the RateLimiter can perform an action, or whether it exceeds the maximum actions
        /// for a given time period or the minimum time between actions.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the RateLimiter can perform the action; otherwise, <c>false</c>.
        /// </returns>
        private bool CanPerformAction()
        {
            if ( ActionsPerPeriod >= MaxActionsPerPeriod )
            {
                return false;
            }

            if ( DateLastActionPerformed.Add( MinTimeBetweenActions ) > DateTime.Now )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the rate limiter.
        /// </summary>
        /// <param name="key">The key that identifies the object to be rate limited.</param>
        /// <param name="clientIpAddress">The client ip address.</param>
        /// <param name="period">The period.</param>
        /// <param name="maxActionsInPeriod">The maximum actions in period.</param>
        /// <param name="minTimeBetweenActions">The minimum time between actions.</param>
        /// <returns></returns>
        private static RateLimiterCache GetRateLimiter( string key, string clientIpAddress, TimeSpan period, int maxActionsInPeriod, TimeSpan? minTimeBetweenActions = null )
        {
            var cacheKey = GetClientRateLimiterCacheKey( key, clientIpAddress );
            return GetOrAddExisting( cacheKey, () => InitializeNewRateLimiterCache( period, maxActionsInPeriod, minTimeBetweenActions ) );
        }

        /// <summary>
        /// Initializes the new rate limiter cache.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="maxActionsInPeriod">The maximum actions in period.</param>
        /// <param name="minTimeBetweenActions">The minimum time between actions.</param>
        /// <returns></returns>
        private static RateLimiterCache InitializeNewRateLimiterCache( TimeSpan period, int maxActionsInPeriod, TimeSpan? minTimeBetweenActions = null )
        {
            return new RateLimiterCache( period, maxActionsInPeriod, minTimeBetweenActions );
        }

        /// <summary>
        /// Gets the rate limiter cache key.
        /// </summary>
        /// <param name="key">The key that identifies the object to be rate limited.</param>
        /// <param name="clientIpAddress">The client ip address.</param>
        /// <returns></returns>
        private static string GetClientRateLimiterCacheKey( string key, string clientIpAddress )
        {
            return $"{key}-{clientIpAddress}";
        }
    }
}
