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
    public abstract class SuggestionComponent : Component
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
        /// Initializes a new instance of the <see cref="SuggestionComponent" /> class.
        /// </summary>
        public SuggestionComponent()
        {
            // Override default constructor of Component that loads attributes (not needed for suggestion components, needs to be done by each following suggestion)
        }

        /// <summary>
        /// Loads the attributes.
        /// </summary>
        /// <exception cref="System.Exception">Suggestion Component attributes are saved specific to the following suggestion, which requires that the current following suggestion is included in order to load or retrieve values. Use the LoadAttributes( FollowingSuggestion followingSuggestion ) method instead.</exception>
        [Obsolete( "Use LoadAttributes( FollowingSuggestion followingSuggestion ) instead", true )]
        public void LoadAttributes()
        {
            // Compiler should generate error if referencing this method, so exception should never be thrown
            // but method is needed to "override" the extension method for IHasAttributes objects
            throw new Exception( "Suggestion Component attributes are saved specific to the following suggestion, which requires that the current following suggestion is included in order to load or retrieve values. Use the LoadAttributes( FollowingSuggestion followingSuggestion ) method instead." );
        }

        /// <summary>
        /// Loads the attributes for the following suggestion.
        /// </summary>
        /// <param name="followingSuggestion">The following suggestion.</param>
        public void LoadAttributes( FollowingSuggestionType followingSuggestion )
        {
            followingSuggestion.LoadAttributes();
        }

        /// <summary>
        /// Use GetAttributeValue( FollowingSuggestion followingSuggestion, string key) instead.  suggestion component attribute values are 
        /// specific to the following suggestion instance (rather than global).  This method will throw an exception
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Suggestion Component attributes are saved specific to the following suggestion, which requires that the current following suggestion is included in order to load or retrieve values. Use the GetAttributeValue( FollowingSuggestion followingSuggestion, string key ) method instead.</exception>
        public override string GetAttributeValue( string key )
        {
            throw new Exception( "Suggestion Component attributes are saved specific to the following suggestion, which requires that the current following suggestion is included in order to load or retrieve values. Use the GetAttributeValue( FollowingSuggestion followingSuggestion, string key ) method instead." );
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
        /// Gets the attribute value for the suggestion 
        /// </summary>
        /// <param name="followingSuggestion">The following suggestion.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected string GetAttributeValue( FollowingSuggestionType followingSuggestion, string key )
        {
            if ( followingSuggestion.AttributeValues == null )
            {
                followingSuggestion.LoadAttributes();
            }

            var values = followingSuggestion.AttributeValues;
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
        /// <param name="followingSuggestionType">Type of the following suggestion.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual string FormatEntityNotification( FollowingSuggestionType followingSuggestionType, IEntity entity )
        {
            if ( followingSuggestionType != null )
            {
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Entity", entity );
                return followingSuggestionType.EntityNotificationFormatLava.ResolveMergeFields( mergeFields );
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
        /// Gets the suggestions.
        /// </summary>
        /// <param name="followingSuggestionType">Type of the following suggestion.</param>
        /// <param name="FollowerPersonIds">The follower person ids.</param>
        /// <returns></returns>
        public abstract List<PersonEntitySuggestion> GetSuggestions( FollowingSuggestionType followingSuggestionType, List<int> FollowerPersonIds );

    }

    public class PersonEntitySuggestion
    {
        public int PersonId { get; set; }
        public int EntityId { get; set; }
        public PersonEntitySuggestion( int personId, int entityId )
        {
            PersonId = personId;
            EntityId = entityId;
        }
    }
}
