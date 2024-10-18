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

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cached version of GroupTypeRole
    /// </summary>
    [Serializable]
    [DataContract]
    public class GroupTypeRoleCache : ModelCache<GroupTypeRoleCache, GroupTypeRole>
    {
        #region Properties

        /// <inheritdoc cref="GroupTypeRole.IsSystem"/>
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <inheritdoc cref="GroupTypeRole.GroupTypeId"/>
        [DataMember]
        public int? GroupTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <inheritdoc cref="GroupTypeRole.Description"/>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; private set; }

        /// <summary>
        /// Gets or sets the maximum count.
        /// </summary>
        /// <value>
        /// The maximum count.
        /// </value>
        [DataMember]
        public int? MaxCount { get; private set; }

        /// <summary>
        /// Gets or sets the minimum count.
        /// </summary>
        /// <value>
        /// The minimum count.
        /// </value>
        [DataMember]
        public int? MinCount { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is leader.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is leader; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsLeader { get; private set; }

        /// <inheritdoc cref="GroupTypeRole.ReceiveRequirementsNotifications"/>
        public bool ReceiveRequirementsNotifications { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can view.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can view; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CanView { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can edit.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can edit; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CanEdit { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can manage members.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can manage members; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CanManageMembers { get; private set; }

        /// <inheritdoc cref="GroupTypeRole.IsExcludedFromPeerNetwork"/>
        [DataMember]
        public bool IsExcludedFromPeerNetwork { get; private set; }

        /// <inheritdoc cref="GroupTypeRole.IsCheckInAllowed"/>
        [DataMember]
        public bool IsCheckInAllowed { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="GroupTypeRoleCache"/> class.
        /// </summary>
        public GroupTypeRoleCache()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupTypeRoleCache"/> class.
        /// </summary>
        /// <param name="role">The role.</param>
        [Obsolete( "Use the GroupTypeRoleCache.Get() methods instead.")]
        [RockObsolete( "1.16.7" )]
        public GroupTypeRoleCache( GroupTypeRole role )
        {
            if ( role == null )
            {
                return;
            }

            SetFromEntity( role );
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( !( entity is GroupTypeRole role ) )
            {
                return;
            }

            IsSystem = role.IsSystem;
            GroupTypeId = role.GroupTypeId;
            Name = role.Name;
            Description = role.Description;
            Order = role.Order;
            MaxCount = role.MaxCount;
            MinCount = role.MinCount;
            IsLeader = role.IsLeader;
            ReceiveRequirementsNotifications = role.ReceiveRequirementsNotifications;
            CanView = role.CanView;
            CanEdit = role.CanEdit;
            CanManageMembers = role.CanManageMembers;
            IsExcludedFromPeerNetwork = role.IsExcludedFromPeerNetwork;
            IsCheckInAllowed = role.IsCheckInAllowed;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
