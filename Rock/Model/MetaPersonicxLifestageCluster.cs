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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a MetaPersonicxLifestageCluster <see cref="Rock.Model.MetaPersonicxLifestageCluster"/>. 
    /// </summary>
    [RockDomain( "Meta" )]
    [Table( "MetaPersonicxLifestageCluster" )]
    [DataContract]
    public class MetaPersonicxLifestageCluster:Model<MetaPersonicxLifestageCluster>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the instance of MetaPersonicxLifestyleGroup.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the instance of MetaPersonicxLifestyleGroup.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int MetaPersonicxLifestyleGroupId { get; set; }

        /// <summary>
        /// Gets or sets the code for the LifestyleCluster.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the code for the LifestyleCluster.
        /// </value>
        [Required]
        [MaxLength( 2 )]
        [Column( TypeName = "nchar" )]
        [DataMember( IsRequired = true )]
        public string LifestyleClusterCode { get; set; }

        /// <summary>
        /// Gets or sets the name of the LifestyleCluster.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the LifestyleCluster.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [MaxLength( 50 )]
        public string LifestyleClusterName { get; set; }

        /// <summary>
        /// Gets or sets the summary of the LifestyleCluster.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the summary of the LifestyleCluster.
        /// </value>
        [DataMember]
        [MaxLength( 1000 )]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the Description of the LifestyleCluster.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Description of the LifestyleCluster.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the DetailsUrl of the LifestyleCluster.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the DetailsUrl of the LifestyleCluster.
        /// </value>
        [MaxLength( 120 )]
        [DataMember]
        public string DetailsUrl { get; set; }

        /// <summary>
        /// Gets or sets the LifeStage
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the LifeStage
        /// </value>
        [MaxLength( 120 )]
        [DataMember]
        public string LifeStage { get; set; }

        /// <summary>
        /// Gets or sets the life stage level.
        /// </summary>
        /// <value>
        /// The life stage level.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string LifeStageLevel { get; set; }

        /// <summary>
        /// Gets or sets the MaritalStatus
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the MaritalStatus
        /// </value>
        [MaxLength( 25 )]
        [DataMember]
        public string MaritalStatus { get; set; }

        /// <summary>
        /// Gets or sets the HomeOwnership
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the HomeOwnership
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string HomeOwnership { get; set; }

        /// <summary>
        /// Gets or sets the Children
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Children
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string Children { get; set; }

        /// <summary>
        /// Gets or sets the Income
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Income
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string Income { get; set; }

        /// <summary>
        /// Gets or sets the IncomeLevel
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the IncomeLevel
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string IncomeLevel { get; set; }

        /// <summary>
        /// Gets or sets the Rank for the Income
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the rank of the Income
        /// </value>
        [DataMember]
        public int? IncomeRank { get; set; }

        /// <summary>
        /// Gets or sets the Urbanicity
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Urbanicity
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string Urbanicity { get; set; }

        /// <summary>
        /// Gets or sets the Rank of Urban city
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the rank of the Urban city
        /// </value>
        [DataMember]
        public int? UrbanicityRank { get; set; }

        /// <summary>
        /// Gets or sets the NetWorth
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the NetWorth
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string NetWorth { get; set; }

        /// <summary>
        /// Gets or sets the Level for the NetWorth
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Level for the net worth
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string NetWorthLevel { get; set; }

        /// <summary>
        /// Gets or sets the rank in reference to NetWorth
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the rank in reference to the net worth
        /// </value>
        [DataMember]
        public int? NetworthRank { get; set; }

        /// <summary>
        /// Gets or sets the percent in reference to US
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> representing the percent in reference to US
        /// </value>
        [DataMember]
        public decimal? PercentUS { get; set; }

        /// <summary>
        /// Gets or sets the percent in reference to Organization
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> representing the percent in reference to Organization
        /// </value>
        [DataMember]
        public decimal? PercentOrganization { get; set; }

        /// <summary>
        /// Gets or sets the count for household in an Organization
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the count for household in an Organization
        /// </value>
        [DataMember]
        public int? OrganizationHouseholdCount { get; set; }

        /// <summary>
        /// Gets or sets the individual count in an Organization
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the individual count in an Organization
        /// </value>
        [DataMember]
        public int? OrganizationIndividualCount { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.MetaPersonicxLifestageGroup" /> that this Meta Personicx Lifestage Group is associated with.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.MetaPersonicxLifestageGroup" /> that this Meta Personicx Lifestage Group is associated with.
        /// </value>
        [LavaInclude]
        public virtual MetaPersonicxLifestageGroup MetaPersonicxLifestageGroup { get; set; }

        #endregion

        #region Public Methods

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class MetaPersonicxLifestageClusterConfiguration : EntityTypeConfiguration<MetaPersonicxLifestageCluster>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetaPersonicxLifestageClusterConfiguration"/> class.
        /// </summary>
        public MetaPersonicxLifestageClusterConfiguration()
        {
            this.HasRequired( f => f.MetaPersonicxLifestageGroup ).WithMany().HasForeignKey( f => f.MetaPersonicxLifestyleGroupId ).WillCascadeOnDelete( false );
        }

    }
    #endregion
}
