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
using Rock.Data;
using Rock.Model;

namespace Rock.Follow
{
    /// <summary>
    /// Additional Merge Fields that are used to change Following Events.
    /// </summary>
    public interface IEventComponentAdditionalMergeFields
    {
        /// <summary>
        /// Formats the entity notification.
        /// </summary>
        /// <param name="followingEvent">The following event.</param>
        /// <param name="entity">The entity being followed.</param>
        /// <param name="additionalMergeFields">The collection of additional merge fields and values for the following event.</param>
        /// <returns></returns>
        string FormatEntityNotification( FollowingEventType followingEvent, IEntity entity, Dictionary<string, List<object>> additionalMergeFields );

        /// <summary>
        /// Determines whether <paramref name="followingEvent"/> has happened for <paramref name="entity"/> since <paramref name="lastNotified"/> date.
        /// </summary>
        /// <param name="followingEvent">The following event.</param>
        /// <param name="entity">The entity being followed.</param>
        /// <param name="lastNotified">The last notified date.</param>
        /// <param name="followedEventObjects">The collection of the followed event's objects.</param>
        /// 
        /// <returns></returns>
        bool HasEventHappened( FollowingEventType followingEvent, IEntity entity, DateTime? lastNotified, out Dictionary<string, List<object>> followedEventObjects );
    }
}
