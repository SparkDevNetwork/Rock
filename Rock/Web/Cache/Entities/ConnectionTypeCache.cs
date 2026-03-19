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
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Attribute;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.Connection;
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
        #region Properties

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
        /// Flags that specify which optional features are enabled for this connection type.
        /// </summary>
        [DataMember]
        public EnabledFeatureFlags EnabledFeatures { get; private set; }

        /// <summary>
        /// Determines how the due date for a request is calculated.
        /// </summary>
        [DataMember]
        public DueDateCalculationMode DueDateCalculationMode { get; private set; }

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

        /// <summary>
        /// Gets or sets the request due date offset in days.
        /// </summary>
        /// <value>
        /// The request due date offset in days.
        /// </value>
        [DataMember]
        public int? RequestDueDateOffsetInDays { get; private set; }

        /// <summary>
        /// Gets or sets the request due soon date offset in days.
        /// </summary>
        /// <value>
        /// The request due soon date offset in days.
        /// </value>
        [DataMember]
        public int? RequestDueSoonOffsetInDays { get; private set; }

        /// <summary>
        /// Flags that specify which request views are enabled for this connection type.
        /// </summary>
        [DataMember]
        public EnabledViewFlags EnabledViews { get; private set; }

        /// <summary>
        /// Gets the ordered <see cref="ConnectionStatus"/> list for this <see cref="ConnectionType"/>.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Will include both active and inactive statuses.
        ///     </para>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "19.0" )]
        public List<ConnectionStatus> OrderedStatuses => _orderedStatuses.Value;
        private readonly Lazy<List<ConnectionStatus>> _orderedStatuses;

        /// <inheritdoc cref="ConnectionType.IsSequentialStatusEnforced"/>
        [DataMember]
        public bool IsSequentialStatusEnforced { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Default constructor for the ConnectionTypeCache class.
        /// </summary>
        public ConnectionTypeCache()
        {
            _orderedStatuses = new Lazy<List<ConnectionStatus>>( () =>
            {
                using ( var rockContext = RockApp.Current.CreateRockContext() )
                {
                    return new ConnectionStatusService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( cs => cs.ConnectionTypeId == Id )
                        .OrderBy( cs => cs.Order )
                        .ThenByDescending( cs => cs.IsDefault )
                        .ThenBy( cs => cs.Name )
                        .ToList();
                }
            } );
        }

        /// <summary>
        /// Gets a list of all attributes defined for the ConnectionTypes specified that
        /// match the entityTypeQualifierColumn and the ConnectionRequest Ids.
        /// </summary>
        /// <param name="entityTypeId">The Entity Type Id for which Attributes to load.</param>
        /// <param name="entityTypeQualifierColumn">The EntityTypeQualifierColumn value to match against.</param>
        /// <returns>A list of attributes defined in the inheritance tree.</returns>
        [Obsolete( "ConnectionRequest now has a ConnectionTypeId to handle inherited attributes." )]
        [RockObsolete( "17.0" )]
        internal List<AttributeCache> GetInheritedAttributesForQualifier( int entityTypeId, string entityTypeQualifierColumn )
        {
            var attributes = new List<AttributeCache>();

            // Generate a list of matching attributes.
            foreach ( var attribute in AttributeCache.GetByEntityType( entityTypeId ) )
            {
                if ( string.Compare( attribute.EntityTypeQualifierColumn, entityTypeQualifierColumn, true ) == 0 )
                {
                    if ( int.TryParse( attribute.EntityTypeQualifierValue, out var connectionTypeIdValue ) && Id == connectionTypeIdValue )
                    {
                        attributes.Add( attribute );
                    }
                }
            }

            return attributes.OrderBy( a => a.Order ).ToList();
        }

        /// <summary>
        /// Gets whether <paramref name="targetStatusId"/> is the next sequential, active <see cref="ConnectionStatus"/>
        /// after <paramref name="currentStatusId"/>.
        /// </summary>
        /// <param name="currentStatusId">The identifier of the current <see cref="ConnectionStatus"/>.</param>
        /// <param name="targetStatusId">The identifier of the target <see cref="ConnectionStatus"/>.</param>
        /// <returns>Whether <paramref name="targetStatusId"/> is the next sequential, active <see cref="ConnectionStatus"/>.</returns>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "19.0" )]
        public bool IsNextSequentialActiveStatus( int currentStatusId, int targetStatusId )
        {
            int? firstActiveStatusId = null;
            var currentStatusFound = false;

            foreach ( var s in OrderedStatuses )
            {
                if ( firstActiveStatusId == null && s.IsActive )
                {
                    // Take note of the first active status we encounter in case we need to compare it against
                    // targetStatusId below (if we somehow don't find currentStatusId in the list).
                    firstActiveStatusId = s.Id;
                }

                if ( !currentStatusFound )
                {
                    if ( s.Id == currentStatusId )
                    {
                        // We found the current status in the list.
                        currentStatusFound = true;
                    }

                    continue;
                }

                // We are after the current status, so the first active we hit is the "next sequential active".
                if ( s.IsActive )
                {
                    // If the next active status IS targetStatusId, return true; otherwise return false.
                    return s.Id == targetStatusId;
                }
            }

            // currentStatusId wasn't in the list. Does targetStatusId match the first active status?
            return !currentStatusFound && firstActiveStatusId == targetStatusId;
        }

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
            EnabledFeatures = sourceModel.EnabledFeatures;
            RequiresPlacementGroupToConnect = sourceModel.RequiresPlacementGroupToConnect;
            DueDateCalculationMode = sourceModel.DueDateCalculationMode;
            OwnerPersonAliasId = sourceModel.OwnerPersonAliasId;
            DaysUntilRequestIdle = sourceModel.DaysUntilRequestIdle;
            EnableRequestSecurity = sourceModel.EnableRequestSecurity;
            ConnectionRequestDetailPageId = sourceModel.ConnectionRequestDetailPageId;
            ConnectionRequestDetailPageRouteId = sourceModel.ConnectionRequestDetailPageRouteId;
            DefaultView = sourceModel.DefaultView;
            RequestHeaderLava = sourceModel.RequestHeaderLava;
            RequestBadgeLava = sourceModel.RequestBadgeLava;
            Order = sourceModel.Order;
            RequestDueDateOffsetInDays = sourceModel.RequestDueDateOffsetInDays;
            RequestDueSoonOffsetInDays = sourceModel.RequestDueSoonOffsetInDays;
            EnabledViews = sourceModel.EnabledViews;
            IsSequentialStatusEnforced = sourceModel.IsSequentialStatusEnforced;
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