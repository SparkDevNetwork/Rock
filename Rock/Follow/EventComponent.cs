// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using Rock.Extension;
using Rock.Model;

namespace Rock.Follow
{
    /// <summary>
    /// Base class for following provider components
    /// </summary>
    public abstract class EventComponent : Component
    {

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get
            {
                var defaults = new Dictionary<string, string>();
                defaults.Add( "Active", "True" );
                defaults.Add( "Order", "0" );
                return defaults;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventComponent" /> class.
        /// </summary>
        public EventComponent()
        {
            // Override default constructor of Component that loads attributes (not needed for event components, needs to be done by each following event)
        }

        /// <summary>
        /// Loads the attributes.
        /// </summary>
        /// <exception cref="System.Exception">Event Component attributes are saved specific to the following event, which requires that the current following event is included in order to load or retrieve values. Use the LoadAttributes( FollowingEvent followingEvent ) method instead.</exception>
        [Obsolete( "Use LoadAttributes( FollowingEvent followingEvent ) instead", true )]
        public void LoadAttributes()
        {
            // Compiler should generate error if referencing this method, so exception should never be thrown
            // but method is needed to "override" the extension method for IHasAttributes objects
            throw new Exception( "Event Component attributes are saved specific to the following event, which requires that the current following event is included in order to load or retrieve values. Use the LoadAttributes( FollowingEvent followingEvent ) method instead." );
        }

        /// <summary>
        /// Loads the attributes for the following event.
        /// </summary>
        /// <param name="followingEvent">The following event.</param>
        public void LoadAttributes( FollowingEventType followingEvent )
        {
            followingEvent.LoadAttributes();
        }

        /// <summary>
        /// Use GetAttributeValue( FollowingEvent followingEvent, string key) instead.  event component attribute values are 
        /// specific to the following event instance (rather than global).  This method will throw an exception
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Event Component attributes are saved specific to the following event, which requires that the current following event is included in order to load or retrieve values. Use the GetAttributeValue( FollowingEvent followingEvent, string key ) method instead.</exception>
        public override string GetAttributeValue( string key )
        {
            throw new Exception( "Event Component attributes are saved specific to the following event, which requires that the current following event is included in order to load or retrieve values. Use the GetAttributeValue( FollowingEvent followingEvent, string key ) method instead." );
        }

        /// <summary>
        /// Always returns 0.  
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public override int Order
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Always returns true. 
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public override bool IsActive
        {
            get
            {
                return true; ;
            }
        }

        /// <summary>
        /// Gets the attribute value for the event 
        /// </summary>
        /// <param name="followingEvent">The following event.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected string GetAttributeValue( FollowingEventType followingEvent, string key )
        {
            if ( followingEvent.AttributeValues == null )
            {
                followingEvent.LoadAttributes();
            }

            var values = followingEvent.AttributeValues;
            if ( values != null && values.ContainsKey( key ) )
            {
                var keyValues = values[key];
                if ( keyValues != null )
                {
                    return keyValues.Value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Formats the entity notification.
        /// </summary>
        /// <param name="followingEvent">The following event.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual string FormatEntityNotification( FollowingEventType followingEvent, IEntity entity )
        {
            if ( followingEvent != null )
            {
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Entity", entity );
                return followingEvent.EntityNotificationFormatLava.ResolveMergeFields( mergeFields );
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the followed entity type identifier.
        /// </summary>
        /// <value>
        /// The followed entity type identifier.
        /// </value>
        public abstract Type FollowedType { get; }

        /// <summary>
        /// Determines whether [has event happened] [the specified entity].
        /// </summary>
        /// <param name="followingEvent">The following event.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public abstract bool HasEventHappened( FollowingEventType followingEvent, IEntity entity );

    }
}
