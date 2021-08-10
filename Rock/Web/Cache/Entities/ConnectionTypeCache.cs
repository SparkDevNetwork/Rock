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
    /// Information about a connection type that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class ConnectionTypeCache : ModelCache<ConnectionTypeCache, ConnectionType>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        public string IconCssClass { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether future follow-ups are enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if future follow-ups are enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableFutureFollowup { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether full activity lists are enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if full activity lists are enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableFullActivityList { get; private set; }


        /// <summary>
        /// Gets or sets a value indicating whether this connection type requires a placement group to connect.
        /// </summary>
        /// <value>
        /// <c>true</c> if connection type requires a placement group to connect; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool RequiresPlacementGroupToConnect { get; private set; }

        /// <summary>
        /// Gets or sets the owner person alias identifier.
        /// </summary>
        /// <value>
        /// The owner person alias identifier.
        /// </value>
        [DataMember]
        public int? OwnerPersonAliasId { get; private set; }

        /// <summary>
        /// Gets or sets the number of days until the request is considered idle.
        /// </summary>
        /// <value>
        /// This determines how many days can pass before the request is considered idle.
        /// </value>
        [DataMember]
        public int DaysUntilRequestIdle { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable request security].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable request security]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableRequestSecurity { get; private set; }

        /// <summary>
        /// Gets or sets the connection request detail page identifier.
        /// </summary>
        /// <value>
        /// The connection request detail page identifier.
        /// </value>
        [DataMember]
        public int? ConnectionRequestDetailPageId { get; private set; }

        /// <summary>
        /// Gets or sets the connection request detail page route identifier.
        /// </summary>
        /// <value>
        /// The connection request detail page route identifier.
        /// </value>
        [DataMember]

        public int? ConnectionRequestDetailPageRouteId { get; private set; }

        /// <summary>
        /// Gets or sets the default view mode (list or board).
        /// </summary>
        /// <value>
        /// The default view.
        /// </value>
        [DataMember]
        public ConnectionTypeViewMode DefaultView { get; private set; }

        /// <summary>
        /// Gets or sets the request header lava.
        /// </summary>
        /// <value>
        /// The request header lava.
        /// </value>
        [DataMember]
        public string RequestHeaderLava { get; private set; }

        /// <summary>
        /// Gets or sets the request badge lava.
        /// </summary>
        /// <value>
        /// The request badge lava.
        /// </value>
        [DataMember]
        public string RequestBadgeLava { get; private set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var sourceModel = entity as ConnectionType;
            if ( sourceModel == null )
            {
                return;
            }

            Name = sourceModel.Name;
            Description = sourceModel.Description;
            IsActive = sourceModel.IsActive;
            IconCssClass = sourceModel.IconCssClass;
            EnableFullActivityList = sourceModel.EnableFullActivityList;
            EnableFutureFollowup = sourceModel.EnableFutureFollowup;
            RequiresPlacementGroupToConnect = sourceModel.RequiresPlacementGroupToConnect;
            OwnerPersonAliasId = sourceModel.OwnerPersonAliasId;
            DaysUntilRequestIdle = sourceModel.DaysUntilRequestIdle;
            EnableRequestSecurity = sourceModel.EnableRequestSecurity;
            ConnectionRequestDetailPageId = sourceModel.ConnectionRequestDetailPageId;
            ConnectionRequestDetailPageRouteId = sourceModel.ConnectionRequestDetailPageRouteId;
            DefaultView = sourceModel.DefaultView;
            RequestHeaderLava = sourceModel.RequestHeaderLava;
            RequestBadgeLava = sourceModel.RequestBadgeLava;
            Order = sourceModel.Order;
        }

        /// <summary>
        /// Converts to string.
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