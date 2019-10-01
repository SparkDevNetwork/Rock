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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Following Event Notification
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "FollowingEventNotification" )]
    [DataContract]
    public partial class FollowingEventNotification : Model<FollowingEventNotification>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the following event type identifier.
        /// </summary>
        /// <value>
        /// The following event type identifier.
        /// </value>
        [DataMember]
        public int FollowingEventTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        [DataMember]
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the last notified.
        /// </summary>
        /// <value>
        /// The last notified.
        /// </value>
        [DataMember]
        public DateTime LastNotified { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the following event.
        /// </summary>
        /// <value>
        /// The type of the following event.
        /// </value>
        [DataMember]
        public virtual FollowingEventType FollowingEventType { get; set; }

        #endregion

        #region Public Methods

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class FollowingEventNotificationConfiguration : EntityTypeConfiguration<FollowingEventNotification>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FollowingConfiguration"/> class.
        /// </summary>
        public FollowingEventNotificationConfiguration()
        {
            this.HasRequired( f => f.FollowingEventType ).WithMany().HasForeignKey( f => f.FollowingEventTypeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}
