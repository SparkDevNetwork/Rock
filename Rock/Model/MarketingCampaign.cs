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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a marketing campaign in Rock
    /// </summary>
    [Table( "MarketingCampaign" )]
    [DataContract]
    public partial class MarketingCampaign : Model<MarketingCampaign>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the title of the marketing campaign. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the title of the Marketing Campaign.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the contact person alias identifier.
        /// </summary>
        /// <value>
        /// The contact person alias identifier.
        /// </value>
        [DataMember]
        public int? ContactPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the email address of the contact. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the email address of the contact. If the contact does not have an email address, this value will be null.
        /// </value>
        [MaxLength( 254 )]
        [DataMember]
        [RegularExpression(@"[\w\.\'_%-]+(\+[\w-]*)?@([\w-]+\.)+[\w-]+", ErrorMessage= "The Contact Email address is invalid")]
        public string ContactEmail { get; set; }

        /// <summary>
        /// Gets or sets the contact phone number.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the contact phone number.
        /// </value>
        [MaxLength( 20 )]
        [DataMember]
        public string ContactPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the full name of the contact.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the full name of the contact.
        /// </value>
        [MaxLength( 152 )]
        [DataMember]
        public string ContactFullName { get; set; }

        /// <summary>
        /// Gets or sets the GroupId of the Event <see cref="Rock.Model.Group"/> that is associated with this Marketing Campaign. If an event group is not associated with this campaign, 
        /// this value will be null. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the GroupId of the Event <see cref="Rock.Model.Group"/> that is associated with this Marketing Campaign. 
        /// </value>
        [DataMember]
        public int? EventGroupId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the contact person alias.
        /// </summary>
        /// <value>
        /// The contact person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias ContactPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the event <see cref="Rock.Model.Group"/> that is associated with this Marketing Campaign. 
        /// </summary>
        /// <value>
        /// The event <see cref="Rock.Model.Group"/> that is associated with this Marketing Campaign. If a group is not associated with this Marketing Campaign this value will be null.
        /// </value>
        public virtual Group EventGroup { get; set; }

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.MarketingCampaignAd">MarketingCampaignAds</see> that belong to this Marketing Campaign.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.MarketingCampaignAd">MarketingCampaignAds</see> that belong to this Marketing Campaign.
        /// </value>
        public virtual ICollection<MarketingCampaignAd> MarketingCampaignAds { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.MarketingCampaignAudience">MarketingCampaignAudiences</see> that this marketing campaign is targeted toward.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.MarketingCampaignAudience">MarketingCampaignAudiences</see> that this marketing campaign is targeted toward.
        /// </value>
        [DataMember]
        public virtual ICollection<MarketingCampaignAudience> MarketingCampaignAudiences { get; set; }

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.MarketingCampaignCampus">MarketingCampaignCampuses</see> (campuses) that this marketing campaign will be used at/targeted to.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.MarketingCampaignCampus">MarketingCampaignCampus</see> (campuses) that this marketing campaign will be used at/targeted to.
        /// </value>
        [DataMember]
        public virtual ICollection<MarketingCampaignCampus> MarketingCampaignCampuses { get; set; }

        #endregion

        #region Methods
        
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this Marketing Campaign
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this marketing campaign.
        /// </returns>
        public override string ToString()
        {
            return this.Title;
        }

        #endregion

    }

    #region Entity Configuration
    
    /// <summary>
    /// 
    /// </summary>
    public partial class MarketingCampaignConfiguration : EntityTypeConfiguration<MarketingCampaign>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MarketingCampaignConfiguration" /> class.
        /// </summary>
        public MarketingCampaignConfiguration()
        {
            this.HasOptional( p => p.ContactPersonAlias ).WithMany().HasForeignKey( p => p.ContactPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.EventGroup ).WithMany().HasForeignKey( p => p.EventGroupId).WillCascadeOnDelete( false );
        }
    }

    #endregion

}