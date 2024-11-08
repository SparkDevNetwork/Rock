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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents a connection opportunity
    /// </summary>
    [RockDomain( "Engagement" )]
    [Table( "ConnectionOpportunity" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.CONNECTION_OPPORTUNITY )]
    public partial class ConnectionOpportunity : Model<ConnectionOpportunity>, IHasActiveFlag, IOrdered
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the public.
        /// </summary>
        /// <value>
        /// The name of the public.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string PublicName { get; set; }

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        /// <value>
        /// The summary.
        /// </value>
        [DataMember]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the photo identifier.
        /// </summary>
        /// <value>
        /// The photo identifier.
        /// </value>
        [DataMember]
        public int? PhotoId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionType"/> identifier.
        /// </summary>
        /// <value>
        /// The connection type identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int ConnectionTypeId { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show status on transfer].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show status on transfer]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowStatusOnTransfer { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether [show connect button].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show connect button]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowConnectButton { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether [show campus on transfer].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show campus on transfer]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowCampusOnTransfer { get; set; } = false;

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionType">type</see> of the connection.
        /// </summary>
        /// <value>
        /// The type of the connection.
        /// </value>
        [LavaVisible]
        public virtual ConnectionType ConnectionType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.BinaryFile"/> that contains the Opportunity's photo.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.BinaryFile"/> that contains the Opportunity's photo.
        /// </value>
        [LavaVisible]
        public virtual BinaryFile Photo { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.ConnectionOpportunityGroup">ConnectionOpportunityGroups</see> who are associated with the ConnectionOpportunity.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.ConnectionOpportunityGroup">ConnectionOpportunityGroups</see> who are associated with the ConnectionOpportunity.
        /// </value>
        public virtual ICollection<ConnectionOpportunityGroup> ConnectionOpportunityGroups
        {
            get { return _connectionOpportunityGroups ?? ( _connectionOpportunityGroups = new Collection<ConnectionOpportunityGroup>() ); }
            set { _connectionOpportunityGroups = value; }
        }

        private ICollection<ConnectionOpportunityGroup> _connectionOpportunityGroups;

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionOpportunityConnectorGroupConfiguration">connection opportunity group configs</see>.
        /// </summary>
        /// <value>
        /// The connection opportunity placement groups.
        /// </value>
        public virtual ICollection<ConnectionOpportunityGroupConfig> ConnectionOpportunityGroupConfigs
        {
            get { return _connectionOpportunityGroupConfigs ?? ( _connectionOpportunityGroupConfigs = new Collection<ConnectionOpportunityGroupConfig>() ); }
            set { _connectionOpportunityGroupConfigs = value; }
        }

        private ICollection<ConnectionOpportunityGroupConfig> _connectionOpportunityGroupConfigs;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.ConnectionWorkflow">ConnectionWorkflows</see> who are associated with the ConnectionOpportunity.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.ConnectionWorkflow">ConnectionWorkflows</see> who are associated with the ConnectionOpportunity.
        /// </value>
        public virtual ICollection<ConnectionWorkflow> ConnectionWorkflows
        {
            get { return _connectionWorkflows ?? ( _connectionWorkflows = new Collection<ConnectionWorkflow>() ); }
            set { _connectionWorkflows = value; }
        }

        private ICollection<ConnectionWorkflow> _connectionWorkflows;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.ConnectionRequest">ConnectionRequests</see> who are associated with the ConnectionOpportunity.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.ConnectionRequest">ConnectionRequests</see> who are associated with the ConnectionOpportunity.
        /// </value>
        public virtual ICollection<ConnectionRequest> ConnectionRequests
        {
            get { return _connectionRequests ?? ( _connectionRequests = new Collection<ConnectionRequest>() ); }
            set { _connectionRequests = value; }
        }

        private ICollection<ConnectionRequest> _connectionRequests;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.ConnectionOpportunityConnectorGroup">ConnectionOpportunityConnectorGroup</see> who are associated with the ConnectionOpportunity.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.ConnectionOpportunityConnectorGroup">ConnectionOpportunityConnectorGroup</see> who are associated with the ConnectionOpportunity.
        /// </value>
        public virtual ICollection<ConnectionOpportunityConnectorGroup> ConnectionOpportunityConnectorGroups
        {
            get { return _connectionOpportunityConnectorGroups ?? ( _connectionOpportunityConnectorGroups = new Collection<ConnectionOpportunityConnectorGroup>() ); }
            set { _connectionOpportunityConnectorGroups = value; }
        }

        private ICollection<ConnectionOpportunityConnectorGroup> _connectionOpportunityConnectorGroups;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.ConnectionOpportunityCampus">ConnectionOpportunityCampuses</see> who are associated with the ConnectionOpportunity.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.ConnectionOpportunityCampus">ConnectionOpportunityCampuses</see> who are associated with the ConnectionOpportunity.
        /// </value>
        [LavaVisible]
        public virtual ICollection<ConnectionOpportunityCampus> ConnectionOpportunityCampuses
        {
            get { return _connectionOpportunityCampuses ?? ( _connectionOpportunityCampuses = new Collection<ConnectionOpportunityCampus>() ); }
            set { _connectionOpportunityCampuses = value; }
        }

        private ICollection<ConnectionOpportunityCampus> _connectionOpportunityCampuses;

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// ConnectionOpportunity Configuration class.
    /// </summary>
    public partial class ConnectionOpportunityConfiguration : EntityTypeConfiguration<ConnectionOpportunity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionOpportunityConfiguration" /> class.
        /// </summary>
        public ConnectionOpportunityConfiguration()
        {
            this.HasRequired( p => p.ConnectionType ).WithMany( p => p.ConnectionOpportunities ).HasForeignKey( p => p.ConnectionTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Photo ).WithMany().HasForeignKey( p => p.PhotoId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}