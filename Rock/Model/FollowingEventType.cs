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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Attribute;
using Rock.Data;
using Rock.Follow;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a following event
    /// </summary>
    [Table( "FollowingEventType" )]
    [DataContract]
    public partial class FollowingEventType : Model<FollowingEventType>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the (internal) Name of the FollowingEvent. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the (internal) name of the FollowingEvent.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user defined description of the FollowingEvent.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the user defined description of the FollowingEvent.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the event MEF component identifier.
        /// </summary>
        /// <value>
        /// The event entity type identifier.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the followed entity type identifier.
        /// </summary>
        /// <value>
        /// The followed entity type identifier.
        /// </value>
        [DataMember]
        public int? FollowedEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }
        private bool _isActive = true;

        /// <summary>
        /// Gets or sets a value indicating whether [send on weekends].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [send on weekends]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool SendOnWeekends { get; set; }

        /// <summary>
        /// Gets or sets the last check.
        /// </summary>
        /// <value>
        /// The last check.
        /// </value>
        [DataMember]
        public DateTime? LastCheckDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this event is required. If not, followers will be able to optionally select if they want to be notified of this event
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is notice required; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsNoticeRequired { get; set; }

        /// <summary>
        /// Gets or sets how an entity should be formatted when included in the event notification to follower.
        /// </summary>
        /// <value>
        /// The item notification lava.
        /// </value>
        [DataMember]
        public string EntityNotificationFormatLava { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the event entity.
        /// </summary>
        /// <value>
        /// The type of the event entity.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the type of the followed entity.
        /// </summary>
        /// <value>
        /// The type of the followed entity.
        /// </value>
        [DataMember]
        public virtual EntityType FollowedEntityType { get; set; }
        
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FollowingEventType"/> class.
        /// </summary>
        public FollowingEventType()
        {
            IsActive = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the event component.
        /// </summary>
        /// <returns></returns>
        public virtual EventComponent GetEventComponent()
        {
            if ( EntityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Read( EntityTypeId.Value );
                if ( entityType != null )
                {
                    return EventContainer.GetComponent( entityType.Name );
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this FollowingEvent.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this FollowingEvent.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// FollowingEvent Configuration class.
    /// </summary>
    public partial class FollowingEventConfiguration : EntityTypeConfiguration<FollowingEventType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FollowingEventConfiguration"/> class.
        /// </summary>
        public FollowingEventConfiguration()
        {
            this.HasOptional( g => g.EntityType).WithMany().HasForeignKey( a => a.EntityTypeId).WillCascadeOnDelete( false );
            this.HasOptional( g => g.FollowedEntityType ).WithMany().HasForeignKey( a => a.FollowedEntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}