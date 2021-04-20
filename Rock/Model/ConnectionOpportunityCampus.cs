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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents a connection opportunity campus
    /// </summary>
    [RockDomain( "Engagement" )]
    [Table( "ConnectionOpportunityCampus" )]
    [DataContract]
    public partial class ConnectionOpportunityCampus : Model<ConnectionOpportunityCampus>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionOpportunity"/> identifier.
        /// </summary>
        /// <value>
        /// The connection opportunity identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int ConnectionOpportunityId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int CampusId { get; set; }

        /// <summary>
        /// Gets or sets the default connector <see cref="Rock.Model.PersonAlias"/> identifier.
        /// </summary>
        /// <value>
        /// The default connector person alias identifier.
        /// </value>
        [DataMember]
        public int? DefaultConnectorPersonAliasId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionOpportunity"/>.
        /// </summary>
        /// <value>
        /// The connection opportunity.
        /// </value>
        [LavaVisible]
        public virtual ConnectionOpportunity ConnectionOpportunity { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/>.
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        [LavaVisible]
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the default connector <see cref="Rock.Model.PersonAlias"/>.
        /// </summary>
        /// <value>
        /// The default connector person alias.
        /// </value>
        [LavaVisible]
        public virtual PersonAlias DefaultConnectorPersonAlias { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// ConnectionOpportunityCampus Configuration class.
    /// </summary>
    public partial class ConnectionOpportunityCampusConfiguration : EntityTypeConfiguration<ConnectionOpportunityCampus>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionOpportunityCampusConfiguration" /> class.
        /// </summary>
        public ConnectionOpportunityCampusConfiguration()
        {
            this.HasRequired( p => p.ConnectionOpportunity ).WithMany( p => p.ConnectionOpportunityCampuses ).HasForeignKey( p => p.ConnectionOpportunityId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.Campus ).WithMany().HasForeignKey( p => p.CampusId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.DefaultConnectorPersonAlias ).WithMany().HasForeignKey( p => p.DefaultConnectorPersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}